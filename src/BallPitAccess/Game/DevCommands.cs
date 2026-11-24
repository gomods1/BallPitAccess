using System;
using System.IO;
using BallPitAccess.Core;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Developer commands for setting up a test save without driving the game window. When
    /// `UserData/BallPitAccess/dev.txt` exists, each line is run once through the game's own save and cheat
    /// code, the results go to Latest.log ("Dev: ..."), and the file is deleted. Only with `Debug = true` under
    /// `[BallPitAccess]` in MelonPreferences.cfg (edit it while the game is closed).
    /// Commands: `resources N` (every resource +N), `blueprint kSheriffOffice` (any BuildingType),
    /// `unseen kFirstHouse` (mark a base tutorial unseen), `tut kFirstHouse` (show it), `cheat kAddResources`
    /// (any BaseCheatType; several do nothing in the shipped game), `probe` (base state and build list),
    /// `select kSheriffOffice` (the build menu's own SelectItem), `save`, `state`.
    /// </summary>
    internal static class DevCommands
    {
        private static float _nextPoll;

        internal static void Update()
        {
            if (Time.unscaledTime < _nextPoll) return;
            _nextPoll = Time.unscaledTime + 1f;
            string path = Path.Combine(Log.Dir ?? "", "dev.txt");
            if (!File.Exists(path)) return;
            string[] lines;
            try { lines = File.ReadAllLines(path); File.Delete(path); }
            catch (Exception ex) { Log.Warn("Dev: cannot read dev.txt: " + ex.Message); return; }
            foreach (var raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                try { Log.Info("Dev: " + line + " -> " + Run(line)); }
                catch (Exception ex) { Log.Error("Dev: " + line + " failed: " + ex); }
            }
        }

        private static string Run(string line)
        {
            var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string cmd = parts[0].ToLowerInvariant();
            string arg = parts.Length > 1 ? parts[1].Trim() : "";
            var sm = SaveMgr.I;
            switch (cmd)
            {
                case "resources":
                {
                    if (sm == null) return "no SaveMgr";
                    int n = arg.Length > 0 ? int.Parse(arg) : 100000;
                    for (int i = 0; i < (int)ResourceType.kNum; i++) sm.AddResources((ResourceType)i, n, false, false);
                    sm.SaveMeta();
                    RefreshBaseUI();
                    return "added " + n + " of each; gold " + sm.GetNumGold();
                }
                case "blueprint":
                {
                    if (sm == null) return "no SaveMgr";
                    var bt = (BuildingType)Enum.Parse(typeof(BuildingType), arg);
                    bool had = sm.HasBlueprint(bt);
                    if (!had) sm.GainBlueprint(bt);
                    sm.SaveMeta();
                    return bt + (had ? " already owned" : " gained");
                }
                case "tut":
                {
                    var tm = BaseTutMgr.I;
                    if (tm == null) return "no BaseTutMgr (not in the base?)";
                    var t = (BaseTutType)Enum.Parse(typeof(BaseTutType), arg);
                    bool seen = tm.IsSeen(t);
                    tm.Show(t);
                    return "showing " + t + " (was seen " + seen + ")";
                }
                case "unseen":
                {
                    if (sm == null || sm.MetaSave == null) return "no save";
                    var t = (BaseTutType)Enum.Parse(typeof(BaseTutType), arg);
                    var arr = sm.MetaSave.IsBaseTutSeen;
                    if (arr == null || (int)t >= arr.Length) return "no seen array";
                    arr[(int)t] = false;
                    sm.SaveMeta();
                    return t + " marked unseen";
                }
                case "cheat":
                {
                    var cm = BaseCheatMgr.I;
                    if (cm == null) return "no BaseCheatMgr in this build";
                    var ct = (BaseCheatType)Enum.Parse(typeof(BaseCheatType), arg);
                    cm.ApplyCheat(ct);
                    RefreshBaseUI();
                    return "applied " + ct;
                }
                case "save":
                    if (sm == null) return "no SaveMgr";
                    sm.SaveMeta();
                    return "saved";
                case "state":
                {
                    if (sm == null) return "no SaveMgr";
                    string s = "gold " + sm.GetNumGold();
                    for (int i = 1; i < (int)ResourceType.kNum; i++) s += ", " + (ResourceType)i + " " + sm.GetNumResources((ResourceType)i);
                    s += "; sheriff blueprint " + sm.HasBlueprint(BuildingType.kSheriffOffice);
                    var tm = BaseTutMgr.I;
                    if (tm != null)
                    {
                        s += "; tut " + tm.CurTut + " step " + tm.CurTutStep + " showing " + tm.IsShowing();
                        foreach (BaseTutType t in Enum.GetValues(typeof(BaseTutType)))
                            if (t != BaseTutType.kNum) s += " " + t + "=" + (tm.IsSeen(t) ? "seen" : "new");
                    }
                    s += "; cheat mgr " + (BaseCheatMgr.I != null);
                    return s;
                }
                case "probe":
                {
                    string s = "base state " + (BaseMgr.I != null ? BaseMgr.I.CurState.ToString() : "none");
                    s += "; placement " + Placement.Active + "; ftue " + UI.Navigator.FtueShowing + "; blocking " + UI.Navigator.Blocking;
                    var es = UnityEngine.EventSystems.EventSystem.current;
                    s += "; selected " + (es != null && es.currentSelectedGameObject != null ? UI.MenuReader.Path(es.currentSelectedGameObject.transform) : "none");
                    var ui = BuildUI.I;
                    if (ui != null)
                        foreach (var bi in ui.GetComponentsInChildren<BuildItem>(false))
                        {
                            var inf = bi.TgtInfo;
                            s += "\n  item " + (inf != null ? inf.Type.ToString() : "null") + " locked " + bi.IsLocked + " afford " + bi.CanAfford
                                + " interactable " + (bi.Btn != null && bi.Btn.IsInteractable()) + " cost '" + (bi.TxtCost != null ? bi.TxtCost.text : "") + "'"
                                + (inf != null ? " canBuildMore " + inf.CanBuildMore() + " blueprint " + (sm != null && sm.HasBlueprint(inf.Type)) : "");
                        }
                    return s;
                }
                case "select":
                {
                    var ui = BuildUI.I;
                    if (ui == null) return "no BuildUI";
                    var bt = (BuildingType)Enum.Parse(typeof(BuildingType), arg);
                    var item = ui.GetItemForBuilding(bt);
                    if (item == null) return "no item for " + bt;
                    ui.SelectItem(item);
                    return "SelectItem done; base state " + (BaseMgr.I != null ? BaseMgr.I.CurState.ToString() : "none");
                }
                case "pieces":
                {
                    var save = BattleSaveData.I;
                    if (save == null || save.Pieces == null) return "not in battle";
                    var sb = new System.Text.StringBuilder("pieces " + save.Pieces.Count);
                    foreach (var inst in save.Pieces)
                    {
                        if (inst == null) continue;
                        var o = inst.Obj;
                        sb.Append("\n  ").Append(inst.Type).Append(" child ").Append(inst.ChildIdx).Append(' ').Append(inst.CurState)
                          .Append(" hp ").Append(inst.CurHealth).Append('/').Append(inst.MaxHealth);
                        if (o != null)
                        {
                            string cls = "?";
                            try { cls = o.GetIl2CppType().Name; } catch { }
                            sb.Append(" obj ").Append(cls).Append(" active ").Append(o.IsActive);
                            try { sb.Append(" canDamage ").Append(o.CanBeDamaged()); } catch { }
                            try { sb.Append(" pct ").Append(o.GetHealthPct().ToString("0.000")); } catch { }
                            try { sb.Append(" sub ").Append(o.TryCast<SubGridPieceObj>() != null); } catch { }
                            try { sb.Append(" at ").Append(o.GetWorldCenterPos()); } catch { }
                        }
                    }
                    foreach (var inst in save.Pieces)
                    {
                        var o = inst?.Obj;
                        if (o == null || !Battle.IsBoss(inst.Type)) continue;
                        try
                        {
                            foreach (var sub in o.GetComponentsInChildren<SubGridPieceObj>(true))
                            {
                                sb.Append("\n  part ").Append(sub.GetIl2CppType().Name).Append(" go ").Append(sub.gameObject.name)
                                  .Append(" active ").Append(sub.IsActive).Append(" goActive ").Append(sub.gameObject.activeInHierarchy)
                                  .Append(" canDamage ").Append(sub.CanBeDamaged()).Append(" pct ").Append(sub.GetHealthPct().ToString("0.000"));
                                try { var si = sub.Inst; sb.Append(" inst ").Append(si != null ? si.Type + " " + si.CurHealth + "/" + si.MaxHealth + (si.Pointer == inst.Pointer ? " SAME" : "") : "null"); } catch { }
                                try { sb.Append(" at ").Append(sub.GetWorldCenterPos()); } catch { }
                                try { var c = sub.Col; if (c != null) sb.Append(" col ").Append(c.enabled).Append(' ').Append(c.bounds.min).Append('-').Append(c.bounds.max).Append(" layer ").Append(c.gameObject.layer); } catch { }
                            }
                            var c2 = o.Col;
                            if (c2 != null) sb.Append("\n  boss col ").Append(c2.GetIl2CppType().Name).Append(' ').Append(c2.enabled).Append(' ').Append(c2.bounds.min).Append('-').Append(c2.bounds.max).Append(" layer ").Append(c2.gameObject.layer);
                            var king = o.TryCast<GridPieceObjSkeletonKing>();
                            if (king != null)
                            {
                                sb.Append("\n  king phase ").Append(king.CurPhase).Append(" crown ").Append(king.Crown != null ? king.Crown.gameObject.name : "null");
                                if (king.LimbColliders != null)
                                    foreach (var lc in king.LimbColliders) if (lc != null) sb.Append("\n  limb ").Append(lc.gameObject.name).Append(' ').Append(lc.enabled).Append(' ').Append(lc.bounds.min).Append('-').Append(lc.bounds.max).Append(" layer ").Append(lc.gameObject.layer);
                            }
                        }
                        catch (Exception ex) { sb.Append("\n  parts failed: ").Append(ex.Message); }
                    }
                    try
                    {
                        var ui = InGameUI.I;
                        if (ui != null && ui.BossBarFill != null) sb.Append("\n  boss bar active ").Append(ui._activatedBossBar).Append(" fill ").Append(ui.BossBarFill.fillAmount.ToString("0.000"));
                    }
                    catch { }
                    return sb.ToString();
                }
                case "gamecheat":
                {
                    var gc = GameCheatMgr.I;
                    if (gc == null) return "no GameCheatMgr in this build";
                    var ct = (GameCheatType)Enum.Parse(typeof(GameCheatType), arg);
                    gc.ApplyCheat(ct);
                    return "applied " + ct;
                }
                case "god":
                {
                    var gc = GameCheatMgr.I;
                    if (gc == null) return "no GameCheatMgr in this build";
                    gc.IsGodMode = arg != "off";
                    return "god mode " + gc.IsGodMode;
                }
                case "status":
                {
                    // status kFrozen 5: put a status effect on the hero for N seconds.
                    var p = Player.I;
                    if (p == null) return "not in battle";
                    var bits = arg.Split(' ');
                    var t = (PlayerStatusEffectType)Enum.Parse(typeof(PlayerStatusEffectType), bits[0]);
                    float len = bits.Length > 1 ? float.Parse(bits[1], System.Globalization.CultureInfo.InvariantCulture) : 5f;
                    p.ApplyStatusEffect(new PlayerStatusEffect(t, len));
                    return "applied " + t + " for " + len + " s; now: " + (Battle.MoveBlock() ?? "can move");
                }
                case "buildings":
                {
                    var bm = BuildingMgr.I;
                    if (bm == null || bm.BuildingsById == null) return "no BuildingMgr";
                    var sb = new System.Text.StringBuilder("buildings " + bm.BuildingsById.Count);
                    foreach (var kv in bm.BuildingsById)
                    {
                        var b = kv.Value;
                        if (b == null) continue;
                        sb.Append("\n  ").Append(b.Type).Append(' ').Append(b.CurState).Append(" pts ").Append(b.UpgradePts).Append('/').Append(b.GetUpgradeTgt())
                          .Append(" canHarvest ").Append(b.CanHarvest());
                        if (b.Obj != null)
                        {
                            sb.Append(" at ").Append(b.Obj.transform.position);
                            try { BaseGrid.Refresh(); var tl = BaseGrid.TopLeftTiles(b); if (tl.HasValue) sb.Append(" tiles from col ").Append(BaseGrid.Col(tl.Value.x)).Append(" row ").Append(BaseGrid.Row(tl.Value.y)).Append(' ').Append(BaseGrid.SizeTiles(b)); } catch { }
                            try { var c = b.Obj.Col; if (c != null) sb.Append(" col ").Append(c.GetIl2CppType().Name).Append(" layer ").Append(c.gameObject.layer).Append(" enabled ").Append(c.enabled).Append(" bounds ").Append(c.bounds.min).Append('-').Append(c.bounds.max); } catch { }
                        }
                    }
                    var ms = MetaSaveData.I;
                    if (ms != null) sb.Append("\n  harvested today ").Append(ms.DidHarvestToday);
                    try { sb.Append(" piece layer ").Append(ColMgr.I.GetGameLayer(GameLayerType.kPiece)).Append(" bounce mask ").Append(ColMgr.kLayerMaskBounce.ToString("x")); } catch { }
                    return sb.ToString();
                }
                case "harvestreset":
                {
                    var ms = MetaSaveData.I;
                    if (ms == null) return "no save";
                    ms.DidHarvestToday = false;
                    try { BaseUI.I?.RefreshWorkerBtn(); } catch { }
                    return "harvest available again";
                }
                case "focus":
                    Input.Keys.DevFocus = arg != "off";
                    return "dev focus " + Input.Keys.DevFocus;
                case "key":
                {
                    // key tab shift+tab enter u: each one a frame down, in order (needs "focus on" with no window).
                    int n = 0;
                    foreach (var spec in arg.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts2 = spec.Split('+');
                        var vks = new int[parts2.Length];
                        for (int i = 0; i < parts2.Length; i++) vks[i] = Vk(parts2[i]);
                        Input.Keys.Inject(vks);
                        n++;
                    }
                    return "queued " + n + " keys";
                }
                case "pad":
                {
                    // pad a view+a up lb: controller buttons, each one a frame down (Input.Pad.Btn names).
                    int n = 0;
                    foreach (var spec in arg.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        if (Input.Pad.Inject(spec)) n++;
                        else return "unknown button " + spec;
                    return "queued " + n + " buttons";
                }
                case "paddump":
                    Log.WriteFile("pad.txt", Input.Pad.Describe());
                    return "pad.txt written";
                case "cheatflag":
                {
                    // cheatflag DisableLevelUp true: any bool / int field of GameCheatMgr.
                    var gc = GameCheatMgr.I;
                    if (gc == null) return "no GameCheatMgr in this build";
                    var bits = arg.Split(' ');
                    var prop = typeof(GameCheatMgr).GetProperty(bits[0]);
                    if (prop == null) return "no field " + bits[0];
                    object v = prop.PropertyType == typeof(bool) ? (object)bool.Parse(bits[1]) : int.Parse(bits[1]);
                    prop.SetValue(gc, v);
                    return bits[0] + " = " + prop.GetValue(gc);
                }
                case "autofire":
                    PlayerPrefsMgr.SetAutofire(arg != "off");
                    return "autofire " + PlayerPrefsMgr.sAutofire;
                case "speed":
                    PlayerPrefsMgr.SetGameSpeed((GameSpeed)Enum.Parse(typeof(GameSpeed), arg));
                    return "speed " + PlayerPrefsMgr.sGameSpeed;
                case "forcedeg":
                    AimAssist.DevDeg = float.Parse(arg, System.Globalization.CultureInfo.InvariantCulture);
                    return "force angle " + AimAssist.DevDeg + " (0 = the assist)";
                case "forceaim":
                    AimAssist.DevForce = arg != "off";
                    return "force aim " + AimAssist.DevForce;
                case "target":
                {
                    var t = AimAssist.Target;
                    string s2 = "target " + (t != null ? t.Type + " " + t.CurHealth + "/" + t.MaxHealth + " obj " + (t.Obj != null ? t.Obj.GetIl2CppType().Name : "null") : "none");
                    if (AimAssist.WantDir(out Vector2 d)) s2 += " dir " + (Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg).ToString("0.0");
                    try { var bm = BallMgr.I; if (bm != null) { var g = bm.GetCurAimDir(); s2 += " game aim " + (Mathf.Atan2(g.y, g.x) * Mathf.Rad2Deg).ToString("0.0"); } } catch { }
                    return s2;
                }
                case "tilemap":
                {
                    // tilemap kHauntedHouse: the tiles each building of that type covers in the chunk grid (# = it),
                    // with its rotation, and the info's inner grid; for the shape / rotation model of placement.
                    var bt = (BuildingType)Enum.Parse(typeof(BuildingType), arg);
                    var bm = BuildingMgr.I;
                    if (bm == null || bm.BuildingsById == null) return "no BuildingMgr";
                    BaseGrid.Refresh();
                    var sb = new System.Text.StringBuilder();
                    foreach (var kv in bm.BuildingsById)
                    {
                        var b = kv.Value;
                        if (b == null || b.Type != bt) continue;
                        var inf = b.GetInfo();
                        sb.Append("\n  ").Append(bt).Append(" rot ").Append(b.Rotation).Append(" X,Y ").Append(b.X).Append(',').Append(b.Y)
                          .Append(" botleft ").Append(b.GetBotLeftGridPos()).Append(" size ").Append(inf.TileSize).Append(" inst size ").Append(b.GetTileSize());
                        if (BaseGrid.Tiles(b, out int x0, out int x1, out int y0, out int y1))
                        {
                            sb.Append(" tiles x ").Append(x0).Append("..").Append(x1).Append(" y ").Append(y0).Append("..").Append(y1);
                            for (int y = y1; y >= y0; y--)
                            {
                                sb.Append("\n    ");
                                for (int x = x0; x <= x1; x++) { var o = BaseGrid.BuildingAt(x, y); sb.Append(o != null && o.Pointer == b.Pointer ? '#' : '.'); }
                            }
                        }
                        sb.Append("\n    inner grid (x right, y up):");
                        for (int y = inf.TileSize.y - 1; y >= 0; y--)
                        {
                            sb.Append("\n    ");
                            for (int x = 0; x < inf.TileSize.x; x++) { bool on; try { on = inf.IsInInnerGrid(x, y); } catch { on = true; } sb.Append(on ? '#' : '.'); }
                        }
                    }
                    var pv = BaseGridMgr.I != null ? BaseGridMgr.I.GetPlacePreview() : null;
                    if (pv != null && pv.Inst != null) sb.Append("\n  preview ").Append(pv.Inst.Type).Append(" rot ").Append(pv.Inst.Rotation).Append(" hover ").Append(BaseGridMgr.I.GetHoverPos()).Append(" cell ").Append(BaseGrid.HoverCell(BaseGridMgr.I.GetHoverPos()));
                    return sb.ToString();
                }
                case "rotate":
                {
                    var g = BaseGridMgr.I;
                    if (g == null || g.GetPlacePreview() == null) return "no preview";
                    g.RotatePlacePreview(arg.Length > 0 ? int.Parse(arg) : 1);
                    return "rotation now " + g.GetPlacePreview().Inst.Rotation;
                }
                case "findtext":
                {
                    // findtext StartStat: every active text object with that name, with its path and text.
                    var sb = new System.Text.StringBuilder();
                    int n = 0;
                    foreach (var t in UnityEngine.Object.FindObjectsOfType<Il2CppTMPro.TMP_Text>())
                    {
                        if (t == null || t.name != arg) continue;
                        sb.Append("\n  ").Append(UI.MenuReader.Path(t.transform)).Append(" = ").Append(t.text).Append(" active ").Append(t.isActiveAndEnabled);
                        if (++n >= 20) break;
                    }
                    var lu = LevelUpUI.I;
                    if (lu != null) sb.Append("\n  LevelUpUI.I at ").Append(UI.MenuReader.Path(lu.transform)).Append(" active ").Append(lu.gameObject.activeInHierarchy);
                    return n + " found" + sb;
                }
                case "terms":
                {
                    // terms Endurance: the I2 terms whose term name or current translation contains the text.
                    var sb = new System.Text.StringBuilder();
                    int n = 0;
                    foreach (var term in Il2CppI2.Loc.LocalizationManager.GetTermsList())
                    {
                        string tr = Il2CppI2.Loc.LocalizationManager.GetTranslation(term) ?? "";
                        if (term.IndexOf(arg, StringComparison.OrdinalIgnoreCase) < 0 && tr.IndexOf(arg, StringComparison.OrdinalIgnoreCase) < 0) continue;
                        sb.Append("\n  ").Append(term).Append(" = ").Append(tr.Length > 80 ? tr.Substring(0, 80) : tr);
                        if (++n >= 40) break;
                    }
                    return n + " terms" + sb;
                }
                case "evosetup":
                {
                    // evosetup 3: own both balls of N evolutions (two base balls each), all at their top level, so the
                    // next fuser (gamestate kPickTreasure) offers several evolutions.
                    var db = InfoDB.I; var um = UpgradeMgr.I; var bsd = BattleSaveData.I;
                    if (db == null || um == null || bsd == null) return "not in battle";
                    int want = arg.Length > 0 ? int.Parse(arg) : 3;
                    var used = new System.Collections.Generic.HashSet<HeroType>();
                    var sb = new System.Text.StringBuilder();
                    int got = 0;
                    foreach (var h in db.Heroes)
                    {
                        if (got >= want || h == null || h.MergeComponents == null || h.MergeComponents.Length == 0) continue;
                        var comps = h.MergeComponents[0];
                        if (comps == null || comps.Length != 2) continue;
                        var a = comps[0] != null ? comps[0].TryCast<HeroInfo>() : null;
                        var b = comps[1] != null ? comps[1].TryCast<HeroInfo>() : null;
                        if (a == null || b == null || a.IsMerged() || b.IsMerged() || used.Contains(a.Type) || used.Contains(b.Type)) continue;
                        used.Add(a.Type); used.Add(b.Type); got++;
                        sb.Append(' ').Append(h.Type).Append('=').Append(a.Type).Append('+').Append(b.Type);
                    }
                    foreach (var t in used)
                    {
                        bool owned = false;
                        foreach (var hi in bsd.Heroes) if (hi != null && hi.Type == t) owned = true;
                        if (!owned) um.AddHero(t);
                    }
                    foreach (var hi in bsd.Heroes) if (hi != null) { int m = hi.GetMaxLvl(); hi.Lvl = m; sb.Append(" | ").Append(hi.Type).Append(" lvl ").Append(hi.Lvl).Append('/').Append(m); }
                    return "evolutions" + sb;
                }
                case "bback":
                {
                    var bm = BaseMgr.I;
                    if (bm == null) return "not in the base";
                    bm.OnBPressed();
                    return "OnBPressed, state " + bm.CurState;
                }
                case "gamestate":
                {
                    var gm = GameMgr.I;
                    if (gm == null) return "not in battle";
                    gm.SetState((GameState)Enum.Parse(typeof(GameState), arg));
                    return "state " + gm.CurState;
                }
                case "autoplay":
                    GameCheatMgr.Autoplay = arg != "off";
                    return "autoplay " + GameCheatMgr.Autoplay;
                case "aim":
                    return Harvest.DevAim(arg);
                case "launch":
                    return Harvest.DevLaunch();
                default:
                    return "unknown command";
            }
        }

        private static int Vk(string name)
        {
            name = name.ToLowerInvariant();
            switch (name)
            {
                case "tab": return 0x09;
                case "enter": return 0x0D;
                case "esc": return 0x1B;
                case "space": return 0x20;
                case "back": return 0x08;
                case "shift": return 0x10;
                case "ctrl": return 0x11;
                case "alt": return 0x12;
                case "left": return 0x25;
                case "up": return 0x26;
                case "right": return 0x27;
                case "down": return 0x28;
                case "home": return 0x24;
                case "end": return 0x23;
                case "pgup": return 0x21;
                case "pgdn": return 0x22;
                case "del": return 0x2E;
                case "plus": return 0x6B;
                case "minus": return 0x6D;
            }
            if (name.Length > 1 && name[0] == 'f' && int.TryParse(name.Substring(1), out int f)) return 0x70 + f - 1;
            if (name.Length == 1) return char.ToUpperInvariant(name[0]);
            throw new ArgumentException("unknown key " + name);
        }

        private static void RefreshBaseUI()
        {
            try { BaseUI.I?.RefreshResourceRect(); } catch { }
        }
    }
}

using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using BallPitAccess.Game;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallPitAccess.UI
{
    /// <summary>Concise summary plus details lines for a rich game object (template 11.1).</summary>
    internal sealed class FocusReadout
    {
        public string Summary;
        public List<string> Details = new List<string>();
        /// <summary>The game's panel for this item was not filled yet: read it again in a moment.</summary>
        public bool Retry;

        internal FocusReadout() { }
        internal FocusReadout(params string[] parts) { Summary = Readers.Join(parts); }
        internal FocusReadout Add(string line)
        {
            if (!string.IsNullOrEmpty(line) && !Details.Contains(line) && line != Summary) Details.Add(line);
            return this;
        }
        internal FocusReadout AddAll(IEnumerable<string> lines)
        {
            if (lines != null) foreach (var l in lines) Add(l);
            return this;
        }
    }

    /// <summary>
    /// Domain readers, most specific first (template 10.2). Each returns null when the focused object is
    /// not its kind; the generic label reader in <see cref="MenuReader"/> handles the rest.
    /// The game's menus are icon grids with a details panel that follows the hover: a reader names the item
    /// from the game's own data and takes the description from that panel, which is what a sighted player
    /// reads. The mod's focus fires the same hover the mouse does, so the panel is already filled.
    /// </summary>
    internal static class Readers
    {
        internal static string Join(params string[] parts)
        {
            var list = new List<string>();
            foreach (var p in parts) if (!string.IsNullOrEmpty(p)) list.Add(p);
            return string.Join(", ", list);
        }

        internal static string Tr(string term) => Names.Tr(term);

        private static bool Is(GameObject go, Component c)
        {
            try { return c != null && c.gameObject.Pointer == go.Pointer; } catch { return false; }
        }

        private static T Up<T>(GameObject go) where T : Component
        {
            try { return go.GetComponentInParent<T>(); } catch { return null; }
        }

        private static List<string> Lines(Component c)
        {
            try { return c != null && c.gameObject.activeInHierarchy ? MenuReader.TextLines(c.transform) : new List<string>(); }
            catch { return new List<string>(); }
        }

        private static List<string> Lines(GameObject g)
        {
            try { return g != null && g.activeInHierarchy ? MenuReader.TextLines(g.transform) : new List<string>(); }
            catch { return new List<string>(); }
        }

        /// <summary>
        /// The object is (part of) an item one of the readers below knows: content of the screen even when
        /// it has no click handler (an encyclopedia entry, a stat row of the run summary). No side effects.
        /// </summary>
        internal static bool IsItem(GameObject go)
        {
            try
            {
                return Has<LevelUpBtn>(go) || Has<EvoSelectBtn>(go) || Has<ComboSelectItem>(go) || Has<FuserOptionBtn>(go) ||
                    Has<FreeUpgradeItem>(go) || Has<LevelUpCurEquipItem>(go) || Has<PetDisplayItemBattle>(go) ||
                    Has<PauseEquipInfoBtn>(go) || Has<GameOverBallStatItem>(go) || Has<HUDUpgradeItem>(go) || Has<GameSpeedBtn>(go) ||
                    Has<BuildItem>(go) || Has<CharSelectItem>(go) || (Has<LevelSelectItem>(go) && !Has<LevelCharItem>(go)) ||
                    Has<WarRoomSelectLvlItem>(go) || Has<EquippedCharBtn>(go) ||
                    Has<WorkerItem>(go) || Has<AssignWorkerItem>(go) || Has<MasseuseItem>(go) || Has<MarketItem>(go) ||
                    Has<HatcheryItem>(go) || Has<EggListItem>(go) || Has<PetLabItem>(go) || Has<PetLabSelectItem>(go) ||
                    Has<SelectPetItem>(go) || Has<EquippedPetItem>(go) || Has<TestTubeItem>(go) ||
                    Has<EncyclopediaItem>(go) || Has<ItemUnlockItem>(go) || Has<HarvestSummaryBuildItem>(go) ||
                    Has<SettingsItem>(go) || Has<SettingsRemapItem>(go) || Has<SelectSlotItem>(go) || Has<BaseSidebarBtn>(go) ||
                    Has<HarvestUpgradeBtn>(go) || Has<StatDisplayItem>(go);
            }
            catch { return true; }
        }

        private static bool Has<T>(GameObject go) where T : Component => go.GetComponentInParent<T>() != null;

        internal static FocusReadout Read(GameObject go)
        {
            try { return ReadInner(go); }
            catch (Exception ex)
            {
                if (Config.Debug) Log.Warn("Reader: " + ex);
                return null;
            }
        }

        /// <summary>
        /// Buttons that belong to a screen rather than to an item: the lift's two upgrade paths (down the
        /// pit, up to Risen Ballbylon), the level sets, the new game plus and encyclopedia set arrows.
        /// </summary>
        /// <summary>A tab of a screen: its name, and "selected" for the one that is open (the game greys that one out).</summary>
        private static FocusReadout Tab(GameObject go, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<CoolButton> tabs, int current, Func<int, string> fallbackName)
        {
            if (tabs == null) return null;
            for (int i = 0; i < tabs.Length; i++)
            {
                if (!Is(go, tabs[i])) continue;
                var own = Lines(go);
                string name = own.Count > 0 ? own[0] : fallbackName != null ? fallbackName(i) : MenuReader.IconName(go.name);
                return new FocusReadout(name, Loc.Get("tab"), i == current ? Loc.Get("selected") : null);
            }
            return null;
        }

        private static FocusReadout ScreenTab(GameObject go)
        {
            try
            {
                var en = EncyclopediaUI.I;
                if (en != null && en.gameObject.activeInHierarchy)
                {
                    var r = Tab(go, en.NavBtns, (int)en.CurPage, i => Names.Mod("page_", ((EncycloPage)i).ToString()));
                    // The statistics are not buttons: the open Statistics tab carries them, one per Ctrl+Up.
                    if (r != null && en.CurPage == EncycloPage.kStatistics && Is(go, en.NavBtns[(int)en.CurPage]))
                        r.AddAll(Navigator.EncyclopediaStatLines(en));
                    if (r != null) return r;
                }
                var pa = PauseUI.I;
                if (pa != null && pa.gameObject.activeInHierarchy)
                {
                    var r = Tab(go, pa.NavBtns, (int)pa.CurPage, null);
                    if (r != null) return r;
                }
                var bu = BuildUI.I;
                if (bu != null && bu.gameObject.activeInHierarchy)
                {
                    var r = Tab(go, bu.CatBtns, (int)bu.TgtCat, null);
                    if (r != null) return r;
                }
                var scr = go.GetComponentInParent<SettingsScreen>();
                if (scr != null)
                {
                    var r = Tab(go, scr.FilterBtns, (int)scr.CurFilter, null);
                    if (r != null) return r;
                }
            }
            catch { }
            return null;
        }

        private static FocusReadout ScreenButton(GameObject go)
        {
            var tab = ScreenTab(go);
            if (tab != null) return tab;
            var ub = UpgradeBuildingUI.I;
            if (ub != null && ub.gameObject.activeInHierarchy)
            {
                if (Is(go, ub.BtnUpgradeElevatorUp)) return ElevatorPath(go, "elevator_upgrade_up", "elev_up", ub.WrapperUpgradeElevatorUpNotif);
                if (Is(go, ub.BtnUpgradeElevatorDown)) return ElevatorPath(go, "elevator_upgrade_depth", "elev_down", ub.WrapperUpgradeElevatorDownNotif);
                if (Is(go, ub.BtnUpgrade))
                {
                    var own = Lines(go);
                    var r = new FocusReadout(own.Count > 0 ? own[0] : Loc.Get("btn_upgrade"), Names.Shown(ub.TxtUpgCost), Names.Shown(ub.LocGearProgress));
                    try { if (!ub.BtnUpgrade.IsInteractable()) r.Summary += ", " + Loc.Get("disabled"); } catch { }
                    r.Add(Names.Shown(ub.LocTitle));
                    r.Add(Names.Shown(ub.LocDesc));
                    return r;
                }
            }
            var csu = CharSelectUI.I;
            if (csu != null && csu.gameObject.activeInHierarchy)
            {
                if (Is(go, csu.BtnSelect))
                {
                    // The button that confirms the chosen character: say whom it confirms.
                    var own = Lines(go);
                    string who = "";
                    try { if (csu._selectedItem != null && csu._selectedItem.TgtInfo != null) who = Names.Char(csu._selectedItem.TgtInfo); } catch { }
                    var r = new FocusReadout(own.Count > 0 ? own[0] : Loc.Get("btn_select"), who.Length > 0 ? who : null);
                    try { if (!csu.BtnSelect.IsInteractable()) r.Summary += ", " + Loc.Get("disabled"); } catch { }
                    return r;
                }
                if (Is(go, csu.BtnStats)) { var own = Lines(go); return new FocusReadout(own.Count > 0 ? own[0] : Loc.Get("btn_stats")); }
                if (Is(go, csu.BtnLeaderboards)) return new FocusReadout(Loc.Get("btn_leaderboards"));
                if (Is(go, csu.BtnPrevNGPlus)) return new FocusReadout(Loc.Get("ngplus_prev"));
                if (Is(go, csu.BtnNextNGPlus)) return new FocusReadout(Loc.Get("ngplus_next"));
            }
            var lsu = LevelSelectUI.I;
            if (lsu != null && lsu.gameObject.activeInHierarchy)
            {
                if (Is(go, lsu.BtnSelectBase)) return LevelSet(go, "lvlselect_base", "set_base", false);
                if (Is(go, lsu.BtnSelectDLC)) return LevelSet(go, "lvlselect_dlc", "set_dlc", !Names.HasDlc);
                if (Is(go, lsu.BtnNGPlusLeft)) return new FocusReadout(Loc.Get("ngplus_prev"), Names.Shown(lsu.LocSelectNGPlus));
                if (Is(go, lsu.BtnNGPlusRight)) return new FocusReadout(Loc.Get("ngplus_next"), Names.Shown(lsu.LocSelectNGPlus));
            }
            var en = EncyclopediaUI.I;
            if (en != null && en.gameObject.activeInHierarchy)
            {
                if (Is(go, en.BtnPrevSet)) return new FocusReadout(Loc.Get("enc_prev_set"), Names.Shown(en.LocCurSet));
                if (Is(go, en.BtnNextSet)) return new FocusReadout(Loc.Get("enc_next_set"), Names.Shown(en.LocCurSet));
            }
            return null;
        }

        private static FocusReadout ElevatorPath(GameObject go, string term, string modKey, GameObject notif)
        {
            var own = Lines(go);
            string label = own.Count > 0 ? own[0] : Tr(term);
            if (label.Length == 0) label = Loc.Get(modKey);
            bool ready = false;
            try { ready = notif != null && notif.activeInHierarchy; } catch { }
            var r = new FocusReadout(label, ready ? Loc.Get("elev_ready") : null);
            r.Add(Loc.Get(modKey));
            for (int i = 1; i < own.Count; i++) r.Add(own[i]);
            return r;
        }

        private static FocusReadout LevelSet(GameObject go, string term, string modKey, bool locked)
        {
            var own = Lines(go);
            string label = own.Count > 0 ? own[0] : Tr(term);
            if (label.Length == 0) label = Loc.Get(modKey);
            var r = new FocusReadout(label, locked ? Loc.Get("locked") : null);
            if (locked) { string why = Tr("desc_dlc_content_locked"); if (why.Length > 0) { r.Summary += ". " + why; r.Add(why); } }
            for (int i = 1; i < own.Count; i++) r.Add(own[i]);
            return r;
        }

        private static FocusReadout ReadInner(GameObject go)
        {
            var screen = ScreenButton(go);
            if (screen != null) return screen;

            // ---- Battle: level up, fuser, pause, game over ----
            var lub = Up<LevelUpBtn>(go);
            if (lub != null) return LevelUp(lub);
            var evo = Up<EvoSelectBtn>(go);
            if (evo != null) return Evo(evo);
            var combo = Up<ComboSelectItem>(go);
            if (combo != null) return ComboBall(combo);
            var fuser = Up<FuserOptionBtn>(go);
            if (fuser != null)
            {
                // What each way of using the fuser does, in the game's own words (its tutorial texts).
                var r = new FocusReadout(Names.Shown(fuser.TxtLabel), Names.Shown(fuser.TxtNumAvail), fuser.IsAvailable ? null : Loc.Get("disabled"));
                string term = fuser.Type == FuserOptionType.kCombo ? "tut_fusion" : fuser.Type == FuserOptionType.kFreeUpgrades ? "tut_fission" : fuser.Type == FuserOptionType.kEvo ? "tut_evolution" : null;
                if (term != null) r.Add(Tr(term));
                return r;
            }
            var lvlUp = LevelUpUI.I;
            if (lvlUp != null && lvlUp.gameObject.activeInHierarchy && On(lvlUp.WrapperSelectCombo))
            {
                var fuse = Navigator.FuseButton(lvlUp);
                if (fuse != null && Is(go, fuse)) return FuseConfirm(go, lvlUp);
            }
            var free = Up<FreeUpgradeItem>(go);
            if (free != null)
            {
                return new FocusReadout(FreeUpgrade(free), free.IsSelected ? Loc.Get("selected") : null).Add(Names.UpgradeDesc(free.TgtInf));
            }
            var curEq = Up<LevelUpCurEquipItem>(go);
            if (curEq != null)
            {
                if (curEq.TgtInfo == null) return new FocusReadout(Loc.Get("empty_slot"));
                // The level as the slot shows it (its own number), not the internal counter.
                var shown = Lines(curEq);
                string lv = shown.Count > 0 && shown[0].Length <= 3 ? shown[0] : curEq.DisplayedLvl.ToString();
                return new FocusReadout(Names.Upgrade(curEq.TgtInfo), Loc.F("level_n", lv)).Add(Names.UpgradeDesc(curEq.TgtInfo));
            }
            var petBattle = Up<PetDisplayItemBattle>(go);
            if (petBattle != null) return new FocusReadout(Names.Shown(petBattle.TxtName));
            var pauseEq = Up<PauseEquipInfoBtn>(go);
            if (pauseEq != null) return PauseEquip(pauseEq);
            var stat = Up<GameOverBallStatItem>(go);
            if (stat != null) return BallStat(stat);
            var hud = Up<HUDUpgradeItem>(go);
            if (hud != null) return new FocusReadout(Loc.Get(hud.IsHero ? "kind_ball" : "kind_passive"), Names.Shown(hud.TxtLvl));
            var speed = Up<GameSpeedBtn>(go);
            if (speed != null) return new FocusReadout(Loc.F("speed", Battle.FormatSpeed(PlayerPrefsMgr.GetGameSpeed(speed.TgtSpeed))));

            // ---- Character stats (pause, character sheet): the name with its value ----
            var sdi = Up<StatDisplayItem>(go);
            if (sdi != null)
            {
                string label = Names.Shown(sdi.LocLabel);
                if (label.Length == 0) label = Names.FromEnum(sdi.Type.ToString());
                string change = Names.Shown(sdi.TxtChange);
                var r = new FocusReadout(label + " " + Names.Shown(sdi.TxtValue), change.Length > 0 ? change : null);
                string scaling = Names.Shown(sdi.TxtScaling);
                if (scaling.Length > 0) r.Add(Loc.F("stat_scaling", scaling));
                // The stat's tooltip (what it does) appears on the label's hover; the focus may sit on another part
                // of the row (a player: "the tooltip for the intelligence stat is not read").
                try { sdi.OnLabelHover(); } catch { }
                r.AddAll(MenuReader.HoverLines());
                return r;
            }

            // ---- Base: build, characters, levels ----
            var bi = Up<BuildItem>(go);
            if (bi != null) return Build(bi);
            var cs = Up<CharSelectItem>(go);
            if (cs != null) return CharSelect(cs);
            var lci = Up<LevelCharItem>(go);
            if (lci != null) return new FocusReadout(CharName(lci.TgtChar), LevelName(lci.TgtLvl), lci.TgtDiff > 0 ? Loc.F("difficulty_n", lci.TgtDiff) : null);
            var ls = Up<LevelSelectItem>(go);
            if (ls != null) return LevelSelect(ls, go);
            var clc = Up<CharLevelCompleteItem>(go);
            if (clc != null) return new FocusReadout(LevelName(clc.TgtLvl), CharName(clc.TgtChar));
            var war = Up<WarRoomSelectLvlItem>(go);
            if (war != null) return new FocusReadout(Names.Level(war.TgtLvl), Names.Shown(war.TxtRewards)).AddAll(Lines(war));
            var eqc = Up<EquippedCharBtn>(go);
            if (eqc != null) return EquippedChar(eqc);

            // ---- Workers ----
            var wi = Up<WorkerItem>(go);
            if (wi != null) return Worker(wi, go);
            var awi = Up<AssignWorkerItem>(go);
            if (awi != null)
            {
                var c = awi.GetChar();
                return c == null ? new FocusReadout(Loc.Get("no_worker")) : new FocusReadout(Names.Char(c), Loc.F("level_n", c.Lvl + 1));
            }
            var mi = Up<MasseuseItem>(go);
            if (mi != null) return new FocusReadout(Names.Char(mi._tgtChar), Names.Shown(mi.TxtStamina), Names.Shown(mi.TxtCost));
            var mk = Up<MarketItem>(go);
            if (mk != null)
                return new FocusReadout(Loc.F(mk.IsBuy ? "market_buy" : "market_sell", Names.Shown(mk.TxtResource), Names.Resource(mk.TgtResource), Names.Shown(mk.TxtGold)));

            // ---- Pets ----
            var hi = Up<HatcheryItem>(go);
            if (hi != null) return hi.IsEmpty() ? new FocusReadout(Loc.Get("empty_incubator")) : Pet(hi.GetPet(), hi.DispItem, null);
            var egg = Up<EggListItem>(go);
            if (egg != null) return Pet(egg.GetInst(), egg.DispItem, Names.Shown(egg.TxtCost));
            var pli = Up<PetLabItem>(go);
            if (pli != null) return Is(go, pli.BtnRemove) ? new FocusReadout(Loc.Get("btn_remove")) : Pet(pli._tgtPet, pli.DispItem, null);
            var pls = Up<PetLabSelectItem>(go);
            if (pls != null) return Pet(pls.GetInst(), pls.DispItem, Join(pls.IsSelected ? Loc.Get("selected") : null, pls.IsValid ? null : Loc.Get("disabled")));
            var spi = Up<SelectPetItem>(go);
            if (spi != null) return Pet(spi.GetPet(), spi.DispItem, null);
            var epi = Up<EquippedPetItem>(go);
            if (epi != null) return Pet(epi._tgtPet, epi.DispItem, Loc.F("slot_n", epi.Idx + 1));
            var tube = Up<TestTubeItem>(go);
            if (tube != null) return new FocusReadout(Loc.Get("test_tube"), tube._isOccupied ? Names.Shown(tube.TxtLvl) : Loc.Get("empty_slot"));

            // ---- Collections ----
            var enc = Up<EncyclopediaItem>(go);
            if (enc != null) return Encyclopedia(enc);
            var unl = Up<ItemUnlockItem>(go);
            if (unl != null)
                return unl._isHidden ? new FocusReadout(Loc.Get("unknown_item")) : new FocusReadout(Names.Upgrade(unl.TgtInf)).Add(Names.UpgradeDesc(unl.TgtInf));
            var hsb = Up<HarvestSummaryBuildItem>(go);
            if (hsb != null)
            {
                var b = hsb.GetBld();
                return new FocusReadout(b != null && b.Inst != null ? Names.Building(b.Inst.GetInfo()) : Loc.Get("unlabeled"));
            }

            // ---- Settings, slots, sidebar ----
            var si = Up<SettingsItem>(go);
            if (si != null) return Setting(si, go);
            var ri = Up<SettingsRemapItem>(go);
            if (ri != null) return Remap(ri, go);
            var slot = Up<SelectSlotItem>(go);
            if (slot != null) return Slot(slot, go);
            var side = Up<BaseSidebarBtn>(go);
            if (side != null && !string.IsNullOrEmpty(side.Label))
            {
                string t = Tr(side.Label);
                return new FocusReadout(t.Length > 0 ? t : Names.FromEnum(side.Label), side.IsEnabled ? null : Loc.Get("disabled"), side.IsActiveMode ? Loc.Get("active") : null);
            }
            var input = go.GetComponent<TMP_InputField>();
            if (input != null) return new FocusReadout(Loc.Get("edit_field"), Speech.Clean(input.text));
            return null;
        }

        // ---------------- Battle ----------------

        /// <summary>"Bleed, +1 level": the item a free upgrade goes to and what it gets.</summary>
        private static bool On(GameObject g) { try { return g != null && g.activeInHierarchy; } catch { return false; } }

        /// <summary>A ball offered for a fusion: its name, whether it is chosen, then what it does.</summary>
        private static FocusReadout ComboBall(ComboSelectItem c)
        {
            var ui = LevelUpUI.I;
            string name = Names.Shown(c.TxtName);
            int chosen = 0;
            try { chosen = ui != null && ui._selectedComboBtns != null ? ui._selectedComboBtns.Count : 0; } catch { }
            string state = null;
            if (c.IsSelected) state = chosen == 1 ? Loc.Get("selected") + ". " + Loc.Get("fusion_pick_second") : Loc.Get("selected");
            var r = new FocusReadout(name, state);
            // The ball behind the button: the hero at its index, checked by name (else found by name).
            HeroInst hero = null;
            try
            {
                var heroes = BattleSaveData.I?.Heroes;
                if (heroes != null)
                {
                    if (c.Idx >= 0 && c.Idx < heroes.Count && heroes[c.Idx] != null && string.Equals(Names.Upgrade(heroes[c.Idx].GetInfo()), name, StringComparison.OrdinalIgnoreCase))
                        hero = heroes[c.Idx];
                    else
                        for (int i = 0; i < heroes.Count && hero == null; i++)
                            if (heroes[i] != null && string.Equals(Names.Upgrade(heroes[i].GetInfo()), name, StringComparison.OrdinalIgnoreCase)) hero = heroes[i];
                }
            }
            catch { }
            if (hero != null)
            {
                try { r.Add(Loc.F("level_n", hero.Lvl + 1)); } catch { }
                r.Add(Names.UpgradeDesc(hero.GetInfo()));
            }
            return r;
        }

        /// <summary>The button that fuses the two chosen balls: which ones, then what the fusion gives, as the panel shows it.</summary>
        private static FocusReadout FuseConfirm(GameObject go, LevelUpUI ui)
        {
            var own = Lines(go);
            var r = new FocusReadout(own.Count > 0 ? own[0] : Loc.Get("kind_fusion"));
            try { if (!go.GetComponent<CoolButton>().IsInteractable()) r.Summary += ", " + Loc.Get("disabled"); } catch { }
            var names = new List<string>();
            try
            {
                var sel = ui._selectedComboBtns;
                for (int i = 0; sel != null && i < sel.Count; i++) if (sel[i] != null) names.Add(Names.Shown(sel[i].TxtName));
            }
            catch { }
            if (names.Count > 0) r.Summary += ": " + string.Join(" + ", names);
            r.Add(Names.Shown(ui.LocDetailsComboDesc1));
            r.Add(Names.Shown(ui.LocDetailsComboDesc2));
            r.Add(Names.Shown(ui.LocDetailsComboDetails));
            return r;
        }

        internal static string FreeUpgrade(FreeUpgradeItem free)
        {
            string what = Names.Upgrade(free.TgtInf), gain = Names.Shown(free.Txt);
            if (gain.Length == 0) foreach (var l in Lines(free)) { gain = l; break; }
            return Join(what, gain);
        }

        /// <summary>The text of the visible TMP object with this name under a root ("" when there is none).</summary>
        internal static string NamedText(Component root, string objName)
        {
            try
            {
                if (root == null) return "";
                foreach (var t in root.GetComponentsInChildren<TMP_Text>(false))
                    if (t != null && t.isActiveAndEnabled && t.name == objName) return Speech.Clean(t.text);
            }
            catch { }
            return "";
        }

        private static FocusReadout LevelUp(LevelUpBtn b)
        {
            var ui = LevelUpUI.I;
            string name;
            if (b.ChoiceType == LvlUpChoiceType.kResource) name = b.NumResources + " " + Names.Resource(b.ResType);
            else name = Names.Upgrade(b.TgtInfo);
            // HoverBtn rebuilds the panel's stat rows from a pool: right after it the rows are gone, so every read
            // that hovered first found none (a player: "the stats of the upgrades aren't being read"). Hover only when
            // the panel shows another choice, and then read again shortly.
            bool hovered = false;
            if (ui != null)
            {
                string showing = NamedText(ui, "TxUpgName");
                if (showing.Length == 0 || name.Length == 0 || (showing.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0 && name.IndexOf(showing, StringComparison.OrdinalIgnoreCase) < 0))
                {
                    try { ui.HoverBtn(b); hovered = true; } catch { }
                }
            }
            if (name.Length == 0 && ui != null) name = Names.Shown(ui.LocDetailsName);
            string kind = null;
            try
            {
                if (b.ChoiceType == LvlUpChoiceType.kHeroCombo) kind = Loc.Get("kind_fusion");
                else if (b.ChoiceType == LvlUpChoiceType.kMerge) kind = Loc.Get("kind_evolution");
                else if (b.TgtInfo != null) kind = KindOf(b.TgtInfo);
            }
            catch { }
            string lvl = Names.Shown(b.TxtLvl);
            if (lvl.Length == 0 && b.IsNew) lvl = Loc.Get("new");
            var r = new FocusReadout(name, kind, lvl);
            string desc = "";
            bool filled = false;
            if (ui != null)
            {
                // The panel that follows the hover. Its visible text gets the numbers a frame or more after the
                // hover (the params manager re-localizes it), so the term is filled from the params here; the
                // visible texts are the fallback (log of 2026-11-14: "durante segundos (máximo )").
                desc = Names.Filled(ui.LocDetailsDesc, ui.ParamsDetailsDesc);
                filled = desc.Length > 0;
                if (desc.Length == 0) desc = NamedText(ui, "TxUpgDesc");
                if (desc.Length == 0) desc = Names.Shown(ui.LocDetailsDesc);
                r.Add(Names.Shown(ui.LocDetailsCat));
                r.Add(NamedText(ui, "TxUpgTitle"));
                r.Add(desc);
                r.Add(Names.Shown(ui.LocDetailsComboDesc1));
                r.Add(Names.Shown(ui.LocDetailsComboDesc2));
                r.Add(Names.Shown(ui.LocDetailsComboDetails));
                foreach (var line in StatRows(ui, b.transform)) r.Add(line);
            }
            // The panel follows the hover a frame or two late; its text is the one with the numbers filled in
            // (the bare translation has placeholders the mod can only drop).
            if (ui != null && b.ChoiceType != LvlUpChoiceType.kResource)
            {
                string panelName = Names.Shown(ui.LocDetailsName);
                if (panelName.Length == 0) panelName = NamedText(ui, "TxUpgName");
                if (desc.Length == 0 || (!filled && HasParams(b.TgtInfo)) || (panelName.Length > 0 && name.Length > 0 && !string.Equals(panelName, name, StringComparison.OrdinalIgnoreCase))) r.Retry = true;
            }
            if (desc.Length == 0) { desc = Names.UpgradeDesc(b.TgtInfo); r.Add(desc); }
            if (hovered && b.ChoiceType != LvlUpChoiceType.kResource) r.Retry = true;
            if (Config.ReadDescriptions && desc.Length > 0) r.Summary += ". " + desc;
            return r;
        }

        /// <summary>
        /// The level-up panel's stat rows ("Damage 27-48 to 36-66", "Bleed Stacks 2 to 4"): every active LevelUpStatRow of
        /// the screen. The label's text sits on a child of the Localize object ("RowTop/Label"), so Names.Shown read
        /// nothing and the rows were never spoken (a player: "the stats of the upgrades aren't being read").
        /// </summary>
        private static int _statRowLogs;

        internal static List<string> StatRows(LevelUpUI ui, Transform near)
        {
            var lines = new List<string>();
            try
            {
                // By the texts' own names: each row has a "Label" (in RowTop) and "StartStat" / "NewStat" (in RowBot).
                // Looking the rows up by their component found none, although the screen showed them (2026-11-22).
                var seen = new HashSet<IntPtr>();
                // Each row is a LvlUpStatRow(Clone) with RowTop/Label and RowBot/StartStat, Arrow, NewStat
                // (UI/CvsLevelUp/BackingDetails/Scroll View/Viewport/Lis/...). Found from its "StartStat" text with a
                // scene-wide search; searching the screen's children found no rows although they were on screen.
                var root = near != null ? near.root : ui.transform.root;
                var ordered = new List<KeyValuePair<int, string>>();
                foreach (var st in UnityEngine.Object.FindObjectsOfType<TMP_Text>())
                {
                    if (st == null || st.name != "StartStat" || !st.isActiveAndEnabled) continue;
                    var bot = st.transform.parent;
                    var row = bot != null ? bot.parent : null;
                    if (row == null || !seen.Add(row.Pointer)) continue;
                    string label = "", o = Speech.Clean(st.text), n = "";
                    for (int i = 0; i < row.childCount; i++)
                    {
                        var part = row.GetChild(i);
                        for (int j = 0; j < part.childCount; j++)
                        {
                            var tt = part.GetChild(j).GetComponent<TMP_Text>();
                            if (tt == null || !tt.isActiveAndEnabled) continue;
                            if (tt.name == "Label") label = Speech.Clean(tt.text);
                            else if (tt.name == "NewStat") n = Speech.Clean(tt.text);
                        }
                    }
                    o = NoDash(o); n = NoDash(n);
                    if (label.Length == 0 || (o.Length == 0 && n.Length == 0)) continue;
                    ordered.Add(new KeyValuePair<int, string>(row.GetSiblingIndex(), n.Length > 0 && n != o ? Loc.F("stat_change", label, o.Length > 0 ? o : "0", n) : label + " " + (o.Length > 0 ? o : n)));
                }
                // In the panel's order, top first (the search returns them in any order).
                ordered.Sort((a, c) => a.Key.CompareTo(c.Key));
                foreach (var kv in ordered) lines.Add(kv.Value);
                if (Config.Debug && _statRowLogs++ < 40) Log.Info("Stat rows: " + lines.Count + " under " + root.name + " (LevelUpUI.I under " + ui.transform.root.name + ", same " + (root.Pointer == ui.transform.root.Pointer) + ")");
            }
            catch (Exception ex) { if (Config.Debug) Log.Warn("Stat rows: " + ex.Message); }
            return lines;
        }

        /// <summary>The upgrade's description has {[PARAM]} placeholders (its numbers come from the game).</summary>
        private static bool HasParams(UpgradeInfo inf)
        {
            try { return inf != null && (Il2CppI2.Loc.LocalizationManager.GetTranslation(inf.GetDescSlug()) ?? "").Contains("{["); }
            catch { return false; }
        }

        private static string KindOf(UpgradeInfo inf)
        {
            switch (inf.GetUpgradeType())
            {
                case UpgradeType.kHero: return Loc.Get("kind_ball");
                case UpgradeType.kPassive: return Loc.Get("kind_passive");
                case UpgradeType.kPet: case UpgradeType.kPetUpgrade: return Loc.Get("kind_pet");
            }
            return null;
        }

        // ---------------- Panels that follow the focus ----------------

        private static readonly Dictionary<IntPtr, KeyValuePair<IntPtr, string>> PanelSeen = new Dictionary<IntPtr, KeyValuePair<IntPtr, string>>();

        /// <summary>
        /// A details panel the game fills from the hover a frame or two late (I2 sets its texts later): true
        /// while it still shows, unchanged, what it showed for another item. The reader then sets Retry and
        /// MenuReader looks again shortly (at most 4 times), so the buffer never takes the previous item's
        /// details (user, 2026-11-16: "off by 1" in the encyclopedia when moving fast, "at random").
        /// </summary>
        private static bool PanelBehind(UnityEngine.Object panel, UnityEngine.Object item, List<string> lines)
        {
            if (panel == null || item == null || lines == null || lines.Count == 0) return false;
            string text = string.Join("\n", lines);
            if (PanelSeen.TryGetValue(panel.Pointer, out var seen) && seen.Key != item.Pointer && seen.Value == text)
            {
                Log.Info("Panel behind the focus: " + panel.name + " still shows '" + (text.Length > 60 ? text.Substring(0, 60) : text).Replace("\n", " / ") + "'");
                return true;
            }
            PanelSeen[panel.Pointer] = new KeyValuePair<IntPtr, string>(item.Pointer, text);
            return false;
        }

        /// <summary>The name an equipment panel (encyclopedia, evolution choice, pause equipment) shows right now.</summary>
        private static string PanelName(EquipmentInfoPanel p)
        {
            try
            {
                if (p == null || !p.gameObject.activeInHierarchy) return "";
                string n = Names.ShownAt(p.LocEquipName);
                if (n.Length == 0) n = NamedText(p, "TxtName");
                return n;
            }
            catch { return ""; }
        }

        /// <summary>
        /// The equipment panel does not show this item yet. It fills in a few hundred milliseconds after the focus
        /// moves (measured 2026-11-22: the encyclopedia's panel still named the previous entry 0.08 s after the
        /// move, so the details of each entry were the ones of the entry above). An empty name counts as behind.
        /// </summary>
        private static bool PanelShowsOther(EquipmentInfoPanel p, string name)
        {
            if (p == null || string.IsNullOrEmpty(name)) return false;
            bool on;
            try { on = p.gameObject.activeInHierarchy; } catch { on = false; }
            if (!on) return false;
            string shown = PanelName(p);
            if (shown.Length == 0) return true;
            return shown.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0 && name.IndexOf(shown, StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static List<string> Joined(params List<string>[] parts)
        {
            var all = new List<string>();
            foreach (var l in parts) if (l != null) all.AddRange(l);
            return all;
        }

        private static FocusReadout Evo(EvoSelectBtn e)
        {
            var ui = LevelUpUI.I;
            if (ui != null) try { ui.HoverEvo(e, true); } catch { }
            var inf = e.GetInfo();
            string evoName = Names.Upgrade(inf);
            var r = new FocusReadout(evoName.Length > 0 ? evoName : Loc.Get("unknown_item"), Loc.Get("kind_evolution"));
            // What it is made of: the two icons a sighted player sees on the card ("Iron x Burn").
            string from = "";
            try
            {
                var comps = inf != null && inf.MergeComponents != null && inf.MergeComponents.Length > 0 ? inf.MergeComponents[0] : null;
                var names = new List<string>();
                for (int i = 0; comps != null && i < comps.Length; i++) if (comps[i] != null) names.Add(Names.Upgrade(comps[i]));
                if (names.Count > 0) from = Loc.F("evo_from", string.Join(" + ", names));
            }
            catch { }
            if (from.Length > 0) r.Summary += ", " + from;
            // The description with its numbers: the card's info panel is only shown on a mouse hover, and the bare
            // translation lost its numbers ("Deals - damage", "Inflicts stacks of bleed"). Fill the game's panel for
            // this evolution (EquipmentInfoPanel.SetUpgrade) and read its texts.
            string desc = "", dmg = "";
            var panel = ui != null ? ui.EvoSelectInfoPanel : null;
            if (panel != null && inf != null)
            {
                try
                {
                    panel.SetUpgrade(inf, 0);
                    var dp = panel.DescPanel;
                    if (dp != null)
                    {
                        desc = Names.Filled(dp.LocDesc, dp.ParamsDesc);
                        if (desc.Length == 0 && dp.TxtDesc != null && !dp.TxtDesc.text.Contains("{[")) desc = Speech.Clean(dp.TxtDesc.text);
                    }
                    dmg = Names.Filled(panel.LocEquipDmg, panel.ParamsEquipDmg);
                    if (Config.Debug) Log.Info("Evolution " + evoName + ": desc '" + desc + "', damage '" + dmg + "'");
                }
                catch (Exception ex) { if (Config.Debug) Log.Warn("Evolution panel: " + ex.Message); }
                var lines = Lines(panel);
                if (lines.Count > 0 && PanelShowsOther(panel, evoName)) r.Retry = true;
                if (desc.Length == 0) foreach (var l in lines) if (l != evoName && l.Length > 20) { desc = l; break; }
            }
            if (desc.Length == 0) desc = Names.UpgradeDesc(inf);
            r.Add(from);
            r.Add(dmg);
            r.Add(desc);
            if (Config.ReadDescriptions && desc.Length > 0) r.Summary += ". " + desc;
            return r;
        }

        private static FocusReadout PauseEquip(PauseEquipInfoBtn p)
        {
            // Levels count from 0 inside the game; the game shows them from 1 ("Bleed, level 1" was its Level 2).
            string name = "", lvl = null;
            if (p._tgtHero != null) { name = Names.Upgrade(p._tgtHero.GetInfo()); lvl = Loc.F("level_n", p._tgtHero.Lvl + 1); }
            else if (p._tgtPassive != null) { name = Names.Upgrade(p._tgtPassive.GetInfo()); lvl = Loc.F("level_n", p._tgtPassive.Lvl + 1); }
            else if (p._tgtPet != null)
            {
                var m = p._tgtPet.GetMetaInst();
                name = m != null ? Speech.Clean(m.Name) : Loc.Get("kind_pet");
            }
            var r = new FocusReadout(name, lvl);
            var ui = PauseUI.I;
            // The details panel follows the game's own preview call, not the focus: it kept describing the first item
            // while the focus moved on (a player: "the tooltips are read in the wrong order").
            if (ui != null)
                try
                {
                    if (p._tgtHero != null) ui.PreviewHero(p, p._tgtHero);
                    else if (p._tgtPassive != null) ui.PreviewPassive(p, p._tgtPassive);
                    else if (p._tgtPet != null) ui.PreviewPet(p, p._tgtPet);
                }
                catch (Exception ex) { if (Config.Debug) Log.Warn("Pause preview: " + ex.Message); }
            if (ui != null)
            {
                var panel = Lines(ui.EquipInfoPanel);
                if (PanelBehind(ui.EquipInfoPanel, p, panel) || (p._tgtPet == null && PanelShowsOther(ui.EquipInfoPanel, name)))
                {
                    r.Retry = true;
                    if (Config.Debug) Log.Info("Pause equipment panel behind: focus '" + name + "', panel '" + PanelName(ui.EquipInfoPanel) + "'");
                }
                r.AddAll(panel);
            }
            return r;
        }

        /// <summary>"--" (the game's "no value", e.g. the damage of a passive like the magnet) → "".</summary>
        private static string NoDash(string v) => v.Trim('-', '–', '—', ' ').Length == 0 ? "" : v;

        private static FocusReadout BallStat(GameOverBallStatItem s)
        {
            string name = s.TgtHero != null ? Names.Upgrade(s.TgtHero.GetInfo()) : s.TgtPassive != null ? Names.Upgrade(s.TgtPassive.GetInfo()) : Loc.Get("baby_balls");
            var r = new FocusReadout(name, Names.Shown(s.TxtLvl));
            string dmg = NoDash(Names.Shown(s.TxtDamage)), dps = NoDash(Names.Shown(s.TxtDamagePerSec)), launches = NoDash(Names.Shown(s.TxtLaunches));
            if (dmg.Length > 0) { r.Summary += ", " + Loc.F("stat_damage", dmg); r.Add(Loc.F("stat_damage", dmg)); }
            if (dps.Length > 0) r.Add(Loc.F("stat_dps", dps));
            if (launches.Length > 0) r.Add(Loc.F("stat_launches", launches));
            return r;
        }

        // ---------------- Base ----------------

        private static FocusReadout Build(BuildItem bi)
        {
            var inf = bi.TgtInfo;
            if (inf == null) return null;
            string name = Names.Building(inf);
            var r = new FocusReadout();
            string cost = bi.TxtCost != null ? Speech.Clean(bi.TxtCost.text) : "";
            var parts = new List<string> { name };
            if (bi.IsLocked) parts.Add(Loc.Get("locked"));
            else
            {
                if (cost.Length > 0) parts.Add(cost);
                if (!bi.CanAfford) parts.Add(Loc.Get("cant_afford"));
            }
            // How much room it needs (a player: "so that we can discover how much space we need to build them").
            string size = Game.BaseGrid.Size(inf);
            if (size.Length > 0) parts.Add(size);
            try { if (bi.WrapperNew != null && bi.WrapperNew.activeInHierarchy) parts.Add(Loc.Get("new")); } catch { }
            r.Summary = string.Join(", ", parts);
            var ui = BuildUI.I;
            string shown = ui != null ? Names.Shown(ui.LocDetailsDesc) : "";
            if (ui != null && PanelBehind(ui.LocDetailsDesc, bi, new List<string> { shown })) r.Retry = true;
            string desc = shown.Length > 0 ? shown : Tr(inf.GetBuildDescSlug());
            if (desc.Length == 0) desc = Tr(inf.GetDescSlug());
            r.Add(desc);
            r.Add(cost);
            string full = Names.Cost(inf.GetCost());
            if (full.Length > 0) r.Add(full);
            return r;
        }

        private static string CharName(CharType t)
        {
            try
            {
                var db = InfoDB.I;
                if (db != null && db.Chars != null)
                    foreach (var c in db.Chars) if (c != null && c.Type == t) return Names.Char(c);
            }
            catch { }
            return Names.FromEnum(t.ToString());
        }

        private static string LevelName(LevelType t)
        {
            try
            {
                var db = InfoDB.I;
                if (db != null && db.Levels != null)
                    foreach (var l in db.Levels) if (l != null && l.Type == t) return Names.Level(l);
            }
            catch { }
            return Names.FromEnum(t.ToString());
        }

        private static FocusReadout CharSelect(CharSelectItem cs)
        {
            var inf = cs.TgtInfo;
            if (inf == null) return null;
            CharMetaInst inst = null;
            try { inst = inf.GetInst(); } catch { }
            var ui = CharSelectUI.I;
            bool locked = inst != null && !inst.IsUnlocked;
            var r = new FocusReadout(Names.Char(inf), locked ? Loc.Get("locked") : inst != null ? Loc.F("level_n", inst.Lvl + 1) : null);
            try { if (cs.WrapperBonus != null && cs.WrapperBonus.activeInHierarchy) r.Summary += ", " + Loc.F("bonus_n", Names.Shown(cs.TxtBonusAmt)); } catch { }
            if (locked && ui != null) { string req = Names.Shown(ui.LocReqDesc); if (req.Length > 0) { r.Summary += ". " + req; r.Add(req); } }
            r.Add(Tr(inf.GetGameplayDescSlug()));
            r.Add(Tr(inf.GetFlavorDescSlug()));
            if (inf.StarterUpgrade != null) r.Add(Loc.F("starter_ball", Names.Upgrade(inf.StarterUpgrade)));
            // The stats the character starts a run with (the game's "More Stats..." screen, which threw an exception
            // when opened from the base on a fresh start: 2026-11-22, UpgradeMgr.CalculateStats).
            if (!locked && inst != null) { var st = CharStats(inst, inf); if (st.Length > 0) r.Add(st); }
            if (ui != null)
            {
                var panel = Lines(ui.DetailsPanel);
                if (PanelBehind(ui.DetailsPanel, cs, Joined(panel, new List<string> { Names.Shown(ui.LocReqDesc) }))) r.Retry = true;
                r.AddAll(panel);
            }
            return r;
        }

        /// <summary>"Stats: Endurance 3, Strength 2, ...": a character's stats as the game's stats panel fills them.</summary>
        internal static string CharStats(CharMetaInst meta, CharInfo inf)
        {
            var parts = new List<string>();
            try
            {
                var csu = CharStatsUI.I;
                var grp = csu != null && csu.InfoPanel != null ? csu.InfoPanel.StatGrp : null;
                if (grp != null && meta != null)
                {
                    grp.InitChar(meta);
                    var items = grp.StatItems;
                    for (int i = 0; items != null && i < items.Length; i++)
                    {
                        var it = items[i];
                        if (it == null) continue;
                        string label = StatName(it.Type);
                        string v = it.TxtValue != null ? Speech.Clean(it.TxtValue.text) : "";
                        if (label.Length > 0 && v.Length > 0) parts.Add(label + " " + v);
                    }
                }
            }
            catch (Exception ex) { if (Config.Debug) Log.Warn("CharStats from the panel: " + ex.Message); }
            if (parts.Count == 0)
            {
                // No stats panel to fill: the character's base stats plus the bonus stats it earned.
                try
                {
                    var st = inf != null ? inf.Stats : null;
                    var bonus = meta != null ? meta.BonusStats : null;
                    for (int i = 0; st != null && i < st.Length && i < (int)StatType.kNum; i++)
                    {
                        int v = st[i] + (bonus != null && i < bonus.Length ? bonus[i] : 0);
                        parts.Add(StatName((StatType)i) + " " + v);
                    }
                }
                catch (Exception ex) { if (Config.Debug) Log.Warn("CharStats from the data: " + ex.Message); }
            }
            return parts.Count > 0 ? Loc.F("char_stats", string.Join(", ", parts)) : "";
        }

        private static readonly Dictionary<StatType, string> StatNames = new Dictionary<StatType, string>();

        /// <summary>A stat's name in the game's words, read from the stat rows the game labels (cached once seen).</summary>
        internal static string StatName(StatType t)
        {
            if (StatNames.TryGetValue(t, out var n) && n.Length > 0) return n;
            // The game's term for the stat's label (found with the dev command "terms Endurance").
            // Two keep older names: Strength is "hero_damage", Dexterity "acceleration".
            string key = t == StatType.kStrength ? "hero_damage" : t == StatType.kDexterity ? "acceleration" : t.ToString().Substring(1).ToLowerInvariant();
            n = Tr("Generated/stat_" + key + "_label");
            if (n.Length > 0) { StatNames[t] = n; return n; }
            try
            {
                foreach (var it in Resources.FindObjectsOfTypeAll<StatDisplayItem>())
                {
                    if (it == null || it.Type != t || it.LocLabel == null) continue;
                    string term = it.LocLabel.Term;
                    if (!string.IsNullOrEmpty(term)) n = Tr(term);
                    if (n.Length == 0 && it.LocLabel.GetComponent<TMP_Text>() != null) n = Speech.Clean(it.LocLabel.GetComponent<TMP_Text>().text);
                    if (n.Length > 0) { Log.Info("Stat name " + t + ": term '" + term + "' -> " + n); break; }
                }
            }
            catch { }
            if (n.Length == 0) return Names.FromEnum(t.ToString());
            StatNames[t] = n;
            return n;
        }

        private static FocusReadout LevelSelect(LevelSelectItem ls, GameObject go)
        {
            if (Is(go, ls.BtnDiffLeft)) return new FocusReadout(Loc.Get("btn_easier"), Names.Shown(ls.LocDifName));
            if (Is(go, ls.BtnDiffRight)) return new FocusReadout(Loc.Get("btn_harder"), Names.Shown(ls.LocDifName));
            if (Is(go, ls.BtnPlay)) return new FocusReadout(Loc.Get("btn_play"), Names.Shown(ls.TxtName), Names.Shown(ls.LocDifName)).AddAll(Lines(ls.WrapperDifficulty));
            if (Is(go, ls.BtnLeaderboards)) return new FocusReadout(Loc.Get("btn_leaderboards"));
            string name = Names.Shown(ls.TxtName);
            if (name.Length == 0) name = Names.Level(ls.TgtInfo);
            // The difficulty belongs to the level (Left / Right change it while the level has the focus).
            string diff = null;
            try { if (!ls.IsLocked && ls.WrapperDifficulty != null && ls.WrapperDifficulty.activeInHierarchy) diff = Names.Shown(ls.LocDifName); } catch { }
            string num = Names.Shown(ls.TxtLvl);
            if (num.Contains("?")) num = null;
            var r = new FocusReadout(num, name, ls.IsLocked ? Loc.Get("locked") : null, ls.TgtNGPlus > 0 ? Loc.F("ngplus_n", ls.TgtNGPlus) : null, diff);
            if (ls.IsLocked) { string why = Names.Shown(ls.LocLockedDesc); if (why.Length > 0) { r.Summary += ". " + why; r.Add(why); } }
            r.AddAll(Lines(ls.WrapperInfo));
            r.AddAll(Lines(ls.WrapperInfoLocked));
            try { if (ls.TgtInfo != null) { r.Add(Tr(ls.TgtInfo.GetDescSlug())); if (ls.TgtInfo.BossInfo != null) r.Add(Loc.F("boss_is", Tr(ls.TgtInfo.BossInfo.GetNameSlug(0)))); } } catch { }
            return r;
        }

        private static FocusReadout EquippedChar(EquippedCharBtn e)
        {
            var ui = LoadoutUI.I;
            string name = "";
            if (ui != null)
            {
                var panel = e.Idx == 0 ? ui.CharPanel : ui.Char2Panel;
                if (panel != null) name = Names.Shown(panel.LocName);
            }
            var r = new FocusReadout(Loc.F("char_slot_n", e.Idx + 1), name.Length > 0 ? name : null);
            if (ui != null) r.AddAll(Lines(e.Idx == 0 ? ui.CharPanel : ui.Char2Panel));
            return r;
        }

        private static FocusReadout Worker(WorkerItem w, GameObject go)
        {
            if (Is(go, w.BtnMoveLeft)) return new FocusReadout(Loc.Get("btn_move_earlier"));
            if (Is(go, w.BtnMoveRight)) return new FocusReadout(Loc.Get("btn_move_later"));
            if (w.IsEmpty()) return new FocusReadout(Loc.Get("no_worker"));
            var c = w.GetTgtChar();
            var r = new FocusReadout(Names.Char(c), Names.Shown(w.TxtLvl), w.IsActive ? Loc.Get("worker_going") : Loc.Get("worker_staying"), w.CanWork ? null : Loc.Get("worker_cant"));
            string cap = Names.Shown(w.TxtCapacity);
            if (cap.Length > 0) r.Add(Loc.F("worker_capacity", cap));
            return r;
        }

        private static FocusReadout Pet(PetInst p, PetDisplayItem disp, string extra)
        {
            if (p == null) return new FocusReadout(Loc.Get("empty_slot"), extra);
            string name = Speech.Clean(p.Name);
            string type = disp != null ? Names.Shown(disp.LocType) : "";
            if (name.Length == 0) name = type.Length > 0 ? type : Names.FromEnum(p.Type.ToString());
            var r = new FocusReadout(name, type != name ? type : null, Loc.F("level_n", p.Lvl), extra);
            if (disp != null) { r.Add(Names.Shown(disp.LocDesc)); r.Add(Names.Shown(disp.TxtXP)); }
            return r;
        }

        private static FocusReadout Encyclopedia(EncyclopediaItem e)
        {
            var ui = EncyclopediaUI.I;
            if (ui != null) try { ui.HoverItem(e); } catch { }
            // An entry of the expansion's set without the expansion: the panel says to buy it.
            var dlcLocked = ui != null ? Lines(ui.WrapperDLCContentLocked) : new List<string>();
            if (dlcLocked.Count > 0)
            {
                var lr = new FocusReadout(Loc.Get("locked"));
                lr.Summary += ". " + dlcLocked[0];
                return lr.AddAll(dlcLocked);
            }
            if (e.IsLocked) return new FocusReadout(Loc.Get("unknown_item"), Loc.Get("locked")).AddAll(ui != null ? Lines(ui.WrapperItemDetailsLocked) : null);
            string name = "";
            if (e.TgtInf != null) name = Names.Upgrade(e.TgtInf);
            else if (e.PieceInf != null)
            {
                name = Tr(e.PieceInf.GetNameSlug(0));
                if (name.Length == 0) name = Speech.Clean(e.PieceInf.DisplayName);
            }
            var r = new FocusReadout(name);
            // The details panel follows the hover a frame or two late (I2 fills its texts later), so the
            // buffer took the previous entry's details (user, 2026-11-16: "off by 1", at random). Until the
            // panel names this entry, MenuReader looks again shortly (at most 4 times).
            if (ui != null && e.TgtInf != null && name.Length > 0)
            {
                if (PanelShowsOther(ui.ItemInfPanel, name))
                { r.Retry = true; if (Config.Debug) Log.Info("Encyclopedia panel behind: focus '" + name + "', panel '" + PanelName(ui.ItemInfPanel) + "'"); }
            }
            else if (ui != null && e.PieceInf != null)
            {
                string want = Tr(e.PieceInf.GetGameplayDescSlug()), shown = "";
                try { shown = Names.Shown(ui.LocEnemyGameplayDesc); } catch { }
                // Compare the start only: a placeholder the panel fills in would make them differ further on.
                int n = Math.Min(12, Math.Min(want.Length, shown.Length));
                if (n > 0 && string.Compare(want, 0, shown, 0, n, StringComparison.OrdinalIgnoreCase) != 0)
                { r.Retry = true; Log.Info("Encyclopedia panel behind: focus '" + name + "', panel '" + shown + "'"); }
            }
            if (ui != null)
            {
                var items = Lines(ui.WrapperItemDetailsUnlocked);
                var enemies = Lines(ui.WrapperEnemyDetailsUnlocked);
                if (PanelBehind(ui, e, Joined(items, enemies))) r.Retry = true;
                r.AddAll(items);
                r.AddAll(enemies);
            }
            if (e.TgtInf != null) r.Add(Names.UpgradeDesc(e.TgtInf));
            if (e.PieceInf != null) r.Add(Tr(e.PieceInf.GetGameplayDescSlug()));
            return r;
        }

        // ---------------- Settings ----------------

        internal static string SettingValue(SettingsItem s)
        {
            try
            {
                // A toggle row shows its state as a mark that is drawn only while the option is on.
                if (s.WrapperToggle != null && s.WrapperToggle.activeInHierarchy && s.ImgToggleOn != null)
                {
                    var img = s.ImgToggleOn;
                    bool on = img.gameObject.activeInHierarchy && img.enabled && img.color.a > 0.05f;
                    return Loc.Get(on ? "on" : "off");
                }
                switch (s.DisplayMode)
                {
                    case OptionDisplayMode.kList: return Names.Shown(s.TxtListOption);
                    case OptionDisplayMode.kSlider:
                        {
                            string t = Names.Shown(s.TxtSliderLabel);
                            if (t.Length > 0) return t;
                            var sl = s.Slider;
                            if (sl != null && sl.maxValue > sl.minValue)
                                return Loc.F("percent", Mathf.RoundToInt((sl.value - sl.minValue) / (sl.maxValue - sl.minValue) * 100f));
                            return "";
                        }
                    case OptionDisplayMode.kToggle:
                        {
                            bool? v = ToggleValue(s);
                            return v.HasValue ? Loc.Get(v.Value ? "on" : "off") : "";
                        }
                }
            }
            catch { }
            return "";
        }

        private static bool? ToggleValue(SettingsItem s)
        {
            var tog = s.GetComponentInChildren<CoolToggleButton>(true);
            if (tog != null) return tog.IsToggled;
            var ut = s.GetComponentInChildren<Toggle>(true);
            if (ut != null) return ut.isOn;
            switch (s.TgtOption)
            {
                case OptionType.kFullScreen: return PlayerPrefsMgr.sFullScreen;
                case OptionType.kVSync: return PlayerPrefsMgr.sVSync > 0;
                case OptionType.kFPSCounter: return PlayerPrefsMgr.sShowFPS;
                case OptionType.kPoolLevelUps: return PlayerPrefsMgr.sPoolLevelUps;
                case OptionType.kDisableChanting: return PlayerPrefsMgr.sDisableChanting;
                case OptionType.kReduceFlashing: return PlayerPrefsMgr.sReduceFlashing;
                case OptionType.kDisableDamageNumbers: return PlayerPrefsMgr.sDisableDamageNumbers;
                case OptionType.kSlowerSpeedOptions: return PlayerPrefsMgr.sSlowerSpeedOptions;
                case OptionType.kUseSystemCursor: return PlayerPrefsMgr.sUseSystemCursor;
                case OptionType.kMoveMouseCursorWithMovement: return PlayerPrefsMgr.sMoveMouseCursorWithMovement;
                case OptionType.kShowBuildVersion: return PlayerPrefsMgr.sShowVersionText;
                case OptionType.kUseBloom: return PlayerPrefsMgr.sUseBloom;
                case OptionType.kUseCRT: return PlayerPrefsMgr.sUseCRT;
                case OptionType.kUseChromAberr: return PlayerPrefsMgr.sUseChromAberr;
                case OptionType.kUseVignette: return PlayerPrefsMgr.sUseVignette;
                case OptionType.kSimplifyGraphics: return PlayerPrefsMgr.sSimplifyGraphics;
                case OptionType.kDisableSwirling: return PlayerPrefsMgr.sDisableSwirling;
                case OptionType.kSkipLevelUpStats: return PlayerPrefsMgr.sSkipLevelUpStats;
                case OptionType.kHideBlood: return PlayerPrefsMgr.sHideBlood;
                case OptionType.kVizColliders: return PlayerPrefsMgr.sVizColliders;
                case OptionType.kBatterySaver: return PlayerPrefsMgr.sBatterySaver;
                case OptionType.kMuteAudioBackground: return PlayerPrefsMgr.sMuteAudioBackground;
                case OptionType.kEnableCloudSaves: return PlayerPrefsMgr.sCloudSavesEnabled;
            }
            return null;
        }

        private static FocusReadout Setting(SettingsItem s, GameObject go)
        {
            // A row is one stop (Left / Right change it), whichever of its buttons holds the focus.
            string label = Names.Shown(s.TxtLabel);
            string kind = s.DisplayMode == OptionDisplayMode.kSlider ? Loc.Get("ctl_slider") : s.DisplayMode == OptionDisplayMode.kList ? Loc.Get("ctl_list") : null;
            var r = new FocusReadout(label, SettingValue(s), kind);
            try
            {
                var descs = SettingsItem.kOptionDescs;
                int i = (int)s.TgtOption;
                if (descs != null && i >= 0 && i < descs.Length) r.Add(Tr(descs[i]));
            }
            catch { }
            return r;
        }

        private static FocusReadout Remap(SettingsRemapItem ri, GameObject go)
        {
            string action = Names.Shown(ri.LocAction);
            string k1 = Names.Shown(ri.LocMappedBtn), k2 = Names.Shown(ri.LocMappedBtn2);
            if (Is(go, ri.BtnMap2)) return new FocusReadout(action, Loc.F("remap_key", 2, k2.Length > 0 ? k2 : Loc.Get("none")));
            if (Is(go, ri.BtnMap1)) return new FocusReadout(action, Loc.F("remap_key", 1, k1.Length > 0 ? k1 : Loc.Get("none")));
            return new FocusReadout(action, k1, k2);
        }

        private static FocusReadout Slot(SelectSlotItem s, GameObject go)
        {
            string num = Loc.F("slot_n", s.SlotIdx + 1);
            if (Is(go, s.BtnDeleteSlot)) return new FocusReadout(Loc.F("slot_delete", s.SlotIdx + 1));
            if (Is(go, s.BtnEmpty)) return new FocusReadout(num, Loc.Get("slot_empty"));
            return new FocusReadout(num, Names.Shown(s.TxtLastPlayed), Names.Shown(s.TxtPlayTime)).AddAll(Lines(s.WrapperFull));
        }
    }
}

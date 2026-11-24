using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using BallPitAccess.Game;
using BallPitAccess.Input;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BallPitAccess.UI
{
    /// <summary>
    /// Keyboard navigation for the game's menus (tier B, template 10.4). With a keyboard the game is
    /// mouse-driven: nothing is selected and the arrows do nothing. The mod gives every screen one ordered
    /// list of its usable buttons (BuildList), built from the game's UI structure and never from screen
    /// positions, and moves the game's own selection through it: Select() on a CoolSelectable, OnSubmit()
    /// for Enter. "Usable" means a raycast at the button's centre hits it, which also leaves out everything
    /// behind an open popup; items scrolled out of a list count too and are scrolled into view.
    /// One key grammar on every screen: Up and Down (and Left and Right) walk the list, Home and End go to
    /// the ends of the group, Tab and Shift+Tab jump to the next or previous group, Enter activates (and on
    /// a character or a level lands on the button that confirms it), Left and Right change a list or slider
    /// setting or a level's difficulty, Control+Tab changes the screen's tab, Backspace goes back.
    /// Inactive while a battle is being played (the arrows aim there).
    /// </summary>
    internal static class Navigator
    {
        private static int _errors;
        private static IntPtr _lastOverlay = IntPtr.Zero;
        private static float _overlaySince;
        private static bool _overlayPending;
        private static string _pageKey = "";
        private static string _lastLevelPage = "";
        private static float _pageSince = -1f;
        private static float _nextEnsure;
        private static float _inputUntil;
        private static string _inputText;

        /// <summary>Menus, and any popup on top of the battle (tutorials). Not while the hero is being moved.</summary>
        internal static bool Active =>
            !ModKeys.ListOpen && (Blocking || (!Game.Placement.Active && !Game.Harvest.Active && (FtueShowing || !Battle.Playing)));

        /// <summary>A popup, or a tutorial step waiting for its continue button, is on top: it owns Enter and the arrows.</summary>
        internal static bool Blocking
        {
            get
            {
                if (TopOverlay() != null) return true;
                try { var f = MaskedFTUEOverlay.I; return f != null && f.IsShowing() && f.IsShowingWithBtn(); } catch { return false; }
            }
        }

        /// <summary>Texts of the open screen: the Details buffer when the screen opens, until an item takes the focus (F2 reads them again).</summary>
        internal static List<string> ScreenLines = new List<string>();

        internal static bool HasOverlay => TopOverlay() != null;

        internal static bool FtueShowing
        {
            get { try { var f = MaskedFTUEOverlay.I; return f != null && f.IsShowing(); } catch { return false; } }
        }

        internal static void Update()
        {
            try
            {
                TrackOverlay();
                TrackBanner();
                TrackPage();
                TrackFtue();
                if (TrackInput()) return;
                if (!Active) return;
                EnsureFocus();
                RunFollowUp();
                if (_backCheck != null && _backCheck()) _backCheck = null;
                if (!Keys.GameHasFocus() || Keys.Alt || Keys.ScreenReaderModifier) return;
                if (Keys.Consumed) return;
                if (Keys.Ctrl)
                {
                    // Control+Tab: the screen's own tabs (pages, categories). Control+arrows are the review buffers.
                    if (Keys.Pressed(Keys.VK_TAB) && TopOverlay() != null) Section(Keys.Shift ? -1 : 1);
                    return;
                }
                if (Keys.Pressed(Keys.VK_UP)) { BaseNav.Release(); Step(-1); }
                else if (Keys.Pressed(Keys.VK_DOWN)) { BaseNav.Release(); Step(1); }
                else if (Keys.Pressed(Keys.VK_LEFT)) { BaseNav.Release(); if (!Adjust(-1)) Step(-1); }
                else if (Keys.Pressed(Keys.VK_RIGHT)) { BaseNav.Release(); if (!Adjust(1)) Step(1); }
                else if (Keys.Pressed(Keys.VK_HOME)) { BaseNav.Release(); Edge(-1); }
                else if (Keys.Pressed(Keys.VK_END)) { BaseNav.Release(); Edge(1); }
                else if (Keys.Pressed(Keys.VK_TAB)) { BaseNav.Release(); Group(Keys.Shift ? -1 : 1); }
                else if (Keys.Pressed(Keys.VK_RETURN)) Activate();
                else if (Keys.Pressed(Keys.VK_BACK)) Back();
                else if (Keys.Pressed(Keys.VK_PRIOR) && TopOverlay() != null) SetKey(-1);
                else if (Keys.Pressed(Keys.VK_NEXT) && TopOverlay() != null) SetKey(1);
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Navigator: " + ex);
            }
        }

        // ---------------- Screens (overlays) ----------------

        internal static OverlayUI TopOverlay()
        {
            var stack = OverlayUI.sOverlayStack;
            if (stack == null) return null;
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                var o = stack[i];
                if (o == null || !o.gameObject.activeInHierarchy) continue;
                if (IsBanner(o)) continue;
                return o;
            }
            return null;
        }

        /// <summary>
        /// The battle's tutorial hints stay on screen while the game goes on ("W A S D to move" until the
        /// player has moved). Such a banner is not a screen: it must not take the arrows or the focus.
        /// </summary>
        private static bool IsBanner(OverlayUI o)
        {
            try { var t = o.TryCast<GameTutUI>(); return t != null && !t.ShouldBlockInput(); }
            catch { return false; }
        }

        private static string _bannerText = "";

        /// <summary>Speak a tutorial banner once when its text appears or changes.</summary>
        private static void TrackBanner()
        {
            GameTutUI tut = null;
            try { tut = GameTutUI.I; } catch { }
            if (tut == null || !tut.gameObject.activeInHierarchy || !IsBanner(tut)) { if (tut == null || !tut.gameObject.activeInHierarchy) _bannerText = ""; return; }
            var lines = MenuReader.TextLines(tut.transform);
            if (lines.Count == 0) return;
            string msg = "";
            try { msg = Message(tut, lines, ""); } catch { }
            if (msg.Length == 0 || msg == _bannerText) return;
            // The same hint was just read as a popup with its button: not again as a banner.
            bool spoken = _lastTutMsg == msg;
            _bannerText = msg;
            if (!spoken) Speech.Event(msg);
        }

        private static string _lastTutMsg = "";

        /// <summary>Announce a popup or screen when it becomes the top of the game's overlay stack.</summary>
        private static void TrackOverlay()
        {
            var top = TopOverlay();
            IntPtr p = top != null ? top.Pointer : IntPtr.Zero;
            float now = Time.unscaledTime;
            if (p != _lastOverlay)
            {
                _lastOverlay = p;
                _overlaySince = now;
                _overlayPending = top != null;
                _pageSince = -1f;
                if (top == null) { ScreenLines = new List<string>(); Buffers.SetDetails(null); }
            }
            // A mod list or the battle map is open (the map pauses the battle, which opens the pause menu under it):
            // the screen is announced once they close, if it is still there.
            if (_overlayPending && ModKeys.ListOpen) { _overlaySince = now; return; }
            // Let the panel fill its texts first.
            if (_overlayPending && now - _overlaySince > 0.35f)
            {
                var o = TopOverlay();
                if (o == null) { _overlayPending = false; return; }
                // The run summary counts its numbers up: wait for the final values.
                var over = o.TryCast<GameOverUI>();
                if (over != null && now - _overlaySince < 8f) { try { if (over.IsRunning()) return; } catch { } }
                _overlayPending = false;
                AnnounceOverlay(o);
            }
        }

        /// <summary>Forget the screen waiting to be announced (the battle map closed the pause menu it opened).</summary>
        internal static void Quiet() => _overlayPending = false;

        private static void AnnounceOverlay(OverlayUI o)
        {
            string name = ScreenName(o);
            Log.Info("Overlay: " + o.GetIl2CppType().Name + " (" + MenuReader.Path(o.transform) + ")");
            var lines = MenuReader.TextLines(o.transform);
            string msg = "";
            try { msg = Message(o, lines, name); } catch (Exception ex) { Log.Warn("Overlay message: " + ex.Message); }
            Speech.Event(name + (msg.Length > 0 && msg != name ? ". " + msg : ""));
            ScreenLines = lines;
            Buffers.SetDetails(lines);
            _pageKey = PageKey(o);
            var lsOpen = o.TryCast<LevelSelectUI>();
            _lastLevelPage = lsOpen != null ? lsOpen.CurPage.ToString() : "";
            var cur = Current();
            if (cur == null || !IsUnder(cur.transform, o.transform)) FocusFirst(o);
            LogList(o.GetIl2CppType().Name);
            var pet = o.TryCast<PetNameUI>();
            if (pet != null && pet.InputName != null) try { pet.InputName.ActivateInputField(); } catch { }
        }

        /// <summary>The title the game shows, or the mod's own name for the screen.</summary>
        private static string ScreenName(OverlayUI o)
        {
            string cls = o.GetIl2CppType().Name;
            string title = "";
            try { title = Title(o); } catch { }
            if (title.Length > 0 && title.Length < 60) return title;
            if (Loc.Has("scr_" + cls)) return Loc.Get("scr_" + cls);
            string n = cls.EndsWith("UI") ? cls.Substring(0, cls.Length - 2) : cls;
            return MenuReader.CleanName(n);
        }

        private static string Title(OverlayUI o)
        {
            var lu = o.TryCast<LevelUpUI>();
            if (lu != null)
            {
                string t = Names.Shown(lu.LocTitleMain);
                return t.Length > 0 ? t : Names.Shown(lu.LocTitleSelection);
            }
            var go = o.TryCast<GameOverUI>(); if (go != null) return Names.Shown(go.LocTitle);
            var ss = o.TryCast<SelectSlotUI>(); if (ss != null) return Names.Shown(ss.LocTitle);
            var br = o.TryCast<BaseReturnUI>(); if (br != null) return Names.Shown(br.LocTitle);
            var hs = o.TryCast<HarvestSummaryUI>(); if (hs != null) return Names.Shown(hs.LocTitle);
            var iu = o.TryCast<ItemUnlockUI>(); if (iu != null) return Names.Shown(iu.LocTitle);
            var aw = o.TryCast<AssignWorkerUI>(); if (aw != null) return Names.Shown(aw.LocTitle);
            var ub = o.TryCast<UpgradeBuildingUI>();
            if (ub != null)
            {
                // Whose upgrade this is comes first: the screen itself only shows a picture of it.
                string what = "";
                try
                {
                    if (ub._tgtBuilding != null && ub._tgtBuilding.Inst != null) what = Names.Building(ub._tgtBuilding.Inst.GetInfo());
                    else if (ub.WrapperElevator != null && ub.WrapperElevator.activeInHierarchy) what = Loc.Get("elevator");
                }
                catch { }
                return Readers.Join(what, Names.Shown(ub.LocTitle));
            }
            var bf = o.TryCast<BlueprintFoundUI>(); if (bf != null) return Names.Shown(bf.LocTitle);
            var en = o.TryCast<EncyclopediaUI>(); if (en != null) return Names.Shown(en.LocHeader);
            var fs = o.TryCast<FullScreenMessageUI>(); if (fs != null) return Names.Shown(fs.LocTitle);
            return "";
        }

        /// <summary>What the screen is telling the player (rule 20): its message, never its whole content.</summary>
        private static string Message(OverlayUI o, List<string> lines, string name)
        {
            var d = o.TryCast<DialogUI>();
            if (d != null) return Names.Shown(d.Txt);
            var tut = o.TryCast<GameTutUI>();
            if (tut != null && lines.Count > 0)
            {
                string term = null;
                try { term = tut._curKbTerm; } catch { }
                try { var gt = GameTutMgr.I; if (gt != null && gt.CurTut == GameTutType.kShootControls) term = "mod_tut_shoot"; } catch { }
                // The key glyphs line ("Keys: W A S D") stays; the message itself is rewritten.
                int at = lines[0].StartsWith(Loc.F("keys", "")) && lines.Count > 1 ? 1 : 0;
                lines[at] = Game.Tutorial.Rewrite(term, lines[at]);
                _lastTutMsg = string.Join(". ", lines.GetRange(0, at + 1));
                return _lastTutMsg;
            }
            var rv = o.TryCast<ReviveUI>(); if (rv != null) return Names.Shown(rv.LocDesc);
            var eg = o.TryCast<EggFoundUI>(); if (eg != null) return Names.Shown(eg.LocDesc);
            var fs = o.TryCast<FullScreenMessageUI>(); if (fs != null) return Names.Shown(fs.LocMessage);
            var ms = o.TryCast<MasseuseUI>(); if (ms != null) return Names.Shown(ms.LocInstructions);
            var iu = o.TryCast<ItemUnlockUI>(); if (iu != null) return Names.Shown(iu.LocDesc);
            var aw = o.TryCast<AssignWorkerUI>(); if (aw != null) return Readers.Join(Names.Shown(aw.LocCurWorker), Names.Shown(aw.LocDesc));
            var ub = o.TryCast<UpgradeBuildingUI>(); if (ub != null) return UpgradeMessage(ub);
            var lsu = o.TryCast<LevelSelectUI>();
            if (lsu != null)
            {
                string alt = lsu.CurPage == LevelSelectPage.kDLCLevels ? Altitude(lsu) : "";
                return alt.Length > 0 ? alt + ". " + Loc.Get("hint_LevelSelectUI") : Loc.Get("hint_LevelSelectUI");
            }
            var hs = o.TryCast<HarvestSummaryUI>(); if (hs != null) return Names.Shown(hs.TxtResources);
            var br = o.TryCast<BaseReturnUI>(); if (br != null) return Names.Shown(br.TxtResources);
            var bf = o.TryCast<BlueprintFoundUI>();
            if (bf != null)
            {
                if (bf.TgtBuilding != null && bf.CurPage == BlueprintFoundPage.kBlueprint)
                    return Readers.Join(Names.Building(bf.TgtBuilding), Names.Tr(bf.TgtBuilding.GetBuildDescSlug()));
                return Names.Shown(bf.LocGearDesc);
            }
            var go = o.TryCast<GameOverUI>();
            if (go != null)
            {
                var parts = new List<string>();
                Pair(parts, Names.Shown(go.LocLabelTime), Names.Shown(go.TxtFinalTime));
                Pair(parts, Names.Shown(go.LocLabelBest), Names.Shown(go.TxtBest));
                Pair(parts, Names.Shown(go.LocLabelKills), Names.Shown(go.TxtNumKills));
                Pair(parts, Names.Shown(go.LocLabelXP), Names.Shown(go.TxtNumXP));
                Pair(parts, Names.Shown(go.LocLabelResources), Names.Shown(go.TxtResources));
                // Blueprints, the level trophy and the lift gears (pit and Risen Ballbylon) won by this run.
                Pair(parts, "", Names.ShownAt(go.ParamsBlueprintsFound));
                Pair(parts, Names.Shown(go.LocTrophyName), Names.Shown(go.LocTrophyBonus));
                Pair(parts, "", Names.ShownAt(go.ParamsGearsFound));
                Pair(parts, "", Names.Shown(go.LocHowToEarnGears));
                return string.Join(". ", parts);
            }
            var pn = o.TryCast<PetNameUI>();
            if (pn != null) return Loc.F("pet_name_prompt", pn.InputName != null ? Speech.Clean(pn.InputName.text) : "");
            var lu = o.TryCast<LevelUpUI>();
            if (lu != null) return LevelUpMessage(lu);
            var enc = o.TryCast<EncyclopediaUI>();
            if (enc != null)
            {
                string st = EncyclopediaStats(enc);
                return Readers.Join(Loc.Get("hint_EncyclopediaUI")) + (st.Length > 0 ? ". " + st : "");
            }
            if (o.TryCast<PauseUI>() != null || o.TryCast<BasePauseUI>() != null || o.TryCast<BaseSettingsUI>() != null ||
                o.TryCast<BuildUI>() != null || o.TryCast<WorkerUI>() != null || o.TryCast<CharSelectUI>() != null ||
                o.TryCast<LevelSelectUI>() != null || o.TryCast<EncyclopediaUI>() != null || o.TryCast<MarketUI>() != null ||
                o.TryCast<HatcheryUI>() != null || o.TryCast<PetLabUI>() != null || o.TryCast<LoadoutUI>() != null ||
                o.TryCast<LeaderboardUI>() != null || o.TryCast<SelectSlotUI>() != null)
                return Loc.Has("hint_" + o.GetIl2CppType().Name) ? Loc.Get("hint_" + o.GetIl2CppType().Name) : "";
            foreach (var l in lines) if (l != name) return l;
            return "";
        }

        /// <summary>The encyclopedia's statistics page: every figure with its label (they are not buttons, so they are read here).</summary>
        private static string EncyclopediaStats(EncyclopediaUI en) => string.Join(". ", EncyclopediaStatLines(en));

        /// <summary>One line per statistic ("Enemies killed in total 317"): the open Statistics tab's details.</summary>
        internal static List<string> EncyclopediaStatLines(EncyclopediaUI en)
        {
            var parts = new List<string>();
            try
            {
                foreach (var it in en.GetComponentsInChildren<EncyclopediaStatItem>(false))
                {
                    if (it == null || !it.isActiveAndEnabled) continue;
                    string label = Names.Shown(it.LocLabel), val = Names.Shown(it.TxtStat);
                    if (label.Length > 0) parts.Add(label + " " + val);
                }
            }
            catch { }
            return parts;
        }

        private static void Pair(List<string> parts, string label, string value)
        {
            if (value.Length > 0) parts.Add(label.Length > 0 ? label + " " + value : value);
        }

        /// <summary>Building or lift upgrade: what it does, its cost, and for the lift the gears collected.</summary>
        private static string UpgradeMessage(UpgradeBuildingUI ub)
        {
            string gears = "";
            try { if (ub.WrapperElevator != null && ub.WrapperElevator.activeInHierarchy) gears = Names.Shown(ub.LocGearProgress); } catch { }
            return Readers.Join(Names.Shown(ub.LocDesc), Names.Shown(ub.TxtUpgCost), gears);
        }

        /// <summary>The altitude sickness notice of the Risen Ballbylon level list, as the game words it.</summary>
        private static string Altitude(LevelSelectUI ls)
        {
            try
            {
                var p = ls.ParamsAltitudeSickness;
                if (p == null || !p.gameObject.activeInHierarchy) return "";
                // The notice is a small group (title + effect lines); never climb into the whole level list.
                var parent = p.transform.parent;
                if (parent != null)
                {
                    var lines = MenuReader.TextLines(parent);
                    if (lines.Count > 0 && lines.Count <= 5) return string.Join(". ", lines);
                }
                return Names.ShownAt(p);
            }
            catch { return ""; }
        }

        private static string NgPlus(LevelSelectUI ls)
        {
            try { return ls.WrapperSelectNGPlus != null && ls.WrapperSelectNGPlus.activeInHierarchy ? Names.Shown(ls.LocSelectNGPlus) : ""; }
            catch { return ""; }
        }

        /// <summary>Page Up / Page Down: the screen's second selector (encyclopedia set, new game plus tier).</summary>
        private static void SetKey(int dir)
        {
            var o = TopOverlay();
            try
            {
                var en = o.TryCast<EncyclopediaUI>();
                if (en != null)
                {
                    var b = dir < 0 ? en.BtnPrevSet : en.BtnNextSet;
                    if (b == null || !b.gameObject.activeInHierarchy || !b.IsInteractable()) { Speech.SayMenu(Loc.Get("no_sets")); return; }
                    if (dir < 0) en.OnPrevSetClicked(); else en.OnNextSetClicked();
                    Log.Info("Encyclopedia set " + en.CurSet);
                    return;
                }
                var ls = o.TryCast<LevelSelectUI>();
                if (ls != null)
                {
                    var b = dir < 0 ? ls.BtnNGPlusLeft : ls.BtnNGPlusRight;
                    if (b == null || !b.gameObject.activeInHierarchy || !b.IsInteractable()) { Speech.SayMenu(Loc.Get("no_ngplus")); return; }
                    if (dir < 0) ls.OnPrevNGPlusClicked(); else ls.OnNextNGPlusClicked();
                    Log.Info("New game plus " + ls._selectedNGPlus);
                    return;
                }
            }
            catch (Exception ex) { Log.Warn("SetKey: " + ex.Message); return; }
            Speech.SayMenu(Loc.Get("no_sets"));
        }

        private static string LevelUpMessage(LevelUpUI lu)
        {
            if (lu.CurPage == LevelUpPage.kViewStats)
            {
                // Stat gains for this level, as the game lists them.
                var gains = new List<string>();
                var all = new List<string>();
                try
                {
                    if (lu.WrapperBasicInfo != null && lu.WrapperBasicInfo.activeInHierarchy)
                        foreach (var it in lu.WrapperBasicInfo.GetComponentsInChildren<StatDisplayItem>(false))
                        {
                            if (it == null) continue;
                            string label = Names.Shown(it.LocLabel), val = Names.Shown(it.TxtValue), ch = Names.Shown(it.TxtChange);
                            if (label.Length == 0) continue;
                            all.Add(label + " " + val);
                            if (ch.Length > 0) gains.Add(label + " " + val + " (" + ch + ")");
                        }
                }
                catch { }
                if (gains.Count > 0) return string.Join(", ", gains);
                if (all.Count > 0) return string.Join(", ", all);
                var l = lu.WrapperBasicInfo != null && lu.WrapperBasicInfo.activeInHierarchy ? MenuReader.TextLines(lu.WrapperBasicInfo.transform) : new List<string>();
                return l.Count > 0 ? string.Join(", ", l.GetRange(0, Math.Min(8, l.Count))) : "";
            }
            if (lu.CurPage == LevelUpPage.kPickUpgrade)
            {
                string gold = Names.Shown(lu.TxtCurGold);
                // The gold text already carries the word (its coin icon).
                string word = Loc.Get("res_gold");
                if (gold.Length > 0 && gold.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0) gold = Loc.F("gold_n", gold);
                return Loc.Get("hint_levelup") + (gold.Length > 0 ? " " + gold : "");
            }
            // The result of a fission or a treasure: what each item got.
            var got = new List<string>();
            try
            {
                foreach (var f in lu.GetComponentsInChildren<FreeUpgradeItem>(false))
                    if (f != null && f.isActiveAndEnabled) { string s = Readers.FreeUpgrade(f); if (s.Length > 0) got.Add(s); }
            }
            catch { }
            if (got.Count > 0) return string.Join(". ", got);
            return Loc.Get("hint_fuser");
        }

        /// <summary>A screen or one of its pages is about to be announced: the focus echo waits for it.</summary>
        internal static bool Settling => _overlayPending || _pageSince >= 0;

        // ---------------- Pages inside a screen ----------------

        private static string PageKey(OverlayUI o)
        {
            try
            {
                var lu = o.TryCast<LevelUpUI>();
                if (lu != null)
                {
                    // The same page is refilled after a pick (pooled level ups) or a reroll: the first choice tells.
                    string first = "";
                    try { var b = lu.Btns; if (b != null && b.Length > 0 && b[0] != null && b[0].TgtInfo != null) first = b[0].TgtInfo.Pointer.ToString() + ":" + b[0].TgtLvl; } catch { }
                    return "lu" + (int)lu.CurPage + "/" + (int)lu._selectedFuserOption + "/" + On(lu.WrapperSelectEvo) + On(lu.WrapperSelectCombo) + On(lu.WrapperFuserOptions) + "/" + first;
                }
                var pa = o.TryCast<PauseUI>(); if (pa != null) return "GamePausePage." + pa.CurPage;
                var bf = o.TryCast<BlueprintFoundUI>(); if (bf != null) return "BlueprintFoundPage." + bf.CurPage;
                var aw = o.TryCast<AssignWorkerUI>(); if (aw != null) return "AssignWorkerPage." + aw.CurPage;
                var ha = o.TryCast<HatcheryUI>(); if (ha != null) return "HatcheryPage." + ha.CurPage;
                var pl = o.TryCast<PetLabUI>(); if (pl != null) return "PetLabPage." + pl.CurPage;
                var en = o.TryCast<EncyclopediaUI>(); if (en != null) return "EncycloPage." + en.CurPage + "/" + (int)en.CurSet;
                var ls = o.TryCast<LevelSelectUI>(); if (ls != null) return "LevelSelectPage." + ls.CurPage + "/" + ls._selectedNGPlus;
                var ub = o.TryCast<UpgradeBuildingUI>(); if (ub != null) return "UpgradeBuildingPage." + ub.CurPage;
                var ci = o.TryCast<GameCharInfoUI>(); if (ci != null) return "GameCharPage." + ci.CurPage;
                var st = o.TryCast<BaseSettingsUI>(); if (st != null && st.Scrn != null) return "SettingsFilter." + st.Scrn.CurFilter;
                var bu = o.TryCast<BuildUI>(); if (bu != null) return "BuildingCat." + bu.TgtCat;
                var lb = o.TryCast<LeaderboardUI>(); if (lb != null) return "LBPage." + lb.CurPage;
            }
            catch { }
            return "";
        }

        private static string On(GameObject g) { try { return g != null && g.activeInHierarchy ? "1" : "0"; } catch { return "?"; } }

        /// <summary>A screen switched page (tab, step of a flow): say which, then put focus on it.</summary>
        private static void TrackPage()
        {
            if (_overlayPending) return;
            var o = TopOverlay();
            if (o == null) return;
            float now = Time.unscaledTime;
            string key = PageKey(o);
            if (key != _pageKey)
            {
                _pageKey = key;
                _pageSince = now;
            }
            if (_pageSince < 0 || now - _pageSince < 0.3f) return;
            _pageSince = -1f;
            Log.Info("Page: " + key);
            string label = "";
            var en2 = o.TryCast<EncyclopediaUI>();
            var lu = o.TryCast<LevelUpUI>();
            if (lu != null)
            {
                string t = "";
                try { t = Title(o); } catch { }
                string m = "";
                try { m = LevelUpMessage(lu); } catch { }
                label = t.Length > 0 && m.Length > 0 ? t + ". " + m : t + m;
            }
            else
            {
                int dot = key.IndexOf('.');
                string val = dot >= 0 ? key.Substring(dot + 1) : key;
                int slash = val.IndexOf('/');
                if (slash >= 0) val = val.Substring(0, slash);
                var en = o.TryCast<EncyclopediaUI>();
                string set = en != null ? Names.Shown(en.LocCurSet) : "";
                label = Readers.Join(Names.Mod("page_", val), set.Length > 0 ? set : null);
                var ls = o.TryCast<LevelSelectUI>();
                if (ls != null)
                {
                    // The game's own names for the two level sets; the altitude notice only when the set changes.
                    string own = val == "kBaseLevels" ? Names.Tr("lvlselect_base") : val == "kDLCLevels" ? Names.Tr("lvlselect_dlc") : "";
                    bool samePage = val == _lastLevelPage;
                    _lastLevelPage = val;
                    label = samePage ? NgPlus(ls) : Readers.Join(own.Length > 0 ? own : label, NgPlus(ls));
                    if (!samePage && val == "kDLCLevels") { string alt = Altitude(ls); if (alt.Length > 0) label += ". " + alt; }
                }
                var ub = o.TryCast<UpgradeBuildingUI>();
                if (ub != null)
                {
                    string m = "";
                    try { m = UpgradeMessage(ub); } catch { }
                    string t = Names.Shown(ub.LocTitle);
                    label = Readers.Join(val == "kDefault" && t.Length > 0 ? t : label) + (m.Length > 0 ? ". " + m : "");
                }
            }
            bool jump = _tabJump;
            _tabJump = false;
            if (en2 != null) { string st = EncyclopediaStats(en2); if (st.Length > 0) label += ". " + st; }
            // The tab the player just chose, in the game's own words.
            if (jump && _tabLabel.Length > 0 && lu == null && en2 == null) label = _tabLabel;
            if (label.Length > 0) Speech.Event(label);
            ScreenLines = MenuReader.TextLines(o.transform);
            Buffers.SetDetails(ScreenLines);
            if (jump) { LogList("page " + key); FocusFirst(o); return; }
            var cur = Current();
            if (cur == null || !IsUnder(cur.transform, o.transform) || IndexOf(BuildList("page " + key), cur) < 0) FocusFirst(o);
            else MenuReader.ForceAnnounce(cur);
        }

        private static string _tabLabel = "";
        private static bool _tabJump;

        /// <summary>Control+Tab / Control+Shift+Tab: the next or previous tab of the screen.</summary>
        /// <summary>The screen's tab buttons and the index of the open one; null when it has none.</summary>
        private static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<CoolButton> ScreenTabs(OverlayUI o, out int cur)
        {
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<CoolButton> tabs = null;
            cur = 0;
            if (o == null) return null;
            try
            {
                var pa = o.TryCast<PauseUI>(); if (pa != null) { tabs = pa.NavBtns; cur = (int)pa.CurPage; }
                var st = o.TryCast<BaseSettingsUI>(); if (st != null && st.Scrn != null) { tabs = st.Scrn.FilterBtns; cur = (int)st.Scrn.CurFilter; }
                var en = o.TryCast<EncyclopediaUI>(); if (en != null) { tabs = en.NavBtns; cur = (int)en.CurPage; }
                var bu = o.TryCast<BuildUI>(); if (bu != null) { tabs = bu.CatBtns; cur = (int)bu.TgtCat; }
                var lu = o.TryCast<LevelUpUI>(); if (lu != null && On(lu.WrapperEquipTabNav) == "1") { tabs = lu.EquipTabBtns; cur = (int)lu.CurEquipTab; }
            }
            catch { }
            return tabs;
        }

        /// <summary>
        /// Enter on a tab that is not open: open it and move into what it shows, as Control+Tab does.
        /// False when <paramref name="go"/> is not such a tab.
        /// </summary>
        private static bool OpenTab(GameObject go)
        {
            var o = TopOverlay();
            var tabs = ScreenTabs(o, out int cur);
            if (tabs == null || PageKey(o).Length == 0) return false;
            for (int i = 0; i < tabs.Length; i++)
            {
                var b = tabs[i];
                if (b == null || b.gameObject.Pointer != go.Pointer) continue;
                if (i == cur) return false;
                Click(b);
                Log.Info("Tab " + MenuReader.Path(b.transform));
                var own = MenuReader.TextLines(b.transform);
                _tabLabel = own.Count > 0 ? own[0] : "";
                _tabJump = true;
                return true;
            }
            return false;
        }

        private static void Section(int dir)
        {
            var o = TopOverlay();
            var tabs = ScreenTabs(o, out int cur);
            if (tabs == null || tabs.Length < 2) { Speech.SayMenu(Loc.Get("no_tabs")); return; }
            for (int step = 1; step <= tabs.Length; step++)
            {
                int i = ((cur + dir * step) % tabs.Length + tabs.Length) % tabs.Length;
                var b = tabs[i];
                if (b == null || !b.gameObject.activeInHierarchy || !b.IsInteractable()) continue;
                if (i == cur) break;
                try
                {
                    Click(b);
                    Log.Info("Section " + MenuReader.Path(b.transform));
                    var own = MenuReader.TextLines(b.transform);
                    _tabLabel = own.Count > 0 ? own[0] : "";
                    _tabJump = true;
                }
                catch (Exception ex) { Log.Warn("Section: " + ex.Message); }
                // Screens without a page value to watch: say the tab itself.
                if (PageKey(o).Length == 0) Focus(b);
                return;
            }
            Speech.SayMenu(Loc.Get("no_tabs"));
        }

        // ---------------- Guided tutorial (FTUE) ----------------

        private static string _ftueText = "";
        private static float _ftueSince;
        private static bool _ftuePending;

        /// <summary>
        /// The game's guided tutorial darkens the screen, shows an instruction and points at one button
        /// (BtnTgt) or shows a continue button. Speak the instruction and focus what it points at, so Enter
        /// does what the tutorial asks.
        /// </summary>
        private static void TrackFtue()
        {
            var f = MaskedFTUEOverlay.I;
            bool showing = f != null && f.IsShowing();
            string text = showing && f.TxtDesc != null ? Speech.Clean(f.TxtDesc.text) : "";
            float now = Time.unscaledTime;
            if (text != _ftueText)
            {
                _ftueText = text;
                _ftueSince = now;
                _ftuePending = text.Length > 0;
            }
            if (!_ftuePending || now - _ftueSince < 0.4f) return;
            _ftuePending = false;
            Log.Info("FTUE: " + text + " target " + (f.BtnTgt != null ? MenuReader.Path(f.BtnTgt.transform) : "none") + " continue " + f.IsShowingWithBtn());
            string term = null;
            try { term = f.GetCurArgs()?.Desc; } catch { }
            Speech.Event(Game.Tutorial.Rewrite(term, text));
            CoolSelectable target = null;
            if (f.IsShowingWithBtn() && f.BtnContinue != null && f.BtnContinue.gameObject.activeInHierarchy) target = f.BtnContinue;
            else if (f.BtnTgt != null && f.BtnTgt.gameObject.activeInHierarchy) target = f.BtnTgt;
            if (target != null) Focus(target);
        }

        // ---------------- Text fields ----------------

        /// <summary>A text field is being edited (or was a moment ago): letters are typing, not HUD keys.</summary>
        internal static bool Typing => Time.unscaledTime < _inputUntil;

        /// <summary>While a text field is being edited the keyboard belongs to it: echo what is typed, navigate nothing.</summary>
        private static bool TrackInput()
        {
            var cur = Current();
            TMP_InputField field = null;
            var pet = TopOverlay()?.TryCast<PetNameUI>();
            if (pet != null) field = pet.InputName;
            else if (cur != null) field = cur.GetComponent<TMP_InputField>();
            float now = Time.unscaledTime;
            if (field != null && field.isFocused)
            {
                _inputUntil = now + 0.25f;
                string t = field.text ?? "";
                if (_inputText != null && t != _inputText)
                {
                    if (t.Length == _inputText.Length + 1 && t.StartsWith(_inputText)) Speech.SayMenu(t.Substring(t.Length - 1));
                    else Speech.SayMenu(t.Length > 0 ? t : Loc.Get("blank"));
                }
                _inputText = t;
                return true;
            }
            _inputText = null;
            return now < _inputUntil;
        }

        // ---------------- Focus ----------------

        private static void FocusFirst(OverlayUI o)
        {
            try
            {
                var d = o.DefaultFocus;
                if (d != null && d.gameObject.activeInHierarchy)
                {
                    // A row or wrapper as default focus keeps the selection on itself, where Enter does nothing:
                    // take the first item inside it.
                    if (d.TryCast<CoolButton>() == null)
                    {
                        foreach (var c in BuildList())
                            if (IsUnder(c.Sel.transform, d.transform)) { Focus(c.Sel); return; }
                    }
                    else { Focus(d); return; }
                }
            }
            catch { }
            var list = BuildList();
            if (list.Count > 0) Focus(list[0].Sel);
        }

        /// <summary>A screen is open and nothing is focused (the game dropped the selection): take the first item.</summary>
        private static void EnsureFocus()
        {
            float now = Time.unscaledTime;
            if (now < _nextEnsure) return;
            _nextEnsure = now + 0.6f;
            if (_overlayPending || _ftuePending || _pageSince >= 0 || Current() != null) return;
            var o = TopOverlay();
            if (o != null)
            {
                if (now - _overlaySince > 1f) FocusFirst(o);
                return;
            }
            // Main menu: land on the first entry so the player hears where they are.
            var mm = MainMenuUI.I;
            if (mm != null && mm.gameObject.activeInHierarchy && !mm.IsRunningSplashScreen && mm.GrpBtns != null && mm.GrpBtns.alpha > 0.9f && mm.GrpBtns.interactable)
            {
                var list = BuildList();
                if (list.Count > 0) Focus(list[0].Sel);
            }
        }

        private static bool IsUnder(Transform t, Transform root)
        {
            for (var p = t; p != null; p = p.parent) if (p.Pointer == root.Pointer) return true;
            return false;
        }

        private static GameObject Current()
        {
            var es = EventSystem.current;
            var go = es != null ? es.currentSelectedGameObject : null;
            return go != null && go.activeInHierarchy ? go : null;
        }

        /// <summary>Where the focused object is in the screen's list: itself, the item it is part of, or the item inside it.</summary>
        private static int IndexOf(List<Cand> list, GameObject cur)
        {
            if (cur == null) return -1;
            for (int i = 0; i < list.Count; i++) if (list[i].Sel.gameObject.Pointer == cur.Pointer) return i;
            for (int i = 0; i < list.Count; i++) if (IsUnder(cur.transform, list[i].Sel.transform)) return i;
            for (int i = 0; i < list.Count; i++) if (IsUnder(list[i].Sel.transform, cur.transform)) return i;
            return -1;
        }

        /// <summary>
        /// Up / Down (and Left / Right where they change no value): the previous or next item of the screen's
        /// list. The list is ordered by what things are (see <see cref="BuildList"/>), never by where they are
        /// drawn. It does not wrap: the ends knock and repeat the item.
        /// </summary>
        private static void Step(int dir)
        {
            var list = BuildList();
            if (list.Count == 0) { SayEmpty(); return; }
            var cur = Current();
            if (cur == null)
            {
                var top = TopOverlay();
                if (top != null) { FocusFirst(top); if (Current() != null) return; }
                Focus(list[0].Sel);
                return;
            }
            int i = IndexOf(list, cur);
            if (i < 0) { Focus(list[dir > 0 ? 0 : list.Count - 1].Sel); return; }
            int j = i + dir;
            if (j < 0 || j >= list.Count) { MenuReader.Repeat(); Audio.Cues.Play("wall", 0f, 0.5f); return; }
            Focus(list[j].Sel);
        }

        /// <summary>
        /// "Nothing to select here", only for a screen that is open and settled. A key pressed while a popup
        /// is still building, during a battle (the arrows aim there) or in a transition stays silent (log of
        /// 2026-11-14: the message came during the level descent, a tutorial popup and the run's end).
        /// </summary>
        private static void SayEmpty()
        {
            if (TopOverlay() == null || Battle.InBattle || Settling) return;
            Speech.SayMenu(Loc.Get("nothing"));
        }

        /// <summary>Home / End: the first or last item of the current group.</summary>
        private static void Edge(int dir)
        {
            var list = BuildList();
            if (list.Count == 0) { SayEmpty(); return; }
            int i = IndexOf(list, Current());
            if (i < 0) { Focus(list[dir > 0 ? list.Count - 1 : 0].Sel); return; }
            int j = i;
            while (j + dir >= 0 && j + dir < list.Count && list[j + dir].Section == list[i].Section) j += dir;
            if (j == i) { MenuReader.Repeat(); Audio.Cues.Play("wall", 0f, 0.5f); return; }
            Focus(list[j].Sel);
        }

        /// <summary>Tab / Shift+Tab: the first item of the next or previous group of the screen (wraps).</summary>
        private static void Group(int dir)
        {
            var list = BuildList();
            if (list.Count == 0) { SayEmpty(); return; }
            int i = IndexOf(list, Current());
            if (i < 0) { Focus(list[0].Sel); return; }
            int n = list[list.Count - 1].Section + 1;
            if (n < 2) { Speech.SayMenu(Loc.Get("one_group")); return; }
            int target = ((list[i].Section + dir) % n + n) % n;
            int first = -1, count = 0;
            for (int k = 0; k < list.Count; k++)
                if (list[k].Section == target) { if (first < 0) first = k; count++; }
            if (first < 0) return;
            string header = "";
            try { header = GroupHeader(list[first].Sel.transform); } catch { }
            if (count > 1) MenuReader.NextSuffix = Loc.F("group_n", count);
            if (header.Length > 0) Speech.SayMenu(header);
            Focus(list[first].Sel);
        }

        /// <summary>The title the game puts over a group of items (a header object just before the group or inside its container).</summary>
        private static string GroupHeader(Transform item)
        {
            var box = item.parent;
            if (box == null) return "";
            for (int i = 0; i < box.childCount; i++)
            {
                var ch = box.GetChild(i);
                if (ch.gameObject.activeInHierarchy && ch.name.ToLowerInvariant().Contains("header"))
                {
                    var l = MenuReader.TextLines(ch);
                    if (l.Count > 0) return l[0];
                }
            }
            var up = box.parent;
            if (up == null) return "";
            int at = box.GetSiblingIndex();
            if (at > 0)
            {
                var prev = up.GetChild(at - 1);
                if (prev.gameObject.activeInHierarchy && prev.name.ToLowerInvariant().Contains("header"))
                {
                    var l = MenuReader.TextLines(prev);
                    if (l.Count > 0) return l[0];
                }
            }
            return "";
        }

        /// <summary>Move the game's mouse hover along with the focus: its details panels and tooltips follow the hover.</summary>
        private static void Hover(GameObject prev, GameObject next)
        {
            var es = EventSystem.current;
            if (prev != null)
            {
                var pb = prev.GetComponent<CoolButton>();
                if (pb != null) try { pb.OnPointerExit(new PointerEventData(es)); } catch { }
            }
            if (next != null)
            {
                var b = next.GetComponent<CoolButton>();
                if (b != null) try { b.OnPointerEnter(new PointerEventData(es)); } catch { }
            }
        }

        internal static void Focus(CoolSelectable s)
        {
            if (s == null) return;
            var prev = Current();
            try { ScrollIntoView(s.transform); } catch (Exception ex) { if (Config.Debug) Log.Warn("Scroll: " + ex.Message); }
            try { s.Select(MoveDirection.None); } catch (Exception ex) { Log.Warn("Select: " + ex.Message); }
            var es = EventSystem.current;
            GameObject now = s.gameObject;
            if (es != null)
            {
                // A container passes the selection on to one of its children: keep that child.
                var sel = es.currentSelectedGameObject;
                if (sel != null && sel.activeInHierarchy && sel.Pointer != now.Pointer && IsUnder(sel.transform, s.transform)) now = sel;
                else if (sel == null || sel.Pointer != now.Pointer) es.SetSelectedGameObject(now);
            }
            Hover(prev, now);
            MenuReader.ForceAnnounce(now);
        }

        /// <summary>An item of a scrolling list that is out of view: move the list so the item is inside its viewport.</summary>
        private static void ScrollIntoView(Transform item)
        {
            var sr = item.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (sr == null || sr.content == null) return;
            var rt = item.TryCast<RectTransform>();
            if (rt == null) return;
            RectTransform view = sr.viewport;
            if (view == null) view = sr.transform.TryCast<RectTransform>();
            if (view == null) return;
            Vector3 a = view.InverseTransformPoint(rt.TransformPoint(new Vector3(rt.rect.xMin, rt.rect.yMin, 0f)));
            Vector3 b = view.InverseTransformPoint(rt.TransformPoint(new Vector3(rt.rect.xMax, rt.rect.yMax, 0f)));
            float lo = Math.Min(a.y, b.y), hi = Math.Max(a.y, b.y), left = Math.Min(a.x, b.x), right = Math.Max(a.x, b.x);
            Rect vr = view.rect;
            const float pad = 12f;
            float dx = 0f, dy = 0f;
            if (sr.vertical && hi - lo < vr.height)
            {
                if (hi > vr.yMax - pad) dy = vr.yMax - pad - hi;
                else if (lo < vr.yMin + pad) dy = vr.yMin + pad - lo;
            }
            if (sr.horizontal && right - left < vr.width)
            {
                if (right > vr.xMax - pad) dx = vr.xMax - pad - right;
                else if (left < vr.xMin + pad) dx = vr.xMin + pad - left;
            }
            if (dx == 0f && dy == 0f) return;
            // Viewport units to the content's parent units (the content is normally the viewport's child).
            var content = sr.content;
            Vector3 delta = new Vector3(dx, dy, 0f);
            var parent = content.parent;
            if (parent != null && parent.Pointer != view.Pointer) delta = parent.InverseTransformVector(view.TransformVector(delta));
            sr.StopMovement();
            content.localPosition = content.localPosition + delta;
            if (Config.Debug) Log.Info("Scrolled " + MenuReader.Path(sr.transform) + " by " + delta);
        }

        // ---------------- Activation ----------------

        /// <summary>
        /// Press a button that does not have the focus (a tab, a side bar button): run its click handler.
        /// Submit is not enough there: a button ignores it unless it has been the selected one for a moment.
        /// </summary>
        internal static void Click(CoolButton b)
        {
            var h = b.OnClicked;
            if (h != null) { h.Invoke(); return; }
            b.Select(MoveDirection.None);
            b.OnSubmit(new BaseEventData(EventSystem.current));
        }

        private static Action _followUp;
        private static float _followAt;

        private static void After(float delay, Action a) { _followUp = a; _followAt = Time.unscaledTime + delay; }

        /// <summary>
        /// The fuser's confirm button (its label is "Fusion!" while balls are being chosen, "Wow" after a
        /// fission): the visible button named BtnContinue under the fuser panel. Null when it is not shown.
        /// </summary>
        internal static CoolButton FuseButton(LevelUpUI ui)
        {
            try
            {
                foreach (var b in ui.GetComponentsInChildren<CoolButton>(false))
                    if (b != null && b.name == "BtnContinue" && b.gameObject.activeInHierarchy && b.transform.parent != null && b.transform.parent.name == "RowBtns"
                        && MenuReader.Path(b.transform).Contains("WrapperGotFusion"))
                        return b;
                // Not under that name: the one other button of the fuser panel (not a ball, an option or Close).
                foreach (var b in ui.GetComponentsInChildren<CoolButton>(false))
                    if (b != null && b.gameObject.activeInHierarchy && MenuReader.Path(b.transform).Contains("WrapperGotFusion")
                        && b.GetComponentInParent<ComboSelectItem>() == null && b.GetComponentInParent<FuserOptionBtn>() == null
                        && b.GetComponentInParent<FreeUpgradeItem>() == null && !b.name.ToLowerInvariant().Contains("close"))
                    {
                        Log.Info("Fuse button by elimination: " + MenuReader.Path(b.transform));
                        return b;
                    }
            }
            catch { }
            return null;
        }

        private static void RunFollowUp()
        {
            if (_followUp == null || Time.unscaledTime < _followAt) return;
            var f = _followUp;
            _followUp = null;
            try { f(); } catch (Exception ex) { Log.Warn("Follow-up: " + ex.Message); }
        }

        private static void Activate()
        {
            var cur = Current();
            if (cur == null) { Step(1); return; }
            var btn = cur.GetComponent<CoolButton>();
            if (btn != null && !btn.IsInteractable()) { Speech.SayMenu(Loc.Get("disabled")); return; }
            // The guided tutorial's continue button ignores Submit: use the overlay's own click handler.
            var f = MaskedFTUEOverlay.I;
            if (f != null && f.IsShowingWithBtn() && f.BtnContinue != null && f.BtnContinue.gameObject.Pointer == cur.Pointer)
            {
                f.OnBtnClicked();
                Log.Info("FTUE continue clicked");
                return;
            }
            // Fission / treasure results: the items only show what each one got, and Enter on them did nothing
            // (a playtest driver got stuck there); their Continue ("Wow") is a group away: Enter presses it.
            if (cur.GetComponentInParent<FreeUpgradeItem>() != null)
            {
                var lu = cur.GetComponentInParent<LevelUpUI>();
                var cont = lu != null ? FuseButton(lu) : null;
                if (cont != null && cont.IsInteractable()) { Click(cont); return; }
            }
            var input = cur.GetComponent<TMP_InputField>();
            if (input != null) { try { input.ActivateInputField(); } catch { } return; }
            var sel = cur.GetComponent<CoolSelectable>();
            if (sel == null) return;
            if (OpenTab(cur)) return;
            // Choosing an item leads to the button that confirms it (a character and its Select, a level and its Play).
            CharSelectItem charItem = null;
            LevelSelectItem levelItem = null;
            try
            {
                charItem = cur.GetComponentInParent<CharSelectItem>();
                var li = cur.GetComponentInParent<LevelSelectItem>();
                if (li != null && li.Btn != null && li.Btn.gameObject.Pointer == cur.Pointer) levelItem = li;
            }
            catch { }
            // A build list item right after a category change ignored OnSubmit (the game's submit gate stayed
            // shut until the focus moved): its own click handler starts the placement every time.
            var buildItem = cur.GetComponentInParent<BuildItem>();
            if (buildItem != null && buildItem.Btn != null && buildItem.Btn.gameObject.Pointer == cur.Pointer)
            {
                try { Click(buildItem.Btn); Log.Info("Build item clicked: " + MenuReader.Path(cur.transform)); }
                catch (Exception ex) { Log.Warn("Build item click: " + ex.Message); }
                MenuReader.Recheck(0.25f);
                return;
            }
            try { sel.OnSubmit(new BaseEventData(EventSystem.current)); Log.Info("Submit " + MenuReader.Path(cur.transform)); }
            catch (Exception ex) { Log.Warn("Submit: " + ex.Message); }
            MenuReader.Recheck(0.25f);
            if (levelItem != null && Keys.Shift)
            {
                try
                {
                    var lb = levelItem.BtnLeaderboards;
                    if (lb != null && lb.gameObject.activeInHierarchy && lb.IsInteractable()) { Log.Info("Level: leaderboard"); levelItem.OnLBClicked(); }
                    else Speech.SayMenu(Loc.Get("disabled"));
                }
                catch (Exception ex) { Log.Warn("Level leaderboard: " + ex.Message); }
                return;
            }
            ComboSelectItem comboItem = null;
            try { comboItem = cur.GetComponentInParent<ComboSelectItem>(); } catch { }
            if (comboItem != null)
                // A fusion takes two balls: once both are chosen, focus goes to the button that fuses them.
                After(0.3f, () =>
                {
                    var top = TopOverlay();
                    var ui = top != null ? top.TryCast<LevelUpUI>() : null;
                    if (ui == null) return;
                    int n = 0;
                    try { n = ui._selectedComboBtns != null ? ui._selectedComboBtns.Count : 0; } catch { }
                    var b = FuseButton(ui);
                    if (n >= 2 && b != null && b.IsInteractable()) { Log.Info("Fusion: two balls, focus on the fuse button"); Focus(b); }
                });
            else if (charItem != null)
                After(0.35f, () =>
                {
                    var top = TopOverlay();
                    var ui = top != null ? top.TryCast<CharSelectUI>() : null;
                    if (ui == null) return;
                    var b = ui.BtnSelect;
                    if (b != null && b.gameObject.activeInHierarchy && b.IsInteractable()) { Log.Info("Character chosen: focus on Select"); Focus(b); }
                    else MenuReader.Repeat();
                });
            else if (levelItem != null)
            {
                // The level is selected by the focus already; Enter is its Play button.
                try
                {
                    var b = levelItem.BtnPlay;
                    if (!levelItem.IsLocked && b != null && b.gameObject.activeInHierarchy && b.IsInteractable())
                    {
                        Log.Info("Level: play");
                        levelItem.OnPlayClicked();
                    }
                    else After(0.35f, () =>
                    {
                        var top = TopOverlay();
                        if (top == null || top.TryCast<LevelSelectUI>() == null) return;
                        var b2 = levelItem.BtnPlay;
                        if (!levelItem.IsLocked && b2 != null && b2.gameObject.activeInHierarchy) Speech.SayMenu(Loc.Get("level_enter_again"));
                        else MenuReader.Repeat();
                    });
                }
                catch (Exception ex) { Log.Warn("Level play: " + ex.Message); }
            }
        }

        /// <summary>Left / Right on a settings row (list or slider) or on a level's Play button (difficulty): change the value instead of moving.</summary>
        private static bool Adjust(int dir)
        {
            var cur = Current();
            if (cur == null) return false;
            try
            {
                var li = cur.GetComponentInParent<LevelSelectItem>();
                if (li != null && ((li.Btn != null && li.Btn.gameObject.Pointer == cur.Pointer) || (li.BtnPlay != null && li.BtnPlay.gameObject.Pointer == cur.Pointer)))
                {
                    var arrow = dir < 0 ? li.BtnDiffLeft : li.BtnDiffRight;
                    if (arrow == null || !arrow.gameObject.activeInHierarchy) { MenuReader.Repeat(); return true; }
                    if (!arrow.IsInteractable()) { MenuReader.Repeat(); Audio.Cues.Play("wall", 0f, 0.5f); return true; }
                    if (dir < 0) li.OnDiffLeftClicked(); else li.OnDiffRightClicked();
                    MenuReader.Recheck(0.15f);
                    return true;
                }
            }
            catch (Exception ex) { Log.Warn("Difficulty: " + ex.Message); }
            SettingsItem si;
            try { si = cur.GetComponentInParent<SettingsItem>(); } catch { return false; }
            if (si == null) return false;
            try
            {
                if (si.DisplayMode == OptionDisplayMode.kList)
                {
                    if (dir < 0) si.OnLeftClicked(); else si.OnRightClicked();
                }
                else if (si.DisplayMode == OptionDisplayMode.kSlider && si.Slider != null)
                {
                    var sl = si.Slider;
                    float step = (sl.maxValue - sl.minValue) / 20f;
                    if (sl.wholeNumbers) step = Math.Max(1f, Mathf.Round(step));
                    sl.value = Mathf.Clamp(sl.value + dir * step, sl.minValue, sl.maxValue);
                }
                else return false;
            }
            catch (Exception ex) { Log.Warn("Adjust: " + ex.Message); return false; }
            MenuReader.Recheck(0.12f);
            return true;
        }

        /// <summary>Backspace: the screen's own back handler (what the controller's B button does).</summary>
        private static void Back()
        {
            // The game may treat the same key press as its own "back": act two frames later, and only if the
            // screen is still the one the key was pressed on (otherwise one press would go back twice).
            var at = TopOverlay();
            IntPtr was = at != null ? at.Pointer : IntPtr.Zero;
            BaseState st = BaseState.kNum;
            try { if (BaseMgr.I != null) st = BaseMgr.I.CurState; } catch { }
            int frame = Time.frameCount + 2;
            _backCheck = () =>
            {
                if (Time.frameCount < frame) return false;
                var now = TopOverlay();
                BaseState st2 = BaseState.kNum;
                try { if (BaseMgr.I != null) st2 = BaseMgr.I.CurState; } catch { }
                if ((now != null ? now.Pointer : IntPtr.Zero) == was && st2 == st) BackNow();
                else Log.Info("Back: the game handled the key itself");
                return true;
            };
        }

        private static Func<bool> _backCheck;

        private static void BackNow()
        {
            try
            {
                var o = TopOverlay();
                if (o != null) { bool handled = o.OnBPressed(); Log.Info("Back on " + o.GetIl2CppType().Name + ": " + handled); return; }
                var bm = BaseMgr.I;
                if (bm != null && bm.CurState != BaseState.kNormal)
                {
                    var st = bm.CurState;
                    // The game's back (OnBPressed) only leaves the harvest aim, the launcher and the level
                    // transitions; expansion and rearrange are left by pressing their side bar button again.
                    var ui = BaseUI.I;
                    if (st == BaseState.kSelectingExpansion && ui != null) ui.OnExpandClicked();
                    else if (st == BaseState.kRearrangeBuildings && ui != null) ui.OnRearrangeClicked();
                    else bm.OnBPressed();
                    Log.Info("Back in base state " + st);
                }
            }
            catch (Exception ex) { Log.Warn("Back: " + ex.Message); }
        }

        // ---------------- The screen's list ----------------

        private sealed class Cand
        {
            public CoolSelectable Sel;
            public Vector2 Pos;
            public bool Scrolled;       // in a scrolling list, out of view
            public bool Close, Info;
            public IntPtr Container;
            public int Section;
        }

        private sealed class Node
        {
            public int Sibling;
            public Cand C;
            public IntPtr Ptr;
            public List<Node> Kids = new List<Node>();
        }

        /// <summary>
        /// The focusable items of the current screen as one ordered list with groups. The order comes from
        /// the game's own UI structure (which container holds what, and the order of its children), so the
        /// items of a grid stay together, an item's own buttons follow it, and nothing is interleaved because
        /// it happens to be drawn on the same row. Screen coordinates take no part in the order.
        /// Order: the screen's content in structure order, then its close button, then hover-only texts
        /// (controls without any click handler, whose text is already in the details).
        /// A group is the set of items that share a container; Tab moves between groups.
        /// </summary>
        private static List<Cand> BuildList(string logAs = null)
        {
            var raw = Candidates();
            var top = TopOverlay();
            IntPtr underlay = IntPtr.Zero;
            try { if (top != null && top.BtnUnderlay != null) underlay = top.BtnUnderlay.gameObject.Pointer; } catch { }

            bool ftueOn = FtueShowing;
            bool levelScreen = false;
            try { levelScreen = top != null && top.TryCast<LevelSelectUI>() != null; } catch { }
            // A settings row is one stop (Left / Right change it): its two arrow buttons are not items.
            var rows = new Dictionary<IntPtr, List<Cand>>();
            var arrows = new HashSet<Cand>();
            var keep = new List<Cand>();
            foreach (var c in raw)
            {
                if (c.Sel.gameObject.Pointer == underlay) continue;
                // A screen or popup lists its own controls only: whatever else is still clickable around it
                // (the battle's speed buttons beside a tutorial popup) is not part of it. The guided tutorial
                // is the exception: it points at buttons of the screen underneath.
                if (top != null && !ftueOn && !IsUnder(c.Sel.transform, top.transform)) continue;
                if (c.Sel.gameObject.name.ToLowerInvariant().Contains("cheat")) continue;      // touch-screen debug button
                try
                {
                    var slot = c.Sel.GetComponentInParent<LevelUpCurEquipItem>();
                    if (slot != null && slot.TgtInfo == null) continue;       // an empty equipment slot
                }
                catch { }
                if (levelScreen && !LevelInnerButtons)
                {
                    // The game keeps the selection on the selected level, so its inner buttons cannot hold the
                    // focus: the level itself is the stop (Left / Right difficulty, Enter plays).
                    LevelSelectItem li = null;
                    try { li = c.Sel.GetComponentInParent<LevelSelectItem>(); } catch { }
                    if (li != null && (li.Btn == null || li.Btn.gameObject.Pointer != c.Sel.gameObject.Pointer)) continue;
                }
                SettingsItem si = null;
                try { si = c.Sel.GetComponentInParent<SettingsItem>(); } catch { }
                if (si != null)
                {
                    if (!rows.TryGetValue(si.Pointer, out var l)) rows[si.Pointer] = l = new List<Cand>();
                    l.Add(c);
                    bool arrow = false;
                    try
                    {
                        arrow = (si.BtnListLeft != null && si.BtnListLeft.gameObject.Pointer == c.Sel.gameObject.Pointer) ||
                                (si.BtnListRight != null && si.BtnListRight.gameObject.Pointer == c.Sel.gameObject.Pointer);
                    }
                    catch { }
                    if (arrow) arrows.Add(c);
                    c.Container = si.Pointer;       // marks "part of a settings row" until the tree sets the real container
                }
                keep.Add(c);
            }
            var drop = new HashSet<Cand>();
            foreach (var kv in rows)
            {
                bool hasMain = false;
                foreach (var c in kv.Value) if (!arrows.Contains(c)) hasMain = true;
                bool first = true;
                foreach (var c in kv.Value)
                {
                    if (hasMain) { if (arrows.Contains(c)) drop.Add(c); }
                    else { if (!first) drop.Add(c); first = false; }
                }
            }
            keep.RemoveAll(c => drop.Contains(c));

            // Hover-only controls: no click, press or right-click handler, not a toggle, not a known item.
            bool anyHandler = false;
            var handler = new Dictionary<Cand, bool>();
            foreach (var c in keep)
            {
                bool h = true;
                try
                {
                    var b = c.Sel.TryCast<CoolButton>();
                    if (b != null) h = b.OnClicked != null || b.OnPressed != null || b.OnRightClicked != null;
                }
                catch { h = true; }
                handler[c] = h;
                if (h) anyHandler = true;
            }
            IntPtr ftueA = IntPtr.Zero, ftueB = IntPtr.Zero, deflt = IntPtr.Zero;
            try
            {
                var f = MaskedFTUEOverlay.I;
                if (f != null && f.IsShowing())
                {
                    if (f.BtnTgt != null) ftueA = f.BtnTgt.gameObject.Pointer;
                    if (f.BtnContinue != null) ftueB = f.BtnContinue.gameObject.Pointer;
                }
                if (top != null && top.DefaultFocus != null) deflt = top.DefaultFocus.gameObject.Pointer;
            }
            catch { }
            foreach (var c in keep)
            {
                var go = c.Sel.gameObject;
                string n = go.name.ToLowerInvariant();
                // Leaving the screen and links out of the game come after the screen's own content.
                c.Close = n.Contains("close") || n.Contains("discord");
                bool settingsRow = c.Container != IntPtr.Zero;
                c.Container = IntPtr.Zero;
                if (!anyHandler || handler[c] || settingsRow || c.Close) continue;
                if (go.Pointer == ftueA || go.Pointer == ftueB || go.Pointer == deflt) continue;
                try
                {
                    if (go.GetComponent<CoolToggleButton>() != null || go.GetComponent<TMP_InputField>() != null) continue;
                    if (go.GetComponentInParent<SettingsRemapItem>() != null) continue;
                }
                catch { }
                if (Readers.IsItem(go)) continue;
                c.Info = true;
            }

            // The structure: every item under its chain of parents.
            var rank = new Dictionary<IntPtr, int>();
            try
            {
                var lsu = top != null ? top.TryCast<LevelSelectUI>() : null;
                if (lsu != null)
                {
                    foreach (var arr in new[] { lsu.Items, lsu.ItemsDLC })
                        if (arr != null)
                            for (int k = 0; k < arr.Length; k++)
                                if (arr[k] != null) rank[arr[k].transform.Pointer] = k;
                }
            }
            catch { }
            // The base side bar: what you came to do first (play, harvest, build), housekeeping last.
            try
            {
                var bu = BaseUI.I;
                if (bu != null && top == null)
                {
                    Rank(rank, bu.BtnPlay, 0); Rank(rank, bu.BtnWorkers, 1); Rank(rank, bu.BtnBuild, 2);
                    Rank(rank, bu.BtnExpand, 3); Rank(rank, bu.BtnRearrange, 4); Rank(rank, bu.BtnOptions, 5);
                }
            }
            catch { }
            try
            {
                var lu = top != null ? top.TryCast<LevelUpUI>() : null;
                if (lu != null)
                {
                    if (lu.Btns != null && lu.Btns.Length > 0 && lu.Btns[0] != null) Lead(rank, lu.Btns[0].transform, 0);
                    if (lu.BtnSelectUpgrade != null) Lead(rank, lu.BtnSelectUpgrade.transform, 1);
                    if (lu.BtnReroll != null) Lead(rank, lu.BtnReroll.transform, 2);
                    if (lu.BtnBanish != null) Lead(rank, lu.BtnBanish.transform, 3);
                }
            }
            catch { }
            var nodes = new Dictionary<IntPtr, Node>();
            var root = new Node();
            var chain = new List<Transform>();
            foreach (var c in keep)
            {
                chain.Clear();
                for (var t = c.Sel.transform; t != null; t = t.parent) chain.Add(t);
                Node at = root;
                for (int k = chain.Count - 1; k >= 0; k--)
                {
                    var t = chain[k];
                    if (!nodes.TryGetValue(t.Pointer, out var nd))
                    {
                        nd = new Node { Ptr = t.Pointer, Sibling = rank.TryGetValue(t.Pointer, out int rk) ? rk : t.GetSiblingIndex() };
                        nodes[t.Pointer] = nd;
                        at.Kids.Add(nd);
                    }
                    at = nd;
                }
                at.C = c;
            }
            var ordered = new List<Cand>();
            Walk(root, IntPtr.Zero, false, ordered);

            var list = new List<Cand>();
            foreach (var c in ordered) if (!c.Close && !c.Info) list.Add(c);
            // Groups: items that share a container; lone items in a row form one group (a button bar).
            int section = -1;
            IntPtr last = new IntPtr(-1);
            var size = new List<int>();
            foreach (var c in list)
            {
                if (c.Container != last) { section++; size.Add(0); last = c.Container; }
                c.Section = section;
                size[section]++;
            }
            int merged = -1;
            bool prevLone = false;
            int prevSection = -1;
            foreach (var c in list)
            {
                if (c.Section != prevSection)
                {
                    bool lone = size[c.Section] == 1;
                    if (!(lone && prevLone)) merged++;
                    prevLone = lone;
                    prevSection = c.Section;
                }
                c.Container = new IntPtr(merged);
            }
            foreach (var c in list) c.Section = (int)c.Container;
            int lastSection = list.Count > 0 ? list[list.Count - 1].Section : -1;
            int lastSize = 0;
            foreach (var c in list) if (c.Section == lastSection) lastSize++;
            // Close goes with a short last group (Upgrade, Close), or stands alone after a long list.
            int closeSection = lastSection >= 0 && lastSize <= 3 ? lastSection : lastSection + 1;
            bool anyClose = false;
            foreach (var c in ordered) if (c.Close) { c.Section = closeSection; list.Add(c); anyClose = true; }
            int infoSection = (list.Count > 0 ? list[list.Count - 1].Section : -1) + 1;
            if (!anyClose && lastSection < 0) infoSection = 0;
            foreach (var c in ordered) if (c.Info && !c.Close) { c.Section = infoSection; list.Add(c); }

            if (logAs != null)
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < list.Count; i++)
                {
                    var c = list[i];
                    string own = "";
                    try { var tl = MenuReader.TextLines(c.Sel.transform); if (tl.Count > 0) own = tl[0]; } catch { }
                    sb.AppendLine($"  {i + 1}. group {c.Section + 1}{(c.Close ? " close" : "")}{(c.Info ? " info" : "")}{(c.Scrolled ? " scrolled" : "")} {MenuReader.Path(c.Sel.transform)} [{own}]");
                }
                if (logAs == "dump" && top != null)
                {
                    sb.AppendLine("  Raw texts:");
                    try
                    {
                        foreach (var t in top.GetComponentsInChildren<TMP_Text>(false))
                            if (t != null && t.isActiveAndEnabled && !string.IsNullOrWhiteSpace(t.text))
                                sb.AppendLine("    " + t.transform.parent.name + "/" + t.name + ": " + t.text.Replace("\n", " | "));
                    }
                    catch { }
                }
                Log.Info("List (" + logAs + "): " + list.Count + " items" + Environment.NewLine + sb);
            }
            return list;
        }

        /// <summary>Experiment switch: list a level's inner buttons (they only hold the focus if the game lets them).</summary>
        internal static bool LevelInnerButtons;

        /// <summary>Put the branch that holds this item ahead of its neighbours at every level of the structure.</summary>
        private static void Lead(Dictionary<IntPtr, int> rank, Transform t, int order)
        {
            for (var p = t; p != null; p = p.parent)
                if (!rank.ContainsKey(p.Pointer)) rank[p.Pointer] = -1000 + order;
        }

        private static void Rank(Dictionary<IntPtr, int> rank, BaseSidebarBtn b, int order)
        {
            if (b == null || b.Btn == null) return;
            // The button and the row that holds it (rows are the children that get compared).
            var t = b.Btn.transform;
            rank[t.Pointer] = order;
            if (t.parent != null) rank[t.parent.Pointer] = order;
            rank[b.transform.Pointer] = order;
        }

        private static void Walk(Node n, IntPtr container, bool insideItem, List<Cand> into)
        {
            if (n.C != null) { n.C.Container = container; into.Add(n.C); insideItem = true; }
            n.Kids.Sort((a, b) => a.Sibling.CompareTo(b.Sibling));
            // What hangs under an item belongs to that item's group; otherwise a branching point starts a group.
            IntPtr inner = insideItem ? container : n.Kids.Count >= 2 ? n.Ptr : container;
            foreach (var k in n.Kids) Walk(k, inner, insideItem, into);
        }

        /// <summary>Write the current screen's list to the log (for the next playtest report).</summary>
        internal static void LogList(string why) { try { BuildList(why); } catch (Exception ex) { Log.Warn("List: " + ex.Message); } }

        private static readonly Il2CppSystem.Collections.Generic.List<RaycastResult> Hits = new Il2CppSystem.Collections.Generic.List<RaycastResult>();

        /// <summary>
        /// Every button the player could use now: a raycast at its centre reaches it (so nothing covers it,
        /// which also leaves out whatever is behind a popup), or it sits in a scrolling list of the current
        /// screen and is merely scrolled out of view.
        /// </summary>
        private static List<Cand> Candidates()
        {
            var list = new List<Cand>();
            var es = EventSystem.current;
            var top = TopOverlay();
            bool ftue = FtueShowing;
            int total = 0, active = 0;
            var diag = Config.Debug ? new System.Text.StringBuilder() : null;
            foreach (var b in UnityEngine.Object.FindObjectsOfType<CoolButton>())
            {
                total++;
                if (b == null || !b.isActiveAndEnabled || !b.gameObject.activeInHierarchy) continue;
                active++;
                Vector2 pos = ScreenPos(b.transform);
                bool on = !(pos.x < 0 || pos.y < 0 || pos.x > Screen.width || pos.y > Screen.height);
                bool reach = on && es != null && Reachable(es, b.gameObject, pos);
                bool scrolled = false;
                if (!reach && !ftue)
                {
                    try { scrolled = ScrolledOut(b.transform, pos, top); } catch { scrolled = false; }
                }
                if (diag != null) diag.AppendLine($"  {MenuReader.Path(b.transform)} pos {pos} on {on} reach {reach} scrolled {scrolled} hit {(es != null && on ? FirstHit(es, pos) : "-")}");
                if (reach || scrolled) list.Add(new Cand { Sel = b, Pos = pos, Scrolled = scrolled });
            }
            if (diag != null) Log.Info($"Candidates: {total} buttons, {active} active, {list.Count} usable" + Environment.NewLine + diag);
            return list;
        }

        /// <summary>The button is in a scrolling list of the current screen, outside its viewport, and not faded out.</summary>
        private static bool ScrolledOut(Transform t, Vector2 pos, OverlayUI top)
        {
            var sr = t.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (sr == null || !sr.isActiveAndEnabled) return false;
            if (top != null && !IsUnder(t, top.transform)) return false;
            RectTransform view = sr.viewport;
            if (view == null) view = sr.transform.TryCast<RectTransform>();
            if (view == null) return false;
            Vector2 a = ToScreen(view, view.TransformPoint(new Vector3(view.rect.xMin, view.rect.yMin, 0f)));
            Vector2 b = ToScreen(view, view.TransformPoint(new Vector3(view.rect.xMax, view.rect.yMax, 0f)));
            bool inside = pos.x >= Math.Min(a.x, b.x) && pos.x <= Math.Max(a.x, b.x) && pos.y >= Math.Min(a.y, b.y) && pos.y <= Math.Max(a.y, b.y);
            if (inside) return false;       // in view and still not reachable: something covers it
            // The list itself must be usable: its viewport is on screen and not behind something else.
            Vector2 mid = (a + b) * 0.5f;
            var es = EventSystem.current;
            if (es == null) return false;
            var ped = new PointerEventData(es) { position = mid };
            Hits.Clear();
            es.RaycastAll(ped, Hits);
            bool viewHit = false;
            if (Hits.Count > 0 && Hits[0].gameObject != null) viewHit = IsUnder(Hits[0].gameObject.transform, sr.transform);
            if (!viewHit) return false;
            for (var p = t; p != null; p = p.parent)
            {
                var cg = p.GetComponent<CanvasGroup>();
                if (cg != null && (cg.alpha < 0.05f || !cg.interactable)) return false;
                if (p.Pointer == sr.transform.Pointer) break;
            }
            return true;
        }

        private static string FirstHit(EventSystem es, Vector2 pos)
        {
            var ped = new PointerEventData(es) { position = pos };
            Hits.Clear();
            es.RaycastAll(ped, Hits);
            return Hits.Count == 0 ? "none" : Hits[0].gameObject.name + " (" + Hits.Count + ")";
        }

        /// <summary>A raycast at the button's centre reaches the button (or a child): nothing covers it.</summary>
        private static bool Reachable(EventSystem es, GameObject go, Vector2 pos)
        {
            if (es == null) return false;
            var ped = new PointerEventData(es) { position = pos };
            Hits.Clear();
            es.RaycastAll(ped, Hits);
            if (Hits.Count == 0) return false;
            var hit = Hits[0].gameObject;
            for (var t = hit != null ? hit.transform : null; t != null; t = t.parent)
                if (t.gameObject.Pointer == go.Pointer) return true;
            return false;
        }

        internal static Vector2 ScreenPos(Transform t)
        {
            var rt = t.TryCast<RectTransform>();
            Vector3 world = rt != null ? rt.TransformPoint(rt.rect.center) : t.position;
            return ToScreen(t, world);
        }

        private static Vector2 ToScreen(Transform t, Vector3 world)
        {
            var canvas = t.GetComponentInParent<Canvas>();
            Camera cam = null;
            if (canvas != null)
            {
                var root = canvas.rootCanvas;
                if (root.renderMode != RenderMode.ScreenSpaceOverlay) cam = root.worldCamera;
            }
            return RectTransformUtility.WorldToScreenPoint(cam, world);
        }
    }
}

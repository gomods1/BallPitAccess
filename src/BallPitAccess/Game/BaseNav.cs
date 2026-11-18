using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using BallPitAccess.Input;
using BallPitAccess.UI;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// The base (New Ballbylon) as a keyboard list. A sighted player points the mouse at a building to read
    /// its popup, clicks to open it and right-clicks to dismantle; here:
    /// - Normal mode: Tab / Shift+Tab walk the buildings (the game's own hover follows, so its popup and its
    ///   own keys apply to that building), Enter clicks it, Delete right-clicks it (dismantle).
    /// - Rearrange mode: Tab chooses a building, Enter picks it up (then the placement keys move it).
    /// - Expansion mode: Tab chooses a plot for sale, Enter buys it.
    /// Also announces the base's mode changes and answers the base HUD keys (resources, harvest).
    /// Everything acts through the game's own click handlers.
    /// </summary>
    internal static class BaseNav
    {
        private static bool _owns;
        private static int _index = -1;
        private static IntPtr _curBuilding = IntPtr.Zero;
        private static int _plot = -1;
        private static bool _onElevator;
        private static BaseState _lastState = BaseState.kNum;
        private static int _errors;

        /// <summary>The arrows took the UI focus back: Enter belongs to the focused button again.</summary>
        internal static void Release() => _owns = false;

        internal static bool InBase
        {
            get { try { return BaseMgr.I != null && BaseGridMgr.I != null; } catch { return false; } }
        }

        internal static void Update()
        {
            try
            {
                var bm = BaseMgr.I;
                if (bm == null || BaseGridMgr.I == null) { _lastState = BaseState.kNum; _owns = false; return; }
                var st = bm.CurState;
                if (st != _lastState) { OnState(_lastState, st); _lastState = st; }
                AnnounceMain(st);
                if (ModKeys.ListOpen || Keys.Consumed || !Keys.GameHasFocus() || Keys.Ctrl || Keys.Alt || Keys.ScreenReaderModifier) return;
                if (Navigator.HasOverlay || Navigator.FtueShowing) { _owns = false; return; }
                if (Explore.Active) return;
                if (_onElevator && !_owns) LeaveElevator();
                switch (st)
                {
                    case BaseState.kNormal:
                        if (Keys.Pressed(Keys.VK_TAB)) { Keys.Consume(); Cycle(Keys.Shift ? -1 : 1); }
                        else if (_owns && _onElevator && Keys.Pressed(Keys.VK_RETURN)) { Keys.Consume(); OpenElevator(); }
                        else if (_owns && Keys.Pressed(Keys.VK_RETURN))
                        {
                            // Enter does what the building is offering: the harvest when it is ready, else its screen.
                            Keys.Consume();
                            if (Keys.Shift || !StartHarvest()) Click(false);
                        }
                        else if (_owns && !_onElevator && Keys.Pressed(Keys.VK_DELETE)) { Keys.Consume(); Click(true); }
                        break;
                    case BaseState.kRearrangeBuildings:
                        if (BaseGridMgr.I.GetPendingRearrangeBuilding() != null) break;     // Placement moves it
                        if (Keys.Pressed(Keys.VK_TAB)) { Keys.Consume(); Cycle(Keys.Shift ? -1 : 1); }
                        else if (_owns && Keys.Pressed(Keys.VK_RETURN)) { Keys.Consume(); PickUp(); }
                        break;
                    case BaseState.kSelectingExpansion:
                        if (Keys.Pressed(Keys.VK_TAB)) { Keys.Consume(); CyclePlot(Keys.Shift ? -1 : 1); }
                        else if (_owns && Keys.Pressed(Keys.VK_RETURN)) { Keys.Consume(); BuyPlot(); }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("BaseNav: " + ex);
            }
        }

        private static void OnState(BaseState from, BaseState to)
        {
            Log.Info("Base state: " + from + " -> " + to);
            _owns = false;
            _plot = -1;
            LeaveElevator();
            // The main base screen: its name and keys on arriving (loading a save, after an expedition or a
            // cutscene), its name only when a menu or mode closes. Said once no screen covers it.
            _mainPending = 0;
            if (to == BaseState.kNormal)
            {
                bool arrival = from == BaseState.kNum || from == BaseState.kCutscene || from == BaseState.kReturningFromLvl;
                _mainPending = arrival ? 2 : 1;
                _mainSince = Time.unscaledTime;
                return;
            }
            if (from == BaseState.kNum) return;
            // The harvest aim passes through a cutscene state for a few frames on its way in: not news.
            if (to == BaseState.kCutscene && from == BaseState.kAimWorkers) return;
            if (to == BaseState.kCutscene) return;      // a camera move: nothing to do or know
            // Expansion mode moves the camera on entry and after a purchase: back from that move it is the same
            // mode, not news (the keys were said twice). After a purchase say what happened instead.
            if (to == BaseState.kSelectingExpansion && from == BaseState.kCutscene) return;
            if (to == BaseState.kSelectingExpansion && from == BaseState.kExpanding) { Speech.Event(Loc.Get("plot_bought")); return; }
            string key = "base_" + to;
            if (Loc.Has(key)) Speech.Event(Loc.Get(key));
        }

        private static int _mainPending;      // 0 nothing, 1 the name, 2 the name and keys
        private static float _mainSince;

        private static void AnnounceMain(BaseState st)
        {
            if (_mainPending == 0) return;
            if (st != BaseState.kNormal) { _mainPending = 0; return; }
            // A screen on top (the title menu while the save loads, a popup, the guided tutorial) speaks first.
            if (Navigator.HasOverlay || Navigator.FtueShowing) { _mainSince = Time.unscaledTime; return; }
            if (Time.unscaledTime - _mainSince < 0.4f) return;
            bool full = _mainPending == 2;
            _mainPending = 0;
            Speech.Event(Loc.Get(!full ? "base_main" : HarvestOpen() ? "base_back_harvest" : "base_back"));
        }

        // ---------------- Buildings ----------------

        private static List<BuildingInst> Buildings()
        {
            var list = new List<BuildingInst>();
            var bm = BuildingMgr.I;
            if (bm == null || bm.BuildingsById == null) return list;
            foreach (var kv in bm.BuildingsById)
            {
                var b = kv.Value;
                if (b == null || b.Obj == null || !b.Obj.gameObject.activeInHierarchy) continue;
                list.Add(b);
            }
            // By kind, then left to right, then top to bottom: the same kinds sit together.
            list.Sort((a, c) =>
            {
                int t = ((int)a.Type).CompareTo((int)c.Type);
                if (t != 0) return t;
                Vector3 pa = a.Obj.transform.position, pc = c.Obj.transform.position;
                int x = pa.x.CompareTo(pc.x);
                return x != 0 ? x : pc.y.CompareTo(pa.y);
            });
            return list;
        }

        private static Vector3 Center(BuildingObj o)
        {
            try
            {
                if (o.PolyCol != null && o.PolyCol.enabled) return o.PolyCol.bounds.center;
                if (o.CircleCol != null && o.CircleCol.enabled) return o.CircleCol.bounds.center;
            }
            catch { }
            return o.transform.position;
        }

        private static BuildingInst Current(List<BuildingInst> list)
        {
            foreach (var b in list) if (b.Pointer == _curBuilding) return b;
            return null;
        }

        private static void Cycle(int dir)
        {
            var list = Buildings();
            // The lift is not a building of the grid, but it is clicked like one: it comes first.
            int lift = HasElevator ? 1 : 0;
            int n = list.Count + lift;
            if (n == 0) { Speech.SayMenu(Loc.Get("base_no_buildings")); return; }
            int i = -1;
            if (_onElevator && lift == 1) i = 0;
            else for (int k = 0; k < list.Count; k++) if (list[k].Pointer == _curBuilding) { i = k + lift; break; }
            if (i < 0) i = dir > 0 ? 0 : n - 1;
            else i = (i + dir + n) % n;
            _index = i;
            if (lift == 1 && i == 0) { FocusElevator(n); return; }
            LeaveElevator();
            i -= lift;
            var b = list[i];
            _curBuilding = b.Pointer;
            _owns = true;
            HoverOn(b);
            var lines = new List<string>();
            string text = Describe(b, lines) + ", " + Loc.F("n_of_m", i + 1 + lift, n);
            Speech.SayMenu(text);
            Buffers.SetDetails(lines, MenuReader.HoverLines);
        }

        // ---------------- The lift (elevator) ----------------

        /// <summary>In the normal base view the lift stands in the world and is hovered and clicked with the mouse.</summary>
        private static bool HasElevator
        {
            get
            {
                try
                {
                    var bm = BaseMgr.I; var e = BaseElevatorObj.I;
                    return bm != null && bm.CurState == BaseState.kNormal && e != null && e.gameObject.activeInHierarchy;
                }
                catch { return false; }
            }
        }

        private static void FocusElevator(int total)
        {
            _onElevator = true;
            _curBuilding = IntPtr.Zero;
            _owns = true;
            Placement.Claim();
            var g = BaseGridMgr.I;
            try
            {
                var hov = g._hoverBuilding;
                if (hov != null) { hov.SetHover(false); g._hoverBuilding = null; }
            }
            catch { }
            // The game's own hover: outline on the lift and its popup (description, gears collected).
            try { BaseElevatorObj.I.SetHover(true); } catch (Exception ex) { if (Config.Debug) Log.Warn("Elevator hover: " + ex.Message); }
            try { var pop = BaseHoverPopup.I; if (pop != null) pop.HoverElevator(true); } catch (Exception ex) { Log.Warn("Elevator popup: " + ex.Message); }
            var parts = new List<string> { Loc.Get("elevator") };
            var details = new List<string>();
            try
            {
                var bm = BaseMgr.I;
                bool down = bm.IsBaseGameElevatorUpgradeVisible(), up = bm.IsDlcElevatorUpgradeVisible();
                if (down && up) parts.Add(Loc.Get("elev_both"));
                else if (up) parts.Add(Loc.Get("elev_up_avail"));
                else if (down) parts.Add(Loc.Get("elev_down_avail"));
                var ms = MetaSaveData.I;
                if (ms != null)
                {
                    details.Add(Loc.F("elev_pit_lvl", ms.ElevatorLvl));
                    if (Names.HasDlc) details.Add(Loc.F("elev_rise_lvl", ms.ElevatorDLCLvl));
                }
                string cost = Names.Cost(bm.GetElevatorUpgradeCost());
                if (cost.Length > 0 && (down || up)) details.Add(Loc.F("bld_can_upgrade", cost));
            }
            catch (Exception ex) { if (Config.Debug) Log.Warn("Describe elevator: " + ex.Message); }
            Speech.SayMenu(string.Join(", ", parts) + ", " + Loc.F("n_of_m", 1, total));
            Buffers.SetDetails(details, MenuReader.HoverLines);
        }

        private static void LeaveElevator()
        {
            if (!_onElevator) return;
            _onElevator = false;
            try { var e = BaseElevatorObj.I; if (e != null) e.SetHover(false); } catch { }
            try { var pop = BaseHoverPopup.I; if (pop != null && pop._isHoveringElevator) pop.HoverElevator(false); } catch { }
        }

        /// <summary>
        /// Enter on the lift: the game's lift upgrade screen, when an upgrade is on offer. That screen asks
        /// which way (down the pit, up to Risen Ballbylon) when both are open, then shows the gears.
        /// </summary>
        private static void OpenElevator()
        {
            try
            {
                var bm = BaseMgr.I;
                var ui = UpgradeBuildingUI.I;
                bool offered = bm.IsElevatorUpgradeVisible();
                Log.Info("Elevator: Enter, upgrade offered " + offered + " (pit " + bm.IsBaseGameElevatorUpgradeVisible() + ", risen " + bm.IsDlcElevatorUpgradeVisible() + ")");
                if (offered && ui != null) { ui.ActivateElevator(); return; }
                // Nothing to upgrade now: the popup says why (gears collected so far).
                var why = MenuReader.HoverLines();
                Speech.SayMenu(Loc.Get("elev_nothing") + (why.Count > 0 ? ". " + string.Join(". ", why) : ""));
            }
            catch (Exception ex) { Log.Warn("Elevator: " + ex.Message); }
        }

        /// <summary>Put the game's own hover on the building (as the mouse would), keeping the real mouse out of it.</summary>
        private static void HoverOn(BuildingInst b)
        {
            var g = BaseGridMgr.I;
            Placement.Claim();
            Vector3 c = Center(b.Obj);
            try { g.SetHover(c.x, c.y, true); } catch (Exception ex) { Log.Warn("SetHover: " + ex.Message); }
            try
            {
                var hov = g._hoverBuilding;
                if (hov == null || hov.Pointer != b.Obj.Pointer)
                {
                    if (hov != null) hov.SetHover(false);
                    g._hoverBuilding = b.Obj;
                    b.Obj.SetHover(true);
                    var pop = BaseHoverPopup.I;
                    if (pop != null) pop.HoverBuilding(b.Obj);
                }
            }
            catch (Exception ex) { if (Config.Debug) Log.Warn("Hover building: " + ex.Message); }
        }

        private static string Describe(BuildingInst b, List<string> details)
        {
            var inf = b.GetInfo();
            var parts = new List<string> { Names.Building(inf) };
            try
            {
                if (b.UpgradeLvl > 0) parts.Add(Loc.F("bld_level", b.UpgradeLvl + 1));
                if (b.CurState == BuildingState.kScaffold) parts.Add(Loc.F("bld_scaffold", Mathf.RoundToInt(b.GetUpgradePct() * 100f)));
                else if (b.CurState == BuildingState.kUpgrading) parts.Add(Loc.F("bld_upgrading", Mathf.RoundToInt(b.GetUpgradePct() * 100f)));
                // Rearrange mode: Enter picks the building up, so the harvest hint ("Enter harvests") was wrong there.
                bool rearranging = false;
                try { rearranging = BaseMgr.I != null && BaseMgr.I.CurState == BaseState.kRearrangeBuildings; } catch { }
                if (!rearranging && b.CanHarvest())
                {
                    bool now = HarvestOpen();
                    parts.Add(Loc.Get(now ? "bld_harvest_enter" : "bld_harvest"));
                    if (now) details.Add(Loc.Get("bld_open_hint"));
                }
                else if (!rearranging && Harvest.UnderConstruction(b))
                    parts.Add(Loc.Get(HarvestOpen() ? "bld_build_enter" : "bld_build_harvest"));
                string held = Names.Cost(b.HeldResources);
                if (held.Length > 0) parts.Add(Loc.F("bld_holds", held));
                if (b.HasWorker()) parts.Add(Loc.F("bld_worker", Names.Char(b.GetWorker())));
                if (b.HasActiveTask()) parts.Add(Loc.F("bld_task", Mathf.RoundToInt(b.GetTaskProgress() * 100f)));
                if (b.CurState == BuildingState.kNormal && b.CanBeUpgraded())
                {
                    string cost = Names.Cost(b.GetUpgradeCost());
                    details.Add(Loc.F("bld_can_upgrade", cost));
                }
                BaseGrid.Refresh();
                var tl = BaseGrid.TopLeftTiles(b) ?? BaseGrid.TopLeft(b);
                details.Add(Loc.F("bld_at", BaseGrid.Col(tl.x), BaseGrid.Row(tl.y)));
                string size = BaseGrid.SizeTiles(b);
                if (size.Length > 0) details.Add(size);
                string desc = Names.Tr(inf.GetDescSlug());
                if (desc.Length == 0) desc = Names.Tr(inf.GetBuildDescSlug());
                if (desc.Length > 0) details.Add(desc);
            }
            catch (Exception ex) { if (Config.Debug) Log.Warn("Describe building: " + ex.Message); }
            return string.Join(", ", parts);
        }

        /// <summary>The side bar's harvest button can be pressed now.</summary>
        private static bool HarvestOpen()
        {
            try
            {
                var ui = BaseUI.I;
                var w = ui != null ? ui.BtnWorkers : null;
                return w != null && w.IsEnabled && w.Btn != null && w.Btn.gameObject.activeInHierarchy && w.Btn.IsInteractable();
            }
            catch { return false; }
        }

        /// <summary>
        /// Enter on a building that is ready to harvest or under construction: press the side bar's harvest
        /// button (the game's own way in) and tell the harvest aim to start on this building.
        /// </summary>
        private static bool StartHarvest()
        {
            var b = Current(Buildings());
            if (b == null) return false;
            try
            {
                if ((!b.CanHarvest() && !Harvest.UnderConstruction(b)) || !HarvestOpen()) return false;
                Harvest.Prefer(b.Pointer);
                var es = UnityEngine.EventSystems.EventSystem.current;
                Navigator.Click(BaseUI.I.BtnWorkers.Btn);
                if (BaseMgr.I.CurState == BaseState.kNormal && !Navigator.HasOverlay) BaseUI.I.OnWorkersClicked();
                bool started = BaseMgr.I.CurState != BaseState.kNormal || Navigator.HasOverlay;
                Log.Info("Harvest from " + b.Type + ": started " + started + ", state " + BaseMgr.I.CurState);
                return started;
            }
            catch (Exception ex) { Log.Warn("Start harvest: " + ex.Message); return false; }
        }

        private static void Click(bool right)
        {
            var b = Current(Buildings());
            if (b == null) { _owns = false; return; }
            HoverOn(b);
            var g = BaseGridMgr.I;
            try
            {
                if (right)
                {
                    g.NormalRightClickOnBuilding();
                    Log.Info("Right click on " + b.Type);
                    // Whatever the game answers (a confirmation, a refund preview) is on its popup or a dialog.
                    Buffers.SetDetails(null, MenuReader.HoverLines);
                    Speech.SayMenu(Loc.F("bld_dismantle", Names.Building(b.GetInfo())));
                }
                else
                {
                    g.NormalClickOnBuilding();
                    Log.Info("Click on " + b.Type);
                }
            }
            catch (Exception ex) { Log.Warn("Building click: " + ex.Message); }
        }

        private static void PickUp()
        {
            var b = Current(Buildings());
            if (b == null) { _owns = false; return; }
            HoverOn(b);
            try { BaseGridMgr.I.ClickRearrange(b.Obj); Log.Info("Rearrange pick up " + b.Type); }
            catch (Exception ex) { Log.Warn("Rearrange: " + ex.Message); }
        }

        // ---------------- Expansion ----------------

        private static List<Vector2Int> Plots()
        {
            var list = new List<Vector2Int>();
            var g = BaseGridMgr.I;
            for (int y = BaseGridMgr.kChunkRows - 1; y >= 0; y--)
                for (int x = 0; x < BaseGridMgr.kChunkCols; x++)
                {
                    bool ok;
                    try { ok = g.IsValidExpansionTgt(x, y); } catch { ok = false; }
                    if (ok) list.Add(new Vector2Int(x, y));
                }
            return list;
        }

        private static void CyclePlot(int dir)
        {
            var list = Plots();
            if (list.Count == 0) { Speech.SayMenu(Loc.Get("plot_none")); return; }
            _plot = _plot < 0 ? (dir > 0 ? 0 : list.Count - 1) : (_plot + dir + list.Count) % list.Count;
            var p = list[_plot];
            _owns = true;
            Placement.Claim();
            var g = BaseGridMgr.I;
            try { g.SetExpansionHover(p.x, p.y); } catch (Exception ex) { Log.Warn("Expansion hover: " + ex.Message); }
            int cost = 0;
            try { cost = g.GetExpansionCost(); } catch { }
            Speech.SayMenu(Loc.F("plot", p.x + 1, p.y + 1, cost, _plot + 1, list.Count));
            Buffers.SetDetails(null, MenuReader.HoverLines);
        }

        private static void BuyPlot()
        {
            var list = Plots();
            if (_plot < 0 || _plot >= list.Count) return;
            var p = list[_plot];
            var g = BaseGridMgr.I;
            try
            {
                g.SetExpansionHover(p.x, p.y);
                g.ClickOnExpansion();
                Log.Info("Expansion click " + p);
            }
            catch (Exception ex) { Log.Warn("Expansion: " + ex.Message); }
        }

        // ---------------- HUD keys ----------------

        internal static void SayResources()
        {
            var lines = new List<string>();
            try
            {
                var ms = MetaSaveData.I;
                if (ms != null && ms.NumResources != null)
                {
                    var parts = new List<string>();
                    for (int i = 0; i < ms.NumResources.Length && i < (int)ResourceType.kNum; i++)
                        if (ms.NumResources[i] > 0 || i == 0) parts.Add(ms.NumResources[i] + " " + Names.Resource((ResourceType)i));
                    lines.Add(string.Join(", ", parts));
                    lines.Add(Loc.F("base_day", ms.CurDay));
                }
            }
            catch (Exception ex) { Log.Warn("Resources: " + ex.Message); }
            if (lines.Count == 0) return;
            Speech.SayMenu(string.Join(". ", lines));
            // In a menu the Details buffer stays the focused item's.
            if (!Navigator.HasOverlay) Buffers.SetDetails(lines);
        }

        internal static void SayHarvest()
        {
            var parts = new List<string>();
            try
            {
                var bm = BuildingMgr.I;
                var ms = MetaSaveData.I;
                if (bm != null && ms != null)
                {
                    if (!ms.HasAnythingToHarvest()) parts.Add(Loc.Get("harvest_none"));
                    else if (bm.IsBuildingAvailableToHarvest() && !ms.DidHarvestToday) parts.Add(Loc.Get("harvest_ready"));
                    else parts.Add(Loc.F("harvest_wait", Mathf.RoundToInt(bm.GetHarvestRefreshProgress() * 100f)));
                    int building = 0;
                    if (bm.BuildingsById != null)
                        foreach (var kv in bm.BuildingsById) if (Harvest.UnderConstruction(kv.Value)) building++;
                    if (building > 0) parts.Add(Loc.F("construct_n", building));
                }
                var ui = BaseUI.I;
                if (ui != null) { string clock = Names.Shown(ui.TxtHarvestClock); if (clock.Length > 0) parts.Add(clock); }
                var wm = WorkerMgr.I;
                if (wm != null) parts.Add(Loc.F("workers_n", wm.GetNumWorkers()));
            }
            catch (Exception ex) { Log.Warn("Harvest status: " + ex.Message); }
            if (parts.Count > 0) Speech.SayMenu(string.Join(". ", parts));
        }
    }
}

using System;
using BallPitAccess.Core;
using BallPitAccess.Input;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Keyboard placement of a building on the base grid (template 13.3, build mode). While the game shows a
    /// placement preview, arrows move the game's own hover one tile, the mod says column, row and whether the
    /// building fits there (the game's own CanBuildPlacePreview), Enter places through the game's Place
    /// button handler, Home jumps to the first empty spot the game reports. R (the game's rotate) still works.
    /// The same keys move a building picked up in rearrange mode; Enter drops it.
    /// </summary>
    internal static class Placement
    {
        private static bool _wasActive;
        private static float _x, _y;
        private static float _announceAt = -1f;
        private static float _checkDropAt = -1f;
        private static int _errors;

        /// <summary>Set by a mod key; while set, the game's mouse handling for the base grid is skipped
        /// (it re-hovers to the mouse position every frame). Cleared when the real mouse moves.</summary>
        private static bool _kbOwned;
        private static Vector3 _mouseAtClaim;

        internal static void TryPatch(HarmonyLib.Harmony h)
        {
            try
            {
                var m = HarmonyLib.AccessTools.Method(typeof(BaseGridMgr), nameof(BaseGridMgr.UpdateMouseControls));
                if (m == null) { Log.Warn("BaseGridMgr.UpdateMouseControls not found: placement cursor fights the mouse."); return; }
                h.Patch(m, prefix: new HarmonyLib.HarmonyMethod(typeof(Placement), nameof(MousePrefix)));
                Log.Info("Patched BaseGridMgr.UpdateMouseControls");
            }
            catch (Exception ex) { Log.Error("Placement patch failed: " + ex); }
        }

        private static bool MousePrefix()
        {
            if (!_kbOwned) return true;
            try
            {
                Vector2 m = GameInput.GetMouseOrTouchPosition();
                if ((new Vector3(m.x, m.y, 0) - _mouseAtClaim).sqrMagnitude > 4f) { _kbOwned = false; return true; }
                // A real click always goes to the game.
                if (GameInput.GetMouseOrTouchDown() || GameInput.GetRightMouseOrTouchDown()) { _kbOwned = false; return true; }
            }
            catch { }
            return false;
        }

        private static bool Rearranging
        {
            get
            {
                try
                {
                    var bm = BaseMgr.I; var g = BaseGridMgr.I;
                    return bm != null && g != null && bm.CurState == BaseState.kRearrangeBuildings && g.GetPendingRearrangeBuilding() != null;
                }
                catch { return false; }
            }
        }

        internal static bool Active
        {
            get
            {
                try { var g = BaseGridMgr.I; return g != null && (g.GetPlacePreview() != null || Rearranging); }
                catch { return false; }
            }
        }

        internal static void Update()
        {
            try
            {
                EnsurePreview();
                bool active = Active;
                if (active && !_wasActive)
                {
                    var g0 = BaseGridMgr.I;
                    var hp = g0.GetHoverPos();
                    BaseGrid.Refresh();
                    // The game makes the preview where the mouse is, which for a player who never touches the mouse
                    // is anywhere, often off the land ("column 26, row 15, blocked"): start in the middle of the land.
                    if (!Rearranging && !BaseGrid.Inside(BaseGrid.HoverCell(hp).x, BaseGrid.HoverCell(hp).y))
                    {
                        var mid = BaseGrid.World((BaseGrid.MinX + BaseGrid.MaxX) / 2, (BaseGrid.MinY + BaseGrid.MaxY) / 2);
                        Claim();
                        g0.SetHover(mid.x, mid.y, true);
                        Log.Info($"Placement: the preview started off the land at {hp}, moved to the middle {mid}");
                        hp = g0.GetHoverPos();
                    }
                    _x = hp.x; _y = hp.y;
                    Log.Info($"Placement start at {hp}; borders L {BaseGridMgr.I.LeftBorderX} R {BaseGridMgr.I.RightBorderX} T {BaseGridMgr.I.TopBorderY} B {BaseGridMgr.I.BottomBorderY} tile {BaseGridMgr.kSpaceWidth} rearrange {Rearranging}; grid x {BaseGrid.MinX}..{BaseGrid.MaxX} y {BaseGrid.MinY}..{BaseGrid.MaxY}");
                    Speech.Event(What() + Loc.Get(Rearranging ? "rearrange_moving" : "place_start"));
                    // The preview animates in: its first legality reading is wrong, wait for it.
                    _announceAt = Time.unscaledTime + 0.5f;
                }
                if (!active && _wasActive) { _kbOwned = false; _search = null; }
                _wasActive = active;
                if (_cancelPending) PendingCancel(active);
                if (_checkDropAt > 0 && Time.unscaledTime >= _checkDropAt)
                {
                    _checkDropAt = -1f;
                    Speech.Event(Loc.Get(Rearranging ? "place_refused" : "rearrange_dropped"));
                }
                if (!active) return;

                if (_search != null) StepSearch();
                WatchRotation();
                if (_announceAt > 0 && Time.unscaledTime >= _announceAt) { _announceAt = -1f; Announce(); }
                if (Keys.Consumed || !Keys.GameHasFocus() || Keys.Ctrl || Keys.Alt || ModKeys.ListOpen || UI.Navigator.Blocking) return;

                float step = BaseGridMgr.kSpaceWidth > 0.01f ? BaseGridMgr.kSpaceWidth : 1f;
                if (Keys.Pressed(Keys.VK_LEFT)) MoveBy(-step, 0);
                else if (Keys.Pressed(Keys.VK_RIGHT)) MoveBy(step, 0);
                else if (Keys.Pressed(Keys.VK_UP)) MoveBy(0, step);
                else if (Keys.Pressed(Keys.VK_DOWN)) MoveBy(0, -step);
                else if (Keys.Pressed(Keys.VK_HOME)) JumpToEmpty();
                else if (Keys.Pressed(Keys.VK_RETURN)) { Keys.Consume(); Place(); }
                else if (Keys.Pressed(Keys.VK_BACK)) { Keys.Consume(); _cancelPending = true; _releasedFrame = -1; }
                else if (Input.Pad.RotatePressed) Rotate();
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Placement: " + ex);
            }
        }

        private static int _noPreviewFrames;

        /// <summary>
        /// The game makes the placement preview in BaseGridMgr.SetHover, which its mouse handler calls every frame.
        /// When the keyboard owned the hover from before (Tab through the buildings, a harvest), that handler is
        /// skipped, no preview was ever made, the base sat in kPlaceBuilding with nothing to place and the side
        /// bar took the keys (a player: "after harvesting it doesn't let me build anything"). Make it here.
        /// </summary>
        private static void EnsurePreview()
        {
            var bm = BaseMgr.I; var g = BaseGridMgr.I;
            if (bm == null || g == null || bm.CurState != BaseState.kPlaceBuilding || g.GetPlacePreview() != null) { _noPreviewFrames = 0; return; }
            if (++_noPreviewFrames < 2) return;     // one frame for the game's own mouse handler
            _noPreviewFrames = 0;
            Vector3 at = g.GetHoverPos();
            if (at.x < g.LeftBorderX || at.x > g.RightBorderX || at.y < g.BottomBorderY || at.y > g.TopBorderY)
                at = new Vector3((g.LeftBorderX + g.RightBorderX) / 2f, (g.BottomBorderY + g.TopBorderY) / 2f, 0f);
            if (BaseGrid.Refresh()) { var c = BaseGrid.CellOf(at); at = BaseGrid.World(c.x, c.y); }
            Claim();
            g.SetHover(at.x, at.y, true);
            Log.Info($"Placement: no preview in kPlaceBuilding (keyboard owned {_kbOwned}), made it at {at}: {(g.GetPlacePreview() != null)}");
        }

        /// <summary>"Wheat field, 2 wide, 2 tall. ": the building being placed, as it is turned now.</summary>
        private static string What()
        {
            try
            {
                var p = BaseGridMgr.I.GetPlacePreview() ?? BaseGridMgr.I.GetPendingRearrangeBuilding();
                var inst = p != null ? p.Inst : null;
                if (inst == null) return "";
                string name = Names.Building(inst.GetInfo());
                string size = BaseGrid.Size(inst.GetInfo());
                return (size.Length > 0 ? name + ", " + size : name) + ". ";
            }
            catch { return ""; }
        }

        /// <summary>The keyboard now owns the base grid's hover until the real mouse moves or clicks.</summary>
        internal static void Claim()
        {
            _kbOwned = true;
            try { Vector2 m = GameInput.GetMouseOrTouchPosition(); _mouseAtClaim = new Vector3(m.x, m.y, 0); } catch { }
        }

        private static void MoveBy(float dx, float dy)
        {
            Claim();
            _search = null;
            var g = BaseGridMgr.I;
            _x = Mathf.Clamp(_x + dx, g.LeftBorderX, g.RightBorderX);
            _y = Mathf.Clamp(_y + dy, g.BottomBorderY, g.TopBorderY);
            g.SetHover(_x, _y, true);
            if (Config.Debug) Log.Info($"Placement move to ({_x},{_y}) -> hover {g.GetHoverPos()}");
            // The preview's colour (legality) refreshes on the next frames.
            _announceAt = Time.unscaledTime + 0.08f;
        }

        /// <summary>
        /// Home: the game's empty spots are single free tiles, and a bigger building (the houses) does not fit
        /// on all of them. Try them in turn (the fit check reads the preview where it stands, so each try waits
        /// a few frames) and stop on the first one the building fits; back to the start if none does.
        /// </summary>
        private static System.Collections.Generic.List<Vector3> _search;
        private static int _searchIdx, _searchAt;
        private static Vector3 _searchFrom;

        private static void JumpToEmpty()
        {
            Claim();
            var g = BaseGridMgr.I;
            var spots = g.FindEmptySpots();
            if (spots == null || spots.Count == 0) { Speech.SayMenu(Loc.Get("place_no_empty")); return; }
            _search = new System.Collections.Generic.List<Vector3>();
            foreach (var s in spots) _search.Add(g.GetWorldPos(s.x, s.y));
            _searchFrom = new Vector3(_x, _y, 0);
            _searchIdx = 0;
            _announceAt = -1f;
            TrySpot(g);
        }

        private static void TrySpot(BaseGridMgr g)
        {
            var wp = _search[_searchIdx];
            _x = wp.x; _y = wp.y;
            g.SetHover(_x, _y, true);
            _searchAt = Time.frameCount + 4;
        }

        private static void StepSearch()
        {
            if (_search == null || Time.frameCount < _searchAt) return;
            var g = BaseGridMgr.I;
            bool fits = g.GetPlacePreview() == null || g.CanBuildPlacePreview();
            if (fits)
            {
                if (Config.Debug) Log.Info($"Placement: spot {_searchIdx + 1} of {_search.Count} fits");
                _search = null;
                Announce();
                return;
            }
            if (++_searchIdx < _search.Count) { TrySpot(g); return; }
            Log.Info($"Placement: none of {_search.Count} empty spots fits");
            _search = null;
            _x = _searchFrom.x; _y = _searchFrom.y;
            g.SetHover(_x, _y, true);
            Speech.SayMenu(Loc.Get("place_no_fit"));
        }

        private static void Announce()
        {
            var g = BaseGridMgr.I;
            var hp = g.GetHoverPos();
            if (Config.Debug) Log.Info($"Placement announce hover {hp} wanted ({_x},{_y})");
            // The game snaps the preview to its grid: step on from where it actually is.
            _x = hp.x; _y = hp.y;
            // The same numbers as explore mode and the building list: the preview's top-left tile.
            int col, row;
            var p0 = g.GetPlacePreview() ?? g.GetPendingRearrangeBuilding();
            Vector2Int cell;
            // The preview's Inst keeps the spot it was made at; the hover point is its bottom-left tile.
            cell = BaseGrid.HoverCell(hp);
            try { if (p0 != null && p0.Inst != null) cell = BaseGrid.PreviewTopLeft(cell, p0.Inst.GetInfo().TileSize); } catch { }
            col = BaseGrid.Col(cell.x); row = BaseGrid.Row(cell.y);
            if (Config.Debug)
            {
                string extra = "";
                try
                {
                    var pv = g.GetPlacePreview(); var rb = g.GetPendingRearrangeBuilding();
                    if (pv != null) extra += $" preview at {pv.transform.position} inst ({pv.Inst?.X},{pv.Inst?.Y})";
                    if (rb != null) extra += $" pending at {rb.transform.position} inst ({rb.Inst?.X},{rb.Inst?.Y})";
                    extra += $" state {BaseMgr.I?.CurState}";
                }
                catch (Exception ex) { extra = " (" + ex.Message + ")"; }
                Log.Info($"Placement cell {cell} (hover cell {BaseGrid.HoverCell(hp)}, rounded {BaseGrid.CellOf(hp)}) -> col {col} row {row}{extra}");
            }
            if (g.GetPlacePreview() == null)
            {
                // Rearranging: the game only tells whether it fits when the building is dropped.
                Speech.SayMenu(Loc.F("place_pos_only", col, row));
                return;
            }
            bool ok = g.CanBuildPlacePreview();
            Speech.SayMenu(Loc.F("place_pos", col, row, Loc.Get(ok ? "place_ok" : "place_blocked")));
            Audio.Cues.Play(ok ? "tick" : "wall", 0f, 0.6f);
        }

        private static void Place()
        {
            var g = BaseGridMgr.I;
            if (g.GetPlacePreview() == null)
            {
                if (!Rearranging) return;
                g.ClickRearrange(null);
                Log.Info("Rearrange drop at " + g.GetHoverPos());
                _checkDropAt = Time.unscaledTime + 0.4f;
                return;
            }
            if (!g.CanBuildPlacePreview()) { Speech.SayMenu(Loc.Get("place_refused")); return; }
            var ui = BaseUI.I;
            if (ui == null) return;
            bool moving = Rearranging;
            ui.OnPlaceBuildingClicked();
            Log.Info((moving ? "Rearrange drop at " : "Placed at ") + g.GetHoverPos());
            // A building picked up in rearrange mode is moved, not built: say so once the game took it.
            if (moving) _checkDropAt = Time.unscaledTime + 0.4f;
            else Speech.Event(Loc.Get("placed"));
        }

        /// <summary>Y on a controller: the game's rotate (R on the keyboard), whose controller mapping the mod has off.</summary>
        private static void Rotate()
        {
            var g = BaseGridMgr.I;
            try
            {
                bool can = g.GetPlacePreview() != null && g.CanRotatePlacePreview();
                Log.Info("Placement: controller rotate, can rotate " + can);
                if (can) g.RotatePlacePreview(1);
                else Audio.Cues.Play("wall", 0f, 0.5f);
            }
            catch (Exception ex) { Log.Warn("Rotate: " + ex.Message); }
        }

        private static int _lastRot = int.MinValue;
        private static bool _rotPending;

        /// <summary>
        /// The game's rotate key (R by default) turns the preview, and whether it fits can change with it: say
        /// the spot again once the turn animation ends. Read from the preview, so any rotate binding works.
        /// </summary>
        private static void WatchRotation()
        {
            var p = BaseGridMgr.I.GetPlacePreview();
            if (p == null || p.Inst == null) { _lastRot = int.MinValue; _rotPending = false; return; }
            int rot = p.Inst.Rotation;
            if (_lastRot != int.MinValue && rot != _lastRot) _rotPending = true;
            _lastRot = rot;
            if (_rotPending && !p.IsRotating())
            {
                _rotPending = false;
                _search = null;
                _announceAt = Time.unscaledTime + 0.08f;
                // Inst.GetTileSize() swaps width and height here, but the tiles taken once built did not (a turned
                // 3 by 2 villa still covered 3 by 2): say the spot again, not a new size.
                Log.Info($"Placement rotated to {rot}, size {p.Inst.GetTileSize()}");
            }
        }

        private static bool _cancelPending;
        private static int _releasedFrame = -1;

        /// <summary>
        /// The game reads Backspace as its own "back" when the key is released: cancelling on the press left the
        /// base in its normal state at the release, and the game opened the pause menu (one press did two
        /// things). Wait for the release and two more frames, then cancel only if the building is still there.
        /// </summary>
        private static void PendingCancel(bool active)
        {
            if (Keys.Down(Keys.VK_BACK)) return;
            if (_releasedFrame < 0) { _releasedFrame = Time.frameCount; return; }
            if (Time.frameCount < _releasedFrame + 2) return;
            _cancelPending = false;
            if (active) Cancel();
            else { Log.Info("Placement: the game cancelled it itself"); Speech.Event(Loc.Get("place_cancelled")); }
        }

        private static void Cancel()
        {
            var g = BaseGridMgr.I;
            try
            {
                if (g.GetPlacePreview() == null) g.CancelRearrange();
                else BaseUI.I?.OnCancelBuildingClicked();
                Speech.Event(Loc.Get("place_cancelled"));
            }
            catch (Exception ex) { Log.Warn("Cancel placement: " + ex.Message); }
        }
    }
}

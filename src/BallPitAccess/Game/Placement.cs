using System;
using System.Collections.Generic;
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
                        SetHoverSnapped(mid.x, mid.y);
                        Log.Info($"Placement: the preview started off the land at {hp}, moved to the middle {mid}");
                        hp = g0.GetHoverPos();
                    }
                    _x = hp.x; _y = hp.y;
                    // A building picked up in rearrange mode starts parked away from the cursor until the hover is set.
                    if (Rearranging) { Claim(); SetHoverSnapped(hp.x, hp.y); }
                    Log.Info($"Placement start at {hp}; borders L {BaseGridMgr.I.LeftBorderX} R {BaseGridMgr.I.RightBorderX} T {BaseGridMgr.I.TopBorderY} B {BaseGridMgr.I.BottomBorderY} tile {BaseGridMgr.kSpaceWidth} rearrange {Rearranging}; grid x {BaseGrid.MinX}..{BaseGrid.MaxX} y {BaseGrid.MinY}..{BaseGrid.MaxY}");
                    Speech.Event(What() + Loc.Get(Rearranging ? "rearrange_moving" : "place_start") + ". " + Loc.F("place_keys", Bindings.KeyOf("paths")));
                    // The preview animates in: its first legality reading is wrong, wait for it.
                    _announceAt = Time.unscaledTime + 0.5f;
                }
                if (!active && _wasActive) { _kbOwned = false; _step = SearchStep.None; }
                _wasActive = active;
                if (_cancelPending) PendingCancel(active);
                if (_checkDropAt > 0 && Time.unscaledTime >= _checkDropAt)
                {
                    _checkDropAt = -1f;
                    Speech.Event(Loc.Get(Rearranging ? "place_refused" : "rearrange_dropped"));
                }
                if (!active) return;

                if (_step != SearchStep.None) StepSearch();
                WatchRotation();
                if (_announceAt > 0 && Time.unscaledTime >= _announceAt) { _announceAt = -1f; Announce(); }
                if (Keys.Consumed || !Keys.GameHasFocus() || Keys.Ctrl || Keys.Alt || ModKeys.ListOpen || UI.Navigator.Blocking) return;

                float step = BaseGridMgr.kSpaceWidth > 0.01f ? BaseGridMgr.kSpaceWidth : 1f;
                if (Keys.Pressed(Keys.VK_LEFT)) MoveBy(-step, 0);
                else if (Keys.Pressed(Keys.VK_RIGHT)) MoveBy(step, 0);
                else if (Keys.Pressed(Keys.VK_UP)) MoveBy(0, step);
                else if (Keys.Pressed(Keys.VK_DOWN)) MoveBy(0, -step);
                else if (Keys.Pressed(Keys.VK_HOME)) { Keys.Consume(); JumpToBest(); }
                else if (Bindings.Pressed("paths")) { Keys.Consume(); SayPaths(); }
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
            if (g.GetPlacePreview() != null) SetHoverSnapped(at.x, at.y);
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

        /// <summary>
        /// Where the game would put the preview for this point: BaseGridMgr.ClampBuildingPreview, what its mouse handler
        /// uses (nearest valid land; tile centres for an odd size, tile corners for an even one, by the turned size).
        /// SetHover(force) does not snap, and a keyboard hover off that grid left buildings half a tile off (2026-11-22).
        /// </summary>
        private static Vector3 Snap(Vector3 v)
        {
            try { var g = BaseGridMgr.I; if (g != null && g.GetPlacePreview() != null) return g.ClampBuildingPreview(v); } catch { }
            return v;
        }

        private static void SetHoverSnapped(float x, float y)
        {
            var v = Snap(new Vector3(x, y, 0f));
            _x = v.x; _y = v.y;
            BaseGridMgr.I.SetHover(_x, _y, true);
        }

        private static void MoveBy(float dx, float dy)
        {
            Claim();
            _step = SearchStep.None;
            var g = BaseGridMgr.I;
            // Past the border: stay. Clamping to the border put a 2 by 2 building's hover half a tile off its grid
            // (2026-11-22: rows read 12, 12, 11 and the shape straddled two rows).
            float nx = _x + dx, ny = _y + dy;
            if (nx < g.LeftBorderX - 0.01f || nx > g.RightBorderX + 0.01f || ny < g.BottomBorderY - 0.01f || ny > g.TopBorderY + 0.01f)
            {
                Audio.Cues.Play("wall", dx < 0 ? -1f : dx > 0 ? 1f : 0f, 0.6f);
                Speech.SayMenu(Loc.Get("explore_edge"));
                return;
            }
            SetHoverSnapped(nx, ny);
            if (Config.Debug) Log.Info($"Placement move to ({_x},{_y}) -> hover {g.GetHoverPos()}");
            // The preview's colour (legality) refreshes on the next frames.
            _announceAt = Time.unscaledTime + 0.08f;
        }

        // ---------------- Home: the best spot ----------------
        // The old search moved the preview over the game's empty spots one by one, a few frames each, with the game's
        // hover sound every time; on a full base it ran for minutes and looked stuck (a player: "the movement sound keeps
        // playing forever"). Now every spot is checked at once from the chunk grid and the building's shape, scored by
        // whether workers can reach it and what it would cut off, and the preview is moved (and turned) once.

        private enum SearchStep { None, Learn, Settle }
        private static SearchStep _step;
        private static int _learnLeft, _settleFrames, _tries, _wantRot;
        private static List<Cand> _cands;
        private static Vector3 _searchFrom;

        private sealed class Cand { public Vector2Int Hover; public int Rot; public List<Vector2Int> Cells; public Paths.Result Res; public float Score; }

        private static BuildingObj Moving()
        {
            var g = BaseGridMgr.I;
            return g == null ? null : g.GetPlacePreview() ?? g.GetPendingRearrangeBuilding();
        }

        /// <summary>The building picked up in rearrange mode (its old tiles are free for it), or null.</summary>
        private static BuildingObj Pending()
        {
            try { return BaseGridMgr.I?.GetPendingRearrangeBuilding(); } catch { return null; }
        }

        private static bool CanTurn()
        {
            try { var g = BaseGridMgr.I; return g.GetPlacePreview() != null && g.CanRotatePlacePreview(); } catch { return false; }
        }

        private static void JumpToBest()
        {
            Claim();
            var o = Moving();
            if (o == null || o.Inst == null) return;
            _tries = 0;
            _searchFrom = new Vector3(_x, _y, 0);
            _announceAt = -1f;
            Speech.SayMenu(Loc.Get("place_searching"));
            // Learn the shape at every rotation first (the preview turns once round, a moment each).
            int unknown = 0;
            bool poly = false;
            try { poly = o.Inst.GetInfo().ColType == ColliderType.kPoly; } catch { }
            if (poly && CanTurn()) for (int r = 0; r < 4; r++) if (!Footprint.Known(o.Inst.Type, r)) unknown++;
            if (unknown > 0) { _step = SearchStep.Learn; _learnLeft = 4; _settleFrames = 0; return; }
            Choose();
        }

        private static void StepSearch()
        {
            var g = BaseGridMgr.I;
            var o = Moving();
            if (o == null || o.Inst == null) { _step = SearchStep.None; return; }
            if (++_settleFrames < 3) return;
            if (_step == SearchStep.Learn)
            {
                if (Footprint.Measure(o) == null)
                {
                    if (_settleFrames > 120) { Log.Warn("Placement: the shape could not be read"); _step = SearchStep.None; Choose(); }
                    return;
                }
                if (_learnLeft-- <= 0) { _step = SearchStep.None; Choose(); return; }
                g.RotatePlacePreview(1);
                SetHoverSnapped(_x, _y);
                _settleFrames = 0;
                return;
            }
            if (_step == SearchStep.Settle)
            {
                if (o.Inst.Rotation != _wantRot && CanTurn())
                {
                    bool turning = false;
                    try { turning = o.IsRotating(); } catch { }
                    if (!turning) { g.RotatePlacePreview(1); SetHoverSnapped(_x, _y); }
                    _settleFrames = 0;
                    return;
                }
                if (Footprint.Measure(o) == null && _settleFrames < 120) return;
                bool fits = g.GetPlacePreview() == null || g.CanBuildPlacePreview();
                var c = _cands != null && _cands.Count > 0 ? _cands[0] : null;
                if (fits || c == null)
                {
                    _step = SearchStep.None;
                    if (c == null) return;
                    var hp = g.GetHoverPos(); _x = hp.x; _y = hp.y;
                    var tl = Footprint.TopLeft(c.Cells);
                    string where = Loc.F("place_pos", BaseGrid.Col(tl.x), BaseGrid.Row(tl.y), Loc.Get(fits ? "place_ok" : "place_blocked"));
                    string shape = Footprint.Describe(c.Cells);
                    string words = Paths.Words(c.Res, Paths.PassOverType(o.Inst.Type));
                    Log.Info($"Placement best spot: hover {c.Hover} rotation {c.Rot} score {c.Score} after {_tries} refusals; {words}");
                    Speech.SayMenu(Loc.Get("place_best") + " " + where + (shape.Length > 0 ? ". " + char.ToUpperInvariant(shape[0]) + shape.Substring(1) : "") + ". " + words);
                    Audio.Cues.Play(fits ? "tick" : "wall", 0f, 0.6f);
                    return;
                }
                // The game's collider test refused it (a neighbour's collider reaches over): the next best.
                Log.Info($"Placement: spot {c.Hover} rotation {c.Rot} refused by the game; preview at {o.transform.position} anchor {Footprint.Anchor(o)} hover {g.GetHoverPos()} rotation {o.Inst.Rotation}, cells {string.Join(" ", c.Cells)}");
                _cands.RemoveAt(0);
                if (++_tries >= 8 || _cands.Count == 0) { GiveUp(); return; }
                Go(_cands[0]);
            }
        }

        private static void Choose()
        {
            var o = Moving();
            if (o == null || o.Inst == null) return;
            BaseGrid.Refresh();
            var g = BaseGridMgr.I;
            var type = o.Inst.Type;
            var inf = o.Inst.GetInfo();
            var pend = Pending();
            var ignore = pend != null ? pend.Inst : null;
            bool passOver = Paths.PassOverType(type);
            var rots = new List<int>();
            if (CanTurn()) { for (int r = 0; r < 4; r++) if (Footprint.Known(type, r)) rots.Add(r); }
            // A rectangle has two ways round (a half turn covers the same tiles).
            if (rots.Count == 0) { rots.Add(o.Inst.Rotation); if (CanTurn() && inf.TileSize.x != inf.TileSize.y) rots.Add((o.Inst.Rotation + 1) % 4); }
            var cands = new List<Cand>();
            var here = BaseGrid.HoverCell(new Vector3(_searchFrom.x, _searchFrom.y, 0));
            float t0 = Time.realtimeSinceStartup;
            for (int x = BaseGrid.MinX - 3; x <= BaseGrid.MaxX + 3; x++)
                for (int y = BaseGrid.MinY - 3; y <= BaseGrid.MaxY + 3; y++)
                    foreach (int r in rots)
                    {
                        var h = new Vector2Int(x, y);
                        var cells = Footprint.At(type, r, h) ?? Footprint.Box(inf, h, r);
                        if (Footprint.Obstacle(cells, ignore).Length > 0) continue;
                        var res = Paths.Analyse(cells, pend, passOver);
                        float score = Paths.Score(res) - 0.5f * (Math.Abs(h.x - here.x) + Math.Abs(h.y - here.y)) - (r != o.Inst.Rotation ? 1f : 0f);
                        cands.Add(new Cand { Hover = h, Rot = r, Cells = cells, Res = res, Score = score });
                    }
            if (cands.Count == 0) { Speech.SayMenu(Loc.Get("place_no_fit")); return; }
            cands.Sort((a, b) => b.Score.CompareTo(a.Score));
            if (cands.Count > 12) cands.RemoveRange(12, cands.Count - 12);
            _cands = cands;
            Log.Info($"Placement: best of the free spots for {type} in {(Time.realtimeSinceStartup - t0) * 1000f:0} ms, rotations {string.Join(",", rots)}: first {cands[0].Hover} rotation {cands[0].Rot} score {cands[0].Score}");
            Go(cands[0]);
        }

        /// <summary>Move (and turn) the preview to a candidate; StepSearch checks it once it has settled.</summary>
        private static void Go(Cand c)
        {
            var g = BaseGridMgr.I;
            var hp = g.GetHoverPos();
            var cur = BaseGrid.HoverCell(hp);
            // The preview stands on the grid where the game snapped it; step from there (the shapes are learned from it).
            try { var o = Moving(); if (o != null) { var pp = o.transform.position; hp = new Vector3(pp.x, pp.y, 0f); cur = Footprint.Anchor(o); } } catch { }
            float step = BaseGridMgr.kSpaceWidth > 0.01f ? BaseGridMgr.kSpaceWidth : 1.125f;
            float sx = BaseGrid.World(1, 0).x - BaseGrid.World(0, 0).x, sy = BaseGrid.World(0, 1).y - BaseGrid.World(0, 0).y;
            if (Math.Abs(sx) < 0.01f) sx = step;
            if (Math.Abs(sy) < 0.01f) sy = step;
            Claim();
            SetHoverSnapped(hp.x + (c.Hover.x - cur.x) * sx, hp.y + (c.Hover.y - cur.y) * sy);
            _wantRot = c.Rot;
            _step = SearchStep.Settle;
            _settleFrames = 0;
        }

        private static void GiveUp()
        {
            _step = SearchStep.None;
            Log.Info("Placement: no candidate was accepted by the game");
            _x = _searchFrom.x; _y = _searchFrom.y;
            BaseGridMgr.I.SetHover(_x, _y, true);
            Speech.SayMenu(Loc.Get("place_no_fit"));
        }

        /// <summary>The tiles the building being placed or moved covers now: its measured shape, else its listed box.</summary>
        private static List<Vector2Int> CurrentCells(BuildingObj o, out bool measured)
        {
            measured = false;
            var g = BaseGridMgr.I;
            var hover = Footprint.Anchor(o);
            var m = Footprint.Measure(o);
            if (m != null) { measured = true; return m; }
            var known = Footprint.At(o.Inst.Type, o.Inst.Rotation, hover);
            if (known != null) { measured = true; return known; }
            return Footprint.Box(o.Inst.GetInfo(), hover, o.Inst.Rotation);
        }

        private static int _announceTries;
        private static bool _turned;

        private static void Announce()
        {
            var g = BaseGridMgr.I;
            var o = Moving();
            if (o == null || o.Inst == null) return;
            // Wait for the preview to stop sliding / turning (its shape and the game's fit test read where it stands).
            bool still = true;
            try { still = !o.IsMoving() && !o.IsRotating(); } catch { }
            if (still)
            {
                // A building just picked up in rearrange mode flies to the cursor: its first position was the far
                // corner of the base ("column 0, row 14").
                var a = Footprint.Anchor(o); var hc = BaseGrid.HoverCell(g.GetHoverPos());
                if (Math.Abs(a.x - hc.x) > 1 || Math.Abs(a.y - hc.y) > 1) still = false;
            }
            if (!still && _announceTries++ < 30) { _announceAt = Time.unscaledTime + 0.05f; return; }
            _announceTries = 0;
            var hp = g.GetHoverPos();
            // The game snaps the preview to its grid: step on from where it actually is.
            _x = hp.x; _y = hp.y;
            BaseGrid.Refresh();
            var cells = CurrentCells(o, out bool measured);
            var tl = Footprint.TopLeft(cells);
            int col = BaseGrid.Col(tl.x), row = BaseGrid.Row(tl.y);
            if (Config.Debug) Log.Info($"Placement announce hover {hp} cell {BaseGrid.HoverCell(hp)} -> col {col} row {row}, {cells.Count} tiles ({(measured ? "measured" : "box")}), rotation {o.Inst.Rotation}");
            string prefix = "";
            if (_turned)
            {
                _turned = false;
                Footprint.Bounds(cells, out int x0, out int x1, out int y0, out int y1);
                string shape = Footprint.Describe(cells);
                prefix = Loc.Get("place_turned") + ", " + (shape.Length > 0 ? shape : BaseGrid.Size(new Vector2Int(x1 - x0 + 1, y1 - y0 + 1))) + ". ";
            }
            bool noPreview = g.GetPlacePreview() == null;
            var pend = Pending();
            string why = Footprint.Obstacle(cells, pend != null ? pend.Inst : null);
            bool ok = noPreview ? why.Length == 0 : g.CanBuildPlacePreview();
            if (!noPreview && ok != (why.Length == 0)) Log.Info($"Placement: the game says {(ok ? "fits" : "blocked")}, the tiles say '{why}' at {string.Join(" ", cells)}; preview at {o.transform.position} hover {hp}");
            string state = Loc.Get(ok ? "place_ok" : "place_blocked");
            if (!ok && why.Length > 0) state = why;
            Speech.SayMenu(prefix + Loc.F("place_pos", col, row, state));
            Audio.Cues.Play(ok ? "tick" : "wall", 0f, 0.6f);
        }

        /// <summary>The paths key: what this spot does to the workers' reach.</summary>
        private static void SayPaths()
        {
            var o = Moving();
            if (o == null || o.Inst == null) return;
            BaseGrid.Refresh();
            var cells = CurrentCells(o, out _);
            bool passOver = Paths.PassOverType(o.Inst.Type);
            var res = Paths.Analyse(cells, Pending(), passOver);
            string words = Paths.Words(res, passOver);
            if (Config.Debug) Log.Info(Paths.Debug(cells));
            Log.Info($"Placement paths at {Footprint.TopLeft(cells)}: self {res.Self.Direct}/{res.Self.Wall}, cut off {res.CutOff.Count}, wall only {res.WallOnly.Count}, lost {res.Lost}: {words}");
            Speech.SayMenu(words);
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
            if (_step != SearchStep.None) { _rotPending = false; return; }     // the Home search turns it itself
            if (_rotPending && !p.IsRotating())
            {
                _rotPending = false;
                _turned = true;
                // A turned 3 by 2 building is 2 by 3: its snap changes, so put it back on the grid.
                try { var hp0 = BaseGridMgr.I.GetHoverPos(); SetHoverSnapped(hp0.x, hp0.y); } catch { }
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

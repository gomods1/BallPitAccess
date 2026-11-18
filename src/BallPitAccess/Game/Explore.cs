using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using BallPitAccess.Input;
using BallPitAccess.UI;
using Il2Cpp;
using Il2CppRewired;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Explore mode in the base (a player asked for it: "a mode just to walk on all the tiles and find how much
    /// free space we have"). The position key (K by default) in the base's main view opens it: the arrows walk
    /// a cursor one tile at a time over the bought land and say what is on each tile (free, a building, the
    /// entrance, land not bought yet), and on a free tile how many free tiles follow to the right and down.
    /// Tab / Shift+Tab jump to the next / previous free tile. The same key, Backspace or Escape leave.
    /// Columns and rows are the ones placement uses (BaseGrid). Nothing in the game changes; the game's
    /// keyboard maps are off while it is open, so Backspace does not reach the game's pause.
    /// </summary>
    internal static class Explore
    {
        private static bool _on;
        private static int _x, _y;
        private static HashSet<long> _free = new HashSet<long>();
        private static readonly List<ControllerMap> Muted = new List<ControllerMap>();
        private static bool _unmutePending;
        private static int _errors;

        internal static bool Active => _on;

        private static float _closedAt = -10f;

        /// <summary>
        /// BaseMgr.MyUpdate opens the pause menu in the base's normal view on Backspace, read straight from
        /// Unity's input (Input.GetKeyDown(Backspace), not through Rewired, so muting the keyboard maps does not
        /// stop it): leaving explore with Backspace also paused the game. Skip the pause menu while exploring and
        /// for a moment after.
        /// </summary>
        internal static void TryPatch(HarmonyLib.Harmony h)
        {
            try
            {
                var m = HarmonyLib.AccessTools.Method(typeof(BasePauseUI), nameof(BasePauseUI.Activate));
                if (m == null) { Log.Warn("BasePauseUI.Activate not found: Backspace in explore also pauses."); return; }
                h.Patch(m, prefix: new HarmonyLib.HarmonyMethod(typeof(Explore), nameof(BackPrefix)));
                Log.Info("Patched BasePauseUI.Activate");
            }
            catch (Exception ex) { Log.Error("Explore patch failed: " + ex); }
        }

        private static bool BackPrefix()
        {
            if (_on || _unmutePending || Time.unscaledTime - _closedAt < 0.5f)
            {
                Log.Info("Explore: pause menu skipped");
                return false;
            }
            return true;
        }

        private static long Key(int x, int y) => ((long)x << 32) | (uint)y;

        internal static void Toggle()
        {
            if (_on) { Close(true); return; }
            Open();
        }

        private static void Open()
        {
            var bm = BaseMgr.I; var g = BaseGridMgr.I;
            if (bm == null || g == null) return;
            if (bm.CurState != BaseState.kNormal || Navigator.HasOverlay || Navigator.FtueShowing)
            {
                Speech.SayMenu(Loc.Get("explore_not_here"));
                return;
            }
            if (!BaseGrid.Refresh()) return;
            ReadFree();
            // Start on the building the base list is on (the game's hover), else on the first free tile.
            var c = BaseGrid.CellOf(g.GetHoverPos());
            if (!BaseGrid.Inside(c.x, c.y))
            {
                c = new Vector2Int((BaseGrid.MinX + BaseGrid.MaxX) / 2, (BaseGrid.MinY + BaseGrid.MaxY) / 2);
                var f = NextFree(BaseGrid.MinX - 1, BaseGrid.Down > 0 ? BaseGrid.MinY : BaseGrid.MaxY, 1);
                if (f.HasValue) c = f.Value;
            }
            _x = c.x; _y = c.y;
            _lastBuilding = IntPtr.Zero;
            _on = true;
            // Reopened before the keys came back from the last walk: they are still off, keep that list.
            if (_unmutePending) _unmutePending = false;
            else Mute(true);
            Log.Info($"Explore open: grid x {BaseGrid.MinX}..{BaseGrid.MaxX} y {BaseGrid.MinY}..{BaseGrid.MaxY}, {_free.Count} free, start ({_x},{_y})");
            Speech.SayMenu(Loc.F("explore_start", _free.Count, BaseGrid.NumCols, BaseGrid.NumRows, Bindings.KeyOf("position")) + ". " + Describe(_x, _y));
        }

        internal static void Close(bool say)
        {
            if (!_on) return;
            _on = false;
            _closedAt = Time.unscaledTime;
            _unmutePending = true;      // once the closing key is up, or the game would see its release
            if (say) Speech.SayMenu(Loc.Get("explore_end"));
            Log.Info("Explore closed");
        }

        private static void ReadFree()
        {
            _free.Clear();
            // The same test as the game's FindEmptySpots (bought land, no building in the chunk grid), over the
            // bought area only (FindEmptySpots also lists the path below the entrance).
            for (int x = BaseGrid.MinX; x <= BaseGrid.MaxX; x++)
                for (int y = BaseGrid.MinY; y <= BaseGrid.MaxY; y++)
                    if (BaseGrid.Free(x, y)) _free.Add(Key(x, y));
        }

        internal static void Update()
        {
            try
            {
                if (_unmutePending && !_on && !Keys.Down(Keys.VK_BACK) && !Keys.Down(Keys.VK_ESCAPE) && !Keys.Down(Keys.VK_RETURN)
                    && !Bindings.Down("position"))
                {
                    _unmutePending = false;
                    Mute(false);
                }
                if (!_on) return;
                var bm = BaseMgr.I;
                // Anything that takes the base away (a popup, a mode, a scene change) ends the walk quietly.
                if (bm == null || BaseGridMgr.I == null || bm.CurState != BaseState.kNormal || Navigator.HasOverlay) { Close(false); return; }
                if (ModKeys.ListOpen || !Keys.GameHasFocus() || Keys.Ctrl || Keys.Alt || Keys.ScreenReaderModifier) return;

                if (Keys.Pressed(Keys.VK_LEFT)) { Keys.Consume(); Move(-1, 0); }
                else if (Keys.Pressed(Keys.VK_RIGHT)) { Keys.Consume(); Move(1, 0); }
                else if (Keys.Pressed(Keys.VK_UP)) { Keys.Consume(); Move(0, -BaseGrid.Down); }
                else if (Keys.Pressed(Keys.VK_DOWN)) { Keys.Consume(); Move(0, BaseGrid.Down); }
                else if (Keys.Pressed(Keys.VK_TAB)) { Keys.Consume(); JumpFree(Keys.Shift ? -1 : 1); }
                else if (Keys.Pressed(Keys.VK_RETURN)) { Keys.Consume(); _lastBuilding = IntPtr.Zero; Speech.SayMenu(Describe(_x, _y)); }
                else if (Keys.Pressed(Keys.VK_BACK) || Keys.Pressed(Keys.VK_ESCAPE)) { Keys.Consume(); Close(true); }
                else if (Keys.Pressed(Keys.VK_HOME) || Keys.Pressed(Keys.VK_END)) Keys.Consume();
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Explore: " + ex);
            }
        }

        private static void Move(int dx, int dy)
        {
            int nx = _x + dx, ny = _y + dy;
            if (!BaseGrid.Inside(nx, ny))
            {
                Audio.Cues.Play("wall", dx < 0 ? -1f : dx > 0 ? 1f : 0f, 0.6f);
                Speech.SayMenu(Loc.Get("explore_edge"));
                return;
            }
            _x = nx; _y = ny;
            Speak();
        }

        private static void Speak()
        {
            bool free = _free.Contains(Key(_x, _y));
            if (free) Audio.Cues.Play("tick", 0f, 0.6f);
            Speech.SayMenu(Describe(_x, _y));
        }

        /// <summary>Reading order: top row first, left to right.</summary>
        private static int Order(int x, int y) => BaseGrid.Row(y) * 10000 + BaseGrid.Col(x);

        private static Vector2Int? NextFree(int fromX, int fromY, int dir)
        {
            int cur = Order(fromX, fromY);
            Vector2Int? best = null; int bestO = 0;
            Vector2Int? wrap = null; int wrapO = 0;
            foreach (var k in _free)
            {
                int x = (int)(k >> 32), y = (int)(uint)(k & 0xffffffff);
                if (!BaseGrid.Inside(x, y)) continue;
                int o = Order(x, y);
                if (dir > 0)
                {
                    if (o > cur && (best == null || o < bestO)) { best = new Vector2Int(x, y); bestO = o; }
                    if (wrap == null || o < wrapO) { wrap = new Vector2Int(x, y); wrapO = o; }
                }
                else
                {
                    if (o < cur && (best == null || o > bestO)) { best = new Vector2Int(x, y); bestO = o; }
                    if (wrap == null || o > wrapO) { wrap = new Vector2Int(x, y); wrapO = o; }
                }
            }
            return best ?? wrap;
        }

        private static void JumpFree(int dir)
        {
            var f = NextFree(_x, _y, dir);
            if (!f.HasValue) { Speech.SayMenu(Loc.Get("explore_no_free")); return; }
            _x = f.Value.x; _y = f.Value.y;
            Speak();
        }

        private static string Describe(int x, int y)
        {
            string what = What(x, y);
            return Loc.F("explore_cell", what, BaseGrid.Col(x), BaseGrid.Row(y));
        }

        private static IntPtr _lastBuilding = IntPtr.Zero;

        private static string What(int x, int y)
        {
            var g = BaseGridMgr.I;
            var prev = _lastBuilding;
            _lastBuilding = IntPtr.Zero;
            if (_free.Contains(Key(x, y)))
            {
                int right = 0, down = 0;
                for (int i = x; BaseGrid.Inside(i, y) && _free.Contains(Key(i, y)); i++) right++;
                for (int j = y; BaseGrid.Inside(x, j) && _free.Contains(Key(x, j)); j += BaseGrid.Down) down++;
                return Loc.F("explore_free", right, down);
            }
            if (!BaseGrid.Bought(x, y)) return Loc.Get("explore_locked");
            try { if (g.IsEntrance(x, y)) return Loc.Get("explore_entrance"); } catch { }
            try
            {
                // The chunk grid, not the colliders: a building's collider reaches over its neighbours' tiles.
                var inst = BaseGrid.BuildingAt(x, y);
                if (inst != null)
                {
                    // Still on the same building: its name only. A new one (even of the same kind right next to
                    // it) gives its size, so two fields side by side can be told apart.
                    string name = Names.Building(inst.GetInfo());
                    bool same = inst.Pointer == prev;
                    _lastBuilding = inst.Pointer;
                    string size = same ? "" : BaseGrid.SizeTiles(inst);
                    if (Config.Debug && !same)
                    {
                        var bl = inst.GetBotLeftGridPos();
                        Log.Info($"Explore: {inst.Type} at ({x},{y}): bot-left {bl}, size {inst.GetTileSize()}, rotation {inst.Rotation}, top-left tiles {BaseGrid.TopLeftTiles(inst)}");
                    }
                    return size.Length > 0 ? name + ", " + size : name;
                }
            }
            catch (Exception ex) { if (Config.Debug) Log.Warn("Explore building: " + ex.Message); }
            return Loc.Get("explore_blocked");
        }

        /// <summary>The game's keyboard maps off while exploring: arrows, Backspace and Escape are the mod's.</summary>
        private static void Mute(bool mute)
        {
            try
            {
                if (mute)
                {
                    Muted.Clear();
                    var p = InputMgr.I?.GetRewiredPlayer();
                    if (p == null) return;
                    var list = new Il2CppSystem.Collections.Generic.List<ControllerMap>();
                    p.controllers.maps.GetAllMaps(ControllerType.Keyboard, list);
                    for (int i = 0; i < list.Count; i++)
                        if (list[i].enabled) { list[i].enabled = false; Muted.Add(list[i]); }
                }
                else
                {
                    foreach (var m in Muted) m.enabled = true;
                    Muted.Clear();
                }
            }
            catch (Exception ex) { Log.Warn("Explore: could not mute game keys: " + ex.Message); }
        }
    }
}

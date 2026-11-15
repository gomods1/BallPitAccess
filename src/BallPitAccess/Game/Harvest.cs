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
    /// Keyboard harvest launch in the base (BaseState.kAimWorkers). The game aims with the mouse and
    /// launches on a click. Here: Tab / Shift+Tab cycle the buildings that can be harvested now, aiming at
    /// each (name, distance and direction spoken); Left / Right turn the aim 5 degrees; Enter or Space
    /// launches through the game's own BaseMgr.LaunchWorkers with that direction.
    /// </summary>
    internal static class Harvest
    {
        private static bool _was;
        private static float _deg = 90f;
        private static int _index = -1;
        private static int _errors;
        private static float _announced = -10f;

        internal static bool Active
        {
            get
            {
                try { var b = BaseMgr.I; return b != null && b.CurState == BaseState.kAimWorkers; }
                catch { return false; }
            }
        }

        internal static void Update()
        {
            try
            {
                bool active = Active;
                if (active && !_was && Time.unscaledTime - _announced > 2f)
                {
                    // The state flickers on entry: set up and announce once.
                    _announced = Time.unscaledTime;
                    _index = -1;
                    _deg = 90f;
                    // Start already aimed at a building (the one Enter was pressed on in the base list, or the
                    // first), so Enter alone launches at something.
                    var list = Ordered();
                    int pick = -1;
                    if (_prefer != IntPtr.Zero && Time.unscaledTime - _preferAt < 6f)
                        for (int k = 0; k < list.Count; k++) if (list[k].Pointer == _prefer) pick = k;
                    _prefer = IntPtr.Zero;
                    if (pick < 0 && list.Count > 0) pick = 0;
                    if (pick >= 0)
                    {
                        _index = pick;
                        _deg = Mathf.Clamp(Angle(list[pick].Obj.transform.position, PlayerPos()), 5f, 175f);
                        Apply();
                        Speech.Event(Loc.F("harvest_start_at", list.Count, TargetName(list[pick]), AngleWords(_deg)));
                    }
                    else Speech.Event(Loc.F("harvest_start", 0));
                }
                _was = active;
                if (!active || Keys.Consumed || !Keys.GameHasFocus() || Keys.Ctrl || Keys.Alt || ModKeys.ListOpen || Navigator.Blocking) return;

                if (Keys.Pressed(Keys.VK_TAB)) Cycle(Keys.Shift ? -1 : 1);
                else if (Keys.Pressed(Keys.VK_LEFT)) Turn(5f);
                else if (Keys.Pressed(Keys.VK_RIGHT)) Turn(-5f);
                else if (Keys.Pressed(Keys.VK_RETURN) || Keys.Pressed(Keys.VK_SPACE)) { Keys.Consume(); Launch(); }
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Harvest: " + ex);
            }
        }

        private static List<BuildingInst> Targets()
        {
            var list = new List<BuildingInst>();
            var bm = BuildingMgr.I;
            if (bm == null || bm.BuildingsById == null) return list;
            foreach (var kv in bm.BuildingsById)
            {
                var b = kv.Value;
                if (b == null || b.Obj == null) continue;
                bool ok;
                try { ok = b.CanHarvest(); } catch { ok = false; }
                if (ok) list.Add(b);
            }
            Vector3 p = PlayerPos();
            list.Sort((a, c) => Angle(a.Obj.transform.position, p).CompareTo(Angle(c.Obj.transform.position, p)));
            return list;
        }

        private static IntPtr _prefer = IntPtr.Zero;
        private static float _preferAt;

        /// <summary>The building the harvest aim should start on (set just before the harvest is opened).</summary>
        internal static void Prefer(IntPtr building) { _prefer = building; _preferAt = Time.unscaledTime; }

        /// <summary>Targets from left to right.</summary>
        private static List<BuildingInst> Ordered()
        {
            var list = Targets();
            list.Reverse();
            return list;
        }

        private static string TargetName(BuildingInst b)
        {
            string name = Readers.Tr(b.GetInfo()?.GetNameSlug());
            return name.Length > 0 ? name : b.Type.ToString().TrimStart('k');
        }

        private static Vector3 PlayerPos()
        {
            var bp = BaseMgr.I?.BPlayer;
            return bp != null ? bp.transform.position : Vector3.zero;
        }

        private static float Angle(Vector3 to, Vector3 from) => Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;

        private static void Cycle(int dir)
        {
            var list = Ordered();
            if (list.Count == 0) { Speech.SayMenu(Loc.Get("harvest_none")); return; }
            _index = _index < 0 ? (dir > 0 ? 0 : list.Count - 1) : (_index + dir + list.Count) % list.Count;
            var b = list[_index];
            Vector3 p = PlayerPos();
            _deg = Mathf.Clamp(Angle(b.Obj.transform.position, p), 5f, 175f);
            Apply();
            string name = TargetName(b);
            float dist = Vector2.Distance(b.Obj.transform.position, p) / (BaseGridMgr.kSpaceWidth > 0.01f ? BaseGridMgr.kSpaceWidth : 1f);
            Speech.SayMenu(Loc.F("harvest_target", name, Mathf.RoundToInt(dist), AngleWords(_deg), _index + 1, list.Count));
        }

        private static void Turn(float d)
        {
            _deg = Mathf.Clamp(_deg + d, 5f, 175f);
            Apply();
            Speech.SayMenu(AngleWords(_deg));
        }

        private static Vector2 Dir() => new Vector2(Mathf.Cos(_deg * Mathf.Deg2Rad), Mathf.Sin(_deg * Mathf.Deg2Rad));

        /// <summary>Show the aim to sighted helpers too (the game's dotted preview follows the player's aim).</summary>
        private static void Apply()
        {
            try { BaseMgr.I?.BPlayer?.SetLastAimDir(Dir()); } catch { }
        }

        private static void Launch()
        {
            var b = BaseMgr.I;
            if (b == null) return;
            b.LaunchWorkers(Dir());
            Log.Info("Harvest launched at " + _deg + " degrees");
            Speech.Event(Loc.Get("harvest_launched"));
        }

        internal static string AngleWords(float deg)
        {
            float off = deg - 90f;
            if (Math.Abs(off) < 1f) return Loc.Get("straight_up");
            return Loc.F(off > 0 ? "deg_left" : "deg_right", Mathf.RoundToInt(Math.Abs(off)));
        }
    }
}

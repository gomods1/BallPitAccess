using System;
using BallPitAccess.Core;
using BallPitAccess.Input;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Walks the hero for the player, through the game's own input override (Player.IsOverridingInput /
    /// InputXOverride / InputYOverride), so the game's movement, speed and collisions still apply.
    /// - Catch: a main ball is coming down: walk sideways to where it lands (Battle.BallLandX) and stop
    ///   under it. A caught ball can be shot again at once; a missed one takes 2 to 3 s to reload.
    /// - Collect: no ball coming: walk to the nearest pickup the hero can reach (XP crystals do not
    ///   collect themselves). Only while Shift is held, never in automatic mode.
    /// - Dodge first: while a shot, laser or bomb will hit the hero (Hazards.Dodging), it steps to the side
    ///   the "_hit" cue points to instead. Surviving matters more than a catch.
    /// W and S still move the hero forward and back while the assist steers sideways (catching), and the
    /// player's own A or D always wins.
    /// Modes (F4): off; hold Shift (default); automatic = catch whenever a ball comes and A / D are up.
    /// Fallback: if the override does not move the hero, it is moved with Player.SetPos at its own speed.
    /// </summary>
    internal static class CatchAssist
    {
        internal enum Mode { Off, Hold, Auto, Count }
        internal static Mode Current => (Mode)Math.Max(0, Math.Min((int)Mode.Count - 1, Config.CatchMode));

        private static bool _active, _logged, _useSetPos;
        internal static bool Active => _active;
        private static float _pushSince = -1f, _pushStartX;
        private static float _speed = 6f;          // world units per game second; learned from the player's own walking
        private static float _lastX, _lastT = -1f;
        private static int _errors;

        internal static void Update()
        {
            try { Run(); }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("CatchAssist: " + ex);
                Release();
            }
        }

        private static void Run()
        {
            var p = Player.I;
            if (!Battle.Playing || p == null || Current == Mode.Off || ModKeys.ListOpen || !Keys.GameHasFocus())
            {
                Release();
                return;
            }
            bool left = Keys.Down('A'), right = Keys.Down('D');
            bool fwd = Keys.Down('W'), back = Keys.Down('S');
            LearnSpeed(p, left || right);

            bool held = Bindings.Down("catch") && !Keys.Ctrl && !Keys.Alt;
            bool want = Current == Mode.Hold ? held : held || (!left && !right);
            if (!want || left || right) { Release(); return; }

            float x, y = (fwd ? 1f : 0f) - (back ? 1f : 0f);
            Vector3 pos = Battle.PlayerPos;
            if (Hazards.Dodging)
                x = Hazards.DodgeDir;
            else if (held && Battle.Hurt && Pickups.NearestHealth(out var heal))
            {
                // Hurt and a health item is lying in reach: it matters more than one catch.
                x = Steer(heal.x - pos.x);
                if (y == 0f) y = Steer(heal.y - pos.y);
            }
            else if (Battle.BallComing)
            {
                float off = Battle.BallLandX - pos.x;
                x = Steer(off);
            }
            else if (held && Pickups.Nearest(out var tgt))
            {
                x = Steer(tgt.x - pos.x);
                if (y == 0f) y = Steer(tgt.y - pos.y);
            }
            else { Release(); return; }

            if (x == 0f && y == 0f && !_active) return;
            Drive(p, x, y);
        }

        /// <summary>Full speed far away, slowing in the last half unit so the hero stops under the target.</summary>
        private static float Steer(float off)
        {
            if (Math.Abs(off) < 0.08f) return 0f;
            return Mathf.Clamp(off / 0.5f, -1f, 1f);
        }

        private static void Drive(Player p, float x, float y)
        {
            if (!_logged) { _logged = true; Log.Info("Catch assist engaged (input override)."); }
            float now = Time.unscaledTime;
            if (_useSetPos)
            {
                p.IsOverridingInput = false;
                float dt = Time.deltaTime, step = _speed * dt;
                Vector3 pos = p.transform.position;
                var dir = new Vector2(x, y);
                if (dir.sqrMagnitude > 1f) dir.Normalize();
                float nx = p.ClampXPos(pos.x + dir.x * step), ny = p.ClampYPos(pos.y + dir.y * step);
                p.SetPos(new Vector3(nx, ny, pos.z));
                _active = true;
                return;
            }
            p.IsOverridingInput = true;
            p.InputXOverride = x;
            p.InputYOverride = y;
            _active = true;
            // Verify once that the override really moves the hero; if not, steer by position instead.
            if (Math.Abs(x) >= 0.99f)
            {
                float px = p.transform.position.x;
                if (_pushSince < 0f) { _pushSince = now; _pushStartX = px; }
                else if (now - _pushSince > 0.35f)
                {
                    bool atWall = px <= p.GetMinX() + 0.05f || px >= p.GetMaxX() - 0.05f;
                    if (Math.Abs(px - _pushStartX) < 0.05f && !atWall && Time.timeScale > 0.01f)
                    {
                        _useSetPos = true;
                        p.IsOverridingInput = false;
                        Log.Warn("Catch assist: the input override did not move the hero; moving it with SetPos instead.");
                    }
                    _pushSince = -2f; // checked: do not test again this session
                }
            }
            else if (_pushSince >= 0f) _pushSince = -1f;
        }

        private static void Release()
        {
            if (!_active) return;
            _active = false;
            if (_pushSince >= 0f) _pushSince = -1f;
            try
            {
                var p = Player.I;
                if (p != null) { p.IsOverridingInput = false; p.InputXOverride = 0f; p.InputYOverride = 0f; }
            }
            catch { }
        }

        /// <summary>The hero's walking speed, measured while the player walks with A or D (used only by the
        /// SetPos fallback).</summary>
        private static void LearnSpeed(Player p, bool walking)
        {
            float t = Time.time, x = p.transform.position.x;
            if (walking && !_active && _lastT >= 0f && t - _lastT > 0.001f)
            {
                float v = Math.Abs(x - _lastX) / (t - _lastT);
                if (v > 1f && v < 30f) _speed = Mathf.Lerp(_speed, v, 0.1f);
            }
            _lastX = x;
            _lastT = t;
        }

        internal static void CycleMode(int dir)
        {
            int n = (int)Mode.Count;
            Config.CatchMode = ((int)Current + (dir < 0 ? n - 1 : 1)) % n;
        }
    }
}

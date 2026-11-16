using System;
using BallPitAccess.Audio;
using BallPitAccess.Core;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Aim assist (an opt-out assist, disclosed in the README). The game turns the mouse position into a
    /// launch direction in BallMgr.MousePosToAimDir; a postfix replaces that direction with one toward the
    /// chosen target, clamped by the game's own ClampAimDir so heroes keep their aim limits.
    /// Modes: Off (the mouse aims), Lowest (the enemy closest to the hero), Strongest (most health),
    /// Column (the closest enemy roughly in line with the hero), Fixed (an angle set with F6/F7).
    /// </summary>
    internal static class AimAssist
    {
        internal enum Mode { Off, Lowest, Strongest, Column, Fixed, Count }

        internal static Mode Current => (Mode)Mathf.Clamp(Config.AimMode, 0, (int)Mode.Count - 1);

        /// <summary>Fixed-mode angle in degrees: 90 straight up, lower values aim right, higher aim left.</summary>
        private static float _fixedDeg = 90f;
        private static GridPieceInst _target;
        internal static GridPieceInst Target => _target;
        private static float _nextPick;
        private static int _calls;
        private static bool _loggedCalls;

        internal static void TryPatch(HarmonyLib.Harmony h)
        {
            try
            {
                var m = AccessTools.Method(typeof(BallMgr), nameof(BallMgr.MousePosToAimDir));
                if (m == null) { Log.Warn("BallMgr.MousePosToAimDir not found: aim assist disabled."); return; }
                h.Patch(m, postfix: new HarmonyMethod(typeof(AimAssist), nameof(Postfix)));
                Log.Info("Patched BallMgr.MousePosToAimDir");
            }
            catch (Exception ex) { Log.Error("Aim assist patch failed: " + ex); }
        }

        private static void Postfix(int charIdx, ref Vector2 __result)
        {
            try
            {
                _calls++;
                var mode = Current;
                if (mode == Mode.Off || !Battle.InBattle) return;
                Vector2 dir;
                if (mode == Mode.Fixed)
                {
                    float r = _fixedDeg * Mathf.Deg2Rad;
                    dir = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
                }
                else
                {
                    if (_target == null) return;
                    var obj = _target.Obj;
                    if (obj == null) return;
                    Vector3 start = Player.I != null ? Player.I.GetAimStartPos(charIdx) : Battle.PlayerPos;
                    Vector3 tgt = obj.GetWorldCenterPos() + _aimOffset;
                    dir = new Vector2(tgt.x - start.x, tgt.y - start.y);
                    if (dir.sqrMagnitude < 0.0001f) return;
                    dir.Normalize();
                }
                // Characters that shoot backward or sideways keep their own aiming: the assist only knows "at the enemy".
                var am = AimMode.kDefault;
                try { var cb = CharBattleInst.I; if (cb != null) am = cb.GetAimMode(); } catch { }
                if (mode != Mode.Fixed && (am == AimMode.kBack || am == AimMode.kSides)) return;
                __result = BallMgr.ClampAimDir(dir, am, charIdx);
            }
            catch (Exception ex)
            {
                if (_calls < 5) Log.Error("Aim assist postfix: " + ex.Message);
            }
        }

        /// <summary>Main thread, every frame: pick the target (with a little hysteresis).</summary>
        internal static void Update()
        {
            float now = Time.unscaledTime;
            if (!_loggedCalls && now > 30f && Battle.InBattle) { _loggedCalls = true; Log.Info("Aim postfix calls so far: " + _calls); }
            var mode = Current;
            if (!Battle.InBattle || mode == Mode.Off || mode == Mode.Fixed) { _target = null; return; }
            bool valid = _target != null && _target.CurState != PieceState.kDead && _target.Obj != null && _target.Obj.IsActive;
            if (now < _nextPick && valid)
            {
                // Same target: keep the aim point on a clear part of it as the hero and the enemies move.
                if (now >= _nextPoint) { _nextPoint = now + 0.1f; RefreshPoint(); }
                LogDrift(now);
                return;
            }
            _nextPick = now + 0.3f;
            _nextPoint = now + 0.1f;
            Pick(mode);
        }

        // ---------------- Target choice (a player found the assist "drifts" and kills bosses slowly) ----------------
        // 1. Aim at the enemy's real hitbox (GridPieceObj.Col bounds), at a point with a clear line from the
        //    hero: the centre, or else a point further along its width that no other enemy covers.
        // 2. Skip enemies at an angle the hero cannot shoot (the game clamps the direction and the ball flies past).
        // 3. Closest-enemy mode goes for the boss when there is one, unless another enemy is about to reach
        //    the attack line.

        private static float _targetX;
        private static Vector3 _aimOffset;       // aim point minus the target's centre, so the postfix follows it
        private static float _nextPoint, _nextDriftLog;
        private static int _driftLogs;
        private const float BallR = 0.22f;      // margin for the ball's size around other enemies (1 column = 1.125)

        private struct Box { public Battle.Threat T; public Bounds B; }
        private static readonly System.Collections.Generic.List<Box> Boxes = new System.Collections.Generic.List<Box>();

        private static void CollectBoxes()
        {
            Boxes.Clear();
            float half = Battle.ColW * 0.45f;
            foreach (var t in Battle.Threats)
            {
                Bounds b;
                try
                {
                    var col = t.Obj != null ? t.Obj.Col : null;
                    b = col != null && col.enabled ? col.bounds : new Bounds(t.Center, new Vector3(half * 2f, half * 2f, 1f));
                    if (b.size.x < 0.05f || b.size.y < 0.05f) b = new Bounds(t.Center, new Vector3(half * 2f, half * 2f, 1f));
                }
                catch { b = new Bounds(t.Center, new Vector3(half * 2f, half * 2f, 1f)); }
                Boxes.Add(new Box { T = t, B = b });
            }
        }

        private static Vector3 AimStart()
        {
            try { if (Player.I != null) return Player.I.GetAimStartPos(0); } catch { }
            return Battle.PlayerPos;
        }

        private static AimMode GameAimMode()
        {
            try { var cb = CharBattleInst.I; if (cb != null) return cb.GetAimMode(); } catch { }
            return AimMode.kDefault;
        }

        /// <summary>The game can shoot in this direction without clamping it (within 3 degrees).</summary>
        private static bool Reachable(Vector2 d, AimMode am)
        {
            if (d.sqrMagnitude < 0.0001f) return false;
            try { return Vector2.Angle(BallMgr.ClampAimDir(d.normalized, am, 0), d) < 3f; }
            catch { return true; }
        }

        /// <summary>Where the segment start→end enters the box (0..1), or -1 when it misses it.</summary>
        private static float Enter(Vector2 s, Vector2 e, Bounds b, float pad)
        {
            Vector2 d = e - s, min = (Vector2)b.min - new Vector2(pad, pad), max = (Vector2)b.max + new Vector2(pad, pad);
            float t0 = 0f, t1 = 1f;
            for (int k = 0; k < 2; k++)
            {
                float sk = k == 0 ? s.x : s.y, dk = k == 0 ? d.x : d.y, lo = k == 0 ? min.x : min.y, hi = k == 0 ? max.x : max.y;
                if (Math.Abs(dk) < 1e-6f) { if (sk < lo || sk > hi) return -1f; continue; }
                float a = (lo - sk) / dk, c = (hi - sk) / dk;
                if (a > c) { float tmp = a; a = c; c = tmp; }
                t0 = Math.Max(t0, a); t1 = Math.Min(t1, c);
                if (t0 > t1) return -1f;
            }
            return t0;
        }

        /// <summary>A point on the target the ball can reach without touching another enemy first.</summary>
        private static Vector3 AimPoint(Box tb, Vector3 start, out bool clear)
        {
            Bounds b = tb.B;
            float w = Math.Max(0f, b.size.x * 0.5f - BallR * 0.5f);
            float y = b.center.y;
            float[] offs = { 0f, -0.35f, 0.35f, -0.7f, 0.7f, -0.95f, 0.95f };
            foreach (float f in offs)
            {
                var p = new Vector3(b.center.x + f * w, y, b.center.z);
                // Stop the line where it enters the target: anything beyond does not matter.
                float tin = Enter(start, p, b, 0f);
                Vector2 s = start, e = tin > 0f ? Vector2.Lerp(start, p, tin) : (Vector2)p;
                bool blocked = false;
                foreach (var o in Boxes)
                {
                    if (o.T == tb.T || (o.T.Boss && tb.T.Boss)) continue;   // parts of the same boss do not hide it
                    if (Enter(s, e, o.B, BallR) >= 0f) { blocked = true; break; }
                }
                if (!blocked) { clear = true; return p; }
            }
            clear = false;
            return b.center;
        }

        private static void Pick(Mode mode)
        {
            CollectBoxes();
            Vector3 start = AimStart();
            var am = GameAimMode();
            bool bossFirst = false;
            if (mode == Mode.Lowest)
            {
                bool boss = false, urgent = false;
                foreach (var t in Battle.Threats) { if (t.Boss) boss = true; else if (t.Line < 1.5f) urgent = true; }
                bossFirst = boss && !urgent;
            }
            float col = Battle.ColW > 0.01f ? Battle.ColW : 1f;
            Box best = default, cur = default;
            Vector3 bestPoint = Vector3.zero, curPoint = Vector3.zero;
            float bestScore = float.MaxValue, curScore = float.MaxValue;
            foreach (var tb in Boxes)
            {
                var t = tb.T;
                Vector3 p = AimPoint(tb, start, out bool clear);
                float score;
                switch (mode)
                {
                    case Mode.Strongest: score = -t.Inst.CurHealth; break;
                    case Mode.Column:
                        float off = Math.Abs(t.Center.x - Battle.PlayerPos.x);
                        score = t.Rows + (off > col * 1.5f ? 1000f + off : 0f);
                        break;
                    default: score = t.Rows - (bossFirst && t.Boss ? 5000f : 0f); break;
                }
                if (!Reachable(p - start, am)) score += 100000f;   // the game would clamp it: only if nothing else
                else if (!clear) score += 500f;                     // a ball would hit another enemy first
                if (score < bestScore) { bestScore = score; best = tb; bestPoint = p; }
                if (_target != null && t.Inst.Pointer == _target.Pointer) { cur = tb; curPoint = p; curScore = score; }
            }
            // Stay on the current target unless the new one is clearly better (1.5 rows, or a boss / a reachable
            // one): re-picking every 0.3 s switched targets 36 times a minute, each with a click.
            if (cur.T != null && curScore < 1000f && curScore <= bestScore + 1.5f) { best = cur; bestPoint = curPoint; }
            var newTarget = best.T?.Inst;
            if (newTarget != null && (_target == null || newTarget.Pointer != _target.Pointer) && Config.AimCues)
            {
                // Telemetry (2026-11-15): a click on every kill (27 a minute), and the kill is heard anyway.
                // After a kill: silent with autofire (the aim follows by itself), and when shooting by hand
                // only when the aim swings more than 1.5 columns. A switch away from a live target (a boss, a
                // closer enemy) always clicks.
                bool lastGone = _target != null && cur.T == null, auto = false;
                try { auto = PlayerPrefsMgr.sAutofire; } catch { }
                if (!lastGone || (!auto && Math.Abs(best.T.Center.x - _targetX) > col * 1.5f))
                    Cues.Play("retarget", Battle.PanFor(best.T.Center.x), 0.6f);
            }
            _target = newTarget;
            if (best.T != null) _targetX = best.T.Center.x;
            _aimOffset = best.T != null ? bestPoint - best.T.Center : Vector3.zero;
        }

        private static void RefreshPoint()
        {
            if (_target == null) return;
            CollectBoxes();
            foreach (var tb in Boxes)
                if (tb.T.Inst.Pointer == _target.Pointer)
                {
                    _aimOffset = AimPoint(tb, AimStart(), out _) - tb.T.Center;
                    return;
                }
        }

        /// <summary>
        /// Latest.log: when the game's aim (BallMgr.GetCurAimDir) stays more than 3 degrees away from where the
        /// assist points, so a playtest shows whether the game turns the aim slowly toward it (at most 40 lines).
        /// </summary>
        private static void LogDrift(float now)
        {
            if (_driftLogs >= 40 || now < _nextDriftLog || !Battle.Playing || _target == null || _target.Obj == null) return;
            _nextDriftLog = now + 1f;
            try
            {
                var bm = BallMgr.I;
                if (bm == null) return;
                Vector3 start = AimStart(), tgt = _target.Obj.GetWorldCenterPos() + _aimOffset;
                Vector2 want = BallMgr.ClampAimDir(((Vector2)(tgt - start)).normalized, GameAimMode(), 0);
                Vector2 game = bm.GetCurAimDir();
                float diff = Vector2.Angle(want, game);
                if (diff < 3f) return;
                _driftLogs++;
                Log.Info($"Aim drift: assist {Mathf.Atan2(want.y, want.x) * Mathf.Rad2Deg:0.0} deg, game {Mathf.Atan2(game.y, game.x) * Mathf.Rad2Deg:0.0} deg, off {diff:0.0}, target {_target.Type}, offset {_aimOffset.x:0.00}");
            }
            catch { }
        }

        private static Mode _lastAuto = Mode.Lowest;

        internal static void CycleMode()
        {
            int m = ((int)Current + 1) % (int)Mode.Count;
            Config.AimMode = m;
            _target = null;
            Speech.SayMenu(Loc.F("aim_mode", Loc.Get("aim_" + (Mode)m)));
        }

        /// <summary>Turn the fixed angle (switching to fixed mode first, starting from where the assist was aiming).</summary>
        internal static void Turn(float deg)
        {
            var cur = Current;
            if (cur != Mode.Fixed)
            {
                if (cur != Mode.Off) _lastAuto = cur;
                // Start from the current target's direction, so the first press is a small correction.
                try
                {
                    var t = _target;
                    if (t != null && t.Obj != null && Player.I != null)
                    {
                        Vector3 start = Player.I.GetAimStartPos(0);
                        Vector3 tgt = t.Obj.GetWorldCenterPos();
                        _fixedDeg = Mathf.Round(Mathf.Atan2(tgt.y - start.y, tgt.x - start.x) * Mathf.Rad2Deg / 5f) * 5f;
                    }
                }
                catch { }
                Config.AimMode = (int)Mode.Fixed;
            }
            _fixedDeg = Mathf.Clamp(_fixedDeg + deg, 10f, 170f);
            Speech.SayMenu(Loc.F("aim_angle", Harvest.AngleWords(_fixedDeg)));
        }

        /// <summary>Leave the fixed angle and let the assist pick targets again.</summary>
        internal static void BackToAuto()
        {
            if (Current == Mode.Fixed || Current == Mode.Off) Config.AimMode = (int)_lastAuto;
            _target = null;
            Speech.SayMenu(Loc.F("aim_mode", Loc.Get("aim_" + Current)));
        }

        internal static void SayAim()
        {
            string t = Current == Mode.Fixed ? Loc.F("aim_angle", Harvest.AngleWords(_fixedDeg)) : TargetText();
            Speech.SayMenu(Loc.F("aim_mode", Loc.Get("aim_" + Current)) + (t != null ? ". " + t : ""));
        }

        internal static string TargetText()
        {
            var t = _target;
            if (t == null) return null;
            foreach (var th in Battle.Threats) if (th.Inst.Pointer == t.Pointer) return Battle.Describe(th);
            return null;
        }
    }
}

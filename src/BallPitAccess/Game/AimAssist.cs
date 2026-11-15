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
                    Vector3 tgt = obj.GetWorldCenterPos();
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
            if (now < _nextPick && _target != null && _target.CurState != PieceState.kDead && _target.Obj != null && _target.Obj.IsActive) return;
            _nextPick = now + 0.3f;
            Battle.Threat best = null;
            float bestScore = float.MaxValue;
            float col = GridMgr.kSpaceWidth > 0.01f ? GridMgr.kSpaceWidth : 1f;
            foreach (var t in Battle.Threats)
            {
                float score;
                switch (mode)
                {
                    case Mode.Strongest: score = -t.Inst.CurHealth; break;
                    case Mode.Column:
                        float off = Math.Abs(t.Center.x - Battle.PlayerPos.x);
                        score = t.Rows + (off > col * 1.5f ? 1000f + off : 0f);
                        break;
                    default: score = t.Rows; break;
                }
                if (score < bestScore) { bestScore = score; best = t; }
            }
            var newTarget = best?.Inst;
            if (newTarget != null && (_target == null || newTarget.Pointer != _target.Pointer) && Config.AimCues) Cues.Play("retarget", Battle.PanFor(best.Center.x), 0.6f);
            _target = newTarget;
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

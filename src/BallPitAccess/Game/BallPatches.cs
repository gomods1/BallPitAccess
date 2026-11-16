using System;
using BallPitAccess.Core;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// BallMgr.RemoveBall(BallObj b, bool isSide, BallReturnType retType): tells us a ball was caught.
    /// BallObj.SetState(BallObjState s): every state change, for the telemetry only (how balls end: caught,
    /// flown home, destroyed), with the ball's id and its distance to the hero.
    /// </summary>
    internal static class BallPatches
    {
        internal static void TryPatch(HarmonyLib.Harmony h)
        {
            try
            {
                var m = AccessTools.Method(typeof(BallMgr), nameof(BallMgr.RemoveBall));
                if (m == null) { Log.Warn("BallMgr.RemoveBall not found: no catch cue."); return; }
                h.Patch(m, prefix: new HarmonyMethod(typeof(BallPatches), nameof(Prefix)));
                Log.Info("Patched BallMgr.RemoveBall");
            }
            catch (Exception ex) { Log.Error("RemoveBall patch failed: " + ex); }
            if (!Telemetry.On) return;
            try
            {
                var s = AccessTools.Method(typeof(BallObj), nameof(BallObj.SetState));
                if (s != null) { h.Patch(s, prefix: new HarmonyMethod(typeof(BallPatches), nameof(StatePrefix))); Log.Info("Patched BallObj.SetState (telemetry)"); }
            }
            catch (Exception ex) { Log.Error("SetState patch failed: " + ex); }
        }

        private static void Prefix(BallObj b, bool isSide, BallReturnType retType)
        {
            try { if (!isSide) Battle.OnBallRemoved(retType); } catch { }
            if (!Telemetry.On) return;
            try
            {
                Vector3 p = b != null ? b.transform.position : Vector3.zero, h = Battle.PlayerPos;
                Telemetry.Write("rm", b != null ? b.Pointer.ToInt64().ToString("x") : "-", retType, b != null ? b.CurState.ToString() : "-",
                    isSide ? 1 : 0, p.x, p.y, h.x, h.y);
            }
            catch { }
        }

        private static void StatePrefix(BallObj __instance, BallObjState s)
        {
            try
            {
                if (__instance == null || __instance.CurState == s) return;
                Vector3 p = __instance.transform.position, h = Battle.PlayerPos;
                Telemetry.Write("bst", __instance.Pointer.ToInt64().ToString("x"), __instance.CurState, s, __instance.IsSide ? 1 : 0, p.x, p.y, h.x, h.y);
            }
            catch { }
        }
    }
}

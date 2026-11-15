using System;
using BallPitAccess.Core;
using HarmonyLib;
using Il2Cpp;

namespace BallPitAccess.Game
{
    /// <summary>BallMgr.RemoveBall(BallObj b, bool isSide, BallReturnType retType): tells us a ball was caught.</summary>
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
        }

        private static void Prefix(bool isSide, BallReturnType retType)
        {
            try { if (!isSide) Battle.OnBallRemoved(retType); } catch { }
        }
    }
}

using System;
using System.Text;
using BallPitAccess.Core;
using Il2Cpp;
using Il2CppRewired;

namespace BallPitAccess.Game
{
    /// <summary>Development dumps into UserData/BallPitAccess.</summary>
    internal static class Dump
    {
        private static bool _done;

        /// <summary>The game's current keyboard bindings per action, so mod keys never collide with them.</summary>
        internal static void Bindings()
        {
            var im = InputMgr.I;
            if (im == null) return;
            var sb = new StringBuilder();
            for (int a = 0; a < (int)GameActionType.kNum; a++)
            {
                var at = (GameActionType)a;
                string pos, neg;
                try { pos = im.GetKeyboardBtnForAction(at, Pole.Positive).ToString(); } catch (Exception ex) { pos = "?" + ex.Message; }
                try { neg = im.GetKeyboardBtnForAction(at, Pole.Negative).ToString(); } catch { neg = "?"; }
                sb.AppendLine($"{at}: +{pos} -{neg}");
            }
            try { sb.Append(Input.Pad.Describe()); } catch (Exception ex) { sb.AppendLine("pad: " + ex.Message); }
            Log.WriteFile("bindings.txt", sb.ToString());
            Log.Info("Bindings:\n" + sb);
        }

        internal static void Update()
        {
            if (_done || InputMgr.I == null || UnityEngine.Time.unscaledTime < 5f) return;
            _done = true;
            try { Bindings(); } catch (Exception ex) { Log.Warn("Binding dump failed: " + ex.Message); }
        }
    }
}

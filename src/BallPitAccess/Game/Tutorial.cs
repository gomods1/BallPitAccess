using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using BallPitAccess.Input;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Makes the game's teaching text teach the mod's controls (template 10.4, "Rewrite the game's mouse
    /// wording"). Messages are matched by their I2 localisation term, so the rewrite works in every language;
    /// a message with no rewrite keeps the game's words, with each mouse glyph replaced by the mod key for the
    /// current mode, and its term is logged so a rewrite can be added.
    /// Also reads the base's bottom hint panel and lets Space stand in for "hold right mouse" during a harvest.
    /// </summary>
    internal static class Tutorial
    {
        /// <summary>Game term → mod string key. Terms come from the log ("Tutorial term ...").</summary>
        private static readonly Dictionary<string, string> Rewrites = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["prompt_aim_workers_intro_base"] = "tut_harvest_aim",
            ["tut_cancel_harvest"] = "tut_harvest_speed",
            ["mod_tut_shoot"] = "tut_shoot",
        };

        private static readonly HashSet<string> Logged = new HashSet<string>();
        private static string _prompt = "";
        private static bool _speedHeld;
        private static int _errors;

        /// <summary>The mod key that does what this mouse button does in the current mode.</summary>
        internal static string MouseKey(string mouseName)
        {
            bool right = mouseName.Contains("right");
            try
            {
                var bm = BaseMgr.I;
                var st = bm != null ? bm.CurState : BaseState.kNum;
                // Game texts already say "hold" around the glyph: name the key only.
                if (st == BaseState.kAimWorkers || st == BaseState.kBounceWorkers) return Loc.Get(right ? "key_space" : "key_enter");
                if (Placement.Active) return Loc.Get("key_enter");
                if (Battle.InBattle) return Loc.Get(right ? "key_aim_assist" : "key_space");
            }
            catch { }
            return Loc.Get("key_enter");
        }

        /// <summary>Text for a tutorial message: the mod's rewrite for its term, or the game's text.</summary>
        internal static string Rewrite(string term, string gameText)
        {
            if (!string.IsNullOrEmpty(term) && Rewrites.TryGetValue(term, out var key)) return Loc.Get(key);
            if (!string.IsNullOrEmpty(term) && Logged.Add(term))
                Log.Info("Tutorial term without rewrite: " + term + " = " + gameText);
            return gameText;
        }

        internal static void Update()
        {
            try
            {
                TrackMainPrompt();
                SpeedUp();
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Tutorial: " + ex);
            }
        }

        /// <summary>The base's bottom hint panel ("aim at the resources and click..."), read when it changes.</summary>
        private static void TrackMainPrompt()
        {
            var ui = BaseUI.I;
            if (ui == null || ui.GrpMainPrompt == null || ui.LocMainPrompt == null) { _prompt = ""; return; }
            bool shown = ui.GrpMainPrompt.gameObject.activeInHierarchy && ui.GrpMainPrompt.alpha > 0.5f;
            string term = shown ? ui.LocMainPrompt.mTerm ?? "" : "";
            if (term == _prompt) return;
            _prompt = term;
            if (term.Length == 0) return;
            var tmp = ui.LocMainPrompt.GetComponent<TMP_Text>();
            string text = tmp != null ? Speech.Clean(tmp.text) : "";
            Speech.Event(Rewrite(term, text));
        }

        /// <summary>Holding Space while workers bounce presses the game's own speed-up button.</summary>
        private static void SpeedUp()
        {
            var bm = BaseMgr.I;
            bool bouncing = bm != null && bm.CurState == BaseState.kBounceWorkers;
            bool want = bouncing && Keys.GameHasFocus() && Keys.Down(Keys.VK_SPACE) && !ModKeys.ListOpen;
            if (want == _speedHeld) return;
            var btn = BaseUI.I?.BtnSpeedUpHarvest;
            if (btn == null) { _speedHeld = false; return; }
            var ped = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            if (want) btn.OnPointerDown(ped); else btn.OnPointerUp(ped);
            _speedHeld = want;
            Log.Info("Harvest speed-up " + (want ? "held" : "released"));
        }
    }
}

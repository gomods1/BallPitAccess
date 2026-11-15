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
    /// The parts of the game that are neither a menu nor play: the boot screens (seizure warning, shader
    /// preparation), the intro cutscene with its captions, the credits. Each is announced when it shows;
    /// its text is spoken as it changes; Enter skips where the game offers a skip with the mouse.
    /// </summary>
    internal static class Scenes
    {
        private static string _splash = "";
        private static string _caption = "";
        private static bool _introSaid, _credits;
        private static float _introSeen = -10f;
        private static int _errors;

        internal static void Update()
        {
            try
            {
                Splash();
                Intro();
                Credits();
                Endings();
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Scenes: " + ex);
            }
        }

        private static bool Visible(CanvasGroup g)
        {
            try { return g != null && g.gameObject.activeInHierarchy && g.alpha > 0.6f; } catch { return false; }
        }

        private static void Splash()
        {
            var mm = MainMenuUI.I;
            if (mm == null || !mm.IsRunningSplashScreen) { _splash = ""; return; }
            CanvasGroup shown = null;
            string id = "";
            if (Visible(mm.CvsGrpEpilepsy)) { shown = mm.CvsGrpEpilepsy; id = "epilepsy"; }
            else if (Visible(mm.CvsGrpShaders)) { shown = mm.CvsGrpShaders; id = "shaders"; }
            else if (Visible(mm.CvsGrpMobileFreeTrial)) { shown = mm.CvsGrpMobileFreeTrial; id = "trial"; }
            if (id != _splash)
            {
                _splash = id;
                if (shown != null)
                {
                    var lines = MenuReader.TextLines(shown.transform);
                    string text = lines.Count > 0 ? string.Join(". ", lines) : "";
                    if (id == "shaders") text = Readers.Join(Loc.Get("splash_shaders"), text.Length > 0 ? text : null);
                    if (text.Length > 0) Speech.Event(text);
                }
            }
            if (id == "shaders" && Keys.GameHasFocus() && Keys.Pressed(Keys.VK_RETURN) && !ModKeys.ListOpen && !Keys.Consumed)
            {
                Keys.Consume();
                try
                {
                    if (mm.BtnPrecompileSkip != null && mm.BtnPrecompileSkip.gameObject.activeInHierarchy) { mm.OnPrecompileSkipClicked(); Log.Info("Shader preparation skipped"); }
                }
                catch (Exception ex) { Log.Warn("Skip shaders: " + ex.Message); }
            }
        }

        private static void Intro()
        {
            var ic = IntroCutsceneObj.I;
            bool playing = false;
            string caption = "";
            if (ic != null && ic.gameObject.activeInHierarchy)
            {
                caption = Names.Shown(ic.Loc);
                var mm = MainMenuUI.I;
                bool menuHidden = mm == null || mm.GrpBtns == null || mm.GrpBtns.alpha < 0.1f;
                bool baseCut = false;
                try { baseCut = BaseMgr.I != null && BaseMgr.I.CurState == BaseState.kCutscene; } catch { }
                // The cutscene's own canvas is faded in while it plays; captions come and go inside it.
                bool shown = false;
                try { shown = ic.CvsGrp != null && ic.CvsGrp.gameObject.activeInHierarchy && ic.CvsGrp.alpha > 0.5f; } catch { }
                playing = shown && (menuHidden || baseCut) && !Navigator.HasOverlay;
                if (!shown) caption = "";
            }
            float now = Time.unscaledTime;
            if (!playing)
            {
                if (now - _introSeen > 1.5f) { _caption = ""; _introSaid = false; }
                return;
            }
            _introSeen = now;
            if (!_introSaid) { _introSaid = true; Speech.Event(Loc.Get("intro")); }
            if (caption.Length > 0 && caption != _caption) { _caption = caption; Speech.Event(caption); }
            if (Keys.GameHasFocus() && Keys.Pressed(Keys.VK_RETURN) && !ModKeys.ListOpen && !Keys.Consumed)
            {
                Keys.Consume();
                try { ic.Complete(true); Log.Info("Intro skipped"); }
                catch (Exception ex) { Log.Warn("Skip intro: " + ex.Message); }
            }
        }

        private static bool _ending, _endingDlc;
        private static readonly List<string> EndingSaid = new List<string>();

        /// <summary>
        /// The two ending sequences (the pit's, and Risen Ballbylon's with its captions). Neither is a menu:
        /// say that it plays, and speak each caption of the expansion's ending as it appears.
        /// </summary>
        private static void Endings()
        {
            bool main = false, dlc = false;
            try { var g = GameCompleteObj.I; main = g != null && g.gameObject.activeInHierarchy && g.IsRunning(); } catch { }
            GameCompleteObjDLC d = null;
            try { d = GameCompleteObjDLC.I; dlc = d != null && d.gameObject.activeInHierarchy && d.IsRunning(); } catch { }
            if (main && !_ending) Speech.Event(Loc.Get("ending"));
            _ending = main;
            if (dlc && !_endingDlc) { EndingSaid.Clear(); Speech.Event(Loc.Get("ending_dlc")); }
            _endingDlc = dlc;
            if (!dlc) return;
            try
            {
                var cvs = d.CvsText;
                if (cvs == null || !cvs.isActiveAndEnabled) return;
                foreach (var line in MenuReader.TextLines(cvs.transform))
                {
                    if (EndingSaid.Contains(line)) continue;
                    EndingSaid.Add(line);
                    Speech.Event(line);
                }
            }
            catch (Exception ex) { if (Config.Debug) Log.Warn("Ending text: " + ex.Message); }
        }

        private static void Credits()
        {
            var c = CreditsUI.I;
            bool running = false;
            try { running = c != null && c.IsRunning; } catch { }
            if (running && !_credits) Speech.Event(Loc.Get("credits"));
            _credits = running;
        }
    }
}

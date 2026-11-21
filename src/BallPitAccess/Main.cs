using System;
using BallPitAccess.Core;
using BallPitAccess.Game;
using BallPitAccess.Input;
using BallPitAccess.UI;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(BallPitAccess.Main), "BALL x PIT Access", "0.6.4", "BALL x PIT Access")]
[assembly: MelonGame("Kenny Sun", "BALL x PIT")]

namespace BallPitAccess
{
    public class Main : MelonMod
    {
        private bool _started;
        private int _errors;

        public override void OnInitializeMelon()
        {
            Log.Init();
            Log.Info("BALL x PIT Access " + Info.Version + " build " + System.IO.File.GetLastWriteTime(typeof(Main).Assembly.Location).ToString("yyyy-MM-dd HH:mm:ss") +
                     ", " + (IntPtr.Size * 8) + "-bit, exe " + System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName);
            Config.Init();
            Input.Bindings.Init();
            Game.Telemetry.Init();
            string bisect = "";
            try { bisect = System.IO.File.ReadAllText(System.IO.Path.Combine(Log.Dir, "bisect.txt")); } catch { }
            if (bisect.Length > 0) Log.Info("bisect: " + bisect);
            _bisect = bisect;
        }

        private bool _patched, _gameLogged;
        private int _frames;
        private string _bisect = "";

        public override void OnUpdate()
        {
            try
            {
                if (!_started)
                {
                    // Prism initialised during the loader's boot crashed this game (access violation before the
                    // first frame); from the game's own first frame it works.
                    _started = true;
                    if (!_bisect.Contains("nospeech")) Speech.Init();
                    if (!_bisect.Contains("noaudio"))
                        try { Audio.Cues.Build(); Audio.AudioEngine.Start(); } catch (Exception ex) { Log.Error("Audio init failed: " + ex); }
                    Speech.Say(Loc.Get("loaded"));
                }
                if (!_patched && Time.unscaledTime > 8f)
                {
                    // Patching game classes during boot crashed the game (access violation): wait until it runs.
                    _patched = true;
                    AimAssist.TryPatch(HarmonyInstance);
                    BallPatches.TryPatch(HarmonyInstance);
                    Placement.TryPatch(HarmonyInstance);
                    Explore.TryPatch(HarmonyInstance);
                    Harvest.TryPatch(HarmonyInstance);
                    Hazards.TryPatch(HarmonyInstance);
                }
                bool trace = _frames++ < 3;
                if (trace) Log.Info("frame " + _frames + " keys");
                Loc.Refresh();
                Keys.Update();
                if (Time.unscaledTime > 6f)
                {
                    if (trace || !_gameLogged) Log.Info("game step dump");
                    Dump.Update();
                    if (Config.Debug) DevCommands.Update();
                    ModKeys.Update();
                    if (!_gameLogged) Log.Info("game step battle");
                    Battle.Update();
                    AimAssist.Update();
                    Hazards.Update();
                    Pickups.Update();
                    CatchAssist.Update();
                    if (!_gameLogged) Log.Info("game step menu");
                    Scenes.Update();
                    Placement.Update();
                    Harvest.Update();
                    Explore.Update();
                    BaseNav.Update();
                    Tutorial.Update();
                    Navigator.Update();
                    if (!ModKeys.ListOpen) MenuReader.Update();
                    Telemetry.Update();
                    if (!_gameLogged) Log.Info("game steps ok");
                    _gameLogged = true;
                }
                if (trace) Log.Info("pump");
                Speech.Pump();
                if (trace) Log.Info("frame done");
            }
            catch (Exception ex)
            {
                if (++_errors <= 20 || _errors % 500 == 0) Log.Error("Update failed: " + ex);
            }
        }

        public override void OnApplicationQuit()
        {
            Audio.AudioEngine.Stop();
            Speech.Shutdown();
            Telemetry.Close();
        }
    }
}

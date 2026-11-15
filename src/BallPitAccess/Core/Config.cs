using MelonLoader;

namespace BallPitAccess.Core
{
    /// <summary>
    /// Mod settings, stored by MelonPreferences in UserData/MelonPreferences.cfg under [BallPitAccess].
    /// The file is storage only; players change these from the mod's settings menu (F4).
    /// </summary>
    internal static class Config
    {
        private static MelonPreferences_Category _cat;
        private static MelonPreferences_Entry<bool> _cues, _debug, _dangerRadar, _ballCues, _wallCues, _hazardCues, _autoRead, _healthCues, _aimCues;
        private static MelonPreferences_Entry<float> _cueVolume;
        private static MelonPreferences_Entry<int> _aimMode, _pickupCues, _catchMode;
        private static MelonPreferences_Entry<string> _backend;

        internal static void Init()
        {
            _cat = MelonPreferences.CreateCategory("BallPitAccess", "BALL x PIT Access");
            _cues = _cat.CreateEntry("Cues", true, "Audio cues on");
            _cueVolume = _cat.CreateEntry("CueVolume", 0.8f, "Cue volume 0..1");
            _dangerRadar = _cat.CreateEntry("DangerRadar", true, "Enemy danger radar");
            _ballCues = _cat.CreateEntry("BallCues", true, "Returning ball cues");
            _wallCues = _cat.CreateEntry("WallCues", true, "Arena edge cues");
            _hazardCues = _cat.CreateEntry("HazardCues", true, "Enemy shots, lasers, bombs and floor hazards");
            _pickupCues = _cat.CreateEntry("PickupCues", 2, "Pickup cues: 0 off, 1 important only, 2 all");
            _healthCues = _cat.CreateEntry("HealthCues", true, "Hurt, heal, heartbeat and status effect cues");
            _aimCues = _cat.CreateEntry("AimCues", true, "Aim assist target change click");
            _autoRead = _cat.CreateEntry("ReadDescriptions", true, "Read the description of upgrade choices on focus");
            _aimMode = _cat.CreateEntry("AimMode", (int)Game.AimAssist.Mode.Lowest, "Aim assist mode");
            _catchMode = _cat.CreateEntry("CatchMode", (int)Game.CatchAssist.Mode.Hold, "Catch assist: 0 off, 1 hold Shift, 2 automatic");
            _backend = _cat.CreateEntry("SpeechBackend", "Auto", "Prism backend name or Auto");
            _debug = _cat.CreateEntry("Debug", false, "Debug logging");
        }

        internal static bool Cues { get => _cues?.Value ?? true; set { _cues.Value = value; Save(); } }
        internal static float CueVolume { get => _cueVolume?.Value ?? 0.8f; set { _cueVolume.Value = value; Save(); } }
        internal static bool DangerRadar { get => _dangerRadar?.Value ?? true; set { _dangerRadar.Value = value; Save(); } }
        internal static bool BallCues { get => _ballCues?.Value ?? true; set { _ballCues.Value = value; Save(); } }
        internal static bool WallCues { get => _wallCues?.Value ?? true; set { _wallCues.Value = value; Save(); } }
        internal static bool HazardCues { get => _hazardCues?.Value ?? true; set { _hazardCues.Value = value; Save(); } }
        internal static int PickupCues { get => _pickupCues?.Value ?? 2; set { _pickupCues.Value = value; Save(); } }
        internal static bool HealthCues { get => _healthCues?.Value ?? true; set { _healthCues.Value = value; Save(); } }
        internal static bool AimCues { get => _aimCues?.Value ?? true; set { _aimCues.Value = value; Save(); } }
        internal static bool ReadDescriptions { get => _autoRead?.Value ?? true; set { _autoRead.Value = value; Save(); } }
        internal static int AimMode { get => _aimMode?.Value ?? 1; set { _aimMode.Value = value; Save(); } }
        internal static int CatchMode { get => _catchMode?.Value ?? 1; set { _catchMode.Value = value; Save(); } }
        internal static string SpeechBackend => _backend?.Value ?? "Auto";
        internal static bool Debug { get => _debug?.Value ?? false; set { _debug.Value = value; Save(); } }

        private static void Save()
        {
            try { _cat.SaveToFile(false); } catch { }
        }
    }
}

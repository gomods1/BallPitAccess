using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using BallPitAccess.Game;

namespace BallPitAccess.Input
{
    /// <summary>
    /// Number keys in battle (the game binds none of 1..0): mute or unmute one group of cues so the player
    /// can concentrate on what matters right now. Ctrl+number keeps only that group; 0 brings all back.
    /// The groups are the same settings as in F4, so a choice made here is kept for the next session; the
    /// muted groups are said when a battle starts, so a forgotten mute is not mistaken for silence.
    /// Muting changes only the sounds: the catch assist still dodges shots, H J P N still answer.
    /// </summary>
    internal static class CueFilters
    {
        private sealed class Group
        {
            public char Key;
            public string Name;            // Loc key
            public Func<bool> Get;
            public Action<bool> Set;
        }

        private static int _pickupMode = 2;   // the pickup mode to bring back when rewards are unmuted

        private static readonly List<Group> Groups = new List<Group>
        {
            new Group { Key = '1', Name = "filter_enemies", Get = () => Config.DangerRadar, Set = v => Config.DangerRadar = v },
            new Group { Key = '2', Name = "filter_balls", Get = () => Config.BallCues, Set = v => Config.BallCues = v },
            new Group
            {
                Key = '3', Name = "filter_rewards", Get = () => Config.PickupCues > 0,
                Set = v =>
                {
                    if (!v && Config.PickupCues > 0) _pickupMode = Config.PickupCues;
                    Config.PickupCues = v ? Math.Max(1, _pickupMode) : 0;
                },
            },
            new Group { Key = '4', Name = "filter_hazards", Get = () => Config.HazardCues, Set = v => Config.HazardCues = v },
            new Group { Key = '5', Name = "filter_edges", Get = () => Config.WallCues, Set = v => Config.WallCues = v },
            new Group { Key = '6', Name = "filter_health", Get = () => Config.HealthCues, Set = v => Config.HealthCues = v },
            new Group { Key = '7', Name = "filter_aim", Get = () => Config.AimCues, Set = v => Config.AimCues = v },
        };

        /// <summary>Digits: toggle a group (Ctrl: keep only it); 0 turns every group and the master switch on.</summary>
        internal static bool Update(bool ctrl)
        {
            for (char c = '0'; c <= '9'; c++)
            {
                if (!Keys.Pressed(c)) continue;
                Keys.Consume();
                if (c == '0') { AllOn(); return true; }   // 0 (with or without Ctrl): everything back on
                var g = Groups.Find(x => x.Key == c);
                if (g == null) { Speech.SayMenu(Loc.Get("filter_none")); return true; }
                if (ctrl) Solo(g); else Toggle(g);
                return true;
            }
            return false;
        }

        private static void Toggle(Group g)
        {
            bool on = !g.Get();
            g.Set(on);
            if (on && !Config.Cues) Config.Cues = true;   // unmuting a group with every sound off would do nothing
            Speech.SayMenu(Loc.F(on ? "filter_on" : "filter_off", Loc.Get(g.Name)));
        }

        private static void Solo(Group only)
        {
            foreach (var g in Groups) g.Set(g == only);
            Config.Cues = true;
            Speech.SayMenu(Loc.F("filter_solo", Loc.Get(only.Name)));
        }

        private static void AllOn()
        {
            foreach (var g in Groups) g.Set(true);
            Config.Cues = true;
            Speech.SayMenu(Loc.Get("filter_all_on"));
        }

        /// <summary>"Muted: balls, edges", or null when everything is on.</summary>
        internal static string MutedSummary()
        {
            if (!Config.Cues) return Loc.Get("filter_master_off");
            var off = new List<string>();
            foreach (var g in Groups) if (!g.Get()) off.Add(Loc.Get(g.Name));
            return off.Count == 0 ? null : Loc.F("filter_muted", string.Join(", ", off));
        }
    }
}

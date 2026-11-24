using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using MelonLoader;

namespace BallPitAccess.Input
{
    /// <summary>
    /// The mod's own hotkeys, remappable from F4 → Keys (a player asked: their Mexican Spanish keyboard has
    /// no plain = key). Stored in UserData/MelonPreferences.cfg under [BallPitAccess_Keys] as Windows
    /// virtual-key codes. Navigation keys (arrows, Enter, Tab, Escape, Backspace, Home, End, Page Up / Down)
    /// and the game's own keys stay fixed and cannot be taken.
    /// </summary>
    internal static class Bindings
    {
        internal sealed class Binding
        {
            public string Id;
            public int Default;
            public string Arg;                 // filter group name for "filterN"
            public MelonPreferences_Entry<int> Entry;
            public int Vk => Entry != null && Entry.Value > 0 && Entry.Value < 256 ? Entry.Value : Default;
            public string Name => Arg != null ? Loc.F("key_filter", Loc.Get(Arg)) : Loc.Get("key_" + Id);
            public void Set(int vk) { if (Entry != null) Entry.Value = vk; }
        }

        internal static readonly List<Binding> All = new List<Binding>();
        private static readonly Dictionary<string, Binding> ById = new Dictionary<string, Binding>();
        private static MelonPreferences_Category _cat;

        private static void Add(string id, int vk, string arg = null)
        {
            var b = new Binding { Id = id, Default = vk, Arg = arg };
            All.Add(b);
            ById[id] = b;
        }

        internal static void Init()
        {
            const int F1 = Keys.VK_F1;
            Add("help", F1);
            Add("screen", F1 + 1);
            Add("repeat", F1 + 2);
            Add("settings", F1 + 3);
            Add("aim_mode", F1 + 4);
            Add("aim_left", F1 + 5);
            Add("aim_right", F1 + 6);
            Add("aim_say", F1 + 7);
            Add("health", 'H');
            Add("status", 'U');
            Add("enemies", 'J');
            Add("position", 'K');
            Add("speed", 'G');
            Add("progress", 'L');
            Add("equipment", 'B');
            Add("pickups", 'P');
            Add("hazards", 'N');
            Add("last_sound", 'Q');
            Add("paths", 'I');
            Add("map", 'M');
            Add("speed_up", VK_ADD);
            Add("speed_down", VK_SUBTRACT);
            Add("catch", Keys.VK_SHIFT);
            string[] groups = { "filter_enemies", "filter_balls", "filter_rewards", "filter_hazards", "filter_edges", "filter_health", "filter_aim" };
            for (int i = 0; i < groups.Length; i++) Add("filter" + (i + 1), '1' + i, groups[i]);
            Add("filter_all", '0');

            _cat = MelonPreferences.CreateCategory("BallPitAccess_Keys", "BALL x PIT Access keys");
            foreach (var b in All)
                b.Entry = _cat.CreateEntry(b.Id, b.Default, null, "Windows virtual-key code; change it in game with F4, Keys");
            // A hand-edited file could put one key on two actions: the later one goes back to its default.
            var used = new HashSet<int>();
            foreach (var b in All)
                if (!used.Add(b.Vk)) { b.Set(b.Default); used.Add(b.Default); }
        }

        internal const int VK_ADD = 0x6B, VK_SUBTRACT = 0x6D;

        internal static Binding Get(string id) => ById.TryGetValue(id, out var b) ? b : null;

        internal static int Vk(string id) => ById.TryGetValue(id, out var b) ? b.Vk : 0;

        /// <summary>The action's key went down this frame.</summary>
        internal static bool Pressed(string id) { int vk = Vk(id); return vk > 0 && Keys.Pressed(vk); }

        /// <summary>The action's key is held now.</summary>
        internal static bool Down(string id) => Keys.Down(Vk(id));

        internal static string KeyOf(string id) => Keys.Name(Vk(id));

        /// <summary>Keys that can never be given to a mod action: menus need them, or the game uses them in a
        /// battle (W A S D move, Space shoots, F autofire, E level up, R rotate, X), or a screen reader does.</summary>
        internal static bool Reserved(int vk)
        {
            switch (vk)
            {
                case Keys.VK_BACK: case Keys.VK_TAB: case Keys.VK_RETURN: case Keys.VK_ESCAPE: case Keys.VK_SPACE:
                case Keys.VK_PRIOR: case Keys.VK_NEXT: case Keys.VK_END: case Keys.VK_HOME:
                case Keys.VK_LEFT: case Keys.VK_UP: case Keys.VK_RIGHT: case Keys.VK_DOWN:
                case Keys.VK_INSERT: case Keys.VK_DELETE: case Keys.VK_CAPITAL: case Keys.VK_LWIN: case Keys.VK_RWIN:
                case Keys.VK_CONTROL: case Keys.VK_MENU: case 0x90 /* Num Lock */: case 0x5D /* menu key */:
                case 'W': case 'A': case 'S': case 'D': case 'F': case 'E': case 'R': case 'X':
                    return true;
            }
            return false;
        }

        /// <summary>Give an action a new key. Another action that had that key takes the old one in exchange;
        /// returns it so the player hears where it went.</summary>
        internal static Binding Assign(Binding b, int vk)
        {
            Binding other = null;
            foreach (var o in All) if (o != b && o.Vk == vk) { other = o; break; }
            int old = b.Vk;
            b.Set(vk);
            other?.Set(old);
            Save();
            return other;
        }

        internal static Binding ResetOne(Binding b)
        {
            return b.Vk == b.Default ? null : Assign(b, b.Default);
        }

        internal static void ResetAll()
        {
            foreach (var b in All) b.Set(b.Default);
            Save();
        }

        private static void Save()
        {
            try { _cat.SaveToFile(false); } catch { }
        }
    }
}

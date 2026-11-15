using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using BallPitAccess.Game;
using BallPitAccess.UI;
using Il2Cpp;
using Il2CppRewired;

namespace BallPitAccess.Input
{
    /// <summary>
    /// The mod's keys, in priority order: an open mod list (help, settings) owns the keyboard, then Ctrl
    /// combos (review buffers), then plain hotkeys. While a mod list is open the game's keyboard maps are
    /// disabled so arrows, Enter and Escape do not also act in the game.
    /// Letter keys are HUD keys (rule 17): standing state a sighted player reads off the screen. They only
    /// exist where the game itself does not use the letter (battle: W A S D F E R are the game's).
    /// </summary>
    internal static class ModKeys
    {
        private sealed class ModList
        {
            public string Title;
            public Func<List<string>> Items;
            public Action<int, int> Change;   // index, direction (0 = Enter)
            public Action<int> Focused;       // an item got the cursor (the sound guide plays its cue)
            public string Closed = "help_closed";
            public int Index;
            public List<int> Sections;        // first line of each section: Tab / Shift+Tab jump between them
        }

        private static ModList _open;
        private static readonly List<ControllerMap> Muted = new List<ControllerMap>();

        private static bool _unmute;

        internal static bool ListOpen => _open != null;

        internal static void Update()
        {
            if (_unmute && _open == null && !Keys.Down(Keys.VK_ESCAPE) && !Keys.Down(Keys.VK_RETURN) && !Keys.Down(Keys.VK_F1) && !Keys.Down(Keys.VK_F1 + 3))
            {
                _unmute = false;
                MuteGame(false);
            }
            if (!Keys.GameHasFocus() || Keys.ScreenReaderModifier) return;
            if (_open != null) { UpdateList(); return; }

            if (Keys.Ctrl)
            {
                if (Keys.Pressed(Keys.VK_UP)) Buffers.Deeper();
                else if (Keys.Pressed(Keys.VK_DOWN)) Buffers.TowardTop();
                else if (Keys.Pressed(Keys.VK_LEFT)) Buffers.Switch(-1);
                else if (Keys.Pressed(Keys.VK_RIGHT)) Buffers.Switch(1);
                else if (Keys.Shift && Keys.Pressed('D')) { Battle.DumpThreats(); Dump.Bindings(); Navigator.LogList("dump"); Speech.SayMenu(Loc.Get("dumped")); }
                else if (Battle.InBattle) CueFilters.Update(true);
                return;
            }
            if (Keys.Alt) return;

            if (Keys.Pressed(Keys.VK_F1)) OpenHelp();
            else if (Keys.Pressed(Keys.VK_F1 + 1)) MenuReader.ReadScreen();
            else if (Keys.Pressed(Keys.VK_F1 + 2)) MenuReader.Repeat();
            else if (Keys.Pressed(Keys.VK_F1 + 3)) OpenSettings();
            else if (Keys.Pressed(Keys.VK_F1 + 4)) AimAssist.CycleMode();
            else if (Keys.Pressed(Keys.VK_F1 + 5)) AimAssist.Turn(10f);
            else if (Keys.Pressed(Keys.VK_F1 + 6)) AimAssist.Turn(-10f);
            else if (Keys.Pressed(Keys.VK_F1 + 7)) AimAssist.SayAim();
            else if (Battle.InBattle)
            {
                // Typing a pet's name or a menu on top: letters are not HUD keys there.
                if (Battle.Playing && !Navigator.HasOverlay && !Navigator.FtueShowing)
                {
                    // The arrows do nothing in the game during play: they fine-tune the aim.
                    if (Keys.Pressed(Keys.VK_LEFT)) { AimAssist.Turn(5f); return; }
                    if (Keys.Pressed(Keys.VK_RIGHT)) { AimAssist.Turn(-5f); return; }
                    if (Keys.Pressed(Keys.VK_UP)) { AimAssist.BackToAuto(); return; }
                    if (Keys.Pressed(Keys.VK_DOWN)) { AimAssist.SayAim(); return; }
                }
                // Number keys: sound filters (the game binds none of them). Also in the pause and level up menus.
                if (CueFilters.Update(false)) return;
                if (Navigator.HasOverlay && !Battle.Playing) return;
                if (Keys.Pressed('H')) Battle.SayHealth();
                else if (Keys.Pressed('J')) Battle.SayEnemies();
                else if (Keys.Pressed('K')) Battle.SayPosition();
                else if (Keys.Pressed('G')) Battle.SaySpeed();
                else if (Keys.Pressed('L')) Battle.SayProgress();
                else if (Keys.Pressed('B')) Battle.SayEquipment();
                else if (Keys.Pressed('P')) Pickups.Say();
                else if (Keys.Pressed('N')) Hazards.Say();
            }
            else if (BaseNav.InBase && !Navigator.Typing)
            {
                // Resources everywhere in the base: its menus (build, upgrades, market), build and harvest modes.
                if (Keys.Pressed('H')) BaseNav.SayResources();
                else if (Keys.Pressed('J') && !Navigator.HasOverlay) BaseNav.SayHarvest();
            }
        }

        // ---------------- Mod lists ----------------

        private static void Open(ModList l, string intro)
        {
            bool nested = _open != null || _unmute;     // opened from another list, or before the last one's keys were restored
            _unmute = false;
            _open = l;
            if (!nested) MuteGame(true);
            var items = l.Items();
            Speech.SayMenu(intro + " " + (items.Count > 0 ? items[Math.Min(l.Index, items.Count - 1)] : ""));
            if (items.Count > 0) l.Focused?.Invoke(Math.Min(l.Index, items.Count - 1));
        }

        private static void Close(string msg)
        {
            _open = null;
            _unmute = true;         // once the closing key is released (the game would see it as a fresh press)
            Speech.SayMenu(msg);
        }

        private static void UpdateList()
        {
            var l = _open;
            var items = l.Items();
            if (Keys.Pressed(Keys.VK_ESCAPE) || Keys.Pressed(Keys.VK_F1) || Keys.Pressed(Keys.VK_F1 + 3))
            {
                Close(Loc.Get(l.Closed));
                return;
            }
            if (items.Count == 0) return;
            int i = l.Index;
            if (Keys.Pressed(Keys.VK_DOWN)) i = Math.Min(items.Count - 1, i + 1);
            else if (Keys.Pressed(Keys.VK_UP)) i = Math.Max(0, i - 1);
            else if (Keys.Pressed(Keys.VK_HOME)) i = 0;
            else if (Keys.Pressed(Keys.VK_END)) i = items.Count - 1;
            else if (l.Sections != null && Keys.Pressed(Keys.VK_TAB))
            {
                int j = -1;
                if (Keys.Shift) { foreach (int s in l.Sections) if (s < i) j = s; }
                else foreach (int s in l.Sections) if (s > i) { j = s; break; }
                if (j < 0) { Audio.Cues.Play("wall", 0f, 0.5f); return; }
                i = j;
            }
            else if (l.Change != null && (Keys.Pressed(Keys.VK_LEFT) || Keys.Pressed(Keys.VK_RIGHT) || Keys.Pressed(Keys.VK_RETURN)))
            {
                int dir = Keys.Pressed(Keys.VK_LEFT) ? -1 : Keys.Pressed(Keys.VK_RIGHT) ? 1 : 0;
                l.Change(i, dir);
                if (_open == l) Speech.SayMenu(l.Items()[i]);
                return;
            }
            else return;
            l.Index = i;
            Speech.SayMenu(items[i]);
            l.Focused?.Invoke(i);
        }

        // Every cue with what it means (template: audio glossary). The cue plays when its line gets the cursor.
        private static readonly string[] Glossary =
        {
            "enemy1", "enemy9", "boss5", "danger", "charge", "boss", "loop_ball:far", "loop_ball:near", "ball_inline", "catch", "ball_miss", "ready",
            "treasure1", "pickup2", "pickup0", "pickup2:behind", "loop_shot", "shot_hit", "beam", "beam_hit", "bomb", "bomb_hit", "floor",
            "hurt", "heal", "heart", "status", "xp", "levelup", "wall", "wall_v", "retarget", "win", "lose",
        };

        private static void OpenSounds()
        {
            var lines = new List<string>();
            foreach (var c in Glossary) lines.Add(Loc.Get("snd_" + c.Replace(':', '_')));
            Open(new ModList
            {
                Title = "sounds", Items = () => lines, Closed = "help_closed",
                Focused = i => Preview(Glossary[i], 0f),
                Change = (i, dir) => Preview(Glossary[i], dir < 0 ? -1f : dir > 0 ? 1f : 0f),
            }, Loc.Get("sounds"));
        }

        /// <summary>A glossary entry "name:behind" plays the cue muffled (something behind the hero),
        /// "name:far" an octave down (the ball clank when you are three columns or more off).</summary>
        private static void Preview(string entry, float pan)
        {
            int c = entry.IndexOf(':');
            string name = c < 0 ? entry : entry.Substring(0, c), how = c < 0 ? "" : entry.Substring(c + 1);
            if (name.StartsWith("loop_"))
            {
                // Continuous sounds: a moment of the loop, as it sounds far off / close.
                if (name == "loop_ball") Audio.Cues.PreviewLoop(name, 1.6f, pan, how == "far" ? 0.45f : 0.89f, how == "far" ? 3f : 10f);
                else Audio.Cues.PreviewLoop(name, 1.6f, pan, 0.9f, 0f);
                return;
            }
            if (c < 0) { Audio.Cues.Play(entry, pan, 1f); return; }
            if (how == "far") Audio.Cues.Play(name, pan, 1f, 0f, 0.47f);
            else Audio.Cues.Play(name, pan, 1f, 1f);
        }

        private static void MuteGame(bool mute)
        {
            try
            {
                if (mute)
                {
                    Muted.Clear();
                    var p = InputMgr.I?.GetRewiredPlayer();
                    if (p == null) return;
                    var list = new Il2CppSystem.Collections.Generic.List<ControllerMap>();
                    p.controllers.maps.GetAllMaps(ControllerType.Keyboard, list);
                    for (int i = 0; i < list.Count; i++)
                        if (list[i].enabled) { list[i].enabled = false; Muted.Add(list[i]); }
                }
                else
                {
                    foreach (var m in Muted) m.enabled = true;
                    Muted.Clear();
                }
            }
            catch (Exception ex) { Log.Warn("Could not mute game keys: " + ex.Message); }
        }

        /// <summary>
        /// Every part of the help, wherever F1 is pressed (people read it before a fight, not during one): the
        /// part for where you are first, then the others. Each part starts with its title line; Tab and
        /// Shift+Tab jump between parts.
        /// </summary>
        private static void OpenHelp()
        {
            string here = Battle.InBattle ? "help_battle" : BaseNav.InBase ? "help_base" : "help_menu";
            var order = new List<string> { here };
            foreach (var p in new[] { "help_battle", "help_sounds", "help_base", "help_menu", "help_common" })
                if (!order.Contains(p)) order.Add(p);
            var lines = new List<string>();
            var sections = new List<int>();
            foreach (var part in order)
            {
                sections.Add(lines.Count);
                foreach (var l in HelpLines(Loc.Get(part)))
                    lines.Add(l);
            }
            Open(new ModList { Title = "help", Items = () => lines, Sections = sections }, Loc.Get("help"));
        }

        /// <summary>A help text as lines: split at line breaks when it has them (Japanese, Chinese and Thai
        /// tables use them), else at ". " and at the full-width stop.</summary>
        internal static List<string> HelpLines(string text)
        {
            var seps = text.IndexOf('\n') >= 0 ? new[] { "\n" } : new[] { ". ", "。" };
            var o = new List<string>();
            foreach (var l in text.Split(seps, StringSplitOptions.RemoveEmptyEntries))
            {
                string t = l.Trim().TrimEnd('.', '。');
                if (t.Length > 0) o.Add(t);
            }
            return o;
        }

        private static void OpenSettings()
        {
            var l = new ModList
            {
                Title = "settings",
                Items = () => new List<string>
                {
                    Loc.F("set_aim", Loc.Get("aim_" + AimAssist.Current)),
                    Loc.F("set_catch", Loc.Get("catch_" + CatchAssist.Current)),
                    Loc.F("set_cues", OnOff(Config.Cues)),
                    Loc.F("set_volume", (int)Math.Round(Config.CueVolume * 100)),
                    Loc.F("set_radar", OnOff(Config.DangerRadar)),
                    Loc.F("set_balls", OnOff(Config.BallCues)),
                    Loc.F("set_walls", OnOff(Config.WallCues)),
                    Loc.F("set_hazards", OnOff(Config.HazardCues)),
                    Loc.F("set_pickups", Loc.Get("pickups_mode" + Math.Max(0, Math.Min(2, Config.PickupCues)))),
                    Loc.F("set_health", OnOff(Config.HealthCues)),
                    Loc.F("set_aimcues", OnOff(Config.AimCues)),
                    Loc.F("set_descriptions", OnOff(Config.ReadDescriptions)),
                    Loc.Get("set_sounds"),
                    Loc.F("set_debug", OnOff(Config.Debug)),
                },
                Closed = "settings_closed",
                Change = (i, dir) =>
                {
                    switch (i)
                    {
                        case 0:
                            int n = (int)AimAssist.Mode.Count;
                            Config.AimMode = ((int)AimAssist.Current + (dir < 0 ? n - 1 : 1)) % n;
                            break;
                        case 1: CatchAssist.CycleMode(dir); break;
                        case 2: Config.Cues = !Config.Cues; break;
                        case 3:
                            float v = Config.CueVolume + (dir < 0 ? -0.1f : 0.1f);
                            if (dir == 0) v = Config.CueVolume >= 0.95f ? 0.1f : Config.CueVolume + 0.1f;
                            Config.CueVolume = (float)Math.Round(Math.Max(0f, Math.Min(1f, v)), 1);
                            Audio.Cues.Play("tick");
                            break;
                        case 4: Config.DangerRadar = !Config.DangerRadar; break;
                        case 5: Config.BallCues = !Config.BallCues; break;
                        case 6: Config.WallCues = !Config.WallCues; break;
                        case 7: Config.HazardCues = !Config.HazardCues; break;
                        case 8: Config.PickupCues = (Math.Max(0, Math.Min(2, Config.PickupCues)) + (dir < 0 ? 2 : 1)) % 3; break;
                        case 9: Config.HealthCues = !Config.HealthCues; break;
                        case 10: Config.AimCues = !Config.AimCues; break;
                        case 11: Config.ReadDescriptions = !Config.ReadDescriptions; break;
                        case 12: if (dir == 0) OpenSounds(); break;
                        case 13: Config.Debug = !Config.Debug; break;
                    }
                },
            };
            Open(l, Loc.Get("settings"));
        }

        private static string OnOff(bool b) => Loc.Get(b ? "on" : "off");
    }
}

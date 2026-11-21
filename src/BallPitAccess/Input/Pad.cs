using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BallPitAccess.Core;
using BallPitAccess.Game;
using BallPitAccess.UI;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppRewired;
using UnityEngine;

namespace BallPitAccess.Input
{
    /// <summary>
    /// Controller support (a player: "lack of controller support is making it much worse to play").
    ///
    /// The pad is read through XInput (Xbox controllers, and any controller Steam Input presents as one), on its
    /// own, beside the game's Rewired. Its buttons become the mod's keys (Keys.Down / Keys.Pressed see them as
    /// held keys), so every handler works unchanged: D-pad = arrows, A = Enter, B = Backspace, and so on.
    ///
    /// Two modes:
    /// - Menus, the base, popups in a battle, mod lists: the mod owns the pad. The game's controller mappings are
    ///   switched off (ActionElementMap.enabled, Start excepted, so the game's own pause still works), or the
    ///   game would move its selection by direction and press buttons a second time. The game is also kept in
    ///   keyboard and mouse mode there (InputMgr.LastControlType), the mode every mod screen was built in.
    /// - A battle being played: the game owns the sticks, triggers and face buttons. The mod takes the D-pad,
    ///   the left stick click (catch assist, like Shift) and View held as a modifier for its info keys; only
    ///   those mappings of the game are off (all buttons while View is held, so a chord does not also act).
    /// </summary>
    internal static class Pad
    {
        // ---------------- XInput ----------------

        [StructLayout(LayoutKind.Sequential)]
        private struct XGamepad
        {
            public ushort Buttons;
            public byte LeftTrigger, RightTrigger;
            public short ThumbLX, ThumbLY, ThumbRX, ThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XState
        {
            public uint Packet;
            public XGamepad Pad;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GetStateFn(uint index, out XState state);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr LoadLibraryW(string name);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)] private static extern IntPtr GetProcAddress(IntPtr module, string name);

        private static GetStateFn _getState;
        private static bool _loadTried;

        private static bool Load()
        {
            if (_loadTried) return _getState != null;
            _loadTried = true;
            foreach (var dll in new[] { "xinput1_4.dll", "xinput1_3.dll", "xinput9_1_0.dll" })
            {
                try
                {
                    IntPtr m = LoadLibraryW(dll);
                    if (m == IntPtr.Zero) continue;
                    IntPtr f = GetProcAddress(m, "XInputGetState");
                    if (f == IntPtr.Zero) continue;
                    _getState = Marshal.GetDelegateForFunctionPointer<GetStateFn>(f);
                    Log.Info("Controller: XInput from " + dll);
                    return true;
                }
                catch (Exception ex) { Log.Warn("Controller: " + dll + ": " + ex.Message); }
            }
            Log.Warn("Controller: no XInput library, controller support off");
            return false;
        }

        // ---------------- Buttons ----------------

        [Flags]
        internal enum Btn : uint
        {
            None = 0, Up = 0x1, Down = 0x2, Left = 0x4, Right = 0x8, Start = 0x10, View = 0x20, L3 = 0x40, R3 = 0x80,
            LB = 0x100, RB = 0x200, A = 0x1000, B = 0x2000, X = 0x4000, Y = 0x8000, LT = 0x10000, RT = 0x20000,
        }

        private static readonly Btn[] Each =
        {
            Btn.Up, Btn.Down, Btn.Left, Btn.Right, Btn.Start, Btn.L3, Btn.R3, Btn.LB, Btn.RB,
            Btn.A, Btn.B, Btn.X, Btn.Y, Btn.LT, Btn.RT,
        };

        private static int _index = -1;
        private static float _nextScan;
        private static uint _cur, _prev, _stickDirs;
        private static Vector2 _right;
        private static float _rightMovedAt = -10f;

        /// <summary>A controller is connected and on (F4 setting).</summary>
        internal static bool Connected => _index >= 0 && Config.Pad;

        /// <summary>A battle is being played: the game owns the sticks and buttons.</summary>
        internal static bool BattleMode { get; private set; }

        /// <summary>Y while a building is being placed: turn it (the game's rotate is a mapping the mod switched off).</summary>
        internal static bool RotatePressed { get; private set; }

        /// <summary>
        /// The right stick has been left alone for a moment in a battle with the game in controller mode: the aim
        /// assist sets the aim itself (the game reads no mouse then, so its mouse postfix never runs).
        /// </summary>
        internal static bool PushAim
        {
            get
            {
                if (!Connected || !BattleMode || Time.unscaledTime - _rightMovedAt < 0.6f) return false;
                try { return InputMgr.I != null && InputMgr.I.LastControlType == InputType.kController; } catch { return false; }
            }
        }

        // ---------------- Virtual keys ----------------

        /// <summary>What a pressed button stands for, chosen when it went down (kept until it is released).</summary>
        private sealed class Hold
        {
            public int[] Vks;
            public bool Repeat;
            public float Since, NextRepeat;
            public bool Gap;          // released for one frame, so the next frame is a fresh press
        }

        private static readonly Dictionary<Btn, Hold> Holds = new Dictionary<Btn, Hold>();
        private static readonly HashSet<int> HeldVks = new HashSet<int>();

        /// <summary>The pad holds this virtual key now.</summary>
        internal static bool HoldsKey(int vk) => HeldVks.Contains(vk);

        // Dev: "pad a", "pad view+a", "pad up" (one frame each, like "key").
        private static readonly Queue<Btn> Injected = new Queue<Btn>();
        private static Btn _injectedNow;

        internal static bool Inject(string spec)
        {
            Btn b = Btn.None;
            foreach (var part in spec.ToLowerInvariant().Split('+'))
            {
                if (!Enum.TryParse(part, true, out Btn one)) return false;
                b |= one;
            }
            Injected.Enqueue(b);
            return true;
        }

        private static bool _wasConnected;
        private static bool _wasBattle;

        /// <summary>Once per frame, before Keys reads its keys.</summary>
        internal static void Update()
        {
            RotatePressed = false;
            HeldVks.Clear();
            if (!Load() || !Config.Pad) { Release(); return; }
            float now = Time.unscaledTime;

            uint raw = 0;
            if (_index < 0 && now >= _nextScan)
            {
                _nextScan = now + 2f;
                for (uint i = 0; i < 4; i++)
                    if (_getState(i, out _) == 0) { _index = (int)i; break; }
            }
            if (_index >= 0)
            {
                if (_getState((uint)_index, out XState s) != 0) { _index = -1; _nextScan = now + 2f; }
                else
                {
                    raw = s.Pad.Buttons;
                    if (s.Pad.LeftTrigger > 100) raw |= (uint)Btn.LT;
                    if (s.Pad.RightTrigger > 100) raw |= (uint)Btn.RT;
                    _right = new Vector2(s.Pad.ThumbRX / 32767f, s.Pad.ThumbRY / 32767f);
                    if (_right.magnitude > 0.3f) _rightMovedAt = now;
                    _stickDirs = StickDirs(s.Pad.ThumbLX / 32767f, s.Pad.ThumbLY / 32767f, _stickDirs);
                }
            }
            if (_index >= 0 != _wasConnected)
            {
                _wasConnected = _index >= 0;
                Log.Info("Controller " + (_wasConnected ? "connected, XInput pad " + _index : "disconnected"));
                if (UnityEngine.Time.frameCount > 10) Speech.Event(Loc.Get(_wasConnected ? "pad_on" : "pad_off"));
            }

            _injectedNow = _injectedNow == Btn.None && Injected.Count > 0 ? Injected.Dequeue() : Btn.None;
            raw |= (uint)_injectedNow;
            if (!Keys.GameHasFocus()) raw = 0;

            bool battle = false;
            try { battle = Battle.Playing && !Navigator.HasOverlay && !Navigator.FtueShowing && !ModKeys.ListOpen; } catch { }
            if (battle != _wasBattle) { _wasBattle = battle; Log.Info("Controller mode: " + (battle ? "battle (the game has the pad)" : "menus (the mod has the pad)")); }
            BattleMode = battle;
            // Outside a battle the left stick moves like the D-pad.
            if (!battle) raw |= _stickDirs;

            _prev = _cur;
            _cur = raw;
            bool view = (_cur & (uint)Btn.View) != 0;

            foreach (var b in Each)
            {
                bool down = (_cur & (uint)b) != 0, was = (_prev & (uint)b) != 0;
                if (down && !was)
                {
                    Holds.Remove(b);
                    var h = Choose(b, view, battle);
                    if (h != null) { h.Since = now; h.NextRepeat = now + 0.45f; Holds[b] = h; }
                }
                else if (!down) Holds.Remove(b);
            }

            foreach (var h in Holds.Values)
            {
                if (h.Gap) { h.Gap = false; continue; }
                if (h.Repeat && now >= h.NextRepeat) { h.NextRepeat = now + 0.11f; h.Gap = true; continue; }
                foreach (int vk in h.Vks) HeldVks.Add(vk);
            }

            UpdateGameMode(battle);
            Block(battle, view);
        }

        private static void Release()
        {
            Holds.Clear();
            _cur = _prev = 0;
            BattleMode = false;
            if (_blockKey != 0) Block(false, false, force: true);
        }

        /// <summary>The left stick as four directions, the strongest axis only, with a little hysteresis.</summary>
        private static uint StickDirs(float x, float y, uint was)
        {
            float ax = Math.Abs(x), ay = Math.Abs(y);
            float on = 0.6f, off = 0.4f;
            uint o = 0;
            if (ax >= ay)
            {
                if (x > on || x > off && (was & (uint)Btn.Right) != 0) o |= (uint)Btn.Right;
                if (x < -on || x < -off && (was & (uint)Btn.Left) != 0) o |= (uint)Btn.Left;
            }
            else
            {
                if (y > on || y > off && (was & (uint)Btn.Up) != 0) o |= (uint)Btn.Up;
                if (y < -on || y < -off && (was & (uint)Btn.Down) != 0) o |= (uint)Btn.Down;
            }
            return o;
        }

        private static Hold Keys1(params int[] vks) => new Hold { Vks = vks };
        private static Hold Bind(string id) { int vk = Bindings.Vk(id); return vk > 0 ? Keys1(vk) : null; }

        /// <summary>The keys a button stands for here (see the class summary and help_pad).</summary>
        private static Hold Choose(Btn b, bool view, bool battle)
        {
            if (view)
            {
                switch (b)
                {
                    case Btn.A: return Bind("help");
                    case Btn.B: return Bind("settings");
                    case Btn.X: return Bind("screen");
                    case Btn.Y: return Bind("repeat");
                    case Btn.Start: return Bind("aim_mode");
                    case Btn.LB: return Keys1(Keys.VK_CONTROL, Keys.VK_UP);
                    case Btn.RB: return Keys1(Keys.VK_CONTROL, Keys.VK_DOWN);
                    case Btn.L3: return Keys1(Keys.VK_CONTROL, Keys.VK_LEFT);
                    case Btn.R3: return Keys1(Keys.VK_CONTROL, Keys.VK_RIGHT);
                }
                if (battle)
                    switch (b)
                    {
                        case Btn.Up: return Bind("hazards");
                        case Btn.Down: return Bind("equipment");
                        case Btn.Left: return Bind("pickups");
                        case Btn.Right: return Bind("last_sound");
                        case Btn.LT: return Bind("speed_down");
                        case Btn.RT: return Bind("speed_up");
                    }
                else
                    switch (b)
                    {
                        case Btn.Up: return Bind("enemies");
                        case Btn.Down: return Bind("health");
                        case Btn.Left: return Bind("position");
                        case Btn.LT: return Keys1(Keys.VK_PRIOR);
                        case Btn.RT: return Keys1(Keys.VK_NEXT);
                    }
                return null;
            }
            if (battle)
            {
                switch (b)
                {
                    case Btn.Up: return Bind("enemies");
                    case Btn.Down: return Bind("health");
                    case Btn.Left: return Bind("status");
                    case Btn.Right: return Bind("progress");
                    case Btn.L3: return Bind("catch");
                }
                return null;
            }
            switch (b)
            {
                case Btn.Up: return new Hold { Vks = new[] { Keys.VK_UP }, Repeat = true };
                case Btn.Down: return new Hold { Vks = new[] { Keys.VK_DOWN }, Repeat = true };
                case Btn.Left: return new Hold { Vks = new[] { Keys.VK_LEFT }, Repeat = true };
                case Btn.Right: return new Hold { Vks = new[] { Keys.VK_RIGHT }, Repeat = true };
                case Btn.A: return Keys1(Keys.VK_RETURN);
                // In a mod list Escape closes it; Backspace there would reach the game (it pauses the base).
                case Btn.B: return Keys1(ModKeys.ListOpen ? Keys.VK_ESCAPE : Keys.VK_BACK);
                case Btn.X: return Keys1(Keys.VK_HOME);
                case Btn.Y:
                    if (Placement.Active) { RotatePressed = true; return null; }
                    return Keys1(Keys.VK_END);
                case Btn.LB: return Keys1(Keys.VK_SHIFT, Keys.VK_TAB);
                case Btn.RB: return Keys1(Keys.VK_TAB);
                case Btn.LT: return Keys1(Keys.VK_CONTROL, Keys.VK_SHIFT, Keys.VK_TAB);
                case Btn.RT: return Keys1(Keys.VK_CONTROL, Keys.VK_TAB);
            }
            return null;
        }

        // ---------------- The game's controller mode ----------------

        internal static void TryPatch(HarmonyLib.Harmony h)
        {
            try
            {
                var m = HarmonyLib.AccessTools.Method(typeof(InputMgr), "OnLastActiveControllerChanged");
                if (m == null) { Log.Warn("InputMgr.OnLastActiveControllerChanged not found"); return; }
                h.Patch(m, prefix: new HarmonyLib.HarmonyMethod(typeof(Pad), nameof(ControllerChangedPrefix)));
                Log.Info("Patched InputMgr.OnLastActiveControllerChanged");
            }
            catch (Exception ex) { Log.Error("Controller patch failed: " + ex); }
        }

        private static bool _calling;

        /// <summary>Outside a battle the game stays in keyboard and mouse mode while the mod has the pad.</summary>
        private static bool ControllerChangedPrefix(Controller controller)
        {
            if (_calling || !Connected || BattleMode) return true;
            try { return controller == null || controller.type != ControllerType.Joystick; } catch { return true; }
        }

        private static void UpdateGameMode(bool battle)
        {
            if (!Connected) return;
            var im = InputMgr.I;
            if (im == null) return;
            try
            {
                if (!battle && im.LastControlType == InputType.kController)
                {
                    im.LastControlType = InputType.kMouseKeyboard;
                    im.OnInputTypeChanged?.Invoke();
                    Log.Info("Controller: the game back in keyboard and mouse mode for the menus");
                }
                else if (battle && im.LastControlType != InputType.kController && Time.unscaledTime - _rightMovedAt < 0.2f)
                {
                    // The game heard of the pad while it was ours: tell it now that the battle has it.
                    var p = im.GetRewiredPlayer();
                    var last = p?.controllers.GetLastActiveController();
                    if (last != null && last.type == ControllerType.Joystick)
                    {
                        _calling = true;
                        try { im.OnLastActiveControllerChanged(p, last); } finally { _calling = false; }
                        Log.Info("Controller: the game in controller mode for the battle: " + im.LastControlType);
                    }
                }
            }
            catch (Exception ex) { if (++_modeErrors <= 3) Log.Warn("Controller mode: " + ex.Message); }
        }

        private static int _modeErrors;

        // ---------------- Switching the game's mappings off ----------------

        private static readonly List<ActionElementMap> Off = new List<ActionElementMap>();
        private static int _blockKey;
        private static float _nextBlock;
        private static int _blockErrors;

        private static readonly ControllerBtn[] BattleBtns =
            { ControllerBtn.kDPadUp, ControllerBtn.kDPadRight, ControllerBtn.kDPadDown, ControllerBtn.kDPadLeft, ControllerBtn.kSelect, ControllerBtn.kL3 };

        private static readonly ControllerBtn[] ChordBtns =
        {
            ControllerBtn.kA, ControllerBtn.kB, ControllerBtn.kX, ControllerBtn.kY, ControllerBtn.kLeftBumper, ControllerBtn.kRightBumper,
            ControllerBtn.kLeftTrigger, ControllerBtn.kRightTrigger, ControllerBtn.kStart, ControllerBtn.kR3,
        };

        /// <summary>
        /// 0 nothing off, 1 battle (D-pad, View, left stick click), 2 battle with View held (every button),
        /// 3 menus (everything but Start). Applied when it changes and every 2 seconds (the game rebuilds its maps
        /// when the player remaps or a controller connects).
        /// </summary>
        private static void Block(bool battle, bool view, bool force = false)
        {
            int key = force || !Connected ? 0 : battle ? (view ? 2 : 1) : 3;
            float now = Time.unscaledTime;
            if (key == _blockKey && now < _nextBlock) return;
            _nextBlock = now + 2f;
            bool changed = key != _blockKey;
            _blockKey = key;
            try
            {
                foreach (var a in Off) { try { a.enabled = true; } catch { } }
                Off.Clear();
                if (key == 0) return;
                var p = InputMgr.I?.GetRewiredPlayer();
                if (p == null) return;
                var maps = new Il2CppSystem.Collections.Generic.List<ControllerMap>();
                p.controllers.maps.GetAllMaps(ControllerType.Joystick, maps);
                for (int i = 0; i < maps.Count; i++)
                {
                    var aems = maps[i].GetElementMaps();
                    if (aems == null) continue;
                    var ids = new HashSet<int>();
                    if (key == 3) Elements(aems, ids, ControllerBtn.kStart);
                    else
                    {
                        Elements(aems, ids, BattleBtns);
                        if (key == 2) Elements(aems, ids, ChordBtns);
                    }
                    if (key == 3)
                    {
                        // Keep the pause action and the Start button; everything else is the mod's.
                        int pause = ActionId(GameActionType.kPause);
                        foreach (var a in aems)
                            if (a != null && a.actionId == pause) ids.Add(a.elementIdentifierId);
                    }
                    foreach (var a in aems)
                    {
                        if (a == null || !a.enabled) continue;
                        bool off = key == 3 ? !ids.Contains(a.elementIdentifierId) : ids.Contains(a.elementIdentifierId);
                        if (!off) continue;
                        a.enabled = false;
                        Off.Add(a);
                    }
                }
                if (changed || Config.Debug) Log.Info($"Controller: {Off.Count} game mappings off (mode {key})");
            }
            catch (Exception ex) { if (++_blockErrors <= 3) Log.Warn("Controller block: " + ex.Message); }
        }

        private static int ActionId(GameActionType t)
        {
            var ids = InputMgr.kGameActionIds;
            return ids != null && (int)t < ids.Length ? ids[(int)t] : -1;
        }

        /// <summary>The element ids (physical buttons) the game's own button actions sit on in this map.</summary>
        private static void Elements(Il2CppReferenceArray<ActionElementMap> aems, HashSet<int> ids, params ControllerBtn[] btns)
        {
            var btnIds = InputMgr.kControllerBtnIds;
            if (btnIds == null) return;
            foreach (var b in btns)
            {
                if ((int)b >= btnIds.Length) continue;
                int act = btnIds[(int)b];
                foreach (var a in aems)
                    if (a != null && a.actionId == act) ids.Add(a.elementIdentifierId);
            }
        }

        // ---------------- Dev ----------------

        /// <summary>The game's controller mappings and the button each action sits on, for the bindings dump.</summary>
        internal static string Describe()
        {
            var sb = new System.Text.StringBuilder();
            var im = InputMgr.I;
            if (im == null) return "";
            sb.AppendLine($"Controller: XInput pad {_index}, game mode {im.LastControlType}, joypad {im.LastJoypadType}");
            for (int a = 0; a < (int)GameActionType.kNum; a++)
            {
                var at = (GameActionType)a;
                string pos, neg;
                try { pos = im.GetControllerBtnForAction(at, Pole.Positive).ToString(); } catch (Exception ex) { pos = "?" + ex.Message; }
                try { neg = im.GetControllerBtnForAction(at, Pole.Negative).ToString(); } catch { neg = "?"; }
                sb.AppendLine($"pad {at}: +{pos} -{neg}");
            }
            try
            {
                var p = im.GetRewiredPlayer();
                var maps = new Il2CppSystem.Collections.Generic.List<ControllerMap>();
                p.controllers.maps.GetAllMaps(ControllerType.Joystick, maps);
                for (int i = 0; i < maps.Count; i++)
                {
                    sb.AppendLine($"joystick map {i}: category {maps[i].categoryId} enabled {maps[i].enabled}");
                    foreach (var a in maps[i].GetElementMaps())
                        if (a != null) sb.AppendLine($"  {a.actionDescriptiveName} (action {a.actionId}) <- {a.elementIdentifierName} (element {a.elementIdentifierId}, {a.axisContribution}) {(a.enabled ? "" : "off")}");
                }
            }
            catch (Exception ex) { sb.AppendLine("maps: " + ex.Message); }
            return sb.ToString();
        }
    }
}

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BallPitAccess.Input
{
    /// <summary>
    /// Raw keyboard state through GetAsyncKeyState. It is read at the moment it is needed (no frame of lag)
    /// and works even when the game's own keyboard device is disabled. Only reports keys while the game
    /// window is in the foreground.
    /// </summary>
    internal static class Keys
    {
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vk);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
        [DllImport("user32.dll")] private static extern uint MapVirtualKeyW(uint code, uint mapType);

        private static readonly uint OwnPid = (uint)Process.GetCurrentProcess().Id;
        private static readonly bool[] Prev = new bool[256];
        private static readonly bool[] Cur = new bool[256];
        private static IntPtr _lastFg;
        private static bool _lastFgOwn;

        internal const int VK_BACK = 0x08, VK_TAB = 0x09, VK_RETURN = 0x0D, VK_SHIFT = 0x10, VK_CONTROL = 0x11, VK_MENU = 0x12,
            VK_CAPITAL = 0x14, VK_ESCAPE = 0x1B, VK_SPACE = 0x20, VK_PRIOR = 0x21, VK_NEXT = 0x22, VK_END = 0x23, VK_HOME = 0x24,
            VK_LEFT = 0x25, VK_UP = 0x26, VK_RIGHT = 0x27, VK_DOWN = 0x28, VK_INSERT = 0x2D, VK_DELETE = 0x2E,
            VK_NUMPAD2 = 0x62, VK_NUMPAD4 = 0x64, VK_NUMPAD5 = 0x65, VK_NUMPAD6 = 0x66, VK_NUMPAD8 = 0x68,
            VK_F1 = 0x70, VK_LSHIFT = 0xA0, VK_RSHIFT = 0xA1, VK_LCONTROL = 0xA2, VK_RCONTROL = 0xA3, VK_LWIN = 0x5B, VK_RWIN = 0x5C;

        /// <summary>Virtual key for a physical key position (scan code), so bindings follow key positions on any layout.</summary>
        internal static int FromScan(uint scan)
        {
            uint vk = MapVirtualKeyW(scan, 3 /* MAPVK_VSC_TO_VK_EX */);
            return (int)vk;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetKeyNameTextW(int lParam, System.Text.StringBuilder buf, int size);

        /// <summary>A key's name as Windows says it in its own language ("H", "F5", "Num +", "Mayús").</summary>
        internal static string Name(int vk)
        {
            if (vk >= '0' && vk <= '9' || vk >= 'A' && vk <= 'Z') return ((char)vk).ToString();
            try
            {
                uint sc = MapVirtualKeyW((uint)vk, 4 /* MAPVK_VK_TO_VSC_EX */);
                if (sc != 0)
                {
                    int lp = (int)((sc & 0xFF) << 16);
                    if ((sc & 0xFF00) == 0xE000) lp |= 1 << 24;
                    var sb = new System.Text.StringBuilder(64);
                    if (GetKeyNameTextW(lp, sb, sb.Capacity) > 0) return sb.ToString();
                }
            }
            catch { }
            return "#" + vk;
        }

        // ---------------- Dev: keys from dev.txt ("key tab", "key shift+tab") ----------------
        // For testing a game that runs with no window to type in (-batchmode, another session). Only reached
        // through DevCommands (Debug = true). Each queued key is down for one frame, with its modifiers.

        internal static bool DevFocus;
        private static readonly System.Collections.Generic.Queue<int[]> Injected = new System.Collections.Generic.Queue<int[]>();
        private static int[] _held;

        internal static void Inject(params int[] vks) => Injected.Enqueue(vks);

        private static bool HeldByDev(int vk)
        {
            if (_held == null) return false;
            foreach (int v in _held) if (v == vk) return true;
            return false;
        }

        internal static bool GameHasFocus()
        {
            if (DevFocus) return true;
            IntPtr fg = GetForegroundWindow();
            if (fg == _lastFg) return _lastFgOwn;
            _lastFg = fg;
            GetWindowThreadProcessId(fg, out uint pid);
            _lastFgOwn = pid == OwnPid;
            return _lastFgOwn;
        }

        /// <summary>Live state, read now.</summary>
        internal static bool Down(int vk) => vk > 0 && ((GetAsyncKeyState(vk) & 0x8000) != 0 || HeldByDev(vk) || Pad.HoldsKey(vk));

        internal static bool Ctrl => Down(VK_CONTROL);
        internal static bool Shift => Down(VK_SHIFT);
        internal static bool Alt => Down(VK_MENU);

        /// <summary>Screen reader modifier keys (NVDA/JAWS) or Windows key held: the keyboard belongs to them.</summary>
        internal static bool ScreenReaderModifier => Down(VK_INSERT) || Down(VK_CAPITAL) || Down(VK_LWIN) || Down(VK_RWIN);

        /// <summary>Once per frame, for edge detection of mod keys.</summary>
        internal static void Update()
        {
            Consumed = false;
            Pad.Update();
            bool focus = GameHasFocus();
            _held = _held == null && Injected.Count > 0 ? Injected.Dequeue() : null;
            for (int vk = 1; vk < 256; vk++)
            {
                Prev[vk] = Cur[vk];
                Cur[vk] = focus && ((GetAsyncKeyState(vk) & 0x8000) != 0 || HeldByDev(vk) || Pad.HoldsKey(vk));
            }
        }

        internal static bool Pressed(int vk) => Cur[vk] && !Prev[vk];

        /// <summary>A handler acted on this frame's key press; later handlers in the frame leave it alone.</summary>
        internal static bool Consumed { get; private set; }
        internal static void Consume() => Consumed = true;
    }
}

using System;
using System.Collections.Generic;

namespace BallPitAccess.Core
{
    /// <summary>
    /// Review buffers (template 11). Two for now: Details (what the focused thing says beyond its name)
    /// and Events (newest first). Ctrl+Up goes deeper, Ctrl+Down back toward the top, Ctrl+Left/Right switch.
    /// Focusing something with details makes Details current with the cursor before the top.
    /// </summary>
    internal static class Buffers
    {
        private sealed class Buffer
        {
            public string Name;
            public readonly List<string> Items = new List<string>();
            public int Cursor = -1;
        }

        private static readonly Buffer Details = new Buffer { Name = "Details" };
        private static readonly Buffer Events = new Buffer { Name = "Events" };
        private static readonly Buffer[] All = { Details, Events };
        private static int _current = 1;

        internal static void AddEvent(string text)
        {
            Events.Items.Insert(0, text);
            if (Events.Cursor >= 0) Events.Cursor++;
            if (Events.Items.Count > 200) Events.Items.RemoveAt(Events.Items.Count - 1);
            if (Events.Cursor >= Events.Items.Count) Events.Cursor = Events.Items.Count - 1;
        }

        private static System.Func<List<string>> _more;
        private static System.Func<List<string>> _fresh;

        /// <summary>
        /// Replace the Details buffer. <paramref name="more"/> is asked for extra lines the first time the
        /// player reads the buffer (tooltips the game fills in a moment after focus). <paramref name="fresh"/>,
        /// when given, reads the item again at that moment and replaces the lines: the game's details panels
        /// fill in a few hundred milliseconds after the focus, so lines taken at focus time could still be the
        /// previous item's (a player: "reading the tooltip from the item above").
        /// </summary>
        internal static void SetDetails(List<string> lines, System.Func<List<string>> more = null, System.Func<List<string>> fresh = null)
        {
            Details.Items.Clear();
            if (lines != null) foreach (var l in lines) if (!string.IsNullOrEmpty(l)) Details.Items.Add(l);
            Details.Cursor = -1;
            _more = more;
            _fresh = fresh;
            _current = Details.Items.Count > 0 || more != null || fresh != null ? 0 : 1;
        }

        private static void Refresh()
        {
            var f = _fresh;
            _fresh = null;
            if (f == null) return;
            try
            {
                var lines = f();
                if (lines == null || lines.Count == 0) return;
                int cursor = Details.Cursor;
                Details.Items.Clear();
                foreach (var l in lines) if (!string.IsNullOrEmpty(l) && !Details.Items.Contains(l)) Details.Items.Add(l);
                Details.Cursor = Math.Min(cursor, Details.Items.Count - 1);
            }
            catch { }
        }

        /// <summary>The first line was just spoken: the next Ctrl+Up reads the second.</summary>
        internal static void StartReading()
        {
            if (Details.Items.Count > 0) Details.Cursor = 0;
        }

        private static void Extend()
        {
            var f = _more;
            _more = null;
            if (f == null) return;
            try
            {
                var extra = f();
                if (extra != null) foreach (var l in extra) if (!string.IsNullOrEmpty(l) && !Details.Items.Contains(l)) Details.Items.Add(l);
            }
            catch { }
        }

        internal static void Deeper()
        {
            var b = All[_current];
            if (b == Details)
            {
                Refresh();
                Extend();
                // Nothing to detail: the same key reads the recent events instead.
                if (b.Items.Count == 0 && Events.Items.Count > 0) { _current = 1; b = Events; }
            }
            if (b.Items.Count == 0) { Speech.SayMenu(Loc.Get(b == Events ? "events_empty" : "details_empty")); return; }
            if (b.Cursor < b.Items.Count - 1) b.Cursor++;
            else { Speech.SayMenu(Loc.Get("end") + ". " + b.Items[b.Cursor]); return; }
            Speech.SayMenu(b.Items[b.Cursor]);
        }

        internal static void TowardTop()
        {
            var b = All[_current];
            if (b == Details) { Refresh(); Extend(); }
            if (b.Items.Count == 0) { Speech.SayMenu(Loc.Get(b == Events ? "events_empty" : "details_empty")); return; }
            if (b.Cursor > 0) b.Cursor--;
            else { b.Cursor = 0; Speech.SayMenu(Loc.Get("top") + ". " + b.Items[0]); return; }
            Speech.SayMenu(b.Items[b.Cursor]);
        }

        internal static void Switch(int dir)
        {
            for (int i = 0; i < All.Length; i++)
            {
                _current = (_current + dir + All.Length) % All.Length;
                if (All[_current].Items.Count > 0) break;
            }
            var b = All[_current];
            if (b == Events) b.Cursor = -1;
            Speech.SayMenu(Loc.Get(b == Events ? "buf_events" : "buf_details") + ", " + b.Items.Count);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BallPitAccess.Core
{
    /// <summary>
    /// The only entry point for speech. Everything is queued and sent from Unity's main thread
    /// (a Prism backend is not thread safe). Never interrupts by default.
    /// - <see cref="Say"/>: on-demand reads and focus echoes.
    /// - <see cref="Event"/>: unsolicited announcements; recorded in the Events buffer and events.log.
    /// - <see cref="Battle"/>: real-time messages with a channel (newer replaces unsent older) and a time to live.
    /// </summary>
    internal static class Speech
    {
        private static readonly PrismSpeech Prism = new PrismSpeech();
        private static readonly object Gate = new object();
        private static readonly List<Pending> Queue = new List<Pending>();

        private sealed class Pending
        {
            public string Text;
            public string Channel;
            public bool Interrupt;
            public bool Menu;
            public float Expires;
        }

        private static bool _lastWasMenu;
        internal static bool Ready => Prism.Ready;
        internal static string LastSpoken { get; private set; } = "";

        internal static void Init()
        {
            string gameDir = MelonLoader.Utils.MelonEnvironment.GameRootDirectory;
            string modDir = Path.GetDirectoryName(typeof(Speech).Assembly.Location) ?? gameDir;
            string where = File.Exists(Path.Combine(gameDir, "prism.dll")) ? gameDir : modDir;
            if (Prism.Initialize(where, Config.SpeechBackend))
                Log.Info("Speech ready: " + Prism.BackendName);
        }

        internal static void Say(string text, bool interrupt = false)
        {
            text = Clean(text);
            if (text.Length == 0) return;
            lock (Gate) Queue.Add(new Pending { Text = text, Interrupt = interrupt, Expires = float.MaxValue });
        }

        /// <summary>Menu focus speech: interrupts the previous focus echo (the screen reader does not see our key presses), and is dropped if the screen changes before it is sent.</summary>
        internal static void SayMenu(string text)
        {
            text = Clean(text);
            if (text.Length == 0) return;
            lock (Gate)
            {
                Queue.RemoveAll(p => p.Menu);
                Queue.Add(new Pending { Text = text, Menu = true, Interrupt = true, Expires = float.MaxValue });
            }
        }

        internal static void Event(string text)
        {
            text = Clean(text);
            if (text.Length == 0) return;
            Log.Event(text);
            Buffers.AddEvent(text);
            lock (Gate) Queue.Add(new Pending { Text = text, Expires = float.MaxValue });
        }

        /// <summary>Real-time battle speech: a stale message is dropped, never spoken late.</summary>
        internal static void Battle(string text, string channel, float ttl = 2f)
        {
            text = Clean(text);
            if (text.Length == 0) return;
            Log.Event(text);
            Buffers.AddEvent(text);
            lock (Gate)
            {
                if (channel != null) Queue.RemoveAll(p => p.Channel == channel);
                Queue.Add(new Pending { Text = text, Channel = channel, Expires = Time.unscaledTime + ttl });
            }
        }

        internal static void Pump()
        {
            Prism.Poll();
            float now = Time.unscaledTime;
            List<Pending> send = null;
            lock (Gate)
            {
                foreach (var p in Queue) if (p.Expires >= now) (send ??= new List<Pending>()).Add(p);
                Queue.Clear();
            }
            if (send == null) return;
            foreach (var p in send)
            {
                bool interrupt = p.Interrupt && (!p.Menu || _lastWasMenu);
                _lastWasMenu = p.Menu;
                LastSpoken = p.Text;
                Log.Speech(p.Text, interrupt);
                Prism.Speak(p.Text, interrupt, true);
            }
        }

        internal static void Stop() => Prism.Stop();
        internal static void Shutdown() => Prism.Shutdown();

        private static readonly Regex Sprites = new Regex("<sprite[^>]*name=\"?([^\" >]+)\"?[^>]*>", RegexOptions.Compiled);
        private static readonly Regex Tags = new Regex("<[^>]{1,120}>", RegexOptions.Compiled);
        private static readonly Regex Spaces = new Regex(@"\s+", RegexOptions.Compiled);
        // An icon followed by its own word ("[wheat] wheat") would be said twice.
        private static readonly Regex Twice = new Regex(@"\b(\p{L}{3,}) \1\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SpaceBeforeStop = new Regex(@" +([.,;:])", RegexOptions.Compiled);

        private static string SpriteToWords(Match m) => " " + Loc.SpriteName(m.Groups[1].Value) + " ";

        /// <summary>Text pipeline: sprite tags to their names, strip rich text tags, collapse whitespace.</summary>
        internal static string Clean(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            bool hadSprite = text.Contains("<sprite");
            text = Sprites.Replace(text, new MatchEvaluator(SpriteToWords));
            text = Tags.Replace(text, " ");
            text = text.Replace(' ', ' ').Replace("​", "");
            text = Spaces.Replace(text, " ").Trim();
            if (hadSprite) text = SpaceBeforeStop.Replace(Twice.Replace(text, "$1"), "$1");
            // "Sustituir ( oro 5)": a sprite or a tag right inside brackets leaves a space there.
            if (text.IndexOf('(') >= 0) text = text.Replace("( ", "(").Replace(" )", ")").Replace("()", "").Trim();
            return text;
        }
    }
}

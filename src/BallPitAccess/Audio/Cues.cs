using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BallPitAccess.Audio
{
    /// <summary>
    /// Every cue of the mod (see CUES.md): WAV files made with genny by tools/make_cues.py, embedded in
    /// the DLL (Sounds/*.wav). A file with the same name in UserData/BallPitAccess/sounds/ replaces the
    /// embedded one, so a cue can be swapped without a rebuild.
    /// One physical family per category: hand drum = enemy (boss = big drum), steel clank = your ball,
    /// glass tink = gems, kalimba sparkle = special pickup, swoosh / buzz / whistle = hazards, knock = edges.
    /// Pitch = closeness (enemies) or height up the field (pickups); pan = side; rate = urgency.
    /// </summary>
    internal static class Cues
    {
        internal const int EnemySteps = 10;
        private static readonly Dictionary<string, float[]> Bank = new Dictionary<string, float[]>();

        internal static void Build()
        {
            Bank.Clear();
            var asm = Assembly.GetExecutingAssembly();
            const string prefix = "Sounds.";
            foreach (var res in asm.GetManifestResourceNames())
            {
                if (!res.StartsWith(prefix) || !res.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) continue;
                string name = res.Substring(prefix.Length, res.Length - prefix.Length - 4);
                try
                {
                    using var s = asm.GetManifestResourceStream(res);
                    Bank[name] = Wav.Read(s);
                }
                catch (Exception ex) { Core.Log.Warn($"Cue {name}: {ex.Message}"); }
            }
            int overrides = 0;
            try
            {
                string dir = Path.Combine(Core.Log.Dir, "sounds");
                if (Directory.Exists(dir))
                    foreach (var f in Directory.GetFiles(dir, "*.wav"))
                    {
                        try
                        {
                            using var s = File.OpenRead(f);
                            Bank[Path.GetFileNameWithoutExtension(f)] = Wav.Read(s);
                            overrides++;
                        }
                        catch (Exception ex) { Core.Log.Warn($"Cue file {f}: {ex.Message}"); }
                    }
            }
            catch (Exception ex) { Core.Log.Warn("Cue folder: " + ex.Message); }
            // Continuous sounds: crossfade tail into head (50 ms) so they loop without a click.
            foreach (var k in new List<string>(Bank.Keys))
                if (k.StartsWith("loop_")) Bank[k] = LoopVoice.MakeLoop(Bank[k], AudioEngine.Rate / 20);
            Core.Log.Info($"Cues loaded: {Bank.Count} ({overrides} from UserData/BallPitAccess/sounds).");
        }

        internal static bool Has(string name) => Bank.ContainsKey(name);

        /// <summary>Start a continuous sound (silent until its Gain is set). Null if the cue is missing.</summary>
        internal static LoopVoice Loop(string name)
        {
            var v = Bank.TryGetValue(name, out var s) ? LoopVoice.Start(s) : null;
            if (v != null) { v.Name = name; Loops.Add(v); }
            return v;
        }

        /// <summary>Play a loop for a moment with fixed settings (sound guide).</summary>
        internal static void PreviewLoop(string name, float seconds, float pan, float pitch, float pulse, float muffle = 0f)
        {
            var v = Loop(name);
            if (v == null) return;
            Loops.Remove(v);   // a preview is not a sound of the battle
            v.Pan = pan; v.Pitch = pitch; v.Pulse = pulse; v.Muffle = muffle; v.Gain = 0.9f;
            v.StopAfter = (int)(seconds * AudioEngine.Rate);
        }

        /// <summary>pitch: playback rate (1 = as recorded, 0.5 = an octave down).</summary>
        internal static void Play(string name, float pan = 0f, float gain = 1f, float muffle = 0f, float pitch = 1f)
        {
            if (!Bank.TryGetValue(name, out var s)) return;
            AudioEngine.Play(s, gain, pan, muffle, pitch);
            if (!Input.ModKeys.ListOpen && Core.Config.Cues && gain > 0.05f) Remember(name, pan, muffle);
            Game.Telemetry.Cue(name, pan, gain, muffle, pitch);
            if (Core.Config.Debug)
                Core.Log.Cue($"{name} pan={pan:F2} gain={gain:F2}" + (muffle > 0.01f ? $" muffle={muffle:F2}" : "") + (Math.Abs(pitch - 1f) > 0.01f ? $" pitch={pitch:F2}" : ""));
        }
        // ---------------- What was that? (a player could not tell the sounds apart) ----------------

        private struct Heard { public string Entry; public float Pan, Muffle; public long Ms; }
        private static readonly List<Heard> Recent = new List<Heard>();       // game thread only
        private static readonly List<LoopVoice> Loops = new List<LoopVoice>();
        private static readonly System.Diagnostics.Stopwatch Clock = System.Diagnostics.Stopwatch.StartNew();

        /// <summary>The sound guide line (snd_*) that explains a cue, or null for clicks with no meaning of their own.</summary>
        /// <summary>The battle's continuous sounds (telemetry).</summary>
        internal static List<LoopVoice> LiveLoops() { Loops.RemoveAll(v => v.Stopped); return Loops; }

        private static string GuideEntry(string name)
        {
            if (name.Length == 6 && name.StartsWith("enemy") && char.IsDigit(name[5])) return name[5] >= '5' ? "enemy9" : "enemy1";
            if (name.Length == 5 && name.StartsWith("boss") && char.IsDigit(name[4])) return "boss5";
            if (name.StartsWith("treasure")) return "treasure1";
            if (name == "pickup0" || name == "pickup1") return "pickup0";
            if (name == "loop_ball") return "loop_ball_far";
            return Core.Loc.Has("snd_" + name) ? name : null;
        }

        private static void Remember(string name, float pan, float muffle)
        {
            string e = GuideEntry(name);
            if (e == null) return;
            Recent.Add(new Heard { Entry = e, Pan = pan, Muffle = muffle, Ms = Clock.ElapsedMilliseconds });
            if (Recent.Count > 64) Recent.RemoveRange(0, Recent.Count - 64);
        }

        /// <summary>
        /// Q in battle: the sounds playing now and the last few heard, newest first, each with what it means and
        /// where it came from. Every kind is said once (the radar drums repeat all the time).
        /// </summary>
        internal static void SayRecent()
        {
            var lines = new List<string>();
            var said = new HashSet<string>();
            Loops.RemoveAll(v => v.Stopped);
            foreach (var v in Loops)
            {
                if (v.Gain < 0.05f || !Core.Config.Cues) continue;
                string e = GuideEntry(v.Name);
                if (e == null || !said.Add(e)) continue;
                lines.Add(Describe(e, v.Pan, v.Muffle));
            }
            long now = Clock.ElapsedMilliseconds;
            for (int i = Recent.Count - 1; i >= 0 && lines.Count < 4; i--)
            {
                var h = Recent[i];
                if (now - h.Ms > 4000) break;
                if (!said.Add(h.Entry)) continue;
                lines.Add(Describe(h.Entry, h.Pan, h.Muffle));
            }
            if (lines.Count == 0) { Core.Speech.SayMenu(Core.Loc.Get("last_none")); Core.Buffers.SetDetails(null); return; }
            Core.Speech.SayMenu(string.Join(". ", lines));
            Core.Buffers.SetDetails(lines);
        }

        private static string Describe(string entry, float pan, float muffle)
        {
            string t = Core.Loc.Get("snd_" + entry);
            int dot = t.IndexOf(". ");
            if (dot < 0) dot = t.IndexOf('。');
            if (dot > 0) t = t.Substring(0, dot);
            string side = Core.Loc.Get(pan < -0.2f ? "left" : pan > 0.2f ? "right" : "center");
            return t + " (" + side + (muffle > 0.3f ? ", " + Core.Loc.Get("behind") : "") + ")";
        }
    }


    /// <summary>Minimal RIFF WAV reader: PCM 8/16/24/32-bit or float 32, any channel count (mixed to mono),
    /// linearly resampled to the engine rate.</summary>
    internal static class Wav
    {
        internal static float[] Read(Stream stream)
        {
            using var r = new BinaryReader(stream);
            if (new string(r.ReadChars(4)) != "RIFF") throw new InvalidDataException("not RIFF");
            r.ReadInt32();
            if (new string(r.ReadChars(4)) != "WAVE") throw new InvalidDataException("not WAVE");
            int format = 0, channels = 0, rate = 0, bits = 0;
            byte[] data = null;
            while (r.BaseStream.Position + 8 <= r.BaseStream.Length)
            {
                string id = new string(r.ReadChars(4));
                int size = r.ReadInt32();
                if (id == "fmt ")
                {
                    format = r.ReadInt16();
                    channels = r.ReadInt16();
                    rate = r.ReadInt32();
                    r.ReadInt32(); r.ReadInt16();
                    bits = r.ReadInt16();
                    if (size > 16) r.ReadBytes(size - 16);
                    if (format == -2 && size >= 26) format = 1; // WAVE_FORMAT_EXTENSIBLE: assume PCM / float by bits
                }
                else if (id == "data") { data = r.ReadBytes(size); break; }
                else r.ReadBytes(size);
                if ((size & 1) == 1 && r.BaseStream.Position < r.BaseStream.Length) r.ReadByte();
            }
            if (data == null || channels <= 0 || rate <= 0) throw new InvalidDataException("no data");
            int bps = bits / 8, frames = data.Length / (bps * channels);
            var mono = new float[frames];
            for (int i = 0; i < frames; i++)
            {
                float sum = 0;
                for (int c = 0; c < channels; c++)
                {
                    int o = (i * channels + c) * bps;
                    float v;
                    if (format == 3 && bits == 32) v = BitConverter.ToSingle(data, o);
                    else if (bits == 16) v = BitConverter.ToInt16(data, o) / 32768f;
                    else if (bits == 24) v = ((data[o] << 8 | data[o + 1] << 16 | data[o + 2] << 24) >> 8) / 8388608f;
                    else if (bits == 32) v = BitConverter.ToInt32(data, o) / 2147483648f;
                    else v = (data[o] - 128) / 128f;
                    sum += v;
                }
                mono[i] = sum / channels;
            }
            if (rate == AudioEngine.Rate) return mono;
            int n = (int)((long)frames * AudioEngine.Rate / rate);
            var o2 = new float[n];
            double step = (double)rate / AudioEngine.Rate;
            for (int i = 0; i < n; i++)
            {
                double p = i * step;
                int a = (int)p;
                float t = (float)(p - a);
                float x0 = mono[Math.Min(a, frames - 1)], x1 = mono[Math.Min(a + 1, frames - 1)];
                o2[i] = x0 + (x1 - x0) * t;
            }
            return o2;
        }
    }
}

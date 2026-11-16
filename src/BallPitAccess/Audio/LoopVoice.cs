using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BallPitAccess.Core;

namespace BallPitAccess.Audio
{
    /// <summary>
    /// A continuous sound for something that moves continuously (your ball, an enemy shot). The game thread
    /// sets the targets every frame; the audio thread glides to them (about 40 ms), so the sound bends with
    /// the thing instead of stepping. Targets:
    /// - Pan: -1 left .. +1 right.  Pitch: playback rate.  Gain: 0 = silent (the loop keeps running).
    /// - Muffle: 0 clear .. 1 behind (same filter as one-shot cues).
    /// - Pulse: Hz of an amplitude throb (0 = steady), the "parking sensor" for urgency.
    /// The source must be loopable: Cues makes every "loop_*" WAV seamless by crossfading its tail into its head.
    /// </summary>
    internal sealed class LoopVoice
    {
        // Set from the game thread.
        public volatile float Pan, Pitch = 1f, Gain, Muffle, Pulse;
        internal volatile bool Stopped;
        internal string Name;          // cue name, for "what was that?"

        // Audio thread state.
        internal float[] Samples;
        internal int StopAfter = -1;   // frames left before it stops by itself (previews); -1 = never
        private double _pos;
        private float _gain, _pan, _pitch = 1f, _cut = 1f, _lp1, _lp2, _phase;

        private static readonly ConcurrentQueue<LoopVoice> Incoming = new ConcurrentQueue<LoopVoice>();
        private static readonly List<LoopVoice> Active = new List<LoopVoice>();

        internal static LoopVoice Start(float[] samples)
        {
            if (samples == null || samples.Length < 16) return null;
            var v = new LoopVoice { Samples = samples };
            Incoming.Enqueue(v);
            return v;
        }

        /// <summary>Mix every running loop into the stereo buffer (audio thread).</summary>
        internal static void MixAll(float[] mix, int frames, int rate)
        {
            while (Incoming.TryDequeue(out var n)) if (Active.Count < 8) Active.Add(n);
            float master = Config.Cues ? Config.CueVolume : 0f;
            for (int i = Active.Count - 1; i >= 0; i--)
            {
                var v = Active[i];
                if (v.Stopped && v._gain < 0.0005f) { Active.RemoveAt(i); continue; }
                v.Mix(mix, frames, rate, master);
                if (v.StopAfter >= 0) { v.StopAfter -= frames; if (v.StopAfter < 0) v.Stopped = true; }
            }
        }

        private void Mix(float[] mix, int frames, int rate, float master)
        {
            float tGain = Stopped ? 0f : Math.Max(0f, Gain) * master;
            float tPan = Math.Max(-1f, Math.Min(1f, Pan));
            float tPitch = Math.Max(0.25f, Math.Min(4f, Pitch));
            float muffle = Math.Max(0f, Math.Min(1f, Muffle));
            if (muffle > 0.01f) tGain *= 1f + muffle * 0.8f;
            float tCut = muffle > 0.01f
                ? (float)(1.0 - Math.Exp(-2.0 * Math.PI * (5000.0 * Math.Pow(700.0 / 5000.0, muffle)) / rate))
                : 1f;
            float pulse = Math.Max(0f, Pulse);
            float k = 1f - (float)Math.Exp(-1.0 / (0.04 * rate)); // ~40 ms glide
            int len = Samples.Length;
            for (int i = 0; i < frames; i++)
            {
                _gain += (tGain - _gain) * k;
                _pan += (tPan - _pan) * k;
                _pitch += (tPitch - _pitch) * k;
                _cut += (tCut - _cut) * k;
                int p = (int)_pos;
                int q = p + 1 >= len ? 0 : p + 1;
                float t = (float)(_pos - p);
                float s = Samples[p] + (Samples[q] - Samples[p]) * t;
                _pos += _pitch;
                while (_pos >= len) _pos -= len;
                if (_cut < 0.999f) { _lp1 += (s - _lp1) * _cut; _lp2 += (_lp1 - _lp2) * _cut; s = _lp2; }
                else { _lp1 = _lp2 = s; }
                float env = 1f;
                if (pulse > 0.05f)
                {
                    _phase += pulse / rate;
                    if (_phase >= 1f) _phase -= 1f;
                    // Sharp throbs: loud at the start of each period, dipping to 25 % between.
                    float c = 0.5f + 0.5f * (float)Math.Cos(2.0 * Math.PI * _phase);
                    env = 0.25f + 0.75f * c * c * c;
                }
                float a = (_pan + 1f) * 0.25f * (float)Math.PI;
                float g = _gain * env;
                mix[i * 2] += s * g * (float)Math.Cos(a);
                mix[i * 2 + 1] += s * g * (float)Math.Sin(a);
            }
        }

        /// <summary>Make a sample loop seamlessly: the last <paramref name="fade"/> frames are crossfaded
        /// (equal power) into the first ones and dropped.</summary>
        internal static float[] MakeLoop(float[] s, int fade)
        {
            if (s.Length < fade * 3) return s;
            int n = s.Length - fade;
            var o = new float[n];
            Array.Copy(s, o, n);
            for (int i = 0; i < fade; i++)
            {
                double x = (double)i / fade;
                float a = (float)Math.Sin(x * Math.PI * 0.5), b = (float)Math.Cos(x * Math.PI * 0.5);
                o[i] = s[i] * a + s[n + i] * b;
            }
            return o;
        }
    }
}

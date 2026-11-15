using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using BallPitAccess.Core;

namespace BallPitAccess.Audio
{
    /// <summary>
    /// The mod's own audio output (template 14.2): a winmm waveOut stream, separate from the game's FMOD
    /// mixer, so cues are never ducked, paused or filtered by the game. 48 kHz stereo 16-bit, four 10 ms
    /// buffers: about 30 to 40 ms from Play() to the speaker.
    /// A dedicated thread mixes voices; the game thread only enqueues voice requests (lock-free queue).
    /// </summary>
    internal static class AudioEngine
    {
        internal const int Rate = 48000;
        private const int BufferFrames = 480; // 10 ms
        private const int BufferCount = 4;

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEFORMATEX
        {
            public ushort wFormatTag, nChannels;
            public uint nSamplesPerSec, nAvgBytesPerSec;
            public ushort nBlockAlign, wBitsPerSample, cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEHDR
        {
            public IntPtr lpData;
            public uint dwBufferLength, dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags, dwLoops;
            public IntPtr lpNext, reserved;
        }

        [DllImport("winmm.dll")] private static extern int waveOutOpen(out IntPtr h, uint dev, ref WAVEFORMATEX fmt, IntPtr cb, IntPtr inst, uint flags);
        [DllImport("winmm.dll")] private static extern int waveOutPrepareHeader(IntPtr h, IntPtr hdr, uint size);
        [DllImport("winmm.dll")] private static extern int waveOutUnprepareHeader(IntPtr h, IntPtr hdr, uint size);
        [DllImport("winmm.dll")] private static extern int waveOutWrite(IntPtr h, IntPtr hdr, uint size);
        [DllImport("winmm.dll")] private static extern int waveOutReset(IntPtr h);
        [DllImport("winmm.dll")] private static extern int waveOutClose(IntPtr h);

        private const uint WAVE_MAPPER = 0xFFFFFFFF, CALLBACK_EVENT = 0x00050000, WHDR_DONE = 1;

        private sealed class Voice
        {
            public float[] Samples;
            public double Pos;
            public float Rate = 1f;    // playback rate: 2 = an octave up, 0.5 = an octave down
            public float Gain, Pan;
            public float Lp, Lp1, Lp2; // two cascaded one-pole low-passes: coefficient (1 = open) and states
        }

        private static readonly ConcurrentQueue<Voice> Incoming = new ConcurrentQueue<Voice>();
        private static readonly List<Voice> Active = new List<Voice>();
        private static Thread _thread;
        private static volatile bool _run;
        private static IntPtr _wave;
        private static AutoResetEvent _event;

        internal static bool Running => _run;

        internal static void Start()
        {
            if (_run) return;
            _run = true;
            _thread = new Thread(Loop) { IsBackground = true, Name = "BallPitAccess audio", Priority = ThreadPriority.AboveNormal };
            _thread.Start();
        }

        internal static void Stop()
        {
            _run = false;
            try { _thread?.Join(500); } catch { }
        }

        /// <summary>Queue a sound. pan -1 left .. +1 right; gain 0..1 (times the cue volume setting);
        /// muffle 0 (clear) .. 1 (heard through a wall: 12 dB/octave low-pass down to 700 Hz), used for "behind you".</summary>
        internal static void Play(float[] samples, float gain = 1f, float pan = 0f, float muffle = 0f, float pitch = 1f)
        {
            if (!_run || samples == null || !Config.Cues) return;
            float lp = 1f;
            if (muffle > 0.01f)
            {
                muffle = Math.Min(1f, muffle);
                // Cutoff glides from 5 kHz (a hint) to 700 Hz (fully behind), exponentially. 700 Hz, not lower:
                // the gem tink sits at 880 to 1570 Hz and must stay audible.
                double fc = 5000.0 * Math.Pow(700.0 / 5000.0, muffle);
                lp = (float)(1.0 - Math.Exp(-2.0 * Math.PI * fc / Rate));
                gain *= 1f + muffle * 0.8f; // the filter takes energy away; give some back so it is not just quieter
            }
            Incoming.Enqueue(new Voice
            {
                Samples = samples, Gain = gain * Config.CueVolume, Pan = Math.Max(-1f, Math.Min(1f, pan)), Lp = lp,
                Rate = Math.Max(0.25f, Math.Min(4f, pitch)),
            });
        }

        private static void Loop()
        {
            var fmt = new WAVEFORMATEX
            {
                wFormatTag = 1, nChannels = 2, nSamplesPerSec = Rate, wBitsPerSample = 16,
                nBlockAlign = 4, nAvgBytesPerSec = Rate * 4, cbSize = 0
            };
            _event = new AutoResetEvent(false);
            int r = waveOutOpen(out _wave, WAVE_MAPPER, ref fmt, _event.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, CALLBACK_EVENT);
            if (r != 0)
            {
                Log.Error("waveOutOpen failed: " + r + "; audio cues disabled.");
                _run = false;
                return;
            }
            int hdrSize = Marshal.SizeOf<WAVEHDR>();
            var headers = new IntPtr[BufferCount];
            var data = new IntPtr[BufferCount];
            var mix = new float[BufferFrames * 2];
            var pcm = new short[BufferFrames * 2];
            for (int i = 0; i < BufferCount; i++)
            {
                data[i] = Marshal.AllocHGlobal(BufferFrames * 4);
                headers[i] = Marshal.AllocHGlobal(hdrSize);
                var h = new WAVEHDR { lpData = data[i], dwBufferLength = (uint)(BufferFrames * 4) };
                Marshal.StructureToPtr(h, headers[i], false);
                waveOutPrepareHeader(_wave, headers[i], (uint)hdrSize);
                Fill(mix, pcm, data[i]);
                waveOutWrite(_wave, headers[i], (uint)hdrSize);
            }
            Log.Info("Audio engine started (waveOut, 48 kHz, 4 x 10 ms).");
            try
            {
                while (_run)
                {
                    _event.WaitOne(50);
                    for (int i = 0; i < BufferCount; i++)
                    {
                        var h = Marshal.PtrToStructure<WAVEHDR>(headers[i]);
                        if ((h.dwFlags & WHDR_DONE) == 0) continue;
                        Fill(mix, pcm, data[i]);
                        waveOutWrite(_wave, headers[i], (uint)hdrSize);
                    }
                }
            }
            catch (Exception ex) { Log.Error("Audio loop failed: " + ex.Message); }
            finally
            {
                waveOutReset(_wave);
                for (int i = 0; i < BufferCount; i++)
                {
                    waveOutUnprepareHeader(_wave, headers[i], (uint)hdrSize);
                    Marshal.FreeHGlobal(headers[i]);
                    Marshal.FreeHGlobal(data[i]);
                }
                waveOutClose(_wave);
            }
        }

        private static void Fill(float[] mix, short[] pcm, IntPtr dest)
        {
            Array.Clear(mix, 0, mix.Length);
            while (Incoming.TryDequeue(out var v)) { if (Active.Count < 16) Active.Add(v); }
            for (int vi = Active.Count - 1; vi >= 0; vi--)
            {
                var v = Active[vi];
                // Equal-power pan.
                float angle = (v.Pan + 1f) * 0.25f * (float)Math.PI;
                float gl = (float)Math.Cos(angle) * v.Gain, gr = (float)Math.Sin(angle) * v.Gain;
                bool filt = v.Lp < 0.999f, plain = Math.Abs(v.Rate - 1f) < 0.001f;
                int len = v.Samples.Length;
                for (int i = 0; i < BufferFrames; i++)
                {
                    float s;
                    if (plain)
                    {
                        int k = (int)v.Pos;
                        if (k >= len) break;
                        s = v.Samples[k];
                    }
                    else
                    {
                        // Resampled (pitch shift by playback rate), linear interpolation.
                        int k = (int)v.Pos;
                        if (k >= len - 1) { v.Pos = len; break; }
                        float t = (float)(v.Pos - k);
                        s = v.Samples[k] + (v.Samples[k + 1] - v.Samples[k]) * t;
                    }
                    if (filt) { v.Lp1 += (s - v.Lp1) * v.Lp; v.Lp2 += (v.Lp1 - v.Lp2) * v.Lp; s = v.Lp2; }
                    mix[i * 2] += s * gl;
                    mix[i * 2 + 1] += s * gr;
                    v.Pos += v.Rate;
                }
                if (v.Pos >= len) Active.RemoveAt(vi);
            }
            LoopVoice.MixAll(mix, BufferFrames, Rate);
            for (int i = 0; i < mix.Length; i++)
            {
                float s = mix[i];
                if (s > 1f) s = 1f; else if (s < -1f) s = -1f;
                pcm[i] = (short)(s * 32000);
            }
            Marshal.Copy(pcm, 0, dest, pcm.Length);
        }
    }
}

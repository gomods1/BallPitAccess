using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using BallPitAccess.Audio;
using BallPitAccess.Core;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Battle recording for tuning the cues (MelonPreferences [BallPitAccess] Telemetry = true; off by default).
    /// Writes UserData/BallPitAccess/telemetry.tsv, one record per line, tab separated, first field = ms since
    /// launch (one clock for everything), second = record type:
    ///   cue   name pan gain muffle pitch            every one-shot cue as it is played
    ///   say   text                                   every spoken line (as queued, with ! = interrupt)
    ///   snap  ...                                    every 0.1 s in battle (see Snap for the fields)
    ///   loop  name gain pan pitch pulse muffle       continuous sounds, with each snap
    ///   ball  id x y state side baby dx dy speed     every active ball, with each snap
    ///   bst   id from to side x y heroX heroY        a ball changes state (BallObj.SetState)
    ///   rm    id returnType state side x y heroX heroY  a ball is removed (BallMgr.RemoveBall)
    ///   enemy type x y rows line boss hp maxhp state the 12 nearest enemies, with each snap
    ///   item  type state x y reach                   every pickup, with each snap
    ///   shot  x y dx dy                              every enemy projectile, with each snap
    ///   ev    what ...                               state changes: game state, health, kills, level, keys
    /// tools/telemetry_report.py turns it into a report.
    /// </summary>
    internal static class Telemetry
    {
        private static StreamWriter _w;
        private static readonly Stopwatch Clock = Stopwatch.StartNew();
        private static readonly object Gate = new object();
        private static float _nextSnap;
        private static string _lastState = "";
        private static float _lastHealth = -1f;
        private static int _lastKills = -1, _errors;
        private static string _lastLevel = "";
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        internal static bool On => _w != null;

        /// <summary>At launch: a new recording; the last session's is kept as telemetry.prev.tsv.</summary>
        internal static void Init()
        {
            if (Config.Telemetry) Open(false);
        }

        /// <summary>Turned on or off from the mod's settings (F4). Turning it on mid-session adds to this session's file.</summary>
        internal static void Toggle()
        {
            Config.Telemetry = !Config.Telemetry;
            if (Config.Telemetry) { if (_w == null) Open(true); }
            else Close();
        }

        private static void Open(bool append)
        {
            try
            {
                _lastState = ""; _lastHealth = -1f; _lastKills = -1; _lastLevel = "";
                string path = Path.Combine(Log.Dir, "telemetry.tsv");
                // A new session starts a new file (the clock restarts at launch); the last one is kept.
                if (!append && File.Exists(path))
                    try { File.Copy(path, Path.Combine(Log.Dir, "telemetry.prev.tsv"), true); } catch { }
                _w = new StreamWriter(path, append, new UTF8Encoding(false)) { AutoFlush = false };
                Write("ev", "start", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Log.Info("Telemetry on: telemetry.tsv");
            }
            catch (Exception ex) { Log.Warn("Telemetry: " + ex.Message); _w = null; }
        }

        private static string F(float v) => v.ToString("0.###", Inv);

        internal static void Write(string type, params object[] fields)
        {
            if (_w == null) return;
            var sb = new StringBuilder();
            sb.Append(Clock.ElapsedMilliseconds).Append('\t').Append(type);
            foreach (var f in fields)
            {
                sb.Append('\t');
                if (f is float fl) sb.Append(F(fl));
                else sb.Append((f?.ToString() ?? "").Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' '));
            }
            lock (Gate) { try { _w.WriteLine(sb.ToString()); } catch { } }
        }

        internal static void Cue(string name, float pan, float gain, float muffle, float pitch)
        {
            if (_w != null) Write("cue", name, pan, gain, muffle, pitch);
        }

        internal static void Say(string text, bool interrupt)
        {
            if (_w != null) Write("say", (interrupt ? "!" : "") + text);
        }

        internal static void Key(string what)
        {
            if (_w != null) Write("ev", "key", what);
        }

        /// <summary>Main thread, after every other module.</summary>
        internal static void Update()
        {
            if (_w == null) return;
            try
            {
                Events();
                float now = Time.unscaledTime;
                if (Battle.InBattle && now >= _nextSnap)
                {
                    _nextSnap = now + 0.1f;
                    Snap();
                }
                if (Time.frameCount % 60 == 0) lock (Gate) _w.Flush();
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Telemetry: " + ex);
            }
        }

        private static void Events()
        {
            string st = "";
            try { st = GameMgr.I != null ? GameMgr.I.CurState.ToString() : ""; } catch { }
            if (st != _lastState) { Write("ev", "state", st); _lastState = st; }
            var save = BattleSaveData.I;
            if (save == null || !Battle.InBattle) { _lastHealth = -1f; _lastKills = -1; return; }
            float h = save.CurHealth;
            if (Math.Abs(h - _lastHealth) > 0.01f) { if (_lastHealth >= 0f) Write("ev", "health", h, h - _lastHealth); _lastHealth = h; }
            int k = save.NumKills;
            if (k != _lastKills) { if (_lastKills >= 0) Write("ev", "kills", k, k - _lastKills); _lastKills = k; }
            string lv = save.CurLevel.ToString();
            if (lv != _lastLevel) { Write("ev", "level", lv); _lastLevel = lv; }
            // Movement keys the player (or the driver) holds: lets the report tell assist steps from own steps.
        }

        private static void Snap()
        {
            var save = BattleSaveData.I;
            Vector3 p = Battle.PlayerPos;
            float speed = 1f;
            try { speed = PlayerPrefsMgr.sGameSpeed; } catch { }
            bool w = Input.Keys.Down('W'), a = Input.Keys.Down('A'), s = Input.Keys.Down('S'), d = Input.Keys.Down('D');
            string keys = (w ? "W" : "") + (a ? "A" : "") + (s ? "S" : "") + (d ? "D" : "") + (Input.Bindings.Down("catch") ? "+" : "");
            string tgt = "";
            try { var t = AimAssist.Target; if (t != null && t.Obj != null) { var c = t.Obj.GetWorldCenterPos(); tgt = t.Type + "@" + F(c.x) + "," + F(c.y); } } catch { }
            Vector2 aim = Vector2.zero;
            try { if (BallMgr.I != null) aim = BallMgr.I.GetCurAimDir(); } catch { }
            // snap: x y health speed playing comingX eta clear dodging dodgeDir catchActive keys aimDeg target
            Write("snap", p.x, p.y, save != null ? save.CurHealth : -1f, speed, Battle.Playing ? 1 : 0,
                Battle.BallComing ? Battle.BallLandX : float.NaN, Battle.BallComing ? Battle.BallEta : float.NaN, Battle.BallClear ? 1 : 0,
                Hazards.Dodging ? 1 : 0, Hazards.DodgeDir, CatchAssist.Active ? 1 : 0, keys,
                Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg, tgt);

            foreach (var v in Cues.LiveLoops())
                if (v.Gain > 0.001f) Write("loop", v.Name, v.Gain, v.Pan, v.Pitch, v.Pulse, v.Muffle);

            var bm = BallMgr.I;
            if (bm != null && bm.ActiveBalls != null)
                for (int i = 0; i < bm.ActiveBalls.Count && i < 30; i++)
                {
                    var b = bm.ActiveBalls[i];
                    if (b == null || !b.IsActive) continue;
                    bool baby = false;
                    try { baby = b.IsBaby(); } catch { }
                    Vector3 bp = b.transform.position, bd = b.AimDir;
                    Write("ball", b.Pointer.ToInt64().ToString("x"), bp.x, bp.y, b.CurState, b.IsSide ? 1 : 0, baby ? 1 : 0, bd.x, bd.y, b.GetSpeed());
                }

            int n = 0;
            foreach (var t in Battle.Threats)
            {
                if (n++ >= 12) break;
                Write("enemy", t.Inst.Type, t.Center.x, t.Center.y, t.Rows, t.Line, t.Boss ? 1 : 0, (float)t.Inst.CurHealth, (float)t.Inst.MaxHealth, t.Inst.CurState);
            }

            var list = save != null ? save.Pickups : null;
            if (list != null)
                for (int i = 0; i < list.Count; i++)
                {
                    var it = list[i];
                    var o = it?.Obj;
                    if (o == null || !o.isActiveAndEnabled) continue;
                    Vector3 ip = o.transform.position;
                    Write("item", it.Type, o.CurState, ip.x, ip.y, ip.y <= Battle.MaxY + 0.6f ? 1 : 0);
                }

            foreach (var sp in Hazards.ShotPositions())
                Write("shot", sp.x, sp.y, sp.z, sp.w);
        }

        internal static void Close()
        {
            if (_w == null) return;
            lock (Gate) { try { _w.Flush(); _w.Dispose(); } catch { } _w = null; }
        }
    }
}

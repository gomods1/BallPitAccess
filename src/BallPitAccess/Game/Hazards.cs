using System;
using System.Collections.Generic;
using System.Text;
using BallPitAccess.Audio;
using BallPitAccess.Core;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Things that hurt and are not an enemy's body: shots in the air (ArrowObj), laser beams being aimed
    /// (EnemyLaserObj), lobbed bombs (CannonballObj, target captured from its Run call) and floor hazards
    /// (ObstacleObj). One rule for the sounds: the plain cue says "this exists, on that side"; the "_hit"
    /// cue says "it will hit where you stand: move", and repeats until you are out of it.
    /// Scanned a few times a second; nothing here changes the game.
    /// </summary>
    internal static class Hazards
    {
        private sealed class Bomb { public Vector3 Target; public float Radius; public float Until; public bool Announced; }

        private static readonly List<Bomb> Bombs = new List<Bomb>();
        private static readonly HashSet<IntPtr> LasersSeen = new HashSet<IntPtr>();
        private static readonly HashSet<IntPtr> LasersNow = new HashSet<IntPtr>();
        private static readonly HashSet<int> FloorOn = new HashSet<int>();
        private static float _nextScan, _nextFloor, _nextHit;
        private static int _errors;
        private static int _shots, _lasers, _floors;
        private static float _dodgeAt = -10f;

        /// <summary>A shot, laser or bomb will hit the hero where it stands: the side to step to (-1 / +1).
        /// Held for 0.3 s after the last hit prediction so the catch assist does not walk straight back in.</summary>
        internal static int DodgeDir { get; private set; }
        internal static bool Dodging => DodgeDir != 0 && Time.unscaledTime - _dodgeAt < 0.3f;

        private static void MarkDodge(float x)
        {
            // Keep the side chosen for this dodge while it lasts, unless the hero has run out of room there:
            // recomputing every scan flipped it near a wall and the hero bounced off the wall (135 knocks a run).
            int dir = Math.Sign(DodgePan(x));
            if (Dodging && DodgeDir != 0)
            {
                float room = DodgeDir > 0 ? Battle.RightX - Battle.PlayerPos.x : Battle.PlayerPos.x - Battle.LeftX;
                if (room > 0.3f) dir = DodgeDir;
            }
            DodgeDir = dir;
            _dodgeAt = Time.unscaledTime;
        }

        internal static void TryPatch(HarmonyLib.Harmony h)
        {
            try
            {
                var m = AccessTools.Method(typeof(CannonballObj), nameof(CannonballObj.Run));
                if (m == null) { Log.Warn("CannonballObj.Run not found: no bomb warnings."); return; }
                h.Patch(m, prefix: new HarmonyMethod(typeof(Hazards), nameof(BombPrefix)));
                Log.Info("Patched CannonballObj.Run");
            }
            catch (Exception ex) { Log.Error("Bomb patch failed: " + ex); }
        }

        private static void BombPrefix(Vector3 tgtPos, float previewLen, float radius)
        {
            try
            {
                float speed = 1f;
                try { speed = Math.Max(0.25f, PlayerPrefsMgr.sGameSpeed); } catch { }
                lock (Bombs)
                    Bombs.Add(new Bomb { Target = tgtPos, Radius = radius, Until = Time.unscaledTime + Mathf.Clamp(previewLen / speed, 0.3f, 6f) });
            }
            catch { }
        }

        private static LoopVoice _shotLoop;

        /// <summary>Every hazard sound goes through here: muted by the hazard filter (number key 4), while the
        /// scan goes on for the catch assist and the N key.</summary>
        private static void Play(string name, float pan = 0f, float gain = 1f, float muffle = 0f)
        {
            if (Config.HazardCues) Cues.Play(name, pan, gain, muffle);
        }

        internal static void Update()
        {
            float now = Time.unscaledTime;
            if (!Battle.InBattle) { ShotLoopOff(); if (Bombs.Count > 0) lock (Bombs) Bombs.Clear(); return; }
            if (!Battle.Playing)
            {
                ShotLoopOff();
                // Paused or in a menu: the bombs in the air wait too.
                if (Bombs.Count > 0) lock (Bombs) foreach (var b in Bombs) b.Until += Time.unscaledDeltaTime;
                return;
            }
            if (now < _nextScan) return;
            _nextScan = now + 0.1f;
            try
            {
                bool hit = false;
                hit |= ScanShots(now);
                hit |= ScanLasers(now);
                hit |= ScanBombs(now);
                if (now >= _nextFloor) { _nextFloor = now + 0.3f; ScanFloor(); }
                if (hit && now >= _nextHit) _nextHit = now + 0.5f;
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Hazards: " + ex);
            }
        }

        /// <summary>Distance from the hero to a ray, and how far along the ray the closest point is (negative: behind it).</summary>
        private static float RayMiss(Vector2 from, Vector2 dir, out float along, out Vector2 closest)
        {
            Vector2 p = Battle.PlayerPos;
            along = Vector2.Dot(p - from, dir);
            closest = from + dir * along;
            return Vector2.Distance(closest, p);
        }

        private static readonly List<Vector4> ShotList = new List<Vector4>();
        // Shot speed from its movement between scans, and the shots already announced as "will hit".
        private struct ShotSeen { public Vector2 Pos; public float T; public float Speed; }
        private static readonly Dictionary<IntPtr, ShotSeen> ShotsSeen = new Dictionary<IntPtr, ShotSeen>();
        private static readonly HashSet<IntPtr> ShotsWarned = new HashSet<IntPtr>();
        private static readonly HashSet<IntPtr> ShotsNow = new HashSet<IntPtr>();

        /// <summary>When the last "will hit" alarm sounded (the radar keeps quiet around it so it is not masked).</summary>
        internal static float LastAlarm { get; private set; } = -10f;

        /// <summary>Enemy shots of the last scan: x, y, direction x, direction y (telemetry).</summary>
        internal static List<Vector4> ShotPositions() => ShotList;

        private static bool ScanShots(float now)
        {
            var arrows = UnityEngine.Object.FindObjectsOfType<ArrowObj>();
            _shots = 0;
            ShotList.Clear();
            float bestAlong = float.MaxValue, bestMiss = 0, bestX = 0, realAlong = 0, bestSpeed = 0;
            Vector3 bestPos = Vector3.zero;
            IntPtr bestPtr = IntPtr.Zero;
            ShotsNow.Clear();
            bool any = false;
            foreach (var a in arrows)
            {
                if (a == null || !a.isActiveAndEnabled) continue;
                bool friendly;
                try { friendly = a.IsFriendly() || a.IsReflected; } catch { friendly = false; }
                if (friendly) continue;
                _shots++;
                ShotsNow.Add(a.Pointer);
                Vector2 pos2 = a.transform.position;
                float spd = 0f;
                if (ShotsSeen.TryGetValue(a.Pointer, out var seen) && now - seen.T > 0.01f)
                    spd = Mathf.Lerp(seen.Speed > 0f ? seen.Speed : Vector2.Distance(pos2, seen.Pos) / (now - seen.T), Vector2.Distance(pos2, seen.Pos) / (now - seen.T), 0.5f);
                ShotsSeen[a.Pointer] = new ShotSeen { Pos = pos2, T = now, Speed = spd };
                Vector3 d3 = a.AimDir;
                if (ShotList.Count < 40) { var ap = a.transform.position; ShotList.Add(new Vector4(ap.x, ap.y, d3.x, d3.y)); }
                Vector2 dir = new Vector2(d3.x, d3.y);
                if (dir.sqrMagnitude < 0.0001f) continue;
                dir.Normalize();
                float miss = RayMiss(a.transform.position, dir, out float along, out Vector2 closest);
                if (along <= 0f || along > Battle.RowH * 9f) continue;      // moving away, or still far
                // The one that matters most: will hit first, else passes closest.
                bool hits = miss < Player.kColHalfWidth + 0.35f;
                float rank = hits ? along : along + 100f + miss;
                if (rank < bestAlong)
                {
                    bestAlong = rank; bestMiss = miss; bestX = hits ? a.transform.position.x : closest.x; any = true;
                    realAlong = along; bestPos = a.transform.position; bestPtr = a.Pointer; bestSpeed = spd;
                }
            }
            // Forget shots that are gone.
            if (ShotsSeen.Count > ShotsNow.Count)
            {
                var gone = new List<IntPtr>();
                foreach (var k in ShotsSeen.Keys) if (!ShotsNow.Contains(k)) gone.Add(k);
                foreach (var k in gone) { ShotsSeen.Remove(k); ShotsWarned.Remove(k); }
            }
            if (!any) { ShotLoopOff(); return false; }
            bool willHit = bestAlong < 100f;
            ShotLoop(willHit, bestPos, realAlong, bestMiss);
            if (willHit)
            {
                MarkDodge(bestX);
                // One alarm per shot (telemetry: the same shot re-alarmed every 0.28 s, 316 alarms for 8 hits),
                // and only once it is close: within 1.5 s of reaching the hero, or 4 rows if its speed is unknown.
                bool soon = bestSpeed > 0.1f ? realAlong / bestSpeed < 1.5f : realAlong < Battle.RowH * 4f;
                if (soon && ShotsWarned.Add(bestPtr)) { Play("shot_hit", Battle.PanFor(bestPos.x), 1f, Battle.Muffle(bestPos.y)); LastAlarm = now; }
                return true;
            }
            return false;
        }

        /// <summary>
        /// The shot that matters most, as one continuous whistle that moves with it: pan = where the shot is
        /// now, pitch = how close it is (low 9 rows away, rising as it comes), louder when it will hit (the
        /// `shot_hit` alarm then says which way to step), muffled if it is behind you. A shot passing further
        /// than 2.5 columns away is silent.
        /// </summary>
        private static void ShotLoop(bool hits, Vector3 pos, float along, float miss)
        {
            if ((!hits && miss >= Battle.ColW * 2.5f) || Input.ModKeys.ListOpen || !Config.HazardCues) { ShotLoopOff(); return; }
            if (_shotLoop == null) { _shotLoop = Cues.Loop("loop_shot"); if (_shotLoop == null) return; }
            float near = 1f - Mathf.Clamp01(along / (Battle.RowH * 9f));
            _shotLoop.Pan = Battle.PanFor(pos.x);
            _shotLoop.Pitch = 0.6f + 0.6f * near;
            _shotLoop.Muffle = Battle.Muffle(pos.y);
            _shotLoop.Pulse = 0f;
            _shotLoop.Gain = hits ? 0.55f : 0.3f;
        }

        private static void ShotLoopOff()
        {
            if (_shotLoop != null) _shotLoop.Gain = 0f;
        }

        /// <summary>
        /// The side the catch assist steps to out of a hit. Not a sound pan: every cue sounds where its thing
        /// is, relative to the hero (CUES.md). Away from the threat; dead on, or when that side has less than a
        /// column of room, toward the side with more room.
        /// </summary>
        private static float DodgePan(float x)
        {
            float px = Battle.PlayerPos.x, off = x - px;
            float roomR = Battle.RightX - px, roomL = px - Battle.LeftX;
            int dir;
            if (Math.Abs(off) < 0.05f) dir = roomR >= roomL ? 1 : -1;
            else
            {
                dir = off < 0 ? 1 : -1;
                if ((dir > 0 ? roomR : roomL) < Battle.ColW && (dir > 0 ? roomL : roomR) > Battle.ColW) dir = -dir;
            }
            return dir * 0.7f;
        }

        private static bool ScanLasers(float now)
        {
            var lasers = UnityEngine.Object.FindObjectsOfType<EnemyLaserObj>();
            LasersNow.Clear();
            _lasers = 0;
            bool inBeam = false;
            float beamX = 0;
            Vector3 beamFrom = Vector3.zero;
            foreach (var l in lasers)
            {
                if (l == null || !l.isActiveAndEnabled) continue;
                _lasers++;
                LasersNow.Add(l.Pointer);
                Vector3 d3 = l.AimDir;
                Vector2 dir = new Vector2(d3.x, d3.y);
                if (dir.sqrMagnitude < 0.0001f) continue;
                dir.Normalize();
                Vector3 start = l.transform.position;
                try { var x = l.GetShootXfm(); if (x != null) start = x.position; } catch { }
                float miss = RayMiss(start, dir, out float along, out Vector2 closest);
                float len = l.Length > 0.1f ? l.Length : 100f;
                bool reaches = along > 0f && along < len + 0.5f;
                bool hits = reaches && miss < l.Width * 0.5f + Player.kColHalfWidth + 0.15f;
                if (hits) { inBeam = true; beamX = closest.x; beamFrom = start; }
                if (LasersSeen.Add(l.Pointer) && !hits)
                    Play("beam", Battle.PanFor(reaches ? closest.x : start.x), 0.9f);
            }
            LasersSeen.IntersectWith(LasersNow);
            if (inBeam) MarkDodge(beamX);
            if (inBeam && now >= _nextHit) { Play("beam_hit", Battle.PanFor(beamFrom.x), 1f, Battle.Muffle(beamFrom.y)); LastAlarm = now; }
            return inBeam;
        }

        private static bool ScanBombs(float now)
        {
            bool inBlast = false;
            float blastX = 0;
            Vector3 blastAt = Vector3.zero;
            lock (Bombs)
            {
                for (int i = Bombs.Count - 1; i >= 0; i--)
                {
                    var b = Bombs[i];
                    if (now > b.Until) { Bombs.RemoveAt(i); continue; }
                    float d = Vector2.Distance(b.Target, Battle.PlayerPos);
                    bool hits = d < b.Radius + Player.kColHalfWidth + 0.2f;
                    if (hits) { inBlast = true; blastX = b.Target.x; blastAt = b.Target; }
                    if (!b.Announced)
                    {
                        b.Announced = true;
                        if (!hits && d < Battle.ColW * 5f) Play("bomb", Battle.PanFor(b.Target.x), 0.8f);
                    }
                }
            }
            if (inBlast) MarkDodge(blastX);
            if (inBlast && now >= _nextHit) { Play("bomb_hit", Battle.PanFor(blastAt.x), 1f, Battle.Muffle(blastAt.y)); LastAlarm = now; }
            return inBlast;
        }

        private static void ScanFloor()
        {
            var obs = UnityEngine.Object.FindObjectsOfType<ObstacleObj>();
            _floors = 0;
            int mask = 0;
            foreach (var o in obs)
            {
                if (o == null || !o.isActiveAndEnabled) continue;
                _floors++;
                bool touching;
                try { touching = o._isTouchingPlayer && o.BallSrc == null; } catch { touching = false; }   // BallSrc: a patch left by the hero's own ball
                if (!touching) continue;
                int t = (int)o.Type;
                mask |= 1 << t;
                if (FloorOn.Add(t))
                {
                    Play("floor");
                    Speech.Battle(Names.Obstacle(o.Type), "floor", 2.5f);
                }
            }
            FloorOn.RemoveWhere(t => (mask & (1 << t)) == 0);
        }

        /// <summary>On demand: what is in the air and on the floor.</summary>
        internal static void Say()
        {
            if (!Battle.InBattle) { Speech.SayMenu(Loc.Get("not_in_battle")); return; }
            var lines = new List<string>();
            try
            {
                foreach (var o in UnityEngine.Object.FindObjectsOfType<ObstacleObj>())
                    if (o != null && o.isActiveAndEnabled && o.BallSrc == null) lines.Add(Names.Obstacle(o.Type) + ", " + Battle.Where(o.transform.position));
            }
            catch { }
            string head = Loc.F("hazards", _shots, _lasers, lines.Count);
            Speech.SayMenu(head + (lines.Count > 0 ? ". " + lines[0] : ""));
            Buffers.SetDetails(lines);
        }

        /// <summary>Where the bombs in the air will land, with their blast radius (the battle map).</summary>
        internal static List<KeyValuePair<Vector3, float>> BombTargets()
        {
            var o = new List<KeyValuePair<Vector3, float>>();
            float now = Time.unscaledTime;
            lock (Bombs) foreach (var b in Bombs) if (now <= b.Until) o.Add(new KeyValuePair<Vector3, float>(b.Target, b.Radius));
            return o;
        }

        internal static string Dump()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"hazards: shots {_shots} lasers {_lasers} floor {_floors} bombs {Bombs.Count}");
            try
            {
                foreach (var a in UnityEngine.Object.FindObjectsOfType<ArrowObj>())
                    if (a != null) sb.AppendLine($"shot {a.Type} pos {a.transform.position} dir {a.AimDir} friendly {a.IsFriendly()} reflected {a.IsReflected}");
                foreach (var l in UnityEngine.Object.FindObjectsOfType<EnemyLaserObj>())
                    if (l != null) sb.AppendLine($"laser {l.LaserType} pos {l.transform.position} dir {l.AimDir} len {l.Length} width {l.Width}");
                foreach (var o in UnityEngine.Object.FindObjectsOfType<ObstacleObj>())
                    if (o != null) sb.AppendLine($"obstacle {o.Type} pos {o.transform.position} size {o._size}");
            }
            catch (Exception ex) { sb.AppendLine("dump failed: " + ex.Message); }
            return sb.ToString();
        }
    }
}

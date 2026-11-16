using System;
using System.Collections.Generic;
using System.Text;
using BallPitAccess.Audio;
using BallPitAccess.Core;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Things lying on the field (BattleSaveData.Pickups). Sighted players see them and walk over.
    /// Checked in GameAssembly.dll (1.308): every type is collected by walking (Player.MyUpdate collects any
    /// pickup collider within the pickup range, once it is kIdle; GridUtl.IsPlayerPickup returns true for all
    /// types; PickupMgr.BallHitPickup is about enemy shots, not items). The game keeps pickups between
    /// Player.MinY - 0.33 and Player.MaxY - 2.125 (GetMin/MaxPickupY), inside the hero's walking area.
    /// - Special pickups (fuser, blueprint, egg, gear, health, magnet, bombs, gem bags) are announced when
    ///   they drop, and the nearest one gets a kalimba beacon.
    /// - Ordinary ones (XP crystals, gold, resources) do not collect themselves: the nearest one the hero can
    ///   walk to (or a heap of them a little further, BestPlain) gets a glass tink on its own timer (quieter and slower while a special one is beaconed).
    /// Both beacons: pan = the side to walk to (centred = straight ahead or back), pitch = how many rows away,
    /// muffled = behind the hero. A pickup beyond the hero's walking area is not beaconed.
    /// </summary>
    internal static class Pickups
    {
        private static readonly HashSet<IntPtr> Seen = new HashSet<IntPtr>();
        private static readonly HashSet<IntPtr> Now = new HashSet<IntPtr>();
        private static float _nextScan, _nextSpecial, _nextPlain, _reach = 0.6f;
        private static int _errors;
        private static IntPtr _plainTarget;

        private sealed class Item { public PickupInst Inst; public Vector3 Pos; public bool Special; public bool Reachable; public float Dist; }
        private static readonly List<Item> Items = new List<Item>();

        internal static bool IsSpecial(PickupInst p)
        {
            switch (p.Type)
            {
                case PickupType.kXP: case PickupType.kXP2: case PickupType.kXP3: case PickupType.kXP4:
                case PickupType.kGold: case PickupType.kGoldSmall: case PickupType.kWheat: case PickupType.kWood: case PickupType.kStone:
                    return false;
            }
            return true;
        }

        internal static void Update()
        {
            if (!Battle.Playing) { if (Seen.Count > 0 && !Battle.InBattle) Seen.Clear(); return; }
            float now = Time.unscaledTime;
            if (now < _nextScan) return;
            _nextScan = now + 0.2f;
            try
            {
                Collect();
                Announce();
                if (Config.PickupCues > 0) Beacon(now);
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Pickups: " + ex);
            }
        }

        private static void Collect()
        {
            Items.Clear();
            var save = BattleSaveData.I;
            var list = save != null ? save.Pickups : null;
            if (list == null) return;
            float reach = 0.6f;
            try { reach = Math.Max(0.3f, Player.I.GetPickupRange()); } catch { }
            _reach = reach;
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (p == null) continue;
                var obj = p.Obj;
                if (obj == null || !obj.isActiveAndEnabled) continue;
                if (obj.CurState == PickupObjState.kPickingUp) continue;
                Vector3 pos = obj.transform.position;
                Items.Add(new Item
                {
                    Inst = p, Pos = pos, Special = IsSpecial(p),
                    Reachable = pos.y <= Battle.MaxY + reach && pos.y >= Battle.MinY - reach,
                    Dist = Vector2.Distance(pos, Battle.PlayerPos),
                });
            }
            Items.Sort((a, b) => a.Dist.CompareTo(b.Dist));
        }

        private static void Announce()
        {
            Now.Clear();
            foreach (var it in Items)
            {
                if (!it.Special) continue;
                Now.Add(it.Inst.Pointer);
                if (!Seen.Add(it.Inst.Pointer)) continue;
                Speech.Battle(Loc.F("pickup_dropped", Names.Pickup(it.Inst.Type), Battle.Where(it.Pos)), "pickup", 3f);
            }
            Seen.IntersectWith(Now);
        }

        private static void Beacon(float now)
        {
            Item special = null, plain = BestPlain();
            foreach (var it in Items)
                if (it.Reachable && it.Special) { special = it; break; }
            if (special != null && now >= _nextSpecial && !(Config.DangerRadar && now < Battle.RadarBusyUntil))
            {
                _nextSpecial = now + Mathf.Clamp(0.35f + special.Dist * 0.12f, 0.4f, 1.2f);
                Cues.Play("treasure" + Height(special.Dist), Battle.PanFor(special.Pos.x), 0.85f, Battle.Muffle(special.Pos.y));
                // Keep the two beacons from landing on top of each other.
                if (_nextPlain < now + 0.2f) _nextPlain = now + 0.2f;
            }
            // One thing to walk to at a time: while a ball is coming its hum guides the hero, the crystal tink
            // waits (telemetry: tinks and ball cues overlapped constantly).
            bool ballGuides = Battle.BallComing && Config.BallCues;
            // Nor on top of an enemy radar sweep (run 3: 126 tinks within 80 ms of a drum).
            bool radar = Config.DangerRadar && (now < Battle.RadarBusyUntil || Battle.NextSweep - now < 0.12f);
            if (radar && _nextPlain <= now) _nextPlain = Math.Max(Battle.RadarBusyUntil, now + 0.05f);
            if (plain != null && Config.PickupCues >= 2 && now >= _nextPlain && !ballGuides && !radar)
            {
                float gap = Mathf.Clamp(0.5f + plain.Dist * 0.15f, 0.6f, 1.6f);
                _nextPlain = now + (special != null ? gap * 1.6f : gap);
                Cues.Play("pickup" + Height(plain.Dist), Battle.PanFor(plain.Pos.x), special != null ? 0.45f : 0.6f, Battle.Muffle(plain.Pos.y));
            }
        }

        /// <summary>
        /// The ordinary pickup worth walking to: the nearest one, unless a bigger heap lies a little further
        /// (each other item within 1 unit of it counts as 0.7 units less walking). Telemetry (2026-11-15): about
        /// 16 crystals lay on the field at any time, mostly 1 to 4 rows ahead of the hero, in heaps where enemies
        /// died; heading for the heap collects several in one walk.
        /// </summary>
        private static Item BestPlain()
        {
            Item best = null, last = null;
            float bestScore = float.MaxValue, lastScore = float.MaxValue;
            foreach (var it in Items)
            {
                if (!it.Reachable || it.Special) continue;
                int heap = 0;
                foreach (var o in Items)
                    if (o != it && !o.Special && Math.Abs(o.Pos.x - it.Pos.x) < 1f && Math.Abs(o.Pos.y - it.Pos.y) < 1f) heap++;
                float score = it.Dist - 0.7f * Math.Min(heap, 5);
                if (score < bestScore) { bestScore = score; best = it; }
                if (it.Inst.Pointer == _plainTarget) { last = it; lastScore = score; }
            }
            // Keep the one being walked to unless another is clearly better, or the beacon would flip between
            // two heaps as the hero moves.
            if (last != null && lastScore <= bestScore + 0.6f) best = last;
            _plainTarget = best != null ? best.Inst.Pointer : IntPtr.Zero;
            return best;
        }

        /// <summary>Pitch step, higher = closer (the same rule as the enemy radar): 2 = on the hero's row,
        /// 1 = one or two rows ahead or behind, 0 = further. Behind is told by the muffle, not the pitch.</summary>
        private static int Height(float dist)
        {
            float rows = dist / Battle.RowH;
            return rows < 0.8f ? 2 : rows < 2.5f ? 1 : 0;
        }

        /// <summary>The pickup the catch assist walks to: the nearest special one the hero can reach, else the
        /// ordinary one BestPlain picks (from the last scan, at most 0.2 s old).</summary>
        internal static bool Nearest(out Vector3 pos)
        {
            Item special = null;
            foreach (var it in Items)
            {
                if (!it.Reachable || !it.Special) continue;
                try { if (it.Inst.Obj == null || it.Inst.Obj.CurState == PickupObjState.kPickingUp) continue; } catch { continue; }
                special = it; break;
            }
            var t = special ?? BestPlain();
            pos = t != null ? t.Pos : Vector3.zero;
            return t != null;
        }

        /// <summary>The nearest health item the hero can walk to (from the last scan).</summary>
        internal static bool NearestHealth(out Vector3 pos)
        {
            foreach (var it in Items)
            {
                if (!it.Reachable || it.Inst.Type != PickupType.kHealth) continue;
                try { if (it.Inst.Obj == null || it.Inst.Obj.CurState == PickupObjState.kPickingUp) continue; } catch { continue; }
                pos = it.Pos;
                return true;
            }
            pos = Vector3.zero;
            return false;
        }

        internal static void Say()
        {
            if (!Battle.InBattle) { Speech.SayMenu(Loc.Get("not_in_battle")); return; }
            try { Collect(); } catch { }
            if (Items.Count == 0) { Speech.SayMenu(Loc.Get("pickups_none")); Buffers.SetDetails(null); return; }
            var lines = new List<string>();
            int plain = 0, plainReach = 0;
            Item nearPlain = null;
            foreach (var it in Items)
            {
                if (it.Special)
                    lines.Add(Names.Pickup(it.Inst.Type) + ", " + Battle.Where(it.Pos) + (it.Reachable ? "" : ", " + Loc.Get("out_of_reach")));
                else
                {
                    plain++;
                    if (it.Reachable) { plainReach++; if (nearPlain == null) nearPlain = it; }
                }
            }
            if (plain > 0)
                lines.Add(Loc.F("pickups_plain", plain, plainReach) + (nearPlain != null ? ". " + Loc.F("pickups_nearest", Battle.Where(nearPlain.Pos)) : ""));
            Speech.SayMenu(string.Join(". ", lines.GetRange(0, Math.Min(3, lines.Count))));
            Buffers.SetDetails(lines);
        }

        internal static string Dump()
        {
            var sb = new StringBuilder();
            foreach (var it in Items)
                sb.AppendLine($"pickup {it.Inst.Type} pos {it.Pos} special {it.Special} reachable {it.Reachable} dist {it.Dist:0.00} state {it.Inst.Obj.CurState} life {it.Inst.RemainingLifetime:0.0}");
            return sb.ToString();
        }
    }
}

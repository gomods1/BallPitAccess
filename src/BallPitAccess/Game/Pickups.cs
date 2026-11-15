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
    /// - Special pickups (fuser, blueprint, egg, gear, health, magnet, bombs, gem bags) are announced when
    ///   they drop, and the nearest one gets a kalimba beacon.
    /// - Ordinary ones (XP crystals, gold, resources) do not collect themselves: the nearest one the hero can
    ///   walk to gets a glass tink on its own timer (quieter and slower while a special one is beaconed).
    /// Both beacons: pan = the side to walk to (centred = straight ahead or back), pitch = how many rows away,
    /// muffled = behind the hero. A pickup beyond the hero's walking area is left to the balls.
    /// </summary>
    internal static class Pickups
    {
        private static readonly HashSet<IntPtr> Seen = new HashSet<IntPtr>();
        private static readonly HashSet<IntPtr> Now = new HashSet<IntPtr>();
        private static float _nextScan, _nextSpecial, _nextPlain, _reach = 0.6f;
        private static int _errors;

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
            Item special = null, plain = null;
            foreach (var it in Items)
            {
                if (!it.Reachable) continue;
                if (it.Special) { if (special == null) special = it; }
                else if (plain == null) plain = it;
                if (special != null) break;
            }
            // Walking straight up or down collects anything within the pickup range of the hero's centre.
            float dead = Math.Max(_reach, Player.kColHalfWidth);
            if (special != null && now >= _nextSpecial)
            {
                _nextSpecial = now + Mathf.Clamp(0.35f + special.Dist * 0.12f, 0.4f, 1.2f);
                Cues.Play("treasure" + Height(special.Pos), Battle.GuidePan(special.Pos.x, dead), 0.85f, Battle.Muffle(special.Pos.y));
                // Keep the two beacons from landing on top of each other.
                if (_nextPlain < now + 0.2f) _nextPlain = now + 0.2f;
            }
            if (plain != null && Config.PickupCues >= 2 && now >= _nextPlain)
            {
                float gap = Mathf.Clamp(0.5f + plain.Dist * 0.15f, 0.6f, 1.6f);
                _nextPlain = now + (special != null ? gap * 1.6f : gap);
                Cues.Play("pickup" + Height(plain.Pos), Battle.GuidePan(plain.Pos.x, dead), special != null ? 0.45f : 0.6f, Battle.Muffle(plain.Pos.y));
            }
        }

        /// <summary>Pitch step, higher = closer (the same rule as the enemy radar): 2 = on the hero's row,
        /// 1 = one or two rows ahead or behind, 0 = further. Behind is told by the muffle, not the pitch.</summary>
        private static int Height(Vector3 pos)
        {
            float rows = Math.Abs(pos.y - Battle.PlayerPos.y) / Battle.RowH;
            return rows < 0.8f ? 2 : rows < 2.5f ? 1 : 0;
        }

        /// <summary>The pickup the catch assist walks to: the nearest special one the hero can reach, else the
        /// nearest ordinary one (from the last scan, at most 0.2 s old).</summary>
        internal static bool Nearest(out Vector3 pos)
        {
            Item special = null, plain = null;
            foreach (var it in Items)
            {
                if (!it.Reachable) continue;
                try { if (it.Inst.Obj == null || it.Inst.Obj.CurState == PickupObjState.kPickingUp) continue; } catch { continue; }
                if (it.Special) { special = it; break; }
                if (plain == null) plain = it;
            }
            var t = special ?? plain;
            pos = t != null ? t.Pos : Vector3.zero;
            return t != null;
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

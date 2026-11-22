using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Whether a building put on some tiles would get in the workers' way (a player rearranged buildings again and
    /// again to clear a path to the Haunted House they were building). Workers start at the hero and fly in a straight
    /// line, bouncing off walls, until they hit a building (see Harvest): a building is reached at an angle when it is
    /// the first one that line hits, or, for fields workers pass over, when the line crosses it before that.
    /// One sweep of rays (5 to 175 degrees, half a degree apart) is traced through the game's physics without the
    /// building being placed or moved; the new tiles are then tested against each ray's path as boxes, with plain
    /// arithmetic, so many spots can be compared at once (the Home search).
    /// </summary>
    internal static class Paths
    {
        private const float From = 5f, To = 175f, Step = 0.5f;

        private struct Cross { public BuildingInst B; public int Seg; public float T; }

        private sealed class RayPath
        {
            public readonly List<Vector2> Pts = new List<Vector2>();   // segment i = Pts[i] → Pts[i + 1]
            public BuildingInst First;                                  // the building it bounces off (end of the last segment)
            public readonly List<Cross> Crossed = new List<Cross>();
        }

        private static List<RayPath> _sweep;
        private static int _sweepFrame = -1;
        private static IntPtr _sweepIgnore;

        /// <summary>A building's reach: how many angles reach it straight and off a wall.</summary>
        internal struct Reach { public int Direct, Wall; public int Total => Direct + Wall; }

        internal sealed class Result
        {
            public Reach Self;
            public readonly List<KeyValuePair<BuildingInst, Reach>> Before = new List<KeyValuePair<BuildingInst, Reach>>();
            public readonly List<KeyValuePair<BuildingInst, Reach>> After = new List<KeyValuePair<BuildingInst, Reach>>();
            /// <summary>Buildings workers reached before and would not reach at all.</summary>
            public readonly List<BuildingInst> CutOff = new List<BuildingInst>();
            /// <summary>Buildings workers reached straight before and now only off a wall.</summary>
            public readonly List<BuildingInst> WallOnly = new List<BuildingInst>();
            /// <summary>Angles lost by the other buildings, all together (for comparing spots).</summary>
            public int Lost;
        }

        /// <summary>
        /// The sweep without `ignore` (the building picked up, or nothing) and without the placement preview: once per
        /// frame at most. The preview's and the moving building's colliders are switched off while tracing.
        /// </summary>
        private static List<RayPath> Sweep(BuildingObj ignore)
        {
            IntPtr ig = ignore != null ? ignore.Pointer : IntPtr.Zero;
            if (_sweep != null && _sweepFrame == Time.frameCount && _sweepIgnore == ig) return _sweep;
            var off = new List<Collider2D>();
            try
            {
                var g = BaseGridMgr.I;
                var objs = new List<BuildingObj>();
                if (ignore != null) objs.Add(ignore);
                if (g != null)
                {
                    try { var p = g.GetPlacePreview(); if (p != null) objs.Add(p); } catch { }
                    try { var p = g._pendingRearrangePlaceholder; if (p != null) objs.Add(p); } catch { }
                    try { var p = g.GetPendingRearrangeBuilding(); if (p != null) objs.Add(p); } catch { }
                }
                foreach (var o in objs)
                    foreach (var c in Footprint.Colliders(o))
                        if (c.enabled) { c.enabled = false; off.Add(c); }
                var list = new List<RayPath>();
                Vector2 start = Harvest.AimOrigin();
                for (float a = From; a <= To + 0.001f; a += Step) list.Add(Trace(start, Harvest.DirOf(a), ignore != null ? ignore.Inst : null));
                _sweep = list;
                _sweepFrame = Time.frameCount;
                _sweepIgnore = ig;
                return list;
            }
            finally { foreach (var c in off) c.enabled = true; }
        }

        private static RayPath Trace(Vector2 start, Vector2 dir, BuildingInst ignore)
        {
            var r = new RayPath();
            int mask, pickMask, piece;
            try { mask = ColMgr.kLayerMaskBounce; pickMask = ColMgr.kLayerMaskPickup; piece = ColMgr.I != null ? ColMgr.I.GetGameLayer(GameLayerType.kPiece) : -1; }
            catch { return r; }
            Vector2 pos = start, d = dir.normalized;
            r.Pts.Add(pos);
            for (int i = 0; i < 5; i++)
            {
                RaycastHit2D hit;
                try { hit = Physics2D.Raycast(pos, d, 80f, mask); } catch { return r; }
                var col = hit.collider;
                float len = col != null ? hit.distance : 80f;
                Vector2 end = pos + d * len;
                foreach (var pb in Harvest.PickupBoxes(pickMask))
                {
                    if (ignore != null && (pb.Key.Pointer == ignore.Pointer || pb.Key.Id == ignore.Id)) continue;
                    float t = Harvest.Enter(pos, end, pb.Value, Harvest._pickupR);
                    if (t < 0f) continue;
                    bool dup = false;
                    foreach (var c in r.Crossed) if (c.B.Pointer == pb.Key.Pointer) dup = true;
                    if (!dup) r.Crossed.Add(new Cross { B = pb.Key, Seg = i, T = t });
                }
                r.Pts.Add(end);
                if (col == null) return r;
                if (col.gameObject.layer == piece) { r.First = Harvest.BuildingOf(col); return r; }
                Vector2 n = hit.normal;
                d = Vector2.Reflect(d, n).normalized;
                pos = hit.point + n * 0.02f;
            }
            return r;
        }

        /// <summary>Where a ray's path first enters any of the boxes: segment and fraction, or false.</summary>
        private static bool FirstEntry(RayPath r, List<Bounds> boxes, out int seg, out float t)
        {
            seg = -1; t = 0f;
            for (int i = 0; i + 1 < r.Pts.Count; i++)
            {
                float best = 2f;
                foreach (var b in boxes)
                {
                    float e = Harvest.Enter(r.Pts[i], r.Pts[i + 1], b, 0f);
                    if (e >= 0f && e < best) best = e;
                }
                if (best <= 1f) { seg = i; t = best; return true; }
            }
            return false;
        }

        private static bool Before(int segA, float tA, int segB, float tB) => segA < segB || (segA == segB && tA < tB);

        /// <summary>The boxes of a set of tiles (each tile a full grid square).</summary>
        internal static List<Bounds> Boxes(List<Vector2Int> cells)
        {
            var list = new List<Bounds>();
            float w = BaseGridMgr.kSpaceWidth > 0.01f ? BaseGridMgr.kSpaceWidth : 1.125f;
            foreach (var c in cells) { var m = BaseGrid.Center(c.x, c.y); list.Add(new Bounds(new Vector3(m.x, m.y, 0f), new Vector3(w * 0.98f, w * 0.98f, 1f))); }
            return list;
        }

        /// <summary>
        /// Compare the workers' reach with and without a building on `cells`. `moving` is the building picked up in
        /// rearrange mode (left out of both). `passOver` is true for a field workers fly over (it blocks nothing).
        /// </summary>
        internal static Result Analyse(List<Vector2Int> cells, BuildingObj moving, bool passOver)
        {
            var sweep = Sweep(moving);
            var boxes = Boxes(cells);
            var res = new Result();
            var before = new Dictionary<IntPtr, Reach>();
            var after = new Dictionary<IntPtr, Reach>();
            var inst = new Dictionary<IntPtr, BuildingInst>();
            foreach (var r in sweep)
            {
                bool hitsNew = FirstEntry(r, boxes, out int seg, out float t);
                // Before: the first building, and fields crossed before it.
                void Count(Dictionary<IntPtr, Reach> map, BuildingInst b, bool direct)
                {
                    if (b == null) return;
                    inst[b.Pointer] = b;
                    map.TryGetValue(b.Pointer, out var v);
                    if (direct) v.Direct++; else v.Wall++;
                    map[b.Pointer] = v;
                }
                int lastSeg = r.Pts.Count - 2;
                Count(before, r.First, lastSeg == 0);
                foreach (var c in r.Crossed) Count(before, c.B, c.Seg == 0);
                if (!hitsNew || passOver)
                {
                    Count(after, r.First, lastSeg == 0);
                    foreach (var c in r.Crossed) Count(after, c.B, c.Seg == 0);
                    if (hitsNew) { if (seg == 0) res.Self.Direct++; else res.Self.Wall++; }
                }
                else
                {
                    // The new building stops the worker: what lay further along is not reached on this ray.
                    if (seg == 0) res.Self.Direct++; else res.Self.Wall++;
                    foreach (var c in r.Crossed) if (Before(c.Seg, c.T, seg, t)) Count(after, c.B, c.Seg == 0);
                }
            }
            foreach (var kv in before)
            {
                after.TryGetValue(kv.Key, out var a);
                var b = inst[kv.Key];
                res.Before.Add(new KeyValuePair<BuildingInst, Reach>(b, kv.Value));
                res.After.Add(new KeyValuePair<BuildingInst, Reach>(b, a));
                res.Lost += Math.Max(0, kv.Value.Total - a.Total);
                if (a.Total == 0) res.CutOff.Add(b);
                else if (kv.Value.Direct > 0 && a.Direct == 0) res.WallOnly.Add(b);
            }
            return res;
        }

        /// <summary>Dev: the launch point, the tiles' centres and the ray aimed at their middle.</summary>
        internal static string Debug(List<Vector2Int> cells)
        {
            var sb = new System.Text.StringBuilder();
            Vector2 start = Harvest.AimOrigin();
            Vector2 mid = Vector2.zero;
            foreach (var c in cells) mid += BaseGrid.Center(c.x, c.y);
            mid /= Math.Max(1, cells.Count);
            float deg = Mathf.Atan2(mid.y - start.y, mid.x - start.x) * Mathf.Rad2Deg;
            sb.Append("Paths debug: start ").Append(start);
            try { var ws = BaseMgr.I.WorkerStartPos; sb.Append(" worker starts"); for (int k = 0; ws != null && k < ws.Count; k++) sb.Append(' ').Append(ws[k]); } catch { }
            try { sb.Append(" hero ").Append(BaseMgr.I.BPlayer.transform.position).Append(" active ").Append(BaseMgr.I.BPlayer.gameObject.activeInHierarchy); } catch { }
            sb.Append(" tiles");
            foreach (var c in cells) sb.Append(' ').Append(BaseGrid.Center(c.x, c.y).ToString("0.00"));
            sb.Append(" angle to them ").Append(deg.ToString("0.0"));
            var sweep = _sweep;
            if (sweep != null)
            {
                int i = Mathf.Clamp(Mathf.RoundToInt((deg - From) / Step), 0, sweep.Count - 1);
                var r = sweep[i];
                sb.Append(" | ray ").Append((From + i * Step).ToString("0.0")).Append(':');
                foreach (var p in r.Pts) sb.Append(' ').Append(p.ToString("0.00"));
                sb.Append(" first ").Append(r.First != null ? r.First.Type.ToString() : "none");
                bool e = FirstEntry(r, Boxes(cells), out int seg, out float t);
                sb.Append(" enters tiles ").Append(e ? "seg " + seg + " t " + t.ToString("0.00") : "no");
            }
            return sb.ToString();
        }

        /// <summary>Workers fly over this kind of building (fields): from a built one of the same type, else false.</summary>
        internal static bool PassOverType(BuildingType t)
        {
            try
            {
                var bm = BuildingMgr.I;
                if (bm != null && bm.BuildingsById != null)
                    foreach (var kv in bm.BuildingsById)
                        if (kv.Value != null && kv.Value.Type == t && kv.Value.Obj != null) return Harvest.PassOver(kv.Value);
            }
            catch { }
            return t == BuildingType.kWheatField || t == BuildingType.kDenseWheat;
        }

        /// <summary>The words for a result: how workers reach the new spot, then what it would cut off.</summary>
        internal static string Words(Result r, bool passOver)
        {
            var parts = new List<string>();
            if (r.Self.Direct > 0) parts.Add(Loc.F("paths_self_direct", Degrees(r.Self.Direct)));
            else if (r.Self.Wall > 0) parts.Add(Loc.Get("paths_self_wall"));
            else parts.Add(Loc.Get("paths_self_none"));
            if (passOver) { parts.Add(Loc.Get("paths_passover")); return Sentences(parts); }
            if (r.CutOff.Count > 0) parts.Add(Loc.F("paths_cutoff", Names(r.CutOff)));
            if (r.WallOnly.Count > 0) parts.Add(Loc.F("paths_wallonly", Names(r.WallOnly)));
            if (r.CutOff.Count == 0 && r.WallOnly.Count == 0) parts.Add(Loc.Get("paths_clear"));
            return Sentences(parts);
        }

        private static string Sentences(List<string> parts)
        {
            for (int i = 0; i < parts.Count; i++)
                if (parts[i].Length > 0) parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
            return string.Join(". ", parts);
        }

        private static string Degrees(int rays) => Mathf.RoundToInt(rays * Step).ToString();

        private static string Names(List<BuildingInst> list)
        {
            var names = new List<string>();
            foreach (var b in list)
            {
                string n = Harvest.PlainName(b);
                if (Harvest.UnderConstruction(b)) n += " (" + Loc.Get("paths_building") + ")";
                if (!names.Contains(n)) names.Add(n);
            }
            return string.Join(", ", names);
        }

        /// <summary>
        /// A score for the Home search: a spot workers reach straight (they must hit a new building to build it),
        /// cutting nothing off, losing few angles for the others, then nearer the starting point.
        /// </summary>
        internal static float Score(Result r)
        {
            float s = 0f;
            s -= r.CutOff.Count * 10000f;
            s -= r.WallOnly.Count * 1000f;
            s -= r.Lost * 2f;
            s += r.Self.Direct > 0 ? 500f + Math.Min(r.Self.Direct, 40) * 10f : r.Self.Wall > 0 ? 100f : -3000f;
            return s;
        }
    }
}

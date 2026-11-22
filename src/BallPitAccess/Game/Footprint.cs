using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// The tiles a building being placed or moved covers, read from the game's own collider: some buildings are not
    /// rectangles (the Haunted House is three tiles of a 2 by 2 square, an L), and the game's rotate key turns that
    /// shape, so a spot that fits one way is blocked the other (measured 2026-11-22). The save's chunk grid marks the
    /// whole box of a placed building, so the shape can only be read from the collider: each tile centre near the hover
    /// point is tested with Collider2D.OverlapPoint (present in the build; CircleCastAll is not).
    /// Shapes are remembered per building type and rotation, relative to the hover tile, so the Home search can try
    /// every rotation without turning the preview for each spot.
    /// </summary>
    internal static class Footprint
    {
        private static readonly Dictionary<long, List<Vector2Int>> Learned = new Dictionary<long, List<Vector2Int>>();

        private static long Key(BuildingType t, int rot) => ((long)(int)t << 8) | (uint)(rot & 3);

        internal static bool Known(BuildingType t, int rot) => Learned.ContainsKey(Key(t, rot));

        /// <summary>The building's own colliders (the box, circle or polygon one it uses).</summary>
        internal static List<Collider2D> Colliders(BuildingObj o)
        {
            var list = new List<Collider2D>();
            if (o == null) return list;
            void Add(Collider2D c)
            {
                if (c == null) return;
                foreach (var x in list) if (x.Pointer == c.Pointer) return;
                list.Add(c);
            }
            try { Add(o.Col); } catch { }
            try { Add(o.PolyCol); } catch { }
            try { Add(o.BoxCol); } catch { }
            try { Add(o.CircleCol); } catch { }
            return list;
        }

        /// <summary>
        /// Read the shape of a preview (or a building picked up in rearrange mode) where it stands now. Null while it
        /// is still sliding or turning, or when its collider says nothing (then the caller uses the listed box).
        /// </summary>
        internal static List<Vector2Int> Measure(BuildingObj o)
        {
            if (o == null || o.Inst == null) return null;
            try { if (o.IsMoving() || o.IsRotating()) return null; } catch { }
            var g = BaseGridMgr.I;
            if (g == null || !BaseGrid.Refresh()) return null;
            var hover = Anchor(o);
            var inf = o.Inst.GetInfo();
            var size = inf.TileSize;
            int r = Math.Max(size.x, size.y) + 1;
            // The game tests the fit with colliders it makes for the test from the building's polygon (BuildingInfo
            // PolyColPoints, CanBuildPlacePreview → ColMgr.GetAvailColAlloc); the preview's own colliders are switched
            // off and parked elsewhere. So the polygon is put where the preview stands, turned as it is turned.
            var poly = WorldPolygon(o, inf);
            if (poly == null) return null;
            var cells = new List<Vector2Int>();
            // Five points per tile (centre and four at a quarter tile from it); a tile counts when three are inside:
            // a polygon a little smaller than its tiles still covers them, one reaching a little over a neighbour's
            // tile does not take it.
            float q = 0.25f * (BaseGridMgr.kSpaceWidth > 0.01f ? BaseGridMgr.kSpaceWidth : 1.125f);
            var offs = new[] { Vector2.zero, new Vector2(q, q), new Vector2(-q, q), new Vector2(q, -q), new Vector2(-q, -q) };
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    var cell = new Vector2Int(hover.x + dx, hover.y + dy);
                    Vector2 p = BaseGrid.Center(cell.x, cell.y);
                    int inside = 0;
                    foreach (var off in offs) if (Inside(poly, p + off)) inside++;
                    if (inside >= 3) cells.Add(cell);
                }
            if (Config.Debug)
            {
                string ps = "";
                foreach (var v in poly) ps += " " + v.ToString("0.00");
                Log.Info($"Footprint measure {o.Inst.Type} rot {o.Inst.Rotation}: hover {g.GetHoverPos()} cell {hover} centre {BaseGrid.Center(hover.x, hover.y)}; polygon{ps}");
            }
            if (cells.Count == 0) return null;
            var rel = new List<Vector2Int>();
            foreach (var c in cells) rel.Add(c - hover);
            int rot = o.Inst.Rotation;
            Learned[Key(o.Inst.Type, rot)] = rel;
            if (Config.Debug) Log.Info($"Footprint {o.Inst.Type} rotation {rot}: {cells.Count} tiles, {Describe(rel)} [{string.Join(" ", rel)}]");
            return cells;
        }

        /// <summary>
        /// The building's collider polygon in the world, where the preview (or the picked-up building) stands and as it
        /// is turned: the info's points through the collider wrapper's transform (the object's own transform if it has
        /// none). A box or circle building gets the box of its listed size around the same centre. Null when unknown.
        /// </summary>
        internal static List<Vector2> WorldPolygon(BuildingObj o, BuildingInfo inf)
        {
            try
            {
                Transform t = o.ColWrapper != null ? o.ColWrapper : o.transform;
                var pts = inf.PolyColPointsScaled;
                if (pts == null || pts.Length < 3) pts = inf.PolyColPoints;
                if (Config.Debug)
                {
                    string a = "", b = "";
                    try { var p1 = inf.PolyColPoints; for (int i = 0; p1 != null && i < p1.Length; i++) a += " " + p1[i].ToString("0.00"); } catch { }
                    try { var p2 = inf.PolyColPointsScaled; for (int i = 0; p2 != null && i < p2.Length; i++) b += " " + p2[i].ToString("0.00"); } catch { }
                    Log.Info($"Polygon of {inf.Type} ({inf.ColType}): points{a} | scaled{b} | obj {o.transform.position} rot {o.transform.eulerAngles.z:0} | colwrapper {(o.ColWrapper != null ? o.ColWrapper.position + " rot " + o.ColWrapper.eulerAngles.z.ToString("0") + " scale " + o.ColWrapper.lossyScale : "none")}");
                }
                var list = new List<Vector2>();
                if (inf.ColType == ColliderType.kPoly && pts != null && pts.Length >= 3)
                {
                    // The points start at the box's corner; the collider is centred on the hover point (a placed
                    // Haunted House's collider ran from hover - 1.125 to hover + 1.125).
                    float x0 = float.MaxValue, x1 = float.MinValue, y0 = float.MaxValue, y1 = float.MinValue;
                    for (int i = 0; i < pts.Length; i++) { x0 = Math.Min(x0, pts[i].x); x1 = Math.Max(x1, pts[i].x); y0 = Math.Min(y0, pts[i].y); y1 = Math.Max(y1, pts[i].y); }
                    float cx = (x0 + x1) / 2f, cy = (y0 + y1) / 2f;
                    for (int i = 0; i < pts.Length; i++) list.Add(t.TransformPoint(new Vector3(pts[i].x - cx, pts[i].y - cy, 0f)));
                    return list;
                }
                return null;
            }
            catch (Exception ex) { Log.Warn("Footprint polygon: " + ex.Message); return null; }
        }

        /// <summary>
        /// "open corner top left" for a placed building that is not a rectangle, from its polygon where it stands and
        /// the tiles its box covers in the chunk grid; "" for a rectangle or when unknown.
        /// </summary>
        internal static string PlacedShape(BuildingInst b)
        {
            try
            {
                var inf = b.GetInfo();
                if (b.Obj == null || inf == null || inf.ColType != ColliderType.kPoly) return "";
                if (!BaseGrid.Tiles(b, out int x0, out int x1, out int y0, out int y1)) return "";
                var poly = WorldPolygon(b.Obj, inf);
                if (poly == null) return "";
                float q = 0.25f * (BaseGridMgr.kSpaceWidth > 0.01f ? BaseGridMgr.kSpaceWidth : 1.125f);
                var offs = new[] { Vector2.zero, new Vector2(q, q), new Vector2(-q, q), new Vector2(q, -q), new Vector2(-q, -q) };
                var cells = new List<Vector2Int>();
                for (int x = x0; x <= x1; x++)
                    for (int y = y0; y <= y1; y++)
                    {
                        Vector2 p = BaseGrid.Center(x, y);
                        int inside = 0;
                        foreach (var off in offs) if (Inside(poly, p + off)) inside++;
                        if (inside >= 3) cells.Add(new Vector2Int(x, y));
                    }
                return cells.Count > 0 && cells.Count < (x1 - x0 + 1) * (y1 - y0 + 1) ? Describe(cells) : "";
            }
            catch { return ""; }
        }

        /// <summary>Point in polygon (even-odd rule).</summary>
        private static bool Inside(List<Vector2> poly, Vector2 p)
        {
            bool c = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                var a = poly[i]; var b = poly[j];
                if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x) c = !c;
            }
            return c;
        }

        /// <summary>The hover tile of a preview where the game has put it (its own position, snapped to the grid).</summary>
        internal static Vector2Int Anchor(BuildingObj o)
        {
            try { return BaseGrid.HoverCell(o.transform.position); }
            catch { return BaseGrid.HoverCell(BaseGridMgr.I.GetHoverPos()); }
        }

        /// <summary>The tiles at this hover tile for a learned shape; null when the shape at that rotation is not known.</summary>
        internal static List<Vector2Int> At(BuildingType t, int rot, Vector2Int hover)
        {
            if (!Learned.TryGetValue(Key(t, rot), out var rel)) return null;
            var list = new List<Vector2Int>(rel.Count);
            foreach (var d in rel) list.Add(hover + d);
            return list;
        }

        /// <summary>The listed box of a building at this hover tile (the anchor placement measured for rectangles).</summary>
        internal static List<Vector2Int> Box(BuildingInfo inf, Vector2Int hover, int rot = 0)
        {
            var size = inf.TileSize;
            // Turned a quarter, a rectangle swaps its sides (BuildingInst.GetTileSize does the same, and the game's
            // snap follows it).
            if ((rot & 1) == 1) size = new Vector2Int(size.y, size.x);
            var tl = BaseGrid.PreviewTopLeft(hover, size);
            var list = new List<Vector2Int>();
            for (int i = 0; i < Math.Max(1, size.x); i++)
                for (int j = 0; j < Math.Max(1, size.y); j++)
                    list.Add(new Vector2Int(tl.x + i, tl.y + j * BaseGrid.Down));
            return list;
        }

        /// <summary>Left, right, bottom, top of a set of tiles.</summary>
        internal static void Bounds(List<Vector2Int> cells, out int x0, out int x1, out int y0, out int y1)
        {
            x0 = y0 = int.MaxValue; x1 = y1 = int.MinValue;
            foreach (var c in cells) { x0 = Math.Min(x0, c.x); x1 = Math.Max(x1, c.x); y0 = Math.Min(y0, c.y); y1 = Math.Max(y1, c.y); }
        }

        /// <summary>The top-left tile of the box around these tiles (the column and row placement says).</summary>
        internal static Vector2Int TopLeft(List<Vector2Int> cells)
        {
            Bounds(cells, out int x0, out _, out int y0, out int y1);
            return new Vector2Int(x0, BaseGrid.Down < 0 ? y1 : y0);
        }

        /// <summary>
        /// "" for a full rectangle; otherwise which part of its box the shape leaves open: "open corner top left" for
        /// three tiles of a square, else the tiles of each row ("top row: columns 2 to 3; bottom row: 1 to 3").
        /// Rows and columns are counted inside the building's own box, top row first.
        /// </summary>
        internal static string Describe(List<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0) return "";
            Bounds(cells, out int x0, out int x1, out int y0, out int y1);
            int w = x1 - x0 + 1, h = y1 - y0 + 1;
            if (cells.Count == w * h) return "";
            var set = new HashSet<long>();
            foreach (var c in cells) set.Add(((long)c.x << 32) | (uint)c.y);
            bool Has(int x, int y) => set.Contains(((long)x << 32) | (uint)y);
            int topY = BaseGrid.Down < 0 ? y1 : y0, botY = BaseGrid.Down < 0 ? y0 : y1;
            if (cells.Count == w * h - 1)
            {
                if (!Has(x0, topY)) return Loc.Get("shape_open_tl");
                if (!Has(x1, topY)) return Loc.Get("shape_open_tr");
                if (!Has(x0, botY)) return Loc.Get("shape_open_bl");
                if (!Has(x1, botY)) return Loc.Get("shape_open_br");
            }
            var rows = new List<string>();
            for (int k = 0; k < h; k++)
            {
                int y = topY - k * (BaseGrid.Down < 0 ? 1 : -1);
                var runs = new List<string>();
                int start = -1;
                for (int x = x0; x <= x1 + 1; x++)
                {
                    bool on = x <= x1 && Has(x, y);
                    if (on && start < 0) start = x;
                    if (!on && start >= 0)
                    {
                        int a = start - x0 + 1, b = x - 1 - x0 + 1;
                        runs.Add(a == b ? a.ToString() : Loc.F("shape_span", a, b));
                        start = -1;
                    }
                }
                rows.Add(Loc.F("shape_row", k + 1, runs.Count > 0 ? string.Join(", ", runs) : Loc.Get("shape_empty")));
            }
            return string.Join("; ", rows);
        }

        private static readonly Dictionary<long, BuildingInst> Occ = new Dictionary<long, BuildingInst>();
        private static int _occFrame = -1;
        private static int _occIgnore = int.MinValue;

        /// <summary>
        /// The building on each tile of the land. The save's chunk grid marks a building's whole box, but the game lets
        /// shapes interlock (a Haunted House's open corner sat on a wheat field's tile, and the chunk grid gave that tile
        /// to the house), so a tile belongs to the building whose collider covers its centre; the chunk grid decides only
        /// for rectangular buildings without a collider there. `ignore` (the building being moved) owns nothing.
        /// </summary>
        private static Dictionary<long, BuildingInst> Occupancy(BuildingInst ignore)
        {
            int ig = ignore != null ? ignore.Id : -1;
            if (_occFrame == Time.frameCount && _occIgnore == ig) return Occ;
            _occFrame = Time.frameCount; _occIgnore = ig;
            Occ.Clear();
            var bm = BuildingMgr.I;
            var cols = new List<KeyValuePair<BuildingInst, List<Collider2D>>>();
            if (bm != null && bm.BuildingsById != null)
                foreach (var kv in bm.BuildingsById)
                {
                    var b = kv.Value;
                    if (b == null || b.Obj == null || (ignore != null && b.Id == ignore.Id)) continue;
                    var list = new List<Collider2D>();
                    foreach (var c in Colliders(b.Obj)) { try { if (c.enabled && c.gameObject.activeInHierarchy) list.Add(c); } catch { } }
                    if (list.Count > 0) cols.Add(new KeyValuePair<BuildingInst, List<Collider2D>>(b, list));
                }
            for (int x = BaseGrid.MinX; x <= BaseGrid.MaxX; x++)
                for (int y = BaseGrid.MinY; y <= BaseGrid.MaxY; y++)
                {
                    Vector2 p = BaseGrid.Center(x, y);
                    BuildingInst owner = null;
                    foreach (var kv in cols)
                    {
                        foreach (var c in kv.Value) { bool hit; try { hit = c.OverlapPoint(p); } catch { hit = false; } if (hit) { owner = kv.Key; break; } }
                        if (owner != null) break;
                    }
                    if (owner == null)
                    {
                        var b = BaseGrid.BuildingAt(x, y);
                        if (b != null && (ignore == null || b.Id != ignore.Id))
                        {
                            bool poly = false;
                            try { poly = b.GetInfo().ColType == ColliderType.kPoly && b.Obj != null; } catch { }
                            if (!poly) owner = b;     // a shape's open corner: free unless a collider covers it
                        }
                    }
                    if (owner != null) Occ[((long)x << 32) | (uint)y] = owner;
                }
            return Occ;
        }

        /// <summary>
        /// Why these tiles cannot take the building, as words for after the position ("blocked by Forest", "blocked,
        /// land not bought", "blocked, entrance"); "" when every tile is free. The game's own fit test can still differ
        /// at the edges (it tests the shapes themselves).
        /// </summary>
        internal static string Obstacle(List<Vector2Int> cells, BuildingInst ignore = null)
        {
            var g = BaseGridMgr.I;
            var occ = Occupancy(ignore);
            foreach (var c in cells)
            {
                if (!BaseGrid.Inside(c.x, c.y) || !BaseGrid.Bought(c.x, c.y)) return Loc.Get("place_blocked") + ", " + Loc.Get("explore_locked");
                try { if (g != null && g.IsEntrance(c.x, c.y)) return Loc.Get("place_blocked") + ", " + Loc.Get("explore_entrance"); } catch { }
                if (occ.TryGetValue(((long)c.x << 32) | (uint)c.y, out var b)) return Loc.F("place_blocked_by", Names.Building(b.GetInfo()));
            }
            return "";
        }
    }
}

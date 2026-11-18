using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// One column / row numbering for the base grid, shared by placement, the building list and explore mode:
    /// column 1 is the leftmost tile of the bought land, row 1 its top row. Tiles are the game's grid cells
    /// (BaseGridMgr.GetWorldPos); the bought area is the box around every purchased chunk, so the numbers
    /// shift when a plot is bought on the left or at the top.
    /// </summary>
    internal static class BaseGrid
    {
        private static Vector3 _origin;
        private static float _dx = 1.125f, _dy = 1.125f;
        internal static int MinX, MaxX, MinY, MaxY;

        internal static int Cols => BaseGridMgr.kChunkCols * BaseGridMgr.kChunkWidth;
        internal static int Rows => BaseGridMgr.kChunkRows * BaseGridMgr.kChunkHeight;

        /// <summary>Read the grid's geometry and the bought area again (cheap enough on demand, not per frame).</summary>
        internal static bool Refresh()
        {
            var g = BaseGridMgr.I;
            if (g == null) return false;
            try
            {
                _origin = g.GetWorldPos(0, 0);
                _dx = g.GetWorldPos(1, 0).x - _origin.x;
                _dy = g.GetWorldPos(0, 1).y - _origin.y;
                if (Mathf.Abs(_dx) < 0.01f) _dx = 1.125f;
                if (Mathf.Abs(_dy) < 0.01f) _dy = 1.125f;
                MinX = int.MaxValue; MinY = int.MaxValue; MaxX = int.MinValue; MaxY = int.MinValue;
                int cw = BaseGridMgr.kChunkWidth, ch = BaseGridMgr.kChunkHeight;
                // The save's chunks hold which building stands on each tile (what the game's FindEmptySpots reads).
                Chunks.Clear();
                var arr = MetaSaveData.I != null ? MetaSaveData.I.BaseChunks : null;
                if (arr != null)
                    foreach (var line in arr)
                        if (line != null)
                            foreach (var c in line)
                                if (c != null) Chunks[Key(c.X, c.Y)] = c;
                foreach (var c in Chunks.Values)
                {
                    if (!c.IsPurchased) continue;
                    MinX = Math.Min(MinX, c.X * cw); MaxX = Math.Max(MaxX, c.X * cw + cw - 1);
                    MinY = Math.Min(MinY, c.Y * ch); MaxY = Math.Max(MaxY, c.Y * ch + ch - 1);
                }
                if (MinX == int.MaxValue)
                for (int cx = 0; cx < BaseGridMgr.kChunkCols; cx++)
                    for (int cy = 0; cy < BaseGridMgr.kChunkRows; cy++)
                    {
                        if (!g.IsPurchasedChunk(cx, cy)) continue;
                        var bl = BaseGridMgr.GetChunkBotLeftGridPos(cx, cy);
                        MinX = Math.Min(MinX, bl.x); MaxX = Math.Max(MaxX, bl.x + cw - 1);
                        MinY = Math.Min(MinY, bl.y); MaxY = Math.Max(MaxY, bl.y + ch - 1);
                    }
                if (MinX == int.MaxValue) { MinX = 0; MaxX = Cols - 1; MinY = 0; MaxY = Rows - 1; }
                return true;
            }
            catch (Exception ex) { Log.Warn("BaseGrid: " + ex.Message); return false; }
        }

        internal static Vector3 World(int x, int y) => new Vector3(_origin.x + x * _dx, _origin.y + y * _dy, 0f);

        internal static Vector2Int CellOf(Vector3 world) =>
            new Vector2Int(Mathf.RoundToInt((world.x - _origin.x) / _dx), Mathf.RoundToInt((world.y - _origin.y) / _dy));

        internal static bool Inside(int x, int y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;

        /// <summary>The grid y that is one row further down the screen.</summary>
        internal static int Down => _dy > 0 ? -1 : 1;

        internal static int Col(int x) => x - MinX + 1;
        internal static int Row(int y) => _dy > 0 ? MaxY - y + 1 : y - MinY + 1;
        internal static int NumCols => MaxX - MinX + 1;
        internal static int NumRows => MaxY - MinY + 1;

        private static readonly Dictionary<long, BaseChunkInst> Chunks = new Dictionary<long, BaseChunkInst>();

        private static long Key(int x, int y) => ((long)x << 32) | (uint)y;

        private static int FloorDiv(int a, int b) => a >= 0 ? a / b : -((-a + b - 1) / b);

        private static BaseChunkInst ChunkAt(int x, int y, out int lx, out int ly)
        {
            int cw = BaseGridMgr.kChunkWidth, ch = BaseGridMgr.kChunkHeight;
            int cx = FloorDiv(x, cw), cy = FloorDiv(y, ch);
            lx = x - cx * cw; ly = y - cy * ch;
            return Chunks.TryGetValue(Key(cx, cy), out var c) ? c : null;
        }

        /// <summary>The building on a tile, from the save's chunk grid (null: nothing, or land not bought).</summary>
        internal static BuildingInst BuildingAt(int x, int y)
        {
            try
            {
                var c = ChunkAt(x, y, out int lx, out int ly);
                if (c == null || c.Buildings == null || lx >= c.Buildings.Length) return null;
                var col = c.Buildings[lx];
                return col != null && ly < col.Length ? col[ly] : null;
            }
            catch { return null; }
        }

        /// <summary>Free the way the game counts it (FindEmptySpots): bought land with no building on it.</summary>
        internal static bool Free(int x, int y) => Bought(x, y) && BuildingAt(x, y) == null;

        /// <summary>The tiles a placed building really covers, from the chunk grid: left, right, bottom, top.</summary>
        internal static bool Tiles(BuildingInst b, out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = minY = int.MaxValue; maxX = maxY = int.MinValue;
            if (b == null) return false;
            for (int x = MinX; x <= MaxX; x++)
                for (int y = MinY; y <= MaxY; y++)
                {
                    var o = BuildingAt(x, y);
                    if (o == null || o.Pointer != b.Pointer) continue;
                    minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                }
            return minX != int.MaxValue;
        }

        /// <summary>Top-left tile a placed building really covers.</summary>
        internal static Vector2Int? TopLeftTiles(BuildingInst b)
        {
            if (!Tiles(b, out int x0, out _, out int y0, out int y1)) return null;
            return new Vector2Int(x0, _dy > 0 ? y1 : y0);
        }

        /// <summary>"3 wide, 2 tall" for a placed building, from the tiles it covers.</summary>
        internal static string SizeTiles(BuildingInst b)
        {
            if (!Tiles(b, out int x0, out int x1, out int y0, out int y1)) return Size(b);
            return Size(new Vector2Int(x1 - x0 + 1, y1 - y0 + 1));
        }

        /// <summary>
        /// The top-left tile a building being placed will cover. Measured in the chunk grid after building: the
        /// hover tile is the middle column (the left one of the middle two for an even width) and the middle row
        /// (the lower one of the middle two) of the building's listed size, and turning it with R did not change
        /// the tiles it took (a turned 3 by 2 villa still covered 3 by 2), so the listed size is used, not
        /// Inst.GetTileSize(), which swaps.
        /// </summary>
        internal static Vector2Int PreviewTopLeft(Vector2Int hover, Vector2Int size)
        {
            int w = Math.Max(1, size.x), h = Math.Max(1, size.y);
            int left = hover.x - (w - 1) / 2;
            int bottom = _dy > 0 ? hover.y - (h - 1) / 2 : hover.y + (h - 1) / 2;
            int top = _dy > 0 ? bottom + h - 1 : bottom - h + 1;
            return new Vector2Int(left, top);
        }

        internal static bool Bought(int x, int y)
        {
            var c = ChunkAt(x, y, out _, out _);
            if (c != null) return c.IsPurchased;
            try
            {
                var w = World(x, y);
                return BaseGridMgr.I.IsPurchasedChunk(BaseGridMgr.GetChunkXFromWorldPos(w.x), BaseGridMgr.GetChunkYFromWorldPos(w.y));
            }
            catch { return false; }
        }

        /// <summary>"3 wide, 2 tall": the tiles a building covers, as it is turned now.</summary>
        internal static string Size(Vector2Int s) => s.x > 0 && s.y > 0 ? Loc.F("bld_size", s.x, s.y) : "";

        internal static string Size(BuildingInfo inf)
        {
            try { return inf != null ? Size(inf.TileSize) : ""; } catch { return ""; }
        }

        internal static string Size(BuildingInst b)
        {
            try { return b != null ? Size(b.GetTileSize()) : ""; } catch { return ""; }
        }

        /// <summary>
        /// Top-left tile of a building whose bottom-left tile is (x, y): the game anchors a building, and its
        /// placement preview's hover point, on its bottom-left tile (checked with explore mode after building).
        /// </summary>
        internal static Vector2Int TopLeft(Vector2Int botLeft, Vector2Int size)
        {
            int h = Math.Max(1, size.y);
            return new Vector2Int(botLeft.x, _dy > 0 ? botLeft.y + h - 1 : botLeft.y);
        }

        /// <summary>Top-left tile of a building: the column and row explore mode gives its first tile.</summary>
        internal static Vector2Int TopLeft(BuildingInst b)
        {
            var bl = b.GetBotLeftGridPos();
            return TopLeft(new Vector2Int(bl.x, bl.y), b.GetTileSize());
        }
    }
}

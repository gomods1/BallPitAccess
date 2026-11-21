using System;
using System.Collections.Generic;
using BallPitAccess.Core;
using BallPitAccess.Input;
using BallPitAccess.UI;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Keyboard harvest launch in the base (BaseState.kAimWorkers). The game aims with the mouse and
    /// launches on a click. Here: Tab / Shift+Tab cycle the buildings that can be harvested now, aiming at
    /// each (name, distance and direction spoken); Left / Right turn the aim 5 degrees; Enter or Space
    /// launches through the game's own BaseMgr.LaunchWorkers with that direction.
    /// Buildings under construction or upgrading are targets too: a worker that bounces off one adds build
    /// points to it (BuildingInst.GetHitByWorker -> AddUpgradePts). A building that finishes is announced.
    /// </summary>
    internal static class Harvest
    {
        private static bool _was;
        private static float _deg = 90f;
        private static int _index = -1;
        private static int _errors;
        private static float _announced = -10f;

        internal static bool Active
        {
            get
            {
                try { var b = BaseMgr.I; return b != null && b.CurState == BaseState.kAimWorkers; }
                catch { return false; }
            }
        }

        internal static void Update()
        {
            try
            {
                bool active = Active;
                if (active && !_was && Time.unscaledTime - _announced > 2f)
                {
                    // The state flickers on entry: set up and announce once.
                    _announced = Time.unscaledTime;
                    _index = -1;
                    _deg = 90f;
                    // Start already aimed at a building (the one Enter was pressed on in the base list, or the
                    // first), so Enter alone launches at something.
                    var list = Ordered();
                    int pick = -1;
                    if (_prefer != IntPtr.Zero && Time.unscaledTime - _preferAt < 6f)
                        for (int k = 0; k < list.Count; k++) if (list[k].Pointer == _prefer) pick = k;
                    _prefer = IntPtr.Zero;
                    if (pick < 0 && list.Count > 0) pick = 0;
                    if (pick >= 0)
                    {
                        _index = pick;
                        string reach = AimAt(list[pick]);
                        int ready = 0;
                        foreach (var b in list) if (!UnderConstruction(b)) ready++;
                        Speech.Event(Loc.F("harvest_start_at", ready, TargetName(list[pick]), AngleWords(_deg) + ", " + reach));
                    }
                    else Speech.Event(Loc.F("harvest_start", 0));
                    int building = 0;
                    foreach (var b in list) if (UnderConstruction(b)) building++;
                    if (building > 0) Speech.Event(Loc.F("construct_n", building));
                }
                _was = active;
                WatchFinished();
                WatchEnd();
                if (!active || Keys.Consumed || !Keys.GameHasFocus() || Keys.Ctrl || Keys.Alt || ModKeys.ListOpen || Navigator.Blocking) return;

                if (Keys.Pressed(Keys.VK_TAB)) Cycle(Keys.Shift ? -1 : 1);
                else if (Keys.Pressed(Keys.VK_LEFT)) Turn(5f);
                else if (Keys.Pressed(Keys.VK_RIGHT)) Turn(-5f);
                else if (Keys.Pressed(Keys.VK_RETURN) || Keys.Pressed(Keys.VK_SPACE)) { Keys.Consume(); Launch(); }
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Harvest: " + ex);
            }
        }

        private static List<BuildingInst> Targets()
        {
            var list = new List<BuildingInst>();
            var bm = BuildingMgr.I;
            if (bm == null || bm.BuildingsById == null) return list;
            foreach (var kv in bm.BuildingsById)
            {
                var b = kv.Value;
                if (b == null || b.Obj == null) continue;
                bool ok;
                try { ok = b.CanHarvest() || UnderConstruction(b); } catch { ok = false; }
                if (ok) list.Add(b);
            }
            Vector3 p = PlayerPos();
            list.Sort((a, c) => Angle(a.Obj.transform.position, p).CompareTo(Angle(c.Obj.transform.position, p)));
            return list;
        }

        private static IntPtr _prefer = IntPtr.Zero;
        private static float _preferAt;

        /// <summary>The building the harvest aim should start on (set just before the harvest is opened).</summary>
        internal static void Prefer(IntPtr building) { _prefer = building; _preferAt = Time.unscaledTime; }

        /// <summary>Targets from left to right.</summary>
        private static List<BuildingInst> Ordered()
        {
            var list = Targets();
            list.Reverse();
            return list;
        }

        /// <summary>Under construction or upgrading: workers that bounce off it build it.</summary>
        internal static bool UnderConstruction(BuildingInst b)
        {
            try { return b != null && (b.CurState == BuildingState.kScaffold || b.CurState == BuildingState.kUpgrading); }
            catch { return false; }
        }

        private static string PlainName(BuildingInst b)
        {
            string name = Readers.Tr(b.GetInfo()?.GetNameSlug());
            return name.Length > 0 ? name : b.Type.ToString().TrimStart('k');
        }

        private static string TargetName(BuildingInst b)
        {
            string name = PlainName(b);
            try
            {
                if (b.CurState == BuildingState.kScaffold)
                    return name + ", " + Loc.F("bld_scaffold", Mathf.RoundToInt(b.GetUpgradePct() * 100f));
                if (b.CurState == BuildingState.kUpgrading)
                    return name + ", " + Loc.F("bld_upgrading", Mathf.RoundToInt(b.GetUpgradePct() * 100f));
            }
            catch { }
            return name;
        }

        // Buildings under construction or upgrading, by pointer, with their names: one that leaves that
        // state (the workers finished it) is announced. Only workers move these states on, and a newly
        // placed building enters the watch already under construction, so nothing else is spoken.
        private static readonly Dictionary<IntPtr, string> _building = new Dictionary<IntPtr, string>();
        private static float _nextWatch;

        private static void WatchFinished()
        {
            if (Time.unscaledTime < _nextWatch) return;
            _nextWatch = Time.unscaledTime + 0.3f;
            var bm = BuildingMgr.I;
            if (bm == null || bm.BuildingsById == null || BaseMgr.I == null) { _building.Clear(); return; }
            var seen = new HashSet<IntPtr>();
            foreach (var kv in bm.BuildingsById)
            {
                var b = kv.Value;
                if (b == null) continue;
                var ptr = b.Pointer;
                seen.Add(ptr);
                bool now = UnderConstruction(b);
                if (now) { if (!_building.ContainsKey(ptr)) _building[ptr] = PlainName(b); }
                else if (_building.TryGetValue(ptr, out var name))
                {
                    _building.Remove(ptr);
                    Speech.Event(Loc.F("bld_done", name));
                    Log.Info("Construction finished: " + b.Type);
                }
            }
            // Dismantled or picked up: forget it without a word.
            if (_building.Count > 0)
                foreach (var ptr in new List<IntPtr>(_building.Keys)) if (!seen.Contains(ptr)) _building.Remove(ptr);
        }

        private static Vector3 PlayerPos()
        {
            var bp = BaseMgr.I?.BPlayer;
            return bp != null ? bp.transform.position : Vector3.zero;
        }

        private static float Angle(Vector3 to, Vector3 from) => Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;

        private static void Cycle(int dir)
        {
            var list = Ordered();
            if (list.Count == 0) { Speech.SayMenu(Loc.Get("harvest_none")); return; }
            _index = _index < 0 ? (dir > 0 ? 0 : list.Count - 1) : (_index + dir + list.Count) % list.Count;
            var b = list[_index];
            Vector3 p = PlayerPos();
            string reach = AimAt(b);
            string name = TargetName(b);
            float dist = Vector2.Distance(b.Obj.transform.position, p) / (BaseGridMgr.kSpaceWidth > 0.01f ? BaseGridMgr.kSpaceWidth : 1f);
            Speech.SayMenu(Loc.F("harvest_target", name, Mathf.RoundToInt(dist), AngleWords(_deg) + ", " + reach, _index + 1, list.Count));
        }

        private static void Turn(float d)
        {
            _deg = Mathf.Clamp(_deg + d, 5f, 175f);
            Apply();
            Speech.SayMenu(AngleWords(_deg) + ", " + HitWords(_deg));
        }

        private static Vector2 Dir() => new Vector2(Mathf.Cos(_deg * Mathf.Deg2Rad), Mathf.Sin(_deg * Mathf.Deg2Rad));

        /// <summary>Show the aim to sighted helpers too (the game's dotted preview follows the player's aim).</summary>
        private static void Apply()
        {
            try { BaseMgr.I?.BPlayer?.SetLastAimDir(Dir()); } catch { }
        }

        private static void Launch()
        {
            var b = BaseMgr.I;
            if (b == null) return;
            Snapshot();
            var list = Ordered();
            _aimedAt = _index >= 0 && _index < list.Count ? list[_index].Pointer : IntPtr.Zero;
            var first = FirstBuilding(LaunchPos(), Dir(), out int walls);
            b.LaunchWorkers(Dir());
            _launchedAt = Time.unscaledTime;
            _loggedStart = false;
            Log.Info("Harvest launched at " + _deg + " degrees from " + LaunchPos() + ", predicted first building "
                + (first != null ? first.Type + " after " + walls + " wall bounces" : "none"));
            Speech.Event(Loc.Get("harvest_launched"));
        }

        // ---------------- Where a worker goes (a player launched at a Haunted House for days and never built it) ----------------
        // BaseMgr.MoveBalls moves each worker with Physics2D.Raycast(position, aim, step, ColMgr.kLayerMaskBounce):
        // a hit on the piece layer is a building (BaseGridMgr.WorkerHitBuilding), anything else is a wall it
        // bounces off. The worker is a point, so the same ray from the launch point says which building it
        // touches first. Aiming at a building's centre can be blocked by a building in front of it; AimAt
        // searches every angle for one whose first building is the target.

        private const float Step = 0.5f;

        /// <summary>Where workers start: measured from the first worker of each launch, the hero until then.</summary>
        private static Vector2 _launchPos;
        private static bool _haveLaunchPos;
        private static float _launchedAt = -10f;
        private static bool _loggedStart;

        private static Vector2 LaunchPos()
        {
            if (_haveLaunchPos) return _launchPos;
            return PlayerPos();
        }

        private static Vector2 DirOf(float deg) => new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));

        /// <summary>The worker's pickup radius (BallObj.PickupRange), read from a live worker; a guess until then.</summary>
        private static float _pickupR = 0.125f;   // measured 2026-11-21

        private static BuildingInst BuildingOf(Collider2D col)
        {
            if (col == null) return null;
            var bo = col.GetComponentInParent<BuildingObj>();
            return bo != null ? bo.Inst : null;
        }

        /// <summary>
        /// Follow a worker launched this way: up to 4 wall bounces until the first building it bounces off
        /// (returned, null when none). `crossed` gets the buildings it passes over on the way (wheat fields and
        /// the other kLayerMaskPickup buildings are harvested by a circle overlap as the worker moves, and do
        /// not stop it). `log` adds every segment for the debug log.
        /// </summary>
        private static BuildingInst Trace(Vector2 start, Vector2 dir, out int walls, List<BuildingInst> crossed, System.Text.StringBuilder log = null, List<int> crossedWalls = null)
        {
            walls = 0;
            int mask, pickMask, piece;
            try { mask = ColMgr.kLayerMaskBounce; pickMask = ColMgr.kLayerMaskPickup; piece = ColMgr.I != null ? ColMgr.I.GetGameLayer(GameLayerType.kPiece) : -1; }
            catch { return null; }
            Vector2 pos = start, d = dir.normalized;
            for (int i = 0; i < 5; i++)
            {
                RaycastHit2D hit;
                try { hit = Physics2D.Raycast(pos, d, 80f, mask); } catch { return null; }
                var col = hit.collider;
                float len = col != null ? hit.distance : 80f;
                // Physics2D.CircleCastAll is not in the game build (calling it closed the game): the pickup
                // buildings are box colliders, so test the segment against each box grown by the pickup radius.
                if (crossed != null)
                    foreach (var pb in PickupBoxes(pickMask))
                    {
                        if (Enter(pos, pos + d * len, pb.Value, _pickupR) < 0f) continue;
                        bool dup = false;
                        foreach (var c in crossed) if (Same(c, pb.Key)) dup = true;
                        if (!dup) { crossed.Add(pb.Key); crossedWalls?.Add(walls); }
                    }
                if (log != null)
                    log.Append(" | from ").Append(pos).Append(" dir ").Append(d).Append(" len ").Append(len.ToString("0.00"))
                       .Append(col != null ? " hit " + col.gameObject.name + " layer " + col.gameObject.layer + " at " + hit.point + " normal " + hit.normal : " nothing");
                if (col == null) return null;
                if (col.gameObject.layer == piece) return BuildingOf(col);
                walls++;
                Vector2 n = hit.normal;
                d = Vector2.Reflect(d, n).normalized;
                pos = hit.point + n * 0.02f;
            }
            return null;
        }

        private static BuildingInst FirstBuilding(Vector2 start, Vector2 dir, out int walls) => Trace(start, dir, out walls, null);

        private static readonly List<KeyValuePair<BuildingInst, Bounds>> _pickBoxes = new List<KeyValuePair<BuildingInst, Bounds>>();
        private static int _pickBoxesFrame = -1;

        /// <summary>Buildings workers pass over (collider on a kLayerMaskPickup layer), with their collider bounds; once per frame.</summary>
        private static List<KeyValuePair<BuildingInst, Bounds>> PickupBoxes(int pickMask)
        {
            if (_pickBoxesFrame == Time.frameCount) return _pickBoxes;
            _pickBoxesFrame = Time.frameCount;
            _pickBoxes.Clear();
            var bm = BuildingMgr.I;
            if (bm == null || bm.BuildingsById == null) return _pickBoxes;
            foreach (var kv in bm.BuildingsById)
            {
                var b = kv.Value;
                try
                {
                    var c = b != null && b.Obj != null ? b.Obj.Col : null;
                    if (c == null || !c.enabled || (pickMask & (1 << c.gameObject.layer)) == 0) continue;
                    _pickBoxes.Add(new KeyValuePair<BuildingInst, Bounds>(b, c.bounds));
                }
                catch { }
            }
            return _pickBoxes;
        }

        /// <summary>Where the segment s to e enters the box grown by pad (0..1), or -1 when it misses it.</summary>
        private static float Enter(Vector2 s, Vector2 e, Bounds b, float pad)
        {
            Vector2 d = e - s, min = (Vector2)b.min - new Vector2(pad, pad), max = (Vector2)b.max + new Vector2(pad, pad);
            float t0 = 0f, t1 = 1f;
            for (int k = 0; k < 2; k++)
            {
                float sk = k == 0 ? s.x : s.y, dk = k == 0 ? d.x : d.y, lo = k == 0 ? min.x : min.y, hi = k == 0 ? max.x : max.y;
                if (Math.Abs(dk) < 1e-6f) { if (sk < lo || sk > hi) return -1f; continue; }
                float a = (lo - sk) / dk, c = (hi - sk) / dk;
                if (a > c) { float tmp = a; a = c; c = tmp; }
                t0 = Math.Max(t0, a); t1 = Math.Min(t1, c);
                if (t0 > t1) return -1f;
            }
            return t0;
        }

        /// <summary>Workers pass over this building instead of bouncing off it (wheat fields).</summary>
        private static bool PassOver(BuildingInst b)
        {
            try
            {
                var c = b.Obj != null ? b.Obj.Col : null;
                return c != null && (ColMgr.kLayerMaskPickup & (1 << c.gameObject.layer)) != 0;
            }
            catch { return false; }
        }

        /// <summary>A worker launched this way reaches `b`: bounces off it first, or passes over it before its first bounce off a building.</summary>
        private static bool Reaches(Vector2 start, Vector2 dir, BuildingInst b, bool passOver, out int walls, out BuildingInst first)
        {
            var crossed = passOver ? new List<BuildingInst>() : null;
            var crossedWalls = passOver ? new List<int>() : null;
            first = Trace(start, dir, out walls, crossed, null, crossedWalls);
            if (!passOver) return Same(first, b);
            for (int i = 0; i < crossed.Count; i++)
                if (Same(crossed[i], b)) { walls = crossedWalls[i]; return true; }
            return false;
        }

        internal static string DevTrace(float deg)
        {
            var sb = new System.Text.StringBuilder();
            var crossed = new List<BuildingInst>();
            var first = Trace(LaunchPos(), DirOf(deg), out int walls, crossed, sb);
            string over = "";
            foreach (var c in crossed) over += " " + c.Type;
            return "trace " + deg + " from " + LaunchPos() + ": first " + (first != null ? first.Type.ToString() : "none") + " after " + walls + " walls, over" + over + sb;
        }

        private static bool Same(BuildingInst a, BuildingInst b) => a != null && b != null && a.Pointer == b.Pointer;

        /// <summary>
        /// Aim at the angle whose ray reaches `b` first: the widest stretch of such angles, straight shots before
        /// bank shots off a wall, the middle of it. Returns the words for how the workers reach it.
        /// </summary>
        private static string AimAt(BuildingInst b)
        {
            Vector2 start = LaunchPos();
            float centre = Mathf.Clamp(Angle(b.Obj.transform.position, start), 5f, 175f);
            float bestMid = -1f, bestWidth = -1f;
            bool bestDirect = false;
            float runStart = -1f;
            bool runDirect = true;
            bool passOver = PassOver(b);
            var blockers = new Dictionary<IntPtr, int>();
            var blockerInst = new Dictionary<IntPtr, BuildingInst>();
            for (float a = 5f; a <= 175f + 0.001f + Step; a += Step)
            {
                bool hit = false;
                int walls = 0;
                if (a <= 175f + 0.001f)
                {
                    hit = Reaches(start, DirOf(a), b, passOver, out walls, out BuildingInst first);
                    if (!hit && first != null && walls == 0 && Math.Abs(a - centre) <= 20f)
                    {
                        blockers.TryGetValue(first.Pointer, out int c);
                        blockers[first.Pointer] = c + 1;
                        blockerInst[first.Pointer] = first;
                    }
                }
                // A stretch is angles of one kind: straight shots and bank shots off a wall are separate stretches.
                if (hit && runStart >= 0f && (walls == 0) != runDirect) CloseRun(a, centre, ref runStart, runDirect, ref bestMid, ref bestWidth, ref bestDirect);
                if (hit)
                {
                    if (runStart < 0f) { runStart = a; runDirect = walls == 0; }
                }
                else if (runStart >= 0f) CloseRun(a, centre, ref runStart, runDirect, ref bestMid, ref bestWidth, ref bestDirect);
            }
            if (bestMid >= 0f)
            {
                _deg = Mathf.Clamp(bestMid, 5f, 175f);
                Apply();
                Log.Info("Harvest aim at " + b.Type + ": " + bestMid + " degrees, " + (bestDirect ? "direct" : "off a wall") + ", stretch " + bestWidth + " degrees, centre " + centre);
                return Loc.Get(passOver ? (bestDirect ? "reach_over" : "reach_over_wall") : bestDirect ? "reach_direct" : "reach_wall");
            }
            _deg = centre;
            Apply();
            BuildingInst blocker = null;
            int most = 0;
            foreach (var kv in blockers) if (kv.Value > most) { most = kv.Value; blocker = blockerInst[kv.Key]; }
            Log.Info("Harvest aim at " + b.Type + ": no angle reaches it first; blocked by " + (blocker != null ? blocker.Type.ToString() : "nothing"));
            return blocker != null ? Loc.F("reach_blocked", PlainName(blocker)) : Loc.Get("reach_none");
        }

        /// <summary>End the stretch before angle `a`; keep it if it beats the best one: straight before a bank
        /// shot, then the wider stretch (more room for error), then the one nearer the building's centre.</summary>
        private static void CloseRun(float a, float centre, ref float runStart, bool runDirect, ref float bestMid, ref float bestWidth, ref bool bestDirect)
        {
            float end = a - Step, width = end - runStart, mid = (runStart + end) * 0.5f;
            bool better = bestMid < 0f
                || (runDirect && !bestDirect)
                || (runDirect == bestDirect && (width > bestWidth + 0.01f
                    || (Math.Abs(width - bestWidth) <= 0.01f && Math.Abs(mid - centre) < Math.Abs(bestMid - centre))));
            if (better) { bestMid = mid; bestWidth = width; bestDirect = runDirect; }
            runStart = -1f;
        }

        /// <summary>What the line at this angle hits first, for the arrow keys.</summary>
        private static string HitWords(float deg)
        {
            var crossed = new List<BuildingInst>();
            var first = Trace(LaunchPos(), DirOf(deg), out int walls, crossed);
            string s = first == null ? Loc.Get("hits_nothing") : Loc.F(walls == 0 ? "hits" : "hits_wall", TargetName(first));
            var names = new List<string>();
            foreach (var c in crossed) { bool ok; try { ok = c.CanHarvest(); } catch { ok = false; } if (ok) names.Add(PlainName(c)); }
            if (names.Count > 0) s = Loc.F("passes_over", string.Join(", ", names)) + ", " + s;
            return s;
        }

        // ---------------- Construction report after a harvest ----------------
        // The game's summary only counts resources: with nothing harvested it says "A Fruitless Endeavor" even
        // when the workers built. The mod says what each building under construction got.

        private struct Before { public string Name; public int Pct; }
        private static readonly Dictionary<IntPtr, Before> _before = new Dictionary<IntPtr, Before>();
        private static readonly Dictionary<IntPtr, int> _hits = new Dictionary<IntPtr, int>();
        private static bool _harvesting;
        private static IntPtr _aimedAt;

        private static int Pct(BuildingInst b)
        {
            try { return Mathf.RoundToInt(b.GetUpgradePct() * 100f); } catch { return 0; }
        }

        private static void Snapshot()
        {
            _before.Clear();
            _hits.Clear();
            _hitLogs = 0;
            var bm = BuildingMgr.I;
            if (bm == null || bm.BuildingsById == null) return;
            foreach (var kv in bm.BuildingsById)
            {
                var b = kv.Value;
                if (b != null && UnderConstruction(b)) _before[b.Pointer] = new Before { Name = PlainName(b), Pct = Pct(b) };
            }
            _harvesting = true;
        }

        private static void WatchEnd()
        {
            var bm = BaseMgr.I;
            if (bm == null) { _harvesting = false; return; }
            BaseState st;
            try { st = bm.CurState; } catch { return; }
            // The first worker's position is where every launch starts (logged once per launch).
            if (!_loggedStart && Time.unscaledTime - _launchedAt < 3f)
            {
                try
                {
                    var balls = bm.ActiveBalls;
                    if (balls != null && balls.Count > 0 && balls[0] != null)
                    {
                        _loggedStart = true;
                        Vector2 p = balls[0].transform.position;
                        try { float r = balls[0].PickupRange; if (r > 0.01f && r < 3f) _pickupR = r; } catch { }
                        string sp = "";
                        try { var ws = bm.WorkerStartPos; if (ws != null) for (int i = 0; i < ws.Count && i < 4; i++) sp += " " + ws[i]; } catch { }
                        Log.Info("Harvest: first worker at " + p + " (" + (Time.unscaledTime - _launchedAt).ToString("0.00") + " s after launch), hero at " + PlayerPos() + ", start positions" + sp + ", pickup range " + _pickupR);
                    }
                }
                catch { }
            }
            if (!_harvesting || st == BaseState.kAimWorkers || st == BaseState.kBounceWorkers || st == BaseState.kCutscene) return;
            _harvesting = false;
            if (_before.Count == 0) return;
            var lines = new List<string>();
            foreach (var kv in _before)
            {
                BuildingInst b = null;
                try { if (BuildingMgr.I != null) foreach (var x in BuildingMgr.I.BuildingsById) if (x.Value != null && x.Value.Pointer == kv.Key) { b = x.Value; break; } }
                catch { }
                _hits.TryGetValue(kv.Key, out int hits);
                if (b == null || !UnderConstruction(b)) continue;   // finished: "X is finished" was said
                int now = Pct(b);
                // Every building the workers built, and the one aimed at when they missed it.
                if (hits > 0 || now != kv.Value.Pct) lines.Add(Loc.F("built_progress", kv.Value.Name, hits, kv.Value.Pct, now));
                else if (kv.Key == _aimedAt) lines.Add(Loc.F("built_missed", kv.Value.Name, now));
            }
            foreach (var l in lines) { Speech.Event(l); Log.Info("Harvest construction: " + l); }
        }

        internal static void TryPatch(HarmonyLib.Harmony h)
        {
            try
            {
                var m = HarmonyLib.AccessTools.Method(typeof(BuildingInst), nameof(BuildingInst.GetHitByWorker));
                if (m == null) { Log.Warn("BuildingInst.GetHitByWorker not found"); return; }
                h.Patch(m, postfix: new HarmonyLib.HarmonyMethod(typeof(Harvest), nameof(HitPostfix)));
                Log.Info("Patched BuildingInst.GetHitByWorker");
            }
            catch (Exception ex) { Log.Error("GetHitByWorker patch failed: " + ex); }
        }

        private static int _hitLogs;

        private static void HitPostfix(BuildingInst __instance)
        {
            try
            {
                if (__instance == null) return;
                var ptr = __instance.Pointer;
                _hits.TryGetValue(ptr, out int n);
                _hits[ptr] = n + 1;
                if (_hitLogs < 40 && Config.Debug)
                {
                    _hitLogs++;
                    Log.Info("Worker hit " + __instance.Type + " (" + __instance.CurState + ", " + Pct(__instance) + " percent)");
                }
            }
            catch { }
        }

        /// <summary>Dev: aim at the first building of a type (kAimWorkers only), as Tab would; or "deg N".</summary>
        internal static string DevAim(string arg)
        {
            if (!Active) return "not aiming workers (state " + (BaseMgr.I != null ? BaseMgr.I.CurState.ToString() : "none") + ")";
            if (arg.StartsWith("trace ")) return DevTrace(float.Parse(arg.Substring(6), System.Globalization.CultureInfo.InvariantCulture));
            if (arg.StartsWith("deg "))
            {
                _deg = Mathf.Clamp(float.Parse(arg.Substring(4), System.Globalization.CultureInfo.InvariantCulture), 5f, 175f);
                Apply();
                return AngleWords(_deg) + ", " + HitWords(_deg);
            }
            var t = (BuildingType)Enum.Parse(typeof(BuildingType), arg);
            var list = Ordered();
            for (int i = 0; i < list.Count; i++)
                if (list[i].Type == t) { _index = i; string r = AimAt(list[i]); return TargetName(list[i]) + ", " + AngleWords(_deg) + " (" + _deg + "), " + r; }
            return "no target of type " + t + " (" + list.Count + " targets)";
        }

        internal static string DevLaunch()
        {
            if (!Active) return "not aiming workers";
            Launch();
            return "launched at " + _deg;
        }

        internal static string AngleWords(float deg)
        {
            float off = deg - 90f;
            if (Math.Abs(off) < 1f) return Loc.Get("straight_up");
            return Loc.F(off > 0 ? "deg_left" : "deg_right", Mathf.RoundToInt(Math.Abs(off)));
        }
    }
}

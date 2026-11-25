using System;
using System.Collections.Generic;
using BallPitAccess.Audio;
using BallPitAccess.Core;
using BallPitAccess.Input;
using BallPitAccess.UI;
using Il2Cpp;
using Il2CppRewired;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// The battle map (a player asked, 2026-11-24: "a map in pause mode where you could walk around and see enemies,
    /// their intent, what they do", "anything on the screen should be there"). The map key (M) pauses the battle
    /// (the game's own pause, GameMgr.SetState(kPaused)) and the arrows walk a cursor one tile at a time over the
    /// arena: each tile says what is on it (enemies with their health, the hero, balls, enemy shots, lasers, bombs,
    /// floor hazards, pickups) and where it is from the hero; Ctrl+Up reads the details of the first thing there
    /// (what an enemy does, its state, status effects, whether a straight shot reaches it).
    /// Tab / Shift+Tab: next / previous enemy, closest first. Page Down / Page Up: next / previous other thing.
    /// Home: back to the hero. Enter: lock the aim assist on the enemy here (after it dies the assist goes back to
    /// its mode); Delete releases the lock. M, Escape or Backspace close the map and resume when the map paused.
    /// Everything is a snapshot taken when the map opens: the game is paused, nothing moves.
    /// </summary>
    internal static class BattleMap
    {
        private enum Kind { Hero, Enemy, Shot, Laser, Bomb, Floor, Ball, Pickup }

        private sealed class Thing
        {
            public Kind Kind;
            public Rect Area;               // world rectangle it covers (a point is a tiny one)
            public bool IsLine; public Vector2 A, B;   // a laser beam
            public Vector3 Pos;             // where Tab / Page Down put the cursor
            public string Name = "";
            public string Short = "";       // what a tile says
            public List<string> Details = new List<string>();
            public IntPtr Id;
            public GridPieceInst Enemy;
            public float Dist;
        }

        private static bool _on, _pausedByUs, _unmutePending;
        private static GameState _openState;
        private static readonly List<Thing> Things = new List<Thing>();
        private static readonly List<ControllerMap> Muted = new List<ControllerMap>();
        private static int _col, _row, _cols, _heroCol, _rowMin, _rowMax;
        private static float _heroY, _left;
        private static IntPtr _lastEnemy;
        private static int _errors;

        internal static bool Active => _on;

        internal static void Toggle()
        {
            if (_on) { Close(true); return; }
            Open();
        }

        private static void Open()
        {
            if (!Battle.InBattle) { Speech.SayMenu(Loc.Get("not_in_battle")); return; }
            var gm = GameMgr.I;
            if (gm == null) return;
            var st = gm.CurState;
            _pausedByUs = false;
            if (st == GameState.kPlaying)
            {
                try { gm.SetState(GameState.kPaused); } catch (Exception ex) { Log.Warn("Map: could not pause: " + ex.Message); }
                if (gm.CurState != GameState.kPaused) { Speech.SayMenu(Loc.Get("map_not_now")); return; }
                _pausedByUs = true;
                _openState = GameState.kPaused;
            }
            // Any other screen that already stops the battle (pause, level up, fuser, revive) keeps it stopped.
            else if (st == GameState.kGameOver || st == GameState.kEndingGame || st == GameState.kEnteringLvl) { Speech.SayMenu(Loc.Get("map_not_now")); return; }
            _openState = gm.CurState;
            _on = true;
            if (_unmutePending) _unmutePending = false;
            else Mute(true);
            Snapshot();
            _col = _heroCol; _row = 0; _lastEnemy = IntPtr.Zero;
            Log.Info($"Map open: {Things.Count} things, columns {_cols}, rows {_rowMin}..{_rowMax}, hero column {_heroCol}, paused by the map {_pausedByUs}");
            string intro = Loc.Get(_pausedByUs ? "map_open_paused" : "map_open") + ". " + Summary();
            Speech.SayMenu(intro + ". " + Loc.F("map_keys", Bindings.KeyOf("map")) + ". " + Describe(_col, _row, null));
        }

        internal static void Close(bool say)
        {
            if (!_on) return;
            _on = false;
            _unmutePending = true;
            if (_pausedByUs)
            {
                _pausedByUs = false;
                try
                {
                    var gm = GameMgr.I;
                    if (gm != null && gm.CurState == GameState.kPaused)
                    {
                        var pu = PauseUI.I;
                        if (pu != null) pu.OnCloseClicked();
                        if (gm.CurState == GameState.kPaused) gm.SetState(GameState.kPlaying);
                    }
                }
                catch (Exception ex) { Log.Warn("Map: could not resume: " + ex.Message); }
                Navigator.Quiet();
            }
            if (say) Speech.SayMenu(Loc.Get("map_closed"));
            Log.Info("Map closed");
        }

        internal static void Update()
        {
            try
            {
                if (_unmutePending && !_on && !Keys.Down(Keys.VK_BACK) && !Keys.Down(Keys.VK_ESCAPE) && !Keys.Down(Keys.VK_RETURN)
                    && !Bindings.Down("map"))
                {
                    _unmutePending = false;
                    Mute(false);
                }
                if (!_on) return;
                var gm = GameMgr.I;
                if (!Battle.InBattle || gm == null || gm.CurState != _openState) { _pausedByUs = false; Close(false); return; }
                if (ModKeys.ModListOpen || Keys.Consumed || !Keys.GameHasFocus() || Keys.Ctrl || Keys.Alt || Keys.ScreenReaderModifier) return;

                if (Keys.Pressed(Keys.VK_LEFT)) { Keys.Consume(); Move(-1, 0); }
                else if (Keys.Pressed(Keys.VK_RIGHT)) { Keys.Consume(); Move(1, 0); }
                else if (Keys.Pressed(Keys.VK_UP)) { Keys.Consume(); Move(0, 1); }
                else if (Keys.Pressed(Keys.VK_DOWN)) { Keys.Consume(); Move(0, -1); }
                else if (Keys.Pressed(Keys.VK_TAB)) { Keys.Consume(); Jump(true, Keys.Shift ? -1 : 1); }
                else if (Keys.Pressed(Keys.VK_NEXT)) { Keys.Consume(); Jump(false, 1); }
                else if (Keys.Pressed(Keys.VK_PRIOR)) { Keys.Consume(); Jump(false, -1); }
                else if (Keys.Pressed(Keys.VK_HOME)) { Keys.Consume(); _col = _heroCol; _row = 0; _lastEnemy = IntPtr.Zero; Speak(null); }
                else if (Keys.Pressed(Keys.VK_END)) { Keys.Consume(); Speech.SayMenu(Summary()); }
                else if (Keys.Pressed(Keys.VK_RETURN)) { Keys.Consume(); LockHere(); }
                else if (Keys.Pressed(Keys.VK_DELETE)) { Keys.Consume(); AimAssist.Unlock(true); }
                else if (Keys.Pressed(Keys.VK_BACK) || Keys.Pressed(Keys.VK_ESCAPE)) { Keys.Consume(); Close(true); }
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("Map: " + ex);
            }
        }

        // ---------------- The snapshot ----------------

        private static Rect Box(Vector3 c, float half) => new Rect(c.x - half, c.y - half, half * 2f, half * 2f);

        private static void Snapshot()
        {
            Things.Clear();
            Battle.RefreshThreats();
            Vector3 hero = Battle.PlayerPos;
            _heroY = hero.y;
            _left = Battle.LeftX;
            float colW = Battle.ColW, rowH = Battle.RowH;
            _cols = Math.Max(1, Mathf.RoundToInt((Battle.RightX - Battle.LeftX) / colW));
            _heroCol = Mathf.Clamp(Mathf.FloorToInt((hero.x - _left) / colW), 0, _cols - 1);

            Things.Add(new Thing { Kind = Kind.Hero, Area = Box(hero, 0.2f), Pos = hero, Name = Loc.Get("map_you"), Short = Loc.Get("map_you") });
            Things[0].Details.Add(Loc.Get("map_you"));

            foreach (var t in Battle.Threats) AddEnemy(t);
            AddBalls();
            AddShots();
            AddLasers();
            AddBombs();
            AddFloor();
            AddPickups();
            foreach (var th in Things) th.Dist = Vector2.Distance(th.Pos, hero);

            float minY = Math.Min(Battle.MinY, hero.y), maxY = Math.Max(Battle.MaxY, hero.y);
            foreach (var th in Things) { minY = Math.Min(minY, th.Area.yMin); maxY = Math.Max(maxY, th.Area.yMax); }
            _rowMin = Math.Min(0, RowOf(minY));
            _rowMax = Mathf.Clamp(RowOf(maxY), 0, 60);
        }

        private static int RowOf(float y) => Mathf.RoundToInt((y - _heroY) / Battle.RowH);

        private static void AddEnemy(Battle.Threat t)
        {
            var inst = t.Inst;
            Rect area;
            try
            {
                var col = t.Obj != null ? t.Obj.Col : null;
                if (col != null && col.enabled && col.bounds.size.x > 0.05f && col.bounds.size.y > 0.05f)
                { var b = col.bounds; area = new Rect(b.min.x, b.min.y, b.size.x, b.size.y); }
                else area = Box(t.Center, Battle.ColW * 0.45f);
            }
            catch { area = Box(t.Center, Battle.ColW * 0.45f); }
            string name = Names.Enemy(inst);
            string health = HealthText(t);
            var th = new Thing { Kind = Kind.Enemy, Area = area, Pos = t.Center, Name = name, Id = inst.Pointer, Enemy = inst };
            string state = StateText(inst);
            bool locked = AimAssist.Locked != null && AimAssist.Locked.Pointer == inst.Pointer;
            th.Short = Readers.Join(name, health, state.Length > 0 ? state : null, locked ? Loc.Get("map_locked_mark") : null);
            var d = th.Details;
            d.Add(name);
            d.Add(health);
            if (state.Length > 0) d.Add(state);
            string fx = EnemyEffects(inst);
            if (fx.Length > 0) d.Add(Loc.F("map_effects", fx));
            string does = "";
            try { var info = inst.GetInfo(); if (info != null) does = Names.Tr(info.GetGameplayDescSlug()); } catch { }
            if (does.Length > 0) d.Add(does);
            try
            {
                var info = inst.GetInfo();
                if (info != null && (info.GridWidth > 1.01f || info.GridHeight > 1.01f))
                    d.Add(Loc.F("enc_size", Mathf.RoundToInt(info.GridWidth), Mathf.RoundToInt(info.GridHeight)));
            }
            catch { }
            d.Add(Loc.F("map_rows_to_line", t.Line.ToString("0.#")));
            if (!t.Hurtable) d.Add(Loc.Get("boss_immune"));
            else d.Add(Loc.Get(AimAssist.StraightTo(t) ? "map_clear_shot" : "map_blocked_shot"));
            if (locked) d.Add(Loc.Get("map_is_locked"));
            else if (AimAssist.Target != null && AimAssist.Target.Pointer == inst.Pointer) d.Add(Loc.Get("map_is_target"));
            Things.Add(th);
        }

        /// <summary>"health 120 of 300, 40 percent"; a boss: its bar's percentage (the Skeleton King's is his crown's).</summary>
        private static string HealthText(Battle.Threat t)
        {
            var inst = t.Inst;
            if (t.Boss && !t.Sub && !t.Part)
            {
                float pct = Battle.BossPct(inst.Type, out _);
                return Loc.F("boss_health", Mathf.RoundToInt(pct * 100f));
            }
            long cur = inst.CurHealth, max = Math.Max(1, inst.MaxHealth);
            return Loc.F("map_health", cur, max, Mathf.RoundToInt(cur * 100f / max));
        }

        private static string StateText(GridPieceInst inst)
        {
            switch (inst.CurState)
            {
                case PieceState.kChargingAttack: return Loc.Get("enemy_charging");
                case PieceState.kAttacking: case PieceState.kAttackingPiece: return Loc.Get("map_attacking");
                case PieceState.kFlying: return Loc.Get("map_flying");
                case PieceState.kEntering: return Loc.Get("map_entering");
                case PieceState.kFalling: return Loc.Get("map_falling");
            }
            return "";
        }

        private static string EnemyEffects(GridPieceInst inst)
        {
            var parts = new List<string>();
            try
            {
                var list = inst.StatusEffects;
                for (int i = 0; list != null && i < list.Count; i++)
                {
                    var e = list[i];
                    if (e == null) continue;
                    string n = Names.Mod("efx_", e.Type.ToString());
                    if (e.NumStacks > 1) n += " " + e.NumStacks;
                    if (!parts.Contains(n)) parts.Add(n);
                }
            }
            catch { }
            return string.Join(", ", parts);
        }

        private static void AddBalls()
        {
            var bm = BallMgr.I;
            if (bm == null || bm.ActiveBalls == null) return;
            for (int i = 0; i < bm.ActiveBalls.Count; i++)
            {
                try
                {
                    var b = bm.ActiveBalls[i];
                    if (b == null || !b.IsActive) continue;
                    string name = "";
                    try { if (b.Inst != null) name = Names.Upgrade(b.Inst.GetInfo()); } catch { }
                    if (name.Length == 0) name = Loc.Get("map_ball");
                    bool baby = false;
                    try { baby = b.IsBaby(); } catch { }
                    if (baby) name = Loc.F("map_baby_ball", name);
                    Vector3 p = b.transform.position, dir = b.AimDir;
                    string how = b.CurState == BallObjState.kReturning ? Loc.Get("map_ball_returning")
                        : dir.y < -0.05f ? Loc.Get("map_ball_down") : Loc.Get("map_ball_up");
                    var th = new Thing { Kind = Kind.Ball, Area = Box(p, 0.15f), Pos = p, Name = name, Id = b.Pointer, Short = name + ", " + how };
                    th.Details.Add(name);
                    th.Details.Add(how);
                    Things.Add(th);
                }
                catch { }
            }
        }

        private static string ShotName(ArrowType t)
        {
            switch (t)
            {
                case ArrowType.kIcicle: case ArrowType.kFrozenSpike: return Loc.Get("shot_ice");
                case ArrowType.kTar: return Loc.Get("shot_tar");
                case ArrowType.kFireball: return Loc.Get("shot_fire");
                case ArrowType.kVenom: case ArrowType.kBigVenom: return Loc.Get("shot_venom");
                case ArrowType.kBoulder: case ArrowType.kMoonRockSmall: case ArrowType.kMoonRockBig: case ArrowType.kHomingRock: return Loc.Get("shot_rock");
                case ArrowType.kNormal: case ArrowType.kHoming: case ArrowType.kHolyArrow: case ArrowType.kBallistaArrow: case ArrowType.kHarpoon:
                case ArrowType.kDagger: case ArrowType.kBone: return Loc.Get("shot_arrow");
            }
            return Loc.Get("shot_other");
        }

        private static void AddShots()
        {
            foreach (var a in UnityEngine.Object.FindObjectsOfType<ArrowObj>())
            {
                try
                {
                    if (a == null || !a.isActiveAndEnabled) continue;
                    bool friendly = false;
                    try { friendly = a.IsFriendly() || a.IsReflected; } catch { }
                    if (friendly) continue;
                    Vector3 p = a.transform.position, d = a.AimDir;
                    string name = ShotName(a.Type);
                    string how = Loc.Get(d.y < -0.05f ? "map_shot_down" : "map_shot_other");
                    var th = new Thing { Kind = Kind.Shot, Area = Box(p, 0.2f), Pos = p, Name = name, Id = a.Pointer, Short = name + ", " + how };
                    th.Details.Add(name);
                    th.Details.Add(how);
                    Things.Add(th);
                }
                catch { }
            }
        }

        private static void AddLasers()
        {
            foreach (var l in UnityEngine.Object.FindObjectsOfType<EnemyLaserObj>())
            {
                try
                {
                    if (l == null || !l.isActiveAndEnabled) continue;
                    Vector3 d3 = l.AimDir;
                    Vector2 dir = new Vector2(d3.x, d3.y);
                    if (dir.sqrMagnitude < 0.0001f) continue;
                    dir.Normalize();
                    Vector3 start = l.transform.position;
                    try { var x = l.GetShootXfm(); if (x != null) start = x.position; } catch { }
                    float len = l.Length > 0.1f ? Math.Min(l.Length, 40f) : 40f;
                    Vector2 a = start, b = a + dir * len;
                    var th = new Thing
                    {
                        Kind = Kind.Laser, IsLine = true, A = a, B = b, Pos = start, Id = l.Pointer,
                        Area = Rect.MinMaxRect(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Max(a.x, b.x), Math.Max(a.y, b.y)),
                        Name = Loc.Get("map_laser"), Short = Loc.Get("map_laser"),
                    };
                    th.Details.Add(th.Name);
                    Things.Add(th);
                }
                catch { }
            }
        }

        private static void AddBombs()
        {
            foreach (var b in Hazards.BombTargets())
            {
                var th = new Thing { Kind = Kind.Bomb, Area = Box(b.Key, Math.Max(0.2f, b.Value)), Pos = b.Key, Name = Loc.Get("map_bomb"), Short = Loc.Get("map_bomb") };
                th.Details.Add(th.Name);
                Things.Add(th);
            }
        }

        private static void AddFloor()
        {
            foreach (var o in UnityEngine.Object.FindObjectsOfType<ObstacleObj>())
            {
                try
                {
                    if (o == null || !o.isActiveAndEnabled || o.BallSrc != null) continue;
                    Rect area;
                    var c = o.Col;
                    if (c != null && c.bounds.size.x > 0.05f) { var bb = c.bounds; area = new Rect(bb.min.x, bb.min.y, bb.size.x, bb.size.y); }
                    else area = Box(o.transform.position, Math.Max(0.3f, o._size * 0.5f));
                    string name = Names.Obstacle(o.Type);
                    var th = new Thing { Kind = Kind.Floor, Area = area, Pos = o.transform.position, Name = name, Id = o.Pointer, Short = name };
                    th.Details.Add(name);
                    Things.Add(th);
                }
                catch { }
            }
        }

        private static void AddPickups()
        {
            var save = BattleSaveData.I;
            var list = save != null ? save.Pickups : null;
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    var p = list[i];
                    if (p == null) continue;
                    var obj = p.Obj;
                    if (obj == null || !obj.isActiveAndEnabled || obj.CurState == PickupObjState.kPickingUp) continue;
                    Vector3 pos = obj.transform.position;
                    string name = Names.Pickup(p.Type);
                    var th = new Thing { Kind = Kind.Pickup, Area = Box(pos, 0.15f), Pos = pos, Name = name, Id = p.Pointer, Short = name };
                    th.Details.Add(name);
                    Things.Add(th);
                }
                catch { }
            }
        }

        // ---------------- Walking ----------------

        private static Rect Cell(int c, int r) =>
            new Rect(_left + c * Battle.ColW, _heroY + (r - 0.5f) * Battle.RowH, Battle.ColW, Battle.RowH);

        private static bool In(Thing t, Rect cell)
        {
            if (t.IsLine) return SegmentHits(t.A, t.B, cell);
            // A little inside the cell: an enemy whose edge only touches it is not "on" it.
            const float m = 0.06f;
            return t.Area.xMax > cell.xMin + m && t.Area.xMin < cell.xMax - m && t.Area.yMax > cell.yMin + m && t.Area.yMin < cell.yMax - m;
        }

        private static bool SegmentHits(Vector2 s, Vector2 e, Rect r)
        {
            Vector2 d = e - s;
            float t0 = 0f, t1 = 1f;
            for (int k = 0; k < 2; k++)
            {
                float sk = k == 0 ? s.x : s.y, dk = k == 0 ? d.x : d.y, lo = k == 0 ? r.xMin : r.yMin, hi = k == 0 ? r.xMax : r.yMax;
                if (Math.Abs(dk) < 1e-6f) { if (sk < lo || sk > hi) return false; continue; }
                float a = (lo - sk) / dk, c = (hi - sk) / dk;
                if (a > c) { float tmp = a; a = c; c = tmp; }
                t0 = Math.Max(t0, a); t1 = Math.Min(t1, c);
                if (t0 > t1) return false;
            }
            return true;
        }

        private static List<Thing> At(int c, int r)
        {
            var cell = Cell(c, r);
            var o = new List<Thing>();
            foreach (var t in Things) if (In(t, cell)) o.Add(t);
            o.Sort((a, b) => a.Kind.CompareTo(b.Kind));
            return o;
        }

        private static void Move(int dc, int dr)
        {
            int c = _col + dc, r = _row + dr;
            if (c < 0 || c >= _cols || r < _rowMin || r > _rowMax)
            {
                Cues.Play("wall", dc < 0 ? -1f : dc > 0 ? 1f : 0f, 0.6f);
                Speech.SayMenu(Loc.Get(dc != 0 ? "map_edge_side" : dr > 0 ? "map_edge_top" : "map_edge_bottom"));
                return;
            }
            _col = c; _row = r;
            Speak(null);
        }

        private static void Speak(Thing first)
        {
            var here = At(_col, _row);
            if (first != null) { here.Remove(first); here.Insert(0, first); }
            Sound(here.Count > 0 ? here[0] : null);
            Speech.SayMenu(Describe(_col, _row, first));
        }

        /// <summary>A sound for the tile, from where it is relative to the hero: the enemy drum (higher = closer),
        /// the pickup chime, the shot or floor sound, or a tick for an empty tile.</summary>
        private static void Sound(Thing t)
        {
            Vector2 c = Cell(_col, _row).center;
            float pan = Battle.PanFor(c.x), muffle = Battle.Muffle(c.y);
            string name = "tick";
            if (t != null)
                switch (t.Kind)
                {
                    case Kind.Enemy: name = EnemyCue(_row); break;
                    case Kind.Pickup: name = "pickup1"; break;
                    case Kind.Shot: name = "shot"; break;
                    case Kind.Laser: name = "beam"; break;
                    case Kind.Bomb: name = "bomb"; break;
                    case Kind.Floor: name = "floor"; break;
                    case Kind.Ball: name = "catch"; break;
                    case Kind.Hero: name = "tick"; break;
                }
            Cues.Play(name, pan, 0.7f, muffle);
        }

        /// <summary>The enemy drum for a thing this many rows ahead of the hero (higher = closer).</summary>
        internal static string EnemyCue(int row) => "enemy" + Mathf.Clamp(9 - Mathf.RoundToInt(Math.Max(0, row) / 1.5f), 0, 9);

        private static string Describe(int c, int r, Thing first)
        {
            var here = At(c, r);
            if (first != null) { here.Remove(first); here.Insert(0, first); }
            var parts = new List<string>();
            IntPtr prev = _lastEnemy;
            _lastEnemy = IntPtr.Zero;
            foreach (var t in here)
            {
                if (t.Kind == Kind.Enemy)
                {
                    // Still on the same (big) enemy: its name only.
                    if (_lastEnemy == IntPtr.Zero) _lastEnemy = t.Id;
                    parts.Add(t.Id == prev && first == null ? Loc.F("map_same", t.Name) : t.Short);
                }
                else parts.Add(t.Short);
            }
            string what = parts.Count > 0 ? string.Join("; ", parts) : Loc.Get("map_empty");
            string where = Where(c, r);
            var lines = new List<string>();
            if (here.Count > 0) { lines.AddRange(here[0].Details); for (int i = 1; i < here.Count; i++) lines.Add(here[i].Short); }
            else lines.Add(what);
            lines.Add(where);
            Buffers.SetDetails(lines);
            return what + ". " + where;
        }

        /// <summary>"2 left, 5 ahead, column 2 of 7": from the hero, then the column counted from the left wall.</summary>
        private static string Where(int c, int r)
        {
            int dc = c - _heroCol;
            string side = dc == 0 ? Loc.Get("map_your_column") : Loc.F(dc < 0 ? "n_left" : "n_right", Math.Abs(dc));
            string row = r == 0 ? Loc.Get("map_your_row") : Loc.F(r > 0 ? "n_ahead" : "n_behind", Math.Abs(r));
            return side + ", " + row + ", " + Loc.F("map_column", c + 1, _cols);
        }

        private static void Jump(bool enemies, int dir)
        {
            var list = new List<Thing>();
            foreach (var t in Things)
                if (enemies ? t.Kind == Kind.Enemy : (t.Kind != Kind.Enemy && t.Kind != Kind.Hero)) list.Add(t);
            if (list.Count == 0) { Speech.SayMenu(Loc.Get(enemies ? "no_enemies" : "map_nothing_else")); return; }
            // Enemies keep Battle.Threats' order (closest to the hero first); the rest go by distance.
            if (!enemies) list.Sort((a, b) => a.Dist.CompareTo(b.Dist));
            int cur = list.FindIndex(t => t == _jumped);
            int i = cur < 0 ? (dir > 0 ? 0 : list.Count - 1) : (cur + dir + list.Count) % list.Count;
            var pick = list[i];
            _jumped = pick;
            _col = Mathf.Clamp(Mathf.FloorToInt((pick.Pos.x - _left) / Battle.ColW), 0, _cols - 1);
            _row = Mathf.Clamp(RowOf(pick.Pos.y), _rowMin, _rowMax);
            _lastEnemy = IntPtr.Zero;
            var here = At(_col, _row);
            Sound(pick);
            string text = Describe(_col, _row, here.Contains(pick) ? pick : null);
            Speech.SayMenu(text + ". " + Loc.F("n_of_m", i + 1, list.Count));
        }

        private static Thing _jumped;

        private static void LockHere()
        {
            var here = At(_col, _row);
            Thing e = null;
            if (_jumped != null && _jumped.Kind == Kind.Enemy && here.Contains(_jumped)) e = _jumped;
            if (e == null) foreach (var t in here) if (t.Kind == Kind.Enemy) { e = t; break; }
            if (e == null) { Speech.SayMenu(Loc.Get("map_no_enemy_here")); return; }
            if (AimAssist.Locked != null && AimAssist.Locked.Pointer == e.Enemy.Pointer) { AimAssist.Unlock(true); return; }
            AimAssist.Lock(e.Enemy, e.Name);
        }

        private static string Summary()
        {
            var counts = new Dictionary<string, int>();
            var order = new List<string>();
            int enemies = 0, pickups = 0, balls = 0, shots = 0, hazards = 0;
            foreach (var t in Things)
            {
                switch (t.Kind)
                {
                    case Kind.Enemy:
                        enemies++;
                        if (!counts.ContainsKey(t.Name)) { counts[t.Name] = 0; order.Add(t.Name); }
                        counts[t.Name]++;
                        break;
                    case Kind.Pickup: pickups++; break;
                    case Kind.Ball: balls++; break;
                    case Kind.Shot: case Kind.Laser: case Kind.Bomb: shots++; break;
                    case Kind.Floor: hazards++; break;
                }
            }
            order.Sort((a, b) => counts[b].CompareTo(counts[a]));
            var kinds = new List<string>();
            foreach (var n in order) kinds.Add(counts[n] + " " + n);
            string s = enemies == 0 ? Loc.Get("no_enemies") : Loc.F("map_enemies", enemies, string.Join(", ", kinds));
            return s + ". " + Loc.F("map_others", balls, shots, hazards, pickups);
        }

        private static void Mute(bool mute)
        {
            try
            {
                if (mute)
                {
                    Muted.Clear();
                    var p = InputMgr.I?.GetRewiredPlayer();
                    if (p == null) return;
                    var list = new Il2CppSystem.Collections.Generic.List<ControllerMap>();
                    p.controllers.maps.GetAllMaps(ControllerType.Keyboard, list);
                    for (int i = 0; i < list.Count; i++)
                        if (list[i].enabled) { list[i].enabled = false; Muted.Add(list[i]); }
                }
                else
                {
                    foreach (var m in Muted) m.enabled = true;
                    Muted.Clear();
                }
            }
            catch (Exception ex) { Log.Warn("Map: could not mute game keys: " + ex.Message); }
        }
    }
}

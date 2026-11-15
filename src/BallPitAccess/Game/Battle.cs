using System;
using System.Collections.Generic;
using BallPitAccess.Audio;
using BallPitAccess.Core;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// The battle: state announcements, the enemy danger radar, returning-ball cues, arena edges, damage,
    /// bosses, status effects, pending level ups and the HUD keys. Reads game state every frame; nothing
    /// here changes the game.
    /// Geometry (see CLAUDE.md): world X grows to the right, Y grows toward the enemies (they march down).
    /// </summary>
    internal static class Battle
    {
        internal sealed class Threat
        {
            public GridPieceInst Inst;
            public GridPieceObj Obj;
            public Vector3 Center;
            public float BotY;
            public float Rows;      // rows between the enemy's front edge and the hero
            public float Line;      // rows between the enemy's front edge and the line where enemies attack
            public bool Boss;
        }

        internal static readonly List<Threat> Threats = new List<Threat>();
        internal static bool InBattle { get; private set; }
        internal static bool Playing { get; private set; }
        internal static Vector3 PlayerPos { get; private set; }
        internal static float LeftX, RightX, RowH = 1f, ColW = 1f;
        internal static float MinY, MaxY;

        private static GameState _lastState = GameState.kNum;
        private static bool _hadState;
        private static float _lastHealth = -1f;
        private static float _nextSweep, _nextHeart, _nextHeal;
        private static bool _atLeft, _atRight, _atFront, _atBack, _wasLow;
        private static float _lastSpeed = -1f;
        private static bool _lastAutofire;
        private static bool _loggedGeometry;
        private static float _nextDanger;
        private static int _lastLevelUps = -1;
        private static BallState _lastBallState = BallState.kNum;
        private static readonly HashSet<int> StatusSeen = new HashSet<int>();
        private static readonly HashSet<IntPtr> Charging = new HashSet<IntPtr>();
        private static readonly HashSet<IntPtr> ChargingNow = new HashSet<IntPtr>();
        private static IntPtr _boss = IntPtr.Zero;
        private static int _bossQuarter = 4;
        private static float _stateSince, _bossSeen, _nextThreats, _maxHealth = -1f, _nextMaxHealth;

        private struct Scheduled { public float At; public string Name; public float Pan; public float Gain; public float Muffle; }
        private static readonly List<Scheduled> Pending = new List<Scheduled>();

        internal static void Update()
        {
            _ballWant = false;
            UpdateInner();
            ApplyBallLoop();
        }

        private static void UpdateInner()
        {
            float now = Time.unscaledTime;
            PlayPending(now);

            var gm = GameMgr.I;
            var player = Player.I;
            var save = BattleSaveData.I;
            var grid = GridMgr.I;
            bool battle = gm != null && player != null && save != null && grid != null && grid.IsInited;
            // The shooting tutorial waits for the player to aim with the mouse or the stick. Here the aim
            // assist (or the arrow keys) aims, which the game does not count: count it, or the hint never leaves.
            if (battle)
                try { var gt = GameTutMgr.I; if (gt != null && gt.CurTut == GameTutType.kShootControls && !gt._didAim) gt._didAim = true; } catch { }
            if (battle != InBattle)
            {
                InBattle = battle;
                Threats.Clear();
                Pending.Clear();
                Balls.Clear();
                _hadState = false;
                _lastHealth = -1f;
                _loggedGeometry = false;
                _lastLevelUps = -1;
                _lastBallState = BallState.kNum;
                _boss = IntPtr.Zero;
                _bossQuarter = 4;
                StatusSeen.Clear();
                Charging.Clear();
                Log.Info("In battle: " + battle);
                if (battle) { var muted = Input.CueFilters.MutedSummary(); if (muted != null) Speech.Event(muted); }
            }
            Playing = battle && gm.CurState == GameState.kPlaying;
            if (!battle) return;

            AnnounceState(gm.CurState, save, now);
            AnnounceSettings();

            PlayerPos = player.transform.position;
            LeftX = grid.LeftBorderX;
            RightX = grid.RightBorderX;
            RowH = GridMgr.kSpaceHeight > 0.01f ? GridMgr.kSpaceHeight : 1f;
            ColW = GridMgr.kSpaceWidth > 0.01f ? GridMgr.kSpaceWidth : 1f;
            try { MinY = player.GetMinY(); MaxY = player.GetMaxY(); } catch { MinY = MaxY = PlayerPos.y; }
            if (!_loggedGeometry) { _loggedGeometry = true; LogGeometry(); }

            if (gm.CurState != GameState.kPlaying) return;

            if (now >= _nextThreats) { _nextThreats = now + 0.06f; CollectThreats(save, grid); }
            CheckHealth(save, now);
            CheckLevelUps(save);
            CheckStatus(save);
            CheckBoss();
            CheckBallState();
            if (Config.DangerRadar) Radar(now);
            PredictBall();
            if (Config.BallCues) BallCue(now);
            if (Config.WallCues) WallCue(player);
        }

        private static void AnnounceState(GameState st, BattleSaveData save, float now)
        {
            if (_hadState && st == _lastState) return;
            bool first = !_hadState;
            var prev = _lastState;
            _hadState = true;
            _lastState = st;
            _stateSince = now;
            Log.Info("Battle state: " + st);
            switch (st)
            {
                case GameState.kPlaying:
                    if (first || prev == GameState.kEnteringLvl) Speech.Event(StartText(save));
                    else Speech.Battle(Loc.Get("state_kPlaying"), "state", 3f);
                    break;
                case GameState.kLevelUp:
                case GameState.kBonusBall:
                case GameState.kBonusPassive:
                    Cues.Play("levelup");
                    break;
                case GameState.kPickTreasure:
                case GameState.kFoundBlueprint:
                case GameState.kFoundEgg:
                    Cues.Play("treasure1");
                    break;
                case GameState.kGameOver:
                    {
                        bool won = false;
                        try { won = save.CompletedLevel; } catch { }
                        Cues.Play(won ? "win" : "lose");
                        break;
                    }
                case GameState.kPaused:
                case GameState.kRevive:
                    break;      // the screen that opens is announced by the navigator
                default:
                    Speech.Event(Loc.Get("state_" + st));
                    break;
            }
        }

        private static string StartText(BattleSaveData save)
        {
            string lvl = "";
            try { var lm = LevelMgr.I; if (lm != null) lvl = Names.Level(lm.CurLevelInfo); } catch { }
            string aim = Loc.F("aim_mode", Loc.Get("aim_" + AimAssist.Current));
            // Risen Ballbylon levels weaken the character (the level list explains by how much).
            try { var lm = LevelMgr.I; if (lm != null && DLCUtl.IsDLC(lm.CurLevel)) lvl += (lvl.Length > 0 ? ". " : "") + Loc.Get("altitude_on"); } catch { }
            // With autofire off nothing is shot until the player holds the shoot key: say so at the start.
            string fire = "";
            try { if (!PlayerPrefsMgr.sAutofire) fire = Loc.Get("autofire_off_hint") + ". "; } catch { }
            return (lvl.Length > 0 ? lvl + ". " : "") + aim + ". " + fire + Loc.Get("battle_hint");
        }

        private static void AnnounceSettings()
        {
            try
            {
                float sp = PlayerPrefsMgr.sGameSpeed;
                bool had = _lastSpeed >= 0;
                if (had && Math.Abs(sp - _lastSpeed) > 0.001f)
                    Speech.Battle(Loc.F("speed", FormatSpeed(sp)), "speed", 2f);
                _lastSpeed = sp;
                bool af = PlayerPrefsMgr.sAutofire;
                if (af != _lastAutofire && had)
                    Speech.Battle(Loc.F("autofire", Loc.Get(af ? "on" : "off")), "autofire", 2f);
                _lastAutofire = af;
            }
            catch { }
        }

        internal static string FormatSpeed(float sp) => sp.ToString("0.##") + "x";

        private static void CollectThreats(BattleSaveData save, GridMgr grid)
        {
            Threats.Clear();
            ChargingNow.Clear();
            var pieces = save.Pieces;
            if (pieces == null) return;
            float py = PlayerPos.y;
            float attackY = grid.AttackY;
            for (int i = 0; i < pieces.Count; i++)
            {
                var inst = pieces[i];
                if (inst == null || inst.CurState == PieceState.kDead) continue;
                var obj = inst.Obj;
                if (obj == null || !obj.IsActive) continue;
                bool ally;
                try { ally = obj.IsAlly(); } catch { ally = false; }
                if (ally) continue;
                Vector3 c;
                float bot;
                try { c = obj.GetWorldCenterPos(); bot = obj.GetBotY(); }
                catch { c = obj.transform.position; bot = c.y; }
                var t = new Threat
                {
                    Inst = inst, Obj = obj, Center = c, BotY = bot,
                    Rows = Math.Max(0f, (bot - py) / RowH),
                    Line = Math.Max(0f, (bot - attackY) / RowH),
                    Boss = IsBoss(inst.Type),
                };
                Threats.Add(t);
                if (inst.CurState == PieceState.kChargingAttack || inst.CurState == PieceState.kAttacking)
                {
                    ChargingNow.Add(inst.Pointer);
                    // An enemy winding up: one warning, panned to it. Its own shot or beam is cued by Hazards.
                    if (Charging.Add(inst.Pointer) && Config.DangerRadar && inst.CurState == PieceState.kChargingAttack)
                        Cues.Play("charge", PanFor(c.x), t.Rows < 4f ? 1f : 0.6f);
                }
            }
            Charging.IntersectWith(ChargingNow);
            Threats.Sort((a, b) => a.Rows.CompareTo(b.Rows));
        }

        internal static bool IsBoss(GridPieceType t)
        {
            switch (t)
            {
                case GridPieceType.kSkeletonKing: case GridPieceType.kYetiQueen: case GridPieceType.kPyramidTank:
                case GridPieceType.kSabertooth: case GridPieceType.kShroomSwarm: case GridPieceType.kDragonPrince:
                case GridPieceType.kOwlLord: case GridPieceType.kWhale: case GridPieceType.kQuartet:
                case GridPieceType.kShieldKnight: case GridPieceType.kEightGreats: case GridPieceType.kMoon:
                    return true;
            }
            return false;
        }

        // ---------------- Health, level ups, status, boss, balls ----------------

        private static void CheckHealth(BattleSaveData save, float now)
        {
            float h = save.CurHealth;
            if (_lastHealth >= 0 && h < _lastHealth - 0.01f)
            {
                if (Config.HealthCues) Cues.Play("hurt");
                Speech.Battle(Loc.F("health", Mathf.CeilToInt(h), MaxHealthText()), "health", 1.5f);
            }
            else if (_lastHealth >= 0 && h > _lastHealth + 0.99f && now >= _nextHeal)
            {
                _nextHeal = now + 2f;       // regeneration ticks: one chime now and then, not one per point
                if (Config.HealthCues) Cues.Play("heal", 0f, 0.7f);
            }
            _lastHealth = h;
            if (now >= _nextMaxHealth) { _nextMaxHealth = now + 0.5f; _maxHealth = MaxHealth(); }
            float max = _maxHealth;
            bool low = max > 0 && h > 0 && h / max <= 0.3f;
            if (low && !_wasLow) Speech.Battle(Loc.Get("low_health"), "lowhealth", 3f);
            _wasLow = low;
            if (low && now >= _nextHeart)
            {
                _nextHeart = now + (h / max <= 0.15f ? 0.9f : 1.6f);
                if (Config.HealthCues) Cues.Play("heart", 0f, 0.8f);
            }
        }

        private static void CheckLevelUps(BattleSaveData save)
        {
            int n = save.NumLevelUpsAvail;
            if (_lastLevelUps >= 0 && n > _lastLevelUps)
            {
                Cues.Play("xp");
                // Unless level ups are pooled the game opens the screen by itself a moment later: only the cue then.
                bool pooled = false;
                try { pooled = PlayerPrefsMgr.sPoolLevelUps; } catch { }
                if (pooled) Speech.Battle(Loc.F("levelup_ready", n, ActionKey(GameActionType.kLevelUp)), "levelups", 4f);
            }
            _lastLevelUps = n;
        }

        internal static string ActionKey(GameActionType a)
        {
            try
            {
                var im = InputMgr.I;
                if (im != null)
                {
                    var kb = im.GetKeyboardBtnForAction(a, Il2CppRewired.Pole.Positive);
                    if (kb != KeyboardBtn.kNum) return UI.MenuReader.KeyName(kb);
                }
            }
            catch { }
            return "?";
        }

        private static void CheckStatus(BattleSaveData save)
        {
            var list = save.PlayerStatusEffects;
            if (list == null) return;
            int mask = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null || e.RemainingLen <= 0f) continue;
                int t = (int)e.Type;
                mask |= 1 << t;
                if (StatusSeen.Add(t) && e.Type != PlayerStatusEffectType.kBonusDamage)
                {
                    if (Config.HealthCues) Cues.Play("status");
                    Speech.Battle(Names.PlayerStatus(e.Type), "status", 2.5f);
                }
            }
            StatusSeen.RemoveWhere(t => (mask & (1 << t)) == 0);
        }

        private static void CheckBoss()
        {
            // A boss can be several pieces (quartet, swarm): one boss per kind, health over all its pieces.
            Threat boss = null;
            double cur = 0, max = 0;
            foreach (var t in Threats)
            {
                if (!t.Boss) continue;
                if (boss == null) boss = t;
                if (t.Inst.Type != boss.Inst.Type) continue;
                try { cur += t.Inst.CurHealth; max += t.Inst.MaxHealth; } catch { }
            }
            float now = Time.unscaledTime;
            if (boss == null)
            {
                if (_boss != IntPtr.Zero && now - _bossSeen > 2f) _boss = IntPtr.Zero;
                return;
            }
            _bossSeen = now;
            float pct = max > 0 ? (float)(cur / max) : 1f;
            IntPtr kind = (IntPtr)((int)boss.Inst.Type + 1);
            if (kind != _boss)
            {
                _boss = kind;
                _bossQuarter = Mathf.Clamp(Mathf.CeilToInt(pct * 4f), 1, 4);
                if (Config.DangerRadar) Cues.Play("boss");
                Speech.Event(Loc.F("boss_arrives", Names.Enemy(boss.Inst)));
                // Boss attacks are far easier to dodge by ear at a slower speed.
                float sp = PlayerPrefsMgr.sGameSpeed;
                if (sp > 1.01f) Speech.Event(Loc.F("boss_slow_hint", FormatSpeed(sp), ActionKey(GameActionType.kDecreaseSpeed)));
                return;
            }
            int q = Mathf.Clamp(Mathf.CeilToInt(pct * 4f), 0, 4);
            if (q < _bossQuarter && q >= 1)
            {
                _bossQuarter = q;
                Speech.Battle(Loc.F("boss_health", Mathf.RoundToInt(pct * 100f)), "boss", 4f);
            }
        }

        private static void CheckBallState()
        {
            var bm = BallMgr.I;
            if (bm == null) return;
            BallState st;
            try { st = bm.GetBallState(); } catch { return; }
            if (st != _lastBallState)
            {
                bool auto = false;
                try { auto = PlayerPrefsMgr.sAutofire; } catch { }
                if (st == BallState.kIdle && _lastBallState != BallState.kNum && !auto && Config.BallCues) Cues.Play("ready", 0f, 0.7f);
                _lastBallState = st;
            }
        }

        // ---------------- Danger radar ----------------

        /// <summary>
        /// Every sweep, tick the three nearest enemies in order, panned to their side of the hero, pitched by
        /// closeness. Sweeps come faster as the nearest enemy gets closer. Bosses tick with a square wave.
        /// </summary>
        private static void Radar(float now)
        {
            if (Threats.Count == 0 || now < _nextSweep) return;
            float nearest = Threats[0].Rows;
            _nextSweep = now + Mathf.Clamp(0.3f + nearest * 0.12f, 0.3f, 1.6f);
            int n = Math.Min(3, Threats.Count);
            for (int i = 0; i < n; i++)
            {
                var t = Threats[i];
                int step = Mathf.Clamp(Cues.EnemySteps - 1 - (int)(t.Rows * 0.9f), 0, Cues.EnemySteps - 1);
                Schedule(now + i * 0.09f, (t.Boss ? "boss" : "enemy") + step, PanFor(t.Center.x), i == 0 ? 1f : 0.7f, Muffle(t.Center.y));
            }
            if ((nearest < 1.5f || Threats[0].Line < 1f) && now >= _nextDanger)
            {
                _nextDanger = now + 2.5f;
                Schedule(now + n * 0.09f, "danger", PanFor(Threats[0].Center.x), 1f, Muffle(Threats[0].Center.y));
            }
        }

        internal static float PanFor(float x)
        {
            float half = Math.Max(0.5f, (RightX - LeftX) * 0.5f);
            return Mathf.Clamp((x - PlayerPos.x) / half, -1f, 1f);
        }

        /// <summary>
        /// Pan for something the hero should walk to (a pickup, where a ball lands): never vague. Inside
        /// <paramref name="dead"/> columns of the hero it is centred (walk straight ahead or back); outside it
        /// is at least half way to that side, so even one column off is clearly left or right.
        /// </summary>
        internal static float GuidePan(float x, float dead)
        {
            float off = x - PlayerPos.x;
            if (Math.Abs(off) <= dead) return 0f;
            float half = Math.Max(0.5f, (RightX - LeftX) * 0.5f);
            float t = Mathf.Clamp01((Math.Abs(off) - dead) / half);
            return Math.Sign(off) * (0.5f + 0.5f * t);
        }

        /// <summary>
        /// How muffled a cue for something at height <paramref name="y"/> sounds: 0 ahead of the hero or on its
        /// row, growing from 0.6 rows behind to fully muffled at 2 rows behind (CUES.md: muffled = behind you).
        /// </summary>
        internal static float Muffle(float y)
        {
            float behind = (PlayerPos.y - y) / RowH;
            return Mathf.Clamp01((behind - 0.6f) / 1.4f);
        }

        private static void Schedule(float at, string name, float pan, float gain, float muffle = 0f) =>
            Pending.Add(new Scheduled { At = at, Name = name, Pan = pan, Gain = gain, Muffle = muffle });

        private static void PlayPending(float now)
        {
            for (int i = 0; i < Pending.Count; i++)
            {
                if (Pending[i].At > now) continue;
                Cues.Play(Pending[i].Name, Pending[i].Pan, Pending[i].Gain, Pending[i].Muffle);
                Pending.RemoveAt(i--);
            }
        }

        // ---------------- Returning balls ----------------

        /// <summary>The next main ball coming down to the hero's line (not one already flying home): where it
        /// lands (side walls folded in), in how many seconds of game time, and whether it is clear of the
        /// enemies (no more bounces but the walls, so the landing spot is final). Updated every frame.</summary>
        internal static bool BallComing { get; private set; }
        internal static float BallLandX { get; private set; }
        internal static float BallEta { get; private set; }
        internal static bool BallClear { get; private set; }

        private struct BallSeen { public BallObjState State; public Vector3 Pos; public float Seen; }
        private static readonly Dictionary<IntPtr, BallSeen> Balls = new Dictionary<IntPtr, BallSeen>();
        private static readonly List<IntPtr> BallsGone = new List<IntPtr>();
        private static float _lastBallOff;

        private static void PredictBall()
        {
            BallComing = false;
            var bm = BallMgr.I;
            if (bm == null || bm.ActiveBalls == null) return;
            float now = Time.unscaledTime;
            float py = PlayerPos.y, best = float.MaxValue, landX = 0, ballY = 0;
            for (int i = 0; i < bm.ActiveBalls.Count; i++)
            {
                var b = bm.ActiveBalls[i];
                if (b == null || !b.IsActive || b.IsSide) continue;
                bool baby;
                try { baby = b.IsBaby(); } catch { baby = false; }
                if (baby) continue;
                Vector3 p = b.transform.position, d = b.AimDir;
                var st = b.CurState;
                // A main ball that went from flying to flying home near the hero's line got past: a miss.
                if (Balls.TryGetValue(b.Pointer, out var prev) && prev.State == BallObjState.kNormal
                    && st == BallObjState.kReturning && prev.Pos.y < py + RowH * 1.5f)
                    OnBallMissed(prev.Pos);
                Balls[b.Pointer] = new BallSeen { State = st, Pos = p, Seen = now };
                if (st != BallObjState.kNormal) continue;
                if (d.y >= -0.05f || p.y <= py) continue;
                float k = (py - p.y) / d.y;
                float tta = k / Math.Max(0.1f, b.GetSpeed());
                if (tta < best) { best = tta; landX = Fold(p.x + d.x * k); ballY = p.y; }
            }
            // Forget balls not seen for a second (caught, destroyed).
            BallsGone.Clear();
            foreach (var kv in Balls) if (now - kv.Value.Seen > 1f) BallsGone.Add(kv.Key);
            foreach (var k in BallsGone) Balls.Remove(k);
            if (best == float.MaxValue) return;
            BallComing = true;
            BallLandX = landX;
            BallEta = best;
            float lowest = float.MaxValue;
            foreach (var t in Threats) if (t.BotY < lowest) lowest = t.BotY;
            BallClear = ballY < lowest;
            _lastBallOff = landX - PlayerPos.x;
        }

        private static LoopVoice _ballLoop;
        private static bool _ballWant, _wasInline;
        private static float _bGain, _bPan, _bPitch, _bPulse;

        /// <summary>
        /// The ball you should catch, by ear (the catch assist is optional), as one continuous sound (a steel
        /// ball rolling with a hum) that follows it every frame:
        /// - pan: the side to walk to, graded (further off = further over; centred = stay);
        /// - pitch: how close you are to where it lands, an octave low at 3 columns off, rising as you close in,
        ///   a step up and a click when you are under it;
        /// - throb: 3 a second while it is far in time, up to 13 a second just before it reaches your line;
        /// - volume: softer while it is still among the enemies (the spot can still change), full once clear.
        /// </summary>
        private static void BallCue(float now)
        {
            if (!BallComing || Input.ModKeys.ListOpen) { _wasInline = false; return; }
            float real = BallEta / Math.Max(0.25f, PlayerPrefsMgr.sGameSpeed);
            float off = BallLandX - PlayerPos.x, dead = CatchHalfWidth;
            bool inline = Math.Abs(off) < dead;
            if (inline && !_wasInline) Cues.Play("ball_inline", 0f, 0.6f);
            _wasInline = inline;
            float cols = Mathf.Clamp01((Math.Abs(off) - dead) / (ColW * 3f));
            _ballWant = true;
            _bGain = BallClear ? 0.7f : 0.4f;
            _bPan = GuidePan(BallLandX, dead);
            _bPitch = inline ? 1f : 0.89f * Mathf.Pow(2f, -cols);
            _bPulse = 3f + 10f * (1f - Mathf.Clamp01(real / 1.5f));
        }

        /// <summary>Hand this frame's ball targets to the loop once (silent when no ball is coming, paused,
        /// or out of battle), so the audio thread never sees a half-set frame.</summary>
        private static void ApplyBallLoop()
        {
            if (_ballLoop == null)
            {
                if (!_ballWant) return;
                _ballLoop = Cues.Loop("loop_ball");
                if (_ballLoop == null) return;
            }
            if (!_ballWant) { _ballLoop.Gain = 0f; return; }
            _ballLoop.Pan = _bPan;
            _ballLoop.Pitch = _bPitch;
            _ballLoop.Pulse = _bPulse;
            _ballLoop.Gain = _bGain;
        }

        /// <summary>How far from the hero's centre a ball still lands on it (tuned from the catch / miss log lines).</summary>
        internal static float CatchHalfWidth => Player.kColHalfWidth + 0.15f;

        private static void OnBallMissed(Vector3 at)
        {
            Log.Info($"Ball missed: last seen {at.x - PlayerPos.x:+0.00;-0.00} from the hero, predicted off {_lastBallOff:+0.00;-0.00}, catch half width {CatchHalfWidth:0.00}");
            if (Config.BallCues) Cues.Play("ball_miss", PanFor(at.x), 0.8f);
        }

        private static float Fold(float x)
        {
            float w = RightX - LeftX;
            if (w <= 0.1f) return x;
            float u = (x - LeftX) % (2 * w);
            if (u < 0) u += 2 * w;
            if (u > w) u = 2 * w - u;
            return LeftX + u;
        }

        internal static void OnBallRemoved(BallReturnType type)
        {
            if (type != BallReturnType.kCatch) return;
            Log.Info($"Ball caught: predicted off {_lastBallOff:+0.00;-0.00}, catch half width {CatchHalfWidth:0.00}");
            if (Config.BallCues) Cues.Play("catch", 0f, 0.8f);
        }

        // ---------------- Arena edges ----------------

        private static void WallCue(Player player)
        {
            float x = PlayerPos.x, y = PlayerPos.y;
            bool atL, atR;
            try { atL = x <= player.GetMinX() + 0.05f; atR = x >= player.GetMaxX() - 0.05f; }
            catch { return; }
            if (atL && !_atLeft) Cues.Play("wall", -1f);
            if (atR && !_atRight) Cues.Play("wall", 1f);
            _atLeft = atL; _atRight = atR;
            if (MaxY - MinY > 0.2f)
            {
                bool atF = y >= MaxY - 0.05f, atB = y <= MinY + 0.05f;
                if (atF && !_atFront) Cues.Play("wall_v", 0f, 1f);
                if (atB && !_atBack) Cues.Play("wall_v", 0f, 0.6f);
                _atFront = atF; _atBack = atB;
            }
        }

        // ---------------- HUD keys ----------------

        internal static void SayHealth()
        {
            if (!InBattle) { Speech.SayMenu(Loc.Get("not_in_battle")); return; }
            var save = BattleSaveData.I;
            string s = Loc.F("health", Mathf.CeilToInt(save.CurHealth), MaxHealthText());
            var fx = new List<string>();
            try
            {
                var list = save.PlayerStatusEffects;
                for (int i = 0; list != null && i < list.Count; i++)
                    if (list[i] != null && list[i].RemainingLen > 0f) fx.Add(Names.PlayerStatus(list[i].Type));
            }
            catch { }
            if (fx.Count > 0) s += ". " + string.Join(", ", fx);
            Speech.SayMenu(s);
        }

        private static string MaxHealthText()
        {
            try
            {
                var ui = InGameUI.I;
                if (ui != null && ui.TxtTotalHealth != null) return Speech.Clean(ui.TxtTotalHealth.text).Trim('/', ' ');
            }
            catch { }
            return "?";
        }

        private static float MaxHealth()
        {
            string s = MaxHealthText();
            var digits = new System.Text.StringBuilder();
            foreach (char c in s) if (char.IsDigit(c)) digits.Append(c);
            return digits.Length > 0 && float.TryParse(digits.ToString(), out float v) ? v : -1f;
        }

        internal static void SayEnemies()
        {
            if (!InBattle) { Speech.SayMenu(Loc.Get("not_in_battle")); return; }
            if (Threats.Count == 0) { Speech.SayMenu(Loc.Get("no_enemies")); return; }
            var t = Threats[0];
            string s = Loc.F("enemies", Threats.Count, Side(t.Center.x), t.Rows.ToString("0.#"));
            foreach (var b in Threats)
                if (b.Boss) { s = BossText(b) + ". " + s; break; }
            Speech.SayMenu(s);
            var lines = new List<string>();
            for (int i = 0; i < Math.Min(12, Threats.Count); i++)
                lines.Add(Describe(Threats[i]));
            Buffers.SetDetails(lines);
        }

        private static string BossText(Threat b)
        {
            float pct = b.Inst.MaxHealth > 0 ? (float)b.Inst.CurHealth / b.Inst.MaxHealth * 100f : 100f;
            return Names.Enemy(b.Inst) + ", " + Loc.F("boss_health", Mathf.RoundToInt(pct));
        }

        internal static string Describe(Threat t)
        {
            string s = Loc.F("enemy_line", Names.Enemy(t.Inst), Side(t.Center.x), t.Rows.ToString("0.#"), t.Inst.CurHealth);
            if (t.Inst.CurState == PieceState.kChargingAttack) s += ", " + Loc.Get("enemy_charging");
            return s;
        }

        /// <summary>"center", "2 left", "3 right": columns from the hero.</summary>
        internal static string Side(float x)
        {
            float off = x - PlayerPos.x;
            if (Math.Abs(off) < ColW * 0.6f) return Loc.Get("center");
            int n = Mathf.Max(1, Mathf.RoundToInt(Math.Abs(off) / ColW));
            return Loc.F(off < 0 ? "n_left" : "n_right", n);
        }

        /// <summary>"2 left, 3 ahead": columns and rows from the hero.</summary>
        internal static string Where(Vector3 p)
        {
            float dy = (p.y - PlayerPos.y) / RowH;
            string side = Side(p.x);
            if (Math.Abs(dy) < 0.6f) return side;
            return side + ", " + Loc.F(dy > 0 ? "n_ahead" : "n_behind", Mathf.Max(1, Mathf.RoundToInt(Math.Abs(dy))));
        }

        internal static void SayPosition()
        {
            if (!InBattle) { Speech.SayMenu(Loc.Get("not_in_battle")); return; }
            float w = RightX - LeftX;
            float pct = w > 0.1f ? (PlayerPos.x - LeftX) / w * 100f : 50f;
            string s = Loc.F("position", Mathf.RoundToInt(pct));
            if (MaxY - MinY > 0.2f)
                s += ". " + Loc.F("position_depth", Mathf.RoundToInt((PlayerPos.y - MinY) / (MaxY - MinY) * 100f));
            Speech.SayMenu(s);
        }

        internal static void SaySpeed()
        {
            try { Speech.SayMenu(Loc.F("speed", FormatSpeed(PlayerPrefsMgr.sGameSpeed)) + ". " + Loc.F("autofire", Loc.Get(PlayerPrefsMgr.sAutofire ? "on" : "off"))); }
            catch { }
        }

        /// <summary>Run progress: level, pending level ups, time, kills, resources.</summary>
        internal static void SayProgress()
        {
            if (!InBattle) { Speech.SayMenu(Loc.Get("not_in_battle")); return; }
            var save = BattleSaveData.I;
            var lines = new List<string>();
            try
            {
                lines.Add(Loc.F("prog_level", save.UpgradeLvl + 1));
                if (save.NumLevelUpsAvail > 0) lines.Add(Loc.F("levelup_ready", save.NumLevelUpsAvail, ActionKey(GameActionType.kLevelUp)));
                lines.Add(Loc.F("prog_time", save.GetElapsedTimeStr()));
                var lm = LevelMgr.I;
                if (lm != null && !save.IsEndless)
                {
                    int final = lm.GetFinalTurn();
                    if (final > 0) lines.Add(Loc.F("prog_wave", save.CurTurn, final));
                    int nextBoss = lm.GetNextBossTurn();
                    if (nextBoss > save.CurTurn) lines.Add(Loc.F("prog_boss", nextBoss - save.CurTurn));
                }
                lines.Add(Loc.F("prog_kills", save.NumKills));
                if (save.NumGearsGotten > 0) lines.Add(Loc.F("prog_gears", save.NumGearsGotten));
                string res = Names.Cost(save.NumResources);
                if (res.Length > 0) lines.Add(res);
            }
            catch (Exception ex) { Log.Warn("Progress: " + ex.Message); }
            if (lines.Count == 0) return;
            Speech.SayMenu(string.Join(". ", lines));
            Buffers.SetDetails(lines);
        }

        /// <summary>Balls and passives carried, with levels; whether the hero can shoot now.</summary>
        internal static void SayEquipment()
        {
            if (!InBattle) { Speech.SayMenu(Loc.Get("not_in_battle")); return; }
            var save = BattleSaveData.I;
            var lines = new List<string>();
            string head = "";
            try
            {
                var bm = BallMgr.I;
                if (bm != null)
                {
                    int away = bm.GetNumActiveNotReturningBalls();
                    head = bm.IsReadyToShoot() ? Loc.Get("balls_ready") : Loc.F("balls_away", away);
                    var cur = bm.GetCurHero();
                    if (cur != null) head += ". " + Loc.F("balls_next", Names.Upgrade(cur.GetInfo()));
                }
                var heroes = save.Heroes;
                for (int i = 0; heroes != null && i < heroes.Count; i++)
                    if (heroes[i] != null) lines.Add(Loc.F("equip_ball", Names.Upgrade(heroes[i].GetInfo()), heroes[i].Lvl));
                var pas = save.Passives;
                for (int i = 0; pas != null && i < pas.Count; i++)
                    if (pas[i] != null) lines.Add(Loc.F("equip_passive", Names.Upgrade(pas[i].GetInfo()), pas[i].Lvl));
            }
            catch (Exception ex) { Log.Warn("Equipment: " + ex.Message); }
            Speech.SayMenu(head + (lines.Count > 0 ? ". " + string.Join(", ", lines) : ""));
            Buffers.SetDetails(lines);
        }

        private static void LogGeometry()
        {
            try
            {
                var g = GridMgr.I;
                var p = Player.I;
                Log.Info($"Geometry: player {PlayerPos} minX {p.GetMinX()} maxX {p.GetMaxX()} minY {p.GetMinY()} maxY {p.GetMaxY()}; " +
                         $"grid L {g.LeftBorderX} R {g.RightBorderX} T {g.TopBorderY} B {g.BottomBorderY} attackY {g.AttackY} frontEnemyY {g.FrontEnemyY} " +
                         $"space {GridMgr.kSpaceWidth}x{GridMgr.kSpaceHeight}; rows {g.TopGridRow}..{g.BotGridRow}");
            }
            catch (Exception ex) { Log.Warn("Geometry log failed: " + ex.Message); }
        }

        internal static void DumpThreats()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"player {PlayerPos} L {LeftX} R {RightX} rowH {RowH} minY {MinY} maxY {MaxY}");
            foreach (var t in Threats)
                sb.AppendLine($"{t.Inst.Type} state {t.Inst.CurState} grid ({t.Inst.X},{t.Inst.Y}) center {t.Center} bot {t.BotY} rows {t.Rows:0.00} line {t.Line:0.00} hp {t.Inst.CurHealth}/{t.Inst.MaxHealth}");
            var bm = BallMgr.I;
            if (bm != null && bm.ActiveBalls != null)
                for (int i = 0; i < bm.ActiveBalls.Count; i++)
                {
                    var b = bm.ActiveBalls[i];
                    if (b == null) continue;
                    sb.AppendLine($"ball {b.CurState} side {b.IsSide} pos {b.transform.position} dir {b.AimDir} speed {b.GetSpeed()}");
                }
            sb.Append(Hazards.Dump());
            sb.Append(Pickups.Dump());
            Log.WriteFile("threats.txt", sb.ToString());
        }
    }
}

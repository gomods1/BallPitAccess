# Audio cues

One mapping everywhere: **pan** = left/right of the hero, **pitch** = closeness (higher = closer),
**muffled** = behind the hero, **repetition rate** = urgency, **timbre** = category. A cue ending in `_hit`
means "this will hit where you stand: move"; its plain version means "this exists, on that side".

**Cues that ask you to walk point the way to go** (user rule, 2026-11-15): the ball clank and the pickup
beacons use `Battle.GuidePan`: centred inside a dead zone (you are lined up: stay, or walk straight ahead /
back), otherwise at least half way to that side, so one column off is never vague. The `_hit` cues are
panned to the side to **step to** (away from the threat, or toward the side with more room when a wall is
in the way), not to where the threat is. Information cues (enemy radar, `shot`, `beam`, `bomb`, `charge`)
stay panned to where the thing is.

**Things that move continuously sound continuously** (user, 2026-11-15: "why just clanks … balls move
continuously"). `Audio/LoopVoice.cs`: a looping voice whose pan, pitch, volume, muffle and throb rate the
game thread sets every frame and the audio thread glides to (~40 ms). Sources are `loop_*` WAVs, made
seamless at load (tail crossfaded into head, 50 ms). Things that appear or change in steps (an enemy row
advancing, a pickup lying there, a bomb launched) stay one-shot cues.

**Behind = muffled** (`Battle.Muffle(y)`, `AudioEngine` per-voice filter): from 0.6 rows behind the hero
to fully muffled at 2 rows, a 12 dB/octave low-pass glides from 5 kHz to 700 Hz (700, not lower, so the
gem tink at 880 to 1570 Hz stays audible), with some gain given back. Applied to the radar, `danger` and
both pickup beacons. F4 → Sound guide has a muffled sample ("pickup2:behind").

Since 2026-11-14 (user playtest: "all the cues are the same, hard to tell enemies from gems") every cue is
a WAV made with **genny** by `tools/make_cues.py` (specs in `src/BallPitAccess/Sounds/cues.json`), embedded
in the DLL, 48 kHz mono. **Each category is its own physical family**, so the timbre names the category
before pitch or pan say where. A WAV of the same name in `UserData/BallPitAccess/sounds/` replaces the
embedded one (no rebuild). To change a sound: edit `tools/make_cues.py`, run it, rebuild.
Every cue can be heard with its meaning in **F4 → Sound guide**.

| Cue | Family / sound | Meaning | Setting |
|---|---|---|---|
| enemy0..9 | **hand drum** (slapped conga), 150 Hz far .. 420 Hz close | Danger radar: the three nearest enemies each sweep, nearest first, panned to their side. Sweeps repeat every 0.3 s (close) to 1.6 s (far). | radar |
| boss0..9 | **big drum** (taiko + tom + rim), 55 .. 125 Hz | A boss in the radar. | radar |
| danger | monster snarl + drum slap | The nearest enemy is within 1.5 rows of the hero or one row of the attack line (at most every 2.5 s). | radar |
| charge | energy whine climbing | An enemy entered `kChargingAttack`, panned to it. | hazards |
| boss | two taiko hits, gong, low horn | A boss appeared (its name is spoken; health at 75, 50, 25 %). | always |
| loop_ball | **steel ball rolling + E4 hum**, continuous | Follows the main ball that will reach the hero's line first, every frame, made to be caught by ear: pan = side to walk to (graded); **pitch = how close you are to where it lands** (0.45 at 3+ columns off, rising to 0.89 next to it, 1.0 under it); **throb** 3/s far in time up to 13/s just before it lands; **soft while still among the enemies** (can still bounce), full once below them; silent when no ball is coming. Shift (catch assist) is optional. | balls |
| ball_inline | double hard metal clank | Played once when you step under the ball (within `Battle.CatchHalfWidth`): stay. | balls |
| catch | glove thud (rubber thup + punch) | You caught a ball. `Latest.log`: "Ball caught: predicted off …". | balls |
| ball_miss | steel on wood thud + sinking tone | A main ball got past you (kNormal → kReturning near your line), panned where it went. `Latest.log`: "Ball missed: …" (to tune the catch width). | balls |
| ready | gun rack "ka-chak" | Balls are back: you can shoot (silent with autofire on). | always |
| pickup0..2 | **glass tink** A5 / D6 / G6 | Nearest ordinary pickup (XP crystals, gold, resources: they do not collect themselves) you can walk to, on its own timer: quieter and slower while a special one is beaconed. 2 = your row, 1 = one or two rows away, 0 = further; muffled = behind. | pickups (mode "all") |
| treasure0..2 | **kalimba** 4-note sparkle + key jingle, C5 / F5 / A#5 | Nearest special pickup you can walk to (fuser, blueprint, egg, gear, health, magnet, bomb, gem bag). Same pitch / muffle rule. Repeats 0.4–1.2 s by distance. | pickups |
| loop_shot | **airy flute whistle + wind**, continuous | The enemy shot that matters most (will hit first, else passes closest within 2.5 columns), following it: pan = where the shot is, pitch = how close (0.6 at 9 rows, 1.2 on you), louder when it will hit, muffled if behind. Replaces the old one-shot `shot` swoosh. | hazards |
| shot_hit | swoosh + crack | A projectile's path crosses the hero. Panned to the side to step to. Every 0.28 s. | hazards |
| beam | electric hum | A laser started aiming (once per laser). | hazards |
| beam_hit | two electric zaps | The hero is inside a laser's line. Panned to the side to step to. | hazards |
| bomb | falling whistle | A lobbed bomb targets a spot within 5 columns (once per bomb). | hazards |
| bomb_hit | fuse sizzle + thump | The hero is inside a bomb's blast radius. Panned to the side to step to. | hazards |
| floor | wet squelch | The hero stepped on a floor hazard (its name is spoken). | hazards |
| hurt | punch | You took damage (health is spoken too). | always |
| heal | gulp + sparkle | Healed by a point or more (at most every 2 s). | always |
| heart | heartbeat lub-dub | Health at 30 % or less: every 1.6 s, every 0.9 s under 15 %. | always |
| status | woozy falling wobble | A status effect landed on the hero (its name is spoken). | always |
| xp | two vibraphone notes up | A level up became available. | always |
| levelup | harp sweep up | The level up / bonus menu opened. | always |
| wall | wooden door knock | Left or right edge of the arena; also the end of a menu. | edges |
| wall_v | metal door knock | Front (louder) or back limit of the hero's walking area. | edges |
| retarget | light-switch click | Aim assist switched to another target, panned to it. | always |
| win / lose | brass fanfare + drum / falling felt piano + gong | The run ended. | always |
| tick | tiny relay click | Settings preview; placement "fits". | always |

**Number keys in battle** (`Input/CueFilters.cs`; the game binds none of 1..0) mute a group to concentrate:
1 enemies (radar, danger, charge, boss arrival), 2 balls (loop, under-it click, catch, miss, ready),
3 rewards (all pickups), 4 shots and hazards (sounds only: the scan goes on for the catch assist and N),
5 edges, 6 your health (hurt, heal, heart, status), 7 aim click. Ctrl+number = only that group;
0 (with or without Ctrl) = every group and the master switch back on. Same settings as F4, so they persist; muted groups are spoken when a battle
starts. Never filtered except by 0: xp, levelup, win / lose, the treasure fanfare.

"always" cues still obey the master switch and volume. Status: genny set rendered and deployed 2026-11-14, **not yet heard by the user**. Known guesses to check by ear: the pickup blip may be too frequent in gem-heavy moments (setting
"special pickups only" removes it); `shot` uses distance, not time, so fast projectiles warn late.

# Audio cues

One mapping everywhere: **pan** = left/right of the hero, **pitch** = closeness (higher = closer),
**muffled** = behind the hero, **repetition rate** = urgency, **timbre** = category. A cue ending in `_hit`
means "this will hit where you stand: move"; its plain version means "this exists, on that side".

**Every cue sounds where its thing is, relative to the hero** (user rule, 2026-11-16, replacing the
"guide pan" of 2026-11-15, which panned walk-to cues to the side to go to and `_hit` cues to the side to
step to: "the sound has to be relative to your character: an enemy on your left sounds on the left, one
behind sounds behind; pan and pitch by the distance to that thing"). Every pan is `Battle.PanFor(x)`
(linear, hero's x to the field's half width), every behind is `Battle.Muffle(y)`. The ball hum sounds
from where the ball is (user, 2026-11-16: "the player is not dumb, they can tell roughly where it will
go"); the landing spot is only the `ball_inline` click; `shot_hit` from the shot, `beam_hit` from the laser's
emitter, `bomb_hit` from where the bomb lands. The side to step to is the player's call (or the catch
assist's: `Hazards.DodgeDir` still picks it, but no sound uses it).

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
| enemy0..9 | **hand drum** (slapped conga), 150 Hz far .. 420 Hz close | Danger radar: up to three nearest enemies each sweep, nearest first, panned to their side (the second and third only within 3 rows, and not when they would repeat an earlier drum's side and pitch). A sweep that would sound exactly like the last one plays only the nearest drum (a full sweep at least every 4 s). Sweeps repeat every 0.5 s (close) to 2.2 s (far), never within 0.6 s of a hazard alarm. | radar |
| boss0..9 | **big drum** (taiko + tom + rim), 55 .. 125 Hz | A boss in the radar. | radar |
| danger | monster snarl + drum slap | The nearest enemy is within 1.5 rows of the hero or one row of the attack line (at most every 2.5 s). | radar |
| charge | energy whine climbing | An enemy entered `kChargingAttack`, panned to it. | hazards |
| boss | two taiko hits, gong, low horn | A boss appeared (its name is spoken; health at 75, 50, 25 %). | always |
| loop_ball | **steel ball rolling + E4 hum**, continuous | Follows the main ball that will reach the hero's line first, every frame, made to be caught by ear: pan and muffle = where the ball is now, relative to the hero; **pitch = distance from the ball to the hero** (1.0 within a row, an octave lower at 12 rows or more); **throb** 3/s far in time up to 13/s just before it lands; **soft while still among the enemies** (can still bounce), full once below them; silent when no ball is coming. Shift (catch assist) is optional. | balls |
| ball_inline | double hard metal clank | Played once when you step onto the spot where the ball will land (within `Battle.CatchHalfWidth`): stay. | balls |
| catch | glove thud (rubber thup + punch) | You caught a ball. `Latest.log`: "Ball caught: predicted off …". | balls |
| ball_miss | steel on wood thud + sinking tone | A main ball got past you (kNormal → kReturning near your line), panned where it went. `Latest.log`: "Ball missed: …" (to tune the catch width). | balls |
| ready | gun rack "ka-chak" | Balls are back: you can shoot (silent with autofire on). | always |
| pickup0..2 | **glass tink** A5 / D6 / G6 | The ordinary pickup (XP crystals, gold, resources: they do not collect themselves) to walk to: the nearest, or a heap a little further (each neighbour within 1 unit counts as 0.7 units less walking). Waits for a radar sweep and while a ball is coming. On its own timer: quieter and slower while a special one is beaconed. Pitch by straight-line distance to the hero: 2 = under 0.8 rows away, 1 = up to 2.5 rows, 0 = further; pan = where it lies; muffled = behind. | pickups (mode "all") |
| treasure0..2 | **kalimba** 4-note sparkle + key jingle, C5 / F5 / A#5 | Nearest special pickup you can walk to (fuser, blueprint, egg, gear, health, magnet, bomb, gem bag). Same pitch / muffle rule. Repeats 0.4–1.2 s by distance. | pickups |
| loop_shot | **airy flute whistle + wind**, continuous | The enemy shot that matters most (will hit first, else passes closest within 2.5 columns), following it: pan = where the shot is, pitch = how close (0.6 at 9 rows, 1.2 on you), louder when it will hit, muffled if behind. Replaces the old one-shot `shot` swoosh. | hazards |
| shot_hit | swoosh + crack | A projectile's path crosses the hero. Panned and muffled where the shot is. Every 0.28 s. | hazards |
| beam | electric hum | A laser started aiming (once per laser). | hazards |
| beam_hit | two electric zaps | The hero is inside a laser's line. Panned and muffled where the laser's emitter is. | hazards |
| bomb | falling whistle | A lobbed bomb targets a spot within 5 columns (once per bomb). | hazards |
| bomb_hit | fuse sizzle + thump | The hero is inside a bomb's blast radius. Panned and muffled where the bomb lands. | hazards |
| floor | wet squelch | The hero stepped on a floor hazard (its name is spoken). | hazards |
| hurt | punch | You took damage (health is spoken too). | always |
| heal | gulp + sparkle | Healed by a point or more (at most every 2 s). | always |
| heart | heartbeat lub-dub | Health at 30 % or less: every 1.6 s, every 0.9 s under 15 %. | always |
| status | woozy falling wobble | A status effect landed on the hero (its name is spoken). | always |
| xp | two vibraphone notes up | A level up became available. | always |
| levelup | harp sweep up | The level up / bonus menu opened. | always |
| wall | wooden door knock | Left or right edge of the arena; also the end of a menu. | edges |
| wall_v | metal door knock | Front (louder) or back limit of the hero's walking area. | edges |
| retarget | light-switch click | Aim assist switched to another target, panned to it. After a kill: silent with autofire, otherwise only when the aim swings more than 1.5 columns. | always |
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

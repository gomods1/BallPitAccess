# Status

## Current state (2026-11-14, session 4)

Game 1.308 with the Risen Ballbylon DLC owned, MelonLoader 0.7.3, Prism 0.18.3, mod **0.2.0** + DLC pass
(built and copied to `Mods/`, 0 build errors).
Test save: the base tutorial has been played up to the first harvest summary (wheat field and forest
built, one harvest done).

**Session 2 ran no game at all** (the user tests). Everything it added is "written", not verified.
PLAYTEST.md lists what to exercise, in order, and which logs to send back.

### Verified in the running game (session 1, unchanged code paths unless noted)
- Mod loads; NVDA speaks. The mod's strings follow the game language (English and Spanish tables).
- Main menu: arrows move, Enter activates (New game, Continue).
- Popups (`OverlayUI` stack) are announced with their text and get default focus.
- Guided tutorial (FTUE): instruction read, focus jumps to the button it points at; continue works.
- Build list items: "Campo de trigo, oro 30"; description in details.
- Building placement: arrows, fits / blocked, Enter builds.
- Harvest: Tab cycles buildings, Left / Right turn, Enter launches.
- Tutorial texts: mouse glyphs replaced; rewrites by I2 term.

Session 2 touched these verified paths; re-check them first (PLAYTEST section 1 and 2):
`Navigator` (rewritten around the same Move / Focus / Activate / Candidates code; `ScreenPos` now uses
the rect centre through `TryCast<RectTransform>`, before it was the pivot because `as` never matched),
`MenuReader` (0.08 s settle, domain readers first), `Speech.Pump` (a focus echo now interrupts only a
previous focus echo, so it no longer cuts a screen's announcement), `Placement` (first position is
announced after 0.5 s; Home uses the game's `GetWorldPos`).

### Written in session 2, not yet run
- **Battle**: boss arrival + health quarters, status effects, low-health heartbeat, heal, "level ups
  ready, press E", ready-to-shoot blip, win / lose stingers, front / back edge knock, level name at start.
  HUD keys L (progress), B (balls and passives), P (pickups), N (hazards); H adds status effects, J uses
  the game's enemy names and the boss. Arrow keys during play: Left / Right turn the aim 5°, Up back to
  the assist, Down says the aim. The assist now clamps with the character's own `AimMode` and stays out
  of back / side shooters.
- **Hazards** (`Game/Hazards.cs`): enemy projectiles (`ArrowObj`), lasers (`EnemyLaserObj`), bombs
  (prefix on `CannonballObj.Run`), floor hazards (`ObstacleObj`), enemies charging an attack.
- **Pickups** (`Game/Pickups.cs`): drop announcement and beacon for special pickups, soft blip for
  ordinary ones within walking reach.
- **Menus**: readers for about 40 item types (`UI/Readers.cs`), screen titles in the game language,
  page / tab tracking, Tab / Shift+Tab sections, Backspace = `OnBPressed`, settings rows with Left /
  Right, text field echo (pet name), lazy tooltip + screen text under Ctrl+Up, auto-focus when a screen
  has none, run summary waits for its counters.
- **Base** (`Game/BaseNav.cs`): Tab through buildings, Enter click, Delete right-click, expansion plots,
  rearrange pick-up, mode announcements, H resources, J harvest.
- **Scenes**: boot screens, intro captions + Enter to skip, credits.
- **Sound guide** in F4; three more settings (hazards, pickup sounds, read descriptions).
- An independent read-through of the new code found 15 defects (soft-lock when a popup opens during
  placement, bombs forgotten across a pause, boss re-announced for multi-piece bosses, Escape leaking to
  the game when closing a mod list, an out-of-range cursor in the Events buffer…); all are fixed.

### Session 3 (2026-11-14): Risen Ballbylon DLC, written, not run (the user tests; PLAYTEST section 6)
The DLC (`DLCId.kBallbylonRise`, shown as "Risen Ballbylon" / "Bolabilonia Resurgida") has no depot of its
own: its code and data were already in build 1.308, gated by `DLCUtl.CanAccessDLC`. Audit result: battle
(DLC bosses, shots, bombs, live arena borders), level items, pickups (`kGearDLC`), encyclopedia set in
the page key and the purchase message were already covered by the generic code. Added:
- **Lift in the base Tab list** (first entry, normal mode only): the game's hover + popup
  (`BaseElevatorObj.SetHover`, `BaseHoverPopup.HoverElevator`), Enter = `UpgradeBuildingUI.ActivateElevator()`
  when `BaseMgr.IsElevatorUpgradeVisible()`, otherwise the popup text (gear counts) is spoken.
- Lift upgrade screen: the two path buttons ("To Risen Ballbylon" / "Down the Pit") named, "ready" from
  their notification; page change speaks title, description, cost, gear progress; upgrade button reads
  cost and gears.
- Level select: set buttons named (DLC one says locked + why without the DLC), the set's own name on
  page change, the **altitude sickness** notice spoken when the DLC list opens, new game plus arrows
  named. Page Up / Page Down = new game plus tier here, encyclopedia set in the encyclopedia.
- Run summary: blueprints found, trophy bonus, gears found + how to earn gears.
- Battle: "altitude sickness applies" at the start of a DLC level; L adds gears won this run.
- Encyclopedia: DLC-locked entries read the "purchase the DLC" panel; set arrows named.
- Endings: "Ending sequence" / "Risen Ballbylon ending" announced; the DLC ending's captions
  (`GameCompleteObjDLC.CvsText`) spoken as they appear. No skip key (the game's is a held button).
- `Names.Tr` turns the title glyph `{[x]}` into a space ("RISEN BALLBYLON", not "RISENBALLBYLON").

DLC guesses to confirm: (a) `ActivateElevator()` called directly behaves like a click on the lift (camera,
base state); (b) `IsElevatorUpgradeVisible` means "an upgrade can be opened", not only "affordable";
(c) the altitude notice is the small group around `LevelSelectUI.ParamsAltitudeSickness`; (d) where the
set-choice page appears in the flow (Backspace from a level list should return to it).
Not covered: save slot DLC progress pips (`SelectSlotItem.ImgDLCProgress`, images only).

### Session 4 (2026-11-14): menu navigation rewritten after the first user playtest
The user's playtest (speech.log of 15:15 to 15:20) showed the menus were unusable: arrows moved by screen
position, so focus hopped between character tiles, hover texts and buttons; "Seleccionar" could not be
found; Enter on a building "ready to harvest" opened its upgrade screen instead. The rule "never navigate
by screen position" is now in CLAUDE.md and in every `starting_points/template_*.md`.

**Verified in the running game this session** (driven with `tools/keys.py`, read back from speech.log):
- One ordered list per screen (`Navigator.BuildList`): main menu, base side bar, base pause, dialogs,
  upgrade screen, harvest summary, character select, level select, battle pause, settings.
  Up / Down walk it, Home / End, Tab / Shift+Tab jump between groups ("…, 30 elementos").
- Character select: all 30 characters are in the list (20 are scrolled out of view and are scrolled in on
  focus); Enter on a character lands on "Seleccionar, El Guerrero"; Enter opens level select.
- Level select: levels in the game's order (`LevelSelectUI.Items`), Enter on a level starts the run.
- Base: Enter on the side bar's harvest button starts the aim already on a building ("Apuntando a Campo
  de trigo, 13 grados a la derecha. Intro lanza…"), Tab aims at the next, Enter launches, summary read.
- Settings: Ctrl+Tab changes tab ("Vídeo", "Sonido"); a row is one stop and reads its value (toggles on /
  off from `ImgToggleOn`, lists, sliders); Enter toggles and the new value is spoken.
- The upgrade screen says whose it is ("Campo de trigo, Siguiente mejora…").
- The game handles Backspace itself in settings (log: `Back: the game handled the key itself`); the mod's
  own back now waits two frames and only acts when the screen has not changed.

**Second pass the same day (the user asked for a play-through): verified in the running game**
- A whole loop was played with `tools/keys.py`: base → Jugar → character → level → a 7 minute run with
  tutorial popups, four level ups, a fuser (fission) and death → run summary → back in the base →
  Tab to a building → Enter → harvest aim → Enter → harvest summary.
- Enter on a building that is ready to harvest starts the harvest aimed at it (was unverified).
- Base side bar order: Jugar, Recolectar, Construir, Expandir, Reorganizar, Opciones.
- Coming back to the base says "En la base. Hay una recolección lista: Tab hasta un edificio y pulsa Intro".
- Battle: tutorial banners that do not block input (`GameTutUI.ShouldBlockInput()` false) are spoken once
  and no longer take the arrows or the focus; a blocking popup lists only its own button (the HUD speed
  buttons were in its list, and Home landed on one). The shooting tutorial no longer waits for a mouse
  aim (`GameTutMgr._didAim`). With autofire off the start line says how to shoot.
- Level up: stat gains as "Vigor 6 (+1)"; the choices come first, then reroll, then owned equipment
  (empty slots are not stops); a fission result names what each item got.
- Build menu: Tab says the section header ("Bloqueado"), Ctrl+Tab says the category in the game's words
  and moves focus into it. Tabs read "Bolas, pestaña" and "seleccionado" for the open one.
- Encyclopedia: the statistics page is read out (its figures are not buttons); tabs are named.
- Stat rows (pause, level up) read name and value. "trigo trigo" and "Oro oro 0" are gone.
- Level select: Shift+Enter opens the level's leaderboard (the inner buttons cannot hold the focus).

**Written, not verified**:
- Upgrade descriptions with their numbers: the reader now takes the visible `TxUpgDesc` text of the
  level-up screen and waits for it; the last run with that build ended before a level up.
- Left / Right change a level's difficulty (level 1 has a single difficulty on this save).
- Shift+Enter leaderboard on a level; Backspace two-frame rule on screens other than settings.
- Screens not opened this session: workers, expansion, rearrange, market, hatchery, pet lab, loadout,
  treasure / blueprint, revive, boss, character level up after a run.

**Facts learned** (also in CLAUDE.md): a `CoolButton` ignores `OnSubmit` unless it has been the selected
one for a moment: press a button that is not focused through `Navigator.Click` (its `OnClicked`
delegate). Level select pulls the selection back onto the selected level (not the mouse: tested with the
pointer parked in a corner), so a level's inner buttons cannot hold the focus. `OverlayUI.DefaultFocus`
can be a row (pause menus). The game opens the base pause menu when its window loses focus.
`tools/keys.py` has `equals`, `minus`, `del`; the autoplay loop used for the run is not in the repo.

**Still wrong or missing**:
- The test "player" only walks left and right; nothing was learned about how well a blind player
  survives. Cue balance and the aim assist remain untuned.
- The move tutorial needs movement in all four directions before it goes away; its text does not say so.

### Session 5 (2026-11-14): cues remade with genny, fixes from the user's 16:40–16:50 playtest log
The user played three runs and found the cues indistinguishable (every cue was a short synth blip in the
same 450–3000 Hz band). All 52 cues are now genny WAVs (`tools/make_cues.py` → `src/BallPitAccess/Sounds/`,
embedded in the DLL, overridable from `UserData/BallPitAccess/sounds/`), one physical family per category
(CUES.md). `Synth` is gone. Sound guide (F4) texts name each sound and has a far (enemy1) and a close
(enemy9) enemy. Built and deployed, **not heard in game yet**.
Fixes from that log, written, not verified:
- Level-up descriptions lost their numbers ("durante segundos (máximo )", "Inflige - de daño"): the reader
  read `LocDetailsDesc` first. It now fills the term's `{[PARAM]}`s from `ParamsDetailsDesc._Params`
  (`Names.Filled`), falls back to the visible `TxUpgDesc`, and retries while the numbers are missing.
- "Sustituir ( oro 5)" → "Sustituir (oro 5)" (`Speech.Clean` trims spaces inside brackets).
- Run summary "Imán, daño --": a dash-only value is dropped.
- "Aquí no hay nada que seleccionar" during the level descent, a tutorial popup in battle and the run's end:
  only said now for a settled screen outside battle.
Seen in the log, not changed: the player died in 1–2 minutes each run (health 60 → 10 in under 10 s
twice); worth asking whether the radar / danger cues gave enough warning once the new sounds are heard.
The fusion screen listed only "Fisión" (the other modes were not available), so the arrows repeated it.

### Session 6 (2026-11-15): catching balls, XP crystals, "behind" and "go this way", written, not run
The user forwarded beginner advice (catch your balls, crystals do not collect themselves, survive first,
fuse / evolve, slow down for bosses) and asked: cues muffled when behind, always pointing where to go,
"we need to be able to catch balls", and to build the mod as a blind player would need it.
- **Muffle = behind** (`AudioEngine` two-pole low-pass per voice, `Battle.Muffle`); radar and pickups.
- **Guide pan** (`Battle.GuidePan`) for the ball clank and both pickup beacons; `_hit` hazard cues now
  point to the side to step to (they pointed at the threat before).
- **Pickups**: gem pitch now higher = closer (like the radar); gems keep their own beacon while a special
  pickup is waiting (before they went silent); `RemainingLifetime` in the dump (do crystals expire?).
- **Catch assist** (`Game/CatchAssist.cs`, F4 setting, default "hold Shift"): through
  `Player.IsOverridingInput` / `InputXOverride` / `InputYOverride`, walks under the next main ball
  (`Battle.PredictBall`, every frame); no ball coming: to the nearest reachable pickup; a predicted hit
  (`Hazards.Dodging`) wins over both. W / S still move; A / D always win. "Automatic" mode catches without
  Shift. **Guess**: the override fields drive movement. If the hero does not move within 0.35 s of a full
  push, it switches to `SetPos` at the speed learned from the player's own walking and logs
  `Catch assist: the input override did not move the hero`.
- Boss arrival at a speed above 1x adds "press minus to slow down".
- Same day, user: "the blind player can do it also without auto walk … it's not fun if all you do is
  shift shift shift". Shift stays, but the **ball cue is made to catch by ear**: pitch rises as you near
  the landing spot (`AudioEngine` now resamples per voice), graded pan, parking-sensor rate, soft while
  the ball is still among enemies, double clank when under it, new `ball_miss` sound. Returning balls
  (flying home) no longer clank. Catch / miss write the predicted offset to `Latest.log` so the catch
  width (`Battle.CatchHalfWidth`, now `kColHalfWidth + 0.15`) can be tuned from a playtest.
- Then, user: "why just clanks … balls move continuously". **Continuous loops** (`Audio/LoopVoice.cs`):
  the ball is now a rolling hum that follows it every frame (pitch = closeness to the landing spot, pan =
  side to walk, throb = time to land, click on stepping under it); the most relevant enemy shot is a
  whistle that moves with it (replaces the one-shot `shot`). Enemies, pickups, bombs stay one-shots.
  Not heard yet: loop volume against the radar, whether the throb reads as urgency.
- Then: **number-key sound filters** in battle (`Input/CueFilters.cs`): 1 enemies, 2 balls, 3 rewards,
  4 shots and hazards, 5 edges, 6 health, 7 aim click; Ctrl+number solo; 0 = everything back on (user asked: "make 0 toggle all on regardless"). New
  settings `HealthCues`, `AimCues`; the enemy wind-up moved from hazards to enemies. Muted groups are
  said when a battle starts.
- Battle help (F1) explains catching, crystals and the muffle.

### Session 6, later: every game language
User: "make the mod help and readme support all the languages the game does". The game has 16 (I2
order en, zh-CN, ja, ko, fr, de, es, ru, pt-BR, zh-TW, es-US, pl, it, th, tr, uk). Mod strings moved to
`Core/Lang/<Lang>.cs` (partial `Loc`; tables looked up lazily because static initializers across partial
files have no defined order); 13 new tables and `README.<code>.md` translated by one agent per language
using the game's own terms (`tools/dump_i2.py` → `game/i2_terms.txt`); es-US reads Spanish. Not reviewed
by native speakers. F1 help now shows every part wherever it is opened (Tab between parts), and its
battle-sounds part was rewritten for the current cues.
All 15 tables pass `check_loc.py` (406 keys each, placeholders match); built and deployed, **not run in
the game in any language but the old en / es strings**. 15 README*.html built, every in-page link and
the 14 language links checked. Final pass: the fuser pickup uses the game's own name (`get_fusion_reactor`,
"Fusion Reactor!") in every new language (fr "réacteur nucléaire" is the game's word); the game's Escape
glyph is now `key_escape` per language. Open: en / es still say "fuser" / "fusionador" while the game
says "Fusion Reactor" / "Reactor de fusión" (left for the user to decide).

### Guesses to confirm in game (highest risk first)
0. Catch assist: does `InputXOverride` move the hero (log line above)? Does the hero stop under the ball
   and catch it (glove thud)? Does it dodge before catching?
1. `BaseGridMgr.NormalRightClickOnBuilding()` on Delete: unknown whether the game confirms before
   dismantling. Try on something cheap.
2. Rearrange: `ClickRearrange(obj)` to pick up, arrows through `SetHover`, `ClickRearrange(null)` to drop.
3. `SetHover(x, y, true)` in normal mode sets the hovered building (the mod also sets `_hoverBuilding`
   and calls `BaseHoverPopup.HoverBuilding` itself).
4. Level up: `BattleSaveData.UpgradeLvl + 1` is spoken as the level (L key); `LevelUpBtn.TxtLvl` as the
   choice's level.
5. Settings toggles: on / off comes from a `CoolToggleButton` / `Toggle` child or a `PlayerPrefsMgr`
   field per option; an option outside that table says no value.
6. Intro skip: `IntroCutsceneObj.Complete(true)` while its `CvsGrp` is visible.
7. Does the game's Escape already act as "back" in every menu? If so Backspace is only a spare.
8. Cue balance: everything in CUES.md is untuned.

### Known gaps
- Idle launcher aim (`BaseState.kAimLauncher`) still needs the mouse (announced as such).
- Leaderboard rows, Twitch voting: generic reading only. Credits have no skip key.
- Mod settings are on F4, not inside the game's settings screen (rule 18).
- Mod strings exist in English and Spanish only; the game ships eight more languages (rule 19).
- No SCREENS.md row is "verified" beyond session 1's list.

## Next steps

0. User re-test of what they reported; when a day's harvest is available, try Tab → building → Enter
   (the unverified path). Any screen whose order is wrong: Ctrl+Shift+D on it writes `List (dump)` to
   `Latest.log`.
1. User playtest following PLAYTEST.md; send back `speech.log`, `Latest.log` and, for a cue that points
   wrong, `threats.txt` (Ctrl+Shift+D).
2. Fix what the playtest shows; tune cue volumes and rates by ear (F4 → Sound guide).
3. Settle the eight guesses above; delete or correct each in this file.
4. Idle launcher aim; leaderboard rows; a "BALL x PIT Access" entry in the game's settings.
5. Remaining languages; README; a full blindfold run from main menu to a boss.

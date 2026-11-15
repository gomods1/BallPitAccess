# Playtest

Coverage map and log of playtest sessions. Session 2 (2026-11-14) wrote a lot of code that has **not run
in the game yet**; this file is the order to exercise it in. After a session, the useful files are in
`S:\steam\steamapps\common\BALLxPIT\UserData\BallPitAccess\`: `speech.log` (every spoken line),
`events.log`, and `MelonLoader\Latest.log` (search for `error`, `Navigator`, `Overlay:`, `Page:`,
`Battle state:`, `Base state:`). Ctrl+Shift+D during a battle writes `threats.txt` (enemies, balls,
shots, lasers, floor hazards, pickups with positions): send it when a cue points the wrong way.

Deploy: `tools/deploy.sh` (builds, stops the game, copies, relaunches) or `tools/deploy.sh nolaunch`.

## 6. Risen Ballbylon DLC (session 3, not yet run)

- [ ] Base: Tab lands first on "Lift, … 1 of N"; Ctrl+Up reads the game's popup (gears collected, pit and
      risen). Enter opens the lift upgrade when one is on offer; otherwise it says why. In `Latest.log`
      look for `Elevator: Enter, upgrade offered …`.
- [ ] Lift upgrade screen: with both paths open, the two buttons say "To Risen Ballbylon" / "Down the
      Pit" (plus "ready"); after choosing, title, description, cost and gear progress are spoken.
- [ ] After the DLC unlock: the tutorial line "Ballbylon has risen!…" is spoken.
- [ ] Level select: the set choice (the pit / Risen Ballbylon) reads both buttons; opening the DLC list
      says its name and the altitude sickness notice; DLC levels read number, name, locked + why.
      Page Up / Page Down change the new game plus tier (when there is one).
- [ ] A DLC level: start line includes "altitude sickness applies"; DLC enemies and bosses are named;
      a risen gear drop is announced ("gear, …"); L says gears won this run.
- [ ] Run summary: gears found and "how to earn gears" are in the summary line.
- [ ] Encyclopedia: Page Up / Page Down change the set and the set name is spoken.
- [ ] DLC ending: "Risen Ballbylon ending", then each caption as it appears.

## 1. Smoke test (2 minutes)

- [ ] The game starts and says "BALL x PIT Access loaded". `Latest.log` has `Patched CannonballObj.Run`
      next to the three older `Patched …` lines and no `error` after them.
- [ ] Main menu: focus lands on the first entry by itself; arrows and Enter still work.
- [ ] F1 (help list), F4 (settings list, new entries: hazards, pickup sounds, read descriptions, sound
      guide). In F4, Enter on "Sound guide": each line plays its sound.

## 2. Menus

- [ ] Settings: each row says label, value and kind; Left / Right change a list and a slider; Enter
      toggles and the new value is spoken; Tab changes tab and the tab name is spoken; Ctrl+Up reads the
      description. Note any toggle that does not say on / off.
- [ ] Save slots: "Slot N, …" / "empty, new game" / "Delete slot N".
- [ ] Backspace closes a screen (settings, build menu, pause). If Escape already does it everywhere,
      say so and Backspace can stay as a spare.
- [ ] A screen's name is spoken in the game language (not "Harvest Summary" in English).

## 3. Base

- [ ] Tab walks the buildings ("Wheat field, ready to harvest, 1 of 3"); Ctrl+Up reads the game's popup.
- [ ] Enter on a building opens what a click opens. Delete: note exactly what the game does
      (confirmation? immediate?). **Try it first on something cheap.**
- [ ] H resources and day, J harvest state.
- [ ] Build menu: Tab changes category; item descriptions under Ctrl+Up.
- [ ] Placement no longer says "blocked" once at the start; Backspace cancels.
- [ ] Workers, market, character select, level select (difficulty buttons), loadout: every item says
      what it is. Write down any item that only says a raw object name.
- [ ] When they unlock: expansion (Tab, Enter), rearrange (Tab, Enter, arrows, Enter), hatchery, pet
      name (typing echoes letters), pet lab.

## 4. Battle

Session 6 (catching, crystals, muffle), try these first:
- [ ] **Without Shift**: catch by ear. The rolling hum follows the ball: does it rise as you walk toward
      the landing spot, click when you are under it, and throb faster as it nears? Is it too loud or too
      quiet next to the enemy drums? Does it go silent when no ball is coming?
- [ ] Number keys 1..7 and 0 mute / unmute ("balls off"); Ctrl+2 = only balls; 0 = all back on.
      Start a new battle with something muted: "Muted: …" is said.
- [ ] Enemy shots: a whistle that moves with the shot, higher as it comes; louder when it will hit. A miss gives a dull thud. Send
      `Latest.log` (lines "Ball caught" / "Ball missed" tell how far off you were).
- [ ] Hold Shift while a ball comes down: the hero walks under it, stops, glove thud. Released: no drift.
      `Latest.log`: "Catch assist engaged"; send it if it also says "input override did not move".
- [ ] Hold Shift with no ball coming: walks to the nearest crystal; W / S still move; A / D take over.
- [ ] A shot coming while Shift is held: the hero sidesteps first.
- [ ] F4 → catch assist → automatic: catches without Shift; A / D still win.
- [ ] A crystal behind you sounds muffled; walking back makes it clear and higher.
- [ ] One column off is clearly left or right; lined up is centred.
- [ ] `shot_hit` / `beam_hit` / `bomb_hit`: stepping **toward** the sound now dodges.
- [ ] Boss at 1.5x or more: "press minus to slow down".

- [ ] Start: level name, aim mode. Enemy radar ticks; ball chirps; catch bell.
- [ ] Arrows: Left / Right turn the aim 5 degrees and say the angle, Up gives it back to the assist,
      Down says the target. F5 still cycles modes.
- [ ] H, J, K, G, and the new L (level, time, wave), B (balls), P (pickups), N (hazards).
- [ ] Pickups: three-note beacon for a special pickup and "fuser, 2 left, 3 ahead" when it drops; soft
      blip for gems. Is the blip too much? (F4 → pickup sounds → special only.)
- [ ] Enemy shots: puff when one passes, harsh buzz when it will hit; does stepping away from the sound
      dodge it? Lasers and bombs the same (later levels).
- [ ] Damage: hurt sound + health; low health heartbeat and "Low health".
- [ ] "N level ups ready, press E"; level up menu: title, hint, first choice with its description;
      arrows between choices; Ctrl+Up reads numbers; reroll and banish buttons; after picking with more
      level ups pooled, the new choices are announced.
- [ ] Fuser: each step announced; options readable.
- [ ] Pause: Tab between character / equipment / system.
- [ ] Boss: "Boss: name", health at 75 / 50 / 25 %.
- [ ] Game over: time, kills, XP, resources after the counters stop; buttons.

## Log

| Date | Session | Who | What was covered | Result |
|---|---|---|---|---|
| 2026-11-14 | 1 | agent + user | Base tutorial to first harvest summary | see STATUS "verified" list |
| 2026-11-14 | 2 | agent | No game run (code only) | nothing new verified |

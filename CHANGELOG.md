# Changelog

Player-facing changes to BALL x PIT Access, newest first. The release notes on GitHub are taken from the
section of each version.

## 0.6.0

For game version 1.308. Most of these changes come from a player's feedback.

* **Every sound comes from where its thing is, relative to your hero**: left of you on the left, ahead in
  the centre, behind muffled; higher means closer. The ball hum, the crystal tink and the special item
  sparkle no longer jump to "the side to go to", and the shot, laser and bomb warnings now sound from the
  shot, the laser and the spot where the bomb lands (before, they sounded on the side to step to). The
  ball hum follows the ball itself: it sounds where the ball is and rises as the ball comes closer, so you
  hear where it is heading; the click still tells you when you stand where it will land. A
  crystal's pitch now follows its real distance, not only how many rows ahead it is.
* **Change the mod's keys**: F4 → Keys lists every mod key; Enter and then the new key changes it, Delete
  restores its default, the last line restores them all. Two actions never share a key (they swap). The
  F1 help ends with the keys as they are set now.
* **Game speed on any keyboard**: Num + and Num - change the game speed in battle. The game's own = and -
  keys are missing on some layouts (on Latin American Spanish, = is Shift+0).
* **Aim assist hits more**: it aims at a part of the enemy that no other enemy hides (from the enemy's real
  hitbox), skips enemies at an angle your hero cannot shoot (the ball flew past them), and the default
  mode, now "closest enemy, boss first", goes for the boss unless another enemy is about to reach you.
* **Catch assist and health**: when you are hurt, holding Shift fetches a health item before the next
  ball. Every item, health included, is picked up by walking over it once it has landed.
* **Catching is detected correctly**: the glove thud now plays on every catch and the sinking thud only when
  a ball really got past you (before, every ball counted as missed). The catch zone used by the sounds and
  the catch assist matches the game's (measured: about 0.4 either side of the hero).
* **Less noise, alarms you can hear**: measured in recorded runs, half as many sounds overall. The "a shot
  will hit you" alarm sounds once per shot, when it is about 1.5 s away (it repeated every quarter second
  before); enemy drums slow down while enemies are far and keep quiet around an alarm; the crystal tink
  waits while a ball is coming; the wall knock no longer repeats while the catch assist dodges along a wall;
  the aim assist changes target less often.
* **Quieter enemy radar**: while the enemies hold still, a sweep plays only the nearest drum (a full sweep at
  least every 4 s, and at once when anything moves); a second drum that would sound like the first is left
  out. Replayed on a recorded run: 146 drums a minute down to 62. The crystal tink no longer lands on a
  drum.
* **Crystals in heaps**: the glass tink (and the catch assist between balls) heads for a heap of crystals
  when it is only a little further than the nearest single one, so one walk collects several.
* **Fewer aim clicks**: after a kill the switch click stays silent with autofire, and when shooting by hand it
  plays only if the aim swings to another part of the field.
* **Battle recording in the mod menu**: F4 → "Record battles to telemetry.tsv" turns on a recording of every
  battle (sounds, speech, balls, enemies, items) in `UserData/BallPitAccess/telemetry.tsv`, for tuning the
  sounds. Off by default; send the file with a report. Each launch starts a new file and keeps the last
  session's as `telemetry.prev.tsv`.
* **Fuser results**: Enter on a result continues (the button was a group away).
* **What was that sound?** Q in battle names the sounds playing now and the last few you heard, with what
  each one means and its side.

## 0.5.0

First public release, for game version 1.308 with or without the Risen Ballbylon DLC.

* **Installer**: `BallPitAccessibilityInstaller.exe` finds the game through Steam and installs, updates or
  removes MelonLoader 0.7.3, the Prism speech library and the mod. The zip is laid out like the game
  folder for installing by hand.
* **Speech in every language the game offers** (16), following the game's language setting, through your
  screen reader (NVDA tested; others supported by Prism should work).
* **Every menu by keyboard**: one list per screen, Up / Down to move, Tab between groups, Enter to
  activate; popups, the guided tutorial, level select with difficulty, characters, the encyclopedia and
  settings are read, with item details from the game's own panels.
* **The base (New Ballbylon)**: buildings, building placement, expansion, rearranging and harvests by
  keyboard, with HUD keys for resources.
* **Battle by ear**: enemies, bosses, your balls (a continuous hum for the ball you can catch), enemy
  shots, lasers, bombs, floor hazards, XP crystals and special items each have their own sound, panned to
  their side, higher when closer and muffled when behind you. Edge knocks, damage, healing, low health,
  status effects and ready level ups are heard too.
* **HUD keys** in battle for health, progress, balls and passives, pickups, hazards and enemies; arrow keys
  turn the aim.
* **Sound filters** on the number keys mute or keep groups of sounds; the choice is remembered.
* **Optional assists**: the aim assist (F5: closest, strongest or front enemy, fixed angle, or off) and
  the catch assist (hold Shift: walks under the next ball, to the nearest pickup, or out of a shot's
  way). Everything they do can also be done by ear.
* **F1** help for the current screen, **F4** mod settings and the sound guide, where every sound can be
  heard on each side.

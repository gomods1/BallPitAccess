# Changelog

Player-facing changes to BALL x PIT Access, newest first. The release notes on GitHub are taken from the
section of each version.

## 0.6.4

From a player's report: workers never built a Haunted House, a frozen hero with no way to tell why, and a
boss that stayed at 100 percent.

* **The harvest says whether the workers will reach a building**: when it aims at one (Tab, or Enter on it
  in the base), the mod follows the workers' path the way the game moves them and picks an angle that
  reaches it ("workers hit it straight", "workers reach it off a wall", "workers pass over it" for wheat
  fields). If no angle does, it says so and names the building in the way ("no clear path, Forest is in the
  way"): workers reach such a building only by chance. Before, the aim went at the building's centre, and
  a building in front of it took every worker. Left / Right also say what the line hits.
* **After a harvest the mod says what each building under construction got** ("Haunted House: 6 hits, 40
  to 55 percent"). The game's summary only counts resources, so "A Fruitless Endeavor" did not mean the
  workers missed.
* **New key U: status effects and whether you can move**: each effect with its seconds left, then "You can
  move", or why not (frozen, knocked back by a hit, teleporting, enemies advancing for turn-based
  characters, a character that walks by itself). When a freeze ends the mod says "You can move again".
* **Boss health is the real one**: some bosses keep their health in a part, and the mod read the main
  piece, which never changes. The Skeleton King showed 100 percent all fight. J now gives the same health
  as the game's boss bar, and the 75 / 50 / 25 percent announcements come.
* **Weak spots**: the Skeleton King's body cannot be hurt at all, only the crown on his head, and his body
  shields it from the front. J says so and where the crown is, and the aim assist goes for the crown
  instead of the body.

## 0.6.3

From a player's report: the game says workers can speed up a building under construction, and the mod had
no way to do it.

* **Building with your workers**: buildings under construction or upgrading are now in the harvest's Tab
  list, with their progress ("Forest, under construction, 40 percent"). Every worker that bounces off one
  builds it, and the mod says when it is finished ("Forest is finished"). Opening the harvest says how
  many buildings are under construction.
* **Enter on a building under construction** in the base starts the harvest already aimed at it, the same
  as on a building ready to harvest. When no harvest is available, Tab on it says that workers build it
  during a harvest.
* **J** (harvest and workers) also says how many buildings are under construction.

## 0.6.2

From a player's reports about building after the Gunsmith unlocked.

* **Building works after a harvest or a run**: choosing a building in the build menu sent you back to the
  side bar (Play, Harvest, Build...) instead of placing it, until the game was restarted. It happened once
  you had walked through your buildings with Tab (as you do to harvest). It now starts placing every time.
* **Building sizes**: the build menu says how many tiles each building takes ("Villa, stone 10, 3 wide, 2
  tall"), placing starts with the building's name and size, and Tab on a placed building gives its size in
  the details.
* **Explore mode**: press K in the base to walk the land tile by tile with the arrows. Each tile says
  whether it is free (and how many free tiles follow to the right and down), which building stands on it,
  or that the land is not bought yet. Tab / Shift+Tab jump to the next / previous free tile; K, Backspace
  or Escape leave (without opening the pause menu). Opening it says how many tiles are free.
* **Placing a building says the column and row of its top-left tile**, the same numbers explore mode
  uses, so a free area found by exploring can be found again while placing. Before, the numbers were
  measured from the middle of the building and did not match.

## 0.6.1

For game version 1.308. Fixes to building in the base, found while checking a player's report about the
Sheriff's Office.

* **Enter on a building in the build menu always starts placing it**: right after changing category with
  Control Tab (for example to Housing), Enter on the first building did nothing until you moved away and
  back.
* **Home finds a spot where the building really fits**: it used to jump to the first free tile, and a house
  (Sheriff's Office and the others) is bigger than one tile, so it often landed on "occupied". It now tries
  the free spots in turn and stops on the first one that fits; if none does, it says so (expand the base or
  move other buildings).
* **R says where you are after rotating**: turning a building can change whether it fits; the column, row
  and "fits" or "occupied" are spoken again once the turn ends.
* **Backspace cancels placing without opening the pause menu**: one press used to cancel and then open
  the pause menu.
* **Backspace leaves expansion and rearrange mode**: in expansion it did nothing (most of all after buying a
  plot).
* **Expansion speaks its keys once**, and says "Plot bought" after a purchase instead of repeating them.
* **Rearrange**: dropping a building says "Building moved" (it said "Built"), and buildings no longer offer
  "Enter harvests" there, since Enter picks the building up.

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

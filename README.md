# BALL x PIT Access

A mod that makes **BALL x PIT** (Kenny Sun / Devolver Digital) playable without sight. It speaks every
menu through your screen reader, lets you use the whole game with the keyboard, and turns the battlefield
into sound: enemies, your balls, enemy shots, XP crystals and special items all have their own sounds,
coming from their side of you.

Version 0.6.0, for game version 1.308, including the Risen Ballbylon DLC. The mod speaks **every language
the game offers** and follows the game's language setting.

<!-- LANGS -->
**English** · [Español](README.es.html) · [Français](README.fr.html) · [Deutsch](README.de.html) · [Italiano](README.it.html) · [Português (Brasil)](README.pt-BR.html) · [Polski](README.pl.html) · [Türkçe](README.tr.html) · [Русский](README.ru.html) · [Українська](README.uk.html) · [日本語](README.ja.html) · [한국어](README.ko.html) · [简体中文](README.zh-CN.html) · [繁體中文](README.zh-TW.html) · [ไทย](README.th.html)
<!-- /LANGS -->

> **Status: early test version.** Menus, the base and the first runs have been played with the mod. The
> battle sounds and the newest parts (continuous ball sound, catch assist, sound filters) have not been
> tuned with players yet. Please report what works and what does not (see [Reporting a problem](#reporting-a-problem)).

## Contents

- [What you need](#what-you-need)
- [Installing](#installing)
- [Removing the mod](#removing-the-mod)
- [First steps](#first-steps)
- [Keys everywhere](#keys-everywhere)
- [Menus](#menus)
- [The base (New Ballbylon)](#the-base-new-ballbylon)
- [Battle](#battle)
- [Battle sounds](#battle-sounds)
- [Sound filters (number keys)](#sound-filters-number-keys)
- [Assists](#assists)
- [Mod settings (F4)](#mod-settings-f4)
- [Changing the keys](#changing-keys)
- [Tips for a first run](#tips-for-a-first-run)
- [Known limits](#known-limits)
- [Reporting a problem](#reporting-a-problem)
- [Credits](#credits)

## What you need

- BALL x PIT on Windows (Steam), version 1.308.
- A screen reader: NVDA is the one tested. Others supported by the Prism speech library (JAWS, Narrator,
  SAPI voices) should work too.
- **MelonLoader 0.7.3** (x64), the mod loader. Free, from its official GitHub releases page.
- Headphones or stereo speakers: left and right matter a lot in battle.

## Installing

**With the installer (easiest):** download `BallPitAccessibilityInstaller.exe` from the
[latest release](https://github.com/gomods1/BallPitAccess/releases/latest) and run it. It finds the game through Steam, installs MelonLoader 0.7.3, the
Prism speech library and the mod, and later updates or removes them. Then start the game: the first start
takes a few minutes and is silent while MelonLoader prepares the game's files, then your screen reader
starts speaking.

**By hand:** download `BallPitAccess-vX.Y.Z.zip` from the same page and extract everything into the game
folder (by default `C:\Program Files (x86)\Steam\steamapps\common\BALLxPIT`, the folder that contains `Balls.exe`), so that `version.dll` ends up next to
`Balls.exe`. The zip already contains MelonLoader: you do not install it separately. Start the game; the
first start is slow and silent, as above.

## Removing the mod

The installer's Uninstall button removes the mod and MelonLoader and keeps your settings in `UserData`.
By hand:

Delete `Mods\BallPitAccess.dll` and `prism.dll`. To remove MelonLoader too, delete `version.dll` and the
folders `MelonLoader`, `Mods`, `Plugins` and `UserData` from the game folder. No game file is changed by
the mod.

## First steps

- Press **F1** at any time for the help: every part of it, starting with the part for where you are.
  Up and Down arrows read it line by line, Tab jumps to the next part, Escape closes.
- Press **F4** for the mod's settings, and inside them open the **sound guide**: it plays every battle
  sound and says what it means. Listening to it once before your first run helps a lot.
- The game itself is mouse driven with a keyboard; with the mod you never need the mouse except for the
  idle launcher in the base (see [Known limits](#known-limits)).

## Keys everywhere

| Key | Action |
|---|---|
| F1 | Help (all parts; Tab / Shift+Tab between parts) |
| F2 | Read the whole screen |
| F3 | Repeat the last thing said |
| F4 | Mod settings and sound guide |
| F5 | Change the aim assist mode |
| F6 / F7 | Turn the aim 10 degrees left / right |
| F8 | Say where you are aiming |
| Ctrl+Up | Read the details of the focused item (Ctrl+Down goes back) |
| Ctrl+Left / Ctrl+Right | Switch between details and recent events |

While a mod list (help, settings, sound guide) is open, the game does not receive your keys.

## Menus

Every screen is **one list in a sensible order**: its choices first, then its buttons, then Close. The
order never depends on where things are drawn on the screen.

| Key | Action |
|---|---|
| Up / Down | Previous / next item |
| Enter | Choose |
| Tab / Shift+Tab | Next / previous group of the screen |
| Home / End | First / last item of the group |
| Left / Right | Change a list or slider setting; on a level, change its difficulty |
| Ctrl+Tab / Ctrl+Shift+Tab | Next / previous tab on screens with tabs |
| Backspace or Escape | Back |
| Page Up / Page Down | Set in the encyclopedia; new game plus tier in the level list |
| Shift+Enter | On a level: open its leaderboard |

- Enter on a character puts you on its Select button.
- When the guided tutorial points at something, focus jumps to it: press Enter.
- Upgrade choices are read with their numbers ("Deals 10 to 15 damage").

## The base (New Ballbylon)

| Key | Action |
|---|---|
| Up / Down, Enter | Side bar buttons: Play, Harvest, Build, Expand, Rearrange, Options |
| Tab / Shift+Tab | Walk through the lift and your buildings |
| Enter on a building | Start a harvest aimed at it if it is ready; otherwise open its upgrade |
| Shift+Enter | Open a building's upgrade |
| Enter on the lift | Open the lift upgrade (down the pit, or up to Risen Ballbylon) |
| Delete | Dismantle a building |
| H | Resources and day |
| J | Harvest and workers |

- **Building**: arrows move the building one tile and say whether it fits, Enter builds, R rotates, Home
  jumps to a free spot, Backspace cancels.
- **Harvest**: it starts aimed at a building. Enter launches the workers, Tab aims at another building,
  Left / Right turn the aim, hold Space to speed up, Escape leaves.
- **Expansion**: Tab chooses a plot, Enter buys it.
- **Rearrange**: Tab chooses a building, Enter picks it up, arrows move it, Enter drops it.

## Battle

### The game's own keys

| Key | Action |
|---|---|
| W A S D | Move |
| Space | Shoot (hold) |
| F | Autofire on / off |
| E | Open a level up when one is ready |
| = / - | Faster / slower game speed (missing on some layouts: use the mod's Num + / Num - below) |
| Escape | Pause |

### Mod keys

| Key | Action |
|---|---|
| Left / Right arrow | Turn the aim 5 degrees by hand |
| Up arrow | Give the aim back to the assist |
| Down arrow | Say where you aim |
| Shift (hold) | Catch assist (see [Assists](#assists)) |
| 1 to 7, 0 | Sound filters (see [Sound filters](#sound-filters-number-keys)) |
| H | Health and status effects |
| J | Enemies and boss |
| K | Your position |
| G | Game speed |
| L | Level, time, wave, gears won |
| B | Your balls and passives |
| P | Pickups on the field |
| N | Hazards (shots, lasers, bombs, floor) |
| Q | What was that sound? The sounds playing now and the last few, with what each means and its side |
| Num + / Num - | Faster / slower game speed, on any keyboard layout |

Every mod key can be changed: see [Changing the keys](#changing-keys).

Spoken during battle: the level at the start, bosses and their health at 75, 50 and 25 percent, damage
and health, status effects, special items when they drop, "level ups ready, press E", the end of the run.

## Battle sounds

Three rules hold for every sound, always relative to your hero:

- **Side**: a sound comes from where the thing it is about is: left of you on the left, right of you on
  the right, centred when it is straight ahead.
- **Pitch**: higher means closer.
- **Muffled** means it is behind you.

Things that move all the time (your ball, enemy shots) are **continuous sounds** that follow them; things
that change in steps (enemies advancing, items lying on the floor) are short sounds.

| Sound | Meaning |
|---|---|
| Hand drum | An enemy. Higher and faster = closer. When the enemies move, the nearest ones play in turn; while nothing changes, only the nearest one plays. |
| Big deep drum | A boss. Drums, gong and horn when one arrives. |
| Snarl | An enemy is about to reach you. |
| Rising whine | An enemy winds up an attack, on its side. |
| **Rolling hum** (continuous) | The ball you can catch. It sounds from **where the ball is** and **rises as the ball gets closer to you**, so you hear where it is heading; it throbs faster as the ball nears your line, and is softer while the ball can still bounce off an enemy. |
| Click | You are standing where the ball will land: stay there. |
| Glove thud | Ball caught. |
| Dull sinking thud | A ball got past you. |
| Gun rack | Your balls are back: you can shoot (silent with autofire). |
| Glass tink | The XP crystal, gold or resource to walk to: the nearest one, or a heap of them a little further. They do not collect themselves. |
| Kalimba sparkle | A special item: fuser, blueprint, egg, gear, health, magnet, bomb, gem bag. |
| **Airy whistle** (continuous) | An enemy shot flying near you, higher as it comes. Louder when it will hit you. |
| Swoosh and crack | A shot will hit you; it sounds from the shot: step out of its way. |
| Electric hum / two zaps | A laser is aiming / you are in its line; both sound from the laser: step out of the line. |
| Falling whistle / fuse hiss | A bomb is falling nearby / it will land on you; both sound from where it lands: step away from it. |
| Squelch | You stepped on a floor hazard (its name is spoken). |
| Punch / gulp | Damage / healing. |
| Heartbeat | Low health; faster when very low. |
| Woozy wobble | A status effect landed on you. |
| Two bell notes up | A level up is ready (press E). |
| Wooden knock | Side edge of the arena (also the end of a menu list). |
| Metal knock | Front or back limit of your walking area. |
| Switch click | The aim assist switched to another target, on the side of the new one. With autofire on, not after a kill. |

Every one of these can be heard in **F4 → Sound guide**, with Left / Right to hear it on each side.
In battle, **Q** names the sounds you just heard.

## Sound filters (number keys)

In battle, the number keys mute or unmute a group of sounds so you can concentrate. The game uses none of
these keys.

| Key | Group |
|---|---|
| 1 | Enemies: drums, snarl, wind-up, boss arrival |
| 2 | Balls: rolling hum, click, catch, miss, gun rack |
| 3 | Rewards: crystals, gold, resources, special items |
| 4 | Shots and hazards |
| 5 | Arena edges |
| 6 | Your health: damage, healing, heartbeat, status |
| 7 | Aim assist click |
| 0 | Everything back on |

- Each number switches its own group, independently of the others.
- **Ctrl+number** keeps only that group. Example: enemies and rewards only = Ctrl+1, then 3.
- Your choice is kept for the next session. When a battle starts with something muted, the mod says
  what is muted.
- Muting changes only the sounds: the H, J, P, N keys still answer and the catch assist still dodges.

## Assists

Everything an assist does can also be done by ear; the assists are there when you want them.

- **Aim assist** (F5 changes the mode): aims for you at the closest enemy (default), the strongest
  enemy, the enemy in front of you, a fixed angle, or off (mouse). The arrows fine-tune it. It aims at a part of the enemy that no other enemy hides, skips enemies at an angle your hero cannot shoot, and in the default mode goes for the boss first unless another enemy is about to reach you.
- **Catch assist** (hold Shift): the hero walks under the next ball coming down; with no ball coming it
  walks to the nearest crystal or item; if a shot is about to hit you it steps out of the way first.
  W / S still move you forward and back, A / D always take over. In F4 it can be off, "hold Shift"
  (default) or automatic. When you are hurt it fetches a health item first.

## Mod settings (F4)

Up / Down move, Left / Right or Enter change, Escape closes. Settings are saved.

- Aim assist mode
- Catch assist: off, hold Shift, automatic
- Audio cues on / off, cue volume
- Enemy radar, ball sounds, arena edges, shots and hazards (on / off)
- Pickup sounds: off, special items only, all
- Health sounds, aim assist click (on / off)
- Read upgrade descriptions on focus
- Keys (Enter opens): see [Changing the keys](#changing-keys)
- Sound guide (Enter opens)
- Debug logging
- Record battles (off by default): every battle is written to `UserData\BallPitAccess\telemetry.tsv`, for tuning the sounds

## Changing the keys {#changing-keys}

**F4 → Keys** lists every mod key with the key it is on now. Up / Down choose, **Enter** then press the
new key (Escape cancels), **Delete** puts back the default key, and the last line restores them all.
If the new key already belongs to another mod action, the two swap and the mod says where the other one
went. Keys the menus or the game need (arrows, Enter, Tab, Escape, Backspace, Space, W A S D F E R X)
cannot be taken. The F1 help ends with the keys as they are set now.

## Tips for a first run

- **Catch your balls.** A caught ball can be shot again at once; a missed one takes 2 to 3 seconds to
  come back. Follow the rolling hum until you hear the click, then stay.
- **Pick up your XP crystals.** They do not collect themselves. Follow the glass tink; muffled means it
  is behind you.
- **Survive first.** Early in a run, staying alive matters more than damage.
- **Fuse and evolve** your balls when the level up offers it: the choices say "fusion" or "evolution".
- **Slow down for bosses** with Num - (or the game's minus key). The mod reminds you when a boss arrives at a high speed.
- Dying is normal: the gold you bring back builds New Ballbylon, and buildings make every later run
  stronger.

## Known limits

- The idle launcher in the base still needs the mouse.
- Leaderboards and Twitch voting are read only in a basic way. The credits cannot be skipped.
- Mod settings are on F4, not inside the game's own options screen.
- The mod's own texts in languages other than English and Spanish were translated without a review by a
  native speaker yet. Corrections are welcome. Names from the game (buildings, balls, enemies, upgrades)
  always come from the game's own translation.
- Sound volumes, rates and the catch width have not been tuned with players yet.

## Reporting a problem

Useful files, in the game folder:

- `UserData\BallPitAccess\speech.log`: everything the mod said.
- `MelonLoader\Latest.log`: the mod's log (errors, screens, catches and misses).
- In battle, **Ctrl+Shift+D** writes `UserData\BallPitAccess\threats.txt` (enemies, balls, shots,
  pickups with positions) and the open screen's list to `Latest.log`: useful when a sound points the
  wrong way or a menu reads in the wrong order.
- `UserData\BallPitAccess\telemetry.tsv`, if you turned on F4 → Record battles: every sound, enemy, ball
  and shot of your battles. The best help when the sounds are hard to follow.

Say what you did, what you heard, and what you expected.

## Credits

- BALL x PIT by Kenny Sun, published by Devolver Digital. This mod is unofficial.
- MelonLoader (LavaGang) loads the mod. Prism sends speech to your screen reader.

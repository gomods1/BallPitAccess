# Changelog

Player-facing changes to BALL x PIT Access, newest first. The release notes on GitHub are taken from the
section of each version.

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

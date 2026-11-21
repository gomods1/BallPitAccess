# BALL x PIT Access: working notes

Blind accessibility mod for **BALL x PIT** (Kenny Sun / Devolver), following
`C:\Users\Nitropc\code\mods\starting_points\template_generic.md` (plus the audio-cue and combat sections of
`template_shooter.md`). Read `STATUS.md` first in every session.

## Game facts (verified)

- Install: `S:\steam\steamapps\common\BALLxPIT`, exe `Balls.exe`. Steam app 2062430. Game version **1.308**
  (MelonLoader log), Unity **6000.0.62f1**, **IL2CPP** x64, metadata v31. No DRM wrapper on GameAssembly.dll,
  no anti-cheat. Single player (leaderboards and Twitch voting exist; the mod does not touch them).
- Input: **Rewired**. Default keyboard bindings (dumped at runtime to `UserData/BallPitAccess/bindings.txt`):
  Shoot Space, Pause Esc, Move WASD, Autofire F, game speed `=` / `-`, Level up E, Grab modifier Space,
  Rotate R, Show upgradeable Tab, Show fully upgraded X, Dismantle / speed up harvest right mouse,
  Cancel harvest Esc. **Aim has no keyboard binding** (mouse or right stick only).
- With a keyboard the UI is mouse-driven: nothing is selected and the arrows do nothing. Controller
  navigation exists (`CoolSelectable.Select/OnMove/OnSubmit`), but `OnMove` does not move in keyboard mode.
- Audio: FMOD. Language: I2 Localization (`Il2CppI2.Loc.LocalizationManager.GetTranslation(term)`).
  The test machine runs the game in **Spanish**. The game ships **16 languages**, I2 column order:
  en, zh-CN, ja, ko, fr, de, es, ru, pt-BR, zh-TW, es-US, pl, it, th, tr, uk. `tools/dump_i2.py` writes
  every term with all its translations to `game/i2_terms.txt` (use it to match the game's own words).
- DLC **Risen Ballbylon** (`DLCId.kBallbylonRise`; Spanish "Bolabilonia Resurgida"): no separate depot, the
  content ships in the base build behind `DLCUtl.CanAccessDLC`. Levels `kSewers, kAlleys, kCastle, kSpire`
  (`LevelMgr.kLevelOrderDLC`), a second lift direction (`MetaSaveData.ElevatorDLCLvl`, `PickupType.kGearDLC`),
  altitude sickness, its own ending (`GameCompleteObjDLC`), I2 terms in `InfoDB.LocDLC` (`lvlselect_dlc`,
  `elevator_upgrade_up`, `elevator_upgrade_depth`, `desc_dlc_content_locked`, `tut_dlc_unlocked`).
  I2 term names and texts can be read offline: `tools/dump_i2.py` (UTF-8; plain `strings` drops accents
  and non-Latin scripts).
- Settings that matter: `PlayerPrefsMgr.sGameSpeed` (float), `sAutofire`, `sSlowerSpeedOptions`
  (game's own slower speeds).

## Code map (namespace `Il2Cpp`)

- `GameMgr.I.CurState: GameState` (kPlaying, kGameOver, kLevelUp, kPaused, kPickTreasure, kFoundBlueprint,
  kBonusBall, kBonusPassive, kFoundEgg, kRevive, kEndingGame, kEnteringLvl).
- `Player.I`: hero. `transform.position`, `GetMinX/MaxX/MinY/MaxY`, `GetAimStartPos(idx)`, `SetAimDir`,
  `kColHalfWidth`. It also has an `RunAI()` autoplayer (used by AutoTestMgr; not used by the mod).
- `BattleSaveData.I`: `Pieces: List<GridPieceInst>` (enemies: `Type`, `CurState`, `CurHealth/MaxHealth`, `Obj`),
  `CurHealth` (player), `NumKills`, `CurLevel`.
- `GridPieceObj`: `GetWorldCenterPos()`, `GetBotY()`, `IsAlly()`, `IsActive`.
- `GridMgr.I`: `LeftBorderX/RightBorderX` (±3.9375), `AttackY` (-8.46), `kSpaceWidth/Height` 1.125, `IsInited`.
- `BallMgr.I.ActiveBalls: List<BallObj>` (`CurState` kNormal/kReturning/..., `AimDir`, `GetSpeed()`,
  `IsSide`, `IsBaby()`); `MousePosToAimDir(mousePos, charIdx)` turns the mouse into the launch direction;
  `ClampAimDir(dir, AimMode, charIdx)`; `RemoveBall(b, isSide, BallReturnType)` with `kCatch` when caught.
- **Geometry**: world X grows right, Y grows toward the enemies. Player starts at y -11.75, playable
  x ±3.375. One grid row/column = 1.125 units.
- UI: `CoolButton : CoolSelectable` (uGUI EventSystem selection, `IsInteractable`, `OnSubmit`,
  `OnPointerEnter/Exit`). `OverlayUI.sOverlayStack` = the game's popup stack (DialogUI, BuildUI, GameTutUI,
  LevelUpUI, PauseUI...). `OverlayUI.DefaultFocus`.
- Guided tutorial: `MaskedFTUEOverlay.I` (`IsShowing()`, `TxtDesc`, `BtnTgt` = the button it points at,
  `BtnContinue`, `IsShowingWithBtn()`).
- Base sidebar: `BaseSidebarBtn.Label` is an I2 term. Build list: `BuildItem.TgtInfo: BuildingInfo`
  (`GetNameSlug()`, `GetBuildDescSlug()`), `IsLocked`, `CanAfford`, `TxtCost`.
- HUD: `InGameUI.I.TxtCurHealth/TxtTotalHealth`.

## Architecture

- `src/BallPitAccess` (net6, references `MelonLoader/Il2CppAssemblies/*`), deployed to `Mods/`.
  - `Core/`: Prism binding (`PrismNative`, `PrismSpeech`), `Speech` (queue; `Say`, `SayMenu`, `Event`,
    `Battle` with channel + TTL), `Loc` (mod strings; one table per game language in `Core/Lang/*.cs`,
    `En.cs` is the reference, es-US uses `Es`; follows the game language; `tools/check_loc.py` checks
    every table against En: keys, duplicates, `{0}` placeholders. A new string goes into **every**
    table. Help texts are split into lines at ". " / "。", or at a `\n` line break when the text has one (ja, zh, th)), `Config` (MelonPreferences `[BallPitAccess]`), `Buffers`
    (Details + Events; Details can be extended lazily with the game's tooltip), `Log`.
  - `Audio/`: `AudioEngine` (winmm waveOut, own stream), `Cues` (loads the genny WAVs embedded from
    `Sounds/`, made by `tools/make_cues.py`; `UserData/BallPitAccess/sounds/*.wav` overrides; see CUES.md).
    One physical family per cue category: never make two categories out of the same kind of blip.
  - `Input/`: `Keys` (GetAsyncKeyState, only while the game window is in front; `Name(vk)` = Windows key
    name), `Bindings` (every mod hotkey is remappable from F4 → Keys: read keys as `Bindings.Pressed("id")`,
    never a hard-coded letter or F-key; a new key = a new `Add` + `key_<id>` string), `ModKeys` (F-keys,
    Ctrl+arrows, HUD letters, battle arrows = aim, help / settings / sound guide lists; mutes Rewired
    keyboard maps while a mod list is open).
  - `Game/`: `Battle` (state, radar, ball cues, edges, boss, status, level ups, HUD keys), `Hazards`
    (enemy shots, lasers, bombs, floor), `Pickups`, `AimAssist` (postfix on `MousePosToAimDir`), `CatchAssist` (walks the hero under the next ball /
    to a pickup / out of a hit through `Player.IsOverridingInput` + `InputX/YOverride`),
    `BallPatches` (catch cue), `Names` (game names through I2), `BaseNav` (buildings, expansion,
    rearrange, base HUD keys), `BaseGrid` (tile numbering, chunk grid), `Explore` (K in the base), `Placement`, `Harvest`, `Tutorial`, `Scenes` (boot, intro, credits), `Dump`.
  - `UI/`: `MenuReader` (EventSystem focus → speech), `Navigator` (keyboard navigation: one ordered list
    per screen, groups, overlays, pages, FTUE, text fields), `Readers` (one reader per item type).
- **Menu navigation never follows screen position** (user rule, 2026-11-14, after a playtest where the
  arrows moved to "the nearest button in that direction" and the Select button could not be found).
  `Navigator.BuildList` makes one list per screen from the UI hierarchy (parent chain, sibling order):
  content in structure order, then buttons named `*close*`, then hover-only controls (no `OnClicked` /
  `OnPressed` / `OnRightClicked` and not a known item type). A group = items sharing a container; lone
  items in a row merge into one group. Keys: Up / Down (Left / Right) previous / next, Home / End ends of
  the group, Tab / Shift+Tab next / previous group, Ctrl+Tab the screen's tabs, Enter activates and on a
  character moves focus to its Select button; on a level Enter plays it and Left / Right change its
  difficulty. Do not bring back `OnMove` or any by-direction search. A screen whose order comes out
  wrong gets its own rule in `BuildList`, from what things are, never from coordinates (level select
  uses `LevelSelectUI.Items`, because sibling order there is not level order).
  The list of each screen is logged at `List (ScreenClass): …` when the screen opens, and Ctrl+Shift+D
  logs `List (dump)` for the screen as it is now.
- `prism.dll` (x64, Prism 0.18.3) lives in the game folder next to `Balls.exe`.
- Cue rules: pan = side, pitch = closeness, **muffled = behind** (`Battle.Muffle`), always **relative to the hero, where the thing is**
  (`Battle.PanFor`): never pan a cue to "the side to go to" (user, 2026-11-16, the guide pan was removed). Things that
  move continuously (balls, shots) are continuous loops (`Audio/LoopVoice`, `loop_*` WAVs) updated every
  frame; things that change in steps stay one-shots. CUES.md.
- Assists are optional helpers, never the way to play (user, 2026-11-15: "not fun if all you do is
  shift shift shift"): every task an assist does must also be doable by ear from the cues alone.
- Update order (`Main.cs`): Keys → ModKeys → Battle → AimAssist → Hazards → Pickups → CatchAssist → Scenes → Placement →
  Harvest → Explore → BaseNav → Tutorial → Navigator → MenuReader → Speech.Pump. A handler that acts on a key calls
  `Keys.Consume()`; later handlers check `Keys.Consumed`.

## How the game's menus work (what the readers rely on)

- Menus are icon grids with a **details panel that follows the hover** (`LevelUpUI.HoverBtn`,
  `BuildUI.HoverItem`, `EncyclopediaUI.HoverItem`, `PauseUI.PreviewHero`…) and hover popups
  (`BaseHoverPopup`, `GameHoverPopup`). `Navigator.Focus` fires `OnPointerEnter` as well as `Select`, so
  those panels are filled; readers name the item from its info object and take the description from the
  panel (descriptions are I2 terms with `{[PARAM]}` placeholders that only the game's panels fill in).
- A screen with steps or tabs has a `CurPage` enum (or `CurFilter`, `TgtCat`): `Navigator.PageKey` watches
  it and re-announces. Tabs are `NavBtns` / `FilterBtns` / `CatBtns` arrays indexed by that enum.
- `OverlayUI.OnBPressed()` is the controller's back button for the screen.
- In `tools/api.py` output, members listed **before** a class's `static I` line (`P float len`,
  `P Vector3 tgtPos`…) belong to nested coroutine classes, not to the class: do not read them.

## Build, deploy, test

- `tools/deploy.sh [nolaunch]`: build, stop the game, copy, relaunch. `tools/launch.ps1`.
- **Release** (public repo `gomods1/BallPitAccess`): bump the version in `Main.cs` (MelonInfo), the
  version line of every README and a `## X.Y.Z` section in `CHANGELOG.md` (the release notes), run
  `tools/release.sh` (stages `release/payload`, committed because CI cannot build against the game's
  assemblies), commit, `git tag -a vX.Y.Z -m vX.Y.Z && git push --follow-tags`. The workflow adds MelonLoader 0.7.3 and
  Prism 0.18.3 (checked by SHA-256) and attaches `installer/BallPitAccessibilityInstaller.exe`, built in
  `../installer` (game index 10; `tools/release.sh installer` copies a new build). If the installer's
  wxWidgets build fails on a missing `link.exe` 14.51, delete the stale `CMakeCache.txt` under
  `target/*/wxdragon_sys_cmake_build/build` and build inside `vcvars64.bat`.
- **Telemetry**: F4 → "Record battles" (or `Telemetry = true` under `[BallPitAccess]` in
  `UserData/MelonPreferences.cfg`) records every battle to `UserData/BallPitAccess/telemetry.tsv`; `python tools/telemetry_report.py [file]` and
  `tools/ball_lifecycle.py` analyse it. Measure before tuning a cue. MelonLoader saves preferences on quit:
  edit the cfg only while the game is closed.
- `tools/check_loc.py`: duplicate / missing mod strings (a duplicate key crashes the mod at load).
- `tools/keys.py`: SendInput by scan code. **It brings the game window to the front: tell the user first.**
- `tools/shot.ps1`: window screenshot → `shots/latest.png`. `tools/api.py Class...`: member summary from
  `game/api/game.txt` (made by `tools/summarize_api.py` from `ilspycmd` output in `game/acs`, gitignored).
- Player docs: `README.md` (English, reference) and `README.<code>.md` for every game language; keep them
  in step with the F1 help when keys, sounds or settings change. `python tools/build_readme.py` refreshes
  the language links (between `<!-- LANGS -->` lines) and makes every `README*.html` with pandoc.
  Headings carry explicit English IDs (`{#installing}`) so in-page links work in every script.
- Logs: `MelonLoader/Latest.log`, `UserData/BallPitAccess/speech.log` (every spoken line), `events.log`.

## Install footprint (uninstall = delete these)

MelonLoader v0.7.3 x64 (official release, sha256 5b2b2f3d…a416): `version.dll`, `MelonLoader/`, `Mods/`,
`Plugins/`, `UserData/`. Added by us: `prism.dll`, `steam_appid.txt` (2062430), `Mods/BallPitAccess.dll`.
No game file was replaced.

## Pitfalls

- **Prism `PrismConfig` must match the shipped `prism.h`** (0.18.3 = config version 4, adds
  `availability_baseline_callback`). The struct copied from the undisputed mod was short: the game crashed
  before its first frame (access violation at address 4, nothing in the log).
- Speech and audio are initialised on the first `OnUpdate`, Harmony patches after 8 s of real time.
- Heredocs with backslashes get mangled in this shell (`\n` inside a C# string became a newline): use the
  Edit tool for code containing escapes.
- Il2Cpp object identity: compare `.Pointer`. Casts: `TryCast<T>()`, never `as` / `is`.
- Heredocs also break on a quote after a backslash: for multi-line code edits write a script file.
- A `CoolButton` ignores `OnSubmit` unless it has been the selected one for a moment (Select then Submit
  in the same frame does nothing): to press a button that is not focused use `Navigator.Click`, which
  runs its `OnClicked` delegate.
- The game handles Backspace as "back" itself on some screens (settings): `Navigator.Back` acts two
  frames later and only if the screen is still the same, or one press goes back twice.
- Level select keeps the selection on the selected level (focus on its Play button bounces back within
  half a second): the level is the only stop there.
- `OverlayUI.DefaultFocus` may be a row, not a button (pause menus): focus its first item.
- Items of a `ScrollRect` that are scrolled out fail the raycast test: `Navigator.ScrolledOut` keeps
  them in the list and `ScrollIntoView` moves the content on focus.
- `GameTutUI` stays in the overlay stack as a banner while the battle goes on
  (`ShouldBlockInput()` false): `Navigator.TopOverlay` skips it and `TrackBanner` speaks it once.
- A screen lists only the buttons under its own overlay (except during the guided tutorial): other
  buttons stay clickable beside a popup (battle speed buttons) and are not part of it.
- The level-up details panel's visible texts are objects named `TxUpgName` / `TxUpgDesc` / `TxUpgTitle`
  (`LevelUpUI.LocDetailsDesc` is not the one shown); a bare translation loses its numbers because the
  `{[PARAM]}` placeholders are dropped. `FocusReadout.Retry` makes `MenuReader` look again shortly. Every details panel that follows
  the hover lags the same way: a reader that copies one calls `Readers.PanelBehind` (panel still shows
  what it showed for another item → `Retry`; logged as `Panel behind the focus`).
- `MenuReader` waits while `Navigator.Settling` (a screen or page announcement is pending) and stays
  quiet in a battle unless a screen is open.
- Ctrl+Shift+D writes `List (dump)` with the raw texts of the open screen to `Latest.log`.
- **Test saves without keys**: with `Debug = true`, `Game/DevCommands` runs the lines of
  `UserData/BallPitAccess/dev.txt` (`resources N`, `blueprint kX`, `unseen kTut`, `tut kTut`, `probe`,
  `select kX`, `state`…) through the game's own SaveMgr / BaseTutMgr. Seen base tutorials are
  `MetaSaveData.IsBaseTutSeen` (the game's `kResetBaseTutorials` cheat does nothing in the shipped build).
  Saves: `%USERPROFILE%/AppData/LocalLow/Kenny Sun/BALL x PIT/meta1*.yankai` (Odin binary); back them up before
  testing and restore with the game closed. A rich test save is in `game/save_backup/test_after_2026-11-17`.
- The game reads **Backspace as its own back on key release**: in the base's normal state it opens the pause
  menu. A mod action on Backspace that returns to the normal state must wait for the release
  (`Placement.PendingCancel`).
- `BaseMgr.OnBPressed` only leaves the harvest aim, launcher and level transitions; expansion and rearrange
  are left with their sidebar toggles (`BaseUI.OnExpandClicked` / `OnRearrangeClicked`).
- `CoolButton.OnSubmit` is gated by input state (`InputMgr._isInputAConsumed` and a static flag): after a
  Ctrl+Tab category change the first build item ignored it. Build items are pressed with `Navigator.Click`.
- `BaseGridMgr.FindEmptySpots` returns single free tiles; `CanBuildPlacePreview` tests the preview's colliders
  where it stands (a few frames after `SetHover`), so multi-tile fit is found by trying spots in turn.
- **The placement preview is made in `BaseGridMgr.SetHover`** (only the game's mouse handler calls it each
  frame). While `Placement.Claim` owns the hover that handler is skipped: `Placement.EnsurePreview` makes the
  preview itself when the base is in `kPlaceBuilding` without one.
- **Which building is on a tile**: `MetaSaveData.BaseChunks[..][..]` → `BaseChunkInst.Buildings[x][y]` (8 × 6
  tiles per chunk), read through `Game/BaseGrid`. `GetBuildingAtWorldPos` uses colliders, which reach over the
  neighbours' tiles. A preview's hover tile is its middle column / row (`BaseGrid.PreviewTopLeft`); R does not
  change the tiles a building takes, although `Inst.GetTileSize()` swaps.
- `BaseMgr.MyUpdate` opens the base pause menu on `Input.GetKeyDown(Backspace)` (Unity input, not Rewired:
  muting the keyboard maps does not stop it). `Explore` skips `BasePauseUI.Activate` instead.
- **Testing with no game window** (a Claude session in Windows session 0 cannot show one): launch
  `Balls.exe -batchmode -nographics -logFile <path>`; drive it with `dev.txt` (`focus on`, then
  `key tab shift+tab enter u …`: mod keys injected one frame each). The aim postfix never runs there
  (no mouse): `forceaim on` pushes the assist's aim. `Physics2D.CircleCastAll` is stripped from the
  build (calling it closes the game silently): use only physics calls the game itself uses.
- **Workers and balls are rays**: `Physics2D.Raycast(pos, aim, step, ColMgr.kLayerMaskBounce)`, bounce by
  reflection; wheat-type buildings (`kLayerMaskPickup`) are harvested by an overlap of radius
  `BallObj.PickupRange` (0.125). In battle the side / back walls are `GridMgr` borders, not colliders.
- **Boss health is `GridPieceObj.GetHealthPct()`** on the main piece: the Skeleton King's own CurHealth never
  moves, his body takes no damage, and his crown is a child `SubGridPieceObj` that is not in `Pieces`.
- `GameMgr.CurState`: several states open an `OverlayUI` (level up, pause, revive, game over…); the
  navigator announces the screen, `Battle` only plays the cue, or both would speak.

# Screens

Inventory from the code (`OverlayUI` subclasses in `game/api/game.txt`, plus the non-overlay UIs and the
play modes). Status: **verified** = exercised in the running game with speech; **written** = code exists,
not yet exercised; **gap** = known hole. Nothing is marked verified because it compiled.

One key grammar everywhere: every screen is one list ordered by what things are (never by screen
position): Up / Down walk it, Home / End go to the ends of the group, Tab / Shift+Tab jump to the next /
previous group, Enter activates (on a character or a level it lands on the confirming button), Left /
Right change a list or slider value (or the difficulty on a level's Play button), Ctrl+Tab changes the
screen's tab, Backspace (or the game's Escape) goes back, Ctrl+Up reads details, F2 reads the whole
screen. In the tables below "Tab changes section / category" now means Ctrl+Tab.

## Outside play

| Screen | Class | What the mod does | Status |
|---|---|---|---|
| Boot: seizure warning, shader preparation | `MainMenuUI` (`IsRunningSplashScreen`, `CvsGrpEpilepsy`, `CvsGrpShaders`) | Text spoken when each shows; Enter skips shader preparation (`OnPrecompileSkipClicked`) | written |
| Main menu | `MainMenuUI`, `MainMenuBtn` | Lands on the first entry; arrows, Enter | verified (arrows, Enter); auto-focus written |
| Language choice (first run) | `MainMenuOption.kLanguage*` buttons | Text buttons, generic reader | written |
| Save slots | `SelectSlotUI`, `SelectSlotItem` | "Slot 2, last played…, play time…", empty, delete button | written; progress pips (base and DLC) are images, not read (gap) |
| Intro cutscene | `IntroCutsceneObj` | Captions spoken as they change; Enter skips (`Complete(true)`) | written |
| Credits | `CreditsUI` | "Credits" announced; F2 reads what is on screen | written; no skip key (gap) |
| Cloud save conflict | `CloudOutOfSyncUI` | Generic: texts and buttons | written |
| Demo / unlock messages | `FullScreenMessageUI` | Title + message | written |
| Dialogs | `DialogUI` | Message spoken, focus on the default button | verified |
| Settings (general, video, audio, Twitch) | `BaseSettingsUI`, `SettingsScreen`, `SettingsItem` | Label, value, control kind; Left / Right change lists and sliders, Enter toggles; description in details; Tab changes tab | written |
| Key remapping | `SettingsRemapItem` | Action and its two keys read; Enter starts the game's own capture | written; the capture prompt itself is untested |

## Base (New Ballbylon)

| Screen / mode | Class / state | What the mod does | Status |
|---|---|---|---|
| Side bar | `BaseUI`, `BaseSidebarBtn` | Label from its I2 term, disabled / active | verified (labels) |
| Guided tutorial | `MaskedFTUEOverlay` | Instruction spoken, focus on the pointed button | verified |
| Tutorial popup | `GameTutUI` | Keys line + message, mouse glyphs replaced | verified |
| Bottom hint | `BaseUI.LocMainPrompt` | Read when it changes | written |
| Buildings in the world | `BaseState.kNormal`, `BaseGridMgr` | Tab walks buildings (name, level, construction, harvest, worker), Enter = click, Delete = right click (dismantle); details from the game's hover popup | written |
| Build menu | `BuildUI`, `BuildItem` | Name, cost, locked / unaffordable / new; description in details; Tab changes category | verified (items); categories written |
| Placement | `kPlaceBuilding` | Arrows move a tile, fits / blocked, Enter builds, Home, R, Backspace cancels | verified (move, build) |
| Rearrange | `kRearrangeBuildings` | Tab chooses, Enter picks up, arrows move, Enter drops | written, **unverified guess** at the drop call (`ClickRearrange`) |
| Expansion | `kSelectingExpansion` | Tab walks plots for sale with cost, Enter = `ClickOnExpansion` | written |
| Lift (elevator) | `BaseElevatorObj`, `BaseHoverPopup.HoverElevator` | First entry of the Tab list; popup (gear counts) in details; Enter opens its upgrade | written (session 3) |
| Upgrade building / lift | `UpgradeBuildingUI` | Title, description, cost, gear progress; lift path buttons (pit / Risen Ballbylon) named | written |
| Workers | `WorkerUI`, `WorkerItem` | Name, level, goes / stays, capacity; reorder buttons named | written |
| Assign worker / war room | `AssignWorkerUI`, `AssignWorkerItem`, `WarRoomSelectLvlItem`, `HarvestUpgradeBtn` | Pages announced; items named | written |
| Harvest aim | `kAimWorkers` | Tab chooses a building, Left / Right turn, Enter launches | verified |
| Harvest running | `kBounceWorkers` | Hold Space = speed up | written (speed effect unconfirmed) |
| Harvest summary | `HarvestSummaryUI` | Title + resources; building items named | verified (popup); items written |
| Launcher aim | `kAimLauncher` | Announced as needing the mouse | **gap** |
| Market | `MarketUI`, `MarketItem` | "Buy 5 wheat for 20 gold"; quantity buttons | written |
| Masseuse | `MasseuseUI`, `MasseuseItem` | Instructions; name, stamina, cost | written |
| Character select | `CharSelectUI`, `CharSelectItem` | Name, level or locked + requirement; descriptions and stats in details | written |
| Character stats | `CharStatsUI` | Generic text; F2 | written |
| Level select | `LevelSelectUI`, `LevelSelectItem` | Set choice (pit / Risen Ballbylon) named; altitude sickness notice on the DLC list; number, name, locked + why; difficulty buttons named with the current difficulty; Page Up / Down = new game plus tier; details | written |
| Loadout (characters, pets) | `LoadoutUI`, `EquippedCharBtn`, `EquippedPetItem`, `SelectPetItem` | Slot, name, level | written |
| Hatchery | `HatcheryUI`, `HatcheryItem`, `EggListItem` | Pages announced; pet name, type, level, cost | written |
| Pet lab | `PetLabUI`, `PetLabItem`, `PetLabSelectItem`, `TestTubeItem` | Same | written |
| Pet name | `PetNameUI` | Field focused, typed characters echoed, Enter confirms | written |
| Encyclopedia | `EncyclopediaUI`, `EncyclopediaItem` | Name or "not discovered" or DLC-locked; details panel in details; Tab changes section, Page Up / Down changes set (base, free DLC, Risen Ballbylon) | written |
| Item unlocks | `ItemUnlockUI`, `ItemUnlockItem` | Title, description; item names | written |
| Leaderboards | `LeaderboardUI` | Generic text and buttons | written, rows not specially read (gap) |
| Return from expedition | `BaseReturnUI` | Title + resources brought back | written |
| Base pause | `BasePauseUI` | Text buttons | written |
| Twitch vote | `TwitchVoteUI` | Generic | not a goal |

## Battle

| Screen / mode | Class / state | What the mod does | Status |
|---|---|---|---|
| Play | `GameState.kPlaying` | Radar, ball cues, hazards, pickups, aim assist, HUD keys (see CUES.md and F1) | written, cues **not yet tuned by ear** |
| Tutorial popups | `GameTutUI` | Message; the shooting tutorial is rewritten for the aim assist | written |
| Level up: stat gains | `LevelUpUI` page `kViewStats` | Title + the gains | written |
| Level up: pick an upgrade | `LevelUpBtn` | Name, kind, level / new, description; numbers (old → new) in details; reroll, banish read with their cost | written |
| Fuser | `LevelUpUI` page `kGotFuser`: `FuserOptionBtn`, `EvoSelectBtn`, `ComboSelectItem`, `FreeUpgradeItem` | Each step announced; options named, availability, description | written |
| Bonus ball / passive | same UI | same | written |
| Pause | `PauseUI`, `PauseEquipInfoBtn` | Tab: character, equipment, system; equipment named with its panel in details | written |
| Treasure / blueprint / egg | `BlueprintFoundUI`, `EggFoundUI` | Title, name, description | written |
| Revive | `ReviveUI` | Question, Yes / No | written |
| Character level up after a run | `GameCharInfoUI` | Pages announced; harvest upgrade choices read | written |
| Run summary | `GameOverUI`, `GameOverBallStatItem` | Waits for the counters, then time, best, kills, XP, resources, blueprints, trophy, gears; per-ball damage | written |
| Endings | `GameCompleteObj`, `GameCompleteObjDLC` | Announced; the DLC ending's captions spoken | written; no skip key |
| Settings / encyclopedia from pause | as above | as above | written |

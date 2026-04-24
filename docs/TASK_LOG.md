# Task Log

## 2026-04-24 - Initial Shell Creation

- Created the Godot 4 .NET project shell.
- Added a main menu with New Run and Quit actions.
- Added a gameplay shell with a placeholder grid, player marker, discrete movement, turn counter, position display, and message log.
- Added initial project documentation for brief, scope, architecture, and task history.

## 2026-04-24 - Shell Foundation Review

- Reviewed the initial shell for responsibility boundaries and project scope.
- Separated the player marker visual from `PlayerController` so movement input/state is not tied to marker drawing.
- Kept functionality unchanged: menu flow, new run, grid display, movement, turn count, position display, message log, and Escape return.
- Updated architecture documentation to match the cleaned-up structure.

## 2026-04-24 - Test-Friendly Repository Structure

- Moved Godot-facing scenes and scripts under `src/Godot/`.
- Added `src/SurvivalGame.Domain/` for plain C# domain code.
- Added `tests/SurvivalGame.Domain.Tests/` with initial tests for grid bounds behavior.
- Updated Godot scene paths and project references for the new layout.
- Added `.gitignore` entries for Godot and .NET generated files.

## 2026-04-24 - Human Player Sprite

- Replaced the simple octagonal player marker with a small human-looking composed sprite in `GameShell.tscn`.
- Kept player movement and simulation/input responsibilities unchanged.

## 2026-04-24 - Minimal Player Inventory Tracking

- Added domain-level player inventory tracking for item ids and quantities.
- Added tests for adding, stacking, removing, and validating inventory quantities.
- Updated scope and architecture docs to clarify that inventory UI and item interactions are not included yet.

## 2026-04-24 - Inventory Display Panel

- Added a read-only inventory section to the gameplay overlay.
- Added `InventoryPanel.cs` to display held item ids and quantities from `PlayerState.Inventory`.
- Kept inventory actions, item pickup/drop/use, equipment, capacity, and item definitions out of scope.

## 2026-04-24 - Prototype Item Definitions

- Added domain item definitions, item catalog lookup, and nested item type paths.
- Added prototype item definitions including `Weapon -> Gun -> Rifle -> AK47`.
- Seeded the shell player with a small prototype starting inventory so the inventory panel shows items.
- Added tests for item subtype checks and catalog behavior.

## 2026-04-24 - Prototype Ground Items

- Added domain storage for item stacks placed on grid positions.
- Added a Godot ground item layer that renders simple colored item markers on tiles.
- Placed a few prototype item stacks on the map for visual testing.
- Kept pickup, drop, use, and item interaction mechanics out of scope.

## 2026-04-24 - JSON Item Definition Data

- Moved item definitions into JSON files under `data/items/`.
- Added `ItemDefinitionLoader` to load item definition files into the domain item catalog.
- Expanded item definitions with description, category, tags, max stack size, weight, icon id, sprite id, and future action ids.
- Kept nested type checks by deriving item type paths from category and ordered tags.

## 2026-04-24 - Near-Future Folder Structure

- Reorganized Godot presentation files by feature under `src/Godot/MainMenu/` and `src/Godot/Game/`.
- Split gameplay presentation into `WorldView/`, `UI/`, and `Prototype/` folders.
- Reorganized domain code by concept under `Actors/`, `World/`, `Items/`, `Inventory/`, and `Content/`.
- Split broad domain bucket files into smaller item and inventory files.
- Moved domain tests into folders that mirror the domain concepts.
- Added README guidance files for the new folders so future AI/developer sessions know what belongs where.

## 2026-04-24 - Item Tile Hover Tooltip

- Added a read-only tooltip for hovering over tiles that contain item stacks.
- The tooltip shows the tile position, item names, stack quantities, and item descriptions.
- Kept item pickup, drop, use, and inspection actions out of scope.

## 2026-04-24 - AK-47 Sprite Applied

- Added generated AK-47 sprite asset at `data/sprites/items/item_ak47.png`.
- Updated the ground item renderer to draw item sprites when a matching `spriteId` asset exists.
- Kept non-sprited items on the existing simple marker fallback.

## 2026-04-25 - Core Action Pipeline

- Added a domain action pipeline for movement, Wait, and Pick Up.
- Added prototype game state in the domain layer for player position, turn count, inventory, and ground items.
- Updated movement input to request move actions instead of mutating player position directly.
- Added an Actions panel with clickable Wait and context-sensitive Pick Up buttons.
- Pickup now moves item stacks from the current tile into the player inventory and advances the turn.

## 2026-04-25 - Responsive Gameplay Layout

- Enabled resizable window/stretch settings for the Godot project.
- Updated the gameplay shell to position the grid board and side panel based on current viewport size.
- Kept the board visible and centered in the available area beside the right-side UI panel.

## 2026-04-25 - Prototype Surface Definitions

- Added domain terrain/surface definitions, a surface catalog, and a bounded surface map.
- Added JSON surface data for grass, carpet, concrete, tile, and ice.
- Updated the grid renderer to color tiles from surface definition data.
- Added tests for loading surface definitions and reading/writing surface map state.
- Kept surface-driven movement effects, including ice sliding, out of scope for now.

## 2026-04-25 - Surface Tags as Properties

- Simplified surface properties back into the existing tag system.
- Removed the separate surface property catalog and loader before they became extra architecture.
- Marked ice with a `slippery` tag for future rule checks.
- Kept all surface tags descriptive only; they do not affect movement or turns yet.

## 2026-04-25 - Surface UI Usage

- Added the current player tile surface to the gameplay overlay.
- Updated tile hover details to show surface name, description, and tags on every tile.
- Kept surface tags descriptive only; they do not affect movement or turns yet.

## 2026-04-25 - Responsive Board Scaling

- Updated the gameplay board to scale its cell size from the current viewport instead of staying locked to 32px cells.
- Reconfigured grid rendering, ground item rendering, hover detection, and player marker placement from the same runtime cell size.
- Switched Godot stretch mode from fixed viewport scaling to canvas item scaling for a cleaner resizable 2D window.
- Kept the map dimensions and gameplay behavior unchanged.

## 2026-04-25 - Player Vital Tracking

- Added domain tracking for health, hunger, thirst, fatigue, sleep debt, pain, and body temperature.
- Added a reusable bounded meter value for 0-100 tracked player meters.
- Added a read-only player status panel in the gameplay overlay.
- Added tests for default vitals, independent updates, and validation.
- Kept survival simulation rules, damage, healing, decay, and status effects out of scope.

## 2026-04-25 - Prototype World Objects

- Added domain definitions, catalog loading, and tile placement for static world objects.
- Added JSON definitions for wall, tree, fridge, wooden door, window, table, chair, bed, storage crate, and boulder.
- Added a Godot world object layer to render placed objects from domain state.
- Added movement blocking for world objects whose definitions block movement.
- Added world object details to tile hover tooltips.
- Added tests for object loading, placement, and movement blocking behavior.
- Kept object interactions, containers, destruction, opening doors, and looting out of scope.

## 2026-04-25 - World Object Sprites

- Generated and added sprite assets for fridge, storage crate, and bed.
- Added optional `spriteId` support to world object definitions.
- Updated the world object renderer to draw object sprites when available and fall back to simple rectangles otherwise.
- Kept world object behavior unchanged.

## 2026-04-25 - Smaller Tiles

- Reduced the responsive gameplay tile size to about 70% of the fitted board size.
- Kept movement, map dimensions, collision, hover detection, and rendering aligned to the same scaled cell size.

## 2026-04-25 - Surface Tile Sprites

- Generated bitmap sprite assets for grass, carpet, concrete, ceramic tile, and ice surfaces.
- Added optional `spriteId` support to surface definitions.
- Updated the grid renderer to draw surface sprites when available and fall back to map colors otherwise.
- Kept surface behavior unchanged; sprite assets only affect presentation.

## 2026-04-25 - Game State Ownership Refactor

- Reviewed current game state ownership against the intended Godot/domain boundary.
- Kept `PrototypeGameState` as the root prototype state rather than renaming it.
- Added `TurnState` for turn count ownership.
- Added `WorldState` and `MapState` so world/map data is grouped outside the root state.
- Moved player grid position into `PlayerState`.
- Updated `GameActionPipeline` and `GameShell` to use `state.Turn`, `state.Player`, and `state.World`.
- Added focused domain tests for the state hierarchy and turn advancement.
- Preserved existing gameplay behavior and avoided new simulation systems.

## 2026-04-25 - Default Window Size

- Updated the default Godot viewport size to 1920x1080.
- Kept the existing responsive layout and resizable window settings.

## 2026-04-25 - Top-Left World Layout

- Updated the gameplay layout so the world board sits in the top-left region.
- Sized the world board from roughly half the viewport width and two-thirds of the viewport height.
- Kept the existing right-side UI panel and gameplay behavior unchanged.

## 2026-04-25 - Larger Info Text

- Increased gameplay overlay, action button, inventory, status, log, and tooltip text sizes slightly.
- Kept the existing UI layout and content unchanged.

## 2026-04-25 - Equipment Slot Domain Model

- Added domain equipment slot ids, definitions, groups, catalog, loadout, equipped item refs, and validation.
- Attached a default equipment loadout to `PlayerState`.
- Added default slots for main hand, off hand, head, body, legs, feet, and back.
- Added tests for default slots, empty/occupied slots, accepted type paths, and invalid type rejection.
- Kept equip/unequip actions, inventory transfer, equipment UI, and item effects out of scope.

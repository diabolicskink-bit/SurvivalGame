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

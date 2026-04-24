# Architecture

## Repository Structure

- `docs/` contains project documentation.
- `data/` contains runtime game content loaded through Godot `res://data/...` paths.
- `data/sprites/items/` contains item sprite bitmap assets referenced by item definition `spriteId` values.
- `data/sprites/surfaces/` contains terrain/surface sprite bitmap assets referenced by surface definition `spriteId` values.
- `data/sprites/world_objects/` contains world object sprite bitmap assets referenced by world object definition `spriteId` values.
- `data/surfaces/` contains JSON terrain/surface definition files.
- `data/world_objects/` contains JSON definitions for static map objects such as walls, trees, and furniture.
- `src/Godot/` contains Godot-facing scenes and scripts, grouped by feature.
- `src/Godot/MainMenu/` contains the main menu scene and menu script.
- `src/Godot/Game/` contains the current playable prototype shell.
- `src/Godot/Game/WorldView/` contains visible world, grid, item marker, and input-view scripts.
- `src/Godot/Game/UI/` contains gameplay overlay controls.
- `src/Godot/Game/Prototype/` is reserved for temporary Godot-side prototype helpers.
- `src/SurvivalGame.Domain/` contains plain C# domain code grouped by simulation/content concept.
- `src/SurvivalGame.Domain/Actions/` contains action requests, action resolution, turn state, and the current prototype root state.
- `src/SurvivalGame.Domain/Actors/` contains player and future actor state.
- `src/SurvivalGame.Domain/World/` contains grid/map/world primitives.
- `src/SurvivalGame.Domain/WorldObjects/` contains static world object ids, definitions, catalogs, and tile placement maps.
- `src/SurvivalGame.Domain/Items/` contains item ids, definitions, catalogs, type paths, and placed item stacks.
- `src/SurvivalGame.Domain/Inventory/` contains inventory data structures and rules.
- `src/SurvivalGame.Domain/Equipment/` contains player equipment slot definitions, loadout state, and type-path validation.
- `src/SurvivalGame.Domain/Content/` contains data loaders and content adapters.
- `tests/SurvivalGame.Domain.Tests/` contains unit tests for domain code, grouped to mirror the domain folders.
- `data/items/` contains JSON item definition files grouped by broad content area.

## Scene Structure

- `src/Godot/MainMenu/MainMenu.tscn` is the entry scene. It uses `src/Godot/MainMenu/MainMenu.cs` to build the simple menu UI and handle button actions.
- `src/Godot/Game/GameShell.tscn` is the prototype gameplay scene. It contains the board, surface-colored grid, player marker, player controller, and UI layer.

## Script Responsibilities

- `MainMenu.cs` handles menu button actions.
- `GameShell.cs` coordinates the gameplay shell scene, sends action requests to the domain action pipeline, positions the player marker, and refreshes the UI overlay from shell state.
- `WorldView/GridView.cs` draws the visible grid from domain surface state. It uses surface sprite assets when available and falls back to surface definition display colors. It does not own terrain rules.
- `WorldView/WorldObjectLayer.cs` renders static world objects from domain placement data. It does not own object rules or placement state.
- `WorldView/GroundItemLayer.cs` renders item stacks placed on visible grid tiles. It uses item sprite assets when available and falls back to simple colored markers. It reads item placement data; it does not own item rules.
- `WorldView/PlayerController.cs` translates movement keys into move action requests. It does not own movement rules, position state, or player rendering.
- `UI/ActionPanel.cs` displays currently available clickable actions such as Wait and Pick Up.
- `UI/PlayerStatusPanel.cs` displays tracked player vitals from domain state. It does not calculate survival effects.
- `UI/InventoryPanel.cs` displays the current player inventory. It does not own inventory state or item rules.
- `UI/ItemTooltip.cs` displays read-only hover details for the currently hovered tile, including its surface definition and any item stacks there.
- `UI/MessageLog.cs` owns the recent visible message display.

## Domain Code

Future simulation logic should be kept separate from Godot presentation where practical. Godot scenes and nodes should present state and collect input, while survival and roguelike rules can grow into plain C# services or model classes later.

The current shell still keeps a small amount of prototype state in Godot-facing scripts so the project can run. As real simulation systems are introduced, movement, turns, messages, and world state should move toward plain C# domain code with Godot nodes acting as views/controllers.

`Actions/GameActionPipeline.cs` is the central action pipeline. It validates and resolves current actions, advances turns for successful actions, and returns messages for the UI to display. Movement, Wait, and Pick Up already go through this pipeline. Movement checks map bounds and static world objects that block movement.

`Actions/PrototypeGameState.cs` is the current root shell/session state. It coordinates `TurnState`, `PlayerState`, and `WorldState` rather than directly owning every game detail.

`Actions/TurnState.cs` owns the current turn count and exposes the small `Advance()` operation used by successful player actions.

`Actors/PlayerState.cs` owns player-specific state: grid position, vitals, and inventory.

`World/WorldState.cs` owns state outside the player, currently the map, ground item placement, and static world object placement.

`World/MapState.cs` owns the current map bounds and tile surface map.

`World/GridBounds.cs` contains `GridBounds`, `GridPosition`, and `GridOffset`, which provide testable grid boundary behavior without instantiating Godot nodes.

`World/TileSurfaceDefinition.cs`, `World/TileSurfaceCatalog.cs`, `World/TileSurfaceMap.cs`, and `Content/TileSurfaceDefinitionLoader.cs` provide prototype terrain/surface definitions. Surface data is loaded from `data/surfaces/*.json` into plain C# objects. Surface definitions can optionally reference bitmap sprites through `spriteId` values under `data/sprites/surfaces/`. Surface tags double as lightweight properties for now, so future rules can ask whether a surface has tags such as `slippery`, `wet`, or `hot` without adding a separate property system yet. Godot currently uses surface definitions only to render the map.

`WorldObjects/WorldObjectDefinition.cs`, `WorldObjects/WorldObjectCatalog.cs`, `WorldObjects/TileObjectMap.cs`, and `Content/WorldObjectDefinitionLoader.cs` provide prototype static world object definitions and placement state. Object definitions are loaded from `data/world_objects/*.json` and currently describe display text, tags, movement blocking, sight blocking, map color, and optional sprite id. Interactions such as opening, looting, moving, destroying, or using objects are not implemented yet.

`Actors/PlayerVitals.cs` and `Actors/BoundedMeter.cs` provide the current minimal player vitals model. `PlayerVitals` tracks health, hunger, thirst, fatigue, sleep debt, pain, and body temperature as domain state. These values are only tracked and displayed for now; no metabolism, damage, healing, sleep, pain, exertion, or temperature simulation has been added yet.

`Inventory/PlayerInventory.cs`, `Inventory/InventoryItemStack.cs`, and `Items/ItemId.cs` provide the current minimal inventory model. They only track held item ids and quantities; item actions, equipment, weight, and inventory management are not part of this layer yet.

`Equipment/EquipmentLoadout.cs`, `Equipment/EquipmentSlotCatalog.cs`, `Equipment/EquipmentSlotDefinition.cs`, and `Equipment/EquipmentValidator.cs` provide the current equipment slot model. `PlayerState` owns an `EquipmentLoadout` with default slots for main hand, off hand, head, body, legs, feet, and back. Equipment stores `ItemId` references plus `ItemTypePath` classifications for validation; equip/unequip actions, inventory transfer, UI, item effects, and slot-specific rules are not implemented yet.

`Items/ItemDefinition.cs`, `Items/ItemCatalog.cs`, `Content/ItemDefinitionLoader.cs`, and `Items/ItemTypePath.cs` provide prototype item definitions. Item definitions are loaded from `data/items/*.json` into immutable C# objects. `ItemTypePath` is derived from category plus tags and supports nested subtype checks by prefix, for example `Weapon -> gun -> rifle -> ak47` is considered a `Weapon`, a `Gun`, and a `Rifle`.

`Items/PrototypeItems.cs` currently keeps stable ids and common type paths used by tests and the shell. The item definitions themselves live in JSON data files.

`Items/GroundItems.cs` contains `TileItemMap`, `GroundItemStack`, and `PlacedItemStack`, which provide minimal domain storage for item stacks placed on grid positions. They do not implement pickup, drop, visibility, physics, ownership, or item interactions.

## Tests

Domain tests live under `tests/SurvivalGame.Domain.Tests/` and should mirror domain concepts, such as `World/`, `Items/`, and `Inventory/`. Prefer adding tests for plain C# simulation/domain code before testing Godot presentation scripts directly.

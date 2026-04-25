# Architecture

## Repository Structure

- `docs/` contains project documentation.
- `data/` contains runtime game content loaded through Godot `res://data/...` paths.
- `data/sprites/items/` contains item sprite bitmap assets referenced by item definition `spriteId` values.
- `data/sprites/surfaces/` contains terrain/surface sprite bitmap assets referenced by surface definition `spriteId` values.
- `data/sprites/world_objects/` contains world object sprite bitmap assets referenced by world object definition `spriteId` values.
- `data/firearms/` contains JSON firearm, ammunition, and feed-device definitions used by the firearm domain catalog.
- `data/surfaces/` contains JSON terrain/surface definition files.
- `data/world_objects/` contains JSON definitions for static map objects such as walls, trees, and furniture.
- `src/Godot/` contains Godot-facing scenes and scripts, grouped by feature.
- `src/Godot/MainMenu/` contains the main menu scene and menu script.
- `src/Godot/Game/` contains the current playable prototype shell.
- `src/Godot/Game/WorldView/` contains visible world, grid, actor marker, item marker, and input-view scripts.
- `src/Godot/Game/UI/` contains gameplay overlay controls.
- `src/Godot/Game/Prototype/` is reserved for temporary Godot-side prototype helpers.
- `src/SurvivalGame.Domain/` contains plain C# domain code grouped by simulation/content concept.
- `src/SurvivalGame.Domain/Actions/` contains action requests, action resolution, world time state, and the current prototype root state.
- `src/SurvivalGame.Domain/Actors/` contains player state, NPC state, and actor collections.
- `src/SurvivalGame.Domain/World/` contains grid/map/world primitives.
- `src/SurvivalGame.Domain/WorldObjects/` contains static world object ids, definitions, catalogs, and tile placement maps.
- `src/SurvivalGame.Domain/Items/` contains item ids, definitions, catalogs, type paths, and placed item stacks.
- `src/SurvivalGame.Domain/Inventory/` contains inventory data structures and rules.
- `src/SurvivalGame.Domain/Equipment/` contains player equipment slot definitions, loadout state, and type-path validation.
- `src/SurvivalGame.Domain/Firearms/` contains firearm, ammunition, feed-device definitions, runtime loaded state, and firearm action rules.
- `src/SurvivalGame.Domain/Content/` contains data loaders and content adapters.
- `tests/SurvivalGame.Domain.Tests/` contains unit tests for domain code, grouped to mirror the domain folders.
- `data/items/` contains JSON item definition files grouped by broad content area.

## Scene Structure

- `src/Godot/MainMenu/MainMenu.tscn` is the entry scene. It uses `src/Godot/MainMenu/MainMenu.cs` to build the simple menu UI and handle button actions.
- `src/Godot/Game/GameShell.tscn` is the prototype gameplay scene. It contains the board, surface grid, world object layer, item layer, NPC layer, player marker, player controller, and UI layer.

## Script Responsibilities

- `MainMenu.cs` handles menu button actions.
- `GameShell.cs` coordinates the gameplay shell scene, sends action requests to the domain action pipeline, positions the player marker, and refreshes the UI overlay from shell state.
- `WorldView/GridView.cs` draws the visible grid from domain surface state. It uses surface sprite assets when available and falls back to surface definition display colors. It does not own terrain rules.
- `WorldView/WorldObjectLayer.cs` renders static world objects from domain placement data. It does not own object rules or placement state.
- `WorldView/GroundItemLayer.cs` renders item stacks and stateful items placed on visible grid tiles. It uses item sprite assets when available and falls back to simple colored markers. It reads item placement data; it does not own item rules.
- `WorldView/NpcLayer.cs` renders NPCs from domain actor state. It does not own NPC health, placement, AI, or behavior.
- `WorldView/PlayerController.cs` translates movement keys into move action requests. It does not own movement rules, position state, or player rendering.
- `UI/ActionPanel.cs` displays global and selected-target clickable actions such as Wait, Pick Up, and Shoot inside the player info/general panel. It should not display every item-specific action at once.
- `UI/PlayerStatusPanel.cs` displays tracked player vitals from domain state. It does not calculate survival effects.
- `UI/EquipmentPanel.cs` displays every player equipment slot separately from inventory and emits selection events for occupied slots. It does not own equipment state or equip rules.
- `UI/InventoryPanel.cs` displays selectable held inventory items, including simple item stacks and freely carried stateful items. It does not own inventory state or item rules.
- `UI/SelectedItemPanel.cs` is used inside the item click popup to display details and contextual actions for the selected inventory or equipment item. It filters actions from existing domain action requests; it does not invent item rules.
- `UI/FirearmPanel.cs` is retained as a prototype firearm status control, but the current gameplay overlay primarily surfaces firearm/feed state through selected item details.
- `UI/ItemTooltip.cs` displays read-only hover details for the currently hovered tile, including its surface definition and any NPC, item stacks, or stateful items there.
- `UI/MessageLog.cs` owns the recent visible message display.

## Domain Code

Future simulation logic should be kept separate from Godot presentation where practical. Godot scenes and nodes should present state and collect input, while survival and roguelike rules can grow into plain C# services or model classes later.

The current shell still keeps a small amount of prototype state in Godot-facing scripts so the project can run. As real simulation systems are introduced, movement, time, messages, and world state should move toward plain C# domain code with Godot nodes acting as views/controllers.

`Actions/GameActionPipeline.cs` is the central action pipeline. It validates and resolves current actions, advances elapsed world ticks for successful time-costing actions, and returns messages plus the tick cost for the UI to display. Movement, Wait, Pick Up, stack item inspect/drop/equip/unequip, stateful item pickup/drop/equip/unequip/inspect, firearm handling, and targeted NPC shooting actions go through this pipeline. Movement checks map bounds, static world objects, and NPCs that block movement. Equip Item validates inventory, item definitions, and equipment slot compatibility, then fills an empty slot without advancing time. Firearm handling and targeted shooting delegate to the firearm domain action service.

The current tick costs are intentionally simple: Move costs 100 ticks, Wait costs 100 ticks, successful Pick Up costs 50 ticks, loading ammunition costs 10 ticks per round loaded, inserting or removing a detachable feed device costs 25 ticks, reloading an inserted detachable feed device combines remove + per-round load + insert costs, and successful shooting costs 100 ticks. Failed actions currently cost 0 ticks. Terrain-based movement modifiers, survival decay, actor scheduling, calendars, and day/night behavior are not implemented yet.

`Actions/PrototypeGameState.cs` is the current root shell/session state. It coordinates `WorldTime`, `PlayerState`, and `WorldState` rather than directly owning every game detail.

`Actions/WorldTime.cs` owns elapsed world ticks and exposes the small `Advance(int ticks)` operation used by successful time-costing player actions.

`Actors/PlayerState.cs` owns player-specific state: grid position, vitals, and inventory.

`World/WorldState.cs` owns state outside the player, currently the map, ground item placement, static world object placement, and NPC roster.

`World/MapState.cs` owns the current map bounds and tile surface map.

`World/GridBounds.cs` contains `GridBounds`, `GridPosition`, and `GridOffset`, which provide testable grid boundary behavior without instantiating Godot nodes.

`World/TileSurfaceDefinition.cs`, `World/TileSurfaceCatalog.cs`, `World/TileSurfaceMap.cs`, and `Content/TileSurfaceDefinitionLoader.cs` provide prototype terrain/surface definitions. Surface data is loaded from `data/surfaces/*.json` into plain C# objects. Surface definitions can optionally reference bitmap sprites through `spriteId` values under `data/sprites/surfaces/`. Surface tags double as lightweight properties for now, so future rules can ask whether a surface has tags such as `slippery`, `wet`, or `hot` without adding a separate property system yet. Godot currently uses surface definitions only to render the map.

`WorldObjects/WorldObjectDefinition.cs`, `WorldObjects/WorldObjectCatalog.cs`, `WorldObjects/TileObjectMap.cs`, and `Content/WorldObjectDefinitionLoader.cs` provide prototype static world object definitions and placement state. Object definitions are loaded from `data/world_objects/*.json` and currently describe display text, tags, movement blocking, sight blocking, map color, and optional sprite id. Interactions such as opening, looting, moving, destroying, or using objects are not implemented yet.

`Actors/PlayerVitals.cs` and `Actors/BoundedMeter.cs` provide the current minimal player vitals model. `PlayerVitals` tracks health, hunger, thirst, fatigue, sleep debt, pain, and body temperature as domain state. These values are only tracked and displayed for now; no metabolism, damage, healing, sleep, pain, exertion, or temperature simulation has been added yet.

`Actors/NpcState.cs`, `Actors/NpcRoster.cs`, and `Actors/NpcId.cs` provide the current minimal NPC model. NPCs have identity, display name, grid position, and health. NPCs can take direct damage and clamp health at 0. NPC AI, faction logic, death/removal behavior, melee actions, and pathfinding are not implemented yet.

`Inventory/PlayerInventory.cs`, `Inventory/InventoryItemStack.cs`, and `Items/ItemId.cs` provide the current stack inventory model. This remains the right place for simple identical quantities such as ammunition rounds, crafting materials, and basic consumable stacks. Stack-backed items can be inspected and dropped through the action pipeline without becoming individual stateful item instances.

`Equipment/EquipmentLoadout.cs`, `Equipment/EquipmentSlotCatalog.cs`, `Equipment/EquipmentSlotDefinition.cs`, and `Equipment/EquipmentValidator.cs` provide the current equipment slot model. `PlayerState` owns an `EquipmentLoadout` with default slots for main hand, off hand, head, body, legs, feet, and back. Equipment stores `ItemId` references plus `ItemTypePath` classifications for validation. The current Equip Item action transfers one item from inventory into an empty compatible slot and costs 0 ticks. Legacy stack-backed equipment can now be unequipped back into inventory through the action pipeline. Equipment replacement, item effects, and slot-specific simulation rules are not implemented yet.

`Firearms/WeaponDefinition.cs`, `Firearms/AmmunitionDefinition.cs`, `Firearms/FeedDeviceDefinition.cs`, and `Firearms/FirearmCatalog.cs` describe firearm content loaded from `data/firearms/*.json`. Weapons declare accepted ammunition sizes, feed type, built-in capacity when relevant, weapon family, compatible detachable feed devices, and prototype effective/maximum range in tiles. Ammunition tracks size, variant, and direct damage. Feed devices track capacity, ammunition size, kind, and compatible weapon families.

`Items/StatefulItemStore.cs`, `Items/StatefulItem.cs`, `Items/StatefulItemLocation.cs`, and related ids/state classes provide the current stateful item model. A stateful item is a specific thing with a stable runtime id, item definition id, quantity, location, condition, optional firearm/feed state, and optional contained item ids. Supported locations are player inventory, equipment, ground, inserted in another item, and contained inside another item. This model is used when identity matters, for example a loaded magazine, inserted feed device, equipped weapon, or backpack with contents.

The project intentionally has both stack inventory and stateful items right now. Simple identical content can stay in `PlayerInventory` as counts. Specific items that must preserve state across pickup, drop, equipment, insertion, removal, inspection, and containment should use `StatefulItemStore`.

`Firearms/PlayerFirearmState.cs`, `Firearms/WeaponRuntimeState.cs`, and `Firearms/FeedDeviceState.cs` own legacy stack-backed loaded firearm state. `FeedDeviceState` is also reused by stateful feed-device items and built-in stateful weapon feeds. Feed devices currently allow only one ammunition item/variant at a time; unload before switching variants. Runtime firearm state should be created by successful state-changing actions, not by read-only UI refresh or action availability queries.

`Firearms/FirearmActionService.cs` validates and resolves loading ammunition into feed devices, unloading feed devices, inserting/removing compatible feed devices, reloading inserted detachable feed devices, loading built-in weapon feeds, test firing one round, and shooting an NPC with an equipped firearm. It supports both legacy stack-backed item ids and newer stateful firearm/feed items. Reloading a detachable-feed weapon is a single prototype action that removes the inserted feed device, loads as many compatible held rounds as fit, and reinserts it while preserving the specific magazine state. Shooting selects an equipped firearm from main hand first, then off hand, checks target range against the weapon maximum range, consumes one loaded round, and applies the ammunition damage to the NPC. Failed firearm actions should leave inventory, loaded counts, inserted feeds, stateful item locations, NPC health, and tracked runtime firearm state unchanged. Test-fire actions currently cost 0 ticks; successful targeted shooting costs 100 ticks through the action pipeline. Shooting does not implement accuracy, misses, cover, line of sight, sound, recoil, durability, jamming, armor, hit locations, or ballistics.

`Items/ItemDefinition.cs`, `Items/ItemCatalog.cs`, `Content/ItemDefinitionLoader.cs`, and `Items/ItemTypePath.cs` provide prototype item definitions. Item definitions are loaded from `data/items/*.json` into immutable C# objects. `ItemTypePath` is derived from category plus tags and supports nested subtype checks by prefix, for example `Weapon -> gun -> rifle -> ak47` is considered a `Weapon`, a `Gun`, and a `Rifle`.

`Items/PrototypeItems.cs` currently keeps stable ids and common type paths used by tests and the shell. The item definitions themselves live in JSON data files.

`Items/GroundItems.cs` contains `TileItemMap`, `GroundItemStack`, and `PlacedItemStack`, which provide minimal domain storage for item stacks placed on grid positions. They do not implement pickup, drop, visibility, physics, ownership, or item interactions.

## Tests

Domain tests live under `tests/SurvivalGame.Domain.Tests/` and should mirror domain concepts, such as `World/`, `Items/`, and `Inventory/`. Prefer adding tests for plain C# simulation/domain code before testing Godot presentation scripts directly.

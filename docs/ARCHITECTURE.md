# Architecture

## Repository Structure

- `docs/` contains project documentation.
- `data/` contains runtime game content loaded through Godot `res://data/...` paths.
- `data/sprites/items/` contains item sprite bitmap assets referenced by item definition `spriteId` values.
- `data/sprites/surfaces/` contains terrain/surface sprite bitmap assets referenced by surface definition `spriteId` values.
- `data/sprites/world_objects/` contains world object sprite bitmap assets referenced by world object definition `spriteId` values.
- `data/firearms/` contains JSON firearm, ammunition, and feed-device definitions used by the firearm domain catalog.
- `data/npcs/` contains JSON NPC definition files.
- `data/surfaces/` contains JSON terrain/surface definition files.
- `data/world_objects/` contains JSON definitions for static map objects such as walls, trees, and furniture.
- `src/Godot/` contains Godot-facing scenes and scripts, grouped by feature.
- `src/Godot/MainMenu/` contains the main menu scene and menu script.
- `src/Godot/Overworld/` contains the first overworld travel screen and map drawing control.
- `src/Godot/Game/` contains the current playable prototype shell.
- `src/Godot/Game/GameSessionShell.cs` owns the current prototype run session and switches between overworld travel and local site gameplay.
- `src/Godot/Game/PrototypeSessionFactory.cs` creates the current prototype local gameplay state and content catalogs used by the session shell and standalone local scene.
- `src/Godot/Game/WorldView/` contains visible world, grid, actor marker, item marker, and input-view scripts.
- `src/Godot/Game/UI/` contains gameplay overlay controls.
- `src/Godot/Game/Prototype/` is reserved for temporary Godot-side prototype helpers.
- `src/SurvivalGame.Domain/` contains plain C# domain code grouped by simulation/content concept.
- `src/SurvivalGame.Domain/Actions/` contains action requests, action resolution, world time state, and the current prototype root state.
- `src/SurvivalGame.Domain/Overworld/` contains plain C# overworld travel state, positions, travel methods, fixed prototype points of interest, and movement/fuel rules.
- `src/SurvivalGame.Domain/Actors/` contains player state, NPC definitions, NPC runtime state, behavior profiles, and actor collections.
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

- `src/Godot/MainMenu/MainMenu.tscn` is the entry scene. It uses `src/Godot/MainMenu/MainMenu.cs` to build the simple menu UI and start a new run.
- `src/Godot/Game/GameSessionShell.tscn` is the current run scene. It creates the overworld travel state, the default prototype local site, and lazily creates the Route 18 Gas Station site. It starts on the overworld and switches between overworld and local modes while preserving shared run state.
- `src/Godot/Overworld/OverworldScreen.tscn` is the first overworld travel screen. It displays the overworld map, travel party marker, travel method, fuel when relevant, time, fixed points of interest, and Enter Site action.
- `src/Godot/Game/GameShell.tscn` is the local prototype gameplay scene. It contains the board, surface grid, world object layer, item layer, NPC layer, player marker, player controller, and UI layer. It can still run standalone, but in the normal New Run flow it is entered from the overworld through `GameSessionShell`. It sizes itself from the active local site's map bounds rather than a hardcoded prototype map size.

## Script Responsibilities

- `MainMenu.cs` handles menu button actions and launches `GameSessionShell` for New Run.
- `GameSessionShell.cs` owns the current prototype run state and is the presentation coordinator for overworld/local mode switching. It preserves overworld position, current travel method, vehicle fuel, shared world time, player inventory, equipment, and firearm/feed-device state by reusing the same shared state objects. It routes the `gas_station` overworld point of interest to the dedicated gas station local site; other current points of interest route to the default prototype local site.
- `PrototypeSessionFactory.cs` creates prototype local gameplay sessions, catalogs, starting inventory, stateful items, and action pipelines in one place. The default site and gas station site each keep their own map, terrain, objects, ground stacks, and NPC roster while sharing player, world time, stateful item store, and vehicle fuel state where appropriate.
- `OverworldScreen.cs` builds the overworld travel HUD, handles travel method selection, advances travel each frame, and exposes Enter Site when the party is near a point of interest.
- `OverworldMapView.cs` draws the simple overworld map, fixed points of interest, travel party marker, destination indicator, and destination line. It converts map clicks into overworld destination requests.
- `GameShell.cs` coordinates the gameplay shell scene, sends action requests to the domain action pipeline, positions the player marker, and refreshes the UI overlay from shell state.
- `WorldView/GridView.cs` draws the visible grid from domain surface state. It uses surface sprite assets when available and falls back to surface definition display colors. It does not own terrain rules.
- `WorldView/WorldObjectLayer.cs` renders static world objects from domain placement data. It does not own object rules or placement state.
- `WorldView/GroundItemLayer.cs` renders item stacks and stateful items placed on visible grid tiles. It uses item sprite assets when available and falls back to simple colored markers. It reads item placement data; it does not own item rules.
- `WorldView/NpcLayer.cs` renders NPCs from domain actor state and uses the NPC catalog for display colors when available. It does not own NPC health, placement, AI, or behavior.
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

`Actions/GameActionPipeline.cs` is the central action pipeline. It validates and resolves current actions, advances elapsed world ticks for successful time-costing actions, and returns messages plus the tick cost for the UI to display. Movement, Wait, Pick Up, stack item inspect/drop/equip/unequip, stateful item pickup/drop/equip/unequip/inspect, firearm handling, targeted NPC shooting, and prototype vehicle refueling actions go through this pipeline. Movement checks map bounds, static world objects, and NPCs that block movement. Equip Item validates inventory, item definitions, and equipment slot compatibility, then fills an empty slot without advancing time. Firearm handling and targeted shooting delegate to the firearm domain action service. Refuel Vehicle is available only when the active local site has a fuel pump cardinally adjacent to the player and the shared vehicle fuel state is below capacity.

The current tick costs are intentionally simple: Move costs 100 ticks, Wait costs 100 ticks, successful Pick Up costs 50 ticks, loading ammunition costs 10 ticks per round loaded, inserting or removing a detachable feed device costs 25 ticks, reloading an inserted detachable feed device combines remove + per-round load + insert costs, successful shooting costs 100 ticks, and successful vehicle refueling costs 100 ticks. Failed actions currently cost 0 ticks. Terrain-based movement modifiers, survival decay, actor scheduling, calendars, and day/night behavior are not implemented yet.

`Actions/PrototypeGameState.cs` is the current root shell/session state. It coordinates `WorldTime`, `PlayerState`, and `WorldState` rather than directly owning every game detail.

`Actions/WorldTime.cs` owns elapsed world ticks and exposes the small `Advance(int ticks)` operation used by successful time-costing player actions.

`Overworld/OverworldTravelState.cs` owns the current overworld party position, optional click destination, selected travel method, and shared prototype vehicle fuel state. Its movement is continuous map-unit movement rather than tile/grid movement. Advancing overworld travel consumes time through the same `WorldTime` used by local gameplay. Walking and pushbike travel do not consume fuel. Vehicle travel consumes fuel, stops when fuel reaches zero, and returns a clear message so the UI can prompt the player to switch to walking or pushbike.

`Overworld/PrototypeTravelMethods.cs` defines the current prototype travel methods: walking, pushbike, and vehicle. These are deliberately simple speed/fuel definitions with a current prototype vehicle fuel capacity of 15.0, not vehicle upgrades, repairs, storage, roads, or pathfinding.

`Overworld/PrototypeOverworldSites.cs` defines a few fixed prototype points of interest, including Route 18 Gas Station. Entering the gas station point of interest switches to a dedicated gas station local site. Other current points of interest switch to the default prototype local gameplay site. Procedural generation, settlement behavior, and save/load are not implemented yet.

`Actors/PlayerState.cs` owns player-specific state: grid position, vitals, and inventory.

`World/WorldState.cs` owns state outside the player for one active local site, currently the map, ground item placement, static world object placement, and NPC roster.

`World/PrototypeLocalSite.cs` and `World/PrototypeLocalSites.cs` describe the current fixed local sites. The default prototype site remains the small existing test map. The Route 18 Gas Station site is a hand-authored 40x28 map with asphalt forecourt, concrete pump island and parking areas, tiled store interior, staff/restroom areas, and gas station fixtures. Local site maps are fixed data for now, not procedural generation.

`World/MapState.cs` owns the current map bounds and tile surface map.

`World/GridBounds.cs` contains `GridBounds`, `GridPosition`, and `GridOffset`, which provide testable grid boundary behavior without instantiating Godot nodes.

`World/TileSurfaceDefinition.cs`, `World/TileSurfaceCatalog.cs`, `World/TileSurfaceMap.cs`, and `Content/TileSurfaceDefinitionLoader.cs` provide prototype terrain/surface definitions. Surface data is loaded from `data/surfaces/*.json` into plain C# objects. Current prototype surfaces include grass, carpet, concrete, tile, ice, and asphalt. Surface definitions can optionally reference bitmap sprites through `spriteId` values under `data/sprites/surfaces/`; asphalt currently uses fallback map color only. Surface tags double as lightweight properties for now, so future rules can ask whether a surface has tags such as `slippery`, `wet`, or `hot` without adding a separate property system yet. Godot currently uses surface definitions only to render the map.

`WorldObjects/WorldObjectDefinition.cs`, `WorldObjects/WorldObjectCatalog.cs`, `WorldObjects/TileObjectMap.cs`, and `Content/WorldObjectDefinitionLoader.cs` provide prototype static world object definitions and placement state. Object definitions are loaded from `data/world_objects/*.json` and currently describe display text, tags, movement blocking, sight blocking, map color, and optional sprite id. Gas station fixtures such as fuel pumps, canopy posts, counters, shelves, restroom fixtures, bollards, and abandoned vehicles are static world objects. `glass_door` is intentionally non-blocking. Interactions such as opening, looting, moving, destroying, or using objects are not implemented yet, with the limited exception that adjacent `fuel_pump` objects can expose the prototype Refuel Vehicle action.

`Actors/PlayerVitals.cs` and `Actors/BoundedMeter.cs` provide the current minimal player vitals model. `PlayerVitals` tracks health, hunger, thirst, fatigue, sleep debt, pain, and body temperature as domain state. These values are only tracked and displayed for now; no metabolism, damage, healing, sleep, pain, exertion, or temperature simulation has been added yet.

`Actors/NpcDefinition.cs`, `Actors/NpcCatalog.cs`, `Actors/NpcBehaviorProfile.cs`, and `Content/NpcDefinitionLoader.cs` provide JSON-backed NPC content definitions loaded from `data/npcs/*.json`. Definitions describe stable content data: display name, description, species, tags, maximum health, movement blocking, map color, and a simple behavior profile. Behavior profiles are descriptive foundation data for now; they do not execute AI decisions yet.

`Actors/NpcState.cs`, `Actors/NpcRoster.cs`, `Actors/NpcId.cs`, and `Actors/NpcDefinitionId.cs` provide the current runtime NPC model. `NpcId` identifies a specific spawned NPC instance, while `NpcDefinitionId` points back to the reusable content definition that created it. Runtime NPCs own per-instance position, health, and blocking state. NPCs can take direct damage and clamp health at 0. NPC AI, faction logic, death/removal behavior, melee actions, actor scheduling, and pathfinding are not implemented yet.

`Inventory/PlayerInventory.cs`, `Inventory/InventoryItemStack.cs`, and `Items/ItemId.cs` provide the current stack inventory model. This remains the right place for simple identical quantities such as ammunition rounds, crafting materials, and basic consumable stacks. Stack-backed items can be inspected and dropped through the action pipeline without becoming individual stateful item instances.

`Equipment/EquipmentLoadout.cs`, `Equipment/EquipmentSlotCatalog.cs`, `Equipment/EquipmentSlotDefinition.cs`, and `Equipment/EquipmentValidator.cs` provide the current equipment slot model. `PlayerState` owns an `EquipmentLoadout` with default slots for main hand, off hand, head, body, legs, feet, and back. Equipment stores `ItemId` references plus `ItemTypePath` classifications for validation. The current Equip Item action transfers one item from inventory into an empty compatible slot and costs 0 ticks. Legacy stack-backed equipment can now be unequipped back into inventory through the action pipeline. Equipment replacement, item effects, and slot-specific simulation rules are not implemented yet.

`Firearms/WeaponDefinition.cs`, `Firearms/AmmunitionDefinition.cs`, `Firearms/FeedDeviceDefinition.cs`, and `Firearms/FirearmCatalog.cs` describe firearm content loaded from `data/firearms/*.json`. Weapons declare accepted ammunition sizes, feed type, built-in capacity when relevant, weapon family, compatible detachable feed devices, and prototype effective/maximum range in tiles. Ammunition tracks size, variant, and direct damage. Feed devices track capacity, ammunition size, kind, and compatible weapon families.

`Items/StatefulItemStore.cs`, `Items/StatefulItem.cs`, `Items/StatefulItemLocation.cs`, and related ids/state classes provide the current stateful item model. A stateful item is a specific thing with a stable runtime id, item definition id, quantity, location, condition, optional firearm/feed state, and optional contained item ids. Supported locations are player inventory, equipment, ground, inserted in another item, and contained inside another item. Ground locations include a local site id so dropped stateful items remain scoped to the site where they were dropped. This model is used when identity matters, for example a loaded magazine, inserted feed device, equipped weapon, or backpack with contents.

The project intentionally has both stack inventory and stateful items right now. Simple identical content can stay in `PlayerInventory` as counts. Specific items that must preserve state across pickup, drop, equipment, insertion, removal, inspection, and containment should use `StatefulItemStore`.

`Firearms/PlayerFirearmState.cs`, `Firearms/WeaponRuntimeState.cs`, and `Firearms/FeedDeviceState.cs` own legacy stack-backed loaded firearm state. `FeedDeviceState` is also reused by stateful feed-device items and built-in stateful weapon feeds. Feed devices currently allow only one ammunition item/variant at a time; unload before switching variants. Runtime firearm state should be created by successful state-changing actions, not by read-only UI refresh or action availability queries.

`Firearms/FirearmActionService.cs` validates and resolves loading ammunition into feed devices, unloading feed devices, inserting/removing compatible feed devices, reloading inserted detachable feed devices, loading built-in weapon feeds, test firing one round, and shooting an NPC with an equipped firearm. It supports both legacy stack-backed item ids and newer stateful firearm/feed items. Reloading a detachable-feed weapon is a single prototype action that removes the inserted feed device, loads as many compatible held rounds as fit, and reinserts it while preserving the specific magazine state. Shooting selects an equipped firearm from main hand first, then off hand, checks target range against the weapon maximum range, consumes one loaded round, and applies the ammunition damage to the NPC. Failed firearm actions should leave inventory, loaded counts, inserted feeds, stateful item locations, NPC health, and tracked runtime firearm state unchanged. Test-fire actions currently cost 0 ticks; successful targeted shooting costs 100 ticks through the action pipeline. Shooting does not implement accuracy, misses, cover, line of sight, sound, recoil, durability, jamming, armor, hit locations, or ballistics.

`Items/ItemDefinition.cs`, `Items/ItemCatalog.cs`, `Content/ItemDefinitionLoader.cs`, and `Items/ItemTypePath.cs` provide prototype item definitions. Item definitions are loaded from `data/items/*.json` into immutable C# objects. `ItemTypePath` is derived from category plus tags and supports nested subtype checks by prefix, for example `Weapon -> gun -> rifle -> ak47` is considered a `Weapon`, a `Gun`, and a `Rifle`.

`Items/PrototypeItems.cs` currently keeps stable ids and common type paths used by tests and the shell. The item definitions themselves live in JSON data files.

`Items/GroundItems.cs` contains `TileItemMap`, `GroundItemStack`, and `PlacedItemStack`, which provide minimal domain storage for item stacks placed on grid positions. They do not implement pickup, drop, visibility, physics, ownership, or item interactions.

## Tests

Domain tests live under `tests/SurvivalGame.Domain.Tests/` and should mirror domain concepts, such as `World/`, `Items/`, and `Inventory/`. Prefer adding tests for plain C# simulation/domain code before testing Godot presentation scripts directly.

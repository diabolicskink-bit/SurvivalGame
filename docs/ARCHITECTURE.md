# Architecture

## Repository Structure

- `docs/` contains project documentation.
- `data/` contains runtime game content loaded through Godot `res://data/...` paths.
- `data/sprites/items/` contains item sprite bitmap assets referenced by item definition `spriteId` values.
- `data/sprites/npcs/` contains NPC sprite bitmap assets referenced by NPC definition `spriteId` values.
- `data/sprites/surfaces/` contains terrain/surface sprite bitmap assets referenced by surface definition `spriteId` values.
- `data/sprites/world_objects/` contains world object sprite bitmap assets referenced by world object definition `spriteId` values.
- `data/firearms/` contains JSON firearm, ammunition, and feed-device definitions used by the firearm domain catalog.
- `data/local_maps/` contains authored local site map definitions loaded into domain local map state.
- `data/npcs/` contains JSON NPC definition files.
- `data/surfaces/` contains JSON terrain/surface definition files.
- `data/world_objects/` contains JSON definitions for static map objects such as walls, trees, and furniture.
- `src/Godot/` contains Godot-facing scenes and scripts, grouped by feature.
- `src/Godot/MainMenu/` contains the main menu scene and menu script.
- `src/Godot/WorldMap/` contains the first world map travel screen and map drawing control.
- `src/Godot/Game/` contains the current playable prototype shell.
- `src/Godot/Game/GameSessionShell.cs` coordinates the current prototype run scene and switches between world map travel and local site gameplay.
- `src/Godot/Game/PrototypeSessionFactory.cs` creates prototype campaign/run state, local gameplay sessions, and content catalogs used by the session shell and standalone local scene.
- `src/Godot/Game/LocalMapView/` contains visible local map, grid, actor marker, item marker, and input-view scripts.
- `src/Godot/Game/UI/` contains gameplay overlay controls.
- `src/Godot/Game/Prototype/` is reserved for temporary Godot-side prototype helpers.
- `src/SurvivalGame.Domain/` contains plain C# domain code grouped by simulation/content concept.
- `src/SurvivalGame.Domain/Campaign/` contains the domain-level prototype campaign/run state root and local-site runtime wrappers.
- `src/SurvivalGame.Domain/Actions/` contains action requests, action handler dispatch, world time state, and the current local gameplay state.
- `src/SurvivalGame.Domain/WorldMap/` contains plain C# world map travel state, positions, travel methods, fixed prototype points of interest, and movement/fuel rules.
- `src/SurvivalGame.Domain/Actors/` contains player state, NPC definitions, NPC runtime state, behavior profiles, and actor collections.
- `src/SurvivalGame.Domain/LocalMaps/` contains local map/grid primitives.
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
- `src/Godot/Game/GameSessionShell.tscn` is the current run scene. It receives a prototype campaign session from `PrototypeSessionFactory`, starts on the world map, and switches between world map and local modes while the domain `CampaignState` owns the persistent run state.
- `src/Godot/WorldMap/WorldMapScreen.tscn` is the first world map travel screen. It displays the world map, travel party marker, travel method, fuel when relevant, time, fixed points of interest, and Enter Site action.
- `src/Godot/Game/GameShell.tscn` is the local prototype gameplay scene. It contains the board, surface grid, item layer, sorted map entity layer, player controller, and UI layer. It can still run standalone, but in the normal New Run flow it is entered from the world map through `GameSessionShell`. It renders local maps through a fixed `27 x 18` tile viewport while the full local map remains in domain state.

## Script Responsibilities

- `MainMenu.cs` handles menu button actions and launches `GameSessionShell` for New Run.
- `GameSessionShell.cs` is the Godot presentation coordinator for world map/local mode switching. It creates the prototype campaign session, shows the world map or local gameplay scene, routes current prototype points of interest to a registered local site id, and asks `CampaignState` to enter or leave local mode. It no longer owns the model of the run.
- `PrototypeSessionFactory.cs` creates prototype content catalogs, the initial domain `CampaignState`, registered local site states, starting inventory/stateful items, and the action pipeline. It can package domain state for Godot scenes, but it is not the runtime owner of the campaign after creation.
- `WorldMapScreen.cs` builds the world map travel HUD, handles travel method selection, advances travel each frame, and exposes Enter Site when the party is near a point of interest.
- `WorldMapView.cs` draws the simple world map, fixed points of interest, travel party marker, destination indicator, and destination line. It converts map clicks into world map destination requests.
- `GameShell.cs` coordinates the gameplay shell scene, sends action requests to the domain action pipeline, converts between map coordinates and the current local viewport, positions the player marker, and refreshes the UI overlay from shell state.
- `LocalMapView/GridView.cs` draws the visible `GridViewport` window from domain surface state, including dark padding where a smaller map does not fill the fixed viewport. It uses surface sprite assets when available and falls back to surface definition display colors. It does not own terrain rules.
- `LocalMapView/GroundItemLayer.cs` renders item stacks and stateful items that fall inside the current local viewport. It uses item sprite assets when available and falls back to simple colored markers. It reads item placement data; it does not own item rules.
- `LocalMapView/MapEntityLayer.cs` renders world objects, NPCs, and the player marker in one Y-sorted pass. World object sprites render once from their placement anchor and facing, while object footprints define the occupied tiles used by collision and hover. NPC sprites may use optional `spriteRender` metadata to draw beyond their owning tile without changing collision, hover, targeting, or placement state.
- `LocalMapView/PlayerController.cs` translates movement keys into move action requests. It does not own movement rules, position state, or player rendering.
- `UI/ActionPanel.cs` displays global and selected-target clickable actions such as Wait, Pick Up, and Shoot inside the player info/general panel. It should not display every item-specific action at once.
- `UI/PlayerStatusPanel.cs` displays tracked player vitals from domain state. It does not calculate survival effects.
- `UI/EquipmentPanel.cs` displays every player equipment slot separately from inventory and emits selection events for occupied slots. It does not own equipment state or equip rules.
- `UI/InventoryPanel.cs` and `UI/InventoryGridView.cs` display the player's carried items in two modes: Inventory shows the physical prototype grid for grid-using stack items and freely carried stateful items, while Ammo lists loose ammunition stacks by type and quantity. They do not own inventory state or item rules.
- `UI/SelectedItemPanel.cs` is used inside the item click popup to display details and contextual actions for the selected inventory or equipment item. It filters actions from existing domain action requests; it does not invent item rules.
- `UI/FirearmPanel.cs` is retained as a prototype firearm status control, but the current gameplay overlay primarily surfaces firearm/feed state through selected item details.
- `UI/ItemTooltip.cs` displays read-only hover details for the currently hovered tile, including its surface definition and any NPC, item stacks, or stateful items there.
- `UI/MessageLog.cs` owns the recent visible message display.

## Domain Code

Future simulation logic should be kept separate from Godot presentation where practical. Godot scenes and nodes should present state and collect input, while survival and roguelike rules can grow into plain C# services or model classes later.

The current shell still keeps Godot scene and catalog wiring in Godot-facing scripts so the project can run. Persistent run state belongs to plain C# domain objects, with Godot nodes acting as views/controllers.

`Campaign/CampaignState.cs` is the current domain-level root of a prototype run. It owns the shared `WorldTime`, `PlayerState`, `StatefulItemStore`, `WorldMapTravelState`, and `VehicleFuelState`; tracks whether the run is currently in world map or local-site mode; tracks the active local site id; and owns the registered local site states. Entering a site selects an existing registered `LocalSiteState`, clears the world map destination, and restores that site's last local player position. Returning to the world map records the active site's current player position without discarding its map, ground items, world objects, or NPC roster.

`Campaign/LocalSiteState.cs` wraps one local site's `PrototypeGameState` with site display metadata, entry position, and last local player position. It keeps local site state reusable across world map/local transitions without making Godot scene scripts responsible for preserving that state.

`Actions/GameActionPipeline.cs` is now a thin dispatcher around action handlers. It builds a `GameActionContext`, resolves the request through `ActionHandlerRegistry`, and runs `NpcCombatService` after successful time-costing actions. Movement, inventory, equipment, inspection, firearm delegation, and prototype refueling live in separate handler classes under `Actions/`. `ItemDescriber` owns action-facing item names and inspection text. `PlayerInventory` owns synchronization between freely carried stateful items and the physical inventory grid. Movement checks map bounds, static world objects, and NPCs that block movement. Pick Up and unequip check the player inventory grid before moving grid-using stack items into inventory; loose ammunition is exempt from grid placement. Stateful item pickup and unequip still require physical grid space. Equip Item validates inventory, item definitions, and equipment slot compatibility, then fills an empty slot without advancing time. Firearm handling and targeted shooting delegate to the firearm domain action service through `FirearmHandler`. Refuel Vehicle is available only when the active local site has a fuel pump cardinally adjacent to the player and the shared vehicle fuel state is below capacity. After successful time-costing local actions, `NpcCombatService` resolves simple fixed automated hazard NPC behavior from NPC behavior tags.

The current tick costs are intentionally simple: Move costs 100 ticks, Wait costs 100 ticks, successful Pick Up costs 50 ticks, loading ammunition costs 10 ticks per round loaded, inserting or removing a detachable feed device costs 25 ticks, reloading an inserted detachable feed device combines remove + per-round load + insert costs, successful shooting costs 100 ticks, and successful vehicle refueling costs 100 ticks. Failed actions currently cost 0 ticks. The Route 18 Gas Station automated turret NPC checks every crossed 75-tick interval after successful time-costing actions, fires within 5 tiles while not disabled, and deals 10 direct health damage per shot. Terrain-based movement modifiers, survival decay, actor scheduling, calendars, and day/night behavior are not implemented yet.

`Actions/PrototypeGameState.cs` is the current active local-site gameplay state. It coordinates a local site's `LocalMapState` with the shared `WorldTime`, `PlayerState`, and `StatefulItemStore` supplied by `CampaignState`.

`Actions/WorldTime.cs` owns elapsed world ticks and exposes the small `Advance(int ticks)` operation used by successful time-costing player actions.

`WorldMap/WorldMapTravelState.cs` owns the current world map party position, optional click destination, selected travel method, and shared prototype vehicle fuel state exposed through `CampaignState`. Its movement is continuous map-unit movement rather than tile/grid movement. Advancing world map travel consumes time through the same `WorldTime` used by local gameplay. Walking and pushbike travel do not consume fuel. Vehicle travel consumes fuel, stops when fuel reaches zero, and returns a clear message so the UI can prompt the player to switch to walking or pushbike.

`WorldMap/WorldMapViewport.cs` provides the current world map camera/window transform. The full prototype world map is larger than the visible screen view, while the visible window keeps the earlier map scale and follows the travel party. The viewport clamps at full-map edges and converts between full map coordinates and visible viewport coordinates for drawing and click destination selection.

`WorldMap/PrototypeTravelMethods.cs` defines the current prototype travel methods: walking, pushbike, and vehicle. These are deliberately simple speed/fuel definitions with a current prototype vehicle fuel capacity of 15.0, not vehicle upgrades, repairs, storage, roads, or pathfinding.

`WorldMap/PrototypeWorldMapSites.cs` defines fixed prototype points of interest on the larger authored prototype world map, including Route 18 Gas Station near the starting region. Entering the gas station point of interest switches to a dedicated gas station local site. Other current points of interest switch to the default prototype local gameplay site. Procedural generation, settlement behavior, and save/load are not implemented yet.

`Actors/PlayerState.cs` owns player-specific state: grid position, vitals, and inventory.

`LocalMaps/LocalMapState.cs` owns state outside the player for one local site, currently the map, ground item placement, static world object placement, and NPC roster.

`LocalMaps/PrototypeLocalSite.cs` describes loaded local site data, while `LocalMaps/PrototypeLocalSites.cs` keeps the stable prototype local site ids and expected bounds. The default prototype site and Route 18 Gas Station are authored JSON maps under `data/local_maps/`. `PrototypeSessionFactory` loads those definitions and registers them as campaign local sites for the current prototype run. Local site maps are fixed authored data for now, not procedural generation.

`LocalMaps/LocalMap.cs` owns the current map bounds and tile surface map.

`LocalMaps/LocalMapBuilder.cs` is the shared domain builder for map sources. It validates bounds and known surface, world-object, item, and NPC ids while assembling `PrototypeLocalSite` data. Current authored JSON maps use it now; recipe and chunked procedural map sources have explicit placeholder types that throw until those systems are implemented.

`LocalMaps/GridBounds.cs` contains `GridBounds`, `GridPosition`, and `GridOffset`, which provide testable grid boundary behavior without instantiating Godot nodes.

`LocalMaps/GridViewport.cs` provides the current local-map viewport transform. Local gameplay renders a fixed `27 x 18` tile viewport focused on the player when possible, clamps to larger map edges near the boundary, and centers smaller maps inside the fixed viewport with dark padding. It converts map coordinates to viewport coordinates for rendering and viewport coordinates back to map coordinates for hover/click input; simulation state remains in full map coordinates.

`LocalMaps/TileSurfaceDefinition.cs`, `LocalMaps/TileSurfaceCatalog.cs`, `LocalMaps/TileSurfaceMap.cs`, and `Content/TileSurfaceDefinitionLoader.cs` provide prototype terrain/surface definitions. Surface data is loaded from `data/surfaces/*.json` into plain C# objects. Current prototype surfaces include grass, carpet, concrete, tile, ice, and asphalt. Surface definitions can optionally reference bitmap sprites through `spriteId` values under `data/sprites/surfaces/`; asphalt currently uses fallback map color only. Surface tags double as lightweight properties for now, so future rules can ask whether a surface has tags such as `slippery`, `wet`, or `hot` without adding a separate property system yet. Godot currently uses surface definitions only to render the map.

`Content/LocalSiteDefinitionLoader.cs` loads authored local map definitions from `data/local_maps/*.json`. It supports surface and object ASCII layers, sparse object placements with optional facing, sparse item placements, and NPC placements, and rejects unsupported source kinds such as recipe or chunked procedural maps with clear `NotSupportedException` messages.

`WorldObjects/WorldObjectDefinition.cs`, `WorldObjects/WorldObjectCatalog.cs`, `WorldObjects/TileObjectMap.cs`, and `Content/WorldObjectDefinitionLoader.cs` provide prototype static world object definitions and placement state. Object definitions are loaded from `data/world_objects/*.json` and currently describe display text, tags, movement blocking, sight blocking, map color, optional sprite id, optional visual-only sprite render metadata, and a rectangular simulation footprint defaulting to `1 x 1`. Object placements can face north, east, south, or west; east/west placements rotate rectangular footprints by swapping width and height. `TileObjectMap` stores one placement per object anchor and indexes every occupied tile for movement and hover lookups. Gas station fixtures such as fuel pumps, canopy posts, counters, shelves, restroom fixtures, bollards, and abandoned vehicles are static world objects. `glass_door` is intentionally non-blocking. Interactions such as opening, looting, moving, destroying, or using objects are not implemented yet, with the limited exception that adjacent `fuel_pump` objects can expose the prototype Refuel Vehicle action.

`Actors/PlayerVitals.cs` and `Actors/BoundedMeter.cs` provide the current minimal player vitals model. `PlayerVitals` tracks health, hunger, thirst, fatigue, sleep debt, pain, and body temperature as domain state. Health can take direct turret damage and clamp at 0, but death, healing, metabolism, sleep, pain, exertion, and temperature simulation have not been added yet.

`Actors/NpcDefinition.cs`, `Actors/NpcCatalog.cs`, `Actors/NpcBehaviorProfile.cs`, and `Content/NpcDefinitionLoader.cs` provide JSON-backed NPC content definitions loaded from `data/npcs/*.json`. Definitions describe stable content data: display name, description, species, tags, maximum health, movement blocking, map color, optional sprite id, optional visual-only sprite render footprint metadata, and a simple behavior profile. Behavior profiles are descriptive foundation data for now; they do not execute AI decisions yet.

`Actors/NpcState.cs`, `Actors/NpcRoster.cs`, `Actors/NpcId.cs`, and `Actors/NpcDefinitionId.cs` provide the current runtime NPC model. `NpcId` identifies a specific spawned NPC instance, while `NpcDefinitionId` points back to the reusable content definition that created it. Runtime NPCs own per-instance position, health, and blocking state. NPCs can take direct damage and clamp health at 0. NPC AI, faction logic, death/removal behavior, melee actions, actor scheduling, and pathfinding are not implemented yet.

`Inventory/ItemContainer.cs`, `Inventory/ItemContainerStore.cs`, `Inventory/ContainerItemRef.cs`, `Inventory/InventoryItemSize.cs`, and `Inventory/InventoryGridPosition.cs` provide the reusable rectangular grid container foundation. Containers are plain domain objects with a width, height, item references, item sizes, and non-overlap/bounds validation. This is intended to support player inventory now and world-object or equipment containers later.

`Inventory/PlayerInventory.cs`, `Inventory/InventoryItemStack.cs`, `Inventory/InventoryGridRules.cs`, and `Items/ItemId.cs` provide the current stack inventory model. The player inventory is backed by a prototype `20 x 10` `ItemContainer`, but stack-backed items can either use the physical grid or be grid-exempt. `InventoryGridRules` currently treats item definitions with category `Ammunition` as loose ammo that does not occupy grid cells; all other current categories use the grid. Stack-backed items can be inspected and dropped through the action pipeline without becoming individual stateful item instances.

`Equipment/EquipmentLoadout.cs`, `Equipment/EquipmentSlotCatalog.cs`, `Equipment/EquipmentSlotDefinition.cs`, and `Equipment/EquipmentValidator.cs` provide the current equipment slot model. `PlayerState` owns an `EquipmentLoadout` with default slots for main hand, off hand, head, body, legs, feet, and back. Equipment stores `ItemId` references plus `ItemTypePath` classifications for validation. The current Equip Item action transfers one item from inventory into an empty compatible slot and costs 0 ticks. Legacy stack-backed equipment can now be unequipped back into inventory through the action pipeline. Equipment replacement, item effects, and slot-specific simulation rules are not implemented yet.

`Firearms/WeaponDefinition.cs`, `Firearms/AmmunitionDefinition.cs`, `Firearms/FeedDeviceDefinition.cs`, and `Firearms/FirearmCatalog.cs` describe firearm content loaded from `data/firearms/*.json`. Weapons declare accepted ammunition sizes, feed type, built-in capacity when relevant, weapon family, compatible detachable feed devices, and prototype effective/maximum range in tiles. Ammunition tracks size, variant, and direct damage. Feed devices track capacity, ammunition size, kind, and compatible weapon families.

`Items/StatefulItemStore.cs`, `Items/StatefulItem.cs`, `Items/StatefulItemLocation.cs`, and related ids/state classes provide the current stateful item model. A stateful item is a specific thing with a stable runtime id, item definition id, quantity, location, condition, optional firearm/feed state, and optional contained item ids. Supported locations are player inventory, equipment, ground, inserted in another item, and contained inside another item. Ground locations include a local site id so dropped stateful items remain scoped to the site where they were dropped. This model is used when identity matters, for example a loaded magazine, inserted feed device, equipped weapon, or backpack with contents.

The project intentionally has both stack inventory and stateful items right now. Simple identical content can stay in `PlayerInventory` as counts. Specific items that must preserve state across pickup, drop, equipment, insertion, removal, inspection, and containment should use `StatefulItemStore`.

`Firearms/PlayerFirearmState.cs`, `Firearms/WeaponRuntimeState.cs`, and `Firearms/FeedDeviceState.cs` own legacy stack-backed loaded firearm state. `FeedDeviceState` is also reused by stateful feed-device items and built-in stateful weapon feeds. Feed devices currently allow only one ammunition item/variant at a time; unload before switching variants. Runtime firearm state should be created by successful state-changing actions, not by read-only UI refresh or action availability queries.

`Firearms/FirearmActionService.cs` is the public firearm action facade used by `FirearmHandler`. It delegates request availability to `FirearmActionProvider`, precondition checks and action plans to `FirearmValidator`, mutations to `FirearmStateOperations`, and stack/stateful item access to the `IFirearmWeaponRef`/`IFirearmFeedRef` adapters in `FirearmRefs.cs`. It supports both legacy stack-backed item ids and newer stateful firearm/feed items. Reloading a detachable-feed weapon is a single prototype action that removes the inserted feed device, loads as many compatible held rounds as fit, and reinserts it while preserving the specific magazine state. Shooting selects an equipped firearm from main hand first, then off hand, checks target range against the weapon maximum range, consumes one loaded round, and applies the ammunition damage to the NPC. Failed firearm actions should leave inventory, loaded counts, inserted feeds, stateful item locations, NPC health, and tracked runtime firearm state unchanged. Test-fire actions currently cost 0 ticks; successful targeted shooting costs 100 ticks through the action pipeline. Shooting does not implement accuracy, misses, cover, line of sight, sound, recoil, durability, jamming, armor, hit locations, or ballistics.

`Items/ItemDefinition.cs`, `Items/ItemCatalog.cs`, `Content/ItemDefinitionLoader.cs`, and `Items/ItemTypePath.cs` provide prototype item definitions. Item definitions are loaded from `data/items/*.json` into immutable C# objects. Item definitions include an `inventorySize`, defaulting to `1 x 1` when omitted, used by grid containers for grid-using items. Loose ammunition definitions keep inventory size data for completeness, but the current grid rule ignores it. `ItemTypePath` is derived from category plus tags and supports nested subtype checks by prefix, for example `Weapon -> gun -> rifle -> ak47` is considered a `Weapon`, a `Gun`, and a `Rifle`.

`Items/PrototypeItems.cs` currently keeps stable ids and common type paths used by tests and the shell. The item definitions themselves live in JSON data files.

`Items/GroundItems.cs` contains `TileItemMap`, `GroundItemStack`, and `PlacedItemStack`, which provide minimal domain storage for item stacks placed on grid positions. They do not implement pickup, drop, visibility, physics, ownership, or item interactions.

## Tests

Domain tests live under `tests/SurvivalGame.Domain.Tests/` and should mirror domain concepts, such as `Campaign/`, `WorldMap/`, `LocalMaps/`, `Items/`, and `Inventory/`. Prefer adding tests for plain C# simulation/domain code before testing Godot presentation scripts directly.

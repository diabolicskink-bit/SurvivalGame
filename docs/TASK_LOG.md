# Task Log

## 2026-04-26 - Real Colorado Road Layer

- Replaced the hand-authored world-map road polylines with a committed generated road file at `data/world_map/colorado_roads.generated.json`.
- Added `tools/world_map/generate_colorado_roads.py` to fetch curated Interstates, US Highways, and State Highways from Colorado GIS basemap layers and simplify them deterministically.
- Extended world-map road domain data from single point lists to multi-segment geometry with road kind, priority, lane count, surface width, and travel influence radius.
- Updated the world-map loader so `colorado.json` can reference external road files while keeping POIs, terrain, and projection data in the main map definition.
- Reworked world-map road rendering to draw casing/fill roads with class/lane-based widths and world-coordinate segment clipping so roads do not pivot while panning.
- Kept current click-to-travel behavior and first-pass road/terrain travel-cost sampling; road snapping and pathfinding remain out of scope.
- Added domain tests for generated road loading, required major routes, multi-segment distance checks, road metadata, bounds, and travel-cost behavior.
- Updated Colorado source notes, architecture docs, and current-scope wording.

## 2026-04-26 - Colorado World Map Scale Tuning

- Reduced the current scaled Colorado world map by one third from 15600x11400 to 10400x7600 to balance marker spacing with navigation scale.
- Reduced road travel influence radii by the same proportion so road-adjacent travel-cost behavior stays consistent.
- Updated tests and current-scope documentation for the tuned map scale.

## 2026-04-26 - Colorado World Map Scale Increase Follow-Up

- Tripled the scaled Colorado world map dimensions again from 5200x3800 to 15600x11400 for much wider spacing between map markers.
- Tripled road travel influence radii from the previous scale so road-adjacent travel-cost behavior remains comparable.
- Updated current-scope documentation for the larger full-map dimensions.

## 2026-04-26 - Colorado World Map Scale Increase

- Doubled the scaled Colorado world map dimensions from 2600x1900 to 5200x3800 while keeping the same real coordinate projection and curated POI/road placement.
- Doubled road travel influence radii so first-pass road/terrain travel-cost behavior remains comparable at the larger scale.
- Updated current-scope documentation for the larger full-map dimensions.

## 2026-04-26 - Colorado World Map V1

- Replaced the hard-coded prototype overworld with a JSON-backed scaled Colorado world map definition under `data/world_map/`.
- Added domain world-map definition models and a loader for projection bounds, city/town markers, landmark POIs, road polylines, terrain regions, start position, label priority, category, and optional local-site routing metadata.
- Added curated Colorado content with major city/town markers, recognizable landmark POIs, simplified major road corridors, broad terrain regions, and Front Range test POIs for the existing gas station and farmstead local maps.
- Updated world map rendering to draw data-backed terrain regions, roads, city markers, landmark markers, local-site markers, and label-priority-filtered text.
- Added first-pass hybrid travel costs so road proximity and terrain modify world-map speed and vehicle fuel use without adding pathfinding or road snapping.
- Added source/selection notes in `docs/COLORADO_WORLD_MAP_SOURCES.md` and updated architecture/current-scope documentation.
- Added domain tests for Colorado map loading, required anchors, test local-site placement, and travel-cost behavior.

## 2026-04-26 - Background Bible Direction Expansion

- Expanded `docs/BACKGROUND.md` with Colorado-specific logistics anchors, real-US social texture, and a clearer private consistency model for The Failover.
- Added earned mobile-base progression guidance from foot travel through bike/cart, fragile vehicles, reliable vehicles, and larger practical hub platforms.
- Clarified grounded tactical combat pressure, Colorado-specific site families, faction seeds, and content decision priorities.
- Kept the additions as soft setting/design guidance only, with no code, data, gameplay, or current-scope changes.

## 2026-04-26 - Direction Documentation Restructure

- Reworked `docs/BACKGROUND.md` into a concise setting reference bible for the Failover premise, Colorado-first US setting, tone, threats, site guidance, factions, and content decision rules.
- Replaced `docs/GAME_BRIEF.md` with `docs/DESIGN_GOALS.md` as a long-term system direction document covering the core loop, travel/mobile base, local maps, combat/ballistics, inventory, NPCs/factions, survival/time, world generation, and UI readability.
- Added a compact Creative North Star section to `AGENTS.md` so future AI sessions can steer toward the intended setting and systems without treating long-term goals as current scope.
- Clarified that `BACKGROUND.md` and `DESIGN_GOALS.md` guide decisions but do not authorize scope expansion; `docs/CURRENT_SCOPE.md` remains the current implementation source of truth.
- Kept code, data, gameplay behavior, and public APIs unchanged.

## 2026-04-26 - Initial Weapon Mod System

- Added JSON-backed weapon mod definitions for red dot sight, hunting scope, and match barrel.
- Added stateful weapon mod installation/removal actions with slot and weapon-family compatibility.
- Stored installed weapon mods on stateful weapon state while keeping each mod as a specific stateful item.
- Applied installed weapon mod bonuses to shooting maximum range and damage.
- Displayed weapon mod details, installed mods, modified ranges, and damage bonuses in item details.
- Added prototype starting mod items for manual testing.
- Added tests for mod loading, compatibility, duplicate-slot rejection, install/remove state, modified range, modified damage, and inspect details.
- Kept stack-backed weapon mods, crafting/tools, accuracy/recoil/suppressor effects, durability, and save/load persistence out of scope.

## 2026-04-26 - Lootable World Object Containers

- Added searchable world-object container metadata with definition-level profiles and placement-level fixed loot config.
- Added stable world-object instance ids for placements, including automatic ids for object-layer placements and explicit ids for authored sparse placements.
- Added lazy per-site container runtime state so containers are only realized after search, preserving searched/remaining contents across local-site re-entry.
- Added Search Container and Take Container Item Stack actions through the domain action pipeline, with inventory-grid validation and time costs.
- Exposed nearby Search/Take actions in the existing Godot action panel.
- Seeded the default prototype site's fridge and storage crate with fixed loot while leaving empty-profile searchable containers ready for future random loot tables.
- Added tests for lazy realization, empty containers, fixed loot, taking loot, inventory-full safety, authored container config loading, and placement instance ids.
- Kept random loot rolling, stateful item container loot, open/close states, nested containers, and full container transfer UI out of scope.

## 2026-04-26 - Sprite Creation Guidance

- Added project-wide AI guidance for creating usable transparent sprites.
- Covered naming, data-footprint alignment, top-down readability, trimming transparent padding, `.import` upkeep, and verification expectations.

## 2026-04-26 - Single Bed Footprint And Sprite Trim

- Renamed the generic bed world object to `single_bed` with player-facing Single bed naming.
- Updated local map references to use the specific single-bed object id.
- Changed the single bed footprint to one tile wide by two tiles long, matching the north-facing bed sprite.
- Renamed and trimmed the single bed sprite asset to remove transparent padding around the visible single bed.
- Updated tests and documentation references from generic bed wording where they referred to the object/sprite.

## 2026-04-26 - Abandoned Farmhouse Local Site

- Added Abandoned Farmhouse as a dedicated `farmstead` local site for the existing World Map point of interest.
- Authored a 64x44 rural property map with south-west dirt track entry, farmhouse rooms, front verandah, rear utility yard, water tank area, shed/workshop, machinery yard, fenced paddock, and scrub/fence perimeter.
- Added rural surfaces for dirt, gravel, weathered wood, linoleum, and scrub.
- Expanded static world objects with farmhouse walls, passable doors and gates, domestic furniture, kitchen/bath/laundry fixtures, workshop clutter, water tank, fences, paddock props, and farm machinery wreckage.
- Added farmhouse-themed pickup item definitions and placed tools, salvage, pantry food, medical supplies, and clothing/equipment across the property.
- Updated local site registration and World Map routing so matching point-of-interest ids enter registered authored local sites, while unmatched points still fall back to the default prototype map.
- Added focused domain tests for farmhouse loading, surfaces, objects, movement collision/passable routes, item placement, and no-NPC spawning.

## 2026-04-26 - Trimmed Additional Sprite Padding

- Cropped transparent padding from the automated turret, store shelf, and fuel pump sprite assets.
- Kept the existing sprite ids and Godot import metadata intact so object/NPC definitions continue to reference the same asset paths.

## 2026-04-26 - World Object Footprint Bug Fixes

- Corrected the abandoned vehicle's data-defined footprint to a canonical 2x4 shape so the east-facing gas station placement blocks the visible 4x2 vehicle area instead of a larger surrounding rectangle.
- Changed the gameplay board to a clipped Control so partially visible world-object sprites do not draw outside the local viewport.
- Updated tests and local map view documentation for the corrected footprint and viewport clipping behavior.

## 2026-04-26 - Hover Item Info And Right-Click Actions

- Split inventory/equipment item interaction into transient hover info and right-click contextual actions.
- Hovering carried or equipped items now shows read-only item details only while the pointer remains over the item.
- Right-clicking carried or equipped items now opens an item action menu, which stays visible only while the pointer is over the item or the menu.
- Kept item actions routed through existing domain action requests and did not add new inventory mechanics.

## 2026-04-26 - Multi-Tile Static World Object Footprints

- Added rectangular world-object footprints and cardinal placement facing to the domain model.
- Updated `TileObjectMap` to store one anchor placement while indexing every occupied tile for movement, hover, and inspection lookups.
- Extended authored local map object placements with optional `facing` data and moved the Route 18 Gas Station abandoned vehicle to a multi-tile east-facing placement.
- Updated world-object rendering to draw each placement once and rotate object sprites from their facing.
- Added domain and loader tests for footprint defaults, rotation, overlap/out-of-bounds validation, facing parsing, and gas-station vehicle collision coverage.

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
- Split gameplay presentation into `LocalMapView/`, `UI/`, and `Prototype/` folders.
- Reorganized domain code by concept under `Actors/`, `LocalMaps/`, `Items/`, `Inventory/`, and `Content/`.
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
- Added JSON definitions for wall, tree, fridge, wooden door, window, table, chair, single bed, storage crate, and boulder.
- Added a Godot world object layer to render placed objects from domain state.
- Added movement blocking for world objects whose definitions block movement.
- Added world object details to tile hover tooltips.
- Added tests for object loading, placement, and movement blocking behavior.
- Kept object interactions, containers, destruction, opening doors, and looting out of scope.

## 2026-04-25 - World Object Sprites

- Generated and added sprite assets for fridge, storage crate, and single bed.
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

## 2026-04-25 - Test Dummy NPC

- Added minimal domain NPC state with id, name, grid position, and health.
- Added an NPC roster to world state so actors live with simulation state rather than Godot nodes.
- Seeded one inert Test Dummy NPC on the prototype map with 200/200 health.
- Added a Godot NPC layer, movement blocking against NPC-occupied tiles, and hover tooltip health display.
- Kept NPC AI, combat, damage resolution, dialogue, factions, and scheduling out of scope.

## 2026-04-25 - Firearm Range Definitions

- Added effective and maximum tile range fields to weapon definitions.
- Added prototype ranges for the 9mm pistol, AK-style rifle, .308 hunting rifle, 12 gauge shotgun, and .22 rifle.
- Updated selected item details to show weapon range.
- Kept range as definition/display data at this stage; combat, accuracy, damage, and ballistics remained out of scope until the follow-up shooting slice.

## 2026-04-25 - Targeted Shooting Test Dummy

- Added prototype ammunition damage values to firearm data.
- Added targeted NPC shooting through the domain action pipeline.
- Shooting now requires an equipped firearm, a loaded round, and a target inside the weapon's maximum tile range.
- Successful shots consume one round, apply ammunition damage to the NPC, advance time by 100 ticks, and refresh the NPC health bar.
- Updated Godot input so clicking an NPC tile selects that NPC, shows it as the current target, and reveals a Shoot action in the global action panel.
- Moved map click targeting into the earlier input path so root UI controls do not swallow board clicks.
- Put the selected target Shoot action first in the global action list so it remains visible even in tighter panel layouts.
- Capped and clamped the tile hover tooltip so it stays inside the viewport instead of expanding into a large blank panel.
- Kept accuracy, misses, cover, line of sight, armor, death/removal, hostile AI, and full combat out of scope.

## 2026-04-25 - Game State Ownership Refactor

- Reviewed current game state ownership against the intended Godot/domain boundary.
- Kept `PrototypeGameState` as the root prototype state rather than renaming it.
- Added `TurnState` for turn count ownership.
- Added `LocalMapState` and `LocalMap` so local map data is grouped outside the root state.
- Moved player grid position into `PlayerState`.
- Updated `GameActionPipeline` and `GameShell` to use `state.Turn`, `state.Player`, and `state.World`.
- Added focused domain tests for the state hierarchy and turn advancement.
- Preserved existing gameplay behavior and avoided new simulation systems.

## 2026-04-25 - Default Window Size

- Updated the default Godot viewport size to 1920x1080.
- Kept the existing responsive layout and resizable window settings.

## 2026-04-25 - Top-Left World Layout

- Updated the gameplay layout so the local map board sits in the top-left region.
- Sized the local map board from roughly half the viewport width and two-thirds of the viewport height.
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

## 2026-04-25 - Prototype Equipment Item Definitions

- Added JSON item definitions for two candidate items per equipment slot.
- Expanded equipment slot accepted type paths so armor helmets, armor jackets, off-hand shields, and back containers validate against the current slot model.
- Added tests that load the new item definitions and check their type paths against equipment slots.
- Kept equip/unequip actions, item effects, sprites, inventory transfer, and equipment UI out of scope.

## 2026-04-25 - Basic Equip Action And Equipment UI

- Added an Equip Item action request handled by the central domain action pipeline.
- Made equip actions available only for held items with `equip` item actions that match an empty equipment slot.
- Equipping now transfers one item from inventory into the chosen empty slot without advancing the turn.
- Added a Godot equipment panel that displays all equipment slots, including empty slots.
- Placed a baseball cap and running shoes on the prototype map so they can be picked up and equipped.
- Added domain tests for equip action availability, no-turn equip resolution, invalid slot rejection, and occupied slot rejection.
- Kept unequip, equipment replacement, item effects, stat modifiers, equipment sprites, and equipment-specific UI interactions out of scope.

## 2026-04-25 - First Firearm Ammunition And Feed System

- Added domain definitions and JSON loaders for firearms, ammunition, and feed devices.
- Added example weapons for 9mm pistol, AK-style rifle, .308 hunting rifle, 12 gauge shotgun, and .22 rifle.
- Added ammunition definitions for 9mm standard, 9mm hollow point, 7.62x39mm, .308, 12 gauge buckshot, 12 gauge slug, and .22 LR.
- Added feed devices for 9mm standard/extended pistol magazines and AK 30-round/damaged 20-round magazines.
- Added runtime firearm state for loaded feed devices, inserted detachable magazines, and built-in weapon feeds.
- Added action-pipeline requests for loading, unloading, inserting, removing, loading directly-fed weapons, and test firing one round.
- Added a prototype firearm status UI and starting inventory content so the system can be manually tested in Godot.
- Added tests for ammunition size/type compatibility, feed device capacity, inventory count changes, magazine compatibility, loaded/empty state, and test-fire round consumption.
- Kept combat, enemies, damage, ballistics, sound propagation, recoil, jamming, durability, weapon condition, saving/loading, and advanced UI out of scope.

## 2026-04-25 - Firearm Feature Hardening Review

- Reviewed the firearm/ammunition/feed-device model for domain ownership, naming, mutation safety, UI clarity, and test coverage.
- Removed unused feed-device runtime metadata that was not pulling its weight.
- Changed firearm action availability queries so they no longer create runtime weapon or feed-device state.
- Tightened failed firearm actions so wrong ammunition, missing ammunition, and empty test-fire attempts do not create placeholder runtime state.
- Updated the firearm panel location text to distinguish inserted feed devices from carried feed devices using domain state.
- Added tests for read-only action queries and failed-action mutation safety.
- Kept the existing scope unchanged: no combat, damage, jamming, durability, recoil, accuracy, sound propagation, saving/loading, procedural generation, or crafting.

## 2026-04-25 - First Stateful Item Model

- Added a domain stateful item store for specific items with runtime ids, locations, optional condition, contained items, and firearm/feed state.
- Added stateful item locations for player inventory, equipment, ground, inserted items, and contained items.
- Added action-pipeline support for picking up, dropping, inspecting, equipping, and unequipping stateful items.
- Extended firearm handling so specific magazines and weapons preserve loaded state when inserted, removed, dropped, picked up, equipped, inspected, and test fired.
- Updated prototype UI panels and hover tooltips to display enough stateful item detail for manual testing.
- Added tests for separate same-type item state, state preservation across insert/remove/drop/pickup/equip, container contents, invalid action safety, and inspection details.
- Kept simple stack inventory for identical bulk items and avoided combat, item effects, durability mechanics, saving/loading, crafting, and advanced UI.

## 2026-04-25 - Tick-Based World Time Foundation

- Replaced the prototype turn counter with domain `WorldTime` elapsed ticks.
- Updated action results to expose the elapsed tick cost of each resolved action.
- Updated the action pipeline so Move costs 100 ticks, Wait costs 100 ticks, successful Pick Up costs 50 ticks, and failed actions currently cost 0 ticks.
- Updated gameplay UI to show elapsed ticks as `Time: n ticks`.
- Updated movement, pickup, action result, and root state tests for tick behavior.
- Kept terrain-based costs, survival decay, actor scheduling, calendars, day/night, and other simulation systems out of scope.

## 2026-04-25 - Gameplay UI Panel And Item Popup Refactor

- Split the gameplay sidebar into three visible panels: player info/general actions, equipment, and inventory.
- Adjusted the panel layout to use the wider available space beside the map instead of compressing everything into one narrow right column.
- Fixed inventory row sizing so the inventory list has usable width inside its scroll panel.
- Added inventory tabs for weapons, weapon parts/ammunition, consumables, and other items.
- Shortened item popup action labels so contextual actions do not repeat the selected item's full name.
- Changed the global action area to show general and selected-target actions such as Wait, Pick Up, and Shoot.
- Made inventory rows and occupied equipment slots selectable with a visible highlight.
- Added an item click popup that shows item definition details, quantity/location, stateful item runtime details, and firearm/feed state where available.
- Moved item-specific actions into the clicked item popup by filtering existing domain action requests for the selected item.
- Preserved movement, wait, pickup, elapsed tick display, inventory display, equipment display, ground item markers, hover tooltip, and message log behavior.
- Kept this as a UI/interaction refactor only; no new gameplay systems or item rules were added.

## 2026-04-25 - Inserted Magazine Reload Timing

- Added reload actions for detachable-feed weapons that already have an inserted magazine/feed device.
- Modelled reload as remove feed device, load compatible held ammunition per round, then reinsert the same feed device.
- Added first-pass firearm handling tick costs: 10 ticks per loaded round, 25 ticks to remove a feed device, and 25 ticks to insert one.
- Updated the action pipeline so successful firearm handling action tick costs advance world time.
- Added tests for reload availability, topping off an inserted stateful magazine, composite reload tick cost, and full-magazine failure safety.

## 2026-04-25 - Stack Item Action Consistency

- Added contextual inspect and drop actions for simple stack-backed inventory items without converting them into stateful item instances.
- Added drop-one and drop-all stack flows that preserve remaining stack quantities and place dropped quantities on the current tile.
- Added unequip support for legacy stack-backed equipment so equipped stack items can return to inventory.
- Kept stack-backed action costs explicit and currently free for inspect, drop, equip, and unequip.
- Added domain tests for stack action availability, inspection, drop quantity handling, safe drop failure, and legacy equipment unequip.

## 2026-04-25 - First World Map Travel Shell

- Added a prototype world map travel state in domain code with continuous map positions, click destinations, travel methods, fixed points of interest, vehicle fuel, and shared world-time advancement.
- Added walking, pushbike, and vehicle prototype travel methods, with only vehicle travel consuming fuel.
- Added domain tests for smooth travel, destination redirection, method-dependent speed, fuel consumption, fuel depletion stopping travel, non-fuel continuation, and nearby point-of-interest detection.
- Added an world map Godot screen with a simple map/background, travel party marker, destination line, fixed point-of-interest markers, travel method controls, time display, fuel display when relevant, messages, and Enter Site action.
- Added a run session shell that starts New Run at the world map, enters the existing local gameplay scene from nearby points of interest, and returns to the world map while preserving world map state and the existing local player/inventory/equipment/firearm state.
- Refactored prototype local-state creation into `PrototypeSessionFactory` so the local scene can run standalone or inside the world map/local session.
- Kept roads, pathfinding, settlements, trading, camping, saving/loading, enemy parties, ambushes, weather, vehicle upgrades, repairs, storage, and new combat systems out of scope.

## 2026-04-25 - JSON-Backed NPC Definition Foundation

- Added reusable NPC definition ids, JSON-backed NPC definitions, NPC catalogs, simple behavior profiles, and an NPC definition loader.
- Added `data/npcs/npcs.json` with the inert Test Dummy definition used by the current prototype map plus five additional prototype NPC definitions: Cautious Survivor, Wandering Scavenger, Injured Traveller, Quiet Mechanic, and Field Researcher.
- Split runtime NPC identity from reusable definition identity so spawned NPCs can preserve instance state while pointing back to shared content data.
- Updated prototype session creation to load NPC definitions from JSON and spawn the test dummy from its definition.
- Updated NPC rendering and hover tooltips to use definition data such as map color, species, tags, behavior kind, and blocking state.
- Added domain tests for NPC definition creation, JSON loading, catalog duplicate validation, runtime definition linkage, and state creation from definitions.
- Kept active NPC AI, decision planning, perception, memory, factions, dialogue, schedules, melee combat, hostile combat, and pathfinding out of scope.

## 2026-04-25 - Route 18 Gas Station Site

- Added Route 18 Gas Station as a fixed world map point of interest that routes to a dedicated local site.
- Added a hand-authored 40x28 gas station local map with asphalt forecourt, concrete pump island and parking area, tiled store interior, back room/staff area, restroom corner, perimeter grass, and blocked scenery objects.
- Added asphalt surface data plus gas station world object definitions for fuel pumps, canopy posts, signage, glass doors, counters, shelves, restroom fixtures, trash bins, bollards, and abandoned vehicles.
- Updated local site/session creation so the default prototype site and gas station keep separate map/object/NPC/ground-stack state while sharing world time, player inventory, equipment, stateful items, firearm/feed state, and vehicle fuel.
- Added a prototype Refuel Vehicle action that appears next to a fuel pump when vehicle fuel is below capacity, restores fuel to 15.0, and advances world time by 100 ticks.
- Added tests for gas station map content, surface/object loading, object collision, site-scoped stateful ground items, and refuel action availability/resolution.
- Kept finite station fuel reserves, payment, fuel cans, pump power, trading, loot, repairs, NPC spawning, saving/loading, and procedural generation out of scope.

## 2026-04-25 - Additional World Object Sprites

- Generated and added transparent sprite assets for tree, boulder, fuel pump, store shelf, and abandoned vehicle world objects.
- Updated the matching world object definitions to reference the new `spriteId` values.
- Kept object behavior unchanged; the new assets only affect presentation.

## 2026-04-25 - Grid Container Inventory Foundation

- Added reusable domain grid container primitives for container ids, item references, item sizes, grid positions, placements, overlap checks, bounds checks, auto-placement, and container stores.
- Backed the player stack inventory with a fixed prototype 20w x 10h item container while preserving stack quantities for simple identical items.
- Added `inventorySize` to JSON item definitions, with a default of 1x1 for definitions that omit it.
- Updated pickup, unequip, and stateful pickup/unequip flows to check inventory grid space before moving items into player inventory.
- Replaced the inventory list display with a selectable 20x10 inventory grid view while keeping category tabs and existing item popup actions.
- Added domain tests for container placement rules, player inventory grid backing, item size loading/defaults, and pickup failure when the inventory grid is full.
- Kept manual rearranging, item rotation, nested grid containers, equipment-provided containers, world-object container contents, weight limits, and save/load persistence out of scope.

## 2026-04-25 - Roadhouse Turret Hazard

- Added an `automated_turret` world object definition and placed one at tile 30,12 on the Route 18 Gas Station map.
- Added post-action turret hazard resolution to the domain action pipeline: successful time-costing local actions trigger one turret check per crossed 75-tick interval, within 5 tiles, for 10 direct health damage per shot.
- Added clamped player health damage through `PlayerVitals.TakeDamage`.
- Added domain tests for turret range, cadence, failed/zero-tick action safety, gas station placement, object loading, and player damage clamping.
- Kept ammo, line of sight, accuracy, projectiles, destructibility, death/game-over handling, and AI scheduling out of scope.

## 2026-04-26 - Campaign State Ownership Refactor

- Added domain `CampaignState` as the persistent run state root for shared world time, player state, stateful items, world map travel, vehicle fuel, active mode/site id, and registered local sites.
- Added `LocalSiteState` so each local site keeps its own `PrototypeGameState`, display metadata, entry position, and last local player position across world map/local transitions.
- Updated `GameSessionShell` to act as a Godot mode coordinator that asks `CampaignState` to enter local sites or return to world map instead of owning default/gas-station sessions and shared state fields.
- Updated `PrototypeSessionFactory` to create the prototype campaign session, catalogs, starting inventory/stateful items, local site states, and action pipeline without becoming the runtime campaign owner.
- Added focused domain tests for shared campaign ownership, world map/local transitions, local site re-entry preservation, and shared inventory/equipment/stateful item persistence.
- Kept stack/stateful item policy, firearm migration, save/load, procedural generation, new gameplay systems, and action-pipeline redesign out of scope.

## 2026-04-26 - Local Map Viewport

- Added domain `GridViewport` for pure map-to-viewport and viewport-to-map coordinate conversion.
- Updated local gameplay rendering to show a fixed 27w x 18h tile viewport instead of the entire local map.
- Kept full local maps in simulation state while `GameShell` and the local render layers crop surfaces, world objects, ground items, stateful ground items, NPCs, player marker placement, hover, and click targeting through the current viewport.
- Larger maps clamp the viewport at map edges; smaller maps remain centered inside the fixed viewport with dark padding.
- Added focused domain tests for centered, clamped, padded, and rejected-padding viewport coordinate behavior.
- Kept map data, movement, collision, pickup, combat, campaign state, and action resolution rules unchanged.

## 2026-04-26 - Maintenance Sweep Instructions

- Added `Do a sweep` guidance to `AGENTS.md` for autonomous, focused codebase maintenance passes.
- Defined sweeps as one coherent non-player-facing improvement at a time, covering cleanup, tests, docs, bug fixes, and architecture refinement.
- Reinforced that sweeps should preserve gameplay behavior, avoid new systems or scope expansion, inspect current code and git state first, and update task logs after implementation.

## 2026-04-26 - Space-Free Ammo Inventory Tab

- Replaced the inventory category tabs with Inventory and Ammo modes.
- Kept non-ammunition carried items and freely carried stateful items in the physical 20w x 10h inventory grid.
- Added loose ammunition handling so item definitions with category `Ammunition` do not consume inventory grid cells.
- Updated pickup, starting inventory, unequip, and firearm unload flows to use the inventory grid rule where catalog data is available.
- Added tests for grid-exempt ammo stacks, ammo pickup with a full inventory grid, and unloading loose ammo from a magazine when the grid is full.
- Kept magazines/feed devices as physical inventory items and left ammo capacity limits, drag/drop rearranging, ammo pouch UI, and nested container behavior out of scope.

## 2026-04-26 - Maintenance Sweep: World Map Travel Validation

- Tightened prototype travel method lookup with an explicit `TryGet` path and clear missing-method errors.
- Validated `WorldMapTravelState` constructor travel method ids so construction and travel-method changes reject unknown enum values consistently.
- Restored `GameActionPipeline` firearm service wiring so item catalog-backed inventory grid rules are available to firearm actions.
- Added focused world map travel tests and verified the full domain test suite passes.

## 2026-04-26 - World Map / Local Map Vocabulary Rename

- Renamed the broad travel layer from world map legacy wording to `WorldMap` code, scene, test, and folder names.
- Renamed the local grid/site map domain from generic world/map wording to `LocalMaps`, `LocalMapState`, and `LocalMap`.
- Updated Godot local map view paths, campaign/session API names, player-facing labels, and documentation to use World Map and Local Map consistently.
- Kept gameplay behavior unchanged: travel, entering sites, returning to the World Map, local viewport rendering, inventory, fuel, and local site persistence all retain their existing rules.

## 2026-04-26 - Id And World Map Site Test Coverage

- Added focused tests for `SurfaceId` and `WorldObjectId` trimming, formatting, and empty-value rejection.
- Added prototype world map site tests for unique ids, in-bounds positions, positive enter radii, and the Route 18 Gas Station site contract.
- Verified the targeted test classes and full domain test suite pass.

## 2026-04-26 - Game Brief Direction Refresh

- Reviewed the current prototype direction against the architecture, scope, task log, and live code structure.
- Replaced the outdated initial shell brief with an updated game brief covering the World Map and Local Map loop, survival/logistics focus, current prototype shape, design pillars, near-term direction, scope guardrails, and technical direction.
- Kept the brief aligned with the current structural prototype rather than promising unimplemented systems.

## 2026-04-26 - Larger World Map Viewport

- Expanded the prototype World Map to 2100w x 1300h while preserving the previous 1200w x 760h visible scale through a following viewport.
- Added `WorldMapViewport` for full-map to visible-window coordinate conversion, edge clamping, and visibility checks.
- Updated World Map drawing and click destination selection to use viewport coordinates so the map pans with the travel party.
- Increased fixed World Map points of interest to twelve, kept Route 18 Gas Station near the starting region, and left all other POIs routing to the default local map.
- Added domain tests for viewport centering, edge clamping, coordinate conversion, visibility checks, destination clamping, and the expanded POI contract.

## 2026-04-26 - Automated Turret NPC Sprite

- Moved the Route 18 Gas Station automated turret from static world-object placement into the NPC roster at tile 30,12.
- Added an Automated Turret NPC definition with sprite metadata and a 32x32 turret sprite under `data/sprites/npcs/`.
- Updated NPC rendering to load optional NPC sprites before falling back to colored markers.
- Updated turret hazard resolution to scan enabled automated turret NPCs instead of world objects, so a disabled turret no longer fires.
- Updated tests and docs for the NPC-based turret placement and reduced static world-object list.

## 2026-04-26 - Authored Local Map Data

- Moved the default prototype local site and Route 18 Gas Station layouts into JSON files under `data/local_maps/`.
- Added a domain local map builder and `LocalSiteDefinitionLoader` for authored surface/object rows plus sparse item and NPC placements.
- Added explicit non-functional recipe and chunked procedural map source stubs that fail clearly until those systems are implemented.
- Updated prototype session creation to register local sites from loaded map data.
- Added loader tests for current map content, malformed authored maps, and unsupported recipe/chunk source kinds.

## 2026-04-26 - Automatic Sprite Overflow And Y-Sorting

- Added optional visual-only `spriteRender` metadata for world object and NPC definitions.
- Replaced separate world object, NPC, and player visual ordering with a single `MapEntityLayer` that draws map entities by screen Y.
- Configured the tree to render larger than one tile and the automated turret to use a wider 48x32 sprite with its barrel extending into the next tile.
- Kept collision, hover, targeting, turret hazards, and tile ownership tied to the original grid tile.
- Added tests for sprite render metadata loading, default absence, and non-positive size rejection.

## 2026-04-26 - Phase 4 Action Pipeline Handler Decomposition

- Split the monolithic `GameActionPipeline` into a thin dispatcher plus movement, inventory, equipment, inspect, firearm, and interaction handlers.
- Added `GameActionContext`, `ActionHandlerRegistry`, `IActionHandler`, `ItemDescriber`, and `NpcCombatService` to make action dependencies and follow-up NPC combat explicit.
- Changed the action execution API to `Execute(request, state)` and updated Godot/test call sites.
- Moved stateful inventory grid placement synchronization into `PlayerInventory`.
- Passed the NPC catalog into prototype action pipeline creation so runtime automated hazard behavior uses NPC behavior tags.
- Kept Phase 5 firearm internals out of scope; firearm handling still delegates to `FirearmActionService`.

## 2026-04-26 - Refactoring And Test Discipline Guidance

- Added project-wide AI guidance that tests should detect unexpected behavior changes, not force stale production code structure to remain.
- Clarified that planned refactors should favor maintainable, scalable architecture and update tests when test expectations describe old APIs, helpers, or file layouts.
- Tightened maintenance sweep wording so behavior preservation refers to player-facing gameplay and domain invariants, not unhelpful implementation shapes.

## 2026-04-26 - Phase 5 Firearm Action Service Decomposition

- Split `FirearmActionService` into a public facade plus `FirearmValidator`, `FirearmStateOperations`, `FirearmActionProvider`, firearm refs/adapters, and shared firearm support helpers.
- Kept stack-backed and stateful firearm behavior aligned while moving validation, mutation, action-list generation, timing/message helpers, and inventory/item lookup out of the facade.
- Removed the old service-only `GetAvailableRounds` and `IsLoaded` helpers and updated firearm tests to assert runtime firearm state directly.
- Preserved current player-facing firearm behavior: load/unload, insert/remove, reload timing and messages, test-fire consumption, shooting range checks, NPC damage, and failed-action mutation safety.
- Verified focused firearm, stateful item, and game action pipeline suites, then verified the full solution test suite.

## 2026-04-26 - C# Brace Style EditorConfig

- Added a root `.editorconfig` for C# files.
- Set `csharp_new_line_before_open_brace = none` so C# opening braces stay on the same line.

## 2026-04-26 - Stronger Test Compatibility Guidance

- Reframed test guidance around all implementation work, not only refactors.
- Clarified that production code should never preserve stale APIs, helpers, fixtures, file layouts, mocks, or expectations solely for test compatibility.
- Directed future agents to update, rewrite, move, or delete tests when the best implementation changes structure while still preserving intended player-facing behavior and domain invariants.

## 2026-04-26 - Edge-Based Structures And 2.5D Wall Rendering

- Added edge-based structure definitions, catalogs, placement maps, authored map loading, and render variant resolution for walls, doors, windows, fences, gates, and gaps.
- Updated movement to check the crossed structure edge before tile-object and NPC blocking.
- Added structure loading to prototype session setup and local map state, plus hover tooltip support for structures bordering the hovered tile.
- Updated `MapEntityLayer` to render edge structures in the Y-sorted entity pass with fallback 2.5D front/side wall geometry when no structure sprite exists.
- Added `data/structures/structures.json` and authored Abandoned Farmhouse structure edges for the house, interior partitions, shed, and paddock fencing.
- Added focused tests for structure edge lookup, duplicate validation, render variant resolution, structure-edge movement, map loading, and farmhouse structure presence.

## 2026-04-26 - Edge Wall Visual Cleanup

- Removed the remaining farmhouse wall, door, window, fence, gate, and gap tile-object definitions from the world-object catalog so those pieces are structure-edge-only.
- Removed unused old wall symbols from the Abandoned Farmhouse object layer legend and tightened tests so those ids cannot return as tile blockers.
- Reworked side-wall fallback rendering to stay on its own vertical edge cell, slant upward, avoid overpainting doorway openings, and only draw opening jambs at the ends of multi-tile openings.
- Reduced repeated per-tile outlines on front wall runs so connected wall stretches and corner joins read less like separate stacked blocks.

## 2026-04-26 - Farm Vehicle World Object Sprites

- Added transparent world-object sprites for the farmhouse `tractor_wreck` and `farm_trailer` objects.
- Wired both definitions to their new sprite ids and normalized their canonical north-facing footprints to `2x3`.
- Set the authored farmhouse placements to face east so their effective occupied map footprint remains `3x2`.
- Updated world-object catalog tests to cover the new sprite ids and canonical footprints.

## 2026-04-26 - Sprite Guidance Follow-Up

- Expanded AI sprite creation guidance with lessons from generated farm vehicle assets.
- Clarified canonical north-facing footprints versus rotated placement `facing`.
- Added generated-sprite workflow notes for chroma-key cleanup, trim/downscale order, alpha validation, sprite reference checks, and temporary `.import` cleanup.

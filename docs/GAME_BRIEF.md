# Game Brief

## High Concept

This is a 2D turn-based open-world survival roguelike about travelling through a dangerous, sparse world, entering local sites, scavenging under pressure, and managing the practical details of survival.

The strategic shape is a Battle Brothers-style travel layer above detailed local-site exploration, not one continuous single-scale grid. The World Map is larger than the visible camera view so travel can grow across a broader authored space without zooming out the interface. The long-term tone is systemic and readable: the game should have the depth and surprising interactions of classic roguelikes, but with a modern interface that makes equipment, inventory, ammunition, travel, and local-site decisions understandable at a glance.

The project is currently a structural prototype, not a content-complete game. The priority is to prove the core shape of play before adding broad simulation.

## Core Player Fantasy

The player is a survivor moving between locations on a broad world map. They may travel on foot, by pushbike, or by vehicle when one is usable and fuel is available. A vehicle is useful, but not assumed to be permanent or reliable. Running out of fuel should change the plan, not end the run.

At points of interest, the player enters local maps where movement becomes tile-based and turn/time based. Local sites should feel inspectable and tactical: objects block movement, items can be picked up, equipment and firearms can be managed, NPCs can exist in the space, and actions advance shared world time.

## Current Prototype Shape

- New Run starts on the World Map, not inside a local site.
- The World Map supports smooth click-to-travel movement, travel methods, shared time, vehicle fuel, fixed points of interest, and entering nearby sites.
- Route 18 Gas Station is the first dedicated local site, with a hand-authored 40x28 map, pump area, store interior, fixtures, collision, refuel action, and a fixed turret NPC hazard.
- Other prototype points of interest enter the default local test site.
- Returning from a local site preserves world map position, time, travel method, vehicle fuel, inventory, equipment, and firearm/feed-device state.
- Local gameplay uses tile movement, collision, pickup, hover tooltips, equipment, a physical 20x10 inventory grid, a separate loose-ammo list, and contextual item actions.
- Firearm handling is already stateful enough to support magazines/feed devices, loaded rounds, reload/unload/insert/remove flows, weapon range, and simple targeted shooting against the test NPC.
- Content definitions are increasingly JSON-backed: items, firearms, ammunition, feed devices, surfaces, world objects, and NPC definitions.

## Design Pillars

1. Systems first, spectacle later.
   The game should grow through small, composable rules: time costs, item state, travel constraints, local-site state, surfaces, world objects, NPC state, and inventory pressure.

2. Two scales of play.
   The World Map is for route, fuel, time, and destination choices. Local Maps are for concrete tactical interaction, searching, movement, equipment, and risk.

3. Survival through logistics.
   Inventory shape, ammunition, fuel, equipment, and time should matter. The interesting question is often what the player can carry, use, repair, load, or risk.

4. Vehicles are tools, not assumptions.
   Walking and pushbike travel must remain valid. Vehicle systems should support fuel scarcity and future condition/repair/storage without making the player dependent on a working car.

5. Data-driven content.
   Definitions should live in data where sensible so future content can expand without scattering rules through UI code.

6. Domain-owned simulation.
   Godot presents and collects input. Plain C# domain code owns canonical simulation state and rules wherever practical.

## Near-Term Direction

The strongest next slices are the ones that deepen the existing loop without exploding scope:

- Container contents for world objects such as fridges, crates, shelves, and abandoned vehicles.
- Local-site interaction actions such as search, open/close, and take-from-container.
- A clearer local-site content model for placed loot, fixtures, and authored encounters.
- Better inventory/container transfer UI built on the existing grid container foundation.
- A more capable NPC foundation: definitions, runtime state, simple needs/roles, perception hooks, and later behavior systems.
- More hand-authored local sites before procedural generation.
- Incremental survival rules that use existing world time, such as hunger/thirst/fatigue decay, only once the player has meaningful ways to respond.

## Scope Guardrails

Do not rush into large systems before the core loop is sturdy. The following are intentionally not the current focus unless explicitly requested:

- Procedural world generation.
- Full hostile AI or broad combat simulation.
- Quests, factions, dialogue, trading, settlements, or economy.
- Saving/loading.
- Weather, roads, pathfinding, camping, or enemy parties on the World Map.
- Vehicle upgrades, repair trees, storage systems, or detailed vehicle condition.
- Complex ballistics, accuracy, armor, sound propagation, recoil, jamming, or hit locations.
- Full survival metabolism before time, travel, inventory, and local interaction can support it.

## Technical Direction

The game should continue to keep simulation logic in `src/SurvivalGame.Domain/` and Godot presentation/input/UI code in `src/Godot/`. New mechanics should normally enter through action requests, validation, simulation resolution, time advancement, messages, and UI refresh.

Prefer fixed, testable vertical slices over broad speculative architecture. Hand-authored maps and data files are acceptable while the design is still proving itself. The foundation should stay simple, but it should keep pointing toward a larger systemic survival roguelike.

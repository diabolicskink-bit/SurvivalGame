# Mechanics Backlog

This is the living tracker for deferred player-facing mechanics and systems. It captures mechanics that are identified during planning or documentation work but intentionally left out of current scope.

Use stable IDs when discussing or planning these items, such as `MECH-1`. Do not renumber existing items. This backlog is memory and triage, not permission to implement new mechanics.

Each `MECH-*` item should describe one implementable system or one small vertical slice. Do not bundle whole families of mechanics into one item. For example, track `Thirst decay and drinking`, `Hunger decay and eating`, and `Fatigue accumulation and rest recovery` separately instead of one broad survival-meters item.

## How To Maintain This Backlog

- Add or update a `MECH-*` item when a plan identifies a meaningful deferred player-facing mechanic or system.
- Split bundled ideas before assigning an ID. If a phrase lists several systems with commas or slashes, it probably needs multiple `MECH-*` items.
- When planning a `MECH-*` implementation, first assess whether the item is too broad for one playable slice. If it is, propose a split into smaller `MECH-*` items instead of forcing one large implementation plan.
- If a split is accepted, preserve stable IDs by creating new `MECH-*` items and either narrowing the original item or marking it `Superseded` with links to the replacement items.
- Append dated `Notes` entries when general work reveals useful implementation context, constraints, risks, or observations for a tracked mechanic.
- Keep notes factual and implementation-relevant. If new information changes a canonical field such as priority, size, dependencies, first playable slice, or completion signal, update that field as well.
- Use notes for extra context, not as a replacement for the item's canonical fields.
- Do not add entries for tiny implementation details, one-off content, architecture debt, or design principles.
- Use `ARCH-*` for architecture pressure and `MECH-*` for future gameplay, UI, simulation, world, and systems mechanics.
- Keep `docs/CURRENT_SCOPE.md` as the source of truth for implemented scope.
- Keep `docs/DESIGN_GOALS.md` as long-term design direction.
- When a mechanic is implemented, mark it `Implemented`, update `docs/CURRENT_SCOPE.md`, and add a task-log entry if it changes durable project state.
- Keep active items ordered by priority first, then ID.

## Priority, Size, And Status

Priorities:

- `P1`: Likely high-value or nearer foundation.
- `P2`: Important but dependent, larger, or best after a nearby foundation.
- `P3`: Long-term watchlist.

Sizes estimate the likely effort and blast radius to reach the tracked `Implemented When` signal, not the whole end-state feature family. If an `xl` or `xxl` item is selected for implementation, first look for a smaller playable slice or split.

Sizes:

- `xs`: Tiny doc, data, test, or one-call-site change.
- `s`: Narrow mechanic or UI change in one small area.
- `m`: Focused playable slice across a few files or tests.
- `l`: Multi-boundary mechanic that needs careful planning.
- `xl`: Large multi-system effort that should usually be split.
- `xxl`: Roadmap-scale mechanic family that must be split before implementation.

Statuses:

- `Open`: Known future mechanic, not currently planned for implementation.
- `Planned`: Selected for an upcoming implementation plan.
- `Active`: Current work is directly implementing it.
- `Implemented`: The mechanic has reached the tracked completion signal.
- `Superseded`: Replaced by another tracker item or design direction.

## Active Items

### MECH-1 - Cover modifier for ranged attacks

- `Priority`: `P1`
- `Size`: `m`
- `Priority Rationale`: Combat already has ranged attacks, hit chances, line-of-fire blockers, and map structures, so cover is a high-value tactical improvement that can be added as a contained rule.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`
- `Category`: Combat, Firearms
- `Player-Facing Goal`: Let nearby structures or blocking objects make shooting decisions feel more tactical without adding full armor, penetration, recoil, or durability yet.
- `Current State`: Targeted shooting has range, accuracy endpoints, line-of-fire blockers, ammunition damage, weapon mods, fire modes, and burst fire. Cover-specific accuracy or damage modifiers are not implemented.
- `Why Deferred`: It needs clear hit-chance explanation and careful rules so cover feels readable instead of hidden.
- `First Playable Slice`: Apply a simple defensive hit-chance modifier when the target has eligible adjacent cover between them and the shooter.
- `Dependencies`: Line-of-fire/query rules, local map structures and blockers, action result messaging, and UI hit-chance display.
- `Implemented When`: Cover affects at least one shot calculation, is visible in player-facing shooting details or messages, and is covered by domain tests.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`, `src/SurvivalGame.Domain/Firearms/`

### MECH-2 - Drag-to-move inventory items

- `Priority`: `P1`
- `Size`: `m`
- `Priority Rationale`: Inventory pressure is already central to play and the current grid exists, so direct item movement is a near-term usability foundation.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`
- `Category`: Inventory, UI
- `Player-Facing Goal`: Let the player manually move an item from one valid inventory grid position to another.
- `Current State`: Inventory displays a fixed grid, ammo list, stack items, stateful items, equipment, and contextual item actions. Manual grid rearranging is not implemented.
- `Why Deferred`: It needs careful UI behavior and must preserve current item identity, stack handling, and action menu behavior.
- `First Playable Slice`: Add drag-to-move for existing player inventory grid items only, without rotation or container transfer.
- `Dependencies`: Existing inventory grid rules, item size data, stateful item display, and item identity cleanup.
- `Implemented When`: The player can drag an inventory item to another valid grid location without losing stack/stateful identity, and current item actions still work.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `src/Godot/Game/UI/InventoryGridView.cs`, `src/SurvivalGame.Domain/Inventory/`

### MECH-5 - Thirst decay and drinking

- `Priority`: `P1`
- `Size`: `l`
- `Priority Rationale`: Thirst is a core survival pressure and can be introduced as one readable meter with one mitigation action before broader survival simulation.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`
- `Category`: Survival, Items, Time
- `Player-Facing Goal`: Make elapsed time increase thirst pressure and let the player reduce it by drinking an appropriate item.
- `Current State`: Player vitals exist and water items exist, but thirst does not decay over time and drinking does not mitigate a survival meter.
- `Why Deferred`: It needs a stable elapsed-time hook, item-use action shape, visible UI feedback, and messages that explain changes.
- `First Playable Slice`: Add thirst increase from elapsed world/local time plus one drink action for an existing water item.
- `Dependencies`: Turn/action timing, player vitals, item use actions, consumable content data, messages, and UI vitals display.
- `Implemented When`: Thirst changes over time, can be reduced by a drink action, persists in session state, and has focused tests.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`, `src/SurvivalGame.Domain/Actors/PlayerVitals.cs`, `src/SurvivalGame.Domain/Actions/`

### MECH-8 - Save/load campaign snapshot

- `Priority`: `P1`
- `Size`: `xl`
- `Priority Rationale`: Persistence is foundational for a larger roguelike run, but the first useful version can be scoped to one campaign snapshot.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/ARCHITECTURAL_DEBT.md`
- `Category`: Persistence, Campaign
- `Player-Facing Goal`: Let the player leave and resume a run without losing campaign, local-site, inventory, and item state.
- `Current State`: Runtime state persists only during the current process/session. Save/load is not implemented.
- `Why Deferred`: It depends on stable ownership boundaries and durable serialization shapes for campaign, local sites, items, and content references.
- `First Playable Slice`: Save and load one current campaign session from a local file, covering world map position, time, player inventory, stateful items, and entered local-site state.
- `Dependencies`: Campaign state ownership, local runtime state, item identity rules, content id stability, and application/session layer.
- `Implemented When`: A run can be saved, the app restarted, and the same world/local/inventory state restored from disk.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `src/SurvivalGame.Application/`, `src/SurvivalGame.Domain/Campaign/`

### MECH-3 - Inventory item rotation

- `Priority`: `P2`
- `Size`: `m`
- `Priority Rationale`: Rotation is useful for grid inventory play, but it should follow basic drag-to-move so the interaction model and item identity are already stable.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`
- `Category`: Inventory, UI
- `Player-Facing Goal`: Let the player rotate eligible inventory items between horizontal and vertical footprints.
- `Current State`: Item grid footprints exist, but player-controlled item rotation is not implemented.
- `Why Deferred`: It needs clear item-shape rules, collision checks, UI preview states, and a decision on which items can rotate.
- `First Playable Slice`: Add a rotate command for one selected grid item while dragging or inspecting it, only when the rotated footprint fits.
- `Dependencies`: Drag-to-move inventory behavior, item footprint metadata, inventory placement validation, and UI feedback.
- `Implemented When`: Eligible items can rotate in the inventory grid, invalid rotations are prevented with feedback, and tests cover footprint validation.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `src/Godot/Game/UI/InventoryGridView.cs`, `src/SurvivalGame.Domain/Inventory/`

### MECH-4 - Recipe-backed local site generation

- `Priority`: `P2`
- `Size`: `xl`
- `Priority Rationale`: Procedural local maps are important for replayability, but one recipe-backed site should build on authored examples and validation first.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`
- `Category`: Local Maps, World Generation
- `Player-Facing Goal`: Generate one believable local site from a recipe instead of hand-authored tile rows.
- `Current State`: Authored JSON local sites load from `data/local_maps`. Recipe and chunked procedural source kinds are placeholders only.
- `Why Deferred`: Generation needs content rules, validation, authored examples, and map query/rendering stability before it can produce useful gameplay.
- `First Playable Slice`: Implement one recipe-backed small local site type using existing surfaces, structures, world objects, and item placements.
- `Dependencies`: Content validation, local map builder rules, map query rules, site authoring guidance, and stable content ids.
- `Implemented When`: One procedural local site can be generated, entered, rendered, navigated, and tested without hand-authored tile rows.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`, `src/SurvivalGame.Domain/LocalMaps/`

### MECH-6 - Hunger decay and eating

- `Priority`: `P2`
- `Size`: `m`
- `Priority Rationale`: Hunger is core survival pressure, but it should follow the first survival-meter slice so decay, mitigation, and UI patterns are already proven.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`
- `Category`: Survival, Items, Time
- `Player-Facing Goal`: Make elapsed time increase hunger pressure and let the player reduce it by eating an appropriate item.
- `Current State`: Player vitals exist and food items can exist as content, but hunger does not decay over time and eating is not a survival action.
- `Why Deferred`: It should reuse the time, item-use, messaging, and UI patterns established by the first survival meter instead of inventing a parallel path.
- `First Playable Slice`: Add hunger increase from elapsed time plus one eat action for an existing or newly defined food item.
- `Dependencies`: Player vitals, elapsed-time updates, item use actions, consumable content data, messages, and UI vitals display.
- `Implemented When`: Hunger changes over time, can be reduced by eating, persists in session state, and has focused tests.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`, `src/SurvivalGame.Domain/Actors/PlayerVitals.cs`, `src/SurvivalGame.Domain/Actions/`

### MECH-7 - Fatigue accumulation and rest recovery

- `Priority`: `P2`
- `Size`: `l`
- `Priority Rationale`: Fatigue can make travel and action costs matter, but it needs time rules and recovery behavior before it can be fair.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`
- `Category`: Survival, Time
- `Player-Facing Goal`: Let exertion or elapsed time build fatigue and let resting reduce it with clear tradeoffs.
- `Current State`: Player vitals exist, but fatigue, rest recovery, sleep pressure, and action penalties are not implemented.
- `Why Deferred`: It needs readable time costs, rest commands, safe/unsafe rest context, and UI feedback so the player understands the trade.
- `First Playable Slice`: Add a rest action that spends time and reduces a fatigue meter that increases from travel or repeated actions.
- `Dependencies`: Action timing, player vitals, world/local time advancement, message log, and UI vitals display.
- `Implemented When`: Fatigue can rise, resting can reduce it, and both changes are visible and tested without adding full sleep simulation.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`, `src/SurvivalGame.Domain/Actors/PlayerVitals.cs`, `src/SurvivalGame.Domain/Actions/`

### MECH-9 - Basic hostile NPC turn behavior

- `Priority`: `P2`
- `Size`: `l`
- `Priority Rationale`: NPC behavior is central to local-site risk, but the first step should be one simple behavior before factions, dialogue, schedules, or social systems.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`
- `Category`: NPCs, Local Sites
- `Player-Facing Goal`: Let one non-turret hostile NPC make a simple turn decision that the player can read and react to.
- `Current State`: NPC definitions, runtime state, blocking, health, sprites, target selection, and a simple automated turret behavior exist. General NPC turn behavior is not implemented.
- `Why Deferred`: It requires action scheduling, map/perception queries, and clear feedback for what the NPC is doing.
- `First Playable Slice`: Add one hostile NPC behavior that waits, approaches, or attacks based on player distance and line of sight.
- `Dependencies`: Turn scheduling, local action effects, perception/map query rules, NPC definitions, and message/UI feedback.
- `Implemented When`: At least one non-turret NPC can make a domain-owned decision that changes local gameplay and is understandable to the player.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`, `src/SurvivalGame.Domain/Actors/`

### MECH-10 - Vehicle cargo capacity grid

- `Priority`: `P2`
- `Size`: `l`
- `Priority Rationale`: Cargo constraints support the mobile-base fantasy, but they should be introduced as one vehicle inventory rule before repairs, towing, upgrades, or vehicle condition.
- `Status`: `Open`
- `Source`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`
- `Category`: Vehicles, Inventory, Mobile Base
- `Player-Facing Goal`: Give travel cargo a visible capacity limit so vehicles create meaningful storage tradeoffs.
- `Current State`: Walking, pushbike, and vehicle travel exist. Vehicle fuel exists, local vehicle anchors exist, and travel cargo persists without capacity or grid limits.
- `Why Deferred`: It needs container UI behavior, item identity stability, and a rule for how capacity changes by travel method.
- `First Playable Slice`: Add a simple cargo grid or slot capacity for the current vehicle/travel cargo while preserving current stow/take actions.
- `Dependencies`: Inventory/container UI, item identity, vehicle/travel method state, world map travel rules, and cargo persistence.
- `Implemented When`: Travel cargo has an enforced visible capacity, stow/take respects it, and cargo still persists across world/local transitions.
- `Notes`:
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/DESIGN_GOALS.md`, `src/SurvivalGame.Domain/WorldMap/`, `src/SurvivalGame.Domain/Campaign/`

### MECH-11 - Interactive doors and doorway visuals

- `Priority`: `P2`
- `Size`: `xl`
- `Priority Rationale`: Doorways are important for site readability and tactical movement, but they should build on the 2.5D tile-wall direction and action/result cleanup rather than becoming static art-only markers.
- `Status`: `Open`
- `Source`: `2026-04-30 farmhouse tile-wall conversion`
- `Category`: Local Maps, Interaction, Structures
- `Player-Facing Goal`: Let doors be visible map features that can be open, closed, and eventually lockable, while open doorways still read clearly as passable openings.
- `Current State`: Some passable farmhouse and shed openings are authored as empty gaps after the tile-wall visual conversion. Existing closed wooden door and glass-door tile visuals are procedural, but generic open/close/lock door state is not implemented.
- `Why Deferred`: It needs canonical wall/door representation, stateful door runtime data, interaction actions, movement and line-of-fire rules, messages, and clear visual states.
- `First Playable Slice`: Add one visible passable doorway/open-door variant for tile-wall maps, then route a single open/close interaction through domain state without adding lockpicking or key systems.
- `Dependencies`: `ARCH-16`, `ARCH-17`, `ARCH-18`, `ARCH-19`, local map query rules, action result/effect shape, hover text, and tile-wall renderer cleanup.
- `Implemented When`: At least one authored door can display open and closed states, movement respects that state, the player can change it through an action, and tests cover passability plus messaging.
- `Notes`:
  - 2026-04-30: The farmhouse conversion intentionally leaves passable openings as gaps for visual inspection; the desired future is interactive doors, not static doorway tiles only.
  - 2026-04-30: Because current doorway gaps are empty map cells, future door mechanics need explicit authored/runtime door identity before they can support open, closed, and lockable state.
  - 2026-04-30: Door implementation should assume 2.5D tile-wall building semantics for now, not a return to edge-based building doors.
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/ARCHITECTURAL_DEBT.md`, `src/SurvivalGame.Domain/Actions/`, `src/SurvivalGame.Domain/LocalMaps/`, `src/Godot/Game/LocalMapView/MapEntityLayer.cs`

### MECH-12 - Tile-based 2.5D fence, gate, and gap objects

- `Priority`: `P2`
- `Size`: `m`
- `Priority Rationale`: Fences and gates are wall-like for movement and readability, but they should not become full building walls. They need tile-based 2.5D objects now that authored edge-structure usage has been retired.
- `Status`: `Open`
- `Source`: `2026-04-30 2.5D tile-wall direction`
- `Category`: Local Maps, Visuals, World Objects
- `Player-Facing Goal`: Make fences, gates, and broken fence gaps read as low 2.5D tile-authored features with clear passable and blocking states.
- `Current State`: Authored farm paddock fence, gate, and broken-gap edge structures were removed in `ARCH-17`. The farmhouse paddock crossings are temporarily open until tile-based replacement objects are added.
- `Why Deferred`: It needs tile-world-object ids, footprints, passability rules, hover text, and visuals that stay lighter than full building-wall tiles.
- `First Playable Slice`: Add tile-authored wire fence, open farm gate, and broken fence gap objects to the farmhouse paddock area with readable 2.5D visuals and explicit movement behavior.
- `Dependencies`: `ARCH-16`, `ARCH-19`, local map query rules, hover text, and tile-world-object content validation.
- `Implemented When`: Farmhouse paddock fences block movement, open gates and broken gaps remain passable, and all three have distinct tile-based 2.5D visuals that do not read as full building walls.
- `Notes`:
  - 2026-04-30: Track this separately from building walls because fences are wall-like boundaries but should stay visually and semantically lighter than full tile-owned walls.
  - 2026-04-30: Former edge-structure style cues worth preserving for tile-object replacement include `wire_fence`, `timber_fence`, `open_farm_gate`, and `broken_fence_gap`.
  - 2026-04-30: Useful future tile object/style concepts include rusty wire fence spans, weathered timber fence spans, open/closed farm gates, collapsed wire gaps, dragged-down fence wire, and post-only broken boundary markers.
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/ARCHITECTURAL_DEBT.md`, `data/world_objects/`, `data/local_maps/`, `src/Godot/Game/LocalMapView/MapEntityLayer.cs`

### MECH-13 - Tile-wall material and style variants

- `Priority`: `P2`
- `Size`: `l`
- `Priority Rationale`: The 2.5D wall shape is now the preferred direction, but different sites need material identity so a farmhouse, gas station, shed, brick shop, and concrete interior do not all read as the same generic wall.
- `Status`: `Open`
- `Source`: `2026-04-30 2.5D tile-wall direction`
- `Category`: Local Maps, Visuals, Content
- `Player-Facing Goal`: Let tile walls communicate material and place identity, such as wood, concrete, brick, plaster, corrugated metal, boarded, or damaged wall styles.
- `Current State`: Current procedural tile walls use generic `wall` and `window` world-object ids with simple colors/details. Farmhouse and gas station walls share the same core renderer even when their buildings should feel materially different.
- `Why Deferred`: It needs a small wall material/style data shape and renderer support without hard-coding one-off map-specific colors into the draw path.
- `First Playable Slice`: Add two or three authored wall material variants for existing sites, such as farmhouse wood/weatherboard, gas-station plaster/concrete, and shed corrugated metal, while preserving current collision.
- `Dependencies`: `ARCH-16`, `ARCH-17`, `ARCH-18`, local map authoring guidance, and content validation for wall object ids or material metadata.
- `Implemented When`: At least two authored local sites show distinct 2.5D wall materials through data-driven wall styling, and tests or validation catch missing wall material references.
- `Notes`:
  - 2026-04-30: This should extend the tile-wall direction, not revive edge-based building wall sprites.
  - 2026-04-30: Former edge-structure style cues worth carrying into tile-wall/material work include `generic_interior`, `farmhouse_weatherboard`, `shed_corrugated`, and `gas_station_plaster`.
  - 2026-04-30: Former detail cues worth preserving for tile-based doors/windows include closed wooden doors, open wooden doors, torn flyscreen/screen doors, open interior doorway thresholds, glass doors, broken windows, and boarded windows.
  - 2026-04-30: Useful future tile-wall style names/concepts include farmhouse weatherboard, generic interior plaster, gas-station plaster/concrete, shed corrugated metal, boarded-over window modules, cracked/broken glass windows, and damaged/aged wall variants.
- `Links`: `docs/CURRENT_SCOPE.md`, `docs/ARCHITECTURAL_DEBT.md`, `data/world_objects/`, `data/local_maps/`, `src/Godot/Game/LocalMapView/MapEntityLayer.cs`

## Archive

No archived items yet.

# Architectural Debt Tracker

This is the living index for architecture debt and improvement opportunities that future AI coding sessions should keep visible. It tracks pressure points in the codebase, not new gameplay scope.

Use stable IDs when discussing or working on these items, such as `ARCH-1`. Do not renumber existing items. When an item is resolved or superseded, move it to the archive section and keep its ID intact.

## How To Maintain This Tracker

- Add an item when architecture debt is important enough that future sessions should remember it across tasks.
- Update an item when related work changes its risk, priority, next action, or status.
- When planning an `ARCH-*` implementation, first assess whether the item is too broad for one safe, behavior-preserving change. If it is, propose a split into smaller `ARCH-*` items instead of forcing one large implementation plan.
- If a split is accepted, preserve stable IDs by creating new `ARCH-*` items and either narrowing the original item or marking it `Superseded` with links to the replacement items.
- Append dated `Notes` entries when general work reveals useful implementation context, constraints, risks, or observations for a tracked item.
- Keep notes factual and implementation-relevant. If new information changes a canonical field such as priority, size, preferred direction, next action, or completion signal, update that field as well.
- Use notes for extra context, not as a replacement for the item's canonical fields.
- Mark an item `Active` when current work is directly addressing it.
- Mark an item `Resolved` only when the architectural pressure is actually removed.
- Mark an item `Superseded` when another `ARCH-*` item or implemented direction replaces it.
- Keep active items ordered by priority first, then ID.
- Keep this tracker compact. Link to deeper plans, docs, or code areas instead of copying long analysis here.
- Do not treat any item here as permission to expand current game scope.

## Priority, Size, And Status

Priorities:

- `P0`: Urgent blocking architectural risk.
- `P1`: Next high-value refactor.
- `P2`: Planned or important improvement.
- `P3`: Watchlist.

Sizes estimate the likely full resolution effort and blast radius for the tracked item, not just the next action. If an `xl` or `xxl` item is selected for implementation, first look for a smaller behavior-preserving slice or split.

Sizes:

- `xs`: Tiny doc, test, or one-call-site cleanup.
- `s`: Narrow change in one small area.
- `m`: Focused vertical slice across a few files or tests.
- `l`: Multi-boundary change that should be planned carefully.
- `xl`: Large multi-system effort that should usually be split.
- `xxl`: Roadmap-scale pressure that must be split before implementation.

Statuses:

- `Open`: Known, not currently being addressed.
- `Active`: Current work is directly addressing it.
- `Resolved`: The architectural pressure has been removed.
- `Superseded`: Replaced by another tracked item or implemented direction.

## Resolution Guardrails

- Preserve current player-facing behavior unless a future task explicitly requests a behavior change.
- Keep Godot responsible for scenes, input capture, rendering, animation, audio, and UI composition.
- Keep simulation rules, validation, state mutation, time advancement, and generated messages in domain or application code.
- Keep `data/` at the Godot project root unless there is a concrete replacement for `res://data/...` runtime access.
- Prefer small vertical refactors with focused tests over broad mechanical moves.
- Move or rename folders when an ownership boundary is being clarified by actual code changes, not for cosmetic tidiness alone.
- Do not treat any item here as permission to add route planning, weather, survival decay, procedural generation, party systems, NPC AI, saving, or other new gameplay scope.

## Active Items

Each active item should include enough direction that a future session can choose a small, behavior-preserving slice without rediscovering the whole problem. `Priority Rationale` explains the rating, `Resolution Path` describes the likely sequence, `Resolved When` describes the signal for moving the item to the archive, and `Notes` captures dated discoveries from related work.

### ARCH-2 - World travel bypasses the command pipeline

- `Priority`: `P1`
- `Size`: `xl`
- `Priority Rationale`: This is `P1` because travel is already a central player-facing loop and future consequences will multiply the cost of the current screen-driven flow. It is not `P0` because current travel behavior is still functioning and can be migrated one command at a time.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: World Map, Action Pipeline
- `Problem`: World map travel still performs destination selection, travel method changes, travel advancement, time updates, fuel handling, and messages through screen-level coordination instead of a campaign-level command flow.
- `Why It Matters`: Future travel consequences such as interruptions, route risk, fatigue, encounters, or weather will be harder to validate consistently if world travel remains UI-driven.
- `Preferred Direction`: Add a campaign-level command pipeline with one outcome shape for elapsed time, messages, domain events, and follow-up effects, while preserving current local action behavior.
- `Resolution Path`: Start with one narrow world-map command, such as changing travel method or advancing travel toward the selected destination, and return a command result containing success/failure, elapsed ticks, messages, and any changed campaign state. Move additional world travel mutations behind the same flow in later slices. Keep screen code focused on input, display, and presenting command results.
- `Next Action`: Define and route the smallest world travel command through domain/application code without changing player-facing travel behavior.
- `Resolved When`: World-map destination selection, travel method changes, travel advancement, fuel use, time advancement, and travel messages are resolved through domain/application commands instead of direct screen mutation, with tests covering current travel behavior.
- `Notes`:
- `Links`: `src/Godot/WorldMap/`, `src/SurvivalGame.Domain/WorldMap/`, `src/SurvivalGame.Domain/Actions/`

### ARCH-10 - Local actions lack structured effect results

- `Priority`: `P1`
- `Size`: `xl`
- `Priority Rationale`: This is `P1` because the action pipeline is already the center of local play and future turn phases, reactions, logs, replay, and command unification will need a typed effect boundary. It is not `P0` because current local actions are functioning and the result shape can be expanded incrementally.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Actions, Turn Pipeline, Messages
- `Problem`: Local action handlers directly mutate broad runtime state, advance time themselves, and return only success/failure, elapsed ticks, and plain string messages. Follow-up systems such as automated turret fire infer effects by comparing elapsed ticks after mutation.
- `Why It Matters`: Future reactions, status updates, save/replay support, UI feedback, and campaign/local command unification will be fragile if the only durable outcome is hidden mutation plus strings.
- `Preferred Direction`: Add a structured action outcome/effect model for things like time advanced, actor moved, item moved, damage dealt, and message generated, while preserving current behavior during migration.
- `Resolution Path`: Start by adding typed effects beside existing messages for wait and movement. Then move automated-hazard follow-up logic to consume time/effect information rather than post-hoc elapsed-tick comparisons. Expand the same outcome shape across other local actions before using it for world/campaign commands.
- `Next Action`: Add structured effects to the smallest local action family, likely wait and move, without changing current messages or tick costs.
- `Resolved When`: Local action results expose structured effects that UI and follow-up systems can consume, automated hazard reactions no longer depend only on elapsed-tick comparison after mutation, and current player-facing messages remain intact.
- `Notes`:
- `Links`: `src/SurvivalGame.Domain/Actions/GameActionPipeline.cs`, `src/SurvivalGame.Domain/Actions/GameActionTypes.cs`, `src/SurvivalGame.Domain/Actions/GameActionContext.cs`, `src/SurvivalGame.Domain/Actions/NpcCombatService.cs`

### ARCH-3 - PrototypeGameState has become real runtime state

- `Priority`: `P2`
- `Size`: `l`
- `Priority Rationale`: This is important but less urgent than `ARCH-2` because renaming/reshaping the state should build on the new application/session owner and the future command boundary. The name is misleading now, but the rename should happen with clear runtime ownership rather than as a cosmetic pass.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Local Runtime State, Naming
- `Problem`: `PrototypeGameState` now represents real local-site runtime state, but its prototype name and broad shape obscure its long-term role.
- `Why It Matters`: New systems may continue to depend on a temporary concept, making later save/load, site transitions, and runtime ownership harder to clarify.
- `Preferred Direction`: Rename and reshape it into an explicit local-site runtime state concept under the appropriate non-Godot ownership boundary.
- `Resolution Path`: Use the new application/session boundary as the owner context, then rename the type to a durable concept such as `LocalSiteRuntimeState`, keep `CampaignState` as the run root, and make local-site responsibilities explicit: local map state, player position, active travel anchor, local NPC/object/container state, and local ground items. Update tests around behavior and invariants rather than preserving old helper shapes.
- `Next Action`: Plan a focused rename/reshape slice that updates tests and callers to reflect the intended persistent campaign/local-site split.
- `Resolved When`: The old prototype state name is gone, local-site runtime state has a clear owner and responsibility boundary, and no production code depends on prototype naming for real gameplay state.
- `Notes`:
- `Links`: `src/Godot/Game/Prototype/`, `src/SurvivalGame.Domain/Actions/`, `src/SurvivalGame.Domain/Campaign/`

### ARCH-4 - Prototype-era project and folder ownership obscures boundaries

- `Priority`: `P2`
- `Size`: `xl`
- `Priority Rationale`: This is important because folder layout is starting to encode old implementation history instead of current ownership. It is `P2`, not `P1`, because the remaining fixes should follow real ownership moves, especially the future command boundary work, rather than standalone churn.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Project Structure, Domain Boundaries, Godot Presentation
- `Problem`: Several folders now mix concepts that are likely to separate as the prototype matures: prototype content facades, broad action orchestration, local map primitives plus local-site runtime concepts, player inventory plus generic containers, and Godot run shell/UI/rendering/application boot code.
- `Why It Matters`: Blurry ownership makes future code harder to place and encourages new systems to depend on historical folder names rather than real boundaries.
- `Preferred Direction`: Let folder moves follow real boundary changes. Keep application ownership in `src/SurvivalGame.Application/`, then split prototype, action, local-site, inventory, and Godot presentation areas only when code moves for a concrete reason.
- `Resolution Path`: Build on the application/session layer, keep the root project as the Godot presentation assembly, and split or rename prototype-era folders as code moves into clearer ownership boundaries.
- `Next Action`: Use the next boundary-changing slice, likely `ARCH-2` or `ARCH-3`, to remove one remaining prototype-era ownership ambiguity.
- `Resolved When`: Application/session construction has a clear non-Godot home, prototype-era folders no longer contain real runtime ownership by accident, and domain/Godot folders communicate their responsibilities without relying on old prototype names.
- `Notes`:
- `Links`: `src/SurvivalGame.Prototype/`, `src/SurvivalGame.Domain/Actions/`, `src/SurvivalGame.Domain/LocalMaps/`, `src/Godot/Game/`

### ARCH-5 - GameShell owns too many gameplay screen responsibilities

- `Priority`: `P2`
- `Size`: `xl`
- `Priority Rationale`: This is `P2` because `GameShell` is a maintainability pressure point, but extracting it is safer after the application/session and command boundaries stop pushing simulation coordination into Godot screens.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Godot UI, Local Gameplay Screen
- `Problem`: `GameShell` owns local UI construction, responsive layout, input handling, selection state, popup behavior, tooltip behavior, action filtering, action execution, map refresh, and overlay refresh.
- `Why It Matters`: A broad screen controller makes local gameplay changes risky and encourages UI, input, presentation state, and command execution to grow together.
- `Preferred Direction`: Extract shared layout and presentation helpers first, then let screen-specific controllers stay small and focused on input/display coordination.
- `Resolution Path`: Start with a reusable Godot-side layout helper or control for board, sidebar, and log sizing. Move reusable panels and item/action display behavior behind clearer UI components. After command execution is moved toward domain/application ownership, keep `GameShell` as a coordinator instead of a rule or layout hub.
- `Next Action`: Extract the smallest shared layout primitive used by local gameplay and suitable for world-map screen reuse, preserving current responsive behavior.
- `Resolved When`: Local gameplay layout, action display, selection/popup behavior, and map refresh are separated into focused helpers or controls, and `GameShell` no longer needs to directly own most local UI details.
- `Notes`:
- `Links`: `src/Godot/Game/GameShell.cs`, `src/Godot/Game/UI/`, `src/Godot/WorldMap/`

### ARCH-6 - Action presentation depends on request-type matching

- `Priority`: `P2`
- `Size`: `l`
- `Priority Rationale`: This is `P2` because it affects every new action, but it should follow or travel with command-pipeline work so the presentation model reflects the intended execution boundary.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Actions, UI Presentation
- `Problem`: UI code has to know too much about individual enum values and request types to decide whether an action is global, item-specific, contextual, hidden, enabled, or disabled.
- `Why It Matters`: Each new action increases UI branching and makes it easier for presentation behavior to drift away from domain validation.
- `Preferred Direction`: Available actions should expose enough domain/application-owned presentation metadata for UI placement and explanation while keeping action execution domain-owned.
- `Resolution Path`: Add action descriptors that carry scope, target reference, grouping, priority, enabled state, and disabled reason text. Update action menus and panels to consume descriptors instead of matching concrete request types. Migrate action families in small slices without changing action outcomes.
- `Next Action`: Add metadata for one existing action group and route one UI panel through it to prove the shape.
- `Resolved When`: UI action menus can render and group available actions from metadata, with minimal request-type switching and no loss of domain validation.
- `Notes`:
- `Links`: `src/SurvivalGame.Domain/Actions/`, `src/Godot/Game/UI/`, `src/Godot/Game/GameShell.cs`

### ARCH-7 - Cross-catalog content validation is scattered

- `Priority`: `P2`
- `Size`: `m`
- `Priority Rationale`: This is `P2` because content volume is already large enough for missing references to matter, but the current loaders work for the prototype and this can be added as a focused safety net.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Content, Data Validation, Tests
- `Problem`: Content loading validates individual files, but cross-file references are still validated in scattered places or only when a path is exercised.
- `Why It Matters`: Missing sprite ids, mismatched item/firearm definitions, unknown local site ids, invalid world-map POI references, and similar issues can survive until runtime or manual testing.
- `Preferred Direction`: Add one content-pack validation pass that loads committed content and validates cross-catalog references.
- `Resolution Path`: Create a registry/validator that loads items, firearms, world objects, NPCs, local maps, world map POIs, sprite ids, and action tags as one committed content pack. Add focused tests that fail on missing or mismatched references while keeping runtime content under `data/` for Godot `res://data/...` access.
- `Next Action`: Add a content validation test that loads the committed catalogs and checks every JSON `spriteId` resolves to an existing PNG.
- `Resolved When`: A single content-pack validation test covers the committed data set and catches missing ids, missing sprite files, bad local-site references, and mismatched item/firearm relationships before runtime.
- `Notes`:
- `Links`: `data/`, `src/SurvivalGame.Domain/Content/`, `tests/`

### ARCH-8 - Stack items and stateful items create parallel item paths

- `Priority`: `P2`
- `Size`: `xxl`
- `Priority Rationale`: This is `P2` because duplication will grow as gear gains condition, contents, ownership, and damage, but a broad item rewrite would be riskier than feature-driven migration.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Items, Inventory, Equipment, Cargo
- `Problem`: Stack-backed items and stateful items now require parallel handling across inventory, equipment, firearms, cargo, and UI.
- `Why It Matters`: Parallel paths make it easy for weapons, feed devices, fuel cans, tools, containers, worn gear, and damaged objects to behave inconsistently as more per-item state is added.
- `Preferred Direction`: Treat stateful item instances as the default for gear with identity, contents, condition, fuel, damage, ownership, or attachments, while reserving stacks for true commodities.
- `Resolution Path`: Define and document item identity rules, then migrate one feature family at a time. Keep stack inventory for loose ammunition, generic materials, and simple consumable quantities. Consolidate shared inventory/equipment/cargo/UI queries as stateful coverage expands.
- `Next Action`: Pick the next item family that already needs identity and remove one duplicate stack/stateful handling path around it.
- `Resolved When`: Stateful items cover weapons, feed devices, fuel cans, tools, containers, worn equipment, damaged objects, and other identity-bearing gear, while stack code is limited to commodity quantities.
- `Notes`:
- `Links`: `src/SurvivalGame.Domain/Items/`, `src/SurvivalGame.Domain/Inventory/`, `src/SurvivalGame.Domain/Equipment/`, `src/SurvivalGame.Domain/Firearms/`

### ARCH-12 - Scene flow is hard-coded across concrete screens

- `Priority`: `P2`
- `Size`: `m`
- `Priority Rationale`: This is `P2` because screen navigation is still small enough to work today, but save/load, continue, death, transition effects, and global back/escape behavior will become hard to coordinate if every screen owns its own flow decisions.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Godot Navigation, Screen Flow
- `Problem`: Concrete Godot screens know scene paths, call scene changes or instantiate other screens, and decide local/world/main-menu transitions directly.
- `Why It Matters`: Run-flow rules will spread across screens, making it harder to keep session lifetime, transition behavior, and future menu/gameplay modes consistent.
- `Preferred Direction`: Add a thin Godot-side scene/run-flow coordinator. Screens should emit intents such as start run, return to world map, enter local site, or exit to menu; the coordinator should own scene loading and transition wiring.
- `Resolution Path`: First centralize scene paths and the existing main-menu, world-map, and local-site transitions. Then move transition decisions out of individual screens while keeping the application/session boundary as the run-state owner.
- `Next Action`: Extract `ShowWorldMap`, `ShowLocalSite`, and main-menu scene path handling behind one Godot navigation coordinator.
- `Resolved When`: Main menu, world map, and local gameplay screens no longer directly load each other or own run-flow transitions, and the current start/enter/return behavior is unchanged.
- `Notes`:
- `Links`: `src/Godot/Game/GameSessionShell.cs`, `src/Godot/MainMenu/MainMenu.cs`, `src/Godot/Game/GameShell.cs`, `src/Godot/WorldMap/WorldMapScreen.cs`

### ARCH-13 - Godot UI panels build domain presentation directly

- `Priority`: `P2`
- `Size`: `l`
- `Priority Rationale`: This is `P2` because presentation duplication is already visible in inventory, tooltip, selected-item, and firearm panels, but it can be addressed one panel family at a time.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Godot UI, Presentation Models
- `Problem`: Godot UI controls query catalogs and runtime domain state, format item/weapon/details text, choose category colors, and expose runtime ids directly. Some of this duplicates domain formatting in `ItemDescriber`.
- `Why It Matters`: UI behavior becomes tightly coupled to simulation types and content details, especially around stack-backed versus stateful items, making future UI changes and domain refactors harder to keep consistent.
- `Preferred Direction`: Add presentation-model builders or formatters that return rows, labels, tooltip content, and action display metadata for Godot controls to render.
- `Resolution Path`: Start with selected-item details and hover tooltip rows, where duplication is most obvious. Keep Godot controls responsible for layout and styling, but move item/firearm/text assembly into a testable presentation boundary.
- `Next Action`: Extract shared item detail and tooltip row building from `SelectedItemPanel` and `ItemTooltip`.
- `Resolved When`: Inventory, selected-item, tooltip, and firearm panels render supplied presentation models with minimal catalog/rule branching in Godot controls, and item detail text stays consistent across UI surfaces.
- `Notes`:
- `Links`: `src/Godot/Game/UI/SelectedItemPanel.cs`, `src/Godot/Game/UI/ItemTooltip.cs`, `src/Godot/Game/UI/InventoryGridView.cs`, `src/Godot/Game/UI/FirearmPanel.cs`, `src/SurvivalGame.Domain/Actions/ItemDescriber.cs`

### ARCH-14 - Content root resolution is duplicated across runtime and tests

- `Priority`: `P2`
- `Size`: `m`
- `Priority Rationale`: This is `P2` because the project already loads the committed `data/` tree from Godot, prototype helpers, and tests through different path strategies, but a shared abstraction can be added without changing gameplay data.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Content Loading, Test Infrastructure, Application Boundary
- `Problem`: Godot runtime code resolves content through `ProjectSettings.GlobalizePath`, prototype helpers walk upward from process directories, and tests define their own committed-data path helpers.
- `Why It Matters`: CI runners, packaged builds, tools, and future application/session code can disagree about the content root or fail differently depending on working directory and execution context.
- `Preferred Direction`: Introduce a small content-root/content-pack path abstraction. Godot should adapt `res://data/...` once, and tests/tools should use the same root contract from normal filesystem paths.
- `Resolution Path`: First consolidate test path helpers around one shared committed-content root helper. Then route runtime and test content path composition through `GameContentPaths`, leaving Godot responsible only for adapting `res://` to that boundary.
- `Next Action`: Add a shared test content-root helper and route the most repeated catalog loader tests through it.
- `Resolved When`: Runtime loading, prototype helpers, tests, and tools use one content-root contract, with Godot-specific `res://` resolution isolated to a thin adapter.
- `Notes`:
- `Links`: `src/Godot/Game/GodotSessionFactory.cs`, `src/SurvivalGame.Application/GameContentPaths.cs`, `src/SurvivalGame.Prototype/PrototypeWorldMapSites.cs`, `tests/SurvivalGame.Domain.Tests/LocalMaps/LocalSiteDefinitionLoaderTests.cs`, `tests/SurvivalGame.Domain.Tests/Firearms/FirearmSystemTests.cs`

### ARCH-18 - Keep tile-wall authoring compatible with future TileMap tooling

- `Priority`: `P2`
- `Size`: `s`
- `Priority Rationale`: This is `P2` because procedural C# walls are the right immediate path, but Godot TileMap/Terrain or TileSet-based authoring may become a better editor/asset workflow once the wall model settles.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Local Map Rendering, Authoring Workflow, Godot TileMap
- `Problem`: The current tile-wall look is tied to procedural drawing in `MapEntityLayer`, while future Godot TileMap/Terrain tooling would need the same wall kind, material, and neighbor-mask information expressed as reusable render data.
- `Why It Matters`: If map data, gameplay semantics, and procedural draw geometry become too entangled, switching to TileMap/Terrain later would require another wall migration instead of just a renderer/asset workflow change.
- `Preferred Direction`: Treat wall kind, tile occupancy, neighbor masks, doorway state, and visual material as the stable data/model layer. Let procedural drawing remain the first renderer, with TileMap/Terrain as a later renderer or authoring adapter over the same model.
- `Resolution Path`: Build on `TileWallRenderModel`, then document the minimum wall-render data a TileMap/Terrain adapter would need. Defer actual TileMapLayer/TileSet work until there is a concrete asset/editor task.
- `Next Action`: Add a short wall-render model note or README section that names wall kinds, neighbor masks, material/style fields, and doorway-state needs without committing to TileMap implementation yet.
- `Resolved When`: The 2.5D wall system has documented renderer-agnostic inputs that can drive the current procedural draw path and could drive a future TileMap/Terrain implementation without changing authored local-map JSON.
- `Notes`:
  - 2026-04-30: This item exists to preserve the future Godot tile/terrain option while still committing to 2.5D tile walls as the near-term wall direction.
  - 2026-04-30: `ARCH-16` extracted `TileWallRenderModel`, so the future adapter can start from existing wall kind, neighbor mask, orientation, bounds, and sort-contact data rather than reverse-engineering `MapEntityLayer` draw code.
- `Links`: `src/Godot/Game/LocalMapView/MapEntityLayer.cs`, `docs/ARCHITECTURE.md`, `data/local_maps/`

### ARCH-20 - Data-drive tile-wall render classification

- `Priority`: `P2`
- `Size`: `m`
- `Priority Rationale`: This is `P2` because the current hard-coded tile-wall id mapping is small and safe, but it will become fragile as wall materials, door states, and site-specific wall variants expand.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Local Map Rendering, Content Definitions, World Objects
- `Problem`: `TileWallRenderModel` currently maps specific world-object ids such as `wall`, `window`, `wooden_door`, and `glass_door` to tile-wall render kinds in code.
- `Why It Matters`: Hard-coded id-to-render-kind mapping will make future wall content awkward, especially if farmhouse wood walls, gas station concrete walls, brick walls, boarded windows, open/closed doors, and lockable doors need distinct authored ids but still share the same renderer behavior.
- `Preferred Direction`: Keep the renderer's closed set of behaviors in code as a typed model, but move the content-id mapping into data-backed world-object metadata such as wall role, material/style, doorway role, and passability/state hooks.
- `Resolution Path`: Add a small wall-render metadata shape to world-object definitions and loaders, map that metadata into the existing `TileWallKind` or successor render model, and update committed wall/window/door object definitions to use metadata instead of hard-coded ids. Keep local-map JSON placement semantics unchanged.
- `Next Action`: Design the smallest world-object metadata field that can classify existing `wall`, `window`, `wooden_door`, and `glass_door` objects without changing visuals.
- `Resolved When`: Tile-wall render classification comes from validated content metadata rather than a hard-coded id switch, current sites render unchanged, and tests/content validation catch unknown wall render roles or missing material references.
- `Notes`:
  - 2026-04-30: The enum itself is acceptable as renderer code because it describes a closed drawing strategy. The architectural pressure is the hard-coded id mapping, not the existence of a typed render-kind enum.
  - 2026-04-30: This should coordinate with `MECH-11` for interactive door states and `MECH-13` for material/style variants so the metadata does not become a throwaway visual-only field.
- `Links`: `src/Godot/Game/LocalMapView/TileWallRenderModel.cs`, `data/world_objects/`, `src/SurvivalGame.Domain/WorldObjects/`, `docs/MECHANICS_BACKLOG.md`

### ARCH-9 - Renderer classes are large integration hubs

- `Priority`: `P3`
- `Size`: `xl`
- `Priority Rationale`: This is `P3` because renderer size is real debt, but the old roadmap explicitly deferred renderer splitting until state, command, and layout boundaries are cleaner.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Godot Rendering, Map Views
- `Problem`: Renderer classes mix asset loading, render-model construction, geometry, draw ordering, fallback drawing, and input conversion.
- `Why It Matters`: Rendering changes become harder to reason about when asset concerns, domain-to-view transformation, draw details, and pointer conversion all live in the same integration hubs.
- `Preferred Direction`: Split renderers after state and command boundaries are cleaner, starting with asset caching and render-model construction.
- `Resolution Path`: Extract asset caching first. Then add render-model builders that convert domain state into draw commands. Finally split local entity drawing by surfaces, structures, world objects, NPCs, player marker, and fallback sprites only after shared layout and application boundaries settle.
- `Next Action`: Wait until `ARCH-5` has reduced screen/layout pressure, then extract one asset cache or render-model builder without changing visual output.
- `Resolved When`: Map view controls draw prepared render commands, asset loading is isolated, input conversion is separate from draw ordering, and visual behavior remains unchanged.
- `Notes`:
- `Links`: `src/Godot/Game/LocalMapView/`, `src/Godot/WorldMap/`

## Archive

### ARCH-11 - Local map query rules are reimplemented per feature

- `Priority`: `P2`
- `Size`: `m`
- `Priority Rationale`: This was `P2` because rule duplication already touched movement, interaction, travel-anchor access, and firearm targeting, but could be resolved in behavior-preserving slices.
- `Status`: `Resolved`
- `Detected`: 2026-04-30
- `Resolved`: 2026-04-30
- `Area`: Local Maps, Movement, Interaction, Targeting
- `Problem`: Movement blocking, walkability, adjacency, object footprint checks, structure-edge blocking, and line-of-fire blocking were implemented separately across action handlers and map services.
- `Resolution`: Added domain-owned `LocalMapQuery` for movement blockers, standing blockers, nearby world-object placement lookup, placement proximity, and line-of-fire sight blocking. Movement, travel-anchor entry, container adjacency, travel cargo/fuel proximity, and firearm targeting now use the shared query boundary.
- `Resolved When`: Movement, interaction adjacency, travel-anchor access, and line-of-fire validation use one shared query boundary for map occupancy/blocking rules, with focused tests for the current behavior.
- `Notes`:
  - 2026-04-30: The query distinguishes movement from standability so tile occupancy affects both while NPCs can choose whether they block movement.
  - 2026-04-30: `ARCH-19` later removed the structure-edge branch from the query; current blockers are map bounds, world objects, and NPCs.
  - 2026-04-30: NPC movement, pathfinding, scheduling, initiative, and new gameplay behavior were intentionally left out.
- `Links`: `src/SurvivalGame.Domain/LocalMaps/LocalMapQuery.cs`, `src/SurvivalGame.Domain/Actions/MovementHandler.cs`, `src/SurvivalGame.Domain/Actions/InteractHandler.cs`, `src/SurvivalGame.Domain/Actions/TravelCargoHandler.cs`, `src/SurvivalGame.Domain/LocalMaps/TravelAnchorService.cs`, `tests/SurvivalGame.Domain.Tests/LocalMaps/LocalMapQueryTests.cs`

### ARCH-1 - Application/session bootstrapping lives in Godot prototype code

- `Priority`: `P1`
- `Size`: `l`
- `Priority Rationale`: This was a high-value next refactor because several other architecture improvements needed a non-Godot place to land first. It was not `P0` because the game still ran and the debt could be resolved incrementally.
- `Status`: `Resolved`
- `Detected`: 2026-04-30
- `Resolved`: 2026-04-30
- `Area`: Application, Godot Adapter
- `Problem`: Session construction, content loading, campaign creation, local site loading, action pipeline setup, and starting item seeding were concentrated in Godot-facing bootstrapping code.
- `Resolution`: Added `SurvivalGame.Application` with `GameContentPaths`, `GameSessionFactory`, `CampaignSession`, and `LocalSiteSession`. Godot now uses `GodotSessionFactory` only to resolve `res://data` to filesystem paths and delegate run/session construction.
- `Resolved When`: Godot-facing code no longer owns run construction rules, session creation is tested without Godot APIs, and the current new-run/local-site flows remain unchanged.
- `Notes`:
- `Links`: `src/SurvivalGame.Application/`, `src/Godot/Game/GodotSessionFactory.cs`, `tests/SurvivalGame.Application.Tests/`

### ARCH-15 - 2.5D tile walls need canonical wall ownership

- `Priority`: `P1`
- `Size`: `xl`
- `Priority Rationale`: This was `P1` because the project needed a clear building-wall direction before cleaning up the older edge-wall experiment.
- `Status`: `Superseded`
- `Detected`: 2026-04-30
- `Superseded`: 2026-04-30
- `Area`: Local Maps, Structures, World Objects
- `Problem`: Procedural full-block 2.5D tile walls became the preferred building-wall look while data, docs, movement rules, hover behavior, line-of-fire, and renderer compatibility still reflected the older edge-wall experiment.
- `Resolution`: The architecture decision is now made: procedural 2.5D tile-object walls are the near-term canonical representation for building walls and windows. The older edge-structure system was removed by `ARCH-19`.
- `Superseded By`: `ARCH-16` for extracting the tile-wall render model, `ARCH-17` for retiring authored edge-structure usage, `ARCH-18` for preserving future Godot TileMap/Terrain compatibility, `ARCH-19` for deleting unused edge-structure code paths, `MECH-11` for interactive doors, `MECH-12` for 2.5D fence/gate/gap visuals, and `MECH-13` for tile-wall material variants.
- `Notes`:
  - 2026-04-30: Abandoned Farmhouse building walls were converted to tile-world-object walls for visual inspection.
  - 2026-04-30: Edge-to-tile conversion can collide with nearby furnishings and former doorway cells. The farmhouse pass moved several objects, including one cabinet into a former doorway sample, so future canonical door semantics need explicit placement rules instead of inferred edge-to-cell mapping.
  - 2026-04-30: The converted farmhouse data has no remaining building structure edges, 156 wall tiles, and 8 window tiles; this makes the visual test clear but also means door openings currently have no authored object identity.
  - 2026-04-30: Shared ids such as `wall` and `window` previously existed in both world-object and structure catalogs. `ARCH-19` removed the structure catalog, so renderer precedence no longer needs edge-structure compatibility behavior.
  - 2026-04-30: User direction is that 2.5D tile walls are the wall path for now; edge walls were visually disliked and should be cleaned out rather than promoted as the main system.
  - 2026-04-30: `ARCH-17` later removed all authored edge structures, including the paddock fence/gate/gap edges. `ARCH-19` then deleted the unused edge-structure code path. Restoring those boundaries is now `MECH-12` tile-based world-object work, not edge-structure work.
- `Links`: `data/local_maps/`, `data/world_objects/`, `src/SurvivalGame.Domain/WorldObjects/`, `src/Godot/Game/LocalMapView/MapEntityLayer.cs`

### ARCH-16 - Extract tile-wall render model from MapEntityLayer

- `Priority`: `P1`
- `Size`: `m`
- `Priority Rationale`: This was `P1` because tile walls became the building-wall direction, and their kind detection, neighbor masks, render bounds, and geometry needed a stable home before more wall content or door state builds on them.
- `Status`: `Resolved`
- `Detected`: 2026-04-30
- `Resolved`: 2026-04-30
- `Area`: Godot Rendering, Local Map View
- `Problem`: `MapEntityLayer` directly owned tile-wall id detection, neighbor connection masks, render bounds, 2.5D wall geometry, window/door detail drawing, and unrelated entity drawing.
- `Resolution`: Added Godot-local `TileWallRenderModel` to build tile-wall render data: wall kind, adjacent-wall neighbor mask, full-block footprint geometry, raised render bounds, orientation, and sort-floor contact. `MapEntityLayer` now consumes that model and keeps the actual procedural drawing commands.
- `Resolved When`: `MapEntityLayer` no longer owns tile-wall classification or geometry directly, current prototype/gas/farmhouse wall visuals are unchanged, and the extracted model can be reused by door visuals and a future TileMap/Terrain adapter.
- `Notes`:
  - 2026-04-30: The helper intentionally remains in `src/Godot/Game/LocalMapView/` because it uses `Rect2`, `Vector2`, viewport offsets, and pixel cell sizes; it is not a domain contract.
  - 2026-04-30: Tile-wall world objects classify directly through the tile-wall render model; `ARCH-19` later removed edge-structure duplicate suppression entirely.
- `Links`: `src/Godot/Game/LocalMapView/TileWallRenderModel.cs`, `src/Godot/Game/LocalMapView/MapEntityLayer.cs`, `src/Godot/Game/LocalMapView/README.md`

### ARCH-17 - Retire authored edge-structure usage

- `Priority`: `P1`
- `Size`: `m`
- `Priority Rationale`: This was `P1` because the project chose tile-object 2.5D walls as the near-term wall direction, and leaving authored edge structures in committed content kept the old representation visually and architecturally alive.
- `Status`: `Resolved`
- `Detected`: 2026-04-30
- `Resolved`: 2026-04-30
- `Area`: Local Maps, Structures, Content
- `Problem`: The Abandoned Farmhouse still authored paddock fences, gates, and broken gaps as structure edges, and the committed structure catalog still described the retired edge-wall/fence vocabulary.
- `Resolution`: Removed committed structure definitions and all authored `structureEdges` from local maps. Farmhouse/shed building walls and windows remain tile-world-object walls, and former paddock fence/gate/gap crossings are temporarily open until tile-based replacements are added through `MECH-12`.
- `Resolved When`: Committed maps have zero `structureEdges`, farmhouse building walls/windows remain tile world objects, and tests cover the temporary absence of paddock fence/gate/gap objects until `MECH-12`.
- `Notes`:
  - 2026-04-30: `ARCH-19` later removed `StructureEdgeMap`, structure loaders, movement edge checks, line-of-fire edge checks, renderer branches, and related in-memory tests.
  - 2026-04-30: Former paddock fence/gate/gap collision and visuals are intentionally absent until `MECH-12` restores them as tile-based 2.5D world objects.
  - 2026-04-30: Useful former style ids to carry forward into tile-based wall/fence work include `generic_interior`, `farmhouse_weatherboard`, `shed_corrugated`, `gas_station_plaster`, `wire_fence`, and `timber_fence`.
  - 2026-04-30: Useful former piece/detail concepts include solid wall, closed wooden door, open wooden door, interior doorway threshold, torn flyscreen/screen door, glass door, window, broken window, boarded window, open farm gate, and broken fence gap.
- `Links`: `data/local_maps/`, `data/world_objects/`, `src/Godot/Game/LocalMapView/MapEntityLayer.cs`, `docs/MECHANICS_BACKLOG.md`

### ARCH-19 - Delete unused edge-structure code paths

- `Priority`: `P2`
- `Size`: `l`
- `Priority Rationale`: This was `P2` because committed content no longer used edge structures, but removing the legacy code path touched loaders, local-site state, movement, line-of-fire, renderer code, and tests.
- `Status`: `Resolved`
- `Detected`: 2026-04-30
- `Resolved`: 2026-04-30
- `Area`: Local Maps, Movement, Targeting, Rendering
- `Problem`: `StructureEdgeMap`, structure definition loaders/catalogs, structure renderer branches, movement edge checks, line-of-fire edge checks, hover hooks, and test fixtures remained after authored edge-structure usage was removed.
- `Resolution`: Deleted the edge-structure domain/catalog/render path, removed structure state from local maps/actions/application sessions, removed structure hover/rendering from Godot, and made authored local-map JSON reject legacy `structureEdges` with a clear migration message.
- `Resolved When`: No production code or tests depend on edge-structure maps/catalogs/loaders/rendering, committed content has no structure definitions or edge placements, and tile-world-object walls plus future tile-based fence replacement remain the active direction.
- `Notes`:
  - 2026-04-30: `MECH-12` remains the player-facing replacement path for farmhouse paddock fences, gates, and broken gaps as tile-based 2.5D world objects.
  - 2026-04-30: Former style/detail vocabulary is preserved in `MECH-12`/`MECH-13`, including farmhouse weatherboard, shed corrugated metal, gas-station plaster, wire/timber fences, open gates, broken fence gaps, torn flyscreen doors, and broken/boarded windows.
- `Links`: `src/SurvivalGame.Domain/LocalMaps/LocalMapQuery.cs`, `src/SurvivalGame.Domain/Content/LocalSiteDefinitionLoader.cs`, `src/Godot/Game/LocalMapView/MapEntityLayer.cs`, `docs/MECHANICS_BACKLOG.md`

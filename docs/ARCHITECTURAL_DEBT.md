# Architectural Debt Tracker

This is the living index for architecture debt and improvement opportunities that future AI coding sessions should keep visible. It tracks pressure points in the codebase, not new gameplay scope.

Use stable IDs when discussing or working on these items, such as `ARCH-1`. Do not renumber existing items. When an item is resolved or superseded, move it to the archive section and keep its ID intact.

## How To Maintain This Tracker

- Add an item when architecture debt is important enough that future sessions should remember it across tasks.
- Update an item when related work changes its risk, priority, next action, or status.
- Mark an item `Active` when current work is directly addressing it.
- Mark an item `Resolved` only when the architectural pressure is actually removed.
- Mark an item `Superseded` when another `ARCH-*` item or implemented direction replaces it.
- Keep active items ordered by priority first, then ID.
- Keep this tracker compact. Link to deeper plans, docs, or code areas instead of copying long analysis here.
- Do not treat any item here as permission to expand current game scope.

## Priority And Status

Priorities:

- `P0`: Urgent blocking architectural risk.
- `P1`: Next high-value refactor.
- `P2`: Planned or important improvement.
- `P3`: Watchlist.

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

Each active item should include enough direction that a future session can choose a small, behavior-preserving slice without rediscovering the whole problem. `Priority Rationale` explains the rating, `Resolution Path` describes the likely sequence, and `Resolved When` describes the signal for moving the item to the archive.

### ARCH-1 - Application/session bootstrapping lives in Godot prototype code

- `Priority`: `P1`
- `Priority Rationale`: This is a high-value next refactor because several other architecture improvements need a non-Godot place to land first. It is not `P0` because the current game still runs and the debt can be resolved incrementally.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Application, Godot Prototype
- `Problem`: Session construction, content loading, campaign creation, local site loading, action pipeline setup, and starting item seeding are still concentrated in Godot-facing prototype bootstrapping code.
- `Why It Matters`: As the run model grows, application ownership will become harder to test and Godot scripts will keep accumulating simulation setup responsibilities.
- `Preferred Direction`: Introduce a non-Godot application/session layer that owns prototype run construction while Godot remains an adapter for paths, scenes, input, and rendering.
- `Resolution Path`: First create a plain C# application/session boundary that can build the current prototype run from normal filesystem paths. Move catalog loading, campaign creation, local map loading, action pipeline wiring, and starting inventory setup into that boundary. Leave a thin Godot adapter responsible only for resolving `res://` paths and handing the resulting session to scenes.
- `Next Action`: Add an application project or equivalent plain C# session factory and migrate bootstrapping behind a thin Godot adapter.
- `Resolved When`: Godot-facing prototype code no longer owns run construction rules, session creation can be tested without Godot APIs, and the current new-run flow still starts on the world map and enters local sites unchanged.
- `Links`: `src/Godot/Game/PrototypeSessionFactory.cs`, `src/Godot/Game/Prototype/`, `src/SurvivalGame.Domain/`

### ARCH-2 - World travel bypasses the command pipeline

- `Priority`: `P1`
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
- `Links`: `src/Godot/WorldMap/`, `src/SurvivalGame.Domain/WorldMap/`, `src/SurvivalGame.Domain/Actions/`

### ARCH-3 - PrototypeGameState has become real runtime state

- `Priority`: `P2`
- `Priority Rationale`: This is important but less urgent than `ARCH-1` and `ARCH-2` because renaming/reshaping the state is safest after the application/session owner is clearer. The name is misleading now, but the boundary can wait until the first two items create better structure around it.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Local Runtime State, Naming
- `Problem`: `PrototypeGameState` now represents real local-site runtime state, but its prototype name and broad shape obscure its long-term role.
- `Why It Matters`: New systems may continue to depend on a temporary concept, making later save/load, site transitions, and runtime ownership harder to clarify.
- `Preferred Direction`: Rename and reshape it into an explicit local-site runtime state concept under the appropriate non-Godot ownership boundary.
- `Resolution Path`: Wait until `ARCH-1` gives the project a clearer application/session owner. Then rename the type to a durable concept such as `LocalSiteRuntimeState`, keep `CampaignState` as the run root, and make local-site responsibilities explicit: local map state, player position, active travel anchor, local NPC/object/container state, and local ground items. Update tests around behavior and invariants rather than preserving old helper shapes.
- `Next Action`: Rename only after the application/session boundary is clearer, then update tests and callers to reflect the intended persistent campaign/local-site split.
- `Resolved When`: The old prototype state name is gone, local-site runtime state has a clear owner and responsibility boundary, and no production code depends on prototype naming for real gameplay state.
- `Links`: `src/Godot/Game/Prototype/`, `src/SurvivalGame.Domain/Actions/`, `src/SurvivalGame.Domain/Campaign/`

### ARCH-4 - Prototype-era project and folder ownership obscures boundaries

- `Priority`: `P2`
- `Priority Rationale`: This is important because folder layout is starting to encode old implementation history instead of current ownership. It is `P2`, not `P1`, because the safest fixes should happen alongside real ownership moves from `ARCH-1` and `ARCH-2`, not as standalone churn.
- `Status`: `Open`
- `Detected`: 2026-04-30
- `Area`: Project Structure, Domain Boundaries, Godot Presentation
- `Problem`: Several folders now mix concepts that are likely to separate as the prototype matures: prototype content facades, broad action orchestration, local map primitives plus local-site runtime concepts, player inventory plus generic containers, and Godot run shell/UI/rendering/application boot code.
- `Why It Matters`: Blurry ownership makes future code harder to place and encourages new systems to depend on historical folder names rather than real boundaries.
- `Preferred Direction`: Let folder moves follow real boundary changes. Add application ownership first, then split prototype, action, local-site, inventory, and Godot presentation areas only when code moves for a concrete reason.
- `Resolution Path`: Introduce `src/SurvivalGame.Application/` as part of `ARCH-1`, keep the root project as the Godot presentation assembly, add application tests beside the new project, and split or rename prototype-era folders as code is moved into clearer ownership boundaries.
- `Next Action`: When `ARCH-1` begins, include project/folder ownership cleanup in that slice rather than leaving bootstrapping in `src/Godot/Game/`.
- `Resolved When`: Application/session construction has a clear non-Godot home, prototype-era folders no longer contain real runtime ownership by accident, and domain/Godot folders communicate their responsibilities without relying on old prototype names.
- `Links`: `src/SurvivalGame.Prototype/`, `src/SurvivalGame.Domain/Actions/`, `src/SurvivalGame.Domain/LocalMaps/`, `src/Godot/Game/`

### ARCH-5 - GameShell owns too many gameplay screen responsibilities

- `Priority`: `P2`
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
- `Links`: `src/Godot/Game/GameShell.cs`, `src/Godot/Game/UI/`, `src/Godot/WorldMap/`

### ARCH-6 - Action presentation depends on request-type matching

- `Priority`: `P2`
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
- `Links`: `src/SurvivalGame.Domain/Actions/`, `src/Godot/Game/UI/`, `src/Godot/Game/GameShell.cs`

### ARCH-7 - Cross-catalog content validation is scattered

- `Priority`: `P2`
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
- `Links`: `data/`, `src/SurvivalGame.Domain/Content/`, `tests/`

### ARCH-8 - Stack items and stateful items create parallel item paths

- `Priority`: `P2`
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
- `Links`: `src/SurvivalGame.Domain/Items/`, `src/SurvivalGame.Domain/Inventory/`, `src/SurvivalGame.Domain/Equipment/`, `src/SurvivalGame.Domain/Firearms/`

### ARCH-9 - Renderer classes are large integration hubs

- `Priority`: `P3`
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
- `Links`: `src/Godot/Game/LocalMapView/`, `src/Godot/WorldMap/`

## Archive

No archived items yet.

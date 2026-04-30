# AGENTS.md

Guidance for future Codex and AI coding sessions in this repository.

## Project Vision

This is an early Godot 4 .NET / C# project for a 2D turn-based open-world survival roguelike. The long-term direction is systemic depth inspired by games like Caves of Qud, paired with a modern, readable interface.

The current priority is a clean foundation. Do not add large gameplay systems unless the task explicitly asks for them.

## Creative North Star

The long-term game is an open sandbox survival roguelike set after a recent infrastructure collapse in the United States. The initial world should be a scaled-down but logistically believable Colorado using real map names, with a long-term possibility of a much larger nationwide journey.

Survival is driven by resource logistics, grounded tactical combat, and local-site risk. The player is currently assumed to be solo, but future party or crew systems are possible. The mobile base is a core fantasy, but "base" starts as travel capacity and continuity: carried gear, knowledge, stash, bike trailer, vehicle, bus, truck, or other platform. It should not imply that a car is mandatory from the start.

Use `docs/BACKGROUND.md` as soft setting and tone guidance, and `docs/DESIGN_GOALS.md` as long-term system direction. These docs should guide naming, content, and implementation choices, but they do not authorize expanding current scope. The scope guardrails below and `docs/CURRENT_SCOPE.md` remain the source of truth for what is implemented now.

For content, tone, worldbuilding, naming, site, item, NPC, sprite, or environmental-storytelling work, read `docs/BACKGROUND.md` and `docs/WORLD_AUTHORING_GUIDE.md` before choosing content direction. `docs/WORLD_AUTHORING_GUIDE.md` owns reusable content templates, authoring checks, and maintenance rules for the world background docs. These world docs are inspirational guidance only and do not authorize unrequested gameplay systems or content scope.

`docs/DESIGN_GOALS.md` is a living end-state design document. Follow its AI planning protocol when work involves high-impact design choices, early slices of future systems, or meaningful tradeoffs between a locally convenient implementation and the intended long-term game. Surface meaningful alternatives and ask focused questions before locking plans when the answer would affect long-term direction, architecture, scope, or player-facing system shape.

Record durable future game-design direction in `docs/DESIGN_GOALS.md`. Record future architecture pressure in `docs/ARCHITECTURAL_DEBT.md`. Record deferred player-facing mechanics and systems in `docs/MECHANICS_BACKLOG.md`. Do not use any of these documents as permission to add unrequested gameplay scope.

## Core Architecture

Godot is the presentation, input, UI, rendering, audio, and scene composition layer.

C# domain code owns the game simulation.

Separate pure or mostly pure C# simulation logic from Godot scene/node/UI/rendering logic wherever practical. Simulation systems should not be deeply coupled to Godot nodes unless there is a clear reason.

Godot should display and interact with simulation state. It should not become the primary source of truth for the simulation.

## Repository Layout

- `docs/` contains project documentation.
- `data/` contains runtime game content definitions such as JSON item data. Keep it at the Godot project root so it is addressable through `res://data/...`.
- `src/Godot/` contains Godot scenes, controls, rendering, input, and UI scripts, grouped by feature.
- `src/Godot/MainMenu/` contains the main menu scene and menu script.
- `src/Godot/WorldMap/` contains the broad world map travel screen and map drawing control.
- `src/Godot/Game/` contains the current playable prototype shell.
- `src/Godot/Game/LocalMapView/` contains visible map/grid/input-view scripts.
- `src/Godot/Game/UI/` contains gameplay overlay controls.
- `src/Godot/Game/Prototype/` contains temporary Godot-side helpers that should migrate to domain code when the matching simulation concept becomes real.
- `src/SurvivalGame.Application/` contains plain C# application/session bootstrapping, content path wiring, and run session construction.
- `src/SurvivalGame.Domain/` contains plain C# simulation/domain code, grouped by concept.
- `src/SurvivalGame.Domain/WorldMap/` contains broad world map travel, positions, points of interest, and travel rules.
- `src/SurvivalGame.Domain/Actors/` contains player, NPC, creature, and actor state models.
- `src/SurvivalGame.Domain/LocalMaps/` contains local map, grid, terrain, chunk, and position logic.
- `src/SurvivalGame.Domain/Items/` contains item ids, definitions, catalogs, type paths, and placed item stacks.
- `src/SurvivalGame.Domain/Inventory/` contains inventory data structures and inventory rules.
- `src/SurvivalGame.Domain/Content/` contains loaders/adapters for data files.
- `tests/` contains automated tests, especially for domain code. Domain test folders should mirror `src/SurvivalGame.Domain/`.

## Simulation And Domain Code

The following should generally live in plain C# classes, records, structs, services, or systems rather than Godot nodes by default:

- World model.
- Chunks.
- Terrain data.
- Tile definitions.
- Entity state.
- Actors.
- Player state.
- NPC and creature state.
- Turn resolution.
- Action validation.
- Action resolution.
- Inventory data.
- Item definitions.
- Body model.
- Injuries.
- Status effects.
- Hunger, thirst, exposure, and fatigue.
- Weather state.
- Procedural generation logic.
- Pathfinding.
- AI decision logic.
- Save/load data models.
- Event and message generation.
- Simulation rules.

These systems should be testable without instantiating Godot scenes where practical.

## Godot Scenes And Nodes

The following belong in Godot scenes, nodes, controls, and presentation scripts:

- Scene composition.
- Visual layout.
- UI panels.
- Input capture.
- Rendering the grid/local map view.
- Tile visualisation.
- Animation.
- Audio.
- Menus.
- Buttons.
- Inspector panels.
- Message log display.
- Highlighting, hover, and selection display.
- Converting player input into simulation action requests.

Godot nodes may coordinate and present simulation state, but canonical gameplay state should live in the simulation layer.

## Data-Driven Design

Prefer data definitions over hard-coded bespoke logic where sensible. This especially applies to:

- Item definitions.
- Terrain definitions.
- Creature definitions.
- Injury definitions.
- Status effect definitions.
- Action definitions.
- Crafting recipes later, if crafting is explicitly added.

Data can initially be simple C# definitions, JSON, or Godot resources. Avoid scattering content rules across UI scripts.

## Sprite And Visual Asset Creation

Sprites are gameplay-facing readability assets, not standalone illustrations. When creating or editing sprites for surfaces, items, NPCs, world objects, or edge-based structures, make them usable in the current local map before making them decorative.

Before creating a sprite:

- Decide the content id, display name, category, map color, movement/sight blocking, and simulation footprint first.
- Match the sprite to the data-defined thing. If an object is specifically a `single_bed`, `fuel_pump`, or `tractor_wreck`, the id, sprite id, map references, tests, and player-facing text should use that specific concept rather than a vague generic name.
- Use the existing sprite id pattern: `surface_<id>`, `item_<id>`, `npc_<id>`, or `world_object_<id>`.
- Treat the footprint as gameplay truth. A sprite can visually overflow a tile through `spriteRender`, but collision, hover, targeting, and placement must still match the intended footprint.
- Decide whether the sprite's canonical north-facing shape or a particular placed orientation defines the footprint. Store the canonical footprint on the definition, then use placement `facing` for rotated map instances. For example, a north-facing `2 x 3` vehicle can be placed east-facing to occupy an effective `3 x 2` area.
- Building walls and windows should generally use the current procedural 2.5D tile-object wall path. Do not add `structureEdges`; that old edge-structure system has been removed. Fences, gates, and broken gaps should return as tile-based 2.5D world objects rather than edge structures. Tile world objects are also for things occupying tile area, such as furniture, trees, vehicles, tanks, machinery, and clutter.

Sprite art direction:

- Use transparent PNGs with no background, frame, grid, text, watermark, UI decoration, or baked-in scene context.
- Use a readable top-down or slightly top-down orthographic view that fits the current local map. Avoid isometric, side-view, cinematic, or heavily perspective art unless the renderer and surrounding assets explicitly support it.
- Keep silhouettes strong at small sizes. The object should still read when displayed around a 32px tile scale.
- Prefer clear, grounded shapes and material cues over tiny high-frequency detail.
- Keep lighting simple and consistent. Avoid dramatic cast shadows, colored glows, and scene-specific lighting that will clash on different surfaces.
- Do not bake large soft shadows into sprites unless the existing asset type already does so consistently.

Footprints, scale, and orientation:

- Make the canonical sprite face north/up by default. Use object placement `facing` to rotate long objects when needed.
- Express footprints in map coordinates: `width` is east-west tiles, `height` is north-south tiles. A north-facing single bed is `width: 1, height: 2`; an east-facing placement of the same object occupies `2 x 1`.
- If the visible sprite should fill its physical footprint, set the data footprint and let the renderer size the sprite from it.
- If the sprite should visually extend beyond its physical tile, keep the footprint small and add `spriteRender` metadata for visual-only size, offset, and sort offset.
- Do not use transparent padding to fake positioning or scale. Use `spriteRender` offsets instead.
- Do not add edge-structure sprites. Current building walls use procedural 2.5D tile-object rendering; keep future wall, fence, gate, and gap assets compatible with tile-world-object rendering unless a specific task changes the representation.

Generated sprite workflow:

- For generated sprites, ask for a flat removable chroma-key background unless true transparency is available in the chosen workflow. The background must be uniform, with no shadow, gradient, floor plane, texture, reflection, or lighting variation.
- Keep generated originals outside project data unless they are intentionally source assets. Copy only the selected working image into `tmp/imagegen/` or another scratch area, then save the final transparent sprite under `data/sprites/...`.
- Remove the chroma key locally, then inspect for leftover key-colored edge pixels. If green or magenta fringe remains, retry with a tighter matte or manually clear only the remaining near-transparent key pixels.
- Trim first, then downscale or optimize. Downscaling before trimming can blur chroma edges into the subject and make cleanup harder.
- View the final sprite on transparency before wiring it into data. A sprite that looks good on a solid background can still have bad edge spill or unusable silhouette at map scale.

Asset hygiene:

- Trim transparent padding tightly after generation or editing, leaving only the pixels needed for antialiasing.
- After trimming, verify the alpha bounding box starts at or very near the image edges, the corners are transparent, and the image has an alpha channel.
- If renaming a sprite file, update its `.import` file source path and any JSON `spriteId` references.
- Keep existing `.import` files with their sprite when possible so Godot import metadata remains stable.
- Do not leave obsolete sprite files, stale `spriteId` references, temporary chroma/alpha working files, or temporary Godot `.import` files behind.

Verification for sprite work:

- Search for old ids, sprite ids, and generic names after renaming or specializing an asset.
- Load the relevant catalogs or run domain tests so missing sprite ids, bad object ids, footprint overlaps, and local map loader errors are caught.
- When adding or changing world-object sprite ids, run a direct reference check that every JSON `spriteId` resolves to an existing PNG.
- Build the Godot project when C# constants, renderer code, or scene-facing references change.
- For visually important sprites, inspect the image dimensions and alpha bounds before finishing, and manually view the sprite when practical.

## Turn And Action Pipeline

The intended long-term flow is:

1. Input creates an action request.
2. The action is validated.
3. The simulation resolves the action.
4. Successful actions advance time or turns.
5. Systems update in a predictable order.
6. Messages are generated from simulation events.
7. UI refreshes from the resulting state.

Movement, wait, interaction, and future actions should eventually go through the same action pipeline. Avoid letting UI/input scripts directly mutate long-term simulation state.

## Scene And Script Boundaries

- `MainMenu` handles only menu flow.
- `GameShell` coordinates the current running prototype.
- `PlayerController` should translate input into action requests, not own full game rules.
- `GridView` should render grid state and handle visual hover/selection, not own world simulation.
- `TurnManager` may exist early, but long-term turn state should belong to the simulation layer.
- `MessageLog` UI displays messages. Simulation/domain code should generate message events or message data.
- Inspector UI displays selected state. It should not be the source of truth.

## Scope Discipline

- Do not add major systems unless explicitly requested.
- Do not invent crafting, combat, hunger, injuries, procedural generation, NPCs, quests, weather, or saving unless the task specifically asks for them.
- Do not helpfully expand scope.
- Preserve existing functionality unless asked to change it.
- Prefer small, testable vertical changes.
- Update docs when architecture or scope changes.

## Implementation And Test Discipline

The best implementation, architecture, code structure, and responsibility boundaries are the only production-code design constraints. Tests are verification artifacts and safety nets; their existing shape, helper APIs, fixture layout, mocks, call sites, file layout, or interface expectations are never compatibility requirements.

Never contort, preserve, or choose production code solely to keep existing tests compiling or green. This applies to every task, not only refactors. If the best implementation changes APIs, helpers, data shapes, or ownership boundaries, update, rewrite, move, or delete tests so they verify the new intended behavior and domain invariants.

When tests fail, first decide whether the failure reveals a real regression against intended player-facing behavior or important domain invariants. Fix production code for real regressions. Update tests for stale expectations about structure, APIs, helpers, fixtures, mocks, file layouts, or other implementation details.

Preserve player-facing behavior and important domain invariants unless the user explicitly asks for a behavior change.

## Architectural Debt Tracker

Use `docs/ARCHITECTURAL_DEBT.md` as the living index for architecture debt and improvement opportunities. Refer to tracked items by stable IDs such as `ARCH-1`.

Check the tracker when doing sweeps, refactors, architecture work, or code changes that touch known boundaries between Godot presentation, application/session coordination, and domain simulation.

When work discovers meaningful architecture debt, add a new `ARCH-*` item if future sessions should keep it visible. Include a rough `Size` estimate using `xs`, `s`, `m`, `l`, `xl`, or `xxl`. When work changes a tracked issue, update its status, priority, size, next action, or other canonical fields. Append dated `Notes` entries for factual implementation context, constraints, risks, or observations that are useful but too detailed for the core fields. Mark items resolved only when the architectural pressure is actually removed, and do not renumber existing items.

This tracker is proactive memory, not only a response to explicit user requests. During contextual discussion, planning, reviews, sweeps, code exploration, implementation, debugging, or test failure analysis, add or update an `ARCH-*` item whenever a meaningful architecture pressure or improvement opportunity becomes clear enough that future sessions should remember it.

When planning implementation for an existing `ARCH-*` item, assess whether its size is still accurate and whether it is too broad for one safe behavior-preserving change. If so, propose a split into smaller `ARCH-*` items and a recommended first slice before implementing.

Keep the tracker compact and ordered by priority first, then ID. Use it as memory and triage, not as permission to expand current gameplay scope.

## Mechanics Backlog

Use `docs/MECHANICS_BACKLOG.md` as the living backlog for deferred player-facing mechanics and systems. Refer to tracked items by stable IDs such as `MECH-1`.

When a plan identifies meaningful gameplay, UI, simulation, world, survival, combat, NPC, item, inventory, vehicle, or procedural-generation mechanics that are intentionally excluded from the current task, add or update a `MECH-*` item so the idea is not lost.

This backlog is proactive memory, not only a response to explicit user requests. During contextual discussion, planning, reviews, sweeps, code exploration, implementation, debugging, or test failure analysis, add or update a `MECH-*` item whenever a deferred player-facing mechanic or system becomes clear enough that future sessions should remember it.

Each `MECH-*` item should be one implementable system or one small vertical slice, and should include a rough `Size` estimate using `xs`, `s`, `m`, `l`, `xl`, or `xxl`. Split bundled feature families before recording them; for example, hunger, thirst, fatigue, sleep, pain, body temperature, and survival decay should not share one ID.

When planning implementation for an existing `MECH-*` item, assess whether its size is still accurate and whether it is too broad for one playable slice. If so, propose a split into smaller `MECH-*` items and a recommended first slice before implementing.

When general work reveals useful context for a tracked mechanic, append a dated `Notes` entry. If the discovery changes priority, size, dependencies, first playable slice, completion signal, or another canonical field, update that field too.

Do not add `MECH-*` entries for tiny implementation details, one-off content, architecture debt, or design principles. Use `ARCH-*` for architecture pressure, `MECH-*` for future mechanics, `docs/CURRENT_SCOPE.md` for implemented scope, and `docs/DESIGN_GOALS.md` for long-term design direction.

When a tracked mechanic is implemented, mark it `Implemented`, update `docs/CURRENT_SCOPE.md`, and add a task-log entry when it changes durable project state.

## Maintenance Sweeps

When the user says `Do a sweep`, treat it as permission to perform one focused codebase improvement without changing the game from a player-facing perspective.

A sweep may include bug fixes, cleanup, naming improvements, dead-code removal, test coverage, documentation alignment, responsibility-boundary cleanup, or architectural refinement. It may be small or large, but it must be one coherent improvement with a clear reason.

Before choosing the sweep target:

- Inspect the relevant code, tests, docs, and current git state.
- Prefer issues that improve correctness, maintainability, testability, clarity, or alignment with the Godot/domain boundary.
- Prefer domain-layer improvements and tests when the issue involves simulation rules.
- Preserve existing gameplay behavior and domain invariants unless the user explicitly asks for a behavior change.

During a sweep:

- Do not add new gameplay systems, mechanics, content, screens, rules, or scope.
- Do not invent crafting, combat expansion, AI, survival simulation, procedural generation, saving, weather, quests, or similar features.
- Keep edits as narrow as practical while still completing the improvement properly.
- If a larger refactor is warranted, keep it gameplay/domain behavior-preserving and verify it with focused tests.
- Do not treat existing test shape, helper APIs, fixtures, or expectations as compatibility boundaries; update tests to match the chosen implementation.
- Work with existing architecture and naming patterns instead of introducing new abstractions by default.
- Consider updating `docs/TASK_LOG.md` after the sweep, but add an entry only when the sweep changes durable player-facing behavior, content/data, architecture ownership, or documented project state. Update architecture or scope docs only when the sweep changes documented structure or guidance.

When completing a sweep, summarise:

- What issue or improvement was selected and why.
- Files changed.
- What changed.
- How it was tested.
- Assumptions.
- Known limitations or anything intentionally left out.

## Performance And Scaling

For future large-map work:

- The world should be chunked.
- Terrain should be represented as data.
- Visible Godot nodes should not represent every offscreen object.
- Avoid one-node-per-tile or one-node-per-item for large worlds.
- Render only the visible or nearby window where practical.
- Offscreen state should remain in simulation data.
- Godot `TileMap` and visual nodes are views, not canonical world storage.

## Coding Style

- Use clear names.
- Keep classes small where practical.
- Avoid giant manager scripts.
- Avoid circular dependencies.
- Prefer explicit data flow.
- Avoid hidden side effects.
- Document non-obvious architectural choices.
- Keep C# code idiomatic.
- Do not over-engineer premature abstractions.

## Documentation Expectations

Update relevant docs when making meaningful changes:

- `AGENTS.md` only when project-wide AI/developer guidance changes.
- `docs/BACKGROUND.md` when setting, tone, or creative north-star guidance changes.
- `docs/WORLD_AUTHORING_GUIDE.md` when reusable AI content-authoring templates, naming guidance, site guidance, worldbuilding patterns, or world-guidance maintenance rules change.
- `docs/DESIGN_GOALS.md` when long-term gameplay or system direction changes.
- `docs/ARCHITECTURE.md` when project structure or major architecture changes.
- `docs/ARCHITECTURAL_DEBT.md` when architecture debt is found, changed, actively addressed, resolved, or superseded.
- `docs/MECHANICS_BACKLOG.md` when deferred player-facing mechanics are identified, changed, planned, actively implemented, implemented, or superseded.
- `docs/CURRENT_SCOPE.md` when project scope changes.
- `docs/TASK_LOG.md` after an implemented task only when it meets the task-log admission rule: player-facing behavior, content/data milestones, architecture ownership changes, resolved or created major tracker systems, significant visual/content asset work, or another durable documented project-state change.

`docs/TASK_LOG.md` is curated milestone memory, not a full changelog. Skip routine bug fixes, tiny cleanup, pure investigations, plans, and review-only notes unless they change durable project state. Prefer `docs/CURRENT_SCOPE.md` for exact current scope, `docs/ARCHITECTURAL_DEBT.md` for future architecture work, `docs/MECHANICS_BACKLOG.md` for deferred mechanics, and Git commits/tests for full implementation detail.

## Future Codex Output Expectations

When completing a task, summarise:

- Files changed.
- What changed.
- How to test manually.
- Assumptions.
- Known limitations.
- Anything intentionally left out.

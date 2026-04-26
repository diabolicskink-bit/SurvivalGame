# AGENTS.md

Guidance for future Codex and AI coding sessions in this repository.

## Project Vision

This is an early Godot 4 .NET / C# project for a 2D turn-based open-world survival roguelike. The long-term direction is systemic depth inspired by games like Caves of Qud, paired with a modern, readable interface.

The current priority is a clean foundation. Do not add large gameplay systems unless the task explicitly asks for them.

## Creative North Star

The long-term game is an open sandbox survival roguelike set after a recent infrastructure collapse in the United States. The initial world should be a scaled-down but logistically believable Colorado using real map names, with a long-term possibility of a much larger nationwide journey.

Survival is driven by resource logistics, grounded tactical combat, and local-site risk. The player is currently assumed to be solo, but future party or crew systems are possible. The mobile base is a core fantasy, but "base" starts as travel capacity and continuity: carried gear, knowledge, stash, bike trailer, vehicle, bus, truck, or other platform. It should not imply that a car is mandatory from the start.

Use `docs/BACKGROUND.md` as soft setting and tone guidance, and `docs/DESIGN_GOALS.md` as long-term system direction. These docs should guide naming, content, and implementation choices, but they do not authorize expanding current scope. The scope guardrails below and `docs/CURRENT_SCOPE.md` remain the source of truth for what is implemented now.

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
- Use the existing sprite id pattern: `surface_<id>`, `item_<id>`, `npc_<id>`, `world_object_<id>`, or `structure_<style>_<piece>_<orientation>_<variant>`.
- Treat the footprint as gameplay truth. A sprite can visually overflow a tile through `spriteRender`, but collision, hover, targeting, and placement must still match the intended footprint.
- Decide whether the sprite's canonical north-facing shape or a particular placed orientation defines the footprint. Store the canonical footprint on the definition, then use placement `facing` for rotated map instances. For example, a north-facing `2 x 3` vehicle can be placed east-facing to occupy an effective `3 x 2` area.
- Walls, doors, windows, fences, gates, and gaps should generally be authored as edge-based structures, not tile world objects. Tile world objects are for things occupying tile area, such as furniture, trees, vehicles, tanks, machinery, and clutter.

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
- Structure sprites are 2.5D edge pieces anchored to the lower edge of the crossed tile boundary. Provide distinct front and side pieces instead of rotating one wall sprite when material direction, wall face, windows, or doors should look different.

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
- Update `docs/TASK_LOG.md` after the sweep. Update architecture or scope docs only when the sweep changes documented structure or guidance.

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
- `docs/DESIGN_GOALS.md` when long-term gameplay or system direction changes.
- `docs/ARCHITECTURE.md` when project structure or major architecture changes.
- `docs/CURRENT_SCOPE.md` when project scope changes.
- `docs/TASK_LOG.md` after each implemented task.

## Future Codex Output Expectations

When completing a task, summarise:

- Files changed.
- What changed.
- How to test manually.
- Assumptions.
- Known limitations.
- Anything intentionally left out.

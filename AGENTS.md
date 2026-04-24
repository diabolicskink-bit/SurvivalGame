# AGENTS.md

Guidance for future Codex and AI coding sessions in this repository.

## Project Vision

This is an early Godot 4 .NET / C# project for a 2D turn-based open-world survival roguelike. The long-term direction is systemic depth inspired by games like Caves of Qud, paired with a modern, readable interface.

The current priority is a clean foundation. Do not add large gameplay systems unless the task explicitly asks for them.

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
- `src/Godot/Game/` contains the current playable prototype shell.
- `src/Godot/Game/WorldView/` contains visible map/grid/input-view scripts.
- `src/Godot/Game/UI/` contains gameplay overlay controls.
- `src/Godot/Game/Prototype/` contains temporary Godot-side helpers that should migrate to domain code when the matching simulation concept becomes real.
- `src/SurvivalGame.Domain/` contains plain C# simulation/domain code, grouped by concept.
- `src/SurvivalGame.Domain/Actors/` contains player, NPC, creature, and actor state models.
- `src/SurvivalGame.Domain/World/` contains grid, map, terrain, chunk, and position logic.
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
- Rendering the grid/world view.
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

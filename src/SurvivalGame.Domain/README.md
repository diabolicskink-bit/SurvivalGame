# SurvivalGame.Domain

This project contains plain C# simulation and content logic that should be testable without Godot scenes.

Keep this layer free of Godot nodes and presentation concerns unless there is a deliberate integration boundary.

## Folder Shape

- `Actors/` contains player, NPC, creature, and actor state models.
- `WorldMap/` contains broad world map travel state, positions, travel methods, and points of interest.
- `LocalMaps/` contains local map, grid, terrain, chunk, and position logic.
- `Items/` contains item ids, item definitions, item catalogs, type paths, and placed item stacks.
- `Inventory/` contains inventory data structures and inventory rules.
- `Content/` contains loaders and adapters for runtime content data such as JSON definitions.

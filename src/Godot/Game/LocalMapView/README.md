# Local Map View

This folder contains Godot-side scripts for showing and interacting with the visible map area.

These scripts may translate input and render local map state, but they should not become canonical storage for terrain, items, actors, turns, or survival rules.

NPC markers are views over domain actor state. Health, identity, and placement belong in `src/SurvivalGame.Domain/Actors/`.

`MapEntityLayer` draws edge-based structures, world objects, NPCs, and the player marker in one Y-sorted pass. Structure collision is domain-owned by `StructureEdgeMap`; world object input, collision, and hover use the domain footprint indexed by `TileObjectMap`; NPC targeting still uses the NPC's grid tile. The board clips rendered children to the visible viewport.

Walls, doors, windows, fences, gates, and gaps should prefer edge-based structure data so movement asks whether the crossed tile boundary is blocked and rendering can choose front/side 2.5D wall pieces. Furniture, vehicles, tanks, trees, machinery, and other tile-occupying clutter remain world objects.

# Local Map View

This folder contains Godot-side scripts for showing and interacting with the visible map area.

These scripts may translate input and render local map state, but they should not become canonical storage for terrain, items, actors, turns, or survival rules.

NPC markers are views over domain actor state. Health, identity, and placement belong in `src/SurvivalGame.Domain/Actors/`.

`MapEntityLayer` draws 2.5D tile-object building walls, remaining edge-based structures, world objects, NPCs, and the player marker in one Y-sorted pass. `TileWallRenderModel` builds tile-wall kind, neighbor-mask, bounds, orientation, and geometry data for that draw pass without issuing draw calls. Structure collision is domain-owned by `StructureEdgeMap`; world object input, collision, and hover use the domain footprint indexed by `TileObjectMap`; NPC targeting still uses the NPC's grid tile. The board clips rendered children to the visible viewport.

Building walls and windows should use the procedural 2.5D tile-object wall path for now. Edge structures remain for current fences, gates, gaps, and legacy cleanup until the wall/fence representation work is resolved. Keep the tile-wall kind and neighbor-mask logic renderer-agnostic so a later Godot TileMap/Terrain path can reuse the same authored wall data.

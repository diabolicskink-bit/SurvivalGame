# Local Map View

This folder contains Godot-side scripts for showing and interacting with the visible map area.

These scripts may translate input and render local map state, but they should not become canonical storage for terrain, items, actors, turns, or survival rules.

NPC markers are views over domain actor state. Health, identity, and placement belong in `src/SurvivalGame.Domain/Actors/`.

`MapEntityLayer` draws world objects, NPCs, and the player marker in one Y-sorted pass. Oversized object/NPC sprite overflow is visual-only; input, collision, hover, and targeting should continue to use the owning grid tile.

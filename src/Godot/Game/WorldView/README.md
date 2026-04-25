# World View

This folder contains Godot-side scripts for showing and interacting with the visible map area.

These scripts may translate input and render world state, but they should not become canonical storage for terrain, items, actors, turns, or survival rules.

NPC markers are views over domain actor state. Health, identity, and placement belong in `src/SurvivalGame.Domain/Actors/`.

# Godot Presentation Layer

This folder contains Godot-facing scenes, controls, rendering, input, and presentation scripts.

Keep canonical gameplay state in `src/SurvivalGame.Domain/` wherever practical. Godot nodes should compose scenes, collect input, render simulation state, and coordinate the current prototype shell.

## Folder Shape

- `MainMenu/` contains the main menu scene and menu flow script.
- `Game/` contains the playable prototype shell.
- `Game/LocalMapView/` contains grid, visible local map, item-marker, and input-view scripts.
- `Game/UI/` contains gameplay overlay controls.
- `Game/Prototype/` contains temporary Godot-side helpers that should likely move into domain simulation later.

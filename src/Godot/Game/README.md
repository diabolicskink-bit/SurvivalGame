# Game Shell

`GameShell.tscn` is the current playable prototype scene. `GameShell.cs` wires together the visible board, prototype player state, item catalog loading, overlay UI, and scene navigation.

As the simulation matures, move long-lived game state and rules into `src/SurvivalGame.Domain/` and keep this folder focused on scene coordination and presentation.

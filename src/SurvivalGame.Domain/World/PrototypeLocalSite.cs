namespace SurvivalGame.Domain;

public sealed record PrototypeLocalSite(
    string Id,
    string DisplayName,
    GridBounds Bounds,
    GridPosition StartPosition,
    TileItemMap GroundItems,
    TileSurfaceMap Surfaces,
    TileObjectMap WorldObjects,
    NpcRoster Npcs
);

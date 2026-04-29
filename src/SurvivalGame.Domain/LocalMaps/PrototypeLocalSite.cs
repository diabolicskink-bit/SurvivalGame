namespace SurvivalGame.Domain;

public sealed record PrototypeLocalSite(
    SiteId Id,
    string DisplayName,
    GridBounds Bounds,
    GridPosition StartPosition,
    TileItemMap GroundItems,
    TileSurfaceMap Surfaces,
    TileObjectMap WorldObjects,
    StructureEdgeMap Structures,
    NpcRoster Npcs,
    IReadOnlyDictionary<TravelMethodId, TravelAnchorPlacement>? ArrivalAnchors = null
);

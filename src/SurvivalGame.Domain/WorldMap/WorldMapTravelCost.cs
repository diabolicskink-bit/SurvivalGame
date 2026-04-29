namespace SurvivalGame.Domain;

public sealed record WorldMapTravelCost(
    double SpeedMultiplier,
    double FuelUseMultiplier,
    WorldMapTerrainKind TerrainKind,
    string TerrainDisplayName,
    bool IsNearRoad);

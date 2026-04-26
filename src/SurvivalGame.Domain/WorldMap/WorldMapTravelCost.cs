namespace SurvivalGame.Domain;

public sealed record WorldMapTravelCost(
    double SpeedMultiplier,
    double FuelUseMultiplier,
    WorldMapTerrainKind TerrainKind,
    bool IsNearRoad);

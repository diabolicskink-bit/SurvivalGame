namespace SurvivalGame.Domain;

public sealed record WorldMapTerrainRegion
{
    public WorldMapTerrainRegion(
        string id,
        string displayName,
        WorldMapTerrainKind kind,
        IReadOnlyList<WorldMapPosition> points,
        double speedMultiplier,
        double fuelUseMultiplier,
        string mapColor)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Terrain region id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Terrain region display name is required.", nameof(displayName));
        }

        ArgumentNullException.ThrowIfNull(points);
        if (points.Count < 3)
        {
            throw new ArgumentException("Terrain regions require at least three points.", nameof(points));
        }

        if (speedMultiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(speedMultiplier), "Speed multiplier must be positive.");
        }

        if (fuelUseMultiplier < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fuelUseMultiplier), "Fuel multiplier cannot be negative.");
        }

        Id = id.Trim();
        DisplayName = displayName.Trim();
        Kind = kind;
        Points = points.ToArray();
        SpeedMultiplier = speedMultiplier;
        FuelUseMultiplier = fuelUseMultiplier;
        MapColor = string.IsNullOrWhiteSpace(mapColor) ? "#31401f" : mapColor.Trim();
    }

    public string Id { get; }

    public string DisplayName { get; }

    public WorldMapTerrainKind Kind { get; }

    public IReadOnlyList<WorldMapPosition> Points { get; }

    public double SpeedMultiplier { get; }

    public double FuelUseMultiplier { get; }

    public string MapColor { get; }

    public bool Contains(WorldMapPosition position)
    {
        var inside = false;
        for (int i = 0, j = Points.Count - 1; i < Points.Count; j = i++)
        {
            var current = Points[i];
            var previous = Points[j];
            var intersects = current.Y > position.Y != previous.Y > position.Y
                && position.X < ((previous.X - current.X) * (position.Y - current.Y) / (previous.Y - current.Y)) + current.X;
            if (intersects)
            {
                inside = !inside;
            }
        }

        return inside;
    }
}

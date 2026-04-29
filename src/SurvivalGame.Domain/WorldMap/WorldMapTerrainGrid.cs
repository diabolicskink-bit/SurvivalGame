namespace SurvivalGame.Domain;

public sealed record WorldMapTerrainProfile(
    WorldMapTerrainKind Kind,
    double SpeedMultiplier,
    double FuelUseMultiplier,
    string DisplayName);

public sealed record WorldMapTerrainSample(
    WorldMapTerrainKind Kind,
    double SpeedMultiplier,
    double FuelUseMultiplier,
    string DisplayName);

public sealed record WorldMapTerrainGrid
{
    public WorldMapTerrainGrid(
        int width,
        int height,
        IReadOnlyList<string> rows,
        IReadOnlyDictionary<char, WorldMapTerrainProfile> profiles)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Terrain grid width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Terrain grid height must be positive.");
        }

        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(profiles);
        if (rows.Count != height)
        {
            throw new ArgumentException("Terrain grid row count must match height.", nameof(rows));
        }

        if (profiles.Count == 0)
        {
            throw new ArgumentException("Terrain grid requires at least one terrain profile.", nameof(profiles));
        }

        foreach (var row in rows)
        {
            if (row.Length != width)
            {
                throw new ArgumentException("Every terrain grid row must match width.", nameof(rows));
            }

            foreach (var code in row)
            {
                if (!profiles.ContainsKey(code))
                {
                    throw new ArgumentException($"Terrain grid row uses unknown terrain code '{code}'.", nameof(rows));
                }
            }
        }

        Width = width;
        Height = height;
        Rows = rows.ToArray();
        Profiles = new Dictionary<char, WorldMapTerrainProfile>(profiles);
    }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<string> Rows { get; }

    public IReadOnlyDictionary<char, WorldMapTerrainProfile> Profiles { get; }

    public WorldMapTerrainSample Sample(WorldMapPosition position, double mapWidth, double mapHeight)
    {
        if (mapWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapWidth), "Map width must be positive.");
        }

        if (mapHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapHeight), "Map height must be positive.");
        }

        var column = (int)Math.Floor(Math.Clamp(position.X / mapWidth, 0.0, 0.999999999) * Width);
        var row = (int)Math.Floor(Math.Clamp(position.Y / mapHeight, 0.0, 0.999999999) * Height);
        var code = Rows[row][column];
        var profile = Profiles[code];
        return new WorldMapTerrainSample(
            profile.Kind,
            profile.SpeedMultiplier,
            profile.FuelUseMultiplier,
            profile.DisplayName);
    }
}

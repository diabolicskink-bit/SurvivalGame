namespace SurvivalGame.Domain;

public sealed record WorldMapDefinition
{
    public WorldMapDefinition(
        string id,
        string displayName,
        double mapWidth,
        double mapHeight,
        double visibleWidth,
        double visibleHeight,
        WorldMapBounds geographicBounds,
        WorldMapPosition startPosition,
        IReadOnlyList<WorldMapPointOfInterest> pointsOfInterest,
        IReadOnlyList<WorldMapRoad> roads,
        IReadOnlyList<WorldMapTerrainRegion> terrainRegions)
    {
        Id = ValidateRequired(id, nameof(id));
        DisplayName = ValidateRequired(displayName, nameof(displayName));

        if (mapWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapWidth), "Map width must be positive.");
        }

        if (mapHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapHeight), "Map height must be positive.");
        }

        if (visibleWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(visibleWidth), "Visible width must be positive.");
        }

        if (visibleHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(visibleHeight), "Visible height must be positive.");
        }

        ArgumentNullException.ThrowIfNull(geographicBounds);
        ArgumentNullException.ThrowIfNull(pointsOfInterest);
        ArgumentNullException.ThrowIfNull(roads);
        ArgumentNullException.ThrowIfNull(terrainRegions);

        MapWidth = mapWidth;
        MapHeight = mapHeight;
        VisibleWidth = visibleWidth;
        VisibleHeight = visibleHeight;
        GeographicBounds = geographicBounds;
        StartPosition = Clamp(startPosition);
        PointsOfInterest = pointsOfInterest.ToArray();
        Roads = roads.ToArray();
        TerrainRegions = terrainRegions.ToArray();

        EnsureUniqueIds(PointsOfInterest.Select(site => site.Id), "point of interest");
        EnsureUniqueIds(Roads.Select(road => road.Id), "road");
        EnsureUniqueIds(TerrainRegions.Select(region => region.Id), "terrain region");
        EnsurePositionsInsideMap();
    }

    public string Id { get; }

    public string DisplayName { get; }

    public double MapWidth { get; }

    public double MapHeight { get; }

    public double VisibleWidth { get; }

    public double VisibleHeight { get; }

    public WorldMapBounds GeographicBounds { get; }

    public WorldMapPosition StartPosition { get; }

    public IReadOnlyList<WorldMapPointOfInterest> PointsOfInterest { get; }

    public IReadOnlyList<WorldMapRoad> Roads { get; }

    public IReadOnlyList<WorldMapTerrainRegion> TerrainRegions { get; }

    public WorldMapPosition Project(double longitude, double latitude)
    {
        var x = (longitude - GeographicBounds.MinLongitude)
            / (GeographicBounds.MaxLongitude - GeographicBounds.MinLongitude)
            * MapWidth;
        var y = (GeographicBounds.MaxLatitude - latitude)
            / (GeographicBounds.MaxLatitude - GeographicBounds.MinLatitude)
            * MapHeight;

        return Clamp(new WorldMapPosition(x, y));
    }

    public WorldMapTravelCost GetTravelCost(WorldMapPosition position, TravelMethodDefinition travelMethod)
    {
        ArgumentNullException.ThrowIfNull(travelMethod);

        var terrain = TerrainRegions.LastOrDefault(region => region.Contains(position));
        var speedMultiplier = terrain?.SpeedMultiplier ?? 1.0;
        var fuelMultiplier = terrain?.FuelUseMultiplier ?? 1.0;
        var nearRoad = IsNearRoad(position);

        if (nearRoad)
        {
            if (travelMethod.Id == TravelMethodId.Vehicle)
            {
                speedMultiplier *= 1.35;
                fuelMultiplier *= 0.75;
            }
            else if (travelMethod.Id == TravelMethodId.Pushbike)
            {
                speedMultiplier *= 1.15;
            }
        }
        else if (travelMethod.Id == TravelMethodId.Vehicle)
        {
            speedMultiplier *= 0.8;
            fuelMultiplier *= 1.2;
        }
        else if (travelMethod.Id == TravelMethodId.Pushbike)
        {
            speedMultiplier *= 0.85;
        }

        return new WorldMapTravelCost(
            Math.Max(0.1, speedMultiplier),
            Math.Max(0.0, fuelMultiplier),
            terrain?.Kind ?? WorldMapTerrainKind.Plains,
            nearRoad
        );
    }

    private bool IsNearRoad(WorldMapPosition position)
    {
        foreach (var road in Roads)
        {
            if (road.DistanceTo(position) <= road.TravelInfluenceRadius)
            {
                return true;
            }
        }

        return false;
    }

    private WorldMapPosition Clamp(WorldMapPosition position)
    {
        return new WorldMapPosition(
            Math.Clamp(position.X, 0, MapWidth),
            Math.Clamp(position.Y, 0, MapHeight)
        );
    }

    private void EnsurePositionsInsideMap()
    {
        foreach (var site in PointsOfInterest)
        {
            if (!IsInsideMap(site.Position))
            {
                throw new ArgumentException($"Point of interest '{site.Id}' is outside world map bounds.");
            }
        }

        foreach (var road in Roads)
        {
            foreach (var point in road.Segments.SelectMany(segment => segment.Points))
            {
                if (!IsInsideMap(point))
                {
                    throw new ArgumentException($"Road '{road.Id}' contains a point outside world map bounds.");
                }
            }
        }

        foreach (var region in TerrainRegions)
        {
            foreach (var point in region.Points)
            {
                if (!IsInsideMap(point))
                {
                    throw new ArgumentException($"Terrain region '{region.Id}' contains a point outside world map bounds.");
                }
            }
        }
    }

    private bool IsInsideMap(WorldMapPosition position)
    {
        return position.X >= 0
            && position.X <= MapWidth
            && position.Y >= 0
            && position.Y <= MapHeight;
    }

    private static void EnsureUniqueIds(IEnumerable<string> ids, string kind)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in ids)
        {
            if (!seen.Add(id))
            {
                throw new ArgumentException($"Duplicate {kind} id '{id}'.");
            }
        }
    }

    private static string ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}

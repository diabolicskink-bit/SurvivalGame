namespace SurvivalGame.Domain;

public enum WorldMapRoadKind
{
    MajorRoad,
    StateHighway,
    UsHighway,
    Interstate
}

public sealed record WorldMapRoadSegment
{
    public WorldMapRoadSegment(IReadOnlyList<WorldMapPosition> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (points.Count < 2)
        {
            throw new ArgumentException("Road segments require at least two points.", nameof(points));
        }

        Points = points.ToArray();
    }

    public IReadOnlyList<WorldMapPosition> Points { get; }
}

public sealed record WorldMapRoad
{
    public WorldMapRoad(
        string id,
        string displayName,
        WorldMapRoadKind kind,
        IReadOnlyList<WorldMapRoadSegment> segments,
        int priority,
        int laneCount,
        int mapLanesPerDirection,
        double surfaceWidthFeet,
        double travelInfluenceRadius)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Road id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Road display name is required.", nameof(displayName));
        }

        ArgumentNullException.ThrowIfNull(segments);
        if (segments.Count == 0)
        {
            throw new ArgumentException("Roads require at least one segment.", nameof(segments));
        }

        if (travelInfluenceRadius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(travelInfluenceRadius), "Road travel radius must be positive.");
        }

        Id = id.Trim();
        DisplayName = displayName.Trim();
        Kind = kind;
        Segments = segments.ToArray();
        Priority = priority;
        LaneCount = Math.Max(1, laneCount);
        MapLanesPerDirection = Math.Clamp(mapLanesPerDirection, 1, 3);
        SurfaceWidthFeet = Math.Max(0.0, surfaceWidthFeet);
        TravelInfluenceRadius = travelInfluenceRadius;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public WorldMapRoadKind Kind { get; }

    public IReadOnlyList<WorldMapRoadSegment> Segments { get; }

    public int Priority { get; }

    public int LaneCount { get; }

    public int MapLanesPerDirection { get; }

    public double SurfaceWidthFeet { get; }

    public double TravelInfluenceRadius { get; }

    public double DistanceTo(WorldMapPosition position)
    {
        var best = double.MaxValue;
        foreach (var segment in Segments)
        {
            for (var i = 0; i < segment.Points.Count - 1; i++)
            {
                best = Math.Min(best, DistanceToSegment(position, segment.Points[i], segment.Points[i + 1]));
            }
        }

        return best;
    }

    private static double DistanceToSegment(WorldMapPosition point, WorldMapPosition start, WorldMapPosition end)
    {
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var lengthSquared = (dx * dx) + (dy * dy);
        if (lengthSquared <= 0)
        {
            return point.DistanceTo(start);
        }

        var t = (((point.X - start.X) * dx) + ((point.Y - start.Y) * dy)) / lengthSquared;
        t = Math.Clamp(t, 0.0, 1.0);
        var closest = new WorldMapPosition(start.X + (t * dx), start.Y + (t * dy));
        return point.DistanceTo(closest);
    }
}

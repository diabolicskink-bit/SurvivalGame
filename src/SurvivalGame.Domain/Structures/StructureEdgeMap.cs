namespace SurvivalGame.Domain;

public enum StructureEdgeDirection
{
    North,
    East,
    South,
    West
}

public enum StructureEdgeAxis
{
    Horizontal,
    Vertical
}

public readonly record struct StructureEdgeKey(int X, int Y, StructureEdgeAxis Axis)
{
    public static StructureEdgeKey FromTileEdge(
        GridPosition position,
        StructureEdgeDirection direction,
        GridBounds bounds)
    {
        if (!bounds.Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Structure edge tile position must be inside map bounds.");
        }

        var key = direction switch
        {
            StructureEdgeDirection.North => new StructureEdgeKey(position.X, position.Y, StructureEdgeAxis.Horizontal),
            StructureEdgeDirection.South => new StructureEdgeKey(position.X, position.Y + 1, StructureEdgeAxis.Horizontal),
            StructureEdgeDirection.West => new StructureEdgeKey(position.X, position.Y, StructureEdgeAxis.Vertical),
            StructureEdgeDirection.East => new StructureEdgeKey(position.X + 1, position.Y, StructureEdgeAxis.Vertical),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unsupported structure edge direction.")
        };

        key.EnsureInside(bounds);
        return key;
    }

    public static bool TryBetween(
        GridPosition from,
        GridPosition to,
        GridBounds bounds,
        out StructureEdgeKey key)
    {
        key = default;
        if (!bounds.Contains(from) || !bounds.Contains(to))
        {
            return false;
        }

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        if (Math.Abs(dx) + Math.Abs(dy) != 1)
        {
            return false;
        }

        var direction = dx switch
        {
            1 => StructureEdgeDirection.East,
            -1 => StructureEdgeDirection.West,
            _ => dy == 1 ? StructureEdgeDirection.South : StructureEdgeDirection.North
        };
        key = FromTileEdge(from, direction, bounds);
        return true;
    }

    public StructureEdgeKey NeighborBefore()
    {
        return Axis == StructureEdgeAxis.Horizontal
            ? this with { X = X - 1 }
            : this with { Y = Y - 1 };
    }

    public StructureEdgeKey NeighborAfter()
    {
        return Axis == StructureEdgeAxis.Horizontal
            ? this with { X = X + 1 }
            : this with { Y = Y + 1 };
    }

    private void EnsureInside(GridBounds bounds)
    {
        var inside = Axis switch
        {
            StructureEdgeAxis.Horizontal => X >= 0 && X < bounds.Width && Y >= 0 && Y <= bounds.Height,
            StructureEdgeAxis.Vertical => X >= 0 && X <= bounds.Width && Y >= 0 && Y < bounds.Height,
            _ => false
        };

        if (!inside)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bounds),
                $"Structure edge {X}, {Y}, {Axis} must stay inside map edge bounds."
            );
        }
    }
}

public readonly record struct PlacedStructureEdge(
    GridPosition Position,
    StructureEdgeDirection Direction,
    StructureEdgeKey Key,
    StructureId StructureId);

public sealed class StructureEdgeMap
{
    private readonly List<PlacedStructureEdge> _edges = new();
    private readonly Dictionary<StructureEdgeKey, int> _edgeIndexesByKey = new();

    public StructureEdgeMap(GridBounds bounds)
    {
        Bounds = bounds;
    }

    public GridBounds Bounds { get; }

    public IReadOnlyList<PlacedStructureEdge> AllEdges => _edges.ToArray();

    public bool IsEmpty => _edges.Count == 0;

    public void Place(GridPosition position, StructureEdgeDirection direction, StructureId structureId)
    {
        ArgumentNullException.ThrowIfNull(structureId);

        var key = StructureEdgeKey.FromTileEdge(position, direction, Bounds);
        if (_edgeIndexesByKey.ContainsKey(key))
        {
            throw new InvalidOperationException($"Structure edge {key.X}, {key.Y}, {key.Axis} already has a structure.");
        }

        var edge = new PlacedStructureEdge(position, direction, key, structureId);
        _edgeIndexesByKey.Add(key, _edges.Count);
        _edges.Add(edge);
    }

    public bool TryGetEdgeAt(
        GridPosition position,
        StructureEdgeDirection direction,
        out PlacedStructureEdge edge)
    {
        var key = StructureEdgeKey.FromTileEdge(position, direction, Bounds);
        return TryGetEdge(key, out edge);
    }

    public bool TryGetEdgeBetween(GridPosition from, GridPosition to, out PlacedStructureEdge edge)
    {
        if (!StructureEdgeKey.TryBetween(from, to, Bounds, out var key))
        {
            edge = default;
            return false;
        }

        return TryGetEdge(key, out edge);
    }

    public bool TryGetEdge(StructureEdgeKey key, out PlacedStructureEdge edge)
    {
        if (_edgeIndexesByKey.TryGetValue(key, out var edgeIndex))
        {
            edge = _edges[edgeIndex];
            return true;
        }

        edge = default;
        return false;
    }
}

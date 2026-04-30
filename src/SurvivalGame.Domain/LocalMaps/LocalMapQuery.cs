namespace SurvivalGame.Domain;

public enum LocalMapBlockerKind
{
    OutOfBounds,
    WorldObject,
    Npc
}

public readonly record struct LocalMapBlocker(LocalMapBlockerKind Kind, string Name);

public sealed class LocalMapQuery
{
    private static readonly GridOffset[] CardinalOffsets =
    [
        GridOffset.Up,
        GridOffset.Down,
        GridOffset.Left,
        GridOffset.Right
    ];

    private readonly LocalMapState _localMap;
    private readonly WorldObjectCatalog? _worldObjects;

    public LocalMapQuery(
        LocalMapState localMap,
        WorldObjectCatalog? worldObjects = null)
    {
        ArgumentNullException.ThrowIfNull(localMap);

        _localMap = localMap;
        _worldObjects = worldObjects;
    }

    public bool TryGetMovementBlocker(GridPosition from, GridPosition to, out LocalMapBlocker blocker)
    {
        if (!_localMap.Map.Contains(to))
        {
            blocker = new LocalMapBlocker(LocalMapBlockerKind.OutOfBounds, "map boundary");
            return true;
        }

        if (TryGetMovementWorldObjectBlocker(to, out blocker)
            || TryGetMovementNpcBlocker(to, out blocker))
        {
            return true;
        }

        blocker = default;
        return false;
    }

    public bool TryGetStandBlocker(GridPosition position, out LocalMapBlocker blocker)
    {
        if (!_localMap.Map.Contains(position))
        {
            blocker = new LocalMapBlocker(LocalMapBlockerKind.OutOfBounds, "map boundary");
            return true;
        }

        if (TryGetMovementWorldObjectBlocker(position, out blocker)
            || TryGetMovementNpcBlocker(position, out blocker))
        {
            return true;
        }

        blocker = default;
        return false;
    }

    public IEnumerable<PlacedWorldObject> GetNearbyWorldObjectPlacements(GridPosition origin, bool includeOrigin)
    {
        var seen = new HashSet<WorldObjectInstanceId>();

        if (includeOrigin
            && _localMap.Map.Contains(origin)
            && _localMap.WorldObjects.TryGetPlacementAt(origin, out var originPlacement)
            && seen.Add(originPlacement.InstanceId))
        {
            yield return originPlacement;
        }

        foreach (var offset in CardinalOffsets)
        {
            var position = origin + offset;
            if (!_localMap.Map.Contains(position)
                || !_localMap.WorldObjects.TryGetPlacementAt(position, out var placement)
                || !seen.Add(placement.InstanceId))
            {
                continue;
            }

            yield return placement;
        }
    }

    public bool IsNearPlacement(GridPosition position, PlacedWorldObject placement)
    {
        foreach (var occupiedPosition in placement.OccupiedPositions())
        {
            if (ManhattanDistance(position, occupiedPosition) <= 1)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryFindSightBlocker(GridPosition from, GridPosition to, out LocalMapBlocker blocker)
    {
        blocker = default;
        if (from == to)
        {
            return false;
        }

        var current = from;
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var stepX = Math.Sign(dx);
        var stepY = Math.Sign(dy);
        var tMaxX = stepX == 0 ? double.PositiveInfinity : 0.5 / Math.Abs(dx);
        var tMaxY = stepY == 0 ? double.PositiveInfinity : 0.5 / Math.Abs(dy);
        var tDeltaX = stepX == 0 ? double.PositiveInfinity : 1.0 / Math.Abs(dx);
        var tDeltaY = stepY == 0 ? double.PositiveInfinity : 1.0 / Math.Abs(dy);

        while (current != to)
        {
            if (NearlyEqual(tMaxX, tMaxY))
            {
                var horizontalStep = current + new GridOffset(0, stepY);
                var verticalStep = current + new GridOffset(stepX, 0);

                if (TryGetSightWorldObjectBlocker(verticalStep, from, to, out blocker)
                    || TryGetSightWorldObjectBlocker(horizontalStep, from, to, out blocker))
                {
                    return true;
                }

                current += new GridOffset(stepX, stepY);
                tMaxX += tDeltaX;
                tMaxY += tDeltaY;
            }
            else if (tMaxX < tMaxY)
            {
                var next = current + new GridOffset(stepX, 0);
                current = next;
                tMaxX += tDeltaX;
            }
            else
            {
                var next = current + new GridOffset(0, stepY);
                current = next;
                tMaxY += tDeltaY;
            }

            if (TryGetSightWorldObjectBlocker(current, from, to, out blocker))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetMovementWorldObjectBlocker(GridPosition position, out LocalMapBlocker blocker)
    {
        blocker = default;
        if (!_localMap.WorldObjects.TryGetObjectAt(position, out var objectId))
        {
            return false;
        }

        if (_worldObjects is null || !_worldObjects.TryGet(objectId, out var worldObject))
        {
            blocker = new LocalMapBlocker(LocalMapBlockerKind.WorldObject, objectId.ToString());
            return true;
        }

        if (!worldObject.BlocksMovement)
        {
            return false;
        }

        blocker = new LocalMapBlocker(LocalMapBlockerKind.WorldObject, worldObject.Name);
        return true;
    }

    private bool TryGetMovementNpcBlocker(GridPosition position, out LocalMapBlocker blocker)
    {
        if (_localMap.Npcs.TryGetAt(position, out var npc) && npc.BlocksMovement)
        {
            blocker = new LocalMapBlocker(LocalMapBlockerKind.Npc, npc.Name);
            return true;
        }

        blocker = default;
        return false;
    }

    private bool TryGetSightWorldObjectBlocker(
        GridPosition position,
        GridPosition shooter,
        GridPosition target,
        out LocalMapBlocker blocker)
    {
        blocker = default;
        if (position == shooter || position == target || !_localMap.Map.Contains(position))
        {
            return false;
        }

        if (!_localMap.WorldObjects.TryGetObjectAt(position, out var objectId))
        {
            return false;
        }

        if (_worldObjects is null || !_worldObjects.TryGet(objectId, out var worldObject))
        {
            blocker = new LocalMapBlocker(LocalMapBlockerKind.WorldObject, objectId.ToString());
            return true;
        }

        if (!worldObject.BlocksSight)
        {
            return false;
        }

        blocker = new LocalMapBlocker(LocalMapBlockerKind.WorldObject, worldObject.Name);
        return true;
    }

    private static int ManhattanDistance(GridPosition a, GridPosition b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    private static bool NearlyEqual(double left, double right)
    {
        return Math.Abs(left - right) < 0.0000001;
    }
}

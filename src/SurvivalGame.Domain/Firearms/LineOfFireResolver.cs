namespace SurvivalGame.Domain;

internal sealed class LineOfFireResolver
{
    private readonly WorldObjectCatalog? _worldObjects;
    private readonly StructureCatalog? _structures;

    public LineOfFireResolver(WorldObjectCatalog? worldObjects, StructureCatalog? structures)
    {
        _worldObjects = worldObjects;
        _structures = structures;
    }

    public bool TryFindBlocker(
        LocalMapState localMap,
        GridPosition from,
        GridPosition to,
        out LineOfFireBlocker blocker)
    {
        ArgumentNullException.ThrowIfNull(localMap);

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

                if (TryFindStructureBlocker(localMap, current, verticalStep, out blocker)
                    || TryFindStructureBlocker(localMap, current, horizontalStep, out blocker)
                    || TryFindObjectBlocker(localMap, verticalStep, from, to, out blocker)
                    || TryFindObjectBlocker(localMap, horizontalStep, from, to, out blocker))
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
                if (TryFindStructureBlocker(localMap, current, next, out blocker))
                {
                    return true;
                }

                current = next;
                tMaxX += tDeltaX;
            }
            else
            {
                var next = current + new GridOffset(0, stepY);
                if (TryFindStructureBlocker(localMap, current, next, out blocker))
                {
                    return true;
                }

                current = next;
                tMaxY += tDeltaY;
            }

            if (TryFindObjectBlocker(localMap, current, from, to, out blocker))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryFindStructureBlocker(
        LocalMapState localMap,
        GridPosition from,
        GridPosition to,
        out LineOfFireBlocker blocker)
    {
        blocker = default;
        if (!localMap.Map.Contains(from)
            || !localMap.Map.Contains(to)
            || !localMap.Structures.TryGetEdgeBetween(from, to, out var edge))
        {
            return false;
        }

        if (_structures is null || !_structures.TryGet(edge.StructureId, out var structure))
        {
            blocker = new LineOfFireBlocker(edge.StructureId.ToString());
            return true;
        }

        if (!structure.BlocksSight)
        {
            return false;
        }

        blocker = new LineOfFireBlocker(structure.Name);
        return true;
    }

    private bool TryFindObjectBlocker(
        LocalMapState localMap,
        GridPosition position,
        GridPosition shooter,
        GridPosition target,
        out LineOfFireBlocker blocker)
    {
        blocker = default;
        if (position == shooter || position == target || !localMap.Map.Contains(position))
        {
            return false;
        }

        if (!localMap.WorldObjects.TryGetObjectAt(position, out var objectId))
        {
            return false;
        }

        if (_worldObjects is null || !_worldObjects.TryGet(objectId, out var worldObject))
        {
            blocker = new LineOfFireBlocker(objectId.ToString());
            return true;
        }

        if (!worldObject.BlocksSight)
        {
            return false;
        }

        blocker = new LineOfFireBlocker(worldObject.Name);
        return true;
    }

    private static bool NearlyEqual(double left, double right)
    {
        return Math.Abs(left - right) < 0.0000001;
    }
}

internal readonly record struct LineOfFireBlocker(string Name);

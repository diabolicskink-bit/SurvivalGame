namespace SurvivalGame.Domain;

public readonly record struct PlacedWorldObject(
    GridPosition Position,
    WorldObjectId ObjectId,
    WorldObjectInstanceId InstanceId,
    WorldObjectFacing Facing,
    WorldObjectFootprint Footprint,
    WorldObjectContainerLootSpec? ContainerLoot)
{
    public WorldObjectFootprint EffectiveFootprint => Footprint.Rotated(Facing);

    public IEnumerable<GridPosition> OccupiedPositions()
    {
        return EffectiveFootprint.PositionsFrom(Position);
    }
}

public sealed class TileObjectMap
{
    private readonly List<PlacedWorldObject> _placements = new();
    private readonly Dictionary<WorldObjectInstanceId, int> _placementIndexesById = new();
    private readonly Dictionary<GridPosition, int> _placementIndexesByPosition = new();

    public IReadOnlyList<PlacedWorldObject> AllObjects
    {
        get
        {
            return _placements.ToArray();
        }
    }

    public bool IsEmpty => _placements.Count == 0;

    public bool TryGetObjectAt(GridPosition position, out WorldObjectId objectId)
    {
        if (TryGetPlacementAt(position, out var placement))
        {
            objectId = placement.ObjectId;
            return true;
        }

        objectId = null!;
        return false;
    }

    public bool TryGetPlacementAt(GridPosition position, out PlacedWorldObject placement)
    {
        if (_placementIndexesByPosition.TryGetValue(position, out var placementIndex))
        {
            placement = _placements[placementIndex];
            return true;
        }

        placement = default;
        return false;
    }

    public bool TryGetPlacement(WorldObjectInstanceId instanceId, out PlacedWorldObject placement)
    {
        ArgumentNullException.ThrowIfNull(instanceId);

        if (_placementIndexesById.TryGetValue(instanceId, out var placementIndex))
        {
            placement = _placements[placementIndex];
            return true;
        }

        placement = default;
        return false;
    }

    public void Place(GridPosition position, WorldObjectId objectId)
    {
        Place(position, objectId, WorldObjectFacing.North, WorldObjectFootprint.SingleTile);
    }

    public void Place(
        GridPosition position,
        WorldObjectId objectId,
        WorldObjectFacing facing,
        WorldObjectFootprint footprint,
        GridBounds? bounds = null,
        WorldObjectInstanceId? instanceId = null,
        WorldObjectContainerLootSpec? containerLoot = null)
    {
        ArgumentNullException.ThrowIfNull(objectId);

        var resolvedInstanceId = instanceId ?? CreateDefaultInstanceId(objectId, position);
        if (_placementIndexesById.ContainsKey(resolvedInstanceId))
        {
            throw new InvalidOperationException($"World object instance '{resolvedInstanceId}' is already placed.");
        }

        var placement = new PlacedWorldObject(position, objectId, resolvedInstanceId, facing, footprint, containerLoot);
        var occupiedPositions = placement.OccupiedPositions().ToArray();
        foreach (var occupiedPosition in occupiedPositions)
        {
            if (bounds is { } mapBounds && !mapBounds.Contains(occupiedPosition))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    $"World object footprint for '{objectId}' must stay inside map bounds."
                );
            }

            if (_placementIndexesByPosition.ContainsKey(occupiedPosition))
            {
                throw new InvalidOperationException(
                    $"Tile {occupiedPosition.X}, {occupiedPosition.Y} already has a world object."
                );
            }
        }

        var placementIndex = _placements.Count;
        _placements.Add(placement);
        _placementIndexesById.Add(resolvedInstanceId, placementIndex);

        foreach (var occupiedPosition in occupiedPositions)
        {
            _placementIndexesByPosition.Add(occupiedPosition, placementIndex);
        }
    }

    private static WorldObjectInstanceId CreateDefaultInstanceId(WorldObjectId objectId, GridPosition position)
    {
        return new WorldObjectInstanceId($"{objectId.Value}@{position.X},{position.Y}");
    }
}

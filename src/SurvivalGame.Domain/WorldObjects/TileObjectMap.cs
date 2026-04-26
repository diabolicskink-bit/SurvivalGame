namespace SurvivalGame.Domain;

public readonly record struct PlacedWorldObject(
    GridPosition Position,
    WorldObjectId ObjectId,
    WorldObjectFacing Facing,
    WorldObjectFootprint Footprint)
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

    public void Place(GridPosition position, WorldObjectId objectId)
    {
        Place(position, objectId, WorldObjectFacing.North, WorldObjectFootprint.SingleTile);
    }

    public void Place(
        GridPosition position,
        WorldObjectId objectId,
        WorldObjectFacing facing,
        WorldObjectFootprint footprint,
        GridBounds? bounds = null)
    {
        ArgumentNullException.ThrowIfNull(objectId);

        var placement = new PlacedWorldObject(position, objectId, facing, footprint);
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

        foreach (var occupiedPosition in occupiedPositions)
        {
            _placementIndexesByPosition.Add(occupiedPosition, placementIndex);
        }
    }
}

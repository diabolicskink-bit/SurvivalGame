namespace SurvivalGame.Domain;

public readonly record struct PlacedWorldObject(GridPosition Position, WorldObjectId ObjectId);

public sealed class TileObjectMap
{
    private readonly Dictionary<GridPosition, WorldObjectId> _objectsByPosition = new();

    public IReadOnlyList<PlacedWorldObject> AllObjects
    {
        get
        {
            return _objectsByPosition
                .Select(entry => new PlacedWorldObject(entry.Key, entry.Value))
                .ToArray();
        }
    }

    public bool IsEmpty => _objectsByPosition.Count == 0;

    public bool TryGetObjectAt(GridPosition position, out WorldObjectId objectId)
    {
        return _objectsByPosition.TryGetValue(position, out objectId!);
    }

    public void Place(GridPosition position, WorldObjectId objectId)
    {
        ArgumentNullException.ThrowIfNull(objectId);

        if (!_objectsByPosition.TryAdd(position, objectId))
        {
            throw new InvalidOperationException($"Tile {position.X}, {position.Y} already has a world object.");
        }
    }
}

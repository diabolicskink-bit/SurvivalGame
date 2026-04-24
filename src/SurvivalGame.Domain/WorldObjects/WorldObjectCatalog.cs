namespace SurvivalGame.Domain;

public sealed class WorldObjectCatalog
{
    private readonly Dictionary<WorldObjectId, WorldObjectDefinition> _objects = new();

    public IReadOnlyCollection<WorldObjectDefinition> Objects => _objects.Values.ToArray();

    public void Add(WorldObjectDefinition worldObject)
    {
        ArgumentNullException.ThrowIfNull(worldObject);

        if (!_objects.TryAdd(worldObject.Id, worldObject))
        {
            throw new InvalidOperationException($"World object '{worldObject.Id}' is already defined.");
        }
    }

    public bool TryGet(WorldObjectId id, out WorldObjectDefinition worldObject)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (_objects.TryGetValue(id, out var foundObject))
        {
            worldObject = foundObject;
            return true;
        }

        worldObject = null!;
        return false;
    }

    public WorldObjectDefinition Get(WorldObjectId id)
    {
        if (TryGet(id, out var worldObject))
        {
            return worldObject;
        }

        throw new KeyNotFoundException($"World object '{id}' is not defined.");
    }
}

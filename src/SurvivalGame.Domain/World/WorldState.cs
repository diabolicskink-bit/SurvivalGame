namespace SurvivalGame.Domain;

public sealed class WorldState
{
    public WorldState(MapState map, TileItemMap groundItems, TileObjectMap worldObjects)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(groundItems);
        ArgumentNullException.ThrowIfNull(worldObjects);

        Map = map;
        GroundItems = groundItems;
        WorldObjects = worldObjects;
    }

    public MapState Map { get; }

    public TileItemMap GroundItems { get; }

    public TileObjectMap WorldObjects { get; }
}

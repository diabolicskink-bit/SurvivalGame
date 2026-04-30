namespace SurvivalGame.Domain;

public sealed class LocalMapState
{
    public LocalMapState(
        LocalMap map,
        TileItemMap groundItems,
        TileObjectMap worldObjects,
        NpcRoster? npcs = null,
        WorldObjectContainerStateStore? containerStates = null)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(groundItems);
        ArgumentNullException.ThrowIfNull(worldObjects);

        Map = map;
        GroundItems = groundItems;
        WorldObjects = worldObjects;
        Npcs = npcs ?? new NpcRoster();
        ContainerStates = containerStates ?? new WorldObjectContainerStateStore();

        foreach (var npc in Npcs.AllNpcs)
        {
            if (!Map.Contains(npc.Position))
            {
                throw new ArgumentOutOfRangeException(nameof(npcs), $"NPC '{npc.Id}' must be inside the map bounds.");
            }
        }
    }

    public LocalMap Map { get; }

    public TileItemMap GroundItems { get; }

    public TileObjectMap WorldObjects { get; }

    public NpcRoster Npcs { get; }

    public WorldObjectContainerStateStore ContainerStates { get; }
}

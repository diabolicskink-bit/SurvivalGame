namespace SurvivalGame.Domain;

public sealed class LocalMapState
{
    public LocalMapState(
        LocalMap map,
        TileItemMap groundItems,
        TileObjectMap worldObjects,
        NpcRoster? npcs = null,
        WorldObjectContainerStateStore? containerStates = null,
        StructureEdgeMap? structures = null)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(groundItems);
        ArgumentNullException.ThrowIfNull(worldObjects);

        Map = map;
        GroundItems = groundItems;
        WorldObjects = worldObjects;
        Structures = structures ?? new StructureEdgeMap(map.Bounds);
        Npcs = npcs ?? new NpcRoster();
        ContainerStates = containerStates ?? new WorldObjectContainerStateStore();

        if (Structures.Bounds != Map.Bounds)
        {
            throw new ArgumentException("Structure edge map bounds must match map bounds.", nameof(structures));
        }

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

    public StructureEdgeMap Structures { get; }

    public NpcRoster Npcs { get; }

    public WorldObjectContainerStateStore ContainerStates { get; }
}

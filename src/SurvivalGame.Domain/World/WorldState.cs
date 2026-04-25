namespace SurvivalGame.Domain;

public sealed class WorldState
{
    public WorldState(MapState map, TileItemMap groundItems, TileObjectMap worldObjects, NpcRoster? npcs = null)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(groundItems);
        ArgumentNullException.ThrowIfNull(worldObjects);

        Map = map;
        GroundItems = groundItems;
        WorldObjects = worldObjects;
        Npcs = npcs ?? new NpcRoster();

        foreach (var npc in Npcs.AllNpcs)
        {
            if (!Map.Contains(npc.Position))
            {
                throw new ArgumentOutOfRangeException(nameof(npcs), $"NPC '{npc.Id}' must be inside the map bounds.");
            }
        }
    }

    public MapState Map { get; }

    public TileItemMap GroundItems { get; }

    public TileObjectMap WorldObjects { get; }

    public NpcRoster Npcs { get; }
}

namespace SurvivalGame.Domain;

public sealed class PrototypeGameState
{
    public const string DefaultSiteId = "prototype_local";

    public PrototypeGameState(GridBounds mapBounds, TileItemMap groundItems, GridPosition startPosition)
        : this(
            CreateWorldState(mapBounds, groundItems, new TileSurfaceMap(mapBounds, PrototypeSurfaces.Concrete), new TileObjectMap()),
            startPosition,
            DefaultSiteId
        )
    {
    }

    public PrototypeGameState(
        GridBounds mapBounds,
        TileItemMap groundItems,
        TileSurfaceMap surfaces,
        GridPosition startPosition
    )
        : this(CreateWorldState(mapBounds, groundItems, surfaces, new TileObjectMap()), startPosition, DefaultSiteId)
    {
    }

    public PrototypeGameState(
        GridBounds mapBounds,
        TileItemMap groundItems,
        TileSurfaceMap surfaces,
        TileObjectMap worldObjects,
        GridPosition startPosition
    )
        : this(CreateWorldState(mapBounds, groundItems, surfaces, worldObjects), startPosition, DefaultSiteId)
    {
    }

    public PrototypeGameState(
        GridBounds mapBounds,
        TileItemMap groundItems,
        TileSurfaceMap surfaces,
        TileObjectMap worldObjects,
        NpcRoster npcs,
        GridPosition startPosition
    )
        : this(CreateWorldState(mapBounds, groundItems, surfaces, worldObjects, npcs), startPosition, DefaultSiteId)
    {
    }

    public PrototypeGameState(WorldState world, GridPosition startPosition)
        : this(world, startPosition, DefaultSiteId)
    {
    }

    public PrototypeGameState(WorldState world, GridPosition startPosition, string siteId)
        : this(
            world,
            startPosition,
            new PlayerState(),
            new WorldTime(),
            new StatefulItemStore(),
            siteId
        )
    {
    }

    public PrototypeGameState(
        WorldState world,
        GridPosition startPosition,
        PlayerState player,
        WorldTime time,
        StatefulItemStore statefulItems,
        string siteId
    )
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(statefulItems);

        if (string.IsNullOrWhiteSpace(siteId))
        {
            throw new ArgumentException("Site id cannot be empty.", nameof(siteId));
        }

        World = world;
        Player = player;
        Time = time;
        StatefulItems = statefulItems;
        SiteId = siteId.Trim();
        Player.SetPosition(world.Map.Clamp(startPosition));
    }

    public string SiteId { get; }

    public WorldTime Time { get; }

    public PlayerState Player { get; }

    public WorldState World { get; }

    public StatefulItemStore StatefulItems { get; }

    public GridBounds MapBounds => World.Map.Bounds;

    public TileItemMap GroundItems => World.GroundItems;

    public TileSurfaceMap Surfaces => World.Map.Surfaces;

    public TileObjectMap WorldObjects => World.WorldObjects;

    public NpcRoster Npcs => World.Npcs;

    public GridPosition PlayerPosition => Player.Position;

    public int ElapsedTicks => Time.ElapsedTicks;

    public void SetPlayerPosition(GridPosition position)
    {
        if (!World.Map.Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Player position must be inside the map bounds.");
        }

        Player.SetPosition(position);
    }

    public void AdvanceTime(int ticks)
    {
        Time.Advance(ticks);
    }

    private static WorldState CreateWorldState(
        GridBounds mapBounds,
        TileItemMap groundItems,
        TileSurfaceMap surfaces,
        TileObjectMap worldObjects,
        NpcRoster? npcs = null
    )
    {
        return new WorldState(
            new MapState(mapBounds, surfaces),
            groundItems,
            worldObjects,
            npcs
        );
    }
}

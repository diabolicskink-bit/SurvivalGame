namespace SurvivalGame.Domain;

public sealed class PrototypeGameState
{
    public const string DefaultSiteId = "prototype_local";

    public PrototypeGameState(GridBounds mapBounds, TileItemMap groundItems, GridPosition startPosition)
        : this(
            CreateLocalMapState(mapBounds, groundItems, new TileSurfaceMap(mapBounds, PrototypeSurfaces.Concrete), new TileObjectMap()),
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
        : this(CreateLocalMapState(mapBounds, groundItems, surfaces, new TileObjectMap()), startPosition, DefaultSiteId)
    {
    }

    public PrototypeGameState(
        GridBounds mapBounds,
        TileItemMap groundItems,
        TileSurfaceMap surfaces,
        TileObjectMap worldObjects,
        GridPosition startPosition
    )
        : this(CreateLocalMapState(mapBounds, groundItems, surfaces, worldObjects), startPosition, DefaultSiteId)
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
        : this(CreateLocalMapState(mapBounds, groundItems, surfaces, worldObjects, npcs), startPosition, DefaultSiteId)
    {
    }

    public PrototypeGameState(LocalMapState localMap, GridPosition startPosition)
        : this(localMap, startPosition, DefaultSiteId)
    {
    }

    public PrototypeGameState(LocalMapState localMap, GridPosition startPosition, string siteId)
        : this(
            localMap,
            startPosition,
            new PlayerState(),
            new WorldTime(),
            new StatefulItemStore(),
            siteId
        )
    {
    }

    public PrototypeGameState(
        LocalMapState localMap,
        GridPosition startPosition,
        PlayerState player,
        WorldTime time,
        StatefulItemStore statefulItems,
        string siteId
    )
    {
        ArgumentNullException.ThrowIfNull(localMap);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(statefulItems);

        if (string.IsNullOrWhiteSpace(siteId))
        {
            throw new ArgumentException("Site id cannot be empty.", nameof(siteId));
        }

        LocalMap = localMap;
        Player = player;
        Time = time;
        StatefulItems = statefulItems;
        SiteId = siteId.Trim();
        Player.SetPosition(localMap.Map.Clamp(startPosition));
    }

    public string SiteId { get; }

    public WorldTime Time { get; }

    public PlayerState Player { get; }

    public LocalMapState LocalMap { get; }

    public StatefulItemStore StatefulItems { get; }

    public GridBounds MapBounds => LocalMap.Map.Bounds;

    public TileItemMap GroundItems => LocalMap.GroundItems;

    public TileSurfaceMap Surfaces => LocalMap.Map.Surfaces;

    public TileObjectMap WorldObjects => LocalMap.WorldObjects;

    public NpcRoster Npcs => LocalMap.Npcs;

    public GridPosition PlayerPosition => Player.Position;

    public int ElapsedTicks => Time.ElapsedTicks;

    public void SetPlayerPosition(GridPosition position)
    {
        if (!LocalMap.Map.Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Player position must be inside the map bounds.");
        }

        Player.SetPosition(position);
    }

    public void AdvanceTime(int ticks)
    {
        Time.Advance(ticks);
    }

    private static LocalMapState CreateLocalMapState(
        GridBounds mapBounds,
        TileItemMap groundItems,
        TileSurfaceMap surfaces,
        TileObjectMap worldObjects,
        NpcRoster? npcs = null
    )
    {
        return new LocalMapState(
            new LocalMap(mapBounds, surfaces),
            groundItems,
            worldObjects,
            npcs
        );
    }
}

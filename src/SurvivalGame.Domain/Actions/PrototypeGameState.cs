namespace SurvivalGame.Domain;

public sealed class PrototypeGameState
{
    public PrototypeGameState(GridBounds mapBounds, TileItemMap groundItems, GridPosition startPosition)
        : this(
            CreateWorldState(mapBounds, groundItems, new TileSurfaceMap(mapBounds, PrototypeSurfaces.Concrete), new TileObjectMap()),
            startPosition
        )
    {
    }

    public PrototypeGameState(
        GridBounds mapBounds,
        TileItemMap groundItems,
        TileSurfaceMap surfaces,
        GridPosition startPosition
    )
        : this(CreateWorldState(mapBounds, groundItems, surfaces, new TileObjectMap()), startPosition)
    {
    }

    public PrototypeGameState(
        GridBounds mapBounds,
        TileItemMap groundItems,
        TileSurfaceMap surfaces,
        TileObjectMap worldObjects,
        GridPosition startPosition
    )
        : this(CreateWorldState(mapBounds, groundItems, surfaces, worldObjects), startPosition)
    {
    }

    public PrototypeGameState(WorldState world, GridPosition startPosition)
    {
        ArgumentNullException.ThrowIfNull(world);

        World = world;
        Player = new PlayerState(world.Map.Clamp(startPosition));
    }

    public TurnState Turn { get; } = new();

    public PlayerState Player { get; }

    public WorldState World { get; }

    public StatefulItemStore StatefulItems { get; } = new();

    public GridBounds MapBounds => World.Map.Bounds;

    public TileItemMap GroundItems => World.GroundItems;

    public TileSurfaceMap Surfaces => World.Map.Surfaces;

    public TileObjectMap WorldObjects => World.WorldObjects;

    public GridPosition PlayerPosition => Player.Position;

    public int TurnCount => Turn.CurrentTurn;

    public void SetPlayerPosition(GridPosition position)
    {
        if (!World.Map.Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Player position must be inside the map bounds.");
        }

        Player.SetPosition(position);
    }

    public void AdvanceTurn()
    {
        Turn.Advance();
    }

    private static WorldState CreateWorldState(
        GridBounds mapBounds,
        TileItemMap groundItems,
        TileSurfaceMap surfaces,
        TileObjectMap worldObjects
    )
    {
        return new WorldState(
            new MapState(mapBounds, surfaces),
            groundItems,
            worldObjects
        );
    }
}

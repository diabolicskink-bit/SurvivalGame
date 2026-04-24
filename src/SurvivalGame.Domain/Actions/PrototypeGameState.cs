namespace SurvivalGame.Domain;

public sealed class PrototypeGameState
{
    public PrototypeGameState(GridBounds mapBounds, TileItemMap groundItems, GridPosition startPosition)
        : this(
            mapBounds,
            groundItems,
            new TileSurfaceMap(mapBounds, PrototypeSurfaces.Concrete),
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
    {
        ArgumentNullException.ThrowIfNull(groundItems);
        ArgumentNullException.ThrowIfNull(surfaces);

        if (surfaces.Bounds != mapBounds)
        {
            throw new ArgumentException("Surface map bounds must match the game state's map bounds.", nameof(surfaces));
        }

        MapBounds = mapBounds;
        GroundItems = groundItems;
        Surfaces = surfaces;
        PlayerPosition = mapBounds.Clamp(startPosition);
    }

    public GridBounds MapBounds { get; }

    public PlayerState Player { get; } = new();

    public TileItemMap GroundItems { get; }

    public TileSurfaceMap Surfaces { get; }

    public GridPosition PlayerPosition { get; private set; }

    public int TurnCount { get; private set; }

    public void SetPlayerPosition(GridPosition position)
    {
        if (!MapBounds.Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Player position must be inside the map bounds.");
        }

        PlayerPosition = position;
    }

    public void AdvanceTurn()
    {
        TurnCount++;
    }
}

namespace SurvivalGame.Domain;

public readonly record struct WorldMapViewport
{
    private WorldMapViewport(
        double mapWidth,
        double mapHeight,
        WorldMapPosition origin,
        double width,
        double height)
    {
        MapWidth = mapWidth;
        MapHeight = mapHeight;
        Origin = origin;
        Width = width;
        Height = height;
    }

    public double MapWidth { get; }

    public double MapHeight { get; }

    public WorldMapPosition Origin { get; }

    public double Width { get; }

    public double Height { get; }

    public double Right => Origin.X + Width;

    public double Bottom => Origin.Y + Height;

    public static WorldMapViewport Create(
        double mapWidth,
        double mapHeight,
        double visibleWidth,
        double visibleHeight,
        WorldMapPosition focus)
    {
        if (mapWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapWidth), "Map width must be positive.");
        }

        if (mapHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapHeight), "Map height must be positive.");
        }

        if (visibleWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(visibleWidth), "Visible width must be positive.");
        }

        if (visibleHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(visibleHeight), "Visible height must be positive.");
        }

        var width = Math.Min(visibleWidth, mapWidth);
        var height = Math.Min(visibleHeight, mapHeight);
        var originX = Math.Clamp(focus.X - (width / 2.0), 0, mapWidth - width);
        var originY = Math.Clamp(focus.Y - (height / 2.0), 0, mapHeight - height);

        return new WorldMapViewport(
            mapWidth,
            mapHeight,
            new WorldMapPosition(originX, originY),
            width,
            height
        );
    }

    public bool Contains(WorldMapPosition position)
    {
        return position.X >= Origin.X
            && position.X <= Right
            && position.Y >= Origin.Y
            && position.Y <= Bottom;
    }

    public WorldMapPosition MapToViewport(WorldMapPosition position)
    {
        return new WorldMapPosition(position.X - Origin.X, position.Y - Origin.Y);
    }

    public WorldMapPosition ViewportToMap(WorldMapPosition position)
    {
        return new WorldMapPosition(
            Math.Clamp(Origin.X + position.X, 0, MapWidth),
            Math.Clamp(Origin.Y + position.Y, 0, MapHeight)
        );
    }
}

namespace SurvivalGame.Domain;

public readonly record struct GridViewport
{
    private GridViewport(GridBounds mapBounds, GridPosition origin, int width, int height)
    {
        MapBounds = mapBounds;
        Origin = origin;
        Width = width;
        Height = height;
    }

    public GridBounds MapBounds { get; }

    public GridPosition Origin { get; }

    public int Width { get; }

    public int Height { get; }

    public GridPosition CenterCell => new(Width / 2, Height / 2);

    public static GridViewport Create(GridBounds mapBounds, GridPosition focus, int width, int height)
    {
        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Viewport width must be at least 1.");
        }

        if (height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Viewport height must be at least 1.");
        }

        var clampedFocus = mapBounds.Clamp(focus);
        var origin = new GridPosition(
            CalculateOrigin(clampedFocus.X, mapBounds.Width, width),
            CalculateOrigin(clampedFocus.Y, mapBounds.Height, height)
        );

        return new GridViewport(mapBounds, origin, width, height);
    }

    public bool ContainsViewportPosition(GridPosition viewportPosition)
    {
        return viewportPosition.X >= 0
            && viewportPosition.Y >= 0
            && viewportPosition.X < Width
            && viewportPosition.Y < Height;
    }

    public bool IsMapPositionVisible(GridPosition mapPosition)
    {
        return TryMapToViewport(mapPosition, out _);
    }

    public bool TryMapToViewport(GridPosition mapPosition, out GridPosition viewportPosition)
    {
        viewportPosition = new GridPosition(mapPosition.X - Origin.X, mapPosition.Y - Origin.Y);
        return MapBounds.Contains(mapPosition) && ContainsViewportPosition(viewportPosition);
    }

    public bool TryViewportToMap(GridPosition viewportPosition, out GridPosition mapPosition)
    {
        mapPosition = new GridPosition(viewportPosition.X + Origin.X, viewportPosition.Y + Origin.Y);
        return ContainsViewportPosition(viewportPosition) && MapBounds.Contains(mapPosition);
    }

    private static int CalculateOrigin(int focus, int mapLength, int viewportLength)
    {
        if (mapLength <= viewportLength)
        {
            return -((viewportLength - mapLength) / 2);
        }

        var centeredOrigin = focus - (viewportLength / 2);
        var maxOrigin = mapLength - viewportLength;
        return Math.Clamp(centeredOrigin, 0, maxOrigin);
    }
}

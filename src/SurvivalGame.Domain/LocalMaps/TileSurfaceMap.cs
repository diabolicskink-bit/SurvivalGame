namespace SurvivalGame.Domain;

public sealed class TileSurfaceMap
{
    private readonly Dictionary<GridPosition, SurfaceId> _surfaceOverrides = new();

    public TileSurfaceMap(GridBounds bounds, SurfaceId defaultSurfaceId)
    {
        ArgumentNullException.ThrowIfNull(defaultSurfaceId);

        Bounds = bounds;
        DefaultSurfaceId = defaultSurfaceId;
    }

    public GridBounds Bounds { get; }

    public SurfaceId DefaultSurfaceId { get; }

    public SurfaceId GetSurfaceId(GridPosition position)
    {
        ValidatePosition(position);

        return _surfaceOverrides.TryGetValue(position, out var surfaceId)
            ? surfaceId
            : DefaultSurfaceId;
    }

    public void SetSurface(GridPosition position, SurfaceId surfaceId)
    {
        ArgumentNullException.ThrowIfNull(surfaceId);
        ValidatePosition(position);

        if (surfaceId == DefaultSurfaceId)
        {
            _surfaceOverrides.Remove(position);
            return;
        }

        _surfaceOverrides[position] = surfaceId;
    }

    private void ValidatePosition(GridPosition position)
    {
        if (!Bounds.Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Surface position must be inside the map bounds.");
        }
    }
}

namespace SurvivalGame.Domain;

public sealed class LocalMap
{
    public LocalMap(GridBounds bounds, TileSurfaceMap surfaces)
    {
        ArgumentNullException.ThrowIfNull(surfaces);

        if (surfaces.Bounds != bounds)
        {
            throw new ArgumentException("Surface map bounds must match map bounds.", nameof(surfaces));
        }

        Bounds = bounds;
        Surfaces = surfaces;
    }

    public GridBounds Bounds { get; }

    public TileSurfaceMap Surfaces { get; }

    public bool Contains(GridPosition position)
    {
        return Bounds.Contains(position);
    }

    public GridPosition Clamp(GridPosition position)
    {
        return Bounds.Clamp(position);
    }
}

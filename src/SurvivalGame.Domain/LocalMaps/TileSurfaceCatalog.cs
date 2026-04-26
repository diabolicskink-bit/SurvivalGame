namespace SurvivalGame.Domain;

public sealed class TileSurfaceCatalog
{
    private readonly Dictionary<SurfaceId, TileSurfaceDefinition> _surfaces = new();

    public IReadOnlyCollection<TileSurfaceDefinition> Surfaces => _surfaces.Values.ToArray();

    public void Add(TileSurfaceDefinition surface)
    {
        ArgumentNullException.ThrowIfNull(surface);

        if (!_surfaces.TryAdd(surface.Id, surface))
        {
            throw new InvalidOperationException($"Surface '{surface.Id}' is already defined.");
        }
    }

    public bool TryGet(SurfaceId id, out TileSurfaceDefinition surface)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (_surfaces.TryGetValue(id, out var foundSurface))
        {
            surface = foundSurface;
            return true;
        }

        surface = null!;
        return false;
    }

    public TileSurfaceDefinition Get(SurfaceId id)
    {
        if (TryGet(id, out var surface))
        {
            return surface;
        }

        throw new KeyNotFoundException($"Surface '{id}' is not defined.");
    }
}

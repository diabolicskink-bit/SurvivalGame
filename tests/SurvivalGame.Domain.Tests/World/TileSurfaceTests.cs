using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class TileSurfaceTests
{
    [Fact]
    public void SurfaceDataLoadsPrototypeSurfaces()
    {
        var catalog = LoadSurfaceCatalog();

        Assert.Equal(5, catalog.Surfaces.Count);
        Assert.Equal("Grass", catalog.Get(PrototypeSurfaces.Grass).Name);
        Assert.Equal("Concrete", catalog.Get(PrototypeSurfaces.Concrete).Name);
        Assert.Equal("surface_grass", catalog.Get(PrototypeSurfaces.Grass).SpriteId);
    }

    [Fact]
    public void IceDefinitionCanCarrySlipperyProperty()
    {
        var catalog = LoadSurfaceCatalog();

        var ice = catalog.Get(PrototypeSurfaces.Ice);

        Assert.True(ice.HasTag("slippery"));
        Assert.True(ice.HasTag("slick"));
        Assert.Equal(1, ice.MovementCost);
    }

    [Fact]
    public void SurfaceMapReturnsDefaultSurfaceUntilOverridden()
    {
        var bounds = new GridBounds(5, 5);
        var surfaces = new TileSurfaceMap(bounds, PrototypeSurfaces.Grass);

        surfaces.SetSurface(new GridPosition(2, 2), PrototypeSurfaces.Ice);

        Assert.Equal(PrototypeSurfaces.Grass, surfaces.GetSurfaceId(new GridPosition(0, 0)));
        Assert.Equal(PrototypeSurfaces.Ice, surfaces.GetSurfaceId(new GridPosition(2, 2)));
    }

    [Fact]
    public void SurfaceMapRejectsPositionsOutsideBounds()
    {
        var surfaces = new TileSurfaceMap(new GridBounds(5, 5), PrototypeSurfaces.Grass);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            surfaces.SetSurface(new GridPosition(5, 0), PrototypeSurfaces.Concrete));
    }

    private static TileSurfaceCatalog LoadSurfaceCatalog()
    {
        return new TileSurfaceDefinitionLoader().LoadDirectory(GetSurfaceDataPath());
    }

    private static string GetSurfaceDataPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var surfaceDataPath = Path.Combine(directory.FullName, "data", "surfaces");
            if (Directory.Exists(surfaceDataPath))
            {
                return surfaceDataPath;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate data/surfaces from the test output directory.");
    }
}

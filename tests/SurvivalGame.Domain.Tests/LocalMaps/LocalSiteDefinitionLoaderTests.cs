using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class LocalSiteDefinitionLoaderTests
{
    [Fact]
    public void LoadsCurrentAuthoredLocalMapsFromData()
    {
        var sites = LoadAllSites();

        var defaultSite = Assert.Single(sites, site => site.Id == PrototypeLocalSites.DefaultSiteId);
        Assert.Equal(PrototypeLocalSites.DefaultBounds, defaultSite.Bounds);
        Assert.Equal(PrototypeLocalSites.DefaultBounds.Center, defaultSite.StartPosition);
        Assert.Equal(PrototypeSurfaces.Concrete, defaultSite.Surfaces.GetSurfaceId(new GridPosition(2, 2)));
        Assert.Equal(PrototypeSurfaces.Carpet, defaultSite.Surfaces.GetSurfaceId(new GridPosition(3, 3)));
        Assert.Equal(PrototypeSurfaces.Tile, defaultSite.Surfaces.GetSurfaceId(new GridPosition(11, 2)));
        Assert.Equal(PrototypeSurfaces.Ice, defaultSite.Surfaces.GetSurfaceId(new GridPosition(12, 8)));
        Assert.True(defaultSite.WorldObjects.TryGetObjectAt(new GridPosition(6, 6), out var door));
        Assert.Equal(PrototypeWorldObjects.WoodenDoor, door);
        Assert.Contains(
            defaultSite.GroundItems.ItemsAt(new GridPosition(4, 4)),
            stack => stack.ItemId == PrototypeItems.Stone && stack.Quantity == 2);
        Assert.True(defaultSite.Npcs.TryGetAt(new GridPosition(14, 8), out var testDummy));
        Assert.Equal(PrototypeNpcs.TestDummy, testDummy.Id);

        var gasStation = Assert.Single(sites, site => site.Id == PrototypeLocalSites.GasStationSiteId);
        Assert.Equal(PrototypeLocalSites.GasStationBounds, gasStation.Bounds);
        Assert.Equal(new GridPosition(21, 14), gasStation.StartPosition);
        Assert.True(gasStation.WorldObjects.TryGetObjectAt(new GridPosition(25, 9), out var pump));
        Assert.Equal(PrototypeWorldObjects.FuelPump, pump);
        Assert.True(gasStation.Npcs.TryGetAt(new GridPosition(30, 12), out var turret));
        Assert.Equal(PrototypeNpcs.GasStationTurret, turret.Id);
    }

    [Fact]
    public void AuthoredMapRejectsWrongRowWidth()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "bad_width",
              "displayName": "Bad Width",
              "sourceKind": "authored",
              "size": { "width": 2, "height": 2 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "grass",
              "surfaceLayer": {
                "legend": { "g": "grass" },
                "rows": [ "gg", "g" ]
              }
            }
            """));

        Assert.Contains("row 1", ex.Message);
    }

    [Fact]
    public void AuthoredMapRejectsUnknownIds()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "bad_surface",
              "displayName": "Bad Surface",
              "sourceKind": "authored",
              "size": { "width": 1, "height": 1 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "missing_surface"
            }
            """));

        Assert.Contains("missing_surface", ex.Message);
    }

    [Fact]
    public void AuthoredMapRejectsOutOfBoundsPlacements()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "bad_item_position",
              "displayName": "Bad Item Position",
              "sourceKind": "authored",
              "size": { "width": 1, "height": 1 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "grass",
              "items": [
                { "itemId": "stone", "quantity": 1, "x": 2, "y": 0 }
              ]
            }
            """));

        Assert.Contains("inside map bounds", ex.Message);
    }

    [Fact]
    public void AuthoredMapRejectsDuplicateObjectPlacements()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "duplicate_object",
              "displayName": "Duplicate Object",
              "sourceKind": "authored",
              "size": { "width": 1, "height": 1 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "grass",
              "objectLayer": {
                "legend": { "#": "wall" },
                "rows": [ "#" ]
              },
              "objectPlacements": [
                { "objectId": "tree", "x": 0, "y": 0 }
              ]
            }
            """));

        Assert.Contains("already has a world object", ex.Message);
    }

    [Theory]
    [InlineData("recipe", "Recipe map generation is not implemented yet.")]
    [InlineData("chunkedProcedural", "Chunked procedural map generation is not implemented yet.")]
    public void NonAuthoredSourceKindsFailClearly(string sourceKind, string expectedMessage)
    {
        var ex = Assert.Throws<NotSupportedException>(() => LoadSingleFromJson(
            $$"""
            {
              "sourceKind": "{{sourceKind}}"
            }
            """));

        Assert.Equal(expectedMessage, ex.Message);
    }

    private static IReadOnlyList<PrototypeLocalSite> LoadAllSites()
    {
        return new LocalSiteDefinitionLoader().LoadDirectory(
            GetDataPath("local_maps"),
            LoadSurfaceCatalog(),
            LoadWorldObjectCatalog(),
            LoadItemCatalog(),
            LoadNpcCatalog()
        );
    }

    private static PrototypeLocalSite LoadSingleFromJson(string json)
    {
        var directory = Directory.CreateTempSubdirectory("survival-map-loader-tests-");
        try
        {
            var filePath = Path.Combine(directory.FullName, "map.json");
            File.WriteAllText(filePath, json);
            return new LocalSiteDefinitionLoader().LoadFile(
                filePath,
                LoadSurfaceCatalog(),
                LoadWorldObjectCatalog(),
                LoadItemCatalog(),
                LoadNpcCatalog()
            );
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }

    private static TileSurfaceCatalog LoadSurfaceCatalog()
    {
        return new TileSurfaceDefinitionLoader().LoadDirectory(GetDataPath("surfaces"));
    }

    private static WorldObjectCatalog LoadWorldObjectCatalog()
    {
        return new WorldObjectDefinitionLoader().LoadDirectory(GetDataPath("world_objects"));
    }

    private static ItemCatalog LoadItemCatalog()
    {
        return new ItemDefinitionLoader().LoadDirectory(GetDataPath("items"));
    }

    private static NpcCatalog LoadNpcCatalog()
    {
        return new NpcDefinitionLoader().LoadDirectory(GetDataPath("npcs"));
    }

    private static string GetDataPath(string childDirectory)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var dataPath = Path.Combine(directory.FullName, "data", childDirectory);
            if (Directory.Exists(dataPath))
            {
                return dataPath;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Could not locate data/{childDirectory} from the test output directory.");
    }
}

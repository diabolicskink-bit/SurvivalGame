using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class FarmhouseSiteTests
{
    private static readonly HashSet<WorldObjectId> OldStructureObjectIds =
    [
        new("farmhouse_wall"),
        new("shed_wall"),
        new("open_wooden_door"),
        new("interior_doorway"),
        new("screen_door"),
        new("broken_window"),
        new("boarded_window"),
        new("wire_fence"),
        new("timber_fence"),
        new("broken_fence_gap"),
        new("open_farm_gate"),
    ];

    [Fact]
    public void FarmhouseMapHasExpectedSizeEntryAndMajorSurfaceZones()
    {
        var site = LoadSite(PrototypeLocalSites.FarmsteadSiteId);

        Assert.Equal(PrototypeLocalSites.FarmsteadSiteId, site.Id);
        Assert.Equal(PrototypeLocalSites.FarmsteadBounds, site.Bounds);
        Assert.Equal(new GridPosition(4, 41), site.StartPosition);
        Assert.Equal(PrototypeSurfaces.Dirt, site.Surfaces.GetSurfaceId(site.StartPosition));
        Assert.Equal(PrototypeSurfaces.WeatheredWood, site.Surfaces.GetSurfaceId(new GridPosition(26, 30)));
        Assert.Equal(PrototypeSurfaces.WeatheredWood, site.Surfaces.GetSurfaceId(new GridPosition(20, 15)));
        Assert.Equal(PrototypeSurfaces.Linoleum, site.Surfaces.GetSurfaceId(new GridPosition(35, 24)));
        Assert.Equal(PrototypeSurfaces.Tile, site.Surfaces.GetSurfaceId(new GridPosition(31, 20)));
        Assert.Equal(PrototypeSurfaces.Concrete, site.Surfaces.GetSurfaceId(new GridPosition(47, 15)));
        Assert.Equal(PrototypeSurfaces.Gravel, site.Surfaces.GetSurfaceId(new GridPosition(50, 29)));
        Assert.Equal(PrototypeSurfaces.Grass, site.Surfaces.GetSurfaceId(new GridPosition(48, 38)));
        Assert.Equal(PrototypeSurfaces.Scrub, site.Surfaces.GetSurfaceId(new GridPosition(61, 38)));
    }

    [Fact]
    public void FarmhouseMapContainsReadableHouseShedAndFarmObjects()
    {
        var site = LoadSite(PrototypeLocalSites.FarmsteadSiteId);

        AssertObject(site, new GridPosition(17, 13), "wall");
        AssertObject(site, new GridPosition(32, 13), "window");
        AssertObject(site, new GridPosition(46, 15), "wall");
        AssertNoObject(site, new GridPosition(26, 28));
        AssertNoObject(site, new GridPosition(40, 22));
        AssertNoObject(site, new GridPosition(54, 15));
        AssertNoObject(site, new GridPosition(25, 17));
        AssertNoObject(site, new GridPosition(35, 22));
        AssertObject(site, new GridPosition(47, 16), "workbench");
        AssertObject(site, new GridPosition(58, 16), "metal_shelf");
        AssertObject(site, new GridPosition(44, 9), "water_tank");
        AssertObject(site, new GridPosition(51, 30), "tractor_wreck");
        AssertObject(site, new GridPosition(56, 32), "farm_trailer");
        AssertObject(site, new GridPosition(50, 38), "trough");
        AssertStructure(site, new GridPosition(44, 34), StructureEdgeDirection.South, "open_farm_gate");
        AssertStructure(site, new GridPosition(55, 42), StructureEdgeDirection.South, "broken_fence_gap");
        AssertObject(site, new GridPosition(61, 38), "scrub_thicket");
        Assert.DoesNotContain(site.WorldObjects.AllObjects, placedObject => OldStructureObjectIds.Contains(placedObject.ObjectId));
    }

    [Fact]
    public void FarmhouseMovementBlocksHeavyFixturesButAllowsOpenRoutes()
    {
        var site = LoadSite(PrototypeLocalSites.FarmsteadSiteId);
        var pipeline = new GameActionPipeline(
            LoadItemCatalog(),
            LoadWorldObjectCatalog(),
            structureCatalog: LoadStructureCatalog());

        var wallState = CreateState(site, new GridPosition(16, 16));
        var blockedByWall = pipeline.Execute(new MoveActionRequest(GridOffset.Right), wallState);
        Assert.False(blockedByWall.Succeeded);
        Assert.Equal(new GridPosition(16, 16), wallState.Player.Position);

        var tankState = CreateState(site, new GridPosition(41, 8));
        var blockedByTank = pipeline.Execute(new MoveActionRequest(GridOffset.Right), tankState);
        Assert.False(blockedByTank.Succeeded);
        Assert.Equal(new GridPosition(41, 8), tankState.Player.Position);

        var fenceState = CreateState(site, new GridPosition(38, 35));
        var blockedByFence = pipeline.Execute(new MoveActionRequest(GridOffset.Right), fenceState);
        Assert.False(blockedByFence.Succeeded);
        Assert.Equal(new GridPosition(38, 35), fenceState.Player.Position);

        var frontDoorState = CreateState(site, new GridPosition(26, 29));
        var throughFrontDoor = pipeline.Execute(new MoveActionRequest(GridOffset.Up), frontDoorState);
        Assert.True(throughFrontDoor.Succeeded);
        Assert.Equal(new GridPosition(26, 28), frontDoorState.Player.Position);

        var interiorDoorwayState = CreateState(site, new GridPosition(24, 17));
        var throughInteriorDoorway = pipeline.Execute(new MoveActionRequest(GridOffset.Right), interiorDoorwayState);
        Assert.True(throughInteriorDoorway.Succeeded);
        Assert.Equal(new GridPosition(25, 17), interiorDoorwayState.Player.Position);

        var gateState = CreateState(site, new GridPosition(44, 35));
        var throughGate = pipeline.Execute(new MoveActionRequest(GridOffset.Up), gateState);
        Assert.True(throughGate.Succeeded);
        Assert.Equal(new GridPosition(44, 34), gateState.Player.Position);

        var brokenFenceState = CreateState(site, new GridPosition(55, 42));
        var throughBrokenFence = pipeline.Execute(new MoveActionRequest(GridOffset.Down), brokenFenceState);
        Assert.True(throughBrokenFence.Succeeded);
        Assert.Equal(new GridPosition(55, 43), brokenFenceState.Player.Position);
    }

    [Fact]
    public void FarmhouseMapPlacesThemedGroundItemsAcrossPropertyZones()
    {
        var site = LoadSite(PrototypeLocalSites.FarmsteadSiteId);

        AssertItem(site, new GridPosition(35, 20), "canned_peaches", 2);
        AssertItem(site, new GridPosition(31, 20), "antiseptic_wipes", 3);
        AssertItem(site, new GridPosition(21, 15), "flannel_shirt", 1);
        AssertItem(site, new GridPosition(39, 26), "rubber_boots", 1);
        AssertItem(site, new GridPosition(49, 17), "hammer", 1);
        AssertItem(site, new GridPosition(56, 24), "bolt_cutters", 1);
        AssertItem(site, new GridPosition(46, 31), "scrap_metal", 4);
        AssertItem(site, new GridPosition(43, 39), "wood_planks", 2);
        AssertItem(site, new GridPosition(12, 27), "garden_trowel", 1);
    }

    [Fact]
    public void FarmhouseMapDoesNotSpawnNpcs()
    {
        var site = LoadSite(PrototypeLocalSites.FarmsteadSiteId);

        Assert.Empty(site.Npcs.AllNpcs);
    }

    private static void AssertObject(PrototypeLocalSite site, GridPosition position, string expectedObjectId)
    {
        Assert.True(site.WorldObjects.TryGetObjectAt(position, out var objectId));
        Assert.Equal(new WorldObjectId(expectedObjectId), objectId);
    }

    private static void AssertNoObject(PrototypeLocalSite site, GridPosition position)
    {
        Assert.False(site.WorldObjects.TryGetObjectAt(position, out _));
    }

    private static void AssertStructure(
        PrototypeLocalSite site,
        GridPosition position,
        StructureEdgeDirection direction,
        string expectedStructureId)
    {
        Assert.True(site.Structures.TryGetEdgeAt(position, direction, out var edge));
        Assert.Equal(new StructureId(expectedStructureId), edge.StructureId);
    }

    private static void AssertItem(
        PrototypeLocalSite site,
        GridPosition position,
        string expectedItemId,
        int expectedQuantity)
    {
        Assert.Contains(
            site.GroundItems.ItemsAt(position),
            stack => stack.ItemId == new ItemId(expectedItemId) && stack.Quantity == expectedQuantity);
    }

    private static PrototypeGameState CreateState(PrototypeLocalSite site, GridPosition playerPosition)
    {
        return new PrototypeGameState(
            new LocalMapState(
                new LocalMap(site.Bounds, site.Surfaces),
                site.GroundItems,
                site.WorldObjects,
                site.Npcs,
                structures: site.Structures
            ),
            playerPosition,
            new PlayerState(),
            new WorldTime(),
            new StatefulItemStore(),
            site.Id
        );
    }

    private static PrototypeLocalSite LoadSite(SiteId siteId)
    {
        return new LocalSiteDefinitionLoader()
            .LoadDirectory(
                GetDataPath("local_maps"),
                LoadSurfaceCatalog(),
                LoadWorldObjectCatalog(),
                LoadStructureCatalog(),
                LoadItemCatalog(),
                LoadNpcCatalog()
            )
            .Single(site => site.Id == siteId);
    }

    private static TileSurfaceCatalog LoadSurfaceCatalog()
    {
        return new TileSurfaceDefinitionLoader().LoadDirectory(GetDataPath("surfaces"));
    }

    private static WorldObjectCatalog LoadWorldObjectCatalog()
    {
        return new WorldObjectDefinitionLoader().LoadDirectory(GetDataPath("world_objects"));
    }

    private static StructureCatalog LoadStructureCatalog()
    {
        return new StructureDefinitionLoader().LoadDirectory(GetDataPath("structures"));
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

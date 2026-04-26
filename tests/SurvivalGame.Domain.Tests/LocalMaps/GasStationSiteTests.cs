using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class GasStationSiteTests
{
    [Fact]
    public void GasStationMapHasExpectedSizeAndCoreObjects()
    {
        var site = LoadSite(PrototypeLocalSites.GasStationSiteId);

        Assert.Equal(PrototypeLocalSites.GasStationSiteId, site.Id);
        Assert.Equal(new GridBounds(40, 28), site.Bounds);
        Assert.True(site.Bounds.Contains(site.StartPosition));
        Assert.True(site.WorldObjects.TryGetObjectAt(new GridPosition(25, 9), out var pump));
        Assert.Equal(PrototypeWorldObjects.FuelPump, pump);
        Assert.True(site.WorldObjects.TryGetObjectAt(new GridPosition(12, 13), out var door));
        Assert.Equal(PrototypeWorldObjects.GlassDoor, door);
        Assert.True(site.WorldObjects.TryGetObjectAt(new GridPosition(15, 11), out var counter));
        Assert.Equal(PrototypeWorldObjects.CheckoutCounter, counter);
        Assert.True(site.WorldObjects.TryGetObjectAt(new GridPosition(7, 6), out var shelf));
        Assert.Equal(PrototypeWorldObjects.StoreShelf, shelf);
        Assert.True(site.WorldObjects.TryGetObjectAt(new GridPosition(33, 21), out var vehicle));
        Assert.Equal(PrototypeWorldObjects.AbandonedVehicle, vehicle);
        Assert.True(site.WorldObjects.TryGetPlacementAt(new GridPosition(35, 21), out var vehiclePlacement));
        Assert.Equal(new GridPosition(32, 20), vehiclePlacement.Position);
        Assert.Equal(WorldObjectFacing.East, vehiclePlacement.Facing);
        Assert.Equal(new WorldObjectFootprint(2, 4), vehiclePlacement.Footprint);
        Assert.Equal(new WorldObjectFootprint(4, 2), vehiclePlacement.EffectiveFootprint);
        Assert.False(site.WorldObjects.TryGetObjectAt(new GridPosition(36, 21), out _));
        Assert.False(site.WorldObjects.TryGetObjectAt(new GridPosition(35, 22), out _));
        Assert.False(site.WorldObjects.TryGetObjectAt(new GridPosition(30, 12), out _));
        Assert.True(site.Npcs.TryGetAt(new GridPosition(30, 12), out var turret));
        Assert.Equal(PrototypeNpcs.GasStationTurret, turret.Id);
        Assert.Equal(PrototypeNpcs.AutomatedTurretDefinition, turret.DefinitionId);
    }

    [Fact]
    public void GasStationMapUsesAsphaltForecourtAndTileStore()
    {
        var site = LoadSite(PrototypeLocalSites.GasStationSiteId);

        Assert.Equal(PrototypeSurfaces.Asphalt, site.Surfaces.GetSurfaceId(new GridPosition(21, 14)));
        Assert.Equal(PrototypeSurfaces.Tile, site.Surfaces.GetSurfaceId(new GridPosition(6, 5)));
        Assert.Equal(PrototypeSurfaces.Concrete, site.Surfaces.GetSurfaceId(new GridPosition(24, 9)));
        Assert.Equal(PrototypeSurfaces.Grass, site.Surfaces.GetSurfaceId(new GridPosition(0, 0)));
    }

    [Fact]
    public void MovementBlocksOnGasStationFixturesButNotGlassDoor()
    {
        var site = LoadSite(PrototypeLocalSites.GasStationSiteId);
        var state = CreateState(site, new GridPosition(24, 9));
        var pipeline = new GameActionPipeline(new ItemCatalog(), LoadWorldObjectCatalog());

        var blockedByPump = pipeline.Execute(new MoveActionRequest(GridOffset.Right), state);

        Assert.False(blockedByPump.Succeeded);
        Assert.Equal(new GridPosition(24, 9), state.Player.Position);

        state.SetPlayerPosition(new GridPosition(12, 14));
        var throughDoor = pipeline.Execute(new MoveActionRequest(GridOffset.Up), state);

        Assert.True(throughDoor.Succeeded);
        Assert.Equal(new GridPosition(12, 13), state.Player.Position);

        state.SetPlayerPosition(new GridPosition(31, 21));
        var blockedByVehicleFootprint = pipeline.Execute(new MoveActionRequest(GridOffset.Right), state);

        Assert.False(blockedByVehicleFootprint.Succeeded);
        Assert.Equal(new GridPosition(31, 21), state.Player.Position);

        state.SetPlayerPosition(new GridPosition(37, 21));
        var aroundVehicleFootprint = pipeline.Execute(new MoveActionRequest(GridOffset.Left), state);

        Assert.True(aroundVehicleFootprint.Succeeded);
        Assert.Equal(new GridPosition(36, 21), state.Player.Position);
    }

    [Fact]
    public void RefuelActionIsAvailableOnlyNextToFuelPump()
    {
        var site = LoadSite(PrototypeLocalSites.GasStationSiteId);
        var fuel = new VehicleFuelState(PrototypeTravelMethods.VehicleFuelCapacity, 4);
        var state = CreateState(site, new GridPosition(24, 9));
        var pipeline = new GameActionPipeline(new ItemCatalog(), LoadWorldObjectCatalog(), vehicleFuelState: fuel);

        var nearPumpActions = pipeline.GetAvailableActions(state);

        Assert.Contains(nearPumpActions, action => action.Kind == GameActionKind.RefuelVehicle);

        state.SetPlayerPosition(site.StartPosition);
        var awayFromPumpActions = pipeline.GetAvailableActions(state);

        Assert.DoesNotContain(awayFromPumpActions, action => action.Kind == GameActionKind.RefuelVehicle);
    }

    [Fact]
    public void RefuelActionRestoresFuelAndAdvancesTime()
    {
        var site = LoadSite(PrototypeLocalSites.GasStationSiteId);
        var fuel = new VehicleFuelState(PrototypeTravelMethods.VehicleFuelCapacity, 4);
        var state = CreateState(site, new GridPosition(24, 9));
        var pipeline = new GameActionPipeline(new ItemCatalog(), LoadWorldObjectCatalog(), vehicleFuelState: fuel);

        var result = pipeline.Execute(new RefuelVehicleActionRequest(), state);

        Assert.True(result.Succeeded);
        Assert.Equal(PrototypeTravelMethods.VehicleFuelCapacity, fuel.CurrentFuel);
        Assert.Equal(GameActionPipeline.RefuelVehicleTickCost, state.Time.ElapsedTicks);
    }

    [Fact]
    public void StatefulGroundItemsAreScopedToLocalSite()
    {
        var gasStation = LoadSite(PrototypeLocalSites.GasStationSiteId);
        var store = new StatefulItemStore();
        var item = store.Create(
            PrototypeItems.Stone,
            1,
            StatefulItemLocation.Ground(new GridPosition(11, 7), PrototypeGameState.DefaultSiteId)
        );

        var groundLoc = Assert.IsType<GroundLocation>(item.Location);
        Assert.Single(store.OnGround(new GridPosition(11, 7), PrototypeGameState.DefaultSiteId));
        Assert.Empty(store.OnGround(new GridPosition(11, 7), gasStation.Id));
        Assert.Equal(PrototypeGameState.DefaultSiteId, groundLoc.SiteId);
    }

    private static PrototypeGameState CreateState(PrototypeLocalSite site, GridPosition playerPosition)
    {
        return new PrototypeGameState(
            new LocalMapState(
                new LocalMap(site.Bounds, site.Surfaces),
                site.GroundItems,
                site.WorldObjects,
                site.Npcs
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
                GetLocalMapDataPath(),
                LoadSurfaceCatalog(),
                LoadWorldObjectCatalog(),
                LoadItemCatalog(),
                LoadNpcCatalog()
            )
            .Single(site => site.Id == siteId);
    }

    private static TileSurfaceCatalog LoadSurfaceCatalog()
    {
        return new TileSurfaceDefinitionLoader().LoadDirectory(GetSurfaceDataPath());
    }

    private static WorldObjectCatalog LoadWorldObjectCatalog()
    {
        return new WorldObjectDefinitionLoader().LoadDirectory(GetWorldObjectDataPath());
    }

    private static ItemCatalog LoadItemCatalog()
    {
        return new ItemDefinitionLoader().LoadDirectory(GetItemDataPath());
    }

    private static NpcCatalog LoadNpcCatalog()
    {
        return new NpcDefinitionLoader().LoadDirectory(GetNpcDataPath());
    }

    private static string GetLocalMapDataPath()
    {
        return GetDataPath("local_maps");
    }

    private static string GetSurfaceDataPath()
    {
        return GetDataPath("surfaces");
    }

    private static string GetWorldObjectDataPath()
    {
        return GetDataPath("world_objects");
    }

    private static string GetItemDataPath()
    {
        return GetDataPath("items");
    }

    private static string GetNpcDataPath()
    {
        return GetDataPath("npcs");
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

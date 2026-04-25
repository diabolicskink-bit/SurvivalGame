using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class GasStationSiteTests
{
    [Fact]
    public void GasStationMapHasExpectedSizeAndCoreObjects()
    {
        var site = PrototypeLocalSites.CreateGasStation();

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
    }

    [Fact]
    public void GasStationMapUsesAsphaltForecourtAndTileStore()
    {
        var site = PrototypeLocalSites.CreateGasStation();

        Assert.Equal(PrototypeSurfaces.Asphalt, site.Surfaces.GetSurfaceId(new GridPosition(21, 14)));
        Assert.Equal(PrototypeSurfaces.Tile, site.Surfaces.GetSurfaceId(new GridPosition(6, 5)));
        Assert.Equal(PrototypeSurfaces.Concrete, site.Surfaces.GetSurfaceId(new GridPosition(24, 9)));
        Assert.Equal(PrototypeSurfaces.Grass, site.Surfaces.GetSurfaceId(new GridPosition(0, 0)));
    }

    [Fact]
    public void MovementBlocksOnGasStationFixturesButNotGlassDoor()
    {
        var site = PrototypeLocalSites.CreateGasStation();
        var state = CreateState(site, new GridPosition(24, 9));
        var pipeline = new GameActionPipeline(new ItemCatalog(), LoadWorldObjectCatalog());

        var blockedByPump = pipeline.Execute(state, new MoveActionRequest(GridOffset.Right));

        Assert.False(blockedByPump.Succeeded);
        Assert.Equal(new GridPosition(24, 9), state.Player.Position);

        state.SetPlayerPosition(new GridPosition(12, 14));
        var throughDoor = pipeline.Execute(state, new MoveActionRequest(GridOffset.Up));

        Assert.True(throughDoor.Succeeded);
        Assert.Equal(new GridPosition(12, 13), state.Player.Position);
    }

    [Fact]
    public void RefuelActionIsAvailableOnlyNextToFuelPump()
    {
        var site = PrototypeLocalSites.CreateGasStation();
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
        var site = PrototypeLocalSites.CreateGasStation();
        var fuel = new VehicleFuelState(PrototypeTravelMethods.VehicleFuelCapacity, 4);
        var state = CreateState(site, new GridPosition(24, 9));
        var pipeline = new GameActionPipeline(new ItemCatalog(), LoadWorldObjectCatalog(), vehicleFuelState: fuel);

        var result = pipeline.Execute(state, new RefuelVehicleActionRequest());

        Assert.True(result.Succeeded);
        Assert.Equal(PrototypeTravelMethods.VehicleFuelCapacity, fuel.CurrentFuel);
        Assert.Equal(GameActionPipeline.RefuelVehicleTickCost, state.Time.ElapsedTicks);
    }

    [Fact]
    public void StatefulGroundItemsAreScopedToLocalSite()
    {
        var gasStation = PrototypeLocalSites.CreateGasStation();
        var store = new StatefulItemStore();
        var item = store.Create(
            PrototypeItems.Stone,
            1,
            StatefulItemLocation.Ground(new GridPosition(11, 7), PrototypeGameState.DefaultSiteId)
        );

        Assert.Single(store.OnGround(new GridPosition(11, 7), PrototypeGameState.DefaultSiteId));
        Assert.Empty(store.OnGround(new GridPosition(11, 7), gasStation.Id));
        Assert.Equal(PrototypeGameState.DefaultSiteId, item.Location.SiteId);
    }

    private static PrototypeGameState CreateState(PrototypeLocalSite site, GridPosition playerPosition)
    {
        return new PrototypeGameState(
            new WorldState(
                new MapState(site.Bounds, site.Surfaces),
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

    private static WorldObjectCatalog LoadWorldObjectCatalog()
    {
        return new WorldObjectDefinitionLoader().LoadDirectory(GetWorldObjectDataPath());
    }

    private static string GetWorldObjectDataPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var objectDataPath = Path.Combine(directory.FullName, "data", "world_objects");
            if (Directory.Exists(objectDataPath))
            {
                return objectDataPath;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate data/world_objects from the test output directory.");
    }
}

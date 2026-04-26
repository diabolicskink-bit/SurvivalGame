using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class WorldMapTravelStateTests
{
    [Fact]
    public void PrototypeWorldMapSitesStayInsideMapAndUseUniqueIds()
    {
        var ids = PrototypeWorldMapSites.All.Select(site => site.Id).ToArray();

        Assert.True(IsInsideMap(PrototypeWorldMapSites.StartPosition));
        Assert.Equal(12, PrototypeWorldMapSites.All.Count);
        Assert.Equal(ids.Length, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(PrototypeWorldMapSites.All, site =>
        {
            Assert.False(string.IsNullOrWhiteSpace(site.Id));
            Assert.False(string.IsNullOrWhiteSpace(site.DisplayName));
            Assert.True(IsInsideMap(site.Position));
            Assert.True(site.EnterRadius > 0);
        });
    }

    [Fact]
    public void PrototypeWorldMapSitesIncludesGasStationLocalSite()
    {
        var gasStation = Assert.Single(
            PrototypeWorldMapSites.All,
            site => site.Id == PrototypeLocalSites.GasStationSiteId);

        Assert.Equal("Route 18 Gas Station", gasStation.DisplayName);
        Assert.Equal(new WorldMapPosition(420.0, 560.0), gasStation.Position);
        Assert.Equal(46.0, gasStation.EnterRadius);
    }

    [Fact]
    public void DestinationClampsToFullWorldMapBounds()
    {
        var state = new WorldMapTravelState(
            PrototypeWorldMapSites.MapWidth,
            PrototypeWorldMapSites.MapHeight,
            PrototypeWorldMapSites.StartPosition,
            TravelMethodId.Walking,
            vehicleFuel: 0
        );

        state.SetDestination(new WorldMapPosition(9999, -25));

        Assert.Equal(
            new WorldMapPosition(PrototypeWorldMapSites.MapWidth, 0),
            state.Destination
        );
    }

    [Fact]
    public void DestinationRedirectsWhileTravelling()
    {
        var state = CreateState(TravelMethodId.Walking);
        var time = new WorldTime();

        state.SetDestination(new WorldMapPosition(300, 100));
        state.Advance(1.0, time, PrototypeTravelMethods.Walking);
        state.SetDestination(new WorldMapPosition(100, 300));

        Assert.Equal(new WorldMapPosition(100, 300), state.Destination);
    }

    [Fact]
    public void WalkingMovesSmoothlyAndAdvancesTime()
    {
        var state = CreateState(TravelMethodId.Walking);
        var time = new WorldTime();
        state.SetDestination(new WorldMapPosition(300, 100));

        var result = state.Advance(1.0, time, PrototypeTravelMethods.Walking);

        Assert.True(result.Moved);
        Assert.InRange(state.Position.X, 145.9, 146.1);
        Assert.Equal(100, time.ElapsedTicks);
    }

    [Fact]
    public void TravelSpeedDependsOnCurrentTravelMethod()
    {
        var walking = CreateState(TravelMethodId.Walking);
        var pushbike = CreateState(TravelMethodId.Pushbike);
        var walkingTime = new WorldTime();
        var pushbikeTime = new WorldTime();

        walking.SetDestination(new WorldMapPosition(300, 100));
        pushbike.SetDestination(new WorldMapPosition(300, 100));
        walking.Advance(1.0, walkingTime, PrototypeTravelMethods.Walking);
        pushbike.Advance(1.0, pushbikeTime, PrototypeTravelMethods.Pushbike);

        Assert.True(pushbike.Position.X > walking.Position.X);
    }

    [Fact]
    public void VehicleTravelConsumesFuel()
    {
        var state = CreateState(TravelMethodId.Vehicle, vehicleFuel: 10);
        var time = new WorldTime();
        state.SetDestination(new WorldMapPosition(300, 100));

        state.Advance(1.0, time, PrototypeTravelMethods.Vehicle);

        Assert.True(state.VehicleFuel < 10);
        Assert.Equal(100, time.ElapsedTicks);
    }

    [Fact]
    public void WalkingAndPushbikeDoNotConsumeFuel()
    {
        var walking = CreateState(TravelMethodId.Walking, vehicleFuel: 4);
        var pushbike = CreateState(TravelMethodId.Pushbike, vehicleFuel: 4);
        var walkingTime = new WorldTime();
        var pushbikeTime = new WorldTime();

        walking.SetDestination(new WorldMapPosition(300, 100));
        pushbike.SetDestination(new WorldMapPosition(300, 100));
        walking.Advance(1.0, walkingTime, PrototypeTravelMethods.Walking);
        pushbike.Advance(1.0, pushbikeTime, PrototypeTravelMethods.Pushbike);

        Assert.Equal(4, walking.VehicleFuel);
        Assert.Equal(4, pushbike.VehicleFuel);
    }

    [Fact]
    public void VehicleFuelDepletionStopsTravelWithClearMessage()
    {
        var state = CreateState(TravelMethodId.Vehicle, vehicleFuel: 10);
        var time = new WorldTime();
        var fuelHungryVehicle = new TravelMethodDefinition(
            TravelMethodId.Vehicle,
            "Vehicle",
            speedMapUnitsPerSecond: 100,
            usesFuel: true,
            fuelUsePerMapUnit: 1
        );
        state.SetDestination(new WorldMapPosition(300, 100));

        var result = state.Advance(1.0, time, fuelHungryVehicle);

        Assert.True(result.FuelDepleted);
        Assert.Null(state.Destination);
        Assert.Equal(0, state.VehicleFuel);
        Assert.Contains(result.Messages, message => message.Contains("fuel ran out", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CanContinueByWalkingAfterVehicleRunsOutOfFuel()
    {
        var state = CreateState(TravelMethodId.Vehicle, vehicleFuel: 0);
        var time = new WorldTime();

        state.SetTravelMethod(TravelMethodId.Walking);
        state.SetDestination(new WorldMapPosition(300, 100));
        var result = state.Advance(1.0, time, PrototypeTravelMethods.Walking);

        Assert.True(result.Moved);
        Assert.Equal(0, state.VehicleFuel);
        Assert.Equal(100, time.ElapsedTicks);
    }

    [Fact]
    public void ConstructorRejectsUnknownTravelMethod()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WorldMapTravelState(
                500,
                500,
                new WorldMapPosition(100, 100),
                (TravelMethodId)999,
                vehicleFuel: 0
            ));

        Assert.Equal("currentTravelMethod", exception.ParamName);
    }

    [Fact]
    public void PrototypeTravelMethodsRejectUnknownIdsWithClearError()
    {
        var exception = Assert.Throws<KeyNotFoundException>(() =>
            PrototypeTravelMethods.Get((TravelMethodId)999));

        Assert.Contains("Travel method '999' is not defined.", exception.Message);
    }

    [Fact]
    public void FindsNearbyPointOfInterest()
    {
        var state = new WorldMapTravelState(
            500,
            500,
            new WorldMapPosition(104, 100),
            TravelMethodId.Walking,
            vehicleFuel: 0
        );
        var site = new WorldMapPointOfInterest("test", "Test Site", new WorldMapPosition(100, 100), 10);

        Assert.Equal(site, state.FindNearbySite(new[] { site }));
    }

    private static WorldMapTravelState CreateState(TravelMethodId method, double vehicleFuel = 15)
    {
        return new WorldMapTravelState(
            500,
            500,
            new WorldMapPosition(100, 100),
            method,
            vehicleFuel
        );
    }

    private static bool IsInsideMap(WorldMapPosition position)
    {
        return position.X >= 0
            && position.X <= PrototypeWorldMapSites.MapWidth
            && position.Y >= 0
            && position.Y <= PrototypeWorldMapSites.MapHeight;
    }
}

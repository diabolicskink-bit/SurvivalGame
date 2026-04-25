using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class OverworldTravelStateTests
{
    [Fact]
    public void DestinationRedirectsWhileTravelling()
    {
        var state = CreateState(TravelMethodId.Walking);
        var time = new WorldTime();

        state.SetDestination(new OverworldPosition(300, 100));
        state.Advance(1.0, time, PrototypeTravelMethods.Walking);
        state.SetDestination(new OverworldPosition(100, 300));

        Assert.Equal(new OverworldPosition(100, 300), state.Destination);
    }

    [Fact]
    public void WalkingMovesSmoothlyAndAdvancesTime()
    {
        var state = CreateState(TravelMethodId.Walking);
        var time = new WorldTime();
        state.SetDestination(new OverworldPosition(300, 100));

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

        walking.SetDestination(new OverworldPosition(300, 100));
        pushbike.SetDestination(new OverworldPosition(300, 100));
        walking.Advance(1.0, walkingTime, PrototypeTravelMethods.Walking);
        pushbike.Advance(1.0, pushbikeTime, PrototypeTravelMethods.Pushbike);

        Assert.True(pushbike.Position.X > walking.Position.X);
    }

    [Fact]
    public void VehicleTravelConsumesFuel()
    {
        var state = CreateState(TravelMethodId.Vehicle, vehicleFuel: 10);
        var time = new WorldTime();
        state.SetDestination(new OverworldPosition(300, 100));

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

        walking.SetDestination(new OverworldPosition(300, 100));
        pushbike.SetDestination(new OverworldPosition(300, 100));
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
        state.SetDestination(new OverworldPosition(300, 100));

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
        state.SetDestination(new OverworldPosition(300, 100));
        var result = state.Advance(1.0, time, PrototypeTravelMethods.Walking);

        Assert.True(result.Moved);
        Assert.Equal(0, state.VehicleFuel);
        Assert.Equal(100, time.ElapsedTicks);
    }

    [Fact]
    public void FindsNearbyPointOfInterest()
    {
        var state = new OverworldTravelState(
            500,
            500,
            new OverworldPosition(104, 100),
            TravelMethodId.Walking,
            vehicleFuel: 0
        );
        var site = new OverworldPointOfInterest("test", "Test Site", new OverworldPosition(100, 100), 10);

        Assert.Equal(site, state.FindNearbySite(new[] { site }));
    }

    private static OverworldTravelState CreateState(TravelMethodId method, double vehicleFuel = 15)
    {
        return new OverworldTravelState(
            500,
            500,
            new OverworldPosition(100, 100),
            method,
            vehicleFuel
        );
    }
}

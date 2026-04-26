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
        Assert.Equal("colorado", PrototypeWorldMapSites.Definition.Id);
        Assert.InRange(PrototypeWorldMapSites.All.Count, 90, 110);
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
            site => site.Id == PrototypeLocalSites.GasStationSiteId.Value);

        Assert.Equal("Foothills Test Gas Station", gasStation.DisplayName);
        Assert.Equal(WorldMapPointCategory.LocalSite, gasStation.Category);
        Assert.Equal(PrototypeLocalSites.GasStationSiteId.Value, gasStation.LocalSiteId);
        Assert.True(gasStation.Position.DistanceTo(PrototypeWorldMapSites.StartPosition) < 5.0);
        Assert.Equal(50.0, gasStation.EnterRadius);
    }

    [Fact]
    public void PrototypeWorldMapSitesIncludesFarmsteadLocalSite()
    {
        var farmstead = Assert.Single(
            PrototypeWorldMapSites.All,
            site => site.Id == PrototypeLocalSites.FarmsteadSiteId.Value);

        Assert.Equal("Foothills Test Farmstead", farmstead.DisplayName);
        Assert.Equal(WorldMapPointCategory.LocalSite, farmstead.Category);
        Assert.Equal(PrototypeLocalSites.FarmsteadSiteId.Value, farmstead.LocalSiteId);
        Assert.True(farmstead.Position.DistanceTo(PrototypeWorldMapSites.StartPosition) < 60.0);
        Assert.Equal(50.0, farmstead.EnterRadius);
    }

    [Fact]
    public void ColoradoWorldMapIncludesRequiredCityAndLandmarkAnchors()
    {
        var cities = PrototypeWorldMapSites.All
            .Where(site => site.Category == WorldMapPointCategory.City)
            .Select(site => site.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var landmarks = PrototypeWorldMapSites.All
            .Where(site => site.Category == WorldMapPointCategory.Landmark)
            .Select(site => site.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.InRange(cities.Count, 45, 50);
        Assert.Contains("denver", cities);
        Assert.Contains("colorado_springs", cities);
        Assert.Contains("grand_junction", cities);
        Assert.Contains("durango", cities);
        Assert.Contains("lamar", cities);
        Assert.Contains("sterling", cities);

        Assert.InRange(landmarks.Count, 40, 50);
        Assert.Contains("rocky_mountain_np", landmarks);
        Assert.Contains("mesa_verde_np", landmarks);
        Assert.Contains("great_sand_dunes_np", landmarks);
        Assert.Contains("black_canyon_gunnison_np", landmarks);
        Assert.Contains("garden_of_the_gods", landmarks);
        Assert.Contains("pikes_peak", landmarks);
        Assert.Contains("red_rocks", landmarks);
        Assert.Contains("eisenhower_tunnel", landmarks);
    }

    [Fact]
    public void ColoradoWorldMapLoadsGeneratedMajorRoadLayer()
    {
        var roads = PrototypeWorldMapSites.Definition.Roads
            .ToDictionary(road => road.Id, StringComparer.OrdinalIgnoreCase);
        var requiredRoutes = new[]
        {
            "i_25",
            "i_70",
            "i_76",
            "us_36",
            "us_50",
            "us_160",
            "us_285",
            "us_550"
        };

        Assert.InRange(roads.Count, 18, 24);
        Assert.All(requiredRoutes, route => Assert.True(roads.ContainsKey(route), $"Missing required road '{route}'."));
        Assert.All(roads.Values, road =>
        {
            Assert.NotEmpty(road.Segments);
            Assert.All(road.Segments, segment => Assert.True(segment.Points.Count >= 2));
            Assert.True(road.LaneCount > 0);
            Assert.True(road.SurfaceWidthFeet > 0);
            Assert.True(road.TravelInfluenceRadius > 0);
        });
        Assert.Equal(WorldMapRoadKind.Interstate, roads["i_25"].Kind);
        Assert.Equal(WorldMapRoadKind.UsHighway, roads["us_50"].Kind);
        Assert.True(roads["us_36"].Segments.Count > 1);
        Assert.True(roads["i_25"].LaneCount >= roads["co_14"].LaneCount);
    }

    [Fact]
    public void ColoradoGeneratedRoadPointsStayInsideMapBounds()
    {
        var definition = PrototypeWorldMapSites.Definition;
        var ids = definition.Roads.Select(road => road.Id).ToArray();

        Assert.Equal(ids.Length, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(definition.Roads, road =>
        {
            Assert.All(road.Segments, segment =>
            {
                Assert.All(segment.Points, point =>
                {
                    Assert.True(IsInsideMap(definition, point), $"{road.Id} has a projected point outside map bounds.");
                });
            });
        });
    }

    [Fact]
    public void ColoradoRoadDistanceWorksAcrossMultiSegmentRoads()
    {
        var road = Assert.Single(PrototypeWorldMapSites.Definition.Roads, road => road.Id == "us_36");
        var pointOnLaterSegment = road.Segments.Last().Points[0];
        var farPoint = PrototypeWorldMapSites.Definition.Project(-106.60, 37.20);

        Assert.True(road.Segments.Count > 1);
        Assert.True(road.DistanceTo(pointOnLaterSegment) < 0.01);
        Assert.True(road.DistanceTo(farPoint) > 500);
    }

    [Fact]
    public void ColoradoWorldMapTravelCostsReflectRoadsAndTerrain()
    {
        var definition = PrototypeWorldMapSites.Definition;
        var road = definition.GetTravelCost(
            definition.Project(-105.205, 39.745),
            PrototypeTravelMethods.Vehicle);
        var plainsOffRoad = definition.GetTravelCost(
            definition.Project(-102.20, 37.30),
            PrototypeTravelMethods.Vehicle);
        var mountainOffRoad = definition.GetTravelCost(
            definition.Project(-106.60, 38.05),
            PrototypeTravelMethods.Vehicle);

        Assert.True(road.IsNearRoad);
        Assert.False(plainsOffRoad.IsNearRoad);
        Assert.False(mountainOffRoad.IsNearRoad);
        Assert.True(road.SpeedMultiplier > plainsOffRoad.SpeedMultiplier);
        Assert.True(plainsOffRoad.SpeedMultiplier > mountainOffRoad.SpeedMultiplier);
        Assert.True(road.FuelUseMultiplier < plainsOffRoad.FuelUseMultiplier);
        Assert.True(mountainOffRoad.FuelUseMultiplier > plainsOffRoad.FuelUseMultiplier);
    }

    [Fact]
    public void ColoradoWorldMapTravelCostChangesMovementDistance()
    {
        var definition = PrototypeWorldMapSites.Definition;
        var roadState = new WorldMapTravelState(
            definition.MapWidth,
            definition.MapHeight,
            definition.Project(-105.205, 39.745),
            TravelMethodId.Vehicle,
            vehicleFuel: 15
        );
        var mountainState = new WorldMapTravelState(
            definition.MapWidth,
            definition.MapHeight,
            definition.Project(-106.60, 38.05),
            TravelMethodId.Vehicle,
            vehicleFuel: 15
        );
        var roadDestination = definition.Project(-104.40, 39.745);
        var mountainDestination = definition.Project(-105.80, 38.05);
        roadState.SetDestination(roadDestination);
        mountainState.SetDestination(mountainDestination);

        roadState.Advance(1.0, new WorldTime(), PrototypeTravelMethods.Vehicle, definition);
        mountainState.Advance(1.0, new WorldTime(), PrototypeTravelMethods.Vehicle, definition);

        Assert.True(roadState.Position.DistanceTo(roadDestination) < mountainState.Position.DistanceTo(mountainDestination));
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

        state.SetDestination(new WorldMapPosition(PrototypeWorldMapSites.MapWidth + 1000, -25));

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

    private static bool IsInsideMap(WorldMapDefinition definition, WorldMapPosition position)
    {
        return position.X >= 0
            && position.X <= definition.MapWidth
            && position.Y >= 0
            && position.Y <= definition.MapHeight;
    }
}

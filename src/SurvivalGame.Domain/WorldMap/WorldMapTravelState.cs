namespace SurvivalGame.Domain;

public sealed class WorldMapTravelState
{
    public const int TravelTicksPerSecond = 100;
    public const double DefaultVehicleFuelCapacity = 15.0;
    private const double ArrivalEpsilon = 0.01;

    private double _pendingTravelTicks;
    private readonly VehicleFuelState _vehicleFuel;

    public WorldMapTravelState(
        double mapWidth,
        double mapHeight,
        WorldMapPosition startPosition,
        TravelMethodId currentTravelMethod,
        double vehicleFuel
    )
        : this(
            mapWidth,
            mapHeight,
            startPosition,
            currentTravelMethod,
            new VehicleFuelState(DefaultVehicleFuelCapacity, vehicleFuel)
        )
    {
    }

    public WorldMapTravelState(
        double mapWidth,
        double mapHeight,
        WorldMapPosition startPosition,
        TravelMethodId currentTravelMethod,
        VehicleFuelState vehicleFuel
    )
    {
        ArgumentNullException.ThrowIfNull(vehicleFuel);
        ValidateTravelMethod(currentTravelMethod, nameof(currentTravelMethod));

        if (mapWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapWidth), "Map width must be positive.");
        }

        if (mapHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(mapHeight), "Map height must be positive.");
        }

        MapWidth = mapWidth;
        MapHeight = mapHeight;
        Position = Clamp(startPosition);
        CurrentTravelMethod = currentTravelMethod;
        _vehicleFuel = vehicleFuel;
    }

    public double MapWidth { get; }

    public double MapHeight { get; }

    public WorldMapPosition Position { get; private set; }

    public WorldMapPosition? Destination { get; private set; }

    public TravelMethodId CurrentTravelMethod { get; private set; }

    public VehicleFuelState VehicleFuelState => _vehicleFuel;

    public double VehicleFuel => _vehicleFuel.CurrentFuel;

    public void SetDestination(WorldMapPosition destination)
    {
        Destination = Clamp(destination);
    }

    public void ClearDestination()
    {
        Destination = null;
    }

    public void SetTravelMethod(TravelMethodId travelMethod)
    {
        ValidateTravelMethod(travelMethod, nameof(travelMethod));
        CurrentTravelMethod = travelMethod;
    }

    public void SetVehicleFuel(double fuel)
    {
        if (fuel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fuel), "Vehicle fuel cannot be negative.");
        }

        _vehicleFuel.SetFuel(fuel);
    }

    public WorldMapPointOfInterest? FindNearbySite(IEnumerable<WorldMapPointOfInterest> sites)
    {
        ArgumentNullException.ThrowIfNull(sites);
        return sites.FirstOrDefault(site => site.IsNear(Position));
    }

    public WorldMapTravelResult Advance(double deltaSeconds, WorldTime time, TravelMethodDefinition travelMethod)
    {
        return Advance(deltaSeconds, time, travelMethod, worldMap: null);
    }

    public WorldMapTravelResult Advance(
        double deltaSeconds,
        WorldTime time,
        TravelMethodDefinition travelMethod,
        WorldMapDefinition? worldMap)
    {
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(travelMethod);

        if (deltaSeconds <= 0 || Destination is null)
        {
            return WorldMapTravelResult.Idle;
        }

        if (travelMethod.Id != CurrentTravelMethod)
        {
            throw new ArgumentException("Travel method definition must match the active travel method.", nameof(travelMethod));
        }

        if (travelMethod.UsesFuel && _vehicleFuel.IsEmpty)
        {
            Destination = null;
            return new WorldMapTravelResult(
                Moved: false,
                Arrived: false,
                FuelDepleted: true,
                ElapsedTicks: 0,
                Messages: new[] { "Vehicle fuel is empty. Select walking or pushbike to continue." }
            );
        }

        var destination = Destination.Value;
        var remainingDistance = Position.DistanceTo(destination);
        if (remainingDistance <= ArrivalEpsilon)
        {
            Destination = null;
            return WorldMapTravelResult.Idle;
        }

        var travelCost = worldMap?.GetTravelCost(Position, travelMethod)
            ?? new WorldMapTravelCost(1.0, 1.0, WorldMapTerrainKind.Plains, "Plains", IsNearRoad: false);
        var effectiveSpeed = travelMethod.SpeedMapUnitsPerSecond * travelCost.SpeedMultiplier;
        var effectiveFuelUse = travelMethod.FuelUsePerMapUnit * travelCost.FuelUseMultiplier;

        var requestedDistance = Math.Min(effectiveSpeed * deltaSeconds, remainingDistance);
        var travelledDistance = requestedDistance;
        var fuelDepleted = false;

        if (travelMethod.UsesFuel)
        {
            var requestedFuel = requestedDistance * effectiveFuelUse;
            if (requestedFuel >= _vehicleFuel.CurrentFuel)
            {
                travelledDistance = effectiveFuelUse <= 0
                    ? requestedDistance
                    : _vehicleFuel.CurrentFuel / effectiveFuelUse;
                fuelDepleted = true;
            }

            _vehicleFuel.Consume(travelledDistance * effectiveFuelUse);
        }

        Position = Position.MoveToward(destination, travelledDistance);
        var actualSeconds = travelledDistance / effectiveSpeed;
        var elapsedTicks = AdvanceTravelTime(time, actualSeconds);
        var arrived = Position.DistanceTo(destination) <= ArrivalEpsilon && !fuelDepleted;

        if (arrived || fuelDepleted)
        {
            Destination = null;
        }

        var messages = new List<string>();
        if (arrived)
        {
            messages.Add("Arrived at destination.");
        }

        if (fuelDepleted)
        {
            messages.Add("Vehicle fuel ran out. Select walking or pushbike to continue.");
        }

        return new WorldMapTravelResult(
            Moved: travelledDistance > 0,
            Arrived: arrived,
            FuelDepleted: fuelDepleted,
            ElapsedTicks: elapsedTicks,
            Messages: messages
        );
    }

    private WorldMapPosition Clamp(WorldMapPosition position)
    {
        return new WorldMapPosition(
            Math.Clamp(position.X, 0, MapWidth),
            Math.Clamp(position.Y, 0, MapHeight)
        );
    }

    private int AdvanceTravelTime(WorldTime time, double actualSeconds)
    {
        if (actualSeconds <= 0)
        {
            return 0;
        }

        var ticks = _pendingTravelTicks + (actualSeconds * TravelTicksPerSecond);
        var wholeTicks = (int)Math.Floor(ticks);
        _pendingTravelTicks = ticks - wholeTicks;

        if (wholeTicks > 0)
        {
            time.Advance(wholeTicks);
        }

        return wholeTicks;
    }

    private static void ValidateTravelMethod(TravelMethodId travelMethod, string parameterName)
    {
        if (!Enum.IsDefined(travelMethod))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Unknown travel method.");
        }
    }
}

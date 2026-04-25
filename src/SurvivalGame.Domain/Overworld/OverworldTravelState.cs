namespace SurvivalGame.Domain;

public sealed class OverworldTravelState
{
    public const int TravelTicksPerSecond = 100;
    private const double ArrivalEpsilon = 0.01;

    private double _pendingTravelTicks;
    private readonly VehicleFuelState _vehicleFuel;

    public OverworldTravelState(
        double mapWidth,
        double mapHeight,
        OverworldPosition startPosition,
        TravelMethodId currentTravelMethod,
        double vehicleFuel
    )
        : this(
            mapWidth,
            mapHeight,
            startPosition,
            currentTravelMethod,
            new VehicleFuelState(PrototypeTravelMethods.VehicleFuelCapacity, vehicleFuel)
        )
    {
    }

    public OverworldTravelState(
        double mapWidth,
        double mapHeight,
        OverworldPosition startPosition,
        TravelMethodId currentTravelMethod,
        VehicleFuelState vehicleFuel
    )
    {
        ArgumentNullException.ThrowIfNull(vehicleFuel);

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

    public OverworldPosition Position { get; private set; }

    public OverworldPosition? Destination { get; private set; }

    public TravelMethodId CurrentTravelMethod { get; private set; }

    public double VehicleFuel => _vehicleFuel.CurrentFuel;

    public void SetDestination(OverworldPosition destination)
    {
        Destination = Clamp(destination);
    }

    public void ClearDestination()
    {
        Destination = null;
    }

    public void SetTravelMethod(TravelMethodId travelMethod)
    {
        if (!Enum.IsDefined(travelMethod))
        {
            throw new ArgumentOutOfRangeException(nameof(travelMethod), "Unknown travel method.");
        }

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

    public OverworldPointOfInterest? FindNearbySite(IEnumerable<OverworldPointOfInterest> sites)
    {
        ArgumentNullException.ThrowIfNull(sites);
        return sites.FirstOrDefault(site => site.IsNear(Position));
    }

    public OverworldTravelResult Advance(double deltaSeconds, WorldTime time, TravelMethodDefinition travelMethod)
    {
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(travelMethod);

        if (deltaSeconds <= 0 || Destination is null)
        {
            return OverworldTravelResult.Idle;
        }

        if (travelMethod.Id != CurrentTravelMethod)
        {
            throw new ArgumentException("Travel method definition must match the active travel method.", nameof(travelMethod));
        }

        if (travelMethod.UsesFuel && _vehicleFuel.IsEmpty)
        {
            Destination = null;
            return new OverworldTravelResult(
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
            return OverworldTravelResult.Idle;
        }

        var requestedDistance = Math.Min(travelMethod.SpeedMapUnitsPerSecond * deltaSeconds, remainingDistance);
        var travelledDistance = requestedDistance;
        var fuelDepleted = false;

        if (travelMethod.UsesFuel)
        {
            var requestedFuel = requestedDistance * travelMethod.FuelUsePerMapUnit;
            if (requestedFuel >= _vehicleFuel.CurrentFuel)
            {
                travelledDistance = travelMethod.FuelUsePerMapUnit <= 0
                    ? requestedDistance
                    : _vehicleFuel.CurrentFuel / travelMethod.FuelUsePerMapUnit;
                fuelDepleted = true;
            }

            _vehicleFuel.Consume(travelledDistance * travelMethod.FuelUsePerMapUnit);
        }

        Position = Position.MoveToward(destination, travelledDistance);
        var actualSeconds = travelledDistance / travelMethod.SpeedMapUnitsPerSecond;
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

        return new OverworldTravelResult(
            Moved: travelledDistance > 0,
            Arrived: arrived,
            FuelDepleted: fuelDepleted,
            ElapsedTicks: elapsedTicks,
            Messages: messages
        );
    }

    private OverworldPosition Clamp(OverworldPosition position)
    {
        return new OverworldPosition(
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
}

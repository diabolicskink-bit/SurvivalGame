namespace SurvivalGame.Domain;

public static class PrototypeTravelMethods
{
    public const double VehicleFuelCapacity = WorldMapTravelState.DefaultVehicleFuelCapacity;

    public const double VehicleStartingFuel = 15.0;

    public static readonly TravelMethodDefinition Walking = new(
        TravelMethodId.Walking,
        "Walking",
        speedMapUnitsPerSecond: 46.0,
        usesFuel: false,
        fuelUsePerMapUnit: 0.0
    );

    public static readonly TravelMethodDefinition Pushbike = new(
        TravelMethodId.Pushbike,
        "Pushbike",
        speedMapUnitsPerSecond: 92.0,
        usesFuel: false,
        fuelUsePerMapUnit: 0.0
    );

    public static readonly TravelMethodDefinition Vehicle = new(
        TravelMethodId.Vehicle,
        "Vehicle",
        speedMapUnitsPerSecond: 190.0,
        usesFuel: true,
        fuelUsePerMapUnit: 0.006
    );

    public static IReadOnlyList<TravelMethodDefinition> All { get; } = new[]
    {
        Walking,
        Pushbike,
        Vehicle
    };

    private static readonly IReadOnlyDictionary<TravelMethodId, TravelMethodDefinition> MethodsById =
        All.ToDictionary(method => method.Id);

    public static bool TryGet(TravelMethodId id, out TravelMethodDefinition method)
    {
        return MethodsById.TryGetValue(id, out method!);
    }

    public static TravelMethodDefinition Get(TravelMethodId id)
    {
        if (TryGet(id, out var method))
        {
            return method;
        }

        throw new KeyNotFoundException($"Travel method '{id}' is not defined.");
    }
}

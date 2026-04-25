namespace SurvivalGame.Domain;

public static class PrototypeTravelMethods
{
    public const double VehicleFuelCapacity = 15.0;

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

    public static TravelMethodDefinition Get(TravelMethodId id)
    {
        return All.First(method => method.Id == id);
    }
}

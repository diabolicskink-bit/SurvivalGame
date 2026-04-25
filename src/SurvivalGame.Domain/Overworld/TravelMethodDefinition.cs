namespace SurvivalGame.Domain;

public sealed record TravelMethodDefinition
{
    public TravelMethodDefinition(
        TravelMethodId id,
        string displayName,
        double speedMapUnitsPerSecond,
        bool usesFuel,
        double fuelUsePerMapUnit
    )
    {
        DisplayName = ValidateDisplayName(displayName);

        if (speedMapUnitsPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(speedMapUnitsPerSecond), "Travel speed must be positive.");
        }

        SpeedMapUnitsPerSecond = speedMapUnitsPerSecond;

        if (fuelUsePerMapUnit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fuelUsePerMapUnit), "Fuel use cannot be negative.");
        }

        if (!usesFuel && fuelUsePerMapUnit > 0)
        {
            throw new ArgumentException("Non-fuel travel methods cannot consume fuel.", nameof(fuelUsePerMapUnit));
        }

        Id = id;
        UsesFuel = usesFuel;
        FuelUsePerMapUnit = fuelUsePerMapUnit;
    }

    public TravelMethodId Id { get; }

    public string DisplayName { get; }

    public double SpeedMapUnitsPerSecond { get; }

    public bool UsesFuel { get; }

    public double FuelUsePerMapUnit { get; }

    private static string ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Travel method display name is required.", nameof(displayName));
        }

        return displayName;
    }
}

namespace SurvivalGame.Domain;

public sealed record FuelContainerDefinition
{
    public FuelContainerDefinition(double capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Fuel container capacity must be positive.");
        }

        Capacity = capacity;
    }

    public double Capacity { get; }
}

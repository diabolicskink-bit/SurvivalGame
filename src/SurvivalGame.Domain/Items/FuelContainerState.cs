namespace SurvivalGame.Domain;

public sealed class FuelContainerState
{
    public FuelContainerState(double capacity, double currentFuel = 0)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Fuel container capacity must be positive.");
        }

        if (currentFuel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentFuel), "Fuel amount cannot be negative.");
        }

        Capacity = capacity;
        CurrentFuel = Math.Min(currentFuel, capacity);
    }

    public double Capacity { get; }

    public double CurrentFuel { get; private set; }

    public bool IsFull => CurrentFuel >= Capacity;

    public bool IsEmpty => CurrentFuel <= 0;

    public double AddFuel(double amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Fuel amount cannot be negative.");
        }

        var accepted = Math.Min(Capacity - CurrentFuel, amount);
        CurrentFuel += accepted;
        return accepted;
    }

    public double RemoveFuel(double amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Fuel amount cannot be negative.");
        }

        var removed = Math.Min(CurrentFuel, amount);
        CurrentFuel -= removed;
        return removed;
    }
}

namespace SurvivalGame.Domain;

public sealed class VehicleFuelState
{
    public VehicleFuelState(double capacity, double currentFuel)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Vehicle fuel capacity must be positive.");
        }

        if (currentFuel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentFuel), "Vehicle fuel cannot be negative.");
        }

        Capacity = capacity;
        CurrentFuel = Math.Min(currentFuel, capacity);
    }

    public double Capacity { get; }

    public double CurrentFuel { get; private set; }

    public bool IsFull => CurrentFuel >= Capacity;

    public bool IsEmpty => CurrentFuel <= 0;

    public void SetFuel(double fuel)
    {
        if (fuel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fuel), "Vehicle fuel cannot be negative.");
        }

        CurrentFuel = Math.Min(fuel, Capacity);
    }

    public double Consume(double fuel)
    {
        if (fuel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fuel), "Fuel consumption cannot be negative.");
        }

        var consumed = Math.Min(CurrentFuel, fuel);
        CurrentFuel -= consumed;
        return consumed;
    }

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

    public void Refill()
    {
        CurrentFuel = Capacity;
    }
}

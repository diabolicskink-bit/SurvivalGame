namespace SurvivalGame.Domain;

public sealed class PlayerVitals
{
    private const float MinimumBodyTemperatureCelsius = 20.0f;
    private const float MaximumBodyTemperatureCelsius = 45.0f;

    public BoundedMeter Health { get; private set; } = new(current: 100);

    public BoundedMeter Hunger { get; private set; } = new(current: 0);

    public BoundedMeter Thirst { get; private set; } = new(current: 0);

    public BoundedMeter Fatigue { get; private set; } = new(current: 0);

    public BoundedMeter SleepDebt { get; private set; } = new(current: 0);

    public BoundedMeter Pain { get; private set; } = new(current: 0);

    public float BodyTemperatureCelsius { get; private set; } = 37.0f;

    public void SetHealth(int current)
    {
        Health = Health.WithCurrent(current);
    }

    public void SetHunger(int current)
    {
        Hunger = Hunger.WithCurrent(current);
    }

    public void SetThirst(int current)
    {
        Thirst = Thirst.WithCurrent(current);
    }

    public void SetFatigue(int current)
    {
        Fatigue = Fatigue.WithCurrent(current);
    }

    public void SetSleepDebt(int current)
    {
        SleepDebt = SleepDebt.WithCurrent(current);
    }

    public void SetPain(int current)
    {
        Pain = Pain.WithCurrent(current);
    }

    public void SetBodyTemperatureCelsius(float temperatureCelsius)
    {
        if (float.IsNaN(temperatureCelsius)
            || temperatureCelsius < MinimumBodyTemperatureCelsius
            || temperatureCelsius > MaximumBodyTemperatureCelsius)
        {
            throw new ArgumentOutOfRangeException(
                nameof(temperatureCelsius),
                "Body temperature must be a plausible Celsius value for tracked player state."
            );
        }

        BodyTemperatureCelsius = temperatureCelsius;
    }
}

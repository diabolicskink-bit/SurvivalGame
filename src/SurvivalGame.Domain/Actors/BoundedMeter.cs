namespace SurvivalGame.Domain;

public readonly record struct BoundedMeter
{
    public BoundedMeter(int current, int maximum = 100, int minimum = 0)
    {
        if (maximum <= minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum must be greater than minimum.");
        }

        if (current < minimum || current > maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(current), "Current value must be inside the meter range.");
        }

        Minimum = minimum;
        Maximum = maximum;
        Current = current;
    }

    public int Minimum { get; }

    public int Maximum { get; }

    public int Current { get; }

    public float Normalized => (Current - Minimum) / (float)(Maximum - Minimum);

    public BoundedMeter WithCurrent(int current)
    {
        return new BoundedMeter(current, Maximum, Minimum);
    }
}

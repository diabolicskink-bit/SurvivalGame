namespace SurvivalGame.Domain;

public readonly record struct OverworldPosition(double X, double Y)
{
    public double DistanceTo(OverworldPosition other)
    {
        var x = other.X - X;
        var y = other.Y - Y;
        return Math.Sqrt((x * x) + (y * y));
    }

    public OverworldPosition MoveToward(OverworldPosition destination, double distance)
    {
        if (distance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distance), "Move distance cannot be negative.");
        }

        var remaining = DistanceTo(destination);
        if (remaining <= 0 || distance >= remaining)
        {
            return destination;
        }

        var factor = distance / remaining;
        return new OverworldPosition(
            X + ((destination.X - X) * factor),
            Y + ((destination.Y - Y) * factor)
        );
    }
}

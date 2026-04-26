namespace SurvivalGame.Domain;

public readonly record struct WorldMapPosition(double X, double Y)
{
    public double DistanceTo(WorldMapPosition other)
    {
        var x = other.X - X;
        var y = other.Y - Y;
        return Math.Sqrt((x * x) + (y * y));
    }

    public WorldMapPosition MoveToward(WorldMapPosition destination, double distance)
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
        return new WorldMapPosition(
            X + ((destination.X - X) * factor),
            Y + ((destination.Y - Y) * factor)
        );
    }
}

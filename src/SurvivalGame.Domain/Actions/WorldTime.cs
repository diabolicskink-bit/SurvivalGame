namespace SurvivalGame.Domain;

public sealed class WorldTime
{
    public WorldTime(int elapsedTicks = 0)
    {
        if (elapsedTicks < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedTicks), "Elapsed ticks cannot be negative.");
        }

        ElapsedTicks = elapsedTicks;
    }

    public int ElapsedTicks { get; private set; }

    public void Advance(int ticks)
    {
        if (ticks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ticks), "World time must advance by a positive tick amount.");
        }

        ElapsedTicks += ticks;
    }
}

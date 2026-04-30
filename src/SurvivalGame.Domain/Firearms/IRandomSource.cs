namespace SurvivalGame.Domain;

public interface IRandomSource
{
    double NextUnitDouble();
}

internal sealed class SystemRandomSource : IRandomSource
{
    public double NextUnitDouble()
    {
        return Random.Shared.NextDouble();
    }
}

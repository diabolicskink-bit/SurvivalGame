namespace SurvivalGame.Domain;

public readonly record struct StatefulItemId(int Value)
{
    public override string ToString()
    {
        return $"item-{Value}";
    }
}

namespace SurvivalGame.Domain;

public readonly record struct InventoryGridPosition
{
    public InventoryGridPosition(int x, int y)
    {
        if (x < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Inventory grid x must be zero or greater.");
        }

        if (y < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(y), "Inventory grid y must be zero or greater.");
        }

        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }
}

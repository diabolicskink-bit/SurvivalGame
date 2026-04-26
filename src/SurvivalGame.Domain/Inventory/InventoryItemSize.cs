namespace SurvivalGame.Domain;

public readonly record struct InventoryItemSize
{
    public static readonly InventoryItemSize Default = new(1, 1);

    public InventoryItemSize(int width, int height)
    {
        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Inventory item width must be at least 1.");
        }

        if (height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Inventory item height must be at least 1.");
        }

        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }
}

namespace SurvivalGame.Domain;

public sealed class PlayerInventory
{
    private readonly Dictionary<ItemId, int> _items = new();

    public bool IsEmpty => _items.Count == 0;

    public IReadOnlyList<InventoryItemStack> Items
    {
        get
        {
            return _items
                .Select(item => new InventoryItemStack(item.Key, item.Value))
                .ToArray();
        }
    }

    public int CountOf(ItemId itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        return _items.GetValueOrDefault(itemId);
    }

    public void Add(ItemId itemId, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ValidatePositiveQuantity(quantity);
        _items[itemId] = CountOf(itemId) + quantity;
    }

    public bool TryRemove(ItemId itemId, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ValidatePositiveQuantity(quantity);

        var currentQuantity = CountOf(itemId);
        if (currentQuantity < quantity)
        {
            return false;
        }

        var remainingQuantity = currentQuantity - quantity;
        if (remainingQuantity == 0)
        {
            _items.Remove(itemId);
            return true;
        }

        _items[itemId] = remainingQuantity;
        return true;
    }

    private static void ValidatePositiveQuantity(int quantity)
    {
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }
    }
}

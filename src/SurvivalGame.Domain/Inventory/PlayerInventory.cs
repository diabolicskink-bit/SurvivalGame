namespace SurvivalGame.Domain;

public sealed class PlayerInventory
{
    private readonly Dictionary<ItemId, int> _items = new();
    private readonly ItemContainer _container = new(
        PrototypeItemContainers.PlayerInventory,
        "Inventory",
        PrototypeItemContainers.PlayerInventorySize
    );

    public bool IsEmpty => _items.Count == 0;

    public ItemContainer Container => _container;

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

    public bool CanAdd(ItemId itemId, InventoryItemSize? size = null, bool usesGrid = true)
    {
        ArgumentNullException.ThrowIfNull(itemId);

        if (!usesGrid || CountOf(itemId) > 0)
        {
            return true;
        }

        return _container.HasSpaceFor(size ?? InventoryItemSize.Default);
    }

    public bool CanAddAll(IEnumerable<(ItemId ItemId, InventoryItemSize Size)> itemSizes)
    {
        ArgumentNullException.ThrowIfNull(itemSizes);
        return CanAddAll(itemSizes.Select(item => (item.ItemId, item.Size, UsesGrid: true)));
    }

    public bool CanAddAll(IEnumerable<(ItemId ItemId, InventoryItemSize Size, bool UsesGrid)> itemSizes)
    {
        ArgumentNullException.ThrowIfNull(itemSizes);

        var container = _container.Copy();
        foreach (var (itemId, size, usesGrid) in itemSizes)
        {
            ArgumentNullException.ThrowIfNull(itemId);

            if (!usesGrid)
            {
                continue;
            }

            var itemRef = ContainerItemRef.Stack(itemId);
            if (CountOf(itemId) > 0 || container.Contains(itemRef))
            {
                continue;
            }

            if (!container.TryAutoPlace(itemRef, size))
            {
                return false;
            }
        }

        return true;
    }

    public void Add(ItemId itemId, int quantity = 1, InventoryItemSize? size = null, bool usesGrid = true)
    {
        if (!TryAdd(itemId, quantity, size, usesGrid))
        {
            throw new InvalidOperationException($"No inventory space available for '{itemId}'.");
        }
    }

    public bool TryAdd(ItemId itemId, int quantity = 1, InventoryItemSize? size = null, bool usesGrid = true)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ValidatePositiveQuantity(quantity);

        var itemSize = size ?? InventoryItemSize.Default;
        if (usesGrid && CountOf(itemId) == 0 && !_container.TryAutoPlace(ContainerItemRef.Stack(itemId), itemSize))
        {
            return false;
        }

        _items[itemId] = CountOf(itemId) + quantity;
        return true;
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
            _container.Remove(ContainerItemRef.Stack(itemId));
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

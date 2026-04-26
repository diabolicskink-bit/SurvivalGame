namespace SurvivalGame.Domain;

public sealed class PlayerInventory
{
    public static readonly ContainerId ContainerId = new("player_inventory");
    public static readonly InventoryItemSize InventorySize = new(20, 10);

    private readonly Dictionary<ItemId, int> _items = new();
    private readonly ItemContainer _container = new(
        ContainerId,
        "Inventory",
        InventorySize
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

    public bool TryPlaceStatefulItem(StatefulItem item, InventoryItemSize size)
    {
        ArgumentNullException.ThrowIfNull(item);

        var itemRef = ContainerItemRef.Stateful(item.Id);
        return _container.Contains(itemRef)
            || _container.TryAutoPlace(itemRef, size);
    }

    public void SynchronizeStatefulInventoryPlacements(
        IEnumerable<StatefulItem> freelyCarriedItems,
        Func<ItemId, InventoryItemSize> getInventorySize)
    {
        ArgumentNullException.ThrowIfNull(freelyCarriedItems);
        ArgumentNullException.ThrowIfNull(getInventorySize);

        var carriedItems = freelyCarriedItems.ToArray();
        var carriedIds = carriedItems
            .Select(item => item.Id)
            .ToHashSet();

        foreach (var placement in _container.Placements
            .Where(placement => placement.Item.Kind == ContainerItemRefKind.Stateful
                && placement.Item.StatefulItemId is not null
                && !carriedIds.Contains(placement.Item.StatefulItemId.Value))
            .ToArray())
        {
            _container.Remove(placement.Item);
        }

        foreach (var item in carriedItems)
        {
            TryPlaceStatefulItem(item, getInventorySize(item.ItemId));
        }
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

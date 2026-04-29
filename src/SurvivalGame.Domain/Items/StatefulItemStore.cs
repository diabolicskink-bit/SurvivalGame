namespace SurvivalGame.Domain;

public sealed class StatefulItemStore
{
    private readonly Dictionary<StatefulItemId, StatefulItem> _items = new();
    private int _nextItemId = 1;

    public IReadOnlyCollection<StatefulItem> Items => _items.Values.ToArray();

    public StatefulItem Create(
        ItemId itemId,
        int quantity,
        StatefulItemLocation location,
        FirearmCatalog? firearmCatalog = null,
        ItemCondition condition = ItemCondition.Good,
        ItemCatalog? itemCatalog = null)
    {
        var item = new StatefulItem(new StatefulItemId(_nextItemId++), itemId, quantity, location, condition);
        AttachKnownState(item, firearmCatalog, itemCatalog);
        _items.Add(item.Id, item);
        return item;
    }

    public StatefulItem Get(StatefulItemId itemId)
    {
        if (TryGet(itemId, out var item))
        {
            return item;
        }

        throw new KeyNotFoundException($"Stateful item '{itemId}' is not tracked.");
    }

    public bool TryGet(StatefulItemId itemId, out StatefulItem item)
    {
        if (_items.TryGetValue(itemId, out var foundItem))
        {
            item = foundItem;
            return true;
        }

        item = null!;
        return false;
    }

    public IReadOnlyList<StatefulItem> InPlayerInventory()
    {
        return _items.Values
            .Where(item => item.Location is PlayerInventoryLocation)
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public IReadOnlyList<StatefulItem> OnGround(GridPosition position, SiteId? siteId = null)
    {
        var resolvedSiteId = siteId ?? SiteId.Default;
        return _items.Values
            .Where(item => item.Location is GroundLocation g
                && g.Position == position
                && g.SiteId == resolvedSiteId)
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public IReadOnlyList<StatefulItem> OnGroundInSite(SiteId? siteId)
    {
        if (siteId is null)
        {
            return Array.Empty<StatefulItem>();
        }

        return _items.Values
            .Where(item => item.Location is GroundLocation g && g.SiteId == siteId)
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public IReadOnlyList<StatefulItem> Equipped()
    {
        return _items.Values
            .Where(item => item.Location is EquipmentLocation)
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public StatefulItem? EquippedIn(EquipmentSlotId slotId)
    {
        ArgumentNullException.ThrowIfNull(slotId);
        return _items.Values.FirstOrDefault(item =>
            item.Location is EquipmentLocation e && e.SlotId == slotId);
    }

    public IReadOnlyList<StatefulItem> ContainedIn(StatefulItemId parentItemId)
    {
        return _items.Values
            .Where(item => item.Location is ContainedLocation c && c.ParentItemId == parentItemId)
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public IReadOnlyList<StatefulItem> InTravelCargo()
    {
        return _items.Values
            .Where(item => item.Location is TravelCargoLocation)
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public StatefulItem? InsertedIn(StatefulItemId parentItemId)
    {
        return _items.Values.FirstOrDefault(item =>
            item.Location is InsertedLocation i && i.ParentItemId == parentItemId);
    }

    public bool IsFreelyCarried(StatefulItemId itemId)
    {
        return TryGet(itemId, out var item)
            && item.Location is PlayerInventoryLocation;
    }

    public void MoveToInventory(StatefulItemId itemId)
    {
        MoveItem(itemId, StatefulItemLocation.PlayerInventory());
    }

    public void MoveToGround(StatefulItemId itemId, GridPosition position)
    {
        MoveItem(itemId, StatefulItemLocation.Ground(position));
    }

    public void MoveToGround(StatefulItemId itemId, GridPosition position, SiteId siteId)
    {
        MoveItem(itemId, StatefulItemLocation.Ground(position, siteId));
    }

    public void MoveToEquipment(StatefulItemId itemId, EquipmentSlotId slotId)
    {
        MoveItem(itemId, StatefulItemLocation.Equipment(slotId));
    }

    public void MoveToInserted(StatefulItemId itemId, StatefulItemId parentItemId)
    {
        MoveItem(itemId, StatefulItemLocation.Inserted(parentItemId));
    }

    public void MoveToContained(StatefulItemId itemId, StatefulItemId parentItemId)
    {
        var item = Get(itemId);
        if (item.Location is ContainedLocation prevContained
            && TryGet(prevContained.ParentItemId, out var previousParentItem))
        {
            previousParentItem.RemoveContent(itemId);
        }

        MoveItem(itemId, StatefulItemLocation.Contained(parentItemId));
        Get(parentItemId).AddContent(itemId);
    }

    public void MoveToTravelCargo(StatefulItemId itemId)
    {
        MoveItem(itemId, StatefulItemLocation.TravelCargo());
    }

    private void MoveItem(StatefulItemId itemId, StatefulItemLocation location)
    {
        var item = Get(itemId);
        if (item.Location is ContainedLocation prevContained
            && TryGet(prevContained.ParentItemId, out var previousParentItem))
        {
            previousParentItem.RemoveContent(itemId);
        }

        item.MoveTo(location);
    }

    private static void AttachKnownState(StatefulItem item, FirearmCatalog? firearmCatalog, ItemCatalog? itemCatalog)
    {
        if (itemCatalog is not null
            && itemCatalog.TryGet(item.ItemId, out var definition)
            && definition.FuelContainer is not null)
        {
            item.AttachFuelContainerState(new FuelContainerState(definition.FuelContainer.Capacity));
        }

        if (firearmCatalog is null)
        {
            return;
        }

        if (firearmCatalog.TryGetFeedDevice(item.ItemId, out var feedDevice))
        {
            item.AttachFeedDeviceState(feedDevice.CreateState());
        }

        if (firearmCatalog.TryGetWeapon(item.ItemId, out var weapon))
        {
            var builtInFeed = weapon.UsesBuiltInFeed ? weapon.CreateBuiltInFeedState() : null;
            item.AttachWeaponState(new StatefulWeaponState(weapon.ItemId, builtInFeed));
        }
    }
}

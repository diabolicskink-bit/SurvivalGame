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
        ItemCondition condition = ItemCondition.Good)
    {
        var item = new StatefulItem(new StatefulItemId(_nextItemId++), itemId, quantity, location, condition);
        AttachKnownState(item, firearmCatalog);
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
        return Items
            .Where(item => item.Location.Kind == StatefulItemLocationKind.PlayerInventory)
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public IReadOnlyList<StatefulItem> OnGround(GridPosition position, string? siteId = null)
    {
        return Items
            .Where(item => item.Location.Kind == StatefulItemLocationKind.Ground
                && item.Location.Position == position
                && string.Equals(item.Location.SiteId, NormalizeSiteId(siteId), StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public IReadOnlyList<StatefulItem> OnGroundInSite(string? siteId)
    {
        return Items
            .Where(item => item.Location.Kind == StatefulItemLocationKind.Ground
                && string.Equals(item.Location.SiteId, NormalizeSiteId(siteId), StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public IReadOnlyList<StatefulItem> Equipped()
    {
        return Items
            .Where(item => item.Location.Kind == StatefulItemLocationKind.Equipment)
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public StatefulItem? EquippedIn(EquipmentSlotId slotId)
    {
        ArgumentNullException.ThrowIfNull(slotId);

        return Items.FirstOrDefault(item =>
            item.Location.Kind == StatefulItemLocationKind.Equipment
            && item.Location.EquipmentSlotId == slotId
        );
    }

    public IReadOnlyList<StatefulItem> ContainedIn(StatefulItemId parentItemId)
    {
        return Items
            .Where(item => item.Location.Kind == StatefulItemLocationKind.Contained && item.Location.ParentItemId == parentItemId)
            .OrderBy(item => item.Id.Value)
            .ToArray();
    }

    public StatefulItem? InsertedIn(StatefulItemId parentItemId)
    {
        return Items.FirstOrDefault(item =>
            item.Location.Kind == StatefulItemLocationKind.Inserted
            && item.Location.ParentItemId == parentItemId
        );
    }

    public bool IsFreelyCarried(StatefulItemId itemId)
    {
        return TryGet(itemId, out var item)
            && item.Location.Kind == StatefulItemLocationKind.PlayerInventory;
    }

    public void MoveToInventory(StatefulItemId itemId)
    {
        MoveItem(itemId, StatefulItemLocation.PlayerInventory());
    }

    public void MoveToGround(StatefulItemId itemId, GridPosition position, string? siteId = null)
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
        var previousParent = item.Location.ParentItemId;
        if (previousParent is not null && TryGet(previousParent.Value, out var previousParentItem))
        {
            previousParentItem.RemoveContent(itemId);
        }

        MoveItem(itemId, StatefulItemLocation.Contained(parentItemId));
        Get(parentItemId).AddContent(itemId);
    }

    private void MoveItem(StatefulItemId itemId, StatefulItemLocation location)
    {
        var item = Get(itemId);
        var previousParent = item.Location.ParentItemId;
        if (item.Location.Kind == StatefulItemLocationKind.Contained
            && previousParent is not null
            && TryGet(previousParent.Value, out var previousParentItem))
        {
            previousParentItem.RemoveContent(itemId);
        }

        item.MoveTo(location);
    }

    private static string NormalizeSiteId(string? siteId)
    {
        return string.IsNullOrWhiteSpace(siteId)
            ? PrototypeGameState.DefaultSiteId
            : siteId.Trim();
    }

    private static void AttachKnownState(StatefulItem item, FirearmCatalog? firearmCatalog)
    {
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

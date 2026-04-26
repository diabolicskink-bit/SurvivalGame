namespace SurvivalGame.Domain;

public sealed class ItemDescriber
{
    private readonly ItemCatalog _itemCatalog;

    public ItemDescriber(ItemCatalog itemCatalog)
    {
        ArgumentNullException.ThrowIfNull(itemCatalog);
        _itemCatalog = itemCatalog;
    }

    public string FormatStack(GroundItemStack stack)
    {
        var itemName = GetItemName(stack.ItemId);
        return stack.Quantity == 1 ? itemName : $"{stack.Quantity} x {itemName}";
    }

    public string GetItemName(ItemId itemId)
    {
        return _itemCatalog.TryGet(itemId, out var item)
            ? item.Name
            : itemId.ToString();
    }

    public InventoryItemSize GetInventorySize(ItemId itemId)
    {
        return _itemCatalog.TryGet(itemId, out var item)
            ? item.InventorySize
            : InventoryItemSize.Default;
    }

    public bool UsesInventoryGrid(ItemId itemId)
    {
        return !_itemCatalog.TryGet(itemId, out var item) || InventoryGridRules.UsesGrid(item);
    }

    public string DescribeStackItem(ItemId itemId, PrototypeGameState state)
    {
        var quantity = state.Player.Inventory.CountOf(itemId);
        var equippedSlots = state.Player.Equipment.Slots
            .Where(slot => state.Player.Equipment.TryGetEquippedItem(slot.Id, out var equippedItem)
                && equippedItem.ItemId == itemId)
            .Select(slot => slot.DisplayName)
            .ToArray();
        var location = quantity > 0 && equippedSlots.Length > 0
            ? $"Inventory x{quantity}; Equipped: {string.Join(", ", equippedSlots)}"
            : quantity > 0
                ? $"Inventory x{quantity}"
                : $"Equipped: {string.Join(", ", equippedSlots)}";

        if (!_itemCatalog.TryGet(itemId, out var definition))
        {
            return $"{itemId}. Location: {location}.";
        }

        var tags = definition.Tags.Count == 0
            ? "none"
            : string.Join(", ", definition.Tags);
        var description = string.IsNullOrWhiteSpace(definition.Description)
            ? string.Empty
            : $" {definition.Description}";

        return $"{definition.DisplayName} - {definition.Category}. Tags: {tags}. Location: {location}.{description}";
    }

    public string FormatStatefulItem(StatefulItem item)
    {
        var name = _itemCatalog.TryGet(item.ItemId, out var definition)
            ? definition.DisplayName
            : item.ItemId.ToString();

        var stateText = item.FeedDevice is not null
            ? $" {FormatFeedState(item.FeedDevice)}"
            : string.Empty;

        return $"{name} [{item.Id}]{stateText}";
    }

    public string DescribeStatefulItem(StatefulItem item)
    {
        var definition = _itemCatalog.TryGet(item.ItemId, out var foundDefinition)
            ? foundDefinition
            : null;
        var name = definition?.DisplayName ?? item.ItemId.ToString();
        var category = definition?.Category ?? "Unknown";
        var tags = definition is null || definition.Tags.Count == 0
            ? "none"
            : string.Join(", ", definition.Tags);
        var location = FormatLocation(item.Location);
        var details = $"{name} [{item.Id}] - {category}. Tags: {tags}. Condition: {item.Condition}. Location: {location}.";

        if (item.FeedDevice is not null)
        {
            details += $" Feed: {FormatFeedState(item.FeedDevice)} accepts {item.FeedDevice.AmmoSize}.";
        }

        if (item.Weapon is not null)
        {
            var feed = item.Weapon.BuiltInFeed;
            var inserted = item.Weapon.InsertedFeedDeviceItemId?.ToString() ?? "none";
            details += feed is null
                ? $" Inserted feed: {inserted}."
                : $" Built-in feed: {FormatFeedState(feed)}.";
        }

        if (!string.IsNullOrWhiteSpace(definition?.Description))
        {
            details += $" {definition.Description}";
        }

        return details;
    }

    private static string FormatFeedState(FeedDeviceState feedDevice)
    {
        var loaded = feedDevice.LoadedAmmunitionVariant is null
            ? "empty"
            : $"{feedDevice.LoadedCount}/{feedDevice.Capacity} {feedDevice.LoadedAmmunitionVariant}";

        return $"({loaded})";
    }

    private static string FormatLocation(StatefulItemLocation location)
    {
        return location switch
        {
            PlayerInventoryLocation => "inventory",
            GroundLocation g => $"ground {g.SiteId} {g.Position.X}, {g.Position.Y}",
            EquipmentLocation e => $"equipment {e.SlotId}",
            InsertedLocation i => $"inserted in {i.ParentItemId}",
            ContainedLocation c => $"inside {c.ParentItemId}",
            _ => location.Kind.ToString()
        };
    }
}

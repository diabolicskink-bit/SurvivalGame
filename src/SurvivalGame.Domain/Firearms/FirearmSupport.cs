namespace SurvivalGame.Domain;

internal sealed class FirearmItemServices
{
    private readonly FirearmCatalog _firearms;
    private readonly ItemCatalog? _items;

    public FirearmItemServices(FirearmCatalog firearms, ItemCatalog? items)
    {
        ArgumentNullException.ThrowIfNull(firearms);
        _firearms = firearms;
        _items = items;
    }

    public string GetAmmunitionName(ItemId ammunitionItemId)
    {
        return _firearms.TryGetAmmunition(ammunitionItemId, out var ammunition)
            ? ammunition.Name
            : ammunitionItemId.ToString();
    }

    public string GetItemName(ItemId itemId)
    {
        if (_firearms.TryGetWeapon(itemId, out var weapon))
        {
            return weapon.Name;
        }

        if (_firearms.TryGetFeedDevice(itemId, out var feedDevice))
        {
            return feedDevice.Name;
        }

        return itemId.ToString();
    }

    public string FormatStatefulName(StatefulItem item, ItemCatalog itemCatalog)
    {
        var name = itemCatalog.TryGet(item.ItemId, out var definition)
            ? definition.DisplayName
            : item.ItemId.ToString();

        return $"{name} [{item.Id}]";
    }

    public bool CanAddToInventory(PlayerInventory inventory, ItemId itemId)
    {
        return inventory.CanAdd(itemId, GetInventorySize(itemId), UsesInventoryGrid(itemId));
    }

    public bool TryAddToInventory(PlayerInventory inventory, ItemId itemId, int quantity = 1)
    {
        return inventory.TryAdd(itemId, quantity, GetInventorySize(itemId), UsesInventoryGrid(itemId));
    }

    private InventoryItemSize GetInventorySize(ItemId itemId)
    {
        return _items is not null && _items.TryGet(itemId, out var item)
            ? item.InventorySize
            : InventoryItemSize.Default;
    }

    private bool UsesInventoryGrid(ItemId itemId)
    {
        if (_firearms.TryGetAmmunition(itemId, out _))
        {
            return false;
        }

        return _items is null
            || !_items.TryGet(itemId, out var item)
            || InventoryGridRules.UsesGrid(item);
    }
}

internal static class FirearmTiming
{
    public static int CalculateLoadQuantity(FeedDeviceState feedDevice, int availableQuantity)
    {
        ArgumentNullException.ThrowIfNull(feedDevice);
        return Math.Min(availableQuantity, feedDevice.Capacity - feedDevice.LoadedCount);
    }

    public static int CalculateLoadTicks(int loadedQuantity)
    {
        return loadedQuantity * FirearmActionService.LoadRoundTickCost;
    }

    public static int CalculateReloadTicks(int loadedQuantity)
    {
        return FirearmActionService.RemoveFeedDeviceTickCost
            + CalculateLoadTicks(loadedQuantity)
            + FirearmActionService.InsertFeedDeviceTickCost;
    }

    public static string FormatReloadMessage(
        int loadedQuantity,
        string ammunitionName,
        string feedDeviceName)
    {
        var loadTicks = CalculateLoadTicks(loadedQuantity);
        return $"Reloaded {loadedQuantity} {ammunitionName} into {feedDeviceName} "
            + $"(remove {FirearmActionService.RemoveFeedDeviceTickCost} ticks, load {loadTicks} ticks, insert {FirearmActionService.InsertFeedDeviceTickCost} ticks).";
    }
}

internal sealed record FirearmValidation<TPlan>(TPlan? Plan, IReadOnlyList<string> Messages)
{
    public bool Succeeded => Plan is not null;

    public static FirearmValidation<TPlan> Success(TPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new FirearmValidation<TPlan>(plan, Array.Empty<string>());
    }

    public static FirearmValidation<TPlan> Failure(params string[] messages)
    {
        return new FirearmValidation<TPlan>(default, messages);
    }

    public GameActionResult ToFailureResult()
    {
        return GameActionResult.Failure(Messages.ToArray());
    }
}

internal sealed record LoadAmmunitionPlan(
    IFirearmFeedRef Feed,
    AmmunitionDefinition Ammunition,
    int AvailableQuantity);

internal sealed record UnloadFeedPlan(IFirearmFeedRef Feed);

internal sealed record InsertFeedPlan(
    IFirearmWeaponRef Weapon,
    IFirearmDetachableFeedRef Feed,
    FeedDeviceDefinition FeedDefinition);

internal sealed record RemoveFeedPlan(IFirearmWeaponRef Weapon, IFirearmDetachableFeedRef Feed);

internal sealed record ReloadFeedPlan(
    IFirearmWeaponRef Weapon,
    IFirearmDetachableFeedRef Feed,
    AmmunitionDefinition Ammunition,
    int AvailableQuantity);

internal sealed record TestFirePlan(string WeaponName, FeedDeviceState ActiveFeed);

internal sealed record ToggleFireModePlan(IFirearmWeaponRef Weapon);

internal sealed record ShootNpcPlan(
    string WeaponName,
    WeaponFireMode FireMode,
    FeedDeviceState ActiveFeed,
    NpcState Target,
    AmmunitionDefinition Ammunition,
    int RoundCount,
    int DistanceTiles,
    ModifiedWeaponStats Stats,
    int DamageOnHit);

internal sealed record InstallWeaponModPlan(
    StatefulItem WeaponItem,
    WeaponDefinition WeaponDefinition,
    StatefulItem ModItem,
    WeaponModDefinition ModDefinition);

internal sealed record RemoveWeaponModPlan(
    StatefulItem WeaponItem,
    WeaponDefinition WeaponDefinition,
    WeaponModSlotId SlotId,
    StatefulItem ModItem,
    WeaponModDefinition ModDefinition);

internal sealed record LoadAmmunitionResult(int LoadedQuantity);

internal sealed record UnloadFeedResult(int Quantity, ItemId AmmunitionItemId);

internal sealed record RemoveFeedResult(ItemId FeedDeviceItemId);

internal sealed record ReloadFeedResult(int LoadedQuantity);

internal sealed record TestFireResult(ItemId AmmunitionItemId);

internal sealed record ToggleFireModeResult(string WeaponName, WeaponFireMode CurrentFireMode);

internal sealed record ShootNpcResult(int DealtDamage, int ConsumedRounds, bool TargetDisabled, bool Hit);

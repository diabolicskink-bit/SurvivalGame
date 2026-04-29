namespace SurvivalGame.Domain;

internal sealed class FirearmStateOperations
{
    private readonly FirearmItemServices _items;

    public FirearmStateOperations(FirearmItemServices items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _items = items;
    }

    public LoadAmmunitionResult LoadAmmunition(LoadAmmunitionPlan plan, PlayerInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(inventory);

        var feed = plan.Feed.EnsureState();
        var loadedQuantity = feed.Load(plan.Ammunition, plan.AvailableQuantity);
        inventory.TryRemove(plan.Ammunition.ItemId, loadedQuantity);

        return new LoadAmmunitionResult(loadedQuantity);
    }

    public UnloadFeedResult UnloadFeed(UnloadFeedPlan plan, PlayerInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(inventory);

        var feed = plan.Feed.EnsureState();
        var unloaded = feed.UnloadAll()
            ?? throw new InvalidOperationException($"{feed.DisplayName} lost its loaded ammunition before unloading.");

        _items.TryAddToInventory(inventory, unloaded.ItemId, unloaded.Quantity);
        return new UnloadFeedResult(unloaded.Quantity, unloaded.ItemId);
    }

    public void InsertFeed(InsertFeedPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        plan.Weapon.InsertFeedDevice(plan.Feed);
    }

    public RemoveFeedResult RemoveFeed(RemoveFeedPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var removedFeed = plan.Weapon.RemoveFeedDevice()
            ?? throw new InvalidOperationException($"{plan.Weapon.Definition.Name} lost its inserted feed before removal.");

        return new RemoveFeedResult(removedFeed.ItemId);
    }

    public ReloadFeedResult Reload(ReloadFeedPlan plan, PlayerInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(inventory);

        var feed = plan.Feed.EnsureState();
        var loadedQuantity = FirearmTiming.CalculateLoadQuantity(feed, plan.AvailableQuantity);

        _ = plan.Weapon.RemoveFeedDevice()
            ?? throw new InvalidOperationException($"{plan.Weapon.Definition.Name} lost its inserted feed before reload.");
        feed.Load(plan.Ammunition, loadedQuantity);
        inventory.TryRemove(plan.Ammunition.ItemId, loadedQuantity);
        plan.Weapon.InsertFeedDevice(plan.Feed);

        return new ReloadFeedResult(loadedQuantity);
    }

    public TestFireResult TestFire(TestFirePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var consumed = plan.ActiveFeed.ConsumeOne()
            ?? throw new InvalidOperationException($"{plan.WeaponName} lost its loaded ammunition before test firing.");

        return new TestFireResult(consumed.ItemId);
    }

    public ToggleFireModeResult ToggleFireMode(ToggleFireModePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var currentMode = plan.Weapon.ToggleFireMode();
        return new ToggleFireModeResult(plan.Weapon.Definition.Name, currentMode);
    }

    public ShootNpcResult Shoot(ShootNpcPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var consumed = plan.ActiveFeed.Consume(plan.RoundCount);
        if (consumed is null)
        {
            throw new InvalidOperationException($"{plan.WeaponName} lost its loaded ammunition before shooting.");
        }

        var dealtDamage = plan.Target.TakeDamage(plan.Damage);
        return new ShootNpcResult(dealtDamage, consumed.Quantity, plan.Target.IsDisabled);
    }

    public void InstallWeaponMod(InstallWeaponModPlan plan, StatefulItemStore items)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(items);

        var weaponState = plan.WeaponItem.Weapon
            ?? throw new InvalidOperationException($"{plan.WeaponDefinition.Name} lost its weapon state before installing a mod.");

        weaponState.InstallMod(plan.ModDefinition.Slot, plan.ModItem.Id);
        items.MoveToInserted(plan.ModItem.Id, plan.WeaponItem.Id);
    }

    public void RemoveWeaponMod(RemoveWeaponModPlan plan, StatefulItemStore items)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(items);

        var weaponState = plan.WeaponItem.Weapon
            ?? throw new InvalidOperationException($"{plan.WeaponDefinition.Name} lost its weapon state before removing a mod.");

        var removedModItemId = weaponState.RemoveMod(plan.SlotId);
        if (removedModItemId is null)
        {
            throw new InvalidOperationException($"{plan.WeaponDefinition.Name} lost its {plan.SlotId} mod before removal.");
        }

        items.MoveToInventory(removedModItemId.Value);
    }
}

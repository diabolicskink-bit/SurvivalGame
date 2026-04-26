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

    public ShootNpcResult Shoot(ShootNpcPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var consumed = plan.ActiveFeed.ConsumeOne();
        if (consumed is null)
        {
            throw new InvalidOperationException($"{plan.WeaponName} lost its loaded ammunition before shooting.");
        }

        var dealtDamage = plan.Target.TakeDamage(plan.Ammunition.Damage);
        return new ShootNpcResult(dealtDamage, plan.Target.IsDisabled);
    }
}

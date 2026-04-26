namespace SurvivalGame.Domain;

public sealed class FirearmActionService
{
    public const int LoadRoundTickCost = 10;
    public const int RemoveFeedDeviceTickCost = 25;
    public const int InsertFeedDeviceTickCost = 25;
    public const int InstallWeaponModTickCost = 50;
    public const int RemoveWeaponModTickCost = 50;

    private readonly FirearmItemServices _items;
    private readonly FirearmValidator _validator;
    private readonly FirearmStateOperations _operations;
    private readonly FirearmActionProvider _actions;

    public FirearmActionService(FirearmCatalog catalog, ItemCatalog? itemCatalog = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        _items = new FirearmItemServices(catalog, itemCatalog);
        var refs = new FirearmRefFactory(catalog, _items);
        _validator = new FirearmValidator(catalog, _items, refs);
        _operations = new FirearmStateOperations(_items);
        _actions = new FirearmActionProvider(catalog, _items);
    }

    public IReadOnlyList<AvailableAction> GetAvailableActions(PrototypeGameState state)
    {
        return _actions.GetAvailableActions(state);
    }

    public IReadOnlyList<AvailableAction> GetAvailableStatefulActions(PrototypeGameState state, ItemCatalog itemCatalog)
    {
        return _actions.GetAvailableStatefulActions(state, itemCatalog);
    }

    public GameActionResult LoadFeedDevice(PrototypeGameState state, ItemId feedDeviceItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(feedDeviceItemId);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        return LoadAmmunition(
            _validator.ValidateLoadFeedDevice(state, feedDeviceItemId, ammunitionItemId),
            state.Player.Inventory
        );
    }

    public GameActionResult UnloadFeedDevice(PrototypeGameState state, ItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(feedDeviceItemId);

        return UnloadFeed(
            _validator.ValidateUnloadFeedDevice(state, feedDeviceItemId),
            state.Player.Inventory
        );
    }

    public GameActionResult InsertFeedDevice(PrototypeGameState state, ItemId weaponItemId, ItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItemId);
        ArgumentNullException.ThrowIfNull(feedDeviceItemId);

        return InsertFeed(_validator.ValidateInsertFeedDevice(state, weaponItemId, feedDeviceItemId));
    }

    public GameActionResult RemoveFeedDevice(PrototypeGameState state, ItemId weaponItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItemId);

        return RemoveFeed(_validator.ValidateRemoveFeedDevice(state, weaponItemId));
    }

    public GameActionResult LoadWeapon(PrototypeGameState state, ItemId weaponItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItemId);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        return LoadAmmunition(
            _validator.ValidateLoadWeapon(state, weaponItemId, ammunitionItemId),
            state.Player.Inventory
        );
    }

    public GameActionResult ReloadWeapon(PrototypeGameState state, ItemId weaponItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItemId);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        return ReloadFeed(
            _validator.ValidateReloadWeapon(state, weaponItemId, ammunitionItemId),
            state.Player.Inventory
        );
    }

    public GameActionResult TestFire(PrototypeGameState state, ItemId weaponItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItemId);

        return TestFire(_validator.ValidateTestFire(state, weaponItemId));
    }

    public GameActionResult LoadStatefulFeedDevice(PrototypeGameState state, StatefulItemId feedDeviceItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        return LoadAmmunition(
            _validator.ValidateLoadStatefulFeedDevice(state, feedDeviceItemId, ammunitionItemId),
            state.Player.Inventory
        );
    }

    public GameActionResult UnloadStatefulFeedDevice(PrototypeGameState state, StatefulItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(state);

        return UnloadFeed(
            _validator.ValidateUnloadStatefulFeedDevice(state, feedDeviceItemId),
            state.Player.Inventory
        );
    }

    public GameActionResult InsertStatefulFeedDevice(PrototypeGameState state, StatefulItemId weaponItemId, StatefulItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(state);

        return InsertFeed(_validator.ValidateInsertStatefulFeedDevice(state, weaponItemId, feedDeviceItemId));
    }

    public GameActionResult RemoveStatefulFeedDevice(PrototypeGameState state, StatefulItemId weaponItemId)
    {
        ArgumentNullException.ThrowIfNull(state);

        return RemoveFeed(_validator.ValidateRemoveStatefulFeedDevice(state, weaponItemId));
    }

    public GameActionResult ReloadStatefulWeapon(PrototypeGameState state, StatefulItemId weaponItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        return ReloadFeed(
            _validator.ValidateReloadStatefulWeapon(state, weaponItemId, ammunitionItemId),
            state.Player.Inventory
        );
    }

    public GameActionResult LoadStatefulWeapon(PrototypeGameState state, StatefulItemId weaponItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        return LoadAmmunition(
            _validator.ValidateLoadStatefulWeapon(state, weaponItemId, ammunitionItemId),
            state.Player.Inventory
        );
    }

    public GameActionResult TestFireStatefulWeapon(PrototypeGameState state, StatefulItemId weaponItemId)
    {
        ArgumentNullException.ThrowIfNull(state);

        return TestFire(_validator.ValidateTestFireStatefulWeapon(state, weaponItemId));
    }

    public GameActionResult InstallStatefulWeaponMod(
        PrototypeGameState state,
        StatefulItemId weaponItemId,
        StatefulItemId modItemId)
    {
        ArgumentNullException.ThrowIfNull(state);

        return InstallWeaponMod(_validator.ValidateInstallStatefulWeaponMod(state, weaponItemId, modItemId), state.StatefulItems);
    }

    public GameActionResult RemoveStatefulWeaponMod(
        PrototypeGameState state,
        StatefulItemId weaponItemId,
        WeaponModSlotId slotId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(slotId);

        return RemoveWeaponMod(_validator.ValidateRemoveStatefulWeaponMod(state, weaponItemId, slotId), state.StatefulItems);
    }

    public GameActionResult ShootEquippedNpc(PrototypeGameState state, NpcId targetNpcId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(targetNpcId);

        var validation = _validator.ValidateShootEquippedNpc(state, targetNpcId);
        if (!validation.Succeeded)
        {
            return validation.ToFailureResult();
        }

        var plan = RequirePlan(validation);
        var result = _operations.Shoot(plan);
        var status = result.TargetDisabled
            ? $"{plan.Target.Name} is disabled."
            : $"{plan.Target.Name} health: {plan.Target.Health.Current}/{plan.Target.Health.Maximum}.";

        return GameActionResult.Success(
            0,
            $"Shot {plan.Target.Name} with {plan.WeaponName} using {plan.Ammunition.Name} for {result.DealtDamage} damage.",
            status
        );
    }

    private GameActionResult LoadAmmunition(
        FirearmValidation<LoadAmmunitionPlan> validation,
        PlayerInventory inventory)
    {
        if (!validation.Succeeded)
        {
            return validation.ToFailureResult();
        }

        var plan = RequirePlan(validation);
        var result = _operations.LoadAmmunition(plan, inventory);
        var feed = plan.Feed.EnsureState();

        return GameActionResult.Success(
            FirearmTiming.CalculateLoadTicks(result.LoadedQuantity),
            $"Loaded {result.LoadedQuantity} {plan.Ammunition.Name} into {feed.DisplayName}."
        );
    }

    private GameActionResult UnloadFeed(
        FirearmValidation<UnloadFeedPlan> validation,
        PlayerInventory inventory)
    {
        if (!validation.Succeeded)
        {
            return validation.ToFailureResult();
        }

        var result = _operations.UnloadFeed(RequirePlan(validation), inventory);
        return GameActionResult.Success(
            0,
            $"Unloaded {result.Quantity} {_items.GetAmmunitionName(result.AmmunitionItemId)}."
        );
    }

    private GameActionResult InsertFeed(FirearmValidation<InsertFeedPlan> validation)
    {
        if (!validation.Succeeded)
        {
            return validation.ToFailureResult();
        }

        var plan = RequirePlan(validation);
        _operations.InsertFeed(plan);

        return GameActionResult.Success(
            InsertFeedDeviceTickCost,
            $"Inserted {plan.FeedDefinition.Name} into {plan.Weapon.Definition.Name}."
        );
    }

    private GameActionResult RemoveFeed(FirearmValidation<RemoveFeedPlan> validation)
    {
        if (!validation.Succeeded)
        {
            return validation.ToFailureResult();
        }

        var plan = RequirePlan(validation);
        var result = _operations.RemoveFeed(plan);

        return GameActionResult.Success(
            RemoveFeedDeviceTickCost,
            $"Removed {_items.GetItemName(result.FeedDeviceItemId)} from {plan.Weapon.Definition.Name}."
        );
    }

    private GameActionResult ReloadFeed(
        FirearmValidation<ReloadFeedPlan> validation,
        PlayerInventory inventory)
    {
        if (!validation.Succeeded)
        {
            return validation.ToFailureResult();
        }

        var plan = RequirePlan(validation);
        var result = _operations.Reload(plan, inventory);
        var feed = plan.Feed.EnsureState();

        return GameActionResult.Success(
            FirearmTiming.CalculateReloadTicks(result.LoadedQuantity),
            FirearmTiming.FormatReloadMessage(result.LoadedQuantity, plan.Ammunition.Name, feed.DisplayName)
        );
    }

    private GameActionResult TestFire(FirearmValidation<TestFirePlan> validation)
    {
        if (!validation.Succeeded)
        {
            return validation.ToFailureResult();
        }

        var plan = RequirePlan(validation);
        var result = _operations.TestFire(plan);

        return GameActionResult.Success(
            0,
            $"Test fired {plan.WeaponName} using {_items.GetAmmunitionName(result.AmmunitionItemId)}."
        );
    }

    private GameActionResult InstallWeaponMod(
        FirearmValidation<InstallWeaponModPlan> validation,
        StatefulItemStore statefulItems)
    {
        if (!validation.Succeeded)
        {
            return validation.ToFailureResult();
        }

        var plan = RequirePlan(validation);
        _operations.InstallWeaponMod(plan, statefulItems);

        return GameActionResult.Success(
            InstallWeaponModTickCost,
            $"Installed {plan.ModDefinition.Name} on {plan.WeaponDefinition.Name}."
        );
    }

    private GameActionResult RemoveWeaponMod(
        FirearmValidation<RemoveWeaponModPlan> validation,
        StatefulItemStore statefulItems)
    {
        if (!validation.Succeeded)
        {
            return validation.ToFailureResult();
        }

        var plan = RequirePlan(validation);
        _operations.RemoveWeaponMod(plan, statefulItems);

        return GameActionResult.Success(
            RemoveWeaponModTickCost,
            $"Removed {plan.ModDefinition.Name} from {plan.WeaponDefinition.Name}."
        );
    }

    private static TPlan RequirePlan<TPlan>(FirearmValidation<TPlan> validation)
        where TPlan : class
    {
        return validation.Plan
            ?? throw new InvalidOperationException("Firearm validation succeeded without a plan.");
    }
}

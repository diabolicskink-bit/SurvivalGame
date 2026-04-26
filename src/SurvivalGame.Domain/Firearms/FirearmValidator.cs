namespace SurvivalGame.Domain;

internal sealed class FirearmValidator
{
    private readonly FirearmCatalog _catalog;
    private readonly FirearmItemServices _items;
    private readonly FirearmRefFactory _refs;

    public FirearmValidator(FirearmCatalog catalog, FirearmItemServices items, FirearmRefFactory refs)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(refs);

        _catalog = catalog;
        _items = items;
        _refs = refs;
    }

    public FirearmValidation<LoadAmmunitionPlan> ValidateLoadFeedDevice(
        PrototypeGameState state,
        ItemId feedDeviceItemId,
        ItemId ammunitionItemId)
    {
        if (!_refs.TryGetStackFeed(state.Player, feedDeviceItemId, out var feed, out var feedDefinition))
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"Unknown feed device: {feedDeviceItemId}.");
        }

        if (!feed.IsFreelyAvailable)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"{feedDefinition.Name} is not in your inventory.");
        }

        return ValidateLoadAmmunition(
            feed,
            ammunitionItemId,
            state.Player.Inventory.CountOf
        );
    }

    public FirearmValidation<UnloadFeedPlan> ValidateUnloadFeedDevice(
        PrototypeGameState state,
        ItemId feedDeviceItemId)
    {
        if (!_refs.TryGetStackFeed(state.Player, feedDeviceItemId, out var feed, out _)
            || feed.ExistingState is null)
        {
            return FirearmValidation<UnloadFeedPlan>.Failure($"Unknown feed device: {feedDeviceItemId}.");
        }

        return ValidateUnloadFeed(feed, state.Player.Inventory);
    }

    public FirearmValidation<InsertFeedPlan> ValidateInsertFeedDevice(
        PrototypeGameState state,
        ItemId weaponItemId,
        ItemId feedDeviceItemId)
    {
        if (!_refs.TryGetOwnedStackWeapon(state.Player, weaponItemId, out var weapon))
        {
            return FirearmValidation<InsertFeedPlan>.Failure("That weapon is not available.");
        }

        if (!_refs.TryGetStackFeed(state.Player, feedDeviceItemId, out var feed, out var feedDefinition))
        {
            return FirearmValidation<InsertFeedPlan>.Failure($"Unknown feed device: {feedDeviceItemId}.");
        }

        if (!feed.IsFreelyAvailable)
        {
            return FirearmValidation<InsertFeedPlan>.Failure($"{feedDefinition.Name} is not in your inventory.");
        }

        if (!weapon.Definition.CanUseFeedDevice(feedDefinition))
        {
            return FirearmValidation<InsertFeedPlan>.Failure("This magazine does not fit that weapon.");
        }

        if (weapon.HasInsertedFeedDevice)
        {
            return FirearmValidation<InsertFeedPlan>.Failure($"{weapon.Definition.Name} already has a feed device inserted.");
        }

        return FirearmValidation<InsertFeedPlan>.Success(new InsertFeedPlan(weapon, feed, feedDefinition));
    }

    public FirearmValidation<RemoveFeedPlan> ValidateRemoveFeedDevice(
        PrototypeGameState state,
        ItemId weaponItemId)
    {
        if (!_refs.TryGetOwnedStackWeapon(state.Player, weaponItemId, out var weapon))
        {
            return FirearmValidation<RemoveFeedPlan>.Failure("That weapon is not available.");
        }

        if (!weapon.HasInsertedFeedDevice || weapon.InsertedFeedDevice is not { } feed)
        {
            return FirearmValidation<RemoveFeedPlan>.Failure($"{weapon.Definition.Name} has no feed device inserted.");
        }

        if (!feed.CanMoveToInventory())
        {
            return FirearmValidation<RemoveFeedPlan>.Failure("Not enough inventory grid space.");
        }

        return FirearmValidation<RemoveFeedPlan>.Success(new RemoveFeedPlan(weapon, feed));
    }

    public FirearmValidation<LoadAmmunitionPlan> ValidateLoadWeapon(
        PrototypeGameState state,
        ItemId weaponItemId,
        ItemId ammunitionItemId)
    {
        if (!_refs.TryGetOwnedStackWeapon(state.Player, weaponItemId, out var weapon))
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure("That weapon is not available.");
        }

        if (weapon.Definition.UsesDetachableFeedDevice)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"{weapon.Definition.Name} must use a compatible feed device.");
        }

        if (!_catalog.TryGetAmmunition(ammunitionItemId, out var ammunition))
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"Unknown ammunition: {ammunitionItemId}.");
        }

        if (!weapon.Definition.AcceptsAmmunition(ammunition))
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"Cannot load {ammunition.Name} into {weapon.Definition.Name}.");
        }

        var availableQuantity = state.Player.Inventory.CountOf(ammunition.ItemId);
        if (availableQuantity < 1)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"No {ammunition.Name} available.");
        }

        var feed = new BuiltInStackFeedRef(state.Player, weapon.Definition);
        var loadFailures = GetLoadFailures(feed, ammunition);
        if (loadFailures.Count > 0)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure(loadFailures.ToArray());
        }

        return FirearmValidation<LoadAmmunitionPlan>.Success(new LoadAmmunitionPlan(feed, ammunition, availableQuantity));
    }

    public FirearmValidation<ReloadFeedPlan> ValidateReloadWeapon(
        PrototypeGameState state,
        ItemId weaponItemId,
        ItemId ammunitionItemId)
    {
        if (!_refs.TryGetOwnedStackWeapon(state.Player, weaponItemId, out var weapon))
        {
            return FirearmValidation<ReloadFeedPlan>.Failure("That weapon is not available.");
        }

        if (!weapon.Definition.UsesDetachableFeedDevice)
        {
            return FirearmValidation<ReloadFeedPlan>.Failure($"{weapon.Definition.Name} does not use a detachable feed device.");
        }

        if (!weapon.HasInsertedFeedDevice)
        {
            return FirearmValidation<ReloadFeedPlan>.Failure($"{weapon.Definition.Name} has no feed device inserted.");
        }

        if (weapon.InsertedFeedDevice is not { } feed)
        {
            return FirearmValidation<ReloadFeedPlan>.Failure("Inserted feed device is not compatible with that weapon.");
        }

        if (feed.ExistingState is null)
        {
            return FirearmValidation<ReloadFeedPlan>.Failure("Inserted feed device state is missing.");
        }

        if (!_catalog.TryGetFeedDevice(feed.ItemId, out var feedDefinition)
            || !weapon.Definition.CanUseFeedDevice(feedDefinition))
        {
            return FirearmValidation<ReloadFeedPlan>.Failure("Inserted feed device is not compatible with that weapon.");
        }

        return ValidateReload(weapon, feed, ammunitionItemId, state.Player.Inventory.CountOf);
    }

    public FirearmValidation<TestFirePlan> ValidateTestFire(
        PrototypeGameState state,
        ItemId weaponItemId)
    {
        if (!_refs.TryGetOwnedStackWeapon(state.Player, weaponItemId, out var weapon))
        {
            return FirearmValidation<TestFirePlan>.Failure("That weapon is not available.");
        }

        if (weapon.ActiveFeed is not { LoadedCount: > 0 } activeFeed)
        {
            return FirearmValidation<TestFirePlan>.Failure("Weapon is empty.");
        }

        return FirearmValidation<TestFirePlan>.Success(new TestFirePlan(weapon.Definition.Name, activeFeed));
    }

    public FirearmValidation<LoadAmmunitionPlan> ValidateLoadStatefulFeedDevice(
        PrototypeGameState state,
        StatefulItemId feedDeviceItemId,
        ItemId ammunitionItemId)
    {
        if (!state.StatefulItems.TryGet(feedDeviceItemId, out var feedDeviceItem)
            || feedDeviceItem.Location is not PlayerInventoryLocation)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure("That feed device is not freely available.");
        }

        if (feedDeviceItem.FeedDevice is null)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure("That item cannot be loaded with ammunition.");
        }

        var feed = new StatefulFeedRef(state.StatefulItems, feedDeviceItem);
        return ValidateLoadAmmunition(feed, ammunitionItemId, state.Player.Inventory.CountOf);
    }

    public FirearmValidation<UnloadFeedPlan> ValidateUnloadStatefulFeedDevice(
        PrototypeGameState state,
        StatefulItemId feedDeviceItemId)
    {
        if (!state.StatefulItems.TryGet(feedDeviceItemId, out var feedDeviceItem)
            || !IsAccessibleFeedDevice(state, feedDeviceItem))
        {
            return FirearmValidation<UnloadFeedPlan>.Failure("That feed device is not available.");
        }

        if (feedDeviceItem.FeedDevice is null)
        {
            return FirearmValidation<UnloadFeedPlan>.Failure("That item does not hold ammunition.");
        }

        var feed = new StatefulFeedRef(state.StatefulItems, feedDeviceItem);
        return ValidateUnloadFeed(feed, state.Player.Inventory);
    }

    public FirearmValidation<InsertFeedPlan> ValidateInsertStatefulFeedDevice(
        PrototypeGameState state,
        StatefulItemId weaponItemId,
        StatefulItemId feedDeviceItemId)
    {
        if (!state.StatefulItems.TryGet(weaponItemId, out var weaponItem)
            || !IsOwnedStatefulWeapon(weaponItem))
        {
            return FirearmValidation<InsertFeedPlan>.Failure("That weapon is not available.");
        }

        if (!state.StatefulItems.TryGet(feedDeviceItemId, out var feedDeviceItem)
            || feedDeviceItem.Location is not PlayerInventoryLocation)
        {
            return FirearmValidation<InsertFeedPlan>.Failure("That feed device is not freely available.");
        }

        if (!_catalog.TryGetWeapon(weaponItem.ItemId, out var weaponDefinition)
            || !_catalog.TryGetFeedDevice(feedDeviceItem.ItemId, out var feedDefinition)
            || weaponItem.Weapon is null
            || feedDeviceItem.FeedDevice is null)
        {
            return FirearmValidation<InsertFeedPlan>.Failure("Those items cannot be combined that way.");
        }

        if (!weaponDefinition.CanUseFeedDevice(feedDefinition))
        {
            return FirearmValidation<InsertFeedPlan>.Failure("This magazine does not fit that weapon.");
        }

        if (weaponItem.Weapon.HasInsertedFeedDevice)
        {
            return FirearmValidation<InsertFeedPlan>.Failure($"{weaponDefinition.Name} already has a feed device inserted.");
        }

        var weapon = new StatefulWeaponRef(state.StatefulItems, weaponItem, weaponDefinition);
        var feed = new StatefulFeedRef(state.StatefulItems, feedDeviceItem);
        return FirearmValidation<InsertFeedPlan>.Success(new InsertFeedPlan(weapon, feed, feedDefinition));
    }

    public FirearmValidation<RemoveFeedPlan> ValidateRemoveStatefulFeedDevice(
        PrototypeGameState state,
        StatefulItemId weaponItemId)
    {
        if (!state.StatefulItems.TryGet(weaponItemId, out var weaponItem)
            || !IsOwnedStatefulWeapon(weaponItem)
            || weaponItem.Weapon is null)
        {
            return FirearmValidation<RemoveFeedPlan>.Failure("That weapon is not available.");
        }

        if (!_catalog.TryGetWeapon(weaponItem.ItemId, out var weaponDefinition))
        {
            return FirearmValidation<RemoveFeedPlan>.Failure("That weapon is not available.");
        }

        var weapon = new StatefulWeaponRef(state.StatefulItems, weaponItem, weaponDefinition);
        if (weapon.InsertedFeedDevice is not { } feed)
        {
            return FirearmValidation<RemoveFeedPlan>.Failure("That weapon has no feed device inserted.");
        }

        return FirearmValidation<RemoveFeedPlan>.Success(new RemoveFeedPlan(weapon, feed));
    }

    public FirearmValidation<ReloadFeedPlan> ValidateReloadStatefulWeapon(
        PrototypeGameState state,
        StatefulItemId weaponItemId,
        ItemId ammunitionItemId)
    {
        if (!state.StatefulItems.TryGet(weaponItemId, out var weaponItem)
            || !IsOwnedStatefulWeapon(weaponItem)
            || weaponItem.Weapon is null)
        {
            return FirearmValidation<ReloadFeedPlan>.Failure("That weapon is not available.");
        }

        if (!_catalog.TryGetWeapon(weaponItem.ItemId, out var weaponDefinition))
        {
            return FirearmValidation<ReloadFeedPlan>.Failure("That item is not a firearm.");
        }

        if (!weaponDefinition.UsesDetachableFeedDevice)
        {
            return FirearmValidation<ReloadFeedPlan>.Failure($"{weaponDefinition.Name} does not use a detachable feed device.");
        }

        if (weaponItem.Weapon.InsertedFeedDeviceItemId is not { } feedDeviceItemId)
        {
            return FirearmValidation<ReloadFeedPlan>.Failure($"{weaponDefinition.Name} has no feed device inserted.");
        }

        if (!state.StatefulItems.TryGet(feedDeviceItemId, out var feedDeviceItem)
            || feedDeviceItem.FeedDevice is null
            || !_catalog.TryGetFeedDevice(feedDeviceItem.ItemId, out var feedDefinition)
            || !weaponDefinition.CanUseFeedDevice(feedDefinition))
        {
            return FirearmValidation<ReloadFeedPlan>.Failure("Inserted feed device is not compatible with that weapon.");
        }

        var weapon = new StatefulWeaponRef(state.StatefulItems, weaponItem, weaponDefinition);
        var feed = new StatefulFeedRef(state.StatefulItems, feedDeviceItem);
        return ValidateReload(weapon, feed, ammunitionItemId, state.Player.Inventory.CountOf);
    }

    public FirearmValidation<LoadAmmunitionPlan> ValidateLoadStatefulWeapon(
        PrototypeGameState state,
        StatefulItemId weaponItemId,
        ItemId ammunitionItemId)
    {
        if (!state.StatefulItems.TryGet(weaponItemId, out var weaponItem)
            || !IsOwnedStatefulWeapon(weaponItem)
            || weaponItem.Weapon is null)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure("That weapon is not available.");
        }

        if (!_catalog.TryGetWeapon(weaponItem.ItemId, out var weaponDefinition))
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure("That item is not a firearm.");
        }

        if (weaponDefinition.UsesDetachableFeedDevice)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"{weaponDefinition.Name} must use a compatible feed device.");
        }

        if (!_catalog.TryGetAmmunition(ammunitionItemId, out var ammunition))
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"Unknown ammunition: {ammunitionItemId}.");
        }

        if (!weaponDefinition.AcceptsAmmunition(ammunition))
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"Cannot load {ammunition.Name} into {weaponDefinition.Name}.");
        }

        var availableQuantity = state.Player.Inventory.CountOf(ammunition.ItemId);
        if (availableQuantity < 1)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"No {ammunition.Name} available.");
        }

        var feed = new BuiltInStatefulFeedRef(weaponItem);
        if (feed.ExistingState is null)
        {
            throw new InvalidOperationException($"{weaponDefinition.Name} is missing built-in feed state.");
        }

        var loadFailures = GetLoadFailures(feed, ammunition);
        if (loadFailures.Count > 0)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure(loadFailures.ToArray());
        }

        return FirearmValidation<LoadAmmunitionPlan>.Success(new LoadAmmunitionPlan(feed, ammunition, availableQuantity));
    }

    public FirearmValidation<TestFirePlan> ValidateTestFireStatefulWeapon(
        PrototypeGameState state,
        StatefulItemId weaponItemId)
    {
        if (!state.StatefulItems.TryGet(weaponItemId, out var weaponItem)
            || !IsOwnedStatefulWeapon(weaponItem)
            || weaponItem.Weapon is null)
        {
            return FirearmValidation<TestFirePlan>.Failure("That weapon is not available.");
        }

        var activeFeed = GetActiveFeedForStatefulWeapon(state, weaponItem);
        if (activeFeed is not { LoadedCount: > 0 })
        {
            return FirearmValidation<TestFirePlan>.Failure("Weapon is empty.");
        }

        return FirearmValidation<TestFirePlan>.Success(new TestFirePlan(_items.GetItemName(weaponItem.ItemId), activeFeed));
    }

    public FirearmValidation<ShootNpcPlan> ValidateShootEquippedNpc(
        PrototypeGameState state,
        NpcId targetNpcId)
    {
        if (!state.LocalMap.Npcs.TryGet(targetNpcId, out var target))
        {
            return FirearmValidation<ShootNpcPlan>.Failure("No target selected.");
        }

        if (target.IsDisabled)
        {
            return FirearmValidation<ShootNpcPlan>.Failure($"{target.Name} is already disabled.");
        }

        var equippedFirearm = FindEquippedFirearm(state);
        if (equippedFirearm is null)
        {
            return FirearmValidation<ShootNpcPlan>.Failure("No equipped firearm.");
        }

        var distance = TileDistance(state.Player.Position, target.Position);
        if (distance > equippedFirearm.Weapon.MaximumRangeTiles)
        {
            return FirearmValidation<ShootNpcPlan>.Failure(
                $"{target.Name} is out of range for {equippedFirearm.Weapon.Name} ({distance}/{equippedFirearm.Weapon.MaximumRangeTiles} tiles)."
            );
        }

        if (equippedFirearm.ActiveFeed?.LoadedAmmunitionItemId is not { } ammunitionItemId
            || equippedFirearm.ActiveFeed.LoadedCount < 1)
        {
            return FirearmValidation<ShootNpcPlan>.Failure($"{equippedFirearm.Weapon.Name} is empty.");
        }

        if (!_catalog.TryGetAmmunition(ammunitionItemId, out var ammunition))
        {
            return FirearmValidation<ShootNpcPlan>.Failure($"Unknown ammunition: {ammunitionItemId}.");
        }

        return FirearmValidation<ShootNpcPlan>.Success(
            new ShootNpcPlan(equippedFirearm.Weapon.Name, equippedFirearm.ActiveFeed, target, ammunition)
        );
    }

    private FirearmValidation<LoadAmmunitionPlan> ValidateLoadAmmunition(
        IFirearmFeedRef feed,
        ItemId ammunitionItemId,
        Func<ItemId, int> countAmmunition)
    {
        if (!_catalog.TryGetAmmunition(ammunitionItemId, out var ammunition))
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"Unknown ammunition: {ammunitionItemId}.");
        }

        var availableQuantity = countAmmunition(ammunition.ItemId);
        if (availableQuantity < 1)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure($"No {ammunition.Name} available.");
        }

        var loadFailures = GetLoadFailures(feed, ammunition);
        if (loadFailures.Count > 0)
        {
            return FirearmValidation<LoadAmmunitionPlan>.Failure(loadFailures.ToArray());
        }

        return FirearmValidation<LoadAmmunitionPlan>.Success(new LoadAmmunitionPlan(feed, ammunition, availableQuantity));
    }

    private FirearmValidation<UnloadFeedPlan> ValidateUnloadFeed(
        IFirearmFeedRef feed,
        PlayerInventory inventory)
    {
        if (feed.ExistingState is not { } feedState)
        {
            return FirearmValidation<UnloadFeedPlan>.Failure($"Unknown feed device: {feed.DisplayName}.");
        }

        if (feedState.LoadedAmmunitionItemId is not { } loadedAmmunitionItemId || feedState.LoadedCount == 0)
        {
            return FirearmValidation<UnloadFeedPlan>.Failure($"{feedState.DisplayName} is empty.");
        }

        if (!_items.CanAddToInventory(inventory, loadedAmmunitionItemId))
        {
            return FirearmValidation<UnloadFeedPlan>.Failure("Not enough inventory grid space.");
        }

        return FirearmValidation<UnloadFeedPlan>.Success(new UnloadFeedPlan(feed));
    }

    private FirearmValidation<ReloadFeedPlan> ValidateReload(
        IFirearmWeaponRef weapon,
        IFirearmDetachableFeedRef feed,
        ItemId ammunitionItemId,
        Func<ItemId, int> countAmmunition)
    {
        if (!_catalog.TryGetAmmunition(ammunitionItemId, out var ammunition))
        {
            return FirearmValidation<ReloadFeedPlan>.Failure($"Unknown ammunition: {ammunitionItemId}.");
        }

        if (!weapon.Definition.AcceptsAmmunition(ammunition))
        {
            return FirearmValidation<ReloadFeedPlan>.Failure($"Cannot load {ammunition.Name} into {weapon.Definition.Name}.");
        }

        var availableQuantity = countAmmunition(ammunition.ItemId);
        if (availableQuantity < 1)
        {
            return FirearmValidation<ReloadFeedPlan>.Failure($"No {ammunition.Name} available.");
        }

        var loadFailures = GetLoadFailures(feed, ammunition);
        if (loadFailures.Count > 0)
        {
            return FirearmValidation<ReloadFeedPlan>.Failure(loadFailures.ToArray());
        }

        return FirearmValidation<ReloadFeedPlan>.Success(new ReloadFeedPlan(weapon, feed, ammunition, availableQuantity));
    }

    private static IReadOnlyList<string> GetLoadFailures(IFirearmFeedRef feed, AmmunitionDefinition ammunition)
    {
        var loadedCount = feed.ExistingState?.LoadedCount ?? 0;
        var loadedAmmunitionItemId = feed.ExistingState?.LoadedAmmunitionItemId;
        var loadedAmmunitionVariant = feed.ExistingState?.LoadedAmmunitionVariant;

        if (ammunition.Size != feed.AmmoSize)
        {
            return new[] { $"Cannot load {ammunition.Name} into {feed.DisplayName}." };
        }

        if (loadedCount >= feed.Capacity)
        {
            return new[] { $"{feed.DisplayName} is full." };
        }

        if (loadedAmmunitionItemId is not null && loadedAmmunitionItemId != ammunition.ItemId)
        {
            return new[] { $"{feed.DisplayName} already contains {loadedAmmunitionVariant} ammunition." };
        }

        return Array.Empty<string>();
    }

    private static bool IsOwnedStatefulWeapon(StatefulItem item)
    {
        return item.Weapon is not null
            && item.Location is PlayerInventoryLocation or EquipmentLocation;
    }

    private static bool IsAccessibleFeedDevice(PrototypeGameState state, StatefulItem item)
    {
        if (item.FeedDevice is null)
        {
            return false;
        }

        if (item.Location is PlayerInventoryLocation)
        {
            return true;
        }

        if (item.Location is not InsertedLocation inserted)
        {
            return false;
        }

        return state.StatefulItems.TryGet(inserted.ParentItemId, out var parentItem)
            && IsOwnedStatefulWeapon(parentItem);
    }

    private static FeedDeviceState? GetActiveFeedForStatefulWeapon(PrototypeGameState state, StatefulItem weaponItem)
    {
        if (weaponItem.Weapon?.BuiltInFeed is not null)
        {
            return weaponItem.Weapon.BuiltInFeed;
        }

        if (weaponItem.Weapon?.InsertedFeedDeviceItemId is not { } feedDeviceItemId)
        {
            return null;
        }

        return state.StatefulItems.TryGet(feedDeviceItemId, out var feedDeviceItem)
            ? feedDeviceItem.FeedDevice
            : null;
    }

    private EquippedFirearm? FindEquippedFirearm(PrototypeGameState state)
    {
        return FindEquippedFirearmInSlot(state, EquipmentSlotId.MainHand)
            ?? FindEquippedFirearmInSlot(state, EquipmentSlotId.OffHand);
    }

    private EquippedFirearm? FindEquippedFirearmInSlot(PrototypeGameState state, EquipmentSlotId slotId)
    {
        return FindEquippedStatefulFirearmInSlot(state, slotId)
            ?? FindEquippedStackFirearmInSlot(state, slotId);
    }

    private EquippedFirearm? FindEquippedStatefulFirearmInSlot(PrototypeGameState state, EquipmentSlotId slotId)
    {
        var weaponItem = state.StatefulItems.EquippedIn(slotId);
        if (weaponItem?.Weapon is null || !_catalog.TryGetWeapon(weaponItem.ItemId, out var weapon))
        {
            return null;
        }

        return new EquippedFirearm(weapon, GetActiveFeedForStatefulWeapon(state, weaponItem));
    }

    private EquippedFirearm? FindEquippedStackFirearmInSlot(PrototypeGameState state, EquipmentSlotId slotId)
    {
        if (!state.Player.Equipment.TryGetEquippedItem(slotId, out var equippedItem)
            || !_catalog.TryGetWeapon(equippedItem.ItemId, out var weapon))
        {
            return null;
        }

        var activeFeed = state.Player.Firearms.TryGetWeapon(weapon.ItemId, out var weaponState)
            ? state.Player.Firearms.GetActiveFeedForWeapon(weaponState)
            : null;

        return new EquippedFirearm(weapon, activeFeed);
    }

    private static int TileDistance(GridPosition from, GridPosition to)
    {
        return Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y));
    }

    private sealed record EquippedFirearm(WeaponDefinition Weapon, FeedDeviceState? ActiveFeed);
}

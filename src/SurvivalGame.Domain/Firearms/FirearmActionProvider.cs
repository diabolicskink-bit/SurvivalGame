namespace SurvivalGame.Domain;

internal sealed class FirearmActionProvider
{
    private readonly FirearmCatalog _catalog;
    private readonly FirearmItemServices _items;

    public FirearmActionProvider(FirearmCatalog catalog, FirearmItemServices items)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(items);
        _catalog = catalog;
        _items = items;
    }

    public IReadOnlyList<AvailableAction> GetAvailableActions(PrototypeGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var actions = new List<AvailableAction>();
        AddLoadFeedDeviceActions(state.Player, actions);
        AddUnloadFeedDeviceActions(state.Player, actions);
        AddInsertFeedDeviceActions(state.Player, actions);
        AddRemoveFeedDeviceActions(state.Player, actions);
        AddReloadWeaponActions(state.Player, actions);
        AddLoadWeaponActions(state.Player, actions);
        AddTestFireActions(state.Player, actions);

        return actions;
    }

    public IReadOnlyList<AvailableAction> GetAvailableStatefulActions(PrototypeGameState state, ItemCatalog itemCatalog)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(itemCatalog);

        var actions = new List<AvailableAction>();

        foreach (var feedDeviceItem in state.StatefulItems.Items.Where(item => item.FeedDevice is not null))
        {
            var feedDevice = feedDeviceItem.FeedDevice!;
            if (!IsAccessibleFeedDevice(state, feedDeviceItem))
            {
                continue;
            }

            if (!feedDevice.IsEmpty)
            {
                actions.Add(new AvailableAction(
                    GameActionKind.UnloadStatefulFeedDevice,
                    $"Unload {_items.FormatStatefulName(feedDeviceItem, itemCatalog)}",
                    new UnloadStatefulFeedDeviceActionRequest(feedDeviceItem.Id)
                ));
            }

            if (feedDeviceItem.Location is not PlayerInventoryLocation || feedDevice.IsFull)
            {
                continue;
            }

            foreach (var ammunition in _catalog.Ammunition)
            {
                if (state.Player.Inventory.CountOf(ammunition.ItemId) < 1 || !feedDevice.CanAccept(ammunition))
                {
                    continue;
                }

                actions.Add(new AvailableAction(
                    GameActionKind.LoadStatefulFeedDevice,
                    $"Load {ammunition.Name} into {_items.FormatStatefulName(feedDeviceItem, itemCatalog)}",
                    new LoadStatefulFeedDeviceActionRequest(feedDeviceItem.Id, ammunition.ItemId)
                ));
            }
        }

        foreach (var weaponItem in OwnedStatefulWeapons(state))
        {
            var weaponDefinition = _catalog.GetWeapon(weaponItem.ItemId);
            var weaponState = weaponItem.Weapon!;

            actions.Add(new AvailableAction(
                GameActionKind.TestFireStatefulWeapon,
                $"Test fire {_items.FormatStatefulName(weaponItem, itemCatalog)}",
                new TestFireStatefulWeaponActionRequest(weaponItem.Id)
            ));

            if (weaponDefinition.UsesBuiltInFeed)
            {
                var feedDevice = weaponState.BuiltInFeed;
                if (feedDevice is not null && !feedDevice.IsFull)
                {
                    foreach (var ammunition in _catalog.Ammunition)
                    {
                        if (state.Player.Inventory.CountOf(ammunition.ItemId) < 1
                            || !weaponDefinition.AcceptsAmmunition(ammunition)
                            || !feedDevice.CanAccept(ammunition))
                        {
                            continue;
                        }

                        actions.Add(new AvailableAction(
                            GameActionKind.LoadStatefulWeapon,
                            $"Load {ammunition.Name} into {_items.FormatStatefulName(weaponItem, itemCatalog)}",
                            new LoadStatefulWeaponActionRequest(weaponItem.Id, ammunition.ItemId)
                        ));
                    }
                }
            }

            if (weaponState.HasInsertedFeedDevice)
            {
                actions.Add(new AvailableAction(
                    GameActionKind.RemoveStatefulFeedDevice,
                    $"Remove feed from {_items.FormatStatefulName(weaponItem, itemCatalog)}",
                    new RemoveStatefulFeedDeviceActionRequest(weaponItem.Id)
                ));

                AddReloadStatefulWeaponActions(state, itemCatalog, actions, weaponItem, weaponDefinition);
                continue;
            }

            if (!weaponDefinition.UsesDetachableFeedDevice)
            {
                continue;
            }

            foreach (var feedDeviceItem in state.StatefulItems.InPlayerInventory().Where(item => item.FeedDevice is not null))
            {
                if (!_catalog.TryGetFeedDevice(feedDeviceItem.ItemId, out var feedDeviceDefinition)
                    || !weaponDefinition.CanUseFeedDevice(feedDeviceDefinition))
                {
                    continue;
                }

                actions.Add(new AvailableAction(
                    GameActionKind.InsertStatefulFeedDevice,
                    $"Insert {_items.FormatStatefulName(feedDeviceItem, itemCatalog)} into {_items.FormatStatefulName(weaponItem, itemCatalog)}",
                    new InsertStatefulFeedDeviceActionRequest(weaponItem.Id, feedDeviceItem.Id)
                ));
            }
        }

        return actions;
    }

    private void AddLoadFeedDeviceActions(PlayerState player, List<AvailableAction> actions)
    {
        foreach (var feedDeviceDefinition in _catalog.FeedDevices)
        {
            if (player.Inventory.CountOf(feedDeviceDefinition.ItemId) < 1)
            {
                continue;
            }

            player.Firearms.TryGetFeedDevice(feedDeviceDefinition.ItemId, out var feedDevice);
            if (feedDevice?.IsFull == true)
            {
                continue;
            }

            foreach (var ammunition in _catalog.Ammunition)
            {
                var canAccept = feedDevice?.CanAccept(ammunition)
                    ?? (ammunition.Size == feedDeviceDefinition.AmmoSize);

                if (player.Inventory.CountOf(ammunition.ItemId) < 1 || !canAccept)
                {
                    continue;
                }

                actions.Add(new AvailableAction(
                    GameActionKind.LoadFeedDevice,
                    $"Load {ammunition.Name} into {feedDeviceDefinition.Name}",
                    new LoadFeedDeviceActionRequest(feedDeviceDefinition.ItemId, ammunition.ItemId)
                ));
            }
        }
    }

    private void AddUnloadFeedDeviceActions(PlayerState player, List<AvailableAction> actions)
    {
        foreach (var feedDevice in player.Firearms.FeedDevices)
        {
            if (feedDevice.IsEmpty)
            {
                continue;
            }

            actions.Add(new AvailableAction(
                GameActionKind.UnloadFeedDevice,
                $"Unload {feedDevice.DisplayName}",
                new UnloadFeedDeviceActionRequest(feedDevice.SourceItemId)
            ));
        }
    }

    private void AddInsertFeedDeviceActions(PlayerState player, List<AvailableAction> actions)
    {
        foreach (var weapon in OwnedWeapons(player))
        {
            var hasInsertedFeed = player.Firearms.TryGetWeapon(weapon.ItemId, out var weaponState)
                && weaponState.HasInsertedFeedDevice;

            if (!weapon.UsesDetachableFeedDevice || hasInsertedFeed)
            {
                continue;
            }

            foreach (var feedDevice in _catalog.FeedDevices)
            {
                if (player.Inventory.CountOf(feedDevice.ItemId) < 1 || !weapon.CanUseFeedDevice(feedDevice))
                {
                    continue;
                }

                actions.Add(new AvailableAction(
                    GameActionKind.InsertFeedDevice,
                    $"Insert {feedDevice.Name} into {weapon.Name}",
                    new InsertFeedDeviceActionRequest(weapon.ItemId, feedDevice.ItemId)
                ));
            }
        }
    }

    private void AddRemoveFeedDeviceActions(PlayerState player, List<AvailableAction> actions)
    {
        foreach (var weapon in OwnedWeapons(player))
        {
            if (!player.Firearms.TryGetWeapon(weapon.ItemId, out var weaponState) || !weaponState.HasInsertedFeedDevice)
            {
                continue;
            }

            actions.Add(new AvailableAction(
                GameActionKind.RemoveFeedDevice,
                $"Remove feed from {weapon.Name}",
                new RemoveFeedDeviceActionRequest(weapon.ItemId)
            ));
        }
    }

    private void AddReloadWeaponActions(PlayerState player, List<AvailableAction> actions)
    {
        foreach (var weapon in OwnedWeapons(player))
        {
            if (!weapon.UsesDetachableFeedDevice
                || !player.Firearms.TryGetWeapon(weapon.ItemId, out var weaponState)
                || weaponState.InsertedFeedDeviceItemId is not { } feedDeviceItemId
                || !player.Firearms.TryGetFeedDevice(feedDeviceItemId, out var feedDevice)
                || feedDevice.IsFull)
            {
                continue;
            }

            foreach (var ammunition in _catalog.Ammunition)
            {
                if (player.Inventory.CountOf(ammunition.ItemId) < 1
                    || !weapon.AcceptsAmmunition(ammunition)
                    || !feedDevice.CanAccept(ammunition))
                {
                    continue;
                }

                actions.Add(new AvailableAction(
                    GameActionKind.ReloadWeapon,
                    $"Reload {weapon.Name} with {ammunition.Name}",
                    new ReloadWeaponActionRequest(weapon.ItemId, ammunition.ItemId)
                ));
            }
        }
    }

    private void AddLoadWeaponActions(PlayerState player, List<AvailableAction> actions)
    {
        foreach (var weapon in OwnedWeapons(player))
        {
            if (weapon.UsesDetachableFeedDevice)
            {
                continue;
            }

            player.Firearms.TryGetWeapon(weapon.ItemId, out var weaponState);
            var feedDevice = weaponState?.BuiltInFeed;
            if (feedDevice?.IsFull == true)
            {
                continue;
            }

            foreach (var ammunition in _catalog.Ammunition)
            {
                var canAccept = feedDevice?.CanAccept(ammunition)
                    ?? weapon.AcceptsAmmunition(ammunition);

                if (player.Inventory.CountOf(ammunition.ItemId) < 1
                    || !weapon.AcceptsAmmunition(ammunition)
                    || !canAccept)
                {
                    continue;
                }

                actions.Add(new AvailableAction(
                    GameActionKind.LoadWeapon,
                    $"Load {ammunition.Name} into {weapon.Name}",
                    new LoadWeaponActionRequest(weapon.ItemId, ammunition.ItemId)
                ));
            }
        }
    }

    private void AddTestFireActions(PlayerState player, List<AvailableAction> actions)
    {
        foreach (var weapon in OwnedWeapons(player))
        {
            actions.Add(new AvailableAction(
                GameActionKind.TestFire,
                $"Test fire {weapon.Name}",
                new TestFireActionRequest(weapon.ItemId)
            ));
        }
    }

    private void AddReloadStatefulWeaponActions(
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        List<AvailableAction> actions,
        StatefulItem weaponItem,
        WeaponDefinition weaponDefinition)
    {
        if (weaponItem.Weapon?.InsertedFeedDeviceItemId is not { } feedDeviceItemId)
        {
            return;
        }

        if (!state.StatefulItems.TryGet(feedDeviceItemId, out var feedDeviceItem)
            || feedDeviceItem.FeedDevice is null
            || feedDeviceItem.FeedDevice.IsFull)
        {
            return;
        }

        foreach (var ammunition in _catalog.Ammunition)
        {
            if (state.Player.Inventory.CountOf(ammunition.ItemId) < 1
                || !weaponDefinition.AcceptsAmmunition(ammunition)
                || !feedDeviceItem.FeedDevice.CanAccept(ammunition))
            {
                continue;
            }

            actions.Add(new AvailableAction(
                GameActionKind.ReloadStatefulWeapon,
                $"Reload {_items.FormatStatefulName(weaponItem, itemCatalog)} with {ammunition.Name}",
                new ReloadStatefulWeaponActionRequest(weaponItem.Id, ammunition.ItemId)
            ));
        }
    }

    private IEnumerable<WeaponDefinition> OwnedWeapons(PlayerState player)
    {
        return _catalog.Weapons.Where(weapon => PlayerOwnsItem(player, weapon.ItemId));
    }

    private static bool PlayerOwnsItem(PlayerState player, ItemId itemId)
    {
        return player.Inventory.CountOf(itemId) > 0 || player.Equipment.ContainsItem(itemId);
    }

    private IEnumerable<StatefulItem> OwnedStatefulWeapons(PrototypeGameState state)
    {
        return state.StatefulItems.Items.Where(IsOwnedStatefulWeapon);
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
}

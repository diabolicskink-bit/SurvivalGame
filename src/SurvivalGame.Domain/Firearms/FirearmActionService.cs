namespace SurvivalGame.Domain;

public sealed class FirearmActionService
{
    private readonly FirearmCatalog _catalog;

    public FirearmActionService(FirearmCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    public IReadOnlyList<AvailableAction> GetAvailableActions(PrototypeGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var actions = new List<AvailableAction>();
        AddLoadFeedDeviceActions(state.Player, actions);
        AddUnloadFeedDeviceActions(state.Player, actions);
        AddInsertFeedDeviceActions(state.Player, actions);
        AddRemoveFeedDeviceActions(state.Player, actions);
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
                    $"Unload {FormatStatefulName(feedDeviceItem, itemCatalog)}",
                    new UnloadStatefulFeedDeviceActionRequest(feedDeviceItem.Id)
                ));
            }

            if (feedDeviceItem.Location.Kind != StatefulItemLocationKind.PlayerInventory || feedDevice.IsFull)
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
                    $"Load {ammunition.Name} into {FormatStatefulName(feedDeviceItem, itemCatalog)}",
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
                $"Test fire {FormatStatefulName(weaponItem, itemCatalog)}",
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
                            $"Load {ammunition.Name} into {FormatStatefulName(weaponItem, itemCatalog)}",
                            new LoadStatefulWeaponActionRequest(weaponItem.Id, ammunition.ItemId)
                        ));
                    }
                }
            }

            if (weaponState.HasInsertedFeedDevice)
            {
                actions.Add(new AvailableAction(
                    GameActionKind.RemoveStatefulFeedDevice,
                    $"Remove feed from {FormatStatefulName(weaponItem, itemCatalog)}",
                    new RemoveStatefulFeedDeviceActionRequest(weaponItem.Id)
                ));
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
                    $"Insert {FormatStatefulName(feedDeviceItem, itemCatalog)} into {FormatStatefulName(weaponItem, itemCatalog)}",
                    new InsertStatefulFeedDeviceActionRequest(weaponItem.Id, feedDeviceItem.Id)
                ));
            }
        }

        return actions;
    }

    public GameActionResult LoadFeedDevice(PrototypeGameState state, ItemId feedDeviceItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(feedDeviceItemId);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        if (!_catalog.TryGetFeedDevice(feedDeviceItemId, out var feedDeviceDefinition))
        {
            return GameActionResult.Failure($"Unknown feed device: {feedDeviceItemId}.");
        }

        if (state.Player.Inventory.CountOf(feedDeviceItemId) < 1)
        {
            return GameActionResult.Failure($"{feedDeviceDefinition.Name} is not in your inventory.");
        }

        if (!_catalog.TryGetAmmunition(ammunitionItemId, out var ammunition))
        {
            return GameActionResult.Failure($"Unknown ammunition: {ammunitionItemId}.");
        }

        var availableQuantity = state.Player.Inventory.CountOf(ammunition.ItemId);
        if (availableQuantity < 1)
        {
            return GameActionResult.Failure($"No {ammunition.Name} available.");
        }

        state.Player.Firearms.TryGetFeedDevice(feedDeviceDefinition.ItemId, out var existingFeedDevice);
        var validationMessage = existingFeedDevice is null
            ? ValidateCanLoad(feedDeviceDefinition.AmmoSize, feedDeviceDefinition.Name, feedDeviceDefinition.Capacity, 0, null, null, ammunition)
            : ValidateCanLoad(existingFeedDevice, ammunition);
        if (validationMessage is not null)
        {
            return GameActionResult.Failure(validationMessage);
        }

        var feedDevice = existingFeedDevice ?? state.Player.Firearms.EnsureFeedDevice(feedDeviceDefinition);
        var loadedQuantity = feedDevice.Load(ammunition, availableQuantity);
        state.Player.Inventory.TryRemove(ammunition.ItemId, loadedQuantity);

        return GameActionResult.Success(
            0,
            $"Loaded {loadedQuantity} {ammunition.Name} into {feedDevice.DisplayName}."
        );
    }

    public GameActionResult UnloadFeedDevice(PrototypeGameState state, ItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(feedDeviceItemId);

        if (!state.Player.Firearms.TryGetFeedDevice(feedDeviceItemId, out var feedDevice))
        {
            return GameActionResult.Failure($"Unknown feed device: {feedDeviceItemId}.");
        }

        var unloaded = feedDevice.UnloadAll();
        if (unloaded is null)
        {
            return GameActionResult.Failure($"{feedDevice.DisplayName} is empty.");
        }

        state.Player.Inventory.Add(unloaded.ItemId, unloaded.Quantity);
        var ammunitionName = GetAmmunitionName(unloaded.ItemId);

        return GameActionResult.Success(
            0,
            $"Unloaded {unloaded.Quantity} {ammunitionName}."
        );
    }

    public GameActionResult InsertFeedDevice(PrototypeGameState state, ItemId weaponItemId, ItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItemId);
        ArgumentNullException.ThrowIfNull(feedDeviceItemId);

        if (!TryGetOwnedWeapon(state.Player, weaponItemId, out var weapon))
        {
            return GameActionResult.Failure("That weapon is not available.");
        }

        if (!_catalog.TryGetFeedDevice(feedDeviceItemId, out var feedDeviceDefinition))
        {
            return GameActionResult.Failure($"Unknown feed device: {feedDeviceItemId}.");
        }

        if (state.Player.Inventory.CountOf(feedDeviceItemId) < 1)
        {
            return GameActionResult.Failure($"{feedDeviceDefinition.Name} is not in your inventory.");
        }

        if (!weapon.CanUseFeedDevice(feedDeviceDefinition))
        {
            return GameActionResult.Failure("This magazine does not fit that weapon.");
        }

        if (state.Player.Firearms.TryGetWeapon(weapon.ItemId, out var existingWeaponState)
            && existingWeaponState.HasInsertedFeedDevice)
        {
            return GameActionResult.Failure($"{weapon.Name} already has a feed device inserted.");
        }

        state.Player.Firearms.EnsureFeedDevice(feedDeviceDefinition);
        state.Player.Inventory.TryRemove(feedDeviceItemId);
        var weaponState = existingWeaponState ?? state.Player.Firearms.EnsureWeapon(weapon);
        weaponState.InsertFeedDevice(feedDeviceItemId);

        return GameActionResult.Success(
            0,
            $"Inserted {feedDeviceDefinition.Name} into {weapon.Name}."
        );
    }

    public GameActionResult RemoveFeedDevice(PrototypeGameState state, ItemId weaponItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItemId);

        if (!TryGetOwnedWeapon(state.Player, weaponItemId, out var weapon))
        {
            return GameActionResult.Failure("That weapon is not available.");
        }

        if (!state.Player.Firearms.TryGetWeapon(weapon.ItemId, out var weaponState))
        {
            return GameActionResult.Failure($"{weapon.Name} has no feed device inserted.");
        }

        var removedFeedDeviceItemId = weaponState.RemoveFeedDevice();
        if (removedFeedDeviceItemId is null)
        {
            return GameActionResult.Failure($"{weapon.Name} has no feed device inserted.");
        }

        state.Player.Inventory.Add(removedFeedDeviceItemId);
        var feedDeviceName = _catalog.TryGetFeedDevice(removedFeedDeviceItemId, out var feedDevice)
            ? feedDevice.Name
            : removedFeedDeviceItemId.ToString();

        return GameActionResult.Success(
            0,
            $"Removed {feedDeviceName} from {weapon.Name}."
        );
    }

    public GameActionResult LoadWeapon(PrototypeGameState state, ItemId weaponItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItemId);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        if (!TryGetOwnedWeapon(state.Player, weaponItemId, out var weapon))
        {
            return GameActionResult.Failure("That weapon is not available.");
        }

        if (weapon.UsesDetachableFeedDevice)
        {
            return GameActionResult.Failure($"{weapon.Name} must use a compatible feed device.");
        }

        if (!_catalog.TryGetAmmunition(ammunitionItemId, out var ammunition))
        {
            return GameActionResult.Failure($"Unknown ammunition: {ammunitionItemId}.");
        }

        if (!weapon.AcceptsAmmunition(ammunition))
        {
            return GameActionResult.Failure($"Cannot load {ammunition.Name} into {weapon.Name}.");
        }

        var availableQuantity = state.Player.Inventory.CountOf(ammunition.ItemId);
        if (availableQuantity < 1)
        {
            return GameActionResult.Failure($"No {ammunition.Name} available.");
        }

        state.Player.Firearms.TryGetWeapon(weapon.ItemId, out var existingWeaponState);
        var existingFeedDevice = existingWeaponState?.BuiltInFeed;
        var validationMessage = existingFeedDevice is null
            ? ValidateCanLoad(weapon.AcceptedAmmoSizes[0], $"{weapon.Name} {FormatFeedKind(weapon.FeedKind)}", weapon.BuiltInCapacity, 0, null, null, ammunition)
            : ValidateCanLoad(existingFeedDevice, ammunition);
        if (validationMessage is not null)
        {
            return GameActionResult.Failure(validationMessage);
        }

        var weaponState = existingWeaponState ?? state.Player.Firearms.EnsureWeapon(weapon);
        var feedDevice = weaponState.BuiltInFeed
            ?? throw new InvalidOperationException($"{weapon.Name} is missing built-in feed state.");

        var loadedQuantity = feedDevice.Load(ammunition, availableQuantity);
        state.Player.Inventory.TryRemove(ammunition.ItemId, loadedQuantity);

        return GameActionResult.Success(
            0,
            $"Loaded {loadedQuantity} {ammunition.Name} into {feedDevice.DisplayName}."
        );
    }

    public GameActionResult TestFire(PrototypeGameState state, ItemId weaponItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItemId);

        if (!TryGetOwnedWeapon(state.Player, weaponItemId, out var weapon))
        {
            return GameActionResult.Failure("That weapon is not available.");
        }

        if (!state.Player.Firearms.TryGetWeapon(weapon.ItemId, out var weaponState))
        {
            return GameActionResult.Failure("Weapon is empty.");
        }

        var activeFeed = state.Player.Firearms.GetActiveFeedForWeapon(weaponState);
        var consumed = activeFeed?.ConsumeOne();
        if (consumed is null)
        {
            return GameActionResult.Failure("Weapon is empty.");
        }

        return GameActionResult.Success(
            0,
            $"Test fired {weapon.Name} using {GetAmmunitionName(consumed.ItemId)}."
        );
    }

    public GameActionResult LoadStatefulFeedDevice(PrototypeGameState state, StatefulItemId feedDeviceItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        var feedDeviceItem = state.StatefulItems.Get(feedDeviceItemId);
        if (feedDeviceItem.Location.Kind != StatefulItemLocationKind.PlayerInventory)
        {
            return GameActionResult.Failure("That feed device is not freely available.");
        }

        if (feedDeviceItem.FeedDevice is null)
        {
            return GameActionResult.Failure("That item cannot be loaded with ammunition.");
        }

        if (!_catalog.TryGetAmmunition(ammunitionItemId, out var ammunition))
        {
            return GameActionResult.Failure($"Unknown ammunition: {ammunitionItemId}.");
        }

        var availableQuantity = state.Player.Inventory.CountOf(ammunition.ItemId);
        if (availableQuantity < 1)
        {
            return GameActionResult.Failure($"No {ammunition.Name} available.");
        }

        var validationMessage = ValidateCanLoad(feedDeviceItem.FeedDevice, ammunition);
        if (validationMessage is not null)
        {
            return GameActionResult.Failure(validationMessage);
        }

        var loadedQuantity = feedDeviceItem.FeedDevice.Load(ammunition, availableQuantity);
        state.Player.Inventory.TryRemove(ammunition.ItemId, loadedQuantity);

        return GameActionResult.Success(
            0,
            $"Loaded {loadedQuantity} {ammunition.Name} into {feedDeviceItem.FeedDevice.DisplayName}."
        );
    }

    public GameActionResult UnloadStatefulFeedDevice(PrototypeGameState state, StatefulItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(state);

        var feedDeviceItem = state.StatefulItems.Get(feedDeviceItemId);
        if (!IsAccessibleFeedDevice(state, feedDeviceItem))
        {
            return GameActionResult.Failure("That feed device is not available.");
        }

        if (feedDeviceItem.FeedDevice is null)
        {
            return GameActionResult.Failure("That item does not hold ammunition.");
        }

        var unloaded = feedDeviceItem.FeedDevice.UnloadAll();
        if (unloaded is null)
        {
            return GameActionResult.Failure($"{feedDeviceItem.FeedDevice.DisplayName} is empty.");
        }

        state.Player.Inventory.Add(unloaded.ItemId, unloaded.Quantity);

        return GameActionResult.Success(
            0,
            $"Unloaded {unloaded.Quantity} {GetAmmunitionName(unloaded.ItemId)}."
        );
    }

    public GameActionResult InsertStatefulFeedDevice(PrototypeGameState state, StatefulItemId weaponItemId, StatefulItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(state);

        var weaponItem = state.StatefulItems.Get(weaponItemId);
        var feedDeviceItem = state.StatefulItems.Get(feedDeviceItemId);

        if (!IsOwnedStatefulWeapon(weaponItem))
        {
            return GameActionResult.Failure("That weapon is not available.");
        }

        if (feedDeviceItem.Location.Kind != StatefulItemLocationKind.PlayerInventory)
        {
            return GameActionResult.Failure("That feed device is not freely available.");
        }

        if (!_catalog.TryGetWeapon(weaponItem.ItemId, out var weaponDefinition)
            || !_catalog.TryGetFeedDevice(feedDeviceItem.ItemId, out var feedDeviceDefinition)
            || weaponItem.Weapon is null
            || feedDeviceItem.FeedDevice is null)
        {
            return GameActionResult.Failure("Those items cannot be combined that way.");
        }

        if (!weaponDefinition.CanUseFeedDevice(feedDeviceDefinition))
        {
            return GameActionResult.Failure("This magazine does not fit that weapon.");
        }

        if (weaponItem.Weapon.HasInsertedFeedDevice)
        {
            return GameActionResult.Failure($"{weaponDefinition.Name} already has a feed device inserted.");
        }

        weaponItem.Weapon.InsertFeedDevice(feedDeviceItem.Id);
        state.StatefulItems.MoveToInserted(feedDeviceItem.Id, weaponItem.Id);

        return GameActionResult.Success(
            0,
            $"Inserted {feedDeviceDefinition.Name} into {weaponDefinition.Name}."
        );
    }

    public GameActionResult RemoveStatefulFeedDevice(PrototypeGameState state, StatefulItemId weaponItemId)
    {
        ArgumentNullException.ThrowIfNull(state);

        var weaponItem = state.StatefulItems.Get(weaponItemId);
        if (!IsOwnedStatefulWeapon(weaponItem) || weaponItem.Weapon is null)
        {
            return GameActionResult.Failure("That weapon is not available.");
        }

        var removedFeedDeviceItemId = weaponItem.Weapon.RemoveFeedDevice();
        if (removedFeedDeviceItemId is null)
        {
            return GameActionResult.Failure("That weapon has no feed device inserted.");
        }

        state.StatefulItems.MoveToInventory(removedFeedDeviceItemId.Value);
        var feedDevice = state.StatefulItems.Get(removedFeedDeviceItemId.Value);

        return GameActionResult.Success(
            0,
            $"Removed {GetItemName(feedDevice.ItemId)} from {GetItemName(weaponItem.ItemId)}."
        );
    }

    public GameActionResult LoadStatefulWeapon(PrototypeGameState state, StatefulItemId weaponItemId, ItemId ammunitionItemId)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(ammunitionItemId);

        var weaponItem = state.StatefulItems.Get(weaponItemId);
        if (!IsOwnedStatefulWeapon(weaponItem) || weaponItem.Weapon is null)
        {
            return GameActionResult.Failure("That weapon is not available.");
        }

        if (!_catalog.TryGetWeapon(weaponItem.ItemId, out var weaponDefinition))
        {
            return GameActionResult.Failure("That item is not a firearm.");
        }

        if (weaponDefinition.UsesDetachableFeedDevice)
        {
            return GameActionResult.Failure($"{weaponDefinition.Name} must use a compatible feed device.");
        }

        if (!_catalog.TryGetAmmunition(ammunitionItemId, out var ammunition))
        {
            return GameActionResult.Failure($"Unknown ammunition: {ammunitionItemId}.");
        }

        if (!weaponDefinition.AcceptsAmmunition(ammunition))
        {
            return GameActionResult.Failure($"Cannot load {ammunition.Name} into {weaponDefinition.Name}.");
        }

        var availableQuantity = state.Player.Inventory.CountOf(ammunition.ItemId);
        if (availableQuantity < 1)
        {
            return GameActionResult.Failure($"No {ammunition.Name} available.");
        }

        var feedDevice = weaponItem.Weapon.BuiltInFeed
            ?? throw new InvalidOperationException($"{weaponDefinition.Name} is missing built-in feed state.");

        var validationMessage = ValidateCanLoad(feedDevice, ammunition);
        if (validationMessage is not null)
        {
            return GameActionResult.Failure(validationMessage);
        }

        var loadedQuantity = feedDevice.Load(ammunition, availableQuantity);
        state.Player.Inventory.TryRemove(ammunition.ItemId, loadedQuantity);

        return GameActionResult.Success(
            0,
            $"Loaded {loadedQuantity} {ammunition.Name} into {feedDevice.DisplayName}."
        );
    }

    public GameActionResult TestFireStatefulWeapon(PrototypeGameState state, StatefulItemId weaponItemId)
    {
        ArgumentNullException.ThrowIfNull(state);

        var weaponItem = state.StatefulItems.Get(weaponItemId);
        if (!IsOwnedStatefulWeapon(weaponItem) || weaponItem.Weapon is null)
        {
            return GameActionResult.Failure("That weapon is not available.");
        }

        var activeFeed = GetActiveFeedForStatefulWeapon(state, weaponItem);
        var consumed = activeFeed?.ConsumeOne();
        if (consumed is null)
        {
            return GameActionResult.Failure("Weapon is empty.");
        }

        return GameActionResult.Success(
            0,
            $"Test fired {GetItemName(weaponItem.ItemId)} using {GetAmmunitionName(consumed.ItemId)}."
        );
    }

    public int GetAvailableRounds(PlayerFirearmState firearmState, WeaponDefinition weapon)
    {
        ArgumentNullException.ThrowIfNull(firearmState);
        ArgumentNullException.ThrowIfNull(weapon);

        if (!firearmState.TryGetWeapon(weapon.ItemId, out var weaponState))
        {
            return 0;
        }

        return firearmState.GetActiveFeedForWeapon(weaponState)?.LoadedCount ?? 0;
    }

    public bool IsLoaded(PlayerFirearmState firearmState, WeaponDefinition weapon)
    {
        return GetAvailableRounds(firearmState, weapon) > 0;
    }

    public FeedDeviceState? GetActiveFeedForStatefulWeapon(PrototypeGameState state, StatefulItem weaponItem)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(weaponItem);

        if (weaponItem.Weapon?.BuiltInFeed is not null)
        {
            return weaponItem.Weapon.BuiltInFeed;
        }

        if (weaponItem.Weapon?.InsertedFeedDeviceItemId is null)
        {
            return null;
        }

        return state.StatefulItems.TryGet(weaponItem.Weapon.InsertedFeedDeviceItemId.Value, out var feedDeviceItem)
            ? feedDeviceItem.FeedDevice
            : null;
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

    private IEnumerable<WeaponDefinition> OwnedWeapons(PlayerState player)
    {
        return _catalog.Weapons.Where(weapon => PlayerOwnsItem(player, weapon.ItemId));
    }

    private bool TryGetOwnedWeapon(PlayerState player, ItemId weaponItemId, out WeaponDefinition weapon)
    {
        if (_catalog.TryGetWeapon(weaponItemId, out var foundWeapon) && PlayerOwnsItem(player, weaponItemId))
        {
            weapon = foundWeapon;
            return true;
        }

        weapon = null!;
        return false;
    }

    private static bool PlayerOwnsItem(PlayerState player, ItemId itemId)
    {
        return player.Inventory.CountOf(itemId) > 0 || player.Equipment.ContainsItem(itemId);
    }

    private static string? ValidateCanLoad(FeedDeviceState feedDevice, AmmunitionDefinition ammunition)
    {
        return ValidateCanLoad(
            feedDevice.AmmoSize,
            feedDevice.DisplayName,
            feedDevice.Capacity,
            feedDevice.LoadedCount,
            feedDevice.LoadedAmmunitionItemId,
            feedDevice.LoadedAmmunitionVariant,
            ammunition
        );
    }

    private static string? ValidateCanLoad(
        AmmoSizeId acceptedAmmoSize,
        string feedDisplayName,
        int capacity,
        int loadedCount,
        ItemId? loadedAmmunitionItemId,
        string? loadedAmmunitionVariant,
        AmmunitionDefinition ammunition)
    {
        if (ammunition.Size != acceptedAmmoSize)
        {
            return $"Cannot load {ammunition.Name} into {feedDisplayName}.";
        }

        if (loadedCount >= capacity)
        {
            return $"{feedDisplayName} is full.";
        }

        if (loadedAmmunitionItemId is not null && loadedAmmunitionItemId != ammunition.ItemId)
        {
            return $"{feedDisplayName} already contains {loadedAmmunitionVariant} ammunition.";
        }

        return null;
    }

    private static string FormatFeedKind(FeedDeviceKind kind)
    {
        return kind switch
        {
            FeedDeviceKind.InternalMagazine => "internal magazine",
            FeedDeviceKind.TubeMagazine => "tube magazine",
            FeedDeviceKind.Cylinder => "cylinder",
            FeedDeviceKind.Chamber => "chamber",
            FeedDeviceKind.Belt => "belt",
            _ => "feed"
        };
    }

    private string GetAmmunitionName(ItemId ammunitionItemId)
    {
        return _catalog.TryGetAmmunition(ammunitionItemId, out var ammunition)
            ? ammunition.Name
            : ammunitionItemId.ToString();
    }

    private IEnumerable<StatefulItem> OwnedStatefulWeapons(PrototypeGameState state)
    {
        return state.StatefulItems.Items.Where(IsOwnedStatefulWeapon);
    }

    private bool IsOwnedStatefulWeapon(StatefulItem item)
    {
        return item.Weapon is not null
            && (item.Location.Kind == StatefulItemLocationKind.PlayerInventory
                || item.Location.Kind == StatefulItemLocationKind.Equipment);
    }

    private bool IsAccessibleFeedDevice(PrototypeGameState state, StatefulItem item)
    {
        if (item.FeedDevice is null)
        {
            return false;
        }

        if (item.Location.Kind == StatefulItemLocationKind.PlayerInventory)
        {
            return true;
        }

        if (item.Location.Kind != StatefulItemLocationKind.Inserted || item.Location.ParentItemId is null)
        {
            return false;
        }

        return state.StatefulItems.TryGet(item.Location.ParentItemId.Value, out var parentItem)
            && IsOwnedStatefulWeapon(parentItem);
    }

    private static string FormatStatefulName(StatefulItem item, ItemCatalog itemCatalog)
    {
        var name = itemCatalog.TryGet(item.ItemId, out var definition)
            ? definition.DisplayName
            : item.ItemId.ToString();

        return $"{name} [{item.Id}]";
    }

    private string GetItemName(ItemId itemId)
    {
        if (_catalog.TryGetWeapon(itemId, out var weapon))
        {
            return weapon.Name;
        }

        if (_catalog.TryGetFeedDevice(itemId, out var feedDevice))
        {
            return feedDevice.Name;
        }

        return itemId.ToString();
    }
}

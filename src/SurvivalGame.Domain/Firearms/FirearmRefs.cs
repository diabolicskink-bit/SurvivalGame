namespace SurvivalGame.Domain;

internal interface IFirearmFeedRef
{
    string DisplayName { get; }

    AmmoSizeId AmmoSize { get; }

    int Capacity { get; }

    FeedDeviceState? ExistingState { get; }

    FeedDeviceState EnsureState();
}

internal interface IFirearmDetachableFeedRef : IFirearmFeedRef
{
    ItemId ItemId { get; }

    bool IsFreelyAvailable { get; }

    bool CanMoveToInventory();

    void MoveToInserted(IFirearmWeaponRef weapon);

    void MoveToInventory();
}

internal interface IFirearmWeaponRef
{
    WeaponDefinition Definition { get; }

    WeaponFireMode CurrentFireMode { get; }

    bool HasInsertedFeedDevice { get; }

    FeedDeviceState? ActiveFeed { get; }

    IFirearmDetachableFeedRef? InsertedFeedDevice { get; }

    FeedDeviceState EnsureBuiltInFeed();

    void InsertFeedDevice(IFirearmDetachableFeedRef feed);

    IFirearmDetachableFeedRef? RemoveFeedDevice();

    WeaponFireMode ToggleFireMode();
}

internal sealed class StackFeedRef : IFirearmDetachableFeedRef
{
    private readonly PlayerState _player;
    private readonly FeedDeviceDefinition _definition;
    private readonly FirearmItemServices _items;

    public StackFeedRef(PlayerState player, FeedDeviceDefinition definition, FirearmItemServices items)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(items);
        _player = player;
        _definition = definition;
        _items = items;
    }

    public ItemId ItemId => _definition.ItemId;

    public bool IsFreelyAvailable => _player.Inventory.CountOf(_definition.ItemId) > 0;

    public string DisplayName => ExistingState?.DisplayName ?? _definition.Name;

    public AmmoSizeId AmmoSize => ExistingState?.AmmoSize ?? _definition.AmmoSize;

    public int Capacity => ExistingState?.Capacity ?? _definition.Capacity;

    public FeedDeviceState? ExistingState =>
        _player.Firearms.TryGetFeedDevice(_definition.ItemId, out var feedDevice)
            ? feedDevice
            : null;

    public FeedDeviceState EnsureState()
    {
        return _player.Firearms.EnsureFeedDevice(_definition);
    }

    public void MoveToInserted(IFirearmWeaponRef weapon)
    {
        _player.Inventory.TryRemove(_definition.ItemId);
    }

    public bool CanMoveToInventory()
    {
        return _items.CanAddToInventory(_player.Inventory, _definition.ItemId);
    }

    public void MoveToInventory()
    {
        if (!_items.TryAddToInventory(_player.Inventory, _definition.ItemId))
        {
            throw new InvalidOperationException($"No inventory space available for '{_definition.ItemId}'.");
        }
    }
}

internal sealed class StatefulFeedRef : IFirearmDetachableFeedRef
{
    private readonly StatefulItemStore _items;
    private readonly StatefulItem _item;

    public StatefulFeedRef(StatefulItemStore items, StatefulItem item)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(item);
        _items = items;
        _item = item;
    }

    public StatefulItem Item => _item;

    public ItemId ItemId => _item.ItemId;

    public bool IsFreelyAvailable => _item.Location is PlayerInventoryLocation;

    public string DisplayName => EnsureState().DisplayName;

    public AmmoSizeId AmmoSize => EnsureState().AmmoSize;

    public int Capacity => EnsureState().Capacity;

    public FeedDeviceState? ExistingState => _item.FeedDevice;

    public FeedDeviceState EnsureState()
    {
        return _item.FeedDevice
            ?? throw new InvalidOperationException($"Stateful item '{_item.Id}' is missing feed device state.");
    }

    public void MoveToInserted(IFirearmWeaponRef weapon)
    {
        if (weapon is not StatefulWeaponRef statefulWeapon)
        {
            throw new InvalidOperationException("Stateful feed devices can only be inserted into stateful weapons.");
        }

        _items.MoveToInserted(_item.Id, statefulWeapon.Item.Id);
    }

    public bool CanMoveToInventory()
    {
        return true;
    }

    public void MoveToInventory()
    {
        _items.MoveToInventory(_item.Id);
    }
}

internal sealed class BuiltInStackFeedRef : IFirearmFeedRef
{
    private readonly PlayerState _player;
    private readonly WeaponDefinition _weapon;

    public BuiltInStackFeedRef(PlayerState player, WeaponDefinition weapon)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(weapon);
        _player = player;
        _weapon = weapon;
    }

    public string DisplayName => ExistingState?.DisplayName ?? $"{_weapon.Name} {FormatFeedKind(_weapon.FeedKind)}";

    public AmmoSizeId AmmoSize => ExistingState?.AmmoSize ?? _weapon.AcceptedAmmoSizes[0];

    public int Capacity => ExistingState?.Capacity ?? _weapon.BuiltInCapacity;

    public FeedDeviceState? ExistingState =>
        _player.Firearms.TryGetWeapon(_weapon.ItemId, out var weaponState)
            ? weaponState.BuiltInFeed
            : null;

    public FeedDeviceState EnsureState()
    {
        var weaponState = _player.Firearms.EnsureWeapon(_weapon);
        return weaponState.BuiltInFeed
            ?? throw new InvalidOperationException($"{_weapon.Name} is missing built-in feed state.");
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
}

internal sealed class BuiltInStatefulFeedRef : IFirearmFeedRef
{
    private readonly StatefulItem _weaponItem;

    public BuiltInStatefulFeedRef(StatefulItem weaponItem)
    {
        ArgumentNullException.ThrowIfNull(weaponItem);
        _weaponItem = weaponItem;
    }

    public string DisplayName => EnsureState().DisplayName;

    public AmmoSizeId AmmoSize => EnsureState().AmmoSize;

    public int Capacity => EnsureState().Capacity;

    public FeedDeviceState? ExistingState => _weaponItem.Weapon?.BuiltInFeed;

    public FeedDeviceState EnsureState()
    {
        return _weaponItem.Weapon?.BuiltInFeed
            ?? throw new InvalidOperationException($"Stateful weapon '{_weaponItem.Id}' is missing built-in feed state.");
    }
}

internal sealed class StackWeaponRef : IFirearmWeaponRef
{
    private readonly PlayerState _player;
    private readonly FirearmCatalog _catalog;
    private readonly FirearmItemServices _items;

    public StackWeaponRef(PlayerState player, FirearmCatalog catalog, FirearmItemServices items, WeaponDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(definition);
        _player = player;
        _catalog = catalog;
        _items = items;
        Definition = definition;
    }

    public WeaponDefinition Definition { get; }

    public WeaponFireMode CurrentFireMode =>
        _player.Firearms.TryGetWeapon(Definition.ItemId, out var weaponState)
            ? weaponState.CurrentFireMode
            : WeaponFireMode.SingleShot;

    public bool HasInsertedFeedDevice =>
        _player.Firearms.TryGetWeapon(Definition.ItemId, out var weaponState)
        && weaponState.HasInsertedFeedDevice;

    public FeedDeviceState? ActiveFeed =>
        _player.Firearms.TryGetWeapon(Definition.ItemId, out var weaponState)
            ? _player.Firearms.GetActiveFeedForWeapon(weaponState)
            : null;

    public IFirearmDetachableFeedRef? InsertedFeedDevice
    {
        get
        {
            if (!_player.Firearms.TryGetWeapon(Definition.ItemId, out var weaponState)
                || weaponState.InsertedFeedDeviceItemId is not { } feedDeviceItemId
                || !_catalog.TryGetFeedDevice(feedDeviceItemId, out var feedDeviceDefinition))
            {
                return null;
            }

            return new StackFeedRef(_player, feedDeviceDefinition, _items);
        }
    }

    public FeedDeviceState EnsureBuiltInFeed()
    {
        return new BuiltInStackFeedRef(_player, Definition).EnsureState();
    }

    public void InsertFeedDevice(IFirearmDetachableFeedRef feed)
    {
        if (feed is not StackFeedRef stackFeed)
        {
            throw new InvalidOperationException("Stack weapons can only accept stack feed devices.");
        }

        stackFeed.EnsureState();
        stackFeed.MoveToInserted(this);
        var weaponState = _player.Firearms.EnsureWeapon(Definition);
        weaponState.InsertFeedDevice(stackFeed.ItemId);
    }

    public IFirearmDetachableFeedRef? RemoveFeedDevice()
    {
        if (!_player.Firearms.TryGetWeapon(Definition.ItemId, out var weaponState)
            || weaponState.InsertedFeedDeviceItemId is not { } feedDeviceItemId
            || !_catalog.TryGetFeedDevice(feedDeviceItemId, out var feedDeviceDefinition))
        {
            return null;
        }

        weaponState.RemoveFeedDevice();
        var feed = new StackFeedRef(_player, feedDeviceDefinition, _items);
        feed.MoveToInventory();
        return feed;
    }

    public WeaponFireMode ToggleFireMode()
    {
        var weaponState = _player.Firearms.EnsureWeapon(Definition);
        return weaponState.ToggleFireMode(Definition);
    }
}

internal sealed class StatefulWeaponRef : IFirearmWeaponRef
{
    private readonly StatefulItemStore _items;

    public StatefulWeaponRef(StatefulItemStore items, StatefulItem item, WeaponDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(definition);
        _items = items;
        Item = item;
        Definition = definition;
    }

    public StatefulItem Item { get; }

    public WeaponDefinition Definition { get; }

    public WeaponFireMode CurrentFireMode => Item.Weapon?.CurrentFireMode ?? WeaponFireMode.SingleShot;

    public bool HasInsertedFeedDevice => Item.Weapon?.HasInsertedFeedDevice == true;

    public FeedDeviceState? ActiveFeed
    {
        get
        {
            if (Item.Weapon?.BuiltInFeed is not null)
            {
                return Item.Weapon.BuiltInFeed;
            }

            if (Item.Weapon?.InsertedFeedDeviceItemId is not { } feedDeviceItemId)
            {
                return null;
            }

            return _items.TryGet(feedDeviceItemId, out var feedDeviceItem)
                ? feedDeviceItem.FeedDevice
                : null;
        }
    }

    public IFirearmDetachableFeedRef? InsertedFeedDevice
    {
        get
        {
            if (Item.Weapon?.InsertedFeedDeviceItemId is not { } feedDeviceItemId
                || !_items.TryGet(feedDeviceItemId, out var feedDeviceItem)
                || feedDeviceItem.FeedDevice is null)
            {
                return null;
            }

            return new StatefulFeedRef(_items, feedDeviceItem);
        }
    }

    public FeedDeviceState EnsureBuiltInFeed()
    {
        return new BuiltInStatefulFeedRef(Item).EnsureState();
    }

    public void InsertFeedDevice(IFirearmDetachableFeedRef feed)
    {
        if (feed is not StatefulFeedRef statefulFeed)
        {
            throw new InvalidOperationException("Stateful weapons can only accept stateful feed devices.");
        }

        Item.Weapon?.InsertFeedDevice(statefulFeed.Item.Id);
        statefulFeed.MoveToInserted(this);
    }

    public IFirearmDetachableFeedRef? RemoveFeedDevice()
    {
        if (Item.Weapon?.RemoveFeedDevice() is not { } feedDeviceItemId
            || !_items.TryGet(feedDeviceItemId, out var feedDeviceItem)
            || feedDeviceItem.FeedDevice is null)
        {
            return null;
        }

        var feed = new StatefulFeedRef(_items, feedDeviceItem);
        feed.MoveToInventory();
        return feed;
    }

    public WeaponFireMode ToggleFireMode()
    {
        return Item.Weapon?.ToggleFireMode(Definition)
            ?? throw new InvalidOperationException($"Stateful weapon '{Item.Id}' is missing weapon state.");
    }
}

internal sealed class FirearmRefFactory
{
    private readonly FirearmCatalog _catalog;
    private readonly FirearmItemServices _items;

    public FirearmRefFactory(FirearmCatalog catalog, FirearmItemServices items)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(items);
        _catalog = catalog;
        _items = items;
    }

    public bool TryGetOwnedStackWeapon(PlayerState player, ItemId weaponItemId, out StackWeaponRef weapon)
    {
        if (_catalog.TryGetWeapon(weaponItemId, out var definition)
            && (player.Inventory.CountOf(weaponItemId) > 0 || player.Equipment.ContainsItem(weaponItemId)))
        {
            weapon = new StackWeaponRef(player, _catalog, _items, definition);
            return true;
        }

        weapon = null!;
        return false;
    }

    public bool TryGetStackFeed(PlayerState player, ItemId feedDeviceItemId, out StackFeedRef feed, out FeedDeviceDefinition definition)
    {
        if (_catalog.TryGetFeedDevice(feedDeviceItemId, out definition!))
        {
            feed = new StackFeedRef(player, definition, _items);
            return true;
        }

        feed = null!;
        return false;
    }

    public bool TryGetStatefulWeapon(StatefulItemStore items, StatefulItem item, out StatefulWeaponRef weapon)
    {
        if (item.Weapon is not null
            && item.Location is PlayerInventoryLocation or EquipmentLocation
            && _catalog.TryGetWeapon(item.ItemId, out var definition))
        {
            weapon = new StatefulWeaponRef(items, item, definition);
            return true;
        }

        weapon = null!;
        return false;
    }

    public bool TryGetStatefulFeed(StatefulItemStore items, StatefulItem item, out StatefulFeedRef feed, out FeedDeviceDefinition definition)
    {
        if (item.FeedDevice is not null && _catalog.TryGetFeedDevice(item.ItemId, out definition!))
        {
            feed = new StatefulFeedRef(items, item);
            return true;
        }

        feed = null!;
        definition = null!;
        return false;
    }
}

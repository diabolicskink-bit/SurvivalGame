namespace SurvivalGame.Domain;

public sealed class PlayerFirearmState
{
    private readonly Dictionary<ItemId, WeaponRuntimeState> _weapons = new();
    private readonly Dictionary<ItemId, FeedDeviceState> _feedDevices = new();

    public IReadOnlyCollection<WeaponRuntimeState> Weapons => _weapons.Values.ToArray();

    public IReadOnlyCollection<FeedDeviceState> FeedDevices => _feedDevices.Values.ToArray();

    public WeaponRuntimeState EnsureWeapon(WeaponDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!_weapons.TryGetValue(definition.ItemId, out var state))
        {
            state = new WeaponRuntimeState(definition);
            _weapons[definition.ItemId] = state;
        }

        return state;
    }

    public FeedDeviceState EnsureFeedDevice(FeedDeviceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!_feedDevices.TryGetValue(definition.ItemId, out var state))
        {
            state = definition.CreateState();
            _feedDevices[definition.ItemId] = state;
        }

        return state;
    }

    public bool TryGetWeapon(ItemId weaponItemId, out WeaponRuntimeState weaponState)
    {
        ArgumentNullException.ThrowIfNull(weaponItemId);
        if (_weapons.TryGetValue(weaponItemId, out var foundWeapon))
        {
            weaponState = foundWeapon;
            return true;
        }

        weaponState = null!;
        return false;
    }

    public bool TryGetFeedDevice(ItemId feedDeviceItemId, out FeedDeviceState feedDeviceState)
    {
        ArgumentNullException.ThrowIfNull(feedDeviceItemId);
        if (_feedDevices.TryGetValue(feedDeviceItemId, out var foundFeedDevice))
        {
            feedDeviceState = foundFeedDevice;
            return true;
        }

        feedDeviceState = null!;
        return false;
    }

    public bool IsFeedDeviceInserted(ItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(feedDeviceItemId);
        return _weapons.Values.Any(weapon => weapon.InsertedFeedDeviceItemId == feedDeviceItemId);
    }

    public FeedDeviceState? GetActiveFeedForWeapon(WeaponRuntimeState weaponState)
    {
        ArgumentNullException.ThrowIfNull(weaponState);

        if (weaponState.BuiltInFeed is not null)
        {
            return weaponState.BuiltInFeed;
        }

        if (weaponState.InsertedFeedDeviceItemId is null)
        {
            return null;
        }

        return _feedDevices.TryGetValue(weaponState.InsertedFeedDeviceItemId, out var feedDevice)
            ? feedDevice
            : null;
    }
}

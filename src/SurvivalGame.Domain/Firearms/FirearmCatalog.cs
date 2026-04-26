namespace SurvivalGame.Domain;

public sealed class FirearmCatalog
{
    private readonly Dictionary<ItemId, WeaponDefinition> _weapons = new();
    private readonly Dictionary<ItemId, AmmunitionDefinition> _ammunition = new();
    private readonly Dictionary<ItemId, FeedDeviceDefinition> _feedDevices = new();
    private readonly Dictionary<ItemId, WeaponModDefinition> _weaponMods = new();

    public IReadOnlyCollection<WeaponDefinition> Weapons => _weapons.Values.ToArray();

    public IReadOnlyCollection<AmmunitionDefinition> Ammunition => _ammunition.Values.ToArray();

    public IReadOnlyCollection<FeedDeviceDefinition> FeedDevices => _feedDevices.Values.ToArray();

    public IReadOnlyCollection<WeaponModDefinition> WeaponMods => _weaponMods.Values.ToArray();

    public void AddWeapon(WeaponDefinition weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);

        if (!_weapons.TryAdd(weapon.ItemId, weapon))
        {
            throw new InvalidOperationException($"Weapon '{weapon.ItemId}' is already defined.");
        }
    }

    public void AddAmmunition(AmmunitionDefinition ammunition)
    {
        ArgumentNullException.ThrowIfNull(ammunition);

        if (!_ammunition.TryAdd(ammunition.ItemId, ammunition))
        {
            throw new InvalidOperationException($"Ammunition '{ammunition.ItemId}' is already defined.");
        }
    }

    public void AddFeedDevice(FeedDeviceDefinition feedDevice)
    {
        ArgumentNullException.ThrowIfNull(feedDevice);

        if (!_feedDevices.TryAdd(feedDevice.ItemId, feedDevice))
        {
            throw new InvalidOperationException($"Feed device '{feedDevice.ItemId}' is already defined.");
        }
    }

    public void AddWeaponMod(WeaponModDefinition weaponMod)
    {
        ArgumentNullException.ThrowIfNull(weaponMod);

        if (!_weaponMods.TryAdd(weaponMod.ItemId, weaponMod))
        {
            throw new InvalidOperationException($"Weapon mod '{weaponMod.ItemId}' is already defined.");
        }
    }

    public bool TryGetWeapon(ItemId itemId, out WeaponDefinition weapon)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        if (_weapons.TryGetValue(itemId, out var foundWeapon))
        {
            weapon = foundWeapon;
            return true;
        }

        weapon = null!;
        return false;
    }

    public bool TryGetAmmunition(ItemId itemId, out AmmunitionDefinition ammunition)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        if (_ammunition.TryGetValue(itemId, out var foundAmmunition))
        {
            ammunition = foundAmmunition;
            return true;
        }

        ammunition = null!;
        return false;
    }

    public bool TryGetFeedDevice(ItemId itemId, out FeedDeviceDefinition feedDevice)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        if (_feedDevices.TryGetValue(itemId, out var foundFeedDevice))
        {
            feedDevice = foundFeedDevice;
            return true;
        }

        feedDevice = null!;
        return false;
    }

    public bool TryGetWeaponMod(ItemId itemId, out WeaponModDefinition weaponMod)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        if (_weaponMods.TryGetValue(itemId, out var foundWeaponMod))
        {
            weaponMod = foundWeaponMod;
            return true;
        }

        weaponMod = null!;
        return false;
    }

    public WeaponDefinition GetWeapon(ItemId itemId)
    {
        if (TryGetWeapon(itemId, out var weapon))
        {
            return weapon;
        }

        throw new KeyNotFoundException($"Weapon '{itemId}' is not defined.");
    }

    public AmmunitionDefinition GetAmmunition(ItemId itemId)
    {
        if (TryGetAmmunition(itemId, out var ammunition))
        {
            return ammunition;
        }

        throw new KeyNotFoundException($"Ammunition '{itemId}' is not defined.");
    }

    public FeedDeviceDefinition GetFeedDevice(ItemId itemId)
    {
        if (TryGetFeedDevice(itemId, out var feedDevice))
        {
            return feedDevice;
        }

        throw new KeyNotFoundException($"Feed device '{itemId}' is not defined.");
    }

    public WeaponModDefinition GetWeaponMod(ItemId itemId)
    {
        if (TryGetWeaponMod(itemId, out var weaponMod))
        {
            return weaponMod;
        }

        throw new KeyNotFoundException($"Weapon mod '{itemId}' is not defined.");
    }
}

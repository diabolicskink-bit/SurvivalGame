namespace SurvivalGame.Domain;

public sealed class StatefulWeaponState
{
    private readonly Dictionary<WeaponModSlotId, StatefulItemId> _installedMods = new();

    public StatefulWeaponState(ItemId weaponItemId, FeedDeviceState? builtInFeed = null)
    {
        ArgumentNullException.ThrowIfNull(weaponItemId);
        WeaponItemId = weaponItemId;
        BuiltInFeed = builtInFeed;
    }

    public ItemId WeaponItemId { get; }

    public StatefulItemId? InsertedFeedDeviceItemId { get; private set; }

    public FeedDeviceState? BuiltInFeed { get; }

    public IReadOnlyDictionary<WeaponModSlotId, StatefulItemId> InstalledMods => _installedMods;

    public bool HasInsertedFeedDevice => InsertedFeedDeviceItemId is not null;

    public bool HasInstalledMod(WeaponModSlotId slot)
    {
        ArgumentNullException.ThrowIfNull(slot);
        return _installedMods.ContainsKey(slot);
    }

    public bool TryGetInstalledMod(WeaponModSlotId slot, out StatefulItemId modItemId)
    {
        ArgumentNullException.ThrowIfNull(slot);
        return _installedMods.TryGetValue(slot, out modItemId);
    }

    public void InstallMod(WeaponModSlotId slot, StatefulItemId modItemId)
    {
        ArgumentNullException.ThrowIfNull(slot);

        if (!_installedMods.TryAdd(slot, modItemId))
        {
            throw new InvalidOperationException($"Weapon already has a mod installed in slot '{slot}'.");
        }
    }

    public StatefulItemId? RemoveMod(WeaponModSlotId slot)
    {
        ArgumentNullException.ThrowIfNull(slot);

        if (!_installedMods.Remove(slot, out var modItemId))
        {
            return null;
        }

        return modItemId;
    }

    public void InsertFeedDevice(StatefulItemId feedDeviceItemId)
    {
        if (InsertedFeedDeviceItemId is not null)
        {
            throw new InvalidOperationException("Weapon already has a feed device inserted.");
        }

        InsertedFeedDeviceItemId = feedDeviceItemId;
    }

    public StatefulItemId? RemoveFeedDevice()
    {
        var removed = InsertedFeedDeviceItemId;
        InsertedFeedDeviceItemId = null;
        return removed;
    }
}

namespace SurvivalGame.Domain;

public sealed class StatefulWeaponState
{
    public StatefulWeaponState(ItemId weaponItemId, FeedDeviceState? builtInFeed = null)
    {
        ArgumentNullException.ThrowIfNull(weaponItemId);
        WeaponItemId = weaponItemId;
        BuiltInFeed = builtInFeed;
    }

    public ItemId WeaponItemId { get; }

    public StatefulItemId? InsertedFeedDeviceItemId { get; private set; }

    public FeedDeviceState? BuiltInFeed { get; }

    public bool HasInsertedFeedDevice => InsertedFeedDeviceItemId is not null;

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

namespace SurvivalGame.Domain;

public sealed class WeaponRuntimeState
{
    public WeaponRuntimeState(WeaponDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        WeaponItemId = definition.ItemId;
        BuiltInFeed = definition.UsesBuiltInFeed
            ? definition.CreateBuiltInFeedState()
            : null;
    }

    public ItemId WeaponItemId { get; }

    public ItemId? InsertedFeedDeviceItemId { get; private set; }

    public FeedDeviceState? BuiltInFeed { get; }

    public bool HasInsertedFeedDevice => InsertedFeedDeviceItemId is not null;

    public void InsertFeedDevice(ItemId feedDeviceItemId)
    {
        ArgumentNullException.ThrowIfNull(feedDeviceItemId);

        if (InsertedFeedDeviceItemId is not null)
        {
            throw new InvalidOperationException("Weapon already has a feed device inserted.");
        }

        InsertedFeedDeviceItemId = feedDeviceItemId;
    }

    public ItemId? RemoveFeedDevice()
    {
        var removed = InsertedFeedDeviceItemId;
        InsertedFeedDeviceItemId = null;
        return removed;
    }
}

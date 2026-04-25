namespace SurvivalGame.Domain;

public sealed class StatefulItem
{
    private readonly List<StatefulItemId> _contents = new();

    public StatefulItem(
        StatefulItemId id,
        ItemId itemId,
        int quantity,
        StatefulItemLocation location,
        ItemCondition condition = ItemCondition.Good)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(location);

        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Stateful item quantity must be at least 1.");
        }

        Id = id;
        ItemId = itemId;
        Quantity = quantity;
        Location = location;
        Condition = condition;
    }

    public StatefulItemId Id { get; }

    public ItemId ItemId { get; }

    public int Quantity { get; private set; }

    public StatefulItemLocation Location { get; private set; }

    public ItemCondition Condition { get; private set; }

    public FeedDeviceState? FeedDevice { get; private set; }

    public StatefulWeaponState? Weapon { get; private set; }

    public IReadOnlyList<StatefulItemId> Contents => _contents.ToArray();

    public bool HasState => FeedDevice is not null
        || Weapon is not null
        || _contents.Count > 0
        || Condition != ItemCondition.Good
        || Quantity != 1;

    public void MoveTo(StatefulItemLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        Location = location;
    }

    public void SetCondition(ItemCondition condition)
    {
        Condition = condition;
    }

    public void AttachFeedDeviceState(FeedDeviceState feedDevice)
    {
        ArgumentNullException.ThrowIfNull(feedDevice);
        FeedDevice = feedDevice;
    }

    public void AttachWeaponState(StatefulWeaponState weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        Weapon = weapon;
    }

    public void AddContent(StatefulItemId itemId)
    {
        if (!_contents.Contains(itemId))
        {
            _contents.Add(itemId);
        }
    }

    public void RemoveContent(StatefulItemId itemId)
    {
        _contents.Remove(itemId);
    }
}

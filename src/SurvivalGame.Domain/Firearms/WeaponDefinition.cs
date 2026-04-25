namespace SurvivalGame.Domain;

public sealed record WeaponDefinition
{
    public WeaponDefinition(
        ItemId itemId,
        string name,
        string weaponFamily,
        IEnumerable<AmmoSizeId> acceptedAmmoSizes,
        FeedDeviceKind feedKind,
        int builtInCapacity = 0,
        IEnumerable<ItemId>? compatibleFeedDeviceIds = null
    )
    {
        ArgumentNullException.ThrowIfNull(itemId);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Weapon name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(weaponFamily))
        {
            throw new ArgumentException("Weapon family cannot be empty.", nameof(weaponFamily));
        }

        var ammoSizes = (acceptedAmmoSizes ?? throw new ArgumentNullException(nameof(acceptedAmmoSizes)))
            .Distinct()
            .ToArray();

        if (ammoSizes.Length == 0)
        {
            throw new ArgumentException("Weapon must accept at least one ammunition size.", nameof(acceptedAmmoSizes));
        }

        if (builtInCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(builtInCapacity), "Built-in capacity cannot be negative.");
        }

        if (feedKind is not (FeedDeviceKind.DetachableMagazine or FeedDeviceKind.Belt) && builtInCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(builtInCapacity), "Built-in feed weapons must have capacity.");
        }

        ItemId = itemId;
        Name = name.Trim();
        WeaponFamily = weaponFamily.Trim();
        AcceptedAmmoSizes = ammoSizes;
        FeedKind = feedKind;
        BuiltInCapacity = builtInCapacity;
        CompatibleFeedDeviceIds = (compatibleFeedDeviceIds ?? Array.Empty<ItemId>()).Distinct().ToArray();
    }

    public ItemId ItemId { get; }

    public string Name { get; }

    public string WeaponFamily { get; }

    public IReadOnlyList<AmmoSizeId> AcceptedAmmoSizes { get; }

    public FeedDeviceKind FeedKind { get; }

    public int BuiltInCapacity { get; }

    public IReadOnlyList<ItemId> CompatibleFeedDeviceIds { get; }

    public bool UsesDetachableFeedDevice => FeedKind is FeedDeviceKind.DetachableMagazine or FeedDeviceKind.Belt;

    public bool UsesBuiltInFeed => !UsesDetachableFeedDevice;

    public bool AcceptsAmmunition(AmmunitionDefinition ammunition)
    {
        ArgumentNullException.ThrowIfNull(ammunition);
        return AcceptsAmmoSize(ammunition.Size);
    }

    public bool AcceptsAmmoSize(AmmoSizeId size)
    {
        ArgumentNullException.ThrowIfNull(size);
        return AcceptedAmmoSizes.Contains(size);
    }

    public bool CanUseFeedDevice(FeedDeviceDefinition feedDevice)
    {
        ArgumentNullException.ThrowIfNull(feedDevice);

        if (!UsesDetachableFeedDevice || !feedDevice.IsCompatibleWith(this))
        {
            return false;
        }

        return CompatibleFeedDeviceIds.Count == 0 || CompatibleFeedDeviceIds.Contains(feedDevice.ItemId);
    }

    public FeedDeviceState CreateBuiltInFeedState()
    {
        if (!UsesBuiltInFeed)
        {
            throw new InvalidOperationException($"{Name} does not use a built-in feed.");
        }

        return new FeedDeviceState(
            ItemId,
            $"{Name} {FormatFeedKind(FeedKind)}",
            FeedKind,
            AcceptedAmmoSizes[0],
            BuiltInCapacity
        );
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

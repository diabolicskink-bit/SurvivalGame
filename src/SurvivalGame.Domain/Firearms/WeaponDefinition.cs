namespace SurvivalGame.Domain;

public sealed record WeaponDefinition
{
    public const int DefaultBurstRoundCount = 3;
    public const int DefaultBurstDamageMultiplier = 2;

    public WeaponDefinition(
        ItemId itemId,
        string name,
        string weaponFamily,
        IEnumerable<AmmoSizeId> acceptedAmmoSizes,
        FeedDeviceKind feedKind,
        int builtInCapacity = 0,
        int effectiveRangeTiles = 1,
        int maximumRangeTiles = 1,
        IEnumerable<ItemId>? compatibleFeedDeviceIds = null,
        IEnumerable<WeaponFireMode>? supportedFireModes = null,
        int burstRoundCount = DefaultBurstRoundCount,
        int burstDamageMultiplier = DefaultBurstDamageMultiplier
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

        if (effectiveRangeTiles < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveRangeTiles), "Effective range must be at least 1 tile.");
        }

        if (maximumRangeTiles < effectiveRangeTiles)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumRangeTiles), "Maximum range must be at least the effective range.");
        }

        var fireModes = (supportedFireModes ?? new[] { WeaponFireMode.SingleShot })
            .Distinct()
            .ToArray();

        if (fireModes.Length == 0)
        {
            throw new ArgumentException("Weapon must support at least one fire mode.", nameof(supportedFireModes));
        }

        if (!fireModes.Contains(WeaponFireMode.SingleShot))
        {
            throw new ArgumentException("Weapons must support single-shot mode.", nameof(supportedFireModes));
        }

        if (burstRoundCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(burstRoundCount), "Burst round count must be at least 1.");
        }

        if (burstDamageMultiplier < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(burstDamageMultiplier), "Burst damage multiplier must be at least 1.");
        }

        ItemId = itemId;
        Name = name.Trim();
        WeaponFamily = weaponFamily.Trim();
        AcceptedAmmoSizes = ammoSizes;
        FeedKind = feedKind;
        BuiltInCapacity = builtInCapacity;
        EffectiveRangeTiles = effectiveRangeTiles;
        MaximumRangeTiles = maximumRangeTiles;
        CompatibleFeedDeviceIds = (compatibleFeedDeviceIds ?? Array.Empty<ItemId>()).Distinct().ToArray();
        SupportedFireModes = fireModes;
        BurstRoundCount = burstRoundCount;
        BurstDamageMultiplier = burstDamageMultiplier;
    }

    public ItemId ItemId { get; }

    public string Name { get; }

    public string WeaponFamily { get; }

    public IReadOnlyList<AmmoSizeId> AcceptedAmmoSizes { get; }

    public FeedDeviceKind FeedKind { get; }

    public int BuiltInCapacity { get; }

    public int EffectiveRangeTiles { get; }

    public int MaximumRangeTiles { get; }

    public IReadOnlyList<ItemId> CompatibleFeedDeviceIds { get; }

    public IReadOnlyList<WeaponFireMode> SupportedFireModes { get; }

    public int BurstRoundCount { get; }

    public int BurstDamageMultiplier { get; }

    public bool UsesDetachableFeedDevice => FeedKind is FeedDeviceKind.DetachableMagazine or FeedDeviceKind.Belt;

    public bool UsesBuiltInFeed => !UsesDetachableFeedDevice;

    public bool HasMultipleFireModes => SupportedFireModes.Count > 1;

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

    public bool SupportsFireMode(WeaponFireMode mode)
    {
        return SupportedFireModes.Contains(mode);
    }

    public WeaponFireMode GetNextFireMode(WeaponFireMode currentMode)
    {
        var currentIndex = Array.IndexOf(SupportedFireModes.ToArray(), currentMode);
        if (currentIndex < 0)
        {
            return WeaponFireMode.SingleShot;
        }

        return SupportedFireModes[(currentIndex + 1) % SupportedFireModes.Count];
    }

    public int GetRoundCount(WeaponFireMode mode)
    {
        return mode == WeaponFireMode.Burst ? BurstRoundCount : 1;
    }

    public int GetDamageMultiplier(WeaponFireMode mode)
    {
        return mode == WeaponFireMode.Burst ? BurstDamageMultiplier : 1;
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

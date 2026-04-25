namespace SurvivalGame.Domain;

public sealed class FeedDeviceState
{
    public FeedDeviceState(
        ItemId sourceItemId,
        string displayName,
        FeedDeviceKind kind,
        AmmoSizeId ammoSize,
        int capacity
    )
    {
        ArgumentNullException.ThrowIfNull(sourceItemId);
        ArgumentNullException.ThrowIfNull(ammoSize);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Feed device display name cannot be empty.", nameof(displayName));
        }

        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Feed device capacity must be at least 1.");
        }

        SourceItemId = sourceItemId;
        DisplayName = displayName.Trim();
        Kind = kind;
        AmmoSize = ammoSize;
        Capacity = capacity;
    }

    public ItemId SourceItemId { get; }

    public string DisplayName { get; }

    public FeedDeviceKind Kind { get; }

    public AmmoSizeId AmmoSize { get; }

    public int Capacity { get; }

    public ItemId? LoadedAmmunitionItemId { get; private set; }

    public string? LoadedAmmunitionVariant { get; private set; }

    public int LoadedCount { get; private set; }

    public bool IsEmpty => LoadedCount == 0;

    public bool IsFull => LoadedCount >= Capacity;

    public bool CanAccept(AmmunitionDefinition ammunition)
    {
        ArgumentNullException.ThrowIfNull(ammunition);

        if (ammunition.Size != AmmoSize || IsFull)
        {
            return false;
        }

        return LoadedAmmunitionItemId is null || LoadedAmmunitionItemId == ammunition.ItemId;
    }

    public int Load(AmmunitionDefinition ammunition, int availableQuantity)
    {
        ArgumentNullException.ThrowIfNull(ammunition);

        if (availableQuantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(availableQuantity), "Available quantity must be at least 1.");
        }

        if (ammunition.Size != AmmoSize)
        {
            throw new InvalidOperationException($"Cannot load {ammunition.Name} into {DisplayName}.");
        }

        if (LoadedAmmunitionItemId is not null && LoadedAmmunitionItemId != ammunition.ItemId)
        {
            throw new InvalidOperationException($"{DisplayName} already contains {LoadedAmmunitionVariant} ammunition.");
        }

        var loadedQuantity = Math.Min(availableQuantity, Capacity - LoadedCount);
        if (loadedQuantity < 1)
        {
            throw new InvalidOperationException($"{DisplayName} is full.");
        }

        LoadedAmmunitionItemId = ammunition.ItemId;
        LoadedAmmunitionVariant = ammunition.Variant;
        LoadedCount += loadedQuantity;

        return loadedQuantity;
    }

    public LoadedAmmunition? UnloadAll()
    {
        if (LoadedAmmunitionItemId is null || LoadedAmmunitionVariant is null || LoadedCount == 0)
        {
            return null;
        }

        var unloaded = new LoadedAmmunition(
            LoadedAmmunitionItemId,
            AmmoSize,
            LoadedAmmunitionVariant,
            LoadedCount
        );

        LoadedAmmunitionItemId = null;
        LoadedAmmunitionVariant = null;
        LoadedCount = 0;

        return unloaded;
    }

    public LoadedAmmunition? ConsumeOne()
    {
        if (LoadedAmmunitionItemId is null || LoadedAmmunitionVariant is null || LoadedCount == 0)
        {
            return null;
        }

        var consumed = new LoadedAmmunition(
            LoadedAmmunitionItemId,
            AmmoSize,
            LoadedAmmunitionVariant,
            1
        );

        LoadedCount--;
        if (LoadedCount == 0)
        {
            LoadedAmmunitionItemId = null;
            LoadedAmmunitionVariant = null;
        }

        return consumed;
    }
}

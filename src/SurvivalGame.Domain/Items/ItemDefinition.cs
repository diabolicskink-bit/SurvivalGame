namespace SurvivalGame.Domain;

public sealed record ItemDefinition
{
    public ItemDefinition(
        ItemId id,
        string name,
        string description,
        string category,
        IEnumerable<string>? tags = null,
        int maxStackSize = 1,
        float weight = 0f,
        string? iconId = null,
        string? spriteId = null,
        IEnumerable<string>? actions = null,
        InventoryItemSize? inventorySize = null,
        FuelContainerDefinition? fuelContainer = null
    )
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Item name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Item category cannot be empty.", nameof(category));
        }

        if (maxStackSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxStackSize), "Max stack size must be at least 1.");
        }

        if (weight < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight cannot be negative.");
        }

        Id = id;
        Name = name.Trim();
        Description = description.Trim();
        Category = category.Trim();
        Tags = NormalizeList(tags);
        MaxStackSize = maxStackSize;
        Weight = weight;
        IconId = NormalizeOptional(iconId);
        SpriteId = NormalizeOptional(spriteId);
        Actions = NormalizeList(actions);
        InventorySize = inventorySize ?? InventoryItemSize.Default;
        FuelContainer = fuelContainer;
        TypePath = BuildTypePath(Category, Tags);
    }

    public ItemId Id { get; }

    public string Name { get; }

    public string Description { get; }

    public string Category { get; }

    public IReadOnlyList<string> Tags { get; }

    public int MaxStackSize { get; }

    public float Weight { get; }

    public string? IconId { get; }

    public string? SpriteId { get; }

    public IReadOnlyList<string> Actions { get; }

    public InventoryItemSize InventorySize { get; }

    public FuelContainerDefinition? FuelContainer { get; }

    public ItemTypePath TypePath { get; }

    public string DisplayName => Name;

    public bool HasTag(string tag)
    {
        return Tags.Any(existingTag => string.Equals(existingTag, tag, StringComparison.OrdinalIgnoreCase));
    }

    public bool AllowsAction(string action)
    {
        return Actions.Any(existingAction => string.Equals(existingAction, action, StringComparison.OrdinalIgnoreCase));
    }

    private static ItemTypePath BuildTypePath(string category, IReadOnlyList<string> tags)
    {
        var typeSegments = new List<string> { category };
        typeSegments.AddRange(tags.Where(tag => !string.Equals(tag, category, StringComparison.OrdinalIgnoreCase)));
        return new ItemTypePath(typeSegments.ToArray());
    }

    private static IReadOnlyList<string> NormalizeList(IEnumerable<string>? values)
    {
        return (values ?? Array.Empty<string>())
            .Select(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Item definition lists cannot contain empty values.");
                }

                return value.Trim();
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

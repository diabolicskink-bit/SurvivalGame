namespace SurvivalGame.Domain;

public sealed record WorldObjectDefinition
{
    public WorldObjectDefinition(
        WorldObjectId id,
        string name,
        string description,
        string category,
        IEnumerable<string>? tags = null,
        bool blocksMovement = false,
        bool blocksSight = false,
        string? mapColor = null,
        string? spriteId = null
    )
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("World object name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("World object category cannot be empty.", nameof(category));
        }

        Id = id;
        Name = name.Trim();
        Description = description.Trim();
        Category = category.Trim();
        Tags = NormalizeList(tags);
        BlocksMovement = blocksMovement;
        BlocksSight = blocksSight;
        MapColor = string.IsNullOrWhiteSpace(mapColor) ? "#6c756a" : mapColor.Trim();
        SpriteId = string.IsNullOrWhiteSpace(spriteId) ? null : spriteId.Trim();
    }

    public WorldObjectId Id { get; }

    public string Name { get; }

    public string Description { get; }

    public string Category { get; }

    public IReadOnlyList<string> Tags { get; }

    public bool BlocksMovement { get; }

    public bool BlocksSight { get; }

    public string MapColor { get; }

    public string? SpriteId { get; }

    public bool HasTag(string tag)
    {
        return Tags.Any(existingTag => string.Equals(existingTag, tag, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> NormalizeList(IEnumerable<string>? values)
    {
        return (values ?? Array.Empty<string>())
            .Select(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("World object definition lists cannot contain empty values.");
                }

                return value.Trim();
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

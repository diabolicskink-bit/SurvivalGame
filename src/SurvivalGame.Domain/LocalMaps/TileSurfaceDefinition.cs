namespace SurvivalGame.Domain;

public sealed record TileSurfaceDefinition
{
    public TileSurfaceDefinition(
        SurfaceId id,
        string name,
        string description,
        string category,
        IEnumerable<string>? tags = null,
        int movementCost = 1,
        string? mapColor = null,
        string? spriteId = null
    )
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Surface name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Surface category cannot be empty.", nameof(category));
        }

        if (movementCost < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(movementCost), "Movement cost must be at least 1.");
        }

        Id = id;
        Name = name.Trim();
        Description = description.Trim();
        Category = category.Trim();
        Tags = NormalizeList(tags);
        MovementCost = movementCost;
        MapColor = string.IsNullOrWhiteSpace(mapColor) ? "#303834" : mapColor.Trim();
        SpriteId = NormalizeOptional(spriteId);
    }

    public SurfaceId Id { get; }

    public string Name { get; }

    public string Description { get; }

    public string Category { get; }

    public IReadOnlyList<string> Tags { get; }

    public int MovementCost { get; }

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
                    throw new ArgumentException("Surface definition lists cannot contain empty values.");
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

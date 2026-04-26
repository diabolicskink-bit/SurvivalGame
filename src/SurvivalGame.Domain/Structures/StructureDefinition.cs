namespace SurvivalGame.Domain;

public sealed record StructureDefinition
{
    public StructureDefinition(
        StructureId id,
        string name,
        string description,
        string category,
        string styleId,
        string pieceKind,
        IEnumerable<string>? tags = null,
        bool blocksMovement = false,
        bool blocksSight = false,
        bool connectsAsWall = true,
        string? mapColor = null)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Structure name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Structure category cannot be empty.", nameof(category));
        }

        if (string.IsNullOrWhiteSpace(styleId))
        {
            throw new ArgumentException("Structure style id cannot be empty.", nameof(styleId));
        }

        if (string.IsNullOrWhiteSpace(pieceKind))
        {
            throw new ArgumentException("Structure piece kind cannot be empty.", nameof(pieceKind));
        }

        Id = id;
        Name = name.Trim();
        Description = description.Trim();
        Category = category.Trim();
        StyleId = styleId.Trim();
        PieceKind = pieceKind.Trim();
        Tags = NormalizeList(tags);
        BlocksMovement = blocksMovement;
        BlocksSight = blocksSight;
        ConnectsAsWall = connectsAsWall;
        MapColor = string.IsNullOrWhiteSpace(mapColor) ? "#6f756f" : mapColor.Trim();
    }

    public StructureId Id { get; }

    public string Name { get; }

    public string Description { get; }

    public string Category { get; }

    public string StyleId { get; }

    public string PieceKind { get; }

    public IReadOnlyList<string> Tags { get; }

    public bool BlocksMovement { get; }

    public bool BlocksSight { get; }

    public bool ConnectsAsWall { get; }

    public string MapColor { get; }

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
                    throw new ArgumentException("Structure definition lists cannot contain empty values.");
                }

                return value.Trim();
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

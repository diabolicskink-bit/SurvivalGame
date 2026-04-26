namespace SurvivalGame.Domain;

public sealed record NpcDefinition
{
    public NpcDefinition(
        NpcDefinitionId id,
        string displayName,
        string description,
        string species,
        int maximumHealth,
        IEnumerable<string>? tags = null,
        bool blocksMovement = true,
        string? mapColor = null,
        string? spriteId = null,
        NpcBehaviorProfile? behavior = null,
        SpriteRenderProfile? spriteRender = null
    )
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("NPC display name cannot be empty.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(species))
        {
            throw new ArgumentException("NPC species cannot be empty.", nameof(species));
        }

        if (maximumHealth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumHealth), "NPC maximum health must be at least 1.");
        }

        Id = id;
        DisplayName = displayName.Trim();
        Description = description.Trim();
        Species = species.Trim();
        MaximumHealth = maximumHealth;
        Tags = NormalizeTags(tags);
        BlocksMovement = blocksMovement;
        MapColor = string.IsNullOrWhiteSpace(mapColor) ? "#c75a3b" : mapColor.Trim();
        SpriteId = string.IsNullOrWhiteSpace(spriteId) ? null : spriteId.Trim();
        Behavior = behavior ?? NpcBehaviorProfile.Inert;
        SpriteRender = spriteRender;
    }

    public NpcDefinitionId Id { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public string Species { get; }

    public int MaximumHealth { get; }

    public IReadOnlyList<string> Tags { get; }

    public bool BlocksMovement { get; }

    public string MapColor { get; }

    public string? SpriteId { get; }

    public NpcBehaviorProfile Behavior { get; }

    public SpriteRenderProfile? SpriteRender { get; }

    public NpcState CreateState(NpcId instanceId, GridPosition position)
    {
        return new NpcState(
            instanceId,
            Id,
            DisplayName,
            position,
            currentHealth: MaximumHealth,
            maximumHealth: MaximumHealth,
            blocksMovement: BlocksMovement
        );
    }

    public bool HasTag(string tag)
    {
        return Tags.Any(existingTag => string.Equals(existingTag, tag, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> NormalizeTags(IEnumerable<string>? tags)
    {
        return (tags ?? Array.Empty<string>())
            .Select(tag =>
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    throw new ArgumentException("NPC tags cannot contain empty values.");
                }

                return tag.Trim();
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

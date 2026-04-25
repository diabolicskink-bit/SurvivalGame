namespace SurvivalGame.Domain;

public sealed record NpcBehaviorProfile
{
    public NpcBehaviorProfile(
        NpcBehaviorKind kind,
        int perceptionRange = 0,
        IEnumerable<string>? tags = null
    )
    {
        if (perceptionRange < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(perceptionRange), "NPC perception range cannot be negative.");
        }

        Kind = kind;
        PerceptionRange = perceptionRange;
        Tags = NormalizeTags(tags);
    }

    public NpcBehaviorKind Kind { get; }

    public int PerceptionRange { get; }

    public IReadOnlyList<string> Tags { get; }

    public static NpcBehaviorProfile Inert { get; } = new(NpcBehaviorKind.Inert);

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
                    throw new ArgumentException("NPC behavior tags cannot contain empty values.");
                }

                return tag.Trim();
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

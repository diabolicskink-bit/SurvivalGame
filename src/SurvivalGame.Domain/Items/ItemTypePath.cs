namespace SurvivalGame.Domain;

public sealed record ItemTypePath
{
    private readonly string[] _segments;

    public ItemTypePath(params string[] segments)
    {
        if (segments.Length == 0)
        {
            throw new ArgumentException("Item type path must contain at least one segment.", nameof(segments));
        }

        _segments = segments
            .Select(ValidateSegment)
            .ToArray();
    }

    public IReadOnlyList<string> Segments => _segments;

    public string LeafName => _segments[^1];

    public bool IsA(ItemTypePath other)
    {
        if (other._segments.Length > _segments.Length)
        {
            return false;
        }

        for (var i = 0; i < other._segments.Length; i++)
        {
            if (!string.Equals(_segments[i], other._segments[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    public override string ToString()
    {
        return string.Join(" -> ", _segments);
    }

    private static string ValidateSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            throw new ArgumentException("Item type path segments cannot be empty.", nameof(segment));
        }

        return segment.Trim();
    }
}

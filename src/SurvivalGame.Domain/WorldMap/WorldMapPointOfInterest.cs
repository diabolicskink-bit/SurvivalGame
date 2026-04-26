namespace SurvivalGame.Domain;

public sealed record WorldMapPointOfInterest
{
    public WorldMapPointOfInterest(
        string id,
        string displayName,
        WorldMapPosition position,
        double enterRadius,
        WorldMapPointCategory category = WorldMapPointCategory.LocalSite,
        int labelPriority = 3,
        string? localSiteId = null
    )
    {
        if (enterRadius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(enterRadius), "Enter radius must be positive.");
        }

        Id = ValidateRequired(id, nameof(id));
        DisplayName = ValidateRequired(displayName, nameof(displayName));
        Position = position;
        EnterRadius = enterRadius;
        Category = category;
        LabelPriority = labelPriority;
        LocalSiteId = string.IsNullOrWhiteSpace(localSiteId) ? null : localSiteId.Trim();
    }

    public string Id { get; }

    public string DisplayName { get; }

    public WorldMapPosition Position { get; }

    public double EnterRadius { get; }

    public WorldMapPointCategory Category { get; }

    public int LabelPriority { get; }

    public string? LocalSiteId { get; }

    public bool IsNear(WorldMapPosition position)
    {
        return Position.DistanceTo(position) <= EnterRadius;
    }

    private static string ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value;
    }
}

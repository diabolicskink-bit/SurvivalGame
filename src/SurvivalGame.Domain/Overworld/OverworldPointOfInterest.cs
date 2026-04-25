namespace SurvivalGame.Domain;

public sealed record OverworldPointOfInterest
{
    public OverworldPointOfInterest(
        string id,
        string displayName,
        OverworldPosition position,
        double enterRadius
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
    }

    public string Id { get; }

    public string DisplayName { get; }

    public OverworldPosition Position { get; }

    public double EnterRadius { get; }

    public bool IsNear(OverworldPosition position)
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

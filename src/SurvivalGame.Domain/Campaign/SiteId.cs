namespace SurvivalGame.Domain;

public sealed record SiteId
{
    public static readonly SiteId Default = new("prototype_local");

    public SiteId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Site id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}

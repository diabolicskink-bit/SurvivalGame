namespace SurvivalGame.Domain;

public sealed record StructureId
{
    public static readonly StructureId Empty = new("empty");

    public StructureId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Structure id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}

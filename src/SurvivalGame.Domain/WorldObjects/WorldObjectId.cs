namespace SurvivalGame.Domain;

public sealed record WorldObjectId
{
    public WorldObjectId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("World object id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}

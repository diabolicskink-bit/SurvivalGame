namespace SurvivalGame.Domain;

public sealed record AmmoSizeId
{
    public AmmoSizeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Ammunition size id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}

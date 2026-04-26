namespace SurvivalGame.Domain;

public sealed record WeaponModSlotId
{
    public WeaponModSlotId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Weapon mod slot id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}

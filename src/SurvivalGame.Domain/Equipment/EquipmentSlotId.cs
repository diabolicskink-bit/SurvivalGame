namespace SurvivalGame.Domain;

public sealed record EquipmentSlotId
{
    public static readonly EquipmentSlotId MainHand = new("MainHand");
    public static readonly EquipmentSlotId OffHand = new("OffHand");
    public static readonly EquipmentSlotId Head = new("Head");
    public static readonly EquipmentSlotId Body = new("Body");
    public static readonly EquipmentSlotId Legs = new("Legs");
    public static readonly EquipmentSlotId Feet = new("Feet");
    public static readonly EquipmentSlotId Back = new("Back");

    public EquipmentSlotId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Equipment slot id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}

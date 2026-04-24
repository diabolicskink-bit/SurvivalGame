namespace SurvivalGame.Domain;

public sealed class EquipmentValidator
{
    public bool CanOccupy(EquipmentSlotDefinition slot, EquippedItemRef item)
    {
        ArgumentNullException.ThrowIfNull(slot);
        return slot.Accepts(item.ItemTypePath);
    }

    public void ValidateCanOccupy(EquipmentSlotDefinition slot, EquippedItemRef item)
    {
        if (!CanOccupy(slot, item))
        {
            throw new InvalidOperationException(
                $"Item type '{item.ItemTypePath}' is not accepted by equipment slot '{slot.Id}'."
            );
        }
    }
}

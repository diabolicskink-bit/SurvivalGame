namespace SurvivalGame.Domain;

public static class InventoryGridRules
{
    public static bool UsesGrid(ItemDefinition item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return !string.Equals(item.Category, "Ammunition", StringComparison.OrdinalIgnoreCase);
    }
}

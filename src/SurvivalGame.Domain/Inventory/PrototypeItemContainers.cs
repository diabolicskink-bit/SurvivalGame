namespace SurvivalGame.Domain;

public static class PrototypeItemContainers
{
    public static readonly ContainerId PlayerInventory = new("player_inventory");

    public static readonly InventoryItemSize PlayerInventorySize = new(20, 10);
}

namespace SurvivalGame.Domain;

public sealed class PlayerState
{
    public PlayerInventory Inventory { get; } = new();
}

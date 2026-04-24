namespace SurvivalGame.Domain;

public sealed class PlayerState
{
    public PlayerState()
        : this(new GridPosition(0, 0))
    {
    }

    public PlayerState(GridPosition position)
    {
        Position = position;
    }

    public GridPosition Position { get; private set; }

    public PlayerInventory Inventory { get; } = new();

    public EquipmentLoadout Equipment { get; } = EquipmentLoadout.CreateDefault();

    public PlayerVitals Vitals { get; } = new();

    public void SetPosition(GridPosition position)
    {
        Position = position;
    }
}

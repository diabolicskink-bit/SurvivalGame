namespace SurvivalGame.Domain;

public sealed class NpcState
{
    public NpcState(
        NpcId id,
        string name,
        GridPosition position,
        int currentHealth = 100,
        int maximumHealth = 100
    )
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("NPC name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name.Trim();
        Position = position;
        Health = new BoundedMeter(currentHealth, maximumHealth);
    }

    public NpcId Id { get; }

    public string Name { get; }

    public GridPosition Position { get; private set; }

    public BoundedMeter Health { get; private set; }

    internal void SetPosition(GridPosition position)
    {
        Position = position;
    }

    public void SetHealth(int current)
    {
        Health = Health.WithCurrent(current);
    }
}

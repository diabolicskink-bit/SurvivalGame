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
        : this(
            id,
            new NpcDefinitionId(id.Value),
            name,
            position,
            currentHealth,
            maximumHealth,
            blocksMovement: true
        )
    {
    }

    public NpcState(
        NpcId id,
        NpcDefinitionId definitionId,
        string name,
        GridPosition position,
        int currentHealth = 100,
        int maximumHealth = 100,
        bool blocksMovement = true
    )
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(definitionId);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("NPC name cannot be empty.", nameof(name));
        }

        Id = id;
        DefinitionId = definitionId;
        Name = name.Trim();
        Position = position;
        Health = new BoundedMeter(currentHealth, maximumHealth);
        BlocksMovement = blocksMovement;
    }

    public NpcId Id { get; }

    public NpcDefinitionId DefinitionId { get; }

    public string Name { get; }

    public GridPosition Position { get; private set; }

    public BoundedMeter Health { get; private set; }

    public bool BlocksMovement { get; }

    public bool IsDisabled => Health.Current <= Health.Minimum;

    internal void SetPosition(GridPosition position)
    {
        Position = position;
    }

    public void SetHealth(int current)
    {
        Health = Health.WithCurrent(current);
    }

    public int TakeDamage(int damage)
    {
        if (damage < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage must be at least 1.");
        }

        var previousHealth = Health.Current;
        var nextHealth = Math.Max(Health.Minimum, Health.Current - damage);
        Health = Health.WithCurrent(nextHealth);
        return previousHealth - nextHealth;
    }
}

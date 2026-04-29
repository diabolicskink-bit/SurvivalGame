namespace SurvivalGame.Domain;

public enum StatefulItemLocationKind
{
    PlayerInventory,
    Equipment,
    Ground,
    Inserted,
    Contained,
    TravelCargo
}

public abstract record StatefulItemLocation
{
    public abstract StatefulItemLocationKind Kind { get; }

    public static PlayerInventoryLocation PlayerInventory() => new();

    public static GroundLocation Ground(GridPosition position, SiteId? siteId = null) =>
        new(position, siteId ?? SiteId.Default);

    public static EquipmentLocation Equipment(EquipmentSlotId slotId)
    {
        ArgumentNullException.ThrowIfNull(slotId);
        return new EquipmentLocation(slotId);
    }

    public static InsertedLocation Inserted(StatefulItemId parentItemId) => new(parentItemId);

    public static ContainedLocation Contained(StatefulItemId parentItemId) => new(parentItemId);

    public static TravelCargoLocation TravelCargo() => new();
}

public sealed record PlayerInventoryLocation : StatefulItemLocation
{
    public override StatefulItemLocationKind Kind => StatefulItemLocationKind.PlayerInventory;
}

public sealed record GroundLocation(GridPosition Position, SiteId SiteId) : StatefulItemLocation
{
    public override StatefulItemLocationKind Kind => StatefulItemLocationKind.Ground;
}

public sealed record EquipmentLocation(EquipmentSlotId SlotId) : StatefulItemLocation
{
    public override StatefulItemLocationKind Kind => StatefulItemLocationKind.Equipment;
}

public sealed record InsertedLocation(StatefulItemId ParentItemId) : StatefulItemLocation
{
    public override StatefulItemLocationKind Kind => StatefulItemLocationKind.Inserted;
}

public sealed record ContainedLocation(StatefulItemId ParentItemId) : StatefulItemLocation
{
    public override StatefulItemLocationKind Kind => StatefulItemLocationKind.Contained;
}

public sealed record TravelCargoLocation : StatefulItemLocation
{
    public override StatefulItemLocationKind Kind => StatefulItemLocationKind.TravelCargo;
}

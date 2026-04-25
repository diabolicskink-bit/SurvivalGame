namespace SurvivalGame.Domain;

public enum StatefulItemLocationKind
{
    PlayerInventory,
    Equipment,
    Ground,
    Inserted,
    Contained
}

public sealed record StatefulItemLocation
{
    private StatefulItemLocation(
        StatefulItemLocationKind kind,
        GridPosition? position = null,
        string? siteId = null,
        EquipmentSlotId? equipmentSlotId = null,
        StatefulItemId? parentItemId = null)
    {
        Kind = kind;
        Position = position;
        SiteId = kind == StatefulItemLocationKind.Ground
            ? NormalizeSiteId(siteId)
            : null;
        EquipmentSlotId = equipmentSlotId;
        ParentItemId = parentItemId;
    }

    public StatefulItemLocationKind Kind { get; }

    public GridPosition? Position { get; }

    public string? SiteId { get; }

    public EquipmentSlotId? EquipmentSlotId { get; }

    public StatefulItemId? ParentItemId { get; }

    public static StatefulItemLocation PlayerInventory()
    {
        return new StatefulItemLocation(StatefulItemLocationKind.PlayerInventory);
    }

    public static StatefulItemLocation Ground(GridPosition position, string? siteId = null)
    {
        return new StatefulItemLocation(StatefulItemLocationKind.Ground, position: position, siteId: siteId);
    }

    private static string NormalizeSiteId(string? siteId)
    {
        return string.IsNullOrWhiteSpace(siteId)
            ? PrototypeGameState.DefaultSiteId
            : siteId.Trim();
    }

    public static StatefulItemLocation Equipment(EquipmentSlotId slotId)
    {
        ArgumentNullException.ThrowIfNull(slotId);
        return new StatefulItemLocation(StatefulItemLocationKind.Equipment, equipmentSlotId: slotId);
    }

    public static StatefulItemLocation Inserted(StatefulItemId parentItemId)
    {
        return new StatefulItemLocation(StatefulItemLocationKind.Inserted, parentItemId: parentItemId);
    }

    public static StatefulItemLocation Contained(StatefulItemId parentItemId)
    {
        return new StatefulItemLocation(StatefulItemLocationKind.Contained, parentItemId: parentItemId);
    }
}

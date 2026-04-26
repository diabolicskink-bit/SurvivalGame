namespace SurvivalGame.Domain;

public enum ContainerItemRefKind
{
    Stack,
    Stateful
}

public sealed record ContainerItemRef
{
    private ContainerItemRef(ContainerItemRefKind kind, ItemId? itemId = null, StatefulItemId? statefulItemId = null)
    {
        Kind = kind;
        ItemId = itemId;
        StatefulItemId = statefulItemId;
    }

    public ContainerItemRefKind Kind { get; }

    public ItemId? ItemId { get; }

    public StatefulItemId? StatefulItemId { get; }

    public static ContainerItemRef Stack(ItemId itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        return new ContainerItemRef(ContainerItemRefKind.Stack, itemId: itemId);
    }

    public static ContainerItemRef Stateful(StatefulItemId itemId)
    {
        return new ContainerItemRef(ContainerItemRefKind.Stateful, statefulItemId: itemId);
    }

    public override string ToString()
    {
        return Kind == ContainerItemRefKind.Stack
            ? ItemId?.ToString() ?? "stack"
            : StatefulItemId?.ToString() ?? "stateful";
    }
}

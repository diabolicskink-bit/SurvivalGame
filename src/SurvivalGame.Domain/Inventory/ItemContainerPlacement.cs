namespace SurvivalGame.Domain;

public sealed record ItemContainerPlacement(
    ContainerItemRef Item,
    InventoryGridPosition Position,
    InventoryItemSize Size
);

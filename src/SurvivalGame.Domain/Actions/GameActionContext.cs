namespace SurvivalGame.Domain;

public sealed class GameActionContext
{
    public GameActionContext(
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        WorldObjectCatalog? worldObjectCatalog,
        NpcCatalog? npcCatalog,
        FirearmActionService? firearmActions,
        VehicleFuelState? vehicleFuelState,
        TravelCargoStore? travelCargo,
        ItemDescriber itemDescriber)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(itemCatalog);
        ArgumentNullException.ThrowIfNull(itemDescriber);

        State = state;
        ItemCatalog = itemCatalog;
        WorldObjectCatalog = worldObjectCatalog;
        NpcCatalog = npcCatalog;
        LocalMapQuery = new LocalMapQuery(state.LocalMap, worldObjectCatalog);
        FirearmActions = firearmActions;
        VehicleFuelState = vehicleFuelState;
        TravelCargo = travelCargo;
        ItemDescriber = itemDescriber;
    }

    public PrototypeGameState State { get; }

    public ItemCatalog ItemCatalog { get; }

    public WorldObjectCatalog? WorldObjectCatalog { get; }

    public NpcCatalog? NpcCatalog { get; }

    public LocalMapQuery LocalMapQuery { get; }

    public FirearmActionService? FirearmActions { get; }

    public VehicleFuelState? VehicleFuelState { get; }

    public TravelCargoStore? TravelCargo { get; }

    public ItemDescriber ItemDescriber { get; }

    public bool IsSlotFree(EquipmentSlotId slotId)
    {
        return State.Player.Equipment.IsEmpty(slotId)
            && State.StatefulItems.EquippedIn(slotId) is null;
    }

    public bool TryPlaceStatefulItemInInventory(StatefulItem item)
    {
        return State.Player.Inventory.TryPlaceStatefulItem(
            item,
            ItemDescriber.GetInventorySize(item.ItemId)
        );
    }

    public void SynchronizeStatefulInventoryPlacements()
    {
        State.Player.Inventory.SynchronizeStatefulInventoryPlacements(
            State.StatefulItems.InPlayerInventory(),
            ItemDescriber.GetInventorySize
        );
    }
}

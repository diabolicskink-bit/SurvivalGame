namespace SurvivalGame.Domain;

public enum GameActionKind
{
    Wait,
    Move,
    Pickup,
    InspectItem,
    DropItemStack,
    EquipItem,
    UnequipItem,
    LoadFeedDevice,
    UnloadFeedDevice,
    InsertFeedDevice,
    RemoveFeedDevice,
    LoadWeapon,
    ReloadWeapon,
    TestFire,
    ToggleFireMode,
    PickupStatefulItem,
    DropStatefulItem,
    InspectStatefulItem,
    EquipStatefulItem,
    UnequipStatefulItem,
    LoadStatefulFeedDevice,
    UnloadStatefulFeedDevice,
    InsertStatefulFeedDevice,
    RemoveStatefulFeedDevice,
    LoadStatefulWeapon,
    ReloadStatefulWeapon,
    TestFireStatefulWeapon,
    ToggleStatefulFireMode,
    InstallStatefulWeaponMod,
    RemoveStatefulWeaponMod,
    ShootNpc,
    RefuelVehicle,
    SearchContainer,
    TakeContainerItemStack,
    StowItemStackInTravelCargo,
    TakeTravelCargoItemStack,
    StowStatefulItemInTravelCargo,
    TakeTravelCargoStatefulItem,
    FillFuelCan,
    PourFuelCanIntoVehicle
}

public abstract record GameActionRequest(GameActionKind Kind);

public sealed record WaitActionRequest() : GameActionRequest(GameActionKind.Wait);

public sealed record MoveActionRequest(GridOffset Direction) : GameActionRequest(GameActionKind.Move);

public sealed record PickupActionRequest() : GameActionRequest(GameActionKind.Pickup);

public sealed record InspectItemActionRequest(ItemId ItemId)
    : GameActionRequest(GameActionKind.InspectItem);

public sealed record DropItemStackActionRequest(ItemId ItemId, int Quantity)
    : GameActionRequest(GameActionKind.DropItemStack);

public sealed record EquipItemActionRequest(ItemId ItemId, EquipmentSlotId SlotId)
    : GameActionRequest(GameActionKind.EquipItem);

public sealed record UnequipItemActionRequest(EquipmentSlotId SlotId)
    : GameActionRequest(GameActionKind.UnequipItem);

public sealed record LoadFeedDeviceActionRequest(ItemId FeedDeviceItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.LoadFeedDevice);

public sealed record UnloadFeedDeviceActionRequest(ItemId FeedDeviceItemId)
    : GameActionRequest(GameActionKind.UnloadFeedDevice);

public sealed record InsertFeedDeviceActionRequest(ItemId WeaponItemId, ItemId FeedDeviceItemId)
    : GameActionRequest(GameActionKind.InsertFeedDevice);

public sealed record RemoveFeedDeviceActionRequest(ItemId WeaponItemId)
    : GameActionRequest(GameActionKind.RemoveFeedDevice);

public sealed record LoadWeaponActionRequest(ItemId WeaponItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.LoadWeapon);

public sealed record ReloadWeaponActionRequest(ItemId WeaponItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.ReloadWeapon);

public sealed record TestFireActionRequest(ItemId WeaponItemId)
    : GameActionRequest(GameActionKind.TestFire);

public sealed record ToggleFireModeActionRequest(ItemId WeaponItemId)
    : GameActionRequest(GameActionKind.ToggleFireMode);

public sealed record PickupStatefulItemActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.PickupStatefulItem);

public sealed record DropStatefulItemActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.DropStatefulItem);

public sealed record InspectStatefulItemActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.InspectStatefulItem);

public sealed record EquipStatefulItemActionRequest(StatefulItemId ItemId, EquipmentSlotId SlotId)
    : GameActionRequest(GameActionKind.EquipStatefulItem);

public sealed record UnequipStatefulItemActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.UnequipStatefulItem);

public sealed record LoadStatefulFeedDeviceActionRequest(StatefulItemId FeedDeviceItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.LoadStatefulFeedDevice);

public sealed record UnloadStatefulFeedDeviceActionRequest(StatefulItemId FeedDeviceItemId)
    : GameActionRequest(GameActionKind.UnloadStatefulFeedDevice);

public sealed record InsertStatefulFeedDeviceActionRequest(StatefulItemId WeaponItemId, StatefulItemId FeedDeviceItemId)
    : GameActionRequest(GameActionKind.InsertStatefulFeedDevice);

public sealed record RemoveStatefulFeedDeviceActionRequest(StatefulItemId WeaponItemId)
    : GameActionRequest(GameActionKind.RemoveStatefulFeedDevice);

public sealed record LoadStatefulWeaponActionRequest(StatefulItemId WeaponItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.LoadStatefulWeapon);

public sealed record ReloadStatefulWeaponActionRequest(StatefulItemId WeaponItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.ReloadStatefulWeapon);

public sealed record TestFireStatefulWeaponActionRequest(StatefulItemId WeaponItemId)
    : GameActionRequest(GameActionKind.TestFireStatefulWeapon);

public sealed record ToggleStatefulFireModeActionRequest(StatefulItemId WeaponItemId)
    : GameActionRequest(GameActionKind.ToggleStatefulFireMode);

public sealed record InstallStatefulWeaponModActionRequest(StatefulItemId WeaponItemId, StatefulItemId ModItemId)
    : GameActionRequest(GameActionKind.InstallStatefulWeaponMod);

public sealed record RemoveStatefulWeaponModActionRequest(StatefulItemId WeaponItemId, WeaponModSlotId SlotId)
    : GameActionRequest(GameActionKind.RemoveStatefulWeaponMod);

public sealed record ShootNpcActionRequest(NpcId TargetNpcId)
    : GameActionRequest(GameActionKind.ShootNpc);

public sealed record RefuelVehicleActionRequest() : GameActionRequest(GameActionKind.RefuelVehicle);

public sealed record SearchContainerActionRequest(WorldObjectInstanceId ContainerId)
    : GameActionRequest(GameActionKind.SearchContainer);

public sealed record TakeContainerItemStackActionRequest(WorldObjectInstanceId ContainerId, ItemId ItemId, int Quantity)
    : GameActionRequest(GameActionKind.TakeContainerItemStack);

public sealed record StowItemStackInTravelCargoActionRequest(ItemId ItemId, int Quantity)
    : GameActionRequest(GameActionKind.StowItemStackInTravelCargo);

public sealed record TakeTravelCargoItemStackActionRequest(ItemId ItemId, int Quantity)
    : GameActionRequest(GameActionKind.TakeTravelCargoItemStack);

public sealed record StowStatefulItemInTravelCargoActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.StowStatefulItemInTravelCargo);

public sealed record TakeTravelCargoStatefulItemActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.TakeTravelCargoStatefulItem);

public sealed record FillFuelCanActionRequest(StatefulItemId FuelCanId)
    : GameActionRequest(GameActionKind.FillFuelCan);

public sealed record PourFuelCanIntoVehicleActionRequest(StatefulItemId FuelCanId)
    : GameActionRequest(GameActionKind.PourFuelCanIntoVehicle);

public sealed record AvailableAction(GameActionKind Kind, string Label, GameActionRequest? Request = null);

public sealed record GameActionResult(bool Succeeded, int ElapsedTicks, IReadOnlyList<string> Messages)
{
    public static GameActionResult Success(int elapsedTicks, params string[] messages)
    {
        if (elapsedTicks < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedTicks), "Elapsed tick cost cannot be negative.");
        }

        return new GameActionResult(true, elapsedTicks, messages);
    }

    public static GameActionResult Failure(params string[] messages)
    {
        return new GameActionResult(false, 0, messages);
    }
}

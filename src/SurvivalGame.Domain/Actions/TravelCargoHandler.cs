namespace SurvivalGame.Domain;

public sealed class TravelCargoHandler : IActionHandler
{
    private const string CargoAnchorTag = "cargo_anchor";
    private const string RefuelSourceTag = "refuel_source";
    private const string FuelReceiverTag = "fuel_receiver";

    private static readonly GridOffset[] NearbyOffsets =
    [
        GridOffset.Zero,
        GridOffset.Up,
        GridOffset.Down,
        GridOffset.Left,
        GridOffset.Right
    ];

    public IReadOnlyList<GameActionKind> HandledKinds { get; } = new[]
    {
        GameActionKind.StowItemStackInTravelCargo,
        GameActionKind.TakeTravelCargoItemStack,
        GameActionKind.StowStatefulItemInTravelCargo,
        GameActionKind.TakeTravelCargoStatefulItem,
        GameActionKind.FillFuelCan,
        GameActionKind.PourFuelCanIntoVehicle
    };

    public IEnumerable<AvailableAction> GetAvailableActions(GameActionContext context)
    {
        var cargo = GetAccessibleCargo(context);
        if (cargo is not null)
        {
            foreach (var stack in context.State.Player.Inventory.Items)
            {
                var itemName = context.ItemDescriber.GetItemName(stack.ItemId);
                yield return new AvailableAction(
                    GameActionKind.StowItemStackInTravelCargo,
                    $"Stow one {itemName} in cargo",
                    new StowItemStackInTravelCargoActionRequest(stack.ItemId, 1)
                );

                if (stack.Quantity > 1)
                {
                    yield return new AvailableAction(
                        GameActionKind.StowItemStackInTravelCargo,
                        $"Stow all {stack.Quantity} {itemName} in cargo",
                        new StowItemStackInTravelCargoActionRequest(stack.ItemId, stack.Quantity)
                    );
                }
            }

            foreach (var stack in cargo.StackItems)
            {
                var itemName = context.ItemDescriber.GetItemName(stack.ItemId);
                yield return new AvailableAction(
                    GameActionKind.TakeTravelCargoItemStack,
                    $"Take {itemName} from cargo",
                    new TakeTravelCargoItemStackActionRequest(stack.ItemId, 1)
                );

                if (stack.Quantity > 1)
                {
                    yield return new AvailableAction(
                        GameActionKind.TakeTravelCargoItemStack,
                        $"Take all {stack.Quantity} {itemName} from cargo",
                        new TakeTravelCargoItemStackActionRequest(stack.ItemId, stack.Quantity)
                    );
                }
            }

            foreach (var item in context.State.StatefulItems.InPlayerInventory())
            {
                yield return new AvailableAction(
                    GameActionKind.StowStatefulItemInTravelCargo,
                    $"Stow {context.ItemDescriber.FormatStatefulItem(item)} in cargo",
                    new StowStatefulItemInTravelCargoActionRequest(item.Id)
                );
            }

            foreach (var itemId in cargo.StatefulItemIds.OrderBy(id => id.Value))
            {
                if (!context.State.StatefulItems.TryGet(itemId, out var item))
                {
                    continue;
                }

                yield return new AvailableAction(
                    GameActionKind.TakeTravelCargoStatefulItem,
                    $"Take {context.ItemDescriber.FormatStatefulItem(item)} from cargo",
                    new TakeTravelCargoStatefulItemActionRequest(item.Id)
                );
            }
        }

        foreach (var fuelCan in context.State.StatefulItems.InPlayerInventory()
            .Where(item => item.FuelContainer is not null))
        {
            if (!fuelCan.FuelContainer!.IsFull && IsNearTaggedWorldObject(context, RefuelSourceTag))
            {
                yield return new AvailableAction(
                    GameActionKind.FillFuelCan,
                    $"Fill {context.ItemDescriber.FormatStatefulItem(fuelCan)}",
                    new FillFuelCanActionRequest(fuelCan.Id)
                );
            }

            if (!fuelCan.FuelContainer.IsEmpty && IsNearActiveAnchorWithTag(context, FuelReceiverTag))
            {
                yield return new AvailableAction(
                    GameActionKind.PourFuelCanIntoVehicle,
                    $"Pour {context.ItemDescriber.FormatStatefulItem(fuelCan)} into vehicle",
                    new PourFuelCanIntoVehicleActionRequest(fuelCan.Id)
                );
            }
        }
    }

    public GameActionResult Handle(GameActionRequest request, GameActionContext context)
    {
        return request switch
        {
            StowItemStackInTravelCargoActionRequest stowStack => StowItemStack(context, stowStack.ItemId, stowStack.Quantity),
            TakeTravelCargoItemStackActionRequest takeStack => TakeItemStack(context, takeStack.ItemId, takeStack.Quantity),
            StowStatefulItemInTravelCargoActionRequest stowStateful => StowStatefulItem(context, stowStateful.ItemId),
            TakeTravelCargoStatefulItemActionRequest takeStateful => TakeStatefulItem(context, takeStateful.ItemId),
            FillFuelCanActionRequest fill => FillFuelCan(context, fill.FuelCanId),
            PourFuelCanIntoVehicleActionRequest pour => PourFuelCanIntoVehicle(context, pour.FuelCanId),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private static GameActionResult StowItemStack(GameActionContext context, ItemId itemId, int quantity)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        if (quantity < 1)
        {
            return GameActionResult.Failure("Stow quantity must be at least 1.");
        }

        var cargo = GetAccessibleCargo(context);
        if (cargo is null)
        {
            return GameActionResult.Failure("You need to stand next to your travel anchor to access cargo.");
        }

        if (context.State.Player.Inventory.CountOf(itemId) < quantity)
        {
            return GameActionResult.Failure("You do not have enough of that item to stow.");
        }

        context.State.Player.Inventory.TryRemove(itemId, quantity);
        cargo.StowStack(itemId, quantity);
        context.State.AdvanceTime(GameActionPipeline.PickupTickCost);

        var stack = new GroundItemStack(itemId, quantity);
        return GameActionResult.Success(
            GameActionPipeline.PickupTickCost,
            $"Stowed {context.ItemDescriber.FormatStack(stack)} in travel cargo. Time +{GameActionPipeline.PickupTickCost}."
        );
    }

    private static GameActionResult TakeItemStack(GameActionContext context, ItemId itemId, int quantity)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        if (quantity < 1)
        {
            return GameActionResult.Failure("Take quantity must be at least 1.");
        }

        var cargo = GetAccessibleCargo(context);
        if (cargo is null)
        {
            return GameActionResult.Failure("You need to stand next to your travel anchor to access cargo.");
        }

        if (cargo.CountOf(itemId) < quantity)
        {
            return GameActionResult.Failure("That item is not in travel cargo.");
        }

        if (!context.State.Player.Inventory.CanAdd(
            itemId,
            context.ItemDescriber.GetInventorySize(itemId),
            context.ItemDescriber.UsesInventoryGrid(itemId)))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        if (!cargo.TryTakeStack(itemId, quantity, out var takenStack))
        {
            return GameActionResult.Failure("That item is not in travel cargo.");
        }

        context.State.Player.Inventory.Add(
            itemId,
            quantity,
            context.ItemDescriber.GetInventorySize(itemId),
            context.ItemDescriber.UsesInventoryGrid(itemId)
        );
        context.State.AdvanceTime(GameActionPipeline.PickupTickCost);

        return GameActionResult.Success(
            GameActionPipeline.PickupTickCost,
            $"Took {context.ItemDescriber.FormatStack(takenStack)} from travel cargo. Time +{GameActionPipeline.PickupTickCost}."
        );
    }

    private static GameActionResult StowStatefulItem(GameActionContext context, StatefulItemId itemId)
    {
        var cargo = GetAccessibleCargo(context);
        if (cargo is null)
        {
            return GameActionResult.Failure("You need to stand next to your travel anchor to access cargo.");
        }

        var item = context.State.StatefulItems.Get(itemId);
        if (item.Location is not PlayerInventoryLocation)
        {
            return GameActionResult.Failure("That item is not freely available to stow.");
        }

        context.State.Player.Inventory.Container.Remove(ContainerItemRef.Stateful(item.Id));
        context.State.StatefulItems.MoveToTravelCargo(item.Id);
        cargo.StowStatefulItem(item.Id);
        context.State.AdvanceTime(GameActionPipeline.PickupTickCost);

        return GameActionResult.Success(
            GameActionPipeline.PickupTickCost,
            $"Stowed {context.ItemDescriber.FormatStatefulItem(item)} in travel cargo. Time +{GameActionPipeline.PickupTickCost}."
        );
    }

    private static GameActionResult TakeStatefulItem(GameActionContext context, StatefulItemId itemId)
    {
        var cargo = GetAccessibleCargo(context);
        if (cargo is null)
        {
            return GameActionResult.Failure("You need to stand next to your travel anchor to access cargo.");
        }

        if (!cargo.ContainsStatefulItem(itemId))
        {
            return GameActionResult.Failure("That item is not in travel cargo.");
        }

        var item = context.State.StatefulItems.Get(itemId);
        if (item.Location is not TravelCargoLocation)
        {
            return GameActionResult.Failure("That item is not in travel cargo.");
        }

        if (!context.TryPlaceStatefulItemInInventory(item))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        cargo.TryTakeStatefulItem(item.Id);
        context.State.StatefulItems.MoveToInventory(item.Id);
        context.State.AdvanceTime(GameActionPipeline.PickupTickCost);

        return GameActionResult.Success(
            GameActionPipeline.PickupTickCost,
            $"Took {context.ItemDescriber.FormatStatefulItem(item)} from travel cargo. Time +{GameActionPipeline.PickupTickCost}."
        );
    }

    private static GameActionResult FillFuelCan(GameActionContext context, StatefulItemId fuelCanId)
    {
        var fuelCan = context.State.StatefulItems.Get(fuelCanId);
        if (fuelCan.Location is not PlayerInventoryLocation || fuelCan.FuelContainer is null)
        {
            return GameActionResult.Failure("That item is not a carried fuel can.");
        }

        if (fuelCan.FuelContainer.IsFull)
        {
            return GameActionResult.Failure("Fuel can is already full.");
        }

        if (!IsNearTaggedWorldObject(context, RefuelSourceTag))
        {
            return GameActionResult.Failure("You need to stand next to a fuel source.");
        }

        var accepted = fuelCan.FuelContainer.AddFuel(fuelCan.FuelContainer.Capacity);
        context.State.AdvanceTime(GameActionPipeline.RefuelVehicleTickCost);

        return GameActionResult.Success(
            GameActionPipeline.RefuelVehicleTickCost,
            $"Filled {context.ItemDescriber.FormatStatefulItem(fuelCan)} with {accepted:0.0} fuel. Time +{GameActionPipeline.RefuelVehicleTickCost}."
        );
    }

    private static GameActionResult PourFuelCanIntoVehicle(GameActionContext context, StatefulItemId fuelCanId)
    {
        if (context.VehicleFuelState is null)
        {
            return GameActionResult.Failure("No vehicle fuel state is available.");
        }

        var fuelCan = context.State.StatefulItems.Get(fuelCanId);
        if (fuelCan.Location is not PlayerInventoryLocation || fuelCan.FuelContainer is null)
        {
            return GameActionResult.Failure("That item is not a carried fuel can.");
        }

        if (fuelCan.FuelContainer.IsEmpty)
        {
            return GameActionResult.Failure("Fuel can is empty.");
        }

        if (!IsNearActiveAnchorWithTag(context, FuelReceiverTag))
        {
            return GameActionResult.Failure("You need to stand next to your vehicle.");
        }

        if (context.VehicleFuelState.IsFull)
        {
            return GameActionResult.Failure("Vehicle fuel is already full.");
        }

        var accepted = context.VehicleFuelState.AddFuel(fuelCan.FuelContainer.CurrentFuel);
        fuelCan.FuelContainer.RemoveFuel(accepted);
        context.State.AdvanceTime(GameActionPipeline.RefuelVehicleTickCost);

        return GameActionResult.Success(
            GameActionPipeline.RefuelVehicleTickCost,
            $"Poured {accepted:0.0} fuel into the vehicle. Vehicle fuel {context.VehicleFuelState.CurrentFuel:0.0}/{context.VehicleFuelState.Capacity:0.0}. Time +{GameActionPipeline.RefuelVehicleTickCost}."
        );
    }

    private static TravelCargoStore? GetAccessibleCargo(GameActionContext context)
    {
        var cargo = context.TravelCargo;
        if (cargo is null || !IsNearActiveAnchorWithTag(context, CargoAnchorTag))
        {
            return null;
        }

        return cargo;
    }

    private static bool IsNearActiveAnchorWithTag(GameActionContext context, string tag)
    {
        if (context.WorldObjectCatalog is null
            || context.State.ActiveTravelAnchorInstanceId is null
            || !context.State.WorldObjects.TryGetPlacement(context.State.ActiveTravelAnchorInstanceId, out var placement)
            || !context.WorldObjectCatalog.TryGet(placement.ObjectId, out var definition)
            || !definition.HasTag(tag))
        {
            return false;
        }

        return IsPlayerNearPlacement(context.State, placement);
    }

    private static bool IsNearTaggedWorldObject(GameActionContext context, string tag)
    {
        if (context.WorldObjectCatalog is null)
        {
            return false;
        }

        foreach (var offset in NearbyOffsets)
        {
            var position = context.State.Player.Position + offset;
            if (!context.State.LocalMap.Map.Contains(position)
                || !context.State.WorldObjects.TryGetPlacementAt(position, out var placement)
                || !context.WorldObjectCatalog.TryGet(placement.ObjectId, out var definition)
                || !definition.HasTag(tag))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool IsPlayerNearPlacement(PrototypeGameState state, PlacedWorldObject placement)
    {
        foreach (var occupiedPosition in placement.OccupiedPositions())
        {
            if (Math.Abs(occupiedPosition.X - state.Player.Position.X)
                + Math.Abs(occupiedPosition.Y - state.Player.Position.Y) <= 1)
            {
                return true;
            }
        }

        return false;
    }
}

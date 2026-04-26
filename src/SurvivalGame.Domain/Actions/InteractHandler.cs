namespace SurvivalGame.Domain;

public sealed class InteractHandler : IActionHandler
{
    public IReadOnlyList<GameActionKind> HandledKinds { get; } = new[]
    {
        GameActionKind.RefuelVehicle,
        GameActionKind.SearchContainer,
        GameActionKind.TakeContainerItemStack
    };

    public IEnumerable<AvailableAction> GetAvailableActions(GameActionContext context)
    {
        if (CanRefuelVehicle(context))
        {
            yield return new AvailableAction(
                GameActionKind.RefuelVehicle,
                "Refuel Vehicle",
                new RefuelVehicleActionRequest()
            );
        }

        foreach (var container in GetAdjacentContainerPlacements(context))
        {
            if (!TryGetContainerDefinition(context, container, out var definition))
            {
                continue;
            }

            if (!context.State.LocalMap.ContainerStates.TryGet(container.InstanceId, out var containerState))
            {
                yield return new AvailableAction(
                    GameActionKind.SearchContainer,
                    $"Search {definition.Name}",
                    new SearchContainerActionRequest(container.InstanceId)
                );
                continue;
            }

            foreach (var stack in containerState.RemainingStacks)
            {
                var itemName = context.ItemDescriber.GetItemName(stack.ItemId);
                yield return new AvailableAction(
                    GameActionKind.TakeContainerItemStack,
                    $"Take {itemName} from {definition.Name}",
                    new TakeContainerItemStackActionRequest(container.InstanceId, stack.ItemId, 1)
                );

                if (stack.Quantity > 1)
                {
                    yield return new AvailableAction(
                        GameActionKind.TakeContainerItemStack,
                        $"Take all {stack.Quantity} {itemName} from {definition.Name}",
                        new TakeContainerItemStackActionRequest(container.InstanceId, stack.ItemId, stack.Quantity)
                    );
                }
            }
        }
    }

    public GameActionResult Handle(GameActionRequest request, GameActionContext context)
    {
        return request switch
        {
            RefuelVehicleActionRequest => RefuelVehicle(context),
            SearchContainerActionRequest search => SearchContainer(context, search.ContainerId),
            TakeContainerItemStackActionRequest take => TakeContainerItemStack(
                context,
                take.ContainerId,
                take.ItemId,
                take.Quantity
            ),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private static GameActionResult SearchContainer(GameActionContext context, WorldObjectInstanceId containerId)
    {
        if (!TryGetAdjacentContainer(context, containerId, out var placement, out var definition))
        {
            return GameActionResult.Failure("You need to stand next to that container.");
        }

        var containerDefinition = definition.Container
            ?? throw new InvalidOperationException($"World object '{definition.Id}' is not a container.");

        if (context.State.LocalMap.ContainerStates.TryGet(containerId, out var existingState))
        {
            return GameActionResult.Success(
                0,
                FormatAlreadySearchedMessage(context, definition, existingState)
            );
        }

        var containerState = context.State.LocalMap.ContainerStates.GetOrRealize(
            containerId,
            placement.ContainerLoot ?? WorldObjectContainerLootSpec.Empty
        );
        context.State.AdvanceTime(containerDefinition.SearchTickCost);

        return GameActionResult.Success(
            containerDefinition.SearchTickCost,
            FormatSearchMessage(context, definition, containerState, containerDefinition.SearchTickCost)
        );
    }

    private static GameActionResult TakeContainerItemStack(
        GameActionContext context,
        WorldObjectInstanceId containerId,
        ItemId itemId,
        int quantity)
    {
        ArgumentNullException.ThrowIfNull(itemId);

        if (quantity < 1)
        {
            return GameActionResult.Failure("Take quantity must be at least 1.");
        }

        if (!TryGetAdjacentContainer(context, containerId, out _, out var definition))
        {
            return GameActionResult.Failure("You need to stand next to that container.");
        }

        if (!context.State.LocalMap.ContainerStates.TryGet(containerId, out var containerState))
        {
            return GameActionResult.Failure($"Search {definition.Name} first.");
        }

        if (containerState.CountOf(itemId) < quantity)
        {
            return GameActionResult.Failure("That item is not in the container.");
        }

        if (!context.State.Player.Inventory.CanAdd(
            itemId,
            context.ItemDescriber.GetInventorySize(itemId),
            context.ItemDescriber.UsesInventoryGrid(itemId)))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        if (!containerState.TryTake(itemId, quantity, out var takenStack))
        {
            return GameActionResult.Failure("That item is not in the container.");
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
            $"Took {context.ItemDescriber.FormatStack(takenStack)} from {definition.Name}. Time +{GameActionPipeline.PickupTickCost}."
        );
    }

    private static GameActionResult RefuelVehicle(GameActionContext context)
    {
        if (context.VehicleFuelState is null)
        {
            return GameActionResult.Failure("No vehicle fuel state is available.");
        }

        if (context.VehicleFuelState.IsFull)
        {
            return GameActionResult.Failure("Vehicle fuel is already full.");
        }

        if (!IsAdjacentToFuelPump(context))
        {
            return GameActionResult.Failure("You need to stand next to a fuel pump.");
        }

        context.VehicleFuelState.Refill();
        context.State.AdvanceTime(GameActionPipeline.RefuelVehicleTickCost);

        return GameActionResult.Success(
            GameActionPipeline.RefuelVehicleTickCost,
            $"Refueled vehicle to {context.VehicleFuelState.CurrentFuel:0.0}/{context.VehicleFuelState.Capacity:0.0}. Time +{GameActionPipeline.RefuelVehicleTickCost}."
        );
    }

    private static bool CanRefuelVehicle(GameActionContext context)
    {
        return context.VehicleFuelState is not null
            && !context.VehicleFuelState.IsFull
            && IsAdjacentToFuelPump(context);
    }

    private static bool IsAdjacentToFuelPump(GameActionContext context)
    {
        return AdjacentOffsets().Any(offset =>
        {
            var position = context.State.Player.Position + offset;
            return context.State.LocalMap.Map.Contains(position)
                && context.State.LocalMap.WorldObjects.TryGetObjectAt(position, out var objectId)
                && context.WorldObjectCatalog is not null
                && context.WorldObjectCatalog.TryGet(objectId, out var worldObjectDef)
                && worldObjectDef.HasTag("refuel_source");
        });
    }

    private static IEnumerable<PlacedWorldObject> GetAdjacentContainerPlacements(GameActionContext context)
    {
        if (context.WorldObjectCatalog is null)
        {
            yield break;
        }

        var seen = new HashSet<WorldObjectInstanceId>();
        foreach (var offset in AdjacentOffsets())
        {
            var position = context.State.Player.Position + offset;
            if (!context.State.LocalMap.Map.Contains(position)
                || !context.State.LocalMap.WorldObjects.TryGetPlacementAt(position, out var placement)
                || !seen.Add(placement.InstanceId)
                || !TryGetContainerDefinition(context, placement, out _))
            {
                continue;
            }

            yield return placement;
        }
    }

    private static bool TryGetAdjacentContainer(
        GameActionContext context,
        WorldObjectInstanceId containerId,
        out PlacedWorldObject placement,
        out WorldObjectDefinition definition)
    {
        foreach (var candidate in GetAdjacentContainerPlacements(context))
        {
            if (candidate.InstanceId != containerId)
            {
                continue;
            }

            placement = candidate;
            return TryGetContainerDefinition(context, candidate, out definition);
        }

        placement = default;
        definition = null!;
        return false;
    }

    private static bool TryGetContainerDefinition(
        GameActionContext context,
        PlacedWorldObject placement,
        out WorldObjectDefinition definition)
    {
        if (context.WorldObjectCatalog is not null
            && context.WorldObjectCatalog.TryGet(placement.ObjectId, out var foundDefinition)
            && foundDefinition.IsContainer)
        {
            definition = foundDefinition;
            return true;
        }

        definition = null!;
        return false;
    }

    private static string FormatSearchMessage(
        GameActionContext context,
        WorldObjectDefinition definition,
        WorldObjectContainerState containerState,
        int elapsedTicks)
    {
        if (containerState.IsEmpty)
        {
            return $"You search {definition.Name}. It is empty. Time +{elapsedTicks}.";
        }

        var contents = string.Join(", ", containerState.RemainingStacks.Select(context.ItemDescriber.FormatStack));
        return $"You search {definition.Name}. You find {contents}. Time +{elapsedTicks}.";
    }

    private static string FormatAlreadySearchedMessage(
        GameActionContext context,
        WorldObjectDefinition definition,
        WorldObjectContainerState containerState)
    {
        if (containerState.IsEmpty)
        {
            return $"{definition.Name} is empty.";
        }

        var contents = string.Join(", ", containerState.RemainingStacks.Select(context.ItemDescriber.FormatStack));
        return $"{definition.Name} still contains {contents}.";
    }

    private static IReadOnlyList<GridOffset> AdjacentOffsets()
    {
        return new[]
        {
            GridOffset.Up,
            GridOffset.Down,
            GridOffset.Left,
            GridOffset.Right
        };
    }
}

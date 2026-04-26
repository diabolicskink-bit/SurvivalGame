namespace SurvivalGame.Domain;

public sealed class InventoryHandler : IActionHandler
{
    public IReadOnlyList<GameActionKind> HandledKinds { get; } = new[]
    {
        GameActionKind.Pickup,
        GameActionKind.DropItemStack,
        GameActionKind.PickupStatefulItem,
        GameActionKind.DropStatefulItem
    };

    public IEnumerable<AvailableAction> GetAvailableActions(GameActionContext context)
    {
        var state = context.State;
        if (state.LocalMap.GroundItems.ItemsAt(state.Player.Position).Count > 0)
        {
            yield return new AvailableAction(GameActionKind.Pickup, "Pick Up", new PickupActionRequest());
        }

        foreach (var item in state.StatefulItems.OnGround(state.Player.Position, state.SiteId))
        {
            yield return new AvailableAction(
                GameActionKind.PickupStatefulItem,
                $"Pick up {context.ItemDescriber.FormatStatefulItem(item)}",
                new PickupStatefulItemActionRequest(item.Id)
            );
        }

        foreach (var item in state.StatefulItems.InPlayerInventory())
        {
            yield return new AvailableAction(
                GameActionKind.DropStatefulItem,
                $"Drop {context.ItemDescriber.FormatStatefulItem(item)}",
                new DropStatefulItemActionRequest(item.Id)
            );
        }

        foreach (var stack in state.Player.Inventory.Items)
        {
            var itemName = context.ItemDescriber.GetItemName(stack.ItemId);
            yield return new AvailableAction(
                GameActionKind.DropItemStack,
                $"Drop one {itemName}",
                new DropItemStackActionRequest(stack.ItemId, 1)
            );

            if (stack.Quantity > 1)
            {
                yield return new AvailableAction(
                    GameActionKind.DropItemStack,
                    $"Drop all {stack.Quantity} {itemName}",
                    new DropItemStackActionRequest(stack.ItemId, stack.Quantity)
                );
            }
        }
    }

    public GameActionResult Handle(GameActionRequest request, GameActionContext context)
    {
        return request switch
        {
            PickupActionRequest => Pickup(context),
            DropItemStackActionRequest dropStack => DropItemStack(context, dropStack.ItemId, dropStack.Quantity),
            PickupStatefulItemActionRequest pickupStateful => PickupStatefulItem(context, pickupStateful.ItemId),
            DropStatefulItemActionRequest dropStateful => DropStatefulItem(context, dropStateful.ItemId),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private static GameActionResult Pickup(GameActionContext context)
    {
        var state = context.State;
        var availableStacks = state.LocalMap.GroundItems.ItemsAt(state.Player.Position);
        if (availableStacks.Count == 0)
        {
            return GameActionResult.Failure("There is nothing here to pick up.");
        }

        var requiredSpaces = availableStacks.Select(stack => (
            stack.ItemId,
            context.ItemDescriber.GetInventorySize(stack.ItemId),
            UsesGrid: context.ItemDescriber.UsesInventoryGrid(stack.ItemId)
        ));
        if (!state.Player.Inventory.CanAddAll(requiredSpaces))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        var itemStacks = state.LocalMap.GroundItems.TakeAllAt(state.Player.Position);
        if (itemStacks.Count == 0)
        {
            return GameActionResult.Failure("There is nothing here to pick up.");
        }

        foreach (var stack in itemStacks)
        {
            state.Player.Inventory.Add(
                stack.ItemId,
                stack.Quantity,
                context.ItemDescriber.GetInventorySize(stack.ItemId),
                context.ItemDescriber.UsesInventoryGrid(stack.ItemId)
            );
        }

        state.AdvanceTime(GameActionPipeline.PickupTickCost);
        var messages = itemStacks
            .Select(stack => $"Picked up {context.ItemDescriber.FormatStack(stack)}. Time +{GameActionPipeline.PickupTickCost}.")
            .ToArray();

        return GameActionResult.Success(GameActionPipeline.PickupTickCost, messages);
    }

    private static GameActionResult PickupStatefulItem(GameActionContext context, StatefulItemId itemId)
    {
        var state = context.State;
        var item = state.StatefulItems.Get(itemId);
        if (item.Location is not GroundLocation groundLoc
            || groundLoc.Position != state.Player.Position
            || groundLoc.SiteId != state.SiteId)
        {
            return GameActionResult.Failure("That item is not on this tile.");
        }

        if (!context.TryPlaceStatefulItemInInventory(item))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        state.StatefulItems.MoveToInventory(item.Id);
        state.AdvanceTime(GameActionPipeline.PickupTickCost);

        return GameActionResult.Success(
            GameActionPipeline.PickupTickCost,
            $"Picked up {context.ItemDescriber.FormatStatefulItem(item)}. Time +{GameActionPipeline.PickupTickCost}."
        );
    }

    private static GameActionResult DropStatefulItem(GameActionContext context, StatefulItemId itemId)
    {
        var state = context.State;
        var item = state.StatefulItems.Get(itemId);
        if (item.Location is not PlayerInventoryLocation)
        {
            return GameActionResult.Failure("That item is not freely available to drop.");
        }

        state.StatefulItems.MoveToGround(item.Id, state.Player.Position, state.SiteId);
        state.Player.Inventory.Container.Remove(ContainerItemRef.Stateful(item.Id));
        return GameActionResult.Success(
            0,
            $"Dropped {context.ItemDescriber.FormatStatefulItem(item)}."
        );
    }

    private static GameActionResult DropItemStack(GameActionContext context, ItemId itemId, int quantity)
    {
        var state = context.State;
        if (quantity < 1)
        {
            return GameActionResult.Failure("Drop quantity must be at least 1.");
        }

        var currentQuantity = state.Player.Inventory.CountOf(itemId);
        if (currentQuantity < quantity)
        {
            return GameActionResult.Failure("You do not have enough of that item to drop.");
        }

        state.Player.Inventory.TryRemove(itemId, quantity);
        state.LocalMap.GroundItems.Place(state.Player.Position, itemId, quantity);

        return GameActionResult.Success(
            GameActionPipeline.DropItemTickCost,
            $"Dropped {context.ItemDescriber.FormatStack(new GroundItemStack(itemId, quantity))}."
        );
    }
}

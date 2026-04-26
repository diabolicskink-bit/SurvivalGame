namespace SurvivalGame.Domain;

public sealed class InspectHandler : IActionHandler
{
    public IReadOnlyList<GameActionKind> HandledKinds { get; } = new[]
    {
        GameActionKind.InspectItem,
        GameActionKind.InspectStatefulItem
    };

    public IEnumerable<AvailableAction> GetAvailableActions(GameActionContext context)
    {
        var state = context.State;
        foreach (var item in state.StatefulItems.OnGround(state.Player.Position, state.SiteId))
        {
            yield return new AvailableAction(
                GameActionKind.InspectStatefulItem,
                $"Inspect {context.ItemDescriber.FormatStatefulItem(item)}",
                new InspectStatefulItemActionRequest(item.Id)
            );
        }

        foreach (var item in state.StatefulItems.InPlayerInventory())
        {
            yield return new AvailableAction(
                GameActionKind.InspectStatefulItem,
                $"Inspect {context.ItemDescriber.FormatStatefulItem(item)}",
                new InspectStatefulItemActionRequest(item.Id)
            );
        }

        foreach (var item in state.StatefulItems.Equipped())
        {
            yield return new AvailableAction(
                GameActionKind.InspectStatefulItem,
                $"Inspect {context.ItemDescriber.FormatStatefulItem(item)}",
                new InspectStatefulItemActionRequest(item.Id)
            );
        }

        foreach (var stack in state.Player.Inventory.Items)
        {
            var itemName = context.ItemDescriber.GetItemName(stack.ItemId);
            yield return new AvailableAction(
                GameActionKind.InspectItem,
                $"Inspect {itemName}",
                new InspectItemActionRequest(stack.ItemId)
            );
        }

        foreach (var slot in state.Player.Equipment.Slots)
        {
            if (!state.Player.Equipment.TryGetEquippedItem(slot.Id, out var equippedItem))
            {
                continue;
            }

            var itemName = context.ItemDescriber.GetItemName(equippedItem.ItemId);
            yield return new AvailableAction(
                GameActionKind.InspectItem,
                $"Inspect {itemName}",
                new InspectItemActionRequest(equippedItem.ItemId)
            );
        }
    }

    public GameActionResult Handle(GameActionRequest request, GameActionContext context)
    {
        return request switch
        {
            InspectItemActionRequest inspect => InspectItem(context, inspect.ItemId),
            InspectStatefulItemActionRequest inspectStateful => InspectStatefulItem(context, inspectStateful.ItemId),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private static GameActionResult InspectItem(GameActionContext context, ItemId itemId)
    {
        var state = context.State;
        if (state.Player.Inventory.CountOf(itemId) < 1 && !state.Player.Equipment.ContainsItem(itemId))
        {
            return GameActionResult.Failure("That item is not available.");
        }

        return GameActionResult.Success(
            GameActionPipeline.InspectItemTickCost,
            context.ItemDescriber.DescribeStackItem(itemId, state)
        );
    }

    private static GameActionResult InspectStatefulItem(GameActionContext context, StatefulItemId itemId)
    {
        var item = context.State.StatefulItems.Get(itemId);
        var messages = new List<string>
        {
            context.ItemDescriber.DescribeStatefulItem(item, context.State.StatefulItems)
        };

        if (item.Contents.Count == 0)
        {
            messages.Add("Contents: empty.");
        }
        else
        {
            var contents = item.Contents
                .Select(id => context.State.StatefulItems.TryGet(id, out var content)
                    ? context.ItemDescriber.FormatStatefulItem(content)
                    : id.ToString());
            messages.Add($"Contents: {string.Join(", ", contents)}.");
        }

        return GameActionResult.Success(0, messages.ToArray());
    }
}

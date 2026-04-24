namespace SurvivalGame.Domain;

public enum GameActionKind
{
    Wait,
    Move,
    Pickup
}

public abstract record GameActionRequest(GameActionKind Kind);

public sealed record WaitActionRequest() : GameActionRequest(GameActionKind.Wait);

public sealed record MoveActionRequest(GridOffset Direction) : GameActionRequest(GameActionKind.Move);

public sealed record PickupActionRequest() : GameActionRequest(GameActionKind.Pickup);

public sealed record AvailableAction(GameActionKind Kind, string Label);

public sealed record GameActionResult(bool Succeeded, bool AdvancedTurn, IReadOnlyList<string> Messages)
{
    public static GameActionResult Success(bool advancedTurn, params string[] messages)
    {
        return new GameActionResult(true, advancedTurn, messages);
    }

    public static GameActionResult Failure(params string[] messages)
    {
        return new GameActionResult(false, false, messages);
    }
}

public sealed class GameActionPipeline
{
    private readonly ItemCatalog _itemCatalog;
    private readonly WorldObjectCatalog? _worldObjectCatalog;

    public GameActionPipeline(ItemCatalog itemCatalog, WorldObjectCatalog? worldObjectCatalog = null)
    {
        ArgumentNullException.ThrowIfNull(itemCatalog);
        _itemCatalog = itemCatalog;
        _worldObjectCatalog = worldObjectCatalog;
    }

    public IReadOnlyList<AvailableAction> GetAvailableActions(PrototypeGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var actions = new List<AvailableAction>
        {
            new(GameActionKind.Wait, "Wait")
        };

        if (state.World.GroundItems.ItemsAt(state.Player.Position).Count > 0)
        {
            actions.Add(new AvailableAction(GameActionKind.Pickup, "Pick Up"));
        }

        return actions;
    }

    public GameActionResult Execute(PrototypeGameState state, GameActionRequest request)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);

        return request switch
        {
            WaitActionRequest => Wait(state),
            MoveActionRequest move => Move(state, move.Direction),
            PickupActionRequest => Pickup(state),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private static GameActionResult Wait(PrototypeGameState state)
    {
        state.Turn.Advance();
        return GameActionResult.Success(advancedTurn: true, "Waited.");
    }

    private GameActionResult Move(PrototypeGameState state, GridOffset direction)
    {
        if (direction == GridOffset.Zero)
        {
            return GameActionResult.Failure("No movement direction selected.");
        }

        var nextPosition = state.Player.Position + direction;
        if (!state.World.Map.Contains(nextPosition))
        {
            return GameActionResult.Failure("Cannot move there.");
        }

        if (IsBlockedByWorldObject(state, nextPosition, out var blockerName))
        {
            return GameActionResult.Failure($"Blocked by {blockerName}.");
        }

        state.SetPlayerPosition(nextPosition);
        state.Turn.Advance();
        return GameActionResult.Success(advancedTurn: true, $"Moved to {nextPosition.X}, {nextPosition.Y}.");
    }

    private bool IsBlockedByWorldObject(PrototypeGameState state, GridPosition position, out string blockerName)
    {
        blockerName = "something";

        if (!state.World.WorldObjects.TryGetObjectAt(position, out var objectId))
        {
            return false;
        }

        if (_worldObjectCatalog is null || !_worldObjectCatalog.TryGet(objectId, out var worldObject))
        {
            blockerName = objectId.ToString();
            return true;
        }

        blockerName = worldObject.Name;
        return worldObject.BlocksMovement;
    }

    private GameActionResult Pickup(PrototypeGameState state)
    {
        var itemStacks = state.World.GroundItems.TakeAllAt(state.Player.Position);
        if (itemStacks.Count == 0)
        {
            return GameActionResult.Failure("There is nothing to pick up.");
        }

        foreach (var stack in itemStacks)
        {
            state.Player.Inventory.Add(stack.ItemId, stack.Quantity);
        }

        state.Turn.Advance();
        var messages = itemStacks
            .Select(stack => $"Picked up {FormatStack(stack)}.")
            .ToArray();

        return GameActionResult.Success(advancedTurn: true, messages);
    }

    private string FormatStack(GroundItemStack stack)
    {
        var itemName = _itemCatalog.TryGet(stack.ItemId, out var item)
            ? item.Name
            : stack.ItemId.ToString();

        return stack.Quantity == 1 ? itemName : $"{itemName} x{stack.Quantity}";
    }
}

namespace SurvivalGame.Domain;

public sealed class MovementHandler : IActionHandler
{
    public IReadOnlyList<GameActionKind> HandledKinds { get; } = new[]
    {
        GameActionKind.Wait,
        GameActionKind.Move
    };

    public IEnumerable<AvailableAction> GetAvailableActions(GameActionContext context)
    {
        yield return new AvailableAction(GameActionKind.Wait, "Wait", new WaitActionRequest());
    }

    public GameActionResult Handle(GameActionRequest request, GameActionContext context)
    {
        return request switch
        {
            WaitActionRequest => Wait(context.State),
            MoveActionRequest move => Move(context, move.Direction),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private static GameActionResult Wait(PrototypeGameState state)
    {
        state.AdvanceTime(GameActionPipeline.WaitTickCost);
        return GameActionResult.Success(
            GameActionPipeline.WaitTickCost,
            $"You wait. Time +{GameActionPipeline.WaitTickCost}."
        );
    }

    private static GameActionResult Move(GameActionContext context, GridOffset direction)
    {
        var state = context.State;
        if (direction == GridOffset.Zero)
        {
            return GameActionResult.Failure("No movement direction selected.");
        }

        var nextPosition = state.Player.Position + direction;
        if (!state.LocalMap.Map.Contains(nextPosition))
        {
            return GameActionResult.Failure("Cannot move there.");
        }

        if (IsBlockedByWorldObject(context, nextPosition, out var blockerName))
        {
            return GameActionResult.Failure($"Blocked by {blockerName}.");
        }

        if (IsBlockedByNpc(state, nextPosition, out var npcName))
        {
            return GameActionResult.Failure($"Blocked by {npcName}.");
        }

        state.SetPlayerPosition(nextPosition);
        state.AdvanceTime(GameActionPipeline.MoveTickCost);
        return GameActionResult.Success(
            GameActionPipeline.MoveTickCost,
            $"Moved to {nextPosition.X}, {nextPosition.Y}. Time +{GameActionPipeline.MoveTickCost}."
        );
    }

    private static bool IsBlockedByWorldObject(GameActionContext context, GridPosition position, out string blockerName)
    {
        blockerName = "something";

        if (!context.State.LocalMap.WorldObjects.TryGetObjectAt(position, out var objectId))
        {
            return false;
        }

        if (context.WorldObjectCatalog is null || !context.WorldObjectCatalog.TryGet(objectId, out var worldObject))
        {
            blockerName = objectId.ToString();
            return true;
        }

        blockerName = worldObject.Name;
        return worldObject.BlocksMovement;
    }

    private static bool IsBlockedByNpc(PrototypeGameState state, GridPosition position, out string npcName)
    {
        if (state.LocalMap.Npcs.TryGetAt(position, out var npc) && npc.BlocksMovement)
        {
            npcName = npc.Name;
            return true;
        }

        npcName = string.Empty;
        return false;
    }
}

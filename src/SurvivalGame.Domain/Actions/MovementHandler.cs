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
        if (context.LocalMapQuery.TryGetMovementBlocker(state.Player.Position, nextPosition, out var blocker))
        {
            return blocker.Kind == LocalMapBlockerKind.OutOfBounds
                ? GameActionResult.Failure("Cannot move there.")
                : GameActionResult.Failure($"Blocked by {blocker.Name}.");
        }

        state.SetPlayerPosition(nextPosition);
        state.AdvanceTime(GameActionPipeline.MoveTickCost);
        return GameActionResult.Success(
            GameActionPipeline.MoveTickCost,
            $"Moved to {nextPosition.X}, {nextPosition.Y}. Time +{GameActionPipeline.MoveTickCost}."
        );
    }
}

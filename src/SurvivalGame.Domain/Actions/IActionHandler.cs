namespace SurvivalGame.Domain;

public interface IActionHandler
{
    IReadOnlyList<GameActionKind> HandledKinds { get; }

    IEnumerable<AvailableAction> GetAvailableActions(GameActionContext context);

    GameActionResult Handle(GameActionRequest request, GameActionContext context);
}

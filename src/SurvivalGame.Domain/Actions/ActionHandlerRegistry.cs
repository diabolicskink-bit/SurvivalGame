namespace SurvivalGame.Domain;

public sealed class ActionHandlerRegistry
{
    private readonly Dictionary<GameActionKind, IActionHandler> _handlersByKind = new();
    private readonly IReadOnlyList<IActionHandler> _handlers;

    public ActionHandlerRegistry(IEnumerable<IActionHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        var orderedHandlers = new List<IActionHandler>();
        foreach (var handler in handlers)
        {
            ArgumentNullException.ThrowIfNull(handler);
            orderedHandlers.Add(handler);

            foreach (var kind in handler.HandledKinds)
            {
                if (!_handlersByKind.TryAdd(kind, handler))
                {
                    throw new InvalidOperationException($"Action kind '{kind}' has more than one handler.");
                }
            }
        }

        foreach (var kind in Enum.GetValues<GameActionKind>())
        {
            if (!_handlersByKind.ContainsKey(kind))
            {
                throw new InvalidOperationException($"Action kind '{kind}' has no handler.");
            }
        }

        _handlers = orderedHandlers;
    }

    public IReadOnlyList<IActionHandler> Handlers => _handlers;

    public bool TryGetHandler(GameActionKind kind, out IActionHandler handler)
    {
        if (_handlersByKind.TryGetValue(kind, out var foundHandler))
        {
            handler = foundHandler;
            return true;
        }

        handler = null!;
        return false;
    }
}

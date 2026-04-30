namespace SurvivalGame.Domain;

public sealed class NpcTurnService
{
    private static readonly GridOffset[] WanderDirections =
    [
        GridOffset.Up,
        GridOffset.Right,
        GridOffset.Down,
        GridOffset.Left
    ];

    private readonly IRandomSource _randomSource;

    public NpcTurnService(IRandomSource? randomSource = null)
    {
        _randomSource = randomSource ?? new SystemRandomSource();
    }

    public GameActionResult ResolveNpcTurns(GameActionContext context, GameActionResult result)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.Succeeded || result.ElapsedTicks <= 0 || context.NpcCatalog is null)
        {
            return result;
        }

        var messages = result.Messages.ToList();
        foreach (var npc in context.State.LocalMap.Npcs.AllNpcs.OrderBy(npc => npc.Id.Value, StringComparer.Ordinal))
        {
            if (!ShouldWander(context, npc) || !TryChooseWanderDestination(context, npc, out var destination))
            {
                continue;
            }

            context.State.LocalMap.Npcs.Move(npc.Id, destination);
            messages.Add($"{npc.Name} moves to {destination.X}, {destination.Y}.");
        }

        return new GameActionResult(result.Succeeded, result.ElapsedTicks, messages);
    }

    private static bool ShouldWander(GameActionContext context, NpcState npc)
    {
        return !npc.IsDisabled
            && context.NpcCatalog is not null
            && context.NpcCatalog.TryGet(npc.DefinitionId, out var definition)
            && definition.Behavior.Kind == NpcBehaviorKind.Wander;
    }

    private bool TryChooseWanderDestination(GameActionContext context, NpcState npc, out GridPosition destination)
    {
        var startIndex = GetWanderStartIndex();
        for (var attempt = 0; attempt < WanderDirections.Length; attempt++)
        {
            var direction = WanderDirections[(startIndex + attempt) % WanderDirections.Length];
            var candidate = npc.Position + direction;
            if (candidate == context.State.Player.Position
                || context.State.LocalMap.Npcs.TryGetAt(candidate, out _)
                || context.LocalMapQuery.TryGetMovementBlocker(npc.Position, candidate, out _))
            {
                continue;
            }

            destination = candidate;
            return true;
        }

        destination = default;
        return false;
    }

    private int GetWanderStartIndex()
    {
        var roll = _randomSource.NextUnitDouble();
        return Math.Clamp((int)(roll * WanderDirections.Length), 0, WanderDirections.Length - 1);
    }
}

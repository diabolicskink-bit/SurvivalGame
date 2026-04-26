namespace SurvivalGame.Domain;

public sealed class NpcCombatService
{
    private const string AutomatedHazardTag = "automated_hazard";

    public GameActionResult ResolveAutomatedFire(
        GameActionContext context,
        int startingElapsedTicks,
        GameActionResult result)
    {
        if (!result.Succeeded || result.ElapsedTicks <= 0)
        {
            return result;
        }

        var crossedIntervals = CountCrossedTurretIntervals(startingElapsedTicks, context.State.Time.ElapsedTicks);
        if (crossedIntervals <= 0)
        {
            return result;
        }

        var automatedHazards = context.State.LocalMap.Npcs.AllNpcs
            .Where(npc => !npc.IsDisabled
                && context.NpcCatalog is not null
                && context.NpcCatalog.TryGet(npc.DefinitionId, out var npcDef)
                && npcDef.Behavior.HasTag(AutomatedHazardTag)
                && TileDistance(npc.Position, context.State.Player.Position) <= GetHazardRange(npcDef))
            .ToArray();
        if (automatedHazards.Length == 0)
        {
            return result;
        }

        var messages = result.Messages.ToList();
        foreach (var npc in automatedHazards)
        {
            for (var shot = 0; shot < crossedIntervals; shot++)
            {
                var dealtDamage = context.State.Player.Vitals.TakeDamage(GameActionPipeline.AutomatedTurretDamage);
                messages.Add(
                    $"{npc.Name} at {npc.Position.X}, {npc.Position.Y} hits you for {dealtDamage} damage. "
                    + $"Health: {context.State.Player.Vitals.Health.Current}/{context.State.Player.Vitals.Health.Maximum}."
                );
            }
        }

        return new GameActionResult(result.Succeeded, result.ElapsedTicks, messages);
    }

    private static int GetHazardRange(NpcDefinition npcDef)
    {
        return npcDef.Behavior.PerceptionRange > 0
            ? npcDef.Behavior.PerceptionRange
            : GameActionPipeline.AutomatedTurretRangeTiles;
    }

    private static int CountCrossedTurretIntervals(int startingElapsedTicks, int endingElapsedTicks)
    {
        if (endingElapsedTicks <= startingElapsedTicks)
        {
            return 0;
        }

        return (endingElapsedTicks / GameActionPipeline.AutomatedTurretTickInterval)
            - (startingElapsedTicks / GameActionPipeline.AutomatedTurretTickInterval);
    }

    private static int TileDistance(GridPosition from, GridPosition to)
    {
        return Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y));
    }
}

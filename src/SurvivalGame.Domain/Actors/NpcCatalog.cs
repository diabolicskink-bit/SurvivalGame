namespace SurvivalGame.Domain;

public sealed class NpcCatalog
{
    private readonly Dictionary<NpcDefinitionId, NpcDefinition> _npcs = new();

    public IReadOnlyCollection<NpcDefinition> Definitions => _npcs.Values.ToArray();

    public void Add(NpcDefinition npc)
    {
        ArgumentNullException.ThrowIfNull(npc);

        if (!_npcs.TryAdd(npc.Id, npc))
        {
            throw new InvalidOperationException($"NPC definition '{npc.Id}' is already defined.");
        }
    }

    public bool TryGet(NpcDefinitionId id, out NpcDefinition npc)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (_npcs.TryGetValue(id, out var foundNpc))
        {
            npc = foundNpc;
            return true;
        }

        npc = null!;
        return false;
    }

    public NpcDefinition Get(NpcDefinitionId id)
    {
        if (TryGet(id, out var npc))
        {
            return npc;
        }

        throw new KeyNotFoundException($"NPC definition '{id}' is not defined.");
    }
}

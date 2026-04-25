namespace SurvivalGame.Domain;

public sealed class NpcRoster
{
    private readonly Dictionary<NpcId, NpcState> _npcs = new();
    private readonly Dictionary<GridPosition, NpcId> _npcIdsByPosition = new();

    public IReadOnlyCollection<NpcState> AllNpcs => _npcs.Values.ToArray();

    public void Add(NpcState npc)
    {
        ArgumentNullException.ThrowIfNull(npc);

        if (_npcs.ContainsKey(npc.Id))
        {
            throw new InvalidOperationException($"NPC '{npc.Id}' is already present.");
        }

        if (_npcIdsByPosition.ContainsKey(npc.Position))
        {
            throw new InvalidOperationException($"An NPC is already placed at {npc.Position.X}, {npc.Position.Y}.");
        }

        _npcs.Add(npc.Id, npc);
        _npcIdsByPosition.Add(npc.Position, npc.Id);
    }

    public bool TryGet(NpcId id, out NpcState npc)
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

    public bool TryGetAt(GridPosition position, out NpcState npc)
    {
        if (_npcIdsByPosition.TryGetValue(position, out var npcId) && _npcs.TryGetValue(npcId, out var foundNpc))
        {
            npc = foundNpc;
            return true;
        }

        npc = null!;
        return false;
    }

    public void Move(NpcId id, GridPosition position)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!_npcs.TryGetValue(id, out var npc))
        {
            throw new KeyNotFoundException($"NPC '{id}' is not present.");
        }

        if (_npcIdsByPosition.TryGetValue(position, out var occupantId) && occupantId != id)
        {
            throw new InvalidOperationException($"An NPC is already placed at {position.X}, {position.Y}.");
        }

        _npcIdsByPosition.Remove(npc.Position);
        npc.SetPosition(position);
        _npcIdsByPosition[position] = id;
    }
}

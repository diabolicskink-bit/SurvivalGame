namespace SurvivalGame.Domain;

public sealed class StructureCatalog
{
    private readonly Dictionary<StructureId, StructureDefinition> _definitions = new();

    public IReadOnlyCollection<StructureDefinition> All => _definitions.Values.ToArray();

    public void Add(StructureDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!_definitions.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException($"Structure '{definition.Id}' is already defined.");
        }
    }

    public bool TryGet(StructureId id, out StructureDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _definitions.TryGetValue(id, out definition!);
    }

    public StructureDefinition Get(StructureId id)
    {
        return TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Structure '{id}' is not defined.");
    }
}

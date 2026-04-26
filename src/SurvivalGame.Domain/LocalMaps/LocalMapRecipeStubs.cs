namespace SurvivalGame.Domain;

public sealed record LocalMapRecipeDefinition(
    string Id,
    IReadOnlyList<LocalMapRecipeStepDefinition> Steps,
    IReadOnlyList<LocalMapStampReference>? Stamps = null
);

public sealed record LocalMapRecipeStepDefinition(
    string Kind,
    IReadOnlyDictionary<string, string>? Parameters = null
);

public sealed record LocalMapStampReference(
    string Id,
    GridPosition Position
);

public sealed class LocalMapRecipeSource
{
    public PrototypeLocalSite Build()
    {
        throw new NotSupportedException("Recipe map generation is not implemented yet.");
    }
}

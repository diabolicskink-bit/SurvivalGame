using SurvivalGame.Domain;

namespace SurvivalGame.Application;

public sealed class LocalSiteSession
{
    public LocalSiteSession(
        LocalSiteState localSite,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog,
        TileSurfaceCatalog surfaceCatalog,
        WorldObjectCatalog worldObjectCatalog,
        StructureCatalog structureCatalog,
        NpcCatalog npcCatalog,
        GameActionPipeline actionPipeline,
        WorldObjectInstanceId? activeTravelAnchorInstanceId = null
    )
    {
        LocalSite = localSite;
        GameState = localSite.GameState;
        SiteDisplayName = localSite.DisplayName;
        EntryPosition = localSite.EntryPosition;
        ItemCatalog = itemCatalog;
        FirearmCatalog = firearmCatalog;
        SurfaceCatalog = surfaceCatalog;
        WorldObjectCatalog = worldObjectCatalog;
        StructureCatalog = structureCatalog;
        NpcCatalog = npcCatalog;
        ActionPipeline = actionPipeline;
        ActiveTravelAnchorInstanceId = activeTravelAnchorInstanceId;
    }

    public LocalSiteState LocalSite { get; }

    public PrototypeGameState GameState { get; }

    public string SiteDisplayName { get; }

    public GridPosition EntryPosition { get; }

    public ItemCatalog ItemCatalog { get; }

    public FirearmCatalog FirearmCatalog { get; }

    public TileSurfaceCatalog SurfaceCatalog { get; }

    public WorldObjectCatalog WorldObjectCatalog { get; }

    public StructureCatalog StructureCatalog { get; }

    public NpcCatalog NpcCatalog { get; }

    public GameActionPipeline ActionPipeline { get; }

    public WorldObjectInstanceId? ActiveTravelAnchorInstanceId { get; }
}

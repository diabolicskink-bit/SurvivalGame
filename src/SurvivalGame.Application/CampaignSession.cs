using SurvivalGame.Domain;

namespace SurvivalGame.Application;

public sealed class CampaignSession
{
    public CampaignSession(
        CampaignState campaignState,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog,
        TileSurfaceCatalog surfaceCatalog,
        WorldObjectCatalog worldObjectCatalog,
        StructureCatalog structureCatalog,
        NpcCatalog npcCatalog,
        GameActionPipeline actionPipeline
    )
    {
        CampaignState = campaignState;
        ItemCatalog = itemCatalog;
        FirearmCatalog = firearmCatalog;
        SurfaceCatalog = surfaceCatalog;
        WorldObjectCatalog = worldObjectCatalog;
        StructureCatalog = structureCatalog;
        NpcCatalog = npcCatalog;
        ActionPipeline = actionPipeline;
    }

    public CampaignState CampaignState { get; }

    public ItemCatalog ItemCatalog { get; }

    public FirearmCatalog FirearmCatalog { get; }

    public TileSurfaceCatalog SurfaceCatalog { get; }

    public WorldObjectCatalog WorldObjectCatalog { get; }

    public StructureCatalog StructureCatalog { get; }

    public NpcCatalog NpcCatalog { get; }

    public GameActionPipeline ActionPipeline { get; }

    public void ReturnToWorldMap()
    {
        CampaignState.ReturnToWorldMap();
    }

    public LocalSiteState EnterLocalSite(WorldMapPointOfInterest pointOfInterest)
    {
        ArgumentNullException.ThrowIfNull(pointOfInterest);

        var requestedSiteId = new SiteId(pointOfInterest.LocalSiteId ?? pointOfInterest.Id);
        var siteId = CampaignState.ContainsLocalSite(requestedSiteId)
            ? requestedSiteId
            : PrototypeLocalSites.DefaultSiteId;

        return CampaignState.EnterLocalSite(siteId);
    }

    public LocalSiteSession CreateLocalSiteSession(SiteId siteId)
    {
        var localSite = CampaignState.GetLocalSite(siteId);
        var activeAnchor = TravelAnchorService.EnsureAnchor(
            localSite,
            CampaignState.WorldMap.CurrentTravelMethod,
            WorldObjectCatalog
        );
        if (activeAnchor is not null)
        {
            localSite.GameState.SetActiveTravelAnchor(activeAnchor.Value.InstanceId);
            if (TravelAnchorService.TryFindEntryPosition(
                localSite.GameState,
                activeAnchor.Value.InstanceId,
                WorldObjectCatalog,
                out var entryPosition))
            {
                localSite.GameState.SetPlayerPosition(entryPosition);
            }
        }
        else
        {
            localSite.GameState.ClearActiveTravelAnchor();
        }

        return new LocalSiteSession(
            localSite,
            ItemCatalog,
            FirearmCatalog,
            SurfaceCatalog,
            WorldObjectCatalog,
            StructureCatalog,
            NpcCatalog,
            ActionPipeline,
            activeAnchor?.InstanceId
        );
    }
}

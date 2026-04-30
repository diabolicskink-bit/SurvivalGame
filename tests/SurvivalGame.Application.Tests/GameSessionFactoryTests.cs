using SurvivalGame.Application;
using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Application.Tests;

public sealed class GameSessionFactoryTests
{
    [Fact]
    public void CreatesCampaignSessionFromFilesystemDataPaths()
    {
        var session = CreateCampaignSession();

        Assert.Equal(CampaignMode.WorldMap, session.CampaignState.Mode);
        Assert.Same(session.CampaignState.VehicleFuel, session.CampaignState.WorldMap.VehicleFuelState);
        Assert.NotNull(session.ItemCatalog);
        Assert.NotNull(session.FirearmCatalog);
        Assert.NotNull(session.SurfaceCatalog);
        Assert.NotNull(session.WorldObjectCatalog);
        Assert.NotNull(session.NpcCatalog);
        Assert.NotNull(session.ActionPipeline);
    }

    [Fact]
    public void RegistersCommittedLocalSites()
    {
        var session = CreateCampaignSession();

        Assert.True(session.CampaignState.ContainsLocalSite(PrototypeLocalSites.DefaultSiteId));
        Assert.True(session.CampaignState.ContainsLocalSite(PrototypeLocalSites.GasStationSiteId));
        Assert.True(session.CampaignState.ContainsLocalSite(PrototypeLocalSites.FarmsteadSiteId));
    }

    [Fact]
    public void SeedsStartingInventoryAndStatefulItems()
    {
        var session = CreateCampaignSession();
        var gameState = session.CampaignState.GetLocalSite(PrototypeLocalSites.DefaultSiteId).GameState;

        Assert.Equal(3, gameState.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Equal(2, gameState.Player.Inventory.CountOf(PrototypeItems.Branch));
        Assert.Equal(35, gameState.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
        Assert.Equal(60, gameState.Player.Inventory.CountOf(PrototypeFirearms.Ammo556Standard));
        Assert.Contains(gameState.StatefulItems.InPlayerInventory(), item => item.ItemId == PrototypeFirearms.Pistol9mm);
        Assert.Contains(gameState.StatefulItems.InPlayerInventory(), item => item.ItemId == PrototypeItems.FuelCan);
        Assert.Contains(
            gameState.StatefulItems.OnGround(new GridPosition(11, 7), PrototypeLocalSites.DefaultSiteId),
            item => item.ItemId == PrototypeFirearms.Magazine9mmStandard && item.FeedDevice?.LoadedCount == 8
        );
    }

    [Fact]
    public void StandaloneLocalSiteSessionEntersDefaultSite()
    {
        var session = GameSessionFactory.CreateStandaloneLocalSiteSession(CreateContentPaths());

        Assert.Equal(PrototypeLocalSites.DefaultSiteId, session.LocalSite.Id);
        Assert.Equal(PrototypeLocalSites.DefaultSiteId, session.GameState.SiteId);
        Assert.Equal(session.LocalSite.EntryPosition, session.GameState.PlayerPosition);
    }

    [Fact]
    public void UnknownPointOfInterestLocalSiteFallsBackToDefaultSite()
    {
        var session = CreateCampaignSession();
        var pointOfInterest = new WorldMapPointOfInterest(
            "unknown_test_poi",
            "Unknown Test POI",
            new WorldMapPosition(0, 0),
            1,
            localSiteId: "missing_local_site"
        );

        var localSite = session.EnterLocalSite(pointOfInterest);

        Assert.Equal(PrototypeLocalSites.DefaultSiteId, localSite.Id);
        Assert.Equal(CampaignMode.LocalSite, session.CampaignState.Mode);
        Assert.Equal(PrototypeLocalSites.DefaultSiteId, session.CampaignState.ActiveLocalSiteId);
    }

    private static CampaignSession CreateCampaignSession()
    {
        return GameSessionFactory.CreateCampaignSession(CreateContentPaths());
    }

    private static GameContentPaths CreateContentPaths()
    {
        return GameContentPaths.FromDataRoot(GetDataRoot());
    }

    private static string GetDataRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var dataRoot = Path.Combine(directory.FullName, "data");
            if (Directory.Exists(Path.Combine(dataRoot, "items"))
                && Directory.Exists(Path.Combine(dataRoot, "local_maps")))
            {
                return dataRoot;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate data root from the test output directory.");
    }
}

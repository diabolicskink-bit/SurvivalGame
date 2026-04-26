namespace SurvivalGame.Domain;

public sealed class CampaignState
{
    private readonly Dictionary<SiteId, LocalSiteState> _localSites = new();

    public CampaignState(
        WorldTime time,
        PlayerState player,
        StatefulItemStore statefulItems,
        WorldMapTravelState worldMap,
        VehicleFuelState vehicleFuel)
    {
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(statefulItems);
        ArgumentNullException.ThrowIfNull(worldMap);
        ArgumentNullException.ThrowIfNull(vehicleFuel);

        if (!ReferenceEquals(worldMap.VehicleFuelState, vehicleFuel))
        {
            throw new ArgumentException("World Map travel state must use the campaign vehicle fuel state.", nameof(worldMap));
        }

        Time = time;
        Player = player;
        StatefulItems = statefulItems;
        WorldMap = worldMap;
        VehicleFuel = vehicleFuel;
        Mode = CampaignMode.WorldMap;
    }

    public WorldTime Time { get; }

    public PlayerState Player { get; }

    public StatefulItemStore StatefulItems { get; }

    public WorldMapTravelState WorldMap { get; }

    public VehicleFuelState VehicleFuel { get; }

    public CampaignMode Mode { get; private set; }

    public SiteId? ActiveLocalSiteId { get; private set; }

    public IReadOnlyDictionary<SiteId, LocalSiteState> LocalSites => _localSites;

    public LocalSiteState AddLocalSite(LocalSiteState localSite)
    {
        ArgumentNullException.ThrowIfNull(localSite);
        EnsureLocalSiteUsesCampaignState(localSite);

        if (!_localSites.TryAdd(localSite.Id, localSite))
        {
            throw new InvalidOperationException($"Local site '{localSite.Id}' is already registered.");
        }

        return localSite;
    }

    public bool ContainsLocalSite(SiteId siteId)
    {
        ArgumentNullException.ThrowIfNull(siteId);
        return _localSites.ContainsKey(siteId);
    }

    public LocalSiteState GetLocalSite(SiteId siteId)
    {
        ArgumentNullException.ThrowIfNull(siteId);
        if (_localSites.TryGetValue(siteId, out var localSite))
        {
            return localSite;
        }

        throw new KeyNotFoundException($"Local site '{siteId}' is not registered in this campaign.");
    }

    public LocalSiteState EnterLocalSite(SiteId siteId)
    {
        StoreActiveLocalSitePlayerPosition();

        var localSite = GetLocalSite(siteId);

        WorldMap.ClearDestination();
        localSite.GameState.SetPlayerPosition(localSite.LastPlayerPosition);
        Mode = CampaignMode.LocalSite;
        ActiveLocalSiteId = localSite.Id;

        return localSite;
    }

    public void ReturnToWorldMap()
    {
        StoreActiveLocalSitePlayerPosition();
        Mode = CampaignMode.WorldMap;
        ActiveLocalSiteId = null;
    }

    private void StoreActiveLocalSitePlayerPosition()
    {
        if (Mode != CampaignMode.LocalSite || ActiveLocalSiteId is null)
        {
            return;
        }

        GetLocalSite(ActiveLocalSiteId).StorePlayerPosition(Player.Position);
    }

    private void EnsureLocalSiteUsesCampaignState(LocalSiteState localSite)
    {
        if (!ReferenceEquals(localSite.GameState.Time, Time))
        {
            throw new ArgumentException("Local site must share the campaign world time.", nameof(localSite));
        }

        if (!ReferenceEquals(localSite.GameState.Player, Player))
        {
            throw new ArgumentException("Local site must share the campaign player state.", nameof(localSite));
        }

        if (!ReferenceEquals(localSite.GameState.StatefulItems, StatefulItems))
        {
            throw new ArgumentException("Local site must share the campaign stateful item store.", nameof(localSite));
        }
    }
}

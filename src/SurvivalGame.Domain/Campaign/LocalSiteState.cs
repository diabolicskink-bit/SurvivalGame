namespace SurvivalGame.Domain;

public sealed class LocalSiteState
{
    private readonly IReadOnlyDictionary<TravelMethodId, TravelAnchorPlacement> _arrivalAnchors;

    public LocalSiteState(
        PrototypeGameState gameState,
        string displayName,
        GridPosition entryPosition,
        IReadOnlyDictionary<TravelMethodId, TravelAnchorPlacement>? arrivalAnchors = null)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Local site display name is required.", nameof(displayName));
        }

        if (!gameState.LocalMap.Map.Contains(entryPosition))
        {
            throw new ArgumentOutOfRangeException(nameof(entryPosition), "Entry position must be inside the local site map.");
        }

        GameState = gameState;
        DisplayName = displayName.Trim();
        EntryPosition = entryPosition;
        LastPlayerPosition = entryPosition;
        _arrivalAnchors = arrivalAnchors is null
            ? new Dictionary<TravelMethodId, TravelAnchorPlacement>()
            : new Dictionary<TravelMethodId, TravelAnchorPlacement>(arrivalAnchors);
    }

    public SiteId Id => GameState.SiteId;

    public string DisplayName { get; }

    public GridPosition EntryPosition { get; }

    public GridPosition LastPlayerPosition { get; private set; }

    public PrototypeGameState GameState { get; }

    public IReadOnlyDictionary<TravelMethodId, TravelAnchorPlacement> ArrivalAnchors => _arrivalAnchors;

    public bool TryGetArrivalAnchor(TravelMethodId travelMethod, out TravelAnchorPlacement anchor)
    {
        return _arrivalAnchors.TryGetValue(travelMethod, out anchor!);
    }

    internal void StorePlayerPosition(GridPosition position)
    {
        if (!GameState.LocalMap.Map.Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Player position must be inside the local site map.");
        }

        LastPlayerPosition = position;
    }
}

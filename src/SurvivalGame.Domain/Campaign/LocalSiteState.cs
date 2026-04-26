namespace SurvivalGame.Domain;

public sealed class LocalSiteState
{
    public LocalSiteState(PrototypeGameState gameState, string displayName, GridPosition entryPosition)
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
    }

    public SiteId Id => GameState.SiteId;

    public string DisplayName { get; }

    public GridPosition EntryPosition { get; }

    public GridPosition LastPlayerPosition { get; private set; }

    public PrototypeGameState GameState { get; }

    internal void StorePlayerPosition(GridPosition position)
    {
        if (!GameState.LocalMap.Map.Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Player position must be inside the local site map.");
        }

        LastPlayerPosition = position;
    }
}

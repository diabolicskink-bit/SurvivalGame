namespace SurvivalGame.Domain;

public static class TravelAnchorService
{
    private static readonly GridOffset[] CardinalOffsets =
    [
        GridOffset.Up,
        GridOffset.Down,
        GridOffset.Left,
        GridOffset.Right
    ];

    public static PlacedWorldObject? EnsureAnchor(
        LocalSiteState localSite,
        TravelMethodId travelMethod,
        WorldObjectCatalog worldObjects)
    {
        ArgumentNullException.ThrowIfNull(localSite);
        ArgumentNullException.ThrowIfNull(worldObjects);

        if (!localSite.TryGetArrivalAnchor(travelMethod, out var anchor)
            || !TravelAnchorRules.TryGetObjectId(travelMethod, out var objectId))
        {
            return null;
        }

        var instanceId = TravelAnchorRules.CreateInstanceId(travelMethod);
        if (localSite.GameState.WorldObjects.TryGetPlacement(instanceId, out var existing))
        {
            return existing;
        }

        var definition = worldObjects.Get(objectId);
        localSite.GameState.WorldObjects.Place(
            anchor.Position,
            objectId,
            anchor.Facing,
            definition.Footprint,
            localSite.GameState.MapBounds,
            instanceId
        );

        return localSite.GameState.WorldObjects.TryGetPlacement(instanceId, out var placed)
            ? placed
            : null;
    }

    public static bool IsPlayerNearAnchor(PrototypeGameState state, WorldObjectInstanceId? anchorInstanceId)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (anchorInstanceId is null
            || !state.WorldObjects.TryGetPlacement(anchorInstanceId, out var anchor))
        {
            return false;
        }

        var query = new LocalMapQuery(state.LocalMap);
        return query.IsNearPlacement(state.Player.Position, anchor);
    }

    public static bool TryFindEntryPosition(
        PrototypeGameState state,
        WorldObjectInstanceId anchorInstanceId,
        WorldObjectCatalog worldObjects,
        out GridPosition position)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(worldObjects);

        if (!state.WorldObjects.TryGetPlacement(anchorInstanceId, out var anchor))
        {
            position = default;
            return false;
        }

        var query = new LocalMapQuery(state.LocalMap, worldObjects);
        if (!query.TryGetStandBlocker(state.Player.Position, out _)
            && query.IsNearPlacement(state.Player.Position, anchor))
        {
            position = state.Player.Position;
            return true;
        }

        var occupied = anchor.OccupiedPositions().ToArray();
        var candidates = occupied
            .SelectMany(occupiedPosition => CardinalOffsets
                .Select(offset => occupiedPosition + offset))
            .Distinct()
            .Where(candidate => !query.TryGetStandBlocker(candidate, out _))
            .OrderBy(candidate => ManhattanDistance(candidate, state.Player.Position))
            .ThenBy(candidate => candidate.Y)
            .ThenBy(candidate => candidate.X)
            .ToArray();

        if (candidates.Length == 0)
        {
            position = default;
            return false;
        }

        position = candidates[0];
        return true;
    }

    private static int ManhattanDistance(GridPosition a, GridPosition b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}

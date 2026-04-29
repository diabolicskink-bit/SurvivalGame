namespace SurvivalGame.Domain;

public static class TravelAnchorService
{
    private static readonly GridOffset[] NearbyOffsets =
    [
        GridOffset.Zero,
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

        return IsPositionNearPlacement(state.Player.Position, anchor);
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

        if (IsWalkable(state, state.Player.Position, worldObjects)
            && IsPositionNearPlacement(state.Player.Position, anchor))
        {
            position = state.Player.Position;
            return true;
        }

        var occupied = anchor.OccupiedPositions().ToArray();
        var candidates = occupied
            .SelectMany(occupiedPosition => NearbyOffsets
                .Where(offset => offset != GridOffset.Zero)
                .Select(offset => occupiedPosition + offset))
            .Distinct()
            .Where(candidate => IsWalkable(state, candidate, worldObjects))
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

    private static bool IsPositionNearPlacement(GridPosition position, PlacedWorldObject placement)
    {
        foreach (var occupiedPosition in placement.OccupiedPositions())
        {
            if (ManhattanDistance(position, occupiedPosition) <= 1)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsWalkable(
        PrototypeGameState state,
        GridPosition position,
        WorldObjectCatalog worldObjects)
    {
        if (!state.LocalMap.Map.Contains(position))
        {
            return false;
        }

        if (state.WorldObjects.TryGetObjectAt(position, out var objectId)
            && (!worldObjects.TryGet(objectId, out var definition) || definition.BlocksMovement))
        {
            return false;
        }

        return !state.Npcs.TryGetAt(position, out var npc) || !npc.BlocksMovement;
    }

    private static int ManhattanDistance(GridPosition a, GridPosition b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}

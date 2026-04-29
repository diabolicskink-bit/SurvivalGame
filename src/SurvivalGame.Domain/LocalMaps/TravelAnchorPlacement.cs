namespace SurvivalGame.Domain;

public sealed record TravelAnchorPlacement(
    TravelMethodId TravelMethod,
    GridPosition Position,
    WorldObjectFacing Facing);

public static class TravelAnchorRules
{
    public static readonly WorldObjectId PlayerVehicleObjectId = new("player_vehicle");
    public static readonly WorldObjectId PlayerPushbikeObjectId = new("player_pushbike");

    public static bool TryGetObjectId(TravelMethodId travelMethod, out WorldObjectId objectId)
    {
        objectId = travelMethod switch
        {
            TravelMethodId.Vehicle => PlayerVehicleObjectId,
            TravelMethodId.Pushbike => PlayerPushbikeObjectId,
            _ => null!
        };

        return objectId is not null;
    }

    public static WorldObjectInstanceId CreateInstanceId(TravelMethodId travelMethod)
    {
        return new WorldObjectInstanceId(travelMethod switch
        {
            TravelMethodId.Vehicle => "arrival_anchor_vehicle",
            TravelMethodId.Pushbike => "arrival_anchor_pushbike",
            _ => throw new ArgumentException($"Travel method '{travelMethod}' does not use a local anchor.", nameof(travelMethod))
        });
    }

    public static bool IsAnchoredTravelMethod(TravelMethodId travelMethod)
    {
        return travelMethod is TravelMethodId.Vehicle or TravelMethodId.Pushbike;
    }
}

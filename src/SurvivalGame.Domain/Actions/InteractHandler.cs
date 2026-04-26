namespace SurvivalGame.Domain;

public sealed class InteractHandler : IActionHandler
{
    public IReadOnlyList<GameActionKind> HandledKinds { get; } = new[]
    {
        GameActionKind.RefuelVehicle
    };

    public IEnumerable<AvailableAction> GetAvailableActions(GameActionContext context)
    {
        if (CanRefuelVehicle(context))
        {
            yield return new AvailableAction(
                GameActionKind.RefuelVehicle,
                "Refuel Vehicle",
                new RefuelVehicleActionRequest()
            );
        }
    }

    public GameActionResult Handle(GameActionRequest request, GameActionContext context)
    {
        return request switch
        {
            RefuelVehicleActionRequest => RefuelVehicle(context),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private static GameActionResult RefuelVehicle(GameActionContext context)
    {
        if (context.VehicleFuelState is null)
        {
            return GameActionResult.Failure("No vehicle fuel state is available.");
        }

        if (context.VehicleFuelState.IsFull)
        {
            return GameActionResult.Failure("Vehicle fuel is already full.");
        }

        if (!IsAdjacentToFuelPump(context))
        {
            return GameActionResult.Failure("You need to stand next to a fuel pump.");
        }

        context.VehicleFuelState.Refill();
        context.State.AdvanceTime(GameActionPipeline.RefuelVehicleTickCost);

        return GameActionResult.Success(
            GameActionPipeline.RefuelVehicleTickCost,
            $"Refueled vehicle to {context.VehicleFuelState.CurrentFuel:0.0}/{context.VehicleFuelState.Capacity:0.0}. Time +{GameActionPipeline.RefuelVehicleTickCost}."
        );
    }

    private static bool CanRefuelVehicle(GameActionContext context)
    {
        return context.VehicleFuelState is not null
            && !context.VehicleFuelState.IsFull
            && IsAdjacentToFuelPump(context);
    }

    private static bool IsAdjacentToFuelPump(GameActionContext context)
    {
        var offsets = new[]
        {
            GridOffset.Up,
            GridOffset.Down,
            GridOffset.Left,
            GridOffset.Right
        };

        return offsets.Any(offset =>
        {
            var position = context.State.Player.Position + offset;
            return context.State.LocalMap.Map.Contains(position)
                && context.State.LocalMap.WorldObjects.TryGetObjectAt(position, out var objectId)
                && context.WorldObjectCatalog is not null
                && context.WorldObjectCatalog.TryGet(objectId, out var worldObjectDef)
                && worldObjectDef.HasTag("refuel_source");
        });
    }
}

namespace SurvivalGame.Domain;

public sealed class GameActionPipeline
{
    public const int MoveTickCost = 100;
    public const int WaitTickCost = 100;
    public const int PickupTickCost = 50;
    public const int InspectItemTickCost = 0;
    public const int DropItemTickCost = 0;
    public const int EquipItemTickCost = 0;
    public const int UnequipItemTickCost = 0;
    public const int ShootTickCost = 100;
    public const int BurstShootTickCost = 150;
    public const int RefuelVehicleTickCost = 100;
    public const int AutomatedTurretRangeTiles = 5;
    public const int AutomatedTurretTickInterval = 75;
    public const int AutomatedTurretDamage = 10;

    private readonly ItemCatalog _itemCatalog;
    private readonly WorldObjectCatalog? _worldObjectCatalog;
    private readonly NpcCatalog? _npcCatalog;
    private readonly FirearmActionService? _firearmActions;
    private readonly VehicleFuelState? _vehicleFuelState;
    private readonly TravelCargoStore? _travelCargo;
    private readonly ItemDescriber _itemDescriber;
    private readonly ActionHandlerRegistry _registry;
    private readonly NpcTurnService _npcTurnService;
    private readonly NpcCombatService _npcCombatService = new();

    public GameActionPipeline(
        ItemCatalog itemCatalog,
        WorldObjectCatalog? worldObjectCatalog = null,
        FirearmCatalog? firearmCatalog = null,
        VehicleFuelState? vehicleFuelState = null,
        NpcCatalog? npcCatalog = null,
        TravelCargoStore? travelCargo = null,
        IRandomSource? randomSource = null
    )
    {
        ArgumentNullException.ThrowIfNull(itemCatalog);

        _itemCatalog = itemCatalog;
        _worldObjectCatalog = worldObjectCatalog;
        _npcCatalog = npcCatalog;
        _firearmActions = firearmCatalog is null
            ? null
            : new FirearmActionService(firearmCatalog, itemCatalog, worldObjectCatalog, randomSource);
        _vehicleFuelState = vehicleFuelState;
        _travelCargo = travelCargo;
        _itemDescriber = new ItemDescriber(itemCatalog, firearmCatalog);
        _npcTurnService = new NpcTurnService(randomSource);
        _registry = new ActionHandlerRegistry(new IActionHandler[]
        {
            new MovementHandler(),
            new InventoryHandler(),
            new TravelCargoHandler(),
            new InteractHandler(),
            new InspectHandler(),
            new EquipmentHandler(),
            new FirearmHandler()
        });
    }

    public IReadOnlyList<AvailableAction> GetAvailableActions(PrototypeGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var context = CreateContext(state);
        return _registry.Handlers
            .SelectMany(handler => handler.GetAvailableActions(context))
            .ToArray();
    }

    public GameActionResult Execute(GameActionRequest request, PrototypeGameState state)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(state);

        if (!_registry.TryGetHandler(request.Kind, out var handler))
        {
            throw new InvalidOperationException($"No action handler is registered for '{request.Kind}'.");
        }

        var startingElapsedTicks = state.Time.ElapsedTicks;
        var context = CreateContext(state);
        var result = handler.Handle(request, context);
        result = _npcTurnService.ResolveNpcTurns(context, result);
        return _npcCombatService.ResolveAutomatedFire(context, startingElapsedTicks, result);
    }

    private GameActionContext CreateContext(PrototypeGameState state)
    {
        return new GameActionContext(
            state,
            _itemCatalog,
            _worldObjectCatalog,
            _npcCatalog,
            _firearmActions,
            _vehicleFuelState,
            _travelCargo,
            _itemDescriber
        );
    }
}

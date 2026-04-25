using Godot;
using SurvivalGame.Domain;

public partial class GameSessionShell : Control
{
    private const string MainMenuScenePath = "res://src/Godot/MainMenu/MainMenu.tscn";
    private const string OverworldScenePath = "res://src/Godot/Overworld/OverworldScreen.tscn";
    private const string GameShellScenePath = "res://src/Godot/Game/GameShell.tscn";

    private PrototypeGameplaySession _gameplaySession = null!;
    private PrototypeGameplaySession? _gasStationSession;
    private OverworldTravelState _overworldState = null!;
    private VehicleFuelState _vehicleFuelState = null!;
    private Control? _currentScreen;

    public override void _Ready()
    {
        SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _vehicleFuelState = new VehicleFuelState(
            PrototypeTravelMethods.VehicleFuelCapacity,
            PrototypeTravelMethods.VehicleStartingFuel
        );
        _gameplaySession = PrototypeSessionFactory.CreateGameplaySession(_vehicleFuelState);
        _overworldState = new OverworldTravelState(
            PrototypeOverworldSites.MapWidth,
            PrototypeOverworldSites.MapHeight,
            PrototypeOverworldSites.StartPosition,
            TravelMethodId.Walking,
            _vehicleFuelState
        );

        ShowOverworld();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_currentScreen is not OverworldScreen
            || @event is not InputEventKey keyEvent
            || !keyEvent.Pressed
            || keyEvent.Echo
            || keyEvent.Keycode != Key.Escape)
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        GetTree().ChangeSceneToFile(MainMenuScenePath);
    }

    private void ShowOverworld()
    {
        ClearCurrentScreen();

        var scene = ResourceLoader.Load<PackedScene>(OverworldScenePath);
        var overworld = scene.Instantiate<OverworldScreen>();
        overworld.Configure(_overworldState, _gameplaySession.GameState.Time);
        overworld.EnterSiteRequested += OnEnterSiteRequested;
        AddChild(overworld);
        _currentScreen = overworld;
    }

    private void OnEnterSiteRequested(OverworldPointOfInterest site)
    {
        var session = site.Id == PrototypeLocalSites.GasStationSiteId
            ? GetGasStationSession()
            : _gameplaySession;
        ShowLocalSite(session);
    }

    private PrototypeGameplaySession GetGasStationSession()
    {
        _gasStationSession ??= PrototypeSessionFactory.CreateGasStationSession(_gameplaySession, _vehicleFuelState);
        return _gasStationSession;
    }

    private void ShowLocalSite(PrototypeGameplaySession session)
    {
        ClearCurrentScreen();

        var scene = ResourceLoader.Load<PackedScene>(GameShellScenePath);
        var localGame = scene.Instantiate<GameShell>();
        session.GameState.SetPlayerPosition(session.EntryPosition);
        localGame.Session = session;
        localGame.ShowsReturnToOverworld = true;
        localGame.ReturnToOverworldRequested += ShowOverworld;
        AddChild(localGame);
        _currentScreen = localGame;
    }

    private void ClearCurrentScreen()
    {
        if (_currentScreen is null)
        {
            return;
        }

        RemoveChild(_currentScreen);
        _currentScreen.QueueFree();
        _currentScreen = null;
    }
}

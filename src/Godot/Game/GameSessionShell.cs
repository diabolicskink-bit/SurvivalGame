using Godot;
using SurvivalGame.Domain;

public partial class GameSessionShell : Control
{
    private const string MainMenuScenePath = "res://src/Godot/MainMenu/MainMenu.tscn";
    private const string WorldMapScenePath = "res://src/Godot/WorldMap/WorldMapScreen.tscn";
    private const string GameShellScenePath = "res://src/Godot/Game/GameShell.tscn";

    private PrototypeCampaignSession _campaignSession = null!;
    private Control? _currentScreen;

    public override void _Ready()
    {
        SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _campaignSession = PrototypeSessionFactory.CreateCampaignSession();

        ShowWorldMap();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_currentScreen is not WorldMapScreen
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

    private void ShowWorldMap()
    {
        ClearCurrentScreen();
        _campaignSession.CampaignState.ReturnToWorldMap();

        var scene = ResourceLoader.Load<PackedScene>(WorldMapScenePath);
        var worldMap = scene.Instantiate<WorldMapScreen>();
        worldMap.Configure(_campaignSession.CampaignState.WorldMap, _campaignSession.CampaignState.Time);
        worldMap.EnterSiteRequested += OnEnterSiteRequested;
        AddChild(worldMap);
        _currentScreen = worldMap;
    }

    private void OnEnterSiteRequested(WorldMapPointOfInterest site)
    {
        var requestedSiteId = new SiteId(site.LocalSiteId ?? site.Id);
        var siteId = _campaignSession.CampaignState.ContainsLocalSite(requestedSiteId)
            ? requestedSiteId
            : PrototypeLocalSites.DefaultSiteId;
        var localSite = _campaignSession.CampaignState.EnterLocalSite(siteId);
        ShowLocalSite(localSite.Id);
    }

    private void ShowLocalSite(SiteId siteId)
    {
        ClearCurrentScreen();

        var scene = ResourceLoader.Load<PackedScene>(GameShellScenePath);
        var localGame = scene.Instantiate<GameShell>();
        localGame.Session = _campaignSession.CreateGameplaySession(siteId);
        localGame.ShowsReturnToWorldMap = true;
        localGame.ReturnToWorldMapRequested += ShowWorldMap;
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

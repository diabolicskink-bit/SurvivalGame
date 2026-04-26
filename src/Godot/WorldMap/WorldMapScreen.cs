using System;
using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

public partial class WorldMapScreen : Control
{
    private const int LayoutMargin = 24;
    private const int StatusFontSize = 18;
    private const int SectionTitleFontSize = 17;

    private readonly Dictionary<TravelMethodId, Button> _travelMethodButtons = new();
    private WorldMapTravelState? _travelState;
    private WorldTime? _time;
    private WorldMapView _mapView = null!;
    private Label _timeLabel = null!;
    private Label _methodLabel = null!;
    private Label _fuelLabel = null!;
    private Label _nearbySiteLabel = null!;
    private Button _enterSiteButton = null!;
    private MessageLog _messageLog = null!;

    public event Action<WorldMapPointOfInterest>? EnterSiteRequested;

    public void Configure(WorldMapTravelState travelState, WorldTime time)
    {
        _travelState = travelState;
        _time = time;

        if (_mapView is not null)
        {
            ApplyStateToControls();
        }
    }

    public override void _Ready()
    {
        SetAnchorsPreset(Control.LayoutPreset.FullRect);
        BuildScreen();

        _travelState ??= new WorldMapTravelState(
            PrototypeWorldMapSites.MapWidth,
            PrototypeWorldMapSites.MapHeight,
            PrototypeWorldMapSites.StartPosition,
            TravelMethodId.Walking,
            PrototypeTravelMethods.VehicleStartingFuel
        );
        _time ??= new WorldTime();

        ApplyStateToControls();
        UpdateOverlay();
        _messageLog.AddMessage("World Map travel started.");
    }

    public override void _Process(double delta)
    {
        if (_travelState is null || _time is null)
        {
            return;
        }

        var method = PrototypeTravelMethods.Get(_travelState.CurrentTravelMethod);
        var result = _travelState.Advance(delta, _time, method, PrototypeWorldMapSites.Definition);
        if (result.Moved || result.Messages.Count > 0)
        {
            foreach (var message in result.Messages)
            {
                _messageLog.AddMessage(message);
            }

            _mapView.QueueRedraw();
            UpdateOverlay();
        }
    }

    private void BuildScreen()
    {
        _mapView = new WorldMapView
        {
            Name = "WorldMapView"
        };
        _mapView.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _mapView.DestinationSelected += OnDestinationSelected;
        AddChild(_mapView);

        var uiLayer = new CanvasLayer
        {
            Name = "UI"
        };
        AddChild(uiLayer);

        var panel = new PanelContainer
        {
            Name = "TravelPanel",
            CustomMinimumSize = new Vector2(350, 0),
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 0,
            AnchorBottom = 0,
            OffsetLeft = LayoutMargin,
            OffsetTop = LayoutMargin,
            OffsetRight = LayoutMargin + 360,
            OffsetBottom = 470
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        uiLayer.AddChild(panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        panel.AddChild(margin);

        var stack = new VBoxContainer();
        stack.AddThemeConstantOverride("separation", 9);
        margin.AddChild(stack);

        stack.AddChild(CreateSectionTitle("World Map"));
        _timeLabel = CreateStatusLabel();
        _methodLabel = CreateStatusLabel();
        _fuelLabel = CreateStatusLabel();
        _nearbySiteLabel = CreateStatusLabel();
        stack.AddChild(_timeLabel);
        stack.AddChild(_methodLabel);
        stack.AddChild(_fuelLabel);
        stack.AddChild(_nearbySiteLabel);

        stack.AddChild(new HSeparator());
        stack.AddChild(CreateSectionTitle("Travel Method"));

        var methodRow = new HBoxContainer();
        methodRow.AddThemeConstantOverride("separation", 6);
        stack.AddChild(methodRow);

        foreach (var method in PrototypeTravelMethods.All)
        {
            var button = new Button
            {
                Text = method.DisplayName,
                ToggleMode = true,
                CustomMinimumSize = new Vector2(94, 38)
            };
            button.Pressed += () => OnTravelMethodPressed(method.Id);
            methodRow.AddChild(button);
            _travelMethodButtons[method.Id] = button;
        }

        _enterSiteButton = new Button
        {
            Text = "Enter Site",
            Disabled = true,
            CustomMinimumSize = new Vector2(0, 40)
        };
        _enterSiteButton.Pressed += OnEnterSitePressed;
        stack.AddChild(_enterSiteButton);

        stack.AddChild(new HSeparator());
        stack.AddChild(CreateSectionTitle("Log"));

        _messageLog = new MessageLog
        {
            Name = "MessageLog"
        };
        stack.AddChild(_messageLog);
    }

    private void ApplyStateToControls()
    {
        if (_travelState is null)
        {
            return;
        }

        _mapView.Configure(_travelState, PrototypeWorldMapSites.Definition);
        UpdateOverlay();
    }

    private void OnDestinationSelected(WorldMapPosition destination)
    {
        if (_travelState is null)
        {
            return;
        }

        _travelState.SetDestination(destination);
        var method = PrototypeTravelMethods.Get(_travelState.CurrentTravelMethod);
        if (method.UsesFuel && _travelState.VehicleFuel <= 0)
        {
            _messageLog.AddMessage("Vehicle fuel is empty. Select walking or pushbike to continue.");
        }
        else
        {
            _messageLog.AddMessage("Destination set.");
        }

        _mapView.QueueRedraw();
        UpdateOverlay();
    }

    private void OnTravelMethodPressed(TravelMethodId travelMethod)
    {
        if (_travelState is null)
        {
            return;
        }

        _travelState.SetTravelMethod(travelMethod);
        var method = PrototypeTravelMethods.Get(travelMethod);
        _messageLog.AddMessage($"Travel method: {method.DisplayName}.");
        UpdateOverlay();
    }

    private void OnEnterSitePressed()
    {
        if (_travelState is null)
        {
            return;
        }

        var nearbySite = _travelState.FindNearbySite(PrototypeWorldMapSites.All);
        if (nearbySite is null)
        {
            return;
        }

        EnterSiteRequested?.Invoke(nearbySite);
    }

    private void UpdateOverlay()
    {
        if (_travelState is null || _time is null)
        {
            return;
        }

        var method = PrototypeTravelMethods.Get(_travelState.CurrentTravelMethod);
        _timeLabel.Text = $"Time: {_time.ElapsedTicks} ticks";
        _methodLabel.Text = $"Travel: {method.DisplayName}";
        _fuelLabel.Visible = method.UsesFuel;
        _fuelLabel.Text = $"Fuel: {_travelState.VehicleFuel:0.0}";

        var nearbySite = _travelState.FindNearbySite(PrototypeWorldMapSites.All);
        _nearbySiteLabel.Text = nearbySite is null
            ? "Nearby: none"
            : $"Nearby: {nearbySite.DisplayName}";
        _enterSiteButton.Disabled = nearbySite is null;
        _enterSiteButton.Text = nearbySite is null
            ? "Enter Site"
            : $"Enter {nearbySite.DisplayName}";

        foreach (var (id, button) in _travelMethodButtons)
        {
            button.ButtonPressed = id == _travelState.CurrentTravelMethod;
        }
    }

    private static Label CreateStatusLabel()
    {
        var label = new Label();
        label.AddThemeFontSizeOverride("font_size", StatusFontSize);
        label.AddThemeColorOverride("font_color", new Color(0.88f, 0.91f, 0.86f));
        return label;
    }

    private static Label CreateSectionTitle(string text)
    {
        var label = new Label
        {
            Text = text
        };
        label.AddThemeFontSizeOverride("font_size", SectionTitleFontSize);
        label.AddThemeColorOverride("font_color", new Color(0.83f, 0.87f, 0.82f));
        return label;
    }

    private static StyleBoxFlat CreatePanelStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.072f, 0.085f, 0.095f, 0.94f),
            BorderColor = new Color(0.2f, 0.31f, 0.29f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusBottomLeft = 8
        };
    }
}

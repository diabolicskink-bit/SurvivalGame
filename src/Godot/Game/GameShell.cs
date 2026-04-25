using Godot;
using SurvivalGame.Domain;

public partial class GameShell : Control
{
    private const string MainMenuScenePath = "res://src/Godot/MainMenu/MainMenu.tscn";
    private const int BaseCellSize = 32;
    private const int MinimumCellSize = 18;
    private const float WorldRegionWidthRatio = 0.5f;
    private const float WorldRegionHeightRatio = 2.0f / 3.0f;
    private const int LayoutMargin = 24;
    private const int MinimumBoardMargin = 16;
    private const float SidePanelWidth = 346.0f;
    private const int StatusFontSize = 18;
    private const int SectionTitleFontSize = 17;
    private static readonly GridBounds MapBounds = new(19, 13);

    private ItemCatalog _itemCatalog = null!;
    private FirearmCatalog _firearmCatalog = null!;
    private TileSurfaceCatalog _surfaceCatalog = null!;
    private WorldObjectCatalog _worldObjectCatalog = null!;
    private GameActionPipeline _actionPipeline = null!;
    private PrototypeGameState _gameState = null!;
    private Node2D _board = null!;
    private GridView _gridView = null!;
    private WorldObjectLayer _worldObjectLayer = null!;
    private GroundItemLayer _groundItemLayer = null!;
    private Node2D _playerMarker = null!;
    private PlayerController _playerController = null!;
    private PanelContainer _sidePanel = null!;
    private PanelContainer _logPanel = null!;
    private ActionPanel _actionPanel = null!;
    private PlayerStatusPanel _statusPanel = null!;
    private EquipmentPanel _equipmentPanel = null!;
    private FirearmPanel _firearmPanel = null!;
    private InventoryPanel _inventoryPanel = null!;
    private ItemTooltip _itemTooltip = null!;
    private MessageLog _messageLog = null!;
    private Label _turnLabel = null!;
    private Label _positionLabel = null!;
    private Label _surfaceLabel = null!;
    private Label _modeLabel = null!;
    private GridPosition? _visibleTooltipPosition;
    private int _cellSize = BaseCellSize;

    public override void _Ready()
    {
        _board = GetNode<Node2D>("Board");
        _gridView = GetNode<GridView>("Board/GridView");
        _worldObjectLayer = GetNode<WorldObjectLayer>("Board/WorldObjectLayer");
        _groundItemLayer = GetNode<GroundItemLayer>("Board/GroundItemLayer");
        _playerMarker = GetNode<Node2D>("Board/PlayerMarker");
        _playerController = GetNode<PlayerController>("Board/PlayerController");

        _itemCatalog = LoadItemCatalog();
        _firearmCatalog = LoadFirearmCatalog();
        _surfaceCatalog = LoadSurfaceCatalog();
        _worldObjectCatalog = LoadWorldObjectCatalog();
        _actionPipeline = new GameActionPipeline(_itemCatalog, _worldObjectCatalog, _firearmCatalog);
        _gameState = new PrototypeGameState(
            MapBounds,
            CreatePrototypeGroundItems(),
            CreatePrototypeSurfaceMap(),
            CreatePrototypeWorldObjects(),
            MapBounds.Center
        );
        AddPrototypeStartingItems();

        _gridView.Configure(_gameState.World.Map.Surfaces, _surfaceCatalog, _cellSize);
        _worldObjectLayer.Configure(_gameState.World.WorldObjects, _worldObjectCatalog, _cellSize);
        _groundItemLayer.Configure(_gameState.World.GroundItems, _itemCatalog, _cellSize, _gameState.StatefulItems);
        _playerController.MoveRequested += OnMoveRequested;
        UpdatePlayerMarker();

        BuildOverlay();
        UpdateResponsiveLayout();
        UpdateOverlay();
        _messageLog.AddMessage("New run started.");
    }

    public override void _Process(double delta)
    {
        UpdateItemTooltip();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        if (keyEvent.Keycode == Key.Escape)
        {
            GetViewport().SetInputAsHandled();
            GetTree().ChangeSceneToFile(MainMenuScenePath);
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized && _sidePanel is not null)
        {
            UpdateResponsiveLayout();
        }
    }

    private void OnMoveRequested(GridOffset direction)
    {
        ExecuteAction(new MoveActionRequest(direction));
    }

    private void BuildOverlay()
    {
        var uiLayer = GetNode<CanvasLayer>("UI");

        _sidePanel = new PanelContainer
        {
            AnchorLeft = 1.0f,
            AnchorTop = 0.0f,
            AnchorRight = 1.0f,
            AnchorBottom = 0.0f,
            OffsetLeft = -(SidePanelWidth + LayoutMargin),
            OffsetTop = LayoutMargin,
            OffsetRight = -LayoutMargin,
            OffsetBottom = 460.0f
        };
        _sidePanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        uiLayer.AddChild(_sidePanel);

        var scroll = new ScrollContainer();
        _sidePanel.AddChild(scroll);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        scroll.AddChild(margin);

        var stack = new VBoxContainer();
        stack.AddThemeConstantOverride("separation", 8);
        margin.AddChild(stack);

        _modeLabel = CreateStatusLabel();
        _turnLabel = CreateStatusLabel();
        _positionLabel = CreateStatusLabel();
        _surfaceLabel = CreateStatusLabel();

        stack.AddChild(_modeLabel);
        stack.AddChild(_turnLabel);
        stack.AddChild(_positionLabel);
        stack.AddChild(_surfaceLabel);

        var separator = new HSeparator();
        stack.AddChild(separator);

        stack.AddChild(CreateSectionTitle("Actions"));

        _actionPanel = new ActionPanel
        {
            Name = "ActionPanel"
        };
        _actionPanel.ActionSelected += OnActionSelected;
        stack.AddChild(_actionPanel);

        stack.AddChild(new HSeparator());

        stack.AddChild(CreateSectionTitle("Status"));

        _statusPanel = new PlayerStatusPanel
        {
            Name = "PlayerStatusPanel"
        };
        stack.AddChild(_statusPanel);

        stack.AddChild(new HSeparator());

        stack.AddChild(CreateSectionTitle("Equipment"));

        _equipmentPanel = new EquipmentPanel
        {
            Name = "EquipmentPanel"
        };
        stack.AddChild(_equipmentPanel);

        stack.AddChild(new HSeparator());

        stack.AddChild(CreateSectionTitle("Firearms"));

        _firearmPanel = new FirearmPanel
        {
            Name = "FirearmPanel"
        };
        stack.AddChild(_firearmPanel);

        stack.AddChild(new HSeparator());

        stack.AddChild(CreateSectionTitle("Inventory"));

        _inventoryPanel = new InventoryPanel
        {
            Name = "InventoryPanel"
        };
        stack.AddChild(_inventoryPanel);

        _logPanel = new PanelContainer
        {
            Name = "LogPanel",
            AnchorLeft = 0.0f,
            AnchorTop = 0.0f,
            AnchorRight = 0.0f,
            AnchorBottom = 0.0f
        };
        _logPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        uiLayer.AddChild(_logPanel);

        var logMargin = new MarginContainer();
        logMargin.AddThemeConstantOverride("margin_left", 18);
        logMargin.AddThemeConstantOverride("margin_top", 14);
        logMargin.AddThemeConstantOverride("margin_right", 18);
        logMargin.AddThemeConstantOverride("margin_bottom", 14);
        _logPanel.AddChild(logMargin);

        var logStack = new VBoxContainer();
        logStack.AddThemeConstantOverride("separation", 8);
        logMargin.AddChild(logStack);

        logStack.AddChild(CreateSectionTitle("Log"));

        _messageLog = new MessageLog
        {
            Name = "MessageLog"
        };
        logStack.AddChild(_messageLog);

        _itemTooltip = new ItemTooltip
        {
            Name = "ItemTooltip"
        };
        uiLayer.AddChild(_itemTooltip);
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

    private void UpdateOverlay()
    {
        var position = _gameState.Player.Position;
        _modeLabel.Text = "Mode: Prototype Shell";
        _turnLabel.Text = $"Turn: {_gameState.Turn.CurrentTurn}";
        _positionLabel.Text = $"Position: {position.X}, {position.Y}";
        _surfaceLabel.Text = $"Surface: {GetSurfaceAt(position).Name}";
        _actionPanel.Display(_actionPipeline.GetAvailableActions(_gameState));
        _statusPanel.Display(_gameState.Player.Vitals);
        _equipmentPanel.Display(_gameState.Player.Equipment, _itemCatalog, _gameState.StatefulItems);
        _firearmPanel.Display(_gameState.Player, _firearmCatalog, _gameState.StatefulItems);
        _inventoryPanel.Display(_gameState.Player.Inventory, _itemCatalog, _gameState.StatefulItems);
    }

    private void AddPrototypeStartingItems()
    {
        _gameState.Player.Inventory.Add(PrototypeItems.Stone, 3);
        _gameState.Player.Inventory.Add(PrototypeItems.Branch, 2);
        _gameState.Player.Inventory.Add(PrototypeItems.WaterBottle);
        _gameState.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 35);
        _gameState.Player.Inventory.Add(PrototypeFirearms.Ammo9mmHollowPoint, 20);
        _gameState.Player.Inventory.Add(PrototypeFirearms.Ammo762x39Standard, 60);
        _gameState.Player.Inventory.Add(PrototypeFirearms.Ammo308Standard, 20);
        _gameState.Player.Inventory.Add(PrototypeFirearms.Ammo12GaugeBuckshot, 20);
        _gameState.Player.Inventory.Add(PrototypeFirearms.Ammo12GaugeSlug, 10);
        _gameState.Player.Inventory.Add(PrototypeFirearms.Ammo22LrStandard, 100);
        AddPrototypeStatefulItems();
    }

    private void AddPrototypeStatefulItems()
    {
        _gameState.StatefulItems.Create(PrototypeFirearms.Pistol9mm, 1, StatefulItemLocation.PlayerInventory(), _firearmCatalog);
        _gameState.StatefulItems.Create(PrototypeItems.Ak47, 1, StatefulItemLocation.PlayerInventory(), _firearmCatalog);
        _gameState.StatefulItems.Create(PrototypeItems.HuntingRifle, 1, StatefulItemLocation.PlayerInventory(), _firearmCatalog);
        _gameState.StatefulItems.Create(PrototypeFirearms.Shotgun12Gauge, 1, StatefulItemLocation.PlayerInventory(), _firearmCatalog);
        _gameState.StatefulItems.Create(PrototypeFirearms.Rifle22, 1, StatefulItemLocation.PlayerInventory(), _firearmCatalog);

        var loadedMagazine = _gameState.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.PlayerInventory(),
            _firearmCatalog
        );
        loadedMagazine.FeedDevice?.Load(_firearmCatalog.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 15);

        _gameState.StatefulItems.Create(PrototypeFirearms.Magazine9mmStandard, 1, StatefulItemLocation.PlayerInventory(), _firearmCatalog);
        _gameState.StatefulItems.Create(PrototypeFirearms.Magazine9mmExtended, 1, StatefulItemLocation.PlayerInventory(), _firearmCatalog);
        _gameState.StatefulItems.Create(PrototypeFirearms.MagazineAk30Round, 1, StatefulItemLocation.PlayerInventory(), _firearmCatalog);
        _gameState.StatefulItems.Create(PrototypeFirearms.MagazineAkDamaged20Round, 1, StatefulItemLocation.PlayerInventory(), _firearmCatalog);

        var droppedMagazine = _gameState.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.Ground(new GridPosition(11, 7)),
            _firearmCatalog
        );
        droppedMagazine.FeedDevice?.Load(_firearmCatalog.GetAmmunition(PrototypeFirearms.Ammo9mmHollowPoint), 8);

        var backpack = _gameState.StatefulItems.Create(
            new ItemId("school_backpack"),
            1,
            StatefulItemLocation.PlayerInventory(),
            _firearmCatalog
        );
        var food = _gameState.StatefulItems.Create(
            new ItemId("canned_beans"),
            1,
            StatefulItemLocation.PlayerInventory(),
            _firearmCatalog
        );
        _gameState.StatefulItems.MoveToContained(food.Id, backpack.Id);
    }

    private static TileItemMap CreatePrototypeGroundItems()
    {
        var itemMap = new TileItemMap();

        itemMap.Place(new GridPosition(4, 4), PrototypeItems.Stone, 2);
        itemMap.Place(new GridPosition(7, 9), PrototypeItems.Branch, 3);
        itemMap.Place(new GridPosition(13, 5), PrototypeItems.WaterBottle);
        itemMap.Place(new GridPosition(16, 10), PrototypeItems.Ak47);
        itemMap.Place(new GridPosition(8, 7), PrototypeItems.BaseballCap);
        itemMap.Place(new GridPosition(10, 7), PrototypeItems.RunningShoes);

        return itemMap;
    }

    private static TileSurfaceMap CreatePrototypeSurfaceMap()
    {
        var surfaceMap = new TileSurfaceMap(MapBounds, PrototypeSurfaces.Grass);

        FillRect(surfaceMap, x: 2, y: 2, width: 8, height: 5, PrototypeSurfaces.Concrete);
        FillRect(surfaceMap, x: 3, y: 3, width: 3, height: 3, PrototypeSurfaces.Carpet);
        FillRect(surfaceMap, x: 11, y: 2, width: 5, height: 4, PrototypeSurfaces.Tile);
        FillRect(surfaceMap, x: 12, y: 8, width: 4, height: 3, PrototypeSurfaces.Ice);

        return surfaceMap;
    }

    private static TileObjectMap CreatePrototypeWorldObjects()
    {
        var objectMap = new TileObjectMap();

        PlaceWallLine(objectMap, y: 2, xStart: 2, xEnd: 9);
        objectMap.Place(new GridPosition(2, 3), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(2, 4), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(9, 3), PrototypeWorldObjects.Window);
        objectMap.Place(new GridPosition(9, 4), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(6, 6), PrototypeWorldObjects.WoodenDoor);
        objectMap.Place(new GridPosition(8, 3), PrototypeWorldObjects.Fridge);
        objectMap.Place(new GridPosition(5, 4), PrototypeWorldObjects.Table);
        objectMap.Place(new GridPosition(5, 5), PrototypeWorldObjects.Chair);
        objectMap.Place(new GridPosition(12, 3), PrototypeWorldObjects.Bed);
        objectMap.Place(new GridPosition(15, 5), PrototypeWorldObjects.StorageCrate);
        objectMap.Place(new GridPosition(1, 10), PrototypeWorldObjects.Tree);
        objectMap.Place(new GridPosition(17, 2), PrototypeWorldObjects.Boulder);

        return objectMap;
    }

    private static void PlaceWallLine(TileObjectMap objectMap, int y, int xStart, int xEnd)
    {
        for (var x = xStart; x <= xEnd; x++)
        {
            objectMap.Place(new GridPosition(x, y), PrototypeWorldObjects.Wall);
        }
    }

    private static void FillRect(TileSurfaceMap surfaceMap, int x, int y, int width, int height, SurfaceId surfaceId)
    {
        for (var row = y; row < y + height; row++)
        {
            for (var column = x; column < x + width; column++)
            {
                var position = new GridPosition(column, row);
                if (MapBounds.Contains(position))
                {
                    surfaceMap.SetSurface(position, surfaceId);
                }
            }
        }
    }

    private static ItemCatalog LoadItemCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/items");
        return new ItemDefinitionLoader().LoadDirectory(dataPath);
    }

    private static FirearmCatalog LoadFirearmCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/firearms");
        return new FirearmDefinitionLoader().LoadDirectory(dataPath);
    }

    private static TileSurfaceCatalog LoadSurfaceCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/surfaces");
        return new TileSurfaceDefinitionLoader().LoadDirectory(dataPath);
    }

    private static WorldObjectCatalog LoadWorldObjectCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/world_objects");
        return new WorldObjectDefinitionLoader().LoadDirectory(dataPath);
    }

    private void UpdateItemTooltip()
    {
        var hoveredPosition = GetHoveredGridPosition();
        if (hoveredPosition is null)
        {
            HideItemTooltip();
            return;
        }

        var itemStacks = _gameState.World.GroundItems.ItemsAt(hoveredPosition.Value);
        var statefulItems = _gameState.StatefulItems.OnGround(hoveredPosition.Value);
        var surface = GetSurfaceAt(hoveredPosition.Value);
        var worldObject = GetWorldObjectAt(hoveredPosition.Value);

        var cursorPosition = GetViewport().GetMousePosition();
        if (_visibleTooltipPosition == hoveredPosition)
        {
            _itemTooltip.MoveTo(cursorPosition);
            return;
        }

        _visibleTooltipPosition = hoveredPosition;
        _itemTooltip.Display(hoveredPosition.Value, surface, worldObject, itemStacks, statefulItems, _itemCatalog, cursorPosition);
    }

    private TileSurfaceDefinition GetSurfaceAt(GridPosition position)
    {
        var surfaceId = _gameState.World.Map.Surfaces.GetSurfaceId(position);
        return _surfaceCatalog.Get(surfaceId);
    }

    private WorldObjectDefinition? GetWorldObjectAt(GridPosition position)
    {
        return _gameState.World.WorldObjects.TryGetObjectAt(position, out var objectId)
            ? _worldObjectCatalog.Get(objectId)
            : null;
    }

    private GridPosition? GetHoveredGridPosition()
    {
        var boardPosition = _board.ToLocal(GetGlobalMousePosition());
        var cell = new GridPosition(
            Mathf.FloorToInt(boardPosition.X / _cellSize),
            Mathf.FloorToInt(boardPosition.Y / _cellSize)
        );

        return MapBounds.Contains(cell) ? cell : null;
    }

    private void HideItemTooltip()
    {
        _visibleTooltipPosition = null;
        _itemTooltip.HideTooltip();
    }

    private void UpdateResponsiveLayout()
    {
        var viewportSize = GetViewportRect().Size;
        var panelWidth = Mathf.Min(SidePanelWidth, Mathf.Max(280.0f, viewportSize.X * 0.32f));
        var boardAreaWidth = Mathf.Max(
            MinimumCellSize * MapBounds.Width,
            (viewportSize.X * WorldRegionWidthRatio) - (LayoutMargin * 2.0f)
        );
        var boardAreaHeight = Mathf.Max(
            MinimumCellSize * MapBounds.Height,
            (viewportSize.Y * WorldRegionHeightRatio) - (LayoutMargin * 2.0f)
        );
        var fittedCellSize = Mathf.FloorToInt(Mathf.Min(boardAreaWidth / MapBounds.Width, boardAreaHeight / MapBounds.Height));
        var nextCellSize = Mathf.Max(
            MinimumCellSize,
            fittedCellSize
        );

        if (_cellSize != nextCellSize)
        {
            _cellSize = nextCellSize;
            _gridView.Configure(_gameState.World.Map.Surfaces, _surfaceCatalog, _cellSize);
            _worldObjectLayer.Configure(_gameState.World.WorldObjects, _worldObjectCatalog, _cellSize);
            _groundItemLayer.Configure(_gameState.World.GroundItems, _itemCatalog, _cellSize, _gameState.StatefulItems);
            UpdatePlayerMarker();
        }

        _board.Position = new Vector2(MinimumBoardMargin, MinimumBoardMargin);
        _playerMarker.Scale = Vector2.One * (_cellSize / (float)BaseCellSize);

        _sidePanel.OffsetLeft = -(panelWidth + LayoutMargin);
        _sidePanel.OffsetTop = LayoutMargin;
        _sidePanel.OffsetRight = -LayoutMargin;
        _sidePanel.OffsetBottom = Mathf.Max(460.0f, viewportSize.Y - LayoutMargin);

        var boardPixelSize = new Vector2(MapBounds.Width * _cellSize, MapBounds.Height * _cellSize);
        var logTop = _board.Position.Y + boardPixelSize.Y + 14.0f;
        _logPanel.OffsetLeft = _board.Position.X;
        _logPanel.OffsetTop = logTop;
        _logPanel.OffsetRight = _board.Position.X + boardPixelSize.X;
        _logPanel.OffsetBottom = Mathf.Max(logTop + 128.0f, viewportSize.Y - LayoutMargin);
    }

    private void UpdatePlayerMarker()
    {
        _playerMarker.Position = CellToBoardPosition(_gameState.Player.Position);
    }

    private void OnActionSelected(AvailableAction action)
    {
        GameActionRequest? request = action.Request ?? action.Kind switch
        {
            GameActionKind.Wait => new WaitActionRequest(),
            GameActionKind.Pickup => new PickupActionRequest(),
            _ => null
        };

        if (request is not null)
        {
            ExecuteAction(request);
        }
    }

    private void ExecuteAction(GameActionRequest request)
    {
        var result = _actionPipeline.Execute(_gameState, request);

        UpdatePlayerMarker();
        _worldObjectLayer.QueueRedraw();
        _groundItemLayer.QueueRedraw();
        UpdateOverlay();
        HideItemTooltip();

        foreach (var message in result.Messages)
        {
            _messageLog.AddMessage(message);
        }
    }

    private Vector2 CellToBoardPosition(GridPosition cell)
    {
        return new Vector2(
            (cell.X + 0.5f) * _cellSize,
            (cell.Y + 0.5f) * _cellSize
        );
    }
}

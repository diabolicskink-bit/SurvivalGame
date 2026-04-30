using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SurvivalGame.Domain;

public partial class GameShell : Control
{
    private const string MainMenuScenePath = "res://src/Godot/MainMenu/MainMenu.tscn";
    private const int BaseCellSize = 32;
    private const int DefaultLocalZoomLevelIndex = 2;
    private const int MinimumCellSize = 18;
    private const float WorldRegionWidthRatio = 0.5f;
    private const float WorldRegionHeightRatio = 2.0f / 3.0f;
    private const int LayoutMargin = 24;
    private const int MinimumBoardMargin = 16;
    private const float SidePanelWidth = 346.0f;
    private const float MinimumWidePanelWidth = 620.0f;
    private const float ItemInfoPopupWidth = 380.0f;
    private const float ItemInfoContentWidth = 330.0f;
    private const int StatusFontSize = 18;
    private const int SectionTitleFontSize = 17;

    private static readonly StructureEdgeDirection[] TooltipStructureDirections =
    [
        StructureEdgeDirection.North,
        StructureEdgeDirection.East,
        StructureEdgeDirection.South,
        StructureEdgeDirection.West
    ];

    private static readonly Vector2I[] LocalZoomLevels =
    [
        new(18, 12),
        new(21, 14),
        new(27, 18),
        new(33, 22),
        new(39, 26)
    ];

    private static readonly Vector2I DefaultLocalViewportSize = LocalZoomLevels[DefaultLocalZoomLevelIndex];

    private ItemCatalog _itemCatalog = null!;
    private FirearmCatalog _firearmCatalog = null!;
    private TileSurfaceCatalog _surfaceCatalog = null!;
    private WorldObjectCatalog _worldObjectCatalog = null!;
    private StructureCatalog _structureCatalog = null!;
    private NpcCatalog _npcCatalog = null!;
    private GameActionPipeline _actionPipeline = null!;
    private PrototypeGameState _gameState = null!;
    private Control _board = null!;
    private GridView _gridView = null!;
    private GroundItemLayer _groundItemLayer = null!;
    private MapEntityLayer _mapEntityLayer = null!;
    private PlayerController _playerController = null!;
    private Control _sidePanel = null!;
    private PanelContainer _itemActionPopup = null!;
    private PanelContainer _logPanel = null!;
    private ActionPanel _actionPanel = null!;
    private PlayerStatusPanel _statusPanel = null!;
    private EquipmentPanel _equipmentPanel = null!;
    private InventoryPanel _inventoryPanel = null!;
    private SelectedItemPanel _selectedItemPanel = null!;
    private PanelContainer _itemInfoPopup = null!;
    private SelectedItemPanel _itemInfoPanel = null!;
    private ItemTooltip _itemTooltip = null!;
    private MessageLog _messageLog = null!;
    private Label _timeLabel = null!;
    private Label _positionLabel = null!;
    private Label _surfaceLabel = null!;
    private Label _targetLabel = null!;
    private Label _modeLabel = null!;
    private Button _returnToWorldMapButton = null!;
    private SelectedItemRef? _selectedItem;
    private SelectedItemRef? _hoveredItem;
    private NpcId? _selectedTargetNpcId;
    private GridPosition? _visibleTooltipPosition;
    private GridBounds _mapBounds = new(1, 1);
    private GridViewport _viewport = GridViewport.Create(
        new GridBounds(1, 1),
        new GridPosition(0, 0),
        DefaultLocalViewportSize.X,
        DefaultLocalViewportSize.Y
    );
    private int _cellSize = BaseCellSize;
    private int _localZoomLevelIndex = DefaultLocalZoomLevelIndex;

    private Vector2I CurrentLocalViewportSize => LocalZoomLevels[_localZoomLevelIndex];

    public event Action? ReturnToWorldMapRequested;

    public PrototypeGameplaySession? Session { get; set; }

    public bool ShowsReturnToWorldMap { get; set; }

    public override void _Ready()
    {
        _board = GetNode<Control>("Board");
        _board.ClipContents = true;
        _board.MouseFilter = MouseFilterEnum.Ignore;
        _gridView = GetNode<GridView>("Board/GridView");
        _groundItemLayer = GetNode<GroundItemLayer>("Board/GroundItemLayer");
        _mapEntityLayer = GetNode<MapEntityLayer>("Board/MapEntityLayer");
        _playerController = GetNode<PlayerController>("Board/PlayerController");

        var isStandaloneSession = Session is null;
        var session = Session ?? PrototypeSessionFactory.CreateGameplaySession();
        _itemCatalog = session.ItemCatalog;
        _firearmCatalog = session.FirearmCatalog;
        _surfaceCatalog = session.SurfaceCatalog;
        _worldObjectCatalog = session.WorldObjectCatalog;
        _structureCatalog = session.StructureCatalog;
        _npcCatalog = session.NpcCatalog;
        _actionPipeline = session.ActionPipeline;
        _gameState = session.GameState;
        _mapBounds = _gameState.MapBounds;
        RefreshViewport();

        ConfigureLocalMapView();
        _playerController.MoveRequested += OnMoveRequested;

        BuildOverlay();
        UpdateResponsiveLayout();
        UpdateOverlay();
        _messageLog.AddMessage(isStandaloneSession ? "New run started." : "Entered local site.");
    }

    public override void _Process(double delta)
    {
        UpdateItemInteractionPopups();
        UpdateItemTooltip();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed)
        {
            return;
        }

        if (TryHandleLocalMapZoom(mouseEvent))
        {
            GetViewport().SetInputAsHandled();
            return;
        }

        if (mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (IsPointerOverGameplayUi(mouseEvent.Position))
            {
                return;
            }

            if (TryHandleBoardClick())
            {
                GetViewport().SetInputAsHandled();
            }
        }
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
            if (_selectedItem is not null || IsPointerOverControl(_itemActionPopup, GetViewport().GetMousePosition()))
            {
                DismissSelectedItem();
                return;
            }

            ReturnFromLocalOrExit();
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

    private void ReturnFromLocalOrExit()
    {
        if (ShowsReturnToWorldMap)
        {
            if (!CanReturnToWorldMap())
            {
                _messageLog.AddMessage("Return to your vehicle or pushbike to leave the site.");
                return;
            }

            ReturnToWorldMapRequested?.Invoke();
            return;
        }

        GetTree().ChangeSceneToFile(MainMenuScenePath);
    }

    private bool CanReturnToWorldMap()
    {
        return Session?.ActiveTravelAnchorInstanceId is null
            || TravelAnchorService.IsPlayerNearAnchor(_gameState, Session.ActiveTravelAnchorInstanceId);
    }

    private void BuildOverlay()
    {
        var uiLayer = GetNode<CanvasLayer>("UI");

        _sidePanel = new VBoxContainer
        {
            AnchorLeft = 0.0f,
            AnchorTop = 0.0f,
            AnchorRight = 0.0f,
            AnchorBottom = 0.0f,
            OffsetLeft = LayoutMargin,
            OffsetTop = LayoutMargin,
            OffsetRight = LayoutMargin + SidePanelWidth,
            OffsetBottom = 460.0f
        };
        ((VBoxContainer)_sidePanel).AddThemeConstantOverride("separation", 12);
        uiLayer.AddChild(_sidePanel);

        var topPanelRow = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        topPanelRow.AddThemeConstantOverride("separation", 12);
        _sidePanel.AddChild(topPanelRow);

        var infoPanel = CreateSidebarPanel("InfoPanel");
        infoPanel.CustomMinimumSize = new Vector2(0, 330);
        infoPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        topPanelRow.AddChild(infoPanel);
        var stack = CreatePanelStack(infoPanel);

        _modeLabel = CreateStatusLabel();
        _timeLabel = CreateStatusLabel();
        _positionLabel = CreateStatusLabel();
        _surfaceLabel = CreateStatusLabel();
        _targetLabel = CreateStatusLabel();

        stack.AddChild(_modeLabel);
        stack.AddChild(_timeLabel);
        stack.AddChild(_positionLabel);
        stack.AddChild(_surfaceLabel);
        stack.AddChild(_targetLabel);

        _returnToWorldMapButton = new Button
        {
            Text = "Return to World Map",
            Visible = ShowsReturnToWorldMap,
            CustomMinimumSize = new Vector2(0, 38)
        };
        _returnToWorldMapButton.Pressed += ReturnFromLocalOrExit;
        stack.AddChild(_returnToWorldMapButton);

        var separator = new HSeparator();
        stack.AddChild(separator);

        stack.AddChild(CreateSectionTitle("Global Actions"));

        _actionPanel = new ActionPanel
        {
            Name = "ActionPanel"
        };
        _actionPanel.ActionSelected += OnActionSelected;
        stack.AddChild(_actionPanel);

        stack.AddChild(new HSeparator());

        stack.AddChild(CreateSectionTitle("Vitals"));

        _statusPanel = new PlayerStatusPanel
        {
            Name = "PlayerStatusPanel"
        };
        stack.AddChild(_statusPanel);

        var equipmentContainer = CreateSidebarPanel("EquipmentPanelContainer");
        equipmentContainer.CustomMinimumSize = new Vector2(0, 330);
        equipmentContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        topPanelRow.AddChild(equipmentContainer);
        var equipmentStack = CreatePanelStack(equipmentContainer);
        equipmentStack.AddChild(CreateSectionTitle("Equipment"));

        _equipmentPanel = new EquipmentPanel
        {
            Name = "EquipmentPanel"
        };
        _equipmentPanel.ItemActionRequested += OnItemActionRequested;
        _equipmentPanel.ItemHovered += OnItemHovered;
        _equipmentPanel.ItemHoverEnded += OnItemHoverEnded;
        equipmentStack.AddChild(_equipmentPanel);

        var inventoryContainer = CreateSidebarPanel("InventoryPanelContainer");
        inventoryContainer.CustomMinimumSize = new Vector2(0, 260);
        inventoryContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        inventoryContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _sidePanel.AddChild(inventoryContainer);
        var inventoryMargin = CreatePanelMargin();
        inventoryContainer.AddChild(inventoryMargin);

        var inventoryStack = new VBoxContainer();
        inventoryStack.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        inventoryStack.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        inventoryStack.AddThemeConstantOverride("separation", 8);
        inventoryMargin.AddChild(inventoryStack);
        inventoryStack.AddChild(CreateSectionTitle("Inventory"));

        var inventoryScroll = new ScrollContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        inventoryStack.AddChild(inventoryScroll);

        _inventoryPanel = new InventoryPanel
        {
            Name = "InventoryPanel",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _inventoryPanel.ItemActionRequested += OnItemActionRequested;
        _inventoryPanel.ItemHovered += OnItemHovered;
        _inventoryPanel.ItemHoverEnded += OnItemHoverEnded;
        inventoryScroll.AddChild(_inventoryPanel);

        _itemActionPopup = new PanelContainer
        {
            Name = "ItemActionPopup",
            Visible = false,
            CustomMinimumSize = new Vector2(360, 0)
        };
        _itemActionPopup.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        uiLayer.AddChild(_itemActionPopup);

        var popupMargin = CreatePanelMargin();
        _itemActionPopup.AddChild(popupMargin);

        var popupScroll = new ScrollContainer
        {
            CustomMinimumSize = new Vector2(330, 0)
        };
        popupMargin.AddChild(popupScroll);

        var popupStack = new VBoxContainer();
        popupStack.AddThemeConstantOverride("separation", 8);
        popupScroll.AddChild(popupStack);

        _selectedItemPanel = new SelectedItemPanel
        {
            Name = "SelectedItemPanel"
        };
        _selectedItemPanel.ActionSelected += OnActionSelected;
        popupStack.AddChild(_selectedItemPanel);

        _itemInfoPopup = new PanelContainer
        {
            Name = "ItemInfoPopup",
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(ItemInfoPopupWidth, 0)
        };
        _itemInfoPopup.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        uiLayer.AddChild(_itemInfoPopup);

        var infoMargin = CreatePanelMargin();
        infoMargin.MouseFilter = Control.MouseFilterEnum.Ignore;
        _itemInfoPopup.AddChild(infoMargin);

        var infoScroll = new ScrollContainer
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(ItemInfoContentWidth, 0),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        infoMargin.AddChild(infoScroll);

        _itemInfoPanel = new SelectedItemPanel
        {
            Name = "ItemInfoPanel",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(ItemInfoContentWidth, 0),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        infoScroll.AddChild(_itemInfoPanel);

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

    private static PanelContainer CreateSidebarPanel(string name)
    {
        var panel = new PanelContainer
        {
            Name = name
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        return panel;
    }

    private static MarginContainer CreatePanelMargin()
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        return margin;
    }

    private static VBoxContainer CreatePanelStack(PanelContainer panel)
    {
        var margin = CreatePanelMargin();
        panel.AddChild(margin);

        var stack = new VBoxContainer();
        stack.AddThemeConstantOverride("separation", 8);
        margin.AddChild(stack);
        return stack;
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
        var availableActions = _actionPipeline.GetAvailableActions(_gameState);
        EnsureSelectedItemStillValid();

        _modeLabel.Text = $"Mode: {Session?.SiteDisplayName ?? "Prototype Shell"}";
        _timeLabel.Text = $"Time: {_gameState.Time.ElapsedTicks} ticks";
        _positionLabel.Text = $"Position: {position.X}, {position.Y}";
        _surfaceLabel.Text = $"Surface: {GetSurfaceAt(position).Name}";
        _targetLabel.Text = FormatSelectedTarget();
        _actionPanel.Display(GetGlobalActions(availableActions));
        _statusPanel.Display(_gameState.Player.Vitals);
        _equipmentPanel.Display(_gameState.Player.Equipment, _itemCatalog, _gameState.StatefulItems, _selectedItem);
        _inventoryPanel.Display(_gameState.Player.Inventory, _itemCatalog, _gameState.StatefulItems, _selectedItem);
        _selectedItemPanel.DisplayActions(
            _selectedItem,
            _gameState,
            _itemCatalog,
            GetContextualActions(availableActions, _selectedItem)
        );
    }

    private void OnItemActionRequested(SelectedItemRef itemRef, Vector2 cursorPosition)
    {
        _selectedItem = itemRef;
        HideItemInfoPopup();
        UpdateOverlay();
        ShowItemActionPopup(cursorPosition);
    }

    private void OnItemHovered(SelectedItemRef itemRef, Vector2 cursorPosition)
    {
        _hoveredItem = itemRef;

        if (_itemActionPopup.Visible)
        {
            HideItemInfoPopup();
            return;
        }

        _itemInfoPanel.DisplayDetails(itemRef, _gameState, _itemCatalog, _firearmCatalog);
        ShowItemInfoPopup(cursorPosition);
    }

    private void OnItemHoverEnded(SelectedItemRef itemRef)
    {
        if (_hoveredItem != itemRef)
        {
            return;
        }

        _hoveredItem = null;
        HideItemInfoPopup();
    }

    private IReadOnlyList<AvailableAction> GetGlobalActions(IReadOnlyList<AvailableAction> availableActions)
    {
        var actions = availableActions
            .Where(action => action.Kind is GameActionKind.Wait
                or GameActionKind.Pickup
                or GameActionKind.RefuelVehicle
                or GameActionKind.SearchContainer
                or GameActionKind.TakeContainerItemStack
                or GameActionKind.TakeTravelCargoItemStack
                or GameActionKind.TakeTravelCargoStatefulItem
                or GameActionKind.FillFuelCan
                or GameActionKind.PourFuelCanIntoVehicle)
            .ToList();

        if (GetSelectedTargetNpc() is { } target)
        {
            actions.Insert(0, new AvailableAction(
                GameActionKind.ShootNpc,
                $"Shoot {target.Name}",
                new ShootNpcActionRequest(target.Id)
            ));
        }

        if (actions.All(action => action.Kind != GameActionKind.Pickup)
            && availableActions.FirstOrDefault(action => action.Kind == GameActionKind.PickupStatefulItem) is { } pickupStatefulItem)
        {
            actions.Add(new AvailableAction(GameActionKind.PickupStatefulItem, "Pick Up", pickupStatefulItem.Request));
        }

        return actions;
    }

    private static IReadOnlyList<AvailableAction> GetContextualActions(
        IReadOnlyList<AvailableAction> availableActions,
        SelectedItemRef? selectedItem)
    {
        if (selectedItem is null)
        {
            return Array.Empty<AvailableAction>();
        }

        return availableActions
            .Where(action => IsActionForSelectedItem(action, selectedItem))
            .GroupBy(action => action.Request)
            .Select(group => group.First())
            .ToArray();
    }

    private static bool IsActionForSelectedItem(AvailableAction action, SelectedItemRef selectedItem)
    {
        return action.Request switch
        {
            EquipItemActionRequest equip => MatchesStackItem(selectedItem, equip.ItemId),
            InspectItemActionRequest inspect => MatchesStackItem(selectedItem, inspect.ItemId),
            DropItemStackActionRequest dropStack => selectedItem.Kind == SelectedItemKind.InventoryStack
                && selectedItem.ItemId == dropStack.ItemId,
            UnequipItemActionRequest unequip => selectedItem.Kind == SelectedItemKind.EquipmentItem
                && selectedItem.EquipmentSlotId == unequip.SlotId,
            LoadFeedDeviceActionRequest loadFeed => MatchesStackItem(selectedItem, loadFeed.FeedDeviceItemId)
                || MatchesStackItem(selectedItem, loadFeed.AmmunitionItemId),
            UnloadFeedDeviceActionRequest unloadFeed => MatchesStackItem(selectedItem, unloadFeed.FeedDeviceItemId),
            InsertFeedDeviceActionRequest insertFeed => MatchesStackItem(selectedItem, insertFeed.WeaponItemId)
                || MatchesStackItem(selectedItem, insertFeed.FeedDeviceItemId),
            RemoveFeedDeviceActionRequest removeFeed => MatchesStackItem(selectedItem, removeFeed.WeaponItemId),
            LoadWeaponActionRequest loadWeapon => MatchesStackItem(selectedItem, loadWeapon.WeaponItemId)
                || MatchesStackItem(selectedItem, loadWeapon.AmmunitionItemId),
            ReloadWeaponActionRequest reloadWeapon => MatchesStackItem(selectedItem, reloadWeapon.WeaponItemId)
                || MatchesStackItem(selectedItem, reloadWeapon.AmmunitionItemId),
            TestFireActionRequest testFire => MatchesStackItem(selectedItem, testFire.WeaponItemId),
            ToggleFireModeActionRequest toggleFireMode => MatchesStackItem(selectedItem, toggleFireMode.WeaponItemId),
            PickupStatefulItemActionRequest pickupStateful => MatchesStatefulItem(selectedItem, pickupStateful.ItemId),
            DropStatefulItemActionRequest dropStateful => MatchesStatefulItem(selectedItem, dropStateful.ItemId),
            InspectStatefulItemActionRequest inspectStateful => MatchesStatefulItem(selectedItem, inspectStateful.ItemId),
            StowItemStackInTravelCargoActionRequest stowStack => selectedItem.Kind == SelectedItemKind.InventoryStack
                && selectedItem.ItemId == stowStack.ItemId,
            StowStatefulItemInTravelCargoActionRequest stowStateful => MatchesStatefulItem(selectedItem, stowStateful.ItemId),
            FillFuelCanActionRequest fillFuelCan => MatchesStatefulItem(selectedItem, fillFuelCan.FuelCanId),
            PourFuelCanIntoVehicleActionRequest pourFuelCan => MatchesStatefulItem(selectedItem, pourFuelCan.FuelCanId),
            EquipStatefulItemActionRequest equipStateful => MatchesStatefulItem(selectedItem, equipStateful.ItemId),
            UnequipStatefulItemActionRequest unequipStateful => MatchesStatefulItem(selectedItem, unequipStateful.ItemId),
            LoadStatefulFeedDeviceActionRequest loadStatefulFeed => MatchesStatefulItem(selectedItem, loadStatefulFeed.FeedDeviceItemId),
            UnloadStatefulFeedDeviceActionRequest unloadStatefulFeed => MatchesStatefulItem(selectedItem, unloadStatefulFeed.FeedDeviceItemId),
            InsertStatefulFeedDeviceActionRequest insertStatefulFeed => MatchesStatefulItem(selectedItem, insertStatefulFeed.WeaponItemId)
                || MatchesStatefulItem(selectedItem, insertStatefulFeed.FeedDeviceItemId),
            RemoveStatefulFeedDeviceActionRequest removeStatefulFeed => MatchesStatefulItem(selectedItem, removeStatefulFeed.WeaponItemId),
            LoadStatefulWeaponActionRequest loadStatefulWeapon => MatchesStatefulItem(selectedItem, loadStatefulWeapon.WeaponItemId),
            ReloadStatefulWeaponActionRequest reloadStatefulWeapon => MatchesStatefulItem(selectedItem, reloadStatefulWeapon.WeaponItemId)
                || MatchesStackItem(selectedItem, reloadStatefulWeapon.AmmunitionItemId),
            TestFireStatefulWeaponActionRequest testStatefulFire => MatchesStatefulItem(selectedItem, testStatefulFire.WeaponItemId),
            ToggleStatefulFireModeActionRequest toggleStatefulFireMode => MatchesStatefulItem(selectedItem, toggleStatefulFireMode.WeaponItemId),
            InstallStatefulWeaponModActionRequest installWeaponMod => MatchesStatefulItem(selectedItem, installWeaponMod.WeaponItemId)
                || MatchesStatefulItem(selectedItem, installWeaponMod.ModItemId),
            RemoveStatefulWeaponModActionRequest removeWeaponMod => MatchesStatefulItem(selectedItem, removeWeaponMod.WeaponItemId),
            _ => false
        };
    }

    private static bool MatchesStackItem(SelectedItemRef selectedItem, ItemId itemId)
    {
        return selectedItem.Kind is SelectedItemKind.InventoryStack or SelectedItemKind.EquipmentItem
            && selectedItem.ItemId == itemId;
    }

    private static bool MatchesStatefulItem(SelectedItemRef selectedItem, StatefulItemId itemId)
    {
        return selectedItem.Kind == SelectedItemKind.StatefulItem
            && selectedItem.StatefulItemId == itemId;
    }

    private void EnsureSelectedItemStillValid()
    {
        if (_selectedItem is null)
        {
            HideItemActionPopup();
            return;
        }

        _selectedItem = _selectedItem.Kind switch
        {
            SelectedItemKind.InventoryStack when _selectedItem.ItemId is not null
                && _gameState.Player.Inventory.CountOf(_selectedItem.ItemId) > 0 => _selectedItem,
            SelectedItemKind.EquipmentItem when IsSelectedEquipmentItemStillEquipped(_selectedItem) => _selectedItem,
            SelectedItemKind.StatefulItem when _selectedItem.StatefulItemId is not null
                && _gameState.StatefulItems.TryGet(_selectedItem.StatefulItemId.Value, out _) => _selectedItem,
            _ => null
        };

        if (_selectedItem is null)
        {
            HideItemActionPopup();
        }
    }

    private bool IsSelectedEquipmentItemStillEquipped(SelectedItemRef selectedItem)
    {
        if (selectedItem.EquipmentSlotId is null || selectedItem.ItemId is null)
        {
            return false;
        }

        return _gameState.Player.Equipment.TryGetEquippedItem(selectedItem.EquipmentSlotId, out var equippedItem)
            && equippedItem.ItemId == selectedItem.ItemId;
    }

    private void ShowItemActionPopup(Vector2 cursorPosition)
    {
        var viewportSize = GetViewportRect().Size;
        var popupSize = new Vector2(380, Mathf.Min(520.0f, viewportSize.Y - (LayoutMargin * 2.0f)));

        _itemActionPopup.Position = GetClampedPopupPosition(cursorPosition, popupSize);
        _itemActionPopup.Size = popupSize;
        _itemActionPopup.Visible = true;
    }

    private void HideItemActionPopup()
    {
        if (_itemActionPopup is not null)
        {
            _itemActionPopup.Visible = false;
        }
    }

    private void ShowItemInfoPopup(Vector2 cursorPosition)
    {
        var viewportSize = GetViewportRect().Size;
        var popupWidth = Mathf.Max(260.0f, Mathf.Min(ItemInfoPopupWidth, viewportSize.X - (LayoutMargin * 2.0f)));
        var popupSize = new Vector2(popupWidth, Mathf.Min(420.0f, viewportSize.Y - (LayoutMargin * 2.0f)));
        var desiredPosition = cursorPosition + new Vector2(18, 18);

        _itemInfoPopup.Position = GetClampedPopupPosition(desiredPosition, popupSize);
        _itemInfoPopup.Size = popupSize;
        _itemInfoPopup.Visible = true;
    }

    private void HideItemInfoPopup()
    {
        if (_itemInfoPopup is not null)
        {
            _itemInfoPopup.Visible = false;
        }
    }

    private void DismissSelectedItem()
    {
        if (_selectedItem is null)
        {
            HideItemActionPopup();
            return;
        }

        _selectedItem = null;
        HideItemActionPopup();
        UpdateOverlay();
    }

    private void UpdateItemInteractionPopups()
    {
        var cursorPosition = GetViewport().GetMousePosition();

        if (_hoveredItem is not null && !_itemActionPopup.Visible)
        {
            _itemInfoPopup.Position = GetClampedPopupPosition(cursorPosition + new Vector2(18, 18), _itemInfoPopup.Size);
        }

        if (_selectedItem is null || !_itemActionPopup.Visible)
        {
            return;
        }

        if (_hoveredItem == _selectedItem || IsPointerOverControl(_itemActionPopup, cursorPosition))
        {
            return;
        }

        DismissSelectedItem();
    }

    private Vector2 GetClampedPopupPosition(Vector2 desiredPosition, Vector2 popupSize)
    {
        var viewportSize = GetViewportRect().Size;
        var maxX = Mathf.Max(LayoutMargin, viewportSize.X - popupSize.X - LayoutMargin);
        var maxY = Mathf.Max(LayoutMargin, viewportSize.Y - popupSize.Y - LayoutMargin);

        return new Vector2(
            Mathf.Clamp(desiredPosition.X, LayoutMargin, maxX),
            Mathf.Clamp(desiredPosition.Y, LayoutMargin, maxY)
        );
    }

    private bool TryHandleBoardClick()
    {
        var clickedPosition = GetHoveredGridPosition();
        if (clickedPosition is null)
        {
            return false;
        }

        if (!_gameState.LocalMap.Npcs.TryGetAt(clickedPosition.Value, out var npc))
        {
            var shouldRefresh = false;
            if (_selectedTargetNpcId is not null)
            {
                _selectedTargetNpcId = null;
                shouldRefresh = true;
            }

            if (_selectedItem is not null)
            {
                _selectedItem = null;
                shouldRefresh = true;
            }

            HideItemActionPopup();
            if (shouldRefresh)
            {
                UpdateOverlay();
            }

            return true;
        }

        _selectedTargetNpcId = npc.Id;
        _selectedItem = null;
        HideItemActionPopup();
        UpdateOverlay();
        _messageLog.AddMessage($"Targeting {npc.Name}.");
        return true;
    }

    private bool IsPointerOverGameplayUi(Vector2 viewportPosition)
    {
        return IsPointerOverControl(_sidePanel, viewportPosition)
            || IsPointerOverControl(_logPanel, viewportPosition)
            || IsPointerOverControl(_itemActionPopup, viewportPosition)
            || IsPointerOverControl(_itemInfoPopup, viewportPosition);
    }

    private bool TryHandleLocalMapZoom(InputEventMouseButton mouseEvent)
    {
        if (mouseEvent.ButtonIndex is not (MouseButton.WheelUp or MouseButton.WheelDown))
        {
            return false;
        }

        if (IsPointerOverGameplayUi(mouseEvent.Position) || !IsPointerOverControl(_board, mouseEvent.Position))
        {
            return false;
        }

        var direction = mouseEvent.ButtonIndex == MouseButton.WheelUp ? -1 : 1;
        SetLocalZoomLevel(_localZoomLevelIndex + direction);
        return true;
    }

    private bool SetLocalZoomLevel(int zoomLevelIndex)
    {
        var clampedZoomLevelIndex = Math.Clamp(zoomLevelIndex, 0, LocalZoomLevels.Length - 1);
        if (clampedZoomLevelIndex == _localZoomLevelIndex)
        {
            return false;
        }

        _localZoomLevelIndex = clampedZoomLevelIndex;
        RefreshViewport();
        UpdateResponsiveLayout();
        ConfigureLocalMapView();
        HideItemTooltip();
        return true;
    }

    private static bool IsPointerOverControl(Control control, Vector2 viewportPosition)
    {
        return control is not null
            && control.Visible
            && control.GetGlobalRect().HasPoint(viewportPosition);
    }

    private void UpdateItemTooltip()
    {
        var cursorPosition = GetViewport().GetMousePosition();
        if (IsPointerOverGameplayUi(cursorPosition))
        {
            HideItemTooltip();
            return;
        }

        var hoveredPosition = GetHoveredGridPosition();
        if (hoveredPosition is null)
        {
            HideItemTooltip();
            return;
        }

        var itemStacks = _gameState.LocalMap.GroundItems.ItemsAt(hoveredPosition.Value);
        var statefulItems = _gameState.StatefulItems.OnGround(hoveredPosition.Value, _gameState.SiteId);
        var surface = GetSurfaceAt(hoveredPosition.Value);
        var worldObject = GetWorldObjectAt(hoveredPosition.Value);
        var structure = GetStructureAt(hoveredPosition.Value);
        var npc = GetNpcAt(hoveredPosition.Value);

        if (_visibleTooltipPosition == hoveredPosition)
        {
            _itemTooltip.MoveTo(cursorPosition);
            return;
        }

        _visibleTooltipPosition = hoveredPosition;
        _itemTooltip.Display(
            hoveredPosition.Value,
            surface,
            worldObject,
            structure,
            npc,
            itemStacks,
            statefulItems,
            _itemCatalog,
            _npcCatalog,
            cursorPosition
        );
    }

    private TileSurfaceDefinition GetSurfaceAt(GridPosition position)
    {
        var surfaceId = _gameState.LocalMap.Map.Surfaces.GetSurfaceId(position);
        return _surfaceCatalog.Get(surfaceId);
    }

    private WorldObjectDefinition? GetWorldObjectAt(GridPosition position)
    {
        return _gameState.LocalMap.WorldObjects.TryGetObjectAt(position, out var objectId)
            ? _worldObjectCatalog.Get(objectId)
            : null;
    }

    private StructureDefinition? GetStructureAt(GridPosition position)
    {
        foreach (var direction in TooltipStructureDirections)
        {
            if (_gameState.LocalMap.Structures.TryGetEdgeAt(position, direction, out var edge))
            {
                return _structureCatalog.Get(edge.StructureId);
            }
        }

        return null;
    }

    private NpcState? GetNpcAt(GridPosition position)
    {
        return _gameState.LocalMap.Npcs.TryGetAt(position, out var npc)
            ? npc
            : null;
    }

    private NpcState? GetSelectedTargetNpc()
    {
        return _selectedTargetNpcId is not null && _gameState.LocalMap.Npcs.TryGet(_selectedTargetNpcId, out var npc)
            ? npc
            : null;
    }

    private string FormatSelectedTarget()
    {
        var target = GetSelectedTargetNpc();
        return target is null
            ? "Target: None"
            : $"Target: {target.Name} ({target.Health.Current}/{target.Health.Maximum})";
    }

    private GridPosition? GetHoveredGridPosition()
    {
        var boardPosition = _board.GetLocalMousePosition();
        var viewportCell = new GridPosition(
            Mathf.FloorToInt(boardPosition.X / _cellSize),
            Mathf.FloorToInt(boardPosition.Y / _cellSize)
        );

        return _viewport.TryViewportToMap(viewportCell, out var mapPosition)
            ? mapPosition
            : null;
    }

    private void HideItemTooltip()
    {
        _visibleTooltipPosition = null;
        _itemTooltip.HideTooltip();
    }

    private void RefreshViewport()
    {
        var viewportSize = CurrentLocalViewportSize;
        _viewport = GridViewport.Create(
            _mapBounds,
            _gameState.Player.Position,
            viewportSize.X,
            viewportSize.Y
        );
    }

    private void ConfigureLocalMapView()
    {
        _gridView.Configure(_gameState.LocalMap.Map.Surfaces, _surfaceCatalog, _cellSize, _viewport);
        _groundItemLayer.Configure(
            _gameState.LocalMap.GroundItems,
            _itemCatalog,
            _cellSize,
            _gameState.StatefulItems,
            _gameState.SiteId,
            _viewport
        );
        _mapEntityLayer.Configure(
            _gameState.LocalMap.WorldObjects,
            _worldObjectCatalog,
            _gameState.LocalMap.Structures,
            _structureCatalog,
            _gameState.LocalMap.Npcs,
            _npcCatalog,
            _gameState.Player,
            _cellSize,
            _viewport
        );
    }

    private void UpdateResponsiveLayout()
    {
        var screenSize = GetViewportRect().Size;
        var localViewportSize = CurrentLocalViewportSize;
        var boardAreaWidth = Mathf.Max(
            MinimumCellSize * localViewportSize.X,
            (screenSize.X * WorldRegionWidthRatio) - (LayoutMargin * 2.0f)
        );
        var boardAreaHeight = Mathf.Max(
            MinimumCellSize * localViewportSize.Y,
            (screenSize.Y * WorldRegionHeightRatio) - (LayoutMargin * 2.0f)
        );
        var fittedCellSize = Mathf.FloorToInt(Mathf.Min(
            boardAreaWidth / localViewportSize.X,
            boardAreaHeight / localViewportSize.Y
        ));
        var nextCellSize = Mathf.Max(
            MinimumCellSize,
            fittedCellSize
        );

        if (_cellSize != nextCellSize)
        {
            _cellSize = nextCellSize;
            ConfigureLocalMapView();
        }

        var boardPixelSize = new Vector2(localViewportSize.X * _cellSize, localViewportSize.Y * _cellSize);
        _board.Position = new Vector2(MinimumBoardMargin, MinimumBoardMargin);
        _board.Size = boardPixelSize;

        var boardRight = _board.Position.X + boardPixelSize.X;
        var boardBottom = _board.Position.Y + boardPixelSize.Y;
        var logTop = boardBottom + 14.0f;
        var widePanelLeft = boardRight + LayoutMargin;
        var widePanelRight = screenSize.X - LayoutMargin;
        var widePanelWidth = widePanelRight - widePanelLeft;

        if (widePanelWidth >= MinimumWidePanelWidth)
        {
            _sidePanel.OffsetLeft = widePanelLeft;
            _sidePanel.OffsetTop = LayoutMargin;
            _sidePanel.OffsetRight = widePanelRight;
            _sidePanel.OffsetBottom = Mathf.Max(620.0f, screenSize.Y - LayoutMargin);
        }
        else
        {
            var panelWidth = Mathf.Min(SidePanelWidth, Mathf.Max(280.0f, screenSize.X * 0.32f));
            _sidePanel.OffsetLeft = screenSize.X - panelWidth - LayoutMargin;
            _sidePanel.OffsetTop = LayoutMargin;
            _sidePanel.OffsetRight = screenSize.X - LayoutMargin;
            _sidePanel.OffsetBottom = Mathf.Max(620.0f, screenSize.Y - LayoutMargin);
        }

        _logPanel.OffsetLeft = _board.Position.X;
        _logPanel.OffsetTop = logTop;
        _logPanel.OffsetRight = boardRight;
        _logPanel.OffsetBottom = Mathf.Max(logTop + 128.0f, screenSize.Y - LayoutMargin);
    }

    private void OnActionSelected(AvailableAction action)
    {
        GameActionRequest? request = action.Request ?? action.Kind switch
        {
            GameActionKind.Wait => new WaitActionRequest(),
            GameActionKind.Pickup => new PickupActionRequest(),
            GameActionKind.RefuelVehicle => new RefuelVehicleActionRequest(),
            _ => null
        };

        if (request is not null)
        {
            _selectedItem = null;
            HideItemActionPopup();
            HideItemInfoPopup();
            ExecuteAction(request);
        }
    }

    private void ExecuteAction(GameActionRequest request)
    {
        var result = _actionPipeline.Execute(request, _gameState);

        RefreshViewport();
        ConfigureLocalMapView();
        _groundItemLayer.QueueRedraw();
        _mapEntityLayer.QueueRedraw();
        UpdateOverlay();
        HideItemTooltip();
        HideItemInfoPopup();

        foreach (var message in result.Messages)
        {
            _messageLog.AddMessage(message);
        }
    }

}

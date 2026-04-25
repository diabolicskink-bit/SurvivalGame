using System;
using System.Collections.Generic;
using System.Linq;
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
    private const float MinimumWidePanelWidth = 620.0f;
    private const int StatusFontSize = 18;
    private const int SectionTitleFontSize = 17;

    private ItemCatalog _itemCatalog = null!;
    private FirearmCatalog _firearmCatalog = null!;
    private TileSurfaceCatalog _surfaceCatalog = null!;
    private WorldObjectCatalog _worldObjectCatalog = null!;
    private NpcCatalog _npcCatalog = null!;
    private GameActionPipeline _actionPipeline = null!;
    private PrototypeGameState _gameState = null!;
    private Node2D _board = null!;
    private GridView _gridView = null!;
    private WorldObjectLayer _worldObjectLayer = null!;
    private GroundItemLayer _groundItemLayer = null!;
    private NpcLayer _npcLayer = null!;
    private Node2D _playerMarker = null!;
    private PlayerController _playerController = null!;
    private Control _sidePanel = null!;
    private PanelContainer _itemActionPopup = null!;
    private PanelContainer _logPanel = null!;
    private ActionPanel _actionPanel = null!;
    private PlayerStatusPanel _statusPanel = null!;
    private EquipmentPanel _equipmentPanel = null!;
    private InventoryPanel _inventoryPanel = null!;
    private SelectedItemPanel _selectedItemPanel = null!;
    private ItemTooltip _itemTooltip = null!;
    private MessageLog _messageLog = null!;
    private Label _timeLabel = null!;
    private Label _positionLabel = null!;
    private Label _surfaceLabel = null!;
    private Label _targetLabel = null!;
    private Label _modeLabel = null!;
    private Button _returnToOverworldButton = null!;
    private SelectedItemRef? _selectedItem;
    private NpcId? _selectedTargetNpcId;
    private GridPosition? _visibleTooltipPosition;
    private GridBounds _mapBounds = new(1, 1);
    private int _cellSize = BaseCellSize;

    public event Action? ReturnToOverworldRequested;

    public PrototypeGameplaySession? Session { get; set; }

    public bool ShowsReturnToOverworld { get; set; }

    public override void _Ready()
    {
        _board = GetNode<Node2D>("Board");
        _gridView = GetNode<GridView>("Board/GridView");
        _worldObjectLayer = GetNode<WorldObjectLayer>("Board/WorldObjectLayer");
        _groundItemLayer = GetNode<GroundItemLayer>("Board/GroundItemLayer");
        _npcLayer = GetNode<NpcLayer>("Board/NpcLayer");
        _playerMarker = GetNode<Node2D>("Board/PlayerMarker");
        _playerController = GetNode<PlayerController>("Board/PlayerController");

        var isStandaloneSession = Session is null;
        var session = Session ?? PrototypeSessionFactory.CreateGameplaySession();
        _itemCatalog = session.ItemCatalog;
        _firearmCatalog = session.FirearmCatalog;
        _surfaceCatalog = session.SurfaceCatalog;
        _worldObjectCatalog = session.WorldObjectCatalog;
        _npcCatalog = session.NpcCatalog;
        _actionPipeline = session.ActionPipeline;
        _gameState = session.GameState;
        _mapBounds = _gameState.MapBounds;

        _gridView.Configure(_gameState.World.Map.Surfaces, _surfaceCatalog, _cellSize);
        _worldObjectLayer.Configure(_gameState.World.WorldObjects, _worldObjectCatalog, _cellSize);
        _groundItemLayer.Configure(
            _gameState.World.GroundItems,
            _itemCatalog,
            _cellSize,
            _gameState.StatefulItems,
            _gameState.SiteId
        );
        _npcLayer.Configure(_gameState.World.Npcs, _npcCatalog, _cellSize);
        _playerController.MoveRequested += OnMoveRequested;
        UpdatePlayerMarker();

        BuildOverlay();
        UpdateResponsiveLayout();
        UpdateOverlay();
        _messageLog.AddMessage(isStandaloneSession ? "New run started." : "Entered local site.");
    }

    public override void _Process(double delta)
    {
        UpdateItemTooltip();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
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
        if (ShowsReturnToOverworld)
        {
            ReturnToOverworldRequested?.Invoke();
            return;
        }

        GetTree().ChangeSceneToFile(MainMenuScenePath);
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

        _returnToOverworldButton = new Button
        {
            Text = "Return to Overworld",
            Visible = ShowsReturnToOverworld,
            CustomMinimumSize = new Vector2(0, 38)
        };
        _returnToOverworldButton.Pressed += ReturnFromLocalOrExit;
        stack.AddChild(_returnToOverworldButton);

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
        _equipmentPanel.ItemSelected += OnItemSelected;
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
        _inventoryPanel.ItemSelected += OnItemSelected;
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
        _selectedItemPanel.Display(
            _selectedItem,
            _gameState,
            _itemCatalog,
            _firearmCatalog,
            GetContextualActions(availableActions, _selectedItem)
        );
    }

    private void OnItemSelected(SelectedItemRef itemRef, Vector2 cursorPosition)
    {
        _selectedItem = itemRef;
        UpdateOverlay();
        ShowItemActionPopup(cursorPosition);
    }

    private IReadOnlyList<AvailableAction> GetGlobalActions(IReadOnlyList<AvailableAction> availableActions)
    {
        var actions = availableActions
            .Where(action => action.Kind is GameActionKind.Wait or GameActionKind.Pickup or GameActionKind.RefuelVehicle)
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
            PickupStatefulItemActionRequest pickupStateful => MatchesStatefulItem(selectedItem, pickupStateful.ItemId),
            DropStatefulItemActionRequest dropStateful => MatchesStatefulItem(selectedItem, dropStateful.ItemId),
            InspectStatefulItemActionRequest inspectStateful => MatchesStatefulItem(selectedItem, inspectStateful.ItemId),
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
        var position = cursorPosition + new Vector2(16, 12);

        if (position.X + popupSize.X > viewportSize.X - LayoutMargin)
        {
            position.X = Mathf.Max(LayoutMargin, cursorPosition.X - popupSize.X - 16);
        }

        if (position.Y + popupSize.Y > viewportSize.Y - LayoutMargin)
        {
            position.Y = Mathf.Max(LayoutMargin, viewportSize.Y - popupSize.Y - LayoutMargin);
        }

        _itemActionPopup.Position = position;
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

    private bool TryHandleBoardClick()
    {
        var clickedPosition = GetHoveredGridPosition();
        if (clickedPosition is null)
        {
            return false;
        }

        if (!_gameState.World.Npcs.TryGetAt(clickedPosition.Value, out var npc))
        {
            if (_selectedTargetNpcId is not null)
            {
                _selectedTargetNpcId = null;
                UpdateOverlay();
            }

            HideItemActionPopup();
            return true;
        }

        _selectedTargetNpcId = npc.Id;
        HideItemActionPopup();
        UpdateOverlay();
        _messageLog.AddMessage($"Targeting {npc.Name}.");
        return true;
    }

    private bool IsPointerOverGameplayUi(Vector2 viewportPosition)
    {
        return IsPointerOverControl(_sidePanel, viewportPosition)
            || IsPointerOverControl(_logPanel, viewportPosition)
            || IsPointerOverControl(_itemActionPopup, viewportPosition);
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

        var itemStacks = _gameState.World.GroundItems.ItemsAt(hoveredPosition.Value);
        var statefulItems = _gameState.StatefulItems.OnGround(hoveredPosition.Value, _gameState.SiteId);
        var surface = GetSurfaceAt(hoveredPosition.Value);
        var worldObject = GetWorldObjectAt(hoveredPosition.Value);
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
        var surfaceId = _gameState.World.Map.Surfaces.GetSurfaceId(position);
        return _surfaceCatalog.Get(surfaceId);
    }

    private WorldObjectDefinition? GetWorldObjectAt(GridPosition position)
    {
        return _gameState.World.WorldObjects.TryGetObjectAt(position, out var objectId)
            ? _worldObjectCatalog.Get(objectId)
            : null;
    }

    private NpcState? GetNpcAt(GridPosition position)
    {
        return _gameState.World.Npcs.TryGetAt(position, out var npc)
            ? npc
            : null;
    }

    private NpcState? GetSelectedTargetNpc()
    {
        return _selectedTargetNpcId is not null && _gameState.World.Npcs.TryGet(_selectedTargetNpcId, out var npc)
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
        var cell = new GridPosition(
            Mathf.FloorToInt(boardPosition.X / _cellSize),
            Mathf.FloorToInt(boardPosition.Y / _cellSize)
        );

        return _mapBounds.Contains(cell) ? cell : null;
    }

    private void HideItemTooltip()
    {
        _visibleTooltipPosition = null;
        _itemTooltip.HideTooltip();
    }

    private void UpdateResponsiveLayout()
    {
        var viewportSize = GetViewportRect().Size;
        var boardAreaWidth = Mathf.Max(
            MinimumCellSize * _mapBounds.Width,
            (viewportSize.X * WorldRegionWidthRatio) - (LayoutMargin * 2.0f)
        );
        var boardAreaHeight = Mathf.Max(
            MinimumCellSize * _mapBounds.Height,
            (viewportSize.Y * WorldRegionHeightRatio) - (LayoutMargin * 2.0f)
        );
        var fittedCellSize = Mathf.FloorToInt(Mathf.Min(boardAreaWidth / _mapBounds.Width, boardAreaHeight / _mapBounds.Height));
        var nextCellSize = Mathf.Max(
            MinimumCellSize,
            fittedCellSize
        );

        if (_cellSize != nextCellSize)
        {
            _cellSize = nextCellSize;
            _gridView.Configure(_gameState.World.Map.Surfaces, _surfaceCatalog, _cellSize);
            _worldObjectLayer.Configure(_gameState.World.WorldObjects, _worldObjectCatalog, _cellSize);
            _groundItemLayer.Configure(
                _gameState.World.GroundItems,
                _itemCatalog,
                _cellSize,
                _gameState.StatefulItems,
                _gameState.SiteId
            );
            _npcLayer.Configure(_gameState.World.Npcs, _npcCatalog, _cellSize);
            UpdatePlayerMarker();
        }

        _board.Position = new Vector2(MinimumBoardMargin, MinimumBoardMargin);
        _playerMarker.Scale = Vector2.One * (_cellSize / (float)BaseCellSize);

        var boardPixelSize = new Vector2(_mapBounds.Width * _cellSize, _mapBounds.Height * _cellSize);
        var boardRight = _board.Position.X + boardPixelSize.X;
        var boardBottom = _board.Position.Y + boardPixelSize.Y;
        var logTop = boardBottom + 14.0f;
        var widePanelLeft = boardRight + LayoutMargin;
        var widePanelRight = viewportSize.X - LayoutMargin;
        var widePanelWidth = widePanelRight - widePanelLeft;

        if (widePanelWidth >= MinimumWidePanelWidth)
        {
            _sidePanel.OffsetLeft = widePanelLeft;
            _sidePanel.OffsetTop = LayoutMargin;
            _sidePanel.OffsetRight = widePanelRight;
            _sidePanel.OffsetBottom = Mathf.Max(620.0f, viewportSize.Y - LayoutMargin);
        }
        else
        {
            var panelWidth = Mathf.Min(SidePanelWidth, Mathf.Max(280.0f, viewportSize.X * 0.32f));
            _sidePanel.OffsetLeft = viewportSize.X - panelWidth - LayoutMargin;
            _sidePanel.OffsetTop = LayoutMargin;
            _sidePanel.OffsetRight = viewportSize.X - LayoutMargin;
            _sidePanel.OffsetBottom = Mathf.Max(620.0f, viewportSize.Y - LayoutMargin);
        }

        _logPanel.OffsetLeft = _board.Position.X;
        _logPanel.OffsetTop = logTop;
        _logPanel.OffsetRight = boardRight;
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
            GameActionKind.RefuelVehicle => new RefuelVehicleActionRequest(),
            _ => null
        };

        if (request is not null)
        {
            HideItemActionPopup();
            ExecuteAction(request);
        }
    }

    private void ExecuteAction(GameActionRequest request)
    {
        var result = _actionPipeline.Execute(_gameState, request);

        UpdatePlayerMarker();
        _worldObjectLayer.QueueRedraw();
        _groundItemLayer.QueueRedraw();
        _npcLayer.QueueRedraw();
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

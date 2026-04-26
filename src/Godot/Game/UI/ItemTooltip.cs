using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

public partial class ItemTooltip : PanelContainer
{
    private const float OffsetFromCursor = 18.0f;
    private const float PreferredWidth = 320.0f;
    private const float ContentWidth = 296.0f;
    private const float VerticalPadding = 20.0f;
    private const float ScreenPadding = 12.0f;
    private const float MaximumHeight = 420.0f;

    private VBoxContainer _content = null!;

    public override void _Ready()
    {
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
        CustomMinimumSize = Vector2.Zero;
        Size = new Vector2(PreferredWidth, 0);
        AddThemeStyleboxOverride("panel", CreatePanelStyle());

        var margin = new MarginContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(PreferredWidth, 0)
        };
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        AddChild(margin);

        _content = new VBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(ContentWidth, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _content.AddThemeConstantOverride("separation", 5);
        margin.AddChild(_content);
    }

    public void Display(
        GridPosition position,
        TileSurfaceDefinition surface,
        WorldObjectDefinition? worldObject,
        StructureDefinition? structure,
        NpcState? npc,
        IReadOnlyList<GroundItemStack> itemStacks,
        IReadOnlyList<StatefulItem> statefulItems,
        ItemCatalog itemCatalog,
        NpcCatalog? npcCatalog,
        Vector2 cursorPosition
    )
    {
        ClearContent();

        _content.AddChild(CreateLabel($"Tile {position.X}, {position.Y}", 15, new Color(0.63f, 0.72f, 0.68f)));
        _content.AddChild(CreateLabel(surface.Name, 17, new Color(0.9f, 0.93f, 0.86f)));

        if (!string.IsNullOrWhiteSpace(surface.Description))
        {
            _content.AddChild(CreateLabel(surface.Description, 14, new Color(0.68f, 0.75f, 0.71f)));
        }

        if (surface.Tags.Count > 0)
        {
            _content.AddChild(CreateLabel($"Tags: {string.Join(", ", surface.Tags)}", 14, new Color(0.58f, 0.68f, 0.66f)));
        }

        if (worldObject is not null)
        {
            _content.AddChild(CreateLabel("Object", 15, new Color(0.63f, 0.72f, 0.68f)));
            _content.AddChild(CreateLabel(worldObject.Name, 17, new Color(0.9f, 0.93f, 0.86f)));

            if (!string.IsNullOrWhiteSpace(worldObject.Description))
            {
                _content.AddChild(CreateLabel(worldObject.Description, 14, new Color(0.68f, 0.75f, 0.71f)));
            }

            _content.AddChild(CreateLabel(
                worldObject.BlocksMovement ? "Blocks movement" : "Does not block movement",
                14,
                new Color(0.58f, 0.68f, 0.66f)
            ));
        }

        if (structure is not null)
        {
            _content.AddChild(CreateLabel("Structure", 15, new Color(0.63f, 0.72f, 0.68f)));
            _content.AddChild(CreateLabel(structure.Name, 17, new Color(0.9f, 0.93f, 0.86f)));

            if (!string.IsNullOrWhiteSpace(structure.Description))
            {
                _content.AddChild(CreateLabel(structure.Description, 14, new Color(0.68f, 0.75f, 0.71f)));
            }

            _content.AddChild(CreateLabel(
                structure.BlocksMovement ? "Blocks movement" : "Does not block movement",
                14,
                new Color(0.58f, 0.68f, 0.66f)
            ));
        }

        if (npc is not null)
        {
            _content.AddChild(CreateLabel("NPC", 15, new Color(0.63f, 0.72f, 0.68f)));
            _content.AddChild(CreateLabel(npc.Name, 17, new Color(0.9f, 0.93f, 0.86f)));
            if (npcCatalog is not null && npcCatalog.TryGet(npc.DefinitionId, out var definition))
            {
                _content.AddChild(CreateLabel(
                    $"{definition.Species} - {definition.Behavior.Kind}",
                    14,
                    new Color(0.68f, 0.75f, 0.71f)
                ));

                if (!string.IsNullOrWhiteSpace(definition.Description))
                {
                    _content.AddChild(CreateLabel(definition.Description, 14, new Color(0.68f, 0.75f, 0.71f)));
                }

                if (definition.Tags.Count > 0)
                {
                    _content.AddChild(CreateLabel(
                        $"Tags: {string.Join(", ", definition.Tags)}",
                        14,
                        new Color(0.58f, 0.68f, 0.66f)
                    ));
                }
            }

            _content.AddChild(CreateLabel(
                $"Health: {npc.Health.Current}/{npc.Health.Maximum}",
                14,
                new Color(0.68f, 0.75f, 0.71f)
            ));
            _content.AddChild(CreateLabel(
                npc.BlocksMovement ? "Blocks movement" : "Does not block movement",
                14,
                new Color(0.58f, 0.68f, 0.66f)
            ));
        }

        var hasShownItemsHeader = false;
        foreach (var stack in itemStacks)
        {
            if (!hasShownItemsHeader)
            {
                _content.AddChild(CreateLabel("Items", 15, new Color(0.63f, 0.72f, 0.68f)));
                hasShownItemsHeader = true;
            }

            _content.AddChild(CreateLabel(FormatItemStack(stack, itemCatalog), 17, new Color(0.9f, 0.93f, 0.86f)));

            if (itemCatalog.TryGet(stack.ItemId, out var item) && !string.IsNullOrWhiteSpace(item.Description))
            {
                _content.AddChild(CreateLabel(item.Description, 14, new Color(0.68f, 0.75f, 0.71f)));
            }
        }

        foreach (var item in statefulItems)
        {
            if (!hasShownItemsHeader)
            {
                _content.AddChild(CreateLabel("Items", 15, new Color(0.63f, 0.72f, 0.68f)));
                hasShownItemsHeader = true;
            }

            _content.AddChild(CreateLabel(FormatStatefulItem(item, itemCatalog), 17, new Color(0.9f, 0.93f, 0.86f)));

            if (itemCatalog.TryGet(item.ItemId, out var definition) && !string.IsNullOrWhiteSpace(definition.Description))
            {
                _content.AddChild(CreateLabel(definition.Description, 14, new Color(0.68f, 0.75f, 0.71f)));
            }
        }

        Visible = true;
        RefreshLayout(cursorPosition);
    }

    public void MoveTo(Vector2 cursorPosition)
    {
        Position = GetClampedPosition(cursorPosition + new Vector2(OffsetFromCursor, OffsetFromCursor));
    }

    public void HideTooltip()
    {
        Visible = false;
    }

    private void ClearContent()
    {
        Size = new Vector2(PreferredWidth, 0);

        foreach (var child in _content.GetChildren())
        {
            _content.RemoveChild(child);
            child.QueueFree();
        }
    }

    private void RefreshLayout(Vector2 cursorPosition)
    {
        var viewportSize = GetViewportRect().Size;
        var maximumHeight = Mathf.Min(MaximumHeight, Mathf.Max(0.0f, viewportSize.Y - (ScreenPadding * 2.0f)));
        var contentHeight = _content.GetCombinedMinimumSize().Y + VerticalPadding;
        var height = Mathf.Min(contentHeight, maximumHeight);

        Size = new Vector2(PreferredWidth, height);
        Position = GetClampedPosition(cursorPosition + new Vector2(OffsetFromCursor, OffsetFromCursor));
    }

    private Vector2 GetClampedPosition(Vector2 desiredPosition)
    {
        var viewportSize = GetViewportRect().Size;
        var maxX = Mathf.Max(ScreenPadding, viewportSize.X - Size.X - ScreenPadding);
        var maxY = Mathf.Max(ScreenPadding, viewportSize.Y - Size.Y - ScreenPadding);

        return new Vector2(
            Mathf.Clamp(desiredPosition.X, ScreenPadding, maxX),
            Mathf.Clamp(desiredPosition.Y, ScreenPadding, maxY)
        );
    }

    private static string FormatItemStack(GroundItemStack stack, ItemCatalog itemCatalog)
    {
        var itemName = stack.ItemId.ToString();
        if (itemCatalog.TryGet(stack.ItemId, out var item))
        {
            itemName = item.Name;
        }

        return stack.Quantity == 1 ? itemName : $"{itemName} x{stack.Quantity}";
    }

    private static string FormatStatefulItem(StatefulItem item, ItemCatalog itemCatalog)
    {
        var itemName = item.ItemId.ToString();
        if (itemCatalog.TryGet(item.ItemId, out var definition))
        {
            itemName = definition.Name;
        }

        if (item.FeedDevice is not null)
        {
            var loadedText = item.FeedDevice.LoadedAmmunitionVariant is null
                ? $"0/{item.FeedDevice.Capacity}"
                : $"{item.FeedDevice.LoadedCount}/{item.FeedDevice.Capacity} {item.FeedDevice.LoadedAmmunitionVariant}";

            return $"{itemName} [{item.Id}] - {loadedText}";
        }

        return $"{itemName} [{item.Id}]";
    }

    private static Label CreateLabel(string text, int fontSize, Color color)
    {
        var label = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(ContentWidth, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private static StyleBoxFlat CreatePanelStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.048f, 0.058f, 0.064f, 0.97f),
            BorderColor = new Color(0.28f, 0.36f, 0.32f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomRight = 6,
            CornerRadiusBottomLeft = 6
        };
    }
}

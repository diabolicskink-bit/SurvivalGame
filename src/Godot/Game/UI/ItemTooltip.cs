using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

public partial class ItemTooltip : PanelContainer
{
    private const float OffsetFromCursor = 18.0f;
    private const float PreferredWidth = 300.0f;

    private VBoxContainer _content = null!;

    public override void _Ready()
    {
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        CustomMinimumSize = new Vector2(PreferredWidth, 0);
        AddThemeStyleboxOverride("panel", CreatePanelStyle());

        var margin = new MarginContainer
        {
            MouseFilter = MouseFilterEnum.Ignore
        };
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_top", 10);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_bottom", 10);
        AddChild(margin);

        _content = new VBoxContainer
        {
            MouseFilter = MouseFilterEnum.Ignore
        };
        _content.AddThemeConstantOverride("separation", 5);
        margin.AddChild(_content);
    }

    public void Display(
        GridPosition position,
        IReadOnlyList<GroundItemStack> itemStacks,
        ItemCatalog itemCatalog,
        Vector2 cursorPosition
    )
    {
        ClearContent();

        _content.AddChild(CreateLabel($"Tile {position.X}, {position.Y}", 13, new Color(0.63f, 0.72f, 0.68f)));

        foreach (var stack in itemStacks)
        {
            _content.AddChild(CreateLabel(FormatItemStack(stack, itemCatalog), 15, new Color(0.9f, 0.93f, 0.86f)));

            if (itemCatalog.TryGet(stack.ItemId, out var item) && !string.IsNullOrWhiteSpace(item.Description))
            {
                _content.AddChild(CreateLabel(item.Description, 12, new Color(0.68f, 0.75f, 0.71f)));
            }
        }

        Position = cursorPosition + new Vector2(OffsetFromCursor, OffsetFromCursor);
        Visible = true;
    }

    public void MoveTo(Vector2 cursorPosition)
    {
        Position = cursorPosition + new Vector2(OffsetFromCursor, OffsetFromCursor);
    }

    public void HideTooltip()
    {
        Visible = false;
    }

    private void ClearContent()
    {
        foreach (var child in _content.GetChildren())
        {
            _content.RemoveChild(child);
            child.QueueFree();
        }
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

    private static Label CreateLabel(string text, int fontSize, Color color)
    {
        var label = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore
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

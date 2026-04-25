using System;
using Godot;
using SurvivalGame.Domain;

public partial class InventoryPanel : VBoxContainer
{
    private const int ItemFontSize = 16;

    public event Action<SelectedItemRef, Vector2>? ItemSelected;

    public override void _Ready()
    {
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddThemeConstantOverride("separation", 4);
    }

    public void Display(
        PlayerInventory inventory,
        ItemCatalog itemCatalog,
        StatefulItemStore statefulItems,
        SelectedItemRef? selectedItem)
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        var statefulInventoryItems = statefulItems.InPlayerInventory();
        if (inventory.IsEmpty && statefulInventoryItems.Count == 0)
        {
            AddChild(CreateItemLabel("Empty", muted: true));
            return;
        }

        foreach (var stack in inventory.Items)
        {
            var itemRef = SelectedItemRef.InventoryStack(stack.ItemId);
            AddChild(CreateItemButton(
                FormatItemStack(stack, itemCatalog),
                itemRef,
                IsSelected(selectedItem, itemRef)
            ));
        }

        foreach (var item in statefulInventoryItems)
        {
            var itemRef = SelectedItemRef.StatefulItem(item.Id);
            AddChild(CreateItemButton(
                FormatStatefulItem(item, itemCatalog),
                itemRef,
                IsSelected(selectedItem, itemRef)
            ));
        }
    }

    private static string FormatItemStack(InventoryItemStack stack, ItemCatalog itemCatalog)
    {
        var itemName = stack.ItemId.ToString();
        if (itemCatalog.TryGet(stack.ItemId, out var item))
        {
            itemName = item.DisplayName;
        }

        return $"{itemName} x{stack.Quantity}";
    }

    private static string FormatStatefulItem(StatefulItem item, ItemCatalog itemCatalog)
    {
        var itemName = item.ItemId.ToString();
        if (itemCatalog.TryGet(item.ItemId, out var definition))
        {
            itemName = definition.DisplayName;
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

    private static Label CreateItemLabel(string text, bool muted = false)
    {
        var label = new Label
        {
            Text = text,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };

        label.AddThemeFontSizeOverride("font_size", ItemFontSize);
        label.AddThemeColorOverride(
            "font_color",
            muted ? new Color(0.52f, 0.58f, 0.55f) : new Color(0.74f, 0.83f, 0.77f)
        );

        return label;
    }

    private Button CreateItemButton(string text, SelectedItemRef itemRef, bool selected)
    {
        var button = new Button
        {
            Text = text,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 32),
            FocusMode = Control.FocusModeEnum.None,
            Alignment = HorizontalAlignment.Left,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };

        button.AddThemeFontSizeOverride("font_size", ItemFontSize);
        button.AddThemeColorOverride("font_color", selected ? new Color(0.92f, 0.95f, 0.82f) : new Color(0.74f, 0.83f, 0.77f));
        button.AddThemeStyleboxOverride("normal", selected ? CreateSelectedStyle() : CreateRowStyle());
        button.AddThemeStyleboxOverride("hover", CreateHoverStyle());
        button.AddThemeStyleboxOverride("pressed", CreateSelectedStyle());
        button.Pressed += () => ItemSelected?.Invoke(itemRef, GetViewport().GetMousePosition());
        return button;
    }

    private static bool IsSelected(SelectedItemRef? selectedItem, SelectedItemRef itemRef)
    {
        return selectedItem == itemRef;
    }

    private static StyleBoxFlat CreateRowStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.05f, 0.062f, 0.066f, 0.0f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4,
            ContentMarginLeft = 8,
            ContentMarginRight = 8
        };
    }

    private static StyleBoxFlat CreateHoverStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.11f, 0.15f, 0.13f, 0.9f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4,
            ContentMarginLeft = 8,
            ContentMarginRight = 8
        };
    }

    private static StyleBoxFlat CreateSelectedStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.17f, 0.23f, 0.19f, 0.95f),
            BorderColor = new Color(0.38f, 0.55f, 0.42f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4,
            ContentMarginLeft = 8,
            ContentMarginRight = 8
        };
    }
}

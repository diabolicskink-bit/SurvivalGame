using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SurvivalGame.Domain;

public partial class InventoryPanel : VBoxContainer
{
    private const int ItemFontSize = 16;
    private const int TabFontSize = 15;
    private InventoryPanelMode _activeMode = InventoryPanelMode.Inventory;

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

        AddChild(CreateModeSwitch(inventory, itemCatalog, statefulItems, selectedItem));

        if (_activeMode == InventoryPanelMode.Inventory)
        {
            DisplayInventoryGrid(inventory, itemCatalog, statefulItems, selectedItem);
            return;
        }

        DisplayAmmoList(inventory, itemCatalog, selectedItem);
    }

    private void DisplayInventoryGrid(
        PlayerInventory inventory,
        ItemCatalog itemCatalog,
        StatefulItemStore statefulItems,
        SelectedItemRef? selectedItem)
    {
        var grid = new InventoryGridView();
        grid.ItemSelected += (itemRef, position) => ItemSelected?.Invoke(itemRef, position);
        AddChild(grid);
        grid.Display(
            inventory,
            itemCatalog,
            statefulItems,
            selectedItem,
            itemId => !IsLooseAmmo(itemId, itemCatalog)
        );

        var displayedItemCount = inventory.Items.Count(stack => !IsLooseAmmo(stack.ItemId, itemCatalog))
            + statefulItems.InPlayerInventory().Count(item => !IsLooseAmmo(item.ItemId, itemCatalog));

        if (displayedItemCount == 0)
        {
            AddChild(CreateItemLabel("Inventory is empty.", muted: true));
        }
    }

    private void DisplayAmmoList(
        PlayerInventory inventory,
        ItemCatalog itemCatalog,
        SelectedItemRef? selectedItem)
    {
        var ammoStacks = inventory.Items
            .Where(stack => IsLooseAmmo(stack.ItemId, itemCatalog))
            .OrderBy(stack => GetItemName(stack.ItemId, itemCatalog), StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (ammoStacks.Length == 0)
        {
            AddChild(CreateItemLabel("No ammo.", muted: true));
            return;
        }

        foreach (var stack in ammoStacks)
        {
            AddChild(CreateAmmoButton(stack, itemCatalog, selectedItem));
        }
    }

    private HBoxContainer CreateModeSwitch(
        PlayerInventory inventory,
        ItemCatalog itemCatalog,
        StatefulItemStore statefulItems,
        SelectedItemRef? selectedItem)
    {
        var tabs = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        tabs.AddThemeConstantOverride("separation", 6);

        foreach (var mode in InventoryModes)
        {
            tabs.AddChild(CreateModeButton(mode, inventory, itemCatalog, statefulItems, selectedItem));
        }

        return tabs;
    }

    private Button CreateModeButton(
        InventoryPanelMode mode,
        PlayerInventory inventory,
        ItemCatalog itemCatalog,
        StatefulItemStore statefulItems,
        SelectedItemRef? selectedItem)
    {
        var selected = mode == _activeMode;
        var button = new Button
        {
            Text = GetModeLabel(mode),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 32),
            FocusMode = Control.FocusModeEnum.None
        };

        button.AddThemeFontSizeOverride("font_size", TabFontSize);
        button.AddThemeColorOverride("font_color", selected ? new Color(0.92f, 0.95f, 0.82f) : new Color(0.66f, 0.74f, 0.69f));
        button.AddThemeStyleboxOverride("normal", selected ? CreateSelectedStyle() : CreateTabStyle());
        button.AddThemeStyleboxOverride("hover", CreateHoverStyle());
        button.AddThemeStyleboxOverride("pressed", CreateSelectedStyle());
        button.Pressed += () =>
        {
            _activeMode = mode;
            Display(inventory, itemCatalog, statefulItems, selectedItem);
        };

        return button;
    }

    private Button CreateAmmoButton(
        InventoryItemStack stack,
        ItemCatalog itemCatalog,
        SelectedItemRef? selectedItem)
    {
        var itemRef = SelectedItemRef.InventoryStack(stack.ItemId);
        var selected = selectedItem == itemRef;
        var button = new Button
        {
            Text = $"{GetItemName(stack.ItemId, itemCatalog)} x{stack.Quantity}",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 34),
            FocusMode = Control.FocusModeEnum.None,
            Alignment = HorizontalAlignment.Left
        };

        button.AddThemeFontSizeOverride("font_size", ItemFontSize);
        button.AddThemeColorOverride("font_color", selected ? new Color(0.92f, 0.95f, 0.82f) : new Color(0.74f, 0.83f, 0.77f));
        button.AddThemeStyleboxOverride("normal", selected ? CreateSelectedStyle() : CreateTabStyle());
        button.AddThemeStyleboxOverride("hover", CreateHoverStyle());
        button.AddThemeStyleboxOverride("pressed", CreateSelectedStyle());
        button.Pressed += () => ItemSelected?.Invoke(itemRef, GetViewport().GetMousePosition());

        return button;
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

    private static bool IsLooseAmmo(ItemId itemId, ItemCatalog itemCatalog)
    {
        return itemCatalog.TryGet(itemId, out var item) && !InventoryGridRules.UsesGrid(item);
    }

    private static string GetItemName(ItemId itemId, ItemCatalog itemCatalog)
    {
        return itemCatalog.TryGet(itemId, out var item)
            ? item.DisplayName
            : itemId.ToString();
    }

    private static string GetModeLabel(InventoryPanelMode mode)
    {
        return mode == InventoryPanelMode.Inventory ? "Inventory" : "Ammo";
    }

    private static StyleBoxFlat CreateTabStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0.075f, 0.09f, 0.095f, 0.9f),
            BorderColor = new Color(0.18f, 0.25f, 0.23f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4,
            ContentMarginLeft = 6,
            ContentMarginRight = 6
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

    private static readonly IReadOnlyList<InventoryPanelMode> InventoryModes =
    [
        InventoryPanelMode.Inventory,
        InventoryPanelMode.Ammo
    ];

    private enum InventoryPanelMode
    {
        Inventory,
        Ammo
    }
}

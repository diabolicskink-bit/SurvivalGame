using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SurvivalGame.Domain;

public partial class InventoryPanel : VBoxContainer
{
    private const int ItemFontSize = 16;
    private const int TabFontSize = 15;
    private InventoryTab _activeTab = InventoryTab.Weapons;

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

        AddChild(CreateTabStrip(inventory, itemCatalog, statefulItems, selectedItem));

        var statefulInventoryItems = statefulItems.InPlayerInventory();
        if (inventory.IsEmpty && statefulInventoryItems.Count == 0)
        {
            AddChild(CreateItemLabel("Empty", muted: true));
            return;
        }

        var displayedItemCount = 0;
        foreach (var stack in inventory.Items)
        {
            if (GetTabForItem(stack.ItemId, itemCatalog) != _activeTab)
            {
                continue;
            }

            var itemRef = SelectedItemRef.InventoryStack(stack.ItemId);
            AddChild(CreateItemButton(
                FormatItemStack(stack, itemCatalog),
                itemRef,
                IsSelected(selectedItem, itemRef)
            ));
            displayedItemCount++;
        }

        foreach (var item in statefulInventoryItems)
        {
            if (GetTabForItem(item.ItemId, itemCatalog) != _activeTab)
            {
                continue;
            }

            var itemRef = SelectedItemRef.StatefulItem(item.Id);
            AddChild(CreateItemButton(
                FormatStatefulItem(item, itemCatalog),
                itemRef,
                IsSelected(selectedItem, itemRef)
            ));
            displayedItemCount++;
        }

        if (displayedItemCount == 0)
        {
            AddChild(CreateItemLabel("No items in this tab.", muted: true));
        }
    }

    private HBoxContainer CreateTabStrip(
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

        foreach (var tab in InventoryTabs)
        {
            tabs.AddChild(CreateTabButton(tab, inventory, itemCatalog, statefulItems, selectedItem));
        }

        return tabs;
    }

    private Button CreateTabButton(
        InventoryTab tab,
        PlayerInventory inventory,
        ItemCatalog itemCatalog,
        StatefulItemStore statefulItems,
        SelectedItemRef? selectedItem)
    {
        var selected = tab == _activeTab;
        var button = new Button
        {
            Text = GetTabLabel(tab),
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
            _activeTab = tab;
            Display(inventory, itemCatalog, statefulItems, selectedItem);
        };

        return button;
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

    private static InventoryTab GetTabForItem(ItemId itemId, ItemCatalog itemCatalog)
    {
        if (!itemCatalog.TryGet(itemId, out var item))
        {
            return InventoryTab.Other;
        }

        if (IsCategory(item, "Weapon"))
        {
            return InventoryTab.Weapons;
        }

        if (IsCategory(item, "Ammunition")
            || IsCategory(item, "FeedDevice")
            || HasAnyTag(item, "ammo", "ammunition", "magazine", "feed", "weapon_part"))
        {
            return InventoryTab.WeaponPartsAmmo;
        }

        if (IsCategory(item, "Food")
            || IsCategory(item, "Medical")
            || HasAnyTag(item, "food", "drink", "medical", "consumable")
            || item.Actions.Any(action => action.Contains("eat", StringComparison.OrdinalIgnoreCase)
                || action.Contains("drink", StringComparison.OrdinalIgnoreCase)
                || action.Contains("apply", StringComparison.OrdinalIgnoreCase)))
        {
            return InventoryTab.Consumables;
        }

        return InventoryTab.Other;
    }

    private static bool IsCategory(ItemDefinition item, string category)
    {
        return string.Equals(item.Category, category, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasAnyTag(ItemDefinition item, params string[] tags)
    {
        return tags.Any(item.HasTag);
    }

    private static string GetTabLabel(InventoryTab tab)
    {
        return tab switch
        {
            InventoryTab.Weapons => "Weapons",
            InventoryTab.WeaponPartsAmmo => "Weapon Parts/Ammo",
            InventoryTab.Consumables => "Consumables",
            InventoryTab.Other => "Other",
            _ => tab.ToString()
        };
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

    private static readonly IReadOnlyList<InventoryTab> InventoryTabs =
    [
        InventoryTab.Weapons,
        InventoryTab.WeaponPartsAmmo,
        InventoryTab.Consumables,
        InventoryTab.Other
    ];

    private enum InventoryTab
    {
        Weapons,
        WeaponPartsAmmo,
        Consumables,
        Other
    }
}

using System;
using Godot;
using SurvivalGame.Domain;

public partial class EquipmentPanel : VBoxContainer
{
    private const int ItemFontSize = 16;

    public event Action<SelectedItemRef, Vector2>? ItemActionRequested;
    public event Action<SelectedItemRef, Vector2>? ItemHovered;
    public event Action<SelectedItemRef>? ItemHoverEnded;

    public override void _Ready()
    {
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddThemeConstantOverride("separation", 4);
    }

    public void Display(
        EquipmentLoadout equipment,
        ItemCatalog itemCatalog,
        StatefulItemStore statefulItems,
        SelectedItemRef? selectedItem)
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        foreach (var slot in equipment.Slots)
        {
            var itemRef = GetSlotItemRef(slot, equipment, statefulItems);
            AddChild(itemRef is null
                ? CreateSlotLabel(FormatSlot(slot, equipment, itemCatalog, statefulItems), muted: true)
                : CreateSlotButton(
                    FormatSlot(slot, equipment, itemCatalog, statefulItems),
                    itemRef,
                    IsSelected(selectedItem, itemRef)
                ));
        }
    }

    private static string FormatSlot(
        EquipmentSlotDefinition slot,
        EquipmentLoadout equipment,
        ItemCatalog itemCatalog,
        StatefulItemStore statefulItems)
    {
        var statefulItem = statefulItems.EquippedIn(slot.Id);
        if (statefulItem is not null)
        {
            var statefulItemName = statefulItem.ItemId.ToString();
            if (itemCatalog.TryGet(statefulItem.ItemId, out var statefulDefinition))
            {
                statefulItemName = statefulDefinition.DisplayName;
            }

            return $"{slot.DisplayName}: {statefulItemName} [{statefulItem.Id}]";
        }

        if (!equipment.TryGetEquippedItem(slot.Id, out var equippedItem))
        {
            return $"{slot.DisplayName}: Empty";
        }

        var itemName = equippedItem.ItemId.ToString();
        if (itemCatalog.TryGet(equippedItem.ItemId, out var item))
        {
            itemName = item.DisplayName;
        }

        return $"{slot.DisplayName}: {itemName}";
    }

    private static bool IsSlotEmpty(EquipmentSlotDefinition slot, EquipmentLoadout equipment, StatefulItemStore statefulItems)
    {
        return equipment.IsEmpty(slot.Id) && statefulItems.EquippedIn(slot.Id) is null;
    }

    private static SelectedItemRef? GetSlotItemRef(
        EquipmentSlotDefinition slot,
        EquipmentLoadout equipment,
        StatefulItemStore statefulItems)
    {
        var statefulItem = statefulItems.EquippedIn(slot.Id);
        if (statefulItem is not null)
        {
            return SelectedItemRef.StatefulItem(statefulItem.Id);
        }

        return equipment.TryGetEquippedItem(slot.Id, out var equippedItem)
            ? SelectedItemRef.EquipmentItem(slot.Id, equippedItem.ItemId)
            : null;
    }

    private static Label CreateSlotLabel(string text, bool muted)
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

    private Button CreateSlotButton(string text, SelectedItemRef itemRef, bool selected)
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
        button.MouseEntered += () => ItemHovered?.Invoke(itemRef, GetViewport().GetMousePosition());
        button.MouseExited += () => ItemHoverEnded?.Invoke(itemRef);
        button.GuiInput += @event =>
        {
            if (@event is InputEventMouseMotion)
            {
                ItemHovered?.Invoke(itemRef, GetViewport().GetMousePosition());
                return;
            }

            if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
            {
                ItemActionRequested?.Invoke(itemRef, GetViewport().GetMousePosition());
                button.AcceptEvent();
            }
        };

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

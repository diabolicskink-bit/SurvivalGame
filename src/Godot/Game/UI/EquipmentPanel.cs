using Godot;
using SurvivalGame.Domain;

public partial class EquipmentPanel : VBoxContainer
{
    private const int ItemFontSize = 16;

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 4);
    }

    public void Display(EquipmentLoadout equipment, ItemCatalog itemCatalog, StatefulItemStore statefulItems)
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        foreach (var slot in equipment.Slots)
        {
            AddChild(CreateSlotLabel(FormatSlot(slot, equipment, itemCatalog, statefulItems), IsSlotEmpty(slot, equipment, statefulItems)));
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

    private static Label CreateSlotLabel(string text, bool muted)
    {
        var label = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };

        label.AddThemeFontSizeOverride("font_size", ItemFontSize);
        label.AddThemeColorOverride(
            "font_color",
            muted ? new Color(0.52f, 0.58f, 0.55f) : new Color(0.74f, 0.83f, 0.77f)
        );

        return label;
    }
}

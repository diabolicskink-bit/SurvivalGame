using Godot;
using SurvivalGame.Domain;

public partial class InventoryPanel : VBoxContainer
{
    private const int ItemFontSize = 16;

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 4);
    }

    public void Display(PlayerInventory inventory, ItemCatalog itemCatalog, StatefulItemStore statefulItems)
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
            AddChild(CreateItemLabel(FormatItemStack(stack, itemCatalog)));
        }

        foreach (var item in statefulInventoryItems)
        {
            AddChild(CreateItemLabel(FormatStatefulItem(item, itemCatalog)));
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

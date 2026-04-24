using Godot;
using SurvivalGame.Domain;

public partial class InventoryPanel : VBoxContainer
{
    private const int ItemFontSize = 16;

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 4);
    }

    public void Display(PlayerInventory inventory, ItemCatalog itemCatalog)
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        if (inventory.IsEmpty)
        {
            AddChild(CreateItemLabel("Empty", muted: true));
            return;
        }

        foreach (var stack in inventory.Items)
        {
            AddChild(CreateItemLabel(FormatItemStack(stack, itemCatalog)));
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

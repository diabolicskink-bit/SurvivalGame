using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SurvivalGame.Domain;

public partial class SelectedItemPanel : VBoxContainer
{
    private const int HeadingFontSize = 17;
    private const int BodyFontSize = 15;
    private const int ActionButtonFontSize = 16;

    public event Action<AvailableAction>? ActionSelected;

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 7);
    }

    public void Display(
        SelectedItemRef? selectedItem,
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog,
        IReadOnlyList<AvailableAction> contextualActions)
    {
        ClearRows();

        if (selectedItem is null)
        {
            AddChild(CreateLabel("No Item Selected", HeadingFontSize, new Color(0.86f, 0.9f, 0.82f)));
            AddChild(CreateLabel("Select an inventory or equipment item to see details and item actions.", BodyFontSize, new Color(0.58f, 0.66f, 0.61f)));
            return;
        }

        AddSelectedItemDetails(selectedItem, state, itemCatalog, firearmCatalog);
        AddChild(new HSeparator());
        AddChild(CreateLabel("Item Actions", HeadingFontSize, new Color(0.86f, 0.9f, 0.82f)));
        AddContextualActions(contextualActions);
    }

    private void AddSelectedItemDetails(
        SelectedItemRef selectedItem,
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
    {
        switch (selectedItem.Kind)
        {
            case SelectedItemKind.InventoryStack:
                AddInventoryStackDetails(selectedItem.ItemId!, state, itemCatalog, firearmCatalog);
                break;
            case SelectedItemKind.EquipmentItem:
                AddEquipmentItemDetails(selectedItem.ItemId!, selectedItem.EquipmentSlotId!, itemCatalog, firearmCatalog);
                break;
            case SelectedItemKind.StatefulItem:
                AddStatefulItemDetails(selectedItem.StatefulItemId!.Value, state, itemCatalog, firearmCatalog);
                break;
        }
    }

    private void AddInventoryStackDetails(
        ItemId itemId,
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
    {
        var quantity = state.Player.Inventory.CountOf(itemId);
        AddDefinitionDetails(itemId, quantity, "Inventory", itemCatalog, firearmCatalog);
    }

    private void AddEquipmentItemDetails(
        ItemId itemId,
        EquipmentSlotId slotId,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
    {
        AddDefinitionDetails(itemId, quantity: 1, $"Equipped: {slotId}", itemCatalog, firearmCatalog);
    }

    private void AddStatefulItemDetails(
        StatefulItemId itemId,
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
    {
        if (!state.StatefulItems.TryGet(itemId, out var item))
        {
            AddChild(CreateLabel("Missing item", HeadingFontSize, new Color(0.9f, 0.73f, 0.65f)));
            AddChild(CreateLabel($"Stateful item {itemId} is no longer tracked.", BodyFontSize, new Color(0.72f, 0.62f, 0.58f)));
            return;
        }

        AddDefinitionDetails(item.ItemId, item.Quantity, FormatLocation(item.Location), itemCatalog, firearmCatalog);
        AddChild(CreateLabel($"Runtime id: {item.Id}", BodyFontSize, new Color(0.6f, 0.68f, 0.64f)));
        AddChild(CreateLabel($"Condition: {item.Condition}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));

        if (item.FeedDevice is not null)
        {
            AddFeedDetails(item.FeedDevice);
        }

        if (item.Weapon is not null)
        {
            AddWeaponStateDetails(item, state);
        }

        var contents = state.StatefulItems.ContainedIn(item.Id);
        if (contents.Count == 0)
        {
            return;
        }

        var names = contents.Select(content => GetItemName(content.ItemId, itemCatalog));
        AddChild(CreateLabel($"Contents: {string.Join(", ", names)}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
    }

    private void AddDefinitionDetails(
        ItemId itemId,
        int quantity,
        string location,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
    {
        var name = GetItemName(itemId, itemCatalog);
        AddChild(CreateLabel(name, HeadingFontSize, new Color(0.9f, 0.93f, 0.84f)));
        AddChild(CreateLabel($"Quantity: {quantity}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
        AddChild(CreateLabel($"Location: {location}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));

        if (itemCatalog.TryGet(itemId, out var definition))
        {
            AddChild(CreateLabel($"Category: {definition.Category}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            AddChild(CreateLabel($"Type: {definition.TypePath}", BodyFontSize, new Color(0.6f, 0.68f, 0.64f)));

            if (definition.Tags.Count > 0)
            {
                AddChild(CreateLabel($"Tags: {string.Join(", ", definition.Tags)}", BodyFontSize, new Color(0.6f, 0.68f, 0.64f)));
            }

            if (!string.IsNullOrWhiteSpace(definition.Description))
            {
                AddChild(CreateLabel(definition.Description, BodyFontSize, new Color(0.68f, 0.75f, 0.7f)));
            }
        }

        AddFirearmDefinitionDetails(itemId, firearmCatalog);
    }

    private void AddFirearmDefinitionDetails(ItemId itemId, FirearmCatalog firearmCatalog)
    {
        if (firearmCatalog.TryGetAmmunition(itemId, out var ammunition))
        {
            AddChild(CreateLabel($"Ammunition: {ammunition.Size}, {ammunition.Variant}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
        }

        if (firearmCatalog.TryGetFeedDevice(itemId, out var feedDevice))
        {
            AddChild(CreateLabel($"Feed: {feedDevice.Kind}, {feedDevice.Capacity} rounds, accepts {feedDevice.AmmoSize}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
        }

        if (firearmCatalog.TryGetWeapon(itemId, out var weapon))
        {
            var acceptedAmmo = string.Join(", ", weapon.AcceptedAmmoSizes);
            AddChild(CreateLabel($"Weapon: accepts {acceptedAmmo}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            AddChild(CreateLabel($"Range: {weapon.EffectiveRangeTiles} effective / {weapon.MaximumRangeTiles} max tiles", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            AddChild(CreateLabel($"Feed type: {weapon.FeedKind}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
        }
    }

    private void AddFeedDetails(FeedDeviceState feedDevice)
    {
        var loadedText = feedDevice.LoadedAmmunitionVariant is null
            ? $"0/{feedDevice.Capacity}"
            : $"{feedDevice.LoadedCount}/{feedDevice.Capacity} {feedDevice.LoadedAmmunitionVariant}";

        AddChild(CreateLabel($"Loaded: {loadedText}", BodyFontSize, new Color(0.8f, 0.85f, 0.75f)));
        AddChild(CreateLabel($"Accepts: {feedDevice.AmmoSize}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
    }

    private void AddWeaponStateDetails(StatefulItem item, PrototypeGameState state)
    {
        var activeFeed = item.Weapon?.BuiltInFeed;
        if (activeFeed is null && item.Weapon?.InsertedFeedDeviceItemId is not null)
        {
            activeFeed = state.StatefulItems.TryGet(item.Weapon.InsertedFeedDeviceItemId.Value, out var insertedItem)
                ? insertedItem.FeedDevice
                : null;
        }

        if (item.Weapon?.InsertedFeedDeviceItemId is not null)
        {
            AddChild(CreateLabel($"Inserted feed: {item.Weapon.InsertedFeedDeviceItemId}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
        }

        if (activeFeed is null)
        {
            AddChild(CreateLabel("Loaded: empty", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            return;
        }

        AddFeedDetails(activeFeed);
    }

    private void AddContextualActions(IReadOnlyList<AvailableAction> contextualActions)
    {
        if (contextualActions.Count == 0)
        {
            AddChild(CreateLabel("No item actions available.", BodyFontSize, new Color(0.58f, 0.66f, 0.61f)));
            return;
        }

        foreach (var action in contextualActions)
        {
            var button = new Button
            {
                Text = action.Label,
                CustomMinimumSize = new Vector2(0, 34),
                FocusMode = Control.FocusModeEnum.None
            };
            button.AddThemeFontSizeOverride("font_size", ActionButtonFontSize);
            button.Pressed += () => ActionSelected?.Invoke(action);
            AddChild(button);
        }
    }

    private void ClearRows()
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }
    }

    private static string GetItemName(ItemId itemId, ItemCatalog itemCatalog)
    {
        return itemCatalog.TryGet(itemId, out var definition)
            ? definition.DisplayName
            : itemId.ToString();
    }

    private static string FormatLocation(StatefulItemLocation location)
    {
        return location.Kind switch
        {
            StatefulItemLocationKind.PlayerInventory => "Inventory",
            StatefulItemLocationKind.Ground => $"Ground {location.Position?.X}, {location.Position?.Y}",
            StatefulItemLocationKind.Equipment => $"Equipped: {location.EquipmentSlotId}",
            StatefulItemLocationKind.Inserted => $"Inserted in {location.ParentItemId}",
            StatefulItemLocationKind.Contained => $"Inside {location.ParentItemId}",
            _ => location.Kind.ToString()
        };
    }

    private static Label CreateLabel(string text, int fontSize, Color color)
    {
        var label = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }
}

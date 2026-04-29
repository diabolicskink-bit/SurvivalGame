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
    private const float MinimumContentWidth = 320.0f;

    public event Action<AvailableAction>? ActionSelected;

    public override void _Ready()
    {
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        CustomMinimumSize = new Vector2(MinimumContentWidth, 0);
        AddThemeConstantOverride("separation", 7);
    }

    public void DisplayActions(
        SelectedItemRef? selectedItem,
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        IReadOnlyList<AvailableAction> contextualActions)
    {
        ClearRows();

        if (selectedItem is null)
        {
            AddChild(CreateLabel("No Item Selected", HeadingFontSize, new Color(0.86f, 0.9f, 0.82f)));
            AddChild(CreateLabel("Select an inventory or equipment item to see details and item actions.", BodyFontSize, new Color(0.58f, 0.66f, 0.61f)));
            return;
        }

        AddChild(CreateLabel("Item Actions", HeadingFontSize, new Color(0.86f, 0.9f, 0.82f)));
        AddContextualActions(contextualActions, selectedItem, state, itemCatalog);
    }

    public void DisplayDetails(
        SelectedItemRef? selectedItem,
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
    {
        ClearRows();

        if (selectedItem is null)
        {
            return;
        }

        AddSelectedItemDetails(selectedItem, state, itemCatalog, firearmCatalog);
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
                AddEquipmentItemDetails(selectedItem.ItemId!, selectedItem.EquipmentSlotId!, state, itemCatalog, firearmCatalog);
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
        AddDefinitionDetails(itemId, quantity, "Inventory", itemCatalog, firearmCatalog, state);
    }

    private void AddEquipmentItemDetails(
        ItemId itemId,
        EquipmentSlotId slotId,
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
    {
        AddDefinitionDetails(itemId, quantity: 1, $"Equipped: {slotId}", itemCatalog, firearmCatalog, state);
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

        if (item.FuelContainer is not null)
        {
            AddChild(CreateLabel(
                $"Fuel: {item.FuelContainer.CurrentFuel:0.0}/{item.FuelContainer.Capacity:0.0}",
                BodyFontSize,
                new Color(0.8f, 0.85f, 0.75f)
            ));
        }

        if (item.Weapon is not null)
        {
            AddWeaponStateDetails(item, state, itemCatalog, firearmCatalog);
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
        FirearmCatalog firearmCatalog,
        PrototypeGameState? state = null)
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

        AddFirearmDefinitionDetails(itemId, firearmCatalog, state);
    }

    private void AddFirearmDefinitionDetails(ItemId itemId, FirearmCatalog firearmCatalog, PrototypeGameState? state)
    {
        if (firearmCatalog.TryGetAmmunition(itemId, out var ammunition))
        {
            AddChild(CreateLabel($"Ammunition: {ammunition.Size}, {ammunition.Variant}, {ammunition.Damage} damage", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
        }

        if (firearmCatalog.TryGetFeedDevice(itemId, out var feedDevice))
        {
            AddChild(CreateLabel($"Feed: {feedDevice.Kind}, {feedDevice.Capacity} rounds, accepts {feedDevice.AmmoSize}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
        }

        if (firearmCatalog.TryGetWeapon(itemId, out var weapon))
        {
            var acceptedAmmo = string.Join(", ", weapon.AcceptedAmmoSizes);
            var fireModes = string.Join(", ", weapon.SupportedFireModes.Select(WeaponFireModeNames.Format));
            AddChild(CreateLabel($"Weapon: accepts {acceptedAmmo}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            AddChild(CreateLabel($"Range: {weapon.EffectiveRangeTiles} effective / {weapon.MaximumRangeTiles} max tiles", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            AddChild(CreateLabel($"Feed type: {weapon.FeedKind}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            AddChild(CreateLabel($"Fire modes: {fireModes}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            if (weapon.SupportsFireMode(WeaponFireMode.Burst))
            {
                AddChild(CreateLabel($"Burst: {weapon.BurstRoundCount} rounds, x{weapon.BurstDamageMultiplier} damage", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            }

            if (state is not null)
            {
                AddChild(CreateLabel($"Current mode: {WeaponFireModeNames.Format(GetStackWeaponFireMode(state, itemId))}", BodyFontSize, new Color(0.8f, 0.85f, 0.75f)));
            }
        }

        if (firearmCatalog.TryGetWeaponMod(itemId, out var weaponMod))
        {
            AddChild(CreateLabel($"Mod slot: {weaponMod.Slot}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            AddChild(CreateLabel($"Effects: {FormatModEffects(weaponMod)}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
            AddChild(CreateLabel($"Fits: {string.Join(", ", weaponMod.CompatibleWeaponFamilies)}", BodyFontSize, new Color(0.6f, 0.68f, 0.64f)));
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

    private void AddWeaponStateDetails(
        StatefulItem item,
        PrototypeGameState state,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
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

        AddChild(CreateLabel($"Current mode: {WeaponFireModeNames.Format(item.Weapon?.CurrentFireMode ?? WeaponFireMode.SingleShot)}", BodyFontSize, new Color(0.8f, 0.85f, 0.75f)));

        if (activeFeed is null)
        {
            AddChild(CreateLabel("Loaded: empty", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
        }
        else
        {
            AddFeedDetails(activeFeed);
        }

        if (item.Weapon is null || !firearmCatalog.TryGetWeapon(item.ItemId, out var weapon))
        {
            return;
        }

        var modifiedStats = WeaponModState.GetModifiedStats(weapon, item.Weapon, state.StatefulItems, firearmCatalog);
        AddChild(CreateLabel(
            $"Modified range: {modifiedStats.EffectiveRangeTiles} effective / {modifiedStats.MaximumRangeTiles} max tiles",
            BodyFontSize,
            new Color(0.72f, 0.8f, 0.74f)
        ));
        AddChild(CreateLabel(
            $"Damage bonus: {FormatSigned(modifiedStats.DamageBonus)}",
            BodyFontSize,
            new Color(0.72f, 0.8f, 0.74f)
        ));

        if (item.Weapon.InstalledMods.Count == 0)
        {
            AddChild(CreateLabel("Mods: none", BodyFontSize, new Color(0.6f, 0.68f, 0.64f)));
            return;
        }

        foreach (var installedMod in item.Weapon.InstalledMods.OrderBy(mod => mod.Key.Value, StringComparer.OrdinalIgnoreCase))
        {
            var modName = state.StatefulItems.TryGet(installedMod.Value, out var modItem)
                ? GetStatefulItemName(modItem.Id, state, itemCatalog)
                : installedMod.Value.ToString();
            AddChild(CreateLabel($"Mod {installedMod.Key}: {modName}", BodyFontSize, new Color(0.72f, 0.8f, 0.74f)));
        }
    }

    private void AddContextualActions(
        IReadOnlyList<AvailableAction> contextualActions,
        SelectedItemRef selectedItem,
        PrototypeGameState state,
        ItemCatalog itemCatalog)
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
                Text = FormatContextualActionLabel(action, selectedItem, state, itemCatalog),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 34),
                FocusMode = Control.FocusModeEnum.None
            };
            button.AddThemeFontSizeOverride("font_size", ActionButtonFontSize);
            button.Pressed += () => ActionSelected?.Invoke(action);
            AddChild(button);
        }
    }

    private static string FormatContextualActionLabel(
        AvailableAction action,
        SelectedItemRef selectedItem,
        PrototypeGameState state,
        ItemCatalog itemCatalog)
    {
        return action.Request switch
        {
            InspectItemActionRequest => "Inspect",
            DropItemStackActionRequest dropStack => dropStack.Quantity == 1 ? "Drop one" : $"Drop {dropStack.Quantity}",
            StowItemStackInTravelCargoActionRequest stowStack => stowStack.Quantity == 1 ? "Stow one" : $"Stow {stowStack.Quantity}",
            EquipItemActionRequest equip => $"Equip ({GetSlotDisplayName(state, equip.SlotId)})",
            UnequipItemActionRequest => "Unequip",
            InspectStatefulItemActionRequest => "Inspect",
            DropStatefulItemActionRequest => "Drop",
            StowStatefulItemInTravelCargoActionRequest => "Stow in cargo",
            FillFuelCanActionRequest => "Fill",
            PourFuelCanIntoVehicleActionRequest => "Pour into vehicle",
            EquipStatefulItemActionRequest equip => $"Equip ({GetSlotDisplayName(state, equip.SlotId)})",
            UnequipStatefulItemActionRequest => "Unequip",
            PickupStatefulItemActionRequest => "Pick up",

            LoadFeedDeviceActionRequest load => IsSelectedStackItem(selectedItem, load.FeedDeviceItemId)
                ? $"Load {GetItemName(load.AmmunitionItemId, itemCatalog)}"
                : $"Load into {GetItemName(load.FeedDeviceItemId, itemCatalog)}",
            UnloadFeedDeviceActionRequest => "Unload",
            InsertFeedDeviceActionRequest insert => IsSelectedStackItem(selectedItem, insert.WeaponItemId)
                ? $"Insert {GetItemName(insert.FeedDeviceItemId, itemCatalog)}"
                : $"Insert into {GetItemName(insert.WeaponItemId, itemCatalog)}",
            RemoveFeedDeviceActionRequest => "Remove feed",
            LoadWeaponActionRequest loadWeapon => IsSelectedStackItem(selectedItem, loadWeapon.WeaponItemId)
                ? $"Load {GetItemName(loadWeapon.AmmunitionItemId, itemCatalog)}"
                : $"Load into {GetItemName(loadWeapon.WeaponItemId, itemCatalog)}",
            ReloadWeaponActionRequest reloadWeapon => IsSelectedStackItem(selectedItem, reloadWeapon.WeaponItemId)
                ? $"Reload with {GetItemName(reloadWeapon.AmmunitionItemId, itemCatalog)}"
                : $"Reload {GetItemName(reloadWeapon.WeaponItemId, itemCatalog)}",
            TestFireActionRequest => "Test fire",
            ToggleFireModeActionRequest => "Switch fire mode",

            LoadStatefulFeedDeviceActionRequest loadStatefulFeed => $"Load {GetItemName(loadStatefulFeed.AmmunitionItemId, itemCatalog)}",
            UnloadStatefulFeedDeviceActionRequest => "Unload",
            InsertStatefulFeedDeviceActionRequest insertStatefulFeed => IsSelectedStatefulItem(selectedItem, insertStatefulFeed.WeaponItemId)
                ? $"Insert {GetStatefulItemName(insertStatefulFeed.FeedDeviceItemId, state, itemCatalog)}"
                : $"Insert into {GetStatefulItemName(insertStatefulFeed.WeaponItemId, state, itemCatalog)}",
            RemoveStatefulFeedDeviceActionRequest => "Remove feed",
            LoadStatefulWeaponActionRequest loadStatefulWeapon => $"Load {GetItemName(loadStatefulWeapon.AmmunitionItemId, itemCatalog)}",
            ReloadStatefulWeaponActionRequest reloadStatefulWeapon => IsSelectedStatefulItem(selectedItem, reloadStatefulWeapon.WeaponItemId)
                ? $"Reload with {GetItemName(reloadStatefulWeapon.AmmunitionItemId, itemCatalog)}"
                : $"Reload {GetStatefulItemName(reloadStatefulWeapon.WeaponItemId, state, itemCatalog)}",
            TestFireStatefulWeaponActionRequest => "Test fire",
            ToggleStatefulFireModeActionRequest => "Switch fire mode",
            InstallStatefulWeaponModActionRequest installWeaponMod => IsSelectedStatefulItem(selectedItem, installWeaponMod.WeaponItemId)
                ? $"Install {GetStatefulItemName(installWeaponMod.ModItemId, state, itemCatalog)}"
                : $"Install into {GetStatefulItemName(installWeaponMod.WeaponItemId, state, itemCatalog)}",
            RemoveStatefulWeaponModActionRequest removeWeaponMod => $"Remove {removeWeaponMod.SlotId} mod",

            _ => action.Label
        };
    }

    private static bool IsSelectedStackItem(SelectedItemRef selectedItem, ItemId itemId)
    {
        return selectedItem.Kind is SelectedItemKind.InventoryStack or SelectedItemKind.EquipmentItem
            && selectedItem.ItemId == itemId;
    }

    private static bool IsSelectedStatefulItem(SelectedItemRef selectedItem, StatefulItemId itemId)
    {
        return selectedItem.Kind == SelectedItemKind.StatefulItem
            && selectedItem.StatefulItemId == itemId;
    }

    private static string GetStatefulItemName(
        StatefulItemId itemId,
        PrototypeGameState state,
        ItemCatalog itemCatalog)
    {
        return state.StatefulItems.TryGet(itemId, out var item)
            ? GetItemName(item.ItemId, itemCatalog)
            : itemId.ToString();
    }

    private static string GetSlotDisplayName(PrototypeGameState state, EquipmentSlotId slotId)
    {
        return state.Player.Equipment.SlotCatalog.TryGet(slotId, out var slot)
            ? slot.DisplayName
            : slotId.ToString();
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
        return location switch
        {
            PlayerInventoryLocation => "Inventory",
            GroundLocation g => $"Ground {g.Position.X}, {g.Position.Y}",
            EquipmentLocation e => $"Equipped: {e.SlotId}",
            InsertedLocation i => $"Inserted in {i.ParentItemId}",
            ContainedLocation c => $"Inside {c.ParentItemId}",
            TravelCargoLocation => "Travel cargo",
            _ => location.Kind.ToString()
        };
    }

    private static string FormatModEffects(WeaponModDefinition mod)
    {
        var effects = new List<string>();
        AddSignedEffect(effects, "effective range", mod.EffectiveRangeBonus);
        AddSignedEffect(effects, "max range", mod.MaximumRangeBonus);
        AddSignedEffect(effects, "damage", mod.DamageBonus);
        return effects.Count == 0 ? "none" : string.Join(", ", effects);
    }

    private static WeaponFireMode GetStackWeaponFireMode(PrototypeGameState state, ItemId itemId)
    {
        return state.Player.Firearms.TryGetWeapon(itemId, out var weaponState)
            ? weaponState.CurrentFireMode
            : WeaponFireMode.SingleShot;
    }

    private static void AddSignedEffect(List<string> effects, string label, int value)
    {
        if (value == 0)
        {
            return;
        }

        effects.Add($"{label} {FormatSigned(value)}");
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private static Label CreateLabel(string text, int fontSize, Color color)
    {
        var label = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(MinimumContentWidth, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }
}

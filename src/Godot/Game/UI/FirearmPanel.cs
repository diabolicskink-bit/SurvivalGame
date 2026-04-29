using System.Linq;
using Godot;
using SurvivalGame.Domain;

public partial class FirearmPanel : VBoxContainer
{
    private const int ItemFontSize = 15;

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 4);
    }

    public void Display(PlayerState player, FirearmCatalog firearmCatalog, StatefulItemStore statefulItems)
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        var hasAnyState = false;
        foreach (var item in statefulItems.Items.Where(item => item.Weapon is not null))
        {
            hasAnyState = true;
            AddChild(CreateLine(FormatStatefulWeapon(statefulItems, item, firearmCatalog), muted: false));
        }

        foreach (var item in statefulItems.Items.Where(item => item.FeedDevice is not null))
        {
            hasAnyState = true;
            AddChild(CreateLine(FormatStatefulFeedDevice(item), muted: item.FeedDevice?.IsEmpty != false));
        }

        foreach (var weapon in firearmCatalog.Weapons)
        {
            if (!PlayerOwnsOrTracksWeapon(player, weapon))
            {
                continue;
            }

            hasAnyState = true;
            AddChild(CreateLine(FormatWeapon(player, weapon), muted: false));
        }

        foreach (var feedDevice in player.Firearms.FeedDevices)
        {
            hasAnyState = true;
            AddChild(CreateLine(FormatFeedDevice(player, feedDevice), muted: feedDevice.IsEmpty));
        }

        if (!hasAnyState)
        {
            AddChild(CreateLine("No firearms tracked", muted: true));
        }
    }

    private static string FormatStatefulWeapon(StatefulItemStore statefulItems, StatefulItem item, FirearmCatalog firearmCatalog)
    {
        var name = firearmCatalog.TryGetWeapon(item.ItemId, out var weapon)
            ? weapon.Name
            : item.ItemId.ToString();

        if (item.Weapon is null)
        {
            return $"{name} [{item.Id}]: no weapon state";
        }

        var mode = WeaponFireModeNames.Format(item.Weapon.CurrentFireMode);

        var activeFeed = item.Weapon.BuiltInFeed;
        if (activeFeed is null && item.Weapon.InsertedFeedDeviceItemId is not null)
        {
            activeFeed = statefulItems.Get(item.Weapon.InsertedFeedDeviceItemId.Value).FeedDevice;
        }

        if (activeFeed is null)
        {
            return $"{name} [{item.Id}]: no feed inserted ({mode})";
        }

        var loadedText = activeFeed.LoadedAmmunitionVariant is null
            ? "empty"
            : $"{activeFeed.LoadedCount}/{activeFeed.Capacity} {activeFeed.LoadedAmmunitionVariant}";

        return $"{name} [{item.Id}]: {loadedText} ({mode})";
    }

    private static string FormatStatefulFeedDevice(StatefulItem item)
    {
        var feedDevice = item.FeedDevice!;
        var location = item.Location is InsertedLocation
            ? "inserted"
            : item.Location is GroundLocation
                ? "ground"
                : "carried";
        var loadedText = feedDevice.LoadedAmmunitionVariant is null
            ? $"0/{feedDevice.Capacity}"
            : $"{feedDevice.LoadedCount}/{feedDevice.Capacity} {feedDevice.LoadedAmmunitionVariant}";

        return $"{feedDevice.DisplayName} [{item.Id}]: {loadedText} ({location})";
    }

    private static bool PlayerOwnsOrTracksWeapon(PlayerState player, WeaponDefinition weapon)
    {
        return player.Inventory.CountOf(weapon.ItemId) > 0
            || player.Equipment.ContainsItem(weapon.ItemId)
            || player.Firearms.TryGetWeapon(weapon.ItemId, out _);
    }

    private static string FormatWeapon(PlayerState player, WeaponDefinition weapon)
    {
        if (!player.Firearms.TryGetWeapon(weapon.ItemId, out var weaponState))
        {
            return $"{weapon.Name}: empty (single shot)";
        }

        var feedDevice = player.Firearms.GetActiveFeedForWeapon(weaponState);
        var mode = WeaponFireModeNames.Format(weaponState.CurrentFireMode);
        if (feedDevice is null)
        {
            return $"{weapon.Name}: no feed inserted ({mode})";
        }

        var loadedText = feedDevice.LoadedAmmunitionVariant is null
            ? "empty"
            : $"{feedDevice.LoadedCount}/{feedDevice.Capacity} {feedDevice.LoadedAmmunitionVariant}";

        if (weaponState.InsertedFeedDeviceItemId is not null)
        {
            return $"{weapon.Name}: {loadedText} in {feedDevice.DisplayName} ({mode})";
        }

        return $"{weapon.Name}: {loadedText} ({mode})";
    }

    private static string FormatFeedDevice(PlayerState player, FeedDeviceState feedDevice)
    {
        var location = player.Firearms.IsFeedDeviceInserted(feedDevice.SourceItemId)
            ? "inserted"
            : "carried";

        var loadedText = feedDevice.LoadedAmmunitionVariant is null
            ? $"0/{feedDevice.Capacity}"
            : $"{feedDevice.LoadedCount}/{feedDevice.Capacity} {feedDevice.LoadedAmmunitionVariant}";

        return $"{feedDevice.DisplayName}: {loadedText} ({location})";
    }

    private static Label CreateLine(string text, bool muted)
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

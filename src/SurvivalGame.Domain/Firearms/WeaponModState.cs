namespace SurvivalGame.Domain;

public static class WeaponModState
{
    public static IReadOnlyList<WeaponModDefinition> GetInstalledMods(
        StatefulWeaponState weapon,
        StatefulItemStore items,
        FirearmCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(catalog);

        return weapon.InstalledMods
            .OrderBy(mod => mod.Key.Value, StringComparer.OrdinalIgnoreCase)
            .Select(mod => items.TryGet(mod.Value, out var modItem)
                && catalog.TryGetWeaponMod(modItem.ItemId, out var definition)
                    ? definition
                    : null)
            .Where(definition => definition is not null)
            .Cast<WeaponModDefinition>()
            .ToArray();
    }

    public static ModifiedWeaponStats GetModifiedStats(
        WeaponDefinition weapon,
        StatefulWeaponState? weaponState,
        StatefulItemStore items,
        FirearmCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(catalog);

        var mods = weaponState is null
            ? Array.Empty<WeaponModDefinition>()
            : GetInstalledMods(weaponState, items, catalog);

        return ModifiedWeaponStats.From(weapon, mods);
    }
}

namespace SurvivalGame.Domain;

public sealed record ModifiedWeaponStats(
    int EffectiveRangeTiles,
    int MaximumRangeTiles,
    int DamageBonus)
{
    public static ModifiedWeaponStats From(
        WeaponDefinition weapon,
        IEnumerable<WeaponModDefinition> weaponMods)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        ArgumentNullException.ThrowIfNull(weaponMods);

        var mods = weaponMods.ToArray();
        var effectiveRange = Math.Max(1, weapon.EffectiveRangeTiles + mods.Sum(mod => mod.EffectiveRangeBonus));
        var maximumRange = Math.Max(effectiveRange, weapon.MaximumRangeTiles + mods.Sum(mod => mod.MaximumRangeBonus));
        var damageBonus = mods.Sum(mod => mod.DamageBonus);

        return new ModifiedWeaponStats(effectiveRange, maximumRange, damageBonus);
    }

    public int ModifyDamage(int baseDamage)
    {
        return Math.Max(0, baseDamage + DamageBonus);
    }
}

namespace SurvivalGame.Domain;

public sealed record ModifiedWeaponStats(
    int EffectiveRangeTiles,
    int MaximumRangeTiles,
    int EffectiveRangeAccuracyPercent,
    int MaximumRangeAccuracyPercent,
    int DamageBonus)
{
    private const int MinimumHitChancePercent = 5;
    private const int MaximumHitChancePercent = 95;

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
        var accuracyBonus = mods.Sum(mod => mod.AccuracyBonus);
        var effectiveAccuracy = ClampAccuracy(weapon.EffectiveRangeAccuracyPercent + accuracyBonus);
        var maximumAccuracy = ClampAccuracy(weapon.MaximumRangeAccuracyPercent + accuracyBonus);

        return new ModifiedWeaponStats(effectiveRange, maximumRange, effectiveAccuracy, maximumAccuracy, damageBonus);
    }

    public int ModifyDamage(int baseDamage)
    {
        return Math.Max(0, baseDamage + DamageBonus);
    }

    public int GetHitChancePercent(int distanceTiles)
    {
        var unclampedChance = distanceTiles <= EffectiveRangeTiles || MaximumRangeTiles == EffectiveRangeTiles
            ? EffectiveRangeAccuracyPercent
            : InterpolateHitChance(distanceTiles);

        return ClampHitChance(unclampedChance);
    }

    private int InterpolateHitChance(int distanceTiles)
    {
        var clampedDistance = Math.Clamp(distanceTiles, EffectiveRangeTiles, MaximumRangeTiles);
        var rangeSpan = MaximumRangeTiles - EffectiveRangeTiles;
        var distanceBeyondEffective = clampedDistance - EffectiveRangeTiles;
        var t = distanceBeyondEffective / (double)rangeSpan;
        var chance = EffectiveRangeAccuracyPercent
            + ((MaximumRangeAccuracyPercent - EffectiveRangeAccuracyPercent) * t);

        return (int)Math.Round(chance, MidpointRounding.AwayFromZero);
    }

    private static int ClampAccuracy(int value)
    {
        return Math.Clamp(value, 0, 100);
    }

    private static int ClampHitChance(int value)
    {
        return Math.Clamp(value, MinimumHitChancePercent, MaximumHitChancePercent);
    }
}

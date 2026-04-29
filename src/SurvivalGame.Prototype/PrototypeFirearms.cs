namespace SurvivalGame.Domain;

public static class PrototypeFirearms
{
    public static readonly AmmoSizeId NineMillimeter = new("9mm");
    public static readonly AmmoSizeId SevenSixTwoByThirtyNine = new("7.62x39mm");
    public static readonly AmmoSizeId FiveFiveSix = new("5.56x45mm");
    public static readonly AmmoSizeId ThreeOhEight = new(".308");
    public static readonly AmmoSizeId TwelveGauge = new("12 gauge");
    public static readonly AmmoSizeId TwentyTwoLongRifle = new(".22 LR");

    public static readonly ItemId Pistol9mm = new("pistol_9mm");
    public static readonly ItemId Carbine556 = new("carbine_556");
    public static readonly ItemId Shotgun12Gauge = new("shotgun_12_gauge");
    public static readonly ItemId Rifle22 = new("rifle_22lr");

    public static readonly ItemId Ammo9mmStandard = new("ammo_9mm_standard");
    public static readonly ItemId Ammo9mmHollowPoint = new("ammo_9mm_hollow_point");
    public static readonly ItemId Ammo762x39Standard = new("ammo_762x39_standard");
    public static readonly ItemId Ammo556Standard = new("ammo_556_standard");
    public static readonly ItemId Ammo308Standard = new("ammo_308_standard");
    public static readonly ItemId Ammo12GaugeBuckshot = new("ammo_12_gauge_buckshot");
    public static readonly ItemId Ammo12GaugeSlug = new("ammo_12_gauge_slug");
    public static readonly ItemId Ammo22LrStandard = new("ammo_22lr_standard");

    public static readonly ItemId Magazine9mmStandard = new("magazine_9mm_standard");
    public static readonly ItemId Magazine9mmExtended = new("magazine_9mm_extended");
    public static readonly ItemId MagazineAk30Round = new("magazine_ak_30_round");
    public static readonly ItemId MagazineAkDamaged20Round = new("magazine_ak_damaged_20_round");
    public static readonly ItemId Magazine55630Round = new("magazine_556_30_round");

    public static readonly WeaponModSlotId OpticSlot = new("optic");
    public static readonly WeaponModSlotId BarrelSlot = new("barrel");

    public static readonly ItemId RedDotSight = new("red_dot_sight");
    public static readonly ItemId HuntingScope = new("hunting_scope");
    public static readonly ItemId MatchBarrel = new("match_barrel");
}

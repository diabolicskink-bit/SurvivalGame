namespace SurvivalGame.Domain;

public static class PrototypeItems
{
    public static readonly ItemTypePath Material = new("Material");
    public static readonly ItemTypePath Food = new("Food");
    public static readonly ItemTypePath Medical = new("Medical");
    public static readonly ItemTypePath Weapon = new("Weapon");
    public static readonly ItemTypePath Gun = new("Weapon", "Gun");
    public static readonly ItemTypePath Rifle = new("Weapon", "Gun", "Rifle");
    public static readonly ItemTypePath Clothing = new("Clothing");
    public static readonly ItemTypePath HeadClothing = new("Clothing", "Head");
    public static readonly ItemTypePath FeetClothing = new("Clothing", "Feet");

    public static readonly ItemId Stone = new("stone");
    public static readonly ItemId Branch = new("branch");
    public static readonly ItemId WaterBottle = new("water_bottle");
    public static readonly ItemId Ak47 = new("ak47");
    public static readonly ItemId HuntingRifle = new("hunting_rifle");
    public static readonly ItemId BaseballCap = new("baseball_cap");
    public static readonly ItemId RunningShoes = new("running_shoes");
}

namespace SurvivalGame.Domain;

public sealed record LoadedAmmunition(ItemId ItemId, AmmoSizeId Size, string Variant, int Quantity);

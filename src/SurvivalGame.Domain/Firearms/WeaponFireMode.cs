namespace SurvivalGame.Domain;

public enum WeaponFireMode
{
    SingleShot,
    Burst
}

public static class WeaponFireModeNames
{
    public static string Format(WeaponFireMode mode)
    {
        return mode switch
        {
            WeaponFireMode.SingleShot => "single shot",
            WeaponFireMode.Burst => "burst",
            _ => mode.ToString()
        };
    }
}

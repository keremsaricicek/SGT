namespace Sgt.Modules.Loyalty.Domain;

public enum Tier
{
    Bronze,
    Silver,
    Gold,
    Platinum,
}

/// <summary>Seviye eşikleri: toplam kazanılan puana göre (Spec 008 KK4).</summary>
public static class Tiers
{
    public static Tier For(long lifetimeEarned) => lifetimeEarned switch
    {
        >= 20_000 => Tier.Platinum,
        >= 5_000 => Tier.Gold,
        >= 1_000 => Tier.Silver,
        _ => Tier.Bronze,
    };
}

namespace Sgt.BuildingBlocks;

/// <summary>Müşteri şirket. Her veri erişimi bir tenant kapsamındadır.</summary>
public readonly record struct TenantId(Guid Value);

/// <summary>Sistem kullanıcısı (personel).</summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
}

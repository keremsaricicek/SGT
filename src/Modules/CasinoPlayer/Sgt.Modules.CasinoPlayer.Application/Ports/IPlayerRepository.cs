using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Search;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Application.Ports;

public interface IPlayerRepository
{
    Task<Player?> Get(TenantId tenantId, PlayerId playerId, CancellationToken ct);

    /// <summary>Tenant içinde bir sonraki oyuncu sırası (KK3).</summary>
    Task<long> NextSequence(TenantId tenantId, CancellationToken ct);

    Task<bool> DocumentInUse(TenantId tenantId, DocumentType type, string number, PlayerId? except, CancellationToken ct);

    Task<bool> CardInUse(TenantId tenantId, CardNumber number, CancellationToken ct);

    /// <summary>Oyuncu no, kart no, belge no (tam) veya ad/soyad başlangıcı (KK5).</summary>
    Task<IReadOnlyList<PlayerSummary>> Search(TenantId tenantId, string term, int take, CancellationToken ct);

    void Add(Player player);
}

/// <summary>Kaydederken bekleyen oyuncu değişikliklerini olay olarak outbox'a yazar.</summary>
public interface IPlayerUnitOfWork : IUnitOfWork
{
}

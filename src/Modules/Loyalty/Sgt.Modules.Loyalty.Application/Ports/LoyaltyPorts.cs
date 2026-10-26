using Sgt.BuildingBlocks;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Application.Ports;

public interface ILoyaltyAccountRepository
{
    Task<LoyaltyAccount?> Get(TenantId tenantId, Guid playerId, CancellationToken ct);

    void Add(LoyaltyAccount account);
}

public interface IApprovalRequestRepository
{
    Task<CompRequest?> GetComp(TenantId tenantId, Guid requestId, CancellationToken ct);

    Task<PointsAdjustmentRequest?> GetAdjustment(TenantId tenantId, Guid requestId, CancellationToken ct);

    void Add(CompRequest request);

    void Add(PointsAdjustmentRequest request);
}

public interface IEarningRuleStore
{
    /// <summary>Tanımlı kural yoksa varsayılan (100 TRY = 1 puan).</summary>
    Task<EarningRule> Current(TenantId tenantId, CancellationToken ct);

    /// <summary>Kuralı kaydeder ve değişikliği denetime yazar.</summary>
    void Save(EarningRule rule, AuditContext audit);
}

public interface ILoyaltyInbox : IInbox
{
}

public interface ILoyaltyUnitOfWork : IUnitOfWork
{
}

using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Application.Ports;

public interface IShiftRepository
{
    Task<CashierShift?> Get(TenantId tenantId, Guid shiftId, CancellationToken ct);

    /// <summary>Kasiyerin veya terminalin açık vardiyası var mı (KK1).</summary>
    Task<bool> HasOpenShift(TenantId tenantId, UserId cashier, string terminal, CancellationToken ct);

    /// <summary>İdempotency: bu anahtarla daha önce yapılmış işlem (KK5).</summary>
    Task<CageTransaction?> FindTransaction(TenantId tenantId, Guid transactionId, CancellationToken ct);

    void Add(CashierShift shift);
}

/// <summary>Vardiya, işlem, defter satırları ve olayları tek transaction'da yazar.</summary>
public interface IFinanceUnitOfWork : IUnitOfWork
{
}

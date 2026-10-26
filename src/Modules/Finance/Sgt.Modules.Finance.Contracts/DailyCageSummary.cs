using Sgt.BuildingBlocks;

namespace Sgt.Modules.Finance.Contracts;

/// <summary>Bir para birimindeki günlük kasa toplamları (Spec 011 KK2).</summary>
public sealed record CageCurrencyTotals(
    string Currency, decimal ChipSales, decimal ChipRedemptions, decimal ForeignBought, decimal ForeignSold, decimal ClosingVariance);

public sealed record DailyCageSummary(int ShiftsOpen, int ShiftsClosed, int Reversals, IReadOnlyList<CageCurrencyTotals> Currencies);

public interface IDailyCageQuery
{
    Task<DailyCageSummary> For(TenantId tenantId, BusinessDay day, CancellationToken ct);
}

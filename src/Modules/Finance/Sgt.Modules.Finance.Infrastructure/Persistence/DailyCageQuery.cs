using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Contracts;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Infrastructure.Persistence;

/// <summary>
/// Günlük kasa özeti (Spec 011 KK2). Ters kayıtlar, ters çevirdikleri işlemin türünden düşülür (net tutarlar).
/// </summary>
internal sealed class DailyCageQuery(FinanceDbContext db) : IDailyCageQuery
{
    public async Task<DailyCageSummary> For(TenantId tenantId, BusinessDay day, CancellationToken ct)
    {
        var tenant = tenantId.Value;
        var transactions = await db.Transactions.AsNoTracking()
            .Where(t => t.TenantId == tenant && t.OccurredAt >= day.Start && t.OccurredAt < day.End)
            .Select(t => new { t.Id, t.Type, t.Amount, t.Currency, t.ReversesId })
            .ToListAsync(ct);

        var reversedIds = transactions.Where(t => t.ReversesId != null).Select(t => t.ReversesId!.Value).ToList();
        var originalTypes = await db.Transactions.AsNoTracking()
            .Where(t => reversedIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Type, ct);

        // Her işlemi "etkili tür" ve işaretle topla: ters kayıt, asıl türden eksi olarak düşer.
        var signed = transactions.Select(t => t.ReversesId is { } original
            ? (Type: originalTypes.GetValueOrDefault(original, string.Empty), t.Currency, Amount: -t.Amount)
            : (t.Type, t.Currency, t.Amount)).ToList();

        var variances = await db.Shifts.AsNoTracking()
            .Where(s => s.TenantId == tenant && s.ClosedAt >= day.Start && s.ClosedAt < day.End)
            .SelectMany(s => s.ClosingCounts)
            .GroupBy(c => c.Currency)
            .Select(g => new { Currency = g.Key, Variance = g.Sum(c => c.Counted - c.Expected) })
            .ToDictionaryAsync(x => x.Currency, x => x.Variance, ct);

        var currencies = signed.Select(s => s.Currency).Concat(variances.Keys).Distinct().Order(StringComparer.Ordinal);
        decimal Sum(string currency, CageTransactionType type) =>
            signed.Where(s => s.Currency == currency && s.Type == type.ToString()).Sum(s => s.Amount);

        var totals = currencies.Select(c => new CageCurrencyTotals(
            c,
            Sum(c, CageTransactionType.ChipSale),
            Sum(c, CageTransactionType.ChipRedeem),
            Sum(c, CageTransactionType.ExchangeForeignToBase),
            Sum(c, CageTransactionType.ExchangeBaseToForeign),
            variances.GetValueOrDefault(c))).ToList();

        var shiftsOpen = await db.Shifts.CountAsync(s => s.TenantId == tenant && s.OpenedAt < day.End && (s.ClosedAt == null || s.ClosedAt >= day.End), ct);
        var shiftsClosed = await db.Shifts.CountAsync(s => s.TenantId == tenant && s.ClosedAt >= day.Start && s.ClosedAt < day.End, ct);
        return new DailyCageSummary(shiftsOpen, shiftsClosed, reversedIds.Count, totals);
    }
}

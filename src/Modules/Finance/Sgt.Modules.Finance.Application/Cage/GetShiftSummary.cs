using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Application.Ports;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Application.Cage;

public sealed record GetShiftSummaryQuery(TenantId TenantId, Guid ShiftId);

public sealed record BalanceView(string Instrument, string Currency, decimal Expected);

public sealed record ClosingCountView(string Instrument, string Currency, decimal Expected, decimal Counted, decimal Variance);

/// <summary>Vardiya özeti (Spec 006 KK10): beklenen bakiyeler, işlemler, varsa kapanış sayımı.</summary>
public sealed record ShiftSummary(
    Guid ShiftId,
    Guid CashierId,
    string Terminal,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    string? ClosingNote,
    IReadOnlyList<BalanceView> Balances,
    IReadOnlyList<CageTransactionResult> Transactions,
    IReadOnlyList<ClosingCountView> ClosingCounts);

public sealed class GetShiftSummaryHandler(IShiftRepository shifts) : IQueryHandler<GetShiftSummaryQuery, ShiftSummary?>
{
    public async Task<ShiftSummary?> Handle(GetShiftSummaryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var shift = await shifts.Get(query.TenantId, query.ShiftId, ct);
        if (shift is null)
        {
            return null;
        }

        var keys = shift.Opening.Select(a => (a.Instrument, a.Currency))
            .Concat(shift.Transactions.SelectMany(t => t.Entries).Where(e => e.Account == LedgerAccount.Drawer).Select(e => (e.Instrument, e.Currency)))
            .Distinct()
            .OrderBy(k => k.Instrument).ThenBy(k => k.Currency, StringComparer.Ordinal);

        return new ShiftSummary(
            shift.Id,
            shift.CashierId.Value,
            shift.Terminal,
            shift.OpenedAt,
            shift.ClosedAt,
            shift.ClosingNote,
            keys.Select(k => new BalanceView(k.Instrument.ToString(), k.Currency, shift.Balance(k.Instrument, k.Currency))).ToList(),
            shift.Transactions.OrderBy(t => t.OccurredAt).Select(CageTransactionResult.From).ToList(),
            shift.ClosingCounts.Select(c => new ClosingCountView(c.Instrument.ToString(), c.Currency, c.Expected, c.Counted, c.Variance)).ToList());
    }
}

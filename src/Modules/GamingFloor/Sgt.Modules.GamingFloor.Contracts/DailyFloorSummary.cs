using Sgt.BuildingBlocks;

namespace Sgt.Modules.GamingFloor.Contracts;

/// <summary>Canlı oyun günlük toplamı; CasinoResult = −oyuncu sonucu (Spec 011 KK3).</summary>
public sealed record LiveCurrencyTotals(string Currency, decimal TheoreticalWin, decimal CasinoResult);

/// <summary>Slot günlük toplamı (Spec 011 KK4).</summary>
public sealed record SlotCurrencyTotals(string Currency, decimal CoinIn, decimal MachineWin);

public sealed record DailyFloorSummary(int RatingsFinished, IReadOnlyList<LiveCurrencyTotals> Live, IReadOnlyList<SlotCurrencyTotals> Slots);

public interface IDailyFloorQuery
{
    Task<DailyFloorSummary> For(TenantId tenantId, BusinessDay day, CancellationToken ct);
}

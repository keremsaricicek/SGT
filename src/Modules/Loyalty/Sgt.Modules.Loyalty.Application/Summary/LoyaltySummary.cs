using Sgt.BuildingBlocks;
using Sgt.Modules.Loyalty.Application.Ports;
using Sgt.Modules.Loyalty.Contracts;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Application.Summary;

public sealed record GetLoyaltySummaryQuery(TenantId TenantId, Guid PlayerId);

public sealed record PointsEntryView(string Kind, long Points, Guid SourceId, DateTimeOffset At);

/// <summary>Oyuncunun puan özeti (Spec 008 KK7). Hesabı yoksa sıfır bakiye, Bronz.</summary>
public sealed record LoyaltySummary(Guid PlayerId, long Balance, long LifetimeEarned, string Tier, IReadOnlyList<PointsEntryView> RecentEntries);

public sealed class GetLoyaltySummaryHandler(ILoyaltyAccountRepository accounts) : IQueryHandler<GetLoyaltySummaryQuery, LoyaltySummary>
{
    private const int RecentCount = 50;

    public async Task<LoyaltySummary> Handle(GetLoyaltySummaryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var account = await accounts.Get(query.TenantId, query.PlayerId, ct) ?? LoyaltyAccount.Open(query.TenantId, query.PlayerId);
        return new LoyaltySummary(
            account.PlayerId,
            account.Balance,
            account.LifetimeEarned,
            account.Tier.ToString(),
            account.Entries.OrderByDescending(e => e.At).Take(RecentCount)
                .Select(e => new PointsEntryView(e.Kind.ToString(), e.Points, e.SourceId, e.At)).ToList());
    }
}

public sealed class PlayerTierQuery(ILoyaltyAccountRepository accounts) : IPlayerTierQuery
{
    public async Task<string> GetTier(TenantId tenantId, Guid playerId, CancellationToken ct) =>
        ((await accounts.Get(tenantId, playerId, ct))?.Tier ?? Tier.Bronze).ToString();
}

using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.Loyalty.Application.Ports;
using Sgt.Modules.Loyalty.Contracts;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Infrastructure.Persistence;

internal sealed class TrackedLoyalty
{
    public Dictionary<LoyaltyAccount, AccountRow?> Accounts { get; } = [];

    public Dictionary<CompRequest, CompRequestRow?> Comps { get; } = [];

    public Dictionary<PointsAdjustmentRequest, AdjustmentRequestRow?> Adjustments { get; } = [];

    public List<IIntegrationEvent> ExtraEvents { get; } = [];
}

internal sealed class LoyaltyAccountRepository(LoyaltyDbContext db, TrackedLoyalty tracked) : ILoyaltyAccountRepository
{
    public async Task<LoyaltyAccount?> Get(TenantId tenantId, Guid playerId, CancellationToken ct)
    {
        var existing = tracked.Accounts.Keys.SingleOrDefault(a => a.TenantId == tenantId && a.PlayerId == playerId);
        if (existing is not null)
        {
            return existing;
        }

        var row = await db.Accounts.Include(a => a.Entries).SingleOrDefaultAsync(a => a.TenantId == tenantId.Value && a.PlayerId == playerId, ct);
        if (row is null)
        {
            return null;
        }

        var account = LoyaltyAccount.Restore(tenantId, playerId, row.Entries.OrderBy(e => e.At).Select(e => new PointsEntry(
            e.Id, Enum.Parse<PointsEntryKind>(e.Kind), e.Points, e.SourceId,
            new AuditContext(new UserId(e.ActorId), e.Terminal, ReasonCode.Of(e.ReasonCode), e.CorrelationId), e.At)));
        tracked.Accounts[account] = row;
        return account;
    }

    public void Add(LoyaltyAccount account) => tracked.Accounts[account] = null;
}

internal sealed class ApprovalRequestRepository(LoyaltyDbContext db, TrackedLoyalty tracked) : IApprovalRequestRepository
{
    public async Task<CompRequest?> GetComp(TenantId tenantId, Guid requestId, CancellationToken ct)
    {
        var row = await db.Comps.SingleOrDefaultAsync(r => r.TenantId == tenantId.Value && r.Id == requestId, ct);
        if (row is null)
        {
            return null;
        }

        var request = CompRequest.Restore(row.Id, tenantId, row.PlayerId, row.Points, row.Text, row.RequestedAt, ToDecision(row));
        tracked.Comps[request] = row;
        return request;
    }

    public async Task<PointsAdjustmentRequest?> GetAdjustment(TenantId tenantId, Guid requestId, CancellationToken ct)
    {
        var row = await db.Adjustments.SingleOrDefaultAsync(r => r.TenantId == tenantId.Value && r.Id == requestId, ct);
        if (row is null)
        {
            return null;
        }

        var request = PointsAdjustmentRequest.Restore(row.Id, tenantId, row.PlayerId, row.Points, row.Text, row.RequestedAt, ToDecision(row));
        tracked.Adjustments[request] = row;
        return request;
    }

    public void Add(CompRequest request) => tracked.Comps[request] = null;

    public void Add(PointsAdjustmentRequest request) => tracked.Adjustments[request] = null;

    private static FourEyesDecision ToDecision(RequestRow row) => FourEyesDecision.Restore(
        new UserId(row.RequestedBy), Enum.Parse<DecisionStatus>(row.Status), row.DecidedBy is { } by ? new UserId(by) : null, row.DecidedAt);
}

internal sealed class EarningRuleStore(LoyaltyDbContext db, TrackedLoyalty tracked, IClock clock) : IEarningRuleStore
{
    public async Task<EarningRule> Current(TenantId tenantId, CancellationToken ct)
    {
        var row = await db.Rules.AsNoTracking().SingleOrDefaultAsync(r => r.TenantId == tenantId.Value, ct);
        return EarningRule.Restore(tenantId, row?.TryPerPoint ?? EarningRule.DefaultTryPerPoint);
    }

    public void Save(EarningRule rule, AuditContext audit)
    {
        var row = db.Rules.Find(rule.TenantId.Value);
        if (row is null)
        {
            db.Rules.Add(new EarningRuleRow { TenantId = rule.TenantId.Value, TryPerPoint = rule.TryPerPoint });
        }
        else
        {
            row.TryPerPoint = rule.TryPerPoint;
        }

        tracked.ExtraEvents.Add(new LoyaltyChangedV1(
            Guid.NewGuid(), audit.CorrelationId, clock.UtcNow, rule.TenantId, audit, null, nameof(LoyaltyChangeKind.RuleChanged), $"{rule.TryPerPoint} TRY = 1 puan"));
    }
}

internal sealed class LoyaltyInbox(LoyaltyDbContext db, IClock clock) : EfInbox<LoyaltyDbContext>(db, clock), ILoyaltyInbox;

internal sealed class LoyaltyUnitOfWork(LoyaltyDbContext db, TrackedLoyalty tracked, IntegrationEventSerializer serializer) : ILoyaltyUnitOfWork
{
    public async Task Commit(CancellationToken ct)
    {
        foreach (var (account, existing) in tracked.Accounts.ToList())
        {
            var row = existing ?? db.Accounts.Add(new AccountRow { TenantId = account.TenantId.Value, PlayerId = account.PlayerId }).Entity;
            tracked.Accounts[account] = row;
            var persisted = row.Entries.Select(e => e.Id).ToHashSet();
            row.Entries.AddRange(account.Entries.Where(e => !persisted.Contains(e.Id)).Select(e => new PointsEntryRow
            {
                Id = e.Id, TenantId = account.TenantId.Value, PlayerId = account.PlayerId, Kind = e.Kind.ToString(), Points = e.Points, SourceId = e.SourceId,
                ActorId = e.Audit.Actor.Value, Terminal = e.Audit.Terminal, ReasonCode = e.Audit.Reason.Value, CorrelationId = e.Audit.CorrelationId, At = e.At,
            }));
            row.EntryCount = account.Entries.Count;
            Publish(account.TenantId, account.PendingChanges);
            account.ClearPendingChanges();
        }

        foreach (var (request, existing) in tracked.Comps.ToList())
        {
            var row = existing ?? db.Comps.Add(new CompRequestRow()).Entity;
            tracked.Comps[request] = row;
            Copy(row, request.Id, request.TenantId, request.PlayerId, request.Points, request.Description, request.RequestedAt, request.Decision);
            Publish(request.TenantId, request.PendingChanges);
            request.ClearPendingChanges();
        }

        foreach (var (request, existing) in tracked.Adjustments.ToList())
        {
            var row = existing ?? db.Adjustments.Add(new AdjustmentRequestRow()).Entity;
            tracked.Adjustments[request] = row;
            Copy(row, request.Id, request.TenantId, request.PlayerId, request.Points, request.Note, request.RequestedAt, request.Decision);
            Publish(request.TenantId, request.PendingChanges);
            request.ClearPendingChanges();
        }

        foreach (var e in tracked.ExtraEvents)
        {
            db.Set<OutboxMessage>().Add(serializer.ToOutbox(e));
        }

        tracked.ExtraEvents.Clear();

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConcurrencyConflictException("Puan hesabı veya istek aynı anda değişti; tekrar deneyin.", e);
        }
    }

    private static void Copy(RequestRow row, Guid id, TenantId tenantId, Guid playerId, long points, string text, DateTimeOffset at, FourEyesDecision decision)
    {
        row.Id = id;
        row.TenantId = tenantId.Value;
        row.PlayerId = playerId;
        row.Points = points;
        row.Text = text;
        row.RequestedAt = at;
        row.RequestedBy = decision.RequestedBy.Value;
        row.Status = decision.Status.ToString();
        row.DecidedBy = decision.DecidedBy?.Value;
        row.DecidedAt = decision.DecidedAt;
    }

    private void Publish(TenantId tenantId, IEnumerable<LoyaltyChange> changes)
    {
        foreach (var c in changes)
        {
            db.Set<OutboxMessage>().Add(serializer.ToOutbox(
                new LoyaltyChangedV1(c.Id, c.Audit.CorrelationId, c.At, tenantId, c.Audit, c.PlayerId, c.Kind.ToString(), c.Detail)));
        }
    }
}

internal sealed class DesignTimeFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<LoyaltyDbContext>
{
    public LoyaltyDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<LoyaltyDbContext>()
            .UseNpgsql("Host=localhost", o => o.MigrationsHistoryTable("__ef_migrations", LoyaltyDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options);
}

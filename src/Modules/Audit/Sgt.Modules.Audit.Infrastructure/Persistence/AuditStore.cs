using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.Audit.Application.Ports;
using Sgt.Modules.Audit.Application.Search;
using Sgt.Modules.Audit.Domain;

namespace Sgt.Modules.Audit.Infrastructure.Persistence;

internal sealed class AuditLog(AuditDbContext db) : IAuditLog
{
    public void Add(AuditEntry entry) => db.Entries.Add(new AuditEntryRow
    {
        Id = entry.Id,
        TenantId = entry.TenantId.Value,
        EventType = entry.EventType,
        Summary = entry.Summary.Length > 1000 ? entry.Summary[..1000] : entry.Summary,
        ActorId = entry.Audit.Actor.Value,
        Terminal = entry.Audit.Terminal,
        ReasonCode = entry.Audit.Reason.Value,
        CorrelationId = entry.Audit.CorrelationId,
        OccurredAt = entry.OccurredAt,
        RecordedAt = entry.RecordedAt,
    });
}

internal sealed class AuditSearch(AuditDbContext db) : IAuditSearch
{
    public async Task<IReadOnlyList<AuditEntryView>> Search(SearchAuditEntriesQuery query, CancellationToken ct)
    {
        var entries = db.Entries.AsNoTracking().Where(e => e.TenantId == query.TenantId.Value);
        if (query.From is { } from)
        {
            entries = entries.Where(e => e.OccurredAt >= from);
        }

        if (query.To is { } to)
        {
            entries = entries.Where(e => e.OccurredAt < to);
        }

        if (query.Actor is { } actor)
        {
            entries = entries.Where(e => e.ActorId == actor.Value);
        }

        if (query.CorrelationId is { } correlationId)
        {
            entries = entries.Where(e => e.CorrelationId == correlationId);
        }

        return await entries
            .OrderByDescending(e => e.OccurredAt)
            .Take(query.Take)
            .Select(e => new AuditEntryView(e.Id, e.EventType, e.Summary, e.ActorId, e.Terminal, e.ReasonCode, e.CorrelationId, e.OccurredAt))
            .ToListAsync(ct);
    }
}

internal sealed class AuditInbox(AuditDbContext db, IClock clock) : EfInbox<AuditDbContext>(db, clock), IAuditInbox;

internal sealed class AuditUnitOfWork(AuditDbContext db) : IAuditUnitOfWork
{
    public Task Commit(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class DesignTimeFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AuditDbContext>()
            .UseNpgsql("Host=localhost", o => o.MigrationsHistoryTable("__ef_migrations", AuditDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options);
}

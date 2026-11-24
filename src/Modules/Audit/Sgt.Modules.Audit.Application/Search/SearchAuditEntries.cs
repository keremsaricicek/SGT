using Sgt.BuildingBlocks;
using Sgt.Modules.Audit.Application.Ports;

namespace Sgt.Modules.Audit.Application.Search;

/// <summary>Denetim kaydı araması (KK8). Her zaman tenant kapsamlıdır; en yeni kayıt önce gelir.</summary>
public sealed record SearchAuditEntriesQuery(
    TenantId TenantId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    UserId? Actor,
    Guid? CorrelationId,
    int Take);

public sealed record AuditEntryView(
    Guid Id,
    string EventType,
    string Summary,
    Guid ActorId,
    string Terminal,
    string ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed class SearchAuditEntriesHandler(IAuditSearch search)
    : IQueryHandler<SearchAuditEntriesQuery, IReadOnlyList<AuditEntryView>>
{
    public const int MaxTake = 500;

    public Task<IReadOnlyList<AuditEntryView>> Handle(SearchAuditEntriesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        return search.Search(query with { Take = Math.Clamp(query.Take, 1, MaxTake) }, ct);
    }
}

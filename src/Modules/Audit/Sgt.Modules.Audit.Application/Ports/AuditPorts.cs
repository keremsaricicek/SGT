using Sgt.BuildingBlocks;
using Sgt.Modules.Audit.Application.Search;
using Sgt.Modules.Audit.Domain;

namespace Sgt.Modules.Audit.Application.Ports;

/// <summary>Denetim kaydına yalnızca ekleme yapılır (KK7).</summary>
public interface IAuditLog
{
    void Add(AuditEntry entry);
}

public interface IAuditSearch
{
    Task<IReadOnlyList<AuditEntryView>> Search(SearchAuditEntriesQuery query, CancellationToken ct);
}

public interface IAuditInbox : IInbox
{
}

public interface IAuditUnitOfWork : IUnitOfWork
{
}

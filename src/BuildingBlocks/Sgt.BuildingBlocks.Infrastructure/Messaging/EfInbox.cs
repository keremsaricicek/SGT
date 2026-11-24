using Microsoft.EntityFrameworkCore;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Modülün inbox'ı. MarkProcessed yalnızca ekler; kayıt modülün unit of work commit'iyle yazılır.</summary>
public class EfInbox<TContext>(TContext db, IClock clock) : IInbox
    where TContext : DbContext
{
    public Task<bool> AlreadyProcessed(Guid eventId, string handler, CancellationToken ct) =>
        db.Set<InboxMessage>().AnyAsync(m => m.EventId == eventId && m.Handler == handler, ct);

    public void MarkProcessed(Guid eventId, string handler) =>
        db.Set<InboxMessage>().Add(new InboxMessage { EventId = eventId, Handler = handler, ProcessedAt = clock.UtcNow });
}

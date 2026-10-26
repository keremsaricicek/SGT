using Microsoft.EntityFrameworkCore;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Operatörün dead-letter listesini görmesi ve yeniden işletmesi (KK5).</summary>
public sealed class OutboxStore<TContext>(string module, TContext db, IClock clock) : IOutboxStore
    where TContext : DbContext
{
    public string Module => module;

    public async Task<IReadOnlyList<DeadLetter>> DeadLetters(int take, CancellationToken ct) =>
        await db.Set<OutboxMessage>().AsNoTracking()
            .Where(m => m.DeadLettered)
            .OrderByDescending(m => m.OccurredAt)
            .Take(take)
            .Select(m => new DeadLetter(module, m.Id, m.Type, m.Attempts, m.LastError, m.OccurredAt))
            .ToListAsync(ct);

    public async Task<bool> Reprocess(Guid eventId, CancellationToken ct)
    {
        var message = await db.Set<OutboxMessage>().SingleOrDefaultAsync(m => m.Id == eventId && m.DeadLettered, ct);
        if (message is null)
        {
            return false;
        }

        message.Reprocess(clock.UtcNow);
        await db.SaveChangesAsync(ct);
        return true;
    }
}

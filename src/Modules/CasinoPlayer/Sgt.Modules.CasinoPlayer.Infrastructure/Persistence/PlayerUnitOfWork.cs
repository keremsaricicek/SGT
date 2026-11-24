using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.CasinoPlayer.Application.Ports;

namespace Sgt.Modules.CasinoPlayer.Infrastructure.Persistence;

/// <summary>
/// Oyuncuları, kart/kısıtlama satırlarını ve olaylarını tek SaveChanges ile yazar.
/// Eşzamanlı değişiklik veya benzersizlik çakışmasında hiçbir şey yazılmaz.
/// </summary>
internal sealed class PlayerUnitOfWork(
    PlayerDbContext db,
    PlayerMapping mapping,
    TrackedPlayers tracked,
    IntegrationEventSerializer serializer) : IPlayerUnitOfWork
{
    public async Task Commit(CancellationToken ct)
    {
        foreach (var (player, existing) in tracked.Items.ToList())
        {
            var row = existing ?? new PlayerRow();
            if (existing is null)
            {
                db.Players.Add(row);
                tracked.Items[player] = row;
            }

            mapping.CopyTo(player, row);
            foreach (var change in player.PendingChanges)
            {
                db.Set<OutboxMessage>().Add(serializer.ToOutbox(PlayerMapping.ToEvent(player, change)));
            }
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConcurrencyConflictException("Oyuncu kaydı başka bir kullanıcı tarafından değiştirildi.", e);
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConcurrencyConflictException("Aynı belge veya kart numarası aynı anda başka bir kayıtta kullanıldı.", e);
        }

        foreach (var player in tracked.Items.Keys)
        {
            player.ClearPendingChanges();
        }
    }
}

internal sealed class DesignTimeFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<PlayerDbContext>
{
    public PlayerDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<PlayerDbContext>()
            .UseNpgsql("Host=localhost", o => o.MigrationsHistoryTable("__ef_migrations", PlayerDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options);
}

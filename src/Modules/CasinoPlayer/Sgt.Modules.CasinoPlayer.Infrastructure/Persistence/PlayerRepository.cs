using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Application.Search;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Infrastructure.Persistence;

/// <summary>Bir istekte yüklenen/eklenen oyuncular ve satırları.</summary>
internal sealed class TrackedPlayers
{
    public Dictionary<Player, PlayerRow?> Items { get; } = [];
}

internal sealed class PlayerRepository(PlayerDbContext db, PlayerMapping mapping, TrackedPlayers tracked, IClock clock) : IPlayerRepository
{
    public async Task<Player?> Get(TenantId tenantId, PlayerId playerId, CancellationToken ct)
    {
        var row = await db.Players
            .Include(p => p.Cards)
            .Include(p => p.Restrictions)
            .SingleOrDefaultAsync(p => p.TenantId == tenantId.Value && p.Id == playerId.Value, ct);
        if (row is null)
        {
            return null;
        }

        var player = mapping.ToDomain(row);
        tracked.Items[player] = row;
        return player;
    }

    /// <summary>Tenant sayacını tek SQL ile artırır; eşzamanlı kayıtlar aynı numarayı alamaz.</summary>
    public async Task<long> NextSequence(TenantId tenantId, CancellationToken ct) =>
        (await db.Database.SqlQuery<long>(
                $"""
                INSERT INTO casino_player.player_sequences (tenant_id, last_value) VALUES ({tenantId.Value}, 1)
                ON CONFLICT (tenant_id) DO UPDATE SET last_value = player_sequences.last_value + 1
                RETURNING last_value AS "Value"
                """)
            .ToListAsync(ct)).Single();

    public Task<bool> DocumentInUse(TenantId tenantId, DocumentType type, string number, PlayerId? except, CancellationToken ct)
    {
        var index = mapping.DocumentIndex(type, number);
        var typeName = type.ToString();
        var excluded = except?.Value;
        return db.Players.AnyAsync(
            p => p.TenantId == tenantId.Value && p.DocumentType == typeName && p.DocumentIndex == index && p.Id != excluded, ct);
    }

    public Task<bool> CardInUse(TenantId tenantId, CardNumber number, CancellationToken ct) =>
        db.Cards.AnyAsync(c => c.TenantId == tenantId.Value && c.Number == number.Value, ct);

    public async Task<IReadOnlyList<PlayerSummary>> Search(TenantId tenantId, string term, int take, CancellationToken ct)
    {
        var upper = term.ToUpperInvariant();
        var documentDigits = new string(upper.Where(char.IsAsciiLetterOrDigit).ToArray());
        var documentIndexes = Enum.GetValues<DocumentType>().Select(t => mapping.DocumentIndex(t, documentDigits)).ToList();
        var namePrefix = term.Replace("%", string.Empty, StringComparison.Ordinal).Replace("_", string.Empty, StringComparison.Ordinal) + "%";
        var now = clock.UtcNow;

        return await db.Players.AsNoTracking()
            .Where(p => p.TenantId == tenantId.Value
                && (p.Number == upper
                    || p.Cards.Any(c => c.Number == upper && c.CancelledAt == null)
                    || documentIndexes.Contains(p.DocumentIndex)
                    || EF.Functions.ILike(p.FirstName, namePrefix)
                    || EF.Functions.ILike(p.LastName, namePrefix)))
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Take(take)
            .Select(p => new PlayerSummary(
                p.Id,
                p.Number,
                p.FirstName,
                p.LastName,
                p.Cards.Where(c => c.CancelledAt == null).Select(c => c.Number).FirstOrDefault(),
                p.Restrictions.Any(r => r.RemovedAt == null && (r.Until == null || r.Until > now))))
            .ToListAsync(ct);
    }

    public void Add(Player player) => tracked.Items[player] = null;
}

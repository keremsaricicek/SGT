using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Infrastructure.Persistence;

/// <summary>Domain ↔ satır çevirisi; şifreleme burada yapılır.</summary>
internal sealed class PlayerMapping(IPersonalDataProtector protector)
{
    public string DocumentIndex(DocumentType type, string number) => protector.BlindIndex($"{type}:{number}");

    public Player ToDomain(PlayerRow row) =>
        Player.Restore(
            new PlayerId(row.Id),
            new TenantId(row.TenantId),
            PlayerNumber.Restore(row.Number),
            new PlayerDetails(
                row.FirstName,
                row.LastName,
                row.BirthDate,
                row.Nationality,
                Enum.Parse<DocumentType>(row.DocumentType),
                protector.Unprotect(row.DocumentCipher),
                row.PhoneCipher is null ? null : protector.Unprotect(row.PhoneCipher)),
            row.Cards.Select(c => PlayerCard.Restore(CardNumber.Parse(c.Number).Value, c.IssuedAt, c.CancelledAt)),
            row.Restrictions.Select(r => PlayerRestriction.Restore(
                r.Id, Enum.Parse<RestrictionType>(r.Type), r.Note, r.Until, new UserId(r.AddedBy), r.AddedAt,
                r.RemovedBy is { } by ? new UserId(by) : null, r.RemovedAt)));

    public void CopyTo(Player player, PlayerRow row)
    {
        var d = player.Details;
        row.Id = player.Id.Value;
        row.TenantId = player.TenantId.Value;
        row.Number = player.Number.Value;
        row.FirstName = d.FirstName;
        row.LastName = d.LastName;
        row.BirthDate = d.BirthDate;
        row.Nationality = d.Nationality;
        row.DocumentType = d.DocumentType.ToString();
        row.DocumentIndex = DocumentIndex(d.DocumentType, d.DocumentNumber);
        row.DocumentCipher = protector.Protect(d.DocumentNumber);
        row.PhoneCipher = d.Phone is null ? null : protector.Protect(d.Phone);

        foreach (var card in player.Cards)
        {
            var cardRow = row.Cards.SingleOrDefault(c => c.Number == card.Number.Value);
            if (cardRow is null)
            {
                row.Cards.Add(new PlayerCardRow
                {
                    Id = Guid.NewGuid(), TenantId = row.TenantId, PlayerId = row.Id, Number = card.Number.Value, IssuedAt = card.IssuedAt,
                    CancelledAt = card.CancelledAt,
                });
            }
            else
            {
                cardRow.CancelledAt = card.CancelledAt;
            }
        }

        foreach (var restriction in player.Restrictions)
        {
            var restrictionRow = row.Restrictions.SingleOrDefault(r => r.Id == restriction.Id);
            if (restrictionRow is null)
            {
                row.Restrictions.Add(new PlayerRestrictionRow
                {
                    Id = restriction.Id, PlayerId = row.Id, Type = restriction.Type.ToString(), Note = restriction.Note, Until = restriction.Until,
                    AddedBy = restriction.AddedBy.Value, AddedAt = restriction.AddedAt,
                    RemovedBy = restriction.RemovedBy?.Value, RemovedAt = restriction.RemovedAt,
                });
            }
            else
            {
                restrictionRow.RemovedBy = restriction.RemovedBy?.Value;
                restrictionRow.RemovedAt = restriction.RemovedAt;
            }
        }
    }

    public static PlayerChangedV1 ToEvent(Player player, PlayerChange change) => new(
        change.Id, change.Audit.CorrelationId, change.OccurredAt, player.TenantId, change.Audit,
        player.Id.Value, player.Number.Value, change.Kind.ToString(), change.Detail);
}

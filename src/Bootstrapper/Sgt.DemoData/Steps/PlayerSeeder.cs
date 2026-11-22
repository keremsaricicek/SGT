using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Cards;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.CasinoPlayer.Application.Restrictions;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.DemoData.Steps;

/// <summary>
/// Sentetik oyuncular (Spec 012 KK3, KK7): kayıt + kart; oyuncuların ~%2'si kısıtlı (en az biri).
/// Sıralı yazılır; oyuncu numarası ile isim eşleşmesi her çalıştırmada aynıdır (KK5).
/// </summary>
public sealed class PlayerSeeder(DemoCommands commands) : IDemoStep
{
    private const double RestrictedShare = 0.02;
    private static readonly TimeSpan PerPlayer = TimeSpan.FromSeconds(3);

    public int Order => 40;

    public string Name => "Oyuncular";

    public async Task Run(DemoRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        var restrictedCount = Math.Max(1, (int)Math.Round(run.Scale.Players * RestrictedShare));
        var restrictedEvery = Math.Max(2, run.Scale.Players / restrictedCount);
        for (var i = 1; i <= run.Scale.Players; i++)
        {
            var person = run.Names.NextPerson(run.Timeline.Today);
            var details = new PlayerDetails(
                person.FirstName, person.LastName, person.BirthDate, person.Nationality, person.DocumentType, person.DocumentNumber, person.Phone);
            var registered = await commands.Ask<ICommandHandler<RegisterPlayerCommand, PlayerRegistered>, PlayerRegistered>(
                "oyuncu kaydı", h => h.Handle(new RegisterPlayerCommand(run.Tenant, details, run.Audit(DemoUsers.Host.Id)), ct));
            var playerId = new PlayerId(registered.PlayerId);
            await commands.Do<ICommandHandler<IssuePlayerCardCommand>>(
                "kart", h => h.Handle(new IssuePlayerCardCommand(run.Tenant, playerId, SyntheticNames.CardNumber(i), run.Audit(DemoUsers.Host.Id)), ct));

            // Her n'inci oyuncu (ilk kısıtlı: n/2'nci) kısıtlıdır; küçük ölçekte de en az bir kısıtlı oyuncu olur.
            var restricted = i % restrictedEvery == restrictedEvery / 2;
            if (restricted)
            {
                var type = run.Chance(0.5) ? RestrictionType.SelfExcluded : RestrictionType.Banned;
                await commands.Ask<ICommandHandler<AddPlayerRestrictionCommand, Guid>, Guid>(
                    "kısıtlama", h => h.Handle(new AddPlayerRestrictionCommand(run.Tenant, playerId, type, "Demo kısıtlaması", null, run.Audit(DemoUsers.CasinoManager.Id)), ct));
            }

            run.Players.Add(new DemoPlayer(registered.PlayerId, person.Nationality, restricted));
            run.Clock.Advance(PerPlayer);
        }
    }
}

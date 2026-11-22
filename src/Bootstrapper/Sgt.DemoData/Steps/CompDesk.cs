using Sgt.BuildingBlocks;
using Sgt.Modules.Loyalty.Application.Comps;
using Sgt.Modules.Loyalty.Application.Summary;

namespace Sgt.DemoData.Steps;

/// <summary>
/// Comp talebi ve kararı (Spec 008 KK6, dört göz): host ister, casino müdürü karar verir. Puan bakiyesi Loyalty'den okunur;
/// yetecek puanı olmayan oyuncu için talep açılmaz.
/// </summary>
public sealed class CompDesk(DemoCommands commands)
{
    private const long MinimumBalance = 20;
    private const double ApprovalShare = 0.85;

    private static readonly string[] Descriptions = ["Restoran ikramı", "Otel konaklaması", "Bar ikramı", "Havalimanı transferi", "Spa ikramı"];

    public async Task Offer(DemoRun run, Guid playerId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        var summary = await commands.Query<GetLoyaltySummaryQuery, LoyaltySummary>(new GetLoyaltySummaryQuery(run.Tenant, playerId), ct);
        if (summary.Balance < MinimumBalance)
        {
            return;
        }

        // Bakiyenin %20–60'ı, 10'a yuvarlanmış (en az 10 puan).
        var points = Math.Max(10, (long)decimal.Round(summary.Balance * (0.2m + (run.NextRatio() * 0.4m)) / 10m) * 10);
        var requestId = run.NextGuid();
        var description = run.Pick(Descriptions);
        await commands.Ask<ICommandHandler<RequestCompCommand, Guid>, Guid>(
            "comp talebi", h => h.Handle(new RequestCompCommand(run.Tenant, requestId, playerId, points, description, run.Audit(DemoUsers.Host.Id)), ct));

        run.Clock.Advance(TimeSpan.FromMinutes(2));
        var approve = run.Chance(ApprovalShare);
        await commands.Do<ICommandHandler<DecideCompCommand>>(
            "comp kararı", h => h.Handle(new DecideCompCommand(run.Tenant, requestId, approve, run.Audit(DemoUsers.CasinoManager.Id)), ct));
    }
}

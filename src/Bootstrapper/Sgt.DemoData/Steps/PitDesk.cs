using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Application.Ratings;
using Sgt.Modules.GamingFloor.Application.Tables;

namespace Sgt.DemoData.Steps;

/// <summary>Demo pit boss'unun masa ve rating işlemleri (Spec 007 komutlarıyla).</summary>
public sealed class PitDesk(DemoCommands commands)
{
    private static UserId PitBoss => DemoUsers.PitBoss.Id;

    public Task OpenTable(DemoRun run, Guid tableId, CancellationToken ct) =>
        commands.Do<OpenTableSessionHandler>("masa açılışı", h => h.Handle(new TableSessionCommand(run.Tenant, tableId, run.Audit(PitBoss)), ct));

    public Task CloseTable(DemoRun run, Guid tableId, CancellationToken ct) =>
        commands.Do<CloseTableSessionHandler>("masa kapanışı", h => h.Handle(new TableSessionCommand(run.Tenant, tableId, run.Audit(PitBoss)), ct));

    public Task<Guid> StartRating(DemoRun run, Guid tableId, Guid playerId, CancellationToken ct) =>
        commands.Ask<ICommandHandler<StartRatingCommand, Guid>, Guid>(
            "rating başlangıcı", h => h.Handle(new StartRatingCommand(run.Tenant, tableId, playerId, run.Audit(PitBoss)), ct));

    public Task FinishRating(DemoRun run, Guid ratingId, decimal averageBet, decimal playerResult, CancellationToken ct) =>
        commands.Do<ICommandHandler<FinishRatingCommand>>(
            "rating bitişi", h => h.Handle(new FinishRatingCommand(run.Tenant, ratingId, averageBet, playerResult, run.Audit(PitBoss)), ct));
}

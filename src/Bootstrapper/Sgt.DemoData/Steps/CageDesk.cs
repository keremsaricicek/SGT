using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Application.Cage;
using Sgt.Modules.Finance.Domain;

namespace Sgt.DemoData.Steps;

/// <summary>
/// Demo kasiyerinin kasa işlemleri (Spec 006 komutlarıyla). Defter kuralları (çekmece yeterliliği, denge, idempotency)
/// handler'larda aynen işler; her işlem tohumdan türeyen bir idempotency anahtarı taşır.
/// </summary>
public sealed class CageDesk(DemoCommands commands)
{
    private static readonly DrawerAmount[] Opening =
    [
        new(Instrument.Cash, "TRY", 1_000_000m),
        new(Instrument.Chips, "TRY", 3_000_000m),
        new(Instrument.Cash, "EUR", 50_000m),
        new(Instrument.Chips, "EUR", 200_000m),
        new(Instrument.Cash, "GBP", 20_000m),
        new(Instrument.Cash, "USD", 30_000m),
    ];

    private static UserId Cashier => DemoUsers.Cashier.Id;

    public Task<Guid> OpenShift(DemoRun run, CancellationToken ct) =>
        commands.Ask<ICommandHandler<OpenShiftCommand, Guid>, Guid>(
            "vardiya açılışı", h => h.Handle(new OpenShiftCommand(run.Tenant, Opening, run.Audit(Cashier)), ct));

    public Task SellChips(DemoRun run, Guid shiftId, Guid playerId, decimal amount, string currency, CancellationToken ct) =>
        commands.Ask<SellChipsHandler, CageTransactionResult>(
            "çip satışı", h => h.Handle(Chip(run, shiftId, playerId, amount, currency), ct));

    public Task RedeemChips(DemoRun run, Guid shiftId, Guid playerId, decimal amount, string currency, CancellationToken ct) =>
        commands.Ask<RedeemChipsHandler, CageTransactionResult>(
            "çip bozdurma", h => h.Handle(Chip(run, shiftId, playerId, amount, currency), ct));

    /// <summary>Oyuncu yabancı para verir, TRY alır.</summary>
    public Task ExchangeToBase(DemoRun run, Guid shiftId, Guid playerId, decimal foreignAmount, string currency, CancellationToken ct) =>
        commands.Ask<ExchangeForeignToBaseHandler, CageTransactionResult>(
            "döviz alış", h => h.Handle(Exchange(run, shiftId, playerId, foreignAmount, currency), ct));

    /// <summary>Oyuncu TRY verir, yabancı para alır.</summary>
    public Task ExchangeToForeign(DemoRun run, Guid shiftId, Guid playerId, decimal foreignAmount, string currency, CancellationToken ct) =>
        commands.Ask<ExchangeBaseToForeignHandler, CageTransactionResult>(
            "döviz satış", h => h.Handle(Exchange(run, shiftId, playerId, foreignAmount, currency), ct));

    /// <summary>Sayım beklenen bakiyeye eşittir (farksız kapanış); beklenen bakiye vardiya özetinden okunur.</summary>
    public async Task CloseShift(DemoRun run, Guid shiftId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        var summary = await commands.Query<GetShiftSummaryQuery, ShiftSummary?>(new GetShiftSummaryQuery(run.Tenant, shiftId), ct)
            ?? throw new DemoSeedException($"Vardiya bulunamadı: {shiftId}");
        var counted = summary.Balances
            .Select(b => new DrawerAmount(Enum.Parse<Instrument>(b.Instrument), b.Currency, b.Expected))
            .ToList();
        await commands.Do<ICommandHandler<CloseShiftCommand>>(
            "vardiya kapanışı", h => h.Handle(new CloseShiftCommand(run.Tenant, shiftId, counted, null, run.Audit(Cashier)), ct));
    }

    private static ChipCommand Chip(DemoRun run, Guid shiftId, Guid playerId, decimal amount, string currency) =>
        new(run.Tenant, shiftId, run.NextGuid(), playerId, amount, currency, run.Audit(Cashier));

    private static ExchangeCommand Exchange(DemoRun run, Guid shiftId, Guid playerId, decimal amount, string currency) =>
        new(run.Tenant, shiftId, run.NextGuid(), playerId, amount, currency, run.Audit(Cashier));
}

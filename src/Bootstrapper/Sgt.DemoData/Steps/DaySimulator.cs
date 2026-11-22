using System.Runtime.ExceptionServices;
using Sgt.BuildingBlocks;

namespace Sgt.DemoData.Steps;

/// <summary>
/// Geçmiş günlerin hareketi (Spec 012 KK3–KK4). Her gün, KKTC yerel saatiyle:
/// 09:00 kurlar · 10:00 kasa vardiyası · 11:30 masalar açılır ·
/// 12:30–23:15 oyuncu oturumları (döviz, çip satışı, rating, çip bozdurma) · 23:30 masalar kapanır · 23:40 vardiya kapanır ·
/// ardından gün sonu (<see cref="DayClosing"/>).
/// </summary>
public sealed class DaySimulator(RateBoard rates, CageDesk cage, PitDesk pit, DayClosing closing) : IDemoStep
{
    private const double OpenTableShare = 0.7;
    private const double ExchangeShare = 0.4;
    private const double ExchangeBackShare = 0.1;
    private const double BustShare = 0.15;
    private const int SessionWindowMinutes = 510; // 12:30 + 510 dk = 21:00 son başlangıç
    private const int PlayersPerRating = 8;
    private const int RatingsPerComp = 12;

    private static readonly Dictionary<string, string> HomeCurrency = new(StringComparer.Ordinal) { ["GB"] = "GBP", ["RU"] = "USD", ["DE"] = "EUR" };

    public int Order => 60;

    public string Name => "Günlük hareket";

    public async Task Run(DemoRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        var ratedBefore = new List<Guid>();
        var drain = new BackgroundDrain();
        try
        {
            foreach (var day in run.Timeline.PastDays)
            {
                var plan = new DayPlan();
                var rated = PlanDay(run, day, plan, ratedBefore, drain);
                await plan.Run(run.Clock, ct);
                ratedBefore.AddRange(rated.Except(ratedBefore).ToList());
            }
        }
        catch (Exception original)
        {
            // Arka plan boşaltması yine beklenir; onun hatası asıl hatayı örtmez, ikisi birlikte bildirilir.
            try
            {
                await drain.Pending;
            }
            catch (Exception cleanup)
            {
                throw new AggregateException("Günlük hareket ve arka plan outbox boşaltması başarısız.", original, cleanup);
            }

            ExceptionDispatchInfo.Capture(original).Throw();
            throw; // erişilmez
        }

        await closing.Finish(drain, ct);
    }

    private List<Guid> PlanDay(DemoRun run, DateOnly day, DayPlan plan, List<Guid> ratedBefore, BackgroundDrain drain)
    {
        DateTimeOffset At(int hour, int minute) => DemoTimeline.At(day, new TimeOnly(hour, minute));

        plan.At(At(9, 0), ct => rates.Publish(run, maxDrift: 0.006m, ct));

        var shift = new ShiftHolder();
        plan.At(At(10, 0), async ct => shift.Id = await cage.OpenShift(run, ct));

        var openTables = run.Tables.Where(_ => run.Chance(OpenTableShare)).ToList();
        if (openTables.Count == 0)
        {
            openTables.Add(run.Tables[0]);
        }

        for (var i = 0; i < openTables.Count; i++)
        {
            var table = openTables[i];
            plan.At(At(11, 30).AddSeconds(i * 30), ct => pit.OpenTable(run, table.Id, ct));
        }

        var rated = PlanSessions(run, At(12, 30), openTables, shift, plan);

        foreach (var table in openTables)
        {
            plan.At(At(23, 30), ct => pit.CloseTable(run, table.Id, ct));
        }

        plan.At(At(23, 40), ct => cage.CloseShift(run, shift.Id, ct));
        var compCount = ratedBefore.Count == 0 ? 0 : Math.Max(1, RatingsPerDay(run) / RatingsPerComp);
        var compPlayers = Enumerable.Range(0, compCount).Select(_ => run.Pick(ratedBefore)).ToList();
        closing.Schedule(run, day, plan, compPlayers, drain);
        return rated;
    }

    /// <summary>Oyuncu oturumlarını zamanlar; bir oyuncu aynı anda tek masada oynar, kısıtlı oyuncu oynamaz.</summary>
    private List<Guid> PlanSessions(DemoRun run, DateTimeOffset windowStart, IReadOnlyList<DemoTable> tables, ShiftHolder shift, DayPlan plan)
    {
        var eligible = run.Players.Where(p => !p.IsRestricted).ToList();
        var busyUntil = new Dictionary<Guid, DateTimeOffset>();
        var starts = Enumerable.Range(0, RatingsPerDay(run))
            .Select(_ => windowStart.AddMinutes(run.Next(0, SessionWindowMinutes)))
            .Order()
            .ToList();

        foreach (var start in starts)
        {
            var player = PickFreePlayer(run, eligible, busyUntil, start);
            if (player is null)
            {
                continue;
            }

            var session = PlanSession(run, player, run.Pick(tables), start);
            busyUntil[player.Id] = session.End.AddMinutes(5);
            Schedule(run, session, shift, plan);
        }

        return busyUntil.Keys.ToList();
    }

    private static DemoPlayer? PickFreePlayer(DemoRun run, IReadOnlyList<DemoPlayer> eligible, Dictionary<Guid, DateTimeOffset> busyUntil, DateTimeOffset at)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = run.Pick(eligible);
            if (!busyUntil.TryGetValue(candidate.Id, out var until) || until <= at)
            {
                return candidate;
            }
        }

        return null;
    }

    private static Session PlanSession(DemoRun run, DemoPlayer player, DemoTable table, DateTimeOffset start)
    {
        var averageBet = table.MinBet * run.Next(1, 7);
        var buyIn = averageBet * run.Next(15, 31);
        var result = run.Chance(BustShare)
            ? -buyIn
            : RoundTo(buyIn * (-0.9m + (run.NextRatio() * 1.5m)), table.MinBet);
        var end = start.AddMinutes(run.Next(30, 136));
        return new Session(player, table, start, end, averageBet, buyIn, result);
    }

    private void Schedule(DemoRun run, Session s, ShiftHolder shift, DayPlan plan)
    {
        var rating = new RatingHolder();
        var home = HomeCurrency.GetValueOrDefault(s.Player.Nationality);
        if (home is not null && s.Table.Currency == SupportedCurrencies.Base && run.Chance(ExchangeShare))
        {
            var foreign = decimal.Ceiling(s.BuyIn / run.MidRates[home] / 50m) * 50m;
            plan.At(s.Start.AddMinutes(-4), ct => cage.ExchangeToBase(run, shift.Id, s.Player.Id, foreign, home, ct));
        }

        plan.At(s.Start.AddMinutes(-2), ct => cage.SellChips(run, shift.Id, s.Player.Id, s.BuyIn, s.Table.Currency, ct));
        plan.At(s.Start, async ct => rating.Id = await pit.StartRating(run, s.Table.Id, s.Player.Id, ct));
        plan.At(s.End, ct => pit.FinishRating(run, rating.Id, s.AverageBet, s.Result, ct));

        var cashOut = s.BuyIn + s.Result;
        if (cashOut > 0)
        {
            plan.At(s.End.AddMinutes(3), ct => cage.RedeemChips(run, shift.Id, s.Player.Id, cashOut, s.Table.Currency, ct));
        }

        if (home is not null && run.Chance(ExchangeBackShare))
        {
            var amount = run.Next(1, 4) * 100m;
            plan.At(s.End.AddMinutes(4), ct => cage.ExchangeToForeign(run, shift.Id, s.Player.Id, amount, home, ct));
        }
    }

    private static int RatingsPerDay(DemoRun run) => Math.Max(3, run.Scale.Players / PlayersPerRating);

    private static decimal RoundTo(decimal value, decimal unit) => decimal.Round(value / unit, MidpointRounding.AwayFromZero) * unit;

    private sealed record Session(DemoPlayer Player, DemoTable Table, DateTimeOffset Start, DateTimeOffset End, decimal AverageBet, decimal BuyIn, decimal Result);

    private sealed class ShiftHolder
    {
        public Guid Id { get; set; }
    }

    private sealed class RatingHolder
    {
        public Guid Id { get; set; }
    }
}

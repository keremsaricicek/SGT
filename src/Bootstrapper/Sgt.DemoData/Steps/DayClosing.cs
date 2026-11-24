using Sgt.BuildingBlocks;

namespace Sgt.DemoData.Steps;

/// <summary>Arka planda süren outbox boşaltması; bir sonraki gün onu bekleyerek puan bakiyesini okur.</summary>
public sealed class BackgroundDrain
{
    public Task Pending { get; private set; } = Task.CompletedTask;

    public void Start(Func<Task> drain)
    {
        ArgumentNullException.ThrowIfNull(drain);
        Pending = drain();
    }
}

/// <summary>
/// Gün sonu (KKTC yerel saati): 23:45 slot sayaç okuması · 23:47 comp talep/karar · 23:50 tüm outbox'lar boşaltılır
/// (rating → puan, denetim kaydı). Boşaltma beklenmez, ertesi günün hareketiyle paralel sürer (KK8 süresi);
/// comp'lar ve son adım onu bekler.
/// </summary>
public sealed class DayClosing(SlotMeterRound slots, CompDesk comps, DemoOutbox outbox)
{
    public void Schedule(DemoRun run, DateOnly day, DayPlan plan, IReadOnlyList<Guid> compPlayers, BackgroundDrain drain)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(drain);
        var readAt = DemoTimeline.At(day, new TimeOnly(23, 45));
        plan.At(readAt, ct => slots.Read(run, readAt, ct));

        // Comp, günün başındaki bakiyeyle hesaplanır: dünkü boşaltma bitmiştir, bugünkü olaylar sayılmaz (KK5 determinizm).
        var balanceAsOf = BusinessDay.For(day).Start;
        plan.At(DemoTimeline.At(day, new TimeOnly(23, 47)), async ct =>
        {
            await drain.Pending;
            foreach (var player in compPlayers)
            {
                await comps.Offer(run, player, balanceAsOf, ct);
            }
        });

        plan.At(DemoTimeline.At(day, new TimeOnly(23, 50)), ct =>
        {
            drain.Start(() => outbox.DrainAll(ct));
            return Task.CompletedTask;
        });
    }

    /// <summary>Son günün boşaltmasını bekler ve kalan olayları dağıtır.</summary>
    public async Task Finish(BackgroundDrain drain, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(drain);
        await drain.Pending;
        await outbox.DrainAll(ct);
    }
}

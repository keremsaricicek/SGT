namespace Sgt.DemoData.Steps;

/// <summary>
/// Bir günün zamanlanmış işlemleri. İşlemler zaman sırasıyla (eşit zamanda ekleniş sırasıyla) çalışır;
/// her işlemden önce demo saati işlemin anına getirilir, böylece handler'lar o anı kaydeder.
/// </summary>
public sealed class DayPlan
{
    private readonly List<(DateTimeOffset At, int Sequence, Func<CancellationToken, Task> Action)> _items = [];

    public void At(DateTimeOffset at, Func<CancellationToken, Task> action) => _items.Add((at, _items.Count, action));

    public async Task Run(DemoClock clock, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(clock);
        foreach (var item in _items.OrderBy(i => i.At).ThenBy(i => i.Sequence))
        {
            if (item.At > clock.UtcNow)
            {
                clock.Set(item.At); // önceki işlem saati ilerlettiyse (ör. onay beklemesi) saat geri alınmaz
            }

            await item.Action(ct);
        }
    }
}

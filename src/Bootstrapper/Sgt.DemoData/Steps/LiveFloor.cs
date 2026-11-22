using Sgt.BuildingBlocks;

namespace Sgt.DemoData.Steps;

/// <summary>
/// Bugünün canlı görüntüsü (Spec 012): saat gerçek ana gelir; bir kasa vardiyası açık kalır, masaların bir kısmı açılır
/// ve birkaç oyuncunun rating'i sürer. Seçimler sabit tohumdan gelir (KK5).
/// Tekrar güvenliği: çip satışı tohumdan türeyen idempotency anahtarı taşır. Vardiya açma, masa açma ve rating başlatma
/// komutlarında anahtar yoktur; aynı kasiyer/terminalde ikinci vardiya, açık masayı tekrar açma ve süren rating'i olan
/// oyuncuya ikinci rating modül kuralıyla reddedilir. Ayrıca kapı (<see cref="DemoSeedGate"/>) yarım yüklemede adımları
/// hiç çalıştırmaz; bu adım bir tenant'ta yalnızca bir kez çalışır.
/// </summary>
public sealed class LiveFloor(CageDesk cage, PitDesk pit) : IDemoStep
{
    private const int TablesEvery = 4;
    private const int PlayersPerLiveRating = 50;

    public int Order => 70;

    public string Name => "Bugün (canlı)";

    public async Task Run(DemoRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        if (run.Clock.UtcNow < run.Timeline.Now)
        {
            run.Clock.Set(run.Timeline.Now);
        }

        var shiftId = await cage.OpenShift(run, ct);
        var tables = run.Tables.Where((_, i) => i % TablesEvery == 0).ToList();
        foreach (var table in tables)
        {
            await pit.OpenTable(run, table.Id, ct);
        }

        var count = Math.Max(2, run.Scale.Players / PlayersPerLiveRating);
        var players = run.Players.Where(p => !p.IsRestricted).Take(count).ToList();
        foreach (var player in players)
        {
            var table = run.Pick(tables);
            var buyIn = table.MinBet * run.Next(10, 31);
            await cage.SellChips(run, shiftId, player.Id, buyIn, table.Currency, ct);
            await pit.StartRating(run, table.Id, player.Id, ct);
        }
    }
}

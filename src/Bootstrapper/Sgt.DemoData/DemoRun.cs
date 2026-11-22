using Sgt.BuildingBlocks;

namespace Sgt.DemoData;

/// <summary>Demo verisi isteği: hangi tenant, hangi ölçek, hangi zaman çizelgesi.</summary>
public sealed record DemoSeedOptions(TenantId Tenant, DemoScale Scale, DemoTimeline Timeline);

/// <summary>Sonuç: <see cref="Seeded"/> false ise tenant'ta demo verisi zaten vardı ve hiçbir şey yazılmadı (KK5).</summary>
public sealed record DemoSeedResult(bool Seeded, TimeSpan Elapsed);

public sealed record DemoPlayer(Guid Id, string Nationality, bool IsRestricted);

public sealed record DemoTable(Guid Id, string Code, string Currency, decimal MinBet, decimal MaxBet);

/// <summary>Slot makinesi ve son okunan kümülatif sayaçları.</summary>
public sealed class DemoSlot(string assetNumber, string currency, decimal denomination)
{
    public string AssetNumber { get; } = assetNumber;

    public string Currency { get; } = currency;

    public decimal Denomination { get; } = denomination;

    public decimal CoinIn { get; private set; }

    public decimal CoinOut { get; private set; }

    public decimal Jackpot { get; private set; }

    public long GamesPlayed { get; private set; }

    public void Play(decimal coinIn, decimal coinOut, decimal jackpot, long games)
    {
        CoinIn += coinIn;
        CoinOut += coinOut;
        Jackpot += jackpot;
        GamesPlayed += games;
    }
}

/// <summary>
/// Tek bir üretimin paylaşılan durumu: sabit tohumlu rastgelelik, saat ve adımların ürettiği kimlikler.
/// Adımlar sırayla çalışır; aynı tohum aynı veriyi üretir (KK5).
/// </summary>
public sealed class DemoRun
{
    /// <summary>Sabit tohum (KK5).</summary>
    public const int Seed = 20_261_121;

    public const string Terminal = "DEMO-SEED";

    public const string Reason = "DEMO";

#pragma warning disable CA5394 // Güvenlik amacı yok: tekrarlanabilir demo verisi için sabit tohumlu rastgelelik.
    private readonly Random _random = new(Seed);

    public DemoRun(DemoSeedOptions options, DemoClock clock)
    {
        ArgumentNullException.ThrowIfNull(options);
        Tenant = options.Tenant;
        Scale = options.Scale;
        Timeline = options.Timeline;
        Clock = clock;
        Names = new SyntheticNames(Seed);
    }

    public TenantId Tenant { get; }

    public DemoScale Scale { get; }

    public DemoTimeline Timeline { get; }

    public DemoClock Clock { get; }

    public SyntheticNames Names { get; }

    public List<DemoPlayer> Players { get; } = [];

    public List<DemoTable> Tables { get; } = [];

    public List<DemoSlot> Slots { get; } = [];

    /// <summary>Yabancı para orta kurları (1 birim = ? TRY); günlük hafif dalgalanır.</summary>
    public Dictionary<string, decimal> MidRates { get; } = new(StringComparer.Ordinal)
    {
        ["EUR"] = 45.20m,
        ["GBP"] = 52.60m,
        ["USD"] = 39.10m,
    };

    /// <summary>[min, max) aralığında tamsayı.</summary>
    public int Next(int min, int max) => _random.Next(min, max);

    /// <summary>[0, 1) aralığında oran. Yalnızca demo verisinin rastgeleliği içindir; para hesabı bununla yapılmaz, sonuç tutarlar yuvarlanır.</summary>
    public decimal NextRatio() => (decimal)_random.NextDouble();

    public bool Chance(double probability) => _random.NextDouble() < probability;

    public T Pick<T>(IReadOnlyList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items[_random.Next(items.Count)];
    }

    /// <summary>
    /// Tohumdan türeyen kimlik (idempotency anahtarı, correlation): aynı çalıştırma aynı kimliği verir.
    /// Tenant kimliğiyle karıştırılır; aynı veritabanındaki iki demo tenant'ı aynı anahtarı üretmez.
    /// </summary>
    public Guid NextGuid()
    {
        Span<byte> bytes = stackalloc byte[16];
        Span<byte> tenant = stackalloc byte[16];
        _random.NextBytes(bytes);
        Tenant.Value.TryWriteBytes(tenant);
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= tenant[i];
        }

        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x40); // sürüm 4
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // RFC 4122 varyantı
        return new Guid(bytes);
    }
#pragma warning restore CA5394

    /// <summary>Kritik işlem bilgisi: aktör, terminal DEMO-SEED, gerekçe DEMO, correlation (anayasa VII).</summary>
    public AuditContext Audit(UserId actor) => new(actor, Terminal, ReasonCode.Of(Reason), NextGuid());
}

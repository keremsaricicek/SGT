using Sgt.BuildingBlocks;

namespace Sgt.Modules.GamingFloor.Domain.Slots;

public static class SlotErrors
{
    public static readonly Error InvalidAssetNumber = new("Slot.InvalidAssetNumber", "Varlık numarası 1-30 karakter (harf, rakam, -) olmalıdır.");
    public static readonly Error InvalidDenomination = new("Slot.InvalidDenomination", "Kupür sıfırdan büyük olmalıdır.");
    public static readonly Error UnsupportedCurrency = new("Slot.UnsupportedCurrency", "Desteklenen para birimleri: TRY, EUR, GBP, USD.");
    public static readonly Error AssetNumberTaken = new("Slot.AssetNumberTaken", "Bu varlık numarası kullanımda.", ErrorKind.Conflict);
    public static readonly Error MachineNotFound = new("Slot.MachineNotFound", "Makine bulunamadı.", ErrorKind.NotFound);
    public static readonly Error InvalidMeters = new("Slot.InvalidMeters", "Sayaçlar negatif olamaz; tutarlar en çok 2 ondalıklı olmalıdır.");
    public static readonly Error MeterWentBackwards = new("Slot.MeterWentBackwards", "Sayaç önceki okumadan küçük; sıfırlandıysa işaretleyin.", ErrorKind.Conflict);
    public static readonly Error ReadingNotAfterPrevious = new("Slot.ReadingNotAfterPrevious", "Okuma zamanı önceki okumadan sonra olmalıdır.", ErrorKind.Conflict);
    public static readonly Error ResetNoteRequired = new("Slot.ResetNoteRequired", "Sayaç sıfırlama için açıklama zorunludur.");
    public static readonly Error ReferenceRequired = new("Slot.ReferenceRequired", "Kaynak referansı zorunludur.");
}

/// <summary>Birikimli makine sayaçları (KK2). Tutarlar makinenin para birimindedir.</summary>
public sealed record MeterSnapshot(decimal CoinIn, decimal CoinOut, decimal Jackpot, long GamesPlayed)
{
    public static readonly MeterSnapshot Zero = new(0, 0, 0, 0);

    /// <summary>Makine kazancı = giren − çıkan − jackpot (KK5).</summary>
    public decimal MachineWin => CoinIn - CoinOut - Jackpot;

    public bool IsValid =>
        CoinIn >= 0 && CoinOut >= 0 && Jackpot >= 0 && GamesPlayed >= 0
        && decimal.Round(CoinIn, 2) == CoinIn && decimal.Round(CoinOut, 2) == CoinOut && decimal.Round(Jackpot, 2) == Jackpot;

    public bool AnyBelow(MeterSnapshot previous) =>
        CoinIn < previous.CoinIn || CoinOut < previous.CoinOut || Jackpot < previous.Jackpot || GamesPlayed < previous.GamesPlayed;

    public MeterSnapshot Minus(MeterSnapshot previous) =>
        new(CoinIn - previous.CoinIn, CoinOut - previous.CoinOut, Jackpot - previous.Jackpot, GamesPlayed - previous.GamesPlayed);

    public MeterSnapshot Plus(MeterSnapshot other) =>
        new(CoinIn + other.CoinIn, CoinOut + other.CoinOut, Jackpot + other.Jackpot, GamesPlayed + other.GamesPlayed);
}

public enum ReadingSource
{
    Manual,
    Import,
    Nexio,
    SSlot,
}

/// <summary>Bir sayaç okuması; kaynağı ve kaynak referansı idempotency anahtarıdır (KK4).</summary>
public sealed record MeterReading(
    Guid Id, DateTimeOffset ReadAt, MeterSnapshot Meters, ReadingSource Source, string SourceReference, bool IsReset, string? Note, AuditContext Audit);

/// <summary>Kaydedilmiş okuma ve önceki okumaya göre faaliyet farkı.</summary>
public sealed record RecordedReading(Guid MachineId, MeterReading Reading, MeterSnapshot Delta);

/// <summary>
/// Slot makinesi (KK1). Son okumayı tutar; yeni okuma ona göre doğrulanır ve fark hesaplanır (KK3, KK5).
/// Okumaların tamamı ayrı, değiştirilemez kayıtlardır.
/// </summary>
public sealed class SlotMachine
{
    private readonly List<FloorChange> _pending = [];

    private SlotMachine(
        Guid id, TenantId tenantId, string assetNumber, string area, string manufacturer, string model, decimal denomination, string currency)
    {
        Id = id;
        TenantId = tenantId;
        AssetNumber = assetNumber;
        Area = area;
        Manufacturer = manufacturer;
        Model = model;
        Denomination = denomination;
        Currency = currency;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public string AssetNumber { get; }

    public string Area { get; }

    public string Manufacturer { get; }

    public string Model { get; }

    public decimal Denomination { get; }

    public string Currency { get; }

    public DateTimeOffset? LastReadAt { get; private set; }

    public MeterSnapshot? LastMeters { get; private set; }

    public IReadOnlyList<FloorChange> PendingChanges => _pending;

    public static Result<SlotMachine> Define(
        Guid id, TenantId tenantId, string assetNumber, string area, string manufacturer, string model, decimal denomination, string currency,
        AuditContext audit, DateTimeOffset at)
    {
        var asset = (assetNumber ?? string.Empty).Trim().ToUpperInvariant();
        if (asset.Length is < 1 or > 30 || !asset.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'))
        {
            return SlotErrors.InvalidAssetNumber;
        }

        if (denomination <= 0)
        {
            return SlotErrors.InvalidDenomination;
        }

        if (!SupportedCurrencies.IsSupported(currency))
        {
            return SlotErrors.UnsupportedCurrency;
        }

        var machine = new SlotMachine(id, tenantId, asset, Clean(area), Clean(manufacturer), Clean(model), denomination, currency.Trim().ToUpperInvariant());
        machine._pending.Add(new FloorChange(Guid.NewGuid(), FloorChangeKind.SlotMachineDefined, asset, audit, at));
        return machine;
    }

    public static SlotMachine Restore(
        Guid id, TenantId tenantId, string assetNumber, string area, string manufacturer, string model, decimal denomination, string currency,
        DateTimeOffset? lastReadAt, MeterSnapshot? lastMeters) =>
        new(id, tenantId, assetNumber, area, manufacturer, model, denomination, currency) { LastReadAt = lastReadAt, LastMeters = lastMeters };

    public Result<RecordedReading> Record(MeterReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);
        if (!reading.Meters.IsValid)
        {
            return SlotErrors.InvalidMeters;
        }

        if (string.IsNullOrWhiteSpace(reading.SourceReference))
        {
            return SlotErrors.ReferenceRequired;
        }

        if (LastReadAt is { } last && reading.ReadAt <= last)
        {
            return SlotErrors.ReadingNotAfterPrevious;
        }

        if (reading.IsReset && string.IsNullOrWhiteSpace(reading.Note))
        {
            return SlotErrors.ResetNoteRequired;
        }

        var previous = reading.IsReset ? MeterSnapshot.Zero : LastMeters ?? MeterSnapshot.Zero;
        if (reading.Meters.AnyBelow(previous))
        {
            return SlotErrors.MeterWentBackwards;
        }

        LastReadAt = reading.ReadAt;
        LastMeters = reading.Meters;
        return new RecordedReading(Id, reading, reading.Meters.Minus(previous));
    }

    public void ClearPendingChanges() => _pending.Clear();

    private static string Clean(string? value) => (value ?? string.Empty).Trim();
}

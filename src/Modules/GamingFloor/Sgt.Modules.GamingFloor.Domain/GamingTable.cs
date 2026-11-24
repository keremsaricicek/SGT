using Sgt.BuildingBlocks;

namespace Sgt.Modules.GamingFloor.Domain;

/// <summary>Bir masa oturumu (açılış–kapanış).</summary>
public sealed record TableSession(Guid Id, DateTimeOffset OpenedAt, UserId OpenedBy, DateTimeOffset? ClosedAt, UserId? ClosedBy)
{
    public bool IsOpen => ClosedAt is null;
}

/// <summary>Canlı oyun masası ve oturumları (Spec 007 KK1–KK2).</summary>
public sealed class GamingTable
{
    private readonly List<TableSession> _sessions = [];
    private readonly List<FloorChange> _pending = [];

    private GamingTable(
        Guid id, TenantId tenantId, string code, GameType gameType, string currency, decimal minBet, decimal maxBet, decimal houseEdgePercent, int handsPerHour)
    {
        Id = id;
        TenantId = tenantId;
        Code = code;
        GameType = gameType;
        Currency = currency;
        MinBet = minBet;
        MaxBet = maxBet;
        HouseEdgePercent = houseEdgePercent;
        HandsPerHour = handsPerHour;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public string Code { get; }

    public GameType GameType { get; }

    public string Currency { get; }

    public decimal MinBet { get; }

    public decimal MaxBet { get; }

    /// <summary>Kasa avantajı yüzde olarak (ör. 2.5).</summary>
    public decimal HouseEdgePercent { get; }

    public int HandsPerHour { get; }

    public IReadOnlyList<TableSession> Sessions => _sessions;

    public bool IsOpen => _sessions.Any(s => s.IsOpen);

    public IReadOnlyList<FloorChange> PendingChanges => _pending;

    public static Result<GamingTable> Define(
        Guid id, TenantId tenantId, string code, GameType gameType, string currency, decimal minBet, decimal maxBet,
        decimal houseEdgePercent, int handsPerHour, AuditContext audit, DateTimeOffset at)
    {
        var normalizedCode = (code ?? string.Empty).Trim().ToUpperInvariant();
        if (normalizedCode.Length is < 2 or > 20 || !normalizedCode.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'))
        {
            return FloorErrors.InvalidTableCode;
        }

        if (!SupportedCurrencies.IsSupported(currency))
        {
            return FloorErrors.UnsupportedCurrency;
        }

        if (minBet <= 0 || minBet > maxBet)
        {
            return FloorErrors.InvalidLimits;
        }

        if (houseEdgePercent is < 0 or > 100)
        {
            return FloorErrors.InvalidHouseEdge;
        }

        if (handsPerHour is < 1 or > 1000)
        {
            return FloorErrors.InvalidHandsPerHour;
        }

        var table = new GamingTable(id, tenantId, normalizedCode, gameType, currency.Trim().ToUpperInvariant(), minBet, maxBet, houseEdgePercent, handsPerHour);
        table.Record(FloorChangeKind.TableDefined, normalizedCode, audit, at);
        return table;
    }

    public static GamingTable Restore(
        Guid id, TenantId tenantId, string code, GameType gameType, string currency, decimal minBet, decimal maxBet, decimal houseEdgePercent,
        int handsPerHour, IEnumerable<TableSession> sessions)
    {
        var table = new GamingTable(id, tenantId, code, gameType, currency, minBet, maxBet, houseEdgePercent, handsPerHour);
        table._sessions.AddRange(sessions);
        return table;
    }

    public Result Open(AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        if (IsOpen)
        {
            return FloorErrors.TableAlreadyOpen;
        }

        _sessions.Add(new TableSession(Guid.NewGuid(), at, audit.Actor, null, null));
        Record(FloorChangeKind.TableOpened, Code, audit, at);
        return Result.Success();
    }

    /// <summary>Masada süren rating varsa kapanmaz (KK2); bu sayı rating deposundan gelir.</summary>
    public Result Close(int runningRatings, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        var open = _sessions.SingleOrDefault(s => s.IsOpen);
        if (open is null)
        {
            return FloorErrors.TableNotOpen;
        }

        if (runningRatings > 0)
        {
            return FloorErrors.RatingsStillRunning;
        }

        _sessions[_sessions.IndexOf(open)] = open with { ClosedAt = at, ClosedBy = audit.Actor };
        Record(FloorChangeKind.TableClosed, Code, audit, at);
        return Result.Success();
    }

    public bool AcceptsBet(decimal averageBet) =>
        averageBet >= MinBet && averageBet <= MaxBet && decimal.Round(averageBet, 2) == averageBet;

    public void ClearPendingChanges() => _pending.Clear();

    private void Record(FloorChangeKind kind, string detail, AuditContext audit, DateTimeOffset at) =>
        _pending.Add(new FloorChange(Guid.NewGuid(), kind, detail, audit, at));
}

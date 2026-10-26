using Sgt.BuildingBlocks;

namespace Sgt.Modules.GamingFloor.Domain;

/// <summary>Bitmiş rating'e yapılan düzeltme; önceki değerleri saklar (KK6).</summary>
public sealed record RatingCorrection(
    Guid Id, decimal PreviousAverageBet, decimal? PreviousPlayerResult, decimal AverageBet, decimal? PlayerResult, AuditContext Audit, DateTimeOffset At);

/// <summary>
/// Oyuncunun bir masadaki oyununun derecelendirmesi (Spec 007 KK3–KK6).
/// Masa parametreleri başlangıçta kopyalanır; masa tanımı sonradan değişse de rating değişmez.
/// </summary>
public sealed class Rating
{
    private readonly List<RatingCorrection> _corrections = [];
    private readonly List<FloorChange> _pending = [];

    private Rating(
        Guid id, TenantId tenantId, Guid playerId, Guid tableId, string tableCode, string currency, decimal houseEdgePercent,
        int handsPerHour, DateTimeOffset startedAt, UserId startedBy)
    {
        Id = id;
        TenantId = tenantId;
        PlayerId = playerId;
        TableId = tableId;
        TableCode = tableCode;
        Currency = currency;
        HouseEdgePercent = houseEdgePercent;
        HandsPerHour = handsPerHour;
        StartedAt = startedAt;
        StartedBy = startedBy;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public Guid PlayerId { get; }

    public Guid TableId { get; }

    public string TableCode { get; }

    public string Currency { get; }

    public decimal HouseEdgePercent { get; }

    public int HandsPerHour { get; }

    public DateTimeOffset StartedAt { get; }

    public UserId StartedBy { get; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public decimal? AverageBet { get; private set; }

    /// <summary>Oyuncunun sonucu: + kazanç, − kayıp.</summary>
    public decimal? PlayerResult { get; private set; }

    public decimal? TheoreticalWin { get; private set; }

    public bool IsRunning => FinishedAt is null;

    public IReadOnlyList<RatingCorrection> Corrections => _corrections;

    public IReadOnlyList<FloorChange> PendingChanges => _pending;

    public static Rating Start(Guid id, GamingTable table, Guid playerId, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(audit);
        var rating = new Rating(id, table.TenantId, playerId, table.Id, table.Code, table.Currency, table.HouseEdgePercent, table.HandsPerHour, at, audit.Actor);
        rating.Record(FloorChangeKind.RatingStarted, table.Code, audit, at);
        return rating;
    }

    public static Rating Restore(
        Guid id, TenantId tenantId, Guid playerId, Guid tableId, string tableCode, string currency, decimal houseEdgePercent, int handsPerHour,
        DateTimeOffset startedAt, UserId startedBy, DateTimeOffset? finishedAt, decimal? averageBet, decimal? playerResult, decimal? theoreticalWin,
        IEnumerable<RatingCorrection> corrections)
    {
        var rating = new Rating(id, tenantId, playerId, tableId, tableCode, currency, houseEdgePercent, handsPerHour, startedAt, startedBy)
        {
            FinishedAt = finishedAt,
            AverageBet = averageBet,
            PlayerResult = playerResult,
            TheoreticalWin = theoreticalWin,
        };
        rating._corrections.AddRange(corrections);
        return rating;
    }

    public Result Finish(GamingTable table, decimal averageBet, decimal? playerResult, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(table);
        if (!IsRunning)
        {
            return FloorErrors.RatingNotRunning;
        }

        var valid = Validate(table, averageBet, playerResult);
        if (valid.IsFailure)
        {
            return valid;
        }

        FinishedAt = at < StartedAt ? StartedAt : at;
        Apply(averageBet, playerResult);
        Record(FloorChangeKind.RatingFinished, $"{TableCode} {averageBet} {Currency}", audit, at);
        return Result.Success();
    }

    public Result Correct(GamingTable table, decimal averageBet, decimal? playerResult, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(table);
        if (IsRunning)
        {
            return FloorErrors.RatingNotFinished;
        }

        var valid = Validate(table, averageBet, playerResult);
        if (valid.IsFailure)
        {
            return valid;
        }

        _corrections.Add(new RatingCorrection(Guid.NewGuid(), AverageBet!.Value, PlayerResult, averageBet, playerResult, audit, at));
        Apply(averageBet, playerResult);
        Record(FloorChangeKind.RatingCorrected, $"{TableCode} {averageBet} {Currency}", audit, at);
        return Result.Success();
    }

    public void ClearPendingChanges() => _pending.Clear();

    private static Result Validate(GamingTable table, decimal averageBet, decimal? playerResult)
    {
        if (!table.AcceptsBet(averageBet))
        {
            return FloorErrors.BetOutsideLimits;
        }

        return playerResult is { } r && decimal.Round(r, 2) != r ? FloorErrors.InvalidResult : Result.Success();
    }

    /// <summary>Teorik kazanç = ortalama bahis × saatlik el × saat × kasa avantajı (KK5).</summary>
    private void Apply(decimal averageBet, decimal? playerResult)
    {
        var hours = (decimal)(FinishedAt!.Value - StartedAt).TotalHours;
        AverageBet = averageBet;
        PlayerResult = playerResult;
        TheoreticalWin = decimal.Round(averageBet * HandsPerHour * hours * HouseEdgePercent / 100m, 2, MidpointRounding.AwayFromZero);
    }

    private void Record(FloorChangeKind kind, string detail, AuditContext audit, DateTimeOffset at) =>
        _pending.Add(new FloorChange(Guid.NewGuid(), kind, detail, audit, at));
}

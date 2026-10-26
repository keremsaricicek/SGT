using Sgt.BuildingBlocks;

namespace Sgt.Modules.Finance.Domain;

/// <summary>Kasa işlemi düzeltme isteği; başka bir yetkili onaylayınca ters kayıt yapılır (Spec 010).</summary>
public sealed class CageAdjustmentRequest
{
    private readonly List<ShiftChange> _pending = [];

    private CageAdjustmentRequest(
        Guid id, TenantId tenantId, Guid shiftId, Guid transactionId, string note, DateTimeOffset requestedAt, FourEyesDecision decision, Guid? reversalId)
    {
        Id = id;
        TenantId = tenantId;
        ShiftId = shiftId;
        TransactionId = transactionId;
        Note = note;
        RequestedAt = requestedAt;
        Decision = decision;
        ReversalId = reversalId;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public Guid ShiftId { get; }

    public Guid TransactionId { get; }

    public string Note { get; }

    public DateTimeOffset RequestedAt { get; }

    public FourEyesDecision Decision { get; }

    public Guid? ReversalId { get; private set; }

    public IReadOnlyList<ShiftChange> PendingChanges => _pending;

    public static Result<CageAdjustmentRequest> Create(CashierShift shift, Guid transactionId, string note, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(shift);
        ArgumentNullException.ThrowIfNull(audit);
        var reversible = shift.CanReverse(transactionId);
        if (reversible.IsFailure)
        {
            return reversible.Error;
        }

        if (string.IsNullOrWhiteSpace(note))
        {
            return FinanceErrors.AdjustmentNoteRequired;
        }

        var request = new CageAdjustmentRequest(Guid.NewGuid(), shift.TenantId, shift.Id, transactionId, note.Trim(), at, FourEyesDecision.Request(audit.Actor), null);
        request._pending.Add(new ShiftChange(Guid.NewGuid(), ShiftChangeKind.AdjustmentRequested, transactionId, request.Note, audit, at));
        return request;
    }

    public static CageAdjustmentRequest Restore(
        Guid id, TenantId tenantId, Guid shiftId, Guid transactionId, string note, DateTimeOffset requestedAt, FourEyesDecision decision, Guid? reversalId) =>
        new(id, tenantId, shiftId, transactionId, note, requestedAt, decision, reversalId);

    public Result Decide(CashierShift shift, bool approve, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(shift);
        ArgumentNullException.ThrowIfNull(audit);
        var allowed = Decision.CanDecide(audit.Actor);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        if (approve)
        {
            var reversal = shift.Reverse(TransactionId, Guid.NewGuid(), audit, at);
            if (reversal.IsFailure)
            {
                return reversal.Error;
            }

            ReversalId = reversal.Value.Id;
        }

        Decision.Decide(audit.Actor, approve, at);
        _pending.Add(new ShiftChange(
            Guid.NewGuid(), approve ? ShiftChangeKind.AdjustmentApproved : ShiftChangeKind.AdjustmentRejected, TransactionId, Note, audit, at));
        return Result.Success();
    }

    public void ClearPendingChanges() => _pending.Clear();
}

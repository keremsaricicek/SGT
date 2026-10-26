using Sgt.BuildingBlocks;

namespace Sgt.Modules.Finance.Domain;

public enum ShiftChangeKind
{
    Opened,
    TransactionRecorded,
    Closed,
}

/// <summary>Denetime düşecek vardiya olayı. TransactionId yalnızca işlem kaydında doludur.</summary>
public sealed record ShiftChange(Guid Id, ShiftChangeKind Kind, Guid? TransactionId, string Detail, AuditContext Audit, DateTimeOffset OccurredAt);

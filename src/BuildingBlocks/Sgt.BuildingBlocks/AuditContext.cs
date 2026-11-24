namespace Sgt.BuildingBlocks;

/// <summary>Her kritik işlemin taşıdığı denetim bilgisi: kim, hangi terminalden, neden, hangi akış içinde.</summary>
public sealed record AuditContext(UserId Actor, string Terminal, ReasonCode Reason, Guid CorrelationId);

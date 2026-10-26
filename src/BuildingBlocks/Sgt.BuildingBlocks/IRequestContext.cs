namespace Sgt.BuildingBlocks;

/// <summary>Kimliği doğrulanmış isteğin bilgileri: hangi müşteri, hangi kullanıcı, hangi terminal.</summary>
public interface IRequestContext
{
    TenantId TenantId { get; }

    UserId UserId { get; }

    AuditContext Audit(ReasonCode reason);
}

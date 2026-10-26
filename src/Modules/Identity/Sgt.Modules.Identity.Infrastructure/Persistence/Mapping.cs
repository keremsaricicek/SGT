using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Contracts;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Infrastructure.Persistence;

/// <summary>Domain nesneleri ile veritabanı satırları arasındaki çeviri.</summary>
internal static class Mapping
{
    public static Role ToDomain(this RoleRow row) =>
        Role.Restore(new RoleId(row.Id), new TenantId(row.TenantId), row.Name, ToSet(row.Permissions));

    public static UserAccess ToDomain(this UserAccessRow row) =>
        UserAccess.Restore(
            new UserId(row.UserId),
            new TenantId(row.TenantId),
            row.IsSuperAdmin,
            row.RoleIds.Select(id => new RoleId(id)),
            ToSet(row.Grants),
            row.Denied.Select(ParseStored));

    public static void CopyFrom(this RoleRow row, Role role)
    {
        row.Id = role.Id.Value;
        row.TenantId = role.TenantId.Value;
        row.Name = role.Name;
        row.Permissions = ToStrings(role.Permissions.Codes);
    }

    public static void CopyFrom(this UserAccessRow row, UserAccess user)
    {
        row.UserId = user.UserId.Value;
        row.TenantId = user.TenantId.Value;
        row.IsSuperAdmin = user.IsSuperAdmin;
        row.RoleIds = user.RoleIds.Select(r => r.Value).Order().ToList();
        row.Grants = ToStrings(user.Grants.Codes);
        row.Denied = ToStrings(user.Denied);
    }

    public static PermissionChangeRow ToRow(this PermissionChange change) => new()
    {
        Id = change.Id,
        TenantId = change.TenantId.Value,
        Target = change.Target.ToString(),
        TargetId = change.TargetId,
        Kind = change.Kind.ToString(),
        Item = change.Item,
        ActorId = change.Audit.Actor.Value,
        Terminal = change.Audit.Terminal,
        ReasonCode = change.Audit.Reason.Value,
        CorrelationId = change.Audit.CorrelationId,
        OccurredAt = change.OccurredAt,
    };

    public static PermissionChangedV1 ToIntegrationEvent(this PermissionChange change) => new(
        change.Id,
        change.Audit.CorrelationId,
        change.OccurredAt,
        change.TenantId,
        change.Audit,
        change.Target.ToString(),
        change.TargetId,
        change.Kind.ToString(),
        change.Item);

    private static PermissionSet ToSet(IEnumerable<string> codes) => PermissionSet.FromCodes(codes.Select(ParseStored));

    private static List<string> ToStrings(IEnumerable<PermissionCode> codes) =>
        codes.Select(c => c.ToString()).Order(StringComparer.Ordinal).ToList();

    private static PermissionCode ParseStored(string value)
    {
        var code = PermissionCode.Parse(value);
        return code.IsSuccess ? code.Value : throw new InvalidOperationException($"Kayıtlı yetki kodu geçersiz: {value}");
    }
}

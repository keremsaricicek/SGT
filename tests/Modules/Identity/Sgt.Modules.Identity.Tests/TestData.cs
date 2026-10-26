using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

/// <summary>Sentetik test verisi. Gerçek kişi verisi kullanılmaz.</summary>
internal static class TestData
{
    public static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    public static readonly TenantId OtherTenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000b2"));
    public static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    public static readonly PermissionCode CageView = Code("cage.adjustment:view");
    public static readonly PermissionCode CageOperate = Code("cage.adjustment:operate");
    public static readonly PermissionCode CageApprove = Code("cage.adjustment:approve");
    public static readonly PermissionCode SlotView = Code("slot.floor:view");

    public static PermissionCode Code(string value) => PermissionCode.Parse(value).Value;

    public static AuditContext Audit() =>
        new(new UserId(Guid.Parse("00000000-0000-0000-0000-000000000001")), "TEST-PC", ReasonCode.Of("ROLE_SETUP"),
            Guid.Parse("00000000-0000-0000-0000-00000000c0de"));

    public static AdminActor SuperAdmin(TenantId? tenant = null) =>
        AdminActor.From(tenant ?? Tenant, Audit(), isSuperAdmin: true).Value;

    public static Role NewRole(string name = "Kasiyer", TenantId? tenant = null) =>
        Role.Create(RoleId.New(), name, SuperAdmin(tenant), Now).Value;

    public static UserAccess NewUser(TenantId? tenant = null) =>
        UserAccess.Create(UserId.New(), tenant ?? Tenant, isSuperAdmin: false);
}

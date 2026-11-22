using System.Globalization;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Roles.CreateDefaultRoles;

namespace Sgt.DemoData;

/// <summary>Bir demo kullanıcısı: Keycloak'taki sabit kimlik (sub), kullanıcı adı ve SGT rolü (Spec 012 KK6).</summary>
public sealed record DemoUser(UserId Id, string UserName, string DisplayName, string EmployeeCode, string? Role);

/// <summary>
/// Demo kullanıcıları. Kimlikler <c>deploy/keycloak/sgt-realm.json</c> ile aynıdır; şifreler <c>deploy/.env</c>'dedir.
/// Süper admin ilk tohumla (IdentityDevelopmentSeed) gelir; rolü yoktur, yetki panelini yönetir.
/// </summary>
public static class DemoUsers
{
    public const string CashierRole = "Kasiyer";
    public const string HostRole = "Host";
    public const string CasinoManagerRole = DefaultRoleTemplates.CasinoManager;
    public const string PitBossRole = DefaultRoleTemplates.PitBoss;
    public const string SlotManagerRole = DefaultRoleTemplates.SlotManager;

    public static readonly DemoUser SuperAdmin = new(Id(1), "superadmin", "Süper Admin", "D-0001", null);
    public static readonly DemoUser Cashier = new(Id(2), "kasiyer1", "Demo Kasiyer", "D-0002", CashierRole);
    public static readonly DemoUser CasinoManager = new(Id(3), "mudur", "Demo Casino Müdürü", "D-0003", CasinoManagerRole);
    public static readonly DemoUser PitBoss = new(Id(4), "pitboss", "Demo Pit Boss", "D-0004", PitBossRole);
    public static readonly DemoUser SlotManager = new(Id(5), "slotmudur", "Demo Slot Müdürü", "D-0005", SlotManagerRole);
    public static readonly DemoUser Host = new(Id(6), "host", "Demo Host", "D-0006", HostRole);

    /// <summary>Süper admin dışındaki, rol atanan kullanıcılar.</summary>
    public static readonly IReadOnlyList<DemoUser> Staff = [Cashier, CasinoManager, PitBoss, SlotManager, Host];

    public static readonly IReadOnlyList<DemoUser> All = [SuperAdmin, .. Staff];

    private static UserId Id(int n) => new(Guid.Parse("00000000-0000-0000-0000-" + n.ToString("D12", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));
}

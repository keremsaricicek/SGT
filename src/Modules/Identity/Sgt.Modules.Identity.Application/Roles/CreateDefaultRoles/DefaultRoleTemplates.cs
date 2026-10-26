using Sgt.Modules.Identity.Application.Ports;

namespace Sgt.Modules.Identity.Application.Roles.CreateDefaultRoles;

/// <summary>
/// Hazır gelen rol şablonları (docs/kararlar.md bölüm 4). Şablon, hangi ekranların tüm eylemleriyle
/// role verileceğini seçer. İşletme rolleri sonradan panelden değiştirebilir.
/// </summary>
public sealed record RoleTemplate(string Name, Func<ScreenDefinition, bool> Includes);

public static class DefaultRoleTemplates
{
    public const string SlotManager = "Slot müdürü";
    public const string PitBoss = "Pit boss";
    public const string CasinoManager = "Casino müdürü";
    public const string HotelGeneralManager = "Otel genel müdürü";
    public const string HotelStaff = "Otel personeli";

    // Slot ve canlı oyun birbirini görmez; "oyuncu şu an nerede" araması ortaktır.
    private static readonly string[] SharedPlayerLookup = ["player.search", "player.locator"];

    public static readonly IReadOnlyList<RoleTemplate> All =
    [
        new(SlotManager, s => s.Area == "slot" || s.Code == "report.slot" || IsSharedForFloor(s)),
        new(PitBoss, s => s.Area == "live" || s.Code == "report.live" || IsSharedForFloor(s)),
        new(CasinoManager, s => s.Area != "admin"),
        new(HotelGeneralManager, s => s.Area != "admin"),
        new(HotelStaff, s => s.Area == "hotel"),
    ];

    private static bool IsSharedForFloor(ScreenDefinition screen) =>
        SharedPlayerLookup.Contains(screen.Code) || screen.Code == "report.daily";
}

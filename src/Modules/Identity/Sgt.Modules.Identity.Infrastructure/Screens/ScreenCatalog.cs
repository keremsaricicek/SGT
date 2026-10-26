using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Infrastructure.Screens;

/// <summary>
/// Ekran kataloğu (docs/s-plus-ekranlar.md). Yeni ekran = bu listeye bir satır.
/// Kod biçimi: &lt;alan&gt;.&lt;ekran&gt;; alanlar rol şablonlarında kullanılır.
/// </summary>
internal sealed class ScreenCatalog : IScreenCatalog
{
    public IReadOnlyList<ScreenDefinition> All { get; } =
    [
        // Çekirdek / Yönetim
        new("admin.users", Product.Core, ["create", "deactivate"]),
        new("admin.roles", Product.Core, ["operate"]),
        new("admin.permission_history", Product.Core, []),
        new("admin.audit", Product.Core, []),
        new("admin.settings", Product.Core, ["exchange_rate"]),
        new("admin.licenses", Product.Core, []),
        new("admin.integrations", Product.Core, ["reprocess"]),

        // Oyuncu
        new("player.search", Product.SPlus, []),
        new("player.locator", Product.SPlus, []),
        new("player.profile", Product.SPlus, ["create", "edit", "issue_card", "cancel_card"]),
        new("player.history", Product.SPlus, []),
        new("player.restrictions", Product.SPlus, ["add", "remove"]),

        // Canlı oyun
        new("live.tables", Product.SPlus, ["define"]),
        new("live.table_session", Product.SPlus, ["open", "close"]),
        new("live.rating", Product.SPlus, ["start", "finish", "correct"]),
        new("live.chips", Product.SPlus, ["request", "approve"]),
        new("live.drop", Product.SPlus, ["enter", "approve"]),

        // Slot (ekranlar S Plus'ındır; veri kaynağı NEXIO veya S Slot)
        new("slot.floor", Product.SPlus, []),
        new("slot.machine", Product.SPlus, []),
        new("slot.jackpot", Product.SPlus, ["record", "approve"]),
        new("slot.manual_entry", Product.SPlus, ["enter", "import"]),
        new("slot.machines_setup", Product.SPlus, ["define"]),

        // Kasa
        new("cage.shift", Product.SPlus, ["open", "close", "count"]),
        new("cage.cash", Product.SPlus, ["cash_in", "cash_out"]),
        new("cage.chips", Product.SPlus, ["sell", "redeem"]),
        new("cage.exchange", Product.SPlus, ["exchange"]),
        new("cage.adjustment", Product.SPlus, ["request", "approve"]),
        new("cage.summary", Product.SPlus, []),

        // Loyalty / Host
        new("loyalty.points", Product.SPlus, ["adjust"]),
        new("loyalty.comp", Product.SPlus, ["request", "approve"]),
        new("loyalty.host_portfolio", Product.SPlus, []),
        new("loyalty.rules", Product.SPlus, ["define"]),

        // Raporlar
        new("report.daily", Product.SPlus, []),
        new("report.live", Product.SPlus, []),
        new("report.slot", Product.SPlus, []),
        new("report.cage", Product.SPlus, []),
        new("report.player", Product.SPlus, []),
        new("report.official", Product.SPlus, []),
    ];
}

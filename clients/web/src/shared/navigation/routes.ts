import type { IconName } from "@/shared/ui/Icon";

/**
 * Web panelindeki sayfalar ve bağlı oldukları ekran kodları. Menü, yalnızca kullanıcının menüsünde
 * (GET /identity/me/menu) bulunan ve burada web sayfası olan ekranları gösterir.
 */
export type NavGroup = "players" | "reports" | "admin";

/** Yan menüdeki grup sırası (Spec 013 KK4); kısayol sırası NAV_ENTRIES sırasıdır ve bununla uyumludur. */
export const NAV_GROUPS: readonly NavGroup[] = ["players", "reports", "admin"];

export interface NavEntry {
  readonly screen: string;
  readonly href: string;
  readonly group: NavGroup;
  readonly icon: IconName;
}

export const NAV_ENTRIES: readonly NavEntry[] = [
  { screen: "player.search", href: "/oyuncular", group: "players", icon: "person" },
  { screen: "report.daily", href: "/raporlar/gunluk", group: "reports", icon: "report" },
  { screen: "admin.roles", href: "/yetki/roller", group: "admin", icon: "roles" },
  { screen: "admin.users", href: "/yetki/kullanicilar", group: "admin", icon: "people" },
  { screen: "admin.permission_history", href: "/yetki/gecmis", group: "admin", icon: "history" },
  { screen: "admin.settings", href: "/kur", group: "admin", icon: "rates" },
  { screen: "admin.audit", href: "/denetim", group: "admin", icon: "audit" },
  { screen: "admin.integrations", href: "/entegrasyon", group: "admin", icon: "integration" },
];

export const routes = {
  home: "/",
  roles: "/yetki/roller",
  role: (roleId: string) => `/yetki/roller/${encodeURIComponent(roleId)}`,
  users: "/yetki/kullanicilar",
  userAccess: (userId: string) => `/yetki/kullanicilar/${encodeURIComponent(userId)}`,
  permissionHistory: "/yetki/gecmis",
  players: "/oyuncular",
  player: (playerId: string) => `/oyuncular/${encodeURIComponent(playerId)}`,
  exchangeRates: "/kur",
  dailyReport: "/raporlar/gunluk",
  audit: "/denetim",
  integration: "/entegrasyon",
} as const;

export function visibleNav(canView: (screen: string) => boolean): NavEntry[] {
  return NAV_ENTRIES.filter((entry) => canView(entry.screen));
}

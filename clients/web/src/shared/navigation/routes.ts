/**
 * Web panelindeki sayfalar ve bağlı oldukları ekran kodları. Menü, yalnızca kullanıcının menüsünde
 * (GET /identity/me/menu) bulunan ve burada web sayfası olan ekranları gösterir.
 */
export interface NavEntry {
  readonly screen: string;
  readonly href: string;
  readonly group: "admin" | "players" | "reports";
}

export const NAV_ENTRIES: readonly NavEntry[] = [
  { screen: "player.search", href: "/oyuncular", group: "players" },
  { screen: "report.daily", href: "/raporlar/gunluk", group: "reports" },
  { screen: "admin.roles", href: "/yetki/roller", group: "admin" },
  { screen: "admin.users", href: "/yetki/kullanicilar", group: "admin" },
  { screen: "admin.permission_history", href: "/yetki/gecmis", group: "admin" },
  { screen: "admin.settings", href: "/kur", group: "admin" },
  { screen: "admin.audit", href: "/denetim", group: "admin" },
  { screen: "admin.integrations", href: "/entegrasyon", group: "admin" },
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

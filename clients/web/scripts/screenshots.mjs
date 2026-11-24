/**
 * Web ekranlarının koyu ve açık temada ekran görüntülerini alır (Spec 013 KK12).
 * Yalnızca SENTETİK veri kullanılır: API ve oturum Playwright içinde taklit edilir, gerçek sunucuya gidilmez.
 *
 * Ön koşul: uygulama şu ortamla derlenip çalışıyor olmalı:
 *   NEXT_PUBLIC_OIDC_AUTHORITY=http://127.0.0.1:9/realms/sgt NEXT_PUBLIC_OIDC_CLIENT_ID=sgt-web \
 *   NEXT_PUBLIC_DEFAULT_TERMINAL=KASA-01 npm run build && npx next start -p 3100
 * Kullanım: node scripts/screenshots.mjs [http://localhost:3100] [çıktı klasörü]
 * Ortam: SGT_SCREENS=ad1,ad2 (yalnızca bu ekranlar), SGT_VIEWPORT=390x844 (varsayılan 1440x900).
 * Playwright projeye bağımlılık olarak eklenmez; ortamda kurulu olmalıdır (require('playwright')).
 */
import { mkdirSync } from "node:fs";
import { createRequire } from "node:module";
import { join } from "node:path";
import { fileURLToPath } from "node:url";

const require = createRequire(import.meta.url);
const { chromium } = require("playwright");

const BASE = process.argv[2] ?? "http://localhost:3100";
const OUT = process.argv[3] ?? fileURLToPath(new URL("../../../docs/ekran-goruntuleri/web/", import.meta.url));
const AUTHORITY = "http://127.0.0.1:9/realms/sgt";
const CLIENT_ID = "sgt-web";

const ALL_ACTIONS = ["view", "operate", "create", "edit", "deactivate", "exchange_rate", "reprocess", "issue_card", "cancel_card", "add", "remove"];
const MENU = [
  "player.search",
  "player.profile",
  "player.restrictions",
  "report.daily",
  "admin.roles",
  "admin.users",
  "admin.permission_history",
  "admin.settings",
  "admin.audit",
  "admin.integrations",
].map((screen) => ({ screen, actions: ALL_ACTIONS }));

const ROLE_ID = "0b8f8a4e-1c11-4b4e-9a51-000000000001";
const USER_ID = "7d0c2f6a-5e2b-4c7d-8a10-000000000101";
const PLAYER_ID = "3a3e1b52-8f0e-4f7a-b0f4-000000000201";
const T = (h, m) => `2026-11-21T${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}:00Z`;
const cid = (n) => `c0ffee00-0000-4000-8000-${String(n).padStart(12, "0")}`;

const roles = [
  ["Kasa şefi", 14],
  ["Kasiyer", 9],
  ["Pit boss", 12],
  ["Slot müdürü", 11],
  ["Slot teknisyeni", 5],
  ["Host", 7],
  ["Resepsiyon", 4],
  ["Denetçi (salt okunur)", 10],
  ["Sistem yöneticisi", 22],
].map(([name, count], i) => ({
  id: i === 0 ? ROLE_ID : `0b8f8a4e-1c11-4b4e-9a51-${String(i + 1).padStart(12, "0")}`,
  name,
  permissions: Array.from({ length: count }, (_, j) => `x.screen${j}:view`),
}));

const screens = [
  ["player.search", "SPlus", ["view"]],
  ["player.profile", "SPlus", ["view", "create", "edit", "issue_card", "cancel_card"]],
  ["player.restrictions", "SPlus", ["view", "add", "remove"]],
  ["cage.shift", "SPlus", ["view", "open", "close", "count"]],
  ["cage.chips", "SPlus", ["view", "sell", "redeem"]],
  ["cage.exchange", "SPlus", ["view", "exchange"]],
  ["live.rating", "SPlus", ["view", "start", "finish"]],
  ["slot.floor", "SSlot", ["view"]],
  ["slot.jackpot", "SSlot", ["view", "record", "approve"]],
  ["report.daily", "SPlus", ["view"]],
  ["admin.roles", "Core", ["view", "operate"]],
  ["admin.audit", "Core", ["view"]],
].map(([code, product, actions]) => ({ code, product, actions }));
const kasaSefi = new Set([
  "player.search:view",
  "player.profile:view",
  "cage.shift:view",
  "cage.shift:open",
  "cage.shift:close",
  "cage.shift:count",
  "cage.chips:view",
  "cage.chips:sell",
  "cage.chips:redeem",
  "cage.exchange:view",
  "cage.exchange:exchange",
  "report.daily:view",
  "slot.floor:view",
  "admin.audit:view",
]);
roles[0].permissions = [...kasaSefi];

const users = [
  ["Ayşe Demir", "P-1001", true, true],
  ["Mehmet Kaya", "P-1002", true, false],
  ["Elif Yıldız", "P-1003", true, false],
  ["Can Öztürk", "P-1004", false, false],
  ["Zeynep Arslan", "P-1005", true, false],
  ["Burak Şahin", "P-1006", true, false],
  ["Deniz Aydın", "P-1007", true, false],
  ["Selin Koç", "P-1008", false, false],
].map(([displayName, employeeCode, isActive, isSuperAdmin], i) => ({
  userId: i === 1 ? USER_ID : `7d0c2f6a-5e2b-4c7d-8a10-${String(i + 100).padStart(12, "0")}`,
  displayName,
  employeeCode,
  isActive,
  isSuperAdmin,
}));

const players = [
  ["P000123", "Ali", "Yılmaz", "C-884120", false],
  ["P000124", "Merve", "Çelik", "C-884133", false],
  ["P000131", "Hasan", "Demirtaş", null, true],
  ["P000142", "Gizem", "Aksoy", "C-884201", false],
  ["P000150", "Oğuz", "Kılıç", "C-884215", false],
  ["P000158", "Nihan", "Erdem", null, false],
  ["P000163", "Kerem", "Yavuz", "C-884260", true],
  ["P000171", "Derya", "Polat", "C-884288", false],
  ["P000177", "Tolga", "Güneş", "C-884301", false],
  ["P000180", "Ebru", "Taş", "C-884322", false],
].map(([playerNumber, firstName, lastName, activeCardNumber, isRestricted], i) => ({
  playerId: i === 2 ? PLAYER_ID : `3a3e1b52-8f0e-4f7a-b0f4-${String(300 + i).padStart(12, "0")}`,
  playerNumber,
  firstName,
  lastName,
  activeCardNumber,
  isRestricted,
}));

const playerView = {
  playerId: PLAYER_ID,
  playerNumber: "P000131",
  firstName: "Hasan",
  lastName: "Demirtaş",
  birthDate: "1979-04-12",
  nationality: "TR",
  documentType: "Passport",
  maskedDocumentNumber: "U•••••482",
  phone: "+90 5•• ••• 12 40",
  activeCardNumber: "C-884140",
  isRestricted: true,
  activeRestrictions: [
    { id: "r-1", type: "Banned", note: "Masada personele saygısızlık; müdür kararı", until: "2027-05-21T00:00:00Z", addedAt: T(9, 14) },
    { id: "r-2", type: "SelfExcluded", note: "Oyuncunun kendi talebi (6 ay)", until: null, addedAt: "2026-10-02T16:40:00Z" },
  ],
};

const history = Array.from({ length: 18 }, (_, i) => ({
  id: `h-${i}`,
  target: i % 3 === 0 ? "User" : "Role",
  targetId: i % 3 === 0 ? USER_ID.slice(0, 8) : ROLE_ID.slice(0, 8),
  kind: ["RolePermissionGranted", "RolePermissionRevoked", "UserRoleAssigned", "UserPermissionDenied"][i % 4],
  item: ["cage.chips:sell", "admin.audit:view", "Kasa şefi", "report.daily:view", "slot.jackpot:approve"][i % 5],
  actorId: users[0].userId.slice(0, 8),
  terminal: ["YONETIM-01", "KASA-01", "KASA-02"][i % 3],
  reasonCode: ["ROLE_SETUP", "CORRECTION", "NEW_STAFF", "MANAGER_REQUEST"][i % 4],
  correlationId: cid(i + 1).slice(0, 18),
  occurredAt: T(18 - Math.floor(i / 2), 50 - i * 2),
}));

const audit = Array.from({ length: 24 }, (_, i) => {
  const kinds = [
    ["cage.ShiftOpened", "Kasiyer vardiyası açıldı: KASA-01, açılış 250.000,00 TRY"],
    ["cage.ChipsSold", "Çip satışı: 12.500,00 TRY, oyuncu P000124"],
    ["cage.ForeignBought", "Döviz alışı: 1.000,00 EUR @ 37,4200"],
    ["player.Registered", "Oyuncu kaydı: P000180"],
    ["player.RestrictionAdded", "Kısıtlama eklendi: P000131 (Yasaklı)"],
    ["identity.RolePermissionGranted", "Rol yetkisi verildi: Kasa şefi, cage.chips:sell"],
    ["configuration.ExchangeRateSet", "Kur girildi: GBP alış 44,1000 / satış 44,9000"],
    ["live.RatingFinished", "Rating bitti: Masa RL-02, P000150, ort. bahis 500,00 TRY"],
  ];
  const [eventType, summary] = kinds[i % kinds.length];
  return {
    id: `a-${i}`,
    eventType,
    summary,
    actorId: users[i % 4].userId.slice(0, 8),
    terminal: ["KASA-01", "KASA-02", "PIT-01", "YONETIM-01"][i % 4],
    reasonCode: ["SHIFT_START", "CUSTOMER_REQUEST", "DAILY_RATE", "CORRECTION"][i % 4],
    correlationId: cid(100 + i).slice(0, 18),
    occurredAt: T(19 - Math.floor(i / 3), 55 - (i % 3) * 17),
  };
});

const daily = {
  date: "2026-11-21",
  from: "2026-11-20T22:00:00Z",
  to: "2026-11-21T22:00:00Z",
  cage: {
    shiftsOpen: 3,
    shiftsClosed: 5,
    reversals: 2,
    currencies: [
      { currency: "TRY", chipSales: "1842500.00", chipRedemptions: "1610250.00", foreignBought: "0", foreignSold: "0", closingVariance: "-350.00" },
      { currency: "EUR", chipSales: "48200.00", chipRedemptions: "51900.00", foreignBought: "12400.00", foreignSold: "3100.00", closingVariance: "0" },
      { currency: "GBP", chipSales: "21000.00", chipRedemptions: "16450.00", foreignBought: "5200.00", foreignSold: "800.00", closingVariance: "20.00" },
      { currency: "USD", chipSales: "15300.00", chipRedemptions: "17800.00", foreignBought: "6900.00", foreignSold: "1200.00", closingVariance: "0" },
    ],
  },
  floor: {
    ratingsFinished: 184,
    live: [
      { currency: "TRY", theoreticalWin: "214380.50", casinoResult: "268900.00" },
      { currency: "EUR", theoreticalWin: "6120.00", casinoResult: "-3700.00" },
      { currency: "GBP", theoreticalWin: "2410.00", casinoResult: "4550.00" },
    ],
    slots: [
      { currency: "TRY", coinIn: "3940250.00", machineWin: "286115.75" },
      { currency: "EUR", coinIn: "84200.00", machineWin: "-1240.00" },
    ],
  },
  loyalty: { pointsEarned: 92840, pointsRedeemed: 31200 },
};

const rates = [
  { currency: "EUR", buy: "37.4200", sell: "38.1500", effectiveFrom: T(7, 30) },
  { currency: "GBP", buy: "44.1000", sell: "44.9000", effectiveFrom: T(7, 30) },
  { currency: "USD", buy: "34.2800", sell: "34.9600", effectiveFrom: T(7, 31) },
];

const deadLetters = [
  {
    module: "Loyalty",
    eventId: "e1000000-0000-4000-8000-000000000001",
    type: "RatingFinishedV1",
    attempts: 10,
    lastError: "Npgsql.PostgresException: 23505 duplicate key value violates unique constraint \"pk_point_entries\"",
    occurredAt: T(14, 22),
  },
  {
    module: "Reporting",
    eventId: "e1000000-0000-4000-8000-000000000002",
    type: "SlotMetersReportedV1",
    attempts: 10,
    lastError: "TimeoutException: inbox kilidi 30 sn içinde alınamadı",
    occurredAt: T(11, 5),
  },
  {
    module: "Finance",
    eventId: "e1000000-0000-4000-8000-000000000003",
    type: "PlayerRestrictedV1",
    attempts: 10,
    lastError: null,
    occurredAt: T(9, 48),
  },
];

/** Yol (sorgu hariç) → yanıt gövdesi. */
function respond(method, path) {
  if (method !== "GET") return { status: 204 };
  const routes = {
    "/api/v1/identity/me/menu": MENU,
    "/api/v1/identity/roles": roles,
    "/api/v1/identity/screens": screens,
    "/api/v1/identity/users": users,
    [`/api/v1/identity/users/${USER_ID}/access`]: {
      userId: USER_ID,
      isSuperAdmin: false,
      roleIds: [roles[1].id, roles[7].id],
      grants: ["cage.adjustment:view", "report.cage:view"],
      denied: ["cage.chips:redeem"],
      effective: [...kasaSefi].filter((p) => p !== "cage.chips:redeem").concat(["cage.adjustment:view", "report.cage:view"]).sort(),
    },
    "/api/v1/identity/permission-changes": history,
    "/api/v1/players": players,
    [`/api/v1/players/${PLAYER_ID}`]: playerView,
    "/api/v1/configuration/exchange-rates/current": rates,
    "/api/v1/reports/daily": daily,
    "/api/v1/audit/entries": audit,
    "/api/v1/integration/dead-letters": deadLetters,
  };
  return path in routes ? { status: 200, body: routes[path] } : { status: 404 };
}

const SCREENS = [
  { name: "giris", path: "/", anonymous: true },
  { name: "ana-sayfa", path: "/" },
  {
    name: "oyuncu-arama",
    path: "/oyuncular",
    action: async (page) => {
      await page.getByRole("searchbox").fill("de");
      await page.getByRole("searchbox").press("Enter");
      await page.getByText("P000131").waitFor();
    },
  },
  { name: "oyuncu-kayit-diyalog", path: "/oyuncular", action: async (page) => page.getByRole("button", { name: /Yeni oyuncu/ }).click() },
  { name: "oyuncu-karti", path: `/oyuncular/${PLAYER_ID}`, wait: "Hasan" },
  { name: "roller", path: "/yetki/roller", wait: "Kasa şefi" },
  { name: "rol-yetkileri", path: `/yetki/roller/${ROLE_ID}`, wait: "cage.chips" },
  { name: "kullanicilar", path: "/yetki/kullanicilar", wait: "Mehmet Kaya" },
  { name: "kullanici-yetkileri", path: `/yetki/kullanicilar/${USER_ID}`, wait: "Kasiyer" },
  { name: "yetki-gecmisi", path: "/yetki/gecmis", wait: "ROLE_SETUP" },
  { name: "kur", path: "/kur", wait: "GBP" },
  { name: "gunluk-ozet", path: "/raporlar/gunluk", wait: "Coin in" },
  { name: "denetim", path: "/denetim", wait: "cage.ShiftOpened" },
  { name: "entegrasyon", path: "/entegrasyon", wait: "RatingFinishedV1" },
];

async function shoot(browser, screen, theme) {
  const [width, height] = (process.env.SGT_VIEWPORT ?? "1440x900").split("x").map(Number);
  const context = await browser.newContext({ viewport: { width, height }, deviceScaleFactor: 2, locale: "tr-TR", timezoneId: "Asia/Famagusta" });
  await context.addInitScript(
    ({ theme, anonymous, key, user }) => {
      window.localStorage.setItem("sgt.theme", theme);
      window.localStorage.setItem("sgt.locale", "tr");
      window.localStorage.setItem("sgt.terminal", "KASA-01");
      if (!anonymous) window.sessionStorage.setItem(key, JSON.stringify(user));
    },
    {
      theme,
      anonymous: Boolean(screen.anonymous),
      key: `oidc.user:${AUTHORITY}:${CLIENT_ID}`,
      user: {
        id_token: "synthetic",
        access_token: "synthetic-token",
        token_type: "Bearer",
        scope: "openid profile",
        profile: { sub: users[0].userId, name: "Ayşe Demir" },
        expires_at: Math.floor(Date.now() / 1000) + 86_400,
      },
    },
  );
  await context.route("**/api/v1/**", async (route) => {
    const url = new URL(route.request().url());
    const { status, body } = respond(route.request().method(), decodeURIComponent(url.pathname));
    await route.fulfill({ status, contentType: "application/json", body: body === undefined ? "" : JSON.stringify(body) });
  });
  await context.route("http://127.0.0.1:9/**", (route) => route.abort());
  const page = await context.newPage();
  await page.goto(`${BASE}${screen.path}`, { waitUntil: "networkidle" });
  if (screen.wait) await page.getByText(screen.wait, { exact: false }).first().waitFor();
  if (screen.action) await screen.action(page);
  await page.evaluate(() => document.fonts.ready);
  await page.waitForTimeout(150);
  const file = join(OUT, `${screen.name}-${theme}.png`);
  await page.screenshot({ path: file });
  await context.close();
  return file;
}

mkdirSync(OUT, { recursive: true });
const browser = await chromium.launch();
try {
  const only = process.env.SGT_SCREENS?.split(",");
  for (const screen of SCREENS.filter((s) => !only || only.includes(s.name))) {
    for (const theme of ["dark", "light"]) console.warn(await shoot(browser, screen, theme));
  }
} finally {
  await browser.close();
}

/**
 * Kullanılan Microsoft Fluent UI System Icons (MIT, @fluentui/svg-icons) çizimlerini tek TypeScript modülüne
 * çıkarır (Spec 013). Simgeler satır içi SVG olarak currentColor ile çizilir; ek yükleyici/çalışma zamanı gerekmez.
 *
 * Kullanım: npm run gen:icons            (src/shared/ui/icons.generated.ts yazar)
 *           npm run gen:icons -- --check (fark varsa çıkış 1)
 * Yeni simge: ICONS'a "ad: fluent_dosya_adı" ekleyip komutu çalıştırın. Windows ve mobil aynı Fluent adlarını kullanır.
 */
import { readFileSync, writeFileSync } from "node:fs";
import { createRequire } from "node:module";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

/** Uygulama adı → Fluent simge dosyası (20px, regular). */
export const ICONS = {
  add: "add_20_regular",
  audit: "clipboard_task_list_ltr_20_regular",
  back: "arrow_left_20_regular",
  card: "contact_card_20_regular",
  close: "dismiss_20_regular",
  dark: "weather_moon_20_regular",
  error: "error_circle_20_regular",
  filter: "filter_20_regular",
  history: "history_20_regular",
  home: "home_20_regular",
  integration: "plug_disconnected_20_regular",
  light: "weather_sunny_20_regular",
  people: "people_20_regular",
  person: "person_20_regular",
  personAdd: "person_add_20_regular",
  rates: "currency_dollar_euro_20_regular",
  refresh: "arrow_clockwise_20_regular",
  report: "data_bar_vertical_20_regular",
  reprocess: "arrow_repeat_all_20_regular",
  restricted: "person_prohibited_20_regular",
  roles: "shield_keyhole_20_regular",
  search: "search_20_regular",
  signOut: "sign_out_20_regular",
  terminal: "desktop_20_regular",
  warning: "warning_20_regular",
};

const require = createRequire(import.meta.url);
const iconDir = join(dirname(require.resolve("@fluentui/svg-icons/package.json")), "icons");
const target = fileURLToPath(new URL("../src/shared/ui/icons.generated.ts", import.meta.url));

export function renderIcons() {
  const version = JSON.parse(readFileSync(join(iconDir, "../package.json"), "utf8")).version;
  const entries = Object.entries(ICONS).map(([name, file]) => {
    const svg = readFileSync(join(iconDir, `${file}.svg`), "utf8");
    if (!/viewBox="0 0 20 20"/.test(svg)) throw new Error(`${file}: beklenmeyen viewBox`);
    const paths = [...svg.matchAll(/<path[^>]*\sd="([^"]+)"/g)].map((m) => m[1]);
    if (paths.length === 0) throw new Error(`${file}: path yok`);
    return `  ${name}: ${JSON.stringify(paths)},`;
  });
  return [
    `// BU DOSYA ÜRETİLİR, ELLE DEĞİŞTİRMEYİN. Kaynak: @fluentui/svg-icons ${version} (MIT), npm run gen:icons.`,
    "/** Fluent UI System Icons, 20×20 regular; her simge bir veya daha çok path. */",
    "export const ICON_PATHS = {",
    ...entries,
    "} as const satisfies Record<string, readonly string[]>;",
    "",
    "export type IconName = keyof typeof ICON_PATHS;",
    "",
  ].join("\n");
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  const fresh = renderIcons();
  let current = null;
  try {
    current = readFileSync(target, "utf8");
  } catch {
    // dosya yok
  }
  if (process.argv.includes("--check")) {
    if (fresh !== current) {
      console.error("icons.generated.ts güncel değil: npm run gen:icons");
      process.exitCode = 1;
    }
  } else if (fresh !== current) {
    writeFileSync(target, fresh);
    console.warn(`Üretildi: ${target}`);
  }
}

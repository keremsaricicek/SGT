/**
 * Tasarım token'larını style-dictionary ile üretir (Spec 013 KK1–KK2).
 *
 *   Kaynak:  design/tokens/sgt-tokens.json (DTCG)
 *   Web:     clients/web/src/app/tokens.css            (CSS değişkenleri; koyu varsayılan + [data-theme])
 *   Windows: design/tokens/build/xaml/SgtTokens.xaml    (ölçüler, yazı aileleri)
 *            design/tokens/build/xaml/SgtTheme.Dark.xaml, SgtTheme.Light.xaml (aynı anahtarlı renk sözlükleri)
 *   Mobil:   design/tokens/build/dart/sgt_tokens.dart   (SgtPalette.dark/light + SgtMetrics)
 *
 * Kullanım: npm run gen:tokens          dosyaları yazar
 *           npm run gen:tokens -- --check  geçici klasöre üretir, depodaki dosyalarla karşılaştırır (fark = çıkış 1)
 */
import { copyFileSync, mkdirSync, mkdtempSync, readFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import StyleDictionary from "style-dictionary";
import { contrastFailures } from "./contrast.mjs";

const repo = fileURLToPath(new URL("../../../", import.meta.url));
const SOURCE = join(repo, "design/tokens/sgt-tokens.json");
const CONTRAST = join(repo, "design/tokens/sgt-contrast.json");
const DEFAULT_THEME = "dark";
const THEMES = ["dark", "light"];

/** Depodaki hedefler: [üretilen ad, depo yolu]. */
export const OUTPUTS = [
  ["web/tokens.css", "clients/web/src/app/tokens.css"],
  ["xaml/SgtTokens.xaml", "design/tokens/build/xaml/SgtTokens.xaml"],
  ["xaml/SgtTheme.Dark.xaml", "design/tokens/build/xaml/SgtTheme.Dark.xaml"],
  ["xaml/SgtTheme.Light.xaml", "design/tokens/build/xaml/SgtTheme.Light.xaml"],
  ["dart/sgt_tokens.dart", "design/tokens/build/dart/sgt_tokens.dart"],
];

const HEADER = "BU DOSYA ÜRETİLİR, ELLE DEĞİŞTİRMEYİN. Kaynak: design/tokens/sgt-tokens.json (npm run gen:tokens, Spec 013).";

const kebab = (s) =>
  String(s)
    .replace(/([a-z0-9])([A-Z])/g, "$1-$2")
    .toLowerCase();
const pascal = (s) => String(s).replace(/(^|[-_ ])(\w)/g, (_, __, c) => c.toUpperCase()).replace(/^(\w)/, (c) => c.toUpperCase());
const camel = (s) => pascal(s).replace(/^(\w)/, (c) => c.toLowerCase());

/** CSS adı: renkler tema öneki olmadan (--panel-head), ölçüler kısa önekli (--text-body, --space-4, --grid-standard-row-height). */
function cssName(path) {
  const [root, a, b] = path;
  switch (root) {
    case "color":
      return kebab(b);
    case "font": {
      const prefix = { family: "font", size: "text", weight: "weight", lineHeight: "leading", letterSpacing: "tracking" }[a];
      return `${prefix}-${kebab(b)}`;
    }
    case "control":
      return a === "height" ? `control-${kebab(b)}` : `control-${kebab(a)}`;
    case "layout":
      return kebab(a);
    default:
      return path.map(kebab).join("-");
  }
}

function cssValue(token) {
  const value = token.$value;
  switch (token.$type) {
    case "dimension":
      return value === 0 ? "0" : `${value}px`;
    case "fontFamily":
      return value.map((f) => (/^(system-ui|sans-serif|serif|monospace)$/.test(f) ? f : `"${f}"`)).join(", ");
    default:
      return String(value);
  }
}

/** Platform bağımsız ad (XAML anahtarı / Dart alanı): yol parçaları birleşik, tema hariç. */
function symbol(path) {
  return path[0] === "color" ? pascal(path[2]) : path.map(pascal).join("");
}

const byTheme = (tokens, theme) => tokens.filter((t) => t.path[0] === "color" && t.path[1] === theme);
const metrics = (tokens) => tokens.filter((t) => t.path[0] !== "color");

function argb(hex) {
  return `FF${hex.slice(1).toUpperCase()}`;
}

function cssFormat({ dictionary }) {
  const line = (t) => `  --${cssName(t.path)}: ${cssValue(t)};`;
  const out = [`/* ${HEADER} */`, "", ":root {", ...metrics(dictionary.allTokens).map(line), "}", ""];
  for (const theme of THEMES) {
    const selector = theme === DEFAULT_THEME ? `:root,\n[data-theme="${theme}"]` : `[data-theme="${theme}"]`;
    out.push(`${selector} {`, ...byTheme(dictionary.allTokens, theme).map(line), `  color-scheme: ${theme};`, "}", "");
  }
  return out.join("\n");
}

function xamlDocument(body) {
  return [
    `<!-- ${HEADER} -->`,
    '<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"',
    '                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"',
    '                    xmlns:sys="clr-namespace:System;assembly=System.Runtime">',
    ...body,
    "</ResourceDictionary>",
    "",
  ].join("\n");
}

function xamlThemeFormat(theme) {
  return ({ dictionary }) =>
    xamlDocument(
      byTheme(dictionary.allTokens, theme).flatMap((t) => [
        `  <Color x:Key="Sgt${symbol(t.path)}Color">#${argb(t.$value)}</Color>`,
        `  <SolidColorBrush x:Key="Sgt${symbol(t.path)}Brush" Color="{StaticResource Sgt${symbol(t.path)}Color}" />`,
      ]),
    );
}

function xamlMetricsFormat({ dictionary }) {
  return xamlDocument(
    metrics(dictionary.allTokens).map((t) => {
      const key = `Sgt${symbol(t.path)}`;
      if (t.$type === "fontFamily") return `  <FontFamily x:Key="${key}">${t.$value.join(", ")}</FontFamily>`;
      return `  <sys:Double x:Key="${key}">${t.$value}</sys:Double>`;
    }),
  );
}

function dartFormat({ dictionary }) {
  const dark = byTheme(dictionary.allTokens, "dark");
  const names = dark.map((t) => camel(t.path[2]));
  const palette = (theme) =>
    byTheme(dictionary.allTokens, theme).map((t) => `    ${camel(t.path[2])}: Color(0x${argb(t.$value)}),`);
  const metric = (t) => {
    const name = camel(symbol(t.path));
    if (t.$type === "fontFamily") return `  static const List<String> ${name} = [${t.$value.map((f) => `'${f}'`).join(", ")}];`;
    return `  static const double ${name} = ${Number(t.$value).toFixed(Number.isInteger(t.$value) ? 1 : 2)};`;
  };
  return [
    `// ${HEADER}`,
    "// ignore_for_file: public_member_api_docs",
    "import 'dart:ui';",
    "",
    "/// Tema renkleri; iki temada aynı alanlar bulunur.",
    "class SgtPalette {",
    `  const SgtPalette({${names.map((n) => `required this.${n}`).join(", ")}});`,
    ...names.map((n) => `  final Color ${n};`),
    "",
    "  static const SgtPalette dark = SgtPalette(",
    ...palette("dark"),
    "  );",
    "",
    "  static const SgtPalette light = SgtPalette(",
    ...palette("light"),
    "  );",
    "}",
    "",
    "/// Tema bağımsız ölçüler (mantıksal piksel) ve yazı aileleri.",
    "abstract final class SgtMetrics {",
    ...metrics(dictionary.allTokens).map(metric),
    "}",
    "",
  ].join("\n");
}

/** Token'ları üretir; buildRoot altında OUTPUTS'taki üretilen adlarla dosya yazar. */
export async function buildTokens(buildRoot) {
  const failures = contrastFailures(JSON.parse(readFileSync(SOURCE, "utf8")), JSON.parse(readFileSync(CONTRAST, "utf8")));
  if (failures.length > 0) {
    const lines = failures.map((f) => `[${f.theme}] ${f.fg} / ${f.bg}: ${f.ratio} < ${f.min}`);
    throw new Error(`Kontrast yetersiz (WCAG AA):\n${lines.join("\n")}`);
  }
  const sd = new StyleDictionary({
    source: [SOURCE],
    log: { verbosity: "silent" },
    hooks: {
      formats: {
        "sgt/css": cssFormat,
        "sgt/xaml-metrics": xamlMetricsFormat,
        "sgt/xaml-dark": xamlThemeFormat("dark"),
        "sgt/xaml-light": xamlThemeFormat("light"),
        "sgt/dart": dartFormat,
      },
    },
    platforms: {
      web: { transforms: [], buildPath: `${join(buildRoot, "web")}/`, files: [{ destination: "tokens.css", format: "sgt/css" }] },
      xaml: {
        transforms: [],
        buildPath: `${join(buildRoot, "xaml")}/`,
        files: [
          { destination: "SgtTokens.xaml", format: "sgt/xaml-metrics" },
          { destination: "SgtTheme.Dark.xaml", format: "sgt/xaml-dark" },
          { destination: "SgtTheme.Light.xaml", format: "sgt/xaml-light" },
        ],
      },
      dart: { transforms: [], buildPath: `${join(buildRoot, "dart")}/`, files: [{ destination: "sgt_tokens.dart", format: "sgt/dart" }] },
    },
  });
  await sd.buildAllPlatforms();
}

async function main() {
  const check = process.argv.includes("--check");
  const tmp = mkdtempSync(join(tmpdir(), "sgt-tokens-"));
  try {
    await buildTokens(tmp);
    let stale = 0;
    for (const [built, target] of OUTPUTS) {
      const fresh = readFileSync(join(tmp, built), "utf8");
      let current = null;
      try {
        current = readFileSync(join(repo, target), "utf8");
      } catch {
        // dosya yok
      }
      if (fresh === current) continue;
      if (check) {
        console.error(`Güncel değil: ${target}`);
        stale += 1;
      } else {
        mkdirSync(dirname(join(repo, target)), { recursive: true });
        copyFileSync(join(tmp, built), join(repo, target));
        console.warn(`Üretildi: ${target}`);
      }
    }
    if (stale > 0) {
      console.error("Token çıktıları kaynakla aynı değil: cd clients/web && npm run gen:tokens");
      process.exitCode = 1;
    }
  } finally {
    rmSync(tmp, { recursive: true, force: true });
  }
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  await main();
}

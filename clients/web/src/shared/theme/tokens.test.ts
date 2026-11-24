import { execFileSync } from "node:child_process";
import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import type { ContrastPairs } from "../../../scripts/contrast.mjs";
import { contrastFailures, contrastRatio } from "../../../scripts/contrast.mjs";

/** Testler clients/web kökünden çalışır (vitest.config.mts). */
const root = process.cwd();
const tokenDir = join(root, "../../design/tokens");
const tokens = JSON.parse(readFileSync(join(tokenDir, "sgt-tokens.json"), "utf8")) as {
  color: Record<string, Record<string, unknown>>;
  radius: Record<string, { $value?: number }>;
};
const pairs = JSON.parse(readFileSync(join(tokenDir, "sgt-contrast.json"), "utf8")) as ContrastPairs;

function cssFiles(dir: string): string[] {
  return readdirSync(dir).flatMap((name) => {
    const path = join(dir, name);
    if (statSync(path).isDirectory()) return cssFiles(path);
    return name.endsWith(".css") ? [path] : [];
  });
}

describe("tasarım token'ları (Spec 013)", () => {
  it("KK2: web, XAML ve Dart çıktıları token dosyasından üretilmiş hâliyle aynıdır", () => {
    expect(() => execFileSync(process.execPath, ["scripts/build-tokens.mjs", "--check"], { stdio: "pipe" })).not.toThrow();
  }, 30_000);

  it("KK1: iki temada da aynı renk anahtarları ve durum renkleri tanımlıdır", () => {
    const keys = (theme: string) => Object.keys(tokens.color[theme] ?? {}).sort();
    expect(keys("light")).toEqual(keys("dark"));
    for (const key of ["moneyIn", "moneyOut", "warning", "danger", "success", "restricted", "restrictedBg"]) {
      expect(keys("dark")).toContain(key);
    }
  });

  it("KK7: metin çiftleri >= 4.5:1, metin dışı işaretler >= 3:1 (iki tema)", () => {
    expect(contrastFailures(tokens, pairs)).toEqual([]);
  });

  it("kontrast hesabı WCAG örnekleriyle uyumludur", () => {
    expect(contrastRatio("#000000", "#ffffff")).toBeCloseTo(21, 5);
    expect(contrastRatio("#777777", "#ffffff")).toBeCloseTo(4.48, 2);
  });

  it("KK3: token'lardaki köşe yarıçapları en çok 3px", () => {
    const values = Object.entries(tokens.radius)
      .filter(([key]) => !key.startsWith("$"))
      .map(([, token]) => token.$value ?? 0);
    expect(values.length).toBeGreaterThan(0);
    for (const value of values) expect(value).toBeLessThanOrEqual(3);
  });

  it("KK3: biçim dosyalarında büyük yarıçap, gradyan ve animasyon yoktur", () => {
    const offences: string[] = [];
    for (const file of cssFiles(join(root, "src"))) {
      const css = readFileSync(file, "utf8");
      for (const match of css.matchAll(/border(?:-[a-z]+)*-radius\s*:\s*([^;]+);/g)) {
        const value = (match[1] ?? "").trim();
        const ok = /^(0|var\(--radius-[a-z-]+\)|[0-3]px)(\s+(0|var\(--radius-[a-z-]+\)|[0-3]px))*$/.test(value);
        if (!ok) offences.push(`${file}: border-radius ${value}`);
      }
      if (/gradient\(/.test(css)) offences.push(`${file}: gradient`);
      if (/@keyframes|animation\s*:|transition\s*:/.test(css)) offences.push(`${file}: animation/transition`);
      if (/backdrop-filter/.test(css)) offences.push(`${file}: backdrop-filter`);
    }
    expect(offences).toEqual([]);
  });
});

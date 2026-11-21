/**
 * WCAG 2.2 kontrast denetimi (Spec 013 KK7). design/tokens/sgt-contrast.json'daki çiftler, sgt-tokens.json'daki
 * color.<tema>.<ad> renkleriyle her tema için ayrı denetlenir: metin >= 4.5:1, metin dışı işaret >= 3:1.
 */

function channel(hex, offset) {
  const c = Number.parseInt(hex.slice(offset, offset + 2), 16) / 255;
  return c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
}

/** WCAG göreli parlaklık (#rrggbb). */
export function luminance(hex) {
  if (!/^#[0-9a-f]{6}$/i.test(hex)) throw new Error(`Geçersiz renk: ${hex}`);
  return 0.2126 * channel(hex, 1) + 0.7152 * channel(hex, 3) + 0.0722 * channel(hex, 5);
}

/** WCAG kontrast oranı (1–21). */
export function contrastRatio(a, b) {
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x);
  return (hi + 0.05) / (lo + 0.05);
}

/** Eşiği geçemeyen (ya da tanımsız renk içeren) çiftleri döner. */
export function contrastFailures(tokens, pairs) {
  const failures = [];
  const groups = [
    ["text", 4.5],
    ["nonText", 3],
  ];
  for (const theme of pairs.themes) {
    const colors = tokens.color[theme];
    for (const [group, min] of groups) {
      for (const [fg, bg] of pairs[group]) {
        const a = colors?.[fg]?.$value;
        const b = colors?.[bg]?.$value;
        if (!a || !b) {
          failures.push({ theme, fg, bg, ratio: 0, min, missing: true });
          continue;
        }
        const ratio = contrastRatio(a, b);
        if (ratio < min) failures.push({ theme, fg, bg, ratio: Math.round(ratio * 100) / 100, min });
      }
    }
  }
  return failures;
}

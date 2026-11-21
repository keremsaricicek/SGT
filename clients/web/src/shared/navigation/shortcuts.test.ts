import { describe, expect, it } from "vitest";
import { navShortcutIndex, shortcutLabel } from "./shortcuts";

const key = (code: string, mods: Partial<{ altKey: boolean; ctrlKey: boolean; metaKey: boolean; shiftKey: boolean }> = {}) => ({
  altKey: true,
  ctrlKey: false,
  metaKey: false,
  shiftKey: false,
  code,
  ...mods,
});

describe("menü kısayolları (Spec 013 KK10)", () => {
  it("Alt+1…9 sıradaki menü öğesini seçer", () => {
    expect(navShortcutIndex(key("Digit1"), 8)).toBe(0);
    expect(navShortcutIndex(key("Numpad3"), 8)).toBe(2);
    expect(navShortcutIndex(key("Digit8"), 8)).toBe(7);
  });

  it("menüde olmayan sıra, başka değiştirici tuş ya da Alt'sız basış kısayol değildir", () => {
    expect(navShortcutIndex(key("Digit9"), 8)).toBeNull();
    expect(navShortcutIndex(key("Digit1", { ctrlKey: true }), 8)).toBeNull();
    expect(navShortcutIndex(key("Digit1", { altKey: false }), 8)).toBeNull();
    expect(navShortcutIndex(key("Digit0"), 8)).toBeNull();
    expect(navShortcutIndex(key("KeyR"), 8)).toBeNull();
  });

  it("ilk dokuz öğe için etiket üretir", () => {
    expect(shortcutLabel(0)).toBe("Alt+1");
    expect(shortcutLabel(9)).toBeNull();
  });
});

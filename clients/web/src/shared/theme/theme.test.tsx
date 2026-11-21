import { act, renderHook } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { useTheme } from "./ThemeProvider";
import { DEFAULT_THEME, THEME_INIT_SCRIPT, THEME_STORAGE_KEY, readStoredTheme, storeTheme } from "./theme";

afterEach(() => {
  window.localStorage.clear();
  document.documentElement.removeAttribute("data-theme");
});

describe("tema (Spec 013 KK6)", () => {
  it("varsayılan tema koyudur; bilinmeyen değer koyuya düşer", () => {
    expect(DEFAULT_THEME).toBe("dark");
    expect(readStoredTheme({ getItem: () => null })).toBe("dark");
    expect(readStoredTheme({ getItem: () => "mavi" })).toBe("dark");
    expect(readStoredTheme({ getItem: () => "light" })).toBe("light");
  });

  it("depolama kapalıysa hata vermez", () => {
    const broken = {
      getItem: () => {
        throw new Error("kapalı");
      },
      setItem: () => {
        throw new Error("kapalı");
      },
    };
    expect(readStoredTheme(broken)).toBe("dark");
    expect(() => storeTheme(broken, "light")).not.toThrow();
  });

  it("ilk çizim betiği saklanan temayı <html> özniteliğine yazar", () => {
    window.localStorage.setItem(THEME_STORAGE_KEY, "light");
    new Function(THEME_INIT_SCRIPT)();
    expect(document.documentElement.getAttribute("data-theme")).toBe("light");
    window.localStorage.removeItem(THEME_STORAGE_KEY);
    new Function(THEME_INIT_SCRIPT)();
    expect(document.documentElement.getAttribute("data-theme")).toBe("dark");
  });

  it("tema değişimi anında uygulanır ve saklanır (yenilemeden sonra korunur)", () => {
    const { result } = renderHook(() => useTheme());
    expect(result.current.theme).toBe("dark");
    act(() => result.current.toggle());
    expect(result.current.theme).toBe("light");
    expect(document.documentElement.getAttribute("data-theme")).toBe("light");
    expect(window.localStorage.getItem(THEME_STORAGE_KEY)).toBe("light");
  });
});

import { describe, expect, it } from "vitest";
import { TERMINAL_STORAGE_KEY, createTerminalStore, normalizeTerminalName } from "./terminal";

describe("terminal adı", () => {
  it("kayıtlı ad varsa onu kullanır", () => {
    window.localStorage.setItem(TERMINAL_STORAGE_KEY, "KASA-01");
    expect(createTerminalStore("WEB-DEFAULT", window.localStorage).get()).toBe("KASA-01");
  });

  it("kayıt yoksa yapılandırmadaki adı kullanır ve saklar", () => {
    const store = createTerminalStore("WEB-OFIS", window.localStorage);
    expect(store.get()).toBe("WEB-OFIS");
    expect(window.localStorage.getItem(TERMINAL_STORAGE_KEY)).toBe("WEB-OFIS");
  });

  it("hiçbiri yoksa WEB- ile başlayan ad üretir", () => {
    expect(createTerminalStore("", window.localStorage).get()).toMatch(/^WEB-[0-9A-F]{6}$/);
  });

  it("yeniden adlandırma dinleyicilere haber verir ve adı temizler", () => {
    const store = createTerminalStore("WEB-A", window.localStorage);
    let calls = 0;
    store.subscribe(() => calls++);
    expect(store.set("  Ofis 2 ")).toBe("Ofis-2");
    expect(store.get()).toBe("Ofis-2");
    expect(calls).toBe(1);
  });

  it("başlığa uygun olmayan karakterleri temizler", () => {
    expect(normalizeTerminalName("a/b\nc")).toBe("a-b-c");
  });
});

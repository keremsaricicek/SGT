import { describe, expect, it } from "vitest";
import { en } from "./en";
import { translate } from "./I18nProvider";
import { actionLabel, screenLabel } from "./labels";
import { tr } from "./tr";

describe("i18n", () => {
  it("İngilizce sözlük Türkçedeki tüm anahtarları içerir", () => {
    expect(Object.keys(en).sort()).toEqual(Object.keys(tr).sort());
  });

  it("parametreleri yerleştirir", () => {
    expect(translate("tr", "permissions.role.title", { name: "Pit boss" })).toBe("Rol: Pit boss");
    expect(translate("en", "permissions.role.title", { name: "Pit boss" })).toBe("Role: Pit boss");
  });

  it("bilinmeyen ekran/eylem kodunu olduğu gibi gösterir", () => {
    const t = (key: Parameters<typeof translate>[1]) => translate("tr", key);
    expect(screenLabel(t, "admin.roles")).toBe("Roller ve yetki paneli");
    expect(screenLabel(t, "hotel.frontdesk")).toBe("hotel.frontdesk");
    expect(actionLabel(t, "approve")).toBe("Onayla");
    expect(actionLabel(t, "teleport")).toBe("teleport");
  });
});

import { describe, expect, it } from "vitest";
import { visibleNav } from "@/shared/navigation/routes";
import { fakeFetch, testApi } from "@/test/fakeApi";
import { AccessPolicy, fetchMyMenu } from "./menu";

describe("AccessPolicy", () => {
  const policy = new AccessPolicy([
    { screen: "admin.roles", actions: ["operate", "view"] },
    { screen: "player.search", actions: ["view"] },
  ]);

  it("yalnızca menüdeki ekranları görünür sayar", () => {
    expect(policy.canView("admin.roles")).toBe(true);
    expect(policy.canView("admin.audit")).toBe(false);
  });

  it("eylem düğmesi yalnızca eylem menüde varsa açılır", () => {
    expect(policy.can("admin.roles", "operate")).toBe(true);
    expect(policy.can("player.search", "operate")).toBe(false);
    expect(policy.can("admin.users", "create")).toBe(false);
  });

  it("yan menü yalnızca görülebilen web ekranlarını listeler", () => {
    const nav = visibleNav((s) => policy.canView(s)).map((e) => e.screen);
    expect(nav).toEqual(["player.search", "admin.roles"]);
  });
});

describe("fetchMyMenu", () => {
  it("GET /api/v1/identity/me/menu yanıtını döner", async () => {
    const menu = [{ screen: "report.daily", actions: ["view"] }];
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/identity/me/menu", body: menu }]);
    await expect(fetchMyMenu(testApi(fake.fetch))).resolves.toEqual(menu);
  });
});

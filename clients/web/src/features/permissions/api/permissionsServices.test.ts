import { describe, expect, it } from "vitest";
import { fakeFetch, testApi } from "@/test/fakeApi";
import { createPermissionHistoryService } from "./historyService";
import { createRolesService } from "./rolesService";
import { createUserAccessService, createUsersService } from "./usersService";

const ROLE_ID = "11111111-1111-4111-8111-111111111111";
const USER_ID = "22222222-2222-4222-8222-222222222222";

describe("RolesService", () => {
  it("rol yetkisini gerekçe koduyla verir (POST gövdesi)", async () => {
    const fake = fakeFetch([{ method: "POST", path: `/api/v1/identity/roles/${ROLE_ID}/permissions` }]);
    await createRolesService(testApi(fake.fetch)).grant(ROLE_ID, "admin.audit:view", "ROLE_SETUP");
    expect(fake.requests[0]?.body).toEqual({ permission: "admin.audit:view", reasonCode: "ROLE_SETUP" });
  });

  it("rol yetkisini geri alırken gerekçe kodunu sorguda gönderir", async () => {
    const path = `/api/v1/identity/roles/${ROLE_ID}/permissions/admin.audit:view`;
    const fake = fakeFetch([{ method: "DELETE", path }]);
    await createRolesService(testApi(fake.fetch)).revoke(ROLE_ID, "admin.audit:view", "CORRECTION");
    const request = fake.requests[0];
    expect(request?.method).toBe("DELETE");
    expect(decodeURIComponent(request?.url.pathname ?? "")).toBe(path);
    expect(request?.url.pathname).toContain("admin.audit%3Aview");
    expect(request?.url.searchParams.get("reasonCode")).toBe("CORRECTION");
  });

  it("rol oluşturur ve yeni kimliği döner", async () => {
    const fake = fakeFetch([{ method: "POST", path: "/api/v1/identity/roles", status: 201, body: { id: ROLE_ID } }]);
    const created = await createRolesService(testApi(fake.fetch)).create("Vardiya şefi", "ROLE_SETUP");
    expect(created.id).toBe(ROLE_ID);
    expect(fake.requests[0]?.body).toEqual({ name: "Vardiya şefi", reasonCode: "ROLE_SETUP" });
  });

  it("varsayılan rolleri gerekçe koduyla oluşturur", async () => {
    const fake = fakeFetch([{ method: "POST", path: "/api/v1/identity/roles/defaults" }]);
    await createRolesService(testApi(fake.fetch)).createDefaults("ROLE_SETUP");
    expect(fake.requests[0]?.body).toEqual({ reasonCode: "ROLE_SETUP" });
  });
});

describe("UsersService ve UserAccessService", () => {
  it("kullanıcı kaydeder", async () => {
    const fake = fakeFetch([{ method: "POST", path: "/api/v1/identity/users" }]);
    const input = { userId: USER_ID, displayName: "Test Kullanıcı", employeeCode: "T-001", reasonCode: "NEW_STAFF" };
    await createUsersService(testApi(fake.fetch)).register(input);
    expect(fake.requests[0]?.body).toEqual(input);
  });

  it("pasifleştirme ve aktifleştirme doğru uca gider", async () => {
    const fake = fakeFetch([
      { method: "POST", path: `/api/v1/identity/users/${USER_ID}/deactivate` },
      { method: "POST", path: `/api/v1/identity/users/${USER_ID}/reactivate` },
    ]);
    const users = createUsersService(testApi(fake.fetch));
    await users.deactivate(USER_ID, "STAFF_LEFT");
    await users.reactivate(USER_ID, "CORRECTION");
    expect(fake.requests.map((r) => r.url.pathname.split("/").at(-1))).toEqual(["deactivate", "reactivate"]);
  });

  it("rol atar, kişiye yetki verir/çıkarır", async () => {
    const fake = fakeFetch([
      { method: "POST", path: `/api/v1/identity/users/${USER_ID}/roles` },
      { method: "POST", path: `/api/v1/identity/users/${USER_ID}/grants` },
      { method: "POST", path: `/api/v1/identity/users/${USER_ID}/denies` },
      { method: "DELETE", path: `/api/v1/identity/users/${USER_ID}/denies/report.daily:view` },
    ]);
    const access = createUserAccessService(testApi(fake.fetch));
    await access.assignRole(USER_ID, ROLE_ID, "ROLE_SETUP");
    await access.grant(USER_ID, "report.daily:view", "ROLE_SETUP");
    await access.deny(USER_ID, "report.daily:view", "ROLE_SETUP");
    await access.undeny(USER_ID, "report.daily:view", "CORRECTION");
    expect(fake.requests[0]?.body).toEqual({ roleId: ROLE_ID, reasonCode: "ROLE_SETUP" });
    expect(fake.requests[2]?.body).toEqual({ permission: "report.daily:view", reasonCode: "ROLE_SETUP" });
    expect(fake.requests[3]?.url.searchParams.get("reasonCode")).toBe("CORRECTION");
  });
});

describe("PermissionHistoryService", () => {
  it("sayfalama için before parametresini gönderir", async () => {
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/identity/permission-changes", body: [] }]);
    await createPermissionHistoryService(testApi(fake.fetch)).recent(50, "2026-10-01T10:00:00Z");
    const query = fake.requests[0]?.url.searchParams;
    expect(query?.get("take")).toBe("50");
    expect(query?.get("before")).toBe("2026-10-01T10:00:00Z");
  });
});

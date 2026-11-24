import { act, renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { HEADER_CORRELATION } from "@/shared/api/client";
import { fakeFetch, testWrapper } from "@/test/fakeApi";
import type { RoleView } from "../api/types";
import { useRolePermission, useRoles } from "./useRoles";

const ROLE: RoleView = { id: "33333333-3333-4333-8333-333333333333", name: "Pit boss", permissions: ["live.tables:view"] };

describe("useRoles", () => {
  it("rolleri yükler", async () => {
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/identity/roles", body: [ROLE] }]);
    const { result } = renderHook(() => useRoles(), { wrapper: testWrapper(fake.fetch) });
    await waitFor(() => expect(result.current.data).toEqual([ROLE]));
  });
});

describe("useRolePermission", () => {
  it("işaretlenen kutu için yetki verir, kaldırılan için geri alır; her eylem yeni correlation id", async () => {
    const fake = fakeFetch([
      { method: "POST", path: `/api/v1/identity/roles/${ROLE.id}/permissions` },
      { method: "DELETE", path: `/api/v1/identity/roles/${ROLE.id}/permissions/live.tables:operate` },
    ]);
    let done = 0;
    const { result } = renderHook(() => useRolePermission(ROLE.id, () => done++), { wrapper: testWrapper(fake.fetch) });

    await act(async () => {
      await result.current.run({ permission: "live.tables:operate", grant: true, reasonCode: "ROLE_SETUP" });
    });
    await act(async () => {
      await result.current.run({ permission: "live.tables:operate", grant: false, reasonCode: "ROLE_SETUP" });
    });

    expect(fake.requests.map((r) => r.method)).toEqual(["POST", "DELETE"]);
    expect(done).toBe(2);
    const ids = fake.requests.map((r) => r.headers.get(HEADER_CORRELATION));
    expect(ids[0]).not.toBe(ids[1]);
  });
});

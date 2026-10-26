import { describe, expect, it } from "vitest";
import { fakeFetch, testApi } from "@/test/fakeApi";
import { AUDIT_DEFAULT_TAKE, AUDIT_MAX_TAKE, createAuditService, toAuditQuery } from "./auditService";

const ACTOR = "88888888-8888-4888-8888-888888888888";

describe("toAuditQuery", () => {
  it("boş filtreleri çıkarır ve varsayılan kayıt sayısını koyar", () => {
    expect(toAuditQuery({ actorId: "  ", correlationId: "" })).toEqual({ take: AUDIT_DEFAULT_TAKE });
  });

  it("kayıt sayısını sunucu sınırına kırpar", () => {
    expect(toAuditQuery({ take: 10_000 }).take).toBe(AUDIT_MAX_TAKE);
    expect(toAuditQuery({ take: 0 }).take).toBe(1);
  });
});

describe("AuditService", () => {
  it("filtreleri sorgu parametresi olarak gönderir", async () => {
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/audit/entries", body: [] }]);
    await createAuditService(testApi(fake.fetch)).search({ from: "2026-10-01T00:00:00.000Z", actorId: ACTOR, take: 20 });
    const query = fake.requests[0]?.url.searchParams;
    expect(query?.get("from")).toBe("2026-10-01T00:00:00.000Z");
    expect(query?.get("actorId")).toBe(ACTOR);
    expect(query?.get("take")).toBe("20");
    expect(query?.has("to")).toBe(false);
  });
});

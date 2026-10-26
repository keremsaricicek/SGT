import { act, renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { fakeFetch, problem, testWrapper } from "@/test/fakeApi";
import { useDeadLetters, useReprocess } from "./useDeadLetters";

const EVENT_ID = "99999999-9999-4999-8999-999999999999";
const LETTER = { module: "identity", eventId: EVENT_ID, type: "RolePermissionGrantedV1", attempts: 5, lastError: "timeout", occurredAt: "2026-10-26T09:00:00Z" };

describe("useDeadLetters", () => {
  it("hata kuyruğunu varsayılan kayıt sayısıyla yükler", async () => {
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/integration/dead-letters", body: [LETTER] }]);
    const { result } = renderHook(() => useDeadLetters(), { wrapper: testWrapper(fake.fetch) });
    await waitFor(() => expect(result.current.data).toEqual([LETTER]));
    expect(fake.requests[0]?.url.searchParams.get("take")).toBe("100");
  });
});

describe("useReprocess", () => {
  it("modül ve olay kimliğiyle yeniden işletir", async () => {
    const fake = fakeFetch([{ method: "POST", path: `/api/v1/integration/dead-letters/identity/${EVENT_ID}/reprocess` }]);
    let reloaded = 0;
    const { result } = renderHook(() => useReprocess(() => reloaded++), { wrapper: testWrapper(fake.fetch) });
    await act(async () => {
      await result.current.run({ module: "identity", eventId: EVENT_ID });
    });
    expect(reloaded).toBe(1);
  });

  it("bulunamayan olayda problem ayrıntısını tutar", async () => {
    const fake = fakeFetch([
      {
        method: "POST",
        path: `/api/v1/integration/dead-letters/identity/${EVENT_ID}/reprocess`,
        status: 404,
        body: problem("Integration.DeadLetterNotFound", "Bu kimlikte dead-letter olay yok.", 404),
      },
    ]);
    const { result } = renderHook(() => useReprocess(), { wrapper: testWrapper(fake.fetch) });
    await act(async () => {
      await result.current.run({ module: "identity", eventId: EVENT_ID });
    });
    expect(result.current.error).toMatchObject({ status: 404, detail: "Bu kimlikte dead-letter olay yok." });
  });
});

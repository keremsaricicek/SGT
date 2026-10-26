import { act, renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { fakeFetch, testWrapper } from "@/test/fakeApi";
import { usePlayerChange, usePlayerSearch } from "./usePlayers";

const PLAYER_ID = "77777777-7777-4777-8777-777777777777";

describe("usePlayerSearch", () => {
  it("kısa terimde yüklemez, yeterli terimde sonuçları getirir", async () => {
    const summary = { playerId: PLAYER_ID, playerNumber: "P000002", firstName: "Deneme", lastName: "Kişi", activeCardNumber: null, isRestricted: true };
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/players", body: [summary] }]);
    const { result, rerender } = renderHook(({ term }) => usePlayerSearch(term), {
      wrapper: testWrapper(fake.fetch),
      initialProps: { term: "P" },
    });
    expect(result.current.loading).toBe(false);
    expect(fake.requests).toHaveLength(0);

    rerender({ term: "P000002" });
    await waitFor(() => expect(result.current.data).toEqual([summary]));
  });
});

describe("usePlayerChange", () => {
  it("kart iptali için doğru uca gider", async () => {
    const fake = fakeFetch([{ method: "POST", path: `/api/v1/players/${PLAYER_ID}/cards/cancel` }]);
    const { result } = renderHook(() => usePlayerChange(PLAYER_ID), { wrapper: testWrapper(fake.fetch) });
    await act(async () => {
      await result.current.run({ kind: "cancelCard", reasonCode: "LOST_CARD" });
    });
    expect(result.current.succeeded).toBe(true);
    expect(fake.requests[0]?.url.pathname).toBe(`/api/v1/players/${PLAYER_ID}/cards/cancel`);
  });
});

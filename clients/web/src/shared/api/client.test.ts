import { describe, expect, it } from "vitest";
import { TEST_TERMINAL, TEST_TOKEN, fakeFetch, problem, testApi } from "@/test/fakeApi";
import { HEADER_CORRELATION, HEADER_TERMINAL } from "./client";
import { ApiError, describeError } from "./problem";

const MENU = "/api/v1/identity/me/menu";

describe("createSgtApi", () => {
  it("her isteğe Bearer token, X-Terminal ve X-Correlation-Id ekler", async () => {
    const fake = fakeFetch([{ method: "GET", path: MENU, body: [] }]);
    const api = testApi(fake.fetch);

    await api.data(api.client.GET(MENU));

    const [request] = fake.requests;
    expect(request?.headers.get("Authorization")).toBe(`Bearer ${TEST_TOKEN}`);
    expect(request?.headers.get(HEADER_TERMINAL)).toBe(TEST_TERMINAL);
    expect(request?.headers.get(HEADER_CORRELATION)).toBe(api.correlationId);
    expect(api.correlationId).toMatch(/^[0-9a-f-]{36}$/);
  });

  it("aynı eylemdeki istekler aynı, farklı eylemler farklı correlation id taşır", async () => {
    const fake = fakeFetch([{ method: "GET", path: MENU, body: [] }]);
    const first = testApi(fake.fetch);
    await first.data(first.client.GET(MENU));
    await first.data(first.client.GET(MENU));
    const second = testApi(fake.fetch);
    await second.data(second.client.GET(MENU));

    const ids = fake.requests.map((r) => r.headers.get(HEADER_CORRELATION));
    expect(ids[0]).toBe(ids[1]);
    expect(ids[2]).not.toBe(ids[0]);
  });

  it("token yoksa Authorization başlığı göndermez", async () => {
    const fake = fakeFetch([{ method: "GET", path: MENU, body: [] }]);
    const { createSgtApi } = await import("./client");
    const api = createSgtApi({
      baseUrl: "http://api.test",
      getAccessToken: async () => null,
      getTerminal: () => "T1",
      fetch: fake.fetch,
    });
    await api.data(api.client.GET(MENU));
    expect(fake.requests[0]?.headers.has("Authorization")).toBe(false);
  });

  it("hata yanıtını problem ayrıntısıyla ApiError'a çevirir", async () => {
    const fake = fakeFetch([
      { method: "GET", path: MENU, status: 403, body: problem("Common.PermissionRequired", "Bu işlem için yetkiniz yok.", 403) },
    ]);
    const api = testApi(fake.fetch);

    const error = await api.data(api.client.GET(MENU)).catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    const apiError = error as ApiError;
    expect(apiError.status).toBe(403);
    expect(apiError.title).toBe("Common.PermissionRequired");
    expect(apiError.detail).toBe("Bu işlem için yetkiniz yok.");
    expect(apiError.correlationId).toBe(api.correlationId);
    expect(describeError(apiError)).toEqual({
      title: "Common.PermissionRequired",
      detail: "Bu işlem için yetkiniz yok.",
      correlationId: api.correlationId,
    });
  });

  it("gövdesiz hata yanıtında durum kodunu başlık yapar", async () => {
    const fake = fakeFetch([]);
    const api = testApi(fake.fetch);
    const error = (await api.done(api.client.GET(MENU)).catch((e: unknown) => e)) as ApiError;
    expect(error.status).toBe(404);
    expect(describeError(error).title).toBeTruthy();
  });
});

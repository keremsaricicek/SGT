import { act, renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ApiError } from "@/shared/api/problem";
import { fakeFetch, problem, testWrapper } from "@/test/fakeApi";
import { useApiMutation } from "./useApiMutation";
import { useApiQuery } from "./useApiQuery";

const PATH = "/api/v1/configuration/exchange-rates/current";

describe("useApiQuery", () => {
  it("yüklenir, veriyi döner ve yeniden yükler", async () => {
    const fake = fakeFetch([{ method: "GET", path: PATH, body: [{ currency: "EUR" }] }]);
    const { result } = renderHook(() => useApiQuery("rates", (api) => api.data<unknown[]>(api.client.GET(PATH))), {
      wrapper: testWrapper(fake.fetch),
    });

    expect(result.current.loading).toBe(true);
    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(result.current.data).toEqual([{ currency: "EUR" }]);

    act(() => result.current.reload());
    await waitFor(() => expect(fake.requests).toHaveLength(2));
  });

  it("anahtar null ise istek atmaz", () => {
    const fake = fakeFetch([]);
    const { result } = renderHook(() => useApiQuery(null, (api) => api.data(api.client.GET(PATH))), {
      wrapper: testWrapper(fake.fetch),
    });
    expect(result.current.loading).toBe(false);
    expect(fake.requests).toHaveLength(0);
  });

  it("hata durumunda ApiError'u verir", async () => {
    const fake = fakeFetch([{ method: "GET", path: PATH, status: 403, body: problem("Common.PermissionRequired", "Yetki yok", 403) }]);
    const { result } = renderHook(() => useApiQuery("rates", (api) => api.data(api.client.GET(PATH))), {
      wrapper: testWrapper(fake.fetch),
    });
    await waitFor(() => expect(result.current.error).toBeInstanceOf(ApiError));
  });
});

describe("useApiMutation", () => {
  it("başarıda onSuccess çağırır; hatada error tutar", async () => {
    const fake = fakeFetch([
      { method: "POST", path: "/api/v1/configuration/exchange-rates", status: 400, body: problem("Common.ReasonCodeRequired", "Gerekçe kodu zorunludur.") },
    ]);
    let succeeded = 0;
    const { result } = renderHook(
      () =>
        useApiMutation(
          (api, reasonCode: string) =>
            api.done(api.client.POST("/api/v1/configuration/exchange-rates", { body: { currency: "EUR", buy: 1, sell: 1, reasonCode } })),
          () => succeeded++,
        ),
      { wrapper: testWrapper(fake.fetch) },
    );

    await act(async () => {
      await result.current.run("X");
    });

    expect(succeeded).toBe(0);
    expect((result.current.error as ApiError).detail).toBe("Gerekçe kodu zorunludur.");
    expect(result.current.pending).toBe(false);
  });
});

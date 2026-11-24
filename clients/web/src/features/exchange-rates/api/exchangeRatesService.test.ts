import { describe, expect, it } from "vitest";
import { fakeFetch, testApi } from "@/test/fakeApi";
import { createExchangeRatesService, toDecimalString } from "./exchangeRatesService";

describe("ExchangeRatesService", () => {
  it("güncel kurları getirir", async () => {
    const rates = [{ currency: "EUR", buy: 36.5, sell: 37.1, effectiveFrom: "2026-10-26T08:00:00Z" }];
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/configuration/exchange-rates/current", body: rates }]);
    await expect(createExchangeRatesService(testApi(fake.fetch)).current()).resolves.toEqual(rates);
  });

  it("kuru ondalık metin ve gerekçe koduyla gönderir", async () => {
    const fake = fakeFetch([{ method: "POST", path: "/api/v1/configuration/exchange-rates" }]);
    await createExchangeRatesService(testApi(fake.fetch)).set({ currency: "GBP", buy: "43.25", sell: "44.10", reasonCode: "DAILY_RATE" });
    expect(fake.requests[0]?.body).toEqual({ currency: "GBP", buy: "43.25", sell: "44.10", reasonCode: "DAILY_RATE" });
  });
});

describe("toDecimalString", () => {
  it("virgülü noktaya çevirir, geçersiz/negatif/sıfır değeri reddeder", () => {
    expect(toDecimalString("36,75")).toBe("36.75");
    expect(toDecimalString(" 40 ")).toBe("40");
    expect(toDecimalString("abc")).toBeNull();
    expect(toDecimalString("-1")).toBeNull();
    expect(toDecimalString("0")).toBeNull();
  });
});

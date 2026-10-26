import type { Schemas, SgtApi } from "@/shared/api/client";

/** Configuration.Contracts.ExchangeRateDto; sözleşmeden üretilir. */
export type ExchangeRate = Schemas["ExchangeRateDto"];

export type SetExchangeRateInput = Schemas["SetExchangeRateRequest"];

/** Ana birim TRY; kur yalnızca yabancı birimler için girilir (Spec 004 KK1–KK2). */
export const BASE_CURRENCY = "TRY";
export const FOREIGN_CURRENCIES = ["EUR", "GBP", "USD"] as const;

export interface ExchangeRatesService {
  current(): Promise<ExchangeRate[]>;
  set(input: SetExchangeRateInput): Promise<void>;
}

export function createExchangeRatesService(api: SgtApi): ExchangeRatesService {
  return {
    current: () => api.data<ExchangeRate[]>(api.client.GET("/api/v1/configuration/exchange-rates/current")),
    set: (input) => api.done(api.client.POST("/api/v1/configuration/exchange-rates", { body: input })),
  };
}

/**
 * Kullanıcının girdiği ondalık değeri API'ye metin olarak verir (decimal hassasiyeti korunur, float hesap yok).
 * Türkçe ondalık virgülü noktaya çevrilir; geçersizse null.
 */
export function toDecimalString(value: string): string | null {
  const normalized = value.trim().replace(",", ".");
  return /^\d+(\.\d+)?$/.test(normalized) && Number(normalized) > 0 ? normalized : null;
}

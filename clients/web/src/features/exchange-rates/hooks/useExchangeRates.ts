"use client";

import { useApiMutation } from "@/shared/hooks/useApiMutation";
import { useApiQuery } from "@/shared/hooks/useApiQuery";
import type { SetExchangeRateInput } from "../api/exchangeRatesService";
import { createExchangeRatesService } from "../api/exchangeRatesService";

export function useCurrentRates() {
  return useApiQuery("configuration/exchange-rates/current", (api) => createExchangeRatesService(api).current());
}

export function useSetRate(onDone?: () => void) {
  return useApiMutation((api, input: SetExchangeRateInput) => createExchangeRatesService(api).set(input), onDone);
}

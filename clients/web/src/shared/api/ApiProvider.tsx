"use client";

import { createContext, useContext, useMemo } from "react";
import type { ReactNode } from "react";
import type { ApiContextOptions, SgtApi } from "./client";
import { createSgtApi } from "./client";

export interface ApiRunner {
  /**
   * Bir kullanıcı eylemini çalıştırır. Her çağrı yeni bir X-Correlation-Id üretir; eylem içindeki
   * tüm istekler (ör. kaydet + listeyi yenile değil, yalnızca kaydet) aynı kimliği taşır.
   */
  run<T>(action: (api: SgtApi) => Promise<T>): Promise<T>;
}

const ApiContext = createContext<ApiRunner | null>(null);

export function ApiProvider({ options, children }: { options: ApiContextOptions; children: ReactNode }) {
  const runner = useMemo<ApiRunner>(() => ({ run: (action) => action(createSgtApi(options)) }), [options]);
  return <ApiContext.Provider value={runner}>{children}</ApiContext.Provider>;
}

export function useApi(): ApiRunner {
  const runner = useContext(ApiContext);
  if (!runner) throw new Error("useApi, ApiProvider içinde kullanılmalı.");
  return runner;
}

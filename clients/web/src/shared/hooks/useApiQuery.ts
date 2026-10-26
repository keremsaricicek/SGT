"use client";

import { useCallback, useEffect, useEffectEvent, useState } from "react";
import type { SgtApi } from "@/shared/api/client";
import { useApi } from "@/shared/api/ApiProvider";

export interface QueryState<T> {
  readonly data: T | undefined;
  readonly error: unknown;
  readonly loading: boolean;
  reload(): void;
}

interface Settled<T> {
  readonly token: string;
  readonly data: T | undefined;
  readonly error: unknown;
}

/**
 * Ekran açılırken veri yükler. <paramref name="key"/> değişince yeniden yükler; null ise yüklemez.
 * Her yükleme ayrı bir kullanıcı eylemidir (yeni correlation id).
 */
export function useApiQuery<T>(key: string | null, load: (api: SgtApi) => Promise<T>): QueryState<T> {
  const runner = useApi();
  const [version, setVersion] = useState(0);
  const [settled, setSettled] = useState<Settled<T> | null>(null);
  const token = key === null ? null : `${key}#${version}`;
  const loader = useEffectEvent(load);

  useEffect(() => {
    if (token === null) return;
    let active = true;
    runner
      .run((api) => loader(api))
      .then(
        (data) => active && setSettled({ token, data, error: undefined }),
        (error: unknown) => active && setSettled((prev) => ({ token, data: prev?.data, error })),
      );
    return () => {
      active = false;
    };
  }, [token, runner]);

  const reload = useCallback(() => setVersion((v) => v + 1), []);
  const current = settled !== null && settled.token === token;
  return {
    data: settled?.data,
    error: current ? settled.error : undefined,
    loading: token !== null && !current,
    reload,
  };
}

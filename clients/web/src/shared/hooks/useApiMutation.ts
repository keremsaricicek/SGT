"use client";

import { useCallback, useState } from "react";
import type { SgtApi } from "@/shared/api/client";
import { useApi } from "@/shared/api/ApiProvider";

export interface MutationState<TInput, TResult> {
  /** Başarılıysa sonucu, hata olursa undefined döner; hata `error` alanında tutulur. */
  run(input: TInput): Promise<TResult | undefined>;
  readonly pending: boolean;
  readonly error: unknown;
  readonly succeeded: boolean;
  reset(): void;
}

/** Değişiklik eylemi (ör. yetki ver). Her çalıştırma yeni bir correlation id ile tek eylemdir. */
export function useApiMutation<TInput, TResult = void>(
  action: (api: SgtApi, input: TInput) => Promise<TResult>,
  onSuccess?: (result: TResult, input: TInput) => void,
): MutationState<TInput, TResult> {
  const runner = useApi();
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<unknown>(undefined);
  const [succeeded, setSucceeded] = useState(false);

  const run = useCallback(
    async (input: TInput) => {
      setPending(true);
      setError(undefined);
      setSucceeded(false);
      try {
        const result = await runner.run((api) => action(api, input));
        setSucceeded(true);
        onSuccess?.(result, input);
        return result;
      } catch (e: unknown) {
        setError(e);
        return undefined;
      } finally {
        setPending(false);
      }
    },
    [runner, action, onSuccess],
  );

  const reset = useCallback(() => {
    setError(undefined);
    setSucceeded(false);
  }, []);

  return { run, pending, error, succeeded, reset };
}

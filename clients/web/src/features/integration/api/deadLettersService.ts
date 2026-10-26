import type { SgtApi } from "@/shared/api/client";

/** BuildingBlocks DeadLetter kaydı (Spec 002 KK5). Sözleşmede yanıt şeması yok; elle yazıldı. */
export interface DeadLetter {
  readonly module: string;
  readonly eventId: string;
  readonly type: string;
  readonly attempts: number;
  readonly lastError: string | null;
  readonly occurredAt: string;
}

export const DEAD_LETTER_TAKE = 100;

export interface DeadLettersService {
  list(take?: number): Promise<DeadLetter[]>;
  /** Olayı yeniden işleme kuyruğuna alır. Sözleşmede gerekçe kodu alanı yoktur. */
  reprocess(module: string, eventId: string): Promise<void>;
}

export function createDeadLettersService(api: SgtApi): DeadLettersService {
  return {
    list: (take = DEAD_LETTER_TAKE) =>
      api.data<DeadLetter[]>(api.client.GET("/api/v1/integration/dead-letters", { params: { query: { take } } })),
    reprocess: (module, eventId) =>
      api.done(
        api.client.POST("/api/v1/integration/dead-letters/{module}/{eventId}/reprocess", {
          params: { path: { module, eventId } },
        }),
      ),
  };
}

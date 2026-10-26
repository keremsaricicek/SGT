import type { Schemas, SgtApi } from "@/shared/api/client";

/** BuildingBlocks DeadLetter kaydı (Spec 002 KK5); sözleşmeden üretilir. */
export type DeadLetter = Schemas["DeadLetter"];

export const DEAD_LETTER_TAKE = 100;

export interface DeadLettersService {
  list(take?: number): Promise<DeadLetter[]>;
  /** Olayı yeniden işleme kuyruğuna alır; gerekçe kodu zorunludur ve audit'e yazılır. */
  reprocess(module: string, eventId: string, reasonCode: string): Promise<void>;
}

export function createDeadLettersService(api: SgtApi): DeadLettersService {
  return {
    list: (take = DEAD_LETTER_TAKE) =>
      api.data<DeadLetter[]>(api.client.GET("/api/v1/integration/dead-letters", { params: { query: { take } } })),
    reprocess: (module, eventId, reasonCode) =>
      api.done(
        api.client.POST("/api/v1/integration/dead-letters/{module}/{eventId}/reprocess", {
          params: { path: { module, eventId } },
          body: { reasonCode },
        }),
      ),
  };
}

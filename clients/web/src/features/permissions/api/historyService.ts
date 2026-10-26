import type { SgtApi } from "@/shared/api/client";
import type { PermissionChangeView } from "./types";

export interface PermissionHistoryService {
  /** En yeniden eskiye; `before` verilirse o andan önceki kayıtlar (sayfalama). */
  recent(take: number, before?: string): Promise<PermissionChangeView[]>;
}

export function createPermissionHistoryService(api: SgtApi): PermissionHistoryService {
  return {
    recent: (take, before) =>
      api.data<PermissionChangeView[]>(
        api.client.GET("/api/v1/identity/permission-changes", {
          params: { query: before ? { take, before } : { take } },
        }),
      ),
  };
}

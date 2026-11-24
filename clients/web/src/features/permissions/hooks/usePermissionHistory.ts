"use client";

import { useApiQuery } from "@/shared/hooks/useApiQuery";
import { createPermissionHistoryService } from "../api/historyService";

export const HISTORY_PAGE_SIZE = 50;

/** Geçmişin bir sayfası; `before` boşsa en yeni kayıtlar. */
export function usePermissionHistoryPage(before: string | undefined) {
  return useApiQuery(`identity/permission-changes?before=${before ?? ""}`, (api) =>
    createPermissionHistoryService(api).recent(HISTORY_PAGE_SIZE, before),
  );
}

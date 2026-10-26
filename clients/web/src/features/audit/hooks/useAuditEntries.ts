"use client";

import { useApiQuery } from "@/shared/hooks/useApiQuery";
import type { AuditFilter } from "../api/auditService";
import { createAuditService, toAuditQuery } from "../api/auditService";

export function useAuditEntries(filter: AuditFilter) {
  return useApiQuery(`audit/entries?${JSON.stringify(toAuditQuery(filter))}`, (api) => createAuditService(api).search(filter));
}

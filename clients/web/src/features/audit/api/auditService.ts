import type { SgtApi } from "@/shared/api/client";

/** Audit AuditEntryView (Spec 002). Sözleşmede yanıt şeması yok; elle yazıldı. */
export interface AuditEntry {
  readonly id: string;
  readonly eventType: string;
  readonly summary: string;
  readonly actorId: string;
  readonly terminal: string;
  readonly reasonCode: string;
  readonly correlationId: string;
  readonly occurredAt: string;
}

export interface AuditFilter {
  /** ISO 8601 */
  readonly from?: string;
  readonly to?: string;
  readonly actorId?: string;
  readonly correlationId?: string;
  readonly take?: number;
}

export const AUDIT_MAX_TAKE = 500;
export const AUDIT_DEFAULT_TAKE = 100;

export interface AuditService {
  search(filter: AuditFilter): Promise<AuditEntry[]>;
}

/** Boş alanları sorgudan çıkarır, take değerini sunucu sınırına kırpar. */
export function toAuditQuery(filter: AuditFilter) {
  const query: { from?: string; to?: string; actorId?: string; correlationId?: string; take: number } = {
    take: Math.min(Math.max(filter.take ?? AUDIT_DEFAULT_TAKE, 1), AUDIT_MAX_TAKE),
  };
  if (filter.from) query.from = filter.from;
  if (filter.to) query.to = filter.to;
  if (filter.actorId?.trim()) query.actorId = filter.actorId.trim();
  if (filter.correlationId?.trim()) query.correlationId = filter.correlationId.trim();
  return query;
}

export function createAuditService(api: SgtApi): AuditService {
  return {
    search: (filter) =>
      api.data<AuditEntry[]>(api.client.GET("/api/v1/audit/entries", { params: { query: toAuditQuery(filter) } })),
  };
}

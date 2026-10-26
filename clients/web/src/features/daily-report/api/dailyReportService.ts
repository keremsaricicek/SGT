import type { Schemas, SgtApi } from "@/shared/api/client";

/** Reporting DailySummary ve bileşenleri (Spec 011); sözleşmeden üretilir. */
export type CageCurrencyTotals = Schemas["CageCurrencyTotals"];
export type LiveCurrencyTotals = Schemas["LiveCurrencyTotals"];
export type SlotCurrencyTotals = Schemas["SlotCurrencyTotals"];
export type DailySummary = Schemas["DailySummary"];

export interface DailyReportService {
  /** `date` YYYY-MM-DD; boşsa sunucu bugünün iş gününü kullanır. */
  daily(date?: string): Promise<DailySummary>;
}

export function createDailyReportService(api: SgtApi): DailyReportService {
  return {
    daily: (date) =>
      api.data<DailySummary>(api.client.GET("/api/v1/reports/daily", { params: { query: date ? { date } : {} } })),
  };
}

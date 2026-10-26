"use client";

import { useApiQuery } from "@/shared/hooks/useApiQuery";
import { createDailyReportService } from "../api/dailyReportService";

export function useDailyReport(date: string) {
  return useApiQuery(`reports/daily?date=${date}`, (api) => createDailyReportService(api).daily(date || undefined));
}

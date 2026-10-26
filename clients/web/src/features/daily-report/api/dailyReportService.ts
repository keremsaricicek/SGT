import type { SgtApi } from "@/shared/api/client";

/** Reporting DailySummary ve bileşenleri (Spec 011). Sözleşmede yanıt şeması yok; elle yazıldı. */
export interface CageCurrencyTotals {
  readonly currency: string;
  readonly chipSales: number;
  readonly chipRedemptions: number;
  readonly foreignBought: number;
  readonly foreignSold: number;
  readonly closingVariance: number;
}

export interface LiveCurrencyTotals {
  readonly currency: string;
  readonly theoreticalWin: number;
  readonly casinoResult: number;
}

export interface SlotCurrencyTotals {
  readonly currency: string;
  readonly coinIn: number;
  readonly machineWin: number;
}

export interface DailySummary {
  readonly date: string;
  readonly from: string;
  readonly to: string;
  readonly cage: {
    readonly shiftsOpen: number;
    readonly shiftsClosed: number;
    readonly reversals: number;
    readonly currencies: readonly CageCurrencyTotals[];
  };
  readonly floor: {
    readonly ratingsFinished: number;
    readonly live: readonly LiveCurrencyTotals[];
    readonly slots: readonly SlotCurrencyTotals[];
  };
  readonly loyalty: {
    readonly pointsEarned: number;
    readonly pointsRedeemed: number;
  };
}

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

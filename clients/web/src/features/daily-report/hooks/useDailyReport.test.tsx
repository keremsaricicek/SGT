import { renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { fakeFetch, testApi, testWrapper } from "@/test/fakeApi";
import type { DailySummary } from "../api/dailyReportService";
import { createDailyReportService } from "../api/dailyReportService";
import { useDailyReport } from "./useDailyReport";

const SUMMARY: DailySummary = {
  date: "2026-10-25",
  from: "2026-10-25T06:00:00+03:00",
  to: "2026-10-26T06:00:00+03:00",
  cage: { shiftsOpen: 1, shiftsClosed: 2, reversals: 0, currencies: [] },
  floor: { ratingsFinished: 3, live: [], slots: [] },
  loyalty: { pointsEarned: 100, pointsRedeemed: 10 },
};

describe("DailyReportService", () => {
  it("tarih verilmezse date parametresi göndermez", async () => {
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/reports/daily", body: SUMMARY }]);
    await createDailyReportService(testApi(fake.fetch)).daily();
    expect(fake.requests[0]?.url.searchParams.has("date")).toBe(false);
  });
});

describe("useDailyReport", () => {
  it("seçilen tarihin özetini yükler", async () => {
    const fake = fakeFetch([{ method: "GET", path: "/api/v1/reports/daily", body: SUMMARY }]);
    const { result } = renderHook(() => useDailyReport("2026-10-25"), { wrapper: testWrapper(fake.fetch) });
    await waitFor(() => expect(result.current.data).toEqual(SUMMARY));
    expect(fake.requests[0]?.url.searchParams.get("date")).toBe("2026-10-25");
  });
});

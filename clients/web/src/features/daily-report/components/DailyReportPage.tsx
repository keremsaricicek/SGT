"use client";

import { useState } from "react";
import { ScreenGuard } from "@/shared/access/AccessProvider";
import { formatAmount, formatDateTime, formatInteger, todayLocal } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import type { MessageKey } from "@/shared/i18n/tr";
import { TextField } from "@/shared/ui/form";
import { EmptyRow, Loading, PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { DailySummary } from "../api/dailyReportService";
import { useDailyReport } from "../hooks/useDailyReport";

const SCREEN = "report.daily";

interface Column<T> {
  readonly label: MessageKey;
  readonly value: (row: T) => number | string;
}

function CurrencyTable<T extends { currency: string }>({ rows, columns }: { rows: readonly T[]; columns: readonly Column<T>[] }) {
  const { t, locale } = useI18n();
  return (
    <table className="table">
      <thead>
        <tr>
          <th>{t("common.currency")}</th>
          {columns.map((c) => (
            <th key={c.label} className="num">
              {t(c.label)}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {rows.length === 0 ? <EmptyRow colSpan={columns.length + 1} /> : null}
        {rows.map((row) => (
          <tr key={row.currency}>
            <td>{row.currency}</td>
            {columns.map((c) => (
              <td key={c.label} className="num">
                {formatAmount(locale, c.value(row))}
              </td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function Stat({ label, value }: { label: MessageKey; value: number | string }) {
  const { t, locale } = useI18n();
  return (
    <div className="stat">
      <span>{t(label)}</span>
      <strong>{formatInteger(locale, value)}</strong>
    </div>
  );
}

function SummaryView({ summary }: { summary: DailySummary }) {
  const { t, locale } = useI18n();
  return (
    <>
      <p className="muted">
        {t("daily.period", { from: formatDateTime(locale, summary.from), to: formatDateTime(locale, summary.to) })}
      </p>
      <Section title={t("daily.cage")}>
        <div className="stats">
          <Stat label="daily.shiftsOpen" value={summary.cage.shiftsOpen} />
          <Stat label="daily.shiftsClosed" value={summary.cage.shiftsClosed} />
          <Stat label="daily.reversals" value={summary.cage.reversals} />
        </div>
        <CurrencyTable
          rows={summary.cage.currencies}
          columns={[
            { label: "daily.chipSales", value: (r) => r.chipSales },
            { label: "daily.chipRedemptions", value: (r) => r.chipRedemptions },
            { label: "daily.foreignBought", value: (r) => r.foreignBought },
            { label: "daily.foreignSold", value: (r) => r.foreignSold },
            { label: "daily.closingVariance", value: (r) => r.closingVariance },
          ]}
        />
      </Section>
      <div className="columns">
        <Section title={t("daily.live")}>
          <div className="stats">
            <Stat label="daily.ratingsFinished" value={summary.floor.ratingsFinished} />
          </div>
          <CurrencyTable
            rows={summary.floor.live}
            columns={[
              { label: "daily.theoreticalWin", value: (r) => r.theoreticalWin },
              { label: "daily.casinoResult", value: (r) => r.casinoResult },
            ]}
          />
        </Section>
        <Section title={t("daily.slots")}>
          <CurrencyTable
            rows={summary.floor.slots}
            columns={[
              { label: "daily.coinIn", value: (r) => r.coinIn },
              { label: "daily.machineWin", value: (r) => r.machineWin },
            ]}
          />
        </Section>
      </div>
      <Section title={t("daily.loyalty")}>
        <div className="stats">
          <Stat label="daily.pointsEarned" value={summary.loyalty.pointsEarned} />
          <Stat label="daily.pointsRedeemed" value={summary.loyalty.pointsRedeemed} />
        </div>
      </Section>
    </>
  );
}

function DailyContent() {
  const { t } = useI18n();
  const [date, setDate] = useState(() => todayLocal());
  const report = useDailyReport(date);
  return (
    <>
      <PageHeader
        title={t("daily.title")}
        actions={
          <div className="inline-form">
            <TextField label={t("common.date")} type="date" value={date} max={todayLocal()} onChange={(e) => setDate(e.target.value)} />
            <button type="button" className="btn" onClick={report.reload}>
              {t("common.refresh")}
            </button>
          </div>
        }
      />
      <ProblemAlert error={report.error} />
      <Loading show={report.loading} />
      {report.data && !report.loading ? <SummaryView summary={report.data} /> : null}
    </>
  );
}

export function DailyReportPage() {
  return (
    <ScreenGuard screen={SCREEN}>
      <DailyContent />
    </ScreenGuard>
  );
}

"use client";

import { useState } from "react";
import { ScreenGuard } from "@/shared/access/AccessProvider";
import { formatDateTime, formatInteger, todayLocal } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import type { MessageKey } from "@/shared/i18n/tr";
import { Button } from "@/shared/ui/Button";
import { DataGrid } from "@/shared/ui/DataGrid";
import { TextField } from "@/shared/ui/form";
import { Amount, Loading, Page, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { DailySummary } from "../api/dailyReportService";
import { useDailyReport } from "../hooks/useDailyReport";

const SCREEN = "report.daily";

interface Column<T> {
  readonly label: MessageKey;
  readonly value: (row: T) => number | string;
  /** İşaretli tutar: negatif değer para çıkışı, pozitif değer para girişi rengiyle gösterilir (Spec 013 KK8). */
  readonly signed?: boolean;
}

function CurrencyTable<T extends { currency: string }>({ label, rows, columns }: {
  label: string;
  rows: readonly T[];
  columns: readonly Column<T>[];
}) {
  const { t } = useI18n();
  return (
    <DataGrid<T>
      label={label}
      rows={rows}
      rowKey={(row) => row.currency}
      columns={[
        { key: "currency", header: t("common.currency"), cell: (row) => <strong>{row.currency}</strong> },
        ...columns.map((c) => ({
          key: c.label,
          header: t(c.label),
          align: "end" as const,
          cell: (row: T) => <Amount value={c.value(row)} signed={c.signed} />,
        })),
      ]}
    />
  );
}

function Figures({ items }: { items: readonly [MessageKey, number | string][] }) {
  const { t, locale } = useI18n();
  return (
    <div className="figures">
      {items.map(([label, value]) => (
        <div key={label} className="figure">
          <span>{t(label)}</span>
          <strong>{formatInteger(locale, value)}</strong>
        </div>
      ))}
    </div>
  );
}

function SummaryView({ summary }: { summary: DailySummary }) {
  const { t } = useI18n();
  return (
    <>
      <Section title={t("daily.cage")} flush>
        <Figures
          items={[
            ["daily.shiftsOpen", summary.cage.shiftsOpen],
            ["daily.shiftsClosed", summary.cage.shiftsClosed],
            ["daily.reversals", summary.cage.reversals],
          ]}
        />
        <CurrencyTable
          label={t("daily.cage")}
          rows={summary.cage.currencies}
          columns={[
            { label: "daily.chipSales", value: (r) => r.chipSales },
            { label: "daily.chipRedemptions", value: (r) => r.chipRedemptions },
            { label: "daily.foreignBought", value: (r) => r.foreignBought },
            { label: "daily.foreignSold", value: (r) => r.foreignSold },
            { label: "daily.closingVariance", value: (r) => r.closingVariance, signed: true },
          ]}
        />
      </Section>
      <div className="columns">
        <Section title={t("daily.live")} flush>
          <Figures items={[["daily.ratingsFinished", summary.floor.ratingsFinished]]} />
          <CurrencyTable
            label={t("daily.live")}
            rows={summary.floor.live}
            columns={[
              { label: "daily.theoreticalWin", value: (r) => r.theoreticalWin },
              { label: "daily.casinoResult", value: (r) => r.casinoResult, signed: true },
            ]}
          />
        </Section>
        <Section title={t("daily.slots")} flush>
          <CurrencyTable
            label={t("daily.slots")}
            rows={summary.floor.slots}
            columns={[
              { label: "daily.coinIn", value: (r) => r.coinIn },
              { label: "daily.machineWin", value: (r) => r.machineWin, signed: true },
            ]}
          />
        </Section>
      </div>
      <Section title={t("daily.loyalty")} flush>
        <Figures
          items={[
            ["daily.pointsEarned", summary.loyalty.pointsEarned],
            ["daily.pointsRedeemed", summary.loyalty.pointsRedeemed],
          ]}
        />
      </Section>
    </>
  );
}

function DailyContent() {
  const { t, locale } = useI18n();
  const [date, setDate] = useState(() => todayLocal());
  const report = useDailyReport(date);
  const summary = report.data && !report.loading ? report.data : null;
  return (
    <Page
      title={t("daily.title")}
      meta={
        summary
          ? t("daily.period", { from: formatDateTime(locale, summary.from), to: formatDateTime(locale, summary.to) })
          : null
      }
      actions={
        <>
          <TextField label={t("common.date")} type="date" value={date} max={todayLocal()} onChange={(e) => setDate(e.target.value)} />
          <Button icon="refresh" onClick={report.reload}>
            {t("common.refresh")}
          </Button>
        </>
      }
    >
      <ProblemAlert error={report.error} />
      <Loading show={report.loading} />
      {summary ? <SummaryView summary={summary} /> : null}
    </Page>
  );
}

export function DailyReportPage() {
  return (
    <ScreenGuard screen={SCREEN}>
      <DailyContent />
    </ScreenGuard>
  );
}

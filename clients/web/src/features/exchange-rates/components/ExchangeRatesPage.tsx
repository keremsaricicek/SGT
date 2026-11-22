"use client";

import { useState } from "react";
import { Can, ScreenGuard } from "@/shared/access/AccessProvider";
import { formatDateTime } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import { ActionForm } from "@/shared/ui/ActionForm";
import { SelectField, TextField } from "@/shared/ui/form";
import { DataGrid } from "@/shared/ui/DataGrid";
import { Amount, Page, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { ExchangeRate } from "../api/exchangeRatesService";
import { BASE_CURRENCY, FOREIGN_CURRENCIES, toDecimalString } from "../api/exchangeRatesService";
import { useCurrentRates, useSetRate } from "../hooks/useExchangeRates";

const SCREEN = "admin.settings";

function SetRateForm({ onDone }: { onDone: () => void }) {
  const { t } = useI18n();
  const [currency, setCurrency] = useState<string>(FOREIGN_CURRENCIES[0]);
  const [buy, setBuy] = useState("");
  const [sell, setSell] = useState("");
  const [invalid, setInvalid] = useState(false);
  const setRate = useSetRate(() => {
    setBuy("");
    setSell("");
    onDone();
  });

  const submit = (reasonCode: string) => {
    const buyValue = toDecimalString(buy);
    const sellValue = toDecimalString(sell);
    setInvalid(!buyValue || !sellValue);
    if (buyValue && sellValue) void setRate.run({ currency, buy: buyValue, sell: sellValue, reasonCode });
  };

  return (
    <ActionForm
      title={t("rates.set")}
      submitLabel={t("rates.set")}
      pending={setRate.pending}
      error={setRate.error}
      succeeded={setRate.succeeded}
      onSubmit={submit}
    >
      <SelectField label={t("common.currency")} value={currency} onChange={(e) => setCurrency(e.target.value)}>
        {FOREIGN_CURRENCIES.map((c) => (
          <option key={c} value={c}>
            {c}
          </option>
        ))}
      </SelectField>
      <TextField label={`${t("rates.buy")} (${BASE_CURRENCY})`} inputMode="decimal" value={buy} onChange={(e) => setBuy(e.target.value)} required aria-invalid={invalid} />
      <TextField label={`${t("rates.sell")} (${BASE_CURRENCY})`} inputMode="decimal" value={sell} onChange={(e) => setSell(e.target.value)} required aria-invalid={invalid} />
    </ActionForm>
  );
}

function RatesContent() {
  const { t, locale } = useI18n();
  const rates = useCurrentRates();
  return (
    <Page title={t("rates.title")} meta={t("rates.baseNote")}>
      <ProblemAlert error={rates.error} />
      <Section flush>
        <DataGrid<ExchangeRate>
          label={t("rates.title")}
          rows={rates.data}
          loading={rates.loading}
          rowKey={(r) => r.currency}
          columns={[
            { key: "currency", header: t("common.currency"), cell: (r) => <strong>{r.currency}</strong> },
            { key: "buy", header: `${t("rates.buy")} (${BASE_CURRENCY})`, align: "end", cell: (r) => <Amount value={r.buy} /> },
            { key: "sell", header: `${t("rates.sell")} (${BASE_CURRENCY})`, align: "end", cell: (r) => <Amount value={r.sell} /> },
            { key: "from", header: t("rates.effectiveFrom"), className: "tabular", cell: (r) => formatDateTime(locale, r.effectiveFrom) },
          ]}
        />
      </Section>
      <Can screen={SCREEN} action="exchange_rate">
        <Section>
          <SetRateForm onDone={rates.reload} />
        </Section>
      </Can>
    </Page>
  );
}

export function ExchangeRatesPage() {
  return (
    <ScreenGuard screen={SCREEN}>
      <RatesContent />
    </ScreenGuard>
  );
}

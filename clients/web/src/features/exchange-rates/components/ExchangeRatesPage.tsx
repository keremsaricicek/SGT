"use client";

import { useState } from "react";
import { Can, ScreenGuard } from "@/shared/access/AccessProvider";
import { formatAmount, formatDateTime } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import { ActionForm } from "@/shared/ui/ActionForm";
import { SelectField, TextField } from "@/shared/ui/form";
import { EmptyRow, Loading, PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
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
    <>
      <PageHeader title={t("rates.title")} />
      <Section>
        <p className="muted">{t("rates.baseNote")}</p>
        <ProblemAlert error={rates.error} />
        <Loading show={rates.loading && !rates.data} />
        <table className="table">
          <thead>
            <tr>
              <th>{t("common.currency")}</th>
              <th className="num">{t("rates.buy")}</th>
              <th className="num">{t("rates.sell")}</th>
              <th>{t("rates.effectiveFrom")}</th>
            </tr>
          </thead>
          <tbody>
            {rates.data?.length === 0 ? <EmptyRow colSpan={4} /> : null}
            {rates.data?.map((rate) => (
              <tr key={rate.currency}>
                <td>{rate.currency}</td>
                <td className="num">{formatAmount(locale, rate.buy)}</td>
                <td className="num">{formatAmount(locale, rate.sell)}</td>
                <td>{formatDateTime(locale, rate.effectiveFrom)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Section>
      <Can screen={SCREEN} action="exchange_rate">
        <Section>
          <SetRateForm onDone={rates.reload} />
        </Section>
      </Can>
    </>
  );
}

export function ExchangeRatesPage() {
  return (
    <ScreenGuard screen={SCREEN}>
      <RatesContent />
    </ScreenGuard>
  );
}

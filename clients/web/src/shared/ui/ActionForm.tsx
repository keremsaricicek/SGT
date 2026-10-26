"use client";

import { useState } from "react";
import type { FormEvent, ReactNode } from "react";
import { useT } from "@/shared/i18n/I18nProvider";
import { ProblemAlert } from "./ProblemAlert";
import { FormSuccess, ReasonCodeField, isValidReasonCode, normalizeReasonCode } from "./form";

interface ActionFormProps {
  readonly title?: string;
  readonly submitLabel: string;
  readonly pending: boolean;
  readonly error: unknown;
  readonly succeeded: boolean;
  /** Gerekçe kodu doğrulanmış ve büyük harfe çevrilmiş olarak verilir. */
  readonly onSubmit: (reasonCode: string) => void | Promise<unknown>;
  readonly children?: ReactNode;
}

/** Değişiklik formu: alanlar + zorunlu gerekçe kodu + API hatası gösterimi. */
export function ActionForm({ title, submitLabel, pending, error, succeeded, onSubmit, children }: ActionFormProps) {
  const t = useT();
  const [reasonCode, setReasonCode] = useState("");
  const [missing, setMissing] = useState(false);

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!isValidReasonCode(reasonCode)) {
      setMissing(true);
      return;
    }
    setMissing(false);
    void onSubmit(normalizeReasonCode(reasonCode));
  };

  return (
    <form className="action-form" onSubmit={submit} noValidate>
      {title ? <h3>{title}</h3> : null}
      <div className="form-grid">
        {children}
        <ReasonCodeField value={reasonCode} onChange={setReasonCode} />
      </div>
      {missing ? <p className="field-error">{t("common.reasonCode.required")}</p> : null}
      <div className="form-actions">
        <button type="submit" className="btn primary" disabled={pending}>
          {submitLabel}
        </button>
      </div>
      <ProblemAlert error={error} />
      <FormSuccess show={succeeded && !pending} />
    </form>
  );
}

/** Satır/kutu işlemleri için ortak gerekçe kodu (ör. yetki matrisi). */
export function useReasonCode() {
  const [value, setValue] = useState("");
  const [missing, setMissing] = useState(false);
  return {
    value,
    setValue,
    missing,
    /** Geçerliyse normalleştirilmiş kodu, değilse null döner ve uyarıyı açar. */
    take(): string | null {
      if (!isValidReasonCode(value)) {
        setMissing(true);
        return null;
      }
      setMissing(false);
      return normalizeReasonCode(value);
    },
  };
}

export function ReasonBar({ reason }: { reason: ReturnType<typeof useReasonCode> }) {
  const t = useT();
  return (
    <div className="reason-bar">
      <ReasonCodeField value={reason.value} onChange={reason.setValue} />
      {reason.missing ? <p className="field-error">{t("common.reasonCode.required")}</p> : null}
    </div>
  );
}

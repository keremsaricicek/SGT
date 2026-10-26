"use client";

import { useId } from "react";
import type { InputHTMLAttributes, ReactNode, SelectHTMLAttributes } from "react";
import { useT } from "@/shared/i18n/I18nProvider";

export const REASON_CODE_MAX = 50;
const REASON_SUGGESTIONS = ["ROLE_SETUP", "NEW_STAFF", "STAFF_LEFT", "CORRECTION", "CUSTOMER_REQUEST", "MANAGER_REQUEST", "DAILY_RATE"];

/** Gerekçe kodu sunucuda büyük harfe çevrilir; istemci aynı biçimi önceden uygular. */
export function normalizeReasonCode(value: string): string {
  return value.trim().toUpperCase();
}

export function isValidReasonCode(value: string): boolean {
  const normalized = normalizeReasonCode(value);
  return normalized.length > 0 && normalized.length <= REASON_CODE_MAX;
}

interface FieldProps extends InputHTMLAttributes<HTMLInputElement> {
  readonly label: string;
}

export function TextField({ label, ...input }: FieldProps) {
  const id = useId();
  return (
    <label className="field" htmlFor={id}>
      <span>{label}</span>
      <input id={id} {...input} />
    </label>
  );
}

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  readonly label: string;
  readonly children: ReactNode;
}

export function SelectField({ label, children, ...select }: SelectProps) {
  const id = useId();
  return (
    <label className="field" htmlFor={id}>
      <span>{label}</span>
      <select id={id} {...select}>
        {children}
      </select>
    </label>
  );
}

/** Her değişiklik formunda zorunlu gerekçe kodu alanı (Spec 001 KK12). */
export function ReasonCodeField({ value, onChange }: { value: string; onChange: (value: string) => void }) {
  const t = useT();
  const id = useId();
  const listId = `${id}-reasons`;
  return (
    <label className="field" htmlFor={id}>
      <span>
        {t("common.reasonCode")} <em className="required">*</em>
      </span>
      <input
        id={id}
        name="reasonCode"
        required
        maxLength={REASON_CODE_MAX}
        list={listId}
        placeholder={t("common.reasonCode.hint")}
        value={value}
        onChange={(e) => onChange(e.target.value.toUpperCase())}
        autoComplete="off"
      />
      <datalist id={listId}>
        {REASON_SUGGESTIONS.map((code) => (
          <option key={code} value={code} />
        ))}
      </datalist>
    </label>
  );
}

export function FormSuccess({ show }: { show: boolean }) {
  const t = useT();
  return show ? (
    <p className="success" role="status">
      {t("common.done")}
    </p>
  ) : null;
}

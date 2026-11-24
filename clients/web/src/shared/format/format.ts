import type { Locale } from "@/shared/i18n/I18nProvider";

const intlLocale: Record<Locale, string> = { tr: "tr-TR", en: "en-GB" };

export function formatDateTime(locale: Locale, value: string | null | undefined): string {
  if (!value) return "—";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat(intlLocale[locale], { dateStyle: "short", timeStyle: "medium" }).format(date);
}

export function formatDate(locale: Locale, value: string | null | undefined): string {
  if (!value) return "—";
  // "YYYY-MM-DD" yerel tarih olarak ele alınır (saat dilimi kayması olmasın).
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  const date = match ? new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3])) : new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat(intlLocale[locale], { dateStyle: "medium" }).format(date);
}

/** Tutarlar API'den decimal olarak gelir; yalnızca gösterim için biçimlenir, hesap yapılmaz. */
export function formatAmount(locale: Locale, value: number | string, currency?: string): string {
  const numeric = typeof value === "number" ? value : Number(value);
  if (!Number.isFinite(numeric)) return String(value);
  const formatted = new Intl.NumberFormat(intlLocale[locale], { minimumFractionDigits: 2, maximumFractionDigits: 6 }).format(numeric);
  return currency ? `${formatted} ${currency}` : formatted;
}

/** Sayaçlar sözleşmede `number | string` olarak tanımlıdır (JSON sayı ya da metin); yalnızca gösterilir. */
export function formatInteger(locale: Locale, value: number | string): string {
  const numeric = typeof value === "number" ? value : Number(value);
  if (!Number.isFinite(numeric)) return String(value);
  return new Intl.NumberFormat(intlLocale[locale]).format(numeric);
}

/** <input type="datetime-local"> değerini ISO 8601'e çevirir; boşsa null. */
export function localInputToIso(value: string): string | null {
  if (!value) return null;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date.toISOString();
}

/** Bugünün yerel tarihi (YYYY-MM-DD). */
export function todayLocal(now: Date = new Date()): string {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
}

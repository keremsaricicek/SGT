"use client";

import type { ReactNode } from "react";
import { formatAmount } from "@/shared/format/format";
import { useI18n, useT } from "@/shared/i18n/I18nProvider";

interface PageProps {
  readonly title: string;
  /** Başlığın yanındaki kısa bilgi (ör. kayıt sayısı, dönem). */
  readonly meta?: ReactNode;
  /** Araç çubuğunun sağındaki işlemler. */
  readonly actions?: ReactNode;
  readonly children: ReactNode;
}

/** Ekran iskeleti (Spec 013 KK4): yapışkan araç çubuğu (başlık + işlemler) ve içerik alanı. */
export function Page({ title, meta, actions, children }: PageProps) {
  return (
    <>
      <header className="toolbar">
        <div className="toolbar-title">
          <h1>{title}</h1>
          {meta ? <span className="toolbar-meta">{meta}</span> : null}
        </div>
        {actions ? <div className="toolbar-actions">{actions}</div> : null}
      </header>
      <div className="page">{children}</div>
    </>
  );
}

interface SectionProps {
  readonly title?: string;
  /** Başlık şeridinin sağı: kayıt sayısı ya da küçük işlemler. */
  readonly aside?: ReactNode;
  /** Gövde boşluksuz (grid kenardan kenara). */
  readonly flush?: boolean;
  readonly children: ReactNode;
}

/** Panel: başlık şeritli, kenarlıklı grup kutusu. */
export function Section({ title, aside, flush, children }: SectionProps) {
  return (
    <section className="panel">
      {title ? (
        <header className="panel-head">
          <h2>{title}</h2>
          {aside ? <span className="count">{aside}</span> : null}
        </header>
      ) : null}
      <div className={flush ? "panel-body flush panel-scroll" : "panel-body"}>{children}</div>
    </section>
  );
}

export function Loading({ show }: { show: boolean }) {
  const t = useT();
  return show ? <p className="muted">{t("app.loading")}</p> : null;
}

export function EmptyRow({ colSpan }: { colSpan: number }) {
  const t = useT();
  return (
    <tr>
      <td colSpan={colSpan} className="empty">
        {t("common.noRecords")}
      </td>
    </tr>
  );
}

/**
 * Tutar gösterimi (Spec 013 KK8): eşit genişlikli rakam; işaretli tutarlarda negatif değer eksi işaretiyle
 * ve para çıkışı rengiyle, pozitif değer para girişi rengiyle yazılır. Hesap yapılmaz, yalnızca gösterilir.
 */
export function Amount({ value, signed = false }: { value: number | string; signed?: boolean }) {
  const { locale } = useI18n();
  const numeric = typeof value === "number" ? value : Number(value);
  const tone = !signed || !Number.isFinite(numeric) || numeric === 0 ? undefined : numeric < 0 ? "amount-out" : "amount-in";
  return <span className={tone}>{formatAmount(locale, value)}</span>;
}

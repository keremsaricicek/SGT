"use client";

import type { ReactNode } from "react";
import { useT } from "@/shared/i18n/I18nProvider";

export function PageHeader({ title, actions }: { title: string; actions?: ReactNode }) {
  return (
    <header className="page-header">
      <h1>{title}</h1>
      {actions ? <div className="page-actions">{actions}</div> : null}
    </header>
  );
}

export function Section({ title, children }: { title?: string; children: ReactNode }) {
  return (
    <section className="card">
      {title ? <h2>{title}</h2> : null}
      {children}
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
      <td colSpan={colSpan} className="muted">
        {t("common.noRecords")}
      </td>
    </tr>
  );
}

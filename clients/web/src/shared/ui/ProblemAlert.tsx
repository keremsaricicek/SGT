"use client";

import { describeError } from "@/shared/api/problem";
import { useT } from "@/shared/i18n/I18nProvider";

/** API problem ayrıntısını (title/detail) kullanıcıya gösterir. */
export function ProblemAlert({ error }: { error: unknown }) {
  const t = useT();
  if (error === undefined || error === null) return null;
  const { title, detail, correlationId } = describeError(error);
  return (
    <div className="alert" role="alert">
      <strong>{t("common.errorTitle")}: </strong>
      <span className="alert-title">{title}</span>
      {detail ? <div className="alert-detail">{detail}</div> : null}
      {correlationId ? (
        <div className="alert-meta">
          {t("common.correlationId")}: <code>{correlationId}</code>
        </div>
      ) : null}
    </div>
  );
}

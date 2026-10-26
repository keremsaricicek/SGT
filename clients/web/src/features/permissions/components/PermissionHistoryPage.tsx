"use client";

import { useState } from "react";
import { ScreenGuard } from "@/shared/access/AccessProvider";
import { formatDateTime } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import { PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import { HISTORY_PAGE_SIZE, usePermissionHistoryPage } from "../hooks/usePermissionHistory";
import { SCREEN_HISTORY } from "./screens";

const COLUMNS = 8;

/** Geçmişin bir sayfası; son sayfa "daha eski" düğmesini gösterir. */
function HistoryChunk({ before, isLast, onMore }: { before?: string; isLast: boolean; onMore: (cursor: string) => void }) {
  const { t, locale } = useI18n();
  const page = usePermissionHistoryPage(before);
  const rows = page.data ?? [];
  const last = rows.at(-1);
  return (
    <>
      {page.error ? (
        <tr>
          <td colSpan={COLUMNS}>
            <ProblemAlert error={page.error} />
          </td>
        </tr>
      ) : null}
      {page.loading ? (
        <tr>
          <td colSpan={COLUMNS} className="muted">
            {t("app.loading")}
          </td>
        </tr>
      ) : null}
      {!page.loading && !before && rows.length === 0 ? (
        <tr>
          <td colSpan={COLUMNS} className="muted">
            {t("common.noRecords")}
          </td>
        </tr>
      ) : null}
      {rows.map((change) => (
        <tr key={change.id}>
          <td>{formatDateTime(locale, change.occurredAt)}</td>
          <td>
            {change.target} <code className="muted">{change.targetId}</code>
          </td>
          <td>{change.kind}</td>
          <td>
            <code>{change.item}</code>
          </td>
          <td>
            <code>{change.actorId}</code>
          </td>
          <td>{change.terminal}</td>
          <td>{change.reasonCode}</td>
          <td>
            <code>{change.correlationId}</code>
          </td>
        </tr>
      ))}
      {isLast && last && rows.length >= HISTORY_PAGE_SIZE ? (
        <tr>
          <td colSpan={COLUMNS}>
            <button type="button" className="btn small" onClick={() => onMore(last.occurredAt)}>
              {t("common.loadMore")}
            </button>
          </td>
        </tr>
      ) : null}
    </>
  );
}

function HistoryContent() {
  const { t } = useI18n();
  const [cursors, setCursors] = useState<(string | undefined)[]>([undefined]);
  return (
    <>
      <PageHeader title={t("permissions.history.title")} />
      <Section>
        <table className="table">
          <thead>
            <tr>
              <th>{t("common.time")}</th>
              <th>{t("permissions.history.target")}</th>
              <th>{t("permissions.history.kind")}</th>
              <th>{t("permissions.history.item")}</th>
              <th>{t("common.actor")}</th>
              <th>{t("common.terminal")}</th>
              <th>{t("common.reasonCode")}</th>
              <th>{t("common.correlationId")}</th>
            </tr>
          </thead>
          <tbody>
            {cursors.map((cursor, index) => (
              <HistoryChunk
                key={cursor ?? "first"}
                before={cursor}
                isLast={index === cursors.length - 1}
                onMore={(next) => setCursors((list) => [...list, next])}
              />
            ))}
          </tbody>
        </table>
      </Section>
    </>
  );
}

export function PermissionHistoryPage() {
  return (
    <ScreenGuard screen={SCREEN_HISTORY}>
      <HistoryContent />
    </ScreenGuard>
  );
}

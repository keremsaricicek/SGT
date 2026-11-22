"use client";

import { ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { formatDateTime } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import { ReasonBar, useReasonCode } from "@/shared/ui/ActionForm";
import { FormSuccess } from "@/shared/ui/form";
import { Button } from "@/shared/ui/Button";
import { DataGrid } from "@/shared/ui/DataGrid";
import { Page, Section } from "@/shared/ui/layout";
import { Badge } from "@/shared/ui/Badge";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { DeadLetter } from "../api/deadLettersService";
import { useDeadLetters, useReprocess } from "../hooks/useDeadLetters";

const SCREEN = "admin.integrations";

function DeadLettersContent() {
  const { t, locale } = useI18n();
  const { policy } = useAccess();
  const letters = useDeadLetters();
  const reprocess = useReprocess(letters.reload);
  const canReprocess = policy.can(SCREEN, "reprocess");
  const reason = useReasonCode();

  const onReprocess = (letter: DeadLetter) => {
    const reasonCode = reason.take();
    if (reasonCode) void reprocess.run({ module: letter.module, eventId: letter.eventId, reasonCode });
  };

  return (
    <Page
      title={t("integration.title")}
      meta={letters.data ? t("common.records", { count: letters.data.length }) : null}
      actions={
        <Button icon="refresh" onClick={letters.reload}>
          {t("common.refresh")}
        </Button>
      }
    >
      <ProblemAlert error={letters.error} />
      <Section flush>
        {canReprocess ? <ReasonBar reason={reason} /> : null}
        <div className="panel-messages">
          <ProblemAlert error={reprocess.error} />
          <FormSuccess show={reprocess.succeeded && !reprocess.pending} />
        </div>
        <DataGrid<DeadLetter>
          label={t("integration.title")}
          rows={letters.data}
          loading={letters.loading}
          rowKey={(l) => `${l.module}/${l.eventId}`}
          columns={[
            { key: "time", header: t("integration.occurredAt"), className: "tabular", cell: (l) => formatDateTime(locale, l.occurredAt) },
            { key: "module", header: t("integration.module"), cell: (l) => <Badge>{l.module}</Badge> },
            { key: "type", header: t("integration.type"), cell: (l) => <code>{l.type}</code> },
            {
              key: "attempts",
              header: t("integration.attempts"),
              align: "end",
              cell: (l) => l.attempts,
            },
            { key: "error", header: t("integration.lastError"), className: "wrap", cell: (l) => l.lastError ?? "—" },
            {
              key: "actions",
              header: <span className="visually-hidden">{t("integration.reprocess")}</span>,
              className: "actions",
              cell: (l) =>
                canReprocess ? (
                  <Button size="small" icon="reprocess" disabled={reprocess.pending} onClick={() => onReprocess(l)}>
                    {t("integration.reprocess")}
                  </Button>
                ) : null,
            },
          ]}
        />
      </Section>
    </Page>
  );
}

export function DeadLettersPage() {
  return (
    <ScreenGuard screen={SCREEN}>
      <DeadLettersContent />
    </ScreenGuard>
  );
}

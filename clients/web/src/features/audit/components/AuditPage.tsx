"use client";

import { useState } from "react";
import type { ChangeEvent, FormEvent } from "react";
import { ScreenGuard } from "@/shared/access/AccessProvider";
import { formatDateTime, localInputToIso } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import { TextField } from "@/shared/ui/form";
import { Button } from "@/shared/ui/Button";
import { DataGrid } from "@/shared/ui/DataGrid";
import { Page, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { AuditEntry, AuditFilter } from "../api/auditService";
import { AUDIT_DEFAULT_TAKE, AUDIT_MAX_TAKE } from "../api/auditService";
import { useAuditEntries } from "../hooks/useAuditEntries";

const SCREEN = "admin.audit";
const EMPTY_FORM = { from: "", to: "", actorId: "", correlationId: "", take: String(AUDIT_DEFAULT_TAKE) };
type FormState = typeof EMPTY_FORM;

function toFilter(form: FormState): AuditFilter {
  return {
    from: localInputToIso(form.from) ?? undefined,
    to: localInputToIso(form.to) ?? undefined,
    actorId: form.actorId,
    correlationId: form.correlationId,
    take: Number.parseInt(form.take, 10) || AUDIT_DEFAULT_TAKE,
  };
}

function AuditContent() {
  const { t, locale } = useI18n();
  const [form, setForm] = useState<FormState>(EMPTY_FORM);
  const [filter, setFilter] = useState<AuditFilter>(() => toFilter(EMPTY_FORM));
  const entries = useAuditEntries(filter);
  const set = (field: keyof FormState) => (e: ChangeEvent<HTMLInputElement>) => setForm((f) => ({ ...f, [field]: e.target.value }));

  const submit = (event: FormEvent) => {
    event.preventDefault();
    setFilter(toFilter(form));
    entries.reload();
  };

  return (
    <Page title={t("audit.title")} meta={entries.data ? t("common.records", { count: entries.data.length }) : null}>
      <Section>
        <form className="form-grid" onSubmit={submit}>
          <TextField label={t("audit.from")} type="datetime-local" value={form.from} onChange={set("from")} />
          <TextField label={t("audit.to")} type="datetime-local" value={form.to} onChange={set("to")} />
          <TextField label={t("audit.actorId")} value={form.actorId} onChange={set("actorId")} />
          <TextField label={t("common.correlationId")} value={form.correlationId} onChange={set("correlationId")} />
          <TextField label={t("common.take")} type="number" min={1} max={AUDIT_MAX_TAKE} value={form.take} onChange={set("take")} />
          <div>
            <Button type="submit" variant="primary" icon="filter">
              {t("common.filter")}
            </Button>
          </div>
        </form>
      </Section>
      <ProblemAlert error={entries.error} />
      <Section flush>
        <DataGrid<AuditEntry>
          label={t("audit.title")}
          density="compact"
          rows={entries.data}
          loading={entries.loading}
          rowKey={(e) => e.id}
          columns={[
            { key: "time", header: t("common.time"), className: "tabular", cell: (e) => formatDateTime(locale, e.occurredAt) },
            { key: "type", header: t("audit.eventType"), cell: (e) => <code>{e.eventType}</code> },
            { key: "summary", header: t("audit.summary"), className: "wrap", cell: (e) => e.summary },
            { key: "actor", header: t("common.actor"), cell: (e) => <code>{e.actorId}</code> },
            { key: "terminal", header: t("common.terminal"), cell: (e) => e.terminal },
            { key: "reason", header: t("common.reasonCode"), cell: (e) => <code>{e.reasonCode}</code> },
            { key: "correlation", header: t("common.correlationId"), cell: (e) => <code>{e.correlationId}</code> },
          ]}
        />
      </Section>
    </Page>
  );
}

export function AuditPage() {
  return (
    <ScreenGuard screen={SCREEN}>
      <AuditContent />
    </ScreenGuard>
  );
}

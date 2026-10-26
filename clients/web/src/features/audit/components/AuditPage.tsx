"use client";

import { useState } from "react";
import type { ChangeEvent, FormEvent } from "react";
import { ScreenGuard } from "@/shared/access/AccessProvider";
import { formatDateTime, localInputToIso } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import { TextField } from "@/shared/ui/form";
import { EmptyRow, Loading, PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { AuditFilter } from "../api/auditService";
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
    <>
      <PageHeader title={t("audit.title")} />
      <Section>
        <form className="form-grid" onSubmit={submit}>
          <TextField label={t("audit.from")} type="datetime-local" value={form.from} onChange={set("from")} />
          <TextField label={t("audit.to")} type="datetime-local" value={form.to} onChange={set("to")} />
          <TextField label={t("audit.actorId")} value={form.actorId} onChange={set("actorId")} />
          <TextField label={t("common.correlationId")} value={form.correlationId} onChange={set("correlationId")} />
          <TextField label={t("common.take")} type="number" min={1} max={AUDIT_MAX_TAKE} value={form.take} onChange={set("take")} />
          <div className="form-actions">
            <button type="submit" className="btn primary">
              {t("common.filter")}
            </button>
          </div>
        </form>
      </Section>
      <Section>
        <ProblemAlert error={entries.error} />
        <Loading show={entries.loading} />
        <table className="table">
          <thead>
            <tr>
              <th>{t("common.time")}</th>
              <th>{t("audit.eventType")}</th>
              <th>{t("audit.summary")}</th>
              <th>{t("common.actor")}</th>
              <th>{t("common.terminal")}</th>
              <th>{t("common.reasonCode")}</th>
              <th>{t("common.correlationId")}</th>
            </tr>
          </thead>
          <tbody>
            {entries.data?.length === 0 ? <EmptyRow colSpan={7} /> : null}
            {entries.data?.map((entry) => (
              <tr key={entry.id}>
                <td>{formatDateTime(locale, entry.occurredAt)}</td>
                <td>{entry.eventType}</td>
                <td>{entry.summary}</td>
                <td>
                  <code>{entry.actorId}</code>
                </td>
                <td>{entry.terminal}</td>
                <td>{entry.reasonCode}</td>
                <td>
                  <code>{entry.correlationId}</code>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Section>
    </>
  );
}

export function AuditPage() {
  return (
    <ScreenGuard screen={SCREEN}>
      <AuditContent />
    </ScreenGuard>
  );
}

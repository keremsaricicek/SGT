"use client";

import { ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { formatDateTime } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import { FormSuccess } from "@/shared/ui/form";
import { EmptyRow, Loading, PageHeader, Section } from "@/shared/ui/layout";
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

  const onReprocess = (letter: DeadLetter) => {
    if (window.confirm(t("integration.reprocessConfirm"))) {
      void reprocess.run({ module: letter.module, eventId: letter.eventId });
    }
  };

  return (
    <>
      <PageHeader
        title={t("integration.title")}
        actions={
          <button type="button" className="btn" onClick={letters.reload}>
            {t("common.refresh")}
          </button>
        }
      />
      <Section>
        <ProblemAlert error={letters.error ?? reprocess.error} />
        <FormSuccess show={reprocess.succeeded && !reprocess.pending} />
        <Loading show={letters.loading && !letters.data} />
        <table className="table">
          <thead>
            <tr>
              <th>{t("integration.occurredAt")}</th>
              <th>{t("integration.module")}</th>
              <th>{t("integration.type")}</th>
              <th className="num">{t("integration.attempts")}</th>
              <th>{t("integration.lastError")}</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {letters.data?.length === 0 ? <EmptyRow colSpan={6} /> : null}
            {letters.data?.map((letter) => (
              <tr key={`${letter.module}/${letter.eventId}`}>
                <td>{formatDateTime(locale, letter.occurredAt)}</td>
                <td>{letter.module}</td>
                <td>{letter.type}</td>
                <td className="num">{letter.attempts}</td>
                <td className="wrap">{letter.lastError ?? "—"}</td>
                <td>
                  {canReprocess ? (
                    <button type="button" className="btn small" disabled={reprocess.pending} onClick={() => onReprocess(letter)}>
                      {t("integration.reprocess")}
                    </button>
                  ) : null}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Section>
    </>
  );
}

export function DeadLettersPage() {
  return (
    <ScreenGuard screen={SCREEN}>
      <DeadLettersContent />
    </ScreenGuard>
  );
}

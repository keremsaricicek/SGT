"use client";

import { useState } from "react";
import { Can, useAccess } from "@/shared/access/AccessProvider";
import { formatDateTime, localInputToIso } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import { ActionForm, ReasonBar, useReasonCode } from "@/shared/ui/ActionForm";
import { FormSuccess, SelectField, TextField } from "@/shared/ui/form";
import { EmptyRow, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { RestrictionType, RestrictionView } from "../api/types";
import { RESTRICTION_TYPES } from "../api/types";
import { usePlayerChange } from "../hooks/usePlayers";
import { SCREEN_RESTRICTIONS } from "./screens";

function restrictionLabel(t: ReturnType<typeof useI18n>["t"], type: string): string {
  return type === "Banned" || type === "SelfExcluded" ? t(`players.restrictionType.${type}`) : type;
}

function AddRestrictionForm({ playerId, onDone }: { playerId: string; onDone: () => void }) {
  const { t } = useI18n();
  const [type, setType] = useState<RestrictionType>("Banned");
  const [note, setNote] = useState("");
  const [until, setUntil] = useState("");
  const change = usePlayerChange(playerId, () => {
    setNote("");
    setUntil("");
    onDone();
  });
  return (
    <ActionForm
      title={t("players.restrictions.add")}
      submitLabel={t("players.restrictions.add")}
      pending={change.pending}
      error={change.error}
      succeeded={change.succeeded}
      onSubmit={(reasonCode) =>
        change.run({
          kind: "addRestriction",
          input: { type, note: note.trim() || null, until: localInputToIso(until), reasonCode },
        })
      }
    >
      <SelectField
        label={t("players.restrictions.type")}
        value={type}
        onChange={(e) => setType(e.target.value as RestrictionType)}
      >
        {RESTRICTION_TYPES.map((value) => (
          <option key={value} value={value}>
            {restrictionLabel(t, value)}
          </option>
        ))}
      </SelectField>
      <TextField label={t("players.restrictions.note")} value={note} onChange={(e) => setNote(e.target.value)} />
      <TextField
        label={t("players.restrictions.until")}
        type="datetime-local"
        value={until}
        onChange={(e) => setUntil(e.target.value)}
      />
    </ActionForm>
  );
}

export function RestrictionsSection({ playerId, restrictions, onChanged }: {
  playerId: string;
  restrictions: readonly RestrictionView[];
  onChanged: () => void;
}) {
  const { t, locale } = useI18n();
  const { policy } = useAccess();
  const reason = useReasonCode();
  const remove = usePlayerChange(playerId, onChanged);
  const canRemove = policy.can(SCREEN_RESTRICTIONS, "remove");

  const onRemove = (restrictionId: string) => {
    const reasonCode = reason.take();
    if (reasonCode) void remove.run({ kind: "removeRestriction", restrictionId, reasonCode });
  };

  return (
    <>
      <Section title={t("players.restrictions.title")}>
        {canRemove && restrictions.length > 0 ? <ReasonBar reason={reason} /> : null}
        <ProblemAlert error={remove.error} />
        <FormSuccess show={remove.succeeded && !remove.pending} />
        <table className="table">
          <thead>
            <tr>
              <th>{t("players.restrictions.type")}</th>
              <th>{t("players.restrictions.note")}</th>
              <th>{t("players.restrictions.until")}</th>
              <th>{t("players.restrictions.addedAt")}</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {restrictions.length === 0 ? <EmptyRow colSpan={5} /> : null}
            {restrictions.map((r) => (
              <tr key={r.id}>
                <td>{restrictionLabel(t, r.type)}</td>
                <td>{r.note ?? "—"}</td>
                <td>{formatDateTime(locale, r.until)}</td>
                <td>{formatDateTime(locale, r.addedAt)}</td>
                <td>
                  {canRemove ? (
                    <button type="button" className="btn small" disabled={remove.pending} onClick={() => onRemove(r.id)}>
                      {t("common.remove")}
                    </button>
                  ) : null}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Section>
      <Can screen={SCREEN_RESTRICTIONS} action="add">
        <Section>
          <AddRestrictionForm playerId={playerId} onDone={onChanged} />
        </Section>
      </Can>
    </>
  );
}

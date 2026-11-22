"use client";

import { useState } from "react";
import { Can, useAccess } from "@/shared/access/AccessProvider";
import { formatDateTime, localInputToIso } from "@/shared/format/format";
import { useI18n } from "@/shared/i18n/I18nProvider";
import { ActionForm, ReasonBar, useReasonCode } from "@/shared/ui/ActionForm";
import { Badge } from "@/shared/ui/Badge";
import { Button } from "@/shared/ui/Button";
import { DataGrid } from "@/shared/ui/DataGrid";
import { FormSuccess, SelectField, TextField } from "@/shared/ui/form";
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

/** Aktif kısıtlamalar gridi + ekleme formu (oyuncu kartı sekmesi). */
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
      {canRemove && restrictions.length > 0 ? <ReasonBar reason={reason} /> : null}
      <div className="panel-messages">
        <ProblemAlert error={remove.error} />
        <FormSuccess show={remove.succeeded && !remove.pending} />
      </div>
      <DataGrid<RestrictionView>
        label={t("players.restrictions.title")}
        rows={restrictions}
        rowKey={(r) => r.id}
        rowClassName={() => "restricted"}
        columns={[
          {
            key: "type",
            header: t("players.restrictions.type"),
            cell: (r) => <Badge tone="restricted">{restrictionLabel(t, r.type)}</Badge>,
          },
          { key: "note", header: t("players.restrictions.note"), className: "wrap", cell: (r) => r.note ?? "—" },
          { key: "until", header: t("players.restrictions.until"), cell: (r) => formatDateTime(locale, r.until) },
          { key: "addedAt", header: t("players.restrictions.addedAt"), cell: (r) => formatDateTime(locale, r.addedAt) },
          {
            key: "actions",
            header: <span className="visually-hidden">{t("common.remove")}</span>,
            className: "actions",
            cell: (r) =>
              canRemove ? (
                <Button size="small" variant="danger" disabled={remove.pending} onClick={() => onRemove(r.id)}>
                  {t("common.remove")}
                </Button>
              ) : null,
          },
        ]}
      />
      <Can screen={SCREEN_RESTRICTIONS} action="add">
        <div className="subform">
          <AddRestrictionForm playerId={playerId} onDone={onChanged} />
        </div>
      </Can>
    </>
  );
}

"use client";

import Link from "next/link";
import { useState } from "react";
import { Can, ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { formatDate } from "@/shared/format/format";
import { hasKey, useI18n } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { ActionForm } from "@/shared/ui/ActionForm";
import { TextField } from "@/shared/ui/form";
import { Loading, PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { PlayerView } from "../api/types";
import { usePlayer, usePlayerChange } from "../hooks/usePlayers";
import { RestrictionsSection } from "./RestrictionsSection";
import { SCREEN_PROFILE, SCREEN_RESTRICTIONS, SCREEN_SEARCH } from "./screens";

function ProfileSummary({ player }: { player: PlayerView }) {
  const { t, locale } = useI18n();
  const documentKey = `players.documentType.${player.documentType}`;
  const rows: [string, string][] = [
    [t("players.number"), player.playerNumber],
    [t("players.firstName"), player.firstName],
    [t("players.lastName"), player.lastName],
    [t("players.birthDate"), formatDate(locale, player.birthDate)],
    [t("players.nationality"), player.nationality],
    [t("players.documentType"), hasKey(documentKey) ? t(documentKey) : player.documentType],
    [t("players.documentNumber"), player.maskedDocumentNumber],
    [t("players.phone"), player.phone ?? "—"],
    [t("players.card"), player.activeCardNumber ?? t("players.card.none")],
    [t("players.restricted"), player.isRestricted ? t("common.yes") : t("common.no")],
  ];
  return (
    <dl className="details">
      {rows.map(([label, value]) => (
        <div key={label}>
          <dt>{label}</dt>
          <dd>{value}</dd>
        </div>
      ))}
    </dl>
  );
}

function IssueCardForm({ playerId, onDone }: { playerId: string; onDone: () => void }) {
  const { t } = useI18n();
  const [cardNumber, setCardNumber] = useState("");
  const change = usePlayerChange(playerId, () => {
    setCardNumber("");
    onDone();
  });
  return (
    <ActionForm
      title={t("players.card.issue")}
      submitLabel={t("players.card.issue")}
      pending={change.pending}
      error={change.error}
      succeeded={change.succeeded}
      onSubmit={(reasonCode) => change.run({ kind: "issueCard", cardNumber: cardNumber.trim(), reasonCode })}
    >
      <TextField
        label={t("players.card.number")}
        value={cardNumber}
        onChange={(e) => setCardNumber(e.target.value)}
        required
        autoComplete="off"
      />
    </ActionForm>
  );
}

function CancelCardForm({ playerId, onDone }: { playerId: string; onDone: () => void }) {
  const { t } = useI18n();
  const change = usePlayerChange(playerId, onDone);
  return (
    <ActionForm
      title={t("players.card.cancel")}
      submitLabel={t("players.card.cancel")}
      pending={change.pending}
      error={change.error}
      succeeded={change.succeeded}
      onSubmit={(reasonCode) => change.run({ kind: "cancelCard", reasonCode })}
    />
  );
}

function DetailContent({ playerId }: { playerId: string }) {
  const { t } = useI18n();
  const { policy } = useAccess();
  const player = usePlayer(playerId);
  const view = player.data;
  return (
    <>
      <PageHeader
        title={t("players.detail.title", { number: view?.playerNumber ?? "…" })}
        actions={policy.canView(SCREEN_SEARCH) ? <Link href={routes.players}>{t("common.back")}</Link> : null}
      />
      <ProblemAlert error={player.error} />
      <Loading show={player.loading && !view} />
      {view ? (
        <>
          <Section>
            <ProfileSummary player={view} />
          </Section>
          <div className="columns">
            {view.activeCardNumber ? (
              <Can screen={SCREEN_PROFILE} action="cancel_card">
                <Section>
                  <CancelCardForm playerId={playerId} onDone={player.reload} />
                </Section>
              </Can>
            ) : null}
            <Can screen={SCREEN_PROFILE} action="issue_card">
              <Section>
                <IssueCardForm playerId={playerId} onDone={player.reload} />
              </Section>
            </Can>
          </div>
          {policy.canView(SCREEN_RESTRICTIONS) ? (
            <RestrictionsSection
              playerId={playerId}
              restrictions={view.activeRestrictions}
              onChanged={player.reload}
            />
          ) : null}
        </>
      ) : null}
    </>
  );
}

export function PlayerDetailPage({ playerId }: { playerId: string }) {
  return (
    <ScreenGuard screen={SCREEN_PROFILE}>
      <DetailContent playerId={playerId} />
    </ScreenGuard>
  );
}

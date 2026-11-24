"use client";

import Link from "next/link";
import { useState } from "react";
import type { ReactNode } from "react";
import { ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { formatDate } from "@/shared/format/format";
import { hasKey, useI18n } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { ActionForm } from "@/shared/ui/ActionForm";
import { Badge } from "@/shared/ui/Badge";
import { Icon } from "@/shared/ui/Icon";
import { TextField } from "@/shared/ui/form";
import { Loading, Page, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { TabItem } from "@/shared/ui/Tabs";
import { Tabs } from "@/shared/ui/Tabs";
import type { PlayerView } from "../api/types";
import { usePlayer, usePlayerChange } from "../hooks/usePlayers";
import { RestrictionsSection } from "./RestrictionsSection";
import { SCREEN_PROFILE, SCREEN_RESTRICTIONS, SCREEN_SEARCH } from "./screens";

function ProfileSummary({ player }: { player: PlayerView }) {
  const { t, locale } = useI18n();
  const documentKey = `players.documentType.${player.documentType}`;
  const rows: [string, ReactNode][] = [
    [t("players.number"), <code key="n">{player.playerNumber}</code>],
    [t("players.lastName"), player.lastName],
    [t("players.firstName"), player.firstName],
    [t("players.birthDate"), formatDate(locale, player.birthDate)],
    [t("players.nationality"), player.nationality],
    [t("players.documentType"), hasKey(documentKey) ? t(documentKey) : player.documentType],
    [t("players.documentNumber"), <code key="d">{player.maskedDocumentNumber}</code>],
    [t("players.phone"), player.phone ?? "—"],
    [t("players.card"), player.activeCardNumber ? <code key="c">{player.activeCardNumber}</code> : t("players.card.none")],
  ];
  return (
    <dl className="props">
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

  const tabs: TabItem[] = [];
  if (view) {
    const cardForms = [
      view.activeCardNumber && policy.can(SCREEN_PROFILE, "cancel_card") ? (
        <Section key="cancel">
          <CancelCardForm playerId={playerId} onDone={player.reload} />
        </Section>
      ) : null,
      policy.can(SCREEN_PROFILE, "issue_card") ? (
        <Section key="issue">
          <IssueCardForm playerId={playerId} onDone={player.reload} />
        </Section>
      ) : null,
    ].filter(Boolean);
    if (policy.canView(SCREEN_RESTRICTIONS)) {
      tabs.push({
        id: "restrictions",
        label: t("players.restrictions.title"),
        count: view.activeRestrictions.length,
        flush: true,
        content: <RestrictionsSection playerId={playerId} restrictions={view.activeRestrictions} onChanged={player.reload} />,
      });
    }
    if (cardForms.length > 0) {
      tabs.push({ id: "cards", label: t("players.cardOps"), content: <div className="columns">{cardForms}</div> });
    }
  }

  return (
    <Page
      title={t("players.detail.title", { number: view?.playerNumber ?? "…" })}
      meta={
        view ? (
          <>
            {view.lastName}, {view.firstName}{" "}
            {view.isRestricted ? (
              <Badge tone="restricted">
                <Icon name="restricted" /> {t("players.restricted")}
              </Badge>
            ) : null}
          </>
        ) : null
      }
      actions={
        policy.canView(SCREEN_SEARCH) ? (
          <Link className="btn" href={routes.players}>
            <Icon name="back" />
            {t("common.back")}
          </Link>
        ) : null
      }
    >
      <ProblemAlert error={player.error} />
      <Loading show={player.loading && !view} />
      {view ? (
        <>
          <Section title={t("players.profile")}>
            <ProfileSummary player={view} />
          </Section>
          {tabs.length > 0 ? (
            <section className="panel">
              <Tabs label={t("players.detail.title", { number: view.playerNumber })} tabs={tabs} />
            </section>
          ) : null}
        </>
      ) : null}
    </Page>
  );
}

export function PlayerDetailPage({ playerId }: { playerId: string }) {
  return (
    <ScreenGuard screen={SCREEN_PROFILE}>
      <DetailContent playerId={playerId} />
    </ScreenGuard>
  );
}

"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { Badge } from "@/shared/ui/Badge";
import { Button } from "@/shared/ui/Button";
import { DataGrid } from "@/shared/ui/DataGrid";
import { Dialog } from "@/shared/ui/Dialog";
import { Page, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { PlayerSummary } from "../api/types";
import { usePlayerSearch } from "../hooks/usePlayers";
import { RegisterPlayerForm } from "./RegisterPlayerForm";
import { SCREEN_PROFILE, SCREEN_SEARCH } from "./screens";

function SearchContent() {
  const t = useT();
  const router = useRouter();
  const { policy } = useAccess();
  const [input, setInput] = useState("");
  const [term, setTerm] = useState("");
  const [registering, setRegistering] = useState(false);
  const results = usePlayerSearch(term);
  const canOpen = policy.canView(SCREEN_PROFILE);
  const canCreate = policy.can(SCREEN_PROFILE, "create");

  return (
    <Page
      title={t("players.search.title")}
      actions={
        canCreate ? (
          <Button icon="personAdd" onClick={() => setRegistering(true)}>
            {t("players.new")}
          </Button>
        ) : null
      }
    >
      <Section>
        <form
          className="inline-form"
          role="search"
          onSubmit={(e) => {
            e.preventDefault();
            setTerm(input);
          }}
        >
          <input
            className="grow"
            type="search"
            aria-label={t("common.search")}
            placeholder={t("players.search.placeholder")}
            value={input}
            onChange={(e) => setInput(e.target.value)}
          />
          <Button type="submit" variant="primary" icon="search">
            {t("common.search")}
          </Button>
        </form>
      </Section>
      <ProblemAlert error={results.error} />
      {term ? (
        <Section
          title={t("players.results")}
          aside={results.data ? t("common.records", { count: results.data.length }) : null}
          flush
        >
          <DataGrid<PlayerSummary>
            label={t("players.results")}
            rows={results.data}
            loading={results.loading}
            rowKey={(p) => p.playerId}
            rowClassName={(p) => (p.isRestricted ? "restricted" : undefined)}
            columns={[
              {
                key: "number",
                header: t("players.number"),
                cell: (p) => (canOpen ? <Link href={routes.player(p.playerId)}>{p.playerNumber}</Link> : p.playerNumber),
              },
              { key: "lastName", header: t("players.lastName"), cell: (p) => p.lastName },
              { key: "firstName", header: t("players.firstName"), cell: (p) => p.firstName },
              { key: "card", header: t("players.card"), cell: (p) => (p.activeCardNumber ? <code>{p.activeCardNumber}</code> : "—") },
              {
                key: "status",
                header: t("common.status"),
                cell: (p) => (p.isRestricted ? <Badge tone="restricted">{t("players.restricted")}</Badge> : null),
              },
            ]}
          />
        </Section>
      ) : null}
      <Dialog open={registering} title={t("players.register")} onClose={() => setRegistering(false)}>
        <RegisterPlayerForm
          onCancel={() => setRegistering(false)}
          onRegistered={(r) => {
            setRegistering(false);
            router.push(routes.player(r.playerId));
          }}
        />
      </Dialog>
    </Page>
  );
}

export function PlayerSearchPage() {
  return (
    <ScreenGuard screen={SCREEN_SEARCH}>
      <SearchContent />
    </ScreenGuard>
  );
}

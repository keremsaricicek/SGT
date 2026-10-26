"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Can, ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { EmptyRow, Loading, PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import { usePlayerSearch } from "../hooks/usePlayers";
import { RegisterPlayerForm } from "./RegisterPlayerForm";
import { SCREEN_PROFILE, SCREEN_SEARCH } from "./screens";

function SearchContent() {
  const t = useT();
  const router = useRouter();
  const { policy } = useAccess();
  const [input, setInput] = useState("");
  const [term, setTerm] = useState("");
  const results = usePlayerSearch(term);
  const canOpen = policy.canView(SCREEN_PROFILE);

  return (
    <>
      <PageHeader title={t("players.search.title")} />
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
            aria-label={t("common.search")}
            placeholder={t("players.search.placeholder")}
            value={input}
            onChange={(e) => setInput(e.target.value)}
          />
          <button type="submit" className="btn primary">
            {t("common.search")}
          </button>
        </form>
        <ProblemAlert error={results.error} />
        <Loading show={results.loading} />
        {results.data ? (
          <table className="table">
            <thead>
              <tr>
                <th>{t("players.number")}</th>
                <th>{t("players.firstName")}</th>
                <th>{t("players.lastName")}</th>
                <th>{t("players.card")}</th>
                <th>{t("players.restricted")}</th>
              </tr>
            </thead>
            <tbody>
              {results.data.length === 0 ? <EmptyRow colSpan={5} /> : null}
              {results.data.map((p) => (
                <tr key={p.playerId} className={p.isRestricted ? "restricted" : undefined}>
                  <td>{canOpen ? <Link href={routes.player(p.playerId)}>{p.playerNumber}</Link> : p.playerNumber}</td>
                  <td>{p.firstName}</td>
                  <td>{p.lastName}</td>
                  <td>{p.activeCardNumber ?? "—"}</td>
                  <td>{p.isRestricted ? t("common.yes") : t("common.no")}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : null}
      </Section>
      <Can screen={SCREEN_PROFILE} action="create">
        <Section>
          <RegisterPlayerForm onRegistered={(r) => router.push(routes.player(r.playerId))} />
        </Section>
      </Can>
    </>
  );
}

export function PlayerSearchPage() {
  return (
    <ScreenGuard screen={SCREEN_SEARCH}>
      <SearchContent />
    </ScreenGuard>
  );
}

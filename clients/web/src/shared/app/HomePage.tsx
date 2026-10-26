"use client";

import Link from "next/link";
import { useAccess } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { screenLabel } from "@/shared/i18n/labels";
import { visibleNav } from "@/shared/navigation/routes";
import { Loading, PageHeader, Section } from "@/shared/ui/layout";

/** Ana sayfa: kullanıcının erişebildiği web ekranlarının kısa yolları. */
export function HomePage() {
  const t = useT();
  const { policy, loading } = useAccess();
  const entries = visibleNav((screen) => policy.canView(screen));
  return (
    <>
      <PageHeader title={t("app.home")} />
      <Section>
        <Loading show={loading} />
        {!loading && entries.length === 0 ? <p className="notice">{t("app.home.empty")}</p> : null}
        {entries.length > 0 ? (
          <>
            <p className="muted">{t("app.home.intro")}</p>
            <ul className="tiles">
              {entries.map((entry) => (
                <li key={entry.screen}>
                  <Link href={entry.href}>{screenLabel(t, entry.screen)}</Link>
                </li>
              ))}
            </ul>
          </>
        ) : null}
      </Section>
    </>
  );
}

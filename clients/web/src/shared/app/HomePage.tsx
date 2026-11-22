"use client";

import Link from "next/link";
import { useAccess } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { screenLabel } from "@/shared/i18n/labels";
import type { NavEntry } from "@/shared/navigation/routes";
import { visibleNav } from "@/shared/navigation/routes";
import { shortcutLabel } from "@/shared/navigation/shortcuts";
import { DataGrid } from "@/shared/ui/DataGrid";
import { Page, Section } from "@/shared/ui/layout";

/** Ana sayfa: kullanıcının erişebildiği ekranlar, kısayollarıyla birlikte tek listede. */
export function HomePage() {
  const t = useT();
  const { policy, loading } = useAccess();
  const entries = visibleNav((screen) => policy.canView(screen));
  return (
    <Page title={t("app.home")} meta={t("app.home.intro")}>
      {!loading && entries.length === 0 ? <p className="notice">{t("app.home.empty")}</p> : null}
      <Section flush>
        <DataGrid<NavEntry>
          label={t("app.home")}
          loading={loading}
          rows={loading ? undefined : entries}
          rowKey={(e) => e.screen}
          columns={[
            {
              key: "shortcut",
              header: t("app.home.shortcut"),
              className: "shortcut",
              cell: (e) => {
                const label = shortcutLabel(entries.indexOf(e));
                return label ? <kbd>{label}</kbd> : null;
              },
            },
            { key: "screen", header: t("app.screen"), cell: (e) => <Link href={e.href}>{screenLabel(t, e.screen)}</Link> },
            { key: "group", header: t("app.home.group"), cell: (e) => t(`nav.group.${e.group}`) },
            { key: "code", header: t("app.home.screenCode"), cell: (e) => <code>{e.screen}</code> },
          ]}
        />
      </Section>
    </Page>
  );
}

"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import type { ReactNode } from "react";
import { useAccess } from "@/shared/access/AccessProvider";
import { useAuth } from "@/shared/auth/AuthProvider";
import type { Locale } from "@/shared/i18n/I18nProvider";
import { LOCALES, useI18n } from "@/shared/i18n/I18nProvider";
import { screenLabel } from "@/shared/i18n/labels";
import { visibleNav } from "@/shared/navigation/routes";
import { useTerminal } from "@/shared/terminal/TerminalProvider";

function TerminalEditor() {
  const { t } = useI18n();
  const terminal = useTerminal();
  const [draft, setDraft] = useState<string | null>(null);
  const value = draft ?? terminal.name;
  return (
    <form
      className="terminal"
      onSubmit={(e) => {
        e.preventDefault();
        terminal.rename(value);
        setDraft(null);
      }}
    >
      <label>
        {t("app.terminal")}
        <input value={value} maxLength={64} onChange={(e) => setDraft(e.target.value)} />
      </label>
      {draft !== null ? (
        <button type="submit" className="btn small">
          {t("app.terminal.save")}
        </button>
      ) : null}
    </form>
  );
}

function LanguageSwitch() {
  const { t, locale, setLocale } = useI18n();
  return (
    <label className="lang">
      {t("app.language")}
      <select value={locale} onChange={(e) => setLocale(e.target.value as Locale)}>
        {LOCALES.map((l) => (
          <option key={l} value={l}>
            {l.toUpperCase()}
          </option>
        ))}
      </select>
    </label>
  );
}

function SideNav() {
  const { t } = useI18n();
  const { policy } = useAccess();
  const pathname = usePathname();
  const entries = visibleNav((screen) => policy.canView(screen));
  return (
    <nav className="sidenav" aria-label="menu">
      <Link href="/" className={pathname === "/" ? "active" : undefined}>
        {t("app.home")}
      </Link>
      {entries.map((entry) => (
        <Link key={entry.screen} href={entry.href} className={pathname.startsWith(entry.href) ? "active" : undefined}>
          {screenLabel(t, entry.screen)}
        </Link>
      ))}
    </nav>
  );
}

/** Panel kabuğu: üst çubuk (kullanıcı, terminal, dil) ve menüye göre oluşan yan menü. */
export function AppShell({ children }: { children: ReactNode }) {
  const { t } = useI18n();
  const auth = useAuth();
  return (
    <div className="shell">
      <header className="topbar">
        <Link href="/" className="brand">
          {t("app.title")}
        </Link>
        <div className="topbar-right">
          <TerminalEditor />
          <LanguageSwitch />
          <span className="user">{auth.userName}</span>
          <button type="button" className="btn small" onClick={() => void auth.logout()}>
            {t("auth.logout")}
          </button>
        </div>
      </header>
      <SideNav />
      <main className="content">{children}</main>
    </div>
  );
}

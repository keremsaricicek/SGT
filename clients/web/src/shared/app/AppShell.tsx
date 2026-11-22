"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { useAccess } from "@/shared/access/AccessProvider";
import { useAuth } from "@/shared/auth/AuthProvider";
import type { Locale } from "@/shared/i18n/I18nProvider";
import { LOCALES, useI18n } from "@/shared/i18n/I18nProvider";
import { screenLabel } from "@/shared/i18n/labels";
import type { NavEntry } from "@/shared/navigation/routes";
import { NAV_GROUPS, visibleNav } from "@/shared/navigation/routes";
import { navShortcutIndex, shortcutLabel } from "@/shared/navigation/shortcuts";
import { useTerminal } from "@/shared/terminal/TerminalProvider";
import { useTheme } from "@/shared/theme/ThemeProvider";
import { Button } from "@/shared/ui/Button";
import { Icon } from "@/shared/ui/Icon";

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
        <Icon name="terminal" />
        {t("app.terminal")}
        <input value={value} maxLength={64} onChange={(e) => setDraft(e.target.value)} />
      </label>
      {draft !== null ? (
        <Button type="submit" size="small">
          {t("app.terminal.save")}
        </Button>
      ) : null}
    </form>
  );
}

function isActive(pathname: string, href: string): boolean {
  return href === "/" ? pathname === "/" : pathname === href || pathname.startsWith(`${href}/`);
}

/** Alt+1…9 ile menüdeki ekranı açar (Spec 013 KK10). */
function useNavShortcuts(entries: readonly NavEntry[]) {
  const router = useRouter();
  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      const index = navShortcutIndex(event, entries.length);
      const entry = index === null ? undefined : entries[index];
      if (!entry) return;
      event.preventDefault();
      router.push(entry.href);
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [entries, router]);
}

function SideNav({ entries }: { entries: readonly NavEntry[] }) {
  const { t } = useI18n();
  const pathname = usePathname();
  const current = (href: string) => (isActive(pathname, href) ? "page" : undefined);
  return (
    <nav className="sidenav" aria-label="menu">
      <div className="nav-group">
        <Link href="/" aria-current={current("/")}>
          <span className="nav-label">
            <Icon name="home" />
            <span className="nav-text">{t("app.home")}</span>
          </span>
        </Link>
      </div>
      {NAV_GROUPS.map((group) => {
        const items = entries.filter((e) => e.group === group);
        if (items.length === 0) return null;
        return (
          <div key={group} className="nav-group" role="group" aria-labelledby={`nav-${group}`}>
            <div className="nav-caption" id={`nav-${group}`}>
              {t(`nav.group.${group}`)}
            </div>
            {items.map((entry) => {
              const shortcut = shortcutLabel(entries.indexOf(entry));
              return (
                <Link
                  key={entry.screen}
                  href={entry.href}
                  aria-current={current(entry.href)}
                  aria-keyshortcuts={shortcut ?? undefined}
                  title={screenLabel(t, entry.screen)}
                >
                  <span className="nav-label">
                    <Icon name={entry.icon} />
                    <span className="nav-text">{screenLabel(t, entry.screen)}</span>
                  </span>
                  {shortcut ? <kbd>{shortcut}</kbd> : null}
                </Link>
              );
            })}
          </div>
        );
      })}
    </nav>
  );
}

function StatusBar({ entries }: { entries: readonly NavEntry[] }) {
  const { t, locale, setLocale } = useI18n();
  const { theme, toggle } = useTheme();
  const { loading } = useAccess();
  const auth = useAuth();
  const terminal = useTerminal();
  const pathname = usePathname();
  const screen = entries.find((e) => isActive(pathname, e.href))?.screen;
  return (
    <footer className="statusbar">
      <span className={`status-cell ${loading ? "status-busy" : "status-ready"}`} role="status">
        {loading ? t("app.busy") : t("app.ready")}
      </span>
      <span className="status-cell grow">
        <span className="label">{t("app.screen")}</span>
        <code>{screen ?? "—"}</code>
      </span>
      <span className="status-cell optional">
        <span className="label">{t("app.terminal")}</span>
        {terminal.name || "—"}
      </span>
      <span className="status-cell optional">
        <span className="label">{t("app.user")}</span>
        {auth.userName ?? "—"}
      </span>
      <label className="status-cell">
        <span className="label">{t("app.language")}</span>
        <select value={locale} onChange={(e) => setLocale(e.target.value as Locale)}>
          {LOCALES.map((l) => (
            <option key={l} value={l}>
              {l.toUpperCase()}
            </option>
          ))}
        </select>
      </label>
      <span className="status-cell">
        <span className="label">{t("app.theme")}</span>
        <Button size="small" icon={theme === "dark" ? "dark" : "light"} onClick={toggle} title={t("app.theme.switch")}>
          {theme === "dark" ? t("app.theme.dark") : t("app.theme.light")}
        </Button>
      </span>
      <span className="status-cell optional muted">{t("app.shortcuts")}</span>
    </footer>
  );
}

/** Panel kabuğu (Spec 013 KK4): uygulama çubuğu, gruplu yan menü, çalışma alanı ve durum çubuğu. */
export function AppShell({ children }: { children: ReactNode }) {
  const { t } = useI18n();
  const auth = useAuth();
  const { policy } = useAccess();
  const entries = visibleNav((screen) => policy.canView(screen));
  useNavShortcuts(entries);
  return (
    <div className="shell">
      <a className="skip-link" href="#workspace">
        {t("app.skipToContent")}
      </a>
      <header className="titlebar">
        <Link href="/" className="brand">
          <span className="brand-mark">SGT</span>
          <span className="brand-name">{t("app.product")}</span>
        </Link>
        <span className="titlebar-spacer" />
        <div className="titlebar-group">
          <TerminalEditor />
          <span className="titlebar-user">
            {auth.userName}
            <Button size="small" icon="signOut" onClick={() => void auth.logout()}>
              {t("auth.logout")}
            </Button>
          </span>
        </div>
      </header>
      <SideNav entries={entries} />
      <main className="workspace" id="workspace" tabIndex={-1}>{children}</main>
      <StatusBar entries={entries} />
    </div>
  );
}

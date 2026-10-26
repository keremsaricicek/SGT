"use client";

import { usePathname } from "next/navigation";
import { useMemo } from "react";
import type { ReactNode } from "react";
import { AccessProvider } from "@/shared/access/AccessProvider";
import type { ApiContextOptions } from "@/shared/api/client";
import { ApiProvider } from "@/shared/api/ApiProvider";
import { useAuth } from "@/shared/auth/AuthProvider";
import { missingAuthConfig, readConfig } from "@/shared/config/env";
import { useT } from "@/shared/i18n/I18nProvider";
import { useTerminal } from "@/shared/terminal/TerminalProvider";
import { AppShell } from "./AppShell";

const CONFIG = readConfig();
const MISSING = missingAuthConfig(CONFIG);

/** Giriş yapılmamışsa giriş ekranı; yapılmışsa API, menü ve kabuk. */
export function PanelProviders({ children }: { children: ReactNode }) {
  const auth = useAuth();
  const t = useT();
  const pathname = usePathname();
  const { store } = useTerminal();
  const { getAccessToken } = auth;

  const apiOptions = useMemo<ApiContextOptions>(
    () => ({ baseUrl: CONFIG.apiBaseUrl, getAccessToken, getTerminal: store.get }),
    [getAccessToken, store],
  );

  if (MISSING.length > 0) {
    return (
      <main className="center">
        <p className="alert">{t("auth.configMissing", { names: MISSING.join(", ") })}</p>
      </main>
    );
  }

  if (auth.status === "loading") {
    return (
      <main className="center">
        <p className="muted">{t("app.loading")}</p>
      </main>
    );
  }

  if (auth.status === "anonymous") {
    return (
      <main className="center login">
        <h1>{t("app.title")}</h1>
        <p>{t("auth.loginRequired")}</p>
        {auth.error ? <p className="alert">{auth.error}</p> : null}
        <button type="button" className="btn primary" onClick={() => void auth.login(pathname)}>
          {t("auth.login")}
        </button>
      </main>
    );
  }

  return (
    <ApiProvider options={apiOptions}>
      <AccessProvider>
        <AppShell>{children}</AppShell>
      </AccessProvider>
    </ApiProvider>
  );
}

"use client";

import { createContext, useContext, useMemo } from "react";
import type { ReactNode } from "react";
import { useApiQuery } from "@/shared/hooks/useApiQuery";
import { useT } from "@/shared/i18n/I18nProvider";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import { AccessPolicy, fetchMyMenu } from "./menu";

interface AccessContextValue {
  readonly policy: AccessPolicy;
  readonly loading: boolean;
  reload(): void;
}

const AccessContext = createContext<AccessContextValue | null>(null);

export function AccessProvider({ children }: { children: ReactNode }) {
  const menu = useApiQuery("me/menu", fetchMyMenu);
  const value = useMemo<AccessContextValue>(
    () => ({ policy: new AccessPolicy(menu.data ?? []), loading: menu.loading, reload: menu.reload }),
    [menu.data, menu.loading, menu.reload],
  );
  return (
    <AccessContext.Provider value={value}>
      <ProblemAlert error={menu.error} />
      {children}
    </AccessContext.Provider>
  );
}

export function useAccess(): AccessContextValue {
  const value = useContext(AccessContext);
  if (!value) throw new Error("useAccess, AccessProvider içinde kullanılmalı.");
  return value;
}

/** Eylem menüde yoksa çocukları göstermez. */
export function Can({ screen, action, children }: { screen: string; action: string; children: ReactNode }) {
  const { policy } = useAccess();
  return policy.can(screen, action) ? <>{children}</> : null;
}

/** Ekran menüde yoksa içerik yerine yetki mesajı gösterir. */
export function ScreenGuard({ screen, children }: { screen: string; children: ReactNode }) {
  const { policy, loading } = useAccess();
  const t = useT();
  if (loading) return <p className="muted">{t("app.loading")}</p>;
  if (!policy.canView(screen)) return <p className="notice">{t("app.forbidden")}</p>;
  return <>{children}</>;
}

export { AccessContext };

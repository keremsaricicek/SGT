"use client";

import type { ReactNode } from "react";
import { AuthProvider } from "@/shared/auth/AuthProvider";
import { readConfig } from "@/shared/config/env";
import { I18nProvider } from "@/shared/i18n/I18nProvider";
import { TerminalProvider } from "@/shared/terminal/TerminalProvider";

/** Her sayfada gerekli sağlayıcılar: dil, kimlik doğrulama ve terminal adı. */
export function RootProviders({ children }: { children: ReactNode }) {
  return (
    <I18nProvider>
      <AuthProvider>
        <TerminalProvider defaultName={readConfig().defaultTerminal}>{children}</TerminalProvider>
      </AuthProvider>
    </I18nProvider>
  );
}

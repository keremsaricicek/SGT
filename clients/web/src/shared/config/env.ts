/**
 * Tarayıcıya açık yapılandırma. Burada yalnızca gizli olmayan değerler bulunur (CLAUDE.md: istemcide secret yok).
 * NEXT_PUBLIC_* değişkenleri derleme anında koda gömülür; değerler .env.local veya ortamdan gelir.
 */
export interface AppConfig {
  readonly oidcAuthority: string;
  readonly oidcClientId: string;
  readonly oidcScope: string;
  /** Boşsa API aynı kökendedir (ör. SGT_API_PROXY_TARGET ile). */
  readonly apiBaseUrl: string;
  readonly defaultTerminal: string;
}

export function readConfig(): AppConfig {
  return {
    oidcAuthority: process.env.NEXT_PUBLIC_OIDC_AUTHORITY ?? "",
    oidcClientId: process.env.NEXT_PUBLIC_OIDC_CLIENT_ID ?? "",
    oidcScope: process.env.NEXT_PUBLIC_OIDC_SCOPE ?? "openid profile",
    apiBaseUrl: (process.env.NEXT_PUBLIC_API_BASE_URL ?? "").replace(/\/+$/, ""),
    defaultTerminal: process.env.NEXT_PUBLIC_DEFAULT_TERMINAL ?? "",
  };
}

export function missingAuthConfig(config: AppConfig): string[] {
  const missing: string[] = [];
  if (!config.oidcAuthority) missing.push("NEXT_PUBLIC_OIDC_AUTHORITY");
  if (!config.oidcClientId) missing.push("NEXT_PUBLIC_OIDC_CLIENT_ID");
  return missing;
}

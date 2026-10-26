import { UserManager, WebStorageStateStore } from "oidc-client-ts";
import type { AppConfig } from "@/shared/config/env";

export const CALLBACK_PATH = "/auth/callback";

/**
 * Keycloak'a OIDC Authorization Code + PKCE ile bağlanır (ADR-0003). İstemci "public client"tır; secret yoktur.
 * Token'lar sessionStorage'da tutulur: sekme kapanınca oturum biter.
 */
export function createUserManager(config: AppConfig, origin: string): UserManager {
  return new UserManager({
    authority: config.oidcAuthority,
    client_id: config.oidcClientId,
    redirect_uri: `${origin}${CALLBACK_PATH}`,
    post_logout_redirect_uri: `${origin}/`,
    response_type: "code",
    scope: config.oidcScope,
    automaticSilentRenew: true,
    loadUserInfo: false,
    userStore: new WebStorageStateStore({ store: window.sessionStorage }),
  });
}

/** Giriş sonrası dönülecek yolu yalnızca aynı uygulama içi yol ise kabul eder (açık yönlendirme önlemi). */
export function safeReturnPath(value: unknown): string {
  if (typeof value !== "string" || !value.startsWith("/") || value.startsWith("//")) return "/";
  return value;
}

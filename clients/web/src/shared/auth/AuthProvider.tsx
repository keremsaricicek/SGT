"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import type { User, UserManager } from "oidc-client-ts";
import { readConfig } from "@/shared/config/env";
import { createUserManager, safeReturnPath } from "./oidc";

export type AuthStatus = "loading" | "anonymous" | "authenticated";

export interface AuthContextValue {
  readonly status: AuthStatus;
  readonly userName: string | null;
  readonly error: string | null;
  login(returnPath?: string): Promise<void>;
  logout(): Promise<void>;
  /** Yönlendirme dönüşünü işler ve dönülecek yolu verir. */
  completeLogin(): Promise<string>;
  getAccessToken(): Promise<string | null>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function displayName(user: User | null): string | null {
  if (!user) return null;
  const profile = user.profile;
  return profile.name ?? profile.preferred_username ?? profile.sub;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [manager] = useState<UserManager | null>(() =>
    typeof window === "undefined" ? null : createUserManager(readConfig(), window.location.origin),
  );
  const [user, setUser] = useState<User | null>(null);
  const [status, setStatus] = useState<AuthStatus>("loading");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!manager) return;
    let active = true;
    manager
      .getUser()
      .then((loaded) => {
        if (!active) return;
        const valid = loaded && !loaded.expired ? loaded : null;
        setUser(valid);
        setStatus(valid ? "authenticated" : "anonymous");
      })
      .catch((e: unknown) => {
        if (!active) return;
        setError(e instanceof Error ? e.message : String(e));
        setStatus("anonymous");
      });

    const onLoaded = (loaded: User) => {
      setUser(loaded);
      setStatus("authenticated");
    };
    const onUnloaded = () => {
      setUser(null);
      setStatus("anonymous");
    };
    manager.events.addUserLoaded(onLoaded);
    manager.events.addUserUnloaded(onUnloaded);
    manager.events.addAccessTokenExpired(onUnloaded);
    return () => {
      active = false;
      manager.events.removeUserLoaded(onLoaded);
      manager.events.removeUserUnloaded(onUnloaded);
      manager.events.removeAccessTokenExpired(onUnloaded);
    };
  }, [manager]);

  const login = useCallback(
    async (returnPath?: string) => {
      if (!manager) return;
      await manager.signinRedirect({ state: { returnPath: safeReturnPath(returnPath ?? "/") } });
    },
    [manager],
  );

  const logout = useCallback(async () => {
    if (!manager) return;
    await manager.signoutRedirect();
  }, [manager]);

  const completeLogin = useCallback(async () => {
    if (!manager) return "/";
    const signedIn = await manager.signinRedirectCallback();
    setUser(signedIn);
    setStatus("authenticated");
    const state = signedIn.state as { returnPath?: unknown } | undefined;
    return safeReturnPath(state?.returnPath);
  }, [manager]);

  const getAccessToken = useCallback(async () => {
    if (!manager) return null;
    const current = await manager.getUser();
    return current && !current.expired ? current.access_token : null;
  }, [manager]);

  const value = useMemo<AuthContextValue>(
    () => ({ status, userName: displayName(user), error, login, logout, completeLogin, getAccessToken }),
    [status, user, error, login, logout, completeLogin, getAccessToken],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth, AuthProvider içinde kullanılmalı.");
  return value;
}

export { AuthContext };

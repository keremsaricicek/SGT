"use client";

import { useCallback, useSyncExternalStore } from "react";
import type { Theme } from "./theme";
import { DEFAULT_THEME, parseTheme, storeTheme } from "./theme";

const listeners = new Set<() => void>();

function subscribe(listener: () => void) {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

function current(): Theme {
  return parseTheme(document.documentElement.getAttribute("data-theme"));
}

function storage(): Storage | undefined {
  try {
    return window.localStorage;
  } catch {
    return undefined;
  }
}

/** Etkin tema ve değiştirici. Tema <html data-theme> üzerinde tutulur; ilk değer satır içi betikle gelir. */
export function useTheme(): { theme: Theme; setTheme(theme: Theme): void; toggle(): void } {
  const theme = useSyncExternalStore(subscribe, current, () => DEFAULT_THEME);
  const setTheme = useCallback((next: Theme) => {
    document.documentElement.setAttribute("data-theme", next);
    storeTheme(storage(), next);
    listeners.forEach((l) => l());
  }, []);
  const toggle = useCallback(() => setTheme(current() === "dark" ? "light" : "dark"), [setTheme]);
  return { theme, setTheme, toggle };
}

"use client";

import { createContext, useContext, useMemo, useState, useSyncExternalStore } from "react";
import type { ReactNode } from "react";
import type { TerminalStore } from "./terminal";
import { createTerminalStore } from "./terminal";

interface TerminalContextValue {
  readonly store: TerminalStore;
  /** Sunucu tarafında boştur; tarayıcıda localStorage'dan okunur. */
  readonly name: string;
  rename(name: string): void;
}

const TerminalContext = createContext<TerminalContextValue | null>(null);

export function TerminalProvider({ defaultName, children }: { defaultName: string; children: ReactNode }) {
  const [store] = useState(() => createTerminalStore(defaultName));
  const name = useSyncExternalStore(store.subscribe, store.get, () => "");
  const value = useMemo(() => ({ store, name, rename: (next: string) => void store.set(next) }), [store, name]);
  return <TerminalContext.Provider value={value}>{children}</TerminalContext.Provider>;
}

export function useTerminal(): TerminalContextValue {
  const value = useContext(TerminalContext);
  if (!value) throw new Error("useTerminal, TerminalProvider içinde kullanılmalı.");
  return value;
}

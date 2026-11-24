/**
 * Tarayıcı terminal adı (X-Terminal başlığı, ADR-0003). Önce localStorage, yoksa yapılandırma,
 * o da yoksa bu tarayıcıya özel üretilen "WEB-XXXXXX" adı kullanılır ve saklanır.
 */
export const TERMINAL_STORAGE_KEY = "sgt.terminal";
const MAX_LENGTH = 64;

export interface TerminalStore {
  readonly get: () => string;
  readonly set: (name: string) => string;
  readonly subscribe: (listener: () => void) => () => void;
}

function safeStorage(): Storage | null {
  try {
    return typeof window === "undefined" ? null : window.localStorage;
  } catch {
    return null;
  }
}

export function normalizeTerminalName(name: string): string {
  return name
    .trim()
    .replace(/[^\p{L}\p{N}._-]+/gu, "-")
    .slice(0, MAX_LENGTH);
}

function generatedName(): string {
  const random = crypto.randomUUID().replace(/-/g, "").slice(0, 6).toUpperCase();
  return `WEB-${random}`;
}

export function createTerminalStore(defaultName: string, storage: Storage | null = safeStorage()): TerminalStore {
  let current = "";
  const listeners = new Set<() => void>();

  const read = (): string => {
    try {
      return storage?.getItem(TERMINAL_STORAGE_KEY) ?? "";
    } catch {
      return "";
    }
  };

  const set = (name: string): string => {
    const normalized = normalizeTerminalName(name) || generatedName();
    current = normalized;
    try {
      storage?.setItem(TERMINAL_STORAGE_KEY, normalized);
    } catch {
      // Depolama kapalıysa ad yalnızca bu oturum için bellekte kalır.
    }
    listeners.forEach((listener) => listener());
    return normalized;
  };

  const get = (): string => {
    if (current) return current;
    const stored = normalizeTerminalName(read());
    if (stored) {
      current = stored;
      return current;
    }
    current = normalizeTerminalName(defaultName) || generatedName();
    try {
      storage?.setItem(TERMINAL_STORAGE_KEY, current);
    } catch {
      // yoksay
    }
    return current;
  };

  const subscribe = (listener: () => void) => {
    listeners.add(listener);
    return () => listeners.delete(listener);
  };

  return { get, set, subscribe };
}

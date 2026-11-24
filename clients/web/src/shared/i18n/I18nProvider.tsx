"use client";

import { createContext, useContext, useMemo, useSyncExternalStore } from "react";
import type { ReactNode } from "react";
import { en } from "./en";
import type { MessageKey } from "./tr";
import { tr } from "./tr";

export type Locale = "tr" | "en";
export const LOCALES: readonly Locale[] = ["tr", "en"];
const STORAGE_KEY = "sgt.locale";
const dictionaries: Record<Locale, Record<MessageKey, string>> = { tr, en };

export type Translate = (key: MessageKey, params?: Record<string, string | number>) => string;

export function translate(locale: Locale, key: MessageKey, params?: Record<string, string | number>): string {
  const template = dictionaries[locale][key] ?? tr[key] ?? key;
  if (!params) return template;
  return template.replace(/\{(\w+)\}/g, (match, name: string) => (name in params ? String(params[name]) : match));
}

/** Sözlükte olmayan dinamik anahtarlar (ör. yeni bir ekran kodu) için: yoksa yedek metni döner. */
export function hasKey(key: string): key is MessageKey {
  return key in tr;
}

interface I18nContextValue {
  readonly locale: Locale;
  readonly t: Translate;
  setLocale(locale: Locale): void;
}

const I18nContext = createContext<I18nContextValue | null>(null);
const listeners = new Set<() => void>();

function readLocale(): Locale {
  try {
    const stored = window.localStorage.getItem(STORAGE_KEY);
    return stored === "en" ? "en" : "tr";
  } catch {
    return "tr";
  }
}

function writeLocale(locale: Locale) {
  try {
    window.localStorage.setItem(STORAGE_KEY, locale);
  } catch {
    // yoksay
  }
  document.documentElement.lang = locale;
  listeners.forEach((l) => l());
}

function subscribe(listener: () => void) {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

export function I18nProvider({ children, initialLocale }: { children: ReactNode; initialLocale?: Locale }) {
  const stored = useSyncExternalStore(subscribe, readLocale, () => "tr" as Locale);
  const locale = initialLocale ?? stored;
  const value = useMemo<I18nContextValue>(
    () => ({ locale, t: (key, params) => translate(locale, key, params), setLocale: writeLocale }),
    [locale],
  );
  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n(): I18nContextValue {
  const value = useContext(I18nContext);
  if (!value) throw new Error("useI18n, I18nProvider içinde kullanılmalı.");
  return value;
}

export function useT(): Translate {
  return useI18n().t;
}

/**
 * Menü kısayolları (Spec 013 KK10): Alt+1…Alt+9 menüdeki sıradaki ekranı açar. Tarayıcının ayırdığı tuşlar
 * (F5, Ctrl+R vb.) kullanılmaz. Fiziksel tuş (event.code) okunur; böylece klavye düzeninden bağımsızdır.
 */
export interface ShortcutKey {
  readonly altKey: boolean;
  readonly ctrlKey: boolean;
  readonly metaKey: boolean;
  readonly shiftKey: boolean;
  readonly code: string;
}

export const MAX_NAV_SHORTCUTS = 9;

/** Kısayolun açacağı menü sırasını (0'dan) döner; kısayol değilse ya da öğe yoksa null. */
export function navShortcutIndex(key: ShortcutKey, entryCount: number): number | null {
  if (!key.altKey || key.ctrlKey || key.metaKey || key.shiftKey) return null;
  const match = /^(?:Digit|Numpad)([1-9])$/.exec(key.code);
  if (!match) return null;
  const index = Number(match[1]) - 1;
  return index < Math.min(entryCount, MAX_NAV_SHORTCUTS) ? index : null;
}

export function shortcutLabel(index: number): string | null {
  return index < MAX_NAV_SHORTCUTS ? `Alt+${index + 1}` : null;
}

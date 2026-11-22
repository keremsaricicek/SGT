/**
 * Tema tercihi (Spec 013 KK6): koyu varsayılan, açık seçilebilir; seçim tarayıcıda saklanır ve
 * <html data-theme="..."> özniteliğiyle uygulanır. Renkler design/tokens/sgt-tokens.json'dan üretilir.
 */
export type Theme = "dark" | "light";
export const THEMES: readonly Theme[] = ["dark", "light"];
export const DEFAULT_THEME: Theme = "dark";
export const THEME_STORAGE_KEY = "sgt.theme";

export function parseTheme(value: unknown): Theme {
  return value === "light" ? "light" : DEFAULT_THEME;
}

/** Saklanan tercihi okur; depolama kapalıysa (gizli pencere) varsayılanı döner. */
export function readStoredTheme(storage: Pick<Storage, "getItem"> | undefined): Theme {
  try {
    return parseTheme(storage?.getItem(THEME_STORAGE_KEY));
  } catch {
    return DEFAULT_THEME;
  }
}

export function storeTheme(storage: Pick<Storage, "setItem"> | undefined, theme: Theme): void {
  try {
    storage?.setItem(THEME_STORAGE_KEY, theme);
  } catch {
    // depolama kapalı: seçim yalnızca bu oturumda geçerli kalır
  }
}

/**
 * İlk çizimden önce çalışan satır içi betik: yanlış temada yanıp sönmeyi önler (KK6).
 * Dışarıdan veri almaz; yalnızca sabit anahtarı okur.
 */
export const THEME_INIT_SCRIPT = `(function(){var t="${DEFAULT_THEME}";try{if(localStorage.getItem("${THEME_STORAGE_KEY}")==="light")t="light"}catch(e){}document.documentElement.setAttribute("data-theme",t)})();`;

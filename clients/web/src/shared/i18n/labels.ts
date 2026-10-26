import type { Translate } from "./I18nProvider";
import { hasKey } from "./I18nProvider";

/** Ekran kodunun görünen adı; sözlükte yoksa kodun kendisi gösterilir. */
export function screenLabel(t: Translate, screen: string): string {
  const key = `screen.${screen}`;
  return hasKey(key) ? t(key) : screen;
}

export function actionLabel(t: Translate, action: string): string {
  const key = `action.${action}`;
  return hasKey(key) ? t(key) : action;
}

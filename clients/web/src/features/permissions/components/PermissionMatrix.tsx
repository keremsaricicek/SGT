"use client";

import { useT } from "@/shared/i18n/I18nProvider";
import { actionLabel, screenLabel } from "@/shared/i18n/labels";
import type { ScreenView } from "../api/types";
import { permissionCode } from "../api/types";

interface PermissionMatrixProps {
  readonly screens: readonly ScreenView[];
  readonly granted: ReadonlySet<string>;
  readonly editable: boolean;
  readonly busy: boolean;
  readonly onToggle: (permission: string, grant: boolean) => void;
}

/** Ekran × eylem tablosu; her kutu bir yetkiyi (ekran:eylem) gösterir. */
export function PermissionMatrix({ screens, granted, editable, busy, onToggle }: PermissionMatrixProps) {
  const t = useT();
  return (
    <table className="table matrix">
      <thead>
        <tr>
          <th>{t("permissions.screen")}</th>
          <th>{t("permissions.product")}</th>
          <th>{t("permissions.actions")}</th>
        </tr>
      </thead>
      <tbody>
        {screens.map((screen) => (
          <tr key={screen.code}>
            <td>
              {screenLabel(t, screen.code)} <code className="muted">{screen.code}</code>
            </td>
            <td>{screen.product}</td>
            <td className="checks">
              {screen.actions.map((action) => {
                const code = permissionCode(screen.code, action);
                const checked = granted.has(code);
                return (
                  <label key={code} className="check">
                    <input
                      type="checkbox"
                      checked={checked}
                      disabled={!editable || busy}
                      onChange={() => onToggle(code, !checked)}
                      aria-label={code}
                    />
                    {actionLabel(t, action)}
                  </label>
                );
              })}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

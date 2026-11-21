"use client";

import { useT } from "@/shared/i18n/I18nProvider";
import { actionLabel, screenLabel } from "@/shared/i18n/labels";
import { Badge } from "@/shared/ui/Badge";
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
    <table className="grid matrix">
      <thead>
        <tr>
          <th scope="col">{t("permissions.screen")}</th>
          <th scope="col">{t("permissions.product")}</th>
          <th scope="col">{t("permissions.actions")}</th>
        </tr>
      </thead>
      <tbody>
        {screens.map((screen) => (
          <tr key={screen.code}>
            <td>
              {screenLabel(t, screen.code)} <code>{screen.code}</code>
            </td>
            <td>
              <Badge>{screen.product}</Badge>
            </td>
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

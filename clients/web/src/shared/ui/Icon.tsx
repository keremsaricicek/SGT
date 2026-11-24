import type { IconName } from "./icons.generated";
import { ICON_PATHS } from "./icons.generated";

export type { IconName };

/**
 * Fluent UI System Icons (Spec 013): metin rengini alır (currentColor), varsayılan 16px.
 * Simge yalnızca görseldir; anlamı her zaman yanındaki metin ya da aria-label taşır.
 */
export function Icon({ name, size = 16 }: { name: IconName; size?: 16 | 20 }) {
  return (
    <svg className="icon" width={size} height={size} viewBox="0 0 20 20" fill="currentColor" aria-hidden="true" focusable="false">
      {ICON_PATHS[name].map((d) => (
        <path key={d} d={d} />
      ))}
    </svg>
  );
}

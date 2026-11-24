import type { ButtonHTMLAttributes } from "react";
import type { IconName } from "./Icon";
import { Icon } from "./Icon";

export type ButtonVariant = "default" | "primary" | "danger" | "ghost";
export type ButtonSize = "medium" | "small";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  readonly variant?: ButtonVariant;
  readonly size?: ButtonSize;
  /** Metnin solundaki Fluent simgesi (yalnızca görsel). */
  readonly icon?: IconName;
}

/** Düz, köşesi en çok 2px düğme (Spec 013 KK5). Varsayılan tür "button"dır; form göndermek için type="submit". */
export function Button({ variant = "default", size = "medium", icon, className, type = "button", children, ...rest }: ButtonProps) {
  const classes = ["btn", variant === "default" ? null : variant, size === "small" ? "small" : null, className]
    .filter(Boolean)
    .join(" ");
  return (
    <button type={type} className={classes} {...rest}>
      {icon ? <Icon name={icon} /> : null}
      {children}
    </button>
  );
}

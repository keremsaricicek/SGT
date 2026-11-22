"use client";

import { useEffect, useId, useRef } from "react";
import type { ReactNode } from "react";
import { useT } from "@/shared/i18n/I18nProvider";

interface DialogProps {
  readonly open: boolean;
  readonly title: string;
  readonly onClose: () => void;
  readonly children: ReactNode;
}

/**
 * Modal diyalog (Spec 013 KK5): tarayıcının <dialog> öğesi; odak tuzağı ve Esc ile kapanma yerleşiktir,
 * kapanınca odak açan öğeye döner.
 */
export function Dialog({ open, title, onClose, children }: DialogProps) {
  const t = useT();
  const ref = useRef<HTMLDialogElement>(null);
  const titleId = useId();

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) {
      if (typeof dialog.showModal === "function") dialog.showModal();
      else dialog.setAttribute("open", "");
    } else if (!open && dialog.open) {
      if (typeof dialog.close === "function") dialog.close();
      else dialog.removeAttribute("open");
    }
  }, [open]);

  return (
    <dialog ref={ref} className="dialog" aria-labelledby={titleId} onClose={onClose} onCancel={onClose}>
      {open ? (
        <>
          <header className="dialog-head">
            <h2 id={titleId}>{title}</h2>
            <button type="button" className="btn ghost small" onClick={onClose} aria-label={t("common.close")}>
              {t("common.close")} <kbd>Esc</kbd>
            </button>
          </header>
          <div className="dialog-body">{children}</div>
        </>
      ) : null}
    </dialog>
  );
}

"use client";

import { useId, useRef, useState } from "react";
import type { KeyboardEvent, ReactNode } from "react";

export interface TabItem {
  readonly id: string;
  readonly label: string;
  readonly count?: number;
  readonly flush?: boolean;
  readonly content: ReactNode;
}

/** Sonraki sekmenin sırası: sol/sağ ok döngüsel, Home/End uçlar (Spec 013 KK10). */
export function nextTabIndex(key: string, index: number, count: number): number | null {
  switch (key) {
    case "ArrowRight":
      return (index + 1) % count;
    case "ArrowLeft":
      return (index - 1 + count) % count;
    case "Home":
      return 0;
    case "End":
      return count - 1;
    default:
      return null;
  }
}

/** Klasik sekme şeridi; WAI-ARIA tabs deseni (yalnızca seçili sekme Tab sırasındadır). */
export function Tabs({ label, tabs }: { label: string; tabs: readonly TabItem[] }) {
  const baseId = useId();
  const [selected, setSelected] = useState(0);
  const refs = useRef<(HTMLButtonElement | null)[]>([]);
  const index = Math.min(selected, tabs.length - 1);
  const active = tabs[index];

  const onKeyDown = (event: KeyboardEvent<HTMLButtonElement>) => {
    const next = nextTabIndex(event.key, index, tabs.length);
    if (next === null) return;
    event.preventDefault();
    setSelected(next);
    refs.current[next]?.focus();
  };

  return (
    <>
      <div role="tablist" aria-label={label} className="tabs">
        {tabs.map((tab, i) => (
          <button
            key={tab.id}
            ref={(el) => {
              refs.current[i] = el;
            }}
            type="button"
            role="tab"
            id={`${baseId}-tab-${tab.id}`}
            aria-controls={`${baseId}-panel-${tab.id}`}
            aria-selected={i === index}
            tabIndex={i === index ? 0 : -1}
            className="tab"
            onClick={() => setSelected(i)}
            onKeyDown={onKeyDown}
          >
            {tab.label}
            {tab.count !== undefined ? <span className="count">{tab.count}</span> : null}
          </button>
        ))}
      </div>
      {active ? (
        <div
          role="tabpanel"
          id={`${baseId}-panel-${active.id}`}
          aria-labelledby={`${baseId}-tab-${active.id}`}
          className={active.flush ? "tab-panel flush" : "tab-panel"}
        >
          {active.content}
        </div>
      ) : null}
    </>
  );
}

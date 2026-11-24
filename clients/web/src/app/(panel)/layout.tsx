import type { ReactNode } from "react";
import { PanelProviders } from "@/shared/app/PanelProviders";

export default function PanelLayout({ children }: { children: ReactNode }) {
  return <PanelProviders>{children}</PanelProviders>;
}

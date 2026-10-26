import type { Metadata, Viewport } from "next";
import type { ReactNode } from "react";
import { RootProviders } from "@/shared/app/RootProviders";
import "./globals.css";

export const metadata: Metadata = {
  title: "SGT Yönetim Paneli",
  description: "SGT web yönetim paneli",
  robots: { index: false, follow: false },
};

export const viewport: Viewport = { width: "device-width", initialScale: 1 };

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="tr">
      <body>
        <RootProviders>{children}</RootProviders>
      </body>
    </html>
  );
}

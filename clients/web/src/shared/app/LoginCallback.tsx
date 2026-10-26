"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { useAuth } from "@/shared/auth/AuthProvider";
import { useT } from "@/shared/i18n/I18nProvider";

/** Keycloak dönüşü: kodu token'a çevirir ve kullanıcıyı geldiği sayfaya yollar. */
export function LoginCallback() {
  const t = useT();
  const router = useRouter();
  const { completeLogin } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const started = useRef(false);

  useEffect(() => {
    // React StrictMode efekti iki kez çalıştırır; kod yalnızca bir kez kullanılabilir.
    if (started.current) return;
    started.current = true;
    completeLogin()
      .then((path) => router.replace(path))
      .catch((e: unknown) => setError(e instanceof Error ? e.message : String(e)));
  }, [completeLogin, router]);

  return (
    <main className="center">
      {error ? (
        <div className="alert" role="alert">
          <strong>{t("auth.failed")}: </strong>
          {error}
          <p>
            <Link href="/">{t("app.home")}</Link>
          </p>
        </div>
      ) : (
        <p className="muted">{t("auth.completing")}</p>
      )}
    </main>
  );
}

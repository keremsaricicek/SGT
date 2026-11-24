/**
 * Test yardımcıları: sahte fetch ve sağlayıcılar. Yalnızca sentetik veri kullanılır (gerçek kişi verisi yok).
 */
import type { ReactNode } from "react";
import { AccessContext } from "@/shared/access/AccessProvider";
import type { MenuItem } from "@/shared/access/menu";
import { AccessPolicy } from "@/shared/access/menu";
import type { ApiContextOptions, SgtApi } from "@/shared/api/client";
import { createSgtApi } from "@/shared/api/client";
import { ApiProvider } from "@/shared/api/ApiProvider";
import { I18nProvider } from "@/shared/i18n/I18nProvider";

export const TEST_BASE_URL = "http://api.test";
export const TEST_TOKEN = "test-token";
export const TEST_TERMINAL = "WEB-TEST";

export interface RecordedRequest {
  readonly method: string;
  readonly url: URL;
  readonly headers: Headers;
  readonly body: unknown;
}

export interface FakeRoute {
  readonly method: string;
  /** Yol (sorgu hariç), ör. "/api/v1/identity/roles". */
  readonly path: string;
  readonly status?: number;
  readonly body?: unknown;
}

export interface FakeFetch {
  readonly fetch: (input: Request) => Promise<Response>;
  readonly requests: RecordedRequest[];
}

const PROBLEM_TYPE = "application/problem+json";

export function fakeFetch(routes: readonly FakeRoute[]): FakeFetch {
  const requests: RecordedRequest[] = [];
  const fetch = async (request: Request): Promise<Response> => {
    const url = new URL(request.url);
    const text = await request.clone().text();
    requests.push({ method: request.method, url, headers: request.headers, body: text ? JSON.parse(text) : undefined });
    const route = routes.find((r) => r.method === request.method && r.path === decodeURIComponent(url.pathname));
    if (!route) return new Response(null, { status: 404 });
    const status = route.status ?? 200;
    if (route.body === undefined) return new Response(null, { status: status === 200 ? 204 : status });
    const contentType = status >= 400 ? PROBLEM_TYPE : "application/json";
    return new Response(JSON.stringify(route.body), { status, headers: { "Content-Type": contentType } });
  };
  return { fetch, requests };
}

export function testApiOptions(fetch: FakeFetch["fetch"]): ApiContextOptions {
  return {
    baseUrl: TEST_BASE_URL,
    getAccessToken: async () => TEST_TOKEN,
    getTerminal: () => TEST_TERMINAL,
    fetch,
  };
}

export function testApi(fetch: FakeFetch["fetch"], correlationId?: string): SgtApi {
  return createSgtApi(testApiOptions(fetch), correlationId);
}

export function problem(title: string, detail: string, status = 400) {
  return { type: "about:blank", title, detail, status, code: title };
}

/** Hook ve bileşen testleri için sağlayıcılar: dil (TR), API (sahte fetch) ve menü. */
export function testWrapper(fetch: FakeFetch["fetch"], menu: readonly MenuItem[] = []) {
  const options = testApiOptions(fetch);
  const access = { policy: new AccessPolicy(menu), loading: false, reload: () => {} };
  return function Wrapper({ children }: { children: ReactNode }) {
    return (
      <I18nProvider initialLocale="tr">
        <ApiProvider options={options}>
          <AccessContext.Provider value={access}>{children}</AccessContext.Provider>
        </ApiProvider>
      </I18nProvider>
    );
  };
}

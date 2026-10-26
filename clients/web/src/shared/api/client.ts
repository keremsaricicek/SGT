import createClient from "openapi-fetch";
import type { Client, Middleware } from "openapi-fetch";
import type { components, paths } from "./generated/schema";
import { ApiError } from "./problem";

/** OpenAPI sözleşmesinden (docs/api/openapi-v1.json) üretilmiş tipli istemci. */
export type SgtClient = Client<paths>;
export type Schemas = components["schemas"];

export const HEADER_TERMINAL = "X-Terminal";
export const HEADER_CORRELATION = "X-Correlation-Id";

export interface ApiContextOptions {
  readonly baseUrl: string;
  readonly getAccessToken: () => Promise<string | null>;
  readonly getTerminal: () => string;
  readonly fetch?: (input: Request) => Promise<Response>;
}

interface ApiCallResult {
  readonly data?: unknown;
  readonly error?: unknown;
  readonly response: Response;
}

/**
 * Tek bir kullanıcı eylemine bağlı API erişimi. Eylem içindeki tüm istekler aynı X-Correlation-Id'yi taşır.
 * Yanıt tipi çağıran serviste sözleşme şemasıyla (Schemas["..."]) belirtilir.
 */
export interface SgtApi {
  readonly client: SgtClient;
  readonly correlationId: string;
  /** Başarılıysa gövdeyi döner, değilse ApiError fırlatır. */
  data<T>(call: Promise<ApiCallResult>): Promise<T>;
  /** Gövdesi önemsiz (204/201) çağrılar için. */
  done(call: Promise<ApiCallResult>): Promise<void>;
}

export function newCorrelationId(): string {
  return crypto.randomUUID();
}

function headersMiddleware(options: ApiContextOptions, correlationId: string): Middleware {
  return {
    async onRequest({ request }) {
      const token = await options.getAccessToken();
      if (token) request.headers.set("Authorization", `Bearer ${token}`);
      request.headers.set(HEADER_TERMINAL, options.getTerminal());
      request.headers.set(HEADER_CORRELATION, correlationId);
      return request;
    },
  };
}

function resolveBaseUrl(baseUrl: string): string {
  if (baseUrl) return baseUrl;
  return typeof window === "undefined" ? "" : window.location.origin;
}

export function createSgtApi(options: ApiContextOptions, correlationId: string = newCorrelationId()): SgtApi {
  const client = createClient<paths>({ baseUrl: resolveBaseUrl(options.baseUrl), fetch: options.fetch });
  client.use(headersMiddleware(options, correlationId));

  const check = async (call: Promise<ApiCallResult>): Promise<unknown> => {
    const result = await call;
    if (!result.response.ok) throw ApiError.fromResponse(result.response, result.error, correlationId);
    return result.data;
  };

  return {
    client,
    correlationId,
    async data<T>(call: Promise<ApiCallResult>) {
      return (await check(call)) as T;
    },
    async done(call: Promise<ApiCallResult>) {
      await check(call);
    },
  };
}

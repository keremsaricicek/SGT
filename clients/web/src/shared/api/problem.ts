/** API'nin döndürdüğü RFC 9457 problem ayrıntısı (ResultHttpExtensions.Problem). */
export interface ProblemDetails {
  readonly type?: string;
  readonly title?: string;
  readonly detail?: string;
  readonly status?: number;
  readonly code?: string;
  readonly errors?: Record<string, string[]>;
}

/** Başarısız API çağrısı. Ekranda title/detail kullanıcıya gösterilir. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails;
  readonly correlationId: string | null;

  constructor(status: number, problem: ProblemDetails, correlationId: string | null) {
    super(problem.detail ?? problem.title ?? `HTTP ${status}`);
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
    this.correlationId = correlationId;
  }

  get title(): string | undefined {
    return this.problem.title;
  }

  get detail(): string | undefined {
    return this.problem.detail;
  }

  static fromResponse(response: Response, body: unknown, correlationId: string | null): ApiError {
    return new ApiError(response.status, toProblem(body, response), correlationId);
  }
}

function toProblem(body: unknown, response: Response): ProblemDetails {
  if (body && typeof body === "object") {
    const candidate = body as Record<string, unknown>;
    return {
      type: asString(candidate.type),
      title: asString(candidate.title),
      detail: asString(candidate.detail),
      status: typeof candidate.status === "number" ? candidate.status : response.status,
      code: asString(candidate.code),
      errors: isErrorMap(candidate.errors) ? candidate.errors : undefined,
    };
  }
  if (typeof body === "string" && body.trim()) {
    return { title: response.statusText || `HTTP ${response.status}`, detail: body, status: response.status };
  }
  return { title: response.statusText || `HTTP ${response.status}`, status: response.status };
}

function asString(value: unknown): string | undefined {
  return typeof value === "string" && value.length > 0 ? value : undefined;
}

function isErrorMap(value: unknown): value is Record<string, string[]> {
  return !!value && typeof value === "object" && Object.values(value).every((v) => Array.isArray(v));
}

/** Herhangi bir hatayı ekranda gösterilecek başlık/ayrıntıya çevirir. */
export function describeError(error: unknown): { title: string; detail?: string; correlationId?: string } {
  if (error instanceof ApiError) {
    return {
      title: error.title ?? `HTTP ${error.status}`,
      detail: error.detail,
      correlationId: error.correlationId ?? undefined,
    };
  }
  if (error instanceof Error) return { title: error.message };
  return { title: String(error) };
}

/**
 * Yetki paneli yanıt tipleri. OpenAPI sözleşmesinde yanıt şeması bulunmadığından Identity modülündeki
 * kayıtlardan (Sgt.Modules.Identity.Application.Panel) elle yazılmıştır; istek tipleri sözleşmeden üretilir.
 */
export interface RoleView {
  readonly id: string;
  readonly name: string;
  readonly permissions: readonly string[];
}

export interface ScreenView {
  readonly code: string;
  readonly product: string;
  /** "view" her zaman ilk sıradadır. */
  readonly actions: readonly string[];
}

export interface UserSummary {
  readonly userId: string;
  readonly displayName: string;
  readonly employeeCode: string;
  readonly isActive: boolean;
  readonly isSuperAdmin: boolean;
}

export interface UserAccessView {
  readonly userId: string;
  readonly isSuperAdmin: boolean;
  readonly roleIds: readonly string[];
  readonly grants: readonly string[];
  readonly denied: readonly string[];
  readonly effective: readonly string[];
}

export interface PermissionChangeView {
  readonly id: string;
  readonly target: string;
  readonly targetId: string;
  readonly kind: string;
  readonly item: string;
  readonly actorId: string;
  readonly terminal: string;
  readonly reasonCode: string;
  readonly correlationId: string;
  readonly occurredAt: string;
}

export interface CreatedId {
  readonly id: string;
}

export function permissionCode(screen: string, action: string): string {
  return `${screen}:${action}`;
}

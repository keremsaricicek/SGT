import type { Schemas } from "@/shared/api/client";

/** Yetki paneli yanıt tipleri (Sgt.Modules.Identity.Application.Panel); sözleşmeden üretilir. */
export type RoleView = Schemas["RoleView"];
/** `actions` içinde "view" her zaman ilk sıradadır. */
export type ScreenView = Schemas["ScreenView"];
export type UserSummary = Schemas["UserSummary"];
export type UserAccessView = Schemas["UserAccessView"];
export type PermissionChangeView = Schemas["PermissionChangeView"];
export type CreatedId = Schemas["CreatedId"];

export function permissionCode(screen: string, action: string): string {
  return `${screen}:${action}`;
}

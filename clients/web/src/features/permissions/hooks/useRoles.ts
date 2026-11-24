"use client";

import { useApiMutation } from "@/shared/hooks/useApiMutation";
import { useApiQuery } from "@/shared/hooks/useApiQuery";
import { createRolesService } from "../api/rolesService";

export function useRoles(enabled = true) {
  return useApiQuery(enabled ? "identity/roles" : null, (api) => createRolesService(api).list());
}

export function useScreenCatalog(enabled = true) {
  return useApiQuery(enabled ? "identity/screens" : null, (api) => createRolesService(api).screens());
}

export function useCreateRole(onDone?: () => void) {
  return useApiMutation(
    (api, input: { name: string; reasonCode: string }) => createRolesService(api).create(input.name, input.reasonCode),
    onDone,
  );
}

export function useCreateDefaultRoles(onDone?: () => void) {
  return useApiMutation((api, reasonCode: string) => createRolesService(api).createDefaults(reasonCode), onDone);
}

export interface RolePermissionChange {
  readonly permission: string;
  readonly grant: boolean;
  readonly reasonCode: string;
}

/** Rol yetkisi ver/geri al (tek eylem). */
export function useRolePermission(roleId: string, onDone?: () => void) {
  return useApiMutation((api, change: RolePermissionChange) => {
    const roles = createRolesService(api);
    return change.grant
      ? roles.grant(roleId, change.permission, change.reasonCode)
      : roles.revoke(roleId, change.permission, change.reasonCode);
  }, onDone);
}

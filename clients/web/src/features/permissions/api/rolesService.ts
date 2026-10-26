import type { SgtApi } from "@/shared/api/client";
import type { CreatedId, RoleView, ScreenView } from "./types";

/** Roller ve rol yetkileri (Spec 001). Değişiklikleri sunucu yalnızca süper admine izin verir. */
export interface RolesService {
  list(): Promise<RoleView[]>;
  screens(): Promise<ScreenView[]>;
  create(name: string, reasonCode: string): Promise<CreatedId>;
  createDefaults(reasonCode: string): Promise<void>;
  grant(roleId: string, permission: string, reasonCode: string): Promise<void>;
  revoke(roleId: string, permission: string, reasonCode: string): Promise<void>;
}

export function createRolesService(api: SgtApi): RolesService {
  const { client } = api;
  return {
    list: () => api.data<RoleView[]>(client.GET("/api/v1/identity/roles")),
    screens: () => api.data<ScreenView[]>(client.GET("/api/v1/identity/screens")),
    create: (name, reasonCode) =>
      api.data<CreatedId>(client.POST("/api/v1/identity/roles", { body: { name, reasonCode } })),
    createDefaults: (reasonCode) => api.done(client.POST("/api/v1/identity/roles/defaults", { body: { reasonCode } })),
    grant: (roleId, permission, reasonCode) =>
      api.done(
        client.POST("/api/v1/identity/roles/{roleId}/permissions", {
          params: { path: { roleId } },
          body: { permission, reasonCode },
        }),
      ),
    revoke: (roleId, permission, reasonCode) =>
      api.done(
        client.DELETE("/api/v1/identity/roles/{roleId}/permissions/{permission}", {
          params: { path: { roleId, permission }, query: { reasonCode } },
        }),
      ),
  };
}

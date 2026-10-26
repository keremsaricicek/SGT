import type { Schemas, SgtApi } from "@/shared/api/client";
import type { UserAccessView, UserSummary } from "./types";

export type RegisterUserInput = Schemas["RegisterUserRequest"];

/** Personel kullanıcıları (Spec 003) ve kişiye özel yetkiler (Spec 001). */
export interface UsersService {
  list(): Promise<UserSummary[]>;
  register(input: RegisterUserInput): Promise<void>;
  deactivate(userId: string, reasonCode: string): Promise<void>;
  reactivate(userId: string, reasonCode: string): Promise<void>;
}

export interface UserAccessService {
  access(userId: string): Promise<UserAccessView>;
  assignRole(userId: string, roleId: string, reasonCode: string): Promise<void>;
  unassignRole(userId: string, roleId: string, reasonCode: string): Promise<void>;
  grant(userId: string, permission: string, reasonCode: string): Promise<void>;
  revokeGrant(userId: string, permission: string, reasonCode: string): Promise<void>;
  deny(userId: string, permission: string, reasonCode: string): Promise<void>;
  undeny(userId: string, permission: string, reasonCode: string): Promise<void>;
}

export function createUsersService(api: SgtApi): UsersService {
  const { client } = api;
  return {
    list: () => api.data<UserSummary[]>(client.GET("/api/v1/identity/users")),
    register: (input) => api.done(client.POST("/api/v1/identity/users", { body: input })),
    deactivate: (userId, reasonCode) =>
      api.done(
        client.POST("/api/v1/identity/users/{userId}/deactivate", { params: { path: { userId } }, body: { reasonCode } }),
      ),
    reactivate: (userId, reasonCode) =>
      api.done(
        client.POST("/api/v1/identity/users/{userId}/reactivate", { params: { path: { userId } }, body: { reasonCode } }),
      ),
  };
}

export function createUserAccessService(api: SgtApi): UserAccessService {
  const { client } = api;
  return {
    access: (userId) =>
      api.data<UserAccessView>(client.GET("/api/v1/identity/users/{userId}/access", { params: { path: { userId } } })),
    assignRole: (userId, roleId, reasonCode) =>
      api.done(
        client.POST("/api/v1/identity/users/{userId}/roles", { params: { path: { userId } }, body: { roleId, reasonCode } }),
      ),
    unassignRole: (userId, roleId, reasonCode) =>
      api.done(
        client.DELETE("/api/v1/identity/users/{userId}/roles/{roleId}", {
          params: { path: { userId, roleId }, query: { reasonCode } },
        }),
      ),
    grant: (userId, permission, reasonCode) =>
      api.done(
        client.POST("/api/v1/identity/users/{userId}/grants", {
          params: { path: { userId } },
          body: { permission, reasonCode },
        }),
      ),
    revokeGrant: (userId, permission, reasonCode) =>
      api.done(
        client.DELETE("/api/v1/identity/users/{userId}/grants/{permission}", {
          params: { path: { userId, permission }, query: { reasonCode } },
        }),
      ),
    deny: (userId, permission, reasonCode) =>
      api.done(
        client.POST("/api/v1/identity/users/{userId}/denies", {
          params: { path: { userId } },
          body: { permission, reasonCode },
        }),
      ),
    undeny: (userId, permission, reasonCode) =>
      api.done(
        client.DELETE("/api/v1/identity/users/{userId}/denies/{permission}", {
          params: { path: { userId, permission }, query: { reasonCode } },
        }),
      ),
  };
}

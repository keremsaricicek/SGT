"use client";

import { useApiMutation } from "@/shared/hooks/useApiMutation";
import { useApiQuery } from "@/shared/hooks/useApiQuery";
import type { RegisterUserInput } from "../api/usersService";
import { createUserAccessService, createUsersService } from "../api/usersService";

export function useUsers(enabled = true) {
  return useApiQuery(enabled ? "identity/users" : null, (api) => createUsersService(api).list());
}

export function useRegisterUser(onDone?: () => void) {
  return useApiMutation((api, input: RegisterUserInput) => createUsersService(api).register(input), onDone);
}

export function useUserActivation(onDone?: () => void) {
  return useApiMutation(
    (api, input: { userId: string; active: boolean; reasonCode: string }) => {
      const users = createUsersService(api);
      return input.active
        ? users.reactivate(input.userId, input.reasonCode)
        : users.deactivate(input.userId, input.reasonCode);
    },
    onDone,
  );
}

export function useUserAccess(userId: string) {
  return useApiQuery(`identity/users/${userId}/access`, (api) => createUserAccessService(api).access(userId));
}

/** Kişiye özel değişiklik türleri; her biri ayrı uç noktaya gider. */
export type UserAccessChange =
  | { readonly kind: "assignRole" | "unassignRole"; readonly roleId: string; readonly reasonCode: string }
  | {
      readonly kind: "grant" | "revokeGrant" | "deny" | "undeny";
      readonly permission: string;
      readonly reasonCode: string;
    };

export function useUserAccessChange(userId: string, onDone?: () => void) {
  return useApiMutation((api, change: UserAccessChange) => {
    const access = createUserAccessService(api);
    switch (change.kind) {
      case "assignRole":
        return access.assignRole(userId, change.roleId, change.reasonCode);
      case "unassignRole":
        return access.unassignRole(userId, change.roleId, change.reasonCode);
      case "grant":
        return access.grant(userId, change.permission, change.reasonCode);
      case "revokeGrant":
        return access.revokeGrant(userId, change.permission, change.reasonCode);
      case "deny":
        return access.deny(userId, change.permission, change.reasonCode);
      case "undeny":
        return access.undeny(userId, change.permission, change.reasonCode);
    }
  }, onDone);
}

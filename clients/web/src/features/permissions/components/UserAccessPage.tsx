"use client";

import Link from "next/link";
import { useState } from "react";
import { ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { ReasonBar, useReasonCode } from "@/shared/ui/ActionForm";
import { FormSuccess, SelectField } from "@/shared/ui/form";
import { Loading, PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { RoleView, ScreenView } from "../api/types";
import { permissionCode } from "../api/types";
import { useRoles, useScreenCatalog } from "../hooks/useRoles";
import type { UserAccessChange } from "../hooks/useUsers";
import { useUserAccess, useUserAccessChange, useUsers } from "../hooks/useUsers";
import { SCREEN_ROLES, SCREEN_USERS } from "./screens";

function allPermissions(screens: readonly ScreenView[]): string[] {
  return screens.flatMap((s) => s.actions.map((a) => permissionCode(s.code, a)));
}

function CodeList({ codes, removeLabel, onRemove }: {
  codes: readonly string[];
  removeLabel?: string;
  onRemove?: (code: string) => void;
}) {
  const t = useT();
  if (codes.length === 0) return <p className="muted">{t("common.noRecords")}</p>;
  return (
    <ul className="chips">
      {codes.map((code) => (
        <li key={code}>
          <code>{code}</code>
          {onRemove ? (
            <button type="button" className="link" onClick={() => onRemove(code)}>
              {removeLabel}
            </button>
          ) : null}
        </li>
      ))}
    </ul>
  );
}

function PickAndApply({ label, placeholder, options, buttonLabel, onApply, disabled }: {
  label: string;
  placeholder: string;
  options: readonly { value: string; label: string }[];
  buttonLabel: string;
  onApply: (value: string) => void;
  disabled: boolean;
}) {
  const [value, setValue] = useState("");
  return (
    <div className="inline-form">
      <SelectField label={label} value={value} onChange={(e) => setValue(e.target.value)}>
        <option value="">{placeholder}</option>
        {options.map((o) => (
          <option key={o.value} value={o.value}>
            {o.label}
          </option>
        ))}
      </SelectField>
      <button type="button" className="btn" disabled={disabled || !value} onClick={() => onApply(value)}>
        {buttonLabel}
      </button>
    </div>
  );
}

function roleName(roles: readonly RoleView[] | undefined, roleId: string): string {
  return roles?.find((r) => r.id === roleId)?.name ?? roleId;
}

function UserAccessContent({ userId }: { userId: string }) {
  const t = useT();
  const { policy } = useAccess();
  const access = useUserAccess(userId);
  const roles = useRoles();
  const screens = useScreenCatalog();
  const users = useUsers(policy.canView(SCREEN_USERS));
  const reason = useReasonCode();
  const change = useUserAccessChange(userId, access.reload);
  const editable = policy.can(SCREEN_ROLES, "operate");

  const apply = (build: (reasonCode: string) => UserAccessChange) => {
    const reasonCode = reason.take();
    if (reasonCode) void change.run(build(reasonCode));
  };

  const view = access.data;
  const name = users.data?.find((u) => u.userId === userId)?.displayName ?? userId;
  const permissionOptions = allPermissions(screens.data ?? []).map((p) => ({ value: p, label: p }));
  const roleOptions = (roles.data ?? [])
    .filter((r) => !view?.roleIds.includes(r.id))
    .map((r) => ({ value: r.id, label: r.name }));

  return (
    <>
      <PageHeader title={t("permissions.access.title", { name })} actions={<Link href={routes.users}>{t("common.back")}</Link>} />
      <ProblemAlert error={access.error ?? roles.error ?? screens.error} />
      <Loading show={access.loading && !view} />
      {view ? (
        <>
          {view.isSuperAdmin ? <p className="notice">{t("permissions.access.superAdminNote")}</p> : null}
          {editable ? (
            <Section>
              <ReasonBar reason={reason} />
              <ProblemAlert error={change.error} />
              <FormSuccess show={change.succeeded && !change.pending} />
            </Section>
          ) : null}
          <div className="columns">
            <Section title={t("permissions.access.roles")}>
              <ul className="chips">
                {view.roleIds.map((roleId) => (
                  <li key={roleId}>
                    {roleName(roles.data, roleId)}
                    {editable ? (
                      <button
                        type="button"
                        className="link"
                        onClick={() => apply((reasonCode) => ({ kind: "unassignRole", roleId, reasonCode }))}
                      >
                        {t("permissions.access.unassign")}
                      </button>
                    ) : null}
                  </li>
                ))}
              </ul>
              {editable ? (
                <PickAndApply
                  label={t("permissions.access.assignRole")}
                  placeholder={t("permissions.access.selectRole")}
                  options={roleOptions}
                  buttonLabel={t("permissions.access.assignRole")}
                  disabled={change.pending}
                  onApply={(roleId) => apply((reasonCode) => ({ kind: "assignRole", roleId, reasonCode }))}
                />
              ) : null}
            </Section>
            <Section title={t("permissions.access.effective")}>
              <CodeList codes={view.effective} />
            </Section>
          </div>
          <div className="columns">
            <Section title={t("permissions.access.grants")}>
              <CodeList
                codes={view.grants}
                removeLabel={t("common.remove")}
                onRemove={
                  editable
                    ? (permission) => apply((reasonCode) => ({ kind: "revokeGrant", permission, reasonCode }))
                    : undefined
                }
              />
              {editable ? (
                <PickAndApply
                  label={t("permissions.access.permission")}
                  placeholder={t("permissions.access.selectPermission")}
                  options={permissionOptions}
                  buttonLabel={t("permissions.access.grant")}
                  disabled={change.pending}
                  onApply={(permission) => apply((reasonCode) => ({ kind: "grant", permission, reasonCode }))}
                />
              ) : null}
            </Section>
            <Section title={t("permissions.access.denied")}>
              <CodeList
                codes={view.denied}
                removeLabel={t("common.remove")}
                onRemove={
                  editable
                    ? (permission) => apply((reasonCode) => ({ kind: "undeny", permission, reasonCode }))
                    : undefined
                }
              />
              {editable ? (
                <PickAndApply
                  label={t("permissions.access.permission")}
                  placeholder={t("permissions.access.selectPermission")}
                  options={permissionOptions}
                  buttonLabel={t("permissions.access.deny")}
                  disabled={change.pending}
                  onApply={(permission) => apply((reasonCode) => ({ kind: "deny", permission, reasonCode }))}
                />
              ) : null}
            </Section>
          </div>
        </>
      ) : null}
    </>
  );
}

export function UserAccessPage({ userId }: { userId: string }) {
  return (
    <ScreenGuard screen={SCREEN_ROLES}>
      <UserAccessContent userId={userId} />
    </ScreenGuard>
  );
}

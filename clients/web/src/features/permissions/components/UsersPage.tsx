"use client";

import Link from "next/link";
import { useState } from "react";
import type { ChangeEvent } from "react";
import { Can, ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { ActionForm, ReasonBar, useReasonCode } from "@/shared/ui/ActionForm";
import { FormSuccess, TextField } from "@/shared/ui/form";
import { EmptyRow, Loading, PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { UserSummary } from "../api/types";
import { useRegisterUser, useUserActivation, useUsers } from "../hooks/useUsers";
import { SCREEN_ROLES, SCREEN_USERS } from "./screens";

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function RegisterUserForm({ onDone }: { onDone: () => void }) {
  const t = useT();
  const empty = { userId: "", displayName: "", employeeCode: "" };
  const [form, setForm] = useState(empty);
  const register = useRegisterUser(() => {
    setForm(empty);
    onDone();
  });
  const set = (field: keyof typeof empty) => (e: ChangeEvent<HTMLInputElement>) =>
    setForm((f) => ({ ...f, [field]: e.target.value }));

  return (
    <ActionForm
      title={t("permissions.users.register")}
      submitLabel={t("permissions.users.register")}
      pending={register.pending}
      error={register.error}
      succeeded={register.succeeded}
      onSubmit={(reasonCode) =>
        register.run({
          userId: form.userId.trim(),
          displayName: form.displayName.trim(),
          employeeCode: form.employeeCode.trim(),
          reasonCode,
        })
      }
    >
      <TextField
        label={t("permissions.users.userId")}
        value={form.userId}
        onChange={set("userId")}
        required
        pattern={GUID.source}
        placeholder="00000000-0000-0000-0000-000000000000"
      />
      <TextField label={t("permissions.users.displayName")} value={form.displayName} onChange={set("displayName")} required />
      <TextField label={t("permissions.users.employeeCode")} value={form.employeeCode} onChange={set("employeeCode")} required />
    </ActionForm>
  );
}

function UserRow({ user, onToggle, canToggle, canOpenAccess }: {
  user: UserSummary;
  onToggle: (user: UserSummary) => void;
  canToggle: boolean;
  canOpenAccess: boolean;
}) {
  const t = useT();
  return (
    <tr className={user.isActive ? undefined : "inactive"}>
      <td>{user.displayName}</td>
      <td>{user.employeeCode}</td>
      <td>{user.isActive ? t("common.yes") : t("common.no")}</td>
      <td>{user.isSuperAdmin ? t("common.yes") : t("common.no")}</td>
      <td className="row-actions">
        {canOpenAccess ? <Link href={routes.userAccess(user.userId)}>{t("permissions.users.access")}</Link> : null}
        {canToggle ? (
          <button type="button" className="btn small" onClick={() => onToggle(user)}>
            {user.isActive ? t("permissions.users.deactivate") : t("permissions.users.reactivate")}
          </button>
        ) : null}
      </td>
    </tr>
  );
}

function UsersContent() {
  const t = useT();
  const { policy } = useAccess();
  const users = useUsers();
  const reason = useReasonCode();
  const activation = useUserActivation(users.reload);
  const canToggle = policy.can(SCREEN_USERS, "deactivate");

  const toggle = (user: UserSummary) => {
    const reasonCode = reason.take();
    if (reasonCode) void activation.run({ userId: user.userId, active: !user.isActive, reasonCode });
  };

  return (
    <>
      <PageHeader title={t("permissions.users.title")} />
      <Section>
        {canToggle ? <ReasonBar reason={reason} /> : null}
        <ProblemAlert error={users.error ?? activation.error} />
        <FormSuccess show={activation.succeeded && !activation.pending} />
        <Loading show={users.loading && !users.data} />
        <table className="table">
          <thead>
            <tr>
              <th>{t("permissions.users.displayName")}</th>
              <th>{t("permissions.users.employeeCode")}</th>
              <th>{t("permissions.users.active")}</th>
              <th>{t("permissions.users.superAdmin")}</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {users.data?.length === 0 ? <EmptyRow colSpan={5} /> : null}
            {users.data?.map((user) => (
              <UserRow
                key={user.userId}
                user={user}
                onToggle={toggle}
                canToggle={canToggle && !activation.pending}
                canOpenAccess={policy.canView(SCREEN_ROLES)}
              />
            ))}
          </tbody>
        </table>
      </Section>
      <Can screen={SCREEN_USERS} action="create">
        <Section>
          <RegisterUserForm onDone={users.reload} />
        </Section>
      </Can>
    </>
  );
}

export function UsersPage() {
  return (
    <ScreenGuard screen={SCREEN_USERS}>
      <UsersContent />
    </ScreenGuard>
  );
}

"use client";

import Link from "next/link";
import { useState } from "react";
import type { ChangeEvent } from "react";
import { ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { ActionForm, ReasonBar, useReasonCode } from "@/shared/ui/ActionForm";
import { FormSuccess, TextField } from "@/shared/ui/form";
import { Badge } from "@/shared/ui/Badge";
import { Button } from "@/shared/ui/Button";
import { DataGrid } from "@/shared/ui/DataGrid";
import { Dialog } from "@/shared/ui/Dialog";
import { Page, Section } from "@/shared/ui/layout";
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

function UsersContent() {
  const t = useT();
  const { policy } = useAccess();
  const users = useUsers();
  const reason = useReasonCode();
  const activation = useUserActivation(users.reload);
  const [registering, setRegistering] = useState(false);
  const canToggle = policy.can(SCREEN_USERS, "deactivate");
  const canCreate = policy.can(SCREEN_USERS, "create");
  const canOpenAccess = policy.canView(SCREEN_ROLES);

  const toggle = (user: UserSummary) => {
    const reasonCode = reason.take();
    if (reasonCode) void activation.run({ userId: user.userId, active: !user.isActive, reasonCode });
  };

  return (
    <Page
      title={t("permissions.users.title")}
      meta={users.data ? t("common.records", { count: users.data.length }) : null}
      actions={
        canCreate ? (
          <Button icon="personAdd" onClick={() => setRegistering(true)}>
            {t("permissions.users.new")}
          </Button>
        ) : null
      }
    >
      <ProblemAlert error={users.error} />
      <Section flush>
        {canToggle ? <ReasonBar reason={reason} /> : null}
        <div className="panel-messages">
          <ProblemAlert error={activation.error} />
          <FormSuccess show={activation.succeeded && !activation.pending} />
        </div>
        <DataGrid<UserSummary>
          label={t("permissions.users.title")}
          rows={users.data}
          loading={users.loading}
          rowKey={(u) => u.userId}
          rowClassName={(u) => (u.isActive ? undefined : "inactive")}
          columns={[
            { key: "name", header: t("permissions.users.displayName"), cell: (u) => u.displayName },
            { key: "code", header: t("permissions.users.employeeCode"), cell: (u) => <code>{u.employeeCode}</code> },
            {
              key: "status",
              header: t("common.status"),
              cell: (u) =>
                u.isActive ? (
                  <Badge tone="success">{t("permissions.users.status.active")}</Badge>
                ) : (
                  <Badge>{t("permissions.users.status.inactive")}</Badge>
                ),
            },
            {
              key: "super",
              header: t("permissions.users.superAdmin"),
              cell: (u) => (u.isSuperAdmin ? <Badge tone="info">{t("permissions.users.superAdmin")}</Badge> : null),
            },
            {
              key: "actions",
              header: <span className="visually-hidden">{t("permissions.users.access")}</span>,
              className: "actions",
              cell: (u) => (
                <span className="row-actions">
                  {canOpenAccess ? <Link href={routes.userAccess(u.userId)}>{t("permissions.users.access")}</Link> : null}
                  {canToggle ? (
                    <Button size="small" variant={u.isActive ? "danger" : "default"} disabled={activation.pending} onClick={() => toggle(u)}>
                      {u.isActive ? t("permissions.users.deactivate") : t("permissions.users.reactivate")}
                    </Button>
                  ) : null}
                </span>
              ),
            },
          ]}
        />
      </Section>
      <Dialog open={registering} title={t("permissions.users.register")} onClose={() => setRegistering(false)}>
        <RegisterUserForm
          onDone={() => {
            setRegistering(false);
            users.reload();
          }}
        />
      </Dialog>
    </Page>
  );
}

export function UsersPage() {
  return (
    <ScreenGuard screen={SCREEN_USERS}>
      <UsersContent />
    </ScreenGuard>
  );
}

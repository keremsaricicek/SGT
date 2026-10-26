"use client";

import Link from "next/link";
import { useState } from "react";
import { Can, ScreenGuard } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { ActionForm } from "@/shared/ui/ActionForm";
import { TextField } from "@/shared/ui/form";
import { EmptyRow, Loading, PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import { useCreateDefaultRoles, useCreateRole, useRoles } from "../hooks/useRoles";
import { SCREEN_ROLES } from "./screens";

function CreateRoleForm({ onCreated }: { onCreated: () => void }) {
  const t = useT();
  const [name, setName] = useState("");
  const create = useCreateRole(() => {
    setName("");
    onCreated();
  });
  return (
    <ActionForm
      title={t("permissions.roles.create")}
      submitLabel={t("permissions.roles.create")}
      pending={create.pending}
      error={create.error}
      succeeded={create.succeeded}
      onSubmit={(reasonCode) => create.run({ name: name.trim(), reasonCode })}
    >
      <TextField label={t("permissions.roles.name")} value={name} required maxLength={100} onChange={(e) => setName(e.target.value)} />
    </ActionForm>
  );
}

function CreateDefaultRolesForm({ onCreated }: { onCreated: () => void }) {
  const t = useT();
  const create = useCreateDefaultRoles(onCreated);
  return (
    <ActionForm
      title={t("permissions.roles.createDefaults")}
      submitLabel={t("permissions.roles.createDefaults")}
      pending={create.pending}
      error={create.error}
      succeeded={create.succeeded}
      onSubmit={(reasonCode) => create.run(reasonCode)}
    >
      <p className="muted span-all">{t("permissions.roles.createDefaults.hint")}</p>
    </ActionForm>
  );
}

function RolesContent() {
  const t = useT();
  const roles = useRoles();
  return (
    <>
      <PageHeader title={t("permissions.roles.title")} />
      <Section>
        <ProblemAlert error={roles.error} />
        <Loading show={roles.loading && !roles.data} />
        <table className="table">
          <thead>
            <tr>
              <th>{t("permissions.roles.name")}</th>
              <th className="num">{t("permissions.roles.permissionCount")}</th>
            </tr>
          </thead>
          <tbody>
            {roles.data?.length === 0 ? <EmptyRow colSpan={2} /> : null}
            {roles.data?.map((role) => (
              <tr key={role.id}>
                <td>
                  <Link href={routes.role(role.id)}>{role.name}</Link>
                </td>
                <td className="num">{role.permissions.length}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </Section>
      <Can screen={SCREEN_ROLES} action="operate">
        <div className="columns">
          <Section>
            <CreateRoleForm onCreated={roles.reload} />
          </Section>
          <Section>
            <CreateDefaultRolesForm onCreated={roles.reload} />
          </Section>
        </div>
      </Can>
    </>
  );
}

export function RolesPage() {
  return (
    <ScreenGuard screen={SCREEN_ROLES}>
      <RolesContent />
    </ScreenGuard>
  );
}

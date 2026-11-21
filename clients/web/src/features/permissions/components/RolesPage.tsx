"use client";

import Link from "next/link";
import { useState } from "react";
import { Can, ScreenGuard } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { ActionForm } from "@/shared/ui/ActionForm";
import { TextField } from "@/shared/ui/form";
import { DataGrid } from "@/shared/ui/DataGrid";
import { Page, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import type { RoleView } from "../api/types";
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
    <Page title={t("permissions.roles.title")} meta={roles.data ? t("common.records", { count: roles.data.length }) : null}>
      <ProblemAlert error={roles.error} />
      <Section flush>
        <DataGrid<RoleView>
          label={t("permissions.roles.title")}
          rows={roles.data}
          loading={roles.loading}
          rowKey={(role) => role.id}
          columns={[
            { key: "name", header: t("permissions.roles.name"), cell: (role) => <Link href={routes.role(role.id)}>{role.name}</Link> },
            { key: "count", header: t("permissions.roles.permissionCount"), align: "end", cell: (role) => role.permissions.length },
          ]}
        />
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
    </Page>
  );
}

export function RolesPage() {
  return (
    <ScreenGuard screen={SCREEN_ROLES}>
      <RolesContent />
    </ScreenGuard>
  );
}

"use client";

import Link from "next/link";
import { useMemo } from "react";
import { ScreenGuard, useAccess } from "@/shared/access/AccessProvider";
import { useT } from "@/shared/i18n/I18nProvider";
import { routes } from "@/shared/navigation/routes";
import { ReasonBar, useReasonCode } from "@/shared/ui/ActionForm";
import { FormSuccess } from "@/shared/ui/form";
import { Loading, PageHeader, Section } from "@/shared/ui/layout";
import { ProblemAlert } from "@/shared/ui/ProblemAlert";
import { useRolePermission, useRoles, useScreenCatalog } from "../hooks/useRoles";
import { PermissionMatrix } from "./PermissionMatrix";
import { SCREEN_ROLES } from "./screens";

function RoleDetailContent({ roleId }: { roleId: string }) {
  const t = useT();
  const { policy } = useAccess();
  const roles = useRoles();
  const screens = useScreenCatalog();
  const reason = useReasonCode();
  const change = useRolePermission(roleId, roles.reload);

  const role = roles.data?.find((r) => r.id === roleId);
  const granted = useMemo(() => new Set(role?.permissions ?? []), [role]);
  const editable = policy.can(SCREEN_ROLES, "operate");

  const toggle = (permission: string, grant: boolean) => {
    const reasonCode = reason.take();
    if (reasonCode) void change.run({ permission, grant, reasonCode });
  };

  return (
    <>
      <PageHeader
        title={t("permissions.role.title", { name: role?.name ?? "…" })}
        actions={<Link href={routes.roles}>{t("common.back")}</Link>}
      />
      <ProblemAlert error={roles.error ?? screens.error} />
      <Loading show={(roles.loading && !roles.data) || (screens.loading && !screens.data)} />
      {roles.data && !role ? <p className="notice">{t("permissions.role.notFound")}</p> : null}
      {role && screens.data ? (
        <Section>
          {editable ? (
            <>
              <p className="muted">{t("permissions.role.matrixHint")}</p>
              <ReasonBar reason={reason} />
            </>
          ) : null}
          <ProblemAlert error={change.error} />
          <FormSuccess show={change.succeeded && !change.pending} />
          <PermissionMatrix
            screens={screens.data}
            granted={granted}
            editable={editable}
            busy={change.pending}
            onToggle={toggle}
          />
        </Section>
      ) : null}
    </>
  );
}

export function RoleDetailPage({ roleId }: { roleId: string }) {
  return (
    <ScreenGuard screen={SCREEN_ROLES}>
      <RoleDetailContent roleId={roleId} />
    </ScreenGuard>
  );
}

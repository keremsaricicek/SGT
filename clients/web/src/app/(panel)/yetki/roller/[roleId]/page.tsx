import { RoleDetailPage } from "@/features/permissions";

export default async function Page({ params }: { params: Promise<{ roleId: string }> }) {
  const { roleId } = await params;
  return <RoleDetailPage roleId={decodeURIComponent(roleId)} />;
}

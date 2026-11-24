import { UserAccessPage } from "@/features/permissions";

export default async function Page({ params }: { params: Promise<{ userId: string }> }) {
  const { userId } = await params;
  return <UserAccessPage userId={decodeURIComponent(userId)} />;
}

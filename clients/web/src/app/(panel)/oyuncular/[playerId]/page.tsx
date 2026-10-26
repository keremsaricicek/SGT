import { PlayerDetailPage } from "@/features/players";

export default async function Page({ params }: { params: Promise<{ playerId: string }> }) {
  const { playerId } = await params;
  return <PlayerDetailPage playerId={decodeURIComponent(playerId)} />;
}

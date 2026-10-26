import Link from "next/link";

export default function NotFound() {
  return (
    <main className="center">
      <p>Sayfa bulunamadı / Page not found.</p>
      <Link href="/">SGT</Link>
    </main>
  );
}

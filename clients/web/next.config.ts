import type { NextConfig } from "next";

/**
 * SGT_API_PROXY_TARGET verilirse /api/v1/* istekleri geliştirme sunucusu üzerinden API'ye aktarılır
 * (tarayıcı aynı kökene istek atar, API'de CORS gerekmez). Verilmezse istemci NEXT_PUBLIC_API_BASE_URL'e gider.
 */
const proxyTarget = process.env.SGT_API_PROXY_TARGET;

const nextConfig: NextConfig = {
  reactStrictMode: true,
  poweredByHeader: false,
  async rewrites() {
    return proxyTarget ? [{ source: "/api/v1/:path*", destination: `${proxyTarget}/api/v1/:path*` }] : [];
  },
  async headers() {
    return [
      {
        source: "/:path*",
        headers: [
          { key: "X-Content-Type-Options", value: "nosniff" },
          { key: "Referrer-Policy", value: "no-referrer" },
          { key: "X-Frame-Options", value: "SAMEORIGIN" },
        ],
      },
    ];
  },
};

export default nextConfig;

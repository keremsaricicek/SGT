import nextVitals from "eslint-config-next/core-web-vitals";
import nextTs from "eslint-config-next/typescript";

const features = ["permissions", "players", "exchange-rates", "daily-report", "audit", "integration"];

/**
 * K2 (docs/kod-kurallari.md §4, Web): feature'lar birbirinin iç koduna erişmez; paylaşılan kod src/shared'dadır.
 * Sayfalar (src/app) feature'ları yalnızca genel girişleri (index.ts) üzerinden kullanır.
 */
const featureBoundaries = {
  files: ["src/**/*.{ts,tsx}"],
  rules: {
    "import/no-restricted-paths": [
      "error",
      {
        zones: [
          ...features.map((feature) => ({
            target: `./src/features/${feature}`,
            from: "./src/features",
            except: [`./${feature}`],
            message: "Feature'lar birbirini import etmez; ortak kod src/shared altına taşınmalı.",
          })),
          {
            target: "./src/shared",
            from: "./src/features",
            message: "src/shared feature'lara bağımlı olamaz.",
          },
        ],
      },
    ],
  },
};

const appUsesPublicEntries = {
  files: ["src/app/**/*.{ts,tsx}"],
  rules: {
    "no-restricted-imports": [
      "error",
      {
        patterns: [
          {
            group: ["@/features/*/*"],
            message: "Sayfalar feature'ları yalnızca @/features/<ad> genel girişinden kullanır.",
          },
        ],
      },
    ],
  },
};

const config = [
  ...nextVitals,
  ...nextTs,
  featureBoundaries,
  appUsesPublicEntries,
  {
    rules: {
      "@typescript-eslint/consistent-type-imports": "error",
      "@typescript-eslint/no-explicit-any": "error",
      "no-console": ["error", { allow: ["warn", "error"] }],
    },
  },
  {
    ignores: [".next/**", "node_modules/**", "next-env.d.ts", "src/shared/api/generated/**", "coverage/**"],
  },
];

export default config;

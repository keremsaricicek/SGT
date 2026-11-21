# Kurulu ajan, skill, plugin ve MCP'ler

Hepsi proje düzeyindedir (depoyla gelir). Az ve işe yarar araç ilkesi: her araç her mesajda bağlam (token) harcar;
bir işi iki araç yapıyorsa biri kapatılır.

## Projeye özel ajanlar (`.claude/agents`)
- `code-rules-reviewer`: K1/K2 kod kuralları.
- `finans-denetci`: para/değer akışı (tutar, değiştirilemezlik, dört göz, eşzamanlılık, idempotency).
- `veri-sagligi-denetci`: migration, şema, indeks, tenant kapsamı, kişisel veri, saklama.

## Plugin'ler (`.claude/settings.json`)
- Disiplin: superpowers (plan → test → kod → inceleme), tdd-workflows.
- .NET: dotnet, dotnet-aspnetcore, dotnet-data, dotnet-test, microsoft-docs.
- Arayüz: frontend-design, interface-design; mobil: dart-flutter.
- İnceleme ve güvenlik: pr-review-toolkit, security-guidance; Trail of Bits static-analysis, insecure-defaults,
  differential-review, sharp-edges, property-based-testing.

Kapatılanlar (tekrar eden veya ilgisiz): compound-engineering, full-stack-orchestration, backend-development,
feature-dev, c4-architecture, ui-ux-pro-max, dotnet-artisan, dotnet-diag, code-review (yerleşik olanı var);
hesap düzeyindeki marketing, data, operations, product-management, engineering.

Kurulması önerilen, onay bekleyenler: postgres-best-practices (Supabase), accesslint (WCAG 2.2 erişilebilirlik),
grafana-k6 (yük testi).

## Skill'ler
impeccable, design-taste-frontend, vercel-react-best-practices (`.claude/skills` ve `.agents/skills`), owasp-security.
Kaldırılanlar: BMAD ve ccpm (Spec Kit + Superpowers ile aynı işi yapıyordu).

## Spec Kit
Şablonlar `.specify/templates`, betikler `.specify/scripts/bash`, komutlar `.claude/commands/speckit.*`.
Anayasa: `.specify/memory/constitution.md`.

## MCP sunucuları (`.mcp.json`)
playwright (ekranı görerek test), context7 (güncel kütüphane dokümanı), postgres (salt okuma, geliştirme veritabanı).
Kaldırılanlar: semgrep (CLI ile CI'da çalışır), serena, taskmaster-ai, figma (bağlanamıyordu veya gereksizdi).

## CI
`claude-review.yml`: PR'larda claude-code-action ile kural incelemesi (depo sırrı `ANTHROPIC_API_KEY` gerekir).

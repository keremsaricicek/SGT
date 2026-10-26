# Kurulu ajan, skill, plugin ve MCP'ler

Kaynak: `docs/ajan-skill-yol-haritasi.pdf`. Hepsi proje düzeyinde kuruludur (depoyla gelir).

## Plugin'ler (`.claude/settings.json`)
frontend-design, code-review, pr-review-toolkit, security-guidance, feature-dev, microsoft-docs (Anthropic);
interface-design; ui-ux-pro-max; superpowers; compound-engineering; c4-architecture, backend-development,
tdd-workflows, full-stack-orchestration (wshobson); dotnet, dotnet-aspnetcore, dotnet-data, dotnet-test, dotnet-diag
(Microsoft); dotnet-artisan; dart-flutter (Google); static-analysis, insecure-defaults, differential-review,
sharp-edges, property-based-testing (Trail of Bits).

## Skill'ler (`.claude/skills`)
impeccable, design-taste-frontend (Taste), vercel-react-best-practices, owasp-security, ccpm.
Hesapta: kerem-frontend-uiux-master.

## Spec Kit
Resmi şablonlar `.specify/templates`, betikler `.specify/scripts/bash`, komutlar `.claude/commands/speckit.*`
(`/speckit.specify`, `/speckit.plan`, `/speckit.tasks`, `/speckit.implement` ...). Anayasa: `.specify/memory/constitution.md`.

## BMAD Method
`_bmad/` ve Claude Code komutları (`npx bmad-method install --tools claude-code`).

## MCP sunucuları (`.mcp.json`)
playwright (ekranı görerek test), context7 (güncel kütüphane dokümanı), semgrep (güvenlik taraması),
serena (kod gezinme), postgres (yalnızca okuma kipinde, geliştirme veritabanı), taskmaster-ai, figma.
Proje MCP'leri her yeni oturumda bir kez onay ister.

## CI
`claude-review.yml`: PR'larda claude-code-action ile kural incelemesi + güvenlik taraması
(depo sırrı `ANTHROPIC_API_KEY` gerekir). Semgrep MCP üzerinden; ayrıca SonarQube kalite kapısı için sunucu gerekir.

## Çalışmak için dış bilgi gerekenler
- figma: Figma hesabıyla yetkilendirme.
- SonarQube: çalışan bir SonarQube sunucusu ve token (kurulmadı).
- shadcn MCP: web paneli shadcn kullanmadığı için kurulmadı.
- claude-code-action: `ANTHROPIC_API_KEY` sırrı.

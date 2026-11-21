#!/bin/bash
# Bulut oturumunda .NET 10 SDK, yerel PostgreSQL (entegrasyon testleri için) ve graphify CLI hazırlar.
set -euo pipefail
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then exit 0; fi
if ! command -v dotnet >/dev/null 2>&1 || ! command -v psql >/dev/null 2>&1; then
  apt-get update -qq && DEBIAN_FRONTEND=noninteractive apt-get install -y -qq dotnet-sdk-10.0 postgresql >/dev/null
fi
command -v graphify >/dev/null 2>&1 || pip install -q --break-system-packages graphifyy >/dev/null 2>&1 || true
service postgresql start >/dev/null 2>&1 || true
su postgres -c "psql -qtAc \"SELECT 1 FROM pg_roles WHERE rolname='sgt'\"" | grep -q 1 \
  || su postgres -c "psql -qc \"CREATE ROLE sgt LOGIN SUPERUSER PASSWORD 'sgt_dev_only'\""
cd "$CLAUDE_PROJECT_DIR" && dotnet tool restore >/dev/null && dotnet restore Sgt.slnx >/dev/null

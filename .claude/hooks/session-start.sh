#!/bin/bash
# Bulut oturumunda .NET 10 SDK yoksa kurar.
set -euo pipefail
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then exit 0; fi
if ! command -v dotnet >/dev/null 2>&1; then
  apt-get update -qq && DEBIAN_FRONTEND=noninteractive apt-get install -y -qq dotnet-sdk-10.0 >/dev/null
fi
dotnet restore "$CLAUDE_PROJECT_DIR/Sgt.slnx" >/dev/null

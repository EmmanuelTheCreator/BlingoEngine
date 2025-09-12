#!/usr/bin/env bash
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
if command -v pwsh >/dev/null 2>&1; then
  pwsh -ExecutionPolicy Bypass -File "$SCRIPT_DIR/SetGodotVersion.ps1" "$@"
else
  echo "PowerShell (pwsh) is required to run SetGodotVersion.ps1" >&2
  exit 1
fi

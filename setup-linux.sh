#!/usr/bin/env bash
# LingoEngine one-click setup script for Linux.
#
# This script is intended to remain a simple way to prepare the repository
# for development. Future updates should keep this goal in mind.

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

cat <<'MSG'
This script will:
  - Ensure the .NET 8 SDK is installed.
  - Copy SDL2 native libraries into build output folders for the TetriGrounds SDL demo and SDL-based test projects.

Press Enter to continue or Ctrl+C to abort.
MSG
read -r

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET SDK not found. Installing via scripts/install-dotnet.sh..."
  ./scripts/install-dotnet.sh
else
  echo ".NET SDK found: $(dotnet --version)"
fi

ARCH=$(uname -m)
case "$ARCH" in
  x86_64) ARCH_DIR="SDL2_LINUX_X64" ;;
  i386|i686) ARCH_DIR="SDL2_LINUX_X86" ;;
  aarch64|arm64) ARCH_DIR="SDL2_LINUX_ARM64" ;;
  arm*) ARCH_DIR="SDL2_LINUX_ARM" ;;
  *) ARCH_DIR="" ;;
esac

LIB_DIR="Libs/$ARCH_DIR"
if [ -z "$ARCH_DIR" ] || [ ! -d "$LIB_DIR" ]; then
  echo "No SDL2 libraries available for architecture '$ARCH'. Skipping copy."
else
  echo "Copying SDL2 libraries for architecture '$ARCH'."
  PROJECTS=(
    "Demo/TetriGrounds/LingoEngine.Demo.TetriGrounds.SDL2"
    "Test/LingoEngine.SDL2.GfxVisualTest"
    "WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.SDL2"
  )
  for proj in "${PROJECTS[@]}"; do
    if [ -d "$proj" ]; then
      for config in Debug Release; do
        TARGET="$proj/bin/$config/net8.0"
        mkdir -p "$TARGET"
        cp "$LIB_DIR"/* "$TARGET/" 2>/dev/null || true
      done
    fi
  done
fi

echo "Setup complete."

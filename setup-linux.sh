#!/usr/bin/env bash
# LingoEngine one-click setup script for Linux.
#
# This script is intended to remain a simple way to prepare the repository
# for development. Future updates should keep this goal in mind.

set -e

FAILED=0
trap 'FAILED=1' ERR
trap 'if [ $FAILED -eq 0 ]; then
  echo "Setup complete."
else
  echo "Setup encountered an error."
fi
read -p "Press Enter to exit..."' EXIT

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

ARCH=$(uname -m)
case "$ARCH" in
  x86_64) ARCH_DIR="SDL2_LINUX_X64" ;;
  i386|i686) ARCH_DIR="SDL2_LINUX_X86" ;;
  aarch64|arm64) ARCH_DIR="SDL2_LINUX_ARM64" ;;
  arm*) ARCH_DIR="SDL2_LINUX_ARM" ;;
  *) ARCH_DIR="" ;;
esac

LIB_DIR="Libs/$ARCH_DIR"
PROJECTS=(
  "Demo/TetriGrounds/LingoEngine.Demo.TetriGrounds.SDL2"
  "Test/LingoEngine.SDL2.GfxVisualTest"
  "WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.SDL2"
  "src/Director/LingoEngine.Director.Runner.SDL2"
)
MEDIA_SRC="Demo/TetriGrounds/LingoEngine.Demo.TetriGrounds.Godot/Media"
MEDIA_DEST="Demo/TetriGrounds/LingoEngine.Demo.TetriGrounds.Blazor/Media"

echo "This script will:"
echo "  - Ensure the .NET 8 SDK is installed."
if [ -z "$ARCH_DIR" ] || [ ! -d "$LIB_DIR" ]; then
  echo "  - SDL2 libraries will not be copied (unsupported architecture '$ARCH')."
else
  echo "  - Copy SDL2 native libraries for architecture '$ARCH' from '$LIB_DIR' into:"
  for proj in "${PROJECTS[@]}"; do
    echo "      - $proj/bin/<config>/net8.0"
  done
fi
echo "  - Copy demo media from '$MEDIA_SRC' to '$MEDIA_DEST'."
echo "  - At the end you will be prompted to locate your Godot executable."
echo "    When the file dialog appears, select the Godot 4 binary you want to use."
echo "    Example: /path/to/Godot_v4.5-stable_mono_linux_x86_64"
echo "    Minimum required version is 4.5."
echo
echo "Press Enter to continue or Ctrl+C to abort."
read -r

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET SDK not found. Installing via scripts/install-dotnet.sh..."
  ./scripts/install-dotnet.sh
else
  echo ".NET SDK found: $(dotnet --version)"
fi

if [ -z "$ARCH_DIR" ] || [ ! -d "$LIB_DIR" ]; then
  echo "No SDL2 libraries available for architecture '$ARCH'. Skipping copy."
else
  echo "Copying SDL2 libraries for architecture '$ARCH'."
  for proj in "${PROJECTS[@]}"; do
    if [ -d "$proj" ]; then
      for config in Debug DebugWithDirector Release; do
        TARGET="$proj/bin/$config/net8.0"
        mkdir -p "$TARGET"
        cp "$LIB_DIR"/* "$TARGET/" 2>/dev/null || true
      done
    fi
  done
fi

if [ -d "$MEDIA_SRC" ]; then
  echo "Copying demo media to '$MEDIA_DEST'."
  rm -rf "$MEDIA_DEST"
  cp -r "$MEDIA_SRC" "$MEDIA_DEST"
else
  echo "Demo media source '$MEDIA_SRC' not found. Skipping."
fi

GODOT_PATH=""
if command -v zenity >/dev/null 2>&1; then
  echo "Select the Godot executable (e.g., /path/to/Godot_v4.5-stable_mono_linux_x86_64)"
  echo "Minimum version 4.5 required."
  GODOT_PATH=$(zenity --file-selection --title="Select Godot executable" 2>/dev/null || true)
fi
if [ -z "$GODOT_PATH" ]; then
  echo "Select the Godot executable (e.g., /path/to/Godot_v4.5-stable_mono_linux_x86_64)"
  read -p "Minimum version 4.5 required. Enter path to Godot executable: " GODOT_PATH
fi

if [ -n "$GODOT_PATH" ]; then
  GODOT_VERSION=$(basename "$GODOT_PATH" | sed -E 's/^Godot_v([^_]+)_.+$/\1/')
  python3 - "$GODOT_PATH" <<'PY'
import json,sys,os
path=sys.argv[1]
settings_path=os.path.join('.vscode','settings.json')
with open(settings_path) as f: data=json.load(f)
data['godotTools.editorPath.godot4']=path
with open(settings_path,'w') as f: json.dump(data,f,indent=4)
PY
  ./scripts/SetGodotVersion.sh "$GODOT_VERSION"
fi


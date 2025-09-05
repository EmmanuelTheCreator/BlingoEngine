#!/usr/bin/env bash
set -euo pipefail

# ---------- PATHs ----------
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$HOME/.local/bin:$PATH"

# ---------- Apt packages (Ubuntu/Debian) ----------
if command -v apt-get >/dev/null 2>&1; then
  if command -v sudo >/dev/null 2>&1; then
    export DEBIAN_FRONTEND=noninteractive

    need_pkgs=(
      ca-certificates curl unzip
      libx11-6 libxcursor1 libxi6 libxrandr2 libxinerama1
      libgl1 libasound2t64
      libsdl2-2.0-0 libsdl2-dev
      libsdl2-image-2.0-0 libsdl2-image-dev
      libsdl2-mixer-2.0-0 libsdl2-mixer-dev
      libsdl2-ttf-2.0-0 libsdl2-ttf-dev
    )

    missing=()
    for p in "${need_pkgs[@]}"; do
      dpkg-query -W -f='${Status}\n' "$p" 2>/dev/null | grep -q "install ok installed" || missing+=("$p")
    done

    if ((${#missing[@]})); then
      sudo apt-get update -yq
      sudo apt-get install -yq --no-install-recommends "${missing[@]}"
      sudo rm -rf /var/lib/apt/lists/*
    fi
  else
    echo "sudo not found; skipping apt packages. Install SDL2/* and X libs manually." >&2
  fi
fi

# ---------- .NET SDKs (8 LTS + 9) ----------
if ! command -v dotnet >/dev/null 2>&1; then
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
else
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
fi

# SDK 8 (LTS)
if ! dotnet --list-sdks 2>/dev/null | grep -q '^8\.'; then
  /tmp/dotnet-install.sh --channel LTS --install-dir "$HOME/.dotnet"
fi

# SDK 9
if ! dotnet --list-sdks 2>/dev/null | grep -q '^9\.'; then
  /tmp/dotnet-install.sh --channel 9.0 --install-dir "$HOME/.dotnet"
fi

# Global tool: dotnet-format
dotnet tool install -g dotnet-format || true

# ---------- Godot (mono) ----------
: "${GODOT_URL:=https://github.com/godotengine/godot-builds/releases/download/4.5-dev5/Godot_v4.5-dev5_mono_linux_x86_64.zip}"

mkdir -p "$HOME/.local/bin" "$HOME/.cache"
tmpzip="$(mktemp "$HOME/.cache/godot.XXXX.zip")"
echo "Downloading Godot from $GODOT_URL"
curl -fL --retry 5 --retry-delay 2 "$GODOT_URL" -o "$tmpzip"

dest="$HOME/.local/godot/$(date +%s)"
mkdir -p "$dest"
unzip -oq "$tmpzip" -d "$dest"
rm -f "$tmpzip"

GODOT_BIN="$(find "$dest" -type f \( -name 'Godot_*linux_x86_64*' -o -name 'Godot*.x86_64' \) | head -n1)"
if [ -z "${GODOT_BIN:-}" ]; then
  echo "Godot binary not found under $dest" >&2
  find "$dest" -maxdepth 2 -print >&2
  exit 1
fi
chmod +x "$GODOT_BIN"
ln -sf "$GODOT_BIN" "$HOME/.local/bin/godot"

# ---------- Prints ----------
echo
dotnet --info || true
dotnet --list-runtimes || true
dotnet format --version || true
godot --version || true

echo
echo "Done. Ensure ~/.dotnet and ~/.local/bin are on PATH:"
echo '  export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$HOME/.local/bin:$PATH"'

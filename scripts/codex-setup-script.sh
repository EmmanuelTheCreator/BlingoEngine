#!/usr/bin/env bash
set -euo pipefail
set -x

# Fast exit when cached
if command -v godot >/dev/null 2>&1; then godot --version || true; exit 0; fi

export DEBIAN_FRONTEND=noninteractive

retry() { n=0; until "$@"; do n=$((n+1)); [ $n -ge 5 ] && return 1; sleep $((2*n)); done; }

# --- Packages (skip if already installed) ---
need_pkgs=(
	ca-certificates curl unzip
	libx11-6 libxcursor1 libxi6 libxrandr2 libxinerama1
	libgl1 libasound2t64
	# SDL2 runtime + dev
	libsdl2-2.0-0
	libsdl2-dev
	libsdl2-image-2.0-0
	libsdl2-image-dev
	libsdl2-mixer-2.0-0
	libsdl2-mixer-dev
	libsdl2-ttf-2.0-0
	libsdl2-ttf-dev
	# optional if you use gfx:
	libsdl2-gfx-1.0-0
	libsdl2-gfx-dev
)

missing=()
for p in "${need_pkgs[@]}"; do
  if ! dpkg-query -W -f='${Status}\n' "$p" 2>/dev/null | grep -q "install ok installed"; then
    missing+=("$p")
  fi
done

if ((${#missing[@]})); then
  retry sudo apt-get update
  retry sudo apt-get install -yq --no-install-recommends "${missing[@]}"
  sudo rm -rf /var/lib/apt/lists/*
fi


# --- .NET runtimes (8 for Godot, 9 for Blazor) ---
if ! command -v dotnet >/dev/null 2>&1; then
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
fi

# Install .NET 8 runtime
if ! ls /usr/share/dotnet/shared/Microsoft.NETCore.App/ 2>/dev/null | grep -q '^8\.0\.'; then
  /tmp/dotnet-install.sh --version 8.0.8 --runtime dotnet --install-dir /usr/share/dotnet
fi

# Install .NET 9 runtime
if ! ls /usr/share/dotnet/shared/Microsoft.NETCore.App/ 2>/dev/null | grep -q '^9\.0\.'; then
  /tmp/dotnet-install.sh --channel 9.0 --runtime dotnet --install-dir /usr/share/dotnet
fi

# --- .NET SDKs (needed for dotnet tool install) ---
if ! dotnet --list-sdks 2>/dev/null | grep -q '^8\.'; then
  /tmp/dotnet-install.sh --channel LTS --install-dir /usr/share/dotnet
fi
if ! dotnet --list-sdks 2>/dev/null | grep -q '^9\.'; then
  /tmp/dotnet-install.sh --channel 9.0 --install-dir /usr/share/dotnet
fi

# Add global symlink + env
sudo ln -sf /usr/share/dotnet/dotnet /usr/local/bin/dotnet
export DOTNET_ROOT=/usr/share/dotnet
export PATH="$PATH:$HOME/.dotnet/tools"

# Install/update dotnet-format
dotnet tool install -g dotnet-format || dotnet tool update -g dotnet-format

dotnet --list-runtimes
dotnet --list-sdks
dotnet format --version



# --- Godot (dev/alpha/beta via GODOT_URL) ---
# Example: https://github.com/godotengine/godot-builds/releases/download/4.5-dev5/Godot_v4.5-dev5_mono_linux_x86_64.zip
: "${GODOT_URL:?Set GODOT_URL to a Godot build zip (mono/linux/x86_64 or *_headless.zip)}"

tmpzip="$(mktemp /tmp/godot.XXXX.zip)"
echo "Downloading Godot from $GODOT_URL"
curl -fL --retry 5 --retry-delay 2 "$GODOT_URL" -o "$tmpzip"

dest="/opt/godot/$(date +%s)"
sudo mkdir -p "$dest"
sudo unzip -oq "$tmpzip" -d "$dest"
rm -f "$tmpzip"

# Find the binary anywhere under the extracted folder
GODOT_BIN="$(sudo find "$dest" -type f \( -name 'Godot_*linux_x86_64*' -o -name 'Godot*.x86_64' \) | head -n1)"

# Ensure executable
if [ -n "$GODOT_BIN" ]; then
  sudo chmod +x "$GODOT_BIN"
  sudo ln -sf "$GODOT_BIN" /usr/local/bin/godot
  
else
  echo "Godot binary not found under $dest. Contents:" >&2
  sudo find "$dest" -maxdepth 2 -print >&2
  exit 1
fi
# Stable path for all users
sudo ln -sf "$GODOT_BIN" /usr/local/bin/godot

# Export for current shell
export GODOT_BIN=/usr/local/bin/godot

# Persist for future shells (Linux)
sudo tee /etc/profile.d/godot-bin.sh >/dev/null <<'EOF'
export GODOT_BIN=/usr/local/bin/godot
EOF
sudo chmod 644 /etc/profile.d/godot-bin.sh

# CI-friendly: if env file mechanisms exist, write them too
[ -n "${GITHUB_ENV:-}" ] && echo "GODOT_BIN=/usr/local/bin/godot" >> "$GITHUB_ENV"
[ -n "${BASH_ENV:-}" ] && echo 'export GODOT_BIN=/usr/local/bin/godot' >> "$BASH_ENV"


godot --version

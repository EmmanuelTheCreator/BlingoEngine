#!/usr/bin/env bash
set -e

export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$SCRIPT_DIR/.."
PROJECT="$ROOT_DIR/Demo/TetriGrounds/LingoEngine.Demo.TetriGrounds.SDL2/LingoEngine.Demo.TetriGrounds.SDL2.csproj"
OUT_DIR="$ROOT_DIR/Publish/Tetrigrounds-RasberryPi-64"
ARCH=arm64

mkdir -p "$OUT_DIR"

dotnet publish "$PROJECT" -c Release -r linux-arm64 --self-contained false -o "$OUT_DIR"

cat >"$OUT_DIR/install.sh" <<EOF
#!/usr/bin/env bash
set -e

curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --runtime dotnet --architecture $ARCH
EOF

chmod +x "$OUT_DIR/install.sh"

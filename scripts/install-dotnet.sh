#!/usr/bin/env bash
set -e

# Ensure $HOME/.dotnet is on PATH
if [[ ":$PATH:" != *":$HOME/.dotnet:"* ]]; then
  export PATH="$HOME/.dotnet:$PATH"
fi

if command -v dotnet >/dev/null 2>&1; then
  echo ".NET SDK already installed: $(dotnet --version)"
  exit 0
fi

# Download and run Microsoft's install script
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel LTS
rm dotnet-install.sh

echo "Installation complete. \$HOME/.dotnet has been added to your PATH."

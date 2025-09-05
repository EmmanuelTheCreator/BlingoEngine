#!/usr/bin/env bash
set -e

# Ensure $HOME/.dotnet is on PATH
if [[ ":$PATH:" != *":$HOME/.dotnet:"* ]]; then
  export PATH="$HOME/.dotnet:$PATH"
fi

if command -v dotnet >/dev/null 2>&1; then
  if DOTNET_VERSION=$(dotnet --version 2>/dev/null); then
    DOTNET_MAJOR="${DOTNET_VERSION%%.*}"
    if [[ "$DOTNET_MAJOR" -ge 8 ]]; then
      echo ".NET SDK already installed: $DOTNET_VERSION"
      exit 0
    fi
  fi
  echo "Existing dotnet installation is insufficient. Installing required SDKs..."
fi

# Download and run Microsoft's install script
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
# Install the latest LTS (currently .NET 8) and .NET 9 SDKs
./dotnet-install.sh --channel LTS
./dotnet-install.sh --channel 9.0
rm dotnet-install.sh

echo "Installation complete. \$HOME/.dotnet has been added to your PATH."

#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

if [ $# -lt 1 ]; then
  echo "Usage: $0 <version> [release-notes-file]" >&2
  exit 1
fi

VERSION="$1"
NOTES_FILE="${2:-}"

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")"/.. && pwd)"
SRC_DIR="$ROOT_DIR/src"
OUT_DIR="$ROOT_DIR/Publish/packages"

command -v gh >/dev/null 2>&1 || { echo "GitHub CLI 'gh' is required" >&2; exit 1; }

mkdir -p "$OUT_DIR"

echo "Packing projects under $SRC_DIR" >&2
find "$SRC_DIR" -name '*.csproj' -print0 | while IFS= read -r -d '' proj; do
  echo "Packing $proj" >&2
  dotnet pack "$proj" -c Release -p:PackageVersion="$VERSION" -o "$OUT_DIR"
done

TAG="v$VERSION"
ASSETS=("$OUT_DIR"/*.nupkg)

if [ "${NOTES_FILE}" != "" ]; then
  gh release create "$TAG" "${ASSETS[@]}" -t "$TAG" -F "$NOTES_FILE"
else
  gh release create "$TAG" "${ASSETS[@]}" -t "$TAG" -n "Release $VERSION"
fi


#!/bin/sh
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BIN_DIR="$SCRIPT_DIR/bin/Debug/net8.0"
mkdir -p "$BIN_DIR"
ARCH_DIR="SDL2_WIN_X64"
if [ "$(uname -m)" != "x86_64" ]; then ARCH_DIR="SDL2_WIN_X86"; fi
SRC_DIR="$SCRIPT_DIR/../../../../Libs/$ARCH_DIR"
if [ -d "$SRC_DIR" ]; then
  cp "$SRC_DIR"/* "$BIN_DIR"/ 2>/dev/null || true
fi

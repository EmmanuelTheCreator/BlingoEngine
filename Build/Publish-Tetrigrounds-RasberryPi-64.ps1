# Requires PowerShell 5+
$ErrorActionPreference = "Stop"

$rootDir = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $rootDir "Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.SDL2/BlingoEngine.Demo.TetriGrounds.SDL2.csproj"
$outDir = Join-Path $rootDir "Publish/Tetrigrounds-RasberryPi-64"
$arch = "arm64"

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

dotnet publish $project -c Release -r linux-arm64 --self-contained false -o $outDir

$installScript = @"
#!/usr/bin/env bash
set -e

curl -fsSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --runtime dotnet --architecture $arch
"@
$installPath = Join-Path $outDir "install.sh"
Set-Content -Path $installPath -Value $installScript

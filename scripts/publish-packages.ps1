#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory=$true)][string]$Version,
    [string]$ReleaseNotes
)

$ErrorActionPreference = "Stop"

$rootDir = Resolve-Path (Join-Path $PSScriptRoot "..")
$srcDir = Join-Path $rootDir "src"
$outDir = Join-Path $rootDir "Publish/packages"

if (-not (Get-Command "gh" -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI 'gh' is required"
}

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

Write-Host "Packing projects under $srcDir"
Get-ChildItem $srcDir -Recurse -Filter *.csproj | ForEach-Object {
    Write-Host "Packing $($_.FullName)"
    dotnet pack $_.FullName -c Release -p:PackageVersion=$Version -o $outDir
}

$tag = "v$Version"
$assets = Get-ChildItem $outDir -Filter *.nupkg | ForEach-Object { $_.FullName }

if ($ReleaseNotes) {
    gh release create $tag $assets -t $tag -F $ReleaseNotes
} else {
    gh release create $tag $assets -t $tag -n "Release $Version"
}

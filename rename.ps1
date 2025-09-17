# Rename-BlingoToBlingo.ps1
param([string]$Path = ".")

Write-Host "Starting rename in: $Path"

# Exclude dirs: .git, bin, obj, IDE/caches, pkg caches, build outputs, dist, Blazor bootstrap
$excludeDirRe = '\\(\.git|bin|obj|\.vs|\.idea|\.vscode|node_modules|packages|artifacts|build|dist|wwwroot\\dist|bootstrap)($|\\)'

# Text/code extensions to edit (skip binaries)
$textExt = @(
  '.sln','.csproj','.props','.targets','.cs','.razor',
  '.json','.yml','.yaml','.xml','.md','.txt','.csv','.editorconfig',
  '.gitignore','.gitattributes','.ps1','.bat','.cmd','.sh',
  '.ts','.tsx','.js','.jsx','.css','.scss','.html'
)

# ---------- 1) Update file contents ----------
# Rule: keep standalone "Lingo"/"lingo" words; only change when part of a larger token (e.g., BlingoEngine).
# Also avoid double-rename like "Bblingo" or "BBlingo".
Get-ChildItem -Path $Path -Recurse -File |
  Where-Object {
    $_.FullName -notmatch $excludeDirRe -and
    ($textExt -contains $_.Extension.ToLower())
  } |
  ForEach-Object {
    $p = $_.FullName
    try { $c = Get-Content -LiteralPath $p -Raw -ErrorAction Stop } catch { Write-Host "Skip (read error): $p"; return }
    if ($null -eq $c) { return }

    # Uppercase: replace only when adjacent to word chars; don't touch standalone "Lingo"; don't touch "Blingo"
    $new = [regex]::Replace($c, '(?<![Bb])(?:(?<=\w)Lingo|Lingo(?=\w))', 'Blingo')
    # Lowercase: same logic
    $new = [regex]::Replace($new, '(?<![Bb])(?:(?<=\w)lingo|lingo(?=\w))', 'blingo')

    if ($new -ne $c) {
      Set-Content -LiteralPath $p -Value $new -Encoding UTF8
      Write-Host "Updated content: $p"
    }
  }

# ---------- 2) Rename files (deepest first) ----------
# For names: rename all occurrences, but avoid creating "Bblingo"/"bblingo".
Get-ChildItem -Path $Path -Recurse -File |
  Where-Object { $_.FullName -notmatch $excludeDirRe } |
  Sort-Object FullName -Descending |
  ForEach-Object {
    $name = $_.Name
    $newName = $name `
      -replace '(?<![Bb])Lingo','Blingo' `
      -replace '(?<![b])lingo','blingo'
    if ($newName -ne $name) {
      Rename-Item -LiteralPath $_.FullName -NewName $newName
      Write-Host "Renamed file: $($_.FullName) -> $newName"
    }
  }

# ---------- 3) Rename folders (deepest first) ----------
Get-ChildItem -Path $Path -Recurse -Directory |
  Where-Object { $_.FullName -notmatch $excludeDirRe } |
  Sort-Object FullName -Descending |
  ForEach-Object {
    $name = $_.Name
    $newName = $name `
      -replace '(?<![Bb])Lingo','Blingo' `
      -replace '(?<![b])lingo','blingo'
    if ($newName -ne $name) {
      Rename-Item -LiteralPath $_.FullName -NewName $newName
      Write-Host "Renamed folder: $($_.FullName) -> $newName"
    }
  }

Write-Host "Done."


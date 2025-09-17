param([string]$VersionTag)

function Get-VersionFromCandidate {
    param([string]$Candidate)

    if([string]::IsNullOrWhiteSpace($Candidate)){
        return $null
    }

    $value = $Candidate.Trim()

    if($value.Length -eq 0){
        return $null
    }

    $value = $value -replace '\\"', [string][char]34
    $value = $value -replace "\\'", [string][char]39
    $value = $value -replace '\\', '\'

    [char[]]$quoteTrim = @([char]34, [char]39, [char]96, [char]0x201C, [char]0x201D)
    $value = $value.Trim($quoteTrim).Trim()
    $value = $value.TrimEnd('.', ',', ';').Trim()

    if($value.Length -eq 0){
        return $null
    }

    $keyMatch = [regex]::Match($value, '[:=]\s*(?<rest>[^:=]+)$')
    if($keyMatch.Success){
        $rest = $keyMatch.Groups['rest'].Value.Trim()
        if($rest -match '\d'){
            $value = $rest
        }
    }

    if($value.IndexOfAny(@([char]47, [char]92)) -ge 0){
        $segments = $value -split '[\\/]'
        if($segments.Count -gt 0){
            $value = $segments[$segments.Count - 1]
        }
    }

    $value = $value.Trim()

    $value = [System.Text.RegularExpressions.Regex]::Replace(
        $value,
        '(?i)\.(?:zip|exe|msi|dmg|pkg|appimage|tar\.gz|tar\.xz)$',
        ''
    )

    $match = [regex]::Match($value, '^Godot_v(?<rest>.+)$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if($match.Success){
        $value = $match.Groups['rest'].Value.Trim()
    }else{
        $match = [regex]::Match($value, '^Godot\.NET\.Sdk/(?<rest>.+)$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if($match.Success){
            $value = $match.Groups['rest'].Value.Trim()
        }else{
            $match = [regex]::Match($value, '^Godot\.(?<rest>\d.+)$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
            if($match.Success){
                $value = $match.Groups['rest'].Value.Trim()
            }
        }
    }

    if($value.Length -gt 1 -and ($value[0] -eq 'v' -or $value[0] -eq 'V') -and [char]::IsDigit($value[1])){
        $value = $value.Substring(1)
    }

    if($value.Contains('_')){
        foreach($segment in $value.Split('_')){
            if(-not [string]::IsNullOrWhiteSpace($segment) -and $segment -match '\d'){
                $value = $segment
                break
            }
        }
    }

    $versionPattern = '^\d+(?:\.\d+){1,3}(?:-[0-9A-Za-z]+(?:\.[0-9A-Za-z]+)*)?'

    if([regex]::IsMatch($value, $versionPattern)){
        return $value
    }

    $match = [regex]::Match($value, $versionPattern)
    if($match.Success){
        return $match.Value
    }

    return $null
}

function Get-NormalizedVersionTag {
    param([string]$RawInput)

    if([string]::IsNullOrWhiteSpace($RawInput)){
        return $null
    }

    $value = $RawInput.Trim()

    if($value.Length -eq 0){
        return $null
    }

    $value = $value -replace '\\"', [string][char]34
    $value = $value -replace "\\'", [string][char]39
    $value = $value -replace '\\', '\'

    [char[]]$quoteTrim = @([char]34, [char]39, [char]96, [char]0x201C, [char]0x201D)
    $value = $value.Trim($quoteTrim).Trim()
    [char[]]$trailingTrim = @([char]44, [char]59)
    $value = $value.TrimEnd($trailingTrim).Trim()

    if($value.Length -eq 0){
        return $null
    }

    $keyMatch = [regex]::Match($value, "^\s*[""']?[A-Za-z0-9_.-]+[""']?\s*[:=]\s*(?<rest>.+)$")
    if($keyMatch.Success){
        $rest = $keyMatch.Groups['rest'].Value.Trim()
        if($rest.Length -gt 0){
            $value = $rest
        }
    }

    if($value.Length -eq 0){
        return $null
    }

    $patterns = @(
        'Godot_v(?<candidate>[^\s`"''/\\]+)',
        'Godot\.NET\.Sdk/(?<candidate>[^\s`"''/\\]+)',
        '\bv(?<candidate>\d[^\s`"''/\\]+)',
        '\b(?<candidate>\d[^\s`"''/\\]+)'
    )

    foreach($pattern in $patterns){
        foreach($match in [regex]::Matches($value, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)){
            $candidate = $match.Groups['candidate'].Value
            $version = Get-VersionFromCandidate $candidate
            if($version){
                return $version
            }
        }
    }

    $tokens = [System.Text.RegularExpressions.Regex]::Split($value, "[\s""',;(){}\[\]]+")
    foreach($token in $tokens){
        $version = Get-VersionFromCandidate $token
        if($version){
            return $version
        }
    }

    return Get-VersionFromCandidate $value
}

function Get-NuGetVersionFromTag {
    param([string]$Tag)

    if([string]::IsNullOrWhiteSpace($Tag)){
        throw "Godot version tag cannot be empty."
    }

    $parts = $Tag -split '-', 2
    $basePart = $parts[0]
    $preReleasePart = if($parts.Count -gt 1){ $parts[1] } else { $null }

    $baseSegments = @($basePart -split '\.')
    $normalizedBase = @()
    foreach($segment in $baseSegments){
        if($segment -eq ''){ continue }
        $number = 0
        if([int]::TryParse($segment, [ref]$number)){
            $normalizedBase += $number.ToString()
        }else{
            $normalizedBase += $segment
        }
    }
    while($normalizedBase.Count -lt 3){
        $normalizedBase += '0'
    }
    if($normalizedBase.Count -gt 3){
        $normalizedBase = $normalizedBase[0..2]
    }
    $baseVersion = [string]::Join('.', $normalizedBase)

    $preReleaseVersion = $null
    if($preReleasePart){
        $segments = @()
        foreach($segment in ($preReleasePart -split '\.')){
            if($segment -eq ''){ continue }
            $matches = [regex]::Matches($segment, '[A-Za-z]+|\d+')
            foreach($match in $matches){
                if($match.Value){
                    $segments += $match.Value
                }
            }
        }
        if($segments.Count -gt 0){
            $preReleaseVersion = [string]::Join('.', $segments)
        }
    }

    if($preReleaseVersion){
        return '{0}-{1}' -f $baseVersion, $preReleaseVersion
    }

    return $baseVersion
}

if(-not $VersionTag){
    $VersionTag = Read-Host 'Enter Godot version (e.g., 4.5-dev5)'
}

$VersionTag = Get-NormalizedVersionTag $VersionTag
if(-not $VersionTag){
    throw 'Unable to determine Godot version from the provided input.'
}

$NugetVersion = Get-NuGetVersionFromTag $VersionTag
# Update .csproj files
$csprojs = Get-ChildItem -Recurse -Filter *.csproj | Where-Object { Select-String -Path $_.FullName -Pattern 'Godot.NET.Sdk' -Quiet }
foreach($file in $csprojs){
    $content = Get-Content $file.FullName
    $content = $content -replace 'Godot.NET.Sdk/[^"\s]+', "Godot.NET.Sdk/$NugetVersion"
    $content = $content -replace '<GodotVersion>[^<]+</GodotVersion>', "<GodotVersion>$NugetVersion</GodotVersion>"
    $content = $content -replace '<PackageReference Include="Godot.NET.Sdk" Version="[^"\s]+" />', ('<PackageReference Include="Godot.NET.Sdk" Version="{0}" />' -f $NugetVersion)
    Set-Content -Encoding UTF8 $file.FullName $content
    Write-Host ("Updated Godot version in {0}" -f $file.FullName)
}
# Update launchSettings.json files
$execVersion = "v$VersionTag"
$launchFiles = Get-ChildItem -Recurse -Filter launchSettings.json
foreach($file in $launchFiles){
    $content = Get-Content $file.FullName
    $content = $content -replace 'Godot_v[^_"\\]+', "Godot_$execVersion"
    $content = $content -replace 'Godot\.[0-9A-Za-z\.-]+', "Godot.$VersionTag"
    Set-Content -Encoding UTF8 $file.FullName $content
    Write-Host ("Updated Godot version in {0}" -f $file.FullName)
}
Write-Host ('Godot version updated to {0} (NuGet: {1})' -f $VersionTag, $NugetVersion)

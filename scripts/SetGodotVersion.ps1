param([string]$VersionTag)

function Get-NormalizedVersionTag {
    param([string]$Input)

    if([string]::IsNullOrWhiteSpace($Input)){
        return $null
    }

    $value = $Input.Trim()

    if($value -match 'Godot_v([^_]+)_'){
        $value = $Matches[1]
    }

    if($value -match '^v(.+)$'){
        $value = $Matches[1]
    }

    return $value
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

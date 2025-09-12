param([string]$VersionTag)
if(-not $VersionTag){
    $VersionTag = Read-Host 'Enter Godot version (e.g., 4.5-dev5)'
}
if($VersionTag -match '^(\d+\.\d+)-(.*)$'){
    $NugetVersion = "$($Matches[1]).0-" + ($Matches[2] -replace '-', '.')
}else{
    $NugetVersion = $VersionTag
}
# Update .csproj files
$csprojs = Get-ChildItem -Recurse -Filter *.csproj | Where-Object { Select-String -Path $_.FullName -Pattern 'Godot.NET.Sdk' -Quiet }
foreach($file in $csprojs){
    $content = Get-Content $file.FullName
    $content = $content -replace 'Godot.NET.Sdk/[^"\s]+', "Godot.NET.Sdk/$NugetVersion"
    $content = $content -replace '<GodotVersion>[^<]+</GodotVersion>', "<GodotVersion>$NugetVersion</GodotVersion>"
    $content = $content -replace '<PackageReference Include="Godot.NET.Sdk" Version="[^"\s]+" />', "<PackageReference Include="Godot.NET.Sdk" Version="$NugetVersion" />"
    Set-Content -Encoding UTF8 $file.FullName $content
}
# Update launchSettings.json files
$execVersion = "v$VersionTag"
$launchFiles = Get-ChildItem -Recurse -Filter launchSettings.json
foreach($file in $launchFiles){
    $content = Get-Content $file.FullName
    $content = $content -replace 'Godot_v[^_"\\]+', "Godot_$execVersion"
    $content = $content -replace 'Godot\.[0-9A-Za-z\.-]+', "Godot.$VersionTag"
    Set-Content -Encoding UTF8 $file.FullName $content
}
Write-Host ('Godot version updated to {0} (NuGet: {1})' -f $VersionTag, $NugetVersion)

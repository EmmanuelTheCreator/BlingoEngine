@echo off
REM LingoEngine one-click setup script for Windows.
REM This script is intended to remain a simple way to prepare the repository.
REM Future updates should keep this goal in mind.

setlocal enabledelayedexpansion
set "EXIT_CODE=0"
set "ROOT=%~dp0"
cd /d "%ROOT%"

set "ARCH=%PROCESSOR_ARCHITECTURE%"
if /i "%ARCH%"=="AMD64" (
    set "LIB_DIR=%ROOT%Libs\SDL2_WIN_X64"
) else if /i "%ARCH%"=="x86" (
    set "LIB_DIR=%ROOT%Libs\SDL2_WIN_X86"
) else if /i "%ARCH%"=="ARM64" (
    set "LIB_DIR=%ROOT%Libs\SDL2_WIN_ARM64"
) else if /i "%ARCH%"=="ARM" (
    set "LIB_DIR=%ROOT%Libs\SDL2_WIN_ARM"
) else (
    set "LIB_DIR="
)

set PROJECTS="Demo\TetriGrounds\LingoEngine.Demo.TetriGrounds.SDL2" "Test\LingoEngine.SDL2.GfxVisualTest" "WillMoveToOwnRepo\AbstUI\Test\AbstUI.GfxVisualTest.SDL2" "src\Director\LingoEngine.Director.Runner.SDL2"
set "MEDIA_SRC=%ROOT%Demo\TetriGrounds\LingoEngine.Demo.TetriGrounds.Godot\Media"
set "MEDIA_DEST=%ROOT%Demo\TetriGrounds\LingoEngine.Demo.TetriGrounds.Blazor\Media"

echo This script will:
echo   - Ensure the .NET 8 SDK is installed.
if defined LIB_DIR (
    echo   - Copy SDL2 native libraries for %ARCH% from %LIB_DIR% into:
    for %%P in (%PROJECTS%) do echo       - %%~P\bin\^<config^>\net8.0
) else (
    echo   - SDL2 libraries will not be copied ^(unsupported architecture %ARCH%^).
)
echo   - Copy demo media from %MEDIA_SRC% to %MEDIA_DEST%.
echo   - At the end you will be asked to locate your Godot executable.
echo     When the file dialog appears, select the Godot 4 executable to use (minimum version 4.5).
echo     Example: C:\path\to\Godot_v4.5-stable_mono_win64.exe
echo.
echo Press any key to continue or Ctrl+C to abort.
pause >nul

where dotnet >nul 2>&1
if errorlevel 1 (
    echo .NET SDK not found. Attempting installation via winget...
    winget install --id Microsoft.DotNet.SDK.8 --source winget
    if errorlevel 1 (
        echo Failed to install .NET SDK automatically. Please install it manually and re-run this script.
        set "EXIT_CODE=1"
        goto END
    )
) else (
    for /f %%v in ('dotnet --version') do set "DOTNET_VER=%%v"
    echo .NET SDK found: !DOTNET_VER!
)

if defined LIB_DIR (
    if not exist "%LIB_DIR%" (
        echo No SDL2 libraries found for %ARCH% in %LIB_DIR%.
    ) else (
        echo Copying SDL2 libraries for %ARCH%...
        for %%P in (%PROJECTS%) do (
            if exist "%ROOT%%%~P" (
                for %%C in (Debug DebugWithDirector Release) do (
                    if not exist "%ROOT%%%~P\bin\%%C\net8.0" mkdir "%ROOT%%%~P\bin\%%C\net8.0"
                    copy /Y "%LIB_DIR%\*" "%ROOT%%%~P\bin\%%C\net8.0" >nul
                )
            )
        )
    )
)

if exist "%MEDIA_SRC%" (
    echo Copying demo media...
    robocopy "%MEDIA_SRC%" "%MEDIA_DEST%" /MIR >nul
) else (
    echo Demo media source not found: %MEDIA_SRC%
)

echo.
echo Select the Godot executable (e.g., C:\path\to\Godot_v4.5-stable_mono_win64.exe). Minimum version 4.5 required.
powershell -NoProfile -ExecutionPolicy Bypass -Command "Add-Type -AssemblyName System.Windows.Forms; $dlg=New-Object System.Windows.Forms.OpenFileDialog; $dlg.Filter='Godot executable|Godot*.exe'; if($dlg.ShowDialog() -eq 'OK'){ $path=$dlg.FileName; $settingsPath='.vscode/settings.json'; $json=Get-Content $settingsPath | ConvertFrom-Json; $json.'godotTools.editorPath.godot4'=$path; $json | ConvertTo-Json | Set-Content $settingsPath; $ver=[regex]::Match((Split-Path $path -Leaf), 'Godot_v([^_]+)_').Groups[1].Value; & 'scripts\\SetGodotVersion.ps1' $ver } else { Write-Host 'No Godot executable selected.' }"
set "EXIT_CODE=%ERRORLEVEL%"

:END
if %EXIT_CODE% equ 0 (
    echo Setup complete.
) else (
    echo Setup encountered errors.
)
pause
endlocal & exit /b %EXIT_CODE%

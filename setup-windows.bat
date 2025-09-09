@echo off
REM LingoEngine one-click setup script for Windows.
REM This script is intended to remain a simple way to prepare the repository.
REM Future updates should keep this goal in mind.

setlocal enabledelayedexpansion
set "ROOT=%~dp0"
cd /d "%ROOT%"

echo This script will:
echo   - Ensure the .NET 8 SDK is installed.
echo   - Copy SDL2 native libraries into build output folders for the TetriGrounds SDL demo and SDL-based test projects.
echo.
echo Press any key to continue or Ctrl+C to abort.
pause >nul

where dotnet >nul 2>&1
if errorlevel 1 (
    echo .NET SDK not found. Attempting installation via winget...
    winget install --id Microsoft.DotNet.SDK.8 --source winget
    if errorlevel 1 (
        echo Failed to install .NET SDK automatically. Please install it manually and re-run this script.
        exit /b 1
    )
) else (
    for /f %%v in ('dotnet --version') do set "DOTNET_VER=%%v"
    echo .NET SDK found: !DOTNET_VER!
)

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

if not defined LIB_DIR (
    echo Unsupported or unknown architecture: %ARCH%
    goto :EOF
)

if not exist "%LIB_DIR%" (
    echo No SDL2 libraries found for %ARCH% in %LIB_DIR%.
    goto :EOF
)

echo Copying SDL2 libraries for %ARCH%...
set PROJECTS="Demo\TetriGrounds\LingoEngine.Demo.TetriGrounds.SDL2" "Test\LingoEngine.SDL2.GfxVisualTest" "WillMoveToOwnRepo\AbstUI\Test\AbstUI.GfxVisualTest.SDL2"
for %%P in (%PROJECTS%) do (
    if exist "%ROOT%%%~P" (
        for %%C in (Debug Release) do (
            if not exist "%ROOT%%%~P\bin\%%C\net8.0" mkdir "%ROOT%%%~P\bin\%%C\net8.0"
            copy /Y "%LIB_DIR%\*" "%ROOT%%%~P\bin\%%C\net8.0" >nul
        )
    )
)

echo Setup complete.
endlocal

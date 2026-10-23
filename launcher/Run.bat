@echo off
chcp 65001 >nul

echo.
echo No Rest for the Potato - Launcher
echo.

where powershell >nul 2>&1
if errorlevel 1 (
    echo ERROR: PowerShell not found in PATH
    pause
    exit /b 1
)

set "SCRIPT_DIR=%~dp0"
set "PS1_FILE=%SCRIPT_DIR%Launcher.ps1"

if not exist "%PS1_FILE%" (
    echo ERROR: Launcher.ps1 not found in script directory
    echo Expected: %PS1_FILE%
    pause
    exit /b 1
)

echo Found script: %PS1_FILE%
echo Launching PowerShell...
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "& '%PS1_FILE%'"

if errorlevel 1 (
    echo.
    echo PowerShell exited with error code %errorlevel%
) else (
    echo.
    echo Done.
)
pause
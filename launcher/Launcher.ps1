# No Rest for the Potato — Launcher
# Manages graphics settings via external config file (potato_graphics.cfg)
# Mod reads this file and applies settings in-game

$ErrorActionPreference = "Continue"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\NoRestForTheWicked"
$GameExe = Join-Path $GameDir "NoRestForTheWicked.exe"
$ModsDir = Join-Path $GameDir "Mods"
$ConfigFile = Join-Path $ScriptDir "potato_graphics.cfg"
$ModDllSource = Join-Path $ScriptDir "Mods\NRTPMod.dll"

Write-Host "=== No Rest for the Potato - Launcher ===" -ForegroundColor Cyan
Write-Host ""

# Check game directory
if (-not (Test-Path $GameDir)) {
    Write-Host "ERROR: Game directory not found: $GameDir" -ForegroundColor Red
    Write-Host "Edit GameDir in Launcher.ps1 to match your installation path." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

if (-not (Test-Path $GameExe)) {
    Write-Host "ERROR: Game executable not found: $GameExe" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Function: read config file
function Get-Config {
    $settings = @{}
    if (-not (Test-Path $ConfigFile)) {
        Write-Host "Config file not found, will create defaults" -ForegroundColor Yellow
        return $settings
    }
    try {
        foreach ($line in Get-Content $ConfigFile) {
            if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) { continue }
            $parts = $line.Split("=", 2)
            if ($parts.Length -eq 2) {
                $settings[$parts[0].Trim()] = $parts[1].Trim()
            }
        }
    } catch {
        Write-Host "WARNING: Could not read config file" -ForegroundColor Yellow
    }
    return $settings
}

# Function: save config file
function Save-Config($settings) {
    try {
        $lines = New-Object System.Collections.Generic.List[string]
        $lines.Add("# No Rest for the Potato - Graphics Settings")
        $lines.Add("# Edit this file to change graphics settings before launching the game")
        $lines.Add("# Then save and launch the game - settings apply automatically")
        $lines.Add("")
        foreach ($key in $settings.Keys) {
            $lines.Add("$key=$($settings[$key])")
        }
        Set-Content -Path $ConfigFile -Value $lines -Encoding UTF8
        Write-Host "Config saved: $ConfigFile" -ForegroundColor Green
    } catch {
        Write-Host "ERROR: Failed to save config: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Function: enable/disable mod (copy/remove DLL in Mods folder)
function Set-ModEnabled($enabled) {
    if (-not (Test-Path $ModsDir)) {
        New-Item -ItemType Directory -Path $ModsDir -Force | Out-Null
    }

    $modTarget = Join-Path $ModsDir "NRTPMod.dll"

    if ($enabled) {
        if (Test-Path $ModDllSource) {
            Copy-Item -Path $ModDllSource -Destination $modTarget -Force
            Write-Host "Mod enabled: NRTPMod.dll copied to Mods" -ForegroundColor Green
        } else {
            Write-Host "ERROR: Mod file not found: $ModDllSource" -ForegroundColor Red
        }
    } else {
        if (Test-Path $modTarget) {
            Remove-Item -Path $modTarget -Force
            Write-Host "Mod disabled: NRTPMod.dll removed from Mods" -ForegroundColor Yellow
        }
    }
}

# Presets
$presets = @{
    UltraLow = @{
        Name = "Ultra Low"
        ShadowsEnabled = $false
        ShadowDistance = 20
        ShadowCascades = 0
        ShadowResolution = 0
        TextureQuality = 2
        AntiAliasing = 0
        Anisotropic = $false
        MotionBlur = $false
        FogEnabled = $false
        ReflectionsEnabled = $false
        PostProcessingEnabled = $false
        VolumetricLightingEnabled = $false
        CloudShadowsEnabled = $false
        AmbientOcclusionEnabled = $false
        BloomEnabled = $false
        DepthOfFieldEnabled = $false
        HdrEnabled = $false
        ModEnabled = $true
    }
    Low = @{
        Name = "Low"
        ShadowsEnabled = $false
        ShadowDistance = 20
        ShadowCascades = 0
        ShadowResolution = 0
        TextureQuality = 2
        AntiAliasing = 0
        Anisotropic = $false
        MotionBlur = $false
        FogEnabled = $false
        ReflectionsEnabled = $false
        PostProcessingEnabled = $false
        VolumetricLightingEnabled = $false
        CloudShadowsEnabled = $false
        AmbientOcclusionEnabled = $false
        BloomEnabled = $false
        DepthOfFieldEnabled = $false
        HdrEnabled = $false
        ModEnabled = $true
    }
    Medium = @{
        Name = "Medium"
        ShadowsEnabled = $true
        ShadowDistance = 40
        ShadowCascades = 1
        ShadowResolution = 0
        TextureQuality = 1
        AntiAliasing = 2
        Anisotropic = $true
        MotionBlur = $true
        FogEnabled = $true
        ReflectionsEnabled = $false
        PostProcessingEnabled = $false
        VolumetricLightingEnabled = $false
        CloudShadowsEnabled = $false
        AmbientOcclusionEnabled = $true
        BloomEnabled = $false
        DepthOfFieldEnabled = $false
        HdrEnabled = $true
        ModEnabled = $true
    }
    High = @{
        Name = "High"
        ShadowsEnabled = $true
        ShadowDistance = 80
        ShadowCascades = 3
        ShadowResolution = 2
        TextureQuality = 0
        AntiAliasing = 4
        Anisotropic = $true
        MotionBlur = $true
        FogEnabled = $true
        ReflectionsEnabled = $true
        PostProcessingEnabled = $true
        VolumetricLightingEnabled = $true
        CloudShadowsEnabled = $true
        AmbientOcclusionEnabled = $true
        BloomEnabled = $true
        DepthOfFieldEnabled = $true
        HdrEnabled = $true
        ModEnabled = $false
    }
    Default = @{
        Name = "Default"
        ShadowsEnabled = $true
        ShadowDistance = 50
        ShadowCascades = 2
        ShadowResolution = 2
        TextureQuality = 0
        AntiAliasing = 2
        Anisotropic = $true
        MotionBlur = $true
        FogEnabled = $true
        ReflectionsEnabled = $true
        PostProcessingEnabled = $true
        VolumetricLightingEnabled = $true
        CloudShadowsEnabled = $true
        AmbientOcclusionEnabled = $true
        BloomEnabled = $true
        DepthOfFieldEnabled = $true
        HdrEnabled = $true
        ModEnabled = $false
    }
}

# Show current config if exists
$current = Get-Config
if ($current.Count -gt 0) {
    Write-Host "Current config:" -ForegroundColor Gray
    Write-Host "  ShadowsEnabled: $($current.ShadowsEnabled)"
    Write-Host "  TextureQuality: $($current.TextureQuality)"
    Write-Host "  MotionBlur: $($current.MotionBlur)"
    Write-Host "  HdrEnabled: $($current.HdrEnabled)"
    Write-Host ""
}

# Choose preset
Write-Host "Choose a preset:" -ForegroundColor Cyan
Write-Host "  1) Ultra Low   (max FPS, very blurry, mod enabled)"
Write-Host "  2) Low   (minimum graphics, mod enabled)"
Write-Host "  3) Medium (balanced, mod enabled)"
Write-Host "  4) High   (full quality, mod disabled)"
Write-Host "  5) Default (game defaults, mod disabled)"
Write-Host ""
$choice = Read-Host "Enter number (1-5)"

switch ($choice) {
    "1" { $preset = $presets.UltraLow }
    "2" { $preset = $presets.Low }
    "3" { $preset = $presets.Medium }
    "4" { $preset = $presets.High }
    "5" { $preset = $presets.Default }
    default { $preset = $presets.UltraLow }
}

# Apply preset to config
Write-Host ""
Write-Host "Applying preset: $($preset.Name)" -ForegroundColor Green

$config = Get-Config
foreach ($key in $preset.Keys) {
    if ($key -ne "Name") {
        $config[$key] = $preset[$key]
    }
}
Save-Config $config

# Enable/disable mod
Set-ModEnabled $preset.ModEnabled

# Launch game
Write-Host ""
Write-Host "Launching game..." -ForegroundColor Cyan
Start-Process -FilePath $GameExe -WorkingDirectory $GameDir -Wait

Write-Host ""
Write-Host "Game exited." -ForegroundColor Cyan
Read-Host "Press Enter to exit"
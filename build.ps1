<#
.SYNOPSIS
    Build script for TidyPackRat

.DESCRIPTION
    Builds the GUI application and prepares files for installer creation

.PARAMETER Configuration
    Build configuration (Debug or Release)

.PARAMETER SkipGUI
    Skip building the GUI application

.PARAMETER BuildInstaller
    Also build the WiX installer (requires WiX Toolset)

.EXAMPLE
    .\build.ps1 -Configuration Release

.EXAMPLE
    .\build.ps1 -Configuration Release -BuildInstaller
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$SkipGUI,

    [switch]$BuildInstaller
)

$ErrorActionPreference = "Stop"

# Paths
$rootDir = $PSScriptRoot
$guiProject = Join-Path $rootDir "src\gui\TidyPackRat.csproj"
$installerProject = Join-Path $rootDir "src\installer\TidyPackRat.Installer.wixproj"
$buildDir = Join-Path $rootDir "build"
$guiBuildOutput = Join-Path $rootDir "src\gui\bin\$Configuration"

Write-Host "TidyPackRat Build Script" -ForegroundColor Cyan
Write-Host "========================`n" -ForegroundColor Cyan

# Check for MSBuild
$msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
if (-not $msbuild) {
    Write-Host "MSBuild not found in PATH" -ForegroundColor Red
    Write-Host "Please run this from Visual Studio Developer Command Prompt" -ForegroundColor Yellow
    Write-Host "Or add MSBuild to your PATH" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using MSBuild: $($msbuild.Source)" -ForegroundColor Green

# Build GUI Application
if (-not $SkipGUI) {
    Write-Host "`nBuilding GUI application ($Configuration)..." -ForegroundColor Cyan

    if (-not (Test-Path $guiProject)) {
        Write-Host "GUI project not found: $guiProject" -ForegroundColor Red
        exit 1
    }

    # Restore NuGet packages first
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    $nuget = Get-Command nuget -ErrorAction SilentlyContinue
    if (-not $nuget) {
        Write-Host "NuGet not found in PATH" -ForegroundColor Red
        Write-Host "Please install NuGet CLI or add it to your PATH" -ForegroundColor Yellow
        exit 1
    }
    & nuget restore "$guiProject"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "NuGet restore failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    # Build the project
    $buildArgs = @(
        "`"$guiProject`"",
        "/p:Configuration=$Configuration",
        "/p:Platform=AnyCPU",
        "/t:Rebuild",
        "/v:minimal"
    )

    & msbuild $buildArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Host "GUI build failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    Write-Host "GUI build completed successfully!" -ForegroundColor Green
}
else {
    Write-Host "Skipping GUI build" -ForegroundColor Yellow
}

# Prepare build directory
Write-Host "`nPreparing build directory..." -ForegroundColor Cyan

if (Test-Path $buildDir) {
    Remove-Item $buildDir -Recurse -Force
}

New-Item -Path $buildDir -ItemType Directory | Out-Null
New-Item -Path (Join-Path $buildDir "gui") -ItemType Directory | Out-Null
New-Item -Path (Join-Path $buildDir "worker") -ItemType Directory | Out-Null
New-Item -Path (Join-Path $buildDir "config") -ItemType Directory | Out-Null

# Copy GUI binaries
if (-not $SkipGUI) {
    Write-Host "Copying GUI binaries..." -ForegroundColor Cyan

    $guiFiles = @(
        "TidyPackRat.exe",
        "TidyPackRat.exe.config",
        "Newtonsoft.Json.dll"
    )

    foreach ($file in $guiFiles) {
        $source = Join-Path $guiBuildOutput $file
        $dest = Join-Path $buildDir "gui\$file"

        if (Test-Path $source) {
            Copy-Item $source $dest
            Write-Host "  Copied: $file" -ForegroundColor Gray
        }
        else {
            Write-Host "  Warning: $file not found" -ForegroundColor Yellow
        }
    }
}

# Copy worker script
Write-Host "Copying worker script..." -ForegroundColor Cyan
Copy-Item (Join-Path $rootDir "src\worker\TidyPackRat-Worker.ps1") `
          (Join-Path $buildDir "worker\")
Write-Host "  Copied: TidyPackRat-Worker.ps1" -ForegroundColor Gray

# Copy configuration
Write-Host "Copying configuration..." -ForegroundColor Cyan
Copy-Item (Join-Path $rootDir "config\default-config.json") `
          (Join-Path $buildDir "config\")
Write-Host "  Copied: default-config.json" -ForegroundColor Gray

Write-Host "`nBuild directory prepared: $buildDir" -ForegroundColor Green

# Build installer if requested
if ($BuildInstaller) {
    Write-Host "`nBuilding WiX installer..." -ForegroundColor Cyan

    # Check for WiX - search in common locations and PATH
    $wixCandle = $null

    # Check PATH first
    $wixCandle = Get-Command candle.exe -ErrorAction SilentlyContinue

    # Check common installation locations
    if (-not $wixCandle) {
        $wixPaths = @(
            "C:\Program Files (x86)\WiX Toolset v3.14\bin\candle.exe",
            "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe",
            "C:\Program Files\WiX Toolset v3.14\bin\candle.exe",
            "C:\Program Files\WiX Toolset v3.11\bin\candle.exe",
            "${env:WIX}bin\candle.exe"
        )

        foreach ($path in $wixPaths) {
            if (Test-Path $path) {
                $wixCandle = $path
                break
            }
        }
    }

    if (-not $wixCandle) {
        Write-Host "WiX Toolset not found!" -ForegroundColor Red
        Write-Host "Please install WiX Toolset from https://wixtoolset.org" -ForegroundColor Yellow
        Write-Host "Or ensure WiX bin directory is in your PATH" -ForegroundColor Yellow
        exit 1
    }

    Write-Host "Using WiX: $wixCandle" -ForegroundColor Green

    if (-not (Test-Path $installerProject)) {
        Write-Host "Installer project not found: $installerProject" -ForegroundColor Red
        exit 1
    }

    $buildArgs = @(
        "`"$installerProject`"",
        "/p:Configuration=$Configuration",
        "/p:Platform=x86",
        "/p:SourceDir=..\..\build",
        "/t:Rebuild",
        "/v:minimal"
    )

    & msbuild $buildArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Installer build failed!" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    $installerOutput = Join-Path $rootDir "src\installer\bin\$Configuration\TidyPackRat-Setup.msi"

    if (Test-Path $installerOutput) {
        Write-Host "`nInstaller created successfully!" -ForegroundColor Green
        Write-Host "Location: $installerOutput" -ForegroundColor Cyan

        $fileInfo = Get-Item $installerOutput
        Write-Host "Size: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Gray
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "  1. Test the application from build\gui\TidyPackRat.exe" -ForegroundColor White
Write-Host "  2. Run tests: .\tests\test-worker.ps1" -ForegroundColor White
if (-not $BuildInstaller) {
    Write-Host "  3. Build installer: .\build.ps1 -BuildInstaller" -ForegroundColor White
}

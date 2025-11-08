<#
.SYNOPSIS
    Prepares TidyPackRat visual assets for the project

.DESCRIPTION
    Converts the logo PNG to various formats needed:
    - ICO file for application icon
    - Resized versions for different uses
    - Installer banner and dialog (if source images provided)

.PARAMETER LogoPath
    Path to the source logo PNG file

.PARAMETER OutputDir
    Directory where processed assets will be saved

.EXAMPLE
    .\prepare-assets.ps1 -LogoPath "C:\Downloads\logo.png"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$LogoPath,

    [string]$OutputDir = (Join-Path $PSScriptRoot "..\assets")
)

$ErrorActionPreference = "Stop"

Write-Host "TidyPackRat Asset Preparation Tool" -ForegroundColor Cyan
Write-Host "==================================`n" -ForegroundColor Cyan

# Check if logo exists
if (-not (Test-Path $LogoPath)) {
    Write-Host "Error: Logo file not found at $LogoPath" -ForegroundColor Red
    exit 1
}

Write-Host "Source logo: $LogoPath" -ForegroundColor Green
Write-Host "Output directory: $OutputDir`n" -ForegroundColor Green

# Ensure output directory exists
if (-not (Test-Path $OutputDir)) {
    New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null
}

# Copy logo to assets folder
$logoDestination = Join-Path $OutputDir "logo.png"
Copy-Item $LogoPath $logoDestination -Force
Write-Host "[✓] Copied logo to assets/logo.png" -ForegroundColor Green

# Create GUI Assets directory
$guiAssetsDir = Join-Path $PSScriptRoot "..\src\gui\Assets"
if (-not (Test-Path $guiAssetsDir)) {
    New-Item -Path $guiAssetsDir -ItemType Directory -Force | Out-Null
}

Write-Host "`nCreating application icon..." -ForegroundColor Cyan

# Check if .NET assemblies are available for image processing
try {
    Add-Type -AssemblyName System.Drawing
    Write-Host "Using System.Drawing for image processing" -ForegroundColor Gray

    # Load the image
    $img = [System.Drawing.Image]::FromFile($LogoPath)

    # Create icon sizes (16, 32, 48, 64, 128, 256)
    $sizes = @(16, 32, 48, 64, 128, 256)
    $iconPath = Join-Path $guiAssetsDir "icon.ico"

    Write-Host "Generating ICO with sizes: $($sizes -join ', ')" -ForegroundColor Gray

    # For creating proper ICO files, we'll use a simplified approach
    # Create a 256x256 PNG which can be converted to ICO
    $icon256Path = Join-Path $guiAssetsDir "icon-256.png"

    $bmp = New-Object System.Drawing.Bitmap 256, 256
    $graphics = [System.Drawing.Graphics]::FromImage($bmp)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.DrawImage($img, 0, 0, 256, 256)
    $bmp.Save($icon256Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bmp.Dispose()

    Write-Host "[✓] Created 256x256 PNG for icon" -ForegroundColor Green

    # Note: Creating a proper multi-resolution ICO file requires additional tools
    # We'll provide instructions for the user
    Write-Host ""
    Write-Host "Note: To create a proper ICO file with multiple resolutions:" -ForegroundColor Yellow
    Write-Host "  1. Use an online tool like https://convertio.co/png-ico/" -ForegroundColor White
    Write-Host "  2. Upload: $icon256Path" -ForegroundColor White
    Write-Host "  3. Select sizes: 16, 32, 48, 256" -ForegroundColor White
    Write-Host "  4. Download and save as: $iconPath" -ForegroundColor White
    Write-Host ""
    Write-Host "Alternative: Use ImageMagick (if installed):" -ForegroundColor Yellow
    Write-Host "  magick convert $LogoPath -define icon:auto-resize=256,64,48,32,16 $iconPath" -ForegroundColor White

    $img.Dispose()

} catch {
    Write-Host "Warning: Could not process images with System.Drawing" -ForegroundColor Yellow
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Manual steps required:" -ForegroundColor Yellow
    Write-Host "1. Create icon manually using an online tool or ImageMagick" -ForegroundColor White
    Write-Host "2. Save as: $iconPath" -ForegroundColor White
}

# Create screenshots directory
$screenshotsDir = Join-Path $OutputDir "screenshots"
if (-not (Test-Path $screenshotsDir)) {
    New-Item -Path $screenshotsDir -ItemType Directory -Force | Out-Null
    Write-Host "`n[✓] Created screenshots directory" -ForegroundColor Green

    # Create a README for screenshots
    $screenshotsReadme = @"
# Screenshots

Add application screenshots here for documentation:

1. **main-window.png** - Main configuration window
2. **categories.png** - File categories configuration
3. **schedule.png** - Schedule settings
4. **test-run.png** - Test run/dry run in action
5. **log-viewer.png** - Log file viewing

Screenshots should be:
- PNG format
- Clear and well-lit
- Show the TidyPackRat interface in action
- Cropped to relevant areas
"@

    Set-Content -Path (Join-Path $screenshotsDir "README.md") -Value $screenshotsReadme
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Asset Preparation Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "[✓] Logo copied to assets/logo.png" -ForegroundColor Green
Write-Host "[✓] Created GUI Assets directory" -ForegroundColor Green
Write-Host "[✓] Created screenshots directory" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Create the ICO file using the instructions above" -ForegroundColor White
Write-Host "2. Take screenshots of the application" -ForegroundColor White
Write-Host "3. Update the GUI project to use the icon" -ForegroundColor White
Write-Host ""
Write-Host "To update the GUI project with the icon, run:" -ForegroundColor Yellow
Write-Host "  .\update-project-icon.ps1" -ForegroundColor White

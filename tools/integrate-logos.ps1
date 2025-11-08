<#
.SYNOPSIS
    Integrates TidyPackRat logos into the project

.DESCRIPTION
    Sets up both the mascot logo and title logo in the appropriate locations

.PARAMETER MascotLogoPath
    Path to the pack rat mascot logo PNG

.PARAMETER TitleLogoPath
    Path to the "TIDY PACK RAT" title logo PNG

.EXAMPLE
    .\integrate-logos.ps1 -MascotLogoPath "C:\Downloads\mascot.png" -TitleLogoPath "C:\Downloads\title.png"
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$MascotLogoPath,

    [Parameter(Mandatory=$false)]
    [string]$TitleLogoPath
)

$ErrorActionPreference = "Stop"

Write-Host "TidyPackRat Logo Integration" -ForegroundColor Cyan
Write-Host "============================`n" -ForegroundColor Cyan

$assetsDir = Join-Path $PSScriptRoot "..\assets"

# Ensure assets directory exists
if (-not (Test-Path $assetsDir)) {
    New-Item -Path $assetsDir -ItemType Directory -Force | Out-Null
}

# Process mascot logo
if ($MascotLogoPath -and (Test-Path $MascotLogoPath)) {
    $mascotDest = Join-Path $assetsDir "logo.png"
    Copy-Item $MascotLogoPath $mascotDest -Force
    Write-Host "[✓] Copied mascot logo to assets/logo.png" -ForegroundColor Green
    Write-Host "    Use for: Application icon, circular avatar, profile pic" -ForegroundColor Gray
} elseif ($MascotLogoPath) {
    Write-Host "[!] Mascot logo not found: $MascotLogoPath" -ForegroundColor Yellow
} else {
    Write-Host "[ ] Mascot logo not provided (use -MascotLogoPath)" -ForegroundColor Gray
}

# Process title logo
if ($TitleLogoPath -and (Test-Path $TitleLogoPath)) {
    $titleDest = Join-Path $assetsDir "title-logo.png"
    Copy-Item $TitleLogoPath $titleDest -Force
    Write-Host "[✓] Copied title logo to assets/title-logo.png" -ForegroundColor Green
    Write-Host "    Use for: README header, social media, banners" -ForegroundColor Gray
} elseif ($TitleLogoPath) {
    Write-Host "[!] Title logo not found: $TitleLogoPath" -ForegroundColor Yellow
} else {
    Write-Host "[ ] Title logo not provided (use -TitleLogoPath)" -ForegroundColor Gray
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Logo Usage Guide" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "MASCOT LOGO (pack rat with glasses):" -ForegroundColor Yellow
Write-Host "  • Application icon (.ico)" -ForegroundColor White
Write-Host "  • GitHub profile/avatar" -ForegroundColor White
Write-Host "  • Small icons and badges" -ForegroundColor White
Write-Host "  • Favicon for documentation" -ForegroundColor White

Write-Host "`nTITLE LOGO (colorful text):" -ForegroundColor Yellow
Write-Host "  • README.md header" -ForegroundColor White
Write-Host "  • Documentation pages" -ForegroundColor White
Write-Host "  • Social media posts" -ForegroundColor White
Write-Host "  • Installer banner" -ForegroundColor White
Write-Host "  • Project website header" -ForegroundColor White

Write-Host "`nCOMBINED (both together):" -ForegroundColor Yellow
Write-Host "  • Create a banner with both" -ForegroundColor White
Write-Host "  • Marketing materials" -ForegroundColor White
Write-Host "  • Conference slides" -ForegroundColor White

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "1. Update README.md to use title logo as header" -ForegroundColor White
Write-Host "2. Create application icon from mascot logo" -ForegroundColor White
Write-Host "3. Create installer banner combining both logos" -ForegroundColor White
Write-Host "4. Take screenshots of the application" -ForegroundColor White

Write-Host "`nTo create application icon:" -ForegroundColor Yellow
Write-Host "  .\tools\prepare-assets.ps1 -LogoPath `"$assetsDir\logo.png`"" -ForegroundColor Gray

Write-Host "`nTo update README with title logo:" -ForegroundColor Yellow
Write-Host "  Edit README.md and update the logo line to:" -ForegroundColor Gray
Write-Host '  <img src="assets/title-logo.png" alt="TidyPackRat" width="600"/>' -ForegroundColor Gray

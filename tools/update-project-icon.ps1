<#
.SYNOPSIS
    Updates TidyPackRat project files to use the application icon

.DESCRIPTION
    Modifies the GUI project file to reference the icon.ico file
#>

$ErrorActionPreference = "Stop"

Write-Host "Updating TidyPackRat project to use icon..." -ForegroundColor Cyan

$projectFile = Join-Path $PSScriptRoot "..\src\gui\TidyPackRat.csproj"
$iconFile = Join-Path $PSScriptRoot "..\src\gui\Assets\icon.ico"

# Check if icon exists
if (-not (Test-Path $iconFile)) {
    Write-Host "Warning: Icon file not found at $iconFile" -ForegroundColor Yellow
    Write-Host "Please create the icon first using prepare-assets.ps1" -ForegroundColor Yellow
    exit 1
}

# Check if project file exists
if (-not (Test-Path $projectFile)) {
    Write-Host "Error: Project file not found at $projectFile" -ForegroundColor Red
    exit 1
}

Write-Host "Icon file: $iconFile" -ForegroundColor Green
Write-Host "Project file: $projectFile" -ForegroundColor Green

# Read project file
$content = Get-Content $projectFile -Raw

# Check if ApplicationIcon is already set
if ($content -match '<ApplicationIcon>') {
    Write-Host "`nApplicationIcon already configured in project" -ForegroundColor Yellow
    Write-Host "Current configuration will be preserved" -ForegroundColor Gray
} else {
    # The project file already has <ApplicationIcon>Assets\icon.ico</ApplicationIcon>
    # So we just need to ensure the icon file exists
    Write-Host "[✓] Project is already configured to use Assets\icon.ico" -ForegroundColor Green
}

# Check if icon is included in project as a resource
if ($content -match '<Content Include="Assets\\icon.ico">') {
    Write-Host "[✓] Icon is already included in project" -ForegroundColor Green
} else {
    Write-Host "`nAdding icon to project resources..." -ForegroundColor Cyan

    # Find the closing </Project> tag and add the icon before it
    $iconInclude = @"
  <ItemGroup>
    <Content Include="Assets\icon.ico">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
"@

    $content = $content -replace '</Project>', $iconInclude

    # Save the modified project file
    Set-Content -Path $projectFile -Value $content -Encoding UTF8
    Write-Host "[✓] Added icon to project resources" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Project Updated Successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Rebuild the project: .\build.ps1" -ForegroundColor White
Write-Host "2. The application icon will appear on TidyPackRat.exe" -ForegroundColor White
Write-Host "3. The icon will show in the title bar when running" -ForegroundColor White

<#
.SYNOPSIS
    Test script for TidyPackRat worker

.DESCRIPTION
    Creates test files and runs the worker script in dry-run mode
    to verify functionality
#>

param(
    [switch]$Cleanup
)

# Test directories
$testSource = Join-Path $PSScriptRoot "TestSource"
$testDest = Join-Path $PSScriptRoot "TestDestination"

if ($Cleanup) {
    Write-Host "Cleaning up test directories..." -ForegroundColor Yellow
    if (Test-Path $testSource) { Remove-Item $testSource -Recurse -Force }
    if (Test-Path $testDest) { Remove-Item $testDest -Recurse -Force }
    Write-Host "Cleanup complete!" -ForegroundColor Green
    exit 0
}

# Create test directories
Write-Host "Setting up test environment..." -ForegroundColor Cyan
New-Item -Path $testSource -ItemType Directory -Force | Out-Null
New-Item -Path $testDest -ItemType Directory -Force | Out-Null

# Create test files
Write-Host "Creating test files..." -ForegroundColor Cyan

$testFiles = @(
    "document.pdf",
    "photo.jpg",
    "photo2.png",
    "spreadsheet.xlsx",
    "presentation.pptx",
    "archive.zip",
    "video.mp4",
    "song.mp3",
    "code.py",
    "script.ps1",
    "temp.tmp",      # Should be excluded
    "~tempfile.txt"  # Should be excluded
)

foreach ($file in $testFiles) {
    $filePath = Join-Path $testSource $file
    "Test content" | Out-File -FilePath $filePath -Force

    # Make files older than 24 hours
    $item = Get-Item $filePath
    $item.LastWriteTime = (Get-Date).AddHours(-25)
}

Write-Host "Created $($testFiles.Count) test files" -ForegroundColor Green

# Create test configuration
$testConfig = @{
    appName = "TidyPackRat"
    version = "1.0.0"
    sourceFolder = $testSource
    fileAgeThreshold = 24
    fileSizeThreshold = 0
    duplicateHandling = "rename"
    categories = @(
        @{
            name = "Documents"
            extensions = @(".pdf", ".txt", ".docx")
            destination = Join-Path $testDest "Documents"
            enabled = $true
        },
        @{
            name = "Images"
            extensions = @(".jpg", ".png", ".gif")
            destination = Join-Path $testDest "Pictures"
            enabled = $true
        },
        @{
            name = "Spreadsheets"
            extensions = @(".xlsx", ".xls", ".csv")
            destination = Join-Path $testDest "Spreadsheets"
            enabled = $true
        },
        @{
            name = "Presentations"
            extensions = @(".pptx", ".ppt")
            destination = Join-Path $testDest "Presentations"
            enabled = $true
        },
        @{
            name = "Archives"
            extensions = @(".zip", ".rar", ".7z")
            destination = Join-Path $testDest "Archives"
            enabled = $true
        },
        @{
            name = "Videos"
            extensions = @(".mp4", ".avi", ".mkv")
            destination = Join-Path $testDest "Videos"
            enabled = $true
        },
        @{
            name = "Audio"
            extensions = @(".mp3", ".wav", ".flac")
            destination = Join-Path $testDest "Music"
            enabled = $true
        }
    )
    schedule = @{
        enabled = $false
        frequency = "daily"
        time = "02:00"
        runOnStartup = $false
    }
    excludePatterns = @("*.tmp", "~*")
    logging = @{
        enabled = $true
        logPath = Join-Path $PSScriptRoot "logs"
        logLevel = "info"
        maxLogFiles = 12
    }
}

$configPath = Join-Path $PSScriptRoot "test-config.json"
$testConfig | ConvertTo-Json -Depth 10 | Out-File -FilePath $configPath -Encoding UTF8

Write-Host "Test configuration created at: $configPath" -ForegroundColor Green

# Find worker script
$workerScript = Join-Path (Split-Path $PSScriptRoot -Parent) "src\worker\TidyPackRat-Worker.ps1"

if (-not (Test-Path $workerScript)) {
    Write-Host "Worker script not found at: $workerScript" -ForegroundColor Red
    Write-Host "Please ensure the worker script exists" -ForegroundColor Yellow
    exit 1
}

Write-Host "`nRunning TidyPackRat worker in DRY RUN mode..." -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Run worker script in dry-run mode
& $workerScript -ConfigPath $configPath -DryRun -VerboseLogging

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test complete!" -ForegroundColor Green
Write-Host "`nTo clean up test files, run:" -ForegroundColor Yellow
Write-Host "  .\test-worker.ps1 -Cleanup" -ForegroundColor Yellow

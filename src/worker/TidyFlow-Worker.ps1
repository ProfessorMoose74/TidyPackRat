<#
.SYNOPSIS
    TidyFlow - Automated File Management System
    "Sorting your files, to clean up your mess."

.DESCRIPTION
    Worker script that organizes files from a source folder into categorized
    destination folders based on file type and user-defined rules.

.PARAMETER ConfigPath
    Path to the configuration JSON file

.PARAMETER DryRun
    If specified, simulates the file operations without actually moving files

.PARAMETER Verbose
    Provides detailed output during execution

.EXAMPLE
    .\TidyFlow-Worker.ps1 -ConfigPath "C:\ProgramData\TidyFlow\config.json"

.EXAMPLE
    .\TidyFlow-Worker.ps1 -ConfigPath "config.json" -DryRun -Verbose

.NOTES
    Version: 1.2.0
    Author: TidyFlow Team
    License: MIT
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigPath = "$env:PROGRAMDATA\TidyFlow\config.json",

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [switch]$VerboseLogging
)

# Set strict mode for better error detection
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

#region Helper Functions

<#
.SYNOPSIS
    Expands environment variables in a path string
#>
function Expand-EnvironmentPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $Path
    }

    # Expand environment variables
    $expanded = [System.Environment]::ExpandEnvironmentVariables($Path)
    return $expanded
}

<#
.SYNOPSIS
    Initializes the logging system
#>
function Initialize-Logging {
    param(
        [string]$LogPath,
        [int]$MaxLogFiles = 12
    )

    try {
        $expandedLogPath = Expand-EnvironmentPath -Path $LogPath

        # Create log directory if it doesn't exist
        if (-not (Test-Path -Path $expandedLogPath)) {
            New-Item -Path $expandedLogPath -ItemType Directory -Force | Out-Null
        }

        # Generate log filename with month/year
        $logFileName = "TidyFlow-$(Get-Date -Format 'yyyy-MM').log"
        $script:LogFile = Join-Path -Path $expandedLogPath -ChildPath $logFileName

        # Clean up old log files if there are too many
        $logFiles = Get-ChildItem -Path $expandedLogPath -Filter "TidyFlow-*.log" |
                    Sort-Object -Property LastWriteTime -Descending

        if ($logFiles.Count -gt $MaxLogFiles) {
            $logFiles | Select-Object -Skip $MaxLogFiles | Remove-Item -Force
        }

        Write-Log -Message "========================================" -Level "INFO"
        Write-Log -Message "TidyFlow Worker Started" -Level "INFO"
        Write-Log -Message "Version: 1.2.0" -Level "INFO"
        if ($DryRun) {
            Write-Log -Message "DRY RUN MODE - No files will be moved" -Level "WARN"
        }
        Write-Log -Message "========================================" -Level "INFO"
    }
    catch {
        Write-Error "Failed to initialize logging: $_"
        throw
    }
}

<#
.SYNOPSIS
    Writes a log entry with timestamp
#>
function Write-Log {
    param(
        [string]$Message,
        [ValidateSet("INFO", "WARN", "ERROR", "SUCCESS")]
        [string]$Level = "INFO"
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"

    # Write to log file
    if ($script:LogFile) {
        Add-Content -Path $script:LogFile -Value $logEntry
    }

    # Write to console with color
    if ($VerboseLogging -or $DryRun) {
        switch ($Level) {
            "ERROR"   { Write-Host $logEntry -ForegroundColor Red }
            "WARN"    { Write-Host $logEntry -ForegroundColor Yellow }
            "SUCCESS" { Write-Host $logEntry -ForegroundColor Green }
            default   { Write-Host $logEntry -ForegroundColor Gray }
        }
    }
}

<#
.SYNOPSIS
    Loads and validates the configuration file
#>
function Get-Configuration {
    param([string]$Path)

    try {
        $expandedPath = Expand-EnvironmentPath -Path $Path

        if (-not (Test-Path -Path $expandedPath)) {
            throw "Configuration file not found: $expandedPath"
        }

        Write-Log -Message "Loading configuration from: $expandedPath" -Level "INFO"

        $configJson = Get-Content -Path $expandedPath -Raw -Encoding UTF8
        $config = $configJson | ConvertFrom-Json

        # Validate required fields
        if (-not $config.sourceFolder) {
            throw "Configuration missing required field: sourceFolder"
        }

        if (-not $config.categories) {
            throw "Configuration missing required field: categories"
        }

        Write-Log -Message "Configuration loaded successfully" -Level "SUCCESS"
        return $config
    }
    catch {
        Write-Log -Message "Failed to load configuration: $_" -Level "ERROR"
        throw
    }
}

<#
.SYNOPSIS
    Checks if a file matches any exclude pattern
#>
function Test-ExcludePattern {
    param(
        [string]$FileName,
        [array]$Patterns
    )

    if (-not $Patterns -or $Patterns.Count -eq 0) {
        return $false
    }

    foreach ($pattern in $Patterns) {
        if ($FileName -like $pattern) {
            return $true
        }
    }

    return $false
}

<#
.SYNOPSIS
    Gets the appropriate destination folder for a file based on its extension
#>
function Get-DestinationFolder {
    param(
        [string]$FileExtension,
        [array]$Categories
    )

    $FileExtension = $FileExtension.ToLower()

    foreach ($category in $Categories) {
        # Skip disabled categories
        if ($category.enabled -eq $false) {
            continue
        }

        if ($category.extensions -contains $FileExtension) {
            return @{
                Path = Expand-EnvironmentPath -Path $category.destination
                CategoryName = $category.name
            }
        }
    }

    return $null
}

<#
.SYNOPSIS
    Handles duplicate files by renaming according to the configured strategy
#>
function Get-UniqueFileName {
    param(
        [string]$DestinationPath,
        [string]$FileName,
        [string]$Strategy = "rename"
    )

    $fullPath = Join-Path -Path $DestinationPath -ChildPath $FileName

    # If file doesn't exist, return original name
    if (-not (Test-Path -Path $fullPath)) {
        return $FileName
    }

    # Handle based on strategy
    switch ($Strategy.ToLower()) {
        "skip" {
            return $null  # Signal to skip this file
        }
        "rename" {
            $baseName = [System.IO.Path]::GetFileNameWithoutExtension($FileName)
            $extension = [System.IO.Path]::GetExtension($FileName)
            $counter = 1

            do {
                $newName = "${baseName}_${counter}${extension}"
                $fullPath = Join-Path -Path $DestinationPath -ChildPath $newName
                $counter++
            } while (Test-Path -Path $fullPath)

            return $newName
        }
        default {
            return $null
        }
    }
}

<#
.SYNOPSIS
    Moves a file to its destination with error handling and rollback support
#>
function Move-FileToDestination {
    param(
        [System.IO.FileInfo]$File,
        [string]$DestinationPath,
        [string]$CategoryName,
        [string]$DuplicateStrategy
    )

    try {
        # Create destination directory if it doesn't exist
        if (-not (Test-Path -Path $DestinationPath)) {
            if (-not $DryRun) {
                New-Item -Path $DestinationPath -ItemType Directory -Force | Out-Null
                Write-Log -Message "Created directory: $DestinationPath" -Level "INFO"
            }
        }

        # Handle duplicates
        $targetFileName = Get-UniqueFileName -DestinationPath $DestinationPath -FileName $File.Name -Strategy $DuplicateStrategy

        if ($null -eq $targetFileName) {
            Write-Log -Message "SKIPPED (duplicate): $($File.Name)" -Level "WARN"
            return @{
                Success = $true
                Action = "Skipped"
                Reason = "Duplicate file"
            }
        }

        $destinationFile = Join-Path -Path $DestinationPath -ChildPath $targetFileName

        # Move the file
        if ($DryRun) {
            Write-Log -Message "[DRY RUN] Would move: $($File.FullName) -> $destinationFile" -Level "INFO"
        }
        else {
            Move-Item -Path $File.FullName -Destination $destinationFile -Force
            Write-Log -Message "MOVED [$CategoryName]: $($File.Name) -> $destinationFile" -Level "SUCCESS"
        }

        return @{
            Success = $true
            Action = "Moved"
            Source = $File.FullName
            Destination = $destinationFile
            Category = $CategoryName
        }
    }
    catch {
        Write-Log -Message "ERROR moving $($File.Name): $_" -Level "ERROR"
        return @{
            Success = $false
            Action = "Failed"
            Error = $_.Exception.Message
        }
    }
}

<#
.SYNOPSIS
    Processes files in the source folder
#>
function Invoke-FileOrganization {
    param(
        [object]$Config
    )

    $stats = @{
        TotalFiles = 0
        MovedFiles = 0
        SkippedFiles = 0
        ErrorFiles = 0
        Categories = @{}
    }

    try {
        # Expand source folder path
        $sourceFolder = Expand-EnvironmentPath -Path $Config.sourceFolder

        if (-not (Test-Path -Path $sourceFolder)) {
            throw "Source folder not found: $sourceFolder"
        }

        Write-Log -Message "Scanning source folder: $sourceFolder" -Level "INFO"

        # Get all files (non-recursive by default)
        $files = Get-ChildItem -Path $sourceFolder -File -ErrorAction SilentlyContinue

        if ($files.Count -eq 0) {
            Write-Log -Message "No files found in source folder" -Level "INFO"
            return $stats
        }

        Write-Log -Message "Found $($files.Count) files to process" -Level "INFO"
        $stats.TotalFiles = $files.Count

        # Get file age threshold
        $fileAgeThreshold = if ($Config.fileAgeThreshold) { $Config.fileAgeThreshold } else { 24 }
        $cutoffTime = (Get-Date).AddHours(-$fileAgeThreshold)

        # Get file size threshold (in bytes, 0 = no limit)
        $fileSizeThreshold = if ($Config.fileSizeThreshold) { $Config.fileSizeThreshold } else { 0 }

        foreach ($file in $files) {
            # Check file age
            if ($file.LastWriteTime -gt $cutoffTime) {
                Write-Log -Message "SKIPPED (too recent): $($file.Name)" -Level "WARN"
                $stats.SkippedFiles++
                continue
            }

            # Check file size threshold
            if ($fileSizeThreshold -gt 0 -and $file.Length -lt $fileSizeThreshold) {
                Write-Log -Message "SKIPPED (too small): $($file.Name)" -Level "WARN"
                $stats.SkippedFiles++
                continue
            }

            # Check exclude patterns
            if (Test-ExcludePattern -FileName $file.Name -Patterns $Config.excludePatterns) {
                Write-Log -Message "SKIPPED (excluded): $($file.Name)" -Level "WARN"
                $stats.SkippedFiles++
                continue
            }

            # Get destination for this file type
            $destination = Get-DestinationFolder -FileExtension $file.Extension -Categories $Config.categories

            if ($null -eq $destination) {
                Write-Log -Message "SKIPPED (no category): $($file.Name) [$($file.Extension)]" -Level "WARN"
                $stats.SkippedFiles++
                continue
            }

            # Move the file
            $result = Move-FileToDestination -File $file `
                                            -DestinationPath $destination.Path `
                                            -CategoryName $destination.CategoryName `
                                            -DuplicateStrategy $Config.duplicateHandling

            # Update statistics
            if ($result.Success) {
                if ($result.Action -eq "Moved") {
                    $stats.MovedFiles++

                    # Track by category
                    if (-not $stats.Categories.ContainsKey($destination.CategoryName)) {
                        $stats.Categories[$destination.CategoryName] = 0
                    }
                    $stats.Categories[$destination.CategoryName]++
                }
                elseif ($result.Action -eq "Skipped") {
                    $stats.SkippedFiles++
                }
            }
            else {
                $stats.ErrorFiles++
            }
        }

        return $stats
    }
    catch {
        Write-Log -Message "Critical error during file organization: $_" -Level "ERROR"
        throw
    }
}

<#
.SYNOPSIS
    Displays summary statistics
#>
function Show-Summary {
    param([hashtable]$Stats)

    Write-Log -Message "========================================" -Level "INFO"
    Write-Log -Message "SUMMARY" -Level "INFO"
    Write-Log -Message "Total files found: $($Stats.TotalFiles)" -Level "INFO"
    Write-Log -Message "Files moved: $($Stats.MovedFiles)" -Level "SUCCESS"
    Write-Log -Message "Files skipped: $($Stats.SkippedFiles)" -Level "WARN"
    Write-Log -Message "Errors: $($Stats.ErrorFiles)" -Level $(if ($Stats.ErrorFiles -gt 0) { "ERROR" } else { "INFO" })

    if ($Stats.Categories.Count -gt 0) {
        Write-Log -Message "" -Level "INFO"
        Write-Log -Message "Files moved by category:" -Level "INFO"
        foreach ($category in $Stats.Categories.GetEnumerator() | Sort-Object -Property Value -Descending) {
            Write-Log -Message "  $($category.Key): $($category.Value)" -Level "INFO"
        }
    }

    Write-Log -Message "========================================" -Level "INFO"
}

#endregion

#region Main Execution

try {
    # Load configuration
    $config = Get-Configuration -Path $ConfigPath

    # Initialize logging
    if ($config.logging -and $config.logging.enabled) {
        Initialize-Logging -LogPath $config.logging.logPath -MaxLogFiles $config.logging.maxLogFiles
    }
    else {
        # Fallback logging
        Initialize-Logging -LogPath "$env:PROGRAMDATA\TidyFlow\logs"
    }

    # Run file organization
    $statistics = Invoke-FileOrganization -Config $config

    # Show summary
    Show-Summary -Stats $statistics

    Write-Log -Message "TidyFlow Worker completed successfully" -Level "SUCCESS"
    exit 0
}
catch {
    Write-Log -Message "TidyFlow Worker failed: $_" -Level "ERROR"
    Write-Log -Message "Stack trace: $($_.ScriptStackTrace)" -Level "ERROR"
    exit 1
}

#endregion

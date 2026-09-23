<#
.SYNOPSIS
    Builds, tests and packages TidyFlow.

.DESCRIPTION
    With no switches: restores, builds and runs the unit tests (needs only the .NET 10 SDK).
    -Portable publishes a self-contained, single-file TidyFlow.exe to dist\portable that runs without installing
    anything, and zips it for a GitHub release. This is how TidyFlow is distributed.
    -Package builds the optional MSIX bundle (x64 + ARM64, unsigned; sign it with your own certificate to install
    it). That step needs Visual Studio with the "Windows application development" workload.

.EXAMPLE
    .\build.ps1
.EXAMPLE
    .\build.ps1 -Portable -Runtime win-arm64
.EXAMPLE
    .\build.ps1 -Package
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    # Build the optional MSIX bundle in dist\msix.
    [switch]$Package,

    # Publish a self-contained, single-file build to dist\portable, plus a zip for releases.
    [switch]$Portable,

    [ValidateSet('win-x64', 'win-arm64')]
    [string]$Runtime = 'win-x64',

    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$solution = Join-Path $root 'TidyFlow.slnx'

function Invoke-Step([string]$Name, [scriptblock]$Command) {
    Write-Host "`n== $Name" -ForegroundColor Cyan
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "$Name failed (exit code $LASTEXITCODE)." }
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'The .NET SDK was not found. Install the .NET 10 SDK from https://dot.net.'
}

Invoke-Step 'Build' { dotnet build $solution -c $Configuration -nologo }

if (-not $SkipTests) {
    Invoke-Step 'Test' { dotnet test --solution $solution -c $Configuration --no-build }
}

if ($Portable) {
    $output = Join-Path $root "dist\portable\$Runtime"
    if (Test-Path $output) { Remove-Item $output -Recurse -Force }
    Invoke-Step "Publish portable ($Runtime)" {
        dotnet publish (Join-Path $root 'src\TidyFlow\TidyFlow.csproj') -c $Configuration -r $Runtime --self-contained -o $output -nologo `
            -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true `
            -p:DebugType=none
    }

    [xml]$props = Get-Content (Join-Path $root 'Directory.Build.props')
    $version = $props.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    $zip = Join-Path $root "dist\TidyFlow-$version-$($Runtime -replace '^win-', '')-portable.zip"
    Copy-Item (Join-Path $root 'LICENSE') $output
    Compress-Archive -Path (Join-Path $output '*') -DestinationPath $zip -Force
    Write-Host "Portable build: $output\TidyFlow.exe" -ForegroundColor Green
    Write-Host "Release zip:    $zip" -ForegroundColor Green
}

if ($Package) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = if (Test-Path $vswhere) {
        & $vswhere -latest -prerelease -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }
    if (-not $msbuild) {
        throw 'Visual Studio (with the Windows application development workload) is required to build the MSIX package.'
    }

    Invoke-Step 'Package (MSIX bundle, x64 + ARM64)' {
        & $msbuild $solution -restore -nologo -v:minimal `
            "-p:Configuration=$Configuration" '-p:Platform=x64' `
            '-p:UapAppxPackageBuildMode=SideloadOnly' '-p:AppxBundle=Always' '-p:AppxBundlePlatforms=x64|arm64'
    }

    $bundle = Get-ChildItem (Join-Path $root 'dist\msix') -Filter '*.msixbundle' -Recurse |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    Write-Host "MSIX bundle (unsigned): $($bundle.FullName)" -ForegroundColor Green
}

Write-Host "`nDone." -ForegroundColor Green

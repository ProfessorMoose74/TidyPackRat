# Generate-MsixAssets.ps1
# Generates all required MSIX image assets from the source logo

param(
    [string]$SourceImage = "$PSScriptRoot\..\assets\logo.png",
    [string]$OutputDir = "$PSScriptRoot\..\src\TidyFlow.Package\Images"
)

Add-Type -AssemblyName System.Drawing

# Ensure output directory exists
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Load source image
$source = [System.Drawing.Image]::FromFile((Resolve-Path $SourceImage).Path)

Write-Host "Source image: $($source.Width)x$($source.Height)"
Write-Host "Output directory: $OutputDir"

function Resize-Image {
    param(
        [System.Drawing.Image]$Source,
        [int]$Width,
        [int]$Height,
        [string]$OutputPath,
        [string]$BackgroundColor = "Transparent"
    )

    $bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)

    # Set high quality rendering
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    # Fill background (transparent for PNG)
    if ($BackgroundColor -eq "Transparent") {
        $graphics.Clear([System.Drawing.Color]::Transparent)
    } else {
        $graphics.Clear([System.Drawing.Color]::FromName($BackgroundColor))
    }

    # Calculate scaling to fit image in target size while maintaining aspect ratio
    $sourceRatio = $Source.Width / $Source.Height
    $targetRatio = $Width / $Height

    $drawWidth = $Width
    $drawHeight = $Height
    $drawX = 0
    $drawY = 0

    if ($sourceRatio -gt $targetRatio) {
        # Source is wider - fit to width
        $drawHeight = [int]($Width / $sourceRatio)
        $drawY = [int](($Height - $drawHeight) / 2)
    } else {
        # Source is taller - fit to height
        $drawWidth = [int]($Height * $sourceRatio)
        $drawX = [int](($Width - $drawWidth) / 2)
    }

    $graphics.DrawImage($Source, $drawX, $drawY, $drawWidth, $drawHeight)

    # Save as PNG
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

    $graphics.Dispose()
    $bitmap.Dispose()

    Write-Host "  Created: $OutputPath ($Width x $Height)"
}

Write-Host "`nGenerating MSIX assets..."

# Square logos (app icon)
Resize-Image -Source $source -Width 44 -Height 44 -OutputPath "$OutputDir\Square44x44Logo.png"
Resize-Image -Source $source -Width 24 -Height 24 -OutputPath "$OutputDir\Square44x44Logo.targetsize-24.png"
Resize-Image -Source $source -Width 16 -Height 16 -OutputPath "$OutputDir\Square44x44Logo.targetsize-16.png"
Resize-Image -Source $source -Width 48 -Height 48 -OutputPath "$OutputDir\Square44x44Logo.targetsize-48.png"
Resize-Image -Source $source -Width 256 -Height 256 -OutputPath "$OutputDir\Square44x44Logo.targetsize-256.png"
Resize-Image -Source $source -Width 44 -Height 44 -OutputPath "$OutputDir\Square44x44Logo.altform-unplated.png"
Resize-Image -Source $source -Width 44 -Height 44 -OutputPath "$OutputDir\Square44x44Logo.altform-lightunplated.png"

# Tile logos
Resize-Image -Source $source -Width 71 -Height 71 -OutputPath "$OutputDir\SmallTile.png"
Resize-Image -Source $source -Width 150 -Height 150 -OutputPath "$OutputDir\Square150x150Logo.png"
Resize-Image -Source $source -Width 310 -Height 310 -OutputPath "$OutputDir\LargeTile.png"

# Wide tile (310x150) - centered logo
Resize-Image -Source $source -Width 310 -Height 150 -OutputPath "$OutputDir\Wide310x150Logo.png"

# Store logo
Resize-Image -Source $source -Width 50 -Height 50 -OutputPath "$OutputDir\StoreLogo.png"

# Badge logo (for notifications)
Resize-Image -Source $source -Width 24 -Height 24 -OutputPath "$OutputDir\BadgeLogo.png"

# Splash screen (620x300) - centered logo
Resize-Image -Source $source -Width 620 -Height 300 -OutputPath "$OutputDir\SplashScreen.png"

# Cleanup
$source.Dispose()

Write-Host "`nAsset generation complete!"
Write-Host "Generated $(Get-ChildItem $OutputDir -Filter *.png | Measure-Object | Select-Object -ExpandProperty Count) images"

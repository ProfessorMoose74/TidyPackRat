<#
.SYNOPSIS
    Builds the application icon (src\TidyFlow\Assets\icon.ico) from the mascot logo.

.DESCRIPTION
    Writes a multi-resolution .ico (16-256 px, PNG-compressed entries) using only System.Drawing,
    so no online converter or ImageMagick is needed. Run Generate-MsixAssets.ps1 afterwards to
    refresh the Store/MSIX tile images from the same logo.

.EXAMPLE
    .\tools\New-AppIcon.ps1
    .\tools\New-AppIcon.ps1 -SourceImage C:\path\to\new-logo.png
#>
param(
    [string]$SourceImage = "$PSScriptRoot\..\assets\logo.png",
    [string]$OutputIcon = "$PSScriptRoot\..\src\TidyFlow\Assets\icon.ico",
    [int[]]$Sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$source = [System.Drawing.Image]::FromFile((Resolve-Path $SourceImage).Path)
try {
    $frames = foreach ($size in $Sizes) {
        $bitmap = New-Object System.Drawing.Bitmap $size, $size
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.Clear([System.Drawing.Color]::Transparent)

        # Fit the logo inside the square, preserving its aspect ratio.
        $scale = [Math]::Min($size / $source.Width, $size / $source.Height)
        $w = [int]($source.Width * $scale); $h = [int]($source.Height * $scale)
        $graphics.DrawImage($source, [int](($size - $w) / 2), [int](($size - $h) / 2), $w, $h)

        $stream = New-Object System.IO.MemoryStream
        $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
        $graphics.Dispose(); $bitmap.Dispose()
        [pscustomobject]@{ Size = $size; Bytes = $stream.ToArray() }
    }
}
finally {
    $source.Dispose()
}

# ICO layout: 6-byte header, one 16-byte directory entry per image, then the PNG data.
$out = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter $out
$writer.Write([uint16]0)              # reserved
$writer.Write([uint16]1)              # type: icon
$writer.Write([uint16]$frames.Count)

$offset = 6 + 16 * $frames.Count
foreach ($frame in $frames) {
    $dimension = if ($frame.Size -ge 256) { 0 } else { $frame.Size }   # 0 means 256
    $writer.Write([byte]$dimension)
    $writer.Write([byte]$dimension)
    $writer.Write([byte]0)            # palette size
    $writer.Write([byte]0)            # reserved
    $writer.Write([uint16]1)          # colour planes
    $writer.Write([uint16]32)         # bits per pixel
    $writer.Write([uint32]$frame.Bytes.Length)
    $writer.Write([uint32]$offset)
    $offset += $frame.Bytes.Length
}
foreach ($frame in $frames) { $writer.Write($frame.Bytes) }
$writer.Flush()

$target = [System.IO.Path]::GetFullPath($OutputIcon)
New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
[System.IO.File]::WriteAllBytes($target, $out.ToArray())
$writer.Dispose()

Write-Host "Wrote $target ($($Sizes -join ', ') px)"

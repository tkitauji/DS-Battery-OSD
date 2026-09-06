param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "..\Assets"),
    [string]$SourceImage = (Join-Path $PSScriptRoot "..\Assets\AppIcon-LiquidGlass.png")
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

function New-IconBitmap([int]$width, [int]$height, [bool]$wide = $false) {
    if (-not (Test-Path -LiteralPath $SourceImage)) {
        throw "Icon source image was not found: $SourceImage"
    }

    $source = [System.Drawing.Image]::FromFile((Resolve-Path $SourceImage))
    $bitmap = [System.Drawing.Bitmap]::new($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    if ($wide) {
        $graphics.Clear([System.Drawing.Color]::FromArgb(255, 24, 17, 83))
        $size = $height
        $destination = [System.Drawing.Rectangle]::new([int](($width - $size) / 2), 0, $size, $size)
    } else {
        $destination = [System.Drawing.Rectangle]::new(0, 0, $width, $height)
    }

    $graphics.DrawImage($source, $destination)
    $graphics.Dispose()
    $source.Dispose()
    return $bitmap
}

function Save-Png([string]$name, [int]$width, [int]$height, [bool]$wide = $false) {
    $bitmap = New-IconBitmap $width $height $wide
    $path = Join-Path $OutputDirectory $name
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
Save-Png "Square44x44Logo.png" 44 44
Save-Png "StoreLogo.png" 50 50
Save-Png "Square150x150Logo.png" 150 150
Save-Png "Wide310x150Logo.png" 310 150 $true
Save-Png "Square310x310Logo.png" 310 310

$iconBitmap = New-IconBitmap 256 256
$icon = [System.Drawing.Icon]::FromHandle($iconBitmap.GetHicon())
$stream = [System.IO.File]::Create((Join-Path $OutputDirectory "AppIcon.ico"))
$icon.Save($stream)
$stream.Dispose()
$icon.Dispose()
$iconBitmap.Dispose()

Write-Host "Store assets generated from $SourceImage in $OutputDirectory"

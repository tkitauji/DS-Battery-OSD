param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "..\Assets")
)

Add-Type -AssemblyName System.Drawing

$purple = [System.Drawing.Color]::FromArgb(255, 88, 67, 218)
$white = [System.Drawing.Color]::White
$yellow = [System.Drawing.Color]::FromArgb(255, 255, 216, 77)

function New-IconBitmap([int]$width, [int]$height) {
    $bitmap = [System.Drawing.Bitmap]::new($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.Clear($purple)

    $size = [Math]::Min($width, $height)
    $scale = $size / 512.0
    $offsetX = ($width - $size) / 2.0
    $offsetY = ($height - $size) / 2.0
    $graphics.TranslateTransform($offsetX, $offsetY)
    $graphics.ScaleTransform($scale, $scale)

    $controller = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(154,190), [System.Drawing.PointF]::new(106,211),
        [System.Drawing.PointF]::new(91,249), [System.Drawing.PointF]::new(67,348),
        [System.Drawing.PointF]::new(88,407), [System.Drawing.PointF]::new(137,379),
        [System.Drawing.PointF]::new(164,340), [System.Drawing.PointF]::new(348,340),
        [System.Drawing.PointF]::new(375,379), [System.Drawing.PointF]::new(424,407),
        [System.Drawing.PointF]::new(445,348), [System.Drawing.PointF]::new(421,249),
        [System.Drawing.PointF]::new(406,211), [System.Drawing.PointF]::new(358,190)
    )
    $graphics.FillClosedCurve([System.Drawing.SolidBrush]::new($white), $controller, [System.Drawing.Drawing2D.FillMode]::Winding, 0.25)

    $purpleBrush = [System.Drawing.SolidBrush]::new($purple)
    $graphics.FillRectangle($purpleBrush, 139, 244, 78, 22)
    $graphics.FillRectangle($purpleBrush, 167, 216, 22, 78)
    $graphics.FillEllipse($purpleBrush, 329, 222, 26, 26)
    $graphics.FillEllipse($purpleBrush, 358, 251, 26, 26)

    $battery = [System.Drawing.RectangleF]::new(254, 91, 112, 107)
    $graphics.FillRectangle([System.Drawing.SolidBrush]::new($white), $battery)
    $graphics.FillRectangle([System.Drawing.SolidBrush]::new($white), 366, 120, 19, 49)
    $lightning = [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(316,105), [System.Drawing.PointF]::new(289,148),
        [System.Drawing.PointF]::new(310,148), [System.Drawing.PointF]::new(301,184),
        [System.Drawing.PointF]::new(334,136), [System.Drawing.PointF]::new(312,136)
    )
    $graphics.FillPolygon([System.Drawing.SolidBrush]::new($yellow), $lightning)

    $graphics.ResetTransform()
    $graphics.Dispose()
    return $bitmap
}

function Save-Png([string]$name, [int]$width, [int]$height) {
    $bitmap = New-IconBitmap $width $height
    $path = Join-Path $OutputDirectory $name
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
Save-Png "Square44x44Logo.png" 44 44
Save-Png "StoreLogo.png" 50 50
Save-Png "Square150x150Logo.png" 150 150
Save-Png "Wide310x150Logo.png" 310 150
Save-Png "Square310x310Logo.png" 310 310

$iconBitmap = New-IconBitmap 256 256
$icon = [System.Drawing.Icon]::FromHandle($iconBitmap.GetHicon())
$stream = [System.IO.File]::Create((Join-Path $OutputDirectory "AppIcon.ico"))
$icon.Save($stream)
$stream.Dispose()
$icon.Dispose()
$iconBitmap.Dispose()

Write-Host "Store assets generated in $OutputDirectory"

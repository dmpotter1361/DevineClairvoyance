<#
    One-off asset optimizer for Devine Clairvoyance.
    - Opaque photographic images (the 78 cards + tarot sheet background) -> JPEG q90.
    - Images with real transparency (Card Stack, the 3 spread placeholders) -> downscaled PNG.
    - Removes the unused Sparkle image.
    Re-runnable: skips files already in their target form.
#>
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing
$dir = "C:\Users\micha\DevineClairvoyance\Assets"

$jpegCodec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() |
    Where-Object { $_.MimeType -eq 'image/jpeg' }
$qParams = New-Object System.Drawing.Imaging.EncoderParameters 1
$qParams.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter(
    [System.Drawing.Imaging.Encoder]::Quality, [int64]90)

function Resize-Bitmap($src, $maxDim) {
    $scale = [math]::Min(1.0, $maxDim / [math]::Max($src.Width, $src.Height))
    $w = [int]($src.Width * $scale); $h = [int]($src.Height * $scale)
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.DrawImage($src, 0, 0, $w, $h)
    $g.Dispose()
    return $bmp
}

# --- Transparent art: downscale, keep PNG ---
$artMax = @{ 'Card Stack' = 800; '3 Card Advice' = 640; '3 Card Challenge' = 640; '3 Card Current Situation' = 640 }
foreach ($name in $artMax.Keys) {
    $path = Join-Path $dir "$name.png"
    if (-not (Test-Path $path)) { continue }
    $src = [System.Drawing.Bitmap]::FromFile($path)
    $resized = Resize-Bitmap $src $artMax[$name]
    $src.Dispose()
    $resized.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $resized.Dispose()
    Write-Host "PNG  $name -> $($artMax[$name])px max"
}

# --- Remove unused Sparkle ---
$sparkle = Join-Path $dir "Sparkle.png"
if (Test-Path $sparkle) { Remove-Item $sparkle; Write-Host "Removed unused Sparkle.png" }

# --- Opaque images -> JPEG q90 (cards keep native size; tarot sheet too) ---
$keepPng = @('Card Stack.png','3 Card Advice.png','3 Card Challenge.png','3 Card Current Situation.png')
foreach ($f in Get-ChildItem "$dir\*.png" | Where-Object { $keepPng -notcontains $_.Name }) {
    $src = [System.Drawing.Bitmap]::FromFile($f.FullName)
    # Flatten onto white in case any stray alpha exists, then JPEG-encode.
    $flat = New-Object System.Drawing.Bitmap $src.Width, $src.Height
    $g = [System.Drawing.Graphics]::FromImage($flat)
    $g.Clear([System.Drawing.Color]::White)
    $g.DrawImage($src, 0, 0, $src.Width, $src.Height)
    $g.Dispose(); $src.Dispose()
    $jpgPath = [System.IO.Path]::ChangeExtension($f.FullName, ".jpg")
    $flat.Save($jpgPath, $jpegCodec, $qParams)
    $flat.Dispose()
    Remove-Item $f.FullName
}
Write-Host "Converted opaque images to JPEG q90."

$total = (Get-ChildItem $dir -File | Measure-Object Length -Sum).Sum
Write-Host ("Assets total now: {0:N1} MB" -f ($total/1MB)) -ForegroundColor Green

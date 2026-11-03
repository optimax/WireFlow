# Draws the WireFlow application icons at the standard Windows icon sizes and writes them as .ico files:
#   WireFlow.Designer\Designer.ico - white WF mark on a red tile, as in Documents\logo.png
#   Browser\Browser.ico            - red WF mark on a charcoal tile, after the Browser's MetroDark theme
# The mark is laid out on whole pixels at every size, so it stays sharp at 16 x 16.
# Run with Windows PowerShell 5.1 from the repository root:
#   powershell -ExecutionPolicy Bypass -File Documents\make-icons.ps1
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$Sizes = 16, 20, 24, 32, 40, 48, 64, 256

function Round-Half([double]$x) { [int][Math]::Round($x, [MidpointRounding]::AwayFromZero) }


# The WF mark of Documents\logo.png on a square icon of $S pixels: three bars and an F, as whole-pixel rectangles.
function Get-MarkRects([int]$S) {
    $pad = [Math]::Max(1, (Round-Half ($S * 0.09)))
    $w = $S - 2 * $pad                                  # mark width
    $gap = [Math]::Max(1, (Round-Half ($w * 0.044)))    # gap between bars
    $bar = Round-Half (($w - 3 * $gap) / 5.66)          # bar width; the F's arms are as tall as a bar is wide
    $arm = $w - 3 * $gap - 3 * $bar                     # F top arm width
    $midArm = Round-Half ($arm * 0.495)
    $h = Round-Half ($w * 268 / 478)                    # mark height, in the logo's proportions
    $armGap = [Math]::Max(1, (Round-Half ($bar * 0.44)))
    $top = [int][Math]::Floor(($S - $h) / 2)
    $midTop = $top + $bar + $armGap

    $x = $pad
    $rects = @(New-Object System.Drawing.Rectangle $x, $top, $bar, $h)
    $x += $bar + $gap
    $rects += New-Object System.Drawing.Rectangle $x, $midTop, $bar, ($top + $h - $midTop)
    $x += $bar + $gap
    $rects += New-Object System.Drawing.Rectangle $x, $top, $bar, $h
    $x += $bar + $gap
    $rects += New-Object System.Drawing.Rectangle $x, $top, $arm, $bar
    $rects += New-Object System.Drawing.Rectangle $x, $midTop, $midArm, $bar
    return $rects
}


# A square from $a to $b on both axes, with corners of diameter $d.
function New-RoundedSquare([double]$a, [double]$b, [double]$d) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc($a, $a, $d, $d, 180.0, 90.0)
    $path.AddArc($b - $d, $a, $d, $d, 270.0, 90.0)
    $path.AddArc($b - $d, $b - $d, $d, $d, 0.0, 90.0)
    $path.AddArc($a, $b - $d, $d, $d, 90.0, 90.0)
    $path.CloseFigure()
    return $path
}


# $rimColor, when given, outlines the tile so that a dark tile stays visible on a dark background.
function New-IconBitmap([int]$S, [System.Drawing.Color]$tileColor, [System.Drawing.Color]$markColor, [object]$rimColor) {
    $bmp = New-Object System.Drawing.Bitmap $S, $S, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::Transparent)

    # Rounded tile, anti-aliased, with path coordinates on pixel edges
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $d = 2 * [Math]::Max(2.0, $S / 8.0)
    $path = New-RoundedSquare 0.0 $S $d
    $tileBrush = New-Object System.Drawing.SolidBrush $tileColor
    $g.FillPath($tileBrush, $path)
    if ($rimColor) {
        $rim = [Math]::Max(1.0, [Math]::Round($S / 64.0))
        $rimPath = New-RoundedSquare ($rim / 2) ($S - $rim / 2) ($d - $rim)
        $rimPen = New-Object System.Drawing.Pen $rimColor, ([single]$rim)
        $g.DrawPath($rimPen, $rimPath)
        $rimPen.Dispose(); $rimPath.Dispose()
    }

    # Mark, pixel-exact
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::None
    $markBrush = New-Object System.Drawing.SolidBrush $markColor
    foreach ($rect in Get-MarkRects $S) { $g.FillRectangle($markBrush, $rect) }

    $markBrush.Dispose(); $tileBrush.Dispose(); $path.Dispose(); $g.Dispose()
    return $bmp
}


# An icon image as a 32-bit DIB: BITMAPINFOHEADER, bottom-up BGRA pixels, then the 1-bit AND mask.
function Get-DibBytes([System.Drawing.Bitmap]$bmp) {
    $S = $bmp.Width
    $rect = New-Object System.Drawing.Rectangle 0, 0, $S, $S
    $data = $bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $stride = $S * 4
    $pixels = New-Object byte[] ($stride * $S)
    for ($y = 0; $y -lt $S; $y++) {
        [System.Runtime.InteropServices.Marshal]::Copy([IntPtr]($data.Scan0.ToInt64() + $y * $data.Stride), $pixels, $y * $stride, $stride)
    }
    $bmp.UnlockBits($data)
    $maskStride = [int]([Math]::Ceiling($S / 32.0) * 4)

    $ms = New-Object System.IO.MemoryStream
    $w = New-Object System.IO.BinaryWriter $ms
    # The header's height counts both the colour bitmap and the mask
    $w.Write([int]40); $w.Write([int]$S); $w.Write([int]($S * 2)); $w.Write([int16]1); $w.Write([int16]32)
    $w.Write([int]0); $w.Write([int](($stride + $maskStride) * $S)); $w.Write([int]0); $w.Write([int]0); $w.Write([int]0); $w.Write([int]0)
    for ($y = $S - 1; $y -ge 0; $y--) { $w.Write($pixels, $y * $stride, $stride) }
    for ($y = $S - 1; $y -ge 0; $y--) {
        $row = New-Object byte[] $maskStride
        for ($x = 0; $x -lt $S; $x++) {
            if ($pixels[$y * $stride + $x * 4 + 3] -eq 0) { $row[$x -shr 3] = $row[$x -shr 3] -bor (0x80 -shr ($x -band 7)) }
        }
        $w.Write($row)
    }
    $w.Flush()
    return ,$ms.ToArray()
}


function Get-PngBytes([System.Drawing.Bitmap]$bmp) {
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    return ,$ms.ToArray()
}


# Writes an .ico with one image per size: DIBs below 256 pixels, PNG at 256, as Windows' own icons do.
function Write-Icon([string]$path, [System.Drawing.Color]$tileColor, [System.Drawing.Color]$markColor, [object]$rimColor) {
    $images = foreach ($S in $Sizes) {
        $bmp = New-IconBitmap $S $tileColor $markColor $rimColor
        if ($S -ge 256) { $bytes = Get-PngBytes $bmp } else { $bytes = Get-DibBytes $bmp }
        $bmp.Dispose()
        [pscustomobject]@{ Size = $S; Bytes = $bytes }
    }

    $ms = New-Object System.IO.MemoryStream
    $w = New-Object System.IO.BinaryWriter $ms
    $w.Write([int16]0); $w.Write([int16]1); $w.Write([int16]$images.Count)
    $offset = 6 + 16 * $images.Count
    foreach ($img in $images) {
        $dim = $img.Size
        if ($dim -ge 256) { $dim = 0 }
        $w.Write([byte]$dim); $w.Write([byte]$dim); $w.Write([byte]0); $w.Write([byte]0)
        $w.Write([int16]1); $w.Write([int16]32); $w.Write([int]$img.Bytes.Length); $w.Write([int]$offset)
        $offset += $img.Bytes.Length
    }
    foreach ($img in $images) { $w.Write([byte[]]$img.Bytes) }
    $w.Flush()
    [System.IO.File]::WriteAllBytes($path, $ms.ToArray())
    Write-Output "$path ($($images.Count) sizes, $($ms.Length) bytes)"
}


$red = [System.Drawing.Color]::FromArgb(255, 255, 0, 0)
$white = [System.Drawing.Color]::White
$charcoal = [System.Drawing.Color]::FromArgb(255, 48, 48, 48)
$rimGray = [System.Drawing.Color]::FromArgb(255, 96, 96, 96)

Write-Icon (Join-Path $Root 'WireFlow.Designer\Designer.ico') $red $white $null
Write-Icon (Join-Path $Root 'Browser\Browser.ico') $charcoal $red $rimGray

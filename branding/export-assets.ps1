$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = $PSScriptRoot
$source = Join-Path $root 'source/msf-lang-brand-board.png'
$img = [System.Drawing.Bitmap]::new($source)
function Export-Crop($rect, $target) {
    $crop = $img.Clone($rect, $img.PixelFormat)
    $crop.Save((Join-Path $root $target), [System.Drawing.Imaging.ImageFormat]::Png)
    $crop.Dispose()
}
Export-Crop ([System.Drawing.Rectangle]::new(330,70,880,670)) 'logo/msf-lang-logo.png'
Export-Crop ([System.Drawing.Rectangle]::new(330,70,880,670)) 'logo/msf-lang-logo-dark.png'
Export-Crop ([System.Drawing.Rectangle]::new(440,70,630,510)) 'logo/msf-lang-symbol.png'
$symbol = $img.Clone([System.Drawing.Rectangle]::new(440,70,630,510), $img.PixelFormat)
foreach ($n in @(16,32,48,192,512)) {
    $bitmap = [System.Drawing.Bitmap]::new($n,$n)
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.Clear([System.Drawing.Color]::FromArgb(2,8,28))
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $h = [int][Math]::Round($n * 510 / 630)
    $g.DrawImage($symbol,0,[int](($n-$h)/2),$n,$h)
    $bitmap.Save((Join-Path $root "icons/icon-$n.png"),[System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bitmap.Dispose()
}
# ICO supports PNG-compressed frames, preserving all requested small sizes.
$frames = @(16,32,48 | ForEach-Object { ,[System.IO.File]::ReadAllBytes((Join-Path $root "icons/icon-$_.png")) })
$stream = [System.IO.File]::Create((Join-Path $root 'icons/favicon.ico'))
$writer = [System.IO.BinaryWriter]::new($stream)
$writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]3)
$offset = 54
for ($i=0; $i -lt 3; $i++) {
    $size = @(16,32,48)[$i]
    $writer.Write([byte]$size); $writer.Write([byte]$size)
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]32)
    $writer.Write([uint32]$frames[$i].Length); $writer.Write([uint32]$offset)
    $offset += $frames[$i].Length
}
foreach ($frame in $frames) { $writer.Write([byte[]]$frame) }
$writer.Dispose(); $symbol.Dispose(); $img.Dispose()

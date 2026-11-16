$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = $PSScriptRoot
function New-Mark([int]$size, [string]$path, [bool]$white) {
  $b = [Drawing.Bitmap]::new($size,$size,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $g = [Drawing.Graphics]::FromImage($b); $g.SmoothingMode = 'AntiAlias'; $g.Clear([Drawing.Color]::Transparent)
  if ($white) { $g.Clear([Drawing.Color]::FromArgb(250,248,255)) }
  $scale=$size/600.0; $pen=[Drawing.Pen]::new([Drawing.Color]::FromArgb(151,55,245),[float](78*$scale)); $pen.StartCap='Round'; $pen.EndCap='Round'; $pen.LineJoin='Round'
  $p=[Drawing.Drawing2D.GraphicsPath]::new(); $p.AddLine(135*$scale,430*$scale,135*$scale,150*$scale); $p.AddLine(135*$scale,150*$scale,230*$scale,295*$scale); $p.AddLine(230*$scale,295*$scale,320*$scale,165*$scale); $p.AddLine(320*$scale,165*$scale,410*$scale,295*$scale); $p.AddLine(410*$scale,295*$scale,485*$scale,150*$scale); $p.AddLine(485*$scale,150*$scale,485*$scale,310*$scale)
  $g.DrawPath($pen,$p); $p.Dispose(); $pen.Dispose()
  $loop=[Drawing.Pen]::new([Drawing.Color]::FromArgb(151,55,245),[float](78*$scale)); $loop.StartCap='Round'; $loop.EndCap='Round'; $arc=[Drawing.Rectangle]::new([int](170*$scale),[int](285*$scale),[int](300*$scale),[int](190*$scale)); $g.DrawArc($loop,$arc,0,180); $loop.Dispose()
  $brush=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(0,221,255)); $d=[float](46*$scale); $g.FillEllipse($brush,[float](365*$scale),[float](375*$scale),$d,$d); $brush.Dispose(); $g.Dispose(); $b.Save($path,[Drawing.Imaging.ImageFormat]::Png); $b.Dispose()
}
New-Mark 600 (Join-Path $root 'logo/msf-lang-logo.png') $false
Copy-Item (Join-Path $root 'logo/msf-lang-logo.png') (Join-Path $root 'logo/msf-lang-logo-dark.png') -Force
New-Mark 600 (Join-Path $root 'logo/msf-lang-logo-light.png') $true
New-Mark 600 (Join-Path $root 'logo/msf-lang-logo-monochrome.png') $true
foreach($n in 16,32,48,192,512){ New-Mark $n (Join-Path $root "icons/icon-$n.png") $false }

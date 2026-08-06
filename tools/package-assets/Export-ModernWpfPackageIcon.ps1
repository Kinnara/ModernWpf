param(
    [string]$SourcePath = (Join-Path $PSScriptRoot "..\..\ModernWpf.Gallery\App.xaml"),

    [string]$DestinationPath = (Join-Path $PSScriptRoot "..\..\ModernWpf.Controls\icon.png")
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName WindowsBase

$presentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation"
$xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml"

[xml]$source = Get-Content -LiteralPath $SourcePath -Raw
$namespaces = [System.Xml.XmlNamespaceManager]::new($source.NameTable)
$namespaces.AddNamespace("p", $presentationNamespace)
$namespaces.AddNamespace("x", $xamlNamespace)

$logoNode = $source.SelectSingleNode(
    "//p:DrawingImage[@x:Key='ModernWpfLogoImage']",
    $namespaces)
if ($null -eq $logoNode) {
    throw "Gallery App.xaml does not define the ModernWpfLogoImage vector."
}

# Load a standalone copy of the Gallery-owned vector. Removing x:Key makes it
# a normal DrawingImage rather than a resource-dictionary entry.
$standalone = [System.Xml.XmlDocument]::new()
$drawingNode = $standalone.ImportNode($logoNode, $true)
$drawingNode.RemoveAttribute("Key", $xamlNamespace)
$drawingNode.SetAttribute("xmlns", $presentationNamespace)
$standalone.AppendChild($drawingNode) | Out-Null

$nodeReader = [System.Xml.XmlNodeReader]::new($drawingNode)
try {
    $drawingImage = [System.Windows.Markup.XamlReader]::Load($nodeReader)
}
finally {
    $nodeReader.Dispose()
}

if ($drawingImage -isnot [System.Windows.Media.DrawingImage]) {
    throw "ModernWpfLogoImage did not load as a DrawingImage."
}

$pixelSize = 128
$visual = [System.Windows.Media.DrawingVisual]::new()
$drawingContext = $visual.RenderOpen()
try {
    $drawingContext.DrawImage(
        $drawingImage,
        [System.Windows.Rect]::new(0, 0, $pixelSize, $pixelSize))
}
finally {
    $drawingContext.Close()
}

$bitmap = [System.Windows.Media.Imaging.RenderTargetBitmap]::new(
    $pixelSize,
    $pixelSize,
    96,
    96,
    [System.Windows.Media.PixelFormats]::Pbgra32)
$bitmap.Render($visual)

$encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
$encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))

$resolvedDestination = [System.IO.Path]::GetFullPath($DestinationPath)
$destinationDirectory = [System.IO.Path]::GetDirectoryName($resolvedDestination)
[System.IO.Directory]::CreateDirectory($destinationDirectory) | Out-Null
$stream = [System.IO.File]::Open(
    $resolvedDestination,
    [System.IO.FileMode]::Create,
    [System.IO.FileAccess]::Write,
    [System.IO.FileShare]::None)
try {
    $encoder.Save($stream)
}
finally {
    $stream.Dispose()
}

Write-Host "Exported the Gallery ModernWpfLogoImage vector to $resolvedDestination"

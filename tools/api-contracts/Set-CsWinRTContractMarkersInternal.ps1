param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectionPath
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $ProjectionPath)) {
    throw "C#/WinRT projection file was not generated: $ProjectionPath"
}

$text = [System.IO.File]::ReadAllText($ProjectionPath)
$markers = @(
    "FoundationContract",
    "UniversalApiContract"
)

$changed = $false
foreach ($marker in $markers) {
    $publicDeclaration = "public enum $marker"
    $internalDeclaration = "internal enum $marker"

    if ($text.Contains($publicDeclaration)) {
        $text = $text.Replace($publicDeclaration, $internalDeclaration)
        $changed = $true
    }
    elseif (-not $text.Contains($internalDeclaration)) {
        throw "C#/WinRT projection does not contain the expected $marker declaration."
    }
}

if ($changed) {
    [System.IO.File]::WriteAllText(
        $ProjectionPath,
        $text,
        [System.Text.UTF8Encoding]::new($false))
}

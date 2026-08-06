[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Path,

    [Parameter(Mandatory = $true)]
    [string]$From,

    [Parameter(Mandatory = $true)]
    [string]$To,

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$ExpectedOccurrences
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedPath = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path
$bytes = [IO.File]::ReadAllBytes($resolvedPath)
$hasUtf8Bom = $bytes.Length -ge 3 -and
    $bytes[0] -eq 0xEF -and
    $bytes[1] -eq 0xBB -and
    $bytes[2] -eq 0xBF
$offset = if ($hasUtf8Bom) { 3 } else { 0 }
$text = [Text.Encoding]::UTF8.GetString($bytes, $offset, $bytes.Length - $offset)
$actualOccurrences = ([Regex]::Matches(
    $text,
    [Regex]::Escape($From),
    [Text.RegularExpressions.RegexOptions]::CultureInvariant)).Count
if ($actualOccurrences -ne $ExpectedOccurrences) {
    throw "Expected $ExpectedOccurrences literal '$From' occurrence(s) in " +
        "'$resolvedPath'; found $actualOccurrences."
}
if ($From -eq $To) {
    throw 'A text-replacement migration must change its input.'
}

$updatedText = $text.Replace($From, $To, [StringComparison]::Ordinal)
$encoding = [Text.UTF8Encoding]::new($hasUtf8Bom)
[IO.File]::WriteAllText($resolvedPath, $updatedText, $encoding)
Write-Output $resolvedPath

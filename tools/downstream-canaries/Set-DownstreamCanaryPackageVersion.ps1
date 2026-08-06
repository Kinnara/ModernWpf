[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,

    [Parameter(Mandatory = $true)]
    [string]$PackageId,

    [Parameter(Mandatory = $true)]
    [string]$FromVersion,

    [Parameter(Mandatory = $true)]
    [string]$ToVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedProjectPath = (Resolve-Path -LiteralPath $ProjectPath -ErrorAction Stop).Path
$bytes = [IO.File]::ReadAllBytes($resolvedProjectPath)
$hasUtf8Bom = $bytes.Length -ge 3 -and
    $bytes[0] -eq 0xEF -and
    $bytes[1] -eq 0xBB -and
    $bytes[2] -eq 0xBF
$offset = if ($hasUtf8Bom) { 3 } else { 0 }
$text = [Text.Encoding]::UTF8.GetString($bytes, $offset, $bytes.Length - $offset)

$escapedPackageId = [Regex]::Escape($PackageId)
$escapedFromVersion = [Regex]::Escape($FromVersion)
$quote = '["'']'
$includeLookahead = "(?=[^>]*\bInclude\s*=\s*$quote$escapedPackageId$quote)"
$attributePattern =
    "(?is)(<PackageReference\b$includeLookahead[^>]*\bVersion\s*=\s*$quote)" +
    "($escapedFromVersion)($quote)"
$elementPattern =
    "(?is)(<PackageReference\b$includeLookahead[^>]*>" +
    "(?:(?!</PackageReference>).)*?<Version>\s*)" +
    "($escapedFromVersion)(\s*</Version>)"

$attributeMatches = [Regex]::Matches($text, $attributePattern)
$elementMatches = [Regex]::Matches($text, $elementPattern)
$matchCount = $attributeMatches.Count + $elementMatches.Count
if ($matchCount -ne 1) {
    throw "Expected exactly one '$PackageId' PackageReference at version " +
        "'$FromVersion' in '$resolvedProjectPath'; found $matchCount."
}

$pattern = if ($attributeMatches.Count -eq 1) {
    $attributePattern
}
else {
    $elementPattern
}
$updatedText = [Regex]::Replace(
    $text,
    $pattern,
    { param($match) $match.Groups[1].Value + $ToVersion + $match.Groups[3].Value },
    1)

$encoding = [Text.UTF8Encoding]::new($hasUtf8Bom)
[IO.File]::WriteAllText($resolvedProjectPath, $updatedText, $encoding)
Write-Output $resolvedProjectPath

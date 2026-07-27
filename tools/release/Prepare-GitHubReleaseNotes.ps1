[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$SourcePath,

    [Parameter(Mandatory)]
    [string]$DestinationPath,

    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')]
    [string]$Repository,

    [Parameter(Mandatory)]
    [ValidatePattern('^v[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?$')]
    [string]$Tag,

    [Parameter(Mandatory)]
    [string]$RepositoryRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRootPath = [IO.Path]::GetFullPath($RepositoryRoot).TrimEnd(
    [char[]]@([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar))
$repositoryPrefix = "$repositoryRootPath$([IO.Path]::DirectorySeparatorChar)"
$sourceFullPath = (Resolve-Path -LiteralPath $SourcePath).Path

if (-not $sourceFullPath.StartsWith(
    $repositoryPrefix,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "Release notes must be inside the repository: $sourceFullPath"
}

$sourceDirectory = Split-Path -Parent $sourceFullPath
$releaseNotes = Get-Content -LiteralPath $sourceFullPath -Raw
$relativeMarkdownLinkPattern =
    '(?<prefix>\]\()(?<target>(?![A-Za-z][A-Za-z0-9+.-]*:|/|#)[^)]+\.md(?:#[^)]+)?)(?<suffix>\))'

[Text.RegularExpressions.MatchEvaluator]$replaceRelativeMarkdownLink = {
    param([Text.RegularExpressions.Match]$match)

    $target = $match.Groups['target'].Value
    $fragmentIndex = $target.IndexOf('#')
    if ($fragmentIndex -ge 0) {
        $targetPath = $target.Substring(0, $fragmentIndex)
        $fragment = $target.Substring($fragmentIndex)
    }
    else {
        $targetPath = $target
        $fragment = ''
    }

    $targetFullPath = [IO.Path]::GetFullPath((Join-Path $sourceDirectory $targetPath))
    if (-not $targetFullPath.StartsWith(
        $repositoryPrefix,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "Release-note link leaves the repository: $target"
    }
    if (-not (Test-Path -LiteralPath $targetFullPath -PathType Leaf)) {
        throw "Release-note link target does not exist: $target"
    }

    $targetRelativePath = [IO.Path]::GetRelativePath(
        $repositoryRootPath,
        $targetFullPath).Replace('\', '/')
    $targetUrl = "https://github.com/$Repository/blob/$Tag/$targetRelativePath$fragment"

    return "$($match.Groups['prefix'].Value)$targetUrl$($match.Groups['suffix'].Value)"
}

$preparedReleaseNotes = [Text.RegularExpressions.Regex]::Replace(
    $releaseNotes,
    $relativeMarkdownLinkPattern,
    $replaceRelativeMarkdownLink)
$destinationFullPath = [IO.Path]::GetFullPath($DestinationPath)
$destinationDirectory = Split-Path -Parent $destinationFullPath

if (-not (Test-Path -LiteralPath $destinationDirectory -PathType Container)) {
    New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
}

[IO.File]::WriteAllText(
    $destinationFullPath,
    $preparedReleaseNotes,
    [Text.UTF8Encoding]::new($false))

param(
    [Parameter(Mandatory = $true)]
    [string]$StableTag,

    [Parameter(Mandatory = $true)]
    [string]$AcceptedRcTag,

    [Parameter(Mandatory = $true)]
    [string]$AcceptedRcPublishedAt,

    [string]$RepositoryRoot = (Join-Path $PSScriptRoot "..\.."),

    [string]$NowUtc = ([DateTimeOffset]::UtcNow.ToString("O"))
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$resolvedRepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path

function Invoke-RepositoryGit {
    param([string[]]$Arguments)

    $output = @(& git -C $resolvedRepositoryRoot @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed: $($output -join [Environment]::NewLine)"
    }

    return $output
}

function Get-RequiredPropertyNode {
    param(
        [xml]$Document,
        [string]$Name,
        [string]$Source
    )

    $nodes = @($Document.SelectNodes(
        "/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='$Name']"))
    if ($nodes.Count -ne 1) {
        throw "'$Source' must contain exactly one '$Name' property."
    }

    return $nodes[0]
}

function ConvertTo-UtcDateTimeOffset {
    param(
        [string]$Value,
        [string]$Name
    )

    [DateTimeOffset]$parsed = [DateTimeOffset]::MinValue
    $styles =
        [Globalization.DateTimeStyles]::AssumeUniversal -bor
        [Globalization.DateTimeStyles]::AdjustToUniversal
    if (-not [DateTimeOffset]::TryParse(
        $Value,
        [Globalization.CultureInfo]::InvariantCulture,
        $styles,
        [ref]$parsed)) {
        throw "'$Name' is not a valid timestamp: '$Value'."
    }

    return $parsed.ToUniversalTime()
}

if ($StableTag -notmatch '^v(?<version>[0-9]+\.[0-9]+\.[0-9]+)$') {
    throw "Stable tag must have the form v<major>.<minor>.<patch>: '$StableTag'."
}
$stableVersion = $Matches.version

$escapedStableVersion = [regex]::Escape($stableVersion)
if ($AcceptedRcTag -notmatch "^v$escapedStableVersion-rc\.(?<number>[1-9][0-9]*)$") {
    throw "Accepted RC tag must have the form 'v$stableVersion-rc.<positive integer>': '$AcceptedRcTag'."
}
$acceptedRcVersion = $AcceptedRcTag.Substring(1)

$stableTagType = [string](
    Invoke-RepositoryGit -Arguments @("cat-file", "-t", "refs/tags/$StableTag") |
        Select-Object -First 1)
if ($stableTagType -ne "tag") {
    throw "Stable tag '$StableTag' must be annotated."
}

$rcTagType = [string](
    Invoke-RepositoryGit -Arguments @("cat-file", "-t", "refs/tags/$AcceptedRcTag") |
        Select-Object -First 1)
if ($rcTagType -ne "tag") {
    throw "Accepted RC tag '$AcceptedRcTag' must be annotated."
}

$stableCommit = [string](
    Invoke-RepositoryGit -Arguments @("rev-list", "-n", "1", "refs/tags/$StableTag") |
        Select-Object -First 1)
$rcCommit = [string](
    Invoke-RepositoryGit -Arguments @("rev-list", "-n", "1", "refs/tags/$AcceptedRcTag") |
        Select-Object -First 1)

& git -C $resolvedRepositoryRoot merge-base --is-ancestor $rcCommit $stableCommit
if ($LASTEXITCODE -ne 0) {
    throw "Accepted RC '$AcceptedRcTag' is not an ancestor of '$StableTag'."
}

$publishedAt = ConvertTo-UtcDateTimeOffset `
    -Value $AcceptedRcPublishedAt `
    -Name "AcceptedRcPublishedAt"
$now = ConvertTo-UtcDateTimeOffset -Value $NowUtc -Name "NowUtc"
$minimumSoak = [TimeSpan]::FromDays(14)
if (($now - $publishedAt) -lt $minimumSoak) {
    throw "Accepted RC '$AcceptedRcTag' has not completed the 14-day soak from publication."
}

$rcPropsText = @(
    Invoke-RepositoryGit -Arguments @("show", "${AcceptedRcTag}:Directory.Build.props")) -join "`n"
$stablePropsText = @(
    Invoke-RepositoryGit -Arguments @("show", "${StableTag}:Directory.Build.props")) -join "`n"
[xml]$rcProps = $rcPropsText
[xml]$stableProps = $stablePropsText

$rcVersionNode = Get-RequiredPropertyNode `
    -Document $rcProps `
    -Name "Version" `
    -Source "$AcceptedRcTag`:Directory.Build.props"
$stableVersionNode = Get-RequiredPropertyNode `
    -Document $stableProps `
    -Name "Version" `
    -Source "$StableTag`:Directory.Build.props"
$rcBaselineNode = Get-RequiredPropertyNode `
    -Document $rcProps `
    -Name "ModernWpfPackageValidationBaselineVersion" `
    -Source "$AcceptedRcTag`:Directory.Build.props"
$stableBaselineNode = Get-RequiredPropertyNode `
    -Document $stableProps `
    -Name "ModernWpfPackageValidationBaselineVersion" `
    -Source "$StableTag`:Directory.Build.props"

if ($rcVersionNode.InnerText -ne $acceptedRcVersion) {
    throw "Accepted RC tag '$AcceptedRcTag' contains package version '$($rcVersionNode.InnerText)'."
}
if ($stableVersionNode.InnerText -ne $stableVersion) {
    throw "Stable tag '$StableTag' contains package version '$($stableVersionNode.InnerText)'."
}
if ($stableBaselineNode.InnerText -ne $acceptedRcVersion) {
    throw "Stable package-validation baseline must be the accepted RC version '$acceptedRcVersion'."
}

$rcVersionNode.InnerText = "{VERSION}"
$stableVersionNode.InnerText = "{VERSION}"
$rcBaselineNode.InnerText = "{PACKAGE_VALIDATION_BASELINE}"
$stableBaselineNode.InnerText = "{PACKAGE_VALIDATION_BASELINE}"
if ($rcProps.OuterXml -cne $stableProps.OuterXml) {
    throw "Directory.Build.props differs from '$AcceptedRcTag' outside Version and ModernWpfPackageValidationBaselineVersion."
}

$allowedStablePaths = @(
    "Directory.Build.props",
    "README.md",
    "ModernWpf.Controls/readme.md",
    "samples/PackageConsumer/README.md",
    "docs/release-notes-$stableVersion.md"
)
$diffLines = @(
    Invoke-RepositoryGit -Arguments @(
        "-c",
        "core.quotepath=false",
        "diff",
        "--name-status",
        "--no-renames",
        "$AcceptedRcTag..$StableTag",
        "--"))
$invalidChanges = @()
foreach ($line in $diffLines) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    $parts = @($line -split "`t")
    if ($parts.Count -ne 2 -or
        $parts[0] -notin @("A", "M") -or
        $allowedStablePaths -notcontains $parts[1]) {
        $invalidChanges += $line
    }
}

if ($invalidChanges.Count -ne 0) {
    throw "Stable release contains changes outside the allowed version and release-document delta: $($invalidChanges -join ', ')"
}

Write-Host "Validated stable lineage from $AcceptedRcTag to $StableTag after a 14-day published-RC soak."

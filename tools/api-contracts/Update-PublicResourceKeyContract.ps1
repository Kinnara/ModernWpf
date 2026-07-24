[CmdletBinding()]
param(
    [switch]$InitializeShippedBaseline
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$projectRoot = Join-Path $repositoryRoot "ModernWpf"
$shippedPath = Join-Path $projectRoot "PublicResourceKeys.Shipped.txt"
$unshippedPath = Join-Path $projectRoot "PublicResourceKeys.Unshipped.txt"
$xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml"

$contractSources = @(
    "DensityStyles/Compact.xaml",
    "ModernWpfControlsResources.xaml",
    "ThemeResources/Dark.xaml",
    "ThemeResources/HighContrast.xaml",
    "ThemeResources/Light.xaml"
)

function Get-ContractEntries {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return @()
    }

    return @(
        Get-Content -LiteralPath $Path |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ -and -not $_.StartsWith("#", [System.StringComparison]::Ordinal) }
    )
}

function Write-ContractFile {
    param(
        [string]$Path,
        [string[]]$Entries,
        [string]$State
    )

    $uniqueEntries = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)
    foreach ($entry in $Entries) {
        [void]$uniqueEntries.Add($entry)
    }

    [string[]]$sortedEntries = @($uniqueEntries)
    [System.Array]::Sort($sortedEntries, [System.StringComparer]::Ordinal)
    $lines = @(
        "# ModernWpf v1 public resource-key contract ($State).",
        "# Format: project-relative XAML path|literal top-level x:Key",
        "# Template parts, visual states, implicit/type keys, and unlisted keys are not contracted."
    ) + $sortedEntries

    [System.IO.File]::WriteAllLines(
        $Path,
        $lines,
        [System.Text.UTF8Encoding]::new($false))
}

$currentEntries = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::Ordinal)

foreach ($relativePath in $contractSources) {
    $fullPath = Join-Path $projectRoot $relativePath
    [xml]$document = Get-Content -LiteralPath $fullPath -Raw

    foreach ($node in $document.DocumentElement.ChildNodes) {
        if ($node.NodeType -ne [System.Xml.XmlNodeType]::Element) {
            continue
        }

        $key = $node.GetAttribute("Key", $xamlNamespace)
        if ([string]::IsNullOrWhiteSpace($key) -or
            $key.StartsWith("{", [System.StringComparison]::Ordinal)) {
            continue
        }

        if ($key.Contains("|")) {
            throw "Resource key '$key' in '$relativePath' contains the contract separator '|'."
        }

        $entry = "$relativePath|$key"
        if (-not $currentEntries.Add($entry)) {
            throw "Duplicate top-level resource key contract entry '$entry'."
        }
    }
}

$shippedEntries = @(Get-ContractEntries $shippedPath)
$unshippedEntries = @(Get-ContractEntries $unshippedPath)
$declaredEntries = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::Ordinal)

foreach ($entry in @($shippedEntries + $unshippedEntries)) {
    if (-not $declaredEntries.Add($entry)) {
        throw "Duplicate public resource-key contract entry '$entry'."
    }
}

$removedEntries = @(
    $declaredEntries |
        Where-Object { -not $currentEntries.Contains($_) } |
        Sort-Object
)

if ($removedEntries.Count -ne 0) {
    throw "Declared public resource keys are missing from source:`n$($removedEntries -join [Environment]::NewLine)"
}

if ($InitializeShippedBaseline) {
    Write-ContractFile $shippedPath @($currentEntries) "shipped"
    Write-ContractFile $unshippedPath @() "unshipped"
    Write-Host "Initialized $($currentEntries.Count) shipped public resource-key entries."
    return
}

$newEntries = @(
    $currentEntries |
        Where-Object { -not $declaredEntries.Contains($_) } |
        Sort-Object
)

Write-ContractFile $unshippedPath @($unshippedEntries + $newEntries) "unshipped"
Write-Host "Added $($newEntries.Count) new public resource-key entries to '$unshippedPath'."

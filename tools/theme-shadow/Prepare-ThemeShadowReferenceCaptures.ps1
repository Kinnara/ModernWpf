[CmdletBinding()]
param(
    [string]$ManifestPath,

    [Parameter(Mandatory = $true)]
    [string]$SnapshotDir,

    [Parameter(Mandatory = $true)]
    [string]$ReferenceDir,

    [string]$ChecklistPath,

    [switch]$RequireReferencePngs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-RepoRelativePath {
    param(
        [string]$Path
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $Path))
}

function Test-ExistingFile {
    param(
        [string]$Path
    )

    return [System.IO.File]::Exists($Path)
}

function Escape-MarkdownCell {
    param(
        [string]$Value
    )

    if ($null -eq $Value) {
        return ""
    }

    return $Value.Replace("\", "\\").Replace("|", "\|")
}

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = [System.IO.Path]::GetFullPath((Join-Path $ScriptRoot "..\.."))

if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $RepoRoot "docs\theme-shadow-reference-captures.json"
}

$ManifestPath = Resolve-RepoRelativePath $ManifestPath
$SnapshotDir = [System.IO.Path]::GetFullPath($SnapshotDir)
$ReferenceDir = [System.IO.Path]::GetFullPath($ReferenceDir)

if (-not (Test-ExistingFile $ManifestPath)) {
    throw "ThemeShadow reference capture manifest not found: $ManifestPath"
}

if (-not [System.IO.Directory]::Exists($SnapshotDir)) {
    throw "ModernWpf shadow snapshot directory not found: $SnapshotDir"
}

$manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
$targets = @($manifest.targets)

if ($targets.Count -eq 0) {
    throw "ThemeShadow reference capture manifest has no targets: $ManifestPath"
}

New-Item -ItemType Directory -Force -Path $ReferenceDir | Out-Null

if ([string]::IsNullOrWhiteSpace($ChecklistPath)) {
    $checklistFileProperty = $manifest.PSObject.Properties["referenceChecklistFile"]
    $checklistFile = if ($null -ne $checklistFileProperty -and -not [string]::IsNullOrWhiteSpace($checklistFileProperty.Value)) {
        [string]$checklistFileProperty.Value
    }
    else {
        "theme-shadow-reference-captures-checklist.md"
    }

    $ChecklistPath = Join-Path $ReferenceDir $checklistFile
}

$ChecklistPath = [System.IO.Path]::GetFullPath($ChecklistPath)
$ChecklistDirectory = Split-Path -Parent $ChecklistPath
if (-not [string]::IsNullOrWhiteSpace($ChecklistDirectory)) {
    New-Item -ItemType Directory -Force -Path $ChecklistDirectory | Out-Null
}

$missingSnapshots = New-Object System.Collections.Generic.List[string]
$missingReferencePngs = New-Object System.Collections.Generic.List[string]
$rows = New-Object System.Collections.Generic.List[object]

foreach ($target in $targets) {
    $fileBase = [string]$target.referenceFileBase
    $snapshotPng = Join-Path $SnapshotDir "$fileBase.png"
    $snapshotMetrics = Join-Path $SnapshotDir "$fileBase.txt"
    $snapshotMask = Join-Path $SnapshotDir "$fileBase.mask.txt"
    $referencePng = Join-Path $ReferenceDir "$fileBase.png"
    $referenceMask = Join-Path $ReferenceDir "$fileBase.mask.txt"

    foreach ($requiredSnapshot in @($snapshotPng, $snapshotMetrics, $snapshotMask)) {
        if (-not (Test-ExistingFile $requiredSnapshot)) {
            $missingSnapshots.Add($requiredSnapshot)
        }
    }

    $maskStatus = "missing"
    if (Test-ExistingFile $snapshotMask) {
        Copy-Item -LiteralPath $snapshotMask -Destination $referenceMask -Force
        $maskStatus = "staged"
    }

    $referenceStatus = "missing live PNG"
    if (Test-ExistingFile $referencePng) {
        $referenceStatus = "present"
    }
    else {
        $missingReferencePngs.Add($referencePng)
    }

    $rows.Add([pscustomobject]@{
        Name = [string]$target.name
        FileBase = $fileBase
        Canvas = "$($target.canvasSize.width)x$($target.canvasSize.height)"
        ReferencePng = $referencePng
        Mask = $maskStatus
        Status = $referenceStatus
    })
}

if ($missingSnapshots.Count -gt 0) {
    throw "Missing ModernWpf shadow snapshot files:`n$($missingSnapshots -join [Environment]::NewLine)"
}

if ($RequireReferencePngs -and $missingReferencePngs.Count -gt 0) {
    throw "Missing live WinUI reference PNGs:`n$($missingReferencePngs -join [Environment]::NewLine)"
}

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# ThemeShadow WinUI Reference Capture Checklist")
$lines.Add("")
$lines.Add("Manifest: ``$ManifestPath``")
$lines.Add("Snapshot directory: ``$SnapshotDir``")
$lines.Add("Reference directory: ``$ReferenceDir``")
$lines.Add("")
$lines.Add("Capture or copy each live WinUI reference PNG into the reference directory with the exact file name below. The staged ``.mask.txt`` files come from the ModernWpf shadow snapshot exporter and should remain beside PNG references that include their opaque caster/content region.")
$lines.Add("")
$lines.Add("| Target | Canvas | Reference PNG | Mask | Status |")
$lines.Add("| --- | --- | --- | --- | --- |")

foreach ($row in $rows) {
    $lines.Add((
        "| {0} | {1} | {2}.png | {3} | {4} |" -f
        (Escape-MarkdownCell $row.Name),
        (Escape-MarkdownCell $row.Canvas),
        (Escape-MarkdownCell $row.FileBase),
        (Escape-MarkdownCell $row.Mask),
        (Escape-MarkdownCell $row.Status)))
}

$lines.Add("")
$lines.Add("After the live WinUI PNGs are present, rerun this script with ``-RequireReferencePngs`` and then run the ModernWpf reference-directory and rendered-template comparison tests with ``MODERNWPF_SHADOW_REFERENCE_DIR`` pointing at this directory.")

Set-Content -LiteralPath $ChecklistPath -Value $lines -Encoding UTF8

Write-Host "Staged ThemeShadow reference masks for $($targets.Count) targets."
Write-Host "Checklist: $ChecklistPath"
if ($missingReferencePngs.Count -eq 0) {
    Write-Host "Live WinUI reference PNG check passed."
}
else {
    Write-Host "Live WinUI reference PNGs are still missing; rerun with -RequireReferencePngs after capture."
}

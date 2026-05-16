param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [string[]]$TargetFrameworks = @(
        "net462",
        "net8.0-windows7.0",
        "net10.0-windows7.0"
    )
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression.FileSystem

$resolvedPackagePath = (Resolve-Path $PackagePath).Path
$zip = [System.IO.Compression.ZipFile]::OpenRead($resolvedPackagePath)

try {
    $entries = @($zip.Entries | ForEach-Object { $_.FullName })

    function Assert-PackageEntry {
        param([string]$Path)

        if ($entries -notcontains $Path) {
            throw "Package '$resolvedPackagePath' is missing '$Path'."
        }
    }

    Assert-PackageEntry "ModernWpfUI.nuspec"
    Assert-PackageEntry "readme.md"

    foreach ($targetFramework in $TargetFrameworks) {
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.dll"
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.xml"
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.Controls.dll"
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.Controls.xml"
    }

    $legacyEntries = @($entries | Where-Object {
        $_ -match "^lib/(net45|netcoreapp3\.0|net5\.0-windows)(/|$)"
    })

    if ($legacyEntries.Count -ne 0) {
        throw "Package '$resolvedPackagePath' contains legacy 0.x target assets: $($legacyEntries -join ', ')"
    }

    $nuspecEntry = $zip.Entries | Where-Object { $_.FullName -eq "ModernWpfUI.nuspec" } | Select-Object -First 1
    if ($null -eq $nuspecEntry) {
        throw "Package '$resolvedPackagePath' has no nuspec entry."
    }

    $reader = [System.IO.StreamReader]::new($nuspecEntry.Open())
    try {
        [xml]$nuspec = $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }

    $metadata = $nuspec.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']")
    if ($null -eq $metadata) {
        throw "Package '$resolvedPackagePath' has no nuspec metadata."
    }

    $readme = $metadata.SelectSingleNode("*[local-name()='readme']")
    if ($null -eq $readme -or $readme.InnerText -ne "readme.md") {
        throw "Package '$resolvedPackagePath' must declare readme.md in nuspec metadata."
    }

    $dependencyGroups = @($metadata.SelectNodes("*[local-name()='dependencies']/*[local-name()='group']") | ForEach-Object {
        $_.GetAttribute("targetFramework")
    })

    $expectedDependencyGroups = @{
        "net462" = ".NETFramework4.6.2"
        "net8.0-windows7.0" = "net8.0-windows7.0"
        "net10.0-windows7.0" = "net10.0-windows7.0"
    }

    foreach ($targetFramework in $TargetFrameworks) {
        $dependencyGroup = $expectedDependencyGroups[$targetFramework]
        if ($null -eq $dependencyGroup) {
            $dependencyGroup = $targetFramework
        }

        if ($dependencyGroups -notcontains $dependencyGroup) {
            throw "Package '$resolvedPackagePath' has no dependency group for '$dependencyGroup'."
        }
    }

    $frameworkReferenceGroups = @($metadata.SelectNodes("*[local-name()='frameworkReferences']/*[local-name()='group']") | ForEach-Object {
        $_.GetAttribute("targetFramework")
    })

    foreach ($targetFramework in @("net8.0-windows7.0", "net10.0-windows7.0")) {
        if ($frameworkReferenceGroups -notcontains $targetFramework) {
            throw "Package '$resolvedPackagePath' has no WPF framework reference group for '$targetFramework'."
        }
    }
}
finally {
    $zip.Dispose()
}

Write-Host "Verified ModernWpfUI package: $resolvedPackagePath"

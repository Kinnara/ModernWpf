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
Add-Type -AssemblyName System.Reflection.Metadata

$resolvedPackagePath = (Resolve-Path $PackagePath).Path
$zip = [System.IO.Compression.ZipFile]::OpenRead($resolvedPackagePath)

try {
    $entries = @($zip.Entries | ForEach-Object { $_.FullName })
    $publicTypeSurfaces = @{}

    function Assert-PackageEntry {
        param([string]$Path)

        if ($entries -notcontains $Path) {
            throw "Package '$resolvedPackagePath' is missing '$Path'."
        }
    }

    function Get-PublicTopLevelTypeNames {
        param([System.IO.Compression.ZipArchiveEntry]$Entry)

        $entryStream = $Entry.Open()
        try {
            # ZipArchiveEntry streams are readable but not seekable. PEReader
            # requires both, so inspect a buffered copy of the package entry.
            $assemblyStream = [System.IO.MemoryStream]::new()
            try {
                $entryStream.CopyTo($assemblyStream)
                $assemblyStream.Position = 0

                $peReader = [System.Reflection.PortableExecutable.PEReader]::new($assemblyStream)
                try {
                    $metadataReader =
                        [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
                    $typeNames = @()

                    foreach ($handle in $metadataReader.TypeDefinitions) {
                        $definition = $metadataReader.GetTypeDefinition($handle)
                        $visibility =
                            $definition.Attributes -band [System.Reflection.TypeAttributes]::VisibilityMask
                        if ($visibility -ne [System.Reflection.TypeAttributes]::Public) {
                            continue
                        }

                        $namespace = $metadataReader.GetString($definition.Namespace)
                        $name = $metadataReader.GetString($definition.Name)
                        $fullName = if ($namespace) { "$namespace.$name" } else { $name }
                        $typeNames += $fullName
                    }

                    return @($typeNames | Sort-Object -Unique)
                }
                finally {
                    $peReader.Dispose()
                }
            }
            finally {
                $assemblyStream.Dispose()
            }
        }
        finally {
            $entryStream.Dispose()
        }
    }

    function Assert-PublicAssemblySurface {
        param(
            [string]$TargetFramework,
            [string]$AssemblyName
        )

        $entryPath = "lib/$TargetFramework/$AssemblyName.dll"
        $entry = $zip.Entries |
            Where-Object { $_.FullName -eq $entryPath } |
            Select-Object -First 1
        if ($null -eq $entry) {
            throw "Package '$resolvedPackagePath' is missing '$entryPath'."
        }

        $publicTypeNames = @(Get-PublicTopLevelTypeNames $entry)
        $unexpectedTypes = @(
            $publicTypeNames |
                Where-Object {
                    -not $_.StartsWith("ModernWpf.", [System.StringComparison]::Ordinal) -and
                    $_ -ne "XamlGeneratedNamespace.GeneratedInternalTypeHelper"
                }
        )

        if ($unexpectedTypes.Count -ne 0) {
            throw "Assembly '$entryPath' exports unsupported top-level types: $($unexpectedTypes -join ', ')"
        }

        $publicTypeSurfaces["$TargetFramework|$AssemblyName"] = @(
            $publicTypeNames |
                Where-Object { $_ -ne "XamlGeneratedNamespace.GeneratedInternalTypeHelper" }
        )
    }

    function Assert-PublicXmlDocumentation {
        param(
            [string]$TargetFramework,
            [string]$AssemblyName
        )

        $entryPath = "lib/$TargetFramework/$AssemblyName.xml"
        $entry = $zip.Entries |
            Where-Object { $_.FullName -eq $entryPath } |
            Select-Object -First 1
        if ($null -eq $entry) {
            throw "Package '$resolvedPackagePath' is missing '$entryPath'."
        }

        $reader = [System.IO.StreamReader]::new($entry.Open())
        try {
            [xml]$documentation = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }

        $publicTypeNames = @($publicTypeSurfaces["$TargetFramework|$AssemblyName"])
        $staleMemberIds = @()
        foreach ($member in @($documentation.SelectNodes("/doc/members/member"))) {
            $memberId = $member.GetAttribute("name")
            if ($memberId.Length -lt 3 -or $memberId[1] -ne ":") {
                $staleMemberIds += $memberId
                continue
            }

            $idBody = $memberId.Substring(2)
            $hasPublicContainingType = $false
            foreach ($typeName in $publicTypeNames) {
                if ($idBody -eq $typeName -or
                    $idBody.StartsWith("$typeName.", [System.StringComparison]::Ordinal)) {
                    $hasPublicContainingType = $true
                    break
                }
            }

            if (-not $hasPublicContainingType) {
                $staleMemberIds += $memberId
            }
        }

        if ($staleMemberIds.Count -ne 0) {
            $sample = @($staleMemberIds | Select-Object -First 10) -join ", "
            throw "Documentation '$entryPath' describes non-public types: $sample"
        }
    }

    Assert-PackageEntry "ModernWpfUI.nuspec"
    Assert-PackageEntry "readme.md"

    foreach ($targetFramework in $TargetFrameworks) {
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.dll"
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.xml"
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.Controls.dll"
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.Controls.xml"
        Assert-PublicAssemblySurface $targetFramework "ModernWpf"
        Assert-PublicAssemblySurface $targetFramework "ModernWpf.Controls"
        Assert-PublicXmlDocumentation $targetFramework "ModernWpf"
        Assert-PublicXmlDocumentation $targetFramework "ModernWpf.Controls"
    }

    foreach ($assemblyName in @("ModernWpf", "ModernWpf.Controls")) {
        $referenceFramework = $TargetFrameworks[0]
        $referenceSurface = @($publicTypeSurfaces["$referenceFramework|$assemblyName"])

        foreach ($targetFramework in $TargetFrameworks | Select-Object -Skip 1) {
            $candidateSurface = @($publicTypeSurfaces["$targetFramework|$assemblyName"])
            $differences = @(
                Compare-Object `
                    -ReferenceObject $referenceSurface `
                    -DifferenceObject $candidateSurface
            )

            if ($differences.Count -ne 0) {
                $differenceText = @(
                    $differences |
                        ForEach-Object { "$($_.SideIndicator) $($_.InputObject)" }
                ) -join ", "
                throw "Public top-level types differ between '$referenceFramework' and '$targetFramework' in '$assemblyName.dll': $differenceText"
            }
        }
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

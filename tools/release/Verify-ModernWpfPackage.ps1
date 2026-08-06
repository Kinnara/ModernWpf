param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [string]$SymbolPackagePath,

    [string]$ExpectedRepositoryCommit,

    [string[]]$TargetFrameworks = @(
        "net462",
        "net8.0-windows7.0",
        "net10.0-windows7.0"
    )
)

$ErrorActionPreference = "Stop"

if (-not [string]::IsNullOrWhiteSpace($ExpectedRepositoryCommit) -and
    $ExpectedRepositoryCommit -notmatch "^[0-9a-fA-F]{40}$") {
    throw "Expected repository commit must be a full Git SHA: '$ExpectedRepositoryCommit'."
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Reflection.Metadata

$centralPropsPath = Join-Path $PSScriptRoot "..\..\Directory.Build.props"
[xml]$centralProps = Get-Content -LiteralPath $centralPropsPath -Raw
$systemValueTupleVersion = $centralProps.SelectSingleNode(
    "/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='ModernWpfSystemValueTupleVersion']").InnerText
$windowsSdkContractsVersion = $centralProps.SelectSingleNode(
    "/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='ModernWpfWindowsSdkContractsVersion']").InnerText

if ([string]::IsNullOrWhiteSpace($systemValueTupleVersion) -or
    [string]::IsNullOrWhiteSpace($windowsSdkContractsVersion)) {
    throw "Directory.Build.props must define the package dependency versions."
}

$resolvedPackagePath = (Resolve-Path $PackagePath).Path
$symbolPackageCandidate = if ($SymbolPackagePath) {
    $SymbolPackagePath
}
else {
    [System.IO.Path]::ChangeExtension($resolvedPackagePath, ".snupkg")
}
$resolvedSymbolPackagePath = (Resolve-Path $symbolPackageCandidate).Path
$zip = [System.IO.Compression.ZipFile]::OpenRead($resolvedPackagePath)
$symbolZip = [System.IO.Compression.ZipFile]::OpenRead($resolvedSymbolPackagePath)

try {
    $entries = @($zip.Entries | ForEach-Object { $_.FullName })
    $symbolEntries = @($symbolZip.Entries | ForEach-Object { $_.FullName })
    $publicTypeSurfaces = @{}

    function Assert-PackageEntry {
        param([string]$Path)

        if ($entries -notcontains $Path) {
            throw "Package '$resolvedPackagePath' is missing '$Path'."
        }
    }

    function Get-PackageEntryText {
        param([string]$Path)

        $entry = $zip.Entries |
            Where-Object { $_.FullName -eq $Path } |
            Select-Object -First 1
        if ($null -eq $entry) {
            throw "Package '$resolvedPackagePath' is missing '$Path'."
        }

        $reader = [System.IO.StreamReader]::new($entry.Open())
        try {
            return $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }
    }

    function Assert-PortablePdb {
        param([string]$Path)

        $entry = $symbolZip.Entries |
            Where-Object { $_.FullName -eq $Path } |
            Select-Object -First 1
        if ($null -eq $entry) {
            throw "Symbol package '$resolvedSymbolPackagePath' is missing '$Path'."
        }

        $entryStream = $entry.Open()
        $pdbStream = [System.IO.MemoryStream]::new()
        try {
            $entryStream.CopyTo($pdbStream)
            $entryStream.Dispose()
            $entryStream = $null

            $bytes = $pdbStream.ToArray()
            if ($bytes.Length -lt 4 -or
                $bytes[0] -ne 0x42 -or
                $bytes[1] -ne 0x53 -or
                $bytes[2] -ne 0x4A -or
                $bytes[3] -ne 0x42) {
                throw "Symbol '$Path' is not a portable PDB."
            }

            $pdbStream.Position = 0
            $provider =
                [System.Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream($pdbStream)
            try {
                $metadataReader = $provider.GetMetadataReader()
                $sourceLinkKind =
                    [Guid]::Parse("CC110556-A091-4D38-9FEC-25AB9A351A6A")
                $sourceLinkJson = $null

                foreach ($handle in $metadataReader.CustomDebugInformation) {
                    $information = $metadataReader.GetCustomDebugInformation($handle)
                    if ($metadataReader.GetGuid($information.Kind) -eq $sourceLinkKind) {
                        $sourceLinkJson = [Text.Encoding]::UTF8.GetString(
                            $metadataReader.GetBlobBytes($information.Value))
                        break
                    }
                }

                if ([string]::IsNullOrWhiteSpace($sourceLinkJson)) {
                    throw "Portable PDB '$Path' has no SourceLink record."
                }

                $sourceLink = $sourceLinkJson | ConvertFrom-Json
                if ($null -eq $sourceLink.documents -or
                    @($sourceLink.documents.PSObject.Properties).Count -eq 0) {
                    throw "Portable PDB '$Path' has an empty SourceLink document map."
                }

                if (-not [string]::IsNullOrWhiteSpace($ExpectedRepositoryCommit)) {
                    $mismatchedDocuments = @(
                        $sourceLink.documents.PSObject.Properties |
                            Where-Object {
                                ([string]$_.Value).IndexOf(
                                    $ExpectedRepositoryCommit,
                                    [StringComparison]::OrdinalIgnoreCase) -lt 0
                            } |
                            ForEach-Object { $_.Name }
                    )
                    if ($mismatchedDocuments.Count -ne 0) {
                        throw "Portable PDB '$Path' SourceLink does not identify expected commit '$ExpectedRepositoryCommit'."
                    }
                }
            }
            finally {
                $provider.Dispose()
            }
        }
        finally {
            if ($null -ne $entryStream) {
                $entryStream.Dispose()
            }
            $pdbStream.Dispose()
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
    Assert-PackageEntry "icon.png"

    foreach ($targetFramework in $TargetFrameworks) {
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.dll"
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.xml"
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.Controls.dll"
        Assert-PackageEntry "lib/$targetFramework/ModernWpf.Controls.xml"
        Assert-PublicAssemblySurface $targetFramework "ModernWpf"
        Assert-PublicAssemblySurface $targetFramework "ModernWpf.Controls"
        Assert-PublicXmlDocumentation $targetFramework "ModernWpf"
        Assert-PublicXmlDocumentation $targetFramework "ModernWpf.Controls"
        Assert-PortablePdb "lib/$targetFramework/ModernWpf.pdb"
        Assert-PortablePdb "lib/$targetFramework/ModernWpf.Controls.pdb"
    }

    $pdbEntriesInPackage = @($entries | Where-Object { $_ -match "\.pdb$" })
    if ($pdbEntriesInPackage.Count -ne 0) {
        throw "Main package '$resolvedPackagePath' must not contain PDB files: $($pdbEntriesInPackage -join ', ')"
    }

    $unexpectedSymbolEntries = @(
        $symbolEntries |
            Where-Object {
                $_ -notmatch "^lib/[^/]+/ModernWpf(\.Controls)?\.pdb$" -and
                $_ -notmatch "(^|/)ModernWpfUI\.nuspec$" -and
                $_ -notmatch "(^|/)(_rels|package)/" -and
                $_ -ne "[Content_Types].xml"
            }
    )
    if ($unexpectedSymbolEntries.Count -ne 0) {
        throw "Symbol package '$resolvedSymbolPackagePath' contains unexpected entries: $($unexpectedSymbolEntries -join ', ')"
    }

    $symbolNuspecEntry = $symbolZip.Entries |
        Where-Object { $_.FullName -match "(^|/)ModernWpfUI\.nuspec$" } |
        Select-Object -First 1
    if ($null -eq $symbolNuspecEntry) {
        throw "Symbol package '$resolvedSymbolPackagePath' has no nuspec entry."
    }

    $symbolNuspecReader = [System.IO.StreamReader]::new($symbolNuspecEntry.Open())
    try {
        [xml]$symbolNuspec = $symbolNuspecReader.ReadToEnd()
    }
    finally {
        $symbolNuspecReader.Dispose()
    }

    $symbolRepository = $symbolNuspec.SelectSingleNode(
        "/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='repository']")
    if ($null -eq $symbolRepository) {
        throw "Symbol package '$resolvedSymbolPackagePath' has no repository metadata."
    }

    $symbolRepositoryCommit = $symbolRepository.GetAttribute("commit")
    if ($symbolRepositoryCommit -notmatch "^[0-9a-fA-F]{40}$") {
        throw "Symbol package '$resolvedSymbolPackagePath' repository commit must be a full Git SHA."
    }
    if (-not [string]::IsNullOrWhiteSpace($ExpectedRepositoryCommit) -and
        -not $symbolRepositoryCommit.Equals(
            $ExpectedRepositoryCommit,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "Symbol package repository commit '$symbolRepositoryCommit' does not match checked-out commit '$ExpectedRepositoryCommit'."
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

    $version = $metadata.SelectSingleNode("*[local-name()='version']").InnerText
    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "Package '$resolvedPackagePath' has no version metadata."
    }

    $title = $metadata.SelectSingleNode("*[local-name()='title']").InnerText
    if ($title -ne "ModernWPF") {
        throw "Package '$resolvedPackagePath' must use the ModernWPF display name."
    }

    $icon = $metadata.SelectSingleNode("*[local-name()='icon']")
    if ($null -eq $icon -or $icon.InnerText -ne "icon.png") {
        throw "Package '$resolvedPackagePath' must declare icon.png in nuspec metadata."
    }

    $expectedDescription =
        "Fluent styles and WinUI-inspired controls for WPF, supporting .NET Framework 4.6.2, .NET 8, and .NET 10."
    $description = $metadata.SelectSingleNode("*[local-name()='description']").InnerText
    if ($description -ne $expectedDescription) {
        throw "Package '$resolvedPackagePath' has stale or unexpected description metadata."
    }

    $expectedReleaseNotes =
        "https://github.com/Kinnara/ModernWpf/blob/v$version/docs/release-notes-$version.md"
    $releaseNotes = $metadata.SelectSingleNode("*[local-name()='releaseNotes']").InnerText
    if ($releaseNotes -ne $expectedReleaseNotes) {
        throw "Package '$resolvedPackagePath' release notes must be pinned to its version tag."
    }

    $expectedTags = "WPF XAML Fluent WinUI Windows Desktop Theme Controls ModernWPF"
    $tags = $metadata.SelectSingleNode("*[local-name()='tags']").InnerText
    if ($tags -ne $expectedTags) {
        throw "Package '$resolvedPackagePath' has stale or unexpected tags."
    }

    $projectUrl = $metadata.SelectSingleNode("*[local-name()='projectUrl']").InnerText
    if ($projectUrl -ne "https://github.com/Kinnara/ModernWpf") {
        throw "Package '$resolvedPackagePath' has an unexpected project URL."
    }

    $iconEntry = $zip.Entries |
        Where-Object { $_.FullName -eq "icon.png" } |
        Select-Object -First 1
    $iconStream = $iconEntry.Open()
    $iconBuffer = [System.IO.MemoryStream]::new()
    try {
        $iconStream.CopyTo($iconBuffer)
        [byte[]]$iconBytes = $iconBuffer.ToArray()
    }
    finally {
        $iconStream.Dispose()
        $iconBuffer.Dispose()
    }

    [byte[]]$pngSignature = 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
    if ($iconBytes.Length -lt 24) {
        throw "Package icon.png is truncated."
    }

    for ($index = 0; $index -lt $pngSignature.Length; $index++) {
        if ($iconBytes[$index] -ne $pngSignature[$index]) {
            throw "Package icon.png is not a PNG image."
        }
    }

    $iconWidth =
        ($iconBytes[16] -shl 24) -bor
        ($iconBytes[17] -shl 16) -bor
        ($iconBytes[18] -shl 8) -bor
        $iconBytes[19]
    $iconHeight =
        ($iconBytes[20] -shl 24) -bor
        ($iconBytes[21] -shl 16) -bor
        ($iconBytes[22] -shl 8) -bor
        $iconBytes[23]
    if ($iconWidth -ne 128 -or $iconHeight -ne 128) {
        throw "Package icon.png must be exactly 128x128; found ${iconWidth}x${iconHeight}."
    }

    $packageReadme = Get-PackageEntryText "readme.md"
    $expectedReadmeFragments = @(
        "https://raw.githubusercontent.com/Kinnara/ModernWpf/v$version/docs/images/Gallery.Light.png",
        "dotnet add package ModernWpfUI --version $version",
        '| `net462` |',
        '| `net8.0-windows7.0` |',
        '| `net10.0-windows7.0` |',
        '<ui:ThemeResources />',
        '<ui:FluentControlsResources UseCompactResources="False" />',
        '<ui:XamlControlsResources />',
        $expectedReleaseNotes,
        "https://github.com/Kinnara/ModernWpf/blob/v$version/docs/migrating-from-0.9.md",
        "https://github.com/Kinnara/ModernWpf/issues/new?template=preview-bug.yml",
        "https://github.com/Kinnara/ModernWpf#documentation",
        "frozen and unsupported"
    )
    foreach ($fragment in $expectedReadmeFragments) {
        if (-not $packageReadme.Contains($fragment, [System.StringComparison]::Ordinal)) {
            throw "Package readme.md is missing required content: $fragment"
        }
    }

    $repository = $metadata.SelectSingleNode("*[local-name()='repository']")
    if ($null -eq $repository) {
        throw "Package '$resolvedPackagePath' has no repository metadata."
    }

    if ($repository.GetAttribute("type") -ne "git") {
        throw "Package '$resolvedPackagePath' repository type must be 'git'."
    }

    if ($repository.GetAttribute("url") -ne "https://github.com/Kinnara/ModernWpf") {
        throw "Package '$resolvedPackagePath' has an unexpected repository URL."
    }

    $repositoryCommit = $repository.GetAttribute("commit")
    if ($repositoryCommit -notmatch "^[0-9a-fA-F]{40}$") {
        throw "Package '$resolvedPackagePath' repository commit must be a full Git SHA."
    }
    if (-not [string]::IsNullOrWhiteSpace($ExpectedRepositoryCommit)) {
        if (-not $repositoryCommit.Equals(
            $ExpectedRepositoryCommit,
            [StringComparison]::OrdinalIgnoreCase)) {
            throw "Package repository commit '$repositoryCommit' does not match checked-out commit '$ExpectedRepositoryCommit'."
        }
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

    $net462DependencyGroup = $metadata.SelectSingleNode(
        "*[local-name()='dependencies']/*[local-name()='group'][@targetFramework='.NETFramework4.6.2']")
    if ($null -eq $net462DependencyGroup) {
        throw "Package '$resolvedPackagePath' has no net462 dependency group."
    }

    $expectedNet462Dependencies = @{
        "System.ValueTuple" = $systemValueTupleVersion
        "Microsoft.Windows.SDK.Contracts" = $windowsSdkContractsVersion
    }
    $net462Dependencies = @($net462DependencyGroup.SelectNodes("*[local-name()='dependency']"))

    if ($net462Dependencies.Count -ne $expectedNet462Dependencies.Count) {
        throw "Package '$resolvedPackagePath' must declare exactly the centrally versioned net462 dependencies."
    }

    foreach ($dependency in $net462Dependencies) {
        $dependencyId = $dependency.GetAttribute("id")
        if (-not $expectedNet462Dependencies.ContainsKey($dependencyId)) {
            throw "Package '$resolvedPackagePath' has unexpected net462 dependency '$dependencyId'."
        }

        $expectedVersion = $expectedNet462Dependencies[$dependencyId]
        $actualVersion = $dependency.GetAttribute("version")
        if ($actualVersion -ne $expectedVersion) {
            throw "Package '$resolvedPackagePath' dependency '$dependencyId' uses '$actualVersion'; expected '$expectedVersion'."
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
    $symbolZip.Dispose()
}

Write-Host "Verified ModernWpfUI packages: $resolvedPackagePath and $resolvedSymbolPackagePath"

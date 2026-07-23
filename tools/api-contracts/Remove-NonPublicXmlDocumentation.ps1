[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$OutputRoot
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Reflection.Metadata

function Get-DocumentedTypeContracts {
    param([Parameter(Mandatory = $true)][string]$AssemblyPath)

    $assemblyStream = [System.IO.File]::OpenRead($AssemblyPath)
    try {
        $peReader = [System.Reflection.PortableExecutable.PEReader]::new($assemblyStream)
        try {
            $metadataReader =
                [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
            $definitions = @{}

            foreach ($handle in $metadataReader.TypeDefinitions) {
                $row = [System.Reflection.Metadata.Ecma335.MetadataTokens]::GetRowNumber($handle)
                $definition = $metadataReader.GetTypeDefinition($handle)
                $declaringHandle = $definition.GetDeclaringType()
                $declaringRow = if ($declaringHandle.IsNil) {
                    0
                }
                else {
                    [System.Reflection.Metadata.Ecma335.MetadataTokens]::GetRowNumber(
                        $declaringHandle
                    )
                }

                $definitions[$row] = @{
                    Attributes = $definition.Attributes
                    DeclaringRow = $declaringRow
                    Name = $metadataReader.GetString($definition.Name)
                    Namespace = $metadataReader.GetString($definition.Namespace)
                }
            }

            $contracts = @{}

            function Resolve-TypeContract {
                param([int]$Row)

                if ($contracts.ContainsKey($Row)) {
                    return $contracts[$Row]
                }

                $definition = $definitions[$Row]
                $visibility =
                    $definition.Attributes -band [System.Reflection.TypeAttributes]::VisibilityMask

                if ($definition.DeclaringRow -eq 0) {
                    $name = if ($definition.Namespace) {
                        "$($definition.Namespace).$($definition.Name)"
                    }
                    else {
                        $definition.Name
                    }
                    $isPublic = $visibility -eq [System.Reflection.TypeAttributes]::Public
                    if ($name -eq "XamlGeneratedNamespace.GeneratedInternalTypeHelper") {
                        # WPF emits this compiler helper as public, but it is not
                        # supported application API and is excluded by the
                        # package contract verifier.
                        $isPublic = $false
                    }
                }
                else {
                    $declaringContract = Resolve-TypeContract $definition.DeclaringRow
                    $name = "$($declaringContract.Name).$($definition.Name)"
                    $externallyVisibleNestedType = $visibility -in @(
                        [System.Reflection.TypeAttributes]::NestedPublic,
                        [System.Reflection.TypeAttributes]::NestedFamily,
                        [System.Reflection.TypeAttributes]::NestedFamORAssem
                    )
                    $isPublic =
                        $declaringContract.IsPublic -and $externallyVisibleNestedType
                }

                $contract = [pscustomobject]@{
                    Name = $name
                    IsPublic = $isPublic
                }
                $contracts[$Row] = $contract
                return $contract
            }

            foreach ($row in @($definitions.Keys)) {
                [void](Resolve-TypeContract $row)
            }

            return @($contracts.Values | Sort-Object { $_.Name.Length } -Descending)
        }
        finally {
            $peReader.Dispose()
        }
    }
    finally {
        $assemblyStream.Dispose()
    }
}

function Remove-NonPublicDocumentation {
    param(
        [Parameter(Mandatory = $true)][string]$AssemblyPath,
        [Parameter(Mandatory = $true)][string]$DocumentationPath
    )

    $typeContracts = @(Get-DocumentedTypeContracts $AssemblyPath)
    $documentation = [System.Xml.XmlDocument]::new()
    $documentation.PreserveWhitespace = $true
    $documentation.Load($DocumentationPath)

    $removed = 0
    foreach ($member in @($documentation.SelectNodes("/doc/members/member"))) {
        $memberId = $member.GetAttribute("name")
        if ($memberId.Length -lt 3 -or $memberId[1] -ne ":") {
            [void]$member.ParentNode.RemoveChild($member)
            $removed++
            continue
        }

        $idBody = $memberId.Substring(2)
        $containingType = $typeContracts |
            Where-Object {
                $idBody -eq $_.Name -or
                $idBody.StartsWith("$($_.Name).", [System.StringComparison]::Ordinal)
            } |
            Select-Object -First 1

        if ($null -eq $containingType -or -not $containingType.IsPublic) {
            [void]$member.ParentNode.RemoveChild($member)
            $removed++
        }
    }

    if ($removed -ne 0) {
        $documentation.Save($DocumentationPath)
    }

    Write-Host "Filtered $removed non-public XML documentation entries from '$DocumentationPath'."
}

$resolvedOutputRoot = (Resolve-Path $OutputRoot).Path
$assemblyPaths = @(
    Get-ChildItem -LiteralPath $resolvedOutputRoot -Recurse -File |
        Where-Object {
            $_.Name -in @("ModernWpf.dll", "ModernWpf.Controls.dll") -and
            (Test-Path -LiteralPath ([System.IO.Path]::ChangeExtension($_.FullName, ".xml")))
        } |
        Select-Object -ExpandProperty FullName -Unique
)

if ($assemblyPaths.Count -eq 0) {
    throw "No ModernWpf assembly/XML documentation pairs were found under '$resolvedOutputRoot'."
}

foreach ($assemblyPath in $assemblyPaths) {
    $documentationPath = [System.IO.Path]::ChangeExtension($assemblyPath, ".xml")
    Remove-NonPublicDocumentation $assemblyPath $documentationPath
}

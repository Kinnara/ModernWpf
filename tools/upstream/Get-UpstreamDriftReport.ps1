[CmdletBinding()]
param(
    [Parameter()]
    [string]$ManifestPath = (Join-Path $PSScriptRoot 'upstream-sync.json'),

    [Parameter()]
    [string]$OutputPath,

    [Parameter()]
    [string]$JsonOutputPath,

    [Parameter()]
    [string]$FixturePath,

    [Parameter()]
    [string]$GitHubToken = $env:GITHUB_TOKEN,

    [Parameter()]
    [string]$GeneratedAt,

    [Parameter()]
    [switch]$IncludeEpochComparison,

    [Parameter()]
    [switch]$FailOnEpochDrift,

    [Parameter()]
    [switch]$FailOnObservedDrift,

    [Parameter()]
    [switch]$FailOnIncompleteComparison
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-ObjectProperty {
    param(
        [Parameter(Mandatory)]
        [object]$InputObject,

        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter()]
        [object]$DefaultValue = $null
    )

    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $DefaultValue
    }

    return $property.Value
}

function ConvertTo-NormalizedPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return $Path.Replace('\', '/')
}

function Assert-Manifest {
    param(
        [Parameter(Mandatory)]
        [object]$Manifest,

        [Parameter(Mandatory)]
        [string]$RepositoryRoot
    )

    if ($Manifest.schemaVersion -ne 1) {
        throw "Unsupported upstream manifest schema version '$($Manifest.schemaVersion)'."
    }

    $repositoryIds = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)

    foreach ($repository in @($Manifest.repositories)) {
        if (-not $repositoryIds.Add([string]$repository.id)) {
            throw "Duplicate upstream repository id '$($repository.id)'."
        }

        foreach ($ignorePath in @($repository.ignorePaths)) {
            if ([string]::IsNullOrWhiteSpace([string]$ignorePath.path) -or
                ([string]$ignorePath.path).Contains('\', [StringComparison]::Ordinal) -or
                [string]::IsNullOrWhiteSpace([string]$ignorePath.justification)) {
                throw "Repository '$($repository.id)' has an invalid explicitly ignored path."
            }
        }

        $trackIds = [System.Collections.Generic.HashSet[string]]::new(
            [System.StringComparer]::Ordinal)

        foreach ($track in @($repository.tracks)) {
            if (-not $trackIds.Add([string]$track.id)) {
                throw "Duplicate track '$($track.id)' in repository '$($repository.id)'."
            }

            foreach ($revisionName in @('reviewedBaseline', 'epochTarget')) {
                $revision = Get-ObjectProperty -InputObject $track -Name $revisionName
                if ($null -eq $revision -or
                    [string]$revision.revision -notmatch '^[0-9a-f]{40}$') {
                    throw "Track '$($repository.id)/$($track.id)' has an invalid $revisionName revision."
                }
            }

            $epochAdoption = Get-ObjectProperty `
                -InputObject $track `
                -Name 'epochAdoption'
            if ($null -eq $epochAdoption -or
                [string]$epochAdoption.status -ne 'adopted' -or
                [string]::IsNullOrWhiteSpace(
                    [string]$epochAdoption.dispositionDocument)) {
                throw "Track '$($repository.id)/$($track.id)' must have an adopted epoch with a disposition document."
            }

            $dispositionPath = Join-Path $RepositoryRoot (
                ([string]$epochAdoption.dispositionDocument).Replace(
                    '/',
                    [IO.Path]::DirectorySeparatorChar))
            if (-not (Test-Path -LiteralPath $dispositionPath -PathType Leaf)) {
                throw "Track '$($repository.id)/$($track.id)' references missing epoch disposition '$($epochAdoption.dispositionDocument)'."
            }

            $observedKind = [string]$track.observedHead.kind
            if ($observedKind -notin @('ref', 'latestStableRelease')) {
                throw "Track '$($repository.id)/$($track.id)' has unsupported observed-head kind '$observedKind'."
            }

            if ($track.channel -eq 'stable') {
                $latestStable = Get-ObjectProperty -InputObject $track -Name 'latestStableAtEpoch'
                if ($null -eq $latestStable -or
                    [string]$latestStable.revision -notmatch '^[0-9a-f]{40}$') {
                    throw "Stable track '$($repository.id)/$($track.id)' must pin latestStableAtEpoch."
                }

                if ($observedKind -ne 'latestStableRelease') {
                    throw "Stable track '$($repository.id)/$($track.id)' must use a latestStableRelease selector."
                }
            }
        }
    }

    $familyIds = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)

    foreach ($family in @($Manifest.families)) {
        if (-not $familyIds.Add([string]$family.id)) {
            throw "Duplicate upstream family id '$($family.id)'."
        }

        if (@($family.auditDocuments).Count -eq 0) {
            throw "Upstream family '$($family.id)' has no audit document."
        }

        foreach ($auditDocument in @($family.auditDocuments)) {
            $auditPath = Join-Path $RepositoryRoot (
                ([string]$auditDocument).Replace('/', [IO.Path]::DirectorySeparatorChar))
            if (-not (Test-Path -LiteralPath $auditPath -PathType Leaf)) {
                throw "Upstream family '$($family.id)' references missing audit '$auditDocument'."
            }
        }

        foreach ($watch in @($family.watches)) {
            if (-not $repositoryIds.Contains([string]$watch.repository)) {
                throw "Upstream family '$($family.id)' references unknown repository '$($watch.repository)'."
            }

            foreach ($path in @($watch.paths)) {
                if ([string]::IsNullOrWhiteSpace([string]$path) -or
                    ([string]$path).Contains('\', [StringComparison]::Ordinal)) {
                    throw "Upstream family '$($family.id)' has invalid watched path '$path'."
                }
            }
        }
    }
}

function Invoke-GitHubApi {
    param(
        [Parameter(Mandatory)]
        [string]$Uri,

        [Parameter()]
        [string]$Token
    )

    $headers = @{
        Accept = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
        'User-Agent' = 'ModernWpf-Upstream-Drift'
    }

    if (-not [string]::IsNullOrWhiteSpace($Token)) {
        $headers.Authorization = "Bearer $Token"
    }

    return Invoke-RestMethod -Method Get -Uri $Uri -Headers $headers
}

function Get-FixtureObservedHead {
    param(
        [Parameter(Mandatory)]
        [object]$Fixture,

        [Parameter(Mandatory)]
        [string]$RepositoryId,

        [Parameter(Mandatory)]
        [string]$TrackId
    )

    $matches = @(
        $Fixture.observedHeads |
            Where-Object {
                $_.repository -eq $RepositoryId -and
                $_.track -eq $TrackId
            })

    if ($matches.Count -ne 1) {
        throw "Fixture must contain exactly one observed head for '$RepositoryId/$TrackId'; found $($matches.Count)."
    }

    return $matches[0]
}

function Get-StableTagVersion {
    param(
        [Parameter(Mandatory)]
        [object]$Tag
    )

    $tagName = [string]$Tag.name
    $match = [Regex]::Match(
        $tagName,
        '(?<version>[0-9]+\.[0-9]+\.[0-9]+)$',
        [Text.RegularExpressions.RegexOptions]::CultureInvariant)
    if (-not $match.Success) {
        throw "Stable release tag '$tagName' does not end with a three-part semantic version."
    }

    return [Version]$match.Groups['version'].Value
}

function Select-LatestStableTag {
    param(
        [Parameter(Mandatory)]
        [object[]]$Tags,

        [Parameter(Mandatory)]
        [string]$TagPattern
    )

    $selection = $Tags |
        Where-Object {
            [string]$_.name -match $TagPattern
        } |
        ForEach-Object {
            [pscustomobject]@{
                tag = $_
                version = Get-StableTagVersion -Tag $_
            }
        } |
        Sort-Object -Property version -Descending |
        Select-Object -First 1

    if ($null -eq $selection) {
        throw "No stable release matched '$TagPattern'."
    }

    return $selection.tag
}

function ConvertTo-StableTags {
    param(
        [Parameter(Mandatory)]
        [object[]]$Pages
    )

    return @(
        foreach ($page in $Pages) {
            foreach ($tag in @($page.tags)) {
                [pscustomobject]@{
                    name = [string]$tag.name
                    revision = [string]$tag.commit.sha
                }
            }
        })
}

function Get-ObservedHead {
    param(
        [Parameter(Mandatory)]
        [object]$Repository,

        [Parameter(Mandatory)]
        [object]$Track,

        [Parameter()]
        [object]$Fixture,

        [Parameter()]
        [string]$Token
    )

    $fixtureStableTags = if ($null -ne $Fixture) {
        Get-ObjectProperty `
            -InputObject $Fixture `
            -Name 'stableTags' `
            -DefaultValue @()
    }
    else {
        @()
    }
    $fixtureStableTagPages = if ($null -ne $Fixture) {
        Get-ObjectProperty `
            -InputObject $Fixture `
            -Name 'stableTagPages' `
            -DefaultValue @()
    }
    else {
        @()
    }
    if ($null -ne $Fixture -and
        $Track.observedHead.kind -eq 'latestStableRelease' -and
        @($fixtureStableTagPages).Count -gt 0) {
        $fixturePages = @(
            $fixtureStableTagPages |
                Where-Object {
                    $_.repository -eq $Repository.id -and
                    $_.track -eq $Track.id
                })
        $fixtureTags = ConvertTo-StableTags -Pages $fixturePages
        $tag = Select-LatestStableTag `
            -Tags $fixtureTags `
            -TagPattern ([string]$Track.observedHead.tagPattern)
        return [pscustomobject][ordered]@{
            repository = [string]$Repository.id
            track = [string]$Track.id
            revision = [string]$tag.revision
            label = [string]$tag.name
        }
    }

    if ($null -ne $Fixture -and
        $Track.observedHead.kind -eq 'latestStableRelease' -and
        @($fixtureStableTags).Count -gt 0) {
        $tag = Select-LatestStableTag `
            -Tags @(
                $fixtureStableTags |
                    Where-Object {
                        $_.repository -eq $Repository.id -and
                        $_.track -eq $Track.id
                    }) `
            -TagPattern ([string]$Track.observedHead.tagPattern)
        return [pscustomobject][ordered]@{
            repository = [string]$Repository.id
            track = [string]$Track.id
            revision = [string]$tag.revision
            label = [string]$tag.name
        }
    }

    if ($null -ne $Fixture) {
        return Get-FixtureObservedHead `
            -Fixture $Fixture `
            -RepositoryId $Repository.id `
            -TrackId $Track.id
    }

    $repositoryUri = "https://api.github.com/repos/$($Repository.owner)/$($Repository.name)"
    if ($Track.observedHead.kind -eq 'ref') {
        $ref = [string]$Track.observedHead.ref
        $encodedRef = [Uri]::EscapeDataString($ref)
        $commit = Invoke-GitHubApi `
            -Uri "$repositoryUri/commits/$encodedRef" `
            -Token $Token

        return [pscustomobject][ordered]@{
            repository = [string]$Repository.id
            track = [string]$Track.id
            revision = [string]$commit.sha
            label = $ref
        }
    }

    $tagPattern = [string]$Track.observedHead.tagPattern
    $tagPages = [Collections.Generic.List[object]]::new()
    $pageNumber = 1
    do {
        # Invoke-RestMethod returns a single Object[] pipeline item for JSON
        # arrays. Assign first so @() enumerates the response instead of nesting
        # the complete response as one tag.
        $tagResponse = Invoke-GitHubApi `
            -Uri "$repositoryUri/tags?per_page=100&page=$pageNumber" `
            -Token $Token
        $pageTags = @($tagResponse)
        $tagPages.Add([pscustomobject]@{ tags = $pageTags })
        $pageNumber++
    }
    while ($pageTags.Count -eq 100)

    $stableTags = ConvertTo-StableTags -Pages @($tagPages)
    $tag = Select-LatestStableTag `
        -Tags $stableTags `
        -TagPattern $tagPattern

    return [pscustomobject][ordered]@{
        repository = [string]$Repository.id
        track = [string]$Track.id
        revision = [string]$tag.revision
        label = [string]$tag.name
    }
}

function ConvertTo-Comparison {
    param(
        [Parameter(Mandatory)]
        [object]$Comparison
    )

    $files = @(
        foreach ($file in @(Get-ObjectProperty -InputObject $Comparison -Name 'files' -DefaultValue @())) {
            $previousFilename = [string](Get-ObjectProperty `
                -InputObject $file `
                -Name 'previous_filename' `
                -DefaultValue '')
            [pscustomobject][ordered]@{
                filename = ConvertTo-NormalizedPath -Path ([string]$file.filename)
                previousFilename = if ([string]::IsNullOrWhiteSpace($previousFilename)) {
                    ''
                }
                else {
                    ConvertTo-NormalizedPath -Path $previousFilename
                }
                status = [string](Get-ObjectProperty `
                    -InputObject $file `
                    -Name 'status' `
                    -DefaultValue 'modified')
            }
        })

    $isCompleteProperty = Get-ObjectProperty `
        -InputObject $Comparison `
        -Name 'isComplete'
    $isComplete = if ($null -ne $isCompleteProperty) {
        [bool]$isCompleteProperty
    }
    else {
        $files.Count -lt 300
    }

    return [pscustomobject][ordered]@{
        status = [string](Get-ObjectProperty `
            -InputObject $Comparison `
            -Name 'status' `
            -DefaultValue 'unknown')
        aheadBy = [int](Get-ObjectProperty `
            -InputObject $Comparison `
            -Name 'ahead_by' `
            -DefaultValue (
                Get-ObjectProperty `
                    -InputObject $Comparison `
                    -Name 'aheadBy' `
                    -DefaultValue 0))
        behindBy = [int](Get-ObjectProperty `
            -InputObject $Comparison `
            -Name 'behind_by' `
            -DefaultValue (
                Get-ObjectProperty `
                    -InputObject $Comparison `
                    -Name 'behindBy' `
                    -DefaultValue 0))
        totalCommits = [int](Get-ObjectProperty `
            -InputObject $Comparison `
            -Name 'total_commits' `
            -DefaultValue (
                Get-ObjectProperty `
                    -InputObject $Comparison `
                    -Name 'totalCommits' `
                    -DefaultValue 0))
        isComplete = $isComplete
        files = $files
    }
}

function Get-FixtureComparison {
    param(
        [Parameter(Mandatory)]
        [object]$Fixture,

        [Parameter(Mandatory)]
        [string]$RepositoryId,

        [Parameter(Mandatory)]
        [string]$BaseRevision,

        [Parameter(Mandatory)]
        [string]$HeadRevision
    )

    $matches = @(
        $Fixture.comparisons |
            Where-Object {
                $_.repository -eq $RepositoryId -and
                $_.base -eq $BaseRevision -and
                $_.head -eq $HeadRevision
            })

    if ($matches.Count -ne 1) {
        throw "Fixture must contain exactly one '$RepositoryId' comparison from '$BaseRevision' to '$HeadRevision'; found $($matches.Count)."
    }

    return ConvertTo-Comparison -Comparison $matches[0]
}

function Get-Comparison {
    param(
        [Parameter(Mandatory)]
        [object]$Repository,

        [Parameter(Mandatory)]
        [string]$BaseRevision,

        [Parameter(Mandatory)]
        [string]$HeadRevision,

        [Parameter()]
        [object]$Fixture,

        [Parameter()]
        [string]$Token
    )

    if ($BaseRevision -eq $HeadRevision) {
        return [pscustomobject][ordered]@{
            status = 'identical'
            aheadBy = 0
            behindBy = 0
            totalCommits = 0
            isComplete = $true
            files = @()
        }
    }

    if ($null -ne $Fixture) {
        return Get-FixtureComparison `
            -Fixture $Fixture `
            -RepositoryId $Repository.id `
            -BaseRevision $BaseRevision `
            -HeadRevision $HeadRevision
    }

    $encodedBase = [Uri]::EscapeDataString($BaseRevision)
    $encodedHead = [Uri]::EscapeDataString($HeadRevision)
    $uri = "https://api.github.com/repos/$($Repository.owner)/$($Repository.name)/compare/$encodedBase...${encodedHead}?per_page=100"
    $comparison = Invoke-GitHubApi -Uri $uri -Token $Token
    return ConvertTo-Comparison -Comparison $comparison
}

function Get-FilePathCandidates {
    param(
        [Parameter(Mandatory)]
        [object]$File
    )

    $candidates = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)

    foreach ($path in @([string]$File.filename, [string]$File.previousFilename)) {
        if ([string]::IsNullOrWhiteSpace($path)) {
            continue
        }

        $normalizedPath = ConvertTo-NormalizedPath -Path $path
        [void]$candidates.Add($normalizedPath)
        if ($normalizedPath.StartsWith('src/', [StringComparison]::OrdinalIgnoreCase)) {
            [void]$candidates.Add($normalizedPath.Substring('src/'.Length))
        }
    }

    return @($candidates)
}

function Test-FileMatchesPatterns {
    param(
        [Parameter(Mandatory)]
        [object]$File,

        [Parameter(Mandatory)]
        [string[]]$Patterns
    )

    foreach ($candidatePath in @(Get-FilePathCandidates -File $File)) {
        foreach ($pattern in $Patterns) {
            if ($candidatePath -ilike [string]$pattern) {
                return $true
            }
        }
    }

    return $false
}

function Get-MatchingIgnorePath {
    param(
        [Parameter(Mandatory)]
        [object]$File,

        [Parameter(Mandatory)]
        [object]$Repository
    )

    $currentFile = [pscustomobject]@{
        filename = [string]$File.filename
        previousFilename = ''
    }
    $currentMatch = $null
    foreach ($ignorePath in @($Repository.ignorePaths)) {
        if (Test-FileMatchesPatterns `
            -File $currentFile `
            -Patterns @([string]$ignorePath.path)) {
            $currentMatch = $ignorePath
            break
        }
    }

    if ($null -eq $currentMatch) {
        return $null
    }

    if (-not [string]::IsNullOrWhiteSpace([string]$File.previousFilename)) {
        $previousFile = [pscustomobject]@{
            filename = [string]$File.previousFilename
            previousFilename = ''
        }
        $previousIsIgnored = $false
        foreach ($ignorePath in @($Repository.ignorePaths)) {
            if (Test-FileMatchesPatterns `
                -File $previousFile `
                -Patterns @([string]$ignorePath.path)) {
                $previousIsIgnored = $true
                break
            }
        }

        if (-not $previousIsIgnored) {
            return $null
        }
    }

    return $currentMatch
}

function Get-ScopedComparison {
    param(
        [Parameter(Mandatory)]
        [object]$Manifest,

        [Parameter(Mandatory)]
        [object]$Repository,

        [Parameter(Mandatory)]
        [object]$Comparison
    )

    $matchedFiles = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)
    $familyResults = @(
        foreach ($family in @($Manifest.families)) {
            $watches = @(
                $family.watches |
                    Where-Object { $_.repository -eq $Repository.id })
            if ($watches.Count -eq 0) {
                continue
            }

            $files = @(
                foreach ($file in @($Comparison.files)) {
                    $matched = $false
                    foreach ($watch in $watches) {
                        if (Test-FileMatchesPatterns `
                            -File $file `
                            -Patterns @($watch.paths)) {
                            $matched = $true
                            break
                        }
                    }

                    if ($matched) {
                        [void]$matchedFiles.Add([string]$file.filename)
                        $file
                    }
                })

            if ($files.Count -gt 0) {
                [pscustomobject][ordered]@{
                    id = [string]$family.id
                    displayName = [string]$family.displayName
                    auditDocuments = @($family.auditDocuments)
                    files = $files
                }
            }
        })

    $ignoredFiles = @(
        foreach ($file in @($Comparison.files)) {
            if ($matchedFiles.Contains([string]$file.filename)) {
                continue
            }

            $ignorePath = Get-MatchingIgnorePath `
                -File $file `
                -Repository $Repository
            if ($null -ne $ignorePath) {
                [pscustomobject][ordered]@{
                    filename = [string]$file.filename
                    previousFilename = [string]$file.previousFilename
                    status = [string]$file.status
                    ignorePattern = [string]$ignorePath.path
                    justification = [string]$ignorePath.justification
                }
            }
        })
    $ignoredFileNames = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::Ordinal)
    foreach ($ignoredFile in $ignoredFiles) {
        [void]$ignoredFileNames.Add([string]$ignoredFile.filename)
    }

    $unmappedFiles = @(
        foreach ($file in @($Comparison.files)) {
            if (-not $matchedFiles.Contains([string]$file.filename) -and
                -not $ignoredFileNames.Contains([string]$file.filename)) {
                $file
            }
        })

    return [pscustomobject][ordered]@{
        isEvaluated = $true
        status = [string]$Comparison.status
        aheadBy = [int]$Comparison.aheadBy
        behindBy = [int]$Comparison.behindBy
        totalCommits = [int]$Comparison.totalCommits
        isComplete = [bool]$Comparison.isComplete
        changedFileCount = @($Comparison.files).Count
        watchedChangedFileCount = $matchedFiles.Count
        ignoredChangedFileCount = $ignoredFiles.Count
        unmappedChangedFileCount = $unmappedFiles.Count
        actionableChangedFileCount = $matchedFiles.Count + $unmappedFiles.Count
        families = $familyResults
        ignoredFiles = $ignoredFiles
        unmappedFiles = $unmappedFiles
    }
}

function Get-NotEvaluatedComparison {
    return [pscustomobject][ordered]@{
        isEvaluated = $false
        status = 'notQueried'
        aheadBy = 0
        behindBy = 0
        totalCommits = 0
        isComplete = $true
        changedFileCount = 0
        watchedChangedFileCount = 0
        ignoredChangedFileCount = 0
        unmappedChangedFileCount = 0
        actionableChangedFileCount = 0
        families = @()
        ignoredFiles = @()
        unmappedFiles = @()
    }
}

function Get-ShortRevision {
    param(
        [Parameter(Mandatory)]
        [string]$Revision
    )

    if ($Revision.Length -le 10) {
        return $Revision
    }

    return $Revision.Substring(0, 10)
}

function ConvertTo-ReportTimestamp {
    param(
        [Parameter(Mandatory)]
        [string]$Value
    )

    $styles = [Globalization.DateTimeStyles]::AssumeUniversal -bor
        [Globalization.DateTimeStyles]::AdjustToUniversal
    return [DateTimeOffset]::Parse(
        $Value,
        [Globalization.CultureInfo]::InvariantCulture,
        $styles)
}

function Add-PhaseMarkdown {
    param(
        [Parameter(Mandatory)]
        [Text.StringBuilder]$Builder,

        [Parameter(Mandatory)]
        [string]$Title,

        [Parameter(Mandatory)]
        [object]$Phase
    )

    [void]$Builder.AppendLine("### $Title")
    [void]$Builder.AppendLine()

    if (-not $Phase.comparison.isEvaluated) {
        [void]$Builder.AppendLine(
            'The historical epoch comparison was not queried. The manifest pins ' +
            'both revisions; checked-in family audits/dispositions are the durable ' +
            'epoch record. Use `-IncludeEpochComparison` for an on-demand diagnostic.')
        [void]$Builder.AppendLine()
        return
    }

    [void]$Builder.AppendLine(
        "- Comparison: ``$(Get-ShortRevision -Revision $Phase.baseRevision)`` → " +
        "``$(Get-ShortRevision -Revision $Phase.headRevision)`` " +
        "(``$($Phase.comparison.status)``; ahead $($Phase.comparison.aheadBy), " +
        "behind $($Phase.comparison.behindBy)).")
    [void]$Builder.AppendLine(
        "- Classification: $($Phase.comparison.watchedChangedFileCount) mapped, " +
        "$($Phase.comparison.unmappedChangedFileCount) unmapped, " +
        "$($Phase.comparison.ignoredChangedFileCount) explicitly ignored " +
        "of $($Phase.comparison.changedFileCount) changed files.")

    if (-not $Phase.comparison.isComplete) {
        [void]$Builder.AppendLine(
            "- **Incomplete:** GitHub returned its 300-file comparison limit; " +
            "review the upstream comparison directly before advancing a baseline.")
    }

    [void]$Builder.AppendLine()

    if ($Phase.comparison.changedFileCount -eq 0) {
        [void]$Builder.AppendLine("No files changed.")
        [void]$Builder.AppendLine()
        return
    }

    if (@($Phase.comparison.families).Count -eq 0) {
        [void]$Builder.AppendLine("#### Mapped control families")
        [void]$Builder.AppendLine()
        [void]$Builder.AppendLine("No mapped control-family paths changed.")
        [void]$Builder.AppendLine()
    }
    else {
        foreach ($family in @($Phase.comparison.families)) {
            [void]$Builder.AppendLine("#### $($family.displayName)")
            [void]$Builder.AppendLine()
            $audits = @($family.auditDocuments | ForEach-Object { "``$_``" })
            [void]$Builder.AppendLine("- Audit: $($audits -join ', ')")
            foreach ($file in @($family.files)) {
                $rename = if ([string]::IsNullOrWhiteSpace([string]$file.previousFilename)) {
                    ''
                }
                else {
                    " (from ``$($file.previousFilename)``)"
                }
                [void]$Builder.AppendLine(
                    "- ``$($file.status)`` ``$($file.filename)``$rename")
            }
            [void]$Builder.AppendLine()
        }
    }

    [void]$Builder.AppendLine('#### Explicitly ignored files')
    [void]$Builder.AppendLine()
    if (@($Phase.comparison.ignoredFiles).Count -eq 0) {
        [void]$Builder.AppendLine('None.')
        [void]$Builder.AppendLine()
    }
    else {
        foreach ($file in @($Phase.comparison.ignoredFiles)) {
            [void]$Builder.AppendLine(
                "- ``$($file.status)`` ``$($file.filename)`` matched " +
                "``$($file.ignorePattern)`` — $($file.justification)")
        }
        [void]$Builder.AppendLine()
    }

    [void]$Builder.AppendLine('#### Unmapped files (action required)')
    [void]$Builder.AppendLine()
    if (@($Phase.comparison.unmappedFiles).Count -eq 0) {
        [void]$Builder.AppendLine('None.')
        [void]$Builder.AppendLine()
    }
    else {
        foreach ($file in @($Phase.comparison.unmappedFiles)) {
            $rename = if ([string]::IsNullOrWhiteSpace([string]$file.previousFilename)) {
                ''
            }
            else {
                " (from ``$($file.previousFilename)``)"
            }
            [void]$Builder.AppendLine(
                "- ``$($file.status)`` ``$($file.filename)``$rename")
        }
        [void]$Builder.AppendLine()
    }
}

$resolvedManifestPath = (Resolve-Path -LiteralPath $ManifestPath).Path
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$manifestText = Get-Content -LiteralPath $resolvedManifestPath -Raw
$manifest = $manifestText | ConvertFrom-Json
$schemaReference = [string](Get-ObjectProperty `
    -InputObject $manifest `
    -Name '$schema')
if ([string]::IsNullOrWhiteSpace($schemaReference)) {
    throw "Upstream manifest '$resolvedManifestPath' does not declare a JSON schema."
}

$manifestDirectory = Split-Path -Parent $resolvedManifestPath
$resolvedSchemaPath = (Resolve-Path -LiteralPath (
    Join-Path $manifestDirectory $schemaReference)).Path
if (-not (Test-Json `
    -Json $manifestText `
    -SchemaFile $resolvedSchemaPath `
    -ErrorAction Stop)) {
    throw "Upstream manifest '$resolvedManifestPath' does not satisfy '$resolvedSchemaPath'."
}

Assert-Manifest -Manifest $manifest -RepositoryRoot $repositoryRoot

if ($FailOnEpochDrift -and -not $IncludeEpochComparison) {
    throw '-FailOnEpochDrift requires -IncludeEpochComparison.'
}

$fixture = $null
if (-not [string]::IsNullOrWhiteSpace($FixturePath)) {
    $resolvedFixturePath = (Resolve-Path -LiteralPath $FixturePath).Path
    $fixture = Get-Content -LiteralPath $resolvedFixturePath -Raw | ConvertFrom-Json
}

$generatedTimestamp = if (-not [string]::IsNullOrWhiteSpace($GeneratedAt)) {
    ConvertTo-ReportTimestamp -Value $GeneratedAt
}
elseif ($null -ne $fixture -and
    $null -ne (Get-ObjectProperty -InputObject $fixture -Name 'generatedAt')) {
    ConvertTo-ReportTimestamp -Value ([string]$fixture.generatedAt)
}
else {
    [DateTimeOffset]::UtcNow
}

$trackResults = @(
    foreach ($repository in @($manifest.repositories)) {
        foreach ($track in @($repository.tracks)) {
            $observedHead = Get-ObservedHead `
                -Repository $repository `
                -Track $track `
                -Fixture $fixture `
                -Token $GitHubToken

            if ([string]$observedHead.revision -notmatch '^[0-9a-f]{40}$') {
                throw "Observed head '$($repository.id)/$($track.id)' returned invalid revision '$($observedHead.revision)'."
            }

            $epochComparison = if ($IncludeEpochComparison) {
                $rawEpochComparison = Get-Comparison `
                    -Repository $repository `
                    -BaseRevision $track.reviewedBaseline.revision `
                    -HeadRevision $track.epochTarget.revision `
                    -Fixture $fixture `
                    -Token $GitHubToken
                Get-ScopedComparison `
                    -Manifest $manifest `
                    -Repository $repository `
                    -Comparison $rawEpochComparison
            }
            else {
                Get-NotEvaluatedComparison
            }
            $observedComparison = Get-Comparison `
                -Repository $repository `
                -BaseRevision $track.epochTarget.revision `
                -HeadRevision $observedHead.revision `
                -Fixture $fixture `
                -Token $GitHubToken

            [pscustomobject][ordered]@{
                repository = [string]$repository.id
                repositoryUrl = "https://github.com/$($repository.owner)/$($repository.name)"
                track = [string]$track.id
                channel = [string]$track.channel
                reviewedBaseline = [pscustomobject][ordered]@{
                    revision = [string]$track.reviewedBaseline.revision
                    label = [string]$track.reviewedBaseline.label
                }
                epochTarget = [pscustomobject][ordered]@{
                    revision = [string]$track.epochTarget.revision
                    label = [string]$track.epochTarget.label
                }
                epochAdoption = [pscustomobject][ordered]@{
                    status = [string]$track.epochAdoption.status
                    dispositionDocument =
                        [string]$track.epochAdoption.dispositionDocument
                }
                latestStableAtEpoch = Get-ObjectProperty `
                    -InputObject $track `
                    -Name 'latestStableAtEpoch'
                observedHead = [pscustomobject][ordered]@{
                    revision = [string]$observedHead.revision
                    label = [string]$observedHead.label
                    selector = [string]$track.observedHead.kind
                }
                epoch = [pscustomobject][ordered]@{
                    baseRevision = [string]$track.reviewedBaseline.revision
                    headRevision = [string]$track.epochTarget.revision
                    comparison = $epochComparison
                }
                observed = [pscustomobject][ordered]@{
                    baseRevision = [string]$track.epochTarget.revision
                    headRevision = [string]$observedHead.revision
                    comparison = Get-ScopedComparison `
                        -Manifest $manifest `
                        -Repository $repository `
                        -Comparison $observedComparison
                }
            }
        }
    })

$hasEpochDrift = @(
    $trackResults |
        Where-Object { $_.epoch.comparison.actionableChangedFileCount -gt 0 }
).Count -gt 0
$hasObservedDrift = @(
    $trackResults |
        Where-Object { $_.observed.comparison.actionableChangedFileCount -gt 0 }
).Count -gt 0
$hasEpochUnmappedDrift = @(
    $trackResults |
        Where-Object { $_.epoch.comparison.unmappedChangedFileCount -gt 0 }
).Count -gt 0
$hasObservedUnmappedDrift = @(
    $trackResults |
        Where-Object { $_.observed.comparison.unmappedChangedFileCount -gt 0 }
).Count -gt 0
$hasIncompleteComparison = @(
    $trackResults |
        Where-Object {
            -not $_.epoch.comparison.isComplete -or
            -not $_.observed.comparison.isComplete
        }
).Count -gt 0

$report = [pscustomobject][ordered]@{
    schemaVersion = 1
    generatedAt = $generatedTimestamp.ToUniversalTime().ToString('O')
    hasEpochDrift = $hasEpochDrift
    hasObservedDrift = $hasObservedDrift
    hasEpochUnmappedDrift = $hasEpochUnmappedDrift
    hasObservedUnmappedDrift = $hasObservedUnmappedDrift
    hasIncompleteComparison = $hasIncompleteComparison
    tracks = $trackResults
}

$markdown = [Text.StringBuilder]::new()
[void]$markdown.AppendLine('# ModernWpf upstream drift report')
[void]$markdown.AppendLine()
[void]$markdown.AppendLine("Generated: $($report.generatedAt)")
[void]$markdown.AppendLine()
[void]$markdown.AppendLine(
    'The reviewed baseline, finite sync-epoch target, and moving observed head ' +
    'are intentionally reported separately. This report classifies source drift; ' +
    'it does not port, merge, or advance any baseline.')
[void]$markdown.AppendLine()
[void]$markdown.AppendLine('| Repository / track | Reviewed baseline | Epoch target | Observed head | Epoch mapped / unmapped / ignored | Post-epoch mapped / unmapped / ignored |')
[void]$markdown.AppendLine('| --- | --- | --- | --- | --- | --- |')
foreach ($trackResult in $trackResults) {
    $epochClassification = if ($trackResult.epoch.comparison.isEvaluated) {
        "$($trackResult.epoch.comparison.watchedChangedFileCount) / " +
        "$($trackResult.epoch.comparison.unmappedChangedFileCount) / " +
        "$($trackResult.epoch.comparison.ignoredChangedFileCount)"
    }
    else {
        'not queried'
    }
    $observedClassification =
        "$($trackResult.observed.comparison.watchedChangedFileCount) / " +
        "$($trackResult.observed.comparison.unmappedChangedFileCount) / " +
        "$($trackResult.observed.comparison.ignoredChangedFileCount)"
    [void]$markdown.AppendLine(
        "| $($trackResult.repository) / $($trackResult.track) " +
        "| ``$(Get-ShortRevision -Revision $trackResult.reviewedBaseline.revision)`` " +
        "| ``$(Get-ShortRevision -Revision $trackResult.epochTarget.revision)`` " +
        "| ``$(Get-ShortRevision -Revision $trackResult.observedHead.revision)`` " +
        "| $epochClassification " +
        "| $observedClassification |")
}
[void]$markdown.AppendLine()

foreach ($trackResult in $trackResults) {
    [void]$markdown.AppendLine("## $($trackResult.repository) / $($trackResult.track)")
    [void]$markdown.AppendLine()
    [void]$markdown.AppendLine("- Source: $($trackResult.repositoryUrl)")
    [void]$markdown.AppendLine(
        "- Moving selector: ``$($trackResult.observedHead.selector)`` → " +
        "``$($trackResult.observedHead.label)``.")
    [void]$markdown.AppendLine(
        "- Epoch adoption: ``$($trackResult.epochAdoption.status)``; disposition " +
        "``$($trackResult.epochAdoption.dispositionDocument)``.")
    [void]$markdown.AppendLine()

    Add-PhaseMarkdown `
        -Builder $markdown `
        -Title 'Reviewed baseline → finite epoch target' `
        -Phase $trackResult.epoch
    Add-PhaseMarkdown `
        -Builder $markdown `
        -Title 'Finite epoch target → moving observed head' `
        -Phase $trackResult.observed
}

$markdownText = $markdown.ToString()
$jsonText = $report | ConvertTo-Json -Depth 20

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $outputDirectory = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
        [void](New-Item -ItemType Directory -Path $outputDirectory -Force)
    }
    Set-Content -LiteralPath $OutputPath -Value $markdownText -Encoding utf8NoBOM
}

if (-not [string]::IsNullOrWhiteSpace($JsonOutputPath)) {
    $jsonOutputDirectory = Split-Path -Parent $JsonOutputPath
    if (-not [string]::IsNullOrWhiteSpace($jsonOutputDirectory)) {
        [void](New-Item -ItemType Directory -Path $jsonOutputDirectory -Force)
    }
    Set-Content -LiteralPath $JsonOutputPath -Value $jsonText -Encoding utf8NoBOM
}

Write-Output $markdownText

if ($FailOnIncompleteComparison -and $hasIncompleteComparison) {
    [Console]::Error.WriteLine('At least one upstream comparison was incomplete.')
    exit 3
}

if ($FailOnObservedDrift -and $hasObservedDrift) {
    [Console]::Error.WriteLine(
        'Actionable mapped or unmapped upstream changes arrived after the finite sync-epoch target.')
    exit 2
}

if ($FailOnEpochDrift -and $hasEpochDrift) {
    [Console]::Error.WriteLine(
        'The finite sync epoch contains actionable mapped or unmapped changes that are not in the reviewed baseline.')
    exit 2
}

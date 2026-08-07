[CmdletBinding()]
param(
    [string]$CanaryId,

    [string]$CandidatePackagePath,

    [string]$OutputPath,

    [string]$WorkPath,

    [string]$MSBuildDotNetRoot,

    [string]$MSBuildSdkVersion,

    [string]$ManifestPath = (Join-Path $PSScriptRoot 'downstream-canaries.json'),

    [string]$SchemaPath = (Join-Path $PSScriptRoot 'downstream-canaries.schema.json'),

    [switch]$ValidateManifestOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$allowedChildEnvironmentVariables = @(
    'APPDATA',
    'CommonProgramFiles',
    'CommonProgramFiles(x86)',
    'CommonProgramW6432',
    'ComSpec',
    'HOMEDRIVE',
    'HOMEPATH',
    'LOCALAPPDATA',
    'NUMBER_OF_PROCESSORS',
    'OS',
    'Path',
    'PATHEXT',
    'PROCESSOR_ARCHITECTURE',
    'ProgramData',
    'ProgramFiles',
    'ProgramFiles(x86)',
    'ProgramW6432',
    'SystemDrive',
    'SystemRoot',
    'USERNAME',
    'USERPROFILE',
    'windir'
)

function Test-Manifest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Schema
    )

    $resolvedManifest = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path
    $resolvedSchema = (Resolve-Path -LiteralPath $Schema -ErrorAction Stop).Path
    $manifestText = [IO.File]::ReadAllText($resolvedManifest)
    if (-not ($manifestText | Test-Json -SchemaFile $resolvedSchema -ErrorAction Stop)) {
        throw "Downstream canary manifest '$resolvedManifest' does not match its schema."
    }

    $manifest = $manifestText | ConvertFrom-Json -Depth 20
    $ids = @($manifest.repositories | ForEach-Object { [string]$_.id })
    if (@($ids | Sort-Object -Unique).Count -ne $ids.Count) {
        throw 'Downstream canary IDs must be unique.'
    }

    foreach ($repository in $manifest.repositories) {
        $packageMigrations = @($repository.migrations | Where-Object {
            $_.kind -eq 'package-version'
        })
        if ($packageMigrations.Count -ne 1 -or
            $packageMigrations[0].path -ne $repository.project -or
            $packageMigrations[0].packageId -ne $manifest.packageId -or
            $packageMigrations[0].fromVersion -ne $repository.baselinePackageVersion) {
            throw "Canary '$($repository.id)' must contain one package-version migration " +
                'for its reviewed project and baseline package version.'
        }
        foreach ($textMigration in @($repository.migrations | Where-Object {
            $_.kind -eq 'text-replacement'
        })) {
            if ($textMigration.from -ne 'SimpleStackPanel' -or
                $textMigration.to -ne 'StackPanelEx') {
                throw "Canary '$($repository.id)' contains a text migration that is not " +
                    'the documented SimpleStackPanel to StackPanelEx rename.'
            }
        }
    }

    return $manifest
}

function Resolve-ChildPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string]$RelativePath,

        [switch]$MustExist
    )

    $rootFullPath = [IO.Path]::GetFullPath($Root)
    $candidate = [IO.Path]::GetFullPath((Join-Path $rootFullPath $RelativePath))
    $rootPrefix = $rootFullPath.TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    if (-not $candidate.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path '$RelativePath' escapes the canary worktree."
    }
    if ($MustExist -and -not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
        throw "Expected canary file does not exist: $candidate"
    }

    return $candidate
}

function Get-PackageIdentity {
    param([Parameter(Mandatory = $true)][string]$Path)

    $resolvedPath = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path
    if (-not $resolvedPath.EndsWith('.nupkg', [StringComparison]::OrdinalIgnoreCase) -or
        $resolvedPath.EndsWith('.snupkg', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Candidate package must be a .nupkg file: $resolvedPath"
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($resolvedPath)
    try {
        $nuspecEntries = @($archive.Entries | Where-Object {
            $_.FullName.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase) -and
            -not $_.FullName.Contains('/')
        })
        if ($nuspecEntries.Count -ne 1) {
            throw "Candidate package must contain exactly one root .nuspec; found $($nuspecEntries.Count)."
        }

        $reader = [IO.StreamReader]::new($nuspecEntries[0].Open())
        try {
            [xml]$nuspec = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }
        $idNode = $nuspec.SelectSingleNode(
            "/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='id']")
        $versionNode = $nuspec.SelectSingleNode(
            "/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='version']")
        $repositoryNode = $nuspec.SelectSingleNode(
            "/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='repository']")
        if ($null -eq $idNode -or $null -eq $versionNode) {
            throw 'Candidate package metadata must contain id and version.'
        }
        $repositoryUrl = if ($null -eq $repositoryNode) {
            ''
        }
        else {
            [string]$repositoryNode.GetAttribute('url')
        }
        $repositoryCommit = if ($null -eq $repositoryNode) {
            ''
        }
        else {
            [string]$repositoryNode.GetAttribute('commit')
        }
        if ([string]::IsNullOrWhiteSpace($repositoryUrl) -or
            $repositoryCommit -notmatch '^[0-9a-f]{40}$') {
            throw 'Candidate package repository metadata must contain a URL and a 40-character lowercase commit SHA.'
        }

        return [pscustomobject]@{
            Path = $resolvedPath
            Id = $idNode.InnerText
            Version = $versionNode.InnerText
            RepositoryUrl = $repositoryUrl
            RepositoryCommit = $repositoryCommit
            Sha256 = (Get-FileHash -LiteralPath $resolvedPath -Algorithm SHA256).Hash.ToLowerInvariant()
        }
    }
    finally {
        $archive.Dispose()
    }
}

function New-NuGetConfig {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [string]$LocalFeed,

        [Parameter(Mandatory = $true)]
        [string]$PackageId
    )

    $nugetSource = 'https://api.nuget.org/v3/index.json'
    if ([string]::IsNullOrWhiteSpace($LocalFeed)) {
        $content = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="$nugetSource" protocolVersion="3" />
  </packageSources>
</configuration>
"@
    }
    else {
        $escapedFeed = [Security.SecurityElement]::Escape(
            [IO.Path]::GetFullPath($LocalFeed))
        $content = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="modernwpf-candidate" value="$escapedFeed" />
    <add key="nuget.org" value="$nugetSource" protocolVersion="3" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="modernwpf-candidate">
      <package pattern="$PackageId" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@
    }

    [IO.File]::WriteAllText($Path, $content, [Text.UTF8Encoding]::new($false))
}

function New-IsolatedEnvironment {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    $paths = @{
        Packages = Join-Path $Root 'packages'
        HttpCache = Join-Path $Root 'http-cache'
        PluginsCache = Join-Path $Root 'plugins-cache'
        DotNetHome = Join-Path $Root 'dotnet-home'
        Temp = Join-Path $Root 'temp'
    }
    foreach ($path in $paths.Values) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }

    return @{
        DOTNET_CLI_HOME = $paths.DotNetHome
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        DOTNET_NOLOGO = '1'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        GCM_INTERACTIVE = 'Never'
        GIT_CONFIG_COUNT = '1'
        GIT_CONFIG_KEY_0 = 'credential.helper'
        GIT_CONFIG_VALUE_0 = ''
        GIT_TERMINAL_PROMPT = '0'
        HOME = $paths.DotNetHome
        NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED = 'false'
        NUGET_HTTP_CACHE_PATH = $paths.HttpCache
        NUGET_PACKAGES = $paths.Packages
        NUGET_PLUGINS_CACHE_PATH = $paths.PluginsCache
        TEMP = $paths.Temp
        TMP = $paths.Temp
    }
}

function New-CleanProcessEnvironment {
    param([hashtable]$Overrides = @{})

    $environment = @{
        CI = 'true'
        GCM_INTERACTIVE = 'Never'
        GIT_TERMINAL_PROMPT = '0'
        NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED = 'false'
    }
    foreach ($name in $allowedChildEnvironmentVariables) {
        $value = [Environment]::GetEnvironmentVariable($name)
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            $environment[$name] = $value
        }
    }
    foreach ($entry in $Overrides.GetEnumerator()) {
        $environment[$entry.Key] = [string]$entry.Value
    }

    return $environment
}

function Invoke-LoggedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$FileName,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,

        [Parameter(Mandatory = $true)]
        [string]$LogDirectory,

        [hashtable]$Environment = @{}
    )

    $logPath = Join-Path $LogDirectory "$Name.log"
    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true
    foreach ($argument in $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }
    $childEnvironment = New-CleanProcessEnvironment -Overrides $Environment
    $startInfo.Environment.Clear()
    foreach ($entry in $childEnvironment.GetEnumerator()) {
        $startInfo.Environment[$entry.Key] = [string]$entry.Value
    }

    $commandDisplay = $FileName + ' ' + ($Arguments | ForEach-Object {
        if ($_ -match '\s') { '"' + $_.Replace('"', '\"') + '"' } else { $_ }
    }) -join ' '
    try {
        $process = [Diagnostics.Process]::new()
        $process.StartInfo = $startInfo
        $null = $process.Start()
        $standardOutputTask = $process.StandardOutput.ReadToEndAsync()
        $standardErrorTask = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $standardOutput = $standardOutputTask.GetAwaiter().GetResult()
        $standardError = $standardErrorTask.GetAwaiter().GetResult()
        $exitCode = $process.ExitCode
        $process.Dispose()
    }
    catch {
        $standardOutput = ''
        $standardError = $_.Exception.Message
        $exitCode = -1
    }

    $logText = "Command: $commandDisplay`r`nExit code: $exitCode`r`n`r`n" +
        $standardOutput +
        $(if ([string]::IsNullOrWhiteSpace($standardError)) {
            ''
        }
        else {
            "`r`n--- stderr ---`r`n$standardError"
        })
    [IO.File]::WriteAllText($logPath, $logText, [Text.UTF8Encoding]::new($false))

    return [pscustomobject]@{
        name = $Name
        outcome = if ($exitCode -eq 0) { 'passed' } else { 'failed' }
        exitCode = $exitCode
        log = "logs/$Name.log"
        StandardOutput = $standardOutput
        StandardError = $standardError
    }
}

function Test-EnvironmentalFailure {
    param([Parameter(Mandatory = $true)]$Stage)

    $diagnostic = $Stage.StandardOutput + "`n" + $Stage.StandardError
    return $diagnostic -match
        '(?i)NU130[123]|timed?\s*out|name or service not known|no such host is known|' +
        'temporary failure in name resolution|could not resolve host|connection (?:reset|refused)|' +
        'network is unreachable|SSL connection|TLS (?:handshake|connection)|HTTP (?:408|429|502|503|504)|' +
        'rate limit|no space left|disk full'
}

function Add-Stage {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[object]]$List,

        [Parameter(Mandatory = $true)]
        $Stage
    )

    $List.Add([pscustomobject]@{
        name = $Stage.name
        outcome = $Stage.outcome
        exitCode = $Stage.exitCode
        log = $Stage.log
    })
}

function Get-MSBuildSdkResolverEnvironment {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DotNetRoot,

        [Parameter(Mandatory = $true)]
        [string]$Version,

        [Parameter(Mandatory = $true)]
        [string]$LogDirectory,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[object]]$Stages
    )

    $environment = @{}
    $exitCode = 1
    $message = ''
    try {
        $cliDirectory = [IO.Path]::GetFullPath($DotNetRoot)
        $sdkRoot = Join-Path $cliDirectory 'sdk'
        $installedSdks = @(
            Get-ChildItem -LiteralPath $sdkRoot -Directory -ErrorAction Stop |
                Select-Object -ExpandProperty Name
        )
        if ($installedSdks.Count -ne 1 -or $installedSdks[0] -ne $Version) {
            throw "The isolated .NET root must contain only SDK $Version."
        }

        $sdksDirectory = Join-Path (Join-Path $sdkRoot $Version) 'Sdks'
        if (-not (Test-Path -LiteralPath $sdksDirectory -PathType Container) -or
            -not (Test-Path -LiteralPath (Join-Path $cliDirectory 'dotnet.exe') -PathType Leaf)) {
            throw "The isolated .NET root does not contain SDK $Version and its CLI."
        }

        $environment = @{
            DOTNET_MULTILEVEL_LOOKUP = '0'
            DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR = $cliDirectory
            DOTNET_MSBUILD_SDK_RESOLVER_SDKS_DIR = $sdksDirectory
            DOTNET_MSBUILD_SDK_RESOLVER_SDKS_VER = $Version
        }
        $message = "Pinned full MSBuild to .NET SDK $Version at $sdksDirectory."
        $exitCode = 0
    }
    catch {
        $message = $_.Exception.Message
    }

    [IO.File]::WriteAllText(
        (Join-Path $LogDirectory 'msbuild-sdk-selection.log'),
        $message,
        [Text.UTF8Encoding]::new($false))
    Add-Stage -List $Stages -Stage ([pscustomobject]@{
        name = 'msbuild-sdk-selection'
        outcome = if ($exitCode -eq 0) { 'passed' } else { 'failed' }
        exitCode = $exitCode
        log = 'logs/msbuild-sdk-selection.log'
    })

    return [pscustomobject]@{
        ExitCode = $exitCode
        Environment = $environment
    }
}

function Test-LocalPackageSource {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PhaseRoot,

        [Parameter(Mandatory = $true)]
        [string]$PackageId,

        [Parameter(Mandatory = $true)]
        [string]$PackageVersion,

        [Parameter(Mandatory = $true)]
        [string]$ExpectedLocalFeed,

        [Parameter(Mandatory = $true)]
        [string]$LogDirectory
    )

    $logPath = Join-Path $LogDirectory 'candidate-package-source.log'
    $metadataPath = Join-Path $PhaseRoot (
        "packages/$($PackageId.ToLowerInvariant())/$PackageVersion/.nupkg.metadata")
    $exitCode = 1
    $message = ''
    try {
        if (-not (Test-Path -LiteralPath $metadataPath -PathType Leaf)) {
            throw "NuGet did not write package-source metadata at '$metadataPath'."
        }

        $metadata = Get-Content -LiteralPath $metadataPath -Raw |
            ConvertFrom-Json -Depth 5
        $expectedSource = [IO.Path]::GetFullPath($ExpectedLocalFeed).TrimEnd(
            [IO.Path]::DirectorySeparatorChar,
            [IO.Path]::AltDirectorySeparatorChar)
        $actualSource = [IO.Path]::GetFullPath([string]$metadata.source).TrimEnd(
            [IO.Path]::DirectorySeparatorChar,
            [IO.Path]::AltDirectorySeparatorChar)
        if (-not $actualSource.Equals(
            $expectedSource,
            [StringComparison]::OrdinalIgnoreCase)) {
            throw "Candidate package source '$actualSource' does not match " +
                "the downloaded local feed '$expectedSource'."
        }

        $message = "Verified local candidate source: $actualSource"
        $exitCode = 0
    }
    catch {
        $message = $_.Exception.Message
    }
    [IO.File]::WriteAllText($logPath, $message, [Text.UTF8Encoding]::new($false))

    return [pscustomobject]@{
        name = 'candidate-package-source'
        outcome = if ($exitCode -eq 0) { 'passed' } else { 'failed' }
        exitCode = $exitCode
        log = 'logs/candidate-package-source.log'
        StandardOutput = $message
        StandardError = if ($exitCode -eq 0) { '' } else { $message }
    }
}

function Invoke-RestoreAndBuild {
    param(
        [Parameter(Mandatory = $true)]
        $Canary,

        [Parameter(Mandatory = $true)]
        [string]$SourceRoot,

        [Parameter(Mandatory = $true)]
        [string]$ConfigPath,

        [Parameter(Mandatory = $true)]
        [string]$PhaseRoot,

        [Parameter(Mandatory = $true)]
        [string]$PhaseName,

        [Parameter(Mandatory = $true)]
        [string]$LogDirectory,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[object]]$Stages,

        [string]$CandidatePackageId,

        [string]$CandidatePackageVersion,

        [string]$ExpectedLocalFeed,

        [hashtable]$BuildEnvironment = @{}
    )

    $projectPath = Resolve-ChildPath -Root $SourceRoot `
        -RelativePath $Canary.project -MustExist
    $environment = New-IsolatedEnvironment -Root $PhaseRoot
    foreach ($entry in $BuildEnvironment.GetEnumerator()) {
        $environment[$entry.Key] = [string]$entry.Value
    }
    if ($Canary.buildTool -eq 'dotnet') {
        $restoreArguments = @(
            'restore',
            $projectPath,
            '--configfile',
            $ConfigPath,
            '--packages',
            $environment.NUGET_PACKAGES,
            '--no-cache',
            '--disable-parallel',
            '--property:RestoreIgnoreFailedSources=false'
        )
        $buildArguments = @(
            'build',
            $projectPath,
            '--configuration',
            $Canary.configuration,
            '--no-restore',
            '--maxcpucount:1'
        )
        $fileName = 'dotnet'
    }
    else {
        $restoreArguments = @(
            $projectPath,
            '/t:Restore',
            "/p:RestoreConfigFile=$ConfigPath",
            "/p:RestorePackagesPath=$($environment.NUGET_PACKAGES)",
            '/p:RestoreIgnoreFailedSources=false',
            '/p:RestoreNoCache=true',
            '/m:1',
            '/nologo',
            '/v:minimal'
        )
        $buildArguments = @(
            $projectPath,
            '/t:Build',
            "/p:Configuration=$($Canary.configuration)",
            "/p:RestorePackagesPath=$($environment.NUGET_PACKAGES)",
            '/m:1',
            '/nologo',
            '/v:minimal'
        )
        $fileName = 'msbuild'
    }

    $restore = Invoke-LoggedCommand -Name "$PhaseName-restore" `
        -FileName $fileName -Arguments $restoreArguments `
        -WorkingDirectory $SourceRoot -LogDirectory $LogDirectory `
        -Environment $environment
    Add-Stage -List $Stages -Stage $restore
    if ($restore.exitCode -ne 0) {
        return [pscustomobject]@{ Restore = $restore; Source = $null; Build = $null }
    }

    $source = $null
    if (-not [string]::IsNullOrWhiteSpace($ExpectedLocalFeed)) {
        $source = Test-LocalPackageSource -PhaseRoot $PhaseRoot `
            -PackageId $CandidatePackageId `
            -PackageVersion $CandidatePackageVersion `
            -ExpectedLocalFeed $ExpectedLocalFeed `
            -LogDirectory $LogDirectory
        Add-Stage -List $Stages -Stage $source
        if ($source.exitCode -ne 0) {
            return [pscustomobject]@{
                Restore = $restore
                Source = $source
                Build = $null
            }
        }
    }

    $build = Invoke-LoggedCommand -Name "$PhaseName-build" `
        -FileName $fileName -Arguments $buildArguments `
        -WorkingDirectory $SourceRoot -LogDirectory $LogDirectory `
        -Environment $environment
    Add-Stage -List $Stages -Stage $build
    return [pscustomobject]@{ Restore = $restore; Source = $source; Build = $build }
}

function Write-CanaryReport {
    param(
        [Parameter(Mandatory = $true)]
        $Result,

        [Parameter(Mandatory = $true)]
        [string]$Directory
    )

    $jsonPath = Join-Path $Directory 'result.json'
    $markdownPath = Join-Path $Directory 'result.md'
    $Result.generatedAt = [DateTimeOffset]::UtcNow.ToString('O')
    $Result | ConvertTo-Json -Depth 20 |
        Set-Content -LiteralPath $jsonPath -Encoding utf8NoBOM

    $stageRows = foreach ($stage in $Result.stages) {
        "| ``$($stage.name)`` | $($stage.outcome) | $($stage.exitCode) | ``$($stage.log)`` |"
    }
    $stageRowsText = $stageRows -join "`n"
    $markdown = @(
        "# Downstream canary: $($Result.canary.id)",
        '',
        "- Classification: **$($Result.classification)**",
        "- Summary: $($Result.summary)",
        "- Repository: ``$($Result.canary.repository)@$($Result.canary.commit)``",
        "- Project: ``$($Result.canary.project)`` ($($Result.canary.targetFramework))",
        "- Package: ``$($Result.package.id)`` $($Result.package.baselineVersion) → $($Result.package.candidateVersion)",
        "- Candidate SHA-256: ``$($Result.package.sha256)``",
        "- Resource entry retained for staged migration: ``$($Result.migration.resourceEntry)``",
        '',
        '## Stages',
        '',
        '| Stage | Outcome | Exit code | Log |',
        '| --- | --- | ---: | --- |',
        $stageRowsText,
        '',
        '## Migration',
        '',
        "Only the reviewed package-version and documented API-rename migrations were applied. See the [0.9 migration guide]($($Result.migration.guide)).",
        "The exact generated change is retained as ``$($Result.migration.patch)``; no third-party source is committed to ModernWPF."
    ) -join "`n"
    [IO.File]::WriteAllText(
        $markdownPath,
        $markdown + "`n",
        [Text.UTF8Encoding]::new($false))
}

$manifest = Test-Manifest -Path $ManifestPath -Schema $SchemaPath
if ($ValidateManifestOnly) {
    Write-Output "Validated $($manifest.repositories.Count) downstream canaries."
    exit 0
}

foreach ($requiredValue in @{
    CanaryId = $CanaryId
    CandidatePackagePath = $CandidatePackagePath
    OutputPath = $OutputPath
}.GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace([string]$requiredValue.Value)) {
        throw "-$($requiredValue.Key) is required unless -ValidateManifestOnly is used."
    }
}

$canaryMatches = @($manifest.repositories | Where-Object { $_.id -eq $CanaryId })
if ($canaryMatches.Count -ne 1) {
    throw "Unknown downstream canary '$CanaryId'."
}
$canary = $canaryMatches[0]
if ($canary.buildTool -eq 'msbuild' -and
    ([string]::IsNullOrWhiteSpace($MSBuildDotNetRoot) -or
        $MSBuildSdkVersion -notmatch '^\d+\.\d+\.\d+$')) {
    throw '-MSBuildDotNetRoot and an exact stable -MSBuildSdkVersion are required for an MSBuild canary.'
}
$package = Get-PackageIdentity -Path $CandidatePackagePath
if ($package.Id -ne $manifest.packageId) {
    throw "Expected candidate package '$($manifest.packageId)', found '$($package.Id)'."
}
if (-not $package.Version.StartsWith('1.', [StringComparison]::Ordinal) -or
    $package.Version -eq $canary.baselinePackageVersion) {
    throw "Candidate version '$($package.Version)' is not a ModernWPF 1.x migration candidate."
}

$resolvedOutputPath = [IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path $resolvedOutputPath -Force | Out-Null
$logDirectory = Join-Path $resolvedOutputPath 'logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
if ([string]::IsNullOrWhiteSpace($WorkPath)) {
    $WorkPath = Join-Path ([IO.Path]::GetTempPath()) 'modernwpf-downstream-canaries'
}
$runRoot = Join-Path ([IO.Path]::GetFullPath($WorkPath)) `
    "$($canary.id)-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $runRoot -ErrorAction Stop | Out-Null
$baselineRoot = Join-Path $runRoot 'baseline'
$candidateRoot = Join-Path $runRoot 'candidate'
$baselineConfig = Join-Path $runRoot 'NuGet.baseline.config'
$candidateConfig = Join-Path $runRoot 'NuGet.candidate.config'
New-NuGetConfig -Path $baselineConfig -PackageId $manifest.packageId
New-NuGetConfig -Path $candidateConfig `
    -LocalFeed (Split-Path -Parent $package.Path) -PackageId $manifest.packageId

$stages = [Collections.Generic.List[object]]::new()
$result = [pscustomobject]@{
    schemaVersion = 1
    generatedAt = $null
    classification = 'infrastructure-failure'
    summary = 'The canary runner did not complete.'
    canary = [pscustomobject]@{
        id = $canary.id
        repository = $canary.repository
        commit = $canary.commit
        project = $canary.project
        targetFramework = $canary.targetFramework
        fetchDepth = $canary.fetchDepth
        license = $canary.license
    }
    package = [pscustomobject]@{
        id = $package.Id
        baselineVersion = $canary.baselinePackageVersion
        candidateVersion = $package.Version
        repositoryCommit = $package.RepositoryCommit
        sha256 = $package.Sha256
    }
    migration = [pscustomobject]@{
        guide = "$($package.RepositoryUrl.TrimEnd('/'))/blob/$($package.RepositoryCommit)/$($manifest.migrationGuidePath)"
        guideRevision = $package.RepositoryCommit
        resourceEntry = $canary.resourceEntry
        changedFiles = @()
        patch = 'migration.patch'
    }
    stages = $stages
}
$exitCode = -1

try {
    $cloneEnvironment = @{ GIT_TERMINAL_PROMPT = '0' }
    $cloneInit = Invoke-LoggedCommand -Name 'clone-init' -FileName 'git' `
        -Arguments @('init', $baselineRoot) -WorkingDirectory $runRoot `
        -LogDirectory $logDirectory -Environment $cloneEnvironment
    Add-Stage -List $stages -Stage $cloneInit
    if ($cloneInit.exitCode -ne 0) {
        throw 'Could not initialize the baseline canary worktree.'
    }

    $repositoryUrl = "https://github.com/$($canary.repository).git"
    $cloneRemote = Invoke-LoggedCommand -Name 'clone-remote' -FileName 'git' `
        -Arguments @('-C', $baselineRoot, 'remote', 'add', 'origin', $repositoryUrl) `
        -WorkingDirectory $runRoot -LogDirectory $logDirectory `
        -Environment $cloneEnvironment
    Add-Stage -List $stages -Stage $cloneRemote
    if ($cloneRemote.exitCode -ne 0) {
        throw 'Could not configure the public canary remote.'
    }

    $cloneFetchArguments = @(
        '-c',
        'credential.helper=',
        '-C',
        $baselineRoot,
        'fetch')
    if ($canary.fetchDepth -eq 0) {
        $cloneFetchArguments += '--tags'
    }
    else {
        $cloneFetchArguments += "--depth=$($canary.fetchDepth)"
        $cloneFetchArguments += '--no-tags'
    }
    $cloneFetchArguments += @('origin', $canary.commit)
    $cloneFetch = Invoke-LoggedCommand -Name 'clone-fetch' -FileName 'git' `
        -Arguments $cloneFetchArguments `
        -WorkingDirectory $runRoot -LogDirectory $logDirectory `
        -Environment $cloneEnvironment
    Add-Stage -List $stages -Stage $cloneFetch
    if ($cloneFetch.exitCode -ne 0) {
        $result.classification = 'environmental'
        $result.summary = 'The exact reviewed commit could not be fetched anonymously.'
        $exitCode = 3
    }

    if ($exitCode -eq -1) {
        $cloneCheckout = Invoke-LoggedCommand -Name 'clone-checkout' -FileName 'git' `
            -Arguments @(
                '-C',
                $baselineRoot,
                'checkout',
                '-b',
                'modernwpf-canary',
                $canary.commit) `
            -WorkingDirectory $runRoot -LogDirectory $logDirectory `
            -Environment $cloneEnvironment
        Add-Stage -List $stages -Stage $cloneCheckout
        if ($cloneCheckout.exitCode -ne 0) {
            throw 'Could not check out the exact reviewed commit.'
        }

        $cloneVerify = Invoke-LoggedCommand -Name 'clone-verify' -FileName 'git' `
            -Arguments @('-C', $baselineRoot, 'rev-parse', 'HEAD') `
            -WorkingDirectory $runRoot -LogDirectory $logDirectory `
            -Environment $cloneEnvironment
        Add-Stage -List $stages -Stage $cloneVerify
        if ($cloneVerify.exitCode -ne 0 -or
            $cloneVerify.StandardOutput.Trim() -ne $canary.commit) {
            throw 'Canary checkout does not match the reviewed commit.'
        }

        $candidateClone = Invoke-LoggedCommand -Name 'candidate-copy' -FileName 'git' `
            -Arguments @(
                'clone',
                '--local',
                '--no-hardlinks',
                $baselineRoot,
                $candidateRoot) `
            -WorkingDirectory $runRoot -LogDirectory $logDirectory `
            -Environment $cloneEnvironment
        Add-Stage -List $stages -Stage $candidateClone
        if ($candidateClone.exitCode -ne 0) {
            throw 'Could not create the isolated candidate worktree.'
        }

        if (@($canary.submodules).Count -gt 0) {
            foreach ($submoduleWorktree in @(
                [pscustomobject]@{ Name = 'baseline-submodules'; Root = $baselineRoot },
                [pscustomobject]@{ Name = 'candidate-submodules'; Root = $candidateRoot }
            )) {
                $submodulePaths = @($canary.submodules | ForEach-Object { [string]$_ })
                $submoduleArguments = @(
                    '-c',
                    'credential.helper=',
                    '-C',
                    $submoduleWorktree.Root,
                    'submodule',
                    'update',
                    '--init',
                    '--recursive',
                    '--depth=1',
                    '--jobs=1',
                    '--') + $submodulePaths
                $submoduleUpdate = Invoke-LoggedCommand `
                    -Name $submoduleWorktree.Name -FileName 'git' `
                    -Arguments $submoduleArguments `
                    -WorkingDirectory $runRoot -LogDirectory $logDirectory `
                    -Environment $cloneEnvironment
                Add-Stage -List $stages -Stage $submoduleUpdate
                if ($submoduleUpdate.exitCode -ne 0) {
                    if (Test-EnvironmentalFailure -Stage $submoduleUpdate) {
                        $result.classification = 'environmental'
                        $result.summary = 'A reviewed public submodule could not be fetched because of an environmental failure.'
                        $exitCode = 3
                    }
                    else {
                        $result.classification = 'infrastructure-failure'
                        $result.summary = 'A reviewed public submodule could not be initialized at its pinned commit.'
                        $exitCode = 5
                    }
                    break
                }

                $submoduleStatus = Invoke-LoggedCommand `
                    -Name "$($submoduleWorktree.Name)-status" -FileName 'git' `
                    -Arguments (@(
                        '-C',
                        $submoduleWorktree.Root,
                        'submodule',
                        'status',
                        '--recursive',
                        '--') + $submodulePaths) `
                    -WorkingDirectory $runRoot -LogDirectory $logDirectory `
                    -Environment $cloneEnvironment
                if ($submoduleStatus.exitCode -eq 0 -and
                    $submoduleStatus.StandardOutput -match '(?m)^[\-+U]') {
                    $submoduleStatus.exitCode = 1
                    $submoduleStatus.outcome = 'failed'
                }
                Add-Stage -List $stages -Stage $submoduleStatus
                if ($submoduleStatus.exitCode -ne 0 -or
                    [string]::IsNullOrWhiteSpace($submoduleStatus.StandardOutput)) {
                    $result.classification = 'infrastructure-failure'
                    $result.summary = 'A reviewed public submodule is not checked out at its pinned gitlink.'
                    $exitCode = 5
                    break
                }
            }
        }
    }

    if ($exitCode -eq -1) {
        $buildEnvironment = @{}
        if ($canary.buildTool -eq 'msbuild') {
            $sdkResolver = Get-MSBuildSdkResolverEnvironment `
                -DotNetRoot $MSBuildDotNetRoot -Version $MSBuildSdkVersion `
                -LogDirectory $logDirectory -Stages $stages
            if ($sdkResolver.ExitCode -ne 0) {
                $result.classification = 'infrastructure-failure'
                $result.summary = 'The pinned .NET SDK for full MSBuild could not be selected.'
                $exitCode = 5
            }
            else {
                $buildEnvironment = $sdkResolver.Environment
            }
        }
    }

    if ($exitCode -eq -1) {
        $baseline = Invoke-RestoreAndBuild -Canary $canary `
            -SourceRoot $baselineRoot -ConfigPath $baselineConfig `
            -PhaseRoot (Join-Path $runRoot 'baseline-state') `
            -PhaseName 'baseline' -LogDirectory $logDirectory -Stages $stages `
            -BuildEnvironment $buildEnvironment
        if ($baseline.Restore.exitCode -eq -1) {
            $result.classification = 'infrastructure-failure'
            $result.summary = 'The configured baseline build tool could not be started.'
            $exitCode = 5
        }
        elseif ($baseline.Restore.exitCode -ne 0) {
            if (Test-EnvironmentalFailure -Stage $baseline.Restore) {
                $result.classification = 'environmental'
                $result.summary = 'The unchanged baseline could not restore because of an environmental failure.'
                $exitCode = 3
            }
            else {
                $result.classification = 'baseline-failure'
                $result.summary = 'The unchanged pinned baseline no longer restores.'
                $exitCode = 2
            }
        }
        elseif ($baseline.Build.exitCode -ne 0) {
            $result.classification = 'baseline-failure'
            $result.summary = 'The unchanged pinned baseline does not build.'
            $exitCode = 2
        }
    }

    if ($exitCode -eq -1) {
        $migrationScript = Join-Path $PSScriptRoot `
            'Set-DownstreamCanaryPackageVersion.ps1'
        foreach ($migration in $canary.migrations) {
            $migrationPath = Resolve-ChildPath -Root $candidateRoot `
                -RelativePath $migration.path -MustExist
            if ($migration.kind -eq 'package-version') {
                & $migrationScript -ProjectPath $migrationPath `
                    -PackageId $migration.packageId `
                    -FromVersion $migration.fromVersion `
                    -ToVersion $package.Version | Out-Null
            }
            elseif ($migration.kind -eq 'text-replacement') {
                $textMigrationScript = Join-Path $PSScriptRoot `
                    'Set-DownstreamCanaryTextReplacement.ps1'
                & $textMigrationScript -Path $migrationPath `
                    -From $migration.from -To $migration.to `
                    -ExpectedOccurrences $migration.expectedOccurrences | Out-Null
            }
            else {
                throw "Unsupported migration kind '$($migration.kind)'."
            }
        }

        $changedFilesStage = Invoke-LoggedCommand -Name 'migration-files' `
            -FileName 'git' -Arguments @(
                '-C',
                $candidateRoot,
                'diff',
                '--name-only',
                '--no-ext-diff',
                '--') `
            -WorkingDirectory $runRoot -LogDirectory $logDirectory `
            -Environment $cloneEnvironment
        Add-Stage -List $stages -Stage $changedFilesStage
        $changedFiles = @(
            $changedFilesStage.StandardOutput -split '\r?\n' |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        )
        $expectedFiles = @($canary.migrations | ForEach-Object {
            $_.path.Replace('\', '/')
        } | Sort-Object -Unique)
        if ($changedFilesStage.exitCode -ne 0 -or
            @(Compare-Object ($changedFiles | Sort-Object) $expectedFiles).Count -ne 0) {
            throw 'The generated migration changed files outside the reviewed manifest.'
        }
        $result.migration.changedFiles = $changedFiles

        $diffCheck = Invoke-LoggedCommand -Name 'migration-diff-check' `
            -FileName 'git' -Arguments @(
                '-C',
                $candidateRoot,
                'diff',
                '--check',
                '--no-ext-diff',
                '--') `
            -WorkingDirectory $runRoot -LogDirectory $logDirectory `
            -Environment $cloneEnvironment
        Add-Stage -List $stages -Stage $diffCheck
        if ($diffCheck.exitCode -ne 0) {
            throw 'The generated migration does not pass git diff --check.'
        }

        $patchStage = Invoke-LoggedCommand -Name 'migration-patch' `
            -FileName 'git' -Arguments @(
                '-C',
                $candidateRoot,
                'diff',
                '--no-ext-diff',
                '--') `
            -WorkingDirectory $runRoot -LogDirectory $logDirectory `
            -Environment $cloneEnvironment
        Add-Stage -List $stages -Stage $patchStage
        if ($patchStage.exitCode -ne 0 -or
            [string]::IsNullOrWhiteSpace($patchStage.StandardOutput)) {
            throw 'Could not retain the generated migration patch.'
        }
        [IO.File]::WriteAllText(
            (Join-Path $resolvedOutputPath 'migration.patch'),
            $patchStage.StandardOutput,
            [Text.UTF8Encoding]::new($false))

        $candidateBuild = Invoke-RestoreAndBuild -Canary $canary `
            -SourceRoot $candidateRoot -ConfigPath $candidateConfig `
            -PhaseRoot (Join-Path $runRoot 'candidate-state') `
            -PhaseName 'candidate' -LogDirectory $logDirectory -Stages $stages `
            -CandidatePackageId $package.Id `
            -CandidatePackageVersion $package.Version `
            -ExpectedLocalFeed (Split-Path -Parent $package.Path) `
            -BuildEnvironment $buildEnvironment
        if ($candidateBuild.Restore.exitCode -eq -1) {
            $result.classification = 'infrastructure-failure'
            $result.summary = 'The configured candidate build tool could not be started.'
            $exitCode = 5
        }
        elseif ($candidateBuild.Restore.exitCode -ne 0) {
            if (Test-EnvironmentalFailure -Stage $candidateBuild.Restore) {
                $result.classification = 'environmental'
                $result.summary = 'The candidate restore encountered an environmental failure.'
                $exitCode = 3
            }
            else {
                $result.classification = 'modernwpf-regression'
                $result.summary = 'The baseline restores, but the locally mapped ModernWPF candidate does not.'
                $exitCode = 4
            }
        }
        elseif ($candidateBuild.Source.exitCode -ne 0) {
            $result.classification = 'infrastructure-failure'
            $result.summary = 'NuGet did not restore ModernWPF from the downloaded local candidate feed.'
            $exitCode = 5
        }
        elseif ($candidateBuild.Build.exitCode -ne 0) {
            $result.classification = 'modernwpf-regression'
            $result.summary = 'The baseline builds, but the minimally migrated ModernWPF candidate does not.'
            $exitCode = 4
        }
        else {
            $result.classification = 'migrated'
            $result.summary = 'The unchanged baseline and minimally migrated candidate both build.'
            $exitCode = 0
        }
    }
}
catch {
    if ($exitCode -eq -1) {
        $result.classification = 'infrastructure-failure'
        $result.summary = $_.Exception.Message
        $exitCode = 5
    }
    [IO.File]::WriteAllText(
        (Join-Path $logDirectory 'runner-error.log'),
        ($_ | Out-String),
        [Text.UTF8Encoding]::new($false))
}
finally {
    Write-CanaryReport -Result $result -Directory $resolvedOutputPath
}

exit $exitCode

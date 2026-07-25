[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [switch] $NoBuild,

    [switch] $ValidateOnly
)

$ErrorActionPreference = 'Stop'

if ([Threading.Thread]::CurrentThread.ApartmentState -ne 'STA') {
    throw 'The PowerShell sample requires an STA thread. Start PowerShell with the -STA option.'
}

if ($PSVersionTable.PSEdition -eq 'Desktop') {
    $targetFramework = 'net462'
}
elseif ([Environment]::Version.Major -ge 10) {
    $targetFramework = 'net10.0-windows7.0'
}
elseif ([Environment]::Version.Major -ge 8) {
    $targetFramework = 'net8.0-windows7.0'
}
else {
    throw 'PowerShell Core must run on .NET 8 or later.'
}

$projectPath = Join-Path $PSScriptRoot 'PowerShellSample.csproj'
$outputDirectory = Join-Path $PSScriptRoot "bin\$Configuration\$targetFramework"

if (-not $NoBuild) {
    & dotnet build $projectPath --configuration $Configuration --framework $targetFramework --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Building PowerShellSample for $targetFramework failed with exit code $LASTEXITCODE."
    }
}

Add-Type -AssemblyName PresentationFramework

$valueTuplePath = Join-Path $outputDirectory 'System.ValueTuple.dll'
if (Test-Path -LiteralPath $valueTuplePath) {
    [Reflection.Assembly]::LoadFrom($valueTuplePath) | Out-Null
}

foreach ($assemblyName in @('ModernWpf.dll', 'ModernWpf.Controls.dll')) {
    $assemblyPath = Join-Path $outputDirectory $assemblyName
    if (-not (Test-Path -LiteralPath $assemblyPath)) {
        throw "Required assembly not found: $assemblyPath. Run without -NoBuild first."
    }

    [Reflection.Assembly]::LoadFrom($assemblyPath) | Out-Null
}

$xamlPath = Join-Path $PSScriptRoot 'MainWindow.xaml'
$xaml = Get-Content -LiteralPath $xamlPath -Raw
$window = [Windows.Markup.XamlReader]::Parse($xaml)

if ($ValidateOnly) {
    Write-Output "PowerShellSample validation passed ($targetFramework): $($window.GetType().FullName)"
    return
}

$window.ShowDialog() | Out-Null

param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [string[]]$TargetFrameworks = @(
        "net462",
        "net8.0-windows7.0",
        "net10.0-windows7.0"
    ),

    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$resolvedPackagePath = (Resolve-Path $PackagePath).Path
$packageDirectory = Split-Path -Parent $resolvedPackagePath
$packageFileName = [System.IO.Path]::GetFileNameWithoutExtension($resolvedPackagePath)

if ($packageFileName -notmatch "^ModernWpfUI\.(?<version>.+)$") {
    throw "Cannot infer ModernWpfUI package version from '$resolvedPackagePath'."
}

$packageVersion = $Matches.version
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$stamp = "{0}-{1}" -f (Get-Date -Format "yyyyMMdd-HHmmssfff"), $PID
$workRoot = Join-Path $repoRoot "artifacts\package-smoke\$stamp"
$targetFrameworksValue = $TargetFrameworks -join ";"

function Assert-CandidateRestoredFromLocalFeed {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PackagesPath,

        [Parameter(Mandatory = $true)]
        [string]$ExpectedFeed,

        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    $metadataPath = Join-Path $PackagesPath `
        "modernwpfui\$Version\.nupkg.metadata"
    if (-not (Test-Path -LiteralPath $metadataPath -PathType Leaf)) {
        throw "NuGet did not write ModernWpfUI source metadata at '$metadataPath'."
    }

    $metadata = Get-Content -LiteralPath $metadataPath -Raw |
        ConvertFrom-Json -Depth 5
    $expectedSource = [IO.Path]::GetFullPath($ExpectedFeed).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    $actualSource = [IO.Path]::GetFullPath([string]$metadata.source).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    if (-not $actualSource.Equals(
        $expectedSource,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "ModernWpfUI $Version restored from '$actualSource' instead of the candidate feed '$expectedSource'."
    }
}

$checkedInProject = Join-Path `
    $repoRoot `
    "samples\PackageConsumer\ModernWpf.PackageConsumer.csproj"
$checkedInPackages = Join-Path $workRoot "packages"

dotnet restore $checkedInProject `
    -p:ModernWpfPackageVersion=$packageVersion `
    -p:ModernWpfPackageSource=$packageDirectory `
    -p:RestorePackagesPath=$checkedInPackages `
    --no-cache
if ($LASTEXITCODE -ne 0) {
    throw "Checked-in package consumer restore failed."
}
Assert-CandidateRestoredFromLocalFeed `
    -PackagesPath $checkedInPackages `
    -ExpectedFeed $packageDirectory `
    -Version $packageVersion

foreach ($targetFramework in $TargetFrameworks) {
    dotnet build $checkedInProject `
        --configuration $Configuration `
        --framework $targetFramework `
        --no-restore `
        --maxcpucount:1 `
        --warnaserror:MSB3277 `
        -p:ModernWpfPackageVersion=$packageVersion `
        -p:ModernWpfPackageSource=$packageDirectory `
        -p:RestorePackagesPath=$checkedInPackages
    if ($LASTEXITCODE -ne 0) {
        throw "Checked-in package consumer build failed on '$targetFramework'."
    }

    $checkedInExecutable = Join-Path `
        $repoRoot `
        "samples\PackageConsumer\bin\$Configuration\$targetFramework\ModernWpf.PackageConsumer.exe"
    if (-not (Test-Path $checkedInExecutable)) {
        throw "Checked-in package consumer executable was not produced on '$targetFramework'."
    }

    & $checkedInExecutable --smoke-test
    if ($LASTEXITCODE -ne 0) {
        throw "Checked-in package consumer execution failed on '$targetFramework' with exit code $LASTEXITCODE."
    }
}

# The checked-in consumer above exercises the recommended
# FluentControlsResources path. Retain a generated migration consumer for the
# legacy XamlControlsResources entry without advertising it as a new-app sample.
foreach ($resourceType in @("XamlControlsResources")) {
    $projectDirectory = Join-Path $workRoot $resourceType
    New-Item -ItemType Directory -Force $projectDirectory | Out-Null

    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>$targetFrameworksValue</TargetFrameworks>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <LangVersion>12.0</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ModernWpfUI" Version="$packageVersion" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path (Join-Path $projectDirectory "ModernWpf.PackageSmoke.csproj") -Encoding UTF8

    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-modernwpf" value="$packageDirectory" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -Path (Join-Path $projectDirectory "nuget.config") -Encoding UTF8

    @"
<Application x:Class="ModernWpf.PackageSmoke.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:ui="http://schemas.modernwpf.com/2019">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ui:ThemeResources />
        <ui:$resourceType />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
"@ | Set-Content -Path (Join-Path $projectDirectory "App.xaml") -Encoding UTF8

    @"
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ModernWpf.Controls;

namespace ModernWpf.PackageSmoke;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            if (!Resources.Contains("SystemControlBackgroundChromeMediumLowBrush"))
            {
                throw new InvalidOperationException("Expected ModernWpf theme resources were not resolved.");
            }

            var button = new Button { Content = "Styled button" };
            var navigationView = new NavigationView
            {
                Content = new TextBlock { Text = "Packaged control content" }
            };
            navigationView.MenuItems.Add(new NavigationViewItem { Content = "Home" });

            var root = new StackPanel();
            root.Children.Add(button);
            root.Children.Add(navigationView);

            var window = new Window
            {
                Content = root,
                Width = 320,
                Height = 200,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Opacity = 0
            };

            MainWindow = window;
            window.Show();
            window.UpdateLayout();
            Dispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);

            if (button.Template == null || navigationView.Template == null)
            {
                throw new InvalidOperationException("Packaged WPF and ModernWpf control templates were not applied.");
            }

            window.Close();
            Console.WriteLine("PASS: $resourceType");
            Shutdown(0);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            Shutdown(1);
        }
    }
}
"@ | Set-Content -Path (Join-Path $projectDirectory "App.xaml.cs") -Encoding UTF8

    $projectPath = Join-Path $projectDirectory "ModernWpf.PackageSmoke.csproj"
    $nugetConfigPath = Join-Path $projectDirectory "nuget.config"
    $legacyPackages = Join-Path $projectDirectory "packages"

    dotnet restore $projectPath `
        --configfile $nugetConfigPath `
        --packages $legacyPackages `
        --no-cache
    if ($LASTEXITCODE -ne 0) {
        throw "Package smoke restore failed for '$resourceType'."
    }
    Assert-CandidateRestoredFromLocalFeed `
        -PackagesPath $legacyPackages `
        -ExpectedFeed $packageDirectory `
        -Version $packageVersion

    foreach ($targetFramework in $TargetFrameworks) {
        dotnet build $projectPath `
            --configuration $Configuration `
            --framework $targetFramework `
            --no-restore `
            --maxcpucount:1 `
            --warnaserror:MSB3277 `
            -p:RestorePackagesPath=$legacyPackages
        if ($LASTEXITCODE -ne 0) {
            throw "Package smoke build failed for '$resourceType' on '$targetFramework'."
        }

        $executablePath = Join-Path `
            $projectDirectory `
            "bin\$Configuration\$targetFramework\ModernWpf.PackageSmoke.exe"
        if (-not (Test-Path $executablePath)) {
            throw "Package smoke executable was not produced for '$resourceType' on '$targetFramework'."
        }

        & $executablePath
        if ($LASTEXITCODE -ne 0) {
            throw "Package smoke execution failed for '$resourceType' on '$targetFramework' with exit code $LASTEXITCODE."
        }
    }
}

Write-Host "Executed the checked-in FluentControlsResources consumer and legacy XamlControlsResources migration consumer from '$resolvedPackagePath' on: $($TargetFrameworks -join ', ')"

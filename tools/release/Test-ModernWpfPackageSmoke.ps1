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
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$workRoot = Join-Path $repoRoot "artifacts\package-smoke\$stamp"
$projectDirectory = Join-Path $workRoot "ModernWpf.PackageSmoke"

New-Item -ItemType Directory -Force $projectDirectory | Out-Null

$targetFrameworksValue = $TargetFrameworks -join ";"

@"
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
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
        <ui:XamlControlsResources />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
"@ | Set-Content -Path (Join-Path $projectDirectory "App.xaml") -Encoding UTF8

@"
using System.Windows;

namespace ModernWpf.PackageSmoke;

public partial class App : Application
{
}
"@ | Set-Content -Path (Join-Path $projectDirectory "App.xaml.cs") -Encoding UTF8

@"
<Window x:Class="ModernWpf.PackageSmoke.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ui="http://schemas.modernwpf.com/2019"
        Title="ModernWpf Package Smoke"
        Width="360"
        Height="220"
        ui:WindowHelper.UseModernWindowStyle="True">
  <ui:GridEx Padding="12"
             RowSpacing="8"
             Background="{DynamicResource SystemControlBackgroundChromeMediumLowBrush}">
    <ui:GridEx.RowDefinitions>
      <RowDefinition Height="Auto" />
      <RowDefinition Height="Auto" />
      <RowDefinition Height="Auto" />
    </ui:GridEx.RowDefinitions>
    <TextBlock Text="ModernWpf package smoke" />
    <Button Grid.Row="1"
            Content="Styled button" />
    <ui:ContentPresenterEx Grid.Row="2"
                           Content="WinUI presenter surface"
                           Padding="4"
                           CornerRadius="3"
                           Background="Transparent"
                           BackgroundSizing="OuterBorderEdge" />
  </ui:GridEx>
</Window>
"@ | Set-Content -Path (Join-Path $projectDirectory "MainWindow.xaml") -Encoding UTF8

@"
using System.Windows;

namespace ModernWpf.PackageSmoke;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
"@ | Set-Content -Path (Join-Path $projectDirectory "MainWindow.xaml.cs") -Encoding UTF8

$projectPath = Join-Path $projectDirectory "ModernWpf.PackageSmoke.csproj"
$nugetConfigPath = Join-Path $projectDirectory "nuget.config"

dotnet restore $projectPath --configfile $nugetConfigPath
if ($LASTEXITCODE -ne 0) {
    throw "Package smoke restore failed."
}

foreach ($targetFramework in $TargetFrameworks) {
    dotnet build $projectPath `
        --configuration $Configuration `
        --framework $targetFramework `
        --no-restore `
        --maxcpucount:1
    if ($LASTEXITCODE -ne 0) {
        throw "Package smoke build failed for '$targetFramework'."
    }
}

Write-Host "Verified ModernWpfUI package consumer smoke project: $projectDirectory"

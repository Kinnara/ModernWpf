# ModernWpf 1.x Release Readiness

This document defines the release gate for the `1.0.0-preview.1` maintenance line.

## Supported package shape

The `ModernWpfUI` package is expected to ship these target frameworks:

- `net462`
- `net8.0-windows7.0`
- `net10.0-windows7.0`

The package must not ship the retired 0.x assets:

- `net45`
- `netcoreapp3.0`
- `net5.0-windows`

For each supported target framework, the package must contain:

- `ModernWpf.dll`
- `ModernWpf.xml`
- `ModernWpf.Controls.dll`
- `ModernWpf.Controls.xml`

The package metadata must declare `readme.md`, dependency groups for all supported target frameworks, and WPF framework-reference groups for the modern .NET targets. NuGet normalizes the `net462` dependency group to `.NETFramework4.6.2` in the generated nuspec.

## Local release gate

Run these commands from the repository root:

```powershell
dotnet restore ModernWpf.sln
dotnet build ModernWpf.sln --configuration Release --no-restore
dotnet test .\test\ModernWpf.Theme.Tests\ModernWpf.Theme.Tests.csproj --configuration Release --no-build
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --no-build
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --configuration Release --no-build --filter "LayoutCompatibility|LayoutPanel|TemplateParityTests|Repeater"
dotnet pack .\ModernWpf.Controls\ModernWpf.Controls.csproj --configuration Release --no-build
$package = Get-ChildItem .\artifacts\ModernWpfUI.*.nupkg | Sort-Object LastWriteTime -Descending | Select-Object -First 1
.\tools\release\Verify-ModernWpfPackage.ps1 -PackagePath $package.FullName
.\tools\release\Test-ModernWpfPackageSmoke.ps1 -PackagePath $package.FullName
```

Build and test are intentionally serialized. Running solution build and test builds in parallel can create shared `obj` file locks in WPF projects.

## Source-backed WinUI parity surface

The 1.x preview keeps the WinUI-derived ModernWpf control library, not only a theme layer. The layout/template infrastructure has source-backed parity coverage for:

- `BorderEx`
- `ContentPresenterEx`
- `GridEx`
- `StackPanelEx`
- `RelativePanel`
- `CanvasEx`
- `LayoutPanel`
- Repeater layout primitives used by templates and gallery controls

The important supported behaviors are:

- WinUI-compatible XAML property surfaces for template parsing.
- Border/background chrome where WinUI exposes it.
- `BackgroundSizing` handling where the WinUI source exposes it.
- Padding and border participation in measure and arrange.
- Rounded layout clipping and rounded hit testing.
- Dynamic `CornerRadius` child-clip refresh.
- `ContentPresenterEx` use for template presenter slots instead of `ContentControlEx`.

## WPF-adapted gaps

Some WinUI behavior is intentionally adapted rather than copied:

- WPF platform controls remain WPF controls where WinUI owns a different platform primitive.
- WinUI compositor-backed features, animated visual infrastructure, DComp shadows, TestUI process automation, and Axe scans are not represented as package gates.
- WinUI visual baseline and raw pixel parity are tracked through focused gallery visual checks, not the package gate.
- Official WPF Fluent remains an input to stock-control styling, but ModernWpf still owns WinUI-compatible resource dictionaries, element theme islands, and ModernWpf-specific controls.

Broader per-control parity status stays in `docs/winui2-2.8.7-sync.md`; this file defines the release gate for packaging and consumption.

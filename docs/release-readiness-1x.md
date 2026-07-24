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

The main package must not contain PDBs. The companion `.snupkg` must contain
portable SourceLinked PDBs for both assemblies on every target. Package
metadata must declare `readme.md`, the Git repository URL and exact commit,
dependency groups for all supported target frameworks, and WPF
framework-reference groups for the modern .NET targets. NuGet normalizes the
`net462` dependency group to `.NETFramework4.6.2` in the generated nuspec.

## Forward contract gate

`1.0.0-preview.1` is the first forward-compatibility baseline. Compatibility
with 0.9.x is not a release requirement.

The release gate enforces:

- The shipped CLR inventories in `ModernWpf/PublicAPI.Shipped.txt` and
  `ModernWpf.Controls/PublicAPI.Shipped.txt`. New APIs go in the corresponding
  `PublicAPI.Unshipped.txt`; removals and signature changes fail the build.
- NuGet package validation, including strict validation between compatible
  target frameworks. Releases after preview 1 automatically use
  `1.0.0-preview.1` as their package-validation baseline.
- The source-qualified public resource-key inventories in
  `ModernWpf/PublicResourceKeys.Shipped.txt` and
  `ModernWpf/PublicResourceKeys.Unshipped.txt`.
- Package export checks. Public top-level types must be in `ModernWpf`
  namespaces, apart from WPF's compiler-generated
  `XamlGeneratedNamespace.GeneratedInternalTypeHelper`, and the supported
  top-level type set must agree across all three target frameworks.
- XML documentation checks that reject entries for non-public types.

Template parts, visual states, implicit/type resource keys, and unlisted style
or template resources are intentionally outside this contract. See
`docs/public-api-contract-1x.md` for the complete boundary.

## Local release gate

Run these commands from the repository root:

```powershell
dotnet restore ModernWpf.sln
dotnet build ModernWpf.sln --configuration Release --no-restore
dotnet test .\test\ModernWpf.Tools.Tests\ModernWpf.Tools.Tests.csproj --configuration Release --framework net10.0 --no-build --no-restore --logger "trx;LogFileName=tools-net10.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.Theme.Tests\ModernWpf.Theme.Tests.csproj --configuration Release --framework net8.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=theme-net8.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.Theme.Tests\ModernWpf.Theme.Tests.csproj --configuration Release --framework net10.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=theme-net10.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --framework net8.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=gallery-net8.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --framework net10.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=gallery-net10.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --configuration Release --framework net8.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=winui-net8.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpfTestApp\ModernWpfTestApp.csproj --configuration Release --framework net48 --no-build --no-restore --logger "trx;LogFileName=legacy-net48.trx" --results-directory .\artifacts\test-results
.\tools\test\Assert-TestResults.ps1 -ResultsPath .\artifacts\test-results
dotnet pack .\ModernWpf.Controls\ModernWpf.Controls.csproj --configuration Release --no-build --no-restore
$package = Get-ChildItem .\artifacts\ModernWpfUI.*.nupkg | Sort-Object LastWriteTime -Descending | Select-Object -First 1
.\tools\release\Verify-ModernWpfPackage.ps1 -PackagePath $package.FullName
.\tools\release\Test-ModernWpfPackageSmoke.ps1 -PackagePath $package.FullName
```

Build and test are intentionally serialized. Running solution build and test builds in parallel can create shared `obj` file locks in WPF projects.

The WinUI run above is the complete suite. Before merge, run it three
consecutive times from the final clean tip without retries. The retained
legacy suite permits only the documented retirements in
`docs/legacy-test-retirements.md`; every skipped result must include a reason.
Restore treats every moderate-or-higher NuGet audit finding as an error.

The smoke script builds and executes applications from the actual `.nupkg`
using both `FluentControlsResources` and `XamlControlsResources` on all three
targets.

## Publication

`.github/workflows/build.yml` performs validation only. Publication uses the
manually dispatched `.github/workflows/release.yml`, which accepts an existing
annotated `v<Version>` tag on `master`. The tag version must match
`Directory.Build.props`.

The workflow builds and tests the tag once, retains the packages, symbols, TRX
results, release notes, and `SHA256SUMS`, then pauses at the protected
`nuget-production` environment. The publication job verifies the downloaded
artifact, prepares a draft GitHub prerelease, publishes the exact `.nupkg` to
NuGet, and only then publishes the GitHub prerelease.

When adding an explicitly supported resource key, run:

```powershell
.\tools\api-contracts\Update-PublicResourceKeyContract.ps1
```

Review the resulting unshipped entries. Promote them to the shipped resource
manifest only as part of a release baseline update.

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

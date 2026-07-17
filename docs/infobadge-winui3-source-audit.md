# InfoBadge WinUI 3 Source Audit

Date: 2026-07-17

Scope: existing `InfoBadge` and `InfoBadgeTemplateSettings` only. This audit
maps the WPF implementation to local WinUI 3 source and records the WPF
substitutions that remain because the WinUI implementation depends on platform
features that WPF does not expose directly.

## WinUI 3 Source Baseline

- `controls\dev\InfoBadge\InfoBadge.cpp`
- `controls\dev\InfoBadge\InfoBadge.h`
- `controls\dev\InfoBadge\InfoBadge.xaml`
- `controls\dev\InfoBadge\InfoBadge_themeresources.xaml`
- `controls\dev\InfoBadge\InfoBadgeTemplateSettings.cpp`
- `controls\dev\InfoBadge\InfoBadgeTemplateSettings.h`
- `controls\dev\Generated\InfoBadge.properties.cpp`
- `controls\dev\Generated\InfoBadge.properties.h`
- `controls\dev\Generated\InfoBadgeTemplateSettings.properties.cpp`
- `controls\dev\Generated\InfoBadgeTemplateSettings.properties.h`
- `controls\dev\CommonStyles\Common_themeresources_any.xaml`
- `controls\dev\InfoBadge\APITests\InfoBadgeTests.cs`

## ModernWpf Port Surface

- `ModernWpf.Controls\InfoBadge\InfoBadge.cs`
- `ModernWpf.Controls\InfoBadge\InfoBadge.xaml`
- `ModernWpf.Controls\InfoBadge\InfoBadgeTemplateSettings.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\InfoBadge\InfoBadgeApiTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`
- `test\ModernWpf.Gallery.Tests\WpfGallerySourceShapeTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`

## Ported Source Behavior

| WinUI 3 behavior | ModernWpf WPF port |
| --- | --- |
| `InfoBadge` sets its default style key, creates read-only `TemplateSettings`, listens to `SizeChanged`, and ensures the measured width is at least the measured height. | Matched with WPF dependency properties, read-only `TemplateSettings`, `SizeChanged`, and `MeasureOverride`. WPF also seeds the size-derived radius before arranging the template because WPF dispatches `SizeChanged` after the arrange pass; this prevents a square first frame without changing the source radius calculation. |
| `Value` defaults to `-1`; values below `-1` are rejected. `Value >= 0` wins over `IconSource`; `FontIconSource` uses the `FontIcon` state; other icon sources use `Icon`; no value and no icon uses `Dot`. | Matched with WPF validation and source-shaped state selection. Existing icon template settings are intentionally left intact when `Value` is active, as in source. |
| `IconSource` is converted through `SharedHelpers::MakeIconElementFrom` and stored in `TemplateSettings.IconElement`. | Matched with ModernWpf `IconSource.CreateIconElement`, covering symbol, font, path, bitmap, and image icon sources. |
| The source template root is a rounded `Grid` named `RootGrid`; display states set `IconPresenter` / `ValueTextBlock` visibility and margins from InfoBadge theme resources. | Matched with `GridEx`, the local WPF substitute for WinUI `Grid` chrome, and `VisualStateEx.Setters` for WinUI `VisualState.Setters`. |
| `InfoBadge_themeresources.xaml` defines `InfoBadgeForeground`, `InfoBadgeBackground`, min/max size, icon size, padding, and display-state margin resources. | Matched in Light, Dark, and HighContrast theme dictionaries. The Dark `InfoBadgeIconHeight` remains source `Default` value `8`; Light and HighContrast use source value `9`. |
| `InformationalDotInfoBadgeStyle` overrides `Background` to `SystemFillColorSolidNeutralBrush`. | Matched by restoring `SystemFillColorSolidNeutral` / `SystemFillColorSolidNeutralBrush` and using the source background alias. |

## WPF Substitutions

- WinUI `Grid` has built-in `CornerRadius` and rounded background rendering.
  ModernWpf uses `GridEx`, the existing source-backed WPF layout-chrome
  substitute.
- WinUI `ContentPresenter` template content marks accessibility view as raw.
  WPF does not expose that WinUI automation property in XAML, and this port
  keeps `ContentPresenterEx` so the repo-wide WinUI ContentPresenter surface is
  used consistently.
- WinUI throws `hresult_out_of_bounds` from generated property change handling
  when `Value < -1`. WPF represents the same API contract through dependency
  property validation, which surfaces as `ArgumentException` before the invalid
  value is stored.
- WinUI's `SizeChanged` callback updates the radius before its compositor frame.
  WPF raises `SizeChanged` after arranging the template, so the port performs the
  same `ActualHeight / 2` update before `base.ArrangeOverride` and retains the
  source-shaped `SizeChanged` update. This WPF lifecycle bridge is required for
  an InfoBadge first hosted inside a `NavigationViewItem` to render circularly
  on its first frame.

## Installed WinUI Gallery Pixel Gate

The harness now compares the rendered `GallerySample_InfoBadge_InfoBadge`
artifact with the first WinUI Gallery value badge found from its accent pixels.
Both sides are exact `16x16` crops. Missing accent detection fails the required
crop instead of silently falling back to the whole sample.

- Light: `artifacts/visual-checks/20260717-090752-264-10592/report.md`, mean
  delta `4.44`.
- Dark: `artifacts/visual-checks/20260717-090826-509-8256/report.md`, mean
  delta `3.73`.
- Gate: `5.0` in `Get-ReferencePrimaryCropMeanDeltaThreshold`.

Before the lifecycle fix, the NavigationView-hosted control rendered a `16x16`
accent square and scored `40.57`. The current pixel regression renders the
actual NavigationView subtree and proves a transparent corner plus accent-filled
center, in addition to checking the `RootGrid` radius binding.

## Validation

Run after the InfoBadge pixel-parity refresh:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~InfoBadgeApiTests|FullyQualifiedName~GridExFullyRoundedBackgroundClipsCornerPixels" --no-restore
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~InfoBadgeSampleMatchesWinUIGalleryExamples|FullyQualifiedName~GalleryVisualChecksUseRenderedModernPrimaryArtifactsForSplitViewAndPersonPicture|FullyQualifiedName~GalleryVisualChecksEnforceInfoBadgePixelParityThreshold" --no-restore
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net10.0-windows7.0 --filter "FullyQualifiedName~InfoBadgeSampleMatchesWinUIGalleryExamples|FullyQualifiedName~GalleryVisualChecksUseRenderedModernPrimaryArtifactsForSplitViewAndPersonPicture|FullyQualifiedName~GalleryVisualChecksEnforceInfoBadgePixelParityThreshold" --no-restore
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls InfoBadge -Reference InstalledWinUI3Gallery -Theme Light -FailOnDifference
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls InfoBadge -Reference InstalledWinUI3Gallery -Theme Dark -FailOnDifference
git diff --check
```

Latest verified result on 2026-07-17: the product/raster slice passed 9/9;
the Gallery sample/crop/gate slice passed 3/3 on both .NET 8 and .NET 10; exact
Light and Dark installed-Gallery crops passed the strict `5.0` gate at `4.44`
and `3.73`; and `git diff --check` reported only existing CRLF normalization
warnings.

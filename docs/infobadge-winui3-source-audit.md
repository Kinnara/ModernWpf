# InfoBadge WinUI 3 Source Audit

Date: 2026-05-17

Scope: existing `InfoBadge` and `InfoBadgeTemplateSettings` only. This audit
maps the WPF implementation to local WinUI 3 source and records the WPF
substitutions that remain because the WinUI implementation depends on platform
features that WPF does not expose directly.

## WinUI 3 Source Baseline

- `src\controls\dev\InfoBadge\InfoBadge.cpp`
- `src\controls\dev\InfoBadge\InfoBadge.h`
- `src\controls\dev\InfoBadge\InfoBadge.xaml`
- `src\controls\dev\InfoBadge\InfoBadge_themeresources.xaml`
- `src\controls\dev\InfoBadge\InfoBadgeTemplateSettings.cpp`
- `src\controls\dev\InfoBadge\InfoBadgeTemplateSettings.h`
- `src\controls\dev\Generated\InfoBadge.properties.cpp`
- `src\controls\dev\Generated\InfoBadge.properties.h`
- `src\controls\dev\Generated\InfoBadgeTemplateSettings.properties.cpp`
- `src\controls\dev\Generated\InfoBadgeTemplateSettings.properties.h`
- `src\controls\dev\CommonStyles\Common_themeresources_any.xaml`
- `src\controls\dev\InfoBadge\APITests\InfoBadgeTests.cs`

## ModernWpf Port Surface

- `ModernWpf.Controls\InfoBadge\InfoBadge.cs`
- `ModernWpf.Controls\InfoBadge\InfoBadge.xaml`
- `ModernWpf.Controls\InfoBadge\InfoBadgeTemplateSettings.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\InfoBadge\InfoBadgeApiTests.cs`

## Ported Source Behavior

| WinUI 3 behavior | ModernWpf WPF port |
| --- | --- |
| `InfoBadge` sets its default style key, creates read-only `TemplateSettings`, listens to `SizeChanged`, and ensures the measured width is at least the measured height. | Matched with WPF dependency properties, read-only `TemplateSettings`, `SizeChanged`, and `MeasureOverride`. |
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

## Validation

Run after the InfoBadge source port:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~InfoBadge" --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1
git diff --check
```

Latest verified result on 2026-05-17: InfoBadge tests passed 6/6,
`ModernWpf.Controls` built successfully with existing warnings, and
`git diff --check` reported only existing CRLF normalization warnings.

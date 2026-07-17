# BreadcrumbBar WinUI 3 Source Audit

Date: 2026-07-17

WinUI 3 source snapshot:

```text
D:\repos\microsoft-ui-xaml
c70471c511a0168b61dcca13af9556465f26b673
reference/winui3-current
```

## Source Files

- `src\controls\dev\Breadcrumb\BreadcrumbBar.cpp`
- `src\controls\dev\Breadcrumb\BreadcrumbBar.h`
- `src\controls\dev\Breadcrumb\BreadcrumbBar.idl`
- `src\controls\dev\Breadcrumb\BreadcrumbBar.xaml`
- `src\controls\dev\Breadcrumb\BreadcrumbBar_themeresources.xaml`
- `src\controls\dev\Breadcrumb\BreadcrumbBarElementFactory.cpp`
- `src\controls\dev\Breadcrumb\BreadcrumbIterable.cpp`
- `src\controls\dev\Breadcrumb\BreadcrumbIterator.cpp`
- `src\controls\dev\Breadcrumb\BreadcrumbLayout.cpp`
- `src\controls\dev\Breadcrumb\BreadcrumbBarItem.cpp`
- `src\controls\dev\Breadcrumb\BreadcrumbBarItemAutomationPeer.cpp`
- `src\dxaml\xcp\core\text\TextBlock\TextBlock.cpp`
- `src\dxaml\xcp\core\inc\EnterParams.h`

## ModernWpf Port

- `ModernWpf.Controls\BreadcrumbBar\BreadcrumbBar.cs`
- `ModernWpf.Controls\BreadcrumbBar\BreadcrumbBarItem.cs`
- `ModernWpf.Controls\BreadcrumbBar\BreadcrumbElementFactory.cs`
- `ModernWpf.Controls\BreadcrumbBar\BreadcrumbIterable.cs`
- `ModernWpf.Controls\BreadcrumbBar\BreadcrumbLayout.cs`
- `ModernWpf.Controls\BreadcrumbBar\BreadcrumbBar.xaml`
- `ModernWpf.Controls\BreadcrumbBar\BreadcrumbBarAutomationPeer.cs`
- `ModernWpf.Controls\BreadcrumbBar\BreadcrumbBarItemAutomationPeer.cs`
- `ModernWpf\Controls\ContentPresenterEx.cs`
- `ModernWpf\ModernWpfControlsResources.xaml`
- `test\ModernWpf.WinUI.Tests\BreadcrumbBar\BreadcrumbBarApiTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`

## Ported Source Behavior

- Deleted the old guessed `PART_RootPanel` / manual `StackPanel` rebuild path. The default template now uses source-shaped `PART_ItemsRepeater`.
- Ported the source leading ellipsis element model with `BreadcrumbIterable`, which inserts the hidden ellipsis item at repeater index `0` and shifts user item indexes by one.
- Ported the source `BreadcrumbLayout` behavior: all elements are measured, the ellipsis is arranged only when total breadcrumb width exceeds the available width, earlier items are hidden, and visible items are re-indexed for automation.
- Ported the source `BreadcrumbElementFactory` shape for wrapping data items in `BreadcrumbBarItem` containers and forwarding `ItemTemplate` through the item content template.
- Replaced the simplified item template with the source item-type states: `Inline`, `EllipsisDropDown`, `Default`, `DefaultRTL`, `LastItem`, `Ellipsis`, and `EllipsisRTL`.
- Ported the source focus-target setters for `EllipsisDropDown` and `LastItem` states. `FocusVisualHelper.IsTemplateFocusTarget` and `FocusVisualHelper.FocusVisualMargin` represent WinUI `Control.IsTemplateFocusTarget` and `FocusVisualMargin` on WPF template parts.
- Added the source ellipsis flyout path: hidden elements are cloned in reverse order into an `ItemsRepeater`, dropdown item indexes map back to original item indexes, and dropdown clicks route through `ItemClicked`.
- Added source-style breadcrumb resources for chevrons, item foregrounds, current item foregrounds, dropdown item states, flyout presenter chrome, item font weight, and chevron metrics.
- Replaced WPF's machine-dependent message font with `Segoe UI Variable Text, Segoe UI`, matching WinUI's `XamlAutoFontFamily` on current Windows while retaining a downlevel fallback.
- Ported WinUI's default `UseLayoutRounding=true` behavior at the BreadcrumbBar boundary. The current WinUI `TextBlock::MeasureOverride` ceilings unrounded text dimensions to physical pixels; `ContentPresenterEx` now applies the same ceiling to its generated text element instead of WPF's nearest-pixel rounding. The installed Gallery item-width vector now matches exactly: `56,89,61,84,63,63,65,49` (`530x26` total).

## WPF Substitutions

- WinUI `Grid.CornerRadius`, `Grid.BackgroundSizing`, and `ContentPresenter` chrome are represented with `GridEx` and `ContentPresenterEx`.
- WPF XAML does not support WinUI `VisualState.Setters`; the template uses `VisualStateEx.Setters`, matching the repository's WinUI setter substitute.
- WinUI `Flyout` can be a named template resource. WPF resource lookup uses the same key and instantiates the ellipsis repeater in code before showing the flyout.
- WinUI `AccessibilityView`, `Pointer*` routed events, `FocusState`, gamepad navigation, access-key routing, and XamlRoot-specific focus movement do not have direct WPF equivalents. The WPF port uses hit testing, standard WPF focus, mouse capture, and left/right keyboard movement as substitutes.
- Localized WinUI resource strings for ellipsis and localized control type are currently represented by English strings until localized ModernWpf resource packs add this control.
- WPF and WinUI use different DirectWrite integration/rasterization paths. With geometry and colors matched, the remaining live delta is confined to glyph antialiasing; forcing WPF grayscale rendering was measured and rejected because it increased the Dark delta from `2.33` to `3.10`.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter FullyQualifiedName~BreadcrumbBar --no-restore`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~BreadcrumbBar|FullyQualifiedName~TemplateParityTests" --no-restore`
- `dotnet build .\ModernWpf.sln --no-restore`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~BreadcrumbBarApiTests|FullyQualifiedName~ContentPresenterEx" --no-restore` — 25 passed.
- Strict installed-WinUI-Gallery comparisons use a `3.0` primary-crop gate and exact `530x26` crops: Dark `artifacts\visual-checks\20260717-000406-228-6496\report.md` at `2.33`; Light `artifacts\visual-checks\20260717-000424-684-57708\report.md` at `2.53`.

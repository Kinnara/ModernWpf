# ThemeShadow WinUI 3 Parity

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI source evidence:

- `src\controls\dev\CommonStyles\Common_themeresources.xaml`: `ThemeShadowIsUsingDropShadows=True`.
- `src\controls\dev\NumberBox\NumberBox.cpp`: popup root gets `ThemeShadow{}` and `Translation.Z` from `NumberBoxPopupShadowDepth`, default `16`.
- `src\controls\dev\CommandBarFlyout\CommandBarFlyout.cpp`: command bar flyout presenter uses `Translation.Z=32`, and the command bar flyout adds a `ThemeShadow` when opening its own drop shadow.
- `src\controls\dev\NavigationView\NavigationView.cpp`: overlay pane shadow caster gets `ThemeShadow{}` and `Translation.Z` from `PaneOverlayShadowDepth`, default `16`.

ModernWpf files:

- `ModernWpf\Controls\Primitives\ThemeShadowChrome.cs`
- `ModernWpf.Controls\NumberBox\NumberBox.xaml`
- `ModernWpf.Controls\Flyout\FlyoutPresenter.xaml`
- `ModernWpf.Controls\CommandBar\CommandBar.xaml`
- `ModernWpf.Controls\AutoSuggestBox\AutoSuggestBox.xaml`
- `ModernWpf.Controls\ContentDialog\ContentDialog.xaml`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\NumberBox\NumberBoxApiTests.cs`

## Current WPF Renderer

ModernWpf still exposes `ThemeShadowChrome` as the WPF template host because WPF has no compositor `UIElement.Shadow` or `Translation.Z` equivalent. The implementation is no longer the old two-`Border` `BlurEffect` approximation. It now renders a cached alpha-mask bitmap from the child bounds, corner radius, DPI scale, and WinUI-style depth.

The renderer uses three shadow layers derived from depth:

- A broad ambient layer for the soft outer shadow.
- A smaller occlusion layer near the caster.
- A tight contact layer near the bottom edge.

The computed padding is also depth-driven so popup hosts reserve enough room for the shadow. For the source NumberBox depth `16`, the current profile reserves `32,28,32,36`. For depth `32`, it reserves `52,44,52,60`.

## Remaining Gap

This is still a WPF substitution, not a literal WinUI compositor port. The constants are calibrated to be closer to WinUI than `DropShadowEffect`, but they are not yet screenshot-matched against a WinUI reference window. The next shadow parity round should render the same flyout/NumberBox/ContentDialog samples in WinUI and ModernWpf, compare alpha bounds and peak opacity, then tune the profile from that evidence.

## Verification

Focused tests cover the renderer path, the removal of `BlurEffect` border shadow internals, computed depth padding, and the NumberBox popup's source `NumberBoxPopupShadowDepth=16` path.

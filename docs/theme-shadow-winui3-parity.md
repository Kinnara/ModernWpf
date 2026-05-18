# ThemeShadow WinUI 3 Parity

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI source evidence:

- `src\controls\dev\CommonStyles\Common_themeresources.xaml`: `ThemeShadowIsUsingDropShadows=True`.
- `src\controls\dev\NumberBox\NumberBox.cpp`: popup root gets `ThemeShadow{}` and `Translation.Z` from `NumberBoxPopupShadowDepth`, default `16`.
- `src\controls\dev\CommandBarFlyout\CommandBarFlyout.cpp`: command bar flyout presenter uses `Translation.Z=32`, and the command bar flyout adds a `ThemeShadow` when opening its own drop shadow.
- `src\controls\dev\NavigationView\NavigationView.cpp`: overlay pane shadow caster gets `ThemeShadow{}` and `Translation.Z` from `PaneOverlayShadowDepth`, default `16`.
- `src\dxaml\xcp\dxaml\lib\ElevationHelper.cpp`: default elevated flyout/menu surfaces use base `Translation.Z=32`, plus `8` for each nested depth level.
- `src\dxaml\xcp\core\core\elements\Popup.cpp` and `src\dxaml\xcp\components\graphics\ThemeShadow.cpp`: windowed Popup drop-shadow mode reserves tight source insets, `4,1,4,8` for tooltip popups and `10,2,10,18` for other popups.

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

The renderer ports the WinUI `GetDropShadowRecipe` formulas into WPF:

- `Elevation = min(64, Translation.Z / 2)`.
- Low elevations use only the directional shadow; ambient opacity is zero.
- Elevations above `16` add the ambient shadow with the source light/dark opacity split.
- Directional blur is `Elevation`; directional Y offset is `Elevation * 0.5`.
- Ambient blur is `2` at low elevations and `Elevation / 3` at high elevations.

The computed padding follows the source recipe insets so popup hosts reserve source-shaped shadow space. For the source NumberBox depth `16`, the current profile reserves `8,4,8,12`. For depth `32`, it reserves `16,8,16,24`. For depth `64`, it reserves `32,16,32,48`.

WinUI has a second, Popup-owned inset path for windowed popups. The Popup window bounds do not use the full recipe padding; they use manually calibrated tight insets around the visible drop shadow. `ThemeShadowChrome.WindowedPopupInsetMode` ports that distinction for WPF Popup hosts:

- `Default`: use the renderer recipe padding for explicit child shadows such as NumberBox's `PopupContentRoot` at depth `16`.
- `Small`: use WinUI tooltip popup insets, `4,1,4,8`.
- `Medium`: use WinUI non-tooltip popup insets, `10,2,10,18`.

`FlyoutPresenter`, `AutoSuggestBox` suggestions, and `CommandBar` overflow now opt into `Medium` so the WPF popup margin tracks WinUI's windowed Popup gutter instead of the full renderer blur padding. `ContentDialog` remains on default padding because it is not a WPF Popup host in ModernWpf.

## Remaining Gap

This is still a WPF substitution, not a literal WinUI compositor port. The depth, blur, offset, inset, and light/dark opacity constants now come from WinUI source, but the final rasterization uses WPF software alpha masks rather than compositor `DropShadow` visuals. The next shadow parity round should render the same flyout/NumberBox/ContentDialog samples in WinUI and ModernWpf, compare alpha bounds and peak opacity, then adjust only the WPF rasterization details that differ from the source compositor output.

## Verification

Focused tests cover the renderer path, the removal of `BlurEffect` border shadow internals, computed depth padding, source windowed Popup insets, popup-host template opt-ins, and the NumberBox popup's source `NumberBoxPopupShadowDepth=16` path.

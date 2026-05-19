# ThemeShadow WinUI 3 Parity

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

Source coverage matrix: `docs\theme-shadow-source-coverage.md`.

WinUI source evidence:

- `src\controls\dev\CommonStyles\Common_themeresources.xaml`: `ThemeShadowIsUsingDropShadows=True`.
- `src\controls\dev\NumberBox\NumberBox.cpp`: popup root gets `ThemeShadow{}` and `Translation.Z` from `NumberBoxPopupShadowDepth`, default `16`.
- `src\dxaml\xcp\dxaml\lib\CommandBar_Partial.cpp` and `src\controls\dev\CommonStyles\CommandBar_themeresources.xaml`: normal command bar overflow applies elevation to `SecondaryItemsControlShadowWrapper`, keeping `OverflowContentRoot` as the measured popup root around the shadow wrapper.
- `src\controls\dev\CommandBarFlyout\CommandBarFlyout.cpp` and `src\controls\dev\CommandBarFlyout\CommandBarFlyout_themeresources.xaml`: command bar flyout presenter uses `Translation.Z=32`, and the command bar flyout overflow root uses `OuterOverflowContentRootShadow` / `NoOuterOverflowContentRootShadow` states to attach or clear a `ThemeShadow` at `Translation.Z=32`.
- `src\controls\dev\NavigationView\NavigationView.cpp`: overlay pane shadow caster gets `ThemeShadow{}` and `Translation.Z` from `PaneOverlayShadowDepth`, default `16`.
- `src\controls\dev\TeachingTip\TeachingTip.cpp`: `EstablishShadows` applies `ThemeShadow{}` to `ContentRootGrid` when `m_tipShouldHaveShadow=true`, default `m_contentElevation=32`, and expand/contract animations move translation Z between `0.01` and `32`.
- `src\dxaml\xcp\dxaml\lib\AutoSuggestBox_Partial.cpp`: suggestions popup loaded handling applies elevation to the popup part, which maps to source base elevation `32`.
- `src\dxaml\xcp\dxaml\lib\ContentDialog_Partial.cpp`: drop-shadow mode applies elevation to the background element with `baseElevation=128`.
- `src\dxaml\xcp\dxaml\lib\MenuFlyoutPresenter_Partial.cpp`: drop-shadow mode applies elevation to the `MenuFlyoutPresenter` itself at `GetDepth()`, whose default nested depth is `0`, mapping through the source elevation helper to `Translation.Z=32`.
- `src\dxaml\xcp\dxaml\lib\ElevationHelper.cpp`: default elevated flyout/menu surfaces use base `Translation.Z=32`, plus `8` for each nested depth level.
- `src\controls\dev\CommonStyles\MenuFlyout_themeresources.xaml`: the default `MenuFlyoutPresenter` template uses a presenter border with `BackgroundSizing=InnerBorderEdge`, menu-flyout padding, `FlyoutThemeMinWidth`, `MenuFlyoutThemeMinHeight`, and `OverlayCornerRadius`.
- `src\dxaml\xcp\core\core\elements\Popup.cpp` and `src\dxaml\xcp\components\graphics\ThemeShadow.cpp`: windowed Popup drop-shadow mode reserves tight source insets, `4,1,4,8` for tooltip popups and `10,2,10,18` for other popups.

ModernWpf files:

- `ModernWpf\Controls\Primitives\ThemeShadowChrome.cs`
- `ModernWpf.Controls\NumberBox\NumberBox.xaml`
- `ModernWpf.Controls\Flyout\FlyoutPresenter.xaml`
- `ModernWpf.Controls\MenuFlyout\MenuFlyout.xaml`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.xaml`
- `ModernWpf.Controls\CommandBar\CommandBar.xaml`
- `ModernWpf.Controls\AutoSuggestBox\AutoSuggestBox.xaml`
- `ModernWpf.Controls\NavigationView\NavigationView.xaml`
- `ModernWpf.Controls\TeachingTip\TeachingTip.xaml`
- `ModernWpf\ModernWpfControlsResources.xaml`
- `ModernWpf.Controls\ContentDialog\ContentDialog.xaml`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\NumberBox\NumberBoxApiTests.cs`
- `test\ModernWpf.WinUI.Tests\NavigationView\NavigationViewApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TeachingTip\TeachingTipApiTests.cs`
- `test\ModernWpf.WinUI.Tests\CommandBarFlyout\CommandBarFlyoutApiTests.cs`

## Current WPF Renderer

ModernWpf still exposes `ThemeShadowChrome` as the WPF template host because WPF has no compositor `UIElement.Shadow` or `Translation.Z` equivalent. The implementation is no longer the old two-`Border` `BlurEffect` approximation. It now renders a cached alpha-mask bitmap from the child bounds, corner radius, DPI scale, and WinUI-style depth. The composed shadow clears the caster's rounded shape from the center, matching WinUI's `DropShadowVisual` `NineGridBrush isCenterHollow=True` behavior instead of painting shadow alpha underneath transparent content.

The renderer ports the WinUI `GetDropShadowRecipe` formulas into WPF:

- `Elevation = min(64, Translation.Z / 2)`.
- Low elevations use only the directional shadow; ambient opacity is zero.
- Elevations above `16` add the ambient shadow with the source light/dark opacity split.
- Directional blur is `Elevation`; directional Y offset is `Elevation * 0.5`.
- Ambient blur is `2` at low elevations and `Elevation / 3` at high elevations.

The computed padding follows the source recipe insets so `ThemeShadowChrome` reserves source-shaped shadow space in its own WPF layout slot. The child is arranged at the source content offset and the private shadow visual renders from the outer shadow origin; this avoids relying on WPF overflow rendering for the negative left/top shadow extent, which clips differently from WinUI compositor visuals. For the source NumberBox depth `16`, the current profile reserves `8,4,8,12`. For depth `32`, it reserves `16,8,16,24`. For depth `64`, it reserves `32,16,32,48`.

Childless `ThemeShadowChrome` instances remain explicit caster slots. This keeps source-shaped template parts such as NavigationView's `ShadowCaster` compatible with WPF visual states that set `Width` / `Height` directly: the chrome measures and arranges at the requested size, then renders the source shadow outward from that caster instead of subtracting the renderer padding from the caster dimensions.

WinUI has a second, Popup-owned inset path for windowed popups. The Popup window bounds do not use the full recipe padding; they use manually calibrated tight insets around the visible drop shadow. `ThemeShadowChrome.WindowedPopupInsetMode` ports that distinction for WPF Popup hosts:

- `Default`: use the renderer recipe padding for explicit child shadows such as NumberBox's `PopupContentRoot` at depth `16`.
- `Small`: use WinUI tooltip popup insets, `4,1,4,8`.
- `Medium`: use WinUI non-tooltip popup insets, `10,2,10,18`.

`FlyoutPresenter`, `MenuFlyoutPresenter`, `AutoSuggestBox` suggestions, and `CommandBar` overflow now opt into `Medium` so the WPF chrome layout and popup-placement adjustment track WinUI's windowed Popup gutter instead of the full renderer blur padding. The inset is owned by `ThemeShadowChrome` layout; it is not also applied as a WPF popup-child `Margin`, which would double-count the source gutter. `ContentDialog` remains on default padding because it is not a WPF Popup host in ModernWpf.

`FlyoutPresenter` maps WinUI's `FlyoutPresenter::OnApplyTemplate` elevation path by making the WPF template root a `ThemeShadowChrome`. `IsDefaultShadowEnabled` toggles the shared renderer, depth defaults to `32`, the popup inset mode is `Medium`, and the shadow corner radius follows the presenter surface.

`NavigationView` uses the same renderer for the source `ShadowCaster` template part. `PaneOverlayShadowDepth` is now defined as `16` from the WinUI theme resources, and the WPF `ShadowCaster` remains a state-targeted template part while rendering the source depth profile through `ThemeShadowChrome`.

`ContentDialog` opts into `Depth=128`, matching the source drop-shadow-mode call to `ApplyElevationEffect` with `baseElevation=128`. This maps to the clamped maximum WinUI drop-shadow recipe, with renderer padding `64,32,64,96`.

`TeachingTip` now wraps the source `ContentRootGrid` template part in `ContentRootGridShadowChrome`. The WPF chrome uses depth `32`, `Medium` popup insets, and the source content corner radius, matching the WinUI `m_tipShouldHaveShadow=true` and `m_contentElevation=32` default path. The existing WPF scale animation now also animates `ThemeShadowChrome.Depth` from `0.01` to `32` on open and back toward `0.01` on close, representing the source translation-Z animation without a compositor. WinUI's debug/test-only tail shadow experiment remains intentionally unported.

`CommandBar` now keeps the source split between the measured `OverflowContentRoot` popup root and `SecondaryItemsControlShadowWrapper` shadow target. The WPF `SecondaryItemsControlShadowWrapper` is a `ThemeShadowChrome` at depth `32` with `Medium` popup insets, wrapping `SecondaryItemsControl` and preserving the source corner radius. This better represents the WinUI `ApplyElevationEffect(SecondaryItemsControlShadowWrapper)` path while keeping WPF's popup placement and measurement substitute.

`AutoSuggestBox` keeps the source suggestions popup target shape: WinUI applies elevation to the popup part so the immediate popup child receives the shadow. ModernWpf represents that child with `ThemeShadowChrome` around `SuggestionsContainer`, preserving source depth `32`, `Medium` windowed-popup insets, and the popup surface corner radius binding.

`CommandBarFlyout` now follows both source shadow paths. Its `FlyoutPresenter` starts with the default shadow disabled, enables the WPF `ThemeShadowChrome` presenter shadow when opening with primary commands, removes it for flyout close, removes it during secondary command-bar open/close animations, and restores it when those secondary storyboards complete. The flyout command-bar overflow root also ports the source `OuterOverflowContentRootShadow` / `NoOuterOverflowContentRootShadow` states through an `OuterOverflowContentRootShadowChrome` wrapper at depth `32`: no-primary-command flyouts enable the overflow-root shadow, while primary-command flyouts enable it only when the overflow opens downward. Both paths use `Medium` popup insets, matching WinUI's non-tooltip windowed Popup gutter.

`MenuFlyoutPresenter` no longer uses WPF `ContextMenu.HasDropShadow` as the default shadow path. Its WPF template now hosts the presenter chrome in `ThemeShadowChrome` at depth `32` with `Medium` popup insets and keeps `HasDropShadow=False`, matching the source drop-shadow-mode branch that applies elevation to the presenter surface instead of the child element. The template also uses `BorderEx.BackgroundSizing=InnerBorderEdge` and source menu-flyout presenter background, border, padding, min-size, and corner-radius resources while retaining WPF `ContextMenu` item hosting as the platform substitute.

Raw WPF `DropShadowEffect` is now guarded as an official WPF Fluent stock-control exception only. The allowed product-template occurrences are `ModernWpf\Styles\ComboBox.xaml`, `ModernWpf\Styles\DatePicker.xaml`, `ModernWpf\Styles\MenuItem.xaml`, and `ModernWpf\Styles\ToolTip.xaml`, where the styles intentionally track `PresentationFramework.Fluent`. `TemplateParityTests.ProductTemplatesDoNotUseDropShadowEffectOutsideOfficialWpfFluentStockShadows` fails any other product XAML that reintroduces a raw WPF shadow instead of the shared WinUI-style `ThemeShadowChrome` renderer.

## Calibration Probe

`ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics` exposes an internal bitmap-profile probe for the WPF software renderer. It renders the same alpha-mask path used by `DrawShadow` and reports bitmap size, content offset, non-zero alpha bounds, non-zero pixel count, peak alpha, and alpha centroid. The test suite pins depth `16` and `64` profiles so future renderer changes can be compared against stable WPF output before they are compared against a WinUI reference capture.

`LayoutCompatibilityApiTests.ThemeShadowChromeRendersHollowCenteredVisualShadow` also renders an actual `ThemeShadowChrome` instance through WPF `RenderTargetBitmap` and samples the center and outer shadow pixels. This guards the visual-tree integration path used by templates: the transparent caster center must remain white after the hollow-center mask, while the surrounding pixels must still show the rendered shadow.

`LayoutCompatibilityApiTests.ThemeShadowChromeRenderedPixelsTrackWinUIPixelMasters` renders the actual WPF chrome in the same source-shaped `100x100` white canvas used by WinUI's `ThemeShadowDropShadowSystemThemeRedrawRTB` masters. The test verifies the chrome's layout contract first: the WPF shadow host is `82x82` at depth `32`, while the `50x50` caster is arranged at `25,25`, matching the source sample. It then computes rendered darkening bounds, peak, pixel count, and centroid for light and dark themes, catching regressions where WPF layout clipping removes the source left/top shadow extent even if the internal bitmap renderer is still correct.

`LayoutCompatibilityApiTests.ThemeShadowChromeRerendersWhenRequestedThemeChangesLikeWinUISource` keeps the same WPF chrome instance and switches the source canvas from light to dark theme before rendering again. This follows the WinUI `ThemeShadowDropShadowSystemThemeRedrawRTB` path where the same shadowed element is re-rendered after a theme change, and guards `ThemeShadowChrome`'s actual-theme invalidation path.

`LayoutCompatibilityApiTests.ThemeShadowChromePopupInsetsAreNotDoubleAppliedAsChildMargin` opens a WPF `Popup` whose child is a medium-inset `ThemeShadowChrome`. It verifies that the chrome measures to `70x40` for a `50x20` caster, arranges that caster at `10,2`, and leaves `ThemeShadowChrome.Margin` unset. This keeps source windowed-popup insets single-owned by chrome layout while preserving `PopupMargin` as an internal placement-adjustment input.

`LayoutCompatibilityApiTests.ThemeShadowChromeChildlessCasterUsesExplicitSizeAsSourceCaster` renders a childless `ThemeShadowChrome` with explicit `50x50` size in the same source canvas. This guards the NavigationView-style shadow-caster pattern where the template part itself is the caster and must not lose effective width/height to the layout padding added for childful chrome hosts.

The current WPF baseline for an `80x40` DIP content rect with `CornerRadius=8` at `96` DPI is:

| Case | Bitmap | Content offset | Non-zero alpha bounds | Peak alpha | Non-zero pixels | Alpha centroid |
| --- | --- | --- | --- | --- | --- | --- |
| Light depth 16 | `96x56` | `8,4` | `2,2,92,52` | `32` | `1432` | `47.500,42.030` |
| Dark depth 16 | `96x56` | `8,4` | `1,1,94,54` | `60` | `1672` | `47.500,42.069` |
| Light depth 64 | `144x104` | `32,16` | `8,8,128,88` | `65` | `6832` | `71.500,60.108` |

## WinUI Master Geometry Check

The WinUI source tree includes MockDComp visual-tree masters for `ThemeShadowTests`, which give source-side geometry for the compositor shadow visual even without a live WinUI screenshot capture. `LayoutCompatibilityApiTests.ThemeShadowRendererMatchesWinUIMockDCompMasterGeometry` pins the matching WPF renderer geometry against those masters:

| WinUI master | Source scenario | WinUI `DropShadowVisual` sprite | WinUI sprite offset | ModernWpf renderer geometry |
| --- | --- | --- | --- | --- |
| `Foundation_Graphics_ThemeShadowTests_ThemeShadowBasicDropShadow.master.xml` | `100x100` caster, `Translation.Z=32` | `132x132` | `-16,-8` | `132x132` bitmap, content offset `16,8` |
| `Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowDynamicCornerRadius.4_CR.master.xml` | same caster with `RadiusX=4`, `RadiusY=4` | `132x132` | `-16,-8` | same outer bitmap and offset; WPF uses a direct rounded mask instead of WinUI's adjusted `NineGridBrush` insets |
| `Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowWindowedPopup.Shadow.master.xml` | `50x50` windowed popup caster, `Translation.Z=32`, `200%` scale | `82x82` | `-16,-8` | `82x82` bitmap, content offset `16,8`; source medium popup insets produce `70x70` DIP popup bounds |
| `Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowWindowedPopup125.Shadow.master.xml` | `50.4x50.4` windowed popup caster, `Translation.Z=32`, `125%` scale | `82.4x82.4` | `-16,-8` | `103x103` bitmap at 125% scale, content offset `20,10`, content size `63x63` pixels |

WinUI also has pixel masters for `ThemeShadowDropShadowSystemThemeRedrawRTB`, rendering a `50x50` rounded caster at `Canvas.Left=25`, `Canvas.Top=25`, and `Translation.Z=32` into a `100x100` white `RenderTargetBitmap`. The WPF renderer is not pixel-identical, but after clearing the hollow center it tracks the source output closely enough to use as a bounded regression check:

| Case | WinUI shadow bounds | WPF shadow bounds when placed in source canvas | WinUI peak darkening | WPF peak alpha | WinUI shadow pixels | WPF shadow pixels |
| --- | --- | --- | --- | --- | --- | --- |
| Light | `14,22,72,72` | `13,21,74,74` | `31` | `33` | `2330` | `2564` |
| Dark | `14,21,72,74` | `12,20,76,76` | `58` | `61` | `2542` | `2936` |

## Remaining Gap

This is still a WPF substitution, not a literal WinUI compositor port. The depth, blur, offset, inset, hollow-center behavior, and light/dark opacity constants now come from WinUI source and WinUI masters, but the final rasterization uses WPF software alpha masks rather than compositor `DropShadow` visuals. The next shadow parity round should render the same flyout/NumberBox/ContentDialog/NavigationView samples in WinUI and ModernWpf, compare full screenshots, then adjust only the WPF rasterization details that differ from the source compositor output.

## Verification

Focused tests cover the renderer path, rendered alpha-profile calibration metrics, actual-theme redraw behavior, the removal of `BlurEffect` border shadow internals, computed depth padding, childless explicit-size caster behavior, source windowed Popup insets, popup-host template opt-ins, FlyoutPresenter's source child-elevation shadow path, the NumberBox popup's source `NumberBoxPopupShadowDepth=16` path, NavigationView's source `PaneOverlayShadowDepth=16` shadow caster, ContentDialog's source `baseElevation=128` shadow depth, TeachingTip's source `ContentRootGrid` shadow depth, CommandBar's source `SecondaryItemsControlShadowWrapper` overflow target, AutoSuggestBox's source popup-child suggestions shadow target, CommandBarFlyout's source presenter-shadow toggle lifecycle and overflow-root shadow states, and MenuFlyoutPresenter's source-shaped `ThemeShadowChrome` presenter shadow path.

`TemplateParityTests.ThemeShadowSourceCoverageAuditCoversKnownWinUIShadowInputs` keeps the source coverage matrix aligned with the known WinUI shadow inputs, shared renderer recipe sources, official WPF Fluent stock exceptions, and documented WPF substitutions.

# Official WPF Fluent Backport Sync

This file records the audited batches where the ModernWpf backport is compared
with the official WPF Fluent resources.

## 2026-05-12 Batch 1

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent`
- `Resources\Fonts.xaml`
- `Resources\Variables.xaml`
- `Themes\Fluent.xaml`

### Synced Values

| Resource key | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `SymbolThemeFontFamily` | `Segoe Fluent Icons, Segoe MDL2 Assets` | `Segoe Fluent Icons, Segoe MDL2 Assets` | Align shared icon glyph rendering with the platform Fluent theme while retaining MDL2 fallback for existing glyph contracts. |
| `ControlCornerRadius` | `4,4,4,4` | `4` | Already equivalent for WPF corner radius use. |
| `OverlayCornerRadius` | `8,8,8,8` | `8` | Already equivalent for WPF corner radius use. |
| `ControlContentThemeFontSize` | `14` | `14` | Already aligned. |
| `ContentControlFontSize` | `14` | `14` | Already aligned. |
| `TextControlThemeMinHeight` | `32` | `32` | Already aligned. |
| `TextControlThemePadding` | `10,5,6,6` | `10,5,6,6` | Already aligned. |

### Intentional Differences

| Resource key | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| `TextControlThemeMinWidth` | `0` | `64` | ModernWpf inherited the WinUI-compatible text-control width contract; changing it would be a visible backport layout break. |
| `TreeViewItemPresenterMargin` | `0` | `4,2` | The default backport TreeView follows existing ModernWpf/WinUI-derived layout expectations. Compact mode already provides the official-style `0` value. |
| `TreeViewItemPresenterPadding` | `0` | `0,3,0,5` | Same as TreeView margin; keep default compatibility and use compact resources for denser layout. |
| `TreeViewItemMultiSelectCheckBoxMinHeight` | `24` | `28` | Retained for default ModernWpf/WinUI backport behavior; compact resources already use `24`. |
| Stock WPF control templates | Official WPF Fluent templates | ModernWpf backport templates on `net462` and `net8.0-windows7.0` | Template replacement is intentionally batched. `net10.0-windows7.0` uses official templates through `FluentControlsResources`. |

### Test Evidence

- `ModernWpf.Theme.Tests` verifies that the recommended resource entry resolves
  `SymbolThemeFontFamily` to the official WPF Fluent fallback chain.
- `ModernWpf.Gallery.Tests` verifies that Gallery uses
  `FluentControlsResources`, resolves the synced icon font, and uses the
  official platform Fluent dictionary on `net10.0-windows7.0`.

## 2026-05-18 Batch 2

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `SliderThumbStyle` | 20px thumb with `Normal`, `MouseOver`, and `Pressed` WPF visual states | Same WPF state names and official thumb size | `Slider` is a stock WPF control, so official WPF Fluent is the primary source. |
| Horizontal / vertical Slider templates | WPF `ControlTemplate.Triggers` for `TickPlacement`, `IsMouseOver`, and `IsSelectionRangeEnabled` | Same trigger model, retained under `SliderHorizontal` / `SliderVertical` resource keys | Replaces the WinUI `VisualStateEx` helper port with the platform WPF Fluent adaptation. |
| `DefaultSliderStyle` | Orientation triggers choose horizontal/vertical templates and set minimum extent | Same behavior under the existing `DefaultSliderStyle` key | Keeps ModernWpf resource lookup stable while aligning the template behavior. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| Slider resource aliases | Direct official resources, for example `ControlElevationBorderBrush` | Existing ModernWpf aliases, for example `SliderThumbBorderBrush` | Existing aliases already map to the same Fluent concepts and remain part of the ModernWpf resource surface. |
| Slider template resource names | `HorizontalSliderTemplate` / `VerticalSliderTemplate` | `SliderHorizontal` / `SliderVertical` | Preserve existing ModernWpf resource keys while copying the official template shape. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\SliderVisualStateTests.cs` covers the official WPF Fluent Slider trigger shape, WPF thumb visual-state names, tick placement, selection range visibility, and Slider metrics.

## 2026-05-18 Batch 3

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultButtonStyle` | `ButtonBase` style with `ContentBorder`, WPF `ContentPresenter`, and `ControlTemplate.Triggers` for pointer-over, disabled, and pressed states | Same structure under the existing resource key, with the older-target `ControlHelper.CornerRadius` substitution | `Button` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `Button` style | Based on `DefaultButtonStyle` | Same | Matches official WPF Fluent resource shape while keeping the existing ModernWpf resource key. |
| `AccentButtonStyle` | Self-contained `Button` style with `ContentBorder`, WPF `ContentPresenter`, and WPF trigger chrome | Same structure under the existing resource key, with the older-target `ControlHelper.CornerRadius` substitution | Removes the previous WinUI `VisualStateEx` stock-button template path. |
| `SubtleButtonStyle` | No official WPF Fluent equivalent | ModernWpf-specific style using the same WPF trigger/template structure and existing WinUI-derived resource aliases | Keep the public ModernWpf style while aligning the template mechanism with official WPF Fluent. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| Button corner radius property | `Border.CornerRadius` on `ButtonBase` | `primitives:ControlHelper.CornerRadius` | Older ModernWpf targets do not expose the official source property on `ButtonBase`; this preserves the backport radius bridge. |
| Button focus style resource | `DefaultControlFocusVisualStyle` | `{x:Static SystemParameters.FocusVisualStyleKey}` plus `FocusVisualHelper` settings | ModernWpf keeps its existing focus visual bridge across supported target frameworks. |
| Button brush resources | Direct official resources | Existing ModernWpf aliases such as `ButtonBackground` and `AccentButtonBorderBrush` | Existing aliases map to the same Fluent concepts and remain part of the public resource surface. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\ButtonVisualStateTests.cs` covers the official WPF Fluent Button trigger shape, `ButtonBase` default style target, self-contained accent style, disabled trigger resource application, and ModernWpf-specific subtle button trigger shape.
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` covers the Button and AccentButton WPF presenter slots plus the retained `ControlHelper.CornerRadius` substitution.

## 2026-05-18 Batch 4

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultRepeatButtonStyle` | `RepeatButton` style with `ContentBorder`, WPF `ContentPresenter`, and `ControlTemplate.Triggers` for disabled, pointer-over, and pressed states | Same structure under the existing resource key, with the older-target `ControlHelper.CornerRadius` substitution | `RepeatButton` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `RepeatButton` style | Based on `DefaultRepeatButtonStyle` | Same | Matches official WPF Fluent resource shape while keeping the existing ModernWpf resource key. |
| `RepeatButtonPadding` / `RepeatButtonBorderThemeThickness` | Control-specific padding and border thickness resources | Same control-specific keys used by `DefaultRepeatButtonStyle` | Stops borrowing button metrics for the stock repeat button template. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| RepeatButton corner radius property | `Border.CornerRadius` on `RepeatButton` | `primitives:ControlHelper.CornerRadius` | Older ModernWpf targets do not expose the official source property on `RepeatButton`; this preserves the backport radius bridge. |
| RepeatButton focus style resource | `DefaultControlFocusVisualStyle` | `{x:Static SystemParameters.FocusVisualStyleKey}` plus `FocusVisualHelper` settings | ModernWpf keeps its existing focus visual bridge across supported target frameworks. |
| RepeatButton brush resources | Direct official resources | Existing ModernWpf aliases such as `RepeatButtonBackground` and `RepeatButtonBorderBrush` | Existing aliases map to the same Fluent concepts and remain part of the public resource surface. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\RepeatButtonVisualStateTests.cs` covers the official WPF Fluent RepeatButton trigger shape, WPF presenter slot, control-specific padding and border resources, and disabled trigger resource application.
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` covers the RepeatButton WPF presenter slot plus the retained `ControlHelper.CornerRadius` substitution.

## 2026-05-18 Batch 5

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultToggleButtonStyle` | `ToggleButton` style with `ContentBorder`, WPF `ContentPresenter`, and `MultiTrigger` entries for unchecked, checked, disabled, pointer-over, and pressed states | Same structure under the existing resource key, with the older-target `ControlHelper.CornerRadius` substitution | `ToggleButton` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `ToggleButton` style | Based on `DefaultToggleButtonStyle` | Same | Matches official WPF Fluent resource shape while keeping the existing ModernWpf resource key. |
| `ToggleButtonPadding` / `ToggleButtonBorderThemeThickness` | Control-specific padding and border thickness resources | Same control-specific keys used by `DefaultToggleButtonStyle` | Stops borrowing button metrics for the stock toggle button template. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| ToggleButton corner radius property | `Border.CornerRadius` on `ToggleButton` | `primitives:ControlHelper.CornerRadius` | Older ModernWpf targets do not expose the official source property on `ToggleButton`; this preserves the backport radius bridge. |
| ToggleButton focus style resource | `DefaultControlFocusVisualStyle` | `{x:Static SystemParameters.FocusVisualStyleKey}` plus `FocusVisualHelper` settings | ModernWpf keeps its existing focus visual bridge across supported target frameworks. |
| ToggleButton brush resources | Direct official resources | Existing ModernWpf aliases such as `ToggleButtonBackground` and `ToggleButtonBorderBrush` | Existing aliases map to the same Fluent concepts and remain part of the public resource surface. |
| `ToggleButtonForegroundCheckedDisabled` resource type | Brush-valued resource used by `TextElement.Foreground` | Light/Dark aliases now resolve to `TextOnAccentFillColorDisabledBrush` | The official WPF trigger setter targets `TextElement.Foreground`, which requires a brush, not the underlying color token. |
| Indeterminate stock ToggleButton visuals | No separate official WPF Fluent branch | `IsChecked=null` falls back through the same official-style trigger matrix | This removes the earlier WinUI indeterminate visual-state mapping for the stock WPF control. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\ToggleButtonVisualStateTests.cs` covers the official WPF Fluent ToggleButton trigger shape, WPF presenter slot, control-specific padding and border resources, checked/disabled resource application, and indeterminate fallback behavior.
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` covers the ToggleButton WPF presenter slot plus the retained `ControlHelper.CornerRadius` substitution.

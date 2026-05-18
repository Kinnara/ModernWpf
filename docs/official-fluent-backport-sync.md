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

## 2026-05-18 Batch 6

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultRadioButtonStyle` | `RadioButton` style with `RootBorder`, `RootGrid`, WPF `ContentPresenter`, `Normal` / `MouseOver` / `Pressed` visual states, and native trigger entries for checked, disabled, pointer-over, pressed, and RTL behavior | Same structure under the existing resource key, with the older-target `ControlHelper.CornerRadius` substitution | `RadioButton` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `RadioButton` style | Based on `DefaultRadioButtonStyle` | Same | Matches official WPF Fluent resource shape while keeping the existing ModernWpf resource key. |
| `RadioButtonPadding` / `RadioButtonStrokeThickness` / checked outer-ellipse resources | Control-specific metrics and official checked outer-ellipse trigger resource keys | Same metrics and trigger resource keys are present for the stock template | Removes the previous WinUI `VisualStateEx` stock RadioButton template path. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| RadioButton corner radius property | `Border.CornerRadius` on `RadioButton` | `primitives:ControlHelper.CornerRadius` | Older ModernWpf targets do not expose the official source property on `RadioButton`; this preserves the backport radius bridge. |
| RadioButton focus style resource | `DefaultControlFocusVisualStyle` | `{x:Static SystemParameters.FocusVisualStyleKey}` plus `FocusVisualHelper` settings | ModernWpf keeps its existing focus visual bridge across supported target frameworks. |
| RadioButton brush resources | Direct official resources | Existing ModernWpf aliases such as `RadioButtonBackground` and `RadioButtonOuterEllipseStroke` | Existing aliases map to the same Fluent concepts and remain part of the public resource surface. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\RadioButtonVisualStateTests.cs` covers the official WPF Fluent RadioButton visual-state names, WPF trigger shape, WPF presenter slot, checked/disabled behavior, and newly added checked outer-ellipse resource aliases.
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` covers the RadioButton WPF presenter slot plus the retained `ControlHelper.CornerRadius` substitution.

## 2026-05-18 Batch 7

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\CheckBox.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultCheckBoxStyle` | `CheckBox` style with `RootBorder`, `RootGrid`, `ControlBorderIconPresenter`, `StrokeBorder`, `ControlIcon`, WPF `ContentPresenter`, and WPF trigger entries for checked, indeterminate, disabled, pointer-over, pressed, and empty-content behavior | Same structure under the existing resource key, with the older-target `ControlHelper.CornerRadius` substitution | `CheckBox` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `CheckBox` style | Based on `DefaultCheckBoxStyle` | Same | Matches official WPF Fluent resource shape while keeping the existing ModernWpf resource key. |
| `CheckBoxBorderThickness` / `CheckBoxIconSize` / `CheckBoxCheckedGlyph` / `CheckBoxIndeterminateGlyph` | Official WPF Fluent metrics and text glyphs | Same metrics and glyph values | Replaces the previous WinUI fallback geometry glyph path for stock CheckBox. |
| `CheckBoxCheckGlyphForeground`, `CheckBoxCheckGlyphForegroundPressed`, `CheckBoxCheckGlyphForegroundDisabled` | Official theme glyph brush aliases | Same aliases across Light, Dark, and HighContrast | Required by the official WPF Fluent template shape. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| CheckBox corner radius property | `Border.CornerRadius` on `CheckBox` | `primitives:ControlHelper.CornerRadius` | Older ModernWpf targets do not expose the official source property on `CheckBox`; this preserves the backport radius bridge. |
| CheckBox focus style resource | `DefaultControlFocusVisualStyle` | `{x:Static SystemParameters.FocusVisualStyleKey}` plus `FocusVisualHelper` settings | ModernWpf keeps its existing focus visual bridge across supported target frameworks. |
| CheckBox brush resources | Direct official resources | Existing ModernWpf aliases such as `CheckBoxBackgroundUnchecked` and `CheckBoxCheckBackgroundFillChecked` | Existing aliases map to the same Fluent concepts and remain part of the public resource surface. |
| `DataGridCheckBoxStyle` / `DataGridReadOnlyCheckBoxStyle` | No direct official WPF Fluent equivalents in the CheckBox style file | Retained as ModernWpf/WPF-specific styles based on `DefaultCheckBoxStyle` | These styles serve WPF DataGrid usage and remain outside the stock official template copy. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\CheckBoxVisualStateTests.cs` covers the official WPF Fluent CheckBox trigger shape, WPF presenter slot, official metrics/glyph resources, checked/indeterminate/disabled behavior, and newly added glyph brush aliases.

## 2026-05-18 Batch 8

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultRichTextBoxStyle` | `RichTextBox` style with `ContentBorder`, `PART_ContentHost`, official text-control setters, and WPF triggers for pointer-over, focused, and disabled states | Same structure under the existing resource key, with ModernWpf context-menu, validation, and corner-radius substitutions | `RichTextBox` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `RichTextBox` style | Based on `DefaultRichTextBoxStyle` | Same | Matches official WPF Fluent resource shape while keeping the existing ModernWpf resource key. |
| `RichTextBoxAccentBorderThemeThickness` | `0,0,0,1` | Same | Restores the official RichTextBox-specific accent border resource. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| RichTextBox context menu | `DefaultControlContextMenu` | `TextControlContextMenu` plus `TextContextMenu.UsingTextContextMenu=True` | ModernWpf keeps its existing text-control context-menu integration. |
| RichTextBox corner radius property | `Border.CornerRadius` on `RichTextBox` | `primitives:ControlHelper.CornerRadius` | Older ModernWpf targets do not expose the official source property on `RichTextBox`; this preserves the backport radius bridge. |
| RichTextBox validation chrome | No ModernWpf validation adorner bridge | `Validation.ErrorTemplate` and `ValidationHelper.IsTemplateValidationAdornerSite` retained | Existing ModernWpf validation adorners still need a template-owned chrome site. |
| `RichEditBoxTopHeaderMargin` | No official WPF Fluent equivalent | Retained as an unused public alias | Prevents unnecessary resource-surface churn while the official template no longer consumes header-presenter resources. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\RichTextBoxVisualStateTests.cs` covers the official WPF Fluent RichTextBox trigger shape, WPF template parts, official setters, deleted header/placeholder/description slots, deleted `ContentPresenterEx` slot, disabled trigger resource application, and retained ModernWpf substitutions.

## 2026-05-18 Batch 9

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ToolTip.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultToolTipStyle` | `ToolTip` style with a WPF `Border`, plain WPF `ContentPresenter`, 4px corner radius, WPF `DropShadowEffect`, system status font resources, and `WrapWithOverflow` TextBlock content wrapping | Same structure under the existing resource key | `ToolTip` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `ToolTip` style | Based on `DefaultToolTipStyle` | Same | Matches official WPF Fluent resource shape while keeping the existing ModernWpf resource key. |
| Stock resource merge | Included by the official Fluent theme dictionaries | `ModernWpf\StockControlsResources.xaml` now merges `Styles\ToolTip.xaml` | Makes the official-backed style active through `XamlControlsResources`. |
| `ToolTipForeground` / `ToolTipBackground` | Official theme aliases for tooltip foreground/background brushes | Same aliases across Light, Dark, and HighContrast | Required by the official WPF Fluent resource surface. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| ToolTip theme aliases | Dedicated brush resources in official WPF Fluent | `m:StaticResource` aliases to ModernWpf's existing Fluent brush tokens | Keeps ModernWpf's existing theme-resource alias model while exposing the official keys. |
| `ToolTipContentThemeFontSize` | No active official template use | Retained as an unused public alias | Prevents unnecessary resource-surface churn while the official template uses system status font size. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\ToolTipVisualStateTests.cs` covers the official WPF Fluent ToolTip style setters, WPF `Border` / `ContentPresenter` shape, drop shadow, TextBlock wrapping style, removal of `ContentPresenterEx` / `ThemeShadowChrome`, and official theme aliases.
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` covers the official WPF presenter shape for ToolTip in the broader layout compatibility suite.

## 2026-05-18 Batch 10

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Label.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultLabelStyle` | Setter-only `Label` style with `Padding=0,0,0,4`, `Focusable=False`, `Foreground={DynamicResource LabelForeground}`, and `SnapsToDevicePixels=True` | Same setter-only style under the existing resource key | `Label` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `Label` style | Based on `DefaultLabelStyle` | Same | Matches official WPF Fluent resource shape while keeping the existing ModernWpf resource key. |
| `LabelForeground` | Official theme alias for the label foreground brush | Same alias across Light, Dark, and HighContrast | Required by the official WPF Fluent style surface. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| `LabelForeground` theme alias | Dedicated brush resources in official WPF Fluent | `m:StaticResource` aliases to ModernWpf's existing Fluent brush tokens | Keeps ModernWpf's existing theme-resource alias model while exposing the official key. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\LabelVisualStateTests.cs` covers the official WPF Fluent Label setter-only style shape, runtime values, removal of the old `ContentPresenterEx` template, and official theme alias.
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` covers the official WPF style surface for Label in the broader layout compatibility suite.

## 2026-05-18 Batch 11

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\GroupBox.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultGroupBoxStyle` | `GroupBox` style with `OverridesDefaultStyle=True`, resource-backed `BorderThickness`, `Background`, `BorderBrush`, `Padding`, and a WPF `Border` / `Grid` template | Same structure under the existing resource key | `GroupBox` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `GroupBox` style | Based on `DefaultGroupBoxStyle` | Same | Matches official WPF Fluent resource shape while keeping the existing ModernWpf resource key. |
| `GroupBoxPadding` / `GroupBoxHeaderFontSize` / `GroupBoxHeaderMargin` / `GroupBoxBorderThickness` | Official GroupBox metrics: `0,0,0,16`, `20`, `0,4,0,12`, and `0` | Same | Replaces the earlier ModernWpf spacing guesses. |
| `GroupBoxBackground` / `GroupBoxBorderBrush` / `GroupBoxHeaderForeground` | Official theme aliases for GroupBox chrome and header foreground | Same aliases across Light, Dark, and HighContrast | Required by the official WPF Fluent template shape. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| `system:Double` namespace assembly | `System.Runtime` | `mscorlib` | Keeps the XAML resource compatible with ModernWpf's older target frameworks. |
| GroupBox theme aliases | Dedicated brush resources in official WPF Fluent | `m:StaticResource` aliases to ModernWpf's existing Fluent brush tokens | Keeps ModernWpf's existing theme-resource alias model while exposing the official keys. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\GroupBoxVisualStateTests.cs` covers the official WPF Fluent GroupBox style setters, runtime values, WPF `Border` / plain `ContentPresenter` template shape, header text attached properties, access-key recognition, removal of `ContentPresenterEx`, and official theme aliases.
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` covers the official WPF presenter shape for GroupBox in the broader layout compatibility suite.

## 2026-05-18 Batch 12

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\StatusBar.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\StatusBarItem.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultStatusBarStyle` / implicit `StatusBar` style | Official implicit `StatusBar` style with foreground/background color resources, `ControlElevationBorderBrush`, `BorderThickness=1`, `Padding=12`, and `Margin=0` | Same setter surface under the existing `DefaultStatusBarStyle` key with the implicit style based on it | `StatusBar` is a stock WPF control, so official WPF Fluent is the primary source. |
| `StatusBar.SeparatorStyleKey` | Separator style with transparent background, `ControlElevationBorderBrush`, `Margin=6,0`, `BorderThickness=1,1,0,0`, and override-default-style `Border` template | Same structure | Replaces the old thin foreground-brush separator guess. |
| `DefaultStatusBarItemStyle` | `StatusBarItem` style with `Padding={DynamicResource StatusBarItemPadding}`, transparent background alias, left/center content alignment, WPF `ContentPresenter`, and disabled foreground/background triggers | Same structure under the existing resource key | Removes the previous `ContentPresenterEx` template path for the stock WPF control. |
| `StatusBarItemPadding` | `4` | Same | Restores the official item padding metric. |
| `StatusBarItemBackground` / `StatusBarItemBackgroundDisabled` / `StatusBarItemForegroundDisabled` | Official theme aliases for StatusBarItem chrome and disabled text | Same aliases across Light, Dark, and HighContrast | Required by the official WPF Fluent template shape. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| StatusBar style key | Implicit-only style | Existing `DefaultStatusBarStyle` key plus implicit style based on it | Keeps ModernWpf resource lookup stable while using the official setter surface. |
| StatusBarItem theme aliases | Dedicated brush resources in official WPF Fluent | `m:StaticResource` aliases to ModernWpf's existing Fluent brush tokens | Keeps ModernWpf's existing theme-resource alias model while exposing the official keys. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\StatusBarVisualStateTests.cs` covers the official WPF Fluent StatusBar setter shape, separator style, StatusBarItem setters, runtime values, disabled trigger resources, plain WPF presenter slot, deletion of `ContentPresenterEx`, and official theme aliases.
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` covers the official WPF presenter shape for StatusBarItem in the broader layout compatibility suite.

## 2026-05-18 Batch 13

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ResizeGrip.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultResizeGripStyle` | `ResizeGrip` style with `OverridesDefaultStyle=True`, 12px min size resources, transparent background, and a glyph `TextBlock` template | Same structure under the official resource key | `ResizeGrip` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `ResizeGrip` style | Based on `DefaultResizeGripStyle` | Same | Matches official WPF Fluent resource shape. |
| `ResizeGripMinHeight` / `ResizeGripMinWidth` / `ResizeGripIconSize` / `ResizeGripIconGlyph` | Official values: `12`, `12`, `8.0`, and `\uF169` | Same | Replaces the earlier tiled geometry template and scrollbar-size metrics. |
| `ResizeGripForeground` | Official theme alias for the glyph foreground brush | Same alias across Light, Dark, and HighContrast | Required by the official WPF Fluent template shape. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| `sys` namespace assembly | `System.Runtime` | `mscorlib` | Keeps the XAML resource compatible with ModernWpf's older target frameworks. |
| `ResizeGripForeground` theme alias | Dedicated brush resource in official WPF Fluent | `m:StaticResource` aliases to ModernWpf's existing Fluent brush tokens | Keeps ModernWpf's existing theme-resource alias model while exposing the official key. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\ResizeGripVisualStateTests.cs` covers the official WPF Fluent ResizeGrip setter shape, runtime metric/glyph values, glyph `TextBlock` template, deletion of the old `Path` / `DrawingBrush` template, and official theme aliases.

## 2026-05-18 Batch 14

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\GridSplitter.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultGridSplitterStyle` | `GridSplitter` style with stretch alignment, official min-size/padding resources, official foreground/background aliases, a WPF `Border` root, `PART_Thumb` rectangle, and WPF triggers for pointer-over, dragging, disabled, and `Cursor=SizeNS` | Same structure under the existing resource key | `GridSplitter` is a stock WPF control, so official WPF Fluent is the primary source. |
| Implicit `GridSplitter` style | Based on `DefaultGridSplitterStyle` | Same | Matches official WPF Fluent resource shape. |
| `GridsplitterThumbHeight` / `GridsplitterThumbWidth` / `GridsplitterThumbRadius` / `GridsplitterMinHeight` / `GridsplitterMinWidth` / `GridsplitterPadding` | Official values: `24`, `4`, `2`, `8`, `8`, and `4` | Same | Replaces the earlier flat bar and preview-style guess. |
| `GridsplitterBackground` / `GridsplitterBackgroundPointerOver` / `GridsplitterBackgroundPressed` / `GridsplitterBackgroundDisabled` / `GridsplitterForeground` | Official theme aliases for GridSplitter chrome and thumb foreground | Same aliases across Light, Dark, and HighContrast | Required by the official WPF Fluent template shape. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| `system` namespace assembly | `System.Runtime` | `mscorlib` / `sys` | Keeps the XAML resource compatible with ModernWpf's older target frameworks. |
| GridSplitter theme aliases | Dedicated brush resources in official WPF Fluent | `m:StaticResource` aliases to ModernWpf's existing Fluent brush tokens | Keeps ModernWpf's existing theme-resource alias model while exposing the official keys. |
| `GridsplitterMinWidth` | Defined as `8` but not referenced by the official `MinWidth` setter | Defined as `8`, with `MinWidth` still referencing `GridsplitterMinHeight` like official WPF Fluent | Preserves the official style shape while keeping the public resource available. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\GridSplitterVisualStateTests.cs` covers the official WPF Fluent GridSplitter setter shape, runtime metric values, thumb template, WPF trigger matrix, and official theme aliases.

## 2026-05-18 Batch 15

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Hyperlink.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultHyperlinkStyle` | Setter-only `Hyperlink` style with `Foreground`, `TextDecorations=Underline`, pointer-over foreground/underline trigger, and disabled foreground trigger | Same structure under the existing resource key | `Hyperlink` is a stock WPF text element, so official WPF Fluent is the primary source. |
| Implicit `Hyperlink` style | Based on `DefaultHyperlinkStyle` | Same | Matches official WPF Fluent resource shape. |
| `HyperlinkForegroundPointerOver` | Same color as `HyperlinkForeground` in Light, Dark, and HighContrast | Same same-brush alias behavior in ModernWpf's theme-resource model | Replaces the earlier WinUI-style secondary hover color for stock WPF Hyperlink. |
| `HyperlinkForegroundDisabled` | Official disabled foreground resource | Same alias across Light, Dark, and HighContrast | Required by the official WPF Fluent style shape. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| Hyperlink theme aliases | Dedicated brush resources in official WPF Fluent | `m:StaticResource` aliases to ModernWpf's existing Fluent brush tokens | Keeps ModernWpf's existing theme-resource alias model while exposing the official keys. |
| `HyperlinkForegroundPressed` | No official stock `Hyperlink` style use | Retained as an existing resource but no longer consumed by `DefaultHyperlinkStyle` | Avoids unnecessary resource-surface churn while the stock style follows official WPF Fluent. |
| `HyperlinkHelper` | Not part of official stock `Hyperlink` style | Kept for DataGrid-specific hyperlink styling that still uses a WPF pressed-state substitute | This slice is scoped to the stock `Hyperlink` style; DataGrid remains a separate WPF-specific style. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\HyperlinkVisualStateTests.cs` covers the official WPF Fluent Hyperlink setter/trigger shape, removal of the pressed-helper and cursor triggers, removal of the old underline resource, and official theme aliases.

## 2026-05-18 Batch 16

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\GroupItem.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\CollectionViewGroup.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| Implicit `GroupItem` style | `{x:Type GroupItem}` style with a `PART_Header` `ContentPresenter` and an `ItemsPresenter` margin of `5,0,0,0` | Same implicit style shape | `GroupItem` is a stock WPF grouping container, so official WPF Fluent is the primary source. |
| `CollectionViewGroup` data template | `ContentPresenter` binding `Content` to `Name` | Same template included in `Styles\GroupItem.xaml` | Required for grouped headers to display the group name with the official presenter shape. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| Source file split | `GroupItem.xaml` and `CollectionViewGroup.xaml` are separate files merged by the platform theme | Both official resources live in `ModernWpf\Styles\GroupItem.xaml` | `GroupItem.xaml` is ModernWpf's existing grouped-view style entry point under `StockControlsResources`; adding a new source file is unnecessary. |
| `DefaultGroupItemStyle` | No official key | Removed | The old key represented the guessed `ListViewHeaderItem` wrapper style and was not part of official WPF Fluent. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\GroupItemVisualStateTests.cs` covers the official WPF Fluent GroupItem style key/template shape, CollectionViewGroup header template, `ItemsPresenter` margin, and deletion of the old `ListViewHeaderItem` wrapper.

## 2026-05-18 Batch 17

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Menu.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ContextMenu.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\MenuItem.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

### Synced Values

| Resource key / style | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `DefaultMenuStyle` / implicit `Menu` style | Official WPF Fluent stock `Menu` style with `MenuBarBackground`, `MenuBarForeground`, non-focusable root, WPF `Border`, and clipped `ItemsPresenter` | Same source shape | `Menu` is a stock WPF control, so official WPF Fluent is the primary source. |
| `DefaultContextMenuStyle` / implicit `ContextMenu` style | Official WPF Fluent stock `ContextMenu` style with context-menu aliases, `MinWidth=140`, no drop shadow, no popup animation, rounded WPF `Border`, WPF `ScrollViewer`, and vertical `StackPanel` item host | Same source shape, with ModernWpf `TextControlContextMenu` retained outside the stock style | Replaces the previous WinUI-like flyout presenter guess for stock WPF `ContextMenu`. |
| `DefaultMenuItemStyle` / implicit `MenuItem` style | Official WPF Fluent role-template mapping for top-level header/item and submenu header/item, using WPF `ContentPresenter`, `Popup`, `TextBlock`, and trigger behavior | Same source shape with `mscorlib` namespace compatibility | Replaces the previous `VisualStateEx` / `ContentPresenterEx` / `BorderEx` / `ThemeShadowChrome` guessed menu item templates. |
| Menu-family theme aliases | `MenuBarForeground`, `ContextMenuBackground`, `ContextMenuBorderBrush`, `ContextMenuForeground`, `FlyoutBackground`, and `FlyoutBorderBrush` | Same aliases across Light, Dark, and HighContrast through ModernWpf's theme-resource alias model | Required by the official WPF Fluent `Menu`, `ContextMenu`, and `MenuItem` templates. |
| `DefaultCollectionFocusVisualStyle` | Official menu item focus-style resource | ModernWpf alias based on `HighVisibilityFocusVisual` | Lets the official `MenuItem` focus-style setter resolve through ModernWpf's existing focus visual bridge. |

### Intentional Differences

| Resource key / style | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| Source provenance header | WPF UI contributor attribution in the official WPF Fluent source tree | Preserved header in copied ModernWpf style files | Keep provenance from the upstream official source files. |
| `system` namespace assembly | `System.Runtime` | `mscorlib` | Keeps copied glyph string resources compatible with ModernWpf's older target frameworks. |
| `TextControlContextMenu` | No stock menu-family style dependency | Retained in `ContextMenu.xaml` | Existing ModernWpf text controls still use the custom text context menu resource. |
| `CheckBoxBackground` / `CheckBoxBorderBrush` | Referenced by official `MenuItem` templates | Explicit aliases to transparent/system check-box background concepts | Avoid unresolved menu checkbox resources while preserving the official visual intent. |

### Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\MenuFamilyVisualStateTests.cs` covers the official WPF Fluent Menu, ContextMenu, MenuItem, role-template mapping, deleted ModernWpf-specific helper surface, and theme aliases.
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` classifies the menu-family style files as official WPF Fluent stock templates that should not use `VisualStateEx`.

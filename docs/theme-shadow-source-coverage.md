# ThemeShadow Source Coverage

This audit maps local WinUI 3 ThemeShadow and elevation source paths that affect
existing ModernWpf controls, shared shadow rendering, or explicit stock WPF
control exceptions. It complements `docs\theme-shadow-winui3-parity.md`, which
documents the renderer recipe and calibrated geometry.

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` /
`c70471c511a0168b61dcca13af9556465f26b673`.

## Status

- `Source-backed renderer recipe`: shared WinUI shadow constants, popup insets,
  or projected-shadow behavior represented by `ThemeShadowChrome`.
- `Source-backed ThemeShadowChrome`: a shipped ModernWpf control maps the source
  shadow target to `ThemeShadowChrome` and has focused tests/docs.
- `Official WPF Fluent stock exception`: the shipped path is a stock WPF control
  whose style intentionally follows `PresentationFramework.Fluent`, not WinUI.
- `Documented WPF substitution`: the source behavior has no equivalent shipped
  control or cannot be represented directly in WPF; the substitution is
  documented and bounded.

## Source Shadow Inventory

| WinUI source | Source behavior | ModernWpf status | Evidence |
| --- | --- | --- | --- |
| `src\controls\dev\CommonStyles\Common_themeresources.xaml` | Enables drop-shadow mode through `ThemeShadowIsUsingDropShadows=True`. | Source-backed renderer recipe | `docs\theme-shadow-winui3-parity.md`, `ModernWpf\Controls\Primitives\ThemeShadowChrome.cs`, `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` |
| `src\dxaml\xcp\dxaml\lib\ElevationHelper.cpp` | `ApplyElevationEffect` maps default elevated surfaces to base `Translation.Z=32`, plus `8` per nested depth. | Source-backed renderer recipe | `docs\theme-shadow-winui3-parity.md`, `ModernWpf\Controls\Primitives\ThemeShadowChrome.cs`, `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` |
| `src\dxaml\xcp\components\graphics\ThemeShadow.cpp` | Windowed popup drop-shadow mode reserves tight small and medium popup insets. | Source-backed renderer recipe | `docs\theme-shadow-winui3-parity.md`, `ModernWpf\Controls\Primitives\ThemeShadowChrome.cs`, `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` |
| `src\dxaml\xcp\components\graphics\ProjectedShadowManager.cpp` | Popup-owned `ThemeShadow` is applied to the popup child because the popup itself has no size. | Source-backed renderer recipe | `docs\theme-shadow-winui3-parity.md`, `ModernWpf\Controls\Primitives\ThemeShadowChrome.cs`, `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` |
| `src\dxaml\test\native\external\foundation\graphics\rendering\ThemeShadowTests.cpp` | `ThemeShadowDropShadowOpacity` shows the generated `DropShadowVisual` opacity following the caster opacity. | Source-backed renderer recipe | `docs\theme-shadow-winui3-parity.md`, `ModernWpf\Controls\Primitives\ThemeShadowChrome.cs`, `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` |
| `src\controls\dev\NumberBox\NumberBox.cpp` | NumberBox popup root gets `ThemeShadow{}` and `Translation.Z=NumberBoxPopupShadowDepth`, default `16`. | Source-backed ThemeShadowChrome | `docs\numberbox-winui3-source-audit.md`, `ModernWpf.Controls\NumberBox\NumberBox.xaml`, `test\ModernWpf.WinUI.Tests\NumberBox\NumberBoxApiTests.cs` |
| `src\dxaml\xcp\dxaml\lib\CommandBar_Partial.cpp` | CommandBar overflow applies elevation to the source overflow shadow wrapper. | Source-backed ThemeShadowChrome | `docs\commandbar-winui3-source-audit.md`, `ModernWpf.Controls\CommandBar\CommandBar.xaml`, `test\ModernWpf.WinUI.Tests\CommandBar\CommandBarApiTests.cs` |
| `src\controls\dev\CommandBarFlyout\CommandBarFlyout.cpp` | CommandBarFlyout adds/removes presenter `ThemeShadow` around flyout open, close, and secondary command animations. | Source-backed ThemeShadowChrome | `docs\commandbarflyout-winui3-source-audit.md`, `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.xaml`, `test\ModernWpf.WinUI.Tests\CommandBarFlyout\CommandBarFlyoutApiTests.cs` |
| `src\controls\dev\CommandBarFlyout\CommandBarFlyout_themeresources.xaml` | CommandBarFlyout overflow states attach or clear `CommandBarFlyoutOverflowShadow`. | Source-backed ThemeShadowChrome | `docs\commandbarflyout-winui3-source-audit.md`, `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.xaml`, `test\ModernWpf.WinUI.Tests\CommandBarFlyout\CommandBarFlyoutApiTests.cs` |
| `src\controls\dev\NavigationView\NavigationView.cpp` | Overlay pane `ShadowCaster` gets `ThemeShadow{}` and `Translation.Z=PaneOverlayShadowDepth`, default `16`. | Source-backed ThemeShadowChrome | `docs\navigationview-winui3-source-audit.md`, `ModernWpf.Controls\NavigationView\NavigationView.xaml`, `test\ModernWpf.WinUI.Tests\NavigationView\NavigationViewApiTests.cs` |
| `src\controls\dev\TeachingTip\TeachingTip.cpp` | TeachingTip applies `ThemeShadow{}` to `ContentRootGrid` at source content elevation and animates content Z. | Source-backed ThemeShadowChrome | `docs\teachingtip-winui3-source-audit.md`, `ModernWpf.Controls\TeachingTip\TeachingTip.xaml`, `ModernWpf.Controls\TeachingTip\TeachingTip.cs`, `test\ModernWpf.WinUI.Tests\TeachingTip\TeachingTipApiTests.cs` |
| `src\controls\dev\TeachingTip\TeachingTip.cpp` | TeachingTip has a tail-polygon shadow experiment guarded by source debug/test flags. | Documented WPF substitution | `docs\teachingtip-winui3-source-audit.md`, `docs\theme-shadow-winui3-parity.md` |
| `src\dxaml\xcp\dxaml\lib\AutoSuggestBox_Partial.cpp` | Suggestions popup `Opened` applies elevation to the popup part, so the immediate child is shadowed. | Source-backed ThemeShadowChrome | `docs\autosuggestbox-winui3-source-audit.md`, `ModernWpf.Controls\AutoSuggestBox\AutoSuggestBox.xaml`, `test\ModernWpf.WinUI.Tests\AutoSuggestBox\AutoSuggestBoxApiTests.cs` |
| `src\dxaml\xcp\dxaml\lib\ContentDialog_Partial.cpp` | Drop-shadow mode applies elevation to the background element with `baseElevation=128`. | Source-backed ThemeShadowChrome | `docs\contentdialog-winui3-source-audit.md`, `ModernWpf.Controls\ContentDialog\ContentDialog.xaml`, `test\ModernWpf.WinUI.Tests\ContentDialog\ContentDialogApiTests.cs` |
| `src\dxaml\xcp\dxaml\lib\FlyoutPresenter_partial.cpp` | `FlyoutPresenter::OnApplyTemplate` applies elevation to its first child when default shadows are enabled. | Source-backed ThemeShadowChrome | `docs\flyoutbase-winui3-source-audit.md`, `ModernWpf.Controls\Flyout\FlyoutPresenter.xaml`, `test\ModernWpf.WinUI.Tests\Flyout\FlyoutPresenterApiTests.cs` |
| `src\dxaml\xcp\dxaml\lib\MenuFlyoutPresenter_Partial.cpp` | MenuFlyoutPresenter drop-shadow mode applies elevation to the presenter surface. | Source-backed ThemeShadowChrome | `docs\flyoutbase-winui3-source-audit.md`, `ModernWpf.Controls\MenuFlyout\MenuFlyout.xaml`, `test\ModernWpf.WinUI.Tests\MenuFlyout\MenuFlyoutApiTests.cs` |
| `src\dxaml\xcp\dxaml\lib\ComboBox_Partial.cpp` | WinUI ComboBox applies elevation to the popup child. | Official WPF Fluent stock exception | `docs\combobox-wpf-fluent-source-audit.md`, `ModernWpf\Styles\ComboBox.xaml`, `test\ModernWpf.WinUI.Tests\ComboBox\ComboBoxApiTests.cs`, `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` |
| `src\dxaml\xcp\dxaml\lib\ToolTip_Partial.cpp` | WinUI ToolTip applies elevation to the content presenter with tooltip base elevation `16`. | Official WPF Fluent stock exception | `docs\official-fluent-backport-sync.md`, `ModernWpf\Styles\ToolTip.xaml`, `test\ModernWpf.WinUI.Tests\CommonStyles\ToolTipVisualStateTests.cs`, `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` |
| `src\dxaml\phone\lib\DatePickerFlyoutPresenter_Partial.cpp` | WinUI DatePickerFlyoutPresenter applies elevation to its shadow target. | Official WPF Fluent stock exception | `docs\calendar-datepicker-wpf-fluent-source-audit.md`, `ModernWpf\Styles\DatePicker.xaml`, `test\ModernWpf.WinUI.Tests\CommonStyles\DatePickerVisualStateTests.cs`, `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` |
| `src\dxaml\phone\lib\TimePickerFlyoutPresenter_Partial.cpp` | WinUI TimePickerFlyoutPresenter applies elevation to its shadow target. | Documented WPF substitution | `docs\winui3-control-source-coverage.md`, `docs\theme-shadow-winui3-parity.md` |
| `src\dxaml\xcp\dxaml\lib\UIElement_Partial.cpp` | Generic `UIElement.ApplyElevationEffectProxy` forwards to source elevation helper. | Documented WPF substitution | `docs\theme-shadow-winui3-parity.md`, `ModernWpf\Controls\Primitives\ThemeShadowChrome.cs` |

## Deliberate Non-Goals

- ModernWpf does not add new DatePickerFlyout, TimePickerFlyout, or generic
  `UIElement.Shadow` APIs in this phase.
- Stock WPF controls governed by official WPF Fluent keep their official WPF
  shadow implementation even when WinUI has a different compositor path.
- Closed-caption cue shadows, test hooks, native test helpers, parser metadata,
  and XYFocus "shadow" terminology are outside ModernWpf control shadow parity.

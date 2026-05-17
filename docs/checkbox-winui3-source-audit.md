# CheckBox WinUI 3 Source Audit

ModernWpf maps WinUI 3 `CheckBox` onto WPF's platform `CheckBox` rather than adding a new control. This slice removes the guessed trigger-driven visual implementation from the default style and ports the source-shaped state table into `VisualStateEx.Setters`.

Source snapshot:

```text
D:\repos\microsoft-ui-xaml
c70471c511a0168b61dcca13af9556465f26b673
reference/winui3-current
```

## WinUI 3 Source Files

- `src\dxaml\xcp\dxaml\lib\CheckBox_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\CheckBox_Partial.h`
- `src\controls\dev\CommonStyles\CheckBox_themeresources.xaml`

## ModernWpf Files

- `ModernWpf\Styles\CheckBox.xaml`
- `ModernWpf\Controls\Primitives\CheckBoxHelper.cs`
- `test\ModernWpf.WinUI.Tests\CommonStyles\CheckBoxVisualStateTests.cs`

## Ported Behavior

- The WPF template now uses source-shaped `CombinedStates` instead of a separate WPF `ControlTemplate.Triggers` matrix for checked, unchecked, indeterminate, pointer-over, pressed, and disabled colors.
- The old trigger-only setters for content foreground, root background/border, checkbox rectangle stroke/fill, glyph foreground, glyph opacity, and indeterminate glyph data are now `VisualStateEx.Setters`.
- The root chrome is now `GridEx`, matching the WinUI source root `Grid` shape while preserving WPF-only background, border, and corner-radius chrome.
- The static `FontIconFallback` glyph keeps the existing WPF fallback for WinUI `AnimatedIcon`, but the state names now follow source `AnimatedIcon.State` values.
- Indeterminate states set the square glyph data, zero margin, opacity, and source visual state names from the source template.
- `CheckBoxHelper` now handles Add and Subtract keys like WinUI source for enabled two-state check boxes: Add checks the box, Subtract unchecks it, and three-state check boxes keep the platform key path.

## WPF Substitutions

- ModernWpf still styles WPF's `System.Windows.Controls.CheckBox`; it does not introduce a new `ModernWpf.Controls.CheckBox` in this phase.
- WPF has no WinUI `AnimatedIcon` pipeline in this template, so the static `FontIconFallback` plus `AnimatedIcon.State` attached property remains the compatibility layer.
- WinUI clears `IsPressed` from the Add/Subtract key path. WPF `ToggleButton.IsPressed` is not publicly settable, so ModernWpf applies the checked value and updates the visual state immediately.
- WinUI also falls back to legacy `InteractionStates` and `CheckStates` if a custom template does not expose `CombinedStates`. ModernWpf's default template owns `CombinedStates`; custom WPF templates remain responsible for their own state model.
- WinUI sets `HighContrastAdjustment=None` on the check glyph during `OnApplyTemplate`. WPF does not expose the same `UIElement.HighContrastAdjustment` property, so the WPF template relies on existing high-contrast brush resources.
- Focus states remain handled through WPF focus visuals rather than WinUI `PointerFocused` / `Focused` / `Unfocused` source states.

## Tests

- `CheckBoxVisualStateTests.IndeterminateStatesUseVisualStateSettersForGlyphMargin` verifies that the template no longer has WPF triggers, uses `GridEx`, and applies indeterminate glyph data, margin, opacity, and animated state through visual-state setters.
- `CheckBoxVisualStateTests.CombinedStatesUseWinUISourceVisualStateSetters` verifies the source-shaped setter targets across unchecked, checked, and indeterminate states.
- `CheckBoxVisualStateTests.AddAndSubtractKeysFollowWinUICheckBoxSourceBehavior` verifies the source Add/Subtract keyboard behavior for enabled two-state check boxes and the three-state opt-out.

Validation:

```text
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~CheckBoxVisualStateTests
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~CommonStylesResourceTests
```

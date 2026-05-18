# CheckBox Official WPF Fluent Source Audit

ModernWpf maps `CheckBox` to WPF's platform `System.Windows.Controls.CheckBox`.
For this stock WPF control, the primary source is official WPF Fluent rather
than WinUI 3 common styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\CheckBox.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\CheckBox.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\CheckBoxVisualStateTests.cs`

## Ported Behavior

- The default style now follows the official WPF Fluent `RootBorder` / `RootGrid`
  / `ControlBorderIconPresenter` / `StrokeBorder` / `ControlIcon` structure.
- The WinUI `CombinedStates` and `VisualStateEx.Setters` path was removed for
  this stock WPF control. Checked, unchecked, indeterminate, pointer-over,
  pressed, disabled, and empty-content behavior now uses WPF
  `Trigger` / `MultiTrigger` entries.
- The content slot now uses a plain WPF `ContentPresenter`, matching official
  WPF Fluent, rather than `ContentPresenterEx`.
- The check and indeterminate glyphs now use official WPF Fluent text glyphs
  `CheckBoxCheckedGlyph` and `CheckBoxIndeterminateGlyph`, with official
  `CheckBoxIconSize` and `CheckBoxBorderThickness` metrics.
- The WinUI-specific `CheckBoxHelper` state driver was deleted because the
  official WPF Fluent template does not need a custom visual-state manager or
  Add/Subtract key hook.
- Theme dictionaries now expose official glyph brush aliases:
  `CheckBoxCheckGlyphForeground`, `CheckBoxCheckGlyphForegroundPressed`, and
  `CheckBoxCheckGlyphForegroundDisabled`.

## WPF Substitutions

- Official WPF Fluent uses `Border.CornerRadius`; ModernWpf keeps
  `Border.CornerRadius` for older target-framework support.
- Official WPF Fluent uses `DefaultControlFocusVisualStyle`; ModernWpf keeps
  `{x:Static SystemParameters.FocusVisualStyleKey}` plus
  `FocusVisualHelper.UseSystemFocusVisuals` and `FocusVisualHelper.FocusVisualMargin`.
- Existing ModernWpf brush aliases such as `CheckBoxBackgroundUnchecked` and
  `CheckBoxCheckBackgroundFillChecked` are retained because they already map to
  Fluent color concepts and are part of the public resource surface.
- `DataGridCheckBoxStyle` and `DataGridReadOnlyCheckBoxStyle` remain
  ModernWpf/WPF-specific styles based on `DefaultCheckBoxStyle`.

## Tests

- `CheckBoxVisualStateTests.DefaultCheckBoxStyleUsesOfficialWpfFluentTriggerShape`
  covers the default/implicit style shape, official metrics, WPF presenter slot,
  native trigger matrix, and disabled resource application.
- `CheckBoxVisualStateTests.CheckedAndIndeterminateStatesUseOfficialWpfFluentResources`
  verifies checked and indeterminate glyph/chrome resources at runtime.
- `CheckBoxVisualStateTests.ThemeDictionariesExposeOfficialCheckBoxGlyphResources`
  verifies the new official glyph brush aliases across Light, Dark, and HighContrast.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter CheckBoxVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "CheckBoxVisualStateTests|TemplateParityTests|SyncMatrixTests"
dotnet build ModernWpf.sln --no-restore
```

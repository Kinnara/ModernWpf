# ScrollBar Official WPF Fluent Source Audit

ModernWpf now uses the official WPF Fluent `ScrollBar` style as the source for
the stock `System.Windows.Controls.Primitives.ScrollBar` template.

## Source

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ScrollBar.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\ScrollBar.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\ScrollBarVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Synced Behavior

- `DefaultScrollBarStyle` now follows the official WPF Fluent stock setter
  surface: resource-backed background/border, `Margin=0`, `Padding=0`,
  `SnapsToDevicePixels=True`, and `OverridesDefaultStyle=True`.
- The implicit stock `ScrollBar` style remains based on
  `DefaultScrollBarStyle`.
- Orientation selection now uses the official WPF Fluent trigger shape:
  horizontal scroll bars set `Height=12` and use `HorizontalScrollBarTemplate`;
  vertical scroll bars set `Width=12` and use `VerticalScrollBarTemplate`.
- The vertical and horizontal templates now use the official WPF Fluent
  `PART_Border`, line-button, `PART_Track`, page-button, and thumb shape.
- The old WinUI-derived auto-hide, panning-thumb, `ScrollBarHelper`, and
  setter-backed visual-state template guesses were deleted for the stock WPF
  control.

## ModernWpf Substitutions

- The official source file's `System.Runtime` namespace reference is mapped to
  `mscorlib` for ModernWpf's older target frameworks.
- The official source file's `Border.CornerRadius` setter on `Thumb` is mapped
  to `primitives:ControlHelper.CornerRadius`. This keeps the existing
  ModernWpf backport bridge for older WPF targets while preserving the official
  4px thumb radius.
- Existing ModernWpf theme-resource aliases for ScrollBar colors are retained
  and continue to map to ModernWpf Fluent brush tokens.

## Validation

- `test\ModernWpf.WinUI.Tests\CommonStyles\ScrollBarVisualStateTests.cs`
  covers the official WPF Fluent style setter surface, orientation triggers,
  support styles, template parts, theme aliases, and deletion of the old
  ModernWpf auto-hide/panning/VisualState guesses.
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` classifies
  `ModernWpf\Styles\ScrollBar.xaml` as an official WPF Fluent stock template
  file that should not use `VisualStateEx`.

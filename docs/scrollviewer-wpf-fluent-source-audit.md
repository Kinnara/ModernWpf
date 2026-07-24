# ScrollViewer Official WPF Fluent Source Audit

ModernWpf now uses the official WPF Fluent `ScrollViewer` template as the
source for the stock `System.Windows.Controls.ScrollViewer` default style.

## Source

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ScrollViewer.xaml`

## ModernWpf Files

- `ModernWpf\Styles\ScrollViewer.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\ScrollViewerVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Synced Behavior

- `DefaultScrollViewerStyle` now follows the official WPF Fluent setter surface:
  `Margin=0`, `Padding=0`, `SnapsToDevicePixels=True`, and
  `OverridesDefaultStyle=True`.
- The stock template uses the official WPF Fluent grid layout with
  `PART_ScrollContentPresenter`, `PART_VerticalScrollBar`, and
  `PART_HorizontalScrollBar`.
- The default stock template no longer carries ModernWpf-specific focus,
  border, background, corner-radius, automation-id, or explicit
  `CanHorizontallyScroll` / `CanVerticallyScroll` guesses.
- The implicit stock `ScrollViewer` style remains based on
  `DefaultScrollViewerStyle`.

## ModernWpf Substitutions

- `TextControlContentHostStyle` is retained as a ModernWpf support style for
  text-entry templates. It is actively referenced by `TextBox`, `PasswordBox`,
  `ComboBox`, `AutoSuggestBox`, `DatePicker`, and `NumberBox`.
- `TextControlContentHostStyle` remains based on `DefaultScrollViewerStyle` but
  keeps its WPF text-host presenter-margin and corner-radius support.

## Validation

- `test\ModernWpf.WinUI.Tests\CommonStyles\ScrollViewerVisualStateTests.cs`
  covers the official WPF Fluent stock setter surface, template parts, removed
  default-style guesses, and retained ModernWpf text-control host support.
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` classifies
  `ModernWpf\Styles\ScrollViewer.xaml` as an official WPF Fluent stock template
  file that should not use `VisualStateEx`.

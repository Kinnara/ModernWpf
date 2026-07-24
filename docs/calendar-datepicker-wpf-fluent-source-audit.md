# Calendar / DatePicker Official WPF Fluent Source Audit

ModernWpf now treats WPF `Calendar` and `DatePicker` as stock WPF controls whose
primary source is official WPF Fluent, not WinUI `CalendarView` /
`CalendarDatePicker`.

## Official WPF Fluent Inputs

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Calendar.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\DatePicker.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\DefaultContextMenu.xaml`

## ModernWpf Files

- `ModernWpf\Styles\Calendar.xaml`
- `ModernWpf\Styles\DatePicker.xaml`
- `ModernWpf\Styles\ContextMenu.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\DatePickerVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`

## Ported Behavior

- Replaced the old WinUI-like stock `Calendar` template with the official WPF
  Fluent `CalendarDayButton`, `CalendarButton`, `CalendarItem`, and `Calendar`
  styles.
- Replaced the old WPF `DatePicker` to WinUI `CalendarDatePicker` template
  mapping with the official WPF Fluent `DatePickerTextBox`,
  `DatePickerCalendarStyle`, and `DefaultDatePickerStyle`.
- Deleted the DatePicker helper path that drove WinUI-style `CommonStates`,
  `SelectionStates`, and `HeaderStates`; official WPF DatePicker behavior is
  now expressed through WPF template triggers and the platform popup calendar.
- Added the official `DefaultControlContextMenu` resource required by the
  copied DatePicker style.
- Added official DatePicker and CalendarView theme aliases needed by the copied
  style files.

## Intentional Differences

- `System.Runtime` in official Calendar XAML is normalized to `mscorlib` for
  older ModernWpf target frameworks.
- Official `Border.CornerRadius` attached setters and bindings are represented
  with `Border.CornerRadius`, because older WPF targets do
  not expose the official attached property surface.
- `DatePicker.xaml` locally merges `Calendar.xaml`, and the DatePicker
  `CalendarStyle` setter uses `StaticResource`, so the copied
  `DatePickerCalendarStyle` can resolve `DefaultCalendarStyle` after
  ModernWpf splits the official source files into separate dictionaries.
- The DatePicker popup shadow remains WPF `DropShadowEffect`, matching official
  WPF Fluent. ModernWpf does not reintroduce the previous `ThemeShadowChrome`
  guess for this stock control.
- ModernWpf keeps the old CalendarDatePicker resource aliases for compatibility,
  but the active stock DatePicker template now consumes official
  `DatePicker*` keys.

## Tests

- `DatePickerVisualStateTests` covers the official trigger-based DatePicker
  shape, context menu, resource aliases, corner-radius substitution, no
  `VisualStateEx` groups, and the official popup calendar chrome.
- `LayoutCompatibilityApiTests.CalendarNavigationButtonsUseOfficialWpfPresenterSlots`
  verifies that Calendar navigation buttons use WPF presenter slots rather than
  ModernWpf `ContentPresenterEx`.
- `TemplateParityTests` classifies Calendar and DatePicker as official WPF
  Fluent stock templates that should not use `VisualStateEx`.

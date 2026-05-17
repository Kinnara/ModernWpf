# DatePicker WinUI 3 Source Audit

ModernWpf keeps the WPF platform `DatePicker` for this slice. The closest WinUI 3 source match is `CalendarDatePicker`, not WinUI's selector-style `DatePicker`: WPF already owns the popup calendar/text-box date-editing surface, while WinUI `DatePicker` is a separate day/month/year selector control that would be a new control under the current no-new-controls rule.

## WinUI 3 Source Inputs

- `D:\repos\microsoft-ui-xaml\src\controls\dev\CommonStyles\CalendarDatePicker_themeresources.xaml`
- `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\dxaml\lib\CalendarDatePickerAutomationPeer_Partial.cpp`
- `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\CalendarDatePicker.g.cpp`
- `D:\repos\microsoft-ui-xaml\src\dxaml\test\native\external\enterprise\CalendarDatePicker\CalendarDatePickerIntegrationTests.cpp`

Compared but intentionally not mapped as the WPF `DatePicker` target:

- `D:\repos\microsoft-ui-xaml\src\controls\dev\CommonStyles\DatePicker_themeresources.xaml`
- `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\dxaml\lib\DatePicker_Partial.cpp`

## ModernWpf Files

- `ModernWpf\Styles\DatePicker.xaml`
- `ModernWpf\Controls\Primitives\DatePickerHelper.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\DatePickerVisualStateTests.cs`

## Ported Behavior

- Deleted the old WPF `ControlTemplate.Triggers` matrix from the main DatePicker template. The previous template mixed WPF hover/pressed/disabled triggers and ComboBox overlay triggers that do not exist in the WinUI `CalendarDatePicker` source template.
- Replaced the old `HasDateStates` group with source-shaped `CommonStates`, `FocusStates`, and `SelectionStates` groups using `VisualStateEx.Setters`.
- Added source resource aliases for calendar glyph pointer/pressed, text pointer/pressed, and header foreground states across Light, Dark, and HighContrast dictionaries.
- Set the header foreground to `CalendarDatePickerHeaderForeground`, matching the source template's header presenter resource.
- Changed the text foreground default to the source `CalendarDatePickerTextForeground` resource and the calendar glyph default to `CalendarDatePickerCalendarGlyphForeground`.
- Updated `DatePickerHelper` to drive `Normal`, `PointerOver`, `Pressed`, `Disabled`, `Selected`, `Unselected`, `TopHeader`, and `LeftHeader` state names from WPF platform events and `SelectedDate`.
- Removed the guessed `NotImplementedException` from the first-non-null multi-binding converter. The binding is used as a one-way template value source, so `ConvertBack` now returns `Binding.DoNothing` for each target.

## WPF Substitutions

- WPF `DatePicker` supplies the popup calendar, selected-date coercion, parsing, automation peer, and keyboard behavior instead of porting WinUI `CalendarDatePicker`'s native flyout/calendar-view implementation.
- WPF template parts `PART_TextBox` and `PART_Button` stand in for WinUI source parts `DateText` and `CalendarGlyph`.
- WPF has no native `CalendarDatePicker` `SelectionStates` group. `DatePickerHelper` drives `Selected` / `Unselected` from `SelectedDate`.
- Focus states are present for source shape parity but remain empty because WPF's focus visual model is still platform-owned for the styled platform DatePicker.
- WinUI's selector-style `DatePicker` source remains unmapped here because adding that day/month/year control would violate the current no-new-controls phase.

## Tests

`DatePickerVisualStateTests` covers:

- `CommonStates` source setter targets and runtime application of pointer-over and pressed resources.
- `SelectionStates` source setter targets and selected-date state transition.
- Existing `HeaderStates` setter behavior for top and left headers.

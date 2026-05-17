# ComboBox Helper WinUI 3 Source Audit

ModernWpf uses WPF `ComboBox` as the platform control, but its helper/style layer is now treated as a source-backed WPF compatibility layer instead of the older guessed corner-radius behavior.

## Source Files

Primary WinUI 3 source references:

- `src\controls\dev\ComboBox\ComboBoxHelper.idl`
- `src\controls\dev\ComboBox\ComboBoxHelper.h`
- `src\controls\dev\ComboBox\ComboBoxHelper.cpp`
- `src\controls\dev\Generated\ComboBoxHelper.properties.cpp`
- `src\controls\dev\Generated\ComboBoxHelper.properties.h`
- `src\controls\dev\ComboBox\ComboBox_themeresources.xaml`
- `src\controls\dev\ComboBox\APITests\ComboBoxTests.cs`

ModernWpf files:

- `ModernWpf\Controls\Primitives\ComboBoxHelper.cs`
- `ModernWpf\Styles\ComboBox.xaml`
- `test\ModernWpf.WinUI.Tests\ComboBox\ComboBoxApiTests.cs`

## Ported Source Shape

- `KeepInteriorCornersSquare` follows the WinUI 3 helper shape: it monitors `DropDownOpened` and `DropDownClosed`, then recalculates template radii after the popup is open so the open direction can be observed.
- `UpdateCornerRadius` now follows `ComboBoxHelper.cpp`: the split-corner runtime path runs only when `ComboBox.IsEditable` is true. Noneditable ComboBox keeps its normal background corner radius and keeps `PopupBorder.CornerRadius` at the `OverlayCornerRadius` resource while the dropdown is open.
- Editable ComboBox still mirrors the source behavior by splitting the editable text box radius and popup radius in opposite directions while the dropdown is open, then restoring the full control and overlay radii when it closes. The WPF test allows either open direction because WPF popup placement can choose above or below the control.
- `ComboBox_themeresources.xaml` source `VisualState.Setters` are represented by `VisualStateEx.Setters` for editable overlay states, editable text-box common states, and the dropdown glyph animated-icon state.
- `ComboBoxApiTests` now carries the upstream noneditable and editable corner-radius expectations, plus resource/style guard coverage for the WPF template.

## WPF Substitutions

- WinUI `ComboBoxHelper` stores event revokers in a private attached property. WPF uses explicit event subscription and unsubscription from the `KeepInteriorCornersSquare` property callback because WPF has no WinRT event revoker model.
- WinUI names the editable text part `EditableText`; WPF's stock ComboBox contract uses `PART_EditableTextBox`, so the WPF helper targets that platform part name.
- `ComboBoxHelper.TextBoxStyle` remains a WPF-only attached property because WPF needs an explicit hook to style the editable text box. Current WinUI 3 source does not expose this helper property.
- `ComboBoxLightDismissOverlayBackground` is retained as a resource mapping. WPF `Popup` does not expose WinUI's `LightDismissOverlayMode` or XamlRoot popup overlay infrastructure.
- Broader native ComboBox behavior, including selection, editable text commit, typeahead, automation peers, popup layout, and item realization, remains owned by WPF's platform ComboBox rather than a new ModernWpf control.

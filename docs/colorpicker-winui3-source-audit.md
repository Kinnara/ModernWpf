# ColorPicker WinUI 3 Source Audit

Date: 2026-05-17

WinUI 3 source root: `D:\repos\microsoft-ui-xaml`

## Source Files

- `src\controls\dev\ColorPicker\ColorPicker.idl`
- `src\controls\dev\ColorPicker\ColorPicker.cpp`
- `src\controls\dev\ColorPicker\ColorPicker.h`
- `src\controls\dev\ColorPicker\ColorPicker.xaml`
- `src\controls\dev\ColorPicker\ColorSpectrum.idl`
- `src\controls\dev\ColorPicker\ColorSpectrum.cpp`
- `src\controls\dev\ColorPicker\ColorSpectrum.h`
- `src\controls\dev\ColorPicker\ColorSpectrum.xaml`
- `src\controls\dev\ColorPicker\ColorPickerSlider.idl`
- `src\controls\dev\ColorPicker\ColorPickerSlider.cpp`
- `src\controls\dev\ColorPicker\Common\ColorConversion.cpp`
- `src\controls\dev\ColorPicker\Common\ColorConversion.h`

## ModernWpf Files

- `ModernWpf.Controls\ColorPicker\ColorPicker.cs`
- `ModernWpf.Controls\ColorPicker\ColorSpectrum.cs`
- `ModernWpf.Controls\ColorPicker\ColorPickerSlider.cs`
- `ModernWpf.Controls\ColorPicker\ColorConversion.cs`
- `ModernWpf.Controls\ColorPicker\ColorSpectrumAutomationPeer.cs`
- `ModernWpf.Controls\ColorPicker\ColorPicker.xaml`
- `test\ModernWpf.WinUI.Tests\ColorPicker\ColorPickerApiTests.cs`

## What Was Deleted

- The old guessed `ColorPicker` implementation that only pushed the `Color` property into the spectrum, sliders, preview, and text boxes as one-way template updates.
- The old `StackPanel x:Name="RootPanel"` template shape and `PreviewGrid` / `TextInputGrid` names.
- The old gradient-only `ColorSpectrum` template path with static rectangle/ellipse gradients and the extra `SelectionEllipseOuter` ring.
- The old simplified `ColorConversion` helper that only exposed public `RgbToHsv`, `HsvToRgb`, range validation, and clamping.

## Source-Backed Behavior Now Ported

- `ColorPicker` now uses the WinUI source state model: `_currentRgb`, `_currentHsv`, `_currentHex`, `_currentAlpha`, `_updatingColor`, `_updatingControls`, `_previousString`, `_isFocusedTextBoxValid`, and `_textEntryGridOpened`.
- Template part names now follow WinUI source shape: `RootGrid`, `ColorSpectrumGrid`, `ColorPreviewRectangleGrid`, `ThirdDimensionSliderGrid`, `AlphaSliderGrid`, `MoreEntriesPanel`, `TextEntryGrid`, `ColorRepresentationComboBox`, `RGBComboBoxItem`, `HSVComboBoxItem`, `RgbPanel`, `HsvPanel`, `AlphaPanel`, and source text-box names.
- Visual states now include the source groups for spectrum, preview, previous color, third dimension slider, alpha slider, more button, text entry grid, color-channel input, alpha input, selected color representation, hex input, alpha enabled state, and orientation.
- `ColorSpectrum` now owns source-shaped HSV state and bitmap-backed color maps instead of static WPF gradients. The WPF port builds `WriteableBitmap` maps for the same hue/saturation/value component combinations and uses `ImageBrush` fills as the `SpectrumBrush` substitute.
- Pointer, keyboard, focus, selection ellipse, box/ring, and contrast state logic follows the WinUI source path, with WPF mouse capture and visual-state setters as the platform substitutes.
- Third-dimension slider behavior now follows WinUI source mapping: hue/value components use saturation as the slider, hue/saturation components use value, and value/saturation components use hue with source hue-gradient stops.
- RGB, HSV, alpha, and hex text inputs now follow source validation semantics, including alpha `%` insertion, hex `#` insertion, RGB/HSV range validation, and invalid text rollback on focus loss.
- `ColorSpectrumAutomationPeer` exposes WPF `IValueProvider` with source-shaped hue/saturation/value text.

## WPF Substitutions

- WinUI `SpectrumBrush` is represented by WPF `WriteableBitmap` plus `ImageBrush` fills.
- WinUI asynchronous software-bitmap creation is synchronous in WPF because the template tests and WPF rendering path need immediate brush availability.
- WinRT automation notifications are represented by WPF value-pattern property-change events.
- Localized strings currently use English source text in the WPF port.
- Raw WinUI TestUI pointer automation remains represented by direct WPF helper methods and WPF template/input tests.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~ColorPicker`
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore`

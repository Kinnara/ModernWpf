# ColorPicker WinUI 3 Source Audit

Date: 2026-07-16

WinUI 3 source root: `D:\repos\microsoft-ui-xaml`

Audited WinUI revision: `c70471c511a0168b61dcca13af9556465f26b673` (2026-05-11)

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
- `src\controls\dev\ColorPicker\ColorPickerSliderAutomationPeer.cpp`
- `src\controls\dev\ColorPicker\ColorSpectrumAutomationPeer.cpp`
- `src\controls\dev\ColorPicker\ColorPicker_themeresources.xaml`
- `src\controls\dev\ColorPicker\ColorPickerSlider.cpp`
- `src\controls\dev\ColorPicker\Common\ColorConversion.cpp`
- `src\controls\dev\ColorPicker\Common\ColorConversion.h`

## ModernWpf Files

- `ModernWpf.Controls\ColorPicker\ColorPicker.cs`
- `ModernWpf.Controls\ColorPicker\ColorSpectrum.cs`
- `ModernWpf.Controls\ColorPicker\ColorPickerSlider.cs`
- `ModernWpf.Controls\ColorPicker\ColorConversion.cs`
- `ModernWpf.Controls\ColorPicker\ColorSpectrumAutomationPeer.cs`
- `ModernWpf.Controls\ColorPicker\ColorPickerSliderAutomationPeer.cs`
- `ModernWpf.Controls\ColorPicker\ColorDisplayNameHelper.cs`
- `ModernWpf.Controls\ColorPicker\ColorPicker.xaml`
- `test\ModernWpf.WinUI.Tests\ColorPicker\ColorPickerApiTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`

## What Was Deleted

- The old guessed `ColorPicker` implementation that only pushed the `Color` property into the spectrum, sliders, preview, and text boxes as one-way template updates.
- The old `StackPanel x:Name="RootPanel"` template shape and `PreviewGrid` / `TextInputGrid` names.
- The old gradient-only `ColorSpectrum` template path with static rectangle/ellipse gradients and the extra `SelectionEllipseOuter` ring.
- The old simplified `ColorConversion` helper that only exposed public `RgbToHsv`, `HsvToRgb`, range validation, and clamping.

## Source-Backed Behavior Now Ported

- `ColorPicker` now uses the WinUI source state model: `_currentRgb`, `_currentHsv`, `_currentHex`, `_currentAlpha`, `_updatingColor`, `_updatingControls`, `_previousString`, `_isFocusedTextBoxValid`, and `_textEntryGridOpened`.
- Template part names now follow WinUI source shape: `RootGrid`, `ColorSpectrumGrid`, `ColorPreviewRectangleGrid`, `ThirdDimensionSliderGrid`, `AlphaSliderGrid`, `MoreEntriesPanel`, `TextEntryGrid`, `ColorRepresentationComboBox`, `RGBComboBoxItem`, `HSVComboBoxItem`, `RgbPanel`, `HsvPanel`, `AlphaPanel`, and source text-box names.
- Visual states now include the source groups for spectrum, preview, previous color, third dimension slider, alpha slider, more button, text entry grid, color-channel input, alpha input, selected color representation, hex input, alpha enabled state, and orientation.
- `ColorSpectrum` now owns source-shaped HSV state and bitmap-backed color maps instead of static WPF gradients. The WPF port builds `WriteableBitmap` maps for the same hue/saturation/value component combinations and uses `ImageBrush` fills as the `SpectrumBrush` substitute. Box bitmap storage follows the source's descending-coordinate write order, and ring maps use angle for the first configured component and radius for the second.
- Pointer, keyboard, focus, selection ellipse, box/ring, and contrast state logic follows the WinUI source path, with WPF mouse capture and visual-state setters as the platform substitutes.
- Third-dimension slider behavior now follows WinUI source mapping: hue/value components use saturation as the slider, hue/saturation components use value, and value/saturation components use hue with source hue-gradient stops.
- `ColorPickerSliderStyle` now carries the source thumb template, `CommonStates`, focus-engagement state names, and color-picker-specific thumb/track resources instead of falling back to the stock WPF `Slider` template.
- `ColorPickerSlider` now follows the source keyboard model, including channel-specific increments, Ctrl increments, RTL direction, parent min/max constraints, focus tooltips, and automation value notifications.
- RGB, HSV, alpha, and hex text inputs now follow source validation semantics, including alpha `%` insertion, hex `#` insertion, RGB/HSV range validation, and invalid text rollback on focus loss.
- `ColorSpectrumAutomationPeer` exposes the source slider control type, localized control type/help text, writable WPF `IValueProvider`, and source-shaped friendly-color plus HSV text.
- `ColorPickerSliderAutomationPeer` exposes the source `ValuePattern` text for HSV channels (including the source's not-implemented setter) while preserving WPF's native range behavior for opacity.
- The current WinUI text-entry geometry is ported: 120-pixel representation selector and RGB/HSV fields, 132-pixel hex field, five-pixel column gap, no legacy top offset, and the `Opacity` label.
- The source More/Less button shape is ported with a label, chevron glyph, source padding, source automation names, and source help text.
- Preview checkerboards use transparent four-pixel squares over the active theme's `SystemListLowColor`, and are recreated when the WPF theme changes.

## WPF Substitutions

- WinUI `SpectrumBrush` is represented by WPF `WriteableBitmap` plus `ImageBrush` fills.
- WinUI asynchronous software-bitmap creation is synchronous in WPF because the template tests and WPF rendering path need immediate brush availability.
- WinUI `ColorPickerSliderStyle` has separate horizontal and vertical thumb targets. WPF `Slider` requires a functional `PART_Track`, so ModernWpf keeps one source-named `HorizontalThumb` inside the WPF track and routes both focus-engagement states to that thumb.
- WinRT automation notifications are represented by WPF value-pattern property-change events.
- WinUI `ColorDisplayNameHelper` is represented by exact-or-nearest WPF named-color lookup with PascalCase word spacing.
- Localized strings currently use English WinUI source text in the WPF port.
- Raw WinUI TestUI pointer automation remains represented by direct WPF helper methods and WPF template/input tests.

## Validation

- Focused API/template/input/automation tests: 28 passed on `net8.0-windows7.0`.
- The visual harness selected each live Gallery state through UI Automation, restored the original state, and compared ModernWpf with the installed WinUI 3 Gallery.
- Every comparison used exact matching crop geometry. The required crop-delta threshold is `4.0`.

| Theme | State | Crop size | Mean crop delta | Report |
| --- | --- | --- | ---: | --- |
| Dark | Default | 312x474 | 0.96 | `artifacts\visual-checks\20260716-015447-532-94656\report.md` |
| Light | Default | 312x474 | 0.94 | `artifacts\visual-checks\20260716-015510-837-65468\report.md` |
| Dark | More | 312x343 | 0.79 | `artifacts\visual-checks\20260716-012811-039-89520\report.md` |
| Light | More | 312x343 | 0.82 | `artifacts\visual-checks\20260716-012847-660-82296\report.md` |
| Dark | Opacity | 312x566 | 1.68 | `artifacts\visual-checks\20260716-014517-375-84304\report.md` |
| Light | Opacity | 312x566 | 1.60 | `artifacts\visual-checks\20260716-014548-675-74672\report.md` |
| Dark | Ring | 312x474 | 1.23 | `artifacts\visual-checks\20260716-015447-532-94656\report.md` |
| Light | Ring | 312x474 | 1.11 | `artifacts\visual-checks\20260716-015510-837-65468\report.md` |

Commands used for final verification:

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -c Debug -f net8.0-windows7.0 --no-restore --filter FullyQualifiedName~ColorPickerApiTests -p:UseSharedCompilation=false -m:1`
- `dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -c Debug -f net8.0-windows7.0 --no-restore --filter FullyQualifiedName~GalleryVisualChecksUseRenderedModernPrimaryArtifactsForSplitViewAndPersonPicture -p:UseSharedCompilation=false -m:1`
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj -c Debug --no-restore -p:UseSharedCompilation=false -m:1`
- `.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls ColorPicker -Theme <Dark|Light> -Reference InstalledWinUI3Gallery -IncludeInteractions -ColorPickerState <MoreButton|Alpha|Ring>`

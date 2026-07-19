# ColorPicker WinUI 3 Source Audit

Date: 2026-07-16

The current authoritative Microsoft UI XAML revision is
`de3e767333c2f0717a6a70cb22bd192ced5ad885`; the current authoritative WinUI
Gallery revision is `29f62479d5c046a0b854a5868e5a7cd484572d87`.

## Source Files

- `controls\dev\ColorPicker\ColorPicker.idl`
- `controls\dev\ColorPicker\ColorPicker.cpp`
- `controls\dev\ColorPicker\ColorPicker.h`
- `controls\dev\ColorPicker\ColorPicker.xaml`
- `controls\dev\ColorPicker\ColorSpectrum.idl`
- `controls\dev\ColorPicker\ColorSpectrum.cpp`
- `controls\dev\ColorPicker\ColorSpectrum.h`
- `controls\dev\ColorPicker\ColorSpectrum.xaml`
- `controls\dev\ColorPicker\ColorPickerSlider.idl`
- `controls\dev\ColorPicker\ColorPickerSliderAutomationPeer.cpp`
- `controls\dev\ColorPicker\ColorSpectrumAutomationPeer.cpp`
- `controls\dev\ColorPicker\ColorPicker_themeresources.xaml`
- `controls\dev\ColorPicker\ColorPickerSlider.cpp`
- `controls\dev\Common\ColorConversion.cpp`
- `controls\dev\Common\ColorConversion.h`
- `controls\dev\ColorPicker\APITests\ColorPickerTests.cs`
- `controls\dev\ColorPicker\InteractionTests\ColorPickerTests.cs`

## Current source pins

| Source | Blob |
| --- | --- |
| `ColorPicker.idl` | `25c6a3a43577ab8f33df4998aac0726ef987b022` |
| `ColorPicker.cpp` | `886a769b57c3b8c41ddc841df4040b1967fce778` |
| `ColorPicker.h` | `e18c2003d39ac255c688a31121de2ddd42a6e9bd` |
| `ColorPicker.xaml` | `68f539f2388355850077999d1657b8cecdcbf6cb` |
| `ColorSpectrum.idl` | `329977d0543af09ab461806376f7f30b0e322e86` |
| `ColorSpectrum.cpp` | `79e667cdd62edc25bfbae110302f79c61cd39b42` |
| `ColorSpectrum.h` | `7ed5769f9a1b0c5a259b7ed51b47106015658f0e` |
| `ColorSpectrum.xaml` | `fb5f7bd1a83acafccc63e43c0451d8775d82afcd` |
| `ColorPickerSlider.idl` | `5a252e5d97baacfd07378e1faf5b0ae66d1432ef` |
| `ColorPickerSlider.cpp` | `cf25e64606ef7b3e2bfe4a242bd4dc1a21467d9e` |
| `ColorPickerSliderAutomationPeer.cpp` | `b5a058faa89325ea57e8c813e2ecfcf29ff27780` |
| `ColorSpectrumAutomationPeer.cpp` | `f020687ce61ffbfa5734f0ed536a9e39b91da26a` |
| `ColorPicker_themeresources.xaml` | `ff69cb7eb766097830b201c4b0785041027fa499` |
| `Common/ColorConversion.cpp` | `518d5336c8e32e23b5eb4bc69614a41086f5b8d8` |
| `Common/ColorConversion.h` | `7b8f3e3a8714e5e322d7f35f21bb548138120e39` |
| `APITests/ColorPickerTests.cs` | `df809f64ecff042cb0f1f6aaf387015e34aa089d` |
| `InteractionTests/ColorPickerTests.cs` | `52d1388174eeec49715e061c6a3863b8656466b0` |
| `Strings/en-us/Resources.resw` | `b7cf6af17e1b1fc85c24f28f2a293b354f84172e` |
| Gallery `ColorPickerPage.xaml` | `3d0e3397373b6238bf3c28bb3fda8c1a572f76a6` |
| Gallery `ColorPickerPage.xaml.cs` | `c3d167e1c00b026d12ed7faca1a08f30f790930a` |
| Gallery `ColorPickerProperties.txt` | `96ab9df993aa5234453f483bba56cf81f385753e` |

Every listed product, peer, theme, conversion, resource, and test blob is
byte-identical to the original `c70471c511a0168b61dcca13af9556465f26b673`
audit. Commit `8463f45162149de0ec3ad7df752596893fe3e13e` only moved the WinUI 3 mirror
out of the `src` root. Commit `beabd047460bf5d43a41fcf8bddf7730188bd5a7`
packages the same ColorPicker XAML/theme files into the perf2026 dictionaries;
it does not introduce a second ColorPicker template or resource payload.

WinUI Gallery conversion commit
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` moved the page to its current
folder and externalized `ColorPickerProperties.txt` without changing the live
property/shape/preview behavior.

## ModernWpf Files

- `ModernWpf.Controls\ColorPicker\ColorPicker.cs`
- `ModernWpf.Controls\ColorPicker\ColorSpectrum.cs`
- `ModernWpf.Controls\ColorPicker\ColorPickerSlider.cs`
- `ModernWpf.Controls\ColorPicker\ColorConversion.cs`
- `ModernWpf.Controls\ColorPicker\ColorSpectrumAutomationPeer.cs`
- `ModernWpf.Controls\ColorPicker\ColorPickerSliderAutomationPeer.cs`
- `ModernWpf.Controls\ColorPicker\ColorDisplayNameHelper.cs`
- `ModernWpf.Controls\ColorPicker\ColorPicker.xaml`
- `ModernWpf.Gallery\Pages\BasicInputSampleFactory.cs`
- `ModernWpf.Gallery\Samples\SampleCode\ColorPicker\ColorPickerProperties.txt`
- `test\ModernWpf.WinUI.Tests\ColorPicker\ColorPickerApiTests.cs`
- `test\ModernWpf.Gallery.Tests\ColorPickerSourceAuditTests.cs`
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

## Current Gallery and accessibility mapping

ModernWpf loads the current marker-delimited `ColorPickerProperties.txt` and
displays only its `xaml` section. The source `ControlExample.Options` content
is represented by `GalleryExample.OptionsContent`, rather than being embedded
inside the example body. All seven CheckBox properties, their defaults and
alpha-dependent enablement, the Box/Ring `RadioButtons`, the preview label and
rectangle, and live preview-color propagation mirror the current Gallery page.

Gallery regressions exercise property and shape changes through the live
controls. They now also pin CheckBox roles/names/Toggle providers, the
RadioButtons Group role and header-derived name, and the ColorSpectrum Slider
role, `Color picker` name, and writable Value provider. Product tests retain
the deeper slider, spectrum, text-input, localized name/help-text, and
automation-notification coverage.

## WPF Substitutions

- WinUI `SpectrumBrush` is represented by WPF `WriteableBitmap` plus `ImageBrush` fills.
- WinUI asynchronous software-bitmap creation is synchronous in WPF because the template tests and WPF rendering path need immediate brush availability.
- WinUI `ColorPickerSliderStyle` has separate horizontal and vertical thumb targets. WPF `Slider` requires a functional `PART_Track`, so ModernWpf keeps one source-named `HorizontalThumb` inside the WPF track and routes both focus-engagement states to that thumb.
- WinRT automation notifications are represented by WPF value-pattern property-change events.
- WinUI `ColorDisplayNameHelper` is represented by exact-or-nearest WPF named-color lookup with PascalCase word spacing.
- Localized strings currently use English WinUI source text in the WPF port.
- Raw WinUI TestUI pointer automation remains represented by direct WPF helper methods and WPF template/input tests.

## Validation

- Focused API/template/input/automation tests pass 28/28 on
  `net8.0-windows7.0`.
- Gallery sample/source/gate tests pass 3/3 on both modern targets. They cover
  the external source snippet, official options surface, live bindings,
  preview, roles, names, and providers.
- The visual harness selects each live Gallery state through UI Automation,
  restores the original state, and compares ModernWpf with the installed WinUI
  3 Gallery. The Alpha scenario uses a 900px viewport in both apps so the
  complete live surface is visible below the fixed Gallery header.
- Static and interaction gates require the common `ColorPicker editor surface`
  source, mean delta `<=4.0`, and zero size difference.

| Theme | State | Crop size | Mean crop delta | Report |
| --- | --- | --- | ---: | --- |
| Dark | Default | 312x474 | 0.96 | `artifacts\visual-checks\20260718-083011-718-58464\report.md` |
| Light | Default | 312x474 | 0.94 | `artifacts\visual-checks\20260718-082916-447-87472\report.md` |
| Dark | More | 312x343 | 0.79 | `artifacts\visual-checks\20260718-083011-718-58464\report.md` |
| Light | More | 312x343 | 0.82 | `artifacts\visual-checks\20260718-082916-447-87472\report.md` |
| Dark | Opacity | 312x566 | 1.68 | `artifacts\visual-checks\20260718-083641-163-94428\report.md` |
| Light | Opacity | 312x566 | 1.60 | `artifacts\visual-checks\20260718-083553-318-17576\report.md` |
| Dark | Ring | 312x474 | 1.23 | `artifacts\visual-checks\20260718-083747-519-36496\report.md` |
| Light | Ring | 312x474 | 1.11 | `artifacts\visual-checks\20260718-083723-381-61676\report.md` |

Commands used for final verification:

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -c Debug -f net8.0-windows7.0 --no-restore --filter FullyQualifiedName~ColorPickerApiTests -p:UseSharedCompilation=false -m:1`
- `dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -c Debug -f <net8.0-windows7.0|net10.0-windows7.0> --no-restore --filter "FullyQualifiedName~ColorPickerSampleMatchesWinUIGalleryExample|FullyQualifiedName~ColorPickerSourceAuditTests|FullyQualifiedName~GalleryVisualChecksEnforceColorPickerCurrentSourceSurfaceParity"`
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj -c Debug --no-restore -p:UseSharedCompilation=false -m:1`
- `.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls ColorPicker -Theme <Dark|Light> -IncludeWinUIReference -IncludeInteractions -ColorPickerState <MoreButton|Alpha|Ring> -FailOnDifference`

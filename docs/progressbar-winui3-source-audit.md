# ProgressBar current WinUI 3 source audit

Audit date: 2026-07-18

This audit covers the custom `ModernWpf.Controls.ProgressBar`. The separate
`System.Windows.Controls.ProgressBar` Gallery route remains the official WPF
Gallery port documented by `stock-progressbar-wpf-fluent-source-audit.md`; it
is not evidence for this WinUI-backed control.

## Pinned sources

The product source of truth is official `microsoft/microsoft-ui-xaml`
`winui3/main` commit `de3e767333c2f0717a6a70cb22bd192ced5ad885`.
The current files and Git blob IDs are:

| Upstream file | Blob |
| --- | --- |
| `controls/dev/ProgressBar/ProgressBar.cpp` | `7ab65894f91ca59504729240d044cdf468d266cc` |
| `controls/dev/ProgressBar/ProgressBar.h` | `ecc02ddb7ddd0f6deb6b11ba9bc6d535ed71407e` |
| `controls/dev/ProgressBar/ProgressBar.idl` | `ffa5531f73b8add16756e254b6a76cc630a6d270` |
| `controls/dev/ProgressBar/ProgressBar.xaml` | `5f188ab8b3d45632d74a90b742299e0412e45feb` |
| `controls/dev/ProgressBar/ProgressBar_themeresources.xaml` | `f2cf8b8edbc50ab21d0f2d5f460f30b92d662a36` |
| `controls/dev/ProgressBar/ProgressBarAutomationPeer.cpp` | `cca6b40d87c1ab0e473b108132a13b434c6de9a3` |
| `controls/dev/ProgressBar/ProgressBarAutomationPeer.h` | `e8673c0c2f2d5b44f33334a6809dbae9a9e64d42` |
| `controls/dev/ProgressBar/ProgressBarTemplateSettings.cpp` | `14a3502b4ca4edf921e608335f389514755e7b8b` |
| `controls/dev/ProgressBar/ProgressBarTemplateSettings.h` | `590adb31addfd5a1c161a00dce2da45bdb293b51` |
| `controls/dev/Generated/ProgressBar.properties.cpp` | `ac139cffa54910194a46e2075b41979c94556956` |
| generated ProgressBar template-settings properties | `3e0d42c51005ac5610835952ee973dab6db379bb` |
| `controls/dev/ProgressBar/APITests/ProgressBarTests.cs` | `9ccf87853c457992f7f08a5b7cac71bb3e51315a` |
| `controls/dev/ProgressBar/InteractionTests/ProgressBarTests.cs` | `60f20a78ea4a4f320d1844b70f6026f4463c3801` |
| `controls/dev/ProgressBar/ProgressBar.vcxitems` | `cfd5abc75f385a1466e038c26f0329faa1914c8e` |

The two newer source-history hits are non-substantive for runtime parity:

- `8463f45162149de0ec3ad7df752596893fe3e13e` moved the WinUI source
  mirror from `src/controls/...` to the current root layout.
- `beabd047460bf5d43a41fcf8bddf7730188bd5a7` added perf2026 build-item
  classifications to `ProgressBar.vcxitems`; it packages the same ProgressBar
  and theme-resource dictionaries and changes no runtime or resource blob.

The Gallery source of truth is official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`:

| Gallery file | Blob |
| --- | --- |
| `WinUIGallery/Samples/ProgressBar/ProgressBarPage.xaml` | `dbaba364fe27de13c460db24e7824bbeb2645c54` |
| `WinUIGallery/Samples/ProgressBar/ProgressBarPage.xaml.cs` | `5b0d26ef7b6fe8f95a102183e69659e595383291` |
| `WinUIGallery/Samples/ProgressBar/DeterminateProgressBar.txt` | `5812f9901151066051c65f46a1bf98967c7ab0e4` |
| `WinUIGallery/Samples/ProgressBar/IndeterminateProgressBar.txt` | `dcf4e7e0bf4bf5252b5b12c74fe23b2f18a1c670` |
| `WinUIGallery/SampleSupport/Data/ControlInfoData.json` | `681e0569fcf304ad4d9f925109f1f6797d66c092` |

## Product mapping

ModernWpf retains the complete feasible WPF surface in:

- `ModernWpf/ProgressBar/ProgressBar.cs`;
- `ModernWpf/ProgressBar/ProgressBar.xaml`;
- `ModernWpf/ProgressBar/ProgressBarAutomationPeer.cs`;
- `ModernWpf/ProgressBar/ProgressBarTemplateSettings.cs`;
- `ModernWpf/Controls/Primitives/ProgressBarIndicatorRasterOverlay.cs`.

The port follows current source for `IsIndeterminate`, `ShowPaused`,
`ShowError`, value/range changes, padding, border thickness, visibility,
template application, and size changes. In particular:

- indeterminate states are selected only while the control is visible;
- determinate width subtracts padding and layout-rounded border thickness;
- the second indeterminate indicator is 60 percent normally and the full
  available width for paused/error states;
- `ContainerAnimationMidPosition` remains zero;
- source paused/error visual states, brush-color setters, transition timing,
  and indicator transforms remain in the template;
- the track keeps its source height/radius and remains resource-overridable;
- the automation peer suppresses `RangeValue` while indeterminate and prefixes
  the localized busy, paused, or error status to the base accessible name.

## WPF substitutions

- WinUI property callbacks map to WPF dependency-property metadata,
  `OnPropertyChanged`, and WPF range callbacks.
- WinUI `XamlRoot.RasterizationScale` maps to
  `VisualTreeHelper.GetDpi(this)` for physical-pixel border rounding.
- WPF rejects negative `Rect` dimensions during early layout, so clip width
  and height are clamped to zero until valid dimensions exist.
- The unused WinUI `Normal` state is omitted because WPF's base control-state
  path treats that state specially and would override ProgressBar-owned state.
- WinUI composition transforms/theme animations map to WPF
  `TranslateTransform` and key-frame storyboards.
- A one-scanline raster adapter compensates only for a verified renderer
  difference. WPF antialiases the full upper edge of a three-pixel rounded
  rectangle; WinUI antialiases its caps but leaves the interior scanline solid.
  `ProgressBarIndicatorRasterOverlay` draws one DPI-aware physical scanline,
  inset two physical pixels from both caps and snapped with a `GuidelineSet`.
  Each determinate/indeterminate overlay follows its source rectangle's live
  Fill, ActualWidth, Opacity, and RenderTransform. The source radius, lower
  scanline, width math, track, visual states, animations, hit testing, and
  public `130x3` geometry remain unchanged.

## Current Gallery surface

The current WinUI Gallery has two examples:

1. an indeterminate 130-DIP bar with Running, Paused, and Error options; and
2. a determinate 130-DIP `ProgressBar2`, a blank 60-DIP output field, a
   `Progress` label, and an inline `ProgressValue` NumberBox from 0 to 100.

The port keeps the exact current snippets:

```xaml
<ProgressBar Width="130" IsIndeterminate="True" ShowPaused="$(ShowPaused)" ShowError="$(ShowError)" />
```

```xaml
<ProgressBar Width="130" Value="$(DeterminateProgressValue)" />
```

The NumberBox updates the determinate bar and resets NaN to zero like current
Gallery code-behind. The custom catalog route is deliberately named
`WinUIProgressBar`. Only its WinUI-reference URI maps to official
`ProgressBar`; the ModernWpf route never aliases the stock WPF Gallery page.
The determinate controls expose the current automation names, with
`GallerySample_WinUIProgressBar_DeterminateProgressBar` retained as the local
diagnostic ID.

## Strict live evidence

The visual harness requires the real ModernWpf diagnostic element and official
WinUI `ProgressBar2`, sets `ProgressValue` to 65 through UIA `RangeValue`,
requires a primary crop, and rejects any size difference. The gate is mean
RGB delta `2.0` with size tolerance `0`.

| Theme | Report | ModernWpf / WinUI size | Primary delta |
| --- | --- | --- | ---: |
| Light | `artifacts/visual-checks/20260718-105525-867-12132/report.md` | `130x3` / `130x3` | `0.43` |
| Dark | `artifacts/visual-checks/20260718-105621-848-29676/report.md` | `130x3` / `130x3` | `0.43` |

Both apps pass. The full-window mean is intentionally not a parity assertion;
the two Gallery shells differ. The strict proof is the common live control
crop.

## Regression coverage

- `ProgressBarApiTests` pins defaults, resources, template parts, current
  theme aliases, track geometry, and the DPI-aware overlays.
- `ProgressBarInteractionTests` covers range/width refresh, visibility-gated
  states, paused/error and indeterminate paths, retemplating, and automation.
- `ProgressBarSourceAuditTests` pins the current product/Gallery commits and
  blobs, implementation shape, distinct Gallery route, strict IDs, and gates.
- Gallery runtime/source-shape tests pin both current examples, exact snippets,
  option behavior, accessible names, NaN reset, stock/custom route separation,
  and live harness setup.

Final focused verification passes 17/17 ProgressBar product/source tests on
net8 and 44/44 Gallery runtime/source/curated-ID tests on both net8 and net10.
ModernWpf builds on net462, net8, and net10; Gallery dependency builds on net8
and net10. All complete with zero errors. Reported warnings are existing SDK,
package, generated-code, TitleBar, NavigationView, PersonPicture, and
ItemsRepeater diagnostics rather than ProgressBar warnings.

Reopen this audit only for new ProgressBar product or Gallery source, range or
state behavior, theme/template resources, automation/accessibility, DPI/cap
rasterization, or strict live visual-regression evidence.

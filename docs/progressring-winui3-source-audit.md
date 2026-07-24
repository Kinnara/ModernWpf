# ProgressRing current WinUI 3 source audit

Audit date: 2026-07-18

## Pinned sources

The product source of truth is official `microsoft/microsoft-ui-xaml`
`winui3/main` commit `de3e767333c2f0717a6a70cb22bd192ced5ad885`.

| Upstream file | Blob |
| --- | --- |
| `controls/dev/ProgressRing/ProgressRing.cpp` | `46aaf1a82be04175f7eb6a8f6f2481ac7db7be15` |
| `controls/dev/ProgressRing/ProgressRing.h` | `208152aa309ee51da581320032677088fa816154` |
| `controls/dev/ProgressRing/ProgressRing.idl` | `4ab074e9c8c65714fefd7892fa75236f919463bf` |
| `controls/dev/ProgressRing/ProgressRing.xaml` | `df15b1794bbd0cda30279b5ab0674cafa544a0cd` |
| `controls/dev/ProgressRing/ProgressRing_themeresources.xaml` | `f3fa38ea83ea910200a6a539140a379b128bbf94` |
| `controls/dev/ProgressRing/ProgressRingAutomationPeer.cpp` | `cd10c66d1322c5b4620bdcdadcfd5bb6c49fb856` |
| `controls/dev/ProgressRing/ProgressRingAutomationPeer.h` | `71d6d8b6607ee18a8311efc948f0a0f3ca2f4b18` |
| `controls/dev/ProgressRing/ProgressRingTemplateSettings.cpp` | `7a7326a2601469a5d186217f2088441f60987d66` |
| `controls/dev/ProgressRing/ProgressRingTemplateSettings.h` | `62940cad2761151706148c539791b48eb362eed4` |
| `controls/dev/ProgressRing/AnimatedVisuals/ProgressRingDeterminate.cpp` | `7d5234db42e92d9d4411009bd0086e7a2a1f0e31` |
| `controls/dev/ProgressRing/AnimatedVisuals/ProgressRingDeterminate.h` | `6c93dc39f1e791c16fcdf0f8d3148a15300518c7` |
| `controls/dev/ProgressRing/AnimatedVisuals/ProgressRingIndeterminate.cpp` | `48b0ac924c6010a5bc4ba7aa4d588c200d807907` |
| `controls/dev/ProgressRing/AnimatedVisuals/ProgressRingIndeterminate.h` | `1c3e62025a3bca6f3cb32ae7e5f24d8493c340ab` |
| `controls/dev/Generated/ProgressRing.properties.cpp` | `c04d7bff71f333adb3d3d39442a0ac25a8e0442a` |
| `controls/dev/Generated/ProgressRingTemplateSettings.properties.cpp` | `bc13d0359e0b8d1cbdb0003cc49bb0fe11f208da` |
| `controls/dev/ProgressRing/APITests/ProgressRingTests.cs` | `5a71dd173f31e68611a1fd05300c5c99c1a021d1` |
| `controls/dev/ProgressRing/InteractionTests/ProgressRingTests.cs` | `8b82ad49e3cdf29bb5f5906833168daf882de368` |
| `controls/dev/ProgressRing/Strings/en-us/Resources.resw` | `7a097e1d732bb2bc0b6ce6b283202bec46ee7659` |
| `controls/dev/ProgressRing/ProgressRing.vcxitems` | `cfa8fbdd137cd9aba33a128ba7397f01bdde9f2a` |

All runtime, generated, animation, peer, resource, and test blobs are
byte-identical to the previous source-shaped audit at
`c70471c511a0168b61dcca13af9556465f26b673`. The only later control-tree
changes are:

- `beabd047460bf5d43a41fcf8bddf7730188bd5a7`, which adds perf2026 build
  classifications to `ProgressRing.vcxitems` while packaging the same XAML;
- `8463f45162149de0ec3ad7df752596893fe3e13e`, which moves the source mirror
  from `src/controls/...` to the current root layout.

The current Gallery source of truth is official `microsoft/WinUI-Gallery`
commit `29f62479d5c046a0b854a5868e5a7cd484572d87`.

| Gallery file | Blob |
| --- | --- |
| `WinUIGallery/Samples/ProgressRing/ProgressRingPage.xaml` | `ec4263532cd9e028df8be2ca922d313b4a1d72d0` |
| `WinUIGallery/Samples/ProgressRing/ProgressRingPage.xaml.cs` | `092637258d6c90583e2e29fa4b48d6fafa31c3ae` |
| `WinUIGallery/Samples/ProgressRing/DeterminateProgressRing.txt` | `7c30cfaf363ecbfc49a399806b79bc8e5804e5ca` |
| `WinUIGallery/Samples/ProgressRing/IndeterminateProgressRing.txt` | `e68db7715d70a8f59ed66cf02bfb81ba9ffb02af` |
| `WinUIGallery/SampleSupport/Data/ControlInfoData.json` | `681e0569fcf304ad4d9f925109f1f6797d66c092` |

## Source-backed product behavior

ModernWpf maps the current feasible surface through
`ModernWpf.Controls/ProgressRing`:

- the template uses `LayoutRoot`, `LottiePlayer`, and only the current
  `Inactive`, `DeterminateActive`, and `Active` common states;
- `Inactive` targets `LayoutRoot.Opacity` instead of collapsing the control;
- template application discovers the layout root, refreshes determinate
  progress, then updates state; Loaded refreshes state and SizeChanged updates
  compatibility template settings;
- value/minimum/maximum changes use the current bounded coercion and refresh
  determinate progress;
- compatibility ellipse settings derive from ActualWidth exactly like source;
- default foreground, transparent background, stroke resource, dimensions,
  alignment, hit testing, tab-stop, and range defaults match current XAML;
- active indeterminate automation names are prefixed with localized `Busy`,
  both modes expose the ProgressBar role and localized `ProgressRing` type,
  inactive rings leave the control view, and only determinate rings expose the
  RangeValue provider.

## WPF substitutions

- WinUI renders with `AnimatedVisualPlayer`, `IAnimatedVisualSource`, and the
  generated determinate/indeterminate Composition visuals. WPF uses a
  storyboard-backed `LottiePlayer` grid plus `ProgressRingIndicator`.
- The native WPF indicator derives its geometry from the current generated
  32x32 animation: 8px ellipse radius, 1.5px stroke, and 1.77 shape scale. At
  the Gallery's 60px size this yields the source 26.55px radius and 4.98px
  effective stroke.
- WPF Brush/Freezable invalidation naturally refreshes foreground/background
  subproperty changes in place of WinUI's explicit color callback/revokers.
- Preview-only `DeterminateSource` and `IndeterminateSource` remain omitted;
  exposing them as `object` would be a misleading API without the WinUI
  animated-visual interfaces.
- WPF has no `AutomationProperties.AccessibilityView`; the peer implements the
  source active Content/inactive Raw effect through `IsControlElementCore`.
- WPF visual-state storyboards can retain held values, so state updates reset
  `LayoutRoot.Opacity` before entering the current source state.
- `ProgressRingTemplateSettings` remains for compatibility and drives the WPF
  substitute even though current WinUI's default Lottie template no longer
  consumes those ellipse settings.

## Current Gallery and behavior surface

The current page has two 60x60 examples:

1. active indeterminate `ProgressRing1`, named `Progress image`, with the
   `Progress Options` ToggleSwitch and Transparent/LightGray background option;
2. determinate `ProgressRing2`, named `Progress image`, with a 60-DIP right
   margin, inline `ProgressValue` NumberBox named `Progress amount`, and its own
   background option.

The exact current snippets are:

```xaml
<ProgressRing IsActive="$(IsActive)" $(Background)/>
```

```xaml
<ProgressRing Width="60" Height="60" Value="$(DeterminateProgressValue)"
              IsIndeterminate="False"
              $(Background)/>
```

ModernWpf's ProgressRing has no public Background property, so the documented
WPF Gallery adapter applies the selected background to a transparent host
Border while leaving ring rendering and geometry unchanged. Toggle, value,
Transparent/LightGray, and NaN-to-zero behavior match the page code-behind.

## Strict live evidence

The harness compares only the deterministic determinate state: it locates the
real ModernWpf diagnostic ring and official `ProgressRing2`, sets both
`ProgressValue` NumberBoxes to 65 through UIA RangeValue, refreshes ModernWpf's
rendered artifact, and requires the live primary crop. The gate is mean RGB
delta `1.0` and size tolerance `0`.

| Theme | Report | ModernWpf / WinUI size | Primary delta |
| --- | --- | --- | ---: |
| Light | `artifacts/visual-checks/20260718-111542-017-86572/report.md` | `60x60` / `60x60` | `0.64` |
| Dark | `artifacts/visual-checks/20260718-111628-842-2556/report.md` | `60x60` / `60x60` | `0.63` |

Both apps pass. Whole-window mean values are not parity evidence because the
two Gallery shells differ.

## Regression coverage

- product API tests pin defaults/resources, template and state shape,
  generated-animation raster geometry, role/name/control-view behavior;
- interaction tests pin active/determinate/inactive state transitions, range
  updates/coercion, RangeValue exposure, and indeterminate suppression;
- the current-source audit gate pins current product/Gallery commits and blobs,
  implementation shape, exact current sample surface, and strict harness gates;
- Gallery runtime tests cover the two examples, exact snippets, toggle,
  background selections, value propagation, accessible names, and NaN reset;
- Gallery harness tests pin UIA setup, primary IDs, deterministic value 65,
  the `1.0` delta gate, and zero size tolerance.

Final focused verification passes 11/11 ProgressRing product/source tests on
net8 and 45/45 Gallery runtime/harness/animation/curated-ID tests on both net8
and net10. ModernWpf.Controls builds successfully for net462, net8, and net10
with zero errors. The net462 full rebuild reports 18 existing unrelated
NavigationView, PersonPicture, and ItemsRepeater warnings; the net8/net10 test
builds also retain existing SDK/package diagnostics. No ProgressRing warning is
introduced.

Reopen only for new ProgressRing product/Gallery source, generated animation
geometry, custom-source strategy, range/state/background behavior,
theme/template resources, automation/accessibility, or strict visual evidence.

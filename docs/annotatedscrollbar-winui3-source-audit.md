# AnnotatedScrollBar current WinUI 3 source and Gallery parity audit

Date: 2026-07-18

## Pinned upstream snapshots

| Source | Commit | Purpose |
| --- | --- | --- |
| microsoft-ui-xaml | `de3e767333c2f0717a6a70cb22bd192ced5ad885` | Current `winui3/main` product source audited here. |
| microsoft-ui-xaml | `8463f45162149de0ec3ad7df752596893fe3e13e` | 2026-05-30 move from the src-prefixed mirror to the current root layout. |
| microsoft-ui-xaml | `77360ea4bf813506ee75e1900c9f28f0b35d8495` | Current constant/dynamic-initializer cleanup touching the runtime. |
| microsoft-ui-xaml | `beabd047460bf5d43a41fcf8bddf7730188bd5a7` | Performance2026 resource packaging baseline. |
| WinUI Gallery | `29f62479d5c046a0b854a5868e5a7cd484572d87` | Current Gallery source and installed-app comparison target. |
| WinUI Gallery | `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` | Gallery sample-folder conversion baseline. |

Relative to prior product pin
`c70471c511a0168b61dcca13af9556465f26b673`, all generated, IDL, header,
classic template/theme, event-args, panning, API-test, interaction-test, TestUI,
and ScrollPresenter-primitives inputs are byte-identical after path
normalization. Current `AnnotatedScrollBar.cpp` removes a file-local infinite
size in favor of the shared `LayoutUtils::c_infSize` and iterates labels by
const reference; neither changes behavior. Current packaging adds the
Performance2026 theme dictionary, whose four discrete foreground animations
become equivalent visual-state setters. ModernWpf already expresses those
states with setters, so no second template path is required.

## Current WinUI product inventory

| File under microsoft-ui-xaml | Blob |
| --- | --- |
| `controls\dev\Generated\AnnotatedScrollBar.properties.cpp` | `4a99015f427278858bbfedf397ecb4c915931d4a` |
| `controls\dev\Generated\AnnotatedScrollBar.properties.h` | `d98dfdf2d7df9299e91f54f4d5c39fe7977b79bc` |
| `controls\dev\Generated\AnnotatedScrollBarLabel.properties.cpp` | `931bf2f25d3dfc99c72520fca64e9770bbcc1ed8` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBar.cpp` | `c9ee884da938b6b4bdfa1ba76dd63f1de1f73751` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBar.h` | `1ef148a4a9cf0fb937d9bc53533aa40b76e6b3c8` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBar.idl` | `495e27dc58df515f48aacf340c8769a07ef7484d` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBar.vcxitems` | `0e8fbab6b1b45d832f7b21fec6090cbd7a95b81a` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBar.xaml` | `5e84a8dfe4481d88b8fabec7602990ddb3e0a9e5` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBarDetailLabelRequestedEventArgs.cpp` | `072036aee95a4c89c68907d89892f59ecd0bcaf1` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBarDetailLabelRequestedEventArgs.h` | `e8739c5afcbfb65d5bf287cda16d455d137e8c2f` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBarLabel.cpp` | `1148e8d0fb30727482db3950477e79e5b1d0c2ab` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBarLabel.h` | `31f4e670d40d0dc5181a4927e6a066b4967667f3` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBarPanningInfo.cpp` | `c72ba138ce3569f9a8a51f8f9e9d44632f2e11ac` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBarPanningInfo.h` | `12fdedda933aa12f370c112aa3a2e6f1afdeaf06` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBarScrollingEventArgs.cpp` | `b51b210d1f973cd7d958bc612ebe16a802ed260f` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBarScrollingEventArgs.h` | `d3f4bf345dcefab432402784ad57e0655c23ed4b` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBarTrace.h` | `17d0330dd2a1fb1e6d9ee5b91f248c1f46a8e5a3` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBar_themeresources.xaml` | `dd312445431351223d46afdf1006bd87de06ea0a` |
| `controls\dev\AnnotatedScrollBar\AnnotatedScrollBar_themeresources_perf2026.xaml` | `355e681a505ecb17c0a9dc174e33ef61868769e8` |
| `controls\dev\AnnotatedScrollBar\APITests\AnnotatedScrollBarTests.cs` | `8cf9d20e67173d0afdc25829aebdd499bd87e2f5` |
| `controls\dev\AnnotatedScrollBar\InteractionTests\AnnotatedScrollBarInteractionTests.cs` | `e47aa7cdc0203cfee22763732fb51f287f4d1fb3` |
| `controls\dev\AnnotatedScrollBar\TestUI\AnnotatedScrollBarPage.xaml` | `22a40070604c36fca589530f9498670d325eecb3` |
| `controls\dev\AnnotatedScrollBar\TestUI\AnnotatedScrollBarPage.xaml.cs` | `e71f9d2fb066ec85040e79ade20791e4c60ee1a7` |
| `controls\dev\ScrollPresenter\ScrollPresenterPrimitives.idl` | `eced085d08c7cfe91bbc9473aa54343627f3f0c6` |

### API and scroll-controller behavior

The control exposes an instance-scoped label collection, `LabelTemplate`,
`DetailLabelTemplate`, `SmallChange`, the control itself as
`IScrollController`, and cancelable `Scrolling` plus event-driven
`DetailLabelRequested`. Labels retain only `Content` and `ScrollOffset`; there
is no guessed `ToString()` content fallback.

`SetValues` rejects `maxOffset < minOffset` and negative viewport length,
clamps the incoming offset, updates range/viewport geometry, and defers the
visible value while requested operations are outstanding. `SetIsScrollable`
and `IsEnabled` jointly own `CanScroll` and its change event. Scroll-to and
scroll-by requests use disabled animation and ignored snap points. A cancelled
`Scrolling` event suppresses the request. Source button direction is retained:
the increment part requests a negative small change and decrement requests a
positive small change. Panning info is vertical and rail-enabled; WPF layout
updates replace unavailable composition expression sources.

### Labels, hover, and template

Current source parts are preserved: `PART_VerticalThumb`,
`PART_VerticalThumbGhost`, both repeat buttons, `PART_VerticalGrid`,
`PART_LabelsGrid`, `PART_TooltipContentPresenter`, and
`PART_DetailLabelToolTip`. The old guessed `PART_Rail`, `PART_LabelsHost`, and
label-host `ItemsControl` path remains deleted.

Labels are measured, positioned by the scroll-to-label factor, and collapsed
when out of bounds or colliding, including the source first/last label choice.
Pointer hover positions the tooltip and thumb ghost; tooltip content exists
only when a `DetailLabelRequested` handler supplies it. The 30x3 accent thumb,
44-DIP label minimum, 8-DIP arrow glyphs, 360-DIP tooltip maximum, brushes,
corner radii, High Contrast border, and source button states are retained.
Repeat buttons inherit current common `ButtonPadding`, and their glyphs consume
`SymbolThemeFontFamily`. WPF direct resource resolution preserves the exact
source aliases without an indirect dynamic-resource fallback.

### Accessibility boundary

Current WinUI defines no `AnnotatedScrollBarAutomationPeer` and has no peer
override, so the visual scroll controller has no standalone control role or
pattern. Its internal template presenter is raw accessibility content; the
connected `ScrollView` remains the actionable scroll surface. ModernWpf now
pins the same ownership explicitly: `AnnotatedScrollBar` creates no standalone
WPF peer, while the Gallery's WPF `ScrollViewer` owns the Scroll pattern used by
the interaction recorder.

## Current WinUI Gallery inventory

The converted current sample is unchanged after
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`:

| File under WinUI-Gallery | Blob |
| --- | --- |
| `WinUIGallery\Samples\AnnotatedScrollBar\AnnotatedScrollBarPage.xaml` | `ac83f7bc2a60aa693075fa8349589fd55044f48d` |
| `WinUIGallery\Samples\AnnotatedScrollBar\AnnotatedScrollBarPage.xaml.cs` | `0fc9f18000bc56ef5070d468c6dbfd9851bc7c6c` |
| `WinUIGallery\Samples\AnnotatedScrollBar\AnnotatedscrollbarLinkedScrollview.txt` | `2048d004a267ae774862c406a278f697cf24fb44` |
| `WinUIGallery\Assets\ControlImages\AnnotatedScrollBar.png` | `9a6352e4ae9204c3fabf1074a60c4777f56dc2c3` |

The single example is headed `AnnotatedScrollBar linked to a ScrollView.` It
uses a maximum-500 hidden-scrollbar `ScrollView`, an ItemsRepeater of 250
112x82 color cards in 120x90 cells, five section labels, event-driven detail
labels, a 4/0/48/0 control margin, and a 100-500 height slider that recomputes
label visibility. Its displayed snippet connects
`scrollView.ScrollPresenter.VerticalScrollController` to the control.

ModernWpf's actual sample uses a WPF `ScrollViewer` and WrapPanel, synchronizes
the source-shaped controller through ScrollChanged/Scrolling, and retains the
source counts, card metrics/colors, label-offset formula, tooltip mapping,
slider behavior, layout, margin, and 500-DIP control height. That is the
documented WPF runtime adapter. The displayed header and snippets had drifted
to the adapter vocabulary; they now reproduce the current source-facing
`ScrollView` header/XAML and controller assignment exactly while tests continue
to prove the live WPF adapter underneath.

## Pixel and interaction evidence

The harness crops the real ModernWpf artifact and reconstructs the exact WinUI
control from the first Azure label/adjacent scroll-presenter bounds. Both
current captures are exact 52x500. The mean-delta gate remains 1.5 and size
tolerance is now explicitly zero.

| Theme | Installed Gallery / ModernWpf crop | Primary delta |
| --- | --- | --- |
| Light | `52x500` / `52x500` | `1.20` |
| Dark | `52x500` / `52x500` | `1.21` |

- Light: `artifacts/visual-checks/20260718-193638-007-34996/report.md`.
- Dark: `artifacts/visual-checks/20260718-193710-698-37208/report.md`.
- Light scroll recording
  `artifacts/gallery-recordings/20260718-193743-939/report.md` passes with a
  `102.53` maximum local delta and ScrollPattern movement from 0% to 55%.
- Dark scroll recording
  `artifacts/gallery-recordings/20260718-193759-788/report.md` passes with a
  `102.133` maximum local delta and ScrollPattern movement from 0% to 55%.

## Validation

- Focused product/source tests pass 15/15 on `net8.0-windows7.0`, covering
  defaults, instance collection ownership, public properties, no standalone
  peer, template/resources/parts, labels, range validation, controller
  requests/cancellation/direction, panning, and detail-label ownership.
- Focused Gallery sample/source-shape/gate tests pass on both
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- Current live Light/Dark comparisons and scroll recordings pass as listed
  above.
- `ModernWpf.Controls` and `ModernWpf.Gallery` pass the retained `net462`
  target with zero warnings and zero errors.

Platform substitutions remain bounded to WPF ScrollViewer/controller event
synchronization, WrapPanel item layout, mouse capture, dispatcher layout,
no-op composition panning sources, WPF tooltip hosting, and direct resource
resolution. They do not change the current API, request semantics, label
geometry, source-facing Gallery content, accessibility ownership, or strict
control crop.

# InfoBadge current WinUI 3 source and Gallery parity audit

Date: 2026-07-18

## Pinned upstream snapshots

| Source | Commit | Purpose |
| --- | --- | --- |
| microsoft-ui-xaml | `de3e767333c2f0717a6a70cb22bd192ced5ad885` | Current `winui3/main` product source audited here. |
| microsoft-ui-xaml | `8463f45162149de0ec3ad7df752596893fe3e13e` | 2026-05-30 move from the src-prefixed mirror to the current root layout. |
| WinUI Gallery | `29f62479d5c046a0b854a5868e5a7cd484572d87` | Current Gallery source and installed-app comparison target. |
| WinUI Gallery | `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` | Gallery sample-folder conversion baseline. |

The prior product pin `c70471c511a0168b61dcca13af9556465f26b673`
and the current product pin have byte-identical InfoBadge runtime, generated,
IDL, template, theme, test, and TestUI inputs after path normalization. The
only package change is eight lines in `InfoBadge.vcxitems` registering the same
`InfoBadge.xaml` and `InfoBadge_themeresources.xaml` inputs as Performance2026
style/theme resources. There is no alternate InfoBadge template or runtime
behavior to port.

## Current WinUI product inventory

| File under microsoft-ui-xaml | Blob |
| --- | --- |
| `controls\dev\Generated\InfoBadge.properties.cpp` | `a2b399aa8b2bbf21c74fefce299de9d7ecc55150` |
| `controls\dev\Generated\InfoBadge.properties.h` | `48012d0c21a368165d8c339d28b558ec666c0185` |
| `controls\dev\Generated\InfoBadgeTemplateSettings.properties.cpp` | `f13815c3895e33cb25de3ff56b1c90f05f10327b` |
| `controls\dev\Generated\InfoBadgeTemplateSettings.properties.h` | `0a8d5f59f73b46b2220e025e69465d64b9db3b00` |
| `controls\dev\InfoBadge\InfoBadge.cpp` | `453a2655ed4dd3c78e33c9f08b149b25600db5bf` |
| `controls\dev\InfoBadge\InfoBadge.h` | `50ab8878b3b14bd420e53ab2d117a5143fe1bbbc` |
| `controls\dev\InfoBadge\InfoBadge.idl` | `3d9b9bea183cd492281dd3b0fcb23407adcb7f56` |
| `controls\dev\InfoBadge\InfoBadge.xaml` | `1f6ab40e7cc23c5bb7311a85006ec780e6a27d4f` |
| `controls\dev\InfoBadge\InfoBadgeTemplateSettings.cpp` | `a533d8a2bb93afbcffdd75228cba020eebb14c2c` |
| `controls\dev\InfoBadge\InfoBadgeTemplateSettings.h` | `d5a024fedae932fbf3d41cd3a6b534ced123f67d` |
| `controls\dev\InfoBadge\InfoBadge_themeresources.xaml` | `b09b56572ac8a2159bf9ba2dda7f063ec5460c6a` |
| `controls\dev\InfoBadge\InfoBadge.vcxitems` | `6b32b0d1d5964655fbd520597d5693381332102f` |
| `controls\dev\InfoBadge\APITests\InfoBadgeTests.cs` | `fadb132ac965d2bf7ec435edf011a19a6338de2c` |
| `controls\dev\InfoBadge\TestUI\InfoBadgePage.xaml` | `598139222d5e1a29797a86d0ac237017361e0c9f` |
| `controls\dev\InfoBadge\TestUI\InfoBadgePage.xaml.cs` | `40718da90fe3f19ca462852c5c0fd492ec4899fe` |
| `controls\dev\CommonStyles\Common_themeresources_any.xaml` | `ca07acb962a7c018a174bf28dcd0e945b13ddb4d` |

### API and behavior

The current public control surface is `Value`, `IconSource`, and read-only
`TemplateSettings`; template settings expose `InfoBadgeCornerRadius` and
`IconElement`. `Value` defaults to `-1`, values below `-1` throw, and measured
width is raised to at least measured height.

Display priority is exact: `Value >= 0` selects `Value`; otherwise a
`FontIconSource` selects `FontIcon`, another icon source selects `Icon`, and no
icon selects `Dot`. The source creates an icon element only in an icon state.
It does not clear the previous `TemplateSettings.IconElement` when `Value`
takes priority or when the badge returns to `Dot`. The element is merely hidden
by the visual state. ModernWpf previously cleared it in the dot branch; that
observable template-setting mismatch is now fixed and regression-tested.

The source updates `InfoBadgeCornerRadius` from half the actual height unless
`CornerRadius` has a local value. ModernWpf retains the source `SizeChanged`
path and its WPF lifecycle bridge: WPF raises that event after arrange, so the
same radius is seeded before the first template arrange. This prevents the
first NavigationView-hosted frame from rendering as a square.

### Template and resources

The source `DefaultInfoBadgeStyle` uses the `RootGrid`,
`DisplayKindStates`, `ValueTextBlock`, and `IconPresenter` shape. ModernWpf
maps WinUI Grid chrome to source-backed `GridEx`, visual-state setters to
`VisualStateEx`, and the presenter to `ContentPresenterEx`. Light, Dark, and
HighContrast dictionaries retain the source min/max metrics, 11-DIP value
font, 12-DIP icon width, Light/HighContrast 9-DIP icon height, Dark 8-DIP icon
height, padding, margins, foreground/background aliases, and attention,
informational, success, caution, and critical style families. In particular,
`InformationalDotInfoBadgeStyle` uses
`SystemFillColorSolidNeutralBrush`, not the default accent background.

### Accessibility boundary

Current WinUI has no `InfoBadgeAutomationPeer`, no peer override, and no
standalone automation role or pattern for the indicator. The containing
interactive control supplies the semantic notification. ModernWpf now pins the
same boundary: `InfoBadge` creates no standalone WPF peer, while the Gallery's
inbox `NavigationViewItem` has the current source name
`Inbox, 5 notifications`. This avoids announcing an unlabeled decorative
child separately from its actionable parent.

## Current WinUI Gallery inventory

The current converted Gallery page is unchanged after
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`:

| File under WinUI-Gallery | Blob |
| --- | --- |
| `WinUIGallery\Samples\InfoBadge\InfoBadgePage.xaml` | `0615fb945616d35d6fb5082c2e9029cfca167d74` |
| `WinUIGallery\Samples\InfoBadge\InfoBadgePage.xaml.cs` | `6a97ca72fc8e5cf65e15a525df82a34d05142ee3` |
| `WinUIGallery\Samples\InfoBadge\DifferentInfobadgeStyles.txt` | `f80193f2d2b06931a0c2009f93f51bff36e064cc` |
| `WinUIGallery\Samples\InfoBadge\InfobadgeDynamicValue.txt` | `db5b7520cf7d98697ec5303c9ede9d66992c7cf9` |
| `WinUIGallery\Samples\InfoBadge\InfobadgeEmbeddedNavigationview.txt` | `5c00eef2c46cbe4ae3b40b41558bd8ea98b108a2` |
| `WinUIGallery\Samples\InfoBadge\PlacingInfobadgeInsideAnother.txt` | `f1ac3d32b20fd740d1492c573b31ce436a211709` |
| `WinUIGallery\Assets\ControlImages\InfoBadge.png` | `21263ace6d6f607d1e8a3e3a5337b99a0196c77f` |

Its four examples are:

1. A 300-DIP NavigationView with a value-5 inbox badge, opacity toggle, and
   LeftExpanded, LeftCompact, and Top display modes.
2. Attention, Informational, Success, and Critical icon/value/dot styles.
3. A red icon badge inside a 200x60 refresh button.
4. A dynamic value badge driven by an inline NumberBox with minimum `-1`.

`ModernWpf.Gallery\Pages\StatusInfoSampleFactory.cs` preserves the headers,
snippets, source-facing names, accessible inbox name, default values, four
examples, option transitions, style families, embedded-button layout, and
dynamic value behavior. Direct property/resource application remains the
documented WPF substitute for resolving keyed styles in the isolated test host.

## Pixel and interaction evidence

The installed-Gallery harness renders the real ModernWpf badge artifact and
finds the first WinUI value badge from its accent pixels. Missing accent
detection fails rather than falling back to the sample. Both current crops are
exact 16x16 and the size tolerance is now explicitly zero.

| Theme | Installed Gallery / ModernWpf crop | Primary delta |
| --- | --- | --- |
| Light | `16x16` / `16x16` | `4.44` |
| Dark | `16x16` / `16x16` | `3.73` |

- Light: `artifacts/visual-checks/20260718-190042-656-45992/report.md`.
- Dark: `artifacts/visual-checks/20260718-190124-362-45024/report.md`.
- Both pass the strict `5.0` mean-delta and zero-size-tolerance gates. The
  remaining sparse pixels are WPF/WinUI glyph antialiasing; the accent fill is
  pixel-identical on 137 of the 256 pixels in both themes.
- Light option recording
  `artifacts/gallery-recordings/20260718-190209-312/report.md` passes with a
  `4.072` local delta while toggling opacity.
- Dark option recording
  `artifacts/gallery-recordings/20260718-190250-370/report.md` passes with a
  `3.393` local delta while toggling opacity.

## Validation

- Focused product/source/raster tests pass 12/12 on `net8.0-windows7.0`, covering
  state priority, all WPF icon types, retained icon settings, no standalone
  peer, template/resources/styles, radius lifecycle, rounded raster corners,
  and value validation.
- Focused Gallery sample/crop/gate tests pass 3/3 on both
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- Current live Light/Dark comparisons and option recordings pass as listed
  above.
- `ModernWpf.Controls` and `ModernWpf.Gallery` pass the retained `net462`
  target with zero errors. Controls reports 18 existing unrelated
  NavigationView/PersonPicture/ItemsRepeater warnings; Gallery reports zero
  warnings.

Platform substitutions remain bounded to WPF dependency-property validation,
`GridEx`/`ContentPresenterEx`/`VisualStateEx`, the pre-arrange radius bridge,
and keyed-style test-host application. None changes the current public API,
visible state table, accessible ownership, Gallery behavior, or strict crop
geometry.

# InfoBar current WinUI 3 source and Gallery parity audit

Date: 2026-07-18

## Pinned upstream snapshots

| Source | Commit | Purpose |
| --- | --- | --- |
| microsoft-ui-xaml | `de3e767333c2f0717a6a70cb22bd192ced5ad885` | Current `winui3/main` product source audited here. |
| microsoft-ui-xaml | `8463f45162149de0ec3ad7df752596893fe3e13e` | 2026-05-30 move from the src-prefixed mirror to the current root layout. |
| microsoft-ui-xaml | `beabd047460bf5d43a41fcf8bddf7730188bd5a7` | Performance2026 packaging registration affecting `InfoBar.vcxitems` only. |
| WinUI Gallery | `29f62479d5c046a0b854a5868e5a7cd484572d87` | Current Gallery source and installed-app comparison target. |
| WinUI Gallery | `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` | Gallery sample-folder conversion baseline. |

The prior product pin `c70471c511a0168b61dcca13af9556465f26b673`
and the current product pin have byte-identical InfoBar runtime, generated,
IDL, template, theme, interaction-test, string, and TestUI inputs after path
normalization. The only package change is six lines in `InfoBar.vcxitems`
registering the same `InfoBar.xaml` and `InfoBar_themeresources.xaml` inputs as
Performance2026 style/theme resources. There is no alternate InfoBar runtime
or template behavior to port.

## Current WinUI product inventory

| File under microsoft-ui-xaml | Blob |
| --- | --- |
| `controls\dev\Generated\InfoBar.properties.cpp` | `53d316dcd29b110f1ccd905973f4dd9233eeec05` |
| `controls\dev\Generated\InfoBar.properties.h` | `77b4c6509468032c6fef2753420e2db116071e73` |
| `controls\dev\Generated\InfoBarAutomationPeer.properties.cpp` | `354a5c2f48d7a3edeb2a965e97f796fb3f8badd7` |
| `controls\dev\Generated\InfoBarClosedEventArgs.properties.cpp` | `2a591720592d2b96206e744965cde8f57e4bed91` |
| `controls\dev\Generated\InfoBarClosingEventArgs.properties.cpp` | `b92a9fb4e011d4780562c11b0afeba34d17b4b9c` |
| `controls\dev\Generated\InfoBarOpenedEventArgs.properties.cpp` | `46ce254ef6d538a7a3e38cb4854192fafd235a3f` |
| `controls\dev\Generated\InfoBarPanel.properties.cpp` | `34571f9b3857e95e9cb8fa9b58798f2c77442b67` |
| `controls\dev\Generated\InfoBarPanel.properties.h` | `0db31d720b658ff551b50577d8f0fe3a7214587d` |
| `controls\dev\Generated\InfoBarTemplateSettings.properties.cpp` | `b255b262fa252a4100fdc5ca90b7668612121436` |
| `controls\dev\Generated\InfoBarTemplateSettings.properties.h` | `9a584bf6cb34342ca2350a019cc997a04447e2f8` |
| `controls\dev\InfoBar\InfoBar.cpp` | `4229e91b7384b94539f0897822dac2a320ca07aa` |
| `controls\dev\InfoBar\InfoBar.h` | `4e5e6d2d0bd9f925ffe193688741abb267877fde` |
| `controls\dev\InfoBar\InfoBar.idl` | `5b290141ae4013dd397e33cb415ab5c2c0aaa3f6` |
| `controls\dev\InfoBar\InfoBar.vcxitems` | `7a8a32024626eeba9b9407652ec902577ab10b9f` |
| `controls\dev\InfoBar\InfoBar.xaml` | `1f036889684c7af85187811a1ef776d059eed2a8` |
| `controls\dev\InfoBar\InfoBarAutomationPeer.cpp` | `b78bf1a698b1c38169fa9a60f389c1ef2cd9b3db` |
| `controls\dev\InfoBar\InfoBarAutomationPeer.h` | `0b9f7db9cd79d65a9e95e1f63b6fe713d45bb714` |
| `controls\dev\InfoBar\InfoBarPanel.cpp` | `9cb2a37cb70060d9ab4aa3a3e499169f26eb85a8` |
| `controls\dev\InfoBar\InfoBarPanel.h` | `5dfa676057e905dce4bcfebcab82e3be97d58bcf` |
| `controls\dev\InfoBar\InfoBarTemplateSettings.cpp` | `ce9c1a509675e6c5de48457c989af6a4e95b9c90` |
| `controls\dev\InfoBar\InfoBarTemplateSettings.h` | `dc4361b792c242837317fcc21b0a4d597b6e6bbd` |
| `controls\dev\InfoBar\InfoBar_themeresources.xaml` | `1056af57d0340a7e9aa2d5f9e2f2f177b841897c` |
| `controls\dev\InfoBar\InteractionTests\InfoBarTests.cs` | `65cd8db3b914853c2cc02d5ec7201c072d2e8be1` |
| `controls\dev\InfoBar\Strings\en-us\Resources.resw` | `e8b55610db74a70136be4a392ff96bef89468078` |
| `controls\dev\InfoBar\TestUI\InfoBarPage.xaml` | `817adcc1fa06d8f987d78f26caaf6abff14b8457` |
| `controls\dev\InfoBar\TestUI\InfoBarPage.xaml.cs` | `8fce72e65e348cb28e1487c76b88ea52e2525eca` |
| `dxaml\xcp\core\text\TextBlock\TextBlock.cpp` | `9c17a3ca8aa9e3e58fb3ad2a8ee81be5671deca9` |

### API, events, and state machine

Current defaults are exact: `IsOpen=false`, empty title/message, informational
severity, null icon/action/content/template, `IsIconVisible=true`, and
`IsClosable=true`. The close-button command, command parameter, style, custom
content, and read-only template-settings surface are also retained.

Setting `IsOpen=true` resets the close reason to `Programmatic`, enters
`InfoBarVisible`, updates the automation view, and raises the prerelease
`Opened` event. Closing programmatically raises `Closing: Programmatic` then
`Closed: Programmatic`. The close button raises `CloseButtonClick` first, then
`Closing: CloseButton`, then `Closed: CloseButton`; cancelling `Closing`
restores `IsOpen=true` without a `Closed` event. ModernWpf preserves that exact
ordering and cancellation path.

Severity falls back to `Informational` for unknown enum values and updates the
localized standard-icon automation name. Icon selection is exact: a non-null
`IconSource` creates and exposes a user icon, null clears
`TemplateSettings.IconElement`, and `IsIconVisible=false` hides both standard
and custom icons. `IsClosable` selects the close-button state. Local
`Foreground` overrides title and message through the source visual state.
Title, message, and action-button changes choose `BannerContent`; only when all
three are absent does `NoBannerContent` move custom content to row zero.

### Template, resources, and panel layout

The port retains the source `SeverityLevels`, `IconStates`,
`CloseButtonStates`, `InfoBarVisibility`, `ForegroundStates`, and
`ContentStates` groups; `ContentRoot`, `LayoutRoot`, `StandardIconArea`,
`UserIconBox`, `InfoBarPanel`, `ContentArea`, and `CloseButton` slots; source
48-DIP minimum height, padding/margins, fonts, glyphs, corner radius, border,
severity brushes, action styling, and Light/Dark/HighContrast resource aliases.
The close button keeps source command bindings, 38-DIP chrome, top/right
placement, tooltip/name resources, AppBar button aliases, and Cancel symbol.

`InfoBarPanel` follows the current source horizontal/vertical decision and
margin rules: it stacks vertically for one visible child, overflow, or an item
taller than the parent minimum-height budget, and gives remaining horizontal
space to the last child. WPF preserves fractional TextBlock metrics where
WinUI ceilings page-node dimensions to physical pixels, so the port explicitly
ceilings measured child sizes and applies WinUI's TextBlock layout-rounding
height adjustment during arrange. That bounded framework bridge is what keeps
the installed Gallery crops at the exact 560x95 source size.

### Accessibility boundary

Current WinUI creates an `InfoBarAutomationPeer` with
`AutomationControlType.StatusBar` and class `InfoBar`. An open InfoBar is in
the control accessibility view; a closed/default InfoBar moves to the raw view
and disappears from the normal accessible tree. Open/close notification
processing is `ImportantAll` for Warning/Error and
`CurrentThenMostRecent` otherwise. The standard icon and close button receive
localized names, and the InfoBar receives the localized custom landmark name.

ModernWpf reports the same StatusBar role, class, localized child names, and
open-only control-tree membership. WPF/net462 exposes neither WinUI's
`AutomationProperties.AccessibilityView`, notification-event API, nor
`LocalizedLandmarkType`; `IsControlElementCore` is the closed/raw substitute
and peer invalidation is the notification substitute. These are platform API
boundaries, not visible or semantic ownership changes.

## Current WinUI Gallery inventory

The converted Gallery sample is unchanged after
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`:

| File under WinUI-Gallery | Blob |
| --- | --- |
| `WinUIGallery\Samples\InfoBar\InfoBarPage.xaml` | `be47e83aaa7456db470130bd5154276534cc40f7` |
| `WinUIGallery\Samples\InfoBar\InfoBarPage.xaml.cs` | `ad60e9e14ad011fc33cc9983557f84b1a9372874` |
| `WinUIGallery\Samples\InfoBar\ClosableInfobarLongShort.txt` | `b3110c62d4d76f7909f102ade44150ac779fe449` |
| `WinUIGallery\Samples\InfoBar\ClosableInfobarOptionsChange.txt` | `b5e4033a811f2d6201a5541355f64ce1803a8658` |
| `WinUIGallery\Samples\InfoBar\ClosableInfobarOptionsDisplay.txt` | `ace93fb54ac65eb82352fa85fd717daad2c085cf` |
| `WinUIGallery\Assets\ControlImages\InfoBar.png` | `3d9f9d7f2fb7fe90c9d66044f4febb3823ebf641` |

Its three examples are:

1. An open, closable informational bar with title/message, an `Is Open`
   checkbox, and Informational, Success, Warning, and Error severity options.
2. An open titled bar with short/long message options and None, Button, and
   Hyperlink action options. The current default is the full long message,
   displayed in the snippet as `A long essential app message...`.
3. An open bar with independent `Is Open`, `Is Icon Visible`, and
   `Is Closable` options.

`ModernWpf.Gallery\Pages\StatusInfoSampleFactory.cs` preserves the three
headers, source snippets/substitutions, 560-DIP bar width, 150-DIP options
column, source-facing option names, defaults, severity routing, exact short and
long messages, action content/URI, and all visibility transitions. Focused
Gallery tests now execute every option family instead of checking only initial
state.

## Pixel and interaction evidence

The installed-Gallery harness compares the first ModernWpf bar artifact with
WinUI Gallery `TestInfoBar1`. Both current captures are exact 560x95. The mean
delta gate remains 2.0 and the size tolerance is now explicitly zero.

| Theme | Installed Gallery / ModernWpf crop | Primary delta |
| --- | --- | --- |
| Light | `560x95` / `560x95` | `1.33` |
| Dark | `560x95` / `560x95` | `1.46` |

- Light: `artifacts/visual-checks/20260718-192111-466-94688/report.md`.
- Dark: `artifacts/visual-checks/20260718-192130-115-16160/report.md`.
- Light option recording
  `artifacts/gallery-recordings/20260718-192210-567/report.md` passes with a
  `4.47` maximum local delta while `Is Open` changes from On to Off.
- Dark option recording
  `artifacts/gallery-recordings/20260718-192227-771/report.md` passes with an
  `8.786` maximum local delta while `Is Open` changes from On to Off.

The earlier exact-size 2026-07-17 captures are superseded by these fresh runs;
the numeric primary deltas remain identical, confirming that the stricter size
gate and the current-source re-audit did not hide a changed rendering baseline.

## Validation

- Focused product/source tests pass 12/12 on `net8.0-windows7.0`, covering
  defaults, close/cancel ordering, icon reset and visibility, severity/content
  states, resources/HighContrast, foreground, close chrome, automation role
  and open-only tree membership, source panel layout, physical-pixel ceilings,
  TextBlock render adjustment, and the current-source audit pin.
- Focused Gallery sample/crop/gate tests pass 3/3 on both
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- Current live Light/Dark comparisons and option recordings pass as listed
  above.
- `ModernWpf.Controls` and `ModernWpf.Gallery` pass the retained `net462`
  target with zero warnings and zero errors.

Platform substitutions remain bounded to WPF dependency properties,
`Border`/`ContentPresenterEx`/`VisualStateEx`, physical-pixel TextBlock layout,
and the automation APIs described above. The current product and Gallery audit
found no additional runtime or template drift requiring a control-code change;
this refresh adds stricter size enforcement, current provenance, and regression
coverage for the source-observable icon reset and complete Gallery option set.

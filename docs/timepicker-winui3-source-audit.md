# TimePicker WinUI 3 Source Audit

Date: 2026-08-08

ModernWpf `TimePicker` is a source-backed WPF adaptation of the official
`microsoft-ui-xaml` `winui3/main` tree at commit
`6a556bb28fc227acd2ec8fe67ee64853f559084b` (2026-08-08). The Gallery
contract is pinned to WinUI Gallery commit
`3669519356c67f1376152c33ed8ea45003a91f3a` (2026-08-06). These immutable
revisions are the Preview 3 cutoff recorded in
`docs/winui3-sync-2026-08-08-preview3.md`.

## Product source baseline

Primary current WinUI inputs and blob IDs:

| Source | Current blob |
| --- | --- |
| `controls/dev/CommonStyles/TimePicker_themeresources.xaml` | `2cfb8a7df11925a858a9c877c0886c2ccc3231bf` |
| `controls/dev/CommonStyles/TimePicker_themeresources_perf2026.xaml` | `9c1d6ba50c49f52648bca4e950d28aeb5ab33d55` |
| `dxaml/xcp/tools/XCPTypesAutoGen/XamlOM/Model/Microsoft.UI.Xaml.Controls.cs` | `cf26552e09287ac315955985086b5da27e9b440b` |
| `dxaml/xcp/tools/XCPTypesAutoGen/XamlOM/Model/Microsoft.UI.Xaml.Automation.Peers.cs` | `abe13005f2d704be462949ec7ba4543ca40fe43f` |
| `dxaml/xcp/dxaml/lib/TimePicker_Partial.cpp` | `cfe4eca2cba02bd9c12549425ec080ecf50a3d2d` |
| `dxaml/xcp/dxaml/lib/TimePicker_Partial.h` | `788034e244aa4d871e21297be58e2f4834062d0a` |
| `dxaml/xcp/dxaml/lib/TimePickerAutomationPeer_Partial.cpp` | `95c9752724554d40e851b57ceec44ebe30b72247` |
| `dxaml/xcp/dxaml/lib/TimePickerAutomationPeer_Partial.h` | `1a7270f56ba0b2fd7b16d6ded63ddc71dc51db98` |
| `dxaml/phone/lib/TimePickerFlyout_Partial.cpp` | `59ca2cdb06e3b9c1dd29ee0b63d1d9ade3a3c335` |
| `dxaml/phone/lib/TimePickerFlyout_Partial.h` | `e04859ce8a58f81b8ce2ff1c1c29ccf152dba340` |
| `dxaml/phone/lib/TimePickerFlyoutPresenter_Partial.cpp` | `dfa763828ae36228594968e4f60e2ad4cbe19ed3` |
| `dxaml/phone/lib/TimePickerFlyoutPresenter_Partial.h` | `98198cfd0218c6bdefc34d4fe7ffc4b0add0d2ec` |
| `dxaml/test/native/external/controls/timepicker/TimePickerIntegrationTests.cpp` | `b7ee6f14031aba9345a23df3a940126e53d42399` |
| `dxaml/test/native/external/controls/timepickerflyout/TimePickerFlyoutIntegrationTests.cpp` | `e831f93398f9d18d093766bcc04117352b04be03` |
| `dxaml/test/native/external/controls/timepickerflyout/TimePickerFlyoutAutomationIntegrationTests.cpp` | `2e0b17ab6919ab81b11711f889ff392077440907` |

## Current Gallery baseline

| Gallery source | Current blob |
| --- | --- |
| `WinUIGallery/Assets/ControlImages/TimePicker.png` | `c63aca390ddef62974dc992e9b68544d4458e8ec` |
| `WinUIGallery/Samples/TimePicker/TimePickerPage.xaml` | `fd714a5df8be57ced001f1bcae704c45a9079155` |
| `WinUIGallery/Samples/TimePicker/TimePickerPage.xaml.cs` | `b0b0fb503ffda006f748e0815ec92ec91480f9c3` |
| `WinUIGallery/Samples/TimePicker/SimpleTimepicker.txt` | `4a7ed1bcb73916cd081284021d1ece166062a8e3` |
| `WinUIGallery/Samples/TimePicker/TimepickerHeaderMinuteIncrements.txt` | `da3e955ed32b54eea364437c1bb4c7d1bfa311b6` |
| `WinUIGallery/Samples/TimePicker/Timepicker24HourClock.txt` | `ad1c8b248626baba2e08b232463bea4c08bb6773` |

The current Gallery page has exactly three examples: a simple picker; a picker
with `Header="Arrival time"` and `MinuteIncrement="15"`; and a picker using
`ClockIdentifier="24HourClock"` initialized to the current time. ModernWpf
keeps those headings, displayed definitions, and runtime behaviors.

## Public and behavioral contract

- `Header`, `HeaderTemplate`, `ClockIdentifier`, `MinuteIncrement`, `Time`,
  `LightDismissOverlayMode`, nullable `SelectedTime`, and `HeaderPlacement`
  retain the current dependency-property names and types. WPF additionally
  exposes the established package-wide `CornerRadius` convention.
- The system clock is the default clock identifier. `MinuteIncrement` accepts
  0 through 59, with 0 producing the single `00` minute item.
- `TimeSpan(-1 tick)` is the null sentinel for `Time`. Other negative values
  fail; values beyond one day wrap into a 24-hour range; seconds are removed;
  and minutes round down to the current increment.
- WPF validates negative values before assignment, then performs source-shaped
  normalization through a second `Time` change. This preserves WinUI's
  raw-to-normalized event arguments instead of hiding the first value inside a
  WPF coercion callback.
- `Time` and `SelectedTime` propagate using the source guard. A direct
  `SelectedTime` assignment remains the assigned nullable value while its
  propagated `Time` value is coerced. A later `Time` or increment change
  propagates the resulting coerced value back to `SelectedTime`.
- A committed `Time` change raises `TimeChanged` followed by
  `SelectedTimeChanged`, with source-shaped old/new event arguments.
- Placeholder, hour, minute, period, accept, dismiss, and automation strings
  come from the ModernWpf resource pack. Twelve-hour and 24-hour field order
  follows the active culture.
- The source `TimePickerButtonForegroundDefault` and
  `TimePickerFlyoutPresenterHighlightForegroundColor` resource keys are
  additive Preview 3 public resources and are recorded for Light, Dark, and
  High Contrast in `ModernWpf/PublicResourceKeys.Unshipped.txt`.
- The control peer reports class `TimePicker`, Group role, explicit automation
  name first, then header text, then localized `Time picker`. The interactive
  button includes header/selected value plus the localized control type.

## WPF adaptations

- WinUI's asynchronous `TimePickerFlyout`, `LoopingSelector`, phone presenter,
  composition shadow, and calendar projection are represented by a WPF
  `Popup`, finite `ListBox` selectors, .NET system-culture formatting, and
  explicit accept/dismiss buttons. The automation value transforms the
  culture's short-time pattern to the requested 12- or 24-hour clock while
  preserving localized field order and period placement. The flyout and
  presenter remain implementation details rather than invented package API.
- The WPF popup retains pointer light-dismiss and Escape cancellation. Space or
  Enter reaches the inner WPF button, while Alt+Down and Alt+Up use the
  source keyboard route. `LightDismissOverlayMode` is retained for API parity;
  WPF has no system flyout overlay policy, so it does not create a synthetic
  window-wide overlay.
- WinUI raw-view annotations have no direct WPF attached-property equivalent.
  The outer Group peer and named interactive button are authoritative, while
  decorative time-field text remains inside the button's automation subtree.
- `AccentAAFillColorDefaultBrush` has no separate ModernWpf token. The selected
  flyout row uses `AccentFillColorDefaultBrush`; its source
  `TimePickerFlyoutPresenterHighlightForegroundColor` key still resolves to
  the AA-derived text-on-accent color in Light and Dark.
- WinUI can use a transparent replacement color in High Contrast because its
  `MonochromaticOverlayPresenter` preserves the source selector text. WPF
  renders the selected `ListBoxItem` text directly, so the same public color
  key maps to `SystemColorHighlightTextColor` in High Contrast to keep text
  legible over `SystemColorHighlightColorBrush`.
- Normal-state aliases follow the current source: the elevation border,
  secondary pointer fill, tertiary pressed fill, disabled control fill, and
  secondary pressed text all flow through the TimePicker-specific button
  template. High Contrast uses the exact system-color roles, including
  `SystemColorButtonFaceColorBrush`, `SystemColorHighlightTextColorBrush`,
  `SystemColorWindowTextColorBrush`, and `SystemColorHighlightColorBrush`.
- WinUI's `%1 %2 time picker` resource is represented by the localized .NET
  format string `{0} {1} time picker`; the WPF adapter removes empty-field
  whitespace before assigning the inner button's automation name.

## Regression guards

- `TimePickerSourceAuditTests` pins the immutable product/Gallery revisions,
  every source blob above, the public shape, WPF substitutions, resource
  mapping, Gallery examples, and package inventories.
- `TimePickerApiTests` covers defaults, validation/coercion, propagation and
  event order, 0/5-minute selectors, placeholders, 12/24-hour display and
  culture ordering, accept/cancel behavior, header placement, keyboard routes,
  and automation. `CommonStylesResourceTests` pins the Light, Dark, and High
  Contrast aliases.
- Gallery tests cover all three current official examples, page metadata,
  automation anchors, selection/output behavior, and theme rendering.

## Current validation

The focused API/behavior and source-audit slice passes 12/12 on
`net8.0-windows7.0`, including keyboard accept/cancel and disabled opening.
The complete Gallery suite passes 727/727 on both Gallery targets, and the
serialized Release solution build succeeds for all supported package targets
with zero warnings or errors. Final Preview 3 acceptance still requires the
hosted complete WinUI suite three consecutive times, package verification and
consumers, downstream canaries, and final-tip Light, Dark, and real OS High
Contrast Gallery evidence.

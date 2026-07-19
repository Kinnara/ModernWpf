# RadioMenuFlyoutItem current WinUI source audit

Audit date: 2026-07-19

## Authorities and bounded history

The product authority is `microsoft/microsoft-ui-xaml` `main` at commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`. Every object below was read from
that commit, rather than from the older upstream worktree checkout:

| Current product object | Blob |
| --- | --- |
| `controls/dev/RadioMenuFlyoutItem/RadioMenuFlyoutItem.cpp` | `4d827b32f1b6eaca140f4d0298caa2a3ef47c542` |
| `RadioMenuFlyoutItem.h` | `2e4a02d2d7a4c69ece2e2a1c9e61c79f364f357c` |
| `RadioMenuFlyoutItem.idl` | `253f39404fa6a6741139a9eb9af3e82049b3d5a0` |
| `RadioMenuFlyoutItem.vcxitems` | `c14ecf5a6329a64acef1d71c2413c03acf9d2db8` |
| `RadioMenuFlyoutItem_themeresources.xaml` | `078fadf4058d7c6b269b335350a075d6c079ab03` |
| `RadioMenuFlyoutItem_themeresources_perf2026.xaml` | `4f03dc205b6cd0271015076444cc40bbe183f345` |
| `InteractionTests/RadioMenuFlyoutItemTests.cs` | `c309a8ec605a9fcf2fb0a4ed8624c55c2e82aef1` |
| `TestUI/RadioMenuFlyoutItemPage.xaml` | `50f1912406476ec663810408a0318293ca6e96d8` |
| `TestUI/RadioMenuFlyoutItemPage.xaml.cs` | `e3631219660cb9f10254e570c41fd0220b8df502` |
| `dxaml/xcp/dxaml/lib/ToggleMenuFlyoutItemAutomationPeer_Partial.cpp` | `18a783d06015f40c1e21bf0eb4870162b99fd066` |
| `dxaml/xcp/dxaml/lib/MenuFlyoutItemAutomationPeer_Partial.cpp` | `25492975cb3ccc159f5c5986b204d9ad3ed7bb73` |

The runtime, header, IDL, classic theme, interaction suite, TestUI page, and
both inherited automation peers are byte-identical to previous product
baseline `c70471c511a0168b61dcca13af9556465f26b673`. The bounded post-baseline
history is packaging and theme-performance work:

- `beabd047460bf5d43a41fcf8bddf7730188bd5a7` enables generation and runtime
  selection of the perf2026 dictionaries.
- `49b4d5326b4deba8c036e63a7e676715a5de4f3a` creates the radio perf2026
  dictionary. Its only differences from the classic radio dictionary are two
  zero-duration `NarrowPadding` object animations replaced by equivalent
  `LayoutRoot.Padding` setters: one in the item style and one in the radio
  submenu style.
- mirror commits `5e04eeb82cdab8f66d5d98f066c8914cd6b00b51` and
  `51d82696da7f65c69e6479420a879a8600817401` remove and restore generated
  perf content without changing the runtime contract.
- `8463f45162149de0ec3ad7df752596893fe3e13e` only moves the mirrored source
  tree from `src/` to the repository root.

No current product behavior or classic visual change justifies a new
ModernWpf runtime patch.

## Current behavior and template contract

WinUI publicly derives `RadioMenuFlyoutItem` from `MenuFlyoutItem`, while its
private helper composes `ToggleMenuFlyoutItem`. The public `IsChecked` and
`GroupName` properties are therefore backed by toggle behavior without
changing the public base type. A thread-local weak map retains at most one
checked item per group. Checking a new item safely unchecks the previous one;
direct or user interaction cannot uncheck the selected item. A checked item
reclaims its group on load and leaves the active map on unload without being
forced unchecked.

ModernWpf's existing `RadioMenuItem : MenuItem` maps that contract onto WPF:

- `IsCheckable` is coerced true and `GroupName` defaults to the empty string;
- the weak group map and `m_isSafeUncheck` guard implement the same selection
  and direct-uncheck rules;
- load/unload use the cached checked/group values required by the source
  lifetime behavior;
- `AreCheckStatesEnabled` mirrors a checked radio child onto the WPF submenu
  header so the radio placeholder tracks nested selections;
- common, check, icon, and accelerator states use `VisualStateEx` setters;
- the checked mark is a 12-DIP WPF geometry fallback for WinUI's `E915` mark,
  with the same zero/full opacity behavior and `0,0,16,0` item margin.

The standard desktop path uses the current `11,8,11,9` item padding and
`11,4,11,5` narrow resource. WPF has no WinUI `InputDeviceType`-driven
`MenuFlyoutPresenter` density transition, so the two source `NarrowPadding`
states are recorded as a host boundary rather than claimed as reachable WPF
popup behavior. The current desktop Gallery path is the standard state.

## Accessibility mapping

The source's private toggle base selects the current
`ToggleMenuFlyoutItemAutomationPeer`: control type MenuItem, Toggle pattern,
On/Off state, enabled guard, and toggle-through-invoke behavior. The current
peer blob is byte-stable.

WPF's `MenuItemAutomationPeer` supplies the same externally useful contract
for `RadioMenuItem`: MenuItem control type, the header-derived accessible name,
Toggle provider, and On/Off state. Focused tests prove an unchecked peer can
select itself, the old group member reports Off, and invoking Toggle again on
the selected peer leaves it On. WPF reports its own class name and owns popup
HWND/focus/event plumbing; those platform details are not represented as
WinRT peer identity parity.

## Current Gallery consumer

The live sample authority is `microsoft/WinUI-Gallery` `main` at commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`:

- `WinUIGallery/Samples/MenuFlyout/MenuFlyoutPage.xaml` —
  `834f7da9d1a314fa30c7a92e8814f2bc4aa889c0`;
- `MenuflyoutRadiomenuflyoutitems.txt` —
  `099d74fd883a104cbd45e364f0d5fd1a361b8935`.

Gallery migration commit `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`
only converts the page path/host; the radio example still contains Landscape
and Portrait in `OrientationGroup`, plus Small, Medium, and Large icons in
`SizeGroup`, with Portrait and Medium selected initially.

ModernWpf consumes that exact current snippet and constructs the same two
groups and defaults. The Gallery regression now also pins Landscape and
Portrait as named MenuItem/Toggle providers, selects Landscape through the
provider, observes Portrait turn Off, and proves Landscape cannot toggle
itself off.

## Visual and interaction evidence

RadioMenuItem shares the current MenuFlyout presenter, typography, item
metrics, colors, standard padding, popup chrome, and High Contrast resources.
Fresh installed-Gallery family evidence remains:

- Light `artifacts/visual-checks/20260719-004427-629-26672/report.md` — exact
  `68x64` closed crop at `0.57` and exact `96x102` open crop at `6.70`;
- Dark `artifacts/visual-checks/20260719-004459-735-37204/report.md` — exact
  `68x64` closed crop at `0.55` and exact `96x102` open crop at `4.01`;
- Light `artifacts/gallery-recordings/20260719-004531-064/report.md` and Dark
  `artifacts/gallery-recordings/20260719-004623-068/report.md` prove live menu
  open, leaf invocation, dismissal, and reopen.

Those artifacts exercise the shared first MenuFlyout example (`By rating`),
not the seventh radio example. They are valid shared presenter/chrome evidence
but **not an isolated radio-item pixel proof**. The radio-specific mark,
checked opacity, icon/accelerator layout, standard metrics, submenu placeholder,
groups, guarded selection, and UIA states are deterministic source/API/Gallery
regressions. No standalone current WinUI Gallery page or isolated harness target
is fabricated.

## WPF substitutions and verification

- WPF uses `MenuItem`, `Header`, `InputGestureText`, and its menu popup host in
  place of WinUI's `MenuFlyoutItem`, `Text`, keyboard-accelerator collection,
  and composition popup.
- The process-wide weak map is used only by dispatcher-owned WPF controls;
  WinUI's implementation uses thread-local storage.
- WPF observes submenu children and collection changes to keep the shared
  submenu-header check mark current; WinUI reevaluates its native
  `MenuFlyoutSubItem` on load.
- The 12-DIP geometry fallback replaces the platform symbol-font renderer but
  retains the mapped radio mark size, margin, color, and opacity contract.
- Native popup automation wrappers, WinRT event plumbing, `AccessibilityView`,
  acrylic/composition, and input-device density selection remain platform
  boundaries.

Focused product coverage includes source pins, API/template/resource checks,
group selection, guarded uncheck, load/unload map lifetime, submenu state, and
the MenuItem/Toggle automation contract. Focused Gallery coverage includes the
current seven-example MenuFlyout consumer and its source/gate tests on both
`net8.0-windows7.0` and `net10.0-windows7.0`. Shared Controls/Gallery builds
cover `net462`, net8, and net10.

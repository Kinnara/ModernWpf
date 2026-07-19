# AppBarButton / AppBarToggleButton / AppBarSeparator / AppBarElementContainer WinUI 3 Source Audit

Date: 2026-07-19

Scope: the existing `AppBarButton`, `AppBarToggleButton`, `AppBarSeparator`,
and `AppBarElementContainer` controls. This audit maps the WPF implementation
to current official WinUI 3 source, current WinUI Gallery source and live
rendering/interaction, and the WPF substitutions that remain because the WinUI
implementation depends on XAML platform services that do not exist in WPF.

## WinUI 3 Source Baseline

Official `microsoft/microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885` (2026-07-17) is the current
source baseline. History queries for the three CommonStyles resources and the
four partial implementations found no substantive runtime or classic-theme
AppBar change after the May audit. The only later matching commit for those
authorities is
`8463f45162149de0ec3ad7df752596893fe3e13e` (2026-05-30), which moved the
`winui3/main` mirror to the root layout and removed the old `src/` prefix.

Current classic/perf theme blobs are:

- AppBarButton classic `60bcf2af4daf2ab40dd79709d3a0a7907f79b8c3`;
- AppBarButton perf2026 `c132a7bfd76806c1eff80e1072176d8d16fdf7d6`;
- AppBarToggleButton classic `2c3e6c5ad5aa3d6c4cd6fe1e3c8c5688b8e31f18`;
- AppBarToggleButton perf2026 `a7420f339171d12ac575be68c68036ac2561e349`;
- AppBarSeparator classic `51801458b679a563d21471423f4ebb94f0b13ad0`.

The perf2026 dictionaries added by the current perf-resource rollout replace
zero-duration Compact, LabelOnRight, and LabelCollapsed assignment animations
with equivalent setters. ModernWpf already represents these states with
`VisualStateEx.Setters`; no metric, color, state, behavior, or accessibility
change is introduced by the variants.

- `dxaml/xcp/dxaml/lib/AppBarButton_Partial.cpp`
- `dxaml/xcp/dxaml/lib/AppBarToggleButton_Partial.cpp`
- `dxaml/xcp/dxaml/lib/AppBarSeparator_Partial.cpp`
- `dxaml/xcp/dxaml/lib/AppBarElementContainer_Partial.cpp`
- `dxaml/xcp/dxaml/lib/AppBarButtonAutomationPeer_Partial.cpp`
- `dxaml/xcp/dxaml/lib/AppBarToggleButtonAutomationPeer_Partial.cpp`
- `dxaml/xcp/dxaml/lib/AppBarButtonHelpers.h`
- `dxaml/xcp/dxaml/lib/CommandBar_Partial.cpp`
- `controls/dev/CommonStyles/AppBarButton_themeresources.xaml`
- `controls/dev/CommonStyles/AppBarButton_themeresources_perf2026.xaml`
- `controls/dev/CommonStyles/AppBarToggleButton_themeresources.xaml`
- `controls/dev/CommonStyles/AppBarToggleButton_themeresources_perf2026.xaml`
- `controls/dev/CommonStyles/AppBarSeparator_themeresources.xaml`
- `dxaml/test/native/external/foundation/input/Focus/AllowFocusOnInteractionTests.cpp`
- `dxaml/test/native/external/controls/commandbar/CommandBarAutomationIntegrationTests.cpp`
- `dxaml/test/native/external/controls/appbarbutton/AppBarButtonAutomationIntegrationTests.cpp`
- `dxaml/test/native/external/controls/appbartogglebutton/AppBarToggleButtonAutomationIntegrationTests.cpp`

## ModernWpf Port Surface

- `ModernWpf.Controls\CommandBar\AppBarButton.cs`
- `ModernWpf.Controls\CommandBar\AppBarToggleButton.cs`
- `ModernWpf.Controls\CommandBar\AppBarSeparator.cs`
- `ModernWpf.Controls\CommandBar\AppBarElementContainer.cs`
- `ModernWpf.Controls\CommandBar\AppBarElementContainer.properties.g.cs`
- `ModernWpf.Controls\ModernWpf.Controls.xml`
- `ModernWpf.Controls\CommandBar\AppBarButtonAutomationPeer.cs`
- `ModernWpf.Controls\CommandBar\AppBarToggleButtonAutomationPeer.cs`
- `ModernWpf.Controls\CommandBar\AppBarElementProperties.cs`
- `ModernWpf.Controls\CommandBar\AppBarButton.xaml`
- `ModernWpf.Controls\CommandBar\AppBarToggleButton.xaml`
- `ModernWpf.Controls\CommandBar\AppBarSeparator.xaml`
- `ModernWpf.Controls\CommandBar\AppBarElementContainer.xaml`
- `ModernWpf\Styles\CommandBar.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `ModernWpf.Controls\CommandBar\CommandBar.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutCommandBar.cs`
- `ModernWpf.Gallery\Pages\MenusToolbarsSampleFactory.cs`
- `test\ModernWpf.WinUI.Tests\CommandBar\CommandBarApiTests.cs`
- `test\ModernWpf.WinUI.Tests\CommandBar\AppBarSourceAuditTests.cs`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`
- `test\ModernWpf.Gallery.Tests\WpfGallerySourceShapeTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`
- `tools\visual-checks\Record-GalleryControlInteractions.ps1`

## Current WinUI Gallery Authority

The current live sample authority is `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`:

- AppBarButton page/code: `3c36376c903d2ca6ba296204a5f4ac0a0420ece6` /
  `cbda5afaa30a27b80c28b8a2cb5dd0cf5fc6e520`; snippets:
  `3909d6f845e16371862eff9748448df73c19c277`,
  `e5f2df577cd1a084f3db01066821c8851fad5716`,
  `d6e504639def98274045ae99dcd576d9d66a2a93`,
  `7d8953077dcd776cd7fb1c17a473a2ab4fdacf80`,
  `55e72b1b97493ddbcbbc84d4d1c6e9c9f367fbfd`, and
  `e0178824d784247519c09d4eb4e203773eeff98b`;
- AppBarToggleButton page/code:
  `4bf2045e47bc89b5158ad5b69d284ad2a040b9dd` /
  `b4793cfc6259021c1aab5935c175bd4115adbb1b`; snippets:
  `5e3197259badaca9685edf3e7eb3afcd3bf644bd`,
  `4b0eaa053dfd8703e18c8bc778f07a8cde94f904`,
  `bf2a77f0a89005b5d934ae990c896fb83c627738`, and
  `1ff09ff7ce1941b65b45c8ba9cd539e291b31865`;
- AppBarSeparator page/code/snippet:
  `2e38ea78229ad501ca38a2493e9bdeca06b8ccbe`,
  `38e659a6cadf7966419f59c20bf1dcf0e39f8508`, and
  `2a2d73e92d111a2bcddf9f7ba21e79c616b1a223`.

The only post-conversion change is Gallery commit
`40a22976f78e63d5480afa8b49d5f3f7d5860dc6`, which corrects the displayed
AppBarButton flyout XAML from two self-closing `Flyout` tags and an unclosed
property element to a real `<Flyout><TextBox ... /></Flyout>` tree. The live
control was already correct. ModernWpf now displays the corrected current body
and a Gallery regression rejects the retired malformed tags.

## Ported Source Behavior

| WinUI 3 behavior | ModernWpf WPF port |
| --- | --- |
| `AppBarButton::OnClick` calls `CommandBar::OnCommandExecutionStatic` only when there is no associated flyout, then invokes the base click path and opens the associated flyout. | Matched. WPF tests cover command execution close, flyout preservation, click-open, automation expand, and automation collapse. |
| `AppBarToggleButton::OnClick` always routes through `CommandBar::OnCommandExecutionStatic` before the base toggle click. | Matched. WPF tests cover close behavior, command execution, and toggle state. |
| `AppBarButtonHelpers::OnPropertyChanged` updates internal styles and visual states for `IsCompact`, `UseOverflowStyle`, and `LabelPosition`. | Matched through WPF dependency-property callbacks, `SetDefaultLabelPosition`, `UpdateApplicationViewState`, width coercion, tooltip coercion, and visual-state refresh. |
| `SetOverflowStyleParams` records peer icon/toggle/keyboard-accelerator presence and refreshes visual states. | Matched for `CommandBarOverflowPanel` and `CommandBarFlyoutOverflowPanel`, including shared keyboard-accelerator text width. Normal `CommandBar` and `CommandBarFlyoutCommandBar` also propagate the source overflow-open input mode so secondary commands enter `TouchInputMode` when overflow is opened by touch. |
| `GetEffectiveLabelPosition` treats `LabelPosition=Collapsed` as collapsed and otherwise uses the propagated `CommandBarDefaultLabelPosition`. | Matched. Empty labels are treated as present where WinUI checks non-null `HSTRING`. |
| `UpdateInternalStyles` applies the label-on-right auto-width adjustment only for right labels, non-overflow style, and no local `Width`. | Matched with WPF `WidthProperty` coercion so local widths remain authoritative. |
| WinUI creates explicit AppBar automation peers with source class names, localized control-type strings, label-based name fallback, trimmed keyboard accelerator text, no template children, command-bar hosted keyboard focusability, AppBarButton expand/collapse, and AppBarToggleButton toggle routing through the owner. | Matched with WPF automation peers and tests. |
| WinUI `AllowFocusOnInteraction` tests keep AppBar buttons keyboard-focusable while suppressing pointer-origin focus. | Matched with WPF mouse-origin focus cancellation and tests. |
| `AppBarButtonHelpers::CloseSubMenusOnPointerEntered` closes peer overflow submenus on pointer entry, leaving the hovered AppBarButton submenu open and closing all peers for AppBarToggleButton. | Matched with WPF `MouseEnter` routing through `CommandBar.ClosePeerSubMenusOnPointerEntered`; peer `FlyoutBase` submenus are hidden for regular `CommandBar` and `CommandBarFlyoutCommandBar`. |
| CommonStyles AppBar templates use setter-backed application-view, common, input-mode, keyboard-accelerator, chevron, and checked states. | Matched through `VisualStateEx.Setters` in WPF templates with tests for setter presence, active state effects, and source-style secondary-command touch input-mode propagation. |
| The default button/toggle templates use 68px width, 64px minimum height, 16px icon content, `0,16,0,2` collapsed-icon margin, `2,0,2,8` label margin, and `2,6,2,6` inner chrome. | Matched value-for-value in `ModernWpf/Styles/CommandBar.xaml`, `AppBarButton.xaml`, and `AppBarToggleButton.xaml`. Exact installed-Gallery resting crops are `68x64` in both themes. |
| `AppBarSeparator` uses `2,8,2,8` primary padding, `0,4,0,4` overflow margin, a 1px stroke/overflow height, 0.5 radius, and FullSize/Compact/Overflow states. | Matched value-for-value. Light/Dark use `DividerStrokeColorDefaultBrush`; High Contrast retains `SystemControlForegroundBaseMediumLowBrush`. Product tests cover non-focusability and the compact/overflow state transition. |
| `AppBarElementContainer` is a `ContentControl` implementing `ICommandBarElement` and the private `ICommandBarOverflowElement`; `IsInOverflow` is computed by `CommandBar`. Its template is content-only. | Matched through the public source-shaped `IsCompact`, `IsInOverflow`, and dynamic-overflow properties, the internal attached `UseOverflowStyle` substitute, CommandBar overflow propagation, and the `ContentPresenterEx` content/transition template. |
| Current `ICommandBarElement` exposes `IsCompact`, read-only `IsInOverflow`, and `DynamicOverflowOrder`. `CommandBar::FindMovablePrimaryCommandsFromOrderSet` moves every command in each positive order group from lowest to highest, carries adjacent separators, then falls back right-to-left through order-zero commands. | The previously missing public properties and ordering behavior are now matched across all four AppBar element types. `DynamicOverflowOrder` is a shared dependency property whose callback immediately reapplies overflow; the WPF implementation selects whole order groups and source-shaped separator companions, while recomputing from the original collections on every size/property change. Product regressions cover defaults, all owners, grouped moves, separator moves, order-zero fallback, and live order changes. |
| WinUI Gallery's symbol AppBarButton updates `Control1Output`, and its symbol AppBarToggleButton exposes `TogglePattern` and changes from Off to On. | Matched by the generated Gallery examples and live harness. The button click/output crop and toggle checked-state crop are required cross-app interaction evidence rather than optional screenshots. |

## WPF Substitutions

- WinUI `CascadingMenuHelper`, `ISubMenuOwner`, popup submenu direction, delayed
  close timers, and popup-root tracking do not exist in WPF. ModernWpf uses
  associated `FlyoutBase` instances as the submenu representation and closes
  peer flyouts on WPF `MouseEnter`.
- WinUI can query `DXamlCore::GetIsKeyboardPresent`; ModernWpf approximates
  accelerator visibility from overflow state plus source-shaped peer
  `KeyboardAcceleratorTextOverride` presence.
- WinUI `TextTrimming="Clip"` is not a WPF enum value.
- WinUI touch input-mode state selection is wired for normal `CommandBar` and
  `CommandBarFlyoutCommandBar` overflow through a WPF touch/default input
  substitute. WinUI gamepad/remote input-mode selection is still not wired
  because WPF has no equivalent platform input-device service in this control
  path.
- WinUI disabled `AllowFocusWhenDisabled` has no direct WPF equivalent here, so
  disabled AppBar controls remain non-keyboard-focusable.
- Normal `CommandBar` no longer uses the old WPF `ToolBar` host. Its overflow
  path now uses the same explicit `UseOverflowStyle` model as the AppBar
  controls; see `docs\commandbar-winui3-source-audit.md` for the remaining
  CommandBar-specific WPF substitutions.
- WPF and WinUI use different text/symbol rasterizers. Exact geometry, chrome,
  theme surfaces, and separator pixels align; the bounded resting button delta
  is confined to the symbol and label rows and is therefore gated rather than
  "corrected" with a source-breaking layout offset.

## Validation

- `CommandBarApiTests` plus `AppBarSourceAuditTests`: 41/41 passed on net8.
  The submenu-pointer regression now opens the CommandBar overflow before
  clicking its secondary commands, matching the source interaction precondition
  and keeping the placement target connected to a presentation source.
- Gallery focused AppBar sample/gate tests: 2/2 passed on net8 and 2/2 on net10,
  covering all source examples, click/toggle results, sample anchors, and the
  strict harness contract.
- Fresh strict Light installed-Gallery evidence:
  `artifacts/visual-checks/20260719-012054-235-104188/report.md`.
  - AppBarButton: `4.43` static (`68x64` exact), `5.64` click/output
    (`165x59` versus `163x59`).
  - AppBarToggleButton: `4.45` static (`68x64` exact), `2.37` checked
    (`88x84` exact), with UIA state `Off -> On`.
  - AppBarSeparator: `0.61` (`334x48` exact).
- Fresh strict Dark installed-Gallery evidence:
  `artifacts/visual-checks/20260719-012207-886-65184/report.md`.
  - AppBarButton: `4.47` static, `6.61` click/output.
  - AppBarToggleButton: `4.47` static, `2.51` checked, with UIA state
    `Off -> On`.
  - AppBarSeparator: `0.52`.
- Required gates are button/toggle/separator static `5.0` / `5.0` / `1.0`
  with exact sizes, AppBarButton interaction `7.0` with at most two combined
  size pixels, and AppBarToggleButton interaction `3.0` with exact size.
- Pixel-region analysis found 36/40 completely identical rows in the button /
  toggle crops outside the symbol/label bands. The two separator strokes have
  only `0.007` Light and `0.021` Dark mean delta; no geometry correction is
  justified.
- Fresh Light `artifacts/gallery-recordings/20260719-012259-999/report.md` and
  Dark `artifacts/gallery-recordings/20260719-012334-172/report.md` recordings
  pass 3/3. AppBarButton changes blank output to `You clicked: Button1`;
  AppBarToggleButton changes UIA Toggle state from Off to On; AppBarSeparator
  supplies stable static evidence. Light maximum frame/local deltas are
  `0.030` / `0.519` and `0.370` / `44.607` for button/toggle; Dark values are
  `0.046` / `0.787` and `0.355` / `41.585`.
- `ModernWpf.Controls` and `ModernWpf.Gallery` build successfully for net462,
  net8, and net10.

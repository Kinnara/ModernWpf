# CommandBar WinUI 3 Source Audit

Date: 2026-07-19
Updated: 2026-07-30

This audit pins the ModernWpf `CommandBar` product, Gallery sample, automation,
and installed-Gallery pixel gates to current official sources. The authoritative
Microsoft UI XAML commit is
`de3e767333c2f0717a6a70cb22bd192ced5ad885`; the authoritative WinUI Gallery
commit is `29f62479d5c046a0b854a5868e5a7cd484572d87`.

The central synchronization epoch in `docs/winui3-sync-2026-07-29.md`
reconciles this detailed baseline through product `winui3/main`
`eb75504a1978df0d37a3ad4574d6f72bf4d21583`, stable
`winui3/release/2.3.1` at
`a97562621a1d1ea397a38a3f512c9eef99db52d8`, and Gallery
`f4dc3eb367f4bcecac1793829d9a221e924e5bfb`.

## Current source inputs

| Source | Blob |
| --- | --- |
| `controls/dev/CommonStyles/CommandBar_themeresources.xaml` | `f524c6d543ea735b7b4e833294891eec448b8b5f` |
| `controls/dev/CommonStyles/CommandBar_themeresources_perf2026.xaml` | `f524c6d543ea735b7b4e833294891eec448b8b5f` |
| `dxaml/xcp/dxaml/lib/CommandBar_Partial.cpp` | `ecf554e134db0793668a5993f87f8c80e487ef04` |
| `dxaml/xcp/dxaml/lib/CommandBar_Partial.h` | `e111bf850a8793ea3724d3bd37f2863562cd9680` |
| `dxaml/xcp/dxaml/lib/CommandBarOverflowPresenter_Partial.cpp` | `cbfcea3434da701a5e83b85573b98d47c7d275cf` |
| `dxaml/xcp/tools/XCPTypesAutoGen/Modules/Controls/CommandBar.cs` | `3089af2b982481552e3f713ddfccd1edab1b5bc2` |
| `dxaml/xcp/tools/XCPTypesAutoGen/Modules/Controls/AppBar.cs` | `12f3fdcfffa7e0cb7fb32698c674b2ab86bb5b8e` |
| `dxaml/xcp/dxaml/lib/AppBar_Partial.cpp` | `66009da3123afb1620a999fcf1b7177f1845d9b6` |
| `dxaml/xcp/dxaml/lib/AppBarAutomationPeer_Partial.cpp` | `efa158fccd2cc4094a390d1e15b6aa4e92cbb4e7` |
| `dxaml/test/native/external/controls/commandbar/CommandBarIntegrationTests.cpp` | `4cb5453e89ca371774585161b74445bcbaa1b71b` |
| WinUI Gallery `CommandBarPage.xaml` | `e2c92a5672467c5184198379f8b4b438bfeba8f3` |
| WinUI Gallery `CommandBarPage.xaml.cs` | `452cf2578ca1b15c106fd57632dfd11c07f80af0` |
| WinUI Gallery `CommandBarLabelsSide.txt` | `7a55d8dd9ac97cd10ba0d8bbf11fe5c1c70c2670` |

The Microsoft UI XAML root-layout mirror moved these paths in commit
`8463f45162149de0ec3ad7df752596893fe3e13e`. The current classic and perf2026
theme files are byte-identical, and the header, overflow presenter, generated
API, and integration-test blobs remain byte-current. Commit
`5da716a0536e14b9dc582cf63cac27ef161e1622` only corrects `recieve` to
`receive` in an access-key comment (and changes its indentation); it has no
runtime effect. The converted Gallery page, code-behind, and sole snippet have
no later CommandBar change.

## Product mapping

| WinUI behavior | ModernWpf mapping |
| --- | --- |
| `CommandBar` owns primary/secondary command collections and source-shaped template settings. | Observable command collections, direct WPF panels, and `CommandBarTemplateSettings`; no WPF `ToolBar` host or overflow containers. |
| `IsSticky` defaults to false and suppresses light dismiss and Escape dismissal while set. | Public generated `IsSticky` dependency property; a stable WPF popup plus explicit process-input light dismiss closes only non-sticky bars on outside pointer input. Escape is handled for both states, closes only when non-sticky, and restores focus to More. |
| Overflow is aligned to the command-bar right edge and flips from below when required. | The persistent WPF popup anchors to template `ContentRoot`, uses `BottomEdgeAlignedRight`, and refreshes its native placement after disconnected bitmap rendering. The outer popup HWND aligns to the command-bar edge. |
| More and programmatic/external open paths leave the overflow open. | `StaysOpen=true` avoids WPF's incompatible mouse-capture race; ModernWpf owns WinUI light-dismiss semantics instead. This supports current Gallery ordering `IsOpen=true` then `IsSticky=true`. |
| The current source template hard-codes the More button's `FontIcon` glyph to `E712` and exposes no CLR icon property. | The default geometry remains source-equivalent, while the WPF resource system exposes it as the public `CommandBarMoreButtonIconData` key. A `StreamGeometry` with that key in an individual CommandBar's resources replaces the glyph without retemplating the control or changing its CLR API. |
| Source overflow item exposes label, icon, and keyboard accelerator in a 32-DIP row. | `AppBarButton` overflow states expose Settings and `Ctrl+I`; UIA reports Button / Invoke and exact `167x32` current-Gallery geometry. |
| `ICommandBarElement.DynamicOverflowOrder` and `CommandBar::FindMovablePrimaryCommandsFromOrderSet` move complete positive order groups lowest-first with adjacent separators, then use right-to-left order-zero fallback. `DynamicOverflowItemsChanging` fires before the transition with Adding/Removing action. | Matched. All four AppBar element types own the shared public dependency property; order changes immediately reflow from the original collections. The pre-transition event compares the previous/current moved-primary sets exactly like source, so new members report Adding and pure restoration reports Removing. |
| In Auto mode, current `CommandBar_Partial.cpp` treats the content height as different from `AppBarThemeCompactHeight` only when the absolute delta reaches half a physical pixel: `0.5 / rasterizationScale` (`8dca4cd76468ac49cd2aa31cafa2e320835cb17b`). | `CommandBar` measures its content height, reads the same compact-height resource, and uses WPF's `VisualTreeHelper.GetDpi(this).DpiScaleY` as the rasterization-scale equivalent. Below-threshold subpixel drift leaves an otherwise empty More button collapsed; at or above the threshold it becomes visible. |
| Source shadow wrapper surrounds the overflow presenter. | `OverflowContentRoot` owns `SecondaryItemsControlShadowWrapper`; `ThemeShadowChrome` uses depth 32 and medium windowed-popup inset mode. |
| Command execution closes overflow and source input mode propagates to overflow commands. | Parent ownership callbacks close the bar; WPF default/touch input tracking updates secondary AppBar visual states. |
| The inherited AppBar lifecycle raises Opening, Opened, Closing, and Closed in order and exposes protected virtual hooks. | Matched on the WPF control. `Closed` is completed from the authoritative `IsOpen=false` transition because collapsing the WPF popup content can suppress `Popup.Closed`; external popup closure still synchronizes `IsOpen`. |
| The inherited `AppBarAutomationPeer` reports `ApplicationBar`, AppBar role, Toggle and ExpandCollapse always, Window only while open, modal/topmost Window properties, and open-state property changes. | `CommandBarAutomationPeer` matches class/localized type, Toggle/ExpandCollapse/Window providers and state transitions. WPF's `AutomationControlType` enum has no AppBar member, so the peer uses Custom while exposing localized type `app bar`; this is the sole role-ID substitution. |

## Current Gallery mapping

ModernWpf loads the current `CommandBarLabelsSide.txt` snippet instead of a
stale inline string. The live sample has Right labels; Add/Edit/Share with
`Ctrl+A`, `Ctrl+E`, and `F4`; Settings with `Ctrl+I`; and the source output
`You clicked: <Label>`. Open sets `IsOpen` and `IsSticky`; Close resets both.
Dynamic Button 1, Button 2, separator, Button 3, and Button 4 use `Ctrl+N`,
`Delete`, `Ctrl+Subtract`, and `Ctrl+Add` without the named-command click
handler, exactly matching current Gallery behavior.

The sample pins Button automation roles, `AppBarButton` class names, Invoke
providers, accelerator keys, and Text output semantics. The visual harness
uses official `PrimaryCommandBar`, opens through the source option's UIA Invoke
pattern, requires popup-HWND proof, selects only the sample Settings IDs, and
compares a common visible `CommandBarOpenSurface` rather than unlike shadow
bounds.

## WPF pixel substitutions

- WPF Segoe UI label measurement is one pixel narrower per Right-label button
  at Gallery scale. `AppBarButtonTextLabelOnRightMargin` and the toggle
  equivalent retain every source value except a documented 12-to-13 trailing
  pixel. The three primary commands therefore match WinUI exactly at `271x48`.
- WPF's overflow accelerator column needs the same single trailing pixel.
  AppBarButton/AppBarToggleButton use `24,0,13,0` instead of source
  `24,0,12,0`; Settings then matches WinUI exactly at `167x32` without changing
  the source `CommandBarOverflowMinWidth=160` resource.
- WinUI compositor shadow and WPF's separate popup HWND are proved separately;
  cross-app pixel comparison uses the same visible item bounds on both sides.
- WPF has no WinUI gamepad/remote input-device service or `Popup.ActualPlacement`;
  touch/default input and measured screen placement are the documented
  substitutions.
- Issue #262 requests More-button icon customization. Current WinUI does not
  expose an icon property, so ModernWpf keeps the current CLR shape and uses a
  WPF-specific resource adaptation instead. `CommandBarMoreButtonIconData` is
  a public `Geometry` resource consumed through `DynamicResource`; its default
  value is the unchanged source ellipsis outline.
- ModernWpf cannot inherit the platform-only WinUI `AppBar` base. It now ports
  the feasible inherited open/sticky lifecycle and automation surface directly;
  `ClosedDisplayMode`, `LightDismissOverlayMode`, and AppBar template settings
  remain outside this CommandBar's WPF-shaped public surface because its closed
  layout and light-dismiss policy are owned directly by the CommandBar template
  and popup.

An individual CommandBar can replace the More-button outline without copying
the control template:

```xaml
<controls:CommandBar>
    <controls:CommandBar.Resources>
        <StreamGeometry x:Key="CommandBarMoreButtonIconData">
            M 4,7 L 16,7 10,13 Z
        </StreamGeometry>
    </controls:CommandBar.Resources>
</controls:CommandBar>
```

## Validation

The final failure-on-difference installed-Gallery proofs are:

- Light `artifacts/visual-checks/20260719-015618-975-54260/report.md`: resting
  delta `1.70`, open delta `2.16`, exact `271x48` / `167x32` geometry.
- Dark `artifacts/visual-checks/20260719-015715-320-93036/report.md`: resting
  delta `2.08`, open delta `2.37`, exact `271x48` / `167x32` geometry.

Both pass the required `2.5` resting and `2.5` open mean-delta gates with zero
size tolerance. The current normal Light/Dark row captures supersede the prior
gray-background evidence; their remaining delta is confined to renderer-specific
Settings/accelerator glyph antialiasing, with identical background, icon,
spacing, and geometry.

Fresh Light `artifacts/gallery-recordings/20260719-015752-861/report.md` and
Dark `artifacts/gallery-recordings/20260719-015932-813/report.md` OpenRepeat
recordings pass. Both detect the expected Settings surface on two opens and
produce dense-transition review sheets; maximum frame/local deltas are
`0.524` / `54.178` and `0.098` / `10.513`.

The prior detailed-baseline product coverage passed 44/44. The synchronization
epoch adds
`CommandBarAutoOverflowButtonUsesPhysicalPixelCompactHeightThreshold`; the
final serialized epoch gate reruns the complete project after that focused
regression. The current Gallery sample/source/visual-gate slice previously
passed 3/3 on net8 and net10, the visual-check PowerShell parses, and
Controls/Gallery built on net462, net8, and net10 with zero errors.

`CommandBarMoreButtonIconDataCanBeOverriddenPerInstance` verifies both the
default resource identity and a live per-CommandBar geometry replacement. The
resource is listed in `PublicResourceKeys.Shipped.txt`; Preview 1 records its
audit and migration baseline. Any preview-era change to that public key must be
deliberate, justified by the source audit or a documented WPF adaptation,
rebaselined, and documented for consumers.

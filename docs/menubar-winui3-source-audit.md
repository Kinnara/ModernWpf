# MenuBar WinUI 3 Current-Source Audit

Date: 2026-07-18

MenuBar is locked against the current official WinUI product source and the
current WinUI Gallery page, not only the older local sample. ModernWpf's
source-shaped product port remains byte-current. This refresh corrects the
Gallery runtime/snippet distinction, strengthens live behavior and automation
coverage, and makes both the closed bar and complete opened File menu required
Light/Dark pixel evidence. The current cross-control typography round also
exposed and fixed a WPF-only regression: leaf invocation now dismisses the
MenuBar flyout, and its four-row presenter retains the current source geometry.

## Pinned authorities

Current official `microsoft/microsoft-ui-xaml` `main` is
`de3e767333c2f0717a6a70cb22bd192ced5ad885` (2026-07-17). The current
root-layout MenuBar blobs are:

| Current source | Blob |
| --- | --- |
| `controls/dev/MenuBar/MenuBar.cpp` | `bde55725158ab922a102268b10726ab8f8450957` |
| `controls/dev/MenuBar/MenuBar.h` | `7df8bca8499a173fb7f93f93d5088a690e9c947c` |
| `controls/dev/MenuBar/MenuBar.xaml` | `cb7f431c58b377eff7812f590b10c219ae309d80` |
| `controls/dev/MenuBar/MenuBarAutomationPeer.cpp` | `522043b2333cd875b8af302ede5c4592947d4e6d` |
| `controls/dev/MenuBar/MenuBarItem.cpp` | `888d52d95babee6e0adc2612a42dcdbd29e34c97` |
| `controls/dev/MenuBar/MenuBarItem.h` | `886a43bba6a64b4fb664a7e78906dbcd021d3661` |
| `controls/dev/MenuBar/MenuBarItem.xaml` | `610d3249721d6be66eb202791cfd5b1dc7a67041` |
| `controls/dev/MenuBar/MenuBarItemAutomationPeer.cpp` | `3b9c6432201161c6abc6822a6352609864627e03` |
| `controls/dev/MenuBar/MenuBarItemFlyout.cpp` | `20ba2780e7bf40a9efda78728faaa35339689f2d` |
| `controls/dev/MenuBar/MenuBar_themeresources.xaml` | `96cf1b1e788a327c5c00e07af0e3fc3cbf23e40b` |

The local product checkout is
`c70471c511a0168b61dcca13af9556465f26b673`; all ten audited blobs are
byte-identical there. The only later MenuBar source-history entries are the
generic perf-resource build enablement and root-layout mirror move
`8463f45162149de0ec3ad7df752596893fe3e13e`, not a substantive MenuBar
change. No product-template patch was required.

Current official `microsoft/WinUI-Gallery` `main` is
`29f62479d5c046a0b854a5868e5a7cd484572d87`. Its current MenuBar sources are:

| Current Gallery source | Blob |
| --- | --- |
| `WinUIGallery/Samples/MenuBar/MenuBarPage.xaml` | `e59e706022dabe27d01b05dbedb8bc86a286f343` |
| `WinUIGallery/Samples/MenuBar/MenuBarPage.xaml.cs` | `6804bce2118957356c28897ef47635f89da89ba2` |
| `WinUIGallery/Samples/MenuBar/SimpleMenubar.txt` | `7e45b3d45fe18434764ad9a8fcd1145a38a89f7a` |
| `WinUIGallery/Samples/MenuBar/MenubarKeyboardAccelerators.txt` | `646f9223545884e86a0943be551239b7d9b11c6f` |
| `WinUIGallery/Samples/MenuBar/MenubarSubmenusSeparatorsRadio.txt` | `d51de1b31d1ab75d75b8530546dd2673b13ebce9` |

Commit `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` converted the sample to the
current definition-based path. The current runtime and displayed snippets
intentionally differ: live `Example1`/`Example2` use `Open`, and live
`Example3` uses `Open` and `Other Formats`; the three source snippets retain
`Open...` and `Other Formats...`. ModernWpf now preserves that distinction
and uses the current snippet filenames, so no obsolete extra snippet appears.

## Product mapping

| WinUI behavior or structure | ModernWpf mapping |
| --- | --- |
| `MenuBar` is a control with an owned `MenuBarItem` vector. | `MenuBar : Control` owns `ObservableCollection<MenuBarItem>` and binds it to source-shaped `LayoutRoot`/`ContentRoot` template parts. |
| Automation set metadata is refreshed with the item vector. | `UpdateAutomationSizeAndPosition` supplies position/size metadata where the target framework supports it. |
| `MenuBarItem` owns flyout items, creates `MenuBarItemFlyout`, and coordinates the parent open state. | The WPF port mirrors its item collection into a dedicated flyout and tracks Opening/Closed state on both item and parent. |
| Pointer, keyboard, access-key, and adjacent-item paths open, close, and switch menus. | WPF mouse, key, focus, and access-key paths adapt the same state machine. |
| Invoking a leaf `MenuFlyoutItem` dismisses the owning `MenuBarItemFlyout`. | `MenuBarItem` listens for routed WPF `MenuItem.Click`, then closes leaf items after their handlers run while preserving submenus and explicit `StaysOpenOnClick`. |
| The source template exposes `ContentButton` and current common/selected/open states. | `MenuBar.xaml` retains the template part and represents source setters through `VisualStateEx.Setters`. |
| `MenuBarAutomationPeer` is MenuBar; `MenuBarItemAutomationPeer` is MenuItem with Invoke and ExpandCollapse. | Dedicated WPF peers expose the same roles, names, patterns, and collapsed/expanded state. |

The focused product suite covers collection mutation, XAML content-property
shape, template parts, item/flyout mirroring, empty-item behavior, source 40px
minimum height, open/close state, navigation/input paths, and automation.

## Current Gallery behavior and accessibility

The generated page keeps all three current examples and exact output names:
`SelectedOptionText`, `SelectedOptionText1`, and `SelectedOptionText2`. Clicking
a live item produces current source output `You clicked: <item>`. Tests now
also pin:

- MenuBar role/class semantics;
- File MenuItem role/name plus Invoke and ExpandCollapse providers;
- actual collapsed-to-expanded-to-collapsed provider behavior;
- opened New MenuItem role/name and Invoke provider;
- selected-output Text role/name after clicking current live `Open`;
- keyboard accelerator strings, separators, submenu content, radio grouping,
  checked defaults, current runtime labels, and historical snippet ellipses.

## Pixel proof

The harness requires ModernWpf `GallerySample_MenuBar_MenuBar` and official
`Example1`. It opens File by one real click and only falls back to Invoke if
the click did not expose the menu; the old unconditional click-plus-Invoke
path could reopen WinUI with an unrelated keyboard-focus outline.

`MenuBarOpenSurface` walks from New to the complete menu presenter and captures
its exact screen bounds. This avoids black transparent corners from popup-HWND
bitmap capture and compares all four items, fill, border, corners, padding, and
text. WPF exposes the presenter as `96x134`; WinUI UIA includes its one-pixel
outer edge on each horizontal side and exposes `98x134`. The strict gate allows
only that measured two-pixel aggregate size difference.

- Closed gate: required source `Example1`, delta `<=3.0`, exact `158x40` size.
- Open gate: common `MenuBarOpenSurface`, delta `<=9.0`, size delta `<=2`.
- Light: `artifacts/visual-checks/20260719-000953-705-5868/report.md`, closed
  `2.63`, open `8.61`, `158x40`, open `96x134` versus `98x134`.
- Dark: `artifacts/visual-checks/20260719-001049-872-102252/report.md`, closed
  `2.57`, open `6.74`, `158x40`, open `96x134` versus `98x134`.

The remaining pixels are the native WPF/WinUI text rasterization and the
platform popup-edge representation; menu geometry, row origins, fill, border,
corners, labels, and baselines visually align.

Fresh `OpenRepeat` recordings invoke the visible `Exit` leaf, require the open
element to disappear, and then reopen the same File menu:

- Light: `artifacts/gallery-recordings/20260719-003628-180/report.md` — passed,
  `10.2s`, maximum frame/local deltas `0.982` / `70.624`.
- Dark: `artifacts/gallery-recordings/20260719-003722-561/report.md` — passed,
  `9.8s`, maximum frame/local deltas `0.201` / `12.465`.

## WPF substitutions

- WinUI overlay input pass-through and island placement APIs have no literal
  WPF popup equivalent; the port stores the pass-through element and uses the
  ModernWpf MenuFlyout/ContextMenu host.
- WinUI presenter-subtree routing, XYFocus/gamepad services, access-key display
  mode, and `AutomationProperties.AccessibilityView=Raw` remain WPF platform
  substitutions.
- ModernWpf accepts WPF `MenuItem` and `Separator` objects where WinUI limits
  the vector to WinUI menu-flyout item types.
- The MenuBar adapter scopes WPF system-menu typography, the historical WPF
  content inset, and a one-DIP bottom presenter inset to `MenuBarItemFlyout`.
  This emulates WinUI's 14-DIP glyph raster and exact four-row `134`-pixel
  popup height without changing the separately locked MenuFlyout surface.
- WPF renders the menu in a separate HWND while current WinUI renders it in the
  Gallery window; screen-bound surface capture is therefore the shared proof.

## Validation

- `MenuBarApiTests`: 12/12 passed on `net8.0-windows7.0`, including exact
  four-row presenter metrics and leaf-invocation dismissal.
- Focused Gallery sample/source/gate/harness slice: 4/4 passed on both
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- Strict installed-Gallery Light and Dark checks passed with
  `-FailOnDifference` at the artifact paths above.
- Fresh Light and Dark `OpenRepeat` recordings passed at the paths above.
- `ModernWpf.Gallery.csproj` builds for net462, net8, and net10 with zero
  errors.

Reopen MenuBar only for a new current product/Gallery source change, runtime
and snippet convergence, item-state/input regression, automation regression,
popup/menu-flyout substitution change, or strict visual-regression evidence.

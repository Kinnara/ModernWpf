# MenuFlyout current WinUI source audit

Audit date: 2026-07-19

## Authorities

The product authority is `microsoft/microsoft-ui-xaml` `main` at commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`. The current classic
`controls/dev/CommonStyles/MenuFlyout_themeresources.xaml` blob is
`6f9f3fd322583d2d537dc30c439e62accf1efbd4`; the current
`MenuFlyout_themeresources_perf2026.xaml` blob is
`f5cfea5b8bd4b3bccb8a24c8f4fbc165c1cc69b9`. The audited implementation set
also includes `MenuFlyout`, `MenuFlyoutPresenter`, `MenuFlyoutItem`,
`MenuFlyoutSubItem`, toggle/radio/split items, and their automation peers.
Every current object was read directly from the pinned commit rather than
assuming that the local upstream worktree was current.

History from the prior product baseline `c70471c511a0168b61dcca13af9556465f26b673`
is bounded. Commit `49b4d5326b4deba8c036e63a7e676715a5de4f3a` adds the perf2026 dictionary by
replacing seven zero-duration narrow-padding object animations with equivalent
visual-state setters. Commit `569d6084ab4a5800d18971bc9eefa99d543c355c`
adds SplitMenuFlyout resources/template content to that new dictionary, making
it match the SplitMenuFlyout surface already present in the classic authority.
Commit `8463f45162149de0ec3ad7df752596893fe3e13e` only moves the mirrored tree
from `src/` to the repository root. The classic blob and its metrics, colors,
states, templates, and behavior remain byte-current.

The live sample authority is `microsoft/WinUI-Gallery` `main` at commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`:

- `WinUIGallery/Samples/MenuFlyout/MenuFlyoutPage.xaml` —
  `834f7da9d1a314fa30c7a92e8814f2bc4aa889c0`
- `MenuFlyoutPage.xaml.cs` —
  `569c61d81cb4b9e0c2aece909358ec78071e02df`
- `AppbarbuttonMenuflyout.txt` —
  `b0464e07fd4d23cee06009c33327c74ff365841e`
- `MenuflyoutCascadingMenus.txt` —
  `2f6b5ab20d6227f13845889be199576426d5e182`
- `MenuflyoutIcons.txt` —
  `853428bcea6005bfac6258ae0c55b5f6de4b917e`
- `MenuflyoutIconsKeyboardAccelerators.txt` —
  `3dbfbdd9588e5fe270fafab818342264f07d2991`
- `MenuflyoutRadiomenuflyoutitems.txt` —
  `099d74fd883a104cbd45e364f0d5fd1a361b8935`
- `MenuflyoutSplitmenuflyoutitems.txt` —
  `cb97dc76b6cbfa133146631f1329c54671c5ea2c`
- `MenuflyoutTogglemenuflyoutitemsMenuflyoutseparator.txt` —
  `7a711fca95a11ff1a0c0018c7c6c080bc96561c0`

The Gallery path migration commit `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`
does not alter the seven example bodies or their behavior.

## Source conclusions

Current WinUI uses a 14-DIP content font, the variable content font family, a
`0,2,0,2` presenter inset, `4,2` item margins, standard item padding
`11,8,11,9`, and narrow padding `11,4,11,5`. The first Gallery example opens
three commands from a compact Sort `AppBarButton`; item clicks write
`Sort by: <tag>`. The split example writes `Clicked: <text>`. The accelerator
example inherits `Segoe UI`, overrides Copy to `Consolas`, and explicitly
restores Delete to `Segoe UI`.

WPF `ContextMenu` popups do not inherit target typography. The port therefore
sets the presenter's current WinUI font resources explicitly. Its generic WPF
menu-item template uses a scoped `7,4,7,5` content metric and one bottom
ScrollViewer pixel to reproduce WinUI's 32-DIP item slots and exact 96x102
three-item surface without changing MenuBar or ordinary WPF context menus.
The capture removes only WinUI's three reference-only horizontal
theme-shadow pixels; it preserves the full vertical surface.

Current `RadioMenuFlyoutSubItemStyle` keeps the `E915` radio placeholder visible
at zero opacity until a child is selected, then raises it to full opacity.
WPF has no separate `MenuFlyoutSubItem` style selector, so
`AreCheckStatesEnabled` selects the shared submenu-header placeholder through
`MenuItem.IsCheckable`; its existing child-selection mirror drives
`MenuItem.IsChecked` and therefore the glyph opacity. The stock WPF Fluent
menu templates remain plain WPF templates without restoring the retired
ModernWpf visual-state machinery.

Light and Dark aliases retain the WPF acrylic fallback needed to render the
same opaque flyout pixels when WinUI's `DesktopAcrylicTransparentBrush`
backdrop API is unavailable. High Contrast item, submenu, accelerator,
highlight, and disabled aliases now follow current WinUI system-color tokens.

## Gallery, behavior, and accessibility coverage

ModernWpf loads all seven current snippet filenames, raises the per-control
loader ceiling to seven, and consumes every snippet. The live controls retain
current names, tags, default checked states, icon/accelerator values, submenu
structure, output text, output margins, and font overrides.

Regression coverage proves the Sort `AppBarButton` as a Button named `Sort`,
its Invoke and ExpandCollapse providers and real expanded/collapsed states,
the `By rating` MenuItem Invoke contract, the Repeat MenuItem Toggle contract,
and the output Text peer/name after selection.

## Pixel evidence

The strict installed-Gallery comparison uses the `Sort` source for the closed
primary crop and the actual popup UIA root for the open surface. Gates are:

- closed primary crop: mean delta at most `1.0`, exact size;
- open interaction crop: mean delta at most `8.0`, exact size;
- popup-window proof and reference interaction parity are mandatory.

Fresh branch-tip 1180x820 evidence:

- Light: `artifacts/visual-checks/20260719-004427-629-26672/report.md` —
  closed `0.57` at `68x64`; open `6.70` at exact `96x102`.
- Dark: `artifacts/visual-checks/20260719-004459-735-37204/report.md` —
  closed `0.55` at `68x64`; open `4.01` at exact `96x102`.

Both evidence runs were captured after locking the final `8.0`/zero-size gate
and included interactions plus `-FailOnDifference`.

## Interaction and verification evidence

Fresh Light `artifacts/gallery-recordings/20260719-004531-064/report.md` and
Dark `artifacts/gallery-recordings/20260719-004623-068/report.md` OpenRepeat
recordings pass. Each opens Sort, locates the current `By rating` leaf through
UI Automation, invokes it, proves that the open element disappears, and opens
Sort again. The manifests report `LeafMenuItem:Invoke`, real Expanded states,
and visual first-open/closed/second-open evidence; maximum frame/local deltas
are `0.795` / `75.484` for Light and `0.332` / `15.912` for Dark.

Focused product coverage includes MenuFlyout API/state behavior and the
RadioMenuFlyoutItem selection/lifecycle contract. Focused Gallery coverage pins
the seven-example sample, this current-source audit, and the exact visual gate
on both `net8.0-windows7.0` and `net10.0-windows7.0`. Gallery builds also cover
`net462`; all three target frameworks complete with zero errors.

# DropDownButton WinUI 3 Source Audit

Date: 2026-07-19

## WinUI 3 Source Baseline

The product source of truth is official `microsoft-ui-xaml` `winui3/main`
commit `de3e767333c2f0717a6a70cb22bd192ced5ad885`. The current Gallery authority
is official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`; live comparison uses installed
Microsoft WinUI 3 Controls Gallery `2.9.3.0` with Microsoft Windows App Runtime
`2.2.3.0.0`.

Audited product sources:

- `controls\dev\DropDownButton\DropDownButton.cpp`
- `controls\dev\DropDownButton\DropDownButton.h`
- `controls\dev\DropDownButton\DropDownButton.idl`
- `controls\dev\DropDownButton\DropDownButton.xaml`
- `controls\dev\DropDownButton\DropDownButton_perf2026.xaml`
- `controls\dev\DropDownButton\DropDownButton_themeresources.xaml`
- `controls\dev\DropDownButton\DropDownButtonAutomationPeer.cpp`
- `controls\dev\DropDownButton\DropDownButtonAutomationPeer.h`
- `controls\dev\DropDownButton\DropDownButtonAutomationPeer.idl`
- `controls\dev\DropDownButton\InteractionTests\DropDownButtonTests.cs`
- `controls\dev\DropDownButton\TestUI\DropDownButtonPage.xaml`
- `controls\dev\DropDownButton\TestUI\DropDownButtonPage.xaml.cs`
- `controls\dev\Generated\DropDownButton.properties.cpp`
- `controls\dev\Generated\DropDownButtonAutomationPeer.properties.cpp`

The corresponding current blobs are
`a8baaecbec5bc5e6d73d398bdc27c1beee0f426c` (runtime),
`3aff2d3c64cf0273df19060fdbcd043eb76c14ac` (header),
`b52bb4b25959c687a6320b33d3d8e84bd31dddb4` (IDL),
`aa07b40c6f40b28c347ab25fd73a8ba36524505f` (classic template),
`a8f3afd7123954780a40d8e6c1229b00ee512291` (perf2026 template),
`c1e68023c2f729b36e83b7fc6b91ed7e0d2e97fe` (theme resources),
`7e521cd12666090e241b90f79cfef712e011111c` /
`7666927b7fe2a5129e7c20b029652e40bc0b18a7` /
`45221648d0b03b8e87e61b419b9d5e65def26bd0` (automation peer),
`50691799f2a5b29eff8360dc47953c6bbdcf15ab` (interaction tests),
`6e3566fc09481ffc9b5e03f9d55e5c9eaaadf9cc` /
`9f12c06b7cc6ad3c27d78b9089228a1f1fefe4ee` (TestUI), and
`d55c73e92a68f54adad54107ca0337c5c6b00607` /
`7559c263176f0a2c0e807b68d0801cdf98f48b44` (generated control/peer).
Relative to the previous product pin
`c70471c511a0168b61dcca13af9556465f26b673`, the bounded history contains only
root move `8463f45162149de0ec3ad7df752596893fe3e13e`; it makes no substantive
DropDownButton runtime, template, resource, test, or accessibility change.

The classic/perf2026 template comparison is also behaviorally exact. Perf2026
moves the Background, BorderBrush, content Foreground, and chevron Foreground
assignments for PointerOver, Pressed, and Disabled from zero-duration object
animations into visual-state setters. Its `AnimatedIcon.State` setters and all
geometry/resources are unchanged. ModernWpf already represents every
assignment with `VisualStateEx.Setters`.

Current Gallery sources are:

- `WinUIGallery\ControlPages\DropDownButtonPage.xaml`
- `WinUIGallery\ControlPages\DropDownButtonPage.xaml.cs`
- `WinUIGallery\Samples\ControlPages\DropDownButton_Simple.xaml`
- `WinUIGallery\Samples\ControlPages\DropDownButton_Icon.xaml`
- the DropDownButton control image

Their current blobs are `376c922b56e8d4be1679c2683ba64cfa7b8da432`
(page), `0c55bd9e3fd82a397dd4180f20c2672e891f37eb` (code-behind),
`417bb4c1a8ed0e266fc1c143bdb30e973bcf7062` (simple definition),
`0f0479b8ea3456007f20bc930aab88341910eadd` (icon definition), and
`dc5ff6d936836168db93e3c85245df24760bac1b` (control image). There is no
DropDownButton page/sample change after Gallery conversion commit
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`.

## ModernWpf Port Surface

- `ModernWpf.Controls\DropDownButton\DropDownButton.cs`
- `ModernWpf.Controls\DropDownButton\DropDownButton.properties.g.cs`
- `ModernWpf.Controls\DropDownButton\DropDownButton.xaml`
- `ModernWpf.Controls\DropDownButton\DropDownButtonAutomationPeer.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `ModernWpf.Gallery\Pages\BasicInputSampleFactory.cs`
- `test\ModernWpf.WinUI.Tests\DropDownButton\DropDownButtonApiTests.cs`
- `test\ModernWpf.WinUI.Tests\DropDownButton\DropDownButtonInteractionTests.cs`
- `test\ModernWpf.WinUI.Tests\DropDownButton\DropDownButtonSourceAuditTests.cs`
- `test\ModernWpf.Gallery.Tests\DropDownButtonSourceAuditTests.cs`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`
- `tools\visual-checks\Record-GalleryControlInteractions.ps1`

## Ported Source Behavior

| WinUI 3 behavior | ModernWpf WPF port |
| --- | --- |
| Applying the template registers Opened/Closed handlers on the current Flyout. Replacing Flyout revokes the old handlers, binds the new instance, and keeps `IsFlyoutOpen` plus UIA state synchronized. | Matched. The port explicitly tracks the registered flyout, unsubscribes before rebinding, and the source-shaped accessibility regression expands/collapses both original and replacement flyouts while proving the old counts stop changing. |
| Click/Invoke opens the flyout at the button. ExpandCollapse Expand/Collapse call ShowAt/Hide and expose Collapsed/Expanded according to the flyout lifecycle. | Matched through WPF FlyoutBase and a ButtonAutomationPeer-derived `IExpandCollapseProvider`; class name is `DropDownButton`. |
| The template uses Button chrome/resources, a content presenter, and a 12x12 trailing chevron. PointerOver/Pressed/Disabled own background, border, content, chevron, and AnimatedIcon states. | Matched through GridEx, ContentPresenterEx, FontIconFallback, and VisualStateEx setters. The current perf2026 source shape is already represented. |
| The peer inherits Button semantics, adds ExpandCollapse, and raises the ExpandCollapseState property when the flyout opens/closes. | Matched, including `Button` control type, accessible content-derived name, Invoke inherited from Button, ExpandCollapse, and state-change notifications. |
| The Gallery has two examples: text `Email` and icon-only Email with accessible name. Both menus contain Send, Reply, Reply All and are bottom-left edge aligned; icon glyphs are E715, E725, E8CA, and E8C2. | Matched. The locally rendered samples, displayed definitions, names, placement, items, and glyphs are regression guarded. |

The official displayed sample snippets retain historical `Placement=Bottom`,
while the current live page uses `BottomEdgeAlignedLeft`; ModernWpf deliberately
mirrors that runtime/snippet distinction.

## WPF Substitutions

- WinUI Grid owns BackgroundSizing, CornerRadius, border, and background
  directly. WPF Grid does not, so the template uses `GridEx` for equivalent
  chrome and `ContentPresenterEx` for WinUI text/content properties.
- WinUI uses `AnimatedChevronDownSmallVisualSource`. This repository does not
  carry that composition source, so `FontIconFallback` preserves the exact
  12x12 layout, resource-driven glyph/foreground, and logical Normal /
  PointerOver / Pressed state contract.
- Native WinUI visual states can use setters directly. WPF uses
  `VisualStateEx.Setters` to retain dynamic resources and identical state
  ownership.
- WinUI marks the private chevron `AccessibilityView.Raw`. WPF lacks that
  property; the private icon creates no peer, while the DropDownButton owns the
  accessible name and Button plus ExpandCollapse patterns.
- Flyout presentation is backed by a WPF Popup HWND rather than XamlRoot and
  WinUI composition. Public placement, lifecycle, input, and automation
  behavior are preserved by the shared FlyoutBase adapter.
- At the current desktop scale WPF gives the trailing elevation scanline
  fractional display coverage. The same declared WinUI gradient renders exact
  Light `#CCCCCC` and Dark `#303030` endpoints in the deterministic 96-DPI
  renderer regression, so no live-capture-specific brush correction is used.

## Validation

Run after the current-source refresh:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~DropDownButton" --no-restore
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~DropDownButton" --no-restore
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net10.0-windows7.0 --filter "FullyQualifiedName~DropDownButton" --no-restore
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls DropDownButton -Reference InstalledWinUI3Gallery -Theme Light -IncludeInteractions -FailOnDifference
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls DropDownButton -Reference InstalledWinUI3Gallery -Theme Dark -IncludeInteractions -FailOnDifference
.\tools\visual-checks\Record-GalleryControlInteractions.ps1 -Controls DropDownButton -Theme Light
.\tools\visual-checks\Record-GalleryControlInteractions.ps1 -Controls DropDownButton -Theme Dark
git diff --check
```

Fresh fully live Light
`artifacts/visual-checks/20260719-034918-186-31168/report.md` and Dark
`artifacts/visual-checks/20260719-035013-820-67352/report.md` pass the strict
`4.0` gate at `3.68` / `2.69`, with exact `78x32` primary-control crops and zero
size tolerance. Fresh Light/Dark OpenRepeat recordings
`artifacts/gallery-recordings/20260719-035113-704/report.md` and
`artifacts/gallery-recordings/20260719-035158-155/report.md` pass, detect Send,
Reply, and Reply All on both opens, and produce dense-transition review sheets.
The maximum frame deltas are `0.762` / `0.177`; the maximum local deltas are
`71.273` / `13.696`, proving the popup visibly opened and closed in both themes.

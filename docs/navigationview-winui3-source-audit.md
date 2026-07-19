# NavigationView WinUI 3 Source Audit

Date: 2026-07-18

This audit refreshes ModernWpf's existing source-shaped `NavigationView` port
against official `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885` (2026-07-17) and WinUI Gallery
commit `29f62479d5c046a0b854a5868e5a7cd484572d87` (2026-07-13). Live comparison
uses the installed WinUI Gallery `2.9.3.0`. The local pre-move source baseline
is `c70471c511a0168b61dcca13af9556465f26b673`; root move
`8463f45162149de0ec3ad7df752596893fe3e13e` changed current paths from
`src/controls/...` to `controls/...`.

The bounded post-baseline history includes shared infinity-helper and
allocation/performance cleanups plus two substantive current fixes. Commit
`b6e31de9b2bdf825b894cc831581439ecfaf4579` prevents a negative pane
`MaxHeight` by comparing the menu's actual height and refusing a negative
footer subtraction. Commit `834625ee535b767ca8ab3e381468e52ebed6aeb5`
revalidates expansion, flyout mode, template root, and attached flyout before a
deferred child-flyout show. Both fixes are now ported and regression-tested.

## Current WinUI 3 Source Inputs

- `controls/dev/NavigationView/NavigationView.cpp`
- `controls/dev/NavigationView/NavigationView.h`
- `controls/dev/NavigationView/NavigationView.idl`
- `controls/dev/NavigationView/NavigationView.xaml`
- `controls/dev/NavigationView/NavigationView_themeresources.xaml`
- `controls/dev/NavigationView/NavigationBackButton.xaml`
- `controls/dev/NavigationView/NavigationViewAutomationPeer.cpp`
- `controls/dev/NavigationView/NavigationViewItem.cpp`
- `controls/dev/NavigationView/NavigationViewItem.h`
- `controls/dev/NavigationView/NavigationViewItemBase.cpp`
- `controls/dev/NavigationView/NavigationViewItemBase.h`
- `controls/dev/NavigationView/NavigationViewItemPresenter.cpp`
- `controls/dev/NavigationView/NavigationViewItemPresenter.h`
- `controls/dev/NavigationView/NavigationViewItemAutomationPeer.cpp`
- `controls/dev/NavigationView/NavigationViewItemHeader.cpp`
- `controls/dev/NavigationView/NavigationViewItemSeparator.cpp`
- `controls/dev/NavigationView/TopNavigationViewDataProvider.cpp`
- `controls/dev/NavigationView/NavigationViewItemsFactory.cpp`
- `controls/dev/NavigationView/NavigationViewTemplateSettings.cpp`
- `controls/dev/NavigationView/NavigationView_ApiTests/NavigationViewTests.cs`
- `controls/dev/NavigationView/NavigationView_InteractionTests/*.cs`

Current authoritative blob IDs for the changed and rendered primary inputs are:

| Source | Current blob |
| --- | --- |
| `controls/dev/NavigationView/NavigationView.cpp` | `32fb2f2807190034bf5b6d914b6e00eb98945859` |
| `controls/dev/NavigationView/NavigationViewItem.cpp` | `7f2bc04facd53a283debc100ffb1f0cf903c7971` |
| `controls/dev/NavigationView/NavigationView.xaml` | `a21c87b48e29d0dd25beacaa4ae62167947e14fe` |
| `controls/dev/NavigationView/NavigationView_themeresources.xaml` | `aa3ff11b2c3128dd198d7140417594cfa6fe5c74` |
| `controls/dev/NavigationView/NavigationView_ApiTests/NavigationViewTests.cs` | `c24e68feea12a1f9512aaae22c533e58669fa1b1` |
| `WinUIGallery/Samples/NavigationView/NavigationViewPage.xaml` | `8f2f281cc611df619019772162d9acdb068cbe62` |
| `WinUIGallery/Samples/NavigationView/NavigationViewPage.xaml.cs` | `c62388cd7539009b12ea8400820cf36f1b547e20` |
| `WinUIGallery/Samples/NavigationView/NavigationviewDefaultPanedisplaymode.txt` | `f7826e984b49947dd73971836ea24e8f560d12bd` |
| `WinUIGallery/Samples/NavigationView/NavigationviewPanedisplaymodeTop.txt` | `8e5a5bdfe22cd85b8a7036f7a6cc036377d36f70` |
| `WinUIGallery/Samples/NavigationView/NavigationviewSwitchesPaneOrientation.txt` | `5ed13e4011bb720ef1ac830f722be8ba62a6abcf` |
| `WinUIGallery/Samples/NavigationView/NavigationViewTyingSelectionFocusTabs.txt` | `b1799195171466d6c6018163253d45fc160b6a06` |
| `WinUIGallery/Samples/NavigationView/NavigationViewDataBinding.txt` | `e6c85f5ad452e212d637e40da3a968d8aba66c2d` |
| `WinUIGallery/Samples/NavigationView/NavigationviewFooterMenuItems.txt` | `7f560dbbbb8b35050784227e69840b69bf8edd2a` |
| `WinUIGallery/Samples/NavigationView/HierarchicalNavigationview.txt` | `7b09af9e756d0a8afa750d4d3cf99c14736aafa8` |
| `WinUIGallery/Samples/NavigationView/NavigationViewApiAction.txt` | `ca333fdd290ef82ed2be87f95f970992ac577f7a` |

## Current Gallery Baseline

No NavigationView sample change is present after the current Gallery's
`SampleDefinition` conversion. The page still owns exactly eight examples:
default left/auto, top, adaptive orientation, selection-follows-focus tabs,
data binding, footer items, hierarchy, and the API/options surface. ModernWpf
Gallery retains the source names `nvSample5`, `nvSample6`, `nvSample2`,
`nvSample7`, `nvSample4`, `nvSample9`, `nvSample8`, and `nvSample1`; all eight
definitions, menu/footer contents, pane-position options, hierarchy, selected
content updates, data template, AutoSuggestBox/header/footer toggles, and pane
API controls are covered by the focused Gallery regression.

## ModernWpf Artifacts

- `ModernWpf.Controls/NavigationView/NavigationView.cs`
- `ModernWpf.Controls/NavigationView/NavigationView.properties.cs`
- `ModernWpf.Controls/NavigationView/NavigationView.xaml`
- `ModernWpf.Controls/NavigationView/NavigationViewAutomationPeer.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewItem.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewItem.properties.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewItemBase.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewItemPresenter.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewItemPresenterTemplateSettings.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewItemAutomationPeer.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewItemHeader.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewItemSeparator.cs`
- `ModernWpf.Controls/NavigationView/TopNavigationViewDataProvider.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewItemsFactory.cs`
- `ModernWpf.Controls/NavigationView/NavigationViewTemplateSettings.cs`
- `ModernWpf/Styles/NavigationView.xaml`
- `ModernWpf/Styles/NavigationBackButton.xaml`
- `ModernWpf/ThemeResources/Light.xaml`
- `ModernWpf/ThemeResources/Dark.xaml`
- `ModernWpf/ThemeResources/HighContrast.xaml`
- `ModernWpf.Gallery/Pages/NavigationSampleFactory.cs`
- `test/ModernWpf.WinUI.Tests/NavigationView/NavigationViewApiTests.cs`
- `test/ModernWpf.WinUI.Tests/NavigationView/NavigationViewSourceAuditTests.cs`

## Implementation Mapping

| Current WinUI behavior or contract | ModernWpf mapping |
| --- | --- |
| `NavigationView` owns pane/display-mode state, menu/footer collections, selection, top-navigation data, template parts, and item hierarchy. | The existing WPF control family retains that source-shaped ownership, including the root split view, pane/header/footer hosts, repeaters, overflow host, selection model, item factory, and display-mode updates. |
| Current `UpdatePaneLayout` compares `menuItemsActualHeight` with half the available height and only subtracts an otherwise-unresolved footer repeater when the subtraction cannot be negative. | ModernWpf ports both conditions directly. `PaneLayoutNeverAssignsNegativeScrollViewerMaxHeight` supplies stale desired/actual menu geometry and an oversized footer to prove both menu and footer `ScrollViewer.MaxHeight` values remain non-negative. |
| Current `NavigationViewItem::ShowHideChildren` revalidates expansion, flyout mode, root-grid lifetime, and the attached flyout inside its queued render callback. | The WPF composition-render callback applies the same four gates before `ShowAttachedFlyout`. `DeferredChildFlyoutShowSkipsCollapsedItem` proves an expand-then-collapse race stays closed and that a still-expanded item continues to open normally. |
| The current Light/Dark theme dictionaries use `AcrylicInAppFillColorDefaultBrush` for the default pane, transparent expanded/top pane colors, `LayerFillColorDefaultBrush` for content, primary/secondary text aliases for item states, transparent icon background, secondary header foreground, and `CardStrokeColorDefaultBrush` for the content border. | Light/Dark dictionaries now carry the current aliases and the previously missing icon, header, and content-border keys. High Contrast uses the current system brush mappings. The template consumes all three missing keys. |
| WinUI Gallery renders the in-app acrylic pane over its default Light/Dark page as solid sampled colors `#F2F2F2` / `#1F1F1F`; its content is `#F9F9F9` / `#272727`. | WPF has no acrylic compositor. `SystemControlPageBackgroundChromeLowBrush` and `SystemControlBackgroundChromeMediumBrush` are deterministic theme-specific solid fallbacks for the rendered pane, while `LayerFillColorDefaultBrush` supplies the exact content surface. |
| Current item and top-item normal/hover/pressed/selected foregrounds use primary or secondary text aliases, and hover/pressed top backgrounds use subtle secondary/tertiary fills. | ModernWpf replaces the stale WinUI 2 aliases with the current primary/secondary and subtle-fill resource graph in Light, Dark, and High Contrast. |
| The WinUI Gallery default example is `nvSample5`, 745x460, with `Sample Page 1`, four symbol items, a source `BodyTextBlockStyle` paragraph, and selection-driven `Sample Page N` headers. | The generated Gallery sample preserves the exact host size, symbols/tags, source body style and foreground, and selection behavior. A one-physical-pixel WPF text-origin adapter aligns the source paragraph, header, and content offsets without changing tile geometry or hit targets. |
| Source automation exposes the root selection provider and selected container providers. | `NavigationViewAutomationPeer` remains public and source-shaped; product tests cover provider defaults, empty selection, selected providers, item automation, expand/collapse, and pane behavior. |
| Source applies a depth-16 compositor `ThemeShadow` to `ShadowCaster`. | `ThemeShadowChrome.Depth=16` is the documented software-rendered WPF substitute and retains the source state targets and width binding. |

## WPF Substitutions

- WinUI acrylic material is represented by the deterministic Gallery-surface
  solid fallbacks above; WPF cannot reproduce WinUI's backdrop compositor.
- The source transparent expanded/top pane resources are colors. WPF template
  properties require brushes, so ModernWpf uses the equivalent transparent
  brush resource.
- WPF text, symbol-font, and grayscale antialiasing differ from WinUI's raster
  pipeline. The final residual is confined to glyph edges; pane, content, tile,
  header, and crop geometry align exactly.
- WinUI `ItemsRepeater`, `SplitView`, `Flyout`, popup/XamlRoot services, focus
  movement, top overflow measurement, gamepad/access-key paths, composition
  animations, x:Bind phases, and recycle metadata use the existing documented
  WPF substitutes rather than speculative platform emulation.

## Current Validation

- Initial exact-size baselines were Light `5.93` and Dark `5.15`.
- Fresh post-fix exact `745x460` comparisons are Light
  `artifacts/visual-checks/20260718-214332-077-72640/report.md` at `1.03` and
  Dark `artifacts/visual-checks/20260718-214437-301-37052/report.md` at `0.93`.
- `Run-GalleryVisualChecks.ps1` now requires the `nvSample5` reference crop,
  exact size parity, and mean delta `<=1.2`.
- Fresh Light selection recording
  `artifacts/gallery-recordings/20260718-214530-675/report.md` passes with
  `0.07` maximum frame delta and `4.848` maximum local delta. Fresh Dark
  recording `artifacts/gallery-recordings/20260718-214607-124/report.md` passes
  with `0.072` / `5.139`. Both operate the real NavigationView selection path.
- The WPF initial-window-layout substitute defers SplitView's state replay when
  dispatcher processing is suspended; the existing launch-state regression
  now covers the path without a nested-dispatcher exception.
- NavigationView product/source-audit tests pass 54/54; SplitView product tests
  pass 14/14; the focused Gallery sample/source-shape slice passes 7/7 on net8
  and net10. Gallery builds successfully on net462, net8, and net10 with zero
  errors; current target builds retain existing unrelated warnings.

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~NavigationView
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --no-restore --filter FullyQualifiedName~NavigationView
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls NavigationView -Theme Light -Reference InstalledWinUI3Gallery -FailOnDifference
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls NavigationView -Theme Dark -Reference InstalledWinUI3Gallery -FailOnDifference
git diff --check
```

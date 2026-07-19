# SplitView WinUI 3 Source Audit

Current product snapshot: `D:\repos\microsoft-ui-xaml`, official
`microsoft/microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`.

Current Gallery snapshot: official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`.

Parity refresh: 2026-07-18.

## Current Product Source Pins

| Current source | Git blob |
| --- | --- |
| `dxaml\xcp\core\core\elements\SplitView.cpp` | `d58aff859c2e10d97e0b02a9ec35d65c4337d719` |
| `dxaml\xcp\core\inc\SplitView.h` | `89440a30a4a166bca970c267b1ee192fffa057bd` |
| `dxaml\xcp\core\inc\SplitViewPaneClosingEventArgs.h` | `0002091ec22976dd5ac257219b760eaf1d2b6e0d` |
| `dxaml\xcp\core\inc\SplitViewTemplateSettings.h` | `ee96232a16be1de731c3e6536758f89f03380b1d` |
| `dxaml\xcp\dxaml\lib\SplitView_Partial.cpp` | `d1dc0f4603b4046fed9cdb53b262e73237992fa8` |
| `dxaml\xcp\dxaml\lib\SplitView_Partial.h` | `c62ea613a2f8a53e2a751d28d26db959dc2a8bae` |
| `dxaml\xcp\dxaml\lib\SplitViewPaneAutomationPeer_Partial.cpp` | `09123d961298baa7903bef3f7f5d31b01354281e` |
| `dxaml\xcp\dxaml\lib\SplitViewLightDismissAutomationPeer_Partial.cpp` | `bba15283aa2ab3d4fa58124854f595bd636c1d0f` |
| `dxaml\xcp\tools\XCPTypesAutoGen\Modules\Controls\SplitView\SplitView.cs` | `d793197acda192b7bc01f3251f86cee3e7c30dd6` |
| `dxaml\xcp\tools\XCPTypesAutoGen\Modules\Controls\SplitView\SplitViewTemplateSettings.cs` | `8302c0311b6d453824f2b05a4e96c08fa337c770` |
| `dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Automation.Peers.cs` | `4550b440be61fc5cab2cd91825b068e0b57bd36a` |
| `dxaml\xcp\dxaml\dllsrv\winrt\Microsoft.UI.Xaml.Common.rc` | `c3ffa2f8b3665aa771a8ec0c2586debdbf527f7d` |
| `controls\dev\SplitView\SplitView_themeresources.xaml` | `84cc5a983fc6af7f8ea8466887692095471b6c23` |
| `controls\dev\SplitView\SplitView_themeresources_perf2026.xaml` | `d1622c4c92506535152ea7fee723f8ffeb3b941a` |
| `controls\dev\SplitView\SplitView.vcxitems` | `543d42573c6111c9f9ccf234b4815d6161add694` |
| `dxaml\test\native\external\controls\splitview\SplitViewIntegrationTests.cpp` | `44bf3e3c701e5ed27648db602c5f84b2ca5236fc` |
| `dxaml\test\native\external\controls\splitview\SplitViewAutomationIntegrationTests.cpp` | `5920731bab10d2398e0736bdddcf922585744f6d` |
| `controls\test\MUXControlsTestApp\verification\SplitView.xml` | `6fef38250fbc2b15e2406816a7f1d3e11507b723` |

The prior audit used product commit
`c70471c511a0168b61dcca13af9556465f26b673`. Rename-aware comparison to the
current snapshot shows every audited SplitView runtime, header, generated
surface, peer, XamlOM, classic/perf theme, test, verification, and packaging
file as a byte-identical 100% rename. Commit
`8463f45162149de0ec3ad7df752596893fe3e13e` only removes the mirror's old
`src\` prefix; no SplitView file changes after that move.

Commit `49b4d5326b4deba8c036e63a7e676715a5de4f3a` created the perf2026 dictionary.
It keeps the same resources, metrics, template parts, state names, transitions,
and animations while converting eligible discrete state assignments to
`VisualState.Setters`. ModernWpf retains the corresponding assignments through
`VisualStateEx.Setters` where WPF lacks native setter support; the perf variant
does not require a separate visual or behavioral product fork.

## Current Gallery Source Pins

Current commit `29f62479d5c046a0b854a5868e5a7cd484572d87` carries the SplitView page
converted by `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` (`Convert other samples`,
2026-05-22), with no later SplitView changes:

| Current Gallery source | Git blob |
| --- | --- |
| `WinUIGallery\Samples\SplitView\SplitViewPage.xaml` | `cf10f87348f1791055abf3bce11bdcc6c9b3fdfc` |
| `WinUIGallery\Samples\SplitView\SplitViewPage.xaml.cs` | `919101bde98ecf5d74a56e5196fe9e69cac0c2a7` |
| `WinUIGallery\Samples\SplitView\BasicSplitview.txt` | `f0dbec14b1e20557d0ae4429555aac3b8410c561` |

The current page retains one `A basic SplitView.` example: a `400x300` host,
256-DIP open pane, 48-DIP compact pane, four People/Globe/Message/Mail links,
pane/content headings, selected-page output, and options for IsPaneOpen,
left/right placement, all four display modes, four pane backgrounds, and both
pane lengths. The right-placement state swaps the icon/text layout. ModernWpf's
`LayoutSampleFactory` reproduces that current runtime and preserves the current
source snippet/substitutions with WPF-native bindings and controls.

## Ported Product Behavior

- The API surface retains Content, Pane, IsPaneOpen, OpenPaneLength,
  CompactPaneLength, PanePlacement, DisplayMode, PaneBackground,
  LightDismissOverlayMode, TemplateSettings, four pane events, and cancelable
  `SplitViewPaneClosingEventArgs`.
- The source `[DisplayMode][PanePlacement][IsPaneOpen]` table chooses the exact
  Closed/OpenOverlay/OpenInline/ClosedCompact/OpenCompactOverlay left/right
  states. Current right-inline transitions and setter targets remain present.
- `OpenPaneLength=NaN` measures pane content and feeds the six template-setting
  lengths, grid lengths, negative animation lengths, and clip geometry.
- Opening/closing events follow visual-state completion; property-driven close
  raises noncancelable PaneClosing, while light dismiss raises PaneClosing
  first and honors Cancel before changing IsPaneOpen.
- In-control overlay click, window-level outside click, Escape, size changes,
  and display-mode changes follow the source close/focus rules. WPF local tab
  navigation and saved-focus restoration represent the source focus trap.

## Accessibility Parity

Current WinUI assigns `SplitViewPaneAutomationPeer` to `PaneRoot` and
`SplitViewLightDismissAutomationPeer` to `LightDismissLayer` during template
application. The pane reports class `SplitViewPane`, Window control type, and a
modal/topmost non-resizable Window provider only while the pane is open in
Overlay or CompactOverlay. The dismiss layer reports class
`SplitViewLightDismiss`, Button control type, localized name `Close`, automation
ID `LightDismiss`, and Invoke only while light dismiss is enabled; Invoke uses
the same cancelable close path.

ModernWpf now ports those contracts through peer-owning WPF template elements.
`SplitViewPaneRoot` keeps the existing Border rendering but creates the
conditional modal Window peer. `SplitViewLightDismissLayer` renders the same
transparent/overlay fill and creates the conditional Button/Invoke peer.
Focused tests prove both enabled overlay patterns, provider properties, names,
IDs, Invoke close behavior, and their absence in Inline mode.

## Template and Resource Parity

- The template retains `PaneRoot`, `PaneClipRectangle`, `PaneTransform`,
  `HCPaneBorder`, `ContentRoot`, `ContentTransform`, `LightDismissLayer`, two
  column definitions, and the complete display/overlay state groups.
- Open/close durations, 320/48 default pane lengths, left/right one-pixel border
  resources, transparent/high-contrast border, pane background, overlay brush,
  and zero pane corner radius remain source-aligned.
- The peer-owning pane and dismiss primitives preserve the existing template
  geometry and render output; fresh strict comparisons remain exact-size and
  retain the prior bounded deltas.

## WPF Substitutions

- WinUI uses XamlRoot polygonal outer-dismiss layers, native pointer routing,
  back-button integration, gamepad XY focus, element sounds, and compositor
  plumbing. WPF uses the owning Window's preview mouse input, Escape, local
  keyboard navigation, normal focus APIs, and no-op platform-only effects.
- WPF's peer-owning pane root is a Border because WinUI Grid chrome properties
  do not exist on WPF Grid. The light-dismiss element is a FrameworkElement
  that draws its Fill so it can own the source automation peer while retaining
  Rectangle-equivalent layout and hit testing.
- `LightDismissOverlayMode.Auto` lacks WinUI's Xbox/platform policy; the WPF
  adapter preserves existing On/Off visual behavior.
- `VisualStateEx.Setters` carries source setter semantics on WPF.

## Regression Coverage

- `SplitViewApiTests` covers defaults/properties, length math including Auto,
  source resources/template parts, pane events, cancelable dismiss, Escape,
  display/placement states, right-inline transitions, and the current two
  automation peers/patterns.
- `GalleryAutomationHookTests` pins the current example header, source snippet,
  names, geometry, nav selection, placement layout, four display modes,
  backgrounds, and pane-length options.
- `WpfGallerySourceShapeTests` pins the sample-scoped pane/content reference
  crop, strict `4.0` mean gate, and zero primary size tolerance.
- `SplitViewSourceAuditTests` pins current product/Gallery commits and blobs,
  product/peer/template/Gallery implementation shape, and final reports.

## Live Installed-Gallery Evidence

| Theme | Report | Reference | Crop sizes | Mean delta | Gate |
| --- | --- | --- | --- | ---: | --- |
| Light | `artifacts/visual-checks/20260718-130514-205-39096/report.md` | Cached installed-Gallery Light capture from `20260717-081604-045-11108`; fresh current ModernWpf capture | `400x300` / `400x300` | `3.23` | `4.0`, size `0` |
| Dark | `artifacts/visual-checks/20260718-130409-201-56932/report.md` | Fresh live installed Gallery and ModernWpf | `400x300` / `400x300` | `3.37` | `4.0`, size `0` |

The current installed Gallery was persisted in Dark and its sample theme button
did not yield a verifiable Light pane crop during this refresh. The Light run
therefore reuses the already-proven installed-Gallery Light artifact from the
same Gallery installation/source shape while freshly rebuilding and capturing
the peer-enabled ModernWpf control. Pane width, divider, backgrounds, headers,
item layout, and content placement align. The remaining bounded delta is WPF
text and symbol-font rasterization.

## Verification

- The refreshed SplitView product/source slice passes 16/16 on
  `net8.0-windows7.0`.
- Focused Gallery runtime/source-shape tests pass 3/3 on both
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- The net462 Controls build succeeds with zero errors and 18 existing warnings,
  none in the SplitView port; Gallery tests rebuild the net8/net10 product
  outputs.
- Both final Light and Dark comparisons pass the `4.0` mean gate with exact
  `400x300` crops and zero size tolerance.

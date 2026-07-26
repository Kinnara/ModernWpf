# Repeater WinUI 3 Source Audit

Date: 2026-07-18

ModernWpf `ItemsRepeater`, its layout family, item/source/recycle helpers, and
selection model are tracked as a source-backed WPF port of official
`microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885` (2026-07-17). The current Gallery
contract is pinned to WinUI Gallery commit
`29f62479d5c046a0b854a5868e5a7cd484572d87` (2026-07-13). Live comparison uses
the installed WinUI 3 Controls Gallery `2.9.3.0` with Windows App Runtime
`2.2.3.0.0`.

## Product Source Baseline

The product repository moved its mirrored tree from `src\...` to the repository
root in `8463f45162149de0ec3ad7df752596893fe3e13e`. The last audited pre-move
product baseline is `c70471c511a0168b61dcca13af9556465f26b673`; all current
paths below therefore intentionally omit `src\`.

Primary current WinUI 3 inputs and blob IDs:

| Source | Current blob |
| --- | --- |
| `controls\dev\Repeater\ItemsRepeater.cpp` | `792c35888d49fbfe5b27d97ef4206f71103dcc9e` |
| `controls\dev\Repeater\ItemsRepeater.h` | `dbafec36e1fbc2bf60ab8f317fc3525de24f8414` |
| `controls\dev\Repeater\ViewportManager.cpp` | `c57ed88e9fc7b5a9132b2bfe5d4f9254c8fa6ad7` |
| `controls\dev\Repeater\ViewportManager.h` | `74cb4488dd4ce944792c38adfed88cba51015aa9` |
| `controls\dev\Repeater\RepeaterAutomationPeer.cpp` | `166f14597dc2936c60046dc0af2a0068a70a76ac` |
| `controls\dev\Repeater\StackLayout.cpp` | `6a28140bc291a13e84580a944e5e73e6c65511d0` |
| `controls\dev\Repeater\FlowLayout.cpp` | `915e1a81e6ae380fcdb56ee197226c5cf3fead9f` |
| `controls\dev\Repeater\UniformGridLayout.cpp` | `f52dc50df137855a8670b2c2bbe3cd21f7671e65` |
| `controls\dev\Repeater\ItemsSourceView.cpp` | `b75f6c95faed922d472b5d16e7ea81dd2ea5b952` |
| `controls\dev\Repeater\RecyclePool.cpp` | `f2f65674f53808ae85b4ac4a9ec8e10fb04d505f` |
| `controls\dev\Repeater\SelectionModel.cpp` | `711448f2708ae07d25cbab118560962bcd8deaca` |
| `controls\dev\Repeater\APITests\AccessibilityTests.cs` | `998206abb7b8d3b8b8c71a63e3e0f1419ccf9ed1` |
| `controls\dev\Repeater\APITests\RepeaterTests.cs` | `213e1e974d7bd7f4a132a4ff753a3ab410b6f586` |

The substantive post-baseline history is bounded and audited:

- `262cf0f1f5dcbaf366ac2cb426713e4a961fc7be` adds only C++ template
  disambiguation required by VS 2026; it has no managed-port behavior.
- `ac8c220bb148d4dc5d40b22ed7e1d1e393dbeb07` collapses WinUI's former
  abstract/platform/downlevel viewport hierarchy into one value-owned
  `ViewportManager`. ModernWpf intentionally retains the downlevel WPF
  adaptation because WPF has neither effective-viewport services nor
  `ScrollPresenter`.
- `9018b87dc3f914f70aac40d324f2d49511e7e3a7` guards null `LayoutState` in
  `FlowLayout::OnItemsChangedCore` and `UniformGridLayout::OnItemsChangedCore`
  while still invalidating layout. ModernWpf carries the same guard.
- `132e2cdd30531603e613bb26b8139722e886a379` changes Repeater comments and
  test wording only.

## Current Gallery Baseline

WinUI Gallery converted this page to `SampleDefinition` files in
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`; no later ItemsRepeater commit is
present through the current pin. Current paths and blobs are:

| Gallery source | Current blob |
| --- | --- |
| `WinUIGallery\Samples\ItemsRepeater\ItemsRepeaterPage.xaml` | `7d2d456fd321c14b38e7650fd8fb856c37a7a58f` |
| `WinUIGallery\Samples\ItemsRepeater\ItemsRepeaterPage.xaml.cs` | `eca86cc6c25e988c86d03d7ea03cbc01c436872f` |
| `WinUIGallery\Samples\ItemsRepeater\ItemsRepeaterBasicNonInteractiveItems.txt` | `7e3db62efac2dede6ad24f09ef167a721c9ab30c` |
| `WinUIGallery\Samples\ItemsRepeater\ItemsRepeaterVirtualizingScrollableListItems.txt` | `510cb4b1982308d57ce0d555985a194f69565d8e` |
| `WinUIGallery\Samples\ItemsRepeater\ItemsrepeaterMixedTypeCollection.txt` | `b6b72d5f01e6458d58fcd3bd9ede4a44ee75f75d` |
| `WinUIGallery\Samples\ItemsRepeater\LayingOutNestedItemsrepeaters.txt` | `0b85a7fa228ea40777533ccfbe1404bb5808e2ea` |
| `WinUIGallery\Samples\ItemsRepeater\ItemsRepeaterAnimatedScrollingContentDisplay.txt` | `e454262a237d2c279f94fb36ac48ed437b12256a` |
| `WinUIGallery\Samples\ItemsRepeater\ItemsRepeaterVirtualizedContentHeavyLayout.txt` | `9b7b51307fb845bf4c6a5b4b79ec1e2088f615b8` |

The page still contains exactly six examples: basic bars with add/remove and
three layout choices; a virtualized numbered list; a mixed-type collection;
nested repeaters; the animated color list; and the content-heavy recipe sample
with filtering and sorting. `ModernWpf.Gallery\Pages\CollectionsSampleFactory.cs`
keeps those headers, source-facing names, data shapes, options, snippets, and
runtime interactions. Its custom activity-feed and varied-image layouts remain
documented WPF-feasible `UniformGridLayout` adaptations.

## Ported Runtime and Accessibility Shape

- `ItemsRepeater` owns the source-shaped animation manager, view manager,
  viewport manager, layout context, element mapping, recycle/pin flow,
  item-template wrapping, and layout-replacement hooks.
- The constructor uses WPF `KeyboardNavigationMode.Once`, corresponding to
  WinUI `TabFocusNavigation=Once`; WPF's directional navigation remains enabled
  as the `XYFocusKeyboardNavigation=Enabled` substitute.
- `ItemsRepeater.MeasureOverride` carries WinUI's 60-pass `StackLayout` cycle
  guard and returns the last layout extent once the layout fails to settle. The
  counter resets at source-equivalent layout-updated, unloaded, and layout-change
  points.
- `FlowLayout` and `UniformGridLayout` ignore collection forwarding when their
  context has no correctly typed layout state, and always invalidate layout,
  matching `9018b87d...`.
- `StackLayout`, `FlowLayout`, and `UniformGridLayout` retain source layout
  properties and WPF-feasible virtualization, spacing, wrapping, uniform-slot,
  and orientation algorithms.
- `IndexPath`, `ItemsSourceView`, `RecyclePool`, `ElementFactory`,
  `SelectionModel`, and `ItemsRepeaterScrollHost` retain the existing
  source-shaped API and behavior coverage.
- `RepeaterAutomationPeer` reports `AutomationControlType.Group`, removes peers
  whose immediate repeater child is no longer realized, sorts retained peers by
  item index, and preserves the base peer order of multiple accessible
  descendants beneath one peerless item container. The new WPF regression is a
  direct managed port of current upstream `AccessibilityTests.ValidateChildrenPeers`.
- `ModernWpf.Controls\Repeater\GlobalSuppressions.cs` keeps explicit
  source-audit justifications for source-shaped field names, method signatures,
  animation hooks, viewport hooks, and WPF substitute signatures.
- The Gallery's horizontal, vertical, and circular bar templates map WinUI
  `SystemChromeLowColor` to `SystemControlPageBackgroundChromeLowBrush`. The
  older medium-chrome substitution rendered Light bars too dark.

## WPF Substitutions

- WinUI uses effective viewport, `ScrollPresenter`, scroll anchoring,
  composition, phasing, focus/gamepad navigation, WinRT collection metadata,
  and raw TestUI. ModernWpf maps feasible behavior through WPF `ScrollViewer`,
  `IRepeaterScrollingSurface`, WPF layout invalidation, and direct tests.
- The WPF `ViewportManagerDownLevel` hierarchy is intentionally retained after
  WinUI's `ac8c220b...` consolidation. It is a platform adapter, not stale
  product ownership.
- WinUI's invalid-rect sentinel is `{-1,-1,-1,-1}`. WPF `Rect` rejects negative
  width and height, so ModernWpf uses `Rect.Empty`.
- WinUI lazily initializes the default layout state from `OnLayoutUpdated`.
  ModernWpf installs and initializes its default `StackLayout` in the
  constructor; the WPF layout-updated hook only resets the measure-cycle counter.
- WPF clears a template-created element's resource-backed dependency properties
  to their metadata defaults while dismantling the `FrameworkTemplate` tree.
  `ItemsRepeater` therefore accepts a cleared `ItemTemplate` value after
  recycling realized elements; non-null values must still be a `DataTemplate`,
  `DataTemplateSelector`, or `IElementFactory`. The focused regression replaces
  a `ListView` item whose old template contains a resource-backed repeater,
  covering issue #257's exact teardown path.
- The WPF `ScrollViewer.ChangeView` substitute clamps requested offsets, returns
  `true` only for an applied offset change, returns `false` for no-op and valid
  zoom-only requests, and rejects NaN/infinite offset or zoom values.
- WPF has no `AutomationProperties.AccessibilityView=Raw`. The peer therefore
  carries the current Group role and child-filter/order behavior, while UIA tree
  membership follows WPF's provider rules.
- The composition scaling in the animated Gallery example is represented by
  live realized-item opacity and scrolling behavior; the WPF port does not
  synthesize WinUI compositor expressions.

## Regression Guards

- `RepeaterSourceAuditTests` pins the current product/Gallery commits, root-move
  boundary, substantive history, source blobs, current paths, strict artifact
  evidence, explicit suppression wording, null-state guards, and WPF viewport
  adaptation.
- `RepeaterAutomationPeerTests.AutomationPeerReportsOnlyRealizedChildrenInItemIndexOrderLikeWinUI`
  ports the upstream accessibility scenario: items are realized out of order,
  one is recycled, and a peerless item container contributes two accessible
  descendants. Only the three correct peers remain, in data-index order.
- `RepeaterLayoutTests.FlowLayoutsIgnoreCollectionChangesWhenContextStateIsUnavailableLikeCurrentWinUI`
  covers the current null-state collection-change contract.
- `GalleryAutomationHookTests.ItemsRepeaterSampleMatchesWinUIGalleryExamples`
  covers all six examples, snippets, source names, add/remove behavior, layout
  switching, filtering/sorting, and Low chrome.
- `WpfGallerySourceShapeTests.GalleryVisualChecksCropVisibleItemsRepeaterSourceBarRows`
  pins the live `425x88` source-row crop, required WinUI source, strict `1.0`
  mean-delta gate, and zero size tolerance.

## Current Validation

- Fresh Light comparison
  `artifacts/visual-checks/20260718-203128-999-95628/report.md` passes at `0.53`;
  fresh Dark comparison
  `artifacts/visual-checks/20260718-203156-243-103388/report.md` passes at `0.42`.
  Both compare exact `425x88` live source bar rows under the `1.0` gate.
- Fresh Light scroll recording
  `artifacts/gallery-recordings/20260718-203226-944/report.md` passes with
  `9.168` maximum frame delta and `22.657` maximum local delta. Fresh Dark
  recording `artifacts/gallery-recordings/20260718-203249-206/report.md` passes
  with `6.231` / `11.4`. Both record local scroll evidence and finish after
  `1.5s` of the six-second maximum window.
- The complete Repeater product/source/accessibility slice passes 69/69 on
  `net8.0-windows7.0`.
- The ItemsRepeater Gallery sample/crop slice passes 2/2 on
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- `ModernWpf.Gallery` builds for net462, net8, and net10 with zero warnings and
  zero errors on every target.

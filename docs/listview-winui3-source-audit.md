# ListView / GridView WinUI 3 Source Audit

Date: 2026-07-18

This audit treats official `microsoft-ui-xaml` commit `de3e767333c2f0717a6a70cb22bd192ced5ad885` and official WinUI Gallery commit `29f62479d5c046a0b854a5868e5a7cd484572d87` as the current sources of truth for ModernWpf's custom `ListViewBase`, `ListView`, and `GridView` family. It supersedes the 2026-07-17 audit against `3cae15f071f1ab8565f9a7592dbf27f04bafe651`.

The current refresh found one real accessibility mismatch: ModernWpf's shared item peer always advertised Invoke and inherited WPF class names. Current WinUI advertises Invoke only when `IsItemClickEnabled` is true (or for the unsupported SemanticZoom zoomed-out case), and publishes `ListView`, `GridView`, `ListViewItem`, and `GridViewItem` class names. ModernWpf now matches that feasible contract. No product-template or Gallery layout change was justified: exact installed-Gallery geometry and the existing strict visual/interaction gates remain green in both themes.

## Official Revisions And History

- Product source: `de3e767333c2f0717a6a70cb22bd192ced5ad885`.
- Product pre-root-move comparison point: `c70471c511a0168b61dcca13af9556465f26b673`.
- Source-root move: `8463f45162149de0ec3ad7df752596893fe3e13e`; current paths below intentionally omit `src\`.
- Current product history after `c70471c5` has only two ListView-family semantic candidates:
  - `350c26f0410309eb7367b363cab82cba7735a7ea` reserves `selectedItemIndices.size()` capacity in the native dragged-items `TrackerCollection`. It changes allocation cost only; WPF owns its collection allocation and drag/drop substrate.
  - `49b4d53265cc2283ae5d5d6a10ab2f515417452b`, republished by `51d82696d`, adds `ListViewItem_themeresources_perf2026.xaml`. It preserves the current resources, metrics, parts, and state outcomes while expressing eligible discrete assignments as setters. ModernWpf already uses `VisualStateEx.Setters` for the source-feasible item states.
- Gallery source: `29f62479d5c046a0b854a5868e5a7cd484572d87`.
- Gallery conversion point: `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`. The current page uses three `SampleDefinition` files; its rendered examples and interactions are semantically unchanged after that conversion.

## Current Product Inputs

| Official source | Current blob | Local responsibility |
| --- | --- | --- |
| `dxaml\xcp\dxaml\lib\ListViewBase_Partial.cpp` | `d96d7d4a6bdcca2d50910387c600820754a8245b` | Shared list lifecycle, container and focus substrate. |
| `dxaml\xcp\dxaml\lib\ListViewBase_Partial_Interaction.cpp` | `7758954dccfead1b0f2ce7873c000c50390ca17f` | Primary/secondary gestures, Enter/Space, `ItemClick`, clicked-data resolution. |
| `dxaml\xcp\dxaml\lib\ListViewBase_Partial_Selection.cpp` | `b2e5f0725b175a1b5f6485a6ae053480bc0e86f5` | Selection-mode transitions and multiple-selection behavior. |
| `dxaml\xcp\dxaml\lib\ListViewBase_Partial_Reorder.cpp` | `0e5bb139918d083151c9a2322906f48b31c6e7af` | Drag/reorder reference; current reserve-only change is allocation-equivalent. |
| `dxaml\xcp\dxaml\lib\ListViewBaseItem_Partial.cpp` | `cd6a9ccaba8743906324cbf237bf82086bdb49b4` | Item state routing and parent lookup. |
| `dxaml\xcp\dxaml\lib\ListView_Partial.cpp` | `a51b3eb2694dedd5c641f226f280229ab692907b` | List container and peer creation. |
| `dxaml\xcp\dxaml\lib\GridView_Partial.cpp` | `12103518bc86f46800b488b57148d623ae244b11` | Grid container and peer creation. |
| `dxaml\xcp\dxaml\lib\ListViewBaseAutomationPeer_Partial.cpp` | `ea3e145e914010979f8018530a9543441aae3f12` | List/drop-target peer and child surface. |
| `dxaml\xcp\dxaml\lib\ListViewBaseItemAutomationPeer_Partial.cpp` | `3e588a5e862f0459139afe1e740af4e9041d2e1c` | ListItem role plus conditional Invoke/drag patterns. |
| `dxaml\xcp\dxaml\lib\ListViewAutomationPeer_Partial.cpp` | `b3ed9abcaa0b0c44e94bb382d14d7c1221575552` | `ListView` class name and List role. |
| `dxaml\xcp\dxaml\lib\GridViewAutomationPeer_Partial.cpp` | `dde1ea9edbd1735a10e185147917ac975392951f` | `GridView` class name and List role. |
| `dxaml\xcp\dxaml\lib\ListViewItemAutomationPeer_Partial.cpp` | `a1ef4d2aa33ac6dca0d235797ebf03d7d59e16aa` | `ListViewItem` class name and item focus behavior. |
| `dxaml\xcp\dxaml\lib\GridViewItemAutomationPeer_Partial.cpp` | `648916ba6bdf10db135c9dd263bdecc176c68137` | `GridViewItem` class name. |
| `dxaml\xcp\dxaml\lib\ItemInvokeAdapter_Partial.cpp` | `615975cad92481e83d8a9043a61a7934527d783b` | UIA Invoke forwards to the primary item gesture. |
| `controls\dev\CommonStyles\ListViewItem_themeresources.xaml` | `b292e8fa9aa1f6024d16b8a5eb452e4427d45bda` | Classic ListViewItem resources/template. |
| `controls\dev\CommonStyles\GridViewItem_themeresources.xaml` | `a4ca9accccc27d9f0e7fdcfbddc2a5b8e306360d` | Classic GridViewItem resources/template. |
| `controls\dev\CommonStyles\ListViewItem_themeresources_perf2026.xaml` | `c02c81ecb25fb3d1fdeadb903b1043ba81d61fed` | Equivalent setter-oriented current template family. |
| `dxaml\test\native\external\controls\listviewbaseitem\ListViewBaseItemIntegrationTests.cpp` | `1ee2b3c8610d411b8b05d5aa2c0a3008e52df7b2` | Native item-state/behavior reference. |
| `dxaml\test\native\external\controls\listviewbaseitem\ListViewBaseItemAutomationIntegrationTests.cpp` | `96669ea85a58c83a557b69f4e2354dbfed77d2fa` | Native item automation reference. |
| `dxaml\test\native\external\enterprise\ListView\ListViewIntegrationTests.cpp` | `c6c9db3045a084f3d3af7004a02a8d59ebbb4ed1` | List behavior reference. |
| `dxaml\test\native\external\enterprise\GridView\GridViewIntegrationTests.cpp` | `1e05e80c2e1f64745f544ee2c8e7d026932610ee` | Grid behavior reference. |

## Current Gallery Inputs

| Official source | Current blob | ModernWpf mapping |
| --- | --- | --- |
| `WinUIGallery\Samples\GridView\GridViewPage.xaml` | `248a2341c9c876a398715723ac6b7924d1271e3d` | Three live examples, templates, options, output blocks, names, and accessibility metadata. |
| `WinUIGallery\Samples\GridView\GridViewPage.xaml.cs` | `6708537e07293c2472ea07cc6e9a373a01afe2a4` | Items, template switching, click/selection output, flow, selection mode, and margin/wrap changes. |
| `WinUIGallery\Samples\GridView\BasicGridviewSimpleDatatemplate.txt` | `a749cb7167629b7a89e297ec317fbda32ddb80ce` | First header plus XAML/C# displayed source. |
| `WinUIGallery\Samples\GridView\GridviewLayoutCustomization.txt` | `6114d9dfab83f1359254083b9dd277ae55707eea` | Second header and layout-customization XAML. |
| `WinUIGallery\Samples\GridView\ContentInsideGridview.txt` | `c600ea6854faf485303abe378535372435bf6c9a` | Third header and content/options XAML. |
| `WinUIGallery\Samples\SampleCode\GridView\GridViewSample1_xaml.txt` | `a51c7d8276cc71b8cfca3d19444471d92e9e2452` | Legacy first XAML snippet, still text-identical to the converted definition. |
| `WinUIGallery\Samples\SampleCode\GridView\GridViewSample1_cs.txt` | `99a648b63de345811b709bf474d9873716dd54b5` | Legacy first C# snippet, still text-identical to the converted definition. |
| `WinUIGallery\Styles\GridViewItem.xaml` | `eaa1b365176cea720e288630292d02a461513891` | Gallery-specific item presentation reference. |

ModernWpf's generated page resolves the same current content through `CollectionsSampleFactory`: `Basic GridView with Simple DataTemplate`, `GridView with Layout Customization`, and `Content inside of a GridView.`. The first example retains the exact displayed sample files; the latter two constants retain the current definition text and substitutions. The WPF runtime keeps its `ItemsWrapGrid`, WPF drag/drop, and WPF template construction adapters while preserving the current visible options and outputs.

## Behavior And Accessibility Mapping

| Current WinUI contract | ModernWpf result |
| --- | --- |
| `ItemClick` is raised from the primary gesture and own-container items report their content rather than the container object. | `NotifyListItemClicked` uses WPF generator data, then falls back to the own container's `Content`. |
| Enter and Space are primary gestures; Alt+Space is ignored for the system-menu chord. | `ListViewBaseItem` follows the same key contract. |
| Item visuals expose normal, pointer, pressed, selected combinations, selected-disabled/disabled, and list/grid multiselect outcomes. | Source-named `CommonStates` and `NoMultiSelect` / `ListMultiSelect` / `GridMultiSelect` are driven through `VisualStateEx.Setters`; theme resources include selected-disabled colors. |
| `ListView` and `GridView` peers expose List roles and their source class names. | The shared WPF peer now returns `ListView` or `GridView` by owner type and explicitly returns `AutomationControlType.List`. |
| Item peers expose ListItem roles and `ListViewItem` / `GridViewItem` class names. | The shared item peer resolves the realized/own container type and explicitly exposes the matching class name and `AutomationControlType.ListItem`. |
| Invoke is present only when item click is enabled, apart from SemanticZoom's zoomed-out switch-view case. | The WPF item peer now returns Invoke only while `IsItemClickEnabled` is true. SemanticZoom is not in this retained control slice. |
| Invoke forwards to the primary item-click path and respects owner/item enabled state through UIA. | `IInvokeProvider.Invoke` checks owner/item enabled state and calls the same `NotifyListItemClicked` path. |
| The Gallery image template names each image from the item title and places the WinUI image in Raw view. | The WPF image binds `AutomationProperties.Name` to `Title`; WPF has no `AccessibilityView=Raw` attached-property equivalent, while the item peer remains the ListItem control-view owner. |

## WPF Substitutions And Intentional Limits

- WinUI uses `ListViewItemPresenter` and platform collection panels. Under the no-new-presenter rule, ModernWpf maps the source-visible properties into explicit `Border`, `ContentPresenterEx`, selection-border, and checkbox parts.
- WinUI can repeat visual-state names across groups. WPF namescopes reject duplicate names, so ModernWpf folds the source-feasible selected visuals into `CommonStates`.
- WPF `ListBoxItem`, `SelectionMode`, `ItemContainerGenerator`, `ScrollViewerEx`, `VirtualizingStackPanel`, `WrapPanel`, and `ItemsWrapGrid` remain the platform substrate for selection, virtualization, scrolling, and layout.
- WinUI SemanticZoom, gamepad focus engagement, connected animations, native item presenters, data virtualization, and native drag/reorder providers are not available as direct WPF services. The source audit does not claim those unsupported platform paths.
- The current native dragged-item `Reserve` call is an allocation optimization and has no observable WPF port requirement.
- The current perf2026 dictionary's discrete setter conversion maps to the already-retained `VisualStateEx.Setters`; unsupported drag/reorder/presenter-only state machinery is not fabricated.

## Installed-Gallery Pixel And Interaction Lock

The strict harness requires the real `BasicGridView`, an exact static crop, an invoked `Item 1`, and the exact output `You clicked Item 1.`. Static crops use a `2.0` mean-delta gate and zero size tolerance. The output comparison uses an `8.0` gate, a separate four-pixel metric cap, and only a bounded one-pixel alignment for the platforms' text baseline/height difference.

| Theme | Fresh report | Static sizes / delta | Click-output sizes / delta |
| --- | --- | --- | --- |
| Light | `artifacts/visual-checks/20260718-200820-485-79776/report.md` | `657x412` / `657x412`, `1.61` | `122x18` / `120x19`, `6.40` |
| Dark | `artifacts/visual-checks/20260718-200857-428-3920/report.md` | `657x412` / `657x412`, `1.60` | `122x18` / `120x19`, `6.64` |

Fresh rendered interaction recordings also prove Invoke, selection change, and blank-to-expected output:

- Light: `artifacts/gallery-recordings/20260718-200949-892/report.md`, passed with `0.795` maximum local output delta and `AfterOutput = "You clicked Item 1."`.
- Dark: `artifacts/gallery-recordings/20260718-201017-558/report.md`, passed with `1.085` maximum local output delta and the same expected output.

## Regression Coverage

- `ListViewApiTests` covers both item templates and source state setters, list/grid multiselect state routing, own-container click data, Space, automated Invoke, current peer class/control types, conditional Invoke exposure, brush-typed focus resources, and selected-disabled resources.
- `GalleryAutomationHookTests.GridViewSampleMatchesWinUIGalleryExamples` covers all three current headers/snippets, eight-item data, image dimensions/scaling/name, basic click output, List/GridView peer roles and class names, conditional Invoke, layout margins/wrapping, content-template options, selection output, flow direction, drop, and selection modes.
- `WpfGallerySourceShapeTests.GalleryVisualChecksEnforceGridViewPixelParityThreshold` pins the required static/interaction parity gates, exact-size gate, output crop mapping, and bounded alignment helper.
- `ListViewSourceAuditTests` pins the current revisions, authoritative product/Gallery blobs, local peer fix, Gallery content, strict artifacts, and test/harness shape.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~ListViewApiTests|FullyQualifiedName~ListViewSourceAuditTests" --no-restore` passes 11/11.
- The focused Gallery runtime/source-shape slice passes on both `net8.0-windows7.0` and `net10.0-windows7.0`.
- `ModernWpf.Controls` and `ModernWpf.Gallery` build for `net462`, `net8.0-windows7.0`, and `net10.0-windows7.0` with zero errors. The focused net8 build is warning-free; net462/net10 report the 18 existing unrelated NavigationView, PersonPicture, and ItemsRepeater warnings.
- The two fresh installed-Gallery visual runs and two rendered recordings above pass their strict required gates.

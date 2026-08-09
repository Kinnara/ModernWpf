# TabView current WinUI 3 and Gallery source audit

Date: 2026-08-08

ModernWPF Preview 5 adds a separate WinUI-shaped `TabView` control family.
It does not rename, subclass, or replace WPF's stock `TabControl` and
`TabItem`. Those stock controls remain governed by official WPF Fluent and
`docs/tabcontrol-wpf-fluent-source-audit.md`.

## Pinned upstream snapshots

| Source | Commit | Purpose |
| --- | --- | --- |
| microsoft-ui-xaml `winui3/main` | `e1aa8f64df98d6229f6cd4074d59b654616254da` | Preview 5 product, API, template, resource, automation, interaction-test, and tear-out cutoff. |
| microsoft-ui-xaml `winui3/release/2.3.1` | `a97562621a1d1ea397a38a3f512c9eef99db52d8` | Current stable source reconciled with the main cutoff. |
| WinUI Gallery `main` | `3669519356c67f1376152c33ed8ea45003a91f3a` | Preview 5 Gallery page, sample, helper, and windowing cutoff. |

The upstream repository moved the control tree from `src/controls` to
`controls` between the stable and main snapshots. After accounting for that
move, the public IDL, templates, resources, class declarations, automation
peers, and tear-out behavior are unchanged. Main contains two applicable
implementation refinements: iteration no longer copies each tab item while
updating widths, and selected-tab geometry uses locale-invariant formatting
so decimal-comma cultures cannot produce invalid XAML geometry.

## Current WinUI product inventory

The following Git blob IDs pin the exact product and test inputs:

| File under microsoft-ui-xaml | Blob |
| --- | --- |
| `controls/dev/Generated/TabView.properties.cpp` | `e3646ffbf697c21b6385b307b8410207f38a2957` |
| `controls/dev/Generated/TabView.properties.h` | `54cddd4db544cb3f76fa23336e12ba1eda0dccaa` |
| `controls/dev/Generated/TabViewAutomationPeer.properties.cpp` | `de59fb1d11c75b8a43c6bc730f4ac7aea08a08b4` |
| `controls/dev/Generated/TabViewItem.properties.cpp` | `5918308511c378d87951a6c0f9e3eaba1a4e8b10` |
| `controls/dev/Generated/TabViewItem.properties.h` | `8e746fb9608765adf2f8192f6bd01758da8a8add` |
| `controls/dev/Generated/TabViewItemAutomationPeer.properties.cpp` | `8db30c37223008d42feaac29d42b3f8568a8ece0` |
| `controls/dev/Generated/TabViewItemTemplateSettings.properties.cpp` | `140e40d96a23f9e3293e486fdf298bc722f20f7f` |
| `controls/dev/Generated/TabViewItemTemplateSettings.properties.h` | `8ca0b724e7f80e0ddfe72c69f0c4e180469bfa92` |
| `controls/dev/Generated/TabViewListView.properties.cpp` | `7707d82174be7f9dae89dc8c4253e26d1efc3778` |
| `controls/dev/TabView/TabView.cpp` | `23dd10cb0617bd8bc2214f01981f95f06154959e` |
| `controls/dev/TabView/TabView.h` | `0ca96f28a0040e3ff7122714227e317de4bdd80a` |
| `controls/dev/TabView/TabView.idl` | `cc528d5471727bb568211e989996ac29232058ea` |
| `controls/dev/TabView/TabView.xaml` | `c0e732a4604036f8cce7fee1aed757a9fc4bacdb` |
| `controls/dev/TabView/TabView_perf2026.xaml` | `e6916bc47d0ba173761f31c367d67b92f4067d31` |
| `controls/dev/TabView/TabView_themeresources.xaml` | `e4db61f8793560b59fa8390ade0a59cc9addeb13` |
| `controls/dev/TabView/TabViewAutomationPeer.cpp` | `4e2ab9d81ffd7fa9c839c63fc76e1d0e62b28f40` |
| `controls/dev/TabView/TabViewAutomationPeer.h` | `10c7c154cd69356c6b6214d6cbc6afe3077bc795` |
| `controls/dev/TabView/TabViewItem.cpp` | `5c2479d2984739a3ce5e38e00ee2b5690ffe30ae` |
| `controls/dev/TabView/TabViewItem.h` | `13c6bf902295c1a3b48f34a32d4a82fe1056ecf3` |
| `controls/dev/TabView/TabViewItemAutomationPeer.cpp` | `e4cb1ea6ff2333769abf43a626c07642766cf4c1` |
| `controls/dev/TabView/TabViewItemAutomationPeer.h` | `4af3720cacbe51c536736fde484c69a2d43d9003` |
| `controls/dev/TabView/TabViewItemTemplateSettings.h` | `b90d5e03f34c7064fea38a9c008b7b61775def42` |
| `controls/dev/TabView/TabViewListView.cpp` | `4ff0c810110204b5b47e39412d242cabafafba7f` |
| `controls/dev/TabView/TabViewListView.h` | `9b89cfec5247dd68a650f2f2cbc56fe098965bdd` |
| `controls/dev/TabView/APITests/TabViewTests.cs` | `439e227d2a3d94c5b604be15c30289fd0084297a` |
| `controls/dev/TabView/InteractionTests/TabViewTests.cs` | `f3e0d28d38b3e38ae68c6793750b203b15b4b541` |
| `controls/dev/TabView/InteractionTests/TabViewTearOutTests.cs` | `a4ac32300f87ecd670daf65ffc19bd150d1bb2a5` |
| `controls/dev/TabView/Strings/en-us/Resources.resw` | `fef9f97679a084aa1da5f8ac26dadd85eda04839` |
| `controls/test/TabViewTearOutApp/README.md` | `c78c76a2d234a871b0977c8b951e4c3eb3191161` |
| `controls/test/TabViewTearOutApp/MainPage.xaml` | `5a9f801cfaae605435cb1c3239365b022ecc9f52` |
| `controls/test/TabViewTearOutApp/MainPage.xaml.cpp` | `86bdc15266410101355c7c19bb3ea1930b2cccec` |

### API surface

The source control exposes:

- `TabViewWidthMode` values `Equal`, `SizeToContent`, and `Compact`;
- `TabViewCloseButtonOverlayMode` values `Auto`, `OnPointerOver`, and
  `Always`;
- `TabView` tab-strip header/footer content and templates, add-button
  visibility/command/parameter, `TabItems`, `TabItemsSource`, item template
  and selector, selected item/index, drag/reorder/drop switches, and
  `CanTearOutTabs`;
- add, close, collection-change, selection, drag, dropped-outside, strip
  drag/drop, and four tear-out/rejoin events;
- item/container lookup by item and index;
- `TabViewItem` header/template, icon source, content, closability,
  template settings, and item-level close event; and
- close, drag, drop, and tear-out event-argument types that carry both the
  source item and its realized tab.

The WinUI `TabViewListView` and generated property helpers are template
implementation mechanics. ModernWPF keeps its WPF list host internal rather
than adding another public collection control. `TabViewItemTemplateSettings`
remains public because it is a source-shaped, read-only property on the public
item and carries its generated icon and selected-tab geometry.

### Behavior

The first enabled, visible tab is selected when the application has not made
an explicit selection. Selection updates the displayed content and moves to
the nearest enabled, visible tab after removal. A close request does not
remove data itself: both `TabView.TabCloseRequested` and
`TabViewItem.CloseRequested` are raised so the application remains the owner
of its collection.

`Equal` mode distributes available strip space within the public minimum and
maximum widths; overflow shows backward and forward repeat buttons.
`SizeToContent` measures each header. `Compact` collapses unselected headers
to their icons while keeping the selected header at standard width. Selected
items are brought into view after selection and layout changes. Pointer-hover
close-button policy, middle-click close, tooltip updates, disabled/hidden
selection skipping, and right-click without selection are part of the source
behavior.

Keyboard behavior includes:

- Ctrl+Tab and Ctrl+Shift+Tab select the next or previous enabled tab and
  wrap;
- Ctrl+F4 raises close for the selected closable tab;
- Left and Right move through each tab, its close button, and the add button,
  respecting right-to-left flow;
- Space or Enter selects a focused tab or invokes a focused button; and
- Up and Down do not accidentally move focus between one-pixel-overlapping
  tab headers.

Drag behavior exposes starting/completed events, supports cancellable drag
data, raises `TabItemsChanged` when a mutable collection is reordered, and
reports a drop outside the strip. `CanDragTabs`, `CanReorderTabs`, and
`AllowDropTabs` remain independent switches.

### Accessibility

The `TabView` automation peer reports class `TabView`, control type `Tab`, a
single-selection provider, and required selection. The item peer reports class
`TabViewItem`, control type `TabItem`, a selection-item provider, the parent
selection container, and the header string as its fallback accessible name.
The add, close, and overflow buttons use the upstream neutral strings and
ordinary WPF Invoke providers. Selection and reorder update the corresponding
automation state without manufacturing a second semantic tab tree.

WinUI derives the item peer from its public `ListViewItemAutomationPeer`.
WPF's nearest `ListBoxItemAutomationPeer` requires a `SelectorAutomationPeer`
for a public selector owner; using it here would force an extra selectable list
layer into the automation tree. The WPF item peer therefore derives from
`FrameworkElementAutomationPeer` and directly implements the source
selection-item and scroll-item provider contracts against the owning TabView.

## Current WinUI Gallery inventory

| File under WinUI-Gallery | Blob |
| --- | --- |
| `WinUIGallery/Helpers/TabViewHelper.cs` | `ee688585d3954a21f5cacdf693f6ac16d94352c0` |
| `WinUIGallery/SampleSupport/SamplePages/TabViewWindowingSamplePage.xaml` | `94d1d005345efde2f4ad782af215aa0b783412e5` |
| `WinUIGallery/SampleSupport/SamplePages/TabViewWindowingSamplePage.xaml.cs` | `697d5f88c748ec90a0f69d9b0df43fac4eab55cf` |
| `WinUIGallery/Samples/TabView/CompleteTabviewWindowingSample.txt` | `63f5b631d527659ab3132e8c7e622fb947051662` |
| `WinUIGallery/Samples/TabView/TabViewCloseButtonBePersistent.txt` | `1b53728155666dc418cde8b68b62a7b701b62fcc` |
| `WinUIGallery/Samples/TabView/TabViewPage.xaml` | `ae4e756dababfb5d41adb387fffbef76eb4c185f` |
| `WinUIGallery/Samples/TabView/TabViewPage.xaml.cs` | `1a42b0a6e0b8de2d2330f516802e5cd39a9e6901` |
| `WinUIGallery/Samples/TabView/TabViewTabWidthsEitherBe.txt` | `a59f6e0600d3ecad59d4cf3761211278d6c7e86d` |
| `WinUIGallery/Samples/TabView/TabViewYouPutCustomContent.txt` | `4c0a827aa044686b8492068d3035961ec34d8468` |
| `WinUIGallery/Samples/TabView/TabviewAccentColoredTabstrip.txt` | `b28a576b3a64fb6815c01c75f50fa897f6577c79` |
| `WinUIGallery/Samples/TabView/TabviewBoundCollectionMydata.txt` | `8b5eec01330be490b55625aebfb96f2673f34e5f` |
| `WinUIGallery/Samples/TabView/TabviewColorTabIcons.txt` | `25530d35b3ee419117ca5a4186b28c3e597e91d5` |
| `WinUIGallery/Samples/TabView/TabviewKeyboardingSupport.txt` | `91e4a7e14dd89a78806bb55494f431d333e20f4a` |
| `WinUIGallery/Samples/TabView/TabviewSupportAddingClosing.txt` | `6f2505aac5909d1cbd254eb12f29558c2486006e` |
| `WinUIGallery/Samples/TabView/TabviewTabviewitemsDefinedMarkup.txt` | `b886e482dc08e0d2af3dea488e7b557d0b310135` |

The current Gallery contains ten examples: programmatic add/close, XAML tab
items, an observable data source, application keyboard accelerators, strip
header/footer content, all three width modes, all close-button overlay modes,
full-color icons, an accent-colored strip, and a separate windowing/tear-out
sample. The context menu moves tabs left or right in either an explicit
collection or an `IList` data source.

Preview 5 replaces the retired generated `TabControl`-based TabView facsimile
with the real ModernWPF control. The separate stock WPF `TabControl` page
remains authoritative for `System.Windows.Controls.TabControl`. The TabView
page keeps the ten-example order and source-facing automation names stable.
Examples that previously described drag, reorder, or windowing as an
approximation now use the WPF implementation and document its platform
adaptation.

## WPF mapping and platform boundary

### Collections, drag data, and templates

WPF has no `IObservableVector`, `IVectorChangedEventArgs`, WinRT
`DataPackage`, or `DataPackageOperation`. The WPF projection therefore uses:

| WinUI concept | ModernWPF projection |
| --- | --- |
| `IVector<Object> TabItems` | read-only `ObservableCollection<object>` |
| `Object TabItemsSource` | `IEnumerable` dependency property |
| `IVectorChangedEventArgs` | `NotifyCollectionChangedEventArgs` |
| `DataPackage` | `IDataObject` |
| `DataPackageOperation` | `DragDropEffects` |
| WinUI list drag/drop | WPF `DragDrop.DoDragDrop` and routed drag events |

Explicit `TabViewItem` values are their own containers. For a data source,
`TabItemTemplate` must produce a `TabViewItem`; its bindings receive the data
item as `DataContext`. This preserves the current Gallery's source-shaped
template and keeps header, icon, closability, and content on the public item
rather than introducing a WPF-only view-model interface.

The WPF internal strip host is non-public. Public `ContainerFromItem` and
`ContainerFromIndex` still return the realized `TabViewItem` for either
collection mode.

### Tear-out and rejoin

WinUI's current V7 tear-out path depends on `WindowId`, `AppWindow`,
`InputNonClientPointerSource`, content-island coordinate conversion, and the
native move-size loop. Those primitives do not exist on WPF or on all three
ModernWPF target frameworks.

ModernWPF keeps the event sequence and application ownership while replacing
`WindowId` with `System.Windows.Window`:

1. A pointer drag begins through WPF drag/drop and raises
   `TabDragStarting`; cancellation stops the operation.
2. Reordering within a mutable source moves the source item and raises the
   normal collection and drag-completed events.
3. A release outside every accepting TabView raises `TabDroppedOutside`.
4. When `CanTearOutTabs` is true, `TabTearOutWindowRequested` asks the
   application to create and return a WPF `Window` containing a destination
   TabView. ModernWPF never guesses the application's window class, view
   model, lifetime, or ownership.
5. `TabTearOutRequested` supplies that exact Window and asks the application
   to move its item. The control positions and shows the returned window only
   after the application has accepted the request.
6. A later WPF drag over another tear-out-enabled TabView raises
   `ExternalTornOutTabsDropping` with the proposed insertion index. If the
   application sets `AllowDrop`, `ExternalTornOutTabsDropped` asks it to move
   the item and select it in the destination.

Unlike WinUI's native move-size integration, the new WPF window appears after
the pointer is released outside the source strip; it does not replace the
pointer's in-progress drag image with a live top-level window. Rejoining is a
subsequent ordinary WPF tab drag. This is the documented WPF tear-out
adaptation, not an omitted behavior or an `AppWindow` emulation.

### Templates and resources

The package already ships the 64 TabView-named Light, Dark, and High Contrast
keys used by official WPF Fluent `TabControl`. Preview 5 reuses those values
and does not change the stock style. It adds the current source keys that the
new control consumes:

- per-theme drag background, active-tab button background/foreground,
  item-border brush, add-button border thickness, and close-button border
  thickness; and
- shared header padding, item minimum/maximum sizes, icon and close-button
  dimensions, add/scroll-button dimensions, separator margin, selected-item
  border/margin, and shadow depth.

The existing WPF-specific `TabViewForeground` and
`TabViewItemForegroundSelected` aliases remain supported. The source template
is adapted to WPF bindings, triggers, `ScrollViewer`, focus visuals, and
`DropShadowEffect`; it does not reintroduce the deleted `TabControlHelper`,
`TabItemHelper`, `VisualStateEx`, or guessed stock-control template layer.
The item-border, drag-background, add/scroll-button border-thickness, and
close-button border-thickness resources are consumed dynamically by the WPF
template. Dedicated add, overflow, and close button styles consume their own
normal, pointer-over, pressed, and disabled source resources. WPF triggers
likewise consume the item header and icon state resources, while the
source-shaped separator is hidden on the selected tab, the tab immediately to
its left, and both sides of a hovered tab. The two `ActiveTab` button resources
remain declared because they are part of the pinned upstream theme inventory;
the pinned upstream template does not currently consume them either.

## Validation and release gate

Focused product tests must cover:

- the complete dependency-property, event, enum, and event-argument surface;
- explicit items and observable data sources, container lookup, selection,
  content, removal fallback, and collection-change forwarding;
- all width and close-button modes, live public resource overrides, overflow
  buttons, item/close/overflow pointer and pressed states, selected/adjacent
  separators, pointer-hover, middle-click close, right-click non-selection,
  and locale-independent geometry;
- Ctrl+Tab, Ctrl+Shift+Tab, Ctrl+F4, Left/Right, Space/Enter, focus transfer,
  disabled/hidden skipping, and right-to-left flow;
- automation control types, class/name fallback, required single selection,
  item selection, button Invoke providers, and selection notifications;
- cancellable drag, mutable-source reorder, external drop, dropped-outside,
  and the complete WPF Window-based tear-out/rejoin event sequence; and
- Light, Dark, High Contrast, compact resources, and public API/resource-key
  inventories.

Focused Gallery coverage must exercise all ten current examples with the real
control, including add, close, item source, width, overlay, overflow,
keyboard, reorder, context-menu moves, and a real top-level WPF tear-out and
rejoin. The release Gallery pass must capture the default page, overflow,
keyboard focus, drag/reorder outcome, and tear-out window in Light, Dark, and
real OS High Contrast on `net462`, `net8.0-windows7.0`, and
`net10.0-windows7.0`.

No historical `TabControl` facsimile image or test is accepted as Preview 5
evidence. The final clean tip must also pass the complete serialized release
gate, package/API/resource verification, executable package consumers, and
downstream canaries before the Preview 5 tag is published.

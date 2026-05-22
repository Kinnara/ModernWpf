# WPF Gallery Milestone 1 Tracker

Last updated: 2026-05-22

## Goal

Make ModernWpf Gallery visually near-identical to the official WPF Gallery for
all WPF Gallery-equivalent pages and controls, while keeping ModernWpf and
WinUI-style controls such as `NavigationView` where they fit the WinUI Gallery
interaction model.

## Source References

| Source | Local path | Use |
| --- | --- | --- |
| Official WPF Gallery | `D:\repos\WPF-Samples\Sample Applications\WPFGallery` | Primary visual, page, copy, sample, and catalog reference. |
| ModernWpf Gallery | `ModernWpf.Gallery` | Target implementation. |
| Gallery runtime tests | `test\ModernWpf.Gallery.Tests` | Main regression layer for route, page, shell, catalog, and sample parity checks. |

## Copy vs Adapt Rule

Prefer copying official WPF Gallery page structure, sample XAML, page titles,
descriptions, images, spacing, and labels when the target page is a direct WPF
Gallery equivalent.

Adapt instead of raw-copying when the source depends on:

- `WPFGallery` namespaces, navigation services, or view-model plumbing that does
  not exist in ModernWpf Gallery.
- Official WPF Gallery resource keys, converters, or helpers that need to map
  to ModernWpf resource names or local helper controls.
- ModernWpf-specific routing, catalog aliases, test hooks, or automation IDs.
- Pages where ModernWpf intentionally keeps a distinct WinUI/ModernWpf control
  page next to a WPF Gallery page, such as `Calendar` vs `CalendarView` and
  `RichTextEdit` vs `RichEditBox`.
- Controls or interactions that WPF Gallery implements with app-local helpers
  but ModernWpf already has an equivalent reusable control.

In short: the official WPF Gallery is the source of truth for WPF-equivalent
visuals, but most files still need namespace, resource, route, and test
adaptation before they can live in this repo.

## Architecture Direction

The current broad parity pass got WPF Gallery-equivalent pages represented, but
it still relies too much on ModernWpf-specific sample factories that recreate
official WPF Gallery pages in C#. That makes visual parity slower and easier to
drift.

The preferred next architecture is closer to WPF Gallery for WPF-equivalent
pages:

- Keep the ModernWpf outer shell, catalog, and `NavigationView` model so the
  gallery can still host ModernWpf/WinUI-specific pages.
- Add or strengthen a WPF Gallery compatibility layer inside that shell:
  WPF-style page view models with `PageTitle`, `PageDescription`, and sample
  code strings; WPF Gallery-like `PageHeader`, `ControlExample`, `HeaderTile`,
  and navigation-card components; and explicit catalog item to page mappings.
- Prefer adapted `.xaml` pages copied from official WPF Gallery for direct
  WPF-equivalent pages instead of rebuilding those pages through C# sample
  factories.
- Keep C# sample factories for ModernWpf/WinUI-specific pages, generated sample
  content, or pages that do not exist in official WPF Gallery.
- Treat the official WPF Gallery view model/resource/navigation shape as the
  default design for WPF pages, then adapt only the integration points required
  by ModernWpf routing, resource naming, tests, and mixed WPF/WinUI catalog
  support.

This should make future parity rounds more mechanical: copy official page XAML
and view-model state, adapt namespaces/resources/routes, add targeted tests, and
record the checklist status below.

## Current Status

Branch: `maintenance-reboot-1x`

Latest implementation commit: see the current branch tip; this document is
updated with each coherent round.

Goal tracker status in Codex: active, not complete.

Latest local verification for the current branch tip:

- `dotnet test test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --filter "BasicInputControlExamplesMatchOfficialWpfGallerySampleCode|CollectionsControlExamplesMatchOfficialWpfGallerySampleCode|DateMediaAndStatusControlExamplesMatchOfficialWpfGallerySampleCode|LayoutControlExamplesMatchOfficialWpfGallerySampleCode|NavigationControlExamplesMatchOfficialWpfGallerySampleCode|TextControlExamplesMatchOfficialWpfGallerySampleCode|SystemControlExamplesMatchOfficialWpfGallerySampleCode|MessageBoxDynamicSnippetsMatchOfficialWpfGallerySampleCode|DesignGuidanceControlExamplesMatchOfficialWpfGallerySampleCode|WhatsNewControlExamplesMatchOfficialWpfGallerySampleCode" -p:UseSharedCompilation=false`
- `dotnet test test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --no-build`
- `dotnet build ModernWpf.sln --configuration Release -p:UseSharedCompilation=false`
- `dotnet test test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --filter ContentHostWritesRenderedVisualArtifact -p:UseSharedCompilation=false`
- `dotnet test test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --filter ShellNavigationMenuMatchesWpfGalleryReferenceChrome -p:UseSharedCompilation=false`
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -ListCases`
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -BuildModern -Cases Home -Reference None`
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases Home -Reference OfficialWpfGallery`
  - Latest artifact: `artifacts/wpf-gallery-visual-audit/20260522-081437/report.md`
  - Home Light result: Modern `Passed`, official `Passed`, content delta `8.59`, crops `911x771` vs `918x776`.
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -BuildModern -Cases BasicInput,Button -Reference OfficialWpfGallery`
  - Initial Basic Input/Button artifact after card-text alignment: `artifacts/wpf-gallery-visual-audit/20260522-084944/report.md`
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases BasicInput,Button,CheckBox,ComboBox,RadioButton,Slider -Reference OfficialWpfGallery`
  - Latest Basic Input artifact: `artifacts/wpf-gallery-visual-audit/20260522-085307/report.md`
  - Light results: BasicInput `2.39`, Button `1.32`, CheckBox `2.10`, ComboBox `2.05`, RadioButton `1.92`, Slider `2.68`; all Modern and official captures `Passed`.
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases Collections,DataGrid,ListBox,ListView,TreeView -Reference OfficialWpfGallery`
  - Latest Collections artifact: `artifacts/wpf-gallery-visual-audit/20260522-090501/report.md`
  - Light results: Collections `2.14`, DataGrid `8.17`, ListBox `1.56`, ListView `3.39`, TreeView `0.74`; all Modern and official captures `Passed`.
  - DataGrid's higher delta is dominated by volatile official sample rows from `new Random()`; the visible table layout, header, card chrome, and source pane positions match the reference crop.
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases DateAndCalendar,Calendar,DatePicker -Reference OfficialWpfGallery`
  - Latest Date & Calendar artifact: `artifacts/wpf-gallery-visual-audit/20260522-091528/report.md`
  - Light results: DateAndCalendar `0.94`, Calendar `1.31`, DatePicker `0.25`; all Modern and official captures `Passed`.
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases Layout,Expander,Grid,ResizeGrip,GridSplitter,GroupBox,StackPanel,Border -Reference OfficialWpfGallery -TimeoutSeconds 90`
  - Latest Layout artifact: `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md`
  - Light results: Layout `3.7`, Expander `0.88`, Grid `3.35`, ResizeGrip `1.7`, GridSplitter `6`, GroupBox `0.65`, StackPanel `1.14`, Border `5.03`; all Modern and official captures `Passed`.
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases Media,Canvas,Image -Reference OfficialWpfGallery -TimeoutSeconds 90`
  - Media diagnostic artifact: `artifacts/wpf-gallery-visual-audit/20260522-105429/report.md`
  - Modern captures passed, but official captures failed because the current official WPF Gallery `Models/ControlsInfoData.json` omits `Media`, `Canvas`, and `Image` catalog entries and the active `MainWindow` search box is commented out. Do not treat this artifact's deltas as visual drift; Media needs an alternate direct reference-host capture or an explicit decision to classify it source/test-only for Milestone 1.
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases Navigation,Menu,TabControl,Frame,NavigationWindow -Reference OfficialWpfGallery -TimeoutSeconds 90`
  - Latest Navigation artifact: `artifacts/wpf-gallery-visual-audit/20260522-110106/report.md`
  - Light results: Navigation `2.19`, Menu `0.3`, TabControl `0.44`, Frame `0.54`, NavigationWindow `0.81`; all Modern and official captures `Passed`.
- `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases Text,Label,TextBox,TextBlock,RichTextEdit,PasswordBox,Hyperlink -Reference OfficialWpfGallery -TimeoutSeconds 90`
  - Latest Text artifact: `artifacts/wpf-gallery-visual-audit/20260522-110453/report.md`
  - Light results: Text `3.04`, Label `1.27`, TextBox `1.87`, TextBlock `6.24`, RichTextEdit `0.41`, PasswordBox `0.42`, Hyperlink `0.46`; all Modern and official captures `Passed`.
  - TextBlock's higher delta is localized to text rendering/crop-edge variance around the lower wrapped paragraph; the Modern page XAML and UIA text/bounds were checked against the official source/capture.

## Done

| Area | Status | Notes |
| --- | --- | --- |
| Official WPF Gallery source checkout | Done | Cloned under `D:\repos\WPF-Samples`. |
| WPF Gallery catalog sections | Done | WPF Gallery-equivalent sections are represented, including Design Guidance, Samples, Basic Input, Collections, Date & Calendar, Layout, Media, Navigation, Status & Info, Text, and System. |
| ModernWpf/WinUI page preservation | Done | Distinct ModernWpf pages are kept where aliases overlap WPF pages. |
| Page shell and header model | Mostly done | Item, section, support, sample chrome, Home header tile strip, navigation-card resources, category/all-controls headers, WPF Gallery-style navigation page binding paths, and shared compatibility-control source shape have been moved toward WPF Gallery structure and spacing. |
| WPF Gallery navigation view-model shape | Mostly done | Home, section, and All Controls navigation-card pages now expose explicit WPF Gallery-style `ViewModel` objects with `NavigationCards`, `RecentlyAddedOrUpdatedSamplesInfo`, `PageTitle`, `PageDescription`, and `NavigateCommand` as appropriate; page code-behind only adapts those commands to ModernWpf routing callbacks. |
| WPF Gallery compatibility `PageHeader` | Done | ModernWpf Gallery has an adapted WPF Gallery-style `PageHeader` control for copied WPF-equivalent pages; the shared template now uses the official `NullToVisibilityConverter` resource key and `TitleTextBlock` label name while keeping the local heading-level adapter and page-title automation ID for multi-target/test support. |
| WPF Gallery compatibility `ControlExample` | Mostly done | Template resources follow the official WPF Gallery shape, source-code blocks have WPF Gallery-style URI-backed `XamlCodeSource` and `CSharpCodeSource` properties, and local automation/test hooks remain where needed. |
| WPF Gallery compatibility color controls | Done | Adapted `ColorPageExample` and `ColorTile` controls plus `ColorTilesPanelStyle` are available; all six Color subsections now use adapted official WPF Gallery XAML instead of C# factory recreation. |
| Official WPF Gallery ControlImages asset set | Partial | Shared `Assets/ControlImages` filenames were audited against the official checkout and the missing official-only reference assets were copied locally with pack-resource regression coverage. |
| WPF Gallery visual audit harness | Done | `tools/visual-checks/Run-WpfGalleryVisualAudit.ps1` lists every WPF-equivalent route, can launch ModernWpf in visual-test mode, can UIA-drive the official WPF Gallery checkout including the Settings theme picker, returns the reference app to Home before target navigation, falls back to official navigation-card buttons for section/item pages when native tree input is unavailable, clicks official expanded tree-item header text instead of expanded parent bounds for category pages, accepts nonblank ModernWpf rendered content artifacts as a readiness fallback, and writes screenshot/content-crop/UIA/report artifacts under ignored `artifacts/wpf-gallery-visual-audit/`; ModernWpf visual-test mode now writes in-process `ContentRootGrid.png` and `GalleryContentHost.png` artifacts so local audits still have page-content pixels when OS-level capture returns black client content. |
| Header descriptions | Done | Visible WPF item page descriptions now match official WPF Gallery view models, including empty description slots. |
| What's New support page | Mostly done | Uses adapted official WPF Gallery page content and runtime checks; source panes now match official WPF Gallery snippet strings. Screenshot parity remains. |
| Design Guidance pages | Mostly done | Color, Iconography, Typography, Spacing, and Geometry now use adapted official WPF Gallery page shells/XAML with direct-page runtime tests; Color subsections now use official-style color controls and XAML. |
| Design Guidance sample code panes | Done | Runtime coverage now verifies the existing `ControlExample` snippets for Typography and Geometry against the official WPF Gallery reference values. |
| User Dashboard sample | Mostly done | Uses an adapted official WPF Gallery XAML/code-behind/view-model page under `Pages/WpfGallery/Samples`; deterministic seed data, ModernWpf integration hooks, and runtime layout/behavior tests are in place. Screenshot parity remains. |
| Basic Input pages | Mostly done | Button, CheckBox, ComboBox, RadioButton, and Slider now use adapted official WPF Gallery XAML pages with direct-page runtime tests; section navigation cards now render official WPF Gallery subtitle text instead of richer ModernWpf descriptions. |
| Basic Input sample code panes | Done | Runtime coverage now verifies the ordered `ControlExample` headers and XAML snippets for Button, CheckBox, ComboBox, RadioButton, and Slider against the official WPF Gallery reference values; these pages do not expose C# snippet panes. |
| Collections pages | Mostly done | DataGrid, ListBox, ListView, and TreeView now use adapted official WPF Gallery XAML pages with direct-page runtime tests; the Light visual audit is recorded, with DataGrid's higher delta classified as volatile official sample data rather than observed layout drift. |
| Collections sample code panes | Done | Runtime coverage now verifies the ordered `ControlExample` headers and XAML snippets for DataGrid, ListBox, ListView, and TreeView against the official WPF Gallery reference values; these pages do not expose C# snippet panes. |
| Date & Calendar pages | Mostly done | Calendar and DatePicker now use adapted official WPF Gallery XAML pages while keeping distinct WinUI-style pages; the Light visual audit is recorded with low deltas and matching inspected crops. |
| Date & Calendar sample code panes | Done | Runtime coverage now verifies the ordered `ControlExample` headers and XAML snippets for Calendar and DatePicker against the official WPF Gallery reference values. |
| Layout pages | Mostly done | Border, Expander, Grid, GridSplitter, GroupBox, ResizeGrip, and StackPanel now use adapted official WPF Gallery XAML pages with direct-page runtime tests; the Light visual audit is recorded, with inspected higher-delta pages matching the official layout. |
| Layout sample code panes | Done | Runtime coverage now verifies the ordered `ControlExample` headers, XAML snippets, and the ResizeGrip C# snippet for Border, Expander, Grid, GridSplitter, GroupBox, ResizeGrip, and StackPanel against the official WPF Gallery reference values. |
| Media section | Mostly done | Canvas and Image now use adapted WPF Gallery XAML pages and are exposed in ModernWpf; the current official WPF Gallery checkout still contains orphaned Media pages but omits the Media group/items from `ControlsInfoData.json`, so normal UI-driven visual reference capture is unavailable. |
| Media sample code panes | Done | Runtime coverage now verifies the ordered `ControlExample` headers and XAML snippets for Canvas and Image against the official WPF Gallery reference values. |
| Navigation pages | Mostly done | Menu, TabControl, Frame, and NavigationWindow now use adapted official WPF Gallery XAML pages with direct-page runtime tests; Hyperlink follows the adapted WPF Gallery Text page path; the Light visual audit is recorded with low deltas and matching inspected crops. |
| Navigation sample code panes | Done | Runtime coverage now verifies the ordered `ControlExample` headers, XAML snippets, and the NavigationWindow C# snippet for Menu, TabControl, Frame, and NavigationWindow against the official WPF Gallery reference values. |
| Status & Info pages | Mostly done | ProgressBar and ToolTip now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| Status & Info sample code panes | Done | Runtime coverage now verifies the ordered `ControlExample` headers and XAML snippets for ProgressBar and ToolTip against the official WPF Gallery reference values. |
| Text pages | Mostly done | Label, TextBox, TextBlock, RichTextEdit, PasswordBox, and Hyperlink now use adapted official WPF Gallery XAML pages with direct-page runtime tests; the Light visual audit is recorded, with TextBlock's higher delta classified as rendering/crop-edge variance rather than source drift. |
| Text sample code panes | Done | Runtime coverage now verifies the ordered `ControlExample` headers and XAML snippets for Label, TextBox, TextBlock, RichTextEdit, PasswordBox, and Hyperlink against the official WPF Gallery reference values. |
| System pages | Mostly done | File and folder dialogs, MessageBox, and Clipboard now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| System sample code panes | Done | Runtime coverage now verifies the ordered `ControlExample` headers, XAML snippets, C# snippets, and MessageBox dynamic snippet variants for File and Folder Dialogs, MessageBox, and Clipboard against the official WPF Gallery reference values. |
| Runtime regression layer | Done for current scope | Gallery runtime tests currently cover page loading and many WPF Gallery layout/sample details. |

## Needs Work

| Area | Status | Next action |
| --- | --- | --- |
| Visual screenshot pass against official WPF Gallery | Open | Home Light first pass is recorded at `artifacts/wpf-gallery-visual-audit/20260522-081437/report.md`; full Basic Input Light pass is recorded at `artifacts/wpf-gallery-visual-audit/20260522-085307/report.md`; full Collections Light pass is recorded at `artifacts/wpf-gallery-visual-audit/20260522-090501/report.md`; full Date & Calendar Light pass is recorded at `artifacts/wpf-gallery-visual-audit/20260522-091528/report.md`; full Layout Light pass is recorded at `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md`; full Navigation Light pass is recorded at `artifacts/wpf-gallery-visual-audit/20260522-110106/report.md`; full Text Light pass is recorded at `artifacts/wpf-gallery-visual-audit/20260522-110453/report.md`; Media is source/test-aligned but currently lacks a reachable official catalog route in the checkout, with diagnostic artifact `artifacts/wpf-gallery-visual-audit/20260522-105429/report.md`; continue running `Run-WpfGalleryVisualAudit.ps1` one section at a time against the official WPF Gallery executable, inspect content-crop reports, and record accepted artifact paths/findings. |
| Page-by-page exact XAML audit | Open | For each WPF-equivalent page, compare ModernWpf sample factories against official WPF Gallery XAML and replace approximations with adapted copies where feasible. Remaining section page content, sample-code parity, and visual parity remain open. |
| Home page | Partial | Header tile strip, navigation cards, `ViewModel.NavigationCards` / `ViewModel.RecentlyAddedOrUpdatedSamplesInfo` bindings, and `ViewModel.NavigateCommand` card command binding now match adapted official dashboard shape; Light visual audit passes on both apps with delta `8.59` and near-matching page-root crops (`911x771` vs `918x776`). Continue first-viewport visual review for remaining spacing, text-rendering, and shell chrome drift. |
| All controls page | Partial | Header, navigation cards, `ViewModel.PageTitle` / `ViewModel.NavigationCards` bindings, and `ViewModel.NavigateCommand` card command binding now use adapted official structure; official WPF Gallery has no in-page search on this page, so verify grouping, sort order, card subtitles, and tile sizing against `AllSamplesPage`. |
| Section pages | Partial | Headers, navigation cards, `ViewModel.PageTitle` / `ViewModel.NavigationCards` bindings, and `ViewModel.NavigateCommand` card command binding now use adapted official structure; verify each section's title, description, hero/card layout, item order, and empty-space behavior against official WPF Gallery. |
| Sample code panes | Partial | Basic Input, Collections, Date & Calendar, Design Guidance, Layout, Media, Navigation, Status & Info, Text, System, and What's New `ControlExample` snippets are now covered by runtime parity tests against the official WPF Gallery reference values, including ResizeGrip, NavigationWindow, System C# panes, and MessageBox dynamic snippet variants; continue section-by-section coverage for remaining WPF-equivalent pages and C# snippets where present. |
| Assets and thumbnails | Partial | Common `Assets/ControlImages` files now match the official checkout, and official-only reference assets are present locally; continue verifying item image choices and non-`ControlImages` visuals page-by-page. |
| Typography and spacing metrics | Open | Audit page root margins, header spacing, card spacing, sample spacing, font sizes, and line heights against official WPF Gallery. |
| Theme behavior | Open | Check Light, Dark, and High Contrast views for WPF Gallery-equivalent pages. |
| Keyboard and automation details | Open | Shared `PageHeader` label naming, focus order, tab stops, and heading behavior now match the official template shape through the local compatibility adapter; continue aligning remaining copied pages and controls. |
| Manual visual acceptance checklist | Open | Record reviewed pages with screenshots or artifact paths once a visual pass exists. |

## Working Checklist

Use this checklist for future rounds.

| Page or group | Structural tests | Exact source audit | Visual checked | Notes |
| --- | --- | --- | --- | --- |
| Home | Partial | Partial | Partial | Header tile strip, navigation cards, dashboard-style `ViewModel.*` bindings, and `ViewModel.NavigateCommand` card commands now use adapted official WPF Gallery structure; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-081437/report.md` with Modern/official crops `911x771` vs `918x776` and delta `8.59`; remaining visual review should inspect spacing, text-rendering, and shell chrome drift. |
| What's New | Done | Done | Open | Adapted from official WPF Gallery XAML/code strings with local navigation and accent-resource integration; sample headers/snippets are runtime-covered; screenshot parity remains. |
| All controls | Partial | Partial | Open | Runtime shell checks plus adapted `PageHeader`, navigation-card resources, official-style `ViewModel.*` bindings, and `ViewModel.NavigateCommand` card commands exist; official WPF Gallery has no in-page search, and grouping/order/card sizing still need exact visual audit. |
| Design Guidance section | Partial | Partial | Open | Color, Iconography, Typography, Spacing, and Geometry now use adapted official WPF Gallery page shells/XAML; Typography and Geometry sample code panes are runtime-covered; section visual audit remains. |
| Color | Done | Done | Open | Direct page shell and all six subsections now use adapted official WPF Gallery XAML with `PageHeader`, selector, `ColorPageExample`, and `ColorTile`; screenshot parity remains. |
| Typography | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample header/snippet is runtime-covered; screenshot parity remains. |
| Spacing | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, spacing imagery, and spacing table; screenshot parity remains. |
| Geometry | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, geometry imagery, and corner-radius table; sample snippet is runtime-covered; screenshot parity remains. |
| Iconography | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, instructions, icon library, details pane, and pagination; screenshot parity remains. |
| Samples section | Partial | Partial | Open | User Dashboard exact source audit is covered; section visual audit remains. |
| User Dashboard | Done | Done | Open | Adapted from official WPF Gallery XAML/code-behind/view-model shape with deterministic seed data and local automation notification guards; runtime layout/behavior tested, screenshot parity remains. |
| Basic Input section | Partial | Partial | Partial | Basic Input section header, navigation cards, `ViewModel.*` bindings, item pages, and item-page sample code panes now use adapted official WPF Gallery resources; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-085307/report.md` with BasicInput delta `2.39` and crops `863x767` vs `868x758`. |
| Button | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-085307/report.md` with delta `1.32` and crops `863x767` vs `868x758`; remaining review is mostly text rendering/crop-size drift. |
| CheckBox | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-085307/report.md` with delta `2.10` and crops `863x767` vs `868x758`. |
| ComboBox | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-085307/report.md` with delta `2.05` and crops `863x767` vs `868x758`. |
| RadioButton | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-085307/report.md` with delta `1.92` and crops `863x767` vs `868x758`. |
| Slider | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-085307/report.md` with delta `2.68` and crops `863x767` vs `868x758`. |
| Collections section | Partial | Partial | Partial | Collection item pages and item-page sample code panes now use adapted official WPF Gallery XAML; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-090501/report.md` with Collections delta `2.14` and crops `863x767` vs `868x758`. |
| DataGrid | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-090501/report.md` with delta `8.17` and crops `863x767` vs `868x758`; visible layout matches, and the remaining delta is dominated by official WPF Gallery's random sample row values. |
| ListBox | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-090501/report.md` with delta `1.56` and crops `863x767` vs `868x758`. |
| ListView | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-090501/report.md` with delta `3.39` and crops `863x767` vs `868x758`. |
| TreeView | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-090501/report.md` with delta `0.74` and crops `863x767` vs `868x758`. |
| Date & Calendar section | Partial | Partial | Partial | Date item pages and item-page sample code panes now use adapted official WPF Gallery XAML; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-091528/report.md` with DateAndCalendar delta `0.94` and crops `863x767` vs `868x758`. |
| Calendar | Done | Done | Partial | Adapted from official WPF Gallery XAML; WPF page preserved separately from `CalendarView`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-091528/report.md` with delta `1.31` and crops `863x767` vs `868x758`. |
| DatePicker | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-091528/report.md` with delta `0.25` and crops `863x767` vs `868x758`. |
| Layout section | Partial | Partial | Partial | Layout item pages and item-page sample code panes now use adapted official WPF Gallery XAML/C# snippets; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md` with Layout delta `3.7` and crops `863x767` vs `868x758`. |
| Expander | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md` with delta `0.88` and crops `863x767` vs `868x758`. |
| Grid | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; live shorthand example uses explicit definitions for WPF target compatibility, while sample headers/snippets remain source-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md` with delta `3.35` and crops `863x767` vs `868x758`. |
| ResizeGrip | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, and support click handler; sample header, XAML snippet, and C# snippet are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md` with delta `1.7` and crops `863x767` vs `868x758`. |
| GridSplitter | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md` with delta `6` and crops `863x767` vs `868x758`; inspected crops match aside from minor rendering/crop-size differences. |
| GroupBox | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md` with delta `0.65` and crops `863x767` vs `868x758`. |
| StackPanel | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md` with delta `1.14` and crops `863x767` vs `868x758`. |
| Border | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-105031/report.md` with delta `5.03` and crops `863x767` vs `868x758`; inspected crops match aside from minor rendering/crop-size differences. |
| Media section | Partial | Partial | Reference unavailable | Canvas and Image item pages and item-page sample code panes now use adapted official WPF Gallery XAML; normal official visual audit is unavailable because the current official WPF Gallery catalog omits Media entries. Diagnostic artifact: `artifacts/wpf-gallery-visual-audit/20260522-105429/report.md`. |
| Canvas | Done | Done | Reference unavailable | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; normal official visual audit is unavailable because the current official WPF Gallery catalog omits `Canvas`. |
| Image | Done | Done | Reference unavailable | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, and official image asset; sample headers/snippets are runtime-covered; normal official visual audit is unavailable because the current official WPF Gallery catalog omits `Image`. |
| Navigation section | Partial | Partial | Partial | Navigation item pages and item-page sample code panes now use adapted official WPF Gallery XAML/C# snippets; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110106/report.md` with Navigation delta `2.19` and crops `863x767` vs `868x758`. |
| Menu | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110106/report.md` with delta `0.3` and crops `863x767` vs `868x758`. |
| TabControl | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110106/report.md` with delta `0.44` and crops `863x767` vs `868x758`. |
| Frame | Done | Done | Partial | Adapted from official WPF Gallery XAML with support window/page content; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110106/report.md` with delta `0.54` and crops `863x767` vs `868x758`. |
| NavigationWindow | Done | Done | Partial | Adapted from official WPF Gallery XAML with support navigation pages; sample header, XAML snippet, and C# snippet are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110106/report.md` with delta `0.81` and crops `863x767` vs `868x758`. |
| Text section | Partial | Partial | Partial | Text item pages and item-page sample code panes now use adapted official WPF Gallery XAML; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110453/report.md` with Text delta `3.04` and crops `863x767` vs `868x758`. |
| Label | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110453/report.md` with delta `1.27` and crops `863x767` vs `868x758`. |
| TextBox | Done | Done | Partial | Adapted from official WPF Gallery XAML, including input validation rule binding; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110453/report.md` with delta `1.87` and crops `863x767` vs `868x758`. |
| TextBlock | Done | Done | Partial | Adapted from official WPF Gallery XAML with inline text examples; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110453/report.md` with delta `6.24` and crops `863x767` vs `868x758`; inspected source/UIA show matching wrapped text and bounds, with remaining delta from text rendering/crop-edge variance. |
| RichTextEdit | Done | Done | Partial | Adapted from official WPF Gallery XAML; WPF page preserved separately from `RichEditBox`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110453/report.md` with delta `0.41` and crops `863x767` vs `868x758`. |
| PasswordBox | Done | Done | Partial | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110453/report.md` with delta `0.42` and crops `863x767` vs `868x758`. |
| Hyperlink | Done | Done | Partial | Adapted from official WPF Gallery XAML under Text pages; sample headers/snippets are runtime-covered; Light visual audit recorded at `artifacts/wpf-gallery-visual-audit/20260522-110453/report.md` with delta `0.46` and crops `863x767` vs `868x758`. |
| Status & Info section | Partial | Partial | Open | Status item pages and item-page sample code panes now use adapted official WPF Gallery XAML; section page visual audit remains. |
| ProgressBar | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; screenshot parity remains. |
| ToolTip | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; sample headers/snippets are runtime-covered; screenshot parity remains. |
| System section | Partial | Partial | Open | System item pages and item-page sample code panes now use adapted official WPF Gallery XAML/C# snippets; section page visual audit remains. |
| File and Folder Dialogs | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, and support file/folder handlers; sample headers/snippets are runtime-covered; screenshot parity remains. |
| MessageBox | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, support message-box handlers, and official dynamic snippet variants; sample headers/snippets are runtime-covered; screenshot parity remains. |
| Clipboard | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, Clipboard note, and support clipboard handlers; sample headers/snippets are runtime-covered; screenshot parity remains. |

## Next Recommended Round

Start the visual screenshot audit one section at a time. For each section:

1. Build ModernWpf Gallery and restore/build the official WPF Gallery checkout
   if needed.
2. Run `.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -Cases <section-and-pages> -Reference OfficialWpfGallery`.
3. Inspect the report, full screenshots, content crops, and UIA trees for shell,
   spacing, typography, card, header, and sample-layout drift.
4. Fix the highest-impact visible mismatch in that section, preferring adapted
   official WPF Gallery XAML/resources where possible.
5. Record accepted artifact paths/findings in this file and commit a coherent
   section-sized change.

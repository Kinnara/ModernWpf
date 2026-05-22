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

- `dotnet build test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release -p:UseSharedCompilation=false`
- `dotnet test test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --no-build`
- `dotnet build ModernWpf.sln --configuration Release -p:UseSharedCompilation=false`

## Done

| Area | Status | Notes |
| --- | --- | --- |
| Official WPF Gallery source checkout | Done | Cloned under `D:\repos\WPF-Samples`. |
| WPF Gallery catalog sections | Done | WPF Gallery-equivalent sections are represented, including Design Guidance, Samples, Basic Input, Collections, Date & Calendar, Layout, Media, Navigation, Status & Info, Text, and System. |
| ModernWpf/WinUI page preservation | Done | Distinct ModernWpf pages are kept where aliases overlap WPF pages. |
| Page shell and header model | Mostly done | Item, section, support, sample chrome, Home header tile strip, navigation-card resources, category/all-controls headers, WPF Gallery-style navigation page binding paths, and shared compatibility-control source shape have been moved toward WPF Gallery structure and spacing. |
| WPF Gallery compatibility `PageHeader` | Done | ModernWpf Gallery has an adapted WPF Gallery-style `PageHeader` control for copied WPF-equivalent pages; the shared template now uses the official `NullToVisibilityConverter` resource key and `TitleTextBlock` label name while keeping the local heading-level adapter and page-title automation ID for multi-target/test support. |
| WPF Gallery compatibility `ControlExample` | Mostly done | Template resources follow the official WPF Gallery shape, source-code blocks have WPF Gallery-style URI-backed `XamlCodeSource` and `CSharpCodeSource` properties, and local automation/test hooks remain where needed. |
| WPF Gallery compatibility color controls | Partial | Adapted `ColorPageExample` and `ColorTile` controls plus `ColorTilesPanelStyle` are available; the Color Text subsection now uses adapted official XAML. Remaining color subsections still need migration from the C# factory. |
| Header descriptions | Done | Visible WPF item page descriptions now match official WPF Gallery view models, including empty description slots. |
| Design Guidance pages | Mostly done | Color, Iconography, Typography, Spacing, and Geometry now use adapted official WPF Gallery page shells/XAML with direct-page runtime tests; Color Text subsection now uses official-style color controls and XAML. |
| User Dashboard sample | Mostly done | Uses an adapted official WPF Gallery XAML/code-behind/view-model page under `Pages/WpfGallery/Samples`; deterministic seed data, ModernWpf integration hooks, and runtime layout/behavior tests are in place. Screenshot parity remains. |
| Basic Input pages | Mostly done | Button, CheckBox, ComboBox, RadioButton, and Slider now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| Collections pages | Mostly done | DataGrid, ListBox, ListView, and TreeView now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| Date & Calendar pages | Mostly done | Calendar and DatePicker now use adapted official WPF Gallery XAML pages while keeping distinct WinUI-style pages. |
| Layout pages | Mostly done | Border, Expander, Grid, GridSplitter, GroupBox, ResizeGrip, and StackPanel now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| Media section | Mostly done | Official WPF Gallery media pages are exposed in the catalog/navigation; Canvas and Image now use adapted WPF Gallery XAML pages. |
| Navigation pages | Mostly done | Menu, TabControl, Frame, and NavigationWindow now use adapted official WPF Gallery XAML pages with direct-page runtime tests; Hyperlink follows the adapted WPF Gallery Text page path. |
| Status & Info pages | Mostly done | ProgressBar and ToolTip now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| Text pages | Mostly done | Label, TextBox, TextBlock, RichTextEdit, PasswordBox, and Hyperlink now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| System pages | Mostly done | File and folder dialogs, MessageBox, and Clipboard now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| Runtime regression layer | Done for current scope | Gallery runtime tests currently cover page loading and many WPF Gallery layout/sample details. |

## Needs Work

| Area | Status | Next action |
| --- | --- | --- |
| Visual screenshot pass against official WPF Gallery | Open | Add or run a WPF Gallery reference capture pass and compare page-by-page screenshots, not just runtime structure. |
| Page-by-page exact XAML audit | Open | For each WPF-equivalent page, compare ModernWpf sample factories against official WPF Gallery XAML and replace approximations with adapted copies where feasible. Remaining Color subsections, remaining section page content, sample-code parity, and visual parity remain open. |
| Home page | Partial | Header tile strip, navigation cards, and `ViewModel.NavigationCards` / `ViewModel.RecentlyAddedOrUpdatedSamplesInfo` bindings now match adapted official dashboard shape; compare remaining first-viewport screenshot parity against official WPF Gallery. |
| All controls page | Partial | Header, navigation cards, and `ViewModel.PageTitle` / `ViewModel.NavigationCards` bindings now use adapted official structure; official WPF Gallery has no in-page search on this page, so verify grouping, sort order, card subtitles, and tile sizing against `AllSamplesPage`. |
| Section pages | Partial | Headers, navigation cards, and `ViewModel.PageTitle` / `ViewModel.NavigationCards` bindings now use adapted official structure; verify each section's title, description, hero/card layout, item order, and empty-space behavior against official WPF Gallery. |
| Sample code panes | Open | Verify XAML and C# snippets match official WPF Gallery examples where the sample is WPF-equivalent. |
| Assets and thumbnails | Open | Compare control images and icon choices against official WPF Gallery assets; copy/adapt missing official assets where license and repo structure allow. |
| Typography and spacing metrics | Open | Audit page root margins, header spacing, card spacing, sample spacing, font sizes, and line heights against official WPF Gallery. |
| Theme behavior | Open | Check Light, Dark, and High Contrast views for WPF Gallery-equivalent pages. |
| Keyboard and automation details | Open | Shared `PageHeader` label naming, focus order, tab stops, and heading behavior now match the official template shape through the local compatibility adapter; continue aligning remaining copied pages and controls. |
| Manual visual acceptance checklist | Open | Record reviewed pages with screenshots or artifact paths once a visual pass exists. |

## Working Checklist

Use this checklist for future rounds.

| Page or group | Structural tests | Exact source audit | Visual checked | Notes |
| --- | --- | --- | --- | --- |
| Home | Partial | Partial | Open | Header tile strip, navigation cards, and dashboard-style `ViewModel.*` bindings now use adapted official WPF Gallery structure; screenshot parity still needs audit. |
| All controls | Partial | Partial | Open | Runtime shell checks plus adapted `PageHeader`, navigation-card resources, and official-style `ViewModel.*` bindings exist; official WPF Gallery has no in-page search, and grouping/order/card sizing still need exact visual audit. |
| Design Guidance section | Partial | Partial | Open | Color, Iconography, Typography, Spacing, and Geometry now use adapted official WPF Gallery page shells/XAML; Color non-text subsections and section visual audit remain. |
| Color | Partial | Partial | Open | Direct page shell now matches official WPF Gallery Color page with `PageHeader`, selector, and section host; `ColorPageExample`, `ColorTile`, and the Text subsection are adapted from official XAML. Fill, Stroke, Background, Signal, High Contrast, and screenshot parity remain. |
| Typography | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Spacing | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, spacing imagery, and spacing table; screenshot parity remains. |
| Geometry | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, geometry imagery, and corner-radius table; screenshot parity remains. |
| Iconography | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, instructions, icon library, details pane, and pagination; screenshot parity remains. |
| Samples section | Partial | Partial | Open | User Dashboard exact source audit is covered; section visual audit remains. |
| User Dashboard | Done | Done | Open | Adapted from official WPF Gallery XAML/code-behind/view-model shape with deterministic seed data and local automation notification guards; runtime layout/behavior tested, screenshot parity remains. |
| Basic Input section | Partial | Partial | Open | Basic Input section header, navigation cards, `ViewModel.*` bindings, and item pages now use adapted official WPF Gallery resources; section page visual audit remains. |
| Button | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| CheckBox | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| ComboBox | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| RadioButton | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Slider | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Collections section | Partial | Partial | Open | Collection item pages now use adapted official WPF Gallery XAML; section page visual audit remains. |
| DataGrid | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| ListBox | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| ListView | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| TreeView | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Date & Calendar section | Partial | Partial | Open | Date item pages now use adapted official WPF Gallery XAML; section page visual audit remains. |
| Calendar | Done | Done | Open | Adapted from official WPF Gallery XAML; WPF page preserved separately from `CalendarView`; screenshot parity remains. |
| DatePicker | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Layout section | Partial | Partial | Open | Layout item pages now use adapted official WPF Gallery XAML; section page visual audit remains. |
| Expander | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Grid | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; live shorthand example uses explicit definitions for WPF target compatibility; screenshot parity remains. |
| ResizeGrip | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, and support click handler; screenshot parity remains. |
| GridSplitter | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| GroupBox | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| StackPanel | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Border | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Media section | Partial | Partial | Open | Canvas and Image now use adapted official WPF Gallery XAML; section visual audit remains. |
| Canvas | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Image | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, and official image asset; screenshot parity remains. |
| Navigation section | Partial | Partial | Open | Navigation item pages now use adapted official WPF Gallery XAML; section page visual audit remains. |
| Menu | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| TabControl | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Frame | Done | Done | Open | Adapted from official WPF Gallery XAML with support window/page content; screenshot parity remains. |
| NavigationWindow | Done | Done | Open | Adapted from official WPF Gallery XAML with support navigation pages; screenshot parity remains. |
| Text section | Partial | Partial | Open | Text item pages now use adapted official WPF Gallery XAML; section page visual audit remains. |
| Label | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| TextBox | Done | Done | Open | Adapted from official WPF Gallery XAML, including input validation rule binding; screenshot parity remains. |
| TextBlock | Done | Done | Open | Adapted from official WPF Gallery XAML with inline text examples; screenshot parity remains. |
| RichTextEdit | Done | Done | Open | Adapted from official WPF Gallery XAML; WPF page preserved separately from `RichEditBox`; screenshot parity remains. |
| PasswordBox | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Hyperlink | Done | Done | Open | Adapted from official WPF Gallery XAML under Text pages; screenshot parity remains. |
| Status & Info section | Partial | Partial | Open | Status item pages now use adapted official WPF Gallery XAML; section page visual audit remains. |
| ProgressBar | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| ToolTip | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| System section | Partial | Partial | Open | System item pages now use adapted official WPF Gallery XAML; section page visual audit remains. |
| File and Folder Dialogs | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, and support file/folder handlers; screenshot parity remains. |
| MessageBox | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, and support message-box handlers; screenshot parity remains. |
| Clipboard | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader`, `ControlExample`, Clipboard note, and support clipboard handlers; screenshot parity remains. |

## Next Recommended Round

Start with the page-by-page exact source audit for one section at a time. For
each page:

1. Compare official WPF Gallery XAML, view model title/description, and code
   snippets against the ModernWpf sample factory output.
2. Copy or adapt the official page content where the ModernWpf page is still an
   approximation.
3. Add or tighten tests for labels, margins, sample headers, snippet text, and
   page description behavior.
4. Record the checklist status in this file.
5. Commit a coherent section-sized change.

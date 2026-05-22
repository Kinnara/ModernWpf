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
| Page shell and header model | Mostly done | Item, section, support, and sample chrome have been moved toward WPF Gallery structure and spacing. |
| WPF Gallery compatibility `PageHeader` | Done | ModernWpf Gallery has an adapted WPF Gallery-style `PageHeader` control for copied WPF-equivalent pages. |
| Header descriptions | Done | Visible WPF item page descriptions now match official WPF Gallery view models, including empty description slots. |
| Design Guidance pages | Mostly done | Color, Typography, Spacing, Geometry, and Iconography have WPF Gallery-oriented page coverage and tests. |
| User Dashboard sample | Mostly done | WPF Gallery-style layout and behavior are represented and tested. |
| Basic Input pages | Mostly done | Button, CheckBox, ComboBox, RadioButton, and Slider are covered by WPF Gallery parity tests. |
| Collections pages | Mostly done | DataGrid, ListBox, ListView, and TreeView now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| Date & Calendar pages | Mostly done | Calendar and DatePicker now use adapted official WPF Gallery XAML pages while keeping distinct WinUI-style pages. |
| Layout pages | Mostly done | Expander, Grid, ResizeGrip, GridSplitter, GroupBox, StackPanel, Border, Canvas, and Image coverage exists through sample factories and tests. |
| Media section | Mostly done | Official WPF Gallery media pages are exposed in the catalog/navigation; Canvas and Image now use adapted WPF Gallery XAML pages. |
| Navigation pages | Mostly done | Menu, TabControl, Frame, and NavigationWindow now use adapted official WPF Gallery XAML pages with direct-page runtime tests; Hyperlink follows the adapted WPF Gallery Text page path. |
| Status & Info pages | Mostly done | ProgressBar and ToolTip now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| Text pages | Mostly done | Label, TextBox, TextBlock, RichTextEdit, PasswordBox, and Hyperlink now use adapted official WPF Gallery XAML pages with direct-page runtime tests. |
| System pages | Mostly done | File and folder dialogs, MessageBox, and Clipboard coverage exists through sample factories and tests. |
| Runtime regression layer | Done for current scope | Gallery runtime tests currently cover page loading and many WPF Gallery layout/sample details. |

## Needs Work

| Area | Status | Next action |
| --- | --- | --- |
| Visual screenshot pass against official WPF Gallery | Open | Add or run a WPF Gallery reference capture pass and compare page-by-page screenshots, not just runtime structure. |
| Page-by-page exact XAML audit | Open | For each WPF-equivalent page, compare ModernWpf sample factories against official WPF Gallery XAML and replace approximations with adapted copies where feasible. Canvas and Image are the first adapted XAML pages. |
| Home page | Open | Compare first viewport, card layout, copy, and navigation affordances against official WPF Gallery. |
| All controls page | Open | Verify grouping, sort order, card subtitles, tile sizing, and search behavior against official WPF Gallery. |
| Section pages | Open | Verify each section's title, description, hero/card layout, item order, and empty-space behavior against official WPF Gallery. |
| Sample code panes | Open | Verify XAML and C# snippets match official WPF Gallery examples where the sample is WPF-equivalent. |
| Assets and thumbnails | Open | Compare control images and icon choices against official WPF Gallery assets; copy/adapt missing official assets where license and repo structure allow. |
| Typography and spacing metrics | Open | Audit page root margins, header spacing, card spacing, sample spacing, font sizes, and line heights against official WPF Gallery. |
| Theme behavior | Open | Check Light, Dark, and High Contrast views for WPF Gallery-equivalent pages. |
| Keyboard and automation details | Open | Continue aligning focus order, heading levels, names, and tab stops with official WPF Gallery where visible behavior depends on them. |
| Manual visual acceptance checklist | Open | Record reviewed pages with screenshots or artifact paths once a visual pass exists. |

## Working Checklist

Use this checklist for future rounds.

| Page or group | Structural tests | Exact source audit | Visual checked | Notes |
| --- | --- | --- | --- | --- |
| Home | Open | Open | Open | Needs official first-viewport comparison. |
| All controls | Partial | Open | Open | Runtime shell checks exist; exact visual audit still needed. |
| Design Guidance section | Partial | Open | Open | Section/page layout exists; exact source audit remains. |
| Color | Partial | Open | Open | Selector/text layout tested; screenshot parity remains. |
| Typography | Partial | Open | Open | Needs exact official content and visual pass. |
| Spacing | Partial | Open | Open | Needs exact official content and visual pass. |
| Geometry | Partial | Open | Open | Needs exact official content and visual pass. |
| Iconography | Partial | Open | Open | Icon library layout tested; screenshot parity remains. |
| Samples section | Partial | Open | Open | User Dashboard covered; section visual audit remains. |
| User Dashboard | Partial | Open | Open | Runtime layout/behavior tested. |
| Basic Input section | Partial | Open | Open | Section visual audit remains. |
| Button | Partial | Open | Open | Runtime parity checks exist. |
| CheckBox | Partial | Open | Open | Runtime parity checks exist. |
| ComboBox | Partial | Open | Open | Runtime parity checks exist. |
| RadioButton | Partial | Open | Open | Runtime parity checks exist. |
| Slider | Partial | Open | Open | Runtime parity checks exist. |
| Collections section | Partial | Partial | Open | Collection item pages now use adapted official WPF Gallery XAML; section page visual audit remains. |
| DataGrid | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| ListBox | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| ListView | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| TreeView | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Date & Calendar section | Partial | Partial | Open | Date item pages now use adapted official WPF Gallery XAML; section page visual audit remains. |
| Calendar | Done | Done | Open | Adapted from official WPF Gallery XAML; WPF page preserved separately from `CalendarView`; screenshot parity remains. |
| DatePicker | Done | Done | Open | Adapted from official WPF Gallery XAML with `PageHeader` and `ControlExample`; screenshot parity remains. |
| Layout section | Partial | Open | Open | Section visual audit remains. |
| Expander | Partial | Open | Open | Runtime parity checks exist. |
| Grid | Partial | Open | Open | Runtime parity checks exist. |
| ResizeGrip | Partial | Open | Open | Runtime parity checks exist. |
| GridSplitter | Partial | Open | Open | Runtime parity checks exist. |
| GroupBox | Partial | Open | Open | Runtime parity checks exist. |
| StackPanel | Partial | Open | Open | Runtime parity checks exist. |
| Border | Partial | Open | Open | Runtime parity checks exist. |
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
| System section | Partial | Open | Open | Section visual audit remains. |
| File and Folder Dialogs | Partial | Open | Open | Runtime parity checks exist. |
| MessageBox | Partial | Open | Open | Runtime parity checks exist. |
| Clipboard | Partial | Open | Open | Runtime parity checks exist. |

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

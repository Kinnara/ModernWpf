# WPF Gallery Milestone 1 Tracker

Last updated: 2026-05-21

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

## Current Status

Branch: `maintenance-reboot-1x`

Latest implementation commit: `c4c31a18 Align WPF Gallery item descriptions`

Goal tracker status in Codex: paused, not complete.

Local verification at `c4c31a18`:

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
| Header descriptions | Done | Visible WPF item page descriptions now match official WPF Gallery view models, including empty description slots. |
| Design Guidance pages | Mostly done | Color, Typography, Spacing, Geometry, and Iconography have WPF Gallery-oriented page coverage and tests. |
| User Dashboard sample | Mostly done | WPF Gallery-style layout and behavior are represented and tested. |
| Basic Input pages | Mostly done | Button, CheckBox, ComboBox, RadioButton, and Slider are covered by WPF Gallery parity tests. |
| Collections pages | Mostly done | DataGrid, ListBox, ListView, and TreeView are covered by WPF Gallery parity tests. |
| Date & Calendar pages | Mostly done | Calendar and DatePicker WPF pages are covered while keeping distinct WinUI-style pages. |
| Layout pages | Mostly done | Expander, Grid, ResizeGrip, GridSplitter, GroupBox, StackPanel, Border, Canvas, and Image coverage exists through sample factories and tests. |
| Media section | Done | Official WPF Gallery media pages are exposed in the catalog/navigation. |
| Navigation pages | Mostly done | Menu, TabControl, Frame, NavigationWindow, and Hyperlink coverage exists through sample factories and tests. |
| Status & Info pages | Mostly done | ProgressBar and ToolTip coverage exists through sample factories and tests. |
| Text pages | Mostly done | Label, TextBox, TextBlock, RichTextEdit, and PasswordBox coverage exists through sample factories and tests. |
| System pages | Mostly done | File and folder dialogs, MessageBox, and Clipboard coverage exists through sample factories and tests. |
| Runtime regression layer | Done for current scope | Gallery runtime tests currently cover page loading and many WPF Gallery layout/sample details. |

## Needs Work

| Area | Status | Next action |
| --- | --- | --- |
| Visual screenshot pass against official WPF Gallery | Open | Add or run a WPF Gallery reference capture pass and compare page-by-page screenshots, not just runtime structure. |
| Page-by-page exact XAML audit | Open | For each WPF-equivalent page, compare ModernWpf sample factories against official WPF Gallery XAML and replace approximations with adapted copies where feasible. |
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
| Collections section | Partial | Open | Open | Section visual audit remains. |
| DataGrid | Partial | Open | Open | Runtime parity checks exist. |
| ListBox | Partial | Open | Open | Runtime parity checks exist. |
| ListView | Partial | Open | Open | Runtime parity checks exist. |
| TreeView | Partial | Open | Open | Runtime parity checks exist. |
| Date & Calendar section | Partial | Open | Open | Section visual audit remains. |
| Calendar | Partial | Open | Open | WPF page preserved separately from `CalendarView`. |
| DatePicker | Partial | Open | Open | Runtime parity checks exist. |
| Layout section | Partial | Open | Open | Section visual audit remains. |
| Expander | Partial | Open | Open | Runtime parity checks exist. |
| Grid | Partial | Open | Open | Runtime parity checks exist. |
| ResizeGrip | Partial | Open | Open | Runtime parity checks exist. |
| GridSplitter | Partial | Open | Open | Runtime parity checks exist. |
| GroupBox | Partial | Open | Open | Runtime parity checks exist. |
| StackPanel | Partial | Open | Open | Runtime parity checks exist. |
| Border | Partial | Open | Open | Runtime parity checks exist. |
| Media section | Partial | Open | Open | Section exists; visual audit remains. |
| Canvas | Partial | Open | Open | Header description aligned to official empty string. |
| Image | Partial | Open | Open | Header description aligned to official empty string. |
| Navigation section | Partial | Open | Open | Section visual audit remains. |
| Menu | Partial | Open | Open | Runtime parity checks exist. |
| TabControl | Partial | Open | Open | Runtime parity checks exist. |
| Frame | Partial | Open | Open | Runtime parity checks exist. |
| NavigationWindow | Partial | Open | Open | Runtime parity checks exist. |
| Text section | Partial | Open | Open | Section visual audit remains. |
| Label | Partial | Open | Open | Header description aligned to official empty string. |
| TextBox | Partial | Open | Open | Header description aligned to official empty string. |
| TextBlock | Partial | Open | Open | Header description aligned to official empty string. |
| RichTextEdit | Partial | Open | Open | WPF page preserved separately from `RichEditBox`. |
| PasswordBox | Partial | Open | Open | Header description aligned to official empty string. |
| Hyperlink | Partial | Open | Open | Runtime parity checks exist. |
| Status & Info section | Partial | Open | Open | Section visual audit remains. |
| ProgressBar | Partial | Open | Open | Runtime parity checks exist. |
| ToolTip | Partial | Open | Open | Runtime parity checks exist. |
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

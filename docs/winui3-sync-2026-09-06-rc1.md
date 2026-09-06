# WinUI 3 RC 1 milestone cutoff — 2026-09-06

This finite review advances the Preview 6 source boundary for RC 1. It is
source-disposition evidence, not a substitute for release acceptance. Later
commits open the next interval unless they reveal an applicable security,
data-loss, startup, crash, core-input, or equivalently critical defect.

## Pins and completeness

| Track | Previous cutoff | RC 1 cutoff | Changed paths |
| --- | --- | --- | --- |
| Stable product | `a97562621a1d1ea397a38a3f512c9eef99db52d8` (`winui3/release/2.3.1`) | `e8442d07ae57d2d3e653e616831f504937881bd3` (`winui3/release/2.4.0`) | 11 |
| Product main | `23a73be03d194ea0ece97da71de98b6b53021b70` | `26eb4a71b836378151a7961eb84ff60c8b5ae6e5` | 378 |
| Gallery main | `b78c440193aab788215888561e45adf72da848cb` | `b1730cb60a36014b1715df6b797e79a3f7c414ce` | 35 |

Repositories are `microsoft/microsoft-ui-xaml` and `microsoft/WinUI-Gallery`.
Stable and Gallery use complete GitHub comparisons (one and 13 commits).
Product main has 71 commits and exceeds the REST 300-file limit. Its full
378-path inventory comes from `git diff -M --name-status <base> <head>` of
the fetched exact Git objects, not the truncated REST response. Renames retain
both paths. The [424-row inventory](winui3-sync-2026-09-06-rc1-paths.tsv)
assigns every changed path exactly one disposition below.

## Product dispositions

| ID | Paths | Decision and reason |
| --- | --- | --- |
| S1 | 11 | Stable changes concern WinUI package/dependency metadata, the WinUI compiler, and generated-entry-point scenario/install tests. WPF owns its compiler and application entry point; no ModernWpf control code is derived from these files. |
| R1 | 3 | Port RadioButtons factory wrapper pooling, recycled-state clearing, and custom-factory delegation. Three regressions fail before the fix and pass afterward. See the [RadioButtons audit](radiobuttons-winui3-source-audit.md) for WPF parent ownership and selector clearing. No public surface change. |
| R2 | 2 | NavigationView native ABI event-revoker destruction ordering and disconnected-COM handling do not map to CLR lifetime. WPF uses explicit managed event unhooking and has no active native destructor/COM revoker. No port. |
| R3 | 2 | TeachingTip now guards null TemplateSettings in WinUI. ModernWpf initializes its read-only TemplateSettings dependency property in the constructor; callers cannot replace or clear it through the public API. Keep the existing WPF adaptation rather than introduce a mutable public property. |
| R4 | 2 | CommandBarFlyout's ContentExternalBackdropLink helper migration concerns WinUI compositor interfaces. WPF popup/backdrop composition does not consume those interfaces. No port. |
| R5 | 110 | New experimental TableView/SortIndicator/tabular resources, generated APIs, DLL/build scaffolding, design specs, and GridCoordinateHelper are outside the fixed 1.0 feature scope. Defer evaluation to a post-1.0 milestone; do not expose a partial family during RC freeze. |
| R6 | 14 | Ink input/presenter/toolbar, SystemBackdropElement, and WebView2 implementation changes affect WinUI controls not shipped by ModernWpf. WPF's own InkCanvas is not this WinUI port. No port. |
| R7 | 64 | WinUI platform implementation and its tests: native allocation/profiler/COM tracking, diagnostics resource graph pruning, parser escaping, Window core-content lifetime, popup backdrop links, tooltip root teardown, hyperlink UIA clipping, media/DComp, and experimental callback-authored DataTemplate API/codegen. WPF owns these platform classes and pipelines; do not patch them through ModernWpf or invent matching facades. |
| R8 | 2 | Repeater tests exercise the new WinUI callback DataTemplate constructor and null callback output. WPF DataTemplate has no such constructor; no Repeater product algorithm changes in this interval. Existing WPF templates and IElementFactory remain the supported paths. |
| R9 | 39 | WinUI XAML compiler, compiler tests and generated expected outputs/build entry point. WPF owns compilation. Excluded. |
| R10 | 103 | Native WinUI sample applications, ChartApp additions, InkGrid sample removal, and sample packaging/runtime identifiers. Not shipped ModernWpf Gallery or library inputs. Excluded. |
| R11 | 3 | API-review process, DateTimePicker spec spelling, and proposed FrameworkElement.SetThemeResourceBinding specification. No applicable implemented control delta; WPF owns FrameworkElement. Excluded. |
| R12 | 34 | Remaining build/package/telemetry/test infrastructure, C++/WinRT projection helpers, and developer documentation. No CLR equivalent of ABI revoker internals or WinRT registration is shipped. Excluded. |

Platform exclusions above concern implementation ownership, not a claim that
the corresponding WPF platform is defect-free. In particular, the native
ResourceGraph leak and Window core DP retention fixes cannot be applied to
ModernWpf's managed resource dictionaries or WPF Window implementation.

## Gallery dispositions

| ID | Paths | Decision and reason |
| --- | --- | --- |
| G1 | 2 | ColorSelector helper gains an alpha-enable option/default. ModernWpf's ColorPicker sample already initializes alpha disabled and supplies a toggle. Do not add an upstream Gallery-helper API to the package. |
| G2 | 4 | New ListView scroll-position restoration and ScrollIntoView examples are noncritical sample additions. Defer additional examples until after the RC freeze; WPF owns the stock ListView behavior. |
| G3 | 4 | Additional 12/24-hour TimePicker/ClockIdentifiers sample guidance is a noncritical documentation enhancement. Defer new sample variants until after 1.0; existing ClockIdentifier API is unchanged. |
| G4 | 1 | Border sample spacing correction targets the WinUI sample grid. Stock WPF Border/Grid styling authority is official WPF Fluent, not that Gallery layout. No matching source to synchronize. |
| G5 | 4 | ConnectedAnimation ItemsRepeater return-focus and EasingFunction bring-into-view changes concern pages/helpers not shipped in ModernWpf Gallery. No applicable implementation to port. |
| G6 | 13 | AppWindow title-bar, CaptureElement/media, StoragePickers and SystemBackdropElement examples require native WinRT APIs/pages not shipped by ModernWpf. They do not redefine WindowTitleBar. Excluded. |
| G7 | 7 | Gallery store pipeline, manifests, project/version properties, README and publishing documentation. Not consumed by ModernWpf packaging. Excluded. |

All 424 paths are classified. Acceptance still requires the complete RC test,
package, downstream, and visual/manual gates documented in
[release readiness](release-readiness-1x.md). The intended public surface is
preserved by promoting the accepted inventories without adding/removing
combined entries; PipsPager remains deferred to 1.1.

# ModernWpf 1.x Public API Contract

This document defines preview-era API governance and the stable 1.x
compatibility boundary. `1.0.0-preview.1` records the first public audit and
migration baseline; it does not freeze later 1.0 previews to that exact shape.

## Compatibility policy

- `1.0.0-preview.1` is the first audit, migration, and package-comparison
  baseline.
- Source and binary compatibility with 0.9.x is intentionally not promised.
- During the 1.0 preview series, current applicable WinUI API shape is
  authoritative for WinUI-derived controls. Source-audited additions, changes,
  and removals may deliberately break an earlier preview when the same change
  updates the checked-in inventories, documents WPF adaptations, supplies
  migration guidance, and adds focused tests.
- Checked-in API/resource inventories and package validation are drift
  detectors. They reject accidental changes; they are deliberately rebaselined
  when an accepted preview-era parity change alters the public contract.
- Stable `1.0.0` establishes the SemVer compatibility baseline. Within the
  stable 1.x line, additions remain possible, but an upstream breaking change
  must use a compatible ModernWpf adaptation or wait for the next ModernWpf
  major version.

The contract applies to all supported package targets:

- `net462`
- `net8.0-windows7.0`
- `net10.0-windows7.0`

## Comparison

| Surface | Role in ModernWpf 1.x | Compatibility decision |
| --- | --- | --- |
| Current ModernWpf | The API that ships in `ModernWpf.dll` and `ModernWpf.Controls.dll`. | Audited against `1.0.0-preview.1` and later accepted package baselines. Preview changes may be deliberately rebaselined; stable 1.x changes follow SemVer. |
| `0.9.7-preview.2` | Last public prerelease and historical migration input. | Not a compatibility baseline. It has 263 ModernWpf top-level public types; the v1 candidate has 347. The v1 set adds 117 and removes 33 relative to this release. |
| `0.9.6` | Last stable public release and historical migration input. | Not a compatibility baseline. It has 261 ModernWpf top-level public types; v1 adds 119 and removes 33 relative to it. |
| Current WinUI | Primary naming, control-shape, event, sealing, and versionability authority for WinUI-derived ModernWpf controls. | Follow current applicable WinUI, including API changes during previews, unless WPF requires a documented adaptation. The adopted product epoch is [`microsoft-ui-xaml` commit `d5bdbb19`](https://github.com/microsoft/microsoft-ui-xaml/commit/d5bdbb190cdba0b7f1baec4b3981208a9685a360), reconciled with stable `winui3/release/2.3.1` and Gallery in `docs/winui3-sync-2026-08-06.md`; moving selectors and family mappings live in `tools/upstream/upstream-sync.json`. |
| Official WPF Fluent | Primary styling and behavior authority for stock WPF controls, and the platform Fluent implementation used on .NET 10. | Complements WinUI; it does not rename ModernWpf custom-control CLR APIs. Audited source is [`dotnet/wpf` commit `7f005faa`](https://github.com/dotnet/wpf/commit/7f005faa89e79b0b1fa1cb2c21283bab7916c092). |

The current checked-in CLR baseline contains 1,558 API entries for
`ModernWpf.dll` and 2,718 for `ModernWpf.Controls.dll`. The packaged .NET 8
assemblies expose 122 and 225 supported top-level types respectively. WPF's
generated `GeneratedInternalTypeHelper` is compiler infrastructure and is not
a supported ModernWpf API.

## Preview 1 surface record

Preview 1 recorded these versionability decisions. They remain the migration
starting point and are re-evaluated when current WinUI changes:

- WinRT projection implementation types are internal. `ABI.*`, embedded WinRT
  contract markers, and other generated projection namespaces must never
  become package API.
- `IconSource` is concrete and uses a protected virtual creation hook, matching
  current WinUI's extensible shape.
- `FlyoutBase.CreatePresenter` is virtual rather than abstract, matching the
  current WinUI versioning shape.
- Repeater factories use the public `IElementFactory`; the duplicate
  `IElementFactoryShim` surface is removed.
- `CommandBar.DynamicOverflowItemsChanging` uses
  `TypedEventHandler<CommandBar, DynamicOverflowItemsChangingEventArgs>`.
- Stale repeater animator APIs are implementation-only because the animator
  property is not a public current-WinUI surface.
- Public automation peers live consistently under
  `ModernWpf.Automation.Peers`.
- Caption-button pointer/pressed bookkeeping remains internal to the WPF
  window-shell template rather than package API.
- `StackLayout` exposes current WinUI's positive
  `IsVirtualizationEnabled` property without the obsolete inverse alias.
- Template-only converters, panels, proxies, visual helpers, and private
  automation peers are internal.
- WinUI runtimeclasses are sealed or unsealed to match current source unless a
  WPF extensibility requirement is recorded below.

Notable 0.9 migration changes include:

- `SimpleStackPanel` is replaced by the current `StackPanelEx` surface.
- `IElementFactoryShim` consumers should implement `IElementFactory`.
- The WPF window-shell facade and control are named `WindowTitleBar` and
  `WindowTitleBarControl`, leaving `TitleBar` available for a future port of
  the current WinUI control.
- Types under `ModernWpf.Controls.Primitives` that existed only to service
  templates are no longer public.

## Intentional WPF adaptations

These public names or shapes are not accidental WinUI drift:

| ModernWpf API | Reason |
| --- | --- |
| `StackPanelEx` | Avoids colliding with WPF's stock `StackPanel` while adding WinUI spacing and scroll-snap behavior. |
| `WindowTitleBar` and `WindowTitleBarControl` | WPF window-chrome adapter distinct from current WinUI's content-oriented `TitleBar` control. |
| `ContextFlyoutService` and `FlyoutService` | WPF attached-property adapters for WinUI flyout ownership. |
| `INumberBoxNumberFormatter` | WPF-friendly formatting contract in place of WinRT number-formatting interfaces. |
| `ListViewBaseItem` and its automation peer | WPF realization of platform/XamlOM list-item primitives used by the WinUI control family. |
| `RadioMenuItem` | WPF naming adaptation of WinUI's radio menu-flyout item. |
| `TeachingTipClosingDeferral` | WPF replacement for `Windows.Foundation.Deferral`. |
| `BindingProxy` | WPF resource/binding bridge used by consumer XAML. |
| `WrapGrid` | Remains inheritable because `ItemsWrapGrid` derives from the WPF implementation. |
| `ToggleSwitch` | Remains inheritable to preserve its WPF protected customization hooks. |
| `NavigationViewItemAutomationPeer` set-position overrides | Exist only on WPF target frameworks whose base automation peer exposes those virtual methods; package validation tracks the per-target shape. |

## Interface versioning

The Preview 1 public interfaces are:

- `ICommandBarElement`
- `IElementFactory`
- `IKeyIndexMapping`
- `INumberBoxNumberFormatter`
- `IScrollAnchorProvider`
- `IScrollController`
- `IScrollControllerPanningInfo`
- `IScrollSnapPointsInfo`

For a ModernWpf-originated capability, add a new interface or an extensible
base-class member instead of changing one of these interfaces. During previews,
however, a current WinUI change to the corresponding interface is authoritative:
mirror the feasible source shape, update the inventories, and document the
consumer migration. After stable 1.0, preserve the shipped interface throughout
1.x or defer the upstream break to the next major version.

## Resource-key contract

Only entries in these files are public XAML resource contracts:

- `ModernWpf/PublicResourceKeys.Shipped.txt`
- `ModernWpf/PublicResourceKeys.Unshipped.txt`

Each entry identifies a literal, top-level `x:Key` and the dictionary that
owned it in the accepted public snapshot. The Preview 1 audit baseline contains
5,320 source-qualified entries from:

- `ThemeResources/Light.xaml`
- `ThemeResources/Dark.xaml`
- `ThemeResources/HighContrast.xaml`
- `ModernWpfControlsResources.xaml`
- `DensityStyles/Compact.xaml`

During previews, an upstream-driven change may deliberately add, move, rename,
or remove an inventoried key under the same audit, migration, and rebaseline
rules as a CLR API change. After stable 1.0, these keys are part of the SemVer
contract for the stable 1.x line.

The following are not public contract entries unless later added to an explicit
manifest:

- Template parts
- Visual states
- Implicit or `{x:Type ...}` keys
- Template-local keys
- Unlisted control-template and implementation resources

Run `tools/api-contracts/Update-PublicResourceKeyContract.ps1` to validate the
existing manifest and add newly chosen public keys to the unshipped file.

## Enforcement and release workflow

1. Make the API change.
2. Classify it as additive, a deliberate current-WinUI parity change, a
   documented WPF adaptation, or an accidental change. A deliberate preview
   break must cite the upstream source audit and add a `## Breaking changes`
   migration entry to the current release notes.
3. For a CLR addition or replacement, run `dotnet format` with diagnostic
   `RS0016`, include generated source, and review the new
   `PublicAPI.Unshipped.txt` entries. Remove obsolete shipped entries only for
   an accepted preview break.
4. For a supported resource-key addition, run
   `tools/api-contracts/Update-PublicResourceKeyContract.ps1` and review the
   unshipped resource entries. Update or remove an existing entry only under
   the deliberate-break rule.
5. Build every target framework and run the theme/resource tests.
6. Before publishing an accepted package baseline, promote every accepted
   entry to the corresponding shipped inventory. A build whose version equals
   the active package baseline requires all unshipped inventories to be empty.
7. Pack the NuGet package. Strict cross-target validation always applies. When
   the current version differs from
   `ModernWpfPackageValidationBaselineVersion`, NuGet also compares with that
   published package. Baseline validation uses normal compatibility mode: it
   rejects binary breaks but permits additive APIs because additions are
   separately review-gated by the checked-in public API inventories. To accept
   a deliberate break during previews, advance that property to the current
   development version in the same change as the source audit, inventory
   updates, tests, and release-note migration entry.
   `ModernWpfPreviewAuditBaselineVersion` remains fixed at
   `1.0.0-preview.1` as the machine-readable identifier of the published
   historical package. It is available for explicit migration audits, but it
   is intentionally not the active NuGet compatibility gate.
8. Run `tools/release/Verify-ModernWpfPackage.ps1`. It rejects namespace leaks,
   cross-target top-level type drift, stale XML documentation, and malformed
   package assets.

The analyzer treats differences from the checked-in accepted API inventories
as build errors; an intentional preview change updates those inventories rather
than suppressing the analyzer. Once stable `1.0.0` is published, do not advance
the active package baseline within 1.x to hide an incompatibility. Nullable
annotations are not yet a v1 contract because the existing codebase is
nullable-oblivious; they can be introduced as a separately reviewed contract
improvement.

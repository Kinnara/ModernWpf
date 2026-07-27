# ModernWpf 1.x Public API Contract

This document defines the compatibility boundary beginning with
`1.0.0-preview.1`.

## Compatibility policy

- `1.0.0-preview.1` is the first forward-stable baseline.
- Source and binary compatibility with 0.9.x is intentionally not promised.
- After preview 1, shipped public CLR APIs and explicitly listed public
  resource keys must remain compatible. A deliberate break requires a new
  major-version decision, not an incidental source or template change.
- Additive APIs remain possible. Add members to base classes or new interfaces
  instead of extending an already shipped interface.

The contract applies to all supported package targets:

- `net462`
- `net8.0-windows7.0`
- `net10.0-windows7.0`

## Comparison

| Surface | Role in ModernWpf 1.x | Compatibility decision |
| --- | --- | --- |
| Current ModernWpf | The API that ships in `ModernWpf.dll` and `ModernWpf.Controls.dll`. | Frozen forward from `1.0.0-preview.1` by checked-in API inventories and package validation. |
| `0.9.7-preview.2` | Last public prerelease and historical migration input. | Not a compatibility baseline. It has 263 ModernWpf top-level public types; the v1 candidate has 347. The v1 set adds 117 and removes 33 relative to this release. |
| `0.9.6` | Last stable public release and historical migration input. | Not a compatibility baseline. It has 261 ModernWpf top-level public types; v1 adds 119 and removes 33 relative to it. |
| Current WinUI | Primary naming, control-shape, event, sealing, and versionability authority for WinUI-derived ModernWpf controls. | Follow current WinUI unless WPF requires a documented adaptation. Audited product source is [`microsoft-ui-xaml` commit `de3e7673`](https://github.com/microsoft/microsoft-ui-xaml/commit/de3e767333c2f0717a6a70cb22bd192ced5ad885). |
| Official WPF Fluent | Primary styling and behavior authority for stock WPF controls, and the platform Fluent implementation used on .NET 10. | Complements WinUI; it does not rename ModernWpf custom-control CLR APIs. Audited source is [`dotnet/wpf` commit `7f005faa`](https://github.com/dotnet/wpf/commit/7f005faa89e79b0b1fa1cb2c21283bab7916c092). |

The current checked-in CLR baseline contains 1,558 API entries for
`ModernWpf.dll` and 2,718 for `ModernWpf.Controls.dll`. The packaged .NET 8
assemblies expose 122 and 225 supported top-level types respectively. WPF's
generated `GeneratedInternalTypeHelper` is compiler infrastructure and is not
a supported ModernWpf API.

## v1 surface decisions

The prerelease cleanup made these versionability decisions before freezing the
baseline:

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

## Interface stability

The shipped public interfaces are:

- `ICommandBarElement`
- `IElementFactory`
- `IKeyIndexMapping`
- `INumberBoxNumberFormatter`
- `IScrollAnchorProvider`
- `IScrollController`
- `IScrollControllerPanningInfo`
- `IScrollSnapPointsInfo`

Do not add members to these interfaces after preview 1. Add a new capability
interface or an extensible base class instead.

## Resource-key contract

Only entries in these files are public XAML resource contracts:

- `ModernWpf/PublicResourceKeys.Shipped.txt`
- `ModernWpf/PublicResourceKeys.Unshipped.txt`

Each entry identifies a literal, top-level `x:Key` and the dictionary in which
it must continue to exist. The preview-1 baseline contains 5,320
source-qualified entries from:

- `ThemeResources/Light.xaml`
- `ThemeResources/Dark.xaml`
- `ThemeResources/HighContrast.xaml`
- `ModernWpfControlsResources.xaml`
- `DensityStyles/Compact.xaml`

The following are not forward contracts unless later added to an explicit
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
2. For a CLR addition, run `dotnet format` with diagnostic `RS0016`, include
   generated source, and review the new `PublicAPI.Unshipped.txt` entries.
3. For a supported resource-key addition, run
   `tools/api-contracts/Update-PublicResourceKeyContract.ps1` and review the
   unshipped resource entries.
4. Build every target framework and run the theme/resource tests.
5. Before publishing a new compatibility baseline, promote every accepted
   entry to the corresponding shipped inventory. Baseline builds require all
   unshipped inventories to be empty.
6. Pack the NuGet package. Package validation compares compatible target
   frameworks and, after preview 1, compares with the published
   `1.0.0-preview.1` baseline.
7. Run `tools/release/Verify-ModernWpfPackage.ps1`. It rejects namespace leaks,
   cross-target top-level type drift, stale XML documentation, and malformed
   package assets.

The analyzer treats API removals, signature changes, and accidental public
members as build errors. Nullable annotations are not yet a v1 contract
because the existing codebase is nullable-oblivious; they can be introduced as
a separately reviewed contract improvement.

# Migrating from ModernWpf 0.9.x

ModernWpf 1.x is a new product line, not a binary-compatible update to 0.9.x.
The 1.0 previews may deliberately change to follow current applicable WinUI API
shape; stable `1.0.0` establishes the SemVer compatibility boundary. Migrate in
a branch and compile every application target before replacing a production
package reference.

## 1. Retarget the application

`ModernWpfUI` 1.x supports:

- `net462`
- `net8.0-windows7.0`
- `net10.0-windows7.0`

Applications on `net45`, `netcoreapp3.0`, or `net5.0-windows` must retarget.
The package ID remains `ModernWpfUI`.

## 2. Choose the resource entry

The recommended 1.x application resources are:

```xaml
<ResourceDictionary>
  <ResourceDictionary.MergedDictionaries>
    <ui:ThemeResources />
    <ui:FluentControlsResources UseCompactResources="False" />
  </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>
```

This selects official WPF Fluent stock-control resources on .NET 10 and the
ModernWpf backport on older supported targets. `ThemeManager` continues to
control ModernWpf resources and bridges application/window theme preferences
to WPF `ThemeMode` on .NET 10.

For a staged migration, 1.x retains the legacy control-resource entry:

```xaml
<ui:ThemeResources />
<ui:XamlControlsResources />
```

Do not merge both control-resource entries into the same application scope.

## 3. Update renamed or retired APIs

| 0.9.x use | 1.x migration |
| --- | --- |
| `SimpleStackPanel` | Use `StackPanelEx`; its spacing and layout surface is the supported WinUI-compatible contract. |
| `IElementFactoryShim` | Implement or consume `IElementFactory`. |
| Static `TitleBar` facade | Use `WindowTitleBar`. |
| `TitleBarControl` | Use `WindowTitleBarControl`. |
| `TitleBar.ExtendViewIntoTitleBar` or the shell-control equivalent | Use `WindowTitleBar.ExtendsContentIntoTitleBar` or `WindowTitleBarControl.ExtendsContentIntoTitleBar`. The UWP-shaped `CoreApplicationViewTitleBar.ExtendViewIntoTitleBar` member is unchanged. |
| `StackLayout.DisableVirtualization` | Set `StackLayout.IsVirtualizationEnabled` to `false`. |
| `ModernWpf.Navigation.Extensions.Parameter(...)` | Use `NavigationEventArgsExtensions.GetParameter(...)`. |
| `ModernWpf.Navigation.Extensions.SourcePageType(...)` | Use `NavigationEventArgsExtensions.GetSourcePageType(...)`. |
| Automation peers under control or primitive namespaces | Import the peer from `ModernWpf.Automation.Peers`. |
| `ThemeResouceExtensionConverter` | Use the corrected `ThemeResourceExtensionConverter` name. |
| Template-only public primitive types | Remove direct references and customize through supported control APIs, templates, and documented resource keys. |
| Public `ABI.*` or WinRT projection types | Remove references; these are implementation details. |

The checked-in shipped API and resource inventories record the Preview 1 audit
and migration baseline. During the 1.0 preview series they remain drift gates,
but an accepted WinUI parity change may deliberately rebaseline them with
focused tests and migration guidance. They do not assert compatibility with
0.9.x.

### Choose `TabControl` or `TabView` intentionally

Preview 5 adds `ModernWpf.Controls.TabView`; it does not rename or replace
`System.Windows.Controls.TabControl`. Keep `TabControl` when the application
needs ordinary WPF tab selection and official WPF Fluent styling. Adopt the
new `TabView` when it needs WinUI-shaped add/close commands, compact or
content-sized headers, overflow scrolling, drag/reorder, or the documented
WPF window tear-out flow.

`TabView` raises close and tear-out requests but does not remove or move the
application's data on its own. Update the bound collection in the relevant
event handler. For tear-out, the application creates the destination WPF
`Window` and moves the item after accepting the request; ModernWPF does not
guess the application's window type or view-model lifetime.

### Adopt the Preview 6 item foundations deliberately

Preview 6 adds the WinUI-shaped `ModernWpf.Controls.ItemContainer` and
`LinedFlowLayout` without replacing WPF `ListBoxItem`, `ListViewItem`, or the
stock WPF panels. Use `ItemContainer` when an application or custom virtualized
control needs the source selection chrome and automation contract. Use
`LinedFlowLayout` with `ItemsRepeater` when variable-width items should share
an equal line height and only the viewport/cache range should be realized.

`ItemsRepeater.ItemTransitionProvider` and the public item-collection
transition family are additive Preview 6 APIs. The built-in LinedFlow provider
uses WPF render transforms and respects the system client-area animation
setting. The scrolling bridge required by Preview 7 is internal; Preview 6
does not publish incomplete `ScrollView`, `ScrollPresenter`, or `ItemsView`
shells. Existing Preview 5 applications require only a package-version update.

### Adopt Preview 7 ItemsView intentionally

Preview 7 adds `ModernWpf.Controls.ItemsView`; it does not rename or replace
WPF `ItemsControl`, `ListBox`, `ListView`, or `GridView`. Keep the stock WPF
controls when their established item-container, grouping, editing, or
collection-view behavior is the application requirement. Adopt ItemsView when
the application needs the WinUI-shaped combination of `ItemsRepeater`
virtualization, `ItemContainer` interaction and automation, swappable layouts,
current-item tracking, invocation, and source selection policies.

ItemsView templates must realize `ItemContainer` roots. WPF `DataTemplate` and
`DataTemplateSelector` inputs and the existing `IElementFactory` abstraction
are accepted through the `object`-typed `ItemTemplate` property. Move custom
item visuals inside `ItemContainer` instead of returning an unrelated root.

`ItemsView.ScrollView` returns `System.Windows.Controls.ScrollViewer`, and
`ModernWpf.BringIntoViewOptions` carries target rectangle, alignment, offset,
and animation intent into the WPF scrolling bridge. Do not cast that property
to a WinUI `ScrollView` or depend on private `ScrollPresenter` behavior.
`ItemTransitionProvider` can use the Preview 6 LinedFlow provider for add,
remove, move, and layout transitions.

The new ItemsView and BringIntoViewOptions APIs are additive. Preview 7 makes
no intentional Preview 6 API or resource-key break, so existing Preview 6
applications require only a package-version update.

## 4. Migrate MahApps integration

`ModernWpfUI.MahApps` remains on the legacy 0.9.x line and is not produced for
1.x. Remove that package before upgrading the core package. Recreate only the
application-specific MahApps resource integration that is still required, or
remain on the frozen, unsupported 0.9.x packages at your own risk until that
dependency can be removed. The 0.9.x line receives no updates, including
security fixes.

## 5. Validate behavior

Exercise startup, Light/Dark/High Contrast switching, window chrome,
navigation, menus, `ContentDialog`, `CommandBarFlyout`, keyboard focus, and
pointer dismissal on each application target. Pay particular attention to
custom templates that referenced previously public primitive types or assumed
0.9.x template parts.

For library authors, compare against the
[1.x public API contract](public-api-contract-1x.md) and do not depend on
unlisted template-local resource keys.

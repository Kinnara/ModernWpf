# Migrating from ModernWpf 0.9.x

ModernWpf 1.x is a new forward-compatible line, not a binary-compatible update
to 0.9.x. Migrate in a branch and compile every application target before
replacing a production package reference.

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

For a staged migration, the 0.9 resource entry remains supported:

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
| Template-only public primitive types | Remove direct references and customize through supported control APIs, templates, and documented resource keys. |
| Public `ABI.*` or WinRT projection types | Remove references; these are implementation details. |

The checked-in shipped API and resource inventories define the preview-1
forward baseline. They do not assert compatibility with 0.9.x.

## 4. Migrate MahApps integration

`ModernWpfUI.MahApps` remains on the legacy 0.9.x line and is not produced for
1.x. Remove that package before upgrading the core package. Recreate only the
application-specific MahApps resource integration that is still required, or
remain on 0.9.x until that dependency can be removed.

## 5. Validate behavior

Exercise startup, Light/Dark/High Contrast switching, window chrome,
navigation, menus, `ContentDialog`, `CommandBarFlyout`, keyboard focus, and
pointer dismissal on each application target. Pay particular attention to
custom templates that referenced previously public primitive types or assumed
0.9.x template parts.

For library authors, compare against the
[1.x public API contract](public-api-contract-1x.md) and do not depend on
unlisted template-local resource keys.

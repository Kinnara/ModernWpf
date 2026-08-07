# ModernWPF

ModernWPF brings Fluent styles and WinUI-inspired controls to Windows
Presentation Foundation applications. **ModernWPF** is the product name;
`ModernWpfUI` remains the NuGet package ID and `ModernWpf` remains the CLR
namespace and repository name.

![ModernWPF Gallery showing controls, samples, and navigation](https://raw.githubusercontent.com/Kinnara/ModernWpf/v1.0.0-preview.2/docs/images/Gallery.Light.png)

## Install Preview 2

Install the preview explicitly:

```powershell
dotnet add package ModernWpfUI --version 1.0.0-preview.2
```

| Target framework | Stock-control theme |
| --- | --- |
| `net462` | ModernWPF Fluent backport |
| `net8.0-windows7.0` | ModernWPF Fluent backport |
| `net10.0-windows7.0` | Official WPF Fluent theme |

Add the recommended resources to `App.xaml`:

```xaml
<Application
    ...
    xmlns:ui="http://schemas.modernwpf.com/2019">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ui:ThemeResources />
                <ui:FluentControlsResources UseCompactResources="False" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

To apply ModernWPF window styling, set
`ui:WindowHelper.UseModernWindowStyle="True"` on a WPF `Window`.

`FluentControlsResources` uses the official WPF Fluent theme for stock controls
on .NET 10 and the ModernWPF backport on older supported targets. During a
staged 0.9.x migration, an existing application can temporarily keep the
legacy resource entry:

```xaml
<ui:ThemeResources />
<ui:XamlControlsResources />
```

New applications should use `FluentControlsResources`. The 0.9.x line is
frozen and unsupported; no maintenance or security updates are planned.

## Preview expectations and feedback

The 1.0 preview series may make source-audited API or resource-key corrections
before stable `1.0.0`. Intentional changes are documented with migration
guidance; stable 1.0 will establish the SemVer compatibility boundary for 1.x.

- [Preview 2 release notes](https://github.com/Kinnara/ModernWpf/blob/v1.0.0-preview.2/docs/release-notes-1.0.0-preview.2.md)
- [Migrate from ModernWPF 0.9.x](https://github.com/Kinnara/ModernWpf/blob/v1.0.0-preview.2/docs/migrating-from-0.9.md)
- [Report a Preview bug](https://github.com/Kinnara/ModernWpf/issues/new?template=preview-bug.yml)
- [Documentation and source](https://github.com/Kinnara/ModernWpf#documentation)

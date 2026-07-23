# ModernWPF UI

Thanks for installing the ModernWPF UI NuGet package.

Add the theme resources to your application resources in `App.xaml`:

```xaml
<Application
    ...
    xmlns:ui="http://schemas.modernwpf.com/2019">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ui:ThemeResources />
                <ui:FluentControlsResources UseCompactResources="False" />
                <!-- Other merged dictionaries here -->
            </ResourceDictionary.MergedDictionaries>
            <!-- Other app resources here -->
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

To enable themed style for a window, set `WindowHelper.UseModernWindowStyle` to `true`:

```xaml
<Window
    ...
    xmlns:ui="http://schemas.modernwpf.com/2019"
    ui:WindowHelper.UseModernWindowStyle="True">
    <!-- Window content here -->
</Window>
```

`FluentControlsResources` uses the official WPF Fluent theme for stock controls
on .NET 10 and the ModernWpf backport on older supported targets. The legacy
`XamlControlsResources` entry remains available for applications that need its
0.9-style resource composition.

See https://github.com/Kinnara/ModernWpf for more information.

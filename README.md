# ModernWPF UI Library
[![Gitter](https://badges.gitter.im/ModernWpf/community.svg)](https://gitter.im/ModernWpf/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Modern styles and controls for your WPF applications.

## Features
* Modern styles and new features for [the majority of the stock WPF controls](https://github.com/Kinnara/ModernWpf/wiki/Controls#styled-controls).

* Light and dark themes that can be easily customized. A high contrast theme is also included.

* [Additional controls](https://github.com/Kinnara/ModernWpf/wiki/Controls#additional-controls) to help you build modern applications. Some are ported from the [Windows UI Library](https://github.com/microsoft/microsoft-ui-xaml).

* Targets .NET Framework 4.5+, .NET Core 3+, and .NET 5. Runs on Windows Vista SP2 and above.

![Overview of controls (light theme)](docs/images/Controls.Light.png "Overview of controls (light theme)")

## Quick start
1. Create a new WPF app.

2. Install from NuGet `Install-Package ModernWpfUI`.

3. Edit App.xaml to following:
```xaml
<Application
    ...
    xmlns:ui="http://schemas.modernwpf.com/2019">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ui:ThemeResources />
                <ui:XamlControlsResources />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

4. Edit MainWindow.xaml to following:
```xaml
<Window
    ...
    xmlns:ui="http://schemas.modernwpf.com/2019"
    ui:WindowHelper.UseModernWindowStyle="True">
    <ui:SimpleStackPanel Margin="12" Spacing="24">
        <TextBlock Text="My first ModernWPF app" Style="{StaticResource HeaderTextBlockStyle}" />
        <Button Content="I am a button" />
        <Button Content="I am an accent button" Style="{StaticResource AccentButtonStyle}" />
    </ui:SimpleStackPanel>
</Window>
```

5. See [the wiki](https://github.com/Kinnara/ModernWpf/wiki) for more information.

## Packages
| NuGet Package | Latest Versions |
| --- | --- |
| [ModernWpfUI][NuGet] | [![latest stable version](https://img.shields.io/nuget/v/ModernWpfUI)][NuGet]<br />[![latest prerelease version](https://img.shields.io/nuget/vpre/ModernWpfUI)][NuGet.Pre] |
| [ModernWpfUI.MahApps][NuGet.MahApps] | [![latest stable version](https://img.shields.io/nuget/v/ModernWpfUI.MahApps)][NuGet.MahApps]<br />[![latest prerelease version](https://img.shields.io/nuget/vpre/ModernWpfUI.MahApps)][NuGet.MahApps.Pre] |

## Screenshots
![Overview of controls (dark theme)](docs/images/Controls.Dark.png "Overview of controls (dark theme)")

![Control palette](docs/images/ControlPalette1.png "Control palette")

![Easily customize colors](docs/images/Nighttime.png "Easily customize colors")

![NumberBox](docs/images/NumberBox.png "NumberBox")

![ContentDialog](docs/images/ContentDialog.png "ContentDialog")

![DataGrid](docs/images/DataGrid.png "DataGrid")

![ItemsRepeater](docs/images/ItemsRepeater.png "ItemsRepeater")

![Custom title bar](docs/images/CustomTitleBar.Dark.png "Custom title bar")

![Calendar](docs/images/Calendar.png "Calendar")

![Menu](docs/images/Menu.png "Menu")

![Progress controls](docs/images/Progress.png "Progress controls")

![Color ramp](docs/images/ColorRamp.png "Color ramp")

![Accent color palette](docs/images/AccentColorPalette.png "Accent color palette")

![High contrast mode](docs/images/HighContrast.png "High contrast mode")

[NuGet]: https://www.nuget.org/packages/ModernWpfUI/
[NuGet.Pre]: https://www.nuget.org/packages/ModernWpfUI/absoluteLatest
[NuGet.MahApps]: https://www.nuget.org/packages/ModernWpfUI.MahApps/
[NuGet.MahApps.Pre]: https://www.nuget.org/packages/ModernWpfUI.MahApps/absoluteLatest

# ModernWPF UI Library
Modern styles and controls for your WPF applications.

![Screenshot 1](docs/images/screenshot1.png "Screenshot 1")

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
    <Grid>
        <StackPanel Margin="12">
            <TextBlock
                Text="My first ModernWPF app"
                Style="{StaticResource HeaderTextBlockStyle}" />
            <Button
                Content="I am a button"
                Margin="0,24,0,0" />
        </StackPanel>
    </Grid>
</Window>
```

5. See [Common Tasks](https://github.com/Kinnara/ModernWpf/wiki/Common-Tasks) for more usage information.
## Screenshots
![Screenshot 2](docs/images/screenshot2.png "Screenshot 2")

![Calendar](docs/images/Calendar.png "Calendar")

![ContentDialog](docs/images/ContentDialog.png "ContentDialog")

![Progress controls](docs/images/Progress.png "Progress controls")

![Nighttime colors](docs/images/Nighttime.png "Easily customize colors")

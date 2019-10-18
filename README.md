# Modern WPF UI Library
Modern styles and controls for your WPF applications.

![alt text](docs/images/screenshot1.png "Screenshot 1")

## Quick start
1. Create a new WPF app.

2. Install from NuGet `Install-Package ModernWpfUI`.

3. Edit App.xaml to following:
```
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
```
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

5. Run the app.

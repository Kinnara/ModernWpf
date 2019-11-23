Thanks for installing the ModernWPF UI NuGet package!

Don't forget to add the theme resources to your Application resources in App.xaml:

    <Application
        ...
        xmlns:ui="http://schemas.modernwpf.com/2019">
        <Application.Resources>
            <ResourceDictionary>
                <ResourceDictionary.MergedDictionaries>
                    <ui:ThemeResources />
                    <ui:XamlControlsResources />
                    <!-- Other merged dictionaries here -->
                </ResourceDictionary.MergedDictionaries>
                <!-- Other app resources here -->
            </ResourceDictionary>
        </Application.Resources>
    </Application>

To enable themed style for a window, set WindowHelper.UseModernWindowStyle to true:

    <Window
        ...
        xmlns:ui="http://schemas.modernwpf.com/2019"
        ui:WindowHelper.UseModernWindowStyle="True">
        <!-- Window content here -->
    </Window>

See https://github.com/Kinnara/ModernWpf for more information.

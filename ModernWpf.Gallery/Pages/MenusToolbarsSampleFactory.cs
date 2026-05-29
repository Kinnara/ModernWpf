using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class MenusToolbarsSampleFactory
    {
        private const string CommandBarExampleXaml =
@"<CommandBar Background=""Transparent"" IsOpen=""$(IsOpen)"" DefaultLabelPosition=""Right""$(IsSticky)>
    <AppBarButton Icon=""Add"" Label=""Add""/>
    <AppBarButton Icon=""Edit"" Label=""Edit""/>
    <AppBarButton Icon=""Share"" Label=""Share""/>
    <CommandBar.SecondaryCommands>
        <AppBarButton Icon=""Setting"" Label=""Settings"">
            <AppBarButton.KeyboardAccelerators>
                    <KeyboardAccelerator Modifiers=""Control"" Key=""I"" />
            </AppBarButton.KeyboardAccelerators>
        </AppBarButton>$(MultipleButtonsSecondaryCommands)
    </CommandBar.SecondaryCommands>
</CommandBar>";

        private const string AppBarButtonSymbolXaml =
@"<AppBarButton Icon=""Like"" Label=""SymbolIcon"" Click=""AppBarButton_Click""/>";

        private const string AppBarButtonBitmapXaml =
@"<AppBarButton Label=""BitmapIcon"" Click=""AppBarButton_Click"">
    <AppBarButton.Icon>
        <BitmapIcon UriSource=""ms-appx:///Assets/SampleMedia/Slices2.png""/>
    </AppBarButton.Icon>
</AppBarButton>";

        private const string AppBarButtonFontXaml =
@"<AppBarButton Label=""FontIcon"" Click=""AppBarButton_Click"">
    <AppBarButton.Icon>
        <FontIcon FontFamily=""Candara"" Glyph=""&#x03A3;""/>
    </AppBarButton.Icon>
</AppBarButton>";

        private const string AppBarButtonPathXaml =
@"<AppBarButton Label=""PathIcon"" Click=""AppBarButton_Click"">
    <AppBarButton.Content>
        <Viewbox Stretch=""Uniform"">
            <PathIcon Data=""F1 M 20,20L 24,10L 24,24L 5,24""/>
        </Viewbox>
    </AppBarButton.Content>
</AppBarButton>";

        private const string AppBarButtonKeyboardAcceleratorXaml =
@"<AppBarButton Icon=""Save"" Label=""Save"" Click=""AppBarButton_Click"">
    <AppBarButton.KeyboardAccelerators>
        <KeyboardAccelerator Modifiers=""Control"" Key=""S""/>
    <AppBarButton.KeyboardAccelerators/>
</AppBarButton>";

        private const string AppBarButtonFlyoutXaml =
@"<AppBarButton AllowFocusOnInteraction=""True"" Icon=""Edit"" Label=""Edit"">
    <AppBarButton.Flyout>
        <Flyout/>
            <TextBox MinWidth=""240"" PlaceholderText=""Input text here""/>
        <Flyout/>
    <AppBarButton.Flyout>
</AppBarButton>";

        private const string AppBarToggleButtonSymbolXaml =
@"<AppBarToggleButton Icon=""Shuffle"" Label=""SymbolIcon"" Click=""AppBarButton_Click""/>";

        private const string AppBarToggleButtonBitmapXaml =
@"<AppBarToggleButton Label=""BitmapIcon"" Click=""AppBarButton_Click"">
    <AppBarToggleButton.Icon>
        <BitmapIcon UriSource=""ms-appx:///Assets/SampleMedia/Slices2.png""/>
    </AppBarToggleButton.Icon>
</AppBarToggleButton>";

        private const string AppBarToggleButtonFontXaml =
@"<AppBarToggleButton Label=""FontIcon"" Click=""AppBarButton_Click"">
    <AppBarToggleButton.Icon>
        <FontIcon FontFamily=""Candara"" Glyph=""&#x03A3;""/>
    </AppBarToggleButton.Icon>
</AppBarToggleButton>";

        private const string AppBarToggleButtonPathXaml =
@"<AppBarToggleButton Label=""PathIcon"" Click=""AppBarButton_Click"" IsThreeState=""True"">
    <AppBarToggleButton.Icon>
        <PathIcon Data=""F1 M 20,20L 24,10L 24,24L 5,24""/>
    </AppBarToggleButton.Icon>
</AppBarToggleButton>";

        private const string AppBarSeparatorXaml =
@"<CommandBar>
    <CommandBar.PrimaryCommands>
        <AppBarButton Icon=""AttachCamera"" Label=""Attach Camera""/>
        <AppBarSeparator />
        <AppBarButton Icon=""Like"" Label=""Like""/>
        <AppBarButton Icon=""Dislike"" Label=""Dislike""/>
        <AppBarSeparator />
        <AppBarButton Icon=""Orientation"" Label=""Orientation""/>
    </CommandBar.PrimaryCommands>
</CommandBar>";

        private const string MenuFlyoutAppBarButtonXaml =
@"<AppBarButton Icon=""Sort"" IsCompact=""True"" ToolTipService.ToolTip=""Sort"" AutomationProperties.Name=""Sort"">
    <AppBarButton.Flyout>
        <MenuFlyout>
            <MenuFlyoutItem Text=""By rating"" Click=""MenuFlyoutItem_Click"" Tag=""rating""/>
            <MenuFlyoutItem Text=""By match"" Click=""MenuFlyoutItem_Click"" Tag=""match""/>
            <MenuFlyoutItem Text=""By distance"" Click=""MenuFlyoutItem_Click"" Tag=""distance""/>
        </MenuFlyout>
    </AppBarButton.Flyout>
</AppBarButton>";

        private const string MenuFlyoutToggleXaml =
@"<Button Content=""Options"">
    <Button.Flyout>
        <MenuFlyout>
            <MenuFlyoutItem Text=""Reset""/>
            <MenuFlyoutSeparator/>
            <ToggleMenuFlyoutItem Text=""Repeat"" IsChecked=""$(RepeatToggle)""/>
            <ToggleMenuFlyoutItem Text=""Shuffle"" IsChecked=""$(ShuffleToggle)""/>
        </MenuFlyout>
    </Button.Flyout>
</Button>";

        private const string MenuFlyoutCascadingXaml =
@"<Button Content=""File Options"">
    <Button.Flyout>
        <MenuFlyout>
            <MenuFlyoutItem Text=""Open""/>
            <MenuFlyoutSubItem Text=""Send to"">
                <MenuFlyoutItem Text=""Bluetooth"" />
                <MenuFlyoutItem Text=""Desktop (shortcut)"" />
                <MenuFlyoutSubItem Text=""Compressed file"">
                    <MenuFlyoutItem Text=""Compress and email"" />
                    <MenuFlyoutItem Text=""Compress to .7z"" />
                    <MenuFlyoutItem Text=""Compress to .zip"" />
                </MenuFlyoutSubItem>
            </MenuFlyoutSubItem>
        </MenuFlyout>
    </Button.Flyout>
</Button>";

        private const string MenuFlyoutSplitXaml =
@"<Button Content=""File Options"">
    <Button.Flyout>
        <MenuFlyout>
            <SplitMenuFlyoutItem Text=""Save"" Click=""SplitMenuFlyoutItem_Click"">
                <SplitMenuFlyoutItem.Icon>
                    <FontIcon Glyph=""&#xE74E;""/>
                </SplitMenuFlyoutItem.Icon>
                <MenuFlyoutItem Text=""Save as .docx"" Click=""SplitMenuFlyoutItem_Click""/>
                <MenuFlyoutItem Text=""Save as .pdf"" Click=""SplitMenuFlyoutItem_Click""/>
                <MenuFlyoutItem Text=""Save as .txt"" Click=""SplitMenuFlyoutItem_Click""/>
            </SplitMenuFlyoutItem>
            <SplitMenuFlyoutItem Text=""Share"" Icon=""Share"" Click=""SplitMenuFlyoutItem_Click"">
                <MenuFlyoutItem Text=""Share via email"" Click=""SplitMenuFlyoutItem_Click""/>
                <MenuFlyoutItem Text=""Share via link"" Click=""SplitMenuFlyoutItem_Click""/>
            </SplitMenuFlyoutItem>
        </MenuFlyout>
    </Button.Flyout>
</Button>";

        private const string MenuFlyoutIconsXaml =
@"<Button Content=""Edit Options"">
    <Button.Flyout>
        <MenuFlyout>
            <MenuFlyoutItem Text=""Share"">
                <MenuFlyoutItem.Icon>
                    <FontIcon Glyph=""&#xE72D;""/>
                </MenuFlyoutItem.Icon>
            </MenuFlyoutItem>
            <MenuFlyoutItem Text=""Copy"" Icon=""Copy""/>
            <MenuFlyoutItem Text=""Delete"" Icon=""Delete""/>
            <MenuFlyoutSeparator/>
            <MenuFlyoutItem Text=""Rename""/>
            <MenuFlyoutItem Text=""Select""/>
        </MenuFlyout>
    </Button.Flyout>
</Button>";

        private const string MenuFlyoutKeyboardXaml =
@"<Button Content=""Edit Options"">
    <Button.Flyout>
        <MenuFlyout>
            <MenuFlyoutItem Text=""Share"">
                <MenuFlyoutItem.Icon>
                    <FontIcon Glyph=""&#xE72D;""/>
                </MenuFlyoutItem.Icon>
                <MenuFlyoutItem.KeyboardAccelerators>
                    <KeyboardAccelerator Key=""S"" Modifiers=""Control""/>
                </MenuFlyoutItem.KeyboardAccelerators>
            </MenuFlyoutItem>
            <MenuFlyoutItem Text=""Copy"" Icon=""Copy"">
                <MenuFlyoutItem.KeyboardAccelerators>
                    <KeyboardAccelerator Key=""C"" Modifiers=""Control""/>
                </MenuFlyoutItem.KeyboardAccelerators>
            </MenuFlyoutItem>
            <MenuFlyoutItem Text=""Delete"" Icon=""Delete"">
                <MenuFlyoutItem.KeyboardAccelerators>
                    <KeyboardAccelerator Key=""Delete"" />
                </MenuFlyoutItem.KeyboardAccelerators>
            </MenuFlyoutItem>
            <MenuFlyoutSeparator/>
            <MenuFlyoutItem Text=""Rename""/>
            <MenuFlyoutItem Text=""Select""/>
        </MenuFlyout>
    </Button.Flyout>
</Button>";

        private const string MenuFlyoutRadioXaml =
@"<Button Content=""Options"">
    <Button.Flyout>
        <MenuFlyout>
            <RadioMenuFlyoutItem Text=""Landscape"" GroupName=""OrientationGroup""/>
            <RadioMenuFlyoutItem Text=""Portrait"" GroupName=""OrientationGroup"" IsChecked=""True""/>
            <MenuFlyoutSeparator/>
            <RadioMenuFlyoutItem Text=""Small icons"" GroupName=""SizeGroup""/>
            <RadioMenuFlyoutItem Text=""Medium icons"" IsChecked=""True"" GroupName=""SizeGroup""/>
            <RadioMenuFlyoutItem Text=""Large icons"" GroupName=""SizeGroup""/>
        </MenuFlyout>
    </Button.Flyout>
</Button>";

        private const string SwipeControlRightXaml =
@"<Border>
    <Border.Resources>
        <FontIconSource x:Key=""AcceptIcon"" Glyph=""&#xE8FB;""/>
        <FontIconSource x:Key=""FlagIcon"" Glyph=""&#xE7C1;""/>

        <SwipeItems x:Key=""left"" Mode=""Reveal"">
            <SwipeItem Text=""Accept"" IconSource=""{StaticResource AcceptIcon}"" Invoked=""Accept_ItemInvoked""/>
            <SwipeItem Text=""Flag"" IconSource=""{StaticResource FlagIcon}"" Invoked=""Flag_ItemInvoked""/>
        </SwipeItems>
    </Border.Resources>
    <SwipeControl BorderThickness=""1""
        LeftItems=""{StaticResource left}"" BorderBrush=""{ThemeResource ButtonBackground}""
        Width=""300"" Margin=""12"" Height=""68"">
            <TextBlock Text=""Swipe Right"" Margin=""12""
                       HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
    </SwipeControl>
</Border>";

        private const string SwipeControlLeftExecuteXaml =
@"<Border>
    <Border.Resources>
        <FontIconSource x:Key=""DeleteIcon"" Glyph=""&#xE74D;""/>
        <SwipeItems x:Key=""right"" Mode=""Execute"">
            <SwipeItem Text=""Archive"" IconSource=""{StaticResource ArchiveIcon}""
                       BehaviorOnInvoked=""Close"" Invoked=""DeleteOne_ItemInvoked""/>
        </SwipeItems>
    </Border.Resources>
    <SwipeControl BorderThickness=""1"" BorderBrush=""{ThemeResource ButtonBackground}""
        RightItems=""{StaticResource right}""
        Width=""300"" Margin=""12"" Height=""68"">
        <TextBlock Text=""Swipe Left"" Margin=""12""
                   HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
    </SwipeControl>
</Border>";

        private const string SwipeControlListViewXaml =
@"<ListView x:Name=""lv"" Width=""400"" Height=""300"" Margin=""12"">
    <ListView.Resources>
        <FontIconSource x:Key=""ReplyAllIcon"" Glyph=""&#xE8C2;""/>
        <FontIconSource x:Key=""ReadIcon"" Glyph=""&#xE8C3;""/>
        <FontIconSource x:Key=""DeleteIcon"" Glyph=""&#xE74D;""/>

        <SwipeItems x:Key=""left"" Mode=""Reveal"">
            <SwipeItem Text=""Reply All"" IconSource=""{StaticResource ReplyAllIcon}""
                       Background=""#FF3e6fa7"" Foreground=""White""/>
            <SwipeItem Text=""Open"" IconSource=""{StaticResource ReadIcon}""
                       Background=""#FFff9501"" Foreground=""White""/>
        </SwipeItems>
        <SwipeItems x:Key=""right"" Mode=""Execute"">
            <SwipeItem Text=""Delete"" IconSource=""{StaticResource DeleteIcon}""
                       Background=""#FFF4B183"" Invoked=""DeleteItem_ItemInvoked""/>
        </SwipeItems>
    </ListView.Resources>

    <ListView.ItemTemplate>
        <DataTemplate>
            <SwipeControl BorderThickness=""0,1,0,0"" BorderBrush=""{ThemeResource ButtonBackground}"" Height=""68""
                       Width=""800"" MinWidth=""200"" LeftItems=""{StaticResource left}""
                          RightItems=""{StaticResource right}"">
                <TextBlock Text=""{Binding}"" FontSize=""24"" Margin=""12""
                           HorizontalAlignment=""Stretch"" VerticalAlignment=""Center""/>
            </SwipeControl>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>";

        private const string SwipeControlGradientXaml =
@"<Border>
    <Border.Resources>
        <FontIconSource x:Key=""LockIcon"" Glyph=""&#xE72E;""/>
        <LinearGradientBrush x:Key=""PurpleGradient"" StartPoint=""0,0.5"" EndPoint=""1,0.5"">
            <GradientStop Color=""#ff8990f9"" Offset=""0.0""/>
            <GradientStop Color=""#ff5b66fb"" Offset=""0.5""/>
            <GradientStop Color=""#ff5c1df4"" Offset=""1.0""/>
        <LinearGradientBrush/>
        <SwipeItems x:Key=""right"" Mode=""Execute"">
            <SwipeItem Text=""Lock"" Background=""{StaticResource PurpleGradient}""
                       BehaviorOnInvoked=""Close"" IconSource=""{StaticResource LockIcon}""/>
        </SwipeItems>
    </Border.Resources>
    <SwipeControl BorderThickness=""1"" BorderBrush=""{ThemeResource ButtonBackground}""
        RightItems=""{StaticResource right}""
        Width=""500"" Margin=""12"" Height=""68"">
        <TextBlock Text=""Swipe Left"" Margin=""12""
                   HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
    </SwipeControl>
</Border>";

        private const string SwipeControlCustomIconsXaml =
@"<Border>
    <Border.Resources>
        <SwipeItems x:Key=""left"" Mode=""Reveal"">
            <SwipeItem Text=""Coffee"">
                <SwipeItem.IconSource>
                    <BitmapIconSource UriSource=""ms-appx:///Assets/SampleMedia/CoffeeCup.png""/>
                <SwipeItem.IconSource/>
            <SwipeItem/>
        </SwipeItems>
    </Border.Resources>
    <SwipeControl BorderThickness=""1""
        LeftItems=""{StaticResource left}"" BorderBrush=""{ThemeResource ButtonBackground}""
        Width=""300"" Margin=""12"" Height=""68"">
            <TextBlock Text=""Swipe Right"" Margin=""12""
                       HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
    </SwipeControl>
</Border>";

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "AppBarButton":
                    return CreateAppBarButtonExamples();
                case "AppBarSeparator":
                    return CreateAppBarSeparatorExamples();
                case "AppBarToggleButton":
                    return CreateAppBarToggleButtonExamples();
                case "CommandBar":
                    return CreateCommandBarExamples();
                case "CommandBarFlyout":
                    return CreateCommandBarFlyoutExamples(sampleSnippets);
                case "MenuFlyout":
                    return CreateMenuFlyoutExamples();
                case "MenuBar":
                    return CreateMenuBarExamples(sampleSnippets);
                case "SwipeControl":
                    return CreateSwipeControlExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AppBarButton":
                    return CreateAppBarButtonSample();
                case "AppBarSeparator":
                    return CreateAppBarSeparatorSample();
                case "AppBarToggleButton":
                    return CreateAppBarToggleButtonSample();
                case "CommandBar":
                    return CreateCommandBarSample();
                case "CommandBarFlyout":
                    return CreateCommandBarFlyoutSample();
                case "MenuBar":
                    return CreateMenuBarSample();
                case "MenuFlyout":
                    return CreateMenuFlyoutSample();
                case "SwipeControl":
                    return CreateSwipeControlSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateAppBarButtonSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("AppBarButton"));
            panel.Children.Add(CreateAppBarButtonSymbolExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateAppBarButtonExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "An AppBarButton with a symbol icon.",
                    CreateAppBarButtonSymbolExampleContent(assignRootAutomationId: true),
                    AppBarButtonSymbolXaml,
                    null),
                new GalleryExample(
                    "An AppBarButton with a bitmap icon.",
                    CreateAppBarButtonBitmapExampleContent(),
                    AppBarButtonBitmapXaml,
                    null),
                new GalleryExample(
                    "An AppBarButton with a font icon.",
                    CreateAppBarButtonFontExampleContent(),
                    AppBarButtonFontXaml,
                    null),
                new GalleryExample(
                    "An AppBarButton with a path icon.",
                    CreateAppBarButtonPathExampleContent(),
                    AppBarButtonPathXaml,
                    null),
                new GalleryExample(
                    "An AppBarButton with a KeyboardAccelerator",
                    CreateAppBarButtonKeyboardAcceleratorExampleContent(),
                    AppBarButtonKeyboardAcceleratorXaml,
                    null),
                new GalleryExample(
                    "An AppBarButton that opens a Flyout containing an input control.",
                    CreateAppBarButtonFlyoutExampleContent(),
                    AppBarButtonFlyoutXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateAppBarButtonSymbolExampleContent(bool assignRootAutomationId)
        {
            var output = CreateAppBarOutput("Control1Output");
            var button = CreateSourceAppBarButton("Button1", Mux.Symbol.Like, "SymbolIcon", output);
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarButton", "AppBarButton"));
            return CreateAppBarButtonExampleRoot(assignRootAutomationId, button, output);
        }

        private static GallerySamplePanel CreateAppBarButtonBitmapExampleContent()
        {
            var output = CreateAppBarOutput("Control2Output");
            var button = CreateSourceAppBarButton("Button2", new Mux.BitmapIcon
            {
                UriSource = new Uri(ResourceUri("Assets/SampleMedia/Slices2.png"), UriKind.Absolute)
            }, "BitmapIcon", output);
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarButton", "BitmapIconButton"));
            return CreateAppBarButtonExampleRoot(false, button, output);
        }

        private static GallerySamplePanel CreateAppBarButtonFontExampleContent()
        {
            var output = CreateAppBarOutput("Control3Output");
            var button = CreateSourceAppBarButton("Button3", new Mux.FontIcon
            {
                FontFamily = new FontFamily("Candara"),
                Glyph = "\u03A3"
            }, "FontIcon", output);
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarButton", "FontIconButton"));
            return CreateAppBarButtonExampleRoot(false, button, output);
        }

        private static GallerySamplePanel CreateAppBarButtonPathExampleContent()
        {
            var output = CreateAppBarOutput("Control4Output");
            var pathIcon = new Mux.PathIcon
            {
                Data = Geometry.Parse("F1 M 20,20L 24,10L 24,24L 5,24")
            };
            var button = new Mux.AppBarButton
            {
                Name = "Button4",
                Content = new Viewbox
                {
                    Stretch = Stretch.Uniform,
                    Child = pathIcon
                },
                Label = "PathIcon"
            };
            button.Click += delegate { output.Text = "You clicked: " + button.Name; };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarButton", "PathIconButton"));
            return CreateAppBarButtonExampleRoot(false, button, output);
        }

        private static GallerySamplePanel CreateAppBarButtonKeyboardAcceleratorExampleContent()
        {
            var output = CreateAppBarOutput("Control5Output");
            var button = CreateSourceAppBarButton("Button5", Mux.Symbol.Save, "Save", output);
            button.InputGestureText = "Ctrl+S";
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarButton", "KeyboardAcceleratorButton"));
            return CreateAppBarButtonExampleRoot(false, button, output);
        }

        private static GallerySamplePanel CreateAppBarButtonFlyoutExampleContent()
        {
            var textBox = new TextBox
            {
                MinWidth = 240
            };
            ControlHelper.SetPlaceholderText(textBox, "Input text here");

            var button = new Mux.AppBarButton
            {
                Name = "Button6",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Edit),
                Label = "Edit",
                Flyout = new Mux.Flyout
                {
                    Content = textBox
                }
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarButton", "FlyoutButton"));
            return CreateAppBarButtonExampleRoot(false, button, null);
        }

        private static UIElement CreateAppBarSeparatorSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("AppBarSeparator"));
            panel.Children.Add(CreateAppBarSeparatorExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateAppBarSeparatorExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "AppBarButtons separated by AppBarSeparators.",
                    CreateAppBarSeparatorExampleContent(assignRootAutomationId: true),
                    AppBarSeparatorXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateAppBarSeparatorExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("AppBarSeparator"));
            }

            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
            };

            var bar = new Mux.CommandBar
            {
                Name = "Control1",
                DefaultLabelPosition = Mux.CommandBarDefaultLabelPosition.Collapsed,
                OverflowButtonVisibility = Mux.CommandBarOverflowButtonVisibility.Visible,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(bar, GalleryAutomation.SampleElementId("AppBarSeparator", "CommandBar"));
            bar.PrimaryCommands.Add(CreateAppBarSeparatorButton(Mux.Symbol.AttachCamera, "Attach Camera"));
            bar.PrimaryCommands.Add(new Mux.AppBarSeparator());
            bar.PrimaryCommands.Add(CreateAppBarSeparatorButton(Mux.Symbol.Like, "Like"));
            bar.PrimaryCommands.Add(CreateAppBarSeparatorButton(Mux.Symbol.Dislike, "Dislike"));
            bar.PrimaryCommands.Add(new Mux.AppBarSeparator());
            bar.PrimaryCommands.Add(CreateAppBarSeparatorButton(Mux.Symbol.Orientation, "Orientation"));

            scrollViewer.Content = bar;
            panel.Children.Add(scrollViewer);
            return panel;
        }

        private static UIElement CreateAppBarToggleButtonSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("AppBarToggleButton"));
            panel.Children.Add(CreateAppBarToggleButtonSymbolExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateAppBarToggleButtonExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "An AppBarToggleButton with a symbol icon.",
                    CreateAppBarToggleButtonSymbolExampleContent(assignRootAutomationId: true),
                    AppBarToggleButtonSymbolXaml,
                    null),
                new GalleryExample(
                    "An AppBarToggleButton with a bitmap icon.",
                    CreateAppBarToggleButtonBitmapExampleContent(),
                    AppBarToggleButtonBitmapXaml,
                    null),
                new GalleryExample(
                    "An AppBarToggleButton with a font icon.",
                    CreateAppBarToggleButtonFontExampleContent(),
                    AppBarToggleButtonFontXaml,
                    null),
                new GalleryExample(
                    "A three-state AppBarToggleButton with a path icon.",
                    CreateAppBarToggleButtonPathExampleContent(),
                    AppBarToggleButtonPathXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateAppBarToggleButtonSymbolExampleContent(bool assignRootAutomationId)
        {
            var output = CreateAppBarOutput("Control1Output");
            var button = CreateSourceAppBarToggleButton("Button1", Mux.Symbol.Shuffle, "SymbolIcon", output);
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarToggleButton", "AppBarToggleButton"));
            return CreateAppBarButtonExampleRoot(assignRootAutomationId, button, output);
        }

        private static GallerySamplePanel CreateAppBarToggleButtonBitmapExampleContent()
        {
            var output = CreateAppBarOutput("Control2Output");
            var button = CreateSourceAppBarToggleButton("Button2", new Mux.BitmapIcon
            {
                UriSource = new Uri(ResourceUri("Assets/SampleMedia/Slices2.png"), UriKind.Absolute)
            }, "BitmapIcon", output);
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarToggleButton", "BitmapIconButton"));
            return CreateAppBarButtonExampleRoot(false, button, output);
        }

        private static GallerySamplePanel CreateAppBarToggleButtonFontExampleContent()
        {
            var output = CreateAppBarOutput("Control3Output");
            var button = CreateSourceAppBarToggleButton("Button3", new Mux.FontIcon
            {
                FontFamily = new FontFamily("Candara"),
                Glyph = "\u03A3"
            }, "FontIcon", output);
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarToggleButton", "FontIconButton"));
            return CreateAppBarButtonExampleRoot(false, button, output);
        }

        private static GallerySamplePanel CreateAppBarToggleButtonPathExampleContent()
        {
            var output = CreateAppBarOutput("Control4Output");
            var button = new Mux.AppBarToggleButton
            {
                Name = "Button4",
                Content = new Viewbox
                {
                    Child = new Mux.PathIcon
                    {
                        Data = Geometry.Parse("F1 M 20,20L 24,10L 24,24L 5,24")
                    }
                },
                IsThreeState = true,
                Label = "PathIcon"
            };
            button.Click += delegate { output.Text = "IsChecked = " + button.IsChecked.ToString(); };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("AppBarToggleButton", "PathIconButton"));
            return CreateAppBarButtonExampleRoot(false, button, output);
        }

        private static UIElement CreateCommandBarSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("CommandBar"));
            panel.Children.Add(CreateCommandBarExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateCommandBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A command bar with labels on the side free floating in a page",
                    CreateCommandBarExampleContent(assignRootAutomationId: true),
                    CommandBarExampleXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateCommandBarExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("CommandBar"));
            }

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });

            var samplePanel = new StackPanel();
            var output = new TextBlock
            {
                Name = "SelectedOptionText",
                Padding = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            var commandBar = new Mux.CommandBar
            {
                Name = "PrimaryCommandBar",
                DefaultLabelPosition = Mux.CommandBarDefaultLabelPosition.Right,
                IsOpen = false,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(commandBar, GalleryAutomation.SampleElementId("CommandBar", "CommandBar"));

            var addButton = CreateCommandBarAppBarButton(Mux.Symbol.Add, "Add", output, "Ctrl+A");
            addButton.Name = "addButton";
            var editButton = CreateCommandBarAppBarButton(Mux.Symbol.Edit, "Edit", output, "Ctrl+E");
            editButton.Name = "editButton";
            var shareButton = CreateCommandBarAppBarButton(Mux.Symbol.Share, "Share", output, "F4");
            shareButton.Name = "shareButton";
            commandBar.PrimaryCommands.Add(addButton);
            commandBar.PrimaryCommands.Add(editButton);
            commandBar.PrimaryCommands.Add(shareButton);

            var settingsButton = CreateCommandBarAppBarButton(Mux.Symbol.Setting, "Settings", output, "Ctrl+I");
            settingsButton.Name = "settingsButton";
            commandBar.SecondaryCommands.Add(settingsButton);

            samplePanel.Children.Add(commandBar);
            samplePanel.Children.Add(output);
            layout.Children.Add(samplePanel);

            var options = CreateCommandBarOptions(commandBar, output);
            Grid.SetColumn(options, 2);
            layout.Children.Add(options);

            root.Children.Add(layout);
            return root;
        }

        private static GallerySamplePanel CreateAppBarButtonExampleRoot(bool assignRootAutomationId, UIElement button, TextBlock output)
        {
            var panel = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            if (assignRootAutomationId)
            {
                var controlName = button is Mux.AppBarToggleButton ? "AppBarToggleButton" : "AppBarButton";
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId(controlName));
            }

            panel.Children.Add(button);
            if (output != null)
            {
                panel.Children.Add(output);
            }

            return panel;
        }

        private static StackPanel CreateCommandBarOptions(Mux.CommandBar commandBar, TextBlock output)
        {
            var options = new StackPanel
            {
                Width = 180,
                VerticalAlignment = VerticalAlignment.Top
            };
            options.Children.Add(new TextBlock { Text = "Show or hide" });
            options.Children.Add(CreateOptionButton("Open command bar", delegate
            {
                commandBar.IsOpen = true;
            }));
            options.Children.Add(CreateOptionButton("Close command bar", delegate
            {
                commandBar.IsOpen = false;
            }));
            options.Children.Add(new TextBlock
            {
                Text = "Modify content",
                Margin = new Thickness(0, 16, 0, 0)
            });
            options.Children.Add(CreateOptionButton("Add secondary commands", delegate
            {
                AddCommandBarSecondaryCommands(commandBar, output);
            }));
            options.Children.Add(CreateOptionButton("Remove secondary commands", delegate
            {
                RemoveCommandBarSecondaryCommands(commandBar);
            }));
            return options;
        }

        private static Button CreateOptionButton(string content, RoutedEventHandler click)
        {
            var button = new Button
            {
                Content = content,
                Margin = new Thickness(0, 12, 0, 0)
            };
            button.Click += click;
            return button;
        }

        private static void AddCommandBarSecondaryCommands(Mux.CommandBar commandBar, TextBlock output)
        {
            if (commandBar.SecondaryCommands.Count != 1)
            {
                return;
            }

            commandBar.SecondaryCommands.Add(CreateCommandBarAppBarButton(Mux.Symbol.Add, "Button 1", output, "Ctrl+N"));
            commandBar.SecondaryCommands.Add(CreateCommandBarAppBarButton(Mux.Symbol.Delete, "Button 2", output, "Delete"));
            commandBar.SecondaryCommands.Add(new Mux.AppBarSeparator());
            commandBar.SecondaryCommands.Add(CreateCommandBarAppBarButton(Mux.Symbol.FontDecrease, "Button 3", output, "Ctrl+-"));
            commandBar.SecondaryCommands.Add(CreateCommandBarAppBarButton(Mux.Symbol.FontIncrease, "Button 4", output, "Ctrl++"));
        }

        private static void RemoveCommandBarSecondaryCommands(Mux.CommandBar commandBar)
        {
            while (commandBar.SecondaryCommands.Count > 1)
            {
                commandBar.SecondaryCommands.RemoveAt(commandBar.SecondaryCommands.Count - 1);
            }
        }

        private static UIElement CreateCommandBarFlyoutSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("CommandBarFlyout"));
            panel.Children.Add(CreateCommandBarFlyoutExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateCommandBarFlyoutExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "CommandBarFlyout for commands on an in-app object",
                    CreateCommandBarFlyoutExampleContent(assignRootAutomationId: true),
                    FindSnippetText(sampleSnippets, "CommandBarFlyoutSample1_xaml.txt"),
                    FindSnippetText(sampleSnippets, "CommandBarFlyoutSample1_cs.txt"))
            };
        }

        private static GallerySamplePanel CreateCommandBarFlyoutExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("CommandBarFlyout"));
            }

            root.Children.Add(new TextBlock
            {
                Text = "Click or right click the image to open a CommandBarFlyout",
                TextWrapping = TextWrapping.Wrap
            });

            var output = CreateSelectionOutput("SelectedOptionText");
            var flyout = new Mux.CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Right
            };
            flyout.PrimaryCommands.Add(CreateCommandBarFlyoutAppBarButton(Mux.Symbol.Share, "Share", output, "Share"));
            flyout.PrimaryCommands.Add(CreateCommandBarFlyoutAppBarButton(Mux.Symbol.Save, "Save", output, "Save"));
            flyout.PrimaryCommands.Add(CreateCommandBarFlyoutAppBarButton(Mux.Symbol.Delete, "Delete", output, "Delete"));
            flyout.SecondaryCommands.Add(CreateCommandBarFlyoutAppBarButton("Resize", output));
            flyout.SecondaryCommands.Add(CreateCommandBarFlyoutAppBarButton("Move", output));
            root.Resources.Add("CommandBarFlyout1", flyout);

            var image = new Image
            {
                Name = "Image1",
                Height = 300,
                Source = new BitmapImage(new Uri(
                    "pack://application:,,,/ModernWpf.Gallery;component/Assets/SampleMedia/rainier.jpg",
                    UriKind.Absolute))
            };
            image.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);

            var button = new Button
            {
                Name = "myImageButton",
                Content = image,
                Margin = new Thickness(0, 12, 0, 12),
                Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, "mountain");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("CommandBarFlyout", "ShowButton"));

            void ShowMenu(bool isTransient)
            {
                var options = new FlyoutShowOptions
                {
                    ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard,
                    Placement = FlyoutPlacementMode.RightEdgeAlignedTop
                };
                flyout.ShowAt(image, options);
            }

            button.Click += delegate { ShowMenu(true); };
            button.MouseRightButtonUp += delegate
            {
                ShowMenu(false);
            };

            root.Children.Add(button);
            root.Children.Add(output);
            return root;
        }

        private static UIElement CreateMenuBarSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("MenuBar"));
            panel.Children.Add(CreateSimpleMenuBarExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateMenuBarExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "A simple MenuBar",
                    CreateSimpleMenuBarExampleContent(assignRootAutomationId: true),
                    FindSnippetText(sampleSnippets, "MenuBarSample1.txt"),
                    null),
                new GalleryExample(
                    "MenuBar with keyboard accelerators",
                    CreateKeyboardAcceleratorsMenuBarExampleContent(),
                    FindSnippetText(sampleSnippets, "MenuBarSample3.txt"),
                    null),
                new GalleryExample(
                    "MenuBar with submenus, separators, and radio items",
                    CreateSubmenusMenuBarExampleContent(),
                    FindSnippetText(sampleSnippets, "MenuBarSample2.txt"),
                    null)
            };
        }

        private static GallerySamplePanel CreateMenuBarExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("MenuBar"));
            }

            return root;
        }

        private static GallerySamplePanel CreateSimpleMenuBarExampleContent(bool assignRootAutomationId)
        {
            var panel = CreateMenuBarExampleRoot(assignRootAutomationId);
            var output = CreateSelectionOutput("SelectedOptionText");
            var menu = CreateMenuBar();
            GalleryAutomation.WithAutomationId(menu, GalleryAutomation.SampleElementId("MenuBar", "MenuBar"));

            var file = new Mux.MenuBarItem { Title = "File" };
            file.Items.Add(CreateMenuItem("New", output));
            file.Items.Add(CreateMenuItem("Open...", output));
            file.Items.Add(CreateMenuItem("Save", output));
            file.Items.Add(CreateMenuItem("Exit", output));

            var edit = new Mux.MenuBarItem { Title = "Edit" };
            edit.Items.Add(CreateMenuItem("Undo", output));
            edit.Items.Add(CreateMenuItem("Cut", output));
            edit.Items.Add(CreateMenuItem("Copy", output));
            edit.Items.Add(CreateMenuItem("Paste", output));

            var help = new Mux.MenuBarItem { Title = "Help" };
            help.Items.Add(CreateMenuItem("About", output));

            menu.Items.Add(file);
            menu.Items.Add(edit);
            menu.Items.Add(help);
            panel.Children.Add(output);
            panel.Children.Add(menu);
            return panel;
        }

        private static GallerySamplePanel CreateKeyboardAcceleratorsMenuBarExampleContent()
        {
            var panel = CreateMenuBarExampleRoot(assignRootAutomationId: false);
            var output = CreateSelectionOutput("SelectedOptionText1");
            var menu = CreateMenuBar();
            GalleryAutomation.WithAutomationId(menu, GalleryAutomation.SampleElementId("MenuBar", "KeyboardAcceleratorsMenuBar"));

            var file = new Mux.MenuBarItem { Title = "File" };
            file.Items.Add(CreateMenuItem("New", output, "Ctrl+N"));
            file.Items.Add(CreateMenuItem("Open...", output, "Ctrl+O"));
            file.Items.Add(CreateMenuItem("Save", output, "Ctrl+S"));
            file.Items.Add(CreateMenuItem("Exit", output, "Ctrl+E"));

            var edit = new Mux.MenuBarItem { Title = "Edit" };
            edit.Items.Add(CreateMenuItem("Undo", output, "Ctrl+Z"));
            edit.Items.Add(CreateMenuItem("Cut", output, "Ctrl+X"));
            edit.Items.Add(CreateMenuItem("Copy", output, "Ctrl+C"));
            edit.Items.Add(CreateMenuItem("Paste", output, "Ctrl+V"));

            var help = new Mux.MenuBarItem { Title = "Help" };
            help.Items.Add(CreateMenuItem("About", output, "Ctrl+I"));

            menu.Items.Add(file);
            menu.Items.Add(edit);
            menu.Items.Add(help);
            panel.Children.Add(output);
            panel.Children.Add(menu);
            return panel;
        }

        private static GallerySamplePanel CreateSubmenusMenuBarExampleContent()
        {
            var panel = CreateMenuBarExampleRoot(assignRootAutomationId: false);
            var output = CreateSelectionOutput("SelectedOptionText2");
            var menu = CreateMenuBar();
            GalleryAutomation.WithAutomationId(menu, GalleryAutomation.SampleElementId("MenuBar", "SubmenusMenuBar"));

            var file = new Mux.MenuBarItem { Title = "File" };
            var newSubmenu = new MenuItem { Header = "New" };
            newSubmenu.Items.Add(CreateMenuItem("Plain Text Document", output));
            newSubmenu.Items.Add(CreateMenuItem("Rich Text Document", output));
            newSubmenu.Items.Add(CreateMenuItem("Other Formats...", output));
            file.Items.Add(newSubmenu);
            file.Items.Add(CreateMenuItem("Open...", output));
            file.Items.Add(CreateMenuItem("Save", output));
            file.Items.Add(new Separator());
            file.Items.Add(CreateMenuItem("Exit", output));

            var edit = new Mux.MenuBarItem { Title = "Edit" };
            edit.Items.Add(CreateMenuItem("Undo", output));
            edit.Items.Add(CreateMenuItem("Cut", output));
            edit.Items.Add(CreateMenuItem("Copy", output));
            edit.Items.Add(CreateMenuItem("Paste", output));

            var view = new Mux.MenuBarItem { Title = "View" };
            view.Items.Add(CreateMenuItem("Output", output));
            view.Items.Add(new Separator());
            view.Items.Add(CreateRadioMenuItem("Landscape", "OrientationGroup", isChecked: false, output));
            view.Items.Add(CreateRadioMenuItem("Portrait", "OrientationGroup", isChecked: true, output));
            view.Items.Add(new Separator());
            view.Items.Add(CreateRadioMenuItem("Small icons", "SizeGroup", isChecked: false, output));
            view.Items.Add(CreateRadioMenuItem("Medium icons", "SizeGroup", isChecked: true, output));
            view.Items.Add(CreateRadioMenuItem("Large icons", "SizeGroup", isChecked: false, output));

            var help = new Mux.MenuBarItem { Title = "Help" };
            help.Items.Add(CreateMenuItem("About", output));

            menu.Items.Add(file);
            menu.Items.Add(edit);
            menu.Items.Add(view);
            menu.Items.Add(help);
            panel.Children.Add(output);
            panel.Children.Add(menu);
            return panel;
        }

        private static UIElement CreateMenuFlyoutSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("MenuFlyout"));
            panel.Children.Add(CreateMenuFlyoutAppBarButtonExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateMenuFlyoutExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "An AppBarButton with a MenuFlyout.",
                    CreateMenuFlyoutAppBarButtonExampleContent(assignRootAutomationId: true),
                    MenuFlyoutAppBarButtonXaml,
                    null),
                new GalleryExample(
                    "A MenuFlyout with ToggleMenuFlyoutItems and MenuFlyoutSeparator.",
                    CreateMenuFlyoutToggleExampleContent(),
                    MenuFlyoutToggleXaml,
                    null),
                new GalleryExample(
                    "A MenuFlyout with cascading menus.",
                    CreateMenuFlyoutCascadingExampleContent(),
                    MenuFlyoutCascadingXaml,
                    null),
                new GalleryExample(
                    "A MenuFlyout with SplitMenuFlyoutItems.",
                    CreateMenuFlyoutSplitExampleContent(),
                    MenuFlyoutSplitXaml,
                    null),
                new GalleryExample(
                    "A MenuFlyout with icons.",
                    CreateMenuFlyoutIconsExampleContent(includeKeyboardAccelerators: false),
                    MenuFlyoutIconsXaml,
                    null),
                new GalleryExample(
                    "A MenuFlyout with icons and Keyboard Accelerators.",
                    CreateMenuFlyoutIconsExampleContent(includeKeyboardAccelerators: true),
                    MenuFlyoutKeyboardXaml,
                    null),
                new GalleryExample(
                    "A MenuFlyout with RadioMenuFlyoutItems",
                    CreateMenuFlyoutRadioExampleContent(),
                    MenuFlyoutRadioXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateMenuFlyoutExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("MenuFlyout"));
            }

            return root;
        }

        private static GallerySamplePanel CreateMenuFlyoutAppBarButtonExampleContent(bool assignRootAutomationId)
        {
            var panel = CreateMenuFlyoutExampleRoot(assignRootAutomationId);
            var content = new StackPanel
            {
                Name = "Control1",
                Orientation = Orientation.Horizontal
            };
            var output = CreateSelectionOutput("Control1Output");
            var button = new Mux.AppBarButton
            {
                Icon = new Mux.SymbolIcon(Mux.Symbol.Sort),
                IsCompact = true,
                ToolTip = "Sort"
            };
            AutomationProperties.SetName(button, "Sort");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("MenuFlyout", "AppBarButton"));

            var flyout = new Mux.MenuFlyout { Placement = FlyoutPlacementMode.Bottom };
            flyout.Items.Add(CreateSortMenuFlyoutItem("By rating", "rating", output));
            flyout.Items.Add(CreateSortMenuFlyoutItem("By match", "match", output));
            flyout.Items.Add(CreateSortMenuFlyoutItem("By distance", "distance", output));
            button.Flyout = flyout;

            content.Children.Add(button);
            content.Children.Add(output);
            panel.Children.Add(content);
            return panel;
        }

        private static GallerySamplePanel CreateMenuFlyoutToggleExampleContent()
        {
            var panel = CreateMenuFlyoutExampleRoot(assignRootAutomationId: false);
            var button = CreateButton("Options");
            button.Name = "Control2";
            var flyout = new Mux.MenuFlyout { Placement = FlyoutPlacementMode.Bottom };
            flyout.Items.Add(new MenuItem { Header = "Reset" });
            flyout.Items.Add(new Separator());
            flyout.Items.Add(new MenuItem
            {
                Name = "RepeatToggleMenuFlyoutItem",
                Header = "Repeat",
                IsCheckable = true,
                IsChecked = true
            });
            flyout.Items.Add(new MenuItem
            {
                Name = "ShuffleToggleMenuFlyoutItem",
                Header = "Shuffle",
                IsCheckable = true,
                IsChecked = true
            });
            Mux.FlyoutService.SetFlyout(button, flyout);
            panel.Children.Add(button);
            return panel;
        }

        private static GallerySamplePanel CreateMenuFlyoutCascadingExampleContent()
        {
            var panel = CreateMenuFlyoutExampleRoot(assignRootAutomationId: false);
            var button = CreateButton("File Options");
            button.Name = "Control3";
            var flyout = new Mux.MenuFlyout { Placement = FlyoutPlacementMode.Bottom };
            flyout.Items.Add(new MenuItem { Header = "Open" });
            var sendTo = new MenuItem { Header = "Send to" };
            sendTo.Items.Add(new MenuItem { Header = "Bluetooth" });
            sendTo.Items.Add(new MenuItem { Header = "Desktop (shortcut)" });
            var compressedFile = new MenuItem { Header = "Compressed file" };
            compressedFile.Items.Add(new MenuItem { Header = "Compress and email" });
            compressedFile.Items.Add(new MenuItem { Header = "Compress to .7z" });
            compressedFile.Items.Add(new MenuItem { Header = "Compress to .zip" });
            sendTo.Items.Add(compressedFile);
            flyout.Items.Add(sendTo);
            Mux.FlyoutService.SetFlyout(button, flyout);
            panel.Children.Add(button);
            return panel;
        }

        private static GallerySamplePanel CreateMenuFlyoutSplitExampleContent()
        {
            var panel = CreateMenuFlyoutExampleRoot(assignRootAutomationId: false);
            var content = new StackPanel
            {
                Name = "Control3b",
                Orientation = Orientation.Horizontal
            };
            var button = CreateButton("File Options");
            var output = CreateSelectionOutput("Control3bOutput");
            var flyout = new Mux.MenuFlyout { Placement = FlyoutPlacementMode.Bottom };

            var save = CreateSplitMenuFlyoutItem("Save", output, new Mux.FontIcon { Glyph = "\uE74E" });
            save.Name = "SaveSplitItem";
            save.Items.Add(CreateSplitMenuFlyoutItem("Save as .docx", output, null));
            save.Items.Add(CreateSplitMenuFlyoutItem("Save as .pdf", output, null));
            save.Items.Add(CreateSplitMenuFlyoutItem("Save as .txt", output, null));
            flyout.Items.Add(save);

            var share = CreateSplitMenuFlyoutItem("Share", output, new Mux.SymbolIcon(Mux.Symbol.Share));
            share.Items.Add(CreateSplitMenuFlyoutItem("Share via email", output, null));
            share.Items.Add(CreateSplitMenuFlyoutItem("Share via link", output, null));
            flyout.Items.Add(share);

            Mux.FlyoutService.SetFlyout(button, flyout);
            content.Children.Add(button);
            content.Children.Add(output);
            panel.Children.Add(content);
            return panel;
        }

        private static GallerySamplePanel CreateMenuFlyoutIconsExampleContent(bool includeKeyboardAccelerators)
        {
            var panel = CreateMenuFlyoutExampleRoot(assignRootAutomationId: false);
            var button = CreateButton("Edit Options");
            button.Name = includeKeyboardAccelerators ? "Control5" : "Control4";
            var flyout = new Mux.MenuFlyout { Placement = FlyoutPlacementMode.Bottom };
            flyout.Items.Add(CreateIconMenuFlyoutItem("Share", new Mux.FontIcon { Glyph = "\uE72D" }, includeKeyboardAccelerators ? "Ctrl+S" : null));
            flyout.Items.Add(CreateIconMenuFlyoutItem("Copy", new Mux.SymbolIcon(Mux.Symbol.Copy), includeKeyboardAccelerators ? "Ctrl+C" : null));
            flyout.Items.Add(CreateIconMenuFlyoutItem("Delete", new Mux.SymbolIcon(Mux.Symbol.Delete), includeKeyboardAccelerators ? "Delete" : null));
            flyout.Items.Add(new Separator());
            flyout.Items.Add(new MenuItem { Header = "Rename" });
            flyout.Items.Add(new MenuItem { Header = "Select" });
            Mux.FlyoutService.SetFlyout(button, flyout);
            panel.Children.Add(button);
            return panel;
        }

        private static GallerySamplePanel CreateMenuFlyoutRadioExampleContent()
        {
            var panel = CreateMenuFlyoutExampleRoot(assignRootAutomationId: false);
            var button = CreateButton("Options");
            button.Name = "Control6";
            var flyout = new Mux.MenuFlyout { Placement = FlyoutPlacementMode.Bottom };
            flyout.Items.Add(CreateRadioMenuItem("Landscape", "OrientationGroup", isChecked: false, output: null));
            flyout.Items.Add(CreateRadioMenuItem("Portrait", "OrientationGroup", isChecked: true, output: null));
            flyout.Items.Add(new Separator());
            flyout.Items.Add(CreateRadioMenuItem("Small icons", "SizeGroup", isChecked: false, output: null));
            flyout.Items.Add(CreateRadioMenuItem("Medium icons", "SizeGroup", isChecked: true, output: null));
            flyout.Items.Add(CreateRadioMenuItem("Large icons", "SizeGroup", isChecked: false, output: null));
            Mux.FlyoutService.SetFlyout(button, flyout);
            panel.Children.Add(button);
            return panel;
        }

        private static UIElement CreateSwipeControlSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("SwipeControl"));
            panel.Children.Add(CreateSwipeControlRightExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateSwipeControlExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Swipe right to reveal actions",
                    CreateSwipeControlRightExampleContent(assignRootAutomationId: true),
                    SwipeControlRightXaml,
                    null),
                new GalleryExample(
                    "Swipe left to invoke an execute",
                    CreateSwipeControlLeftExecuteExampleContent(),
                    SwipeControlLeftExecuteXaml,
                    null),
                new GalleryExample(
                    "Custom Swipe in a ListView",
                    CreateSwipeControlListViewExampleContent(),
                    SwipeControlListViewXaml,
                    null),
                new GalleryExample(
                    "Gradient Background",
                    CreateSwipeControlGradientExampleContent(),
                    SwipeControlGradientXaml,
                    null),
                new GalleryExample(
                    "Custom icons",
                    CreateSwipeControlCustomIconsExampleContent(),
                    SwipeControlCustomIconsXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateSwipeControlExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("SwipeControl"));
            }

            return root;
        }

        private static GallerySamplePanel CreateSwipeControlRightExampleContent(bool assignRootAutomationId)
        {
            var panel = CreateSwipeControlExampleRoot(assignRootAutomationId);
            var text = CreateSwipeTextBlock("Swipe Right", 14);
            var swipeControl = CreateSwipeControl(text);
            GalleryAutomation.WithAutomationId(swipeControl, GalleryAutomation.SampleElementId("SwipeControl", "SwipeControl"));

            var accepted = false;
            var flagged = false;
            var accept = CreateSwipeItem("Accept", "\uE8FB");
            var flag = CreateSwipeItem("Flag", "\uE7C1");
            accept.Invoked += delegate
            {
                accepted = !accepted;
                UpdateSwipeControlRightState(text, accepted, flagged);
                accept.IconSource = CreateFontIconSource(accepted ? "\uE711" : "\uE10B");
                accept.Text = accepted ? "Cancel" : "Accept";
            };
            flag.Invoked += delegate
            {
                flagged = !flagged;
                UpdateSwipeControlRightState(text, accepted, flagged);
                flag.IconSource = CreateFontIconSource(flagged ? "\uEB4B" : "\uE129");
                flag.Text = flagged ? "Unmark" : "Flag";
            };
            swipeControl.LeftItems = CreateSwipeItems(Mux.SwipeMode.Reveal, accept, flag);

            panel.Children.Add(CreateSwipeControlHost(swipeControl));
            return panel;
        }

        private static GallerySamplePanel CreateSwipeControlLeftExecuteExampleContent()
        {
            var panel = CreateSwipeControlExampleRoot(assignRootAutomationId: false);
            var text = CreateSwipeTextBlock("Swipe Left", 14);
            var swipeControl = CreateSwipeControl(text);
            var archived = false;
            var archive = CreateSwipeItem("Archive", "\uE7B8");
            archive.BehaviorOnInvoked = Mux.SwipeBehaviorOnInvoked.Close;
            archive.Invoked += delegate
            {
                archived = !archived;
                text.Text = archived ? "Archived - Swipe Left" : "Swipe Left";
            };
            swipeControl.RightItems = CreateSwipeItems(Mux.SwipeMode.Execute, archive);
            panel.Children.Add(CreateSwipeControlHost(swipeControl));
            return panel;
        }

        private static GallerySamplePanel CreateSwipeControlListViewExampleContent()
        {
            var panel = CreateSwipeControlExampleRoot(assignRootAutomationId: false);
            var items = new ObservableCollection<string>
            {
                "Swipe Item 1",
                "Swipe Item 2",
                "Swipe Item 3",
                "Swipe Item 4"
            };

            var listView = new ListView
            {
                Name = "lv",
                Width = 800,
                Height = 300,
                MinWidth = 200,
                Margin = new Thickness(12),
                ItemsSource = items,
                ItemTemplate = CreateSwipeControlListViewItemTemplate(items),
                ItemContainerStyle = CreateSwipeControlListViewItemContainerStyle()
            };
            GalleryAutomation.WithAutomationId(listView, GalleryAutomation.SampleElementId("SwipeControl", "ListView"));
            panel.Children.Add(listView);
            return panel;
        }

        private static GallerySamplePanel CreateSwipeControlGradientExampleContent()
        {
            var panel = CreateSwipeControlExampleRoot(assignRootAutomationId: false);
            var swipeControl = CreateSwipeControl(CreateSwipeTextBlock("Swipe Left", 14));
            var lockItem = CreateSwipeItem("Lock", "\uE72E");
            lockItem.BehaviorOnInvoked = Mux.SwipeBehaviorOnInvoked.Close;
            lockItem.Background = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString("#ff8990f9"), 0.0),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#ff5b66fb"), 0.5),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#ff5c1df4"), 1.0)
                },
                new Point(0, 0.5),
                new Point(1, 0.5));
            swipeControl.RightItems = CreateSwipeItems(Mux.SwipeMode.Execute, lockItem);
            panel.Children.Add(CreateSwipeControlHost(swipeControl));
            return panel;
        }

        private static GallerySamplePanel CreateSwipeControlCustomIconsExampleContent()
        {
            var panel = CreateSwipeControlExampleRoot(assignRootAutomationId: false);
            var swipeControl = CreateSwipeControl(CreateSwipeTextBlock("Swipe Right", 14));
            var coffee = new Mux.SwipeItem
            {
                Text = "Coffee",
                IconSource = new Mux.BitmapIconSource
                {
                    UriSource = new Uri(ResourceUri("Assets/SampleMedia/CoffeeCup.png"), UriKind.Absolute)
                }
            };
            swipeControl.LeftItems = CreateSwipeItems(Mux.SwipeMode.Reveal, coffee);
            panel.Children.Add(CreateSwipeControlHost(swipeControl));
            return panel;
        }

        private static Border CreateSwipeControlHost(Mux.SwipeControl swipeControl)
        {
            var host = new Border
            {
                Width = 500,
                Height = 68,
                Margin = new Thickness(12),
                BorderThickness = new Thickness(1),
                Child = swipeControl
            };
            host.SetResourceReference(Border.BorderBrushProperty, "ButtonBackground");
            return host;
        }

        private static Mux.SwipeControl CreateSwipeControl(TextBlock content)
        {
            return new Mux.SwipeControl
            {
                Content = content,
                DataContext = content.Text
            };
        }

        private static TextBlock CreateSwipeTextBlock(string text, double fontSize)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(12),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = fontSize
            };
        }

        private static Mux.SwipeItem CreateSwipeItem(string text, string glyph)
        {
            return new Mux.SwipeItem
            {
                Text = text,
                IconSource = CreateFontIconSource(glyph)
            };
        }

        private static Mux.FontIconSource CreateFontIconSource(string glyph)
        {
            return new Mux.FontIconSource
            {
                Glyph = glyph
            };
        }

        private static Mux.SwipeItems CreateSwipeItems(Mux.SwipeMode mode, params Mux.SwipeItem[] items)
        {
            var swipeItems = new Mux.SwipeItems
            {
                Mode = mode
            };
            for (var i = 0; i < items.Length; i++)
            {
                swipeItems.Add(items[i]);
            }

            return swipeItems;
        }

        private static DataTemplate CreateSwipeControlListViewItemTemplate(ObservableCollection<string> items)
        {
            var template = new DataTemplate();
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
            border.SetResourceReference(Border.BorderBrushProperty, "ButtonBackground");

            var swipeControl = new FrameworkElementFactory(typeof(Mux.SwipeControl));
            swipeControl.SetValue(FrameworkElement.NameProperty, "ListViewSwipeContainer");
            swipeControl.SetValue(FrameworkElement.HeightProperty, 68.0);
            swipeControl.SetValue(FrameworkElement.MinWidthProperty, 200.0);
            swipeControl.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(delegate(object sender, RoutedEventArgs args)
            {
                var loadedSwipeControl = (Mux.SwipeControl)sender;
                if (loadedSwipeControl.LeftItems != null || loadedSwipeControl.RightItems != null)
                {
                    return;
                }

                var delete = CreateSwipeItem("Delete", "\uE74D");
                delete.Background = Brushes.Red;
                delete.Invoked += delegate
                {
                    if (loadedSwipeControl.DataContext is string item)
                    {
                        items.Remove(item);
                    }
                };
                loadedSwipeControl.LeftItems = CreateSwipeItems(
                    Mux.SwipeMode.Reveal,
                    new Mux.SwipeItem
                    {
                        Text = "Reply All",
                        IconSource = CreateFontIconSource("\uE8C2"),
                        Background = (Brush)new BrushConverter().ConvertFromString("#FF3e6fa7"),
                        Foreground = Brushes.White
                    },
                    new Mux.SwipeItem
                    {
                        Text = "Open",
                        IconSource = CreateFontIconSource("\uE8C3"),
                        Background = (Brush)new BrushConverter().ConvertFromString("#FFff9501"),
                        Foreground = Brushes.White
                    });
                loadedSwipeControl.RightItems = CreateSwipeItems(Mux.SwipeMode.Execute, delete);
            }));

            var textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding());
            textBlock.SetValue(FrameworkElement.MarginProperty, new Thickness(12));
            textBlock.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            textBlock.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            textBlock.SetValue(TextBlock.FontSizeProperty, 24.0);
            swipeControl.AppendChild(textBlock);
            border.AppendChild(swipeControl);
            template.VisualTree = border;
            return template;
        }

        private static Style CreateSwipeControlListViewItemContainerStyle()
        {
            var style = new Style(typeof(ListViewItem));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0)));
            style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));
            return style;
        }

        private static void UpdateSwipeControlRightState(TextBlock textBlock, bool accepted, bool flagged)
        {
            if (accepted && !flagged)
            {
                textBlock.Text = "Swipe Right - Accepted";
            }
            else if (accepted && flagged)
            {
                textBlock.Text = "Swipe Right - Accepted & Flagged";
            }
            else if (!accepted && flagged)
            {
                textBlock.Text = "Swipe Right - Flagged";
            }
            else
            {
                textBlock.Text = "Swipe Right";
            }
        }

        private static CommandListItemData FindCommandListItem(ObservableCollection<CommandListItemData> collection, string text)
        {
            for (var i = 0; i < collection.Count; i++)
            {
                if (string.Equals(collection[i].Text, text, StringComparison.Ordinal))
                {
                    return collection[i];
                }
            }

            return null;
        }

        private sealed class CommandListItemData
        {
            public CommandListItemData(string text, ICommand command)
            {
                Text = text;
                Command = command;
            }

            public string Text { get; }

            public ICommand Command { get; }
        }

        private sealed class DelegateCommand : ICommand
        {
            private readonly Action<object> _execute;

            public DelegateCommand(Action<object> execute)
            {
                _execute = execute;
            }

            public event EventHandler CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }
        }

        private static Mux.AppBarButton CreateAppBarButton(Mux.Symbol symbol, string label)
        {
            return new Mux.AppBarButton
            {
                Icon = new Mux.SymbolIcon(symbol),
                Label = label
            };
        }

        private static Mux.AppBarButton CreateAppBarSeparatorButton(Mux.Symbol symbol, string label)
        {
            var button = CreateAppBarButton(symbol, label);
            button.Width = 68;
            return button;
        }

        private static Mux.AppBarButton CreateSourceAppBarButton(string name, Mux.Symbol symbol, string label, TextBlock output)
        {
            return CreateSourceAppBarButton(name, new Mux.SymbolIcon(symbol), label, output);
        }

        private static Mux.AppBarButton CreateSourceAppBarButton(string name, Mux.IconElement icon, string label, TextBlock output)
        {
            var button = new Mux.AppBarButton
            {
                Name = name,
                Icon = icon,
                Label = label
            };
            button.Click += delegate { output.Text = "You clicked: " + name; };
            return button;
        }

        private static Mux.AppBarToggleButton CreateSourceAppBarToggleButton(string name, Mux.Symbol symbol, string label, TextBlock output)
        {
            return CreateSourceAppBarToggleButton(name, new Mux.SymbolIcon(symbol), label, output);
        }

        private static Mux.AppBarToggleButton CreateSourceAppBarToggleButton(string name, Mux.IconElement icon, string label, TextBlock output)
        {
            var button = new Mux.AppBarToggleButton
            {
                Name = name,
                Icon = icon,
                Label = label
            };
            button.Click += delegate { output.Text = "IsChecked = " + button.IsChecked.ToString(); };
            return button;
        }

        private static TextBlock CreateAppBarOutput(string name)
        {
            return new TextBlock
            {
                Name = name,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static Mux.AppBarButton CreateAppBarButton(Mux.Symbol symbol, string label, TextBlock output)
        {
            var button = CreateAppBarButton(symbol, label);
            button.Click += delegate { output.Text = label + " selected."; };
            return button;
        }

        private static Mux.AppBarButton CreateCommandBarAppBarButton(Mux.Symbol symbol, string label, TextBlock output, string inputGestureText)
        {
            var button = new Mux.AppBarButton
            {
                Icon = new Mux.SymbolIcon(symbol),
                Label = label,
                InputGestureText = inputGestureText
            };
            button.Click += delegate { output.Text = "You clicked: " + label; };
            return button;
        }

        private static Mux.AppBarButton CreateCommandBarFlyoutAppBarButton(Mux.Symbol symbol, string label, TextBlock output, string toolTip)
        {
            var button = CreateCommandBarFlyoutAppBarButton(label, output);
            button.Icon = new Mux.SymbolIcon(symbol);
            button.ToolTip = toolTip;
            return button;
        }

        private static Mux.AppBarButton CreateCommandBarFlyoutAppBarButton(string label, TextBlock output)
        {
            var button = new Mux.AppBarButton
            {
                Label = label
            };
            button.Click += delegate { output.Text = "You clicked: " + label; };
            return button;
        }

        private static MenuItem CreateMenuItem(string header, TextBlock output)
        {
            return CreateMenuItem(header, output, inputGestureText: null);
        }

        private static Mux.MenuBar CreateMenuBar()
        {
            return new Mux.MenuBar
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                MinWidth = 158
            };
        }

        private static MenuItem CreateSortMenuFlyoutItem(string header, string tag, TextBlock output)
        {
            var item = new MenuItem
            {
                Header = header,
                Tag = tag
            };
            item.Click += delegate { output.Text = "Sort by: " + tag; };
            return item;
        }

        private static MenuItem CreateSplitMenuFlyoutItem(string header, TextBlock output, UIElement icon)
        {
            var item = new MenuItem
            {
                Header = header,
                Icon = icon
            };
            item.Click += delegate(object sender, RoutedEventArgs args)
            {
                if (ReferenceEquals(args.OriginalSource, sender))
                {
                    output.Text = "Clicked: " + header;
                }
            };
            return item;
        }

        private static MenuItem CreateIconMenuFlyoutItem(string header, UIElement icon, string inputGestureText)
        {
            return new MenuItem
            {
                Header = header,
                Icon = icon,
                InputGestureText = inputGestureText
            };
        }

        private static MenuItem CreateMenuItem(string header, TextBlock output, string inputGestureText)
        {
            var item = new MenuItem
            {
                Header = header,
                InputGestureText = inputGestureText
            };
            item.Click += delegate { output.Text = "You clicked: " + header; };
            return item;
        }

        private static Mux.RadioMenuItem CreateRadioMenuItem(string header, string groupName, bool isChecked, TextBlock output)
        {
            var item = new Mux.RadioMenuItem
            {
                Header = header,
                GroupName = groupName,
                IsChecked = isChecked
            };
            if (output != null)
            {
                item.Click += delegate { output.Text = "You clicked: " + header; };
            }
            return item;
        }

        private static TextBlock CreateSelectionOutput(string name)
        {
            return new TextBlock
            {
                Name = name,
                Text = string.Empty,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static string FindSnippetText(IReadOnlyList<SampleSnippet> snippets, string title)
        {
            for (var i = 0; i < snippets.Count; i++)
            {
                if (string.Equals(snippets[i].Title, title, StringComparison.Ordinal))
                {
                    return snippets[i].Text;
                }
            }

            return null;
        }

        private static Button CreateSmallButton(string text, TextBlock output)
        {
            var button = new Button
            {
                Content = text,
                Padding = new Thickness(12, 5, 12, 5),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            button.Click += delegate { output.Text = text + " selected."; };
            return button;
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Content = text,
                Padding = new Thickness(16, 6, 16, 6),
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            panel.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return panel;
        }

        private static TextBlock CreateOutput(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static string ResourceUri(string path)
        {
            return "pack://application:,,,/ModernWpf.Gallery;component/" + path;
        }
    }
}

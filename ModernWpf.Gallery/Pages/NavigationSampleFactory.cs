using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Testing;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class NavigationSampleFactory
    {
        private const string BreadcrumbBarSimpleXaml =
@"<BreadcrumbBar x:Name=""BreadcrumbBar1""/>";

        private const string BreadcrumbBarSimpleCSharp =
@"BreadcrumbBar1.ItemsSource = new string[] { ""Home"", ""Documents"", ""Design"", ""Northwind"", ""Images"", ""Folder1"", ""Folder2"", ""Folder3"" };";

        private const string BreadcrumbBarTemplateXaml =
@"<BreadcrumbBar x:Name=""BreadcrumbBar2"">
    <BreadcrumbBar.ItemTemplate>
        <DataTemplate x:DataType=""l:Folder"">
            <BreadcrumbBarItem Content=""{Binding}"" AutomationProperties.Name=""{Binding Name}"">
                <BreadcrumbBarItem.ContentTemplate>
                    <DataTemplate>
                        <TextBlock Text=""{Binding Name}"" />
                    </DataTemplate>
                </BreadcrumbBarItem.ContentTemplate>
            </BreadcrumbBarItem>
        </DataTemplate>
    </BreadcrumbBar.ItemTemplate>
</BreadcrumbBar>";

        private const string BreadcrumbBarTemplateCSharp =
@"public class Folder
{
    public string Name { get; set; }
}

BreadcrumbBar2.ItemsSource = new ObservableCollection<Folder>{
        new Folder { Name = ""Home""},
        new Folder { Name = ""Folder1"" },
        new Folder { Name = ""Folder2"" },
        new Folder { Name = ""Folder3"" },
};
BreadcrumbBar2.ItemClicked += BreadcrumbBar2_ItemClicked;

private void BreadcrumbBar2_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
{
    var items = BreadcrumbBar2.ItemsSource as ObservableCollection<Folder>;
    for (int i = items.Count - 1; i >= args.Index + 1; i--)
    {
        items.RemoveAt(i);
    }
}";

        private const string SelectorBarBasicXaml =
@"<SelectorBar x:Name=""SelectorBar1"">
    <SelectorBarItem x:Name=""SelectorBarItemRecent"" Text=""Recent"" Icon=""Clock"" />
    <SelectorBarItem x:Name=""SelectorBarItemShared"" Text=""Shared"" Icon=""Share"" />
    <SelectorBarItem x:Name=""SelectorBarItemFavorites"" Text=""Favorites"" Icon=""Favorite"" />
</SelectorBar>";

        private const string SelectorBarFrameXaml =
@"<SelectorBar x:Name=""SelectorBar2"" SelectionChanged=""SelectorBar2_SelectionChanged"">
    <SelectorBarItem x:Name=""SelectorBarItemPage1"" Text=""Page1"" IsSelected=""True"" />
    <SelectorBarItem x:Name=""SelectorBarItemPage2"" Text=""Page2"" />
    <SelectorBarItem x:Name=""SelectorBarItemPage3"" Text=""Page3"" />
    <SelectorBarItem x:Name=""SelectorBarItemPage4"" Text=""Page4"" />
    <SelectorBarItem x:Name=""SelectorBarItemPage5"" Text=""Page5"" />
</SelectorBar>

<Frame x:Name=""ContentFrame"" IsNavigationStackEnabled=""False"" />";

        private const string SelectorBarFrameCSharp =
@"private void SelectorBar2_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            System.Type pageType;

            switch (currentSelectedIndex)
            {
                case 0:
                    pageType = typeof(SamplePage1);
                    break;
                case 1:
                    pageType = typeof(SamplePage2);
                    break;
                case 2:
                    pageType = typeof(SamplePage3);
                    break;
                case 3:
                    pageType = typeof(SamplePage4);
                    break;
                default:
                    pageType = typeof(SamplePage5);
                    break;
            }

            var slideNavigationTransitionEffect = currentSelectedIndex - previousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;

            ContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = slideNavigationTransitionEffect });

            previousSelectedIndex = currentSelectedIndex;
        }";

        private const string SelectorBarItemsViewXaml =
@"<SelectorBar x:Name=""SelectorBar3"" SelectionChanged=""SelectorBar3_SelectionChanged"" >
    <SelectorBarItem x:Name=""SelectorBarItemPink"" Text=""Pink"" IsSelected=""True"" />
    <SelectorBarItem x:Name=""SelectorBarItemPlum"" Text=""Plum"" />
    <SelectorBarItem x:Name=""SelectorBarItemPowderBlue"" Text=""PowderBlue"" />
</SelectorBar>

<ItemsView x:Name=""ItemsView3"" ItemTemplate=""{StaticResource ColorsTemplate}"" />
    <ItemsView.Layout>
        <UniformGridLayout />
    </ItemsView.Layout>
</ItemsView/>";

        private const string SelectorBarItemsViewCSharp =
@"private void SelectorBar3_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
{
    if (sender.SelectedItem == SelectorBarItemPink)
    {
        ItemsView3.ItemsSource = PinkColorCollection;
    }
    else if (sender.SelectedItem == SelectorBarItemPlum)
    {
        ItemsView3.ItemsSource = PlumColorCollection;
    }
    else
    {
        ItemsView3.ItemsSource = PowderBlueColorCollection;
    }
}";

        private const string PivotBasicXaml =
@"<Pivot Title=""EMAIL"">
    <PivotItem Header=""All"">
        <TextBlock Text=""all emails go here."" />
    </PivotItem>
    <PivotItem Header=""Unread"">
        <TextBlock Text=""unread emails go here."" />
    </PivotItem>
    <PivotItem Header=""Flagged"">
        <TextBlock Text=""flagged emails go here."" />
    </PivotItem>
    <PivotItem Header=""Urgent"">
        <TextBlock Text=""urgent emails go here."" />
    </PivotItem>
</Pivot>";

        private const double PivotReferenceWidth = 721.0;

        private const double TabViewReferenceWidth = 767.0;

        private const string SamplePageLoremIpsum =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        private const string TabViewBasicXaml =
@"<TabView AddTabButtonClick=""TabView_AddButtonClick"" TabCloseRequested=""TabView_TabCloseRequested"" Loaded=""TabView_Loaded"" />";

        private const string TabViewMarkupXaml =
@"<TabView AddTabButtonClick=""TabView_AddButtonClick"" TabCloseRequested=""TabView_TabCloseRequested"">
    <TabView.TabItems>
        <TabViewItem Header=""Document 0"">
            <TabViewItem.IconSource>
                <SymbolIconSource Symbol=""Placeholder"" />
            </TabViewItem.IconSource>
            <samplepages:SamplePage1 />
        </TabViewItem>
        <TabViewItem Header=""Document 1"">
            <TabViewItem.IconSource>
                <SymbolIconSource Symbol=""Placeholder"" />
            </TabViewItem.IconSource>
            <samplepages:SamplePage2 />
        </TabViewItem>
        <TabViewItem Header=""Document 2"">
            <TabViewItem.IconSource>
                <SymbolIconSource Symbol=""Placeholder"" />
            </TabViewItem.IconSource>
            <samplepages:SamplePage3 />
        </TabViewItem>
    </TabView.TabItems>
</TabView>";

        private const string TabViewItemsSourceXaml =
@"<TabView TabItemsSource=""{x:Bind myDatas, Mode=OneWay}"" AddTabButtonClick=""TabViewItemsSourceSample_AddTabButtonClick"" TabCloseRequested=""TabViewItemsSourceSample_TabCloseRequested"" />";

        private const string TabViewKeyboardingXaml =
@"<TabView AddTabButtonClick=""TabView_AddButtonClick"" TabCloseRequested=""TabView_TabCloseRequested"" Loaded=""TabView_Loaded"">
    <TabView.KeyboardAccelerators>
        <KeyboardAccelerator Key=""T"" Modifiers=""Control"" Invoked=""NewTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""W"" Modifiers=""Control"" Invoked=""CloseSelectedTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""Number1"" Modifiers=""Control"" Invoked=""NavigateToNumberedTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""Number2"" Modifiers=""Control"" Invoked=""NavigateToNumberedTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""Number3"" Modifiers=""Control"" Invoked=""NavigateToNumberedTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""Number4"" Modifiers=""Control"" Invoked=""NavigateToNumberedTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""Number5"" Modifiers=""Control"" Invoked=""NavigateToNumberedTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""Number6"" Modifiers=""Control"" Invoked=""NavigateToNumberedTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""Number7"" Modifiers=""Control"" Invoked=""NavigateToNumberedTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""Number8"" Modifiers=""Control"" Invoked=""NavigateToNumberedTabKeyboardAccelerator_Invoked"" />
        <KeyboardAccelerator Key=""Number9"" Modifiers=""Control"" Invoked=""NavigateToNumberedTabKeyboardAccelerator_Invoked"" />
    </TabView.KeyboardAccelerators>
</TabView>";

        private const string TabViewHeaderFooterXaml =
@"<TabView>
    <TabView.TabStripHeader>
        <TextBlock Text=""TabStripHeader Content"" VerticalAlignment=""Center"" Margin=""8,6"" Style=""{ThemeResource BaseTextBlockStyle}"" />
    </TabView.TabStripHeader>
    <TabView.TabStripFooter>
        <TextBlock Text=""TabStripFooter Content"" VerticalAlignment=""Center"" HorizontalAlignment=""Right"" Margin=""6"" Style=""{ThemeResource BaseTextBlockStyle}"" />
    </TabView.TabStripFooter>
</TabView>";

        private const string TabViewWidthModeXaml =
@"<TabView TabWidthMode=""$(TabWidthMode)"" />";

        private const string TabViewCloseButtonXaml =
@"<TabView CloseButtonOverlayMode=""$(CloseButtonOverlayMode)"" />";

        private const string TabViewColorIconsXaml =
@"<TabView>
    <TabView.TabItems>
        <TabViewItem Header=""CMD Prompt"">
            <TabViewItem.IconSource>
                <BitmapIconSource UriSource=""/Assets/SampleMedia/cmd.png"" ShowAsMonochrome=""False"" />
            </TabViewItem.IconSource>
        </TabViewItem>
        <TabViewItem Header=""PowerShell"">
            <TabViewItem.IconSource>
                <BitmapIconSource UriSource=""/Assets/SampleMedia/powershell.png"" ShowAsMonochrome=""False"" />
            </TabViewItem.IconSource>
        </TabViewItem>
        <TabViewItem Header=""Windows Subsystem for Linux"">
            <TabViewItem.IconSource>
                <BitmapIconSource UriSource=""/Assets/SampleMedia/linux.png"" ShowAsMonochrome=""False"" />
            </TabViewItem.IconSource>
        </TabViewItem>
    </TabView.TabItems>
</TabView>";

        private const string TabViewAccentBackgroundXaml =
@"<TabView>
    <TabView.Resources>
        <ResourceDictionary>
            <ResourceDictionary.ThemeDictionaries>
                <ResourceDictionary x:Key=""Light"">
                    <SolidColorBrush x:Key=""TabViewBackground"" Color=""{ThemeResource SystemAccentColorLight2}""/>
                </ResourceDictionary>
                <ResourceDictionary x:Key=""Dark"">
                    <SolidColorBrush x:Key=""TabViewBackground"" Color=""{ThemeResource SystemAccentColorDark2}""/>
                </ResourceDictionary>
            </ResourceDictionary.ThemeDictionaries>
        </ResourceDictionary>
    </TabView.Resources>
</TabView>";

        private const string TabViewWindowingXaml =
@"Check out the TabViewWindowingSamplePage.xaml and *.cs files to see the complete code.";

        private const string NavigationViewApiXaml =
@"<NavigationView x:Name=""nvSample""
    IsSettingsVisible=""$(SettingsVis)""
    IsBackButtonVisible=""$(BackButtonVis)""
    IsBackEnabled=""$(BackButtonEn)""
    SelectionChanged=""NavigationView_SelectionChanged""
    Header=""$(HeaderText)""
    AlwaysShowHeader=""$(ShowHeader)""
    PaneTitle=""$(PaneTitleText)""
    PaneDisplayMode=""$(PaneDisplayMode)""
    ExpandedModeThresholdWidth=""500""
    SelectionFollowsFocus=""$(SelectionFollowsFocus)""
    IsTabStop=""False"">

    <NavigationView.MenuItems>
        <NavigationViewItem Content=""Menu Item1"" Tag=""SamplePage1"" x:Name=""SamplePage1Item"">
            <NavigationViewItem.Icon>
                <SymbolIcon Symbol=""Play"" />
            </NavigationViewItem.Icon>
        </NavigationViewItem>
        <NavigationViewItemHeader Content=""Actions""/>
        <NavigationViewItem Content=""Menu Item2"" Tag=""SamplePage2"" x:Name=""SamplePage2Item"" SelectsOnInvoked=""$(SelectsOnInvoked)"">
            <NavigationViewItem.Icon>
                <SymbolIcon Symbol=""Save"" />
            </NavigationViewItem.Icon>
        </NavigationViewItem>
        <NavigationViewItem Content=""Menu Item3"" Tag=""SamplePage3"" x:Name=""SamplePage3Item"">
            <NavigationViewItem.Icon>
                <SymbolIcon Symbol=""Refresh"" />
            </NavigationViewItem.Icon>
        </NavigationViewItem>
    </NavigationView.MenuItems>

    <NavigationView.PaneCustomContent>
        <HyperlinkButton x:Name=""PaneHyperlink"" Content=""More info"" Margin=""12,0"" Visibility=""$(PaneCustomContentVis)"" />
    </NavigationView.PaneCustomContent>
    $(NavViewASB)
    <NavigationView.PaneFooter>
        <StackPanel x:Name=""FooterStackPanel"" Orientation=""Vertical"" Visibility=""$(PaneFooterVis)"">
            <NavigationViewItem Icon=""Download"" AutomationProperties.Name=""download"" />
            <NavigationViewItem Icon=""Favorite"" AutomationProperties.Name=""favorite"" />
        </StackPanel>
    </NavigationView.PaneFooter>

    <Frame x:Name=""contentFrame"" />
</NavigationView>";

        private static readonly string[] BreadcrumbFoldersString =
        {
            "Home",
            "Documents",
            "Design",
            "Northwind",
            "Images",
            "Folder1",
            "Folder2",
            "Folder3"
        };

        private static readonly string[] SelectorBarPageColors =
        {
            "#E8F3FF",
            "#F2F2F2",
            "#FFF4CE",
            "#FDE7E9",
            "#E7F6E7"
        };

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "BreadcrumbBar":
                    return CreateBreadcrumbBarSample();
                case "Frame":
                    return CreateFrameSample();
                case "Menu":
                    return CreateMenuSample();
                case "NavigationView":
                    return CreateNavigationViewSample();
                case "NavigationWindow":
                    return CreateNavigationWindowSample();
                case "Pivot":
                    return CreatePivotSample();
                case "SelectorBar":
                    return CreateSelectorBarSample();
                case "TabControl":
                    return CreateTabControlSample();
                case "TabView":
                    return CreateTabViewSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            return CreateExamples(uniqueId, Array.Empty<SampleSnippet>());
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "BreadcrumbBar":
                    return CreateBreadcrumbBarExamples();
                case "NavigationView":
                    return CreateNavigationViewExamples(sampleSnippets);
                case "Pivot":
                    return CreatePivotExamples();
                case "SelectorBar":
                    return CreateSelectorBarExamples();
                case "TabView":
                    return CreateTabViewExamples(sampleSnippets);
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateFrameSample()
        {
            var panel = CreateSamplePanel("Frame hosts Page content and maintains navigation history.");
            var frame = new Frame
            {
                Width = 520,
                Height = 220,
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                Content = CreatePageContent("Home page", "#D9EAF7")
            };

            var commands = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var home = CreateButton("Home");
            var details = CreateButton("Details");
            var back = CreateButton("Back");
            home.Click += delegate { frame.Navigate(CreatePageContent("Home page", "#D9EAF7")); };
            details.Click += delegate { frame.Navigate(CreatePageContent("Details page", "#E6E6E6")); };
            back.Click += delegate
            {
                if (frame.CanGoBack)
                {
                    frame.GoBack();
                }
            };
            commands.Children.Add(home);
            commands.Children.Add(details);
            commands.Children.Add(back);

            panel.Children.Add(frame);
            panel.Children.Add(commands);
            return panel;
        }

        private static UIElement CreateMenuSample()
        {
            var panel = CreateSamplePanel("Menu presents top-level WPF commands with nested MenuItem entries and keyboard access.");
            var output = CreateOutput("Choose a menu command.");
            var menu = new Menu
            {
                Width = 420
            };
            var file = new MenuItem { Header = "_File" };
            file.Items.Add(CreateWpfMenuItem("_New", output));
            file.Items.Add(CreateWpfMenuItem("_Open", output));
            file.Items.Add(new Separator());
            file.Items.Add(CreateWpfMenuItem("E_xit", output));
            var edit = new MenuItem { Header = "_Edit" };
            edit.Items.Add(CreateWpfMenuItem("_Copy", output));
            edit.Items.Add(CreateWpfMenuItem("_Paste", output));
            menu.Items.Add(file);
            menu.Items.Add(edit);
            panel.Children.Add(menu);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateNavigationWindowSample()
        {
            var panel = CreateSamplePanel("NavigationWindow opens Page content in a top-level window with navigation support.");
            var output = CreateOutput("Window not opened yet.");
            var open = CreateButton("Open NavigationWindow");
            open.Click += delegate
            {
                var owner = Window.GetWindow((FrameworkElement)open);
                var window = new System.Windows.Navigation.NavigationWindow
                {
                    Title = "NavigationWindow sample",
                    Width = 480,
                    Height = 320,
                    Content = CreatePageContent("NavigationWindow page", "#D9EAF7")
                };
                if (owner != null)
                {
                    window.Owner = owner;
                }
                window.Show();
                output.Text = "NavigationWindow opened.";
            };
            panel.Children.Add(open);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateTabControlSample()
        {
            var panel = CreateSamplePanel("TabControl presents multiple TabItem pages with one active selection.");
            var tabControl = new TabControl
            {
                Width = 520,
                Height = 220
            };
            tabControl.Items.Add(CreateTab("Overview", "Overview content"));
            tabControl.Items.Add(CreateTab("Details", "Details content"));
            tabControl.Items.Add(CreateTab("History", "History content"));
            tabControl.SelectedIndex = 0;
            panel.Children.Add(tabControl);
            return panel;
        }

        private static UIElement CreateBreadcrumbBarSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("BreadcrumbBar"));
            panel.Children.Add(CreateBreadcrumbBarSimpleExampleContent(false));
            panel.Children.Add(CreateBreadcrumbBarTemplateExampleContent());
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateBreadcrumbBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A BreadcrumbBar control",
                    CreateBreadcrumbBarSimpleExampleContent(true),
                    BreadcrumbBarSimpleXaml,
                    BreadcrumbBarSimpleCSharp),
                new GalleryExample(
                    "BreadCrumbBar Control with Custom DataTemplate",
                    CreateBreadcrumbBarTemplateExampleContent(),
                    BreadcrumbBarTemplateXaml,
                    BreadcrumbBarTemplateCSharp)
            };
        }

        private static UIElement CreateBreadcrumbBarSimpleExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("BreadcrumbBar"));
            }

            var breadcrumbBar = new Mux.BreadcrumbBar
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Name = "BreadcrumbBar1",
                ItemsSource = BreadcrumbFoldersString
            };
            GalleryAutomation.WithAutomationId(breadcrumbBar, GalleryAutomation.SampleElementId("BreadcrumbBar", "BreadcrumbBar"));

            root.Children.Add(breadcrumbBar);
            return root;
        }

        private static UIElement CreateBreadcrumbBarTemplateExampleContent()
        {
            var defaultFolders = new List<BreadcrumbFolder>
            {
                new BreadcrumbFolder { Name = "Home" },
                new BreadcrumbFolder { Name = "Folder1" },
                new BreadcrumbFolder { Name = "Folder2" },
                new BreadcrumbFolder { Name = "Folder3" }
            };
            var folders = new ObservableCollection<BreadcrumbFolder>(defaultFolders);
            var breadcrumbBar = new Mux.BreadcrumbBar
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Name = "BreadcrumbBar2",
                ItemsSource = folders,
                ItemTemplate = CreateBreadcrumbFolderTemplate()
            };
            breadcrumbBar.ItemClicked += delegate(Mux.BreadcrumbBar sender, Mux.BreadcrumbBarItemClickedEventArgs args)
            {
                if (!(sender.ItemsSource is ObservableCollection<BreadcrumbFolder> items))
                {
                    return;
                }

                for (var i = items.Count - 1; i >= args.Index + 1; i--)
                {
                    items.RemoveAt(i);
                }
            };

            var resetSampleButton = new Button
            {
                Name = "ResetSampleBtn",
                Content = "Reset sample"
            };
            resetSampleButton.Click += delegate
            {
                foreach (var folder in defaultFolders)
                {
                    if (!folders.Contains(folder))
                    {
                        folders.Add(folder);
                    }
                }
            };

            return CreateBreadcrumbBarExampleLayout(breadcrumbBar, resetSampleButton);
        }

        private static DataTemplate CreateBreadcrumbFolderTemplate()
        {
            var textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(BreadcrumbFolder.Name)));
            textBlock.SetBinding(AutomationProperties.NameProperty, new Binding(nameof(BreadcrumbFolder.Name)));

            return new DataTemplate(typeof(BreadcrumbFolder))
            {
                VisualTree = textBlock
            };
        }

        private static Grid CreateBreadcrumbBarExampleLayout(UIElement sample, UIElement options)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid.SetColumn(sample, 0);
            grid.Children.Add(sample);

            if (options != null)
            {
                var optionsHost = new Border
                {
                    Margin = new Thickness(24, 0, 0, 0),
                    Child = options
                };
                Grid.SetColumn(optionsHost, 1);
                grid.Children.Add(optionsHost);
            }

            return grid;
        }

        private static UIElement CreateNavigationViewSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("NavigationView"));
            panel.Children.Add(CreateNavigationViewDefaultExampleContent(false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateNavigationViewExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "NavigationView with default PaneDisplayMode",
                    CreateNavigationViewDefaultExampleContent(true),
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample1.txt"),
                    null),
                new GalleryExample(
                    "NavigationView with PaneDisplayMode set to Top",
                    CreateNavigationViewTopExampleContent(),
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample2.txt"),
                    null),
                new GalleryExample(
                    "NavigationView that switches pane orientation based on window width",
                    CreateNavigationViewAdaptiveExampleContent(),
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample3.txt"),
                    null),
                new GalleryExample(
                    "Tying selection and focus - Tabs",
                    CreateNavigationViewTabsExampleContent(),
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample4_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample4_cs.txt")),
                new GalleryExample(
                    "Data binding",
                    CreateNavigationViewDataBindingExampleContent(),
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample5_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample5_cs.txt")),
                new GalleryExample(
                    "NavigationView with Footer Menu Items",
                    CreateNavigationViewFooterExampleContent(),
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample9_xaml.txt"),
                    null),
                new GalleryExample(
                    "Hierarchical NavigationView",
                    CreateNavigationViewHierarchicalExampleContent(),
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample8_xaml.txt"),
                    null),
                new GalleryExample(
                    "API in action",
                    CreateNavigationViewApiExampleContent(),
                    NavigationViewApiXaml,
                    null)
            };
        }

        private static UIElement CreateNavigationViewDefaultExampleContent(bool assignRootAutomationId)
        {
            var root = CreateNavigationViewDescriptionRoot(
                "If you have five or more equally important navigation categories that should prominently appear on larger window widths, consider using a left navigation pane.",
                assignRootAutomationId);

            var navigationView = CreateNavigationViewShell(
                "nvSample5",
                "contentFrame5",
                Mux.NavigationViewPaneDisplayMode.Auto,
                "This is Header Text");
            GalleryAutomation.WithAutomationId(navigationView, GalleryAutomation.SampleElementId("NavigationView", "NavigationView"));
            AddStandardNavigationItems(navigationView, includeIcons: true, firstContent: "Menu Item1", remainingPrefix: "Menu Item");
            HookNavigationHeaderSelection(navigationView);
            SelectFirstNavigationItem(navigationView);
            navigationView.Header = "Sample Page 1";
            root.Children.Add(navigationView);
            return root;
        }

        private static UIElement CreateNavigationViewTopExampleContent()
        {
            var root = CreateNavigationViewDescriptionRoot(
                "If you have equally important navigation categories that should be de-emphasized relative to the content of your app, consider using a top navigation pane.",
                false);

            var navigationView = CreateNavigationViewShell(
                "nvSample6",
                "contentFrame6",
                Mux.NavigationViewPaneDisplayMode.Top,
                "This is Header Text");
            AddStandardNavigationItems(navigationView, includeIcons: false, firstContent: "Menu Item1", remainingPrefix: "Menu Item");
            SelectFirstNavigationItem(navigationView);
            root.Children.Add(navigationView);
            return root;
        }

        private static UIElement CreateNavigationViewAdaptiveExampleContent()
        {
            var root = CreateNavigationViewDescriptionRoot(
                "If you have equally important navigation categories and limited app content space, consider using a top navigation pane on larger window widths and a minimal left navigation pane on smaller window widths.",
                false);

            var navigationView = CreateNavigationViewShell(
                "nvSample2",
                "contentFrame2",
                Mux.NavigationViewPaneDisplayMode.Auto,
                null);
            AddStandardNavigationItems(navigationView, includeIcons: false, firstContent: "Menu Item1", remainingPrefix: "Menu Item");
            SelectFirstNavigationItem(navigationView);
            root.Children.Add(navigationView);
            return root;
        }

        private static UIElement CreateNavigationViewTabsExampleContent()
        {
            var root = CreateNavigationViewDescriptionRoot(
                "For the tabs pattern, ensure that you unify selection and focus by setting the SelectionFollowsFocus property to Enabled. If using a Frame to swap out content, then navigating between items shouldn't be recorded into the Frame's navigation stack. Please see the C# in the sample below to understand how to do this.",
                false);

            var navigationView = CreateNavigationViewShell(
                "nvSample7",
                "contentFrame7",
                Mux.NavigationViewPaneDisplayMode.Top,
                null);
            navigationView.IsBackButtonVisible = Mux.NavigationViewBackButtonVisible.Collapsed;
            navigationView.SelectionFollowsFocus = Mux.NavigationViewSelectionFollowsFocus.Enabled;
            AddStandardNavigationItems(navigationView, includeIcons: false, firstContent: "Item1", remainingPrefix: "Item");
            SelectFirstNavigationItem(navigationView);
            root.Children.Add(navigationView);
            return root;
        }

        private static UIElement CreateNavigationViewDataBindingExampleContent()
        {
            var root = CreateNavigationViewDescriptionRoot(
                "When data binding, use the MenuItemsSource property to bind to an observable collection of items, and do not set the MenuItems property. In addition, set the MenuItemTemplate property and use a NavigationViewItem as the data template. If you wish to bind to the header content as well, use data template selectors via the MenuItemTemplateSelector property. ",
                false);

            var categories = new ObservableCollection<NavigationCategory>
            {
                new NavigationCategory("Category 1", Mux.Symbol.Home, "This is category 1", "SamplePage1"),
                new NavigationCategory("Category 2", Mux.Symbol.Keyboard, "This is category 2", "SamplePage2"),
                new NavigationCategory("Category 3", Mux.Symbol.Library, "This is category 3", "SamplePage3"),
                new NavigationCategory("Category 4", Mux.Symbol.Mail, "This is category 4", "SamplePage4")
            };

            var navigationView = CreateNavigationViewShell(
                "nvSample4",
                "contentFrame4",
                Mux.NavigationViewPaneDisplayMode.Auto,
                null);
            navigationView.MenuItemsSource = categories;
            navigationView.MenuItemTemplate = CreateNavigationCategoryTemplate();
            navigationView.SelectedItem = categories[0];
            root.Children.Add(navigationView);
            return root;
        }

        private static UIElement CreateNavigationViewFooterExampleContent()
        {
            var root = CreateNavigationViewDescriptionRoot(
                "You can add clickable menu items to the footer of your NavigationView that participate in the same selection model as items in the main menu. In Top PaneDisplayMode, these items will appear aligned to the right of the NavigationView. In Left PaneDisplayMode, these items will appear aligned to the bottom of the NavigationView. ",
                false);

            var navigationView = CreateNavigationViewShell(
                "nvSample9",
                "contentFrame9",
                Mux.NavigationViewPaneDisplayMode.Left,
                "This is Header Text");
            navigationView.IsSettingsVisible = false;
            navigationView.MenuItems.Add(CreateNavigationItem("Browse", Mux.Symbol.Library, "SamplePage1"));
            navigationView.MenuItems.Add(CreateNavigationItem("Track an Order", Mux.Symbol.Map, "SamplePage2"));
            navigationView.MenuItems.Add(CreateNavigationItem("Order History", Mux.Symbol.Tag, "SamplePage3"));
            navigationView.FooterMenuItems.Add(CreateNavigationItem("Account", Mux.Symbol.Contact, "SamplePage4"));
            navigationView.FooterMenuItems.Add(CreateNavigationItem("Your Cart", Mux.Symbol.Shop, "SamplePage5"));
            navigationView.FooterMenuItems.Add(CreateNavigationItem("Help", Mux.Symbol.Help, "SamplePage5"));
            SelectFirstNavigationItem(navigationView);
            root.Children.Add(navigationView);

            return CreateNavigationViewExampleLayout(
                root,
                CreateNavigationPanePositionOptions(
                    "Pane position:",
                    "nvSample9Left",
                    "nvSample9Top",
                    null,
                    navigationView));
        }

        private static UIElement CreateNavigationViewHierarchicalExampleContent()
        {
            var root = CreateNavigationViewDescriptionRoot(null, false);
            root.Children.Add(new TextBlock
            {
                Text = "NavigationView supports hierarchy in Left, LeftCompact, and Top display modes.\n\nIn the example below, the \"Account\" tab navigates to its own page while \"Document options\" only opens up its subtree of items. This is done by setting the SelectsOnInvoked property to false on the Document options NavigationView Item.\n\nIn both Top and Left modes, clicking the arrows on NavigationViewItems will expand or collapse the subtree. Clicking or tapping elsewhere on the NavigationViewItem will collapse or expand the subtree.\n\nSwitch between the three pane display modes on the right.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15)
            });

            var navigationView = CreateNavigationViewShell(
                "nvSample8",
                "contentFrame8",
                Mux.NavigationViewPaneDisplayMode.Left,
                null);

            var home = CreateNavigationItem("Home", Mux.Symbol.Home, "SamplePage1");
            home.ToolTip = "Home";
            var account = CreateNavigationItem("Account", Mux.Symbol.Contact, "SamplePage2");
            account.ToolTip = "Account";
            account.MenuItems.Add(CreateToolTippedNavigationItem("Mail", Mux.Symbol.Mail, "SamplePage3"));
            account.MenuItems.Add(CreateToolTippedNavigationItem("Calendar", Mux.Symbol.Calendar, "SamplePage4"));

            var documentOptions = CreateNavigationItem("Document options", Mux.Symbol.Page2, null);
            documentOptions.ToolTip = "Document options";
            documentOptions.SelectsOnInvoked = false;
            documentOptions.MenuItems.Add(CreateToolTippedNavigationItem("Create new", Mux.Symbol.NewFolder, "SamplePage5"));
            documentOptions.MenuItems.Add(CreateToolTippedNavigationItem("Upload file", Mux.Symbol.OpenLocal, "SamplePage6"));

            navigationView.MenuItems.Add(home);
            navigationView.MenuItems.Add(account);
            navigationView.MenuItems.Add(documentOptions);
            SelectFirstNavigationItem(navigationView);
            root.Children.Add(navigationView);

            return CreateNavigationViewExampleLayout(
                root,
                CreateNavigationPanePositionOptions(
                    "PanePosition:",
                    "nvSample8Left",
                    "nvSample8Top",
                    "nvSample8LeftCompact",
                    navigationView));
        }

        private static UIElement CreateNavigationViewApiExampleContent()
        {
            var navigationView = CreateNavigationViewShell(
                "nvSample",
                "contentFrame",
                Mux.NavigationViewPaneDisplayMode.Left,
                "Header");
            navigationView.Height = 540;
            navigationView.Margin = new Thickness(0, 12, 0, 0);
            navigationView.ExpandedModeThresholdWidth = 500;
            navigationView.PaneTitle = "Pane Title";
            navigationView.IsBackButtonVisible = Mux.NavigationViewBackButtonVisible.Visible;
            navigationView.AutoSuggestBox = CreateNavigationAutoSuggestBox();

            var samplePage1Item = CreateNavigationItem("Menu Item1", Mux.Symbol.Play, "SamplePage1");
            samplePage1Item.Name = "SamplePage1Item";
            var samplePage2Item = CreateNavigationItem("Menu Item2", Mux.Symbol.Save, "SamplePage2");
            samplePage2Item.Name = "SamplePage2Item";
            var samplePage3Item = CreateNavigationItem("Menu Item3", Mux.Symbol.Refresh, "SamplePage3");
            samplePage3Item.Name = "SamplePage3Item";

            var paneHyperlink = new Mux.HyperlinkButton
            {
                Name = "PaneHyperlink",
                Content = "More info",
                Margin = new Thickness(12, 0, 0, 0),
                Visibility = Visibility.Collapsed
            };

            var footerStackPanel = new StackPanel
            {
                Name = "FooterStackPanel",
                Orientation = Orientation.Vertical,
                Visibility = Visibility.Collapsed
            };
            footerStackPanel.Children.Add(CreateNavigationItem(null, Mux.Symbol.Download, null, "download"));
            footerStackPanel.Children.Add(CreateNavigationItem(null, Mux.Symbol.Favorite, null, "favorite"));

            navigationView.MenuItems.Add(samplePage1Item);
            navigationView.MenuItems.Add(new Mux.NavigationViewItemHeader { Content = "Actions" });
            navigationView.MenuItems.Add(samplePage2Item);
            navigationView.MenuItems.Add(samplePage3Item);
            navigationView.PaneCustomContent = paneHyperlink;
            navigationView.PaneFooter = footerStackPanel;
            HookNavigationHeaderSelection(navigationView);

            return CreateNavigationViewExampleLayout(
                navigationView,
                CreateNavigationViewApiOptions(
                    navigationView,
                    paneHyperlink,
                    footerStackPanel,
                    samplePage2Item));
        }

        private static UIElement CreatePivotSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("Pivot"));
            panel.Children.Add(CreatePivotBasicExampleContent(false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreatePivotExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A basic pivot.",
                    CreatePivotBasicExampleContent(true),
                    PivotBasicXaml,
                    null)
            };
        }

        private static UIElement CreatePivotBasicExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("Pivot"));
            }

            var pivot = new TabControl
            {
                Name = "Pivot1",
                MinHeight = 400,
                MaxWidth = PivotReferenceWidth,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Style = FindStyleResource("TabControlPivotStyle")
            };
            GalleryAutomation.WithAutomationId(pivot, GalleryAutomation.SampleElementId("Pivot", "Pivot"));
            PivotHelper.SetTitle(pivot, "EMAIL");
            pivot.Items.Add(CreatePivotItem("All", "all emails go here."));
            pivot.Items.Add(CreatePivotItem("Unread", "unread emails go here."));
            pivot.Items.Add(CreatePivotItem("Flagged", "flagged emails go here."));
            pivot.Items.Add(CreatePivotItem("Urgent", "urgent emails go here."));
            pivot.SelectedIndex = 0;
            root.Children.Add(pivot);
            return root;
        }

        private static UIElement CreateSelectorBarSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("SelectorBar"));
            panel.Children.Add(CreateSelectorBarBasicExampleContent(false));
            panel.Children.Add(CreateSelectorBarFrameExampleContent());
            panel.Children.Add(CreateSelectorBarItemsViewExampleContent());
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateSelectorBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A Basic SelectorBar",
                    CreateSelectorBarBasicExampleContent(true),
                    SelectorBarBasicXaml,
                    null),
                new GalleryExample(
                    "SelectorBar with Frame Slide Transitions",
                    CreateSelectorBarFrameExampleContent(),
                    SelectorBarFrameXaml,
                    SelectorBarFrameCSharp),
                new GalleryExample(
                    "SelectorBar Displaying Different Collections Using ItemsView",
                    CreateSelectorBarItemsViewExampleContent(),
                    SelectorBarItemsViewXaml,
                    SelectorBarItemsViewCSharp)
            };
        }

        private static UIElement CreateSelectorBarBasicExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("SelectorBar"));
            }

            var selectorBar = new Mux.SelectorBar
            {
                Name = "SelectorBar1"
            };
            GalleryAutomation.WithAutomationId(selectorBar, GalleryAutomation.SampleElementId("SelectorBar", "SelectorBar"));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemRecent", "Recent", Mux.Symbol.Clock, false));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemShared", "Shared", Mux.Symbol.Share, false));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemFavorites", "Favorites", Mux.Symbol.Favorite, false));

            root.Children.Add(selectorBar);
            return root;
        }

        private static UIElement CreateSelectorBarFrameExampleContent()
        {
            var selectorBar = new Mux.SelectorBar
            {
                Name = "SelectorBar2"
            };
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemPage1", "Page1", null, true));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemPage2", "Page2", null, false));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemPage3", "Page3", null, false));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemPage4", "Page4", null, false));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemPage5", "Page5", null, false));

            var contentFrame = new Frame
            {
                Name = "ContentFrame",
                Width = 520,
                Height = 180,
                Margin = new Thickness(0, 12, 0, 0),
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden
            };

            Action updateContent = delegate
            {
                var currentSelectedIndex = selectorBar.Items.IndexOf(selectorBar.SelectedItem);
                if (currentSelectedIndex < 0)
                {
                    currentSelectedIndex = 0;
                }

                contentFrame.Content = CreatePageContent("SamplePage" + (currentSelectedIndex + 1), SelectorBarPageColors[currentSelectedIndex]);
            };
            selectorBar.SelectionChanged += delegate { updateContent(); };
            selectorBar.SelectedItem = selectorBar.Items[0];
            updateContent();

            var stack = new StackPanel();
            stack.Children.Add(selectorBar);
            stack.Children.Add(contentFrame);
            return stack;
        }

        private static UIElement CreateSelectorBarItemsViewExampleContent()
        {
            var pinkColorCollection = CreateSelectorBarColorCollection(Brushes.Pink, 5);
            var plumColorCollection = CreateSelectorBarColorCollection(Brushes.Plum, 7);
            var powderBlueColorCollection = CreateSelectorBarColorCollection(Brushes.PowderBlue, 4);

            var selectorBar = new Mux.SelectorBar
            {
                Name = "SelectorBar3"
            };
            var pinkItem = CreateSelectorBarItem("SelectorBarItemPink", "Pink", null, true);
            var plumItem = CreateSelectorBarItem("SelectorBarItemPlum", "Plum", null, false);
            var powderBlueItem = CreateSelectorBarItem("SelectorBarItemPowderBlue", "PowderBlue", null, false);
            selectorBar.Items.Add(pinkItem);
            selectorBar.Items.Add(plumItem);
            selectorBar.Items.Add(powderBlueItem);

            var itemsView = new ItemsControl
            {
                Name = "ItemsView3",
                Margin = new Thickness(0, 12, 0, 0),
                ItemTemplate = CreateSelectorBarColorTemplate(),
                ItemsSource = pinkColorCollection
            };
            var itemsPanel = new FrameworkElementFactory(typeof(StackPanel));
            itemsPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            itemsView.ItemsPanel = new ItemsPanelTemplate(itemsPanel);

            selectorBar.SelectionChanged += delegate(Mux.SelectorBar sender, Mux.SelectorBarSelectionChangedEventArgs args)
            {
                if (sender.SelectedItem == pinkItem)
                {
                    itemsView.ItemsSource = pinkColorCollection;
                }
                else if (sender.SelectedItem == plumItem)
                {
                    itemsView.ItemsSource = plumColorCollection;
                }
                else
                {
                    itemsView.ItemsSource = powderBlueColorCollection;
                }
            };
            selectorBar.SelectedItem = pinkItem;

            var stack = new StackPanel();
            stack.Children.Add(selectorBar);
            stack.Children.Add(itemsView);
            return stack;
        }

        private static Mux.SelectorBarItem CreateSelectorBarItem(string name, string text, Mux.Symbol? symbol, bool isSelected)
        {
            var item = new Mux.SelectorBarItem
            {
                Name = name,
                Text = text,
                Foreground = CreateSelectorBarItemForeground(),
                IsSelected = isSelected
            };
            item.Template = CreateSelectorBarItemTemplate();
            if (symbol.HasValue)
            {
                item.Icon = new Mux.SymbolIcon(symbol.Value);
            }

            return item;
        }

        private static Brush CreateSelectorBarItemForeground()
        {
            return IsDarkGalleryTheme()
                ? Brushes.White
                : CreateBrush("#E4000000");
        }

        private static bool IsDarkGalleryTheme()
        {
            if (!string.IsNullOrWhiteSpace(GalleryDiagnostics.Theme))
            {
                return string.Equals(GalleryDiagnostics.Theme, "Dark", StringComparison.OrdinalIgnoreCase);
            }

            return ModernWpf.ThemeManager.Current.ActualApplicationTheme == ModernWpf.ApplicationTheme.Dark;
        }

        private static ControlTemplate CreateSelectorBarItemTemplate()
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, Brushes.Transparent);

            var root = new FrameworkElementFactory(typeof(StackPanel));
            root.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

            var content = new FrameworkElementFactory(typeof(StackPanel));
            content.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            content.SetValue(FrameworkElement.MarginProperty, new Thickness(12, 10, 12, 7));

            var icon = new FrameworkElementFactory(typeof(ContentPresenter));
            icon.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(Mux.SelectorBarItem.IconProperty));
            icon.SetValue(FrameworkElement.MarginProperty, new Thickness(-2, 0, 8, 0));
            icon.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            icon.SetValue(FrameworkElement.RenderTransformOriginProperty, new Point(0.5, 0.5));
            icon.SetValue(UIElement.RenderTransformProperty, new ScaleTransform(0.8, 0.8));
            content.AppendChild(icon);

            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetValue(TextBlock.TextProperty, new TemplateBindingExtension(Mux.SelectorBarItem.TextProperty));
            text.SetValue(TextBlock.ForegroundProperty, new TemplateBindingExtension(Control.ForegroundProperty));
            text.SetValue(TextBlock.FontFamilyProperty, new TemplateBindingExtension(Control.FontFamilyProperty));
            text.SetValue(TextBlock.FontWeightProperty, new TemplateBindingExtension(Control.FontWeightProperty));
            text.SetValue(TextBlock.FontSizeProperty, new TemplateBindingExtension(Control.FontSizeProperty));
            text.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.AppendChild(text);

            var selectionPill = new FrameworkElementFactory(typeof(Rectangle));
            selectionPill.Name = "SelectionPill";
            selectionPill.SetValue(FrameworkElement.WidthProperty, 16.0);
            selectionPill.SetValue(FrameworkElement.HeightProperty, 3.0);
            selectionPill.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            selectionPill.SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);
            selectionPill.SetValue(Shape.FillProperty, CreateBrush("#0067C0"));
            selectionPill.SetValue(Rectangle.RadiusXProperty, 1.0);
            selectionPill.SetValue(Rectangle.RadiusYProperty, 1.0);

            root.AppendChild(content);
            root.AppendChild(selectionPill);
            border.AppendChild(root);

            var template = new ControlTemplate(typeof(Mux.SelectorBarItem))
            {
                VisualTree = border
            };
            var selectedTrigger = new Trigger
            {
                Property = Mux.SelectorBarItem.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible, "SelectionPill"));
            template.Triggers.Add(selectedTrigger);
            return template;
        }

        private static ObservableCollection<SolidColorBrush> CreateSelectorBarColorCollection(Brush brush, int count)
        {
            var colors = new ObservableCollection<SolidColorBrush>();
            for (var i = 0; i < count; i++)
            {
                colors.Add((SolidColorBrush)brush);
            }

            return colors;
        }

        private static DataTemplate CreateSelectorBarColorTemplate()
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(FrameworkElement.WidthProperty, 112.0);
            border.SetValue(FrameworkElement.HeightProperty, 82.0);
            border.SetValue(FrameworkElement.MarginProperty, new Thickness(4));
            border.SetBinding(Border.BackgroundProperty, new Binding());

            return new DataTemplate(typeof(SolidColorBrush))
            {
                VisualTree = border
            };
        }

        private static UIElement CreateTabViewSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("TabView"));
            panel.Children.Add(CreateTabViewBasicExampleContent(false));
            panel.Children.Add(CreateTabViewMarkupExampleContent());
            panel.Children.Add(CreateTabViewItemsSourceExampleContent());
            panel.Children.Add(CreateTabViewKeyboardingExampleContent());
            panel.Children.Add(CreateTabViewHeaderFooterExampleContent());
            panel.Children.Add(CreateTabViewWidthModeExampleContent());
            panel.Children.Add(CreateTabViewCloseButtonExampleContent());
            panel.Children.Add(CreateTabViewColorIconsExampleContent());
            panel.Children.Add(CreateTabViewAccentBackgroundExampleContent());
            panel.Children.Add(CreateTabViewWindowingExampleContent());
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateTabViewExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "A TabView with support for adding, closing, and rearranging tabs",
                    CreateTabViewBasicExampleContent(true),
                    TabViewBasicXaml,
                    FindSampleCodeText(sampleSnippets, "TabViewBasicSample_cs.txt")),
                new GalleryExample(
                    "A TabView with TabViewItems defined in markup",
                    CreateTabViewMarkupExampleContent(),
                    TabViewMarkupXaml,
                    null),
                new GalleryExample(
                    "A TabView bound to a collection of MyData objects",
                    CreateTabViewItemsSourceExampleContent(),
                    TabViewItemsSourceXaml,
                    null),
                new GalleryExample(
                    "A TabView with keyboarding support",
                    CreateTabViewKeyboardingExampleContent(),
                    TabViewKeyboardingXaml,
                    FindSampleCodeText(sampleSnippets, "TabViewKeyboardAcceleratorSample_cs.txt")),
                new GalleryExample(
                    "You can put custom content in TabStripHeader and TabStripFooter",
                    CreateTabViewHeaderFooterExampleContent(),
                    TabViewHeaderFooterXaml,
                    null),
                new GalleryExample(
                    "Tab widths can either be equally sized, sized to the content of the tab, or sized to only show the icon when unselected",
                    CreateTabViewWidthModeExampleContent(),
                    TabViewWidthModeXaml,
                    null),
                new GalleryExample(
                    "The close button can be persistent or only visible on hover",
                    CreateTabViewCloseButtonExampleContent(),
                    TabViewCloseButtonXaml,
                    null),
                new GalleryExample(
                    "TabView with color tab icons",
                    CreateTabViewColorIconsExampleContent(),
                    TabViewColorIconsXaml,
                    null),
                new GalleryExample(
                    "A TabView with accent colored TabStrip background",
                    CreateTabViewAccentBackgroundExampleContent(),
                    TabViewAccentBackgroundXaml,
                    null),
                new GalleryExample(
                    "Complete TabView windowing sample",
                    CreateTabViewWindowingExampleContent(),
                    TabViewWindowingXaml,
                    null)
            };
        }

        private static UIElement CreateTabViewBasicExampleContent(bool assignRootAutomationId)
        {
            var root = CreateTabViewExampleRoot(assignRootAutomationId);
            var tabControl = CreateTabViewControl("TabView1", true);
            AddGeneratedDocumentTabs(tabControl);
            root.Children.Add(tabControl);
            root.Children.Add(CreateTabViewCommandBar(tabControl, "TabView1"));
            return root;
        }

        private static UIElement CreateTabViewMarkupExampleContent()
        {
            var root = CreateTabViewExampleRoot(false);
            var tabControl = CreateTabViewControl("TabViewMarkupSample", false);
            tabControl.Items.Add(CreateTabViewTab("Document 0", "SamplePage1"));
            tabControl.Items.Add(CreateTabViewTab("Document 1", "SamplePage2"));
            tabControl.Items.Add(CreateTabViewTab("Document 2", "SamplePage3"));
            tabControl.SelectedIndex = 0;
            root.Children.Add(tabControl);
            root.Children.Add(CreateTabViewCommandBar(tabControl, "TabViewMarkupSample"));
            return root;
        }

        private static UIElement CreateTabViewItemsSourceExampleContent()
        {
            var root = CreateTabViewExampleRoot(false);
            var data = new ObservableCollection<TabViewData>();
            for (var i = 0; i < 3; i++)
            {
                data.Add(CreateTabViewData(i));
            }

            var tabControl = CreateTabViewControl("TabViewItemsSourceSample", false);
            tabControl.ItemsSource = data;
            tabControl.DisplayMemberPath = "DataHeader";
            tabControl.ContentTemplate = CreateTabViewDataContentTemplate();
            tabControl.SelectedIndex = 0;
            root.Children.Add(tabControl);

            var commands = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var addButton = CreateButton("Add tab");
            addButton.Name = "TabViewItemsSourceSampleAddButton";
            addButton.Click += delegate
            {
                data.Add(CreateTabViewData(data.Count));
                tabControl.SelectedIndex = data.Count - 1;
            };
            var closeButton = CreateButton("Close selected");
            closeButton.Name = "TabViewItemsSourceSampleCloseButton";
            closeButton.Click += delegate
            {
                if (tabControl.SelectedItem is TabViewData selected)
                {
                    data.Remove(selected);
                }
            };
            commands.Children.Add(addButton);
            commands.Children.Add(closeButton);
            root.Children.Add(commands);
            return root;
        }

        private static UIElement CreateTabViewKeyboardingExampleContent()
        {
            var root = CreateTabViewExampleRoot(false);
            root.Children.Add(CreateTextBlock("- Ctrl+T opens a new tab", new Thickness(0)));
            root.Children.Add(CreateTextBlock("- Ctrl+W closes the selected tab", new Thickness(0)));
            root.Children.Add(CreateTextBlock("- Ctrl+1 to Ctrl+8 selects that number tab", new Thickness(0)));
            root.Children.Add(CreateTextBlock("- Ctrl+9 selects the last tab (regardless of the number of tabs)", new Thickness(0, 0, 0, 24)));

            var tabControl = CreateTabViewControl("TabView2", false);
            AddGeneratedDocumentTabs(tabControl);
            root.Children.Add(tabControl);
            root.Children.Add(CreateTabViewCommandBar(tabControl, "TabView2"));
            return root;
        }

        private static UIElement CreateTabViewHeaderFooterExampleContent()
        {
            var root = CreateTabViewExampleRoot(false);
            root.Children.Add(CreateTextBlock("You can put any content in the TabStripHeader and TabStripFooter areas", new Thickness(0, 0, 0, 12)));
            root.Children.Add(CreateTextBlock("If your TabView is used inside the app's titlebar area, use the TabStripFooter to specify a custom drag region", new Thickness(0, 0, 0, 12)));
            root.Children.Add(CreateTextBlock("See TabViewWindowingSamplePage.xaml and *.cs files to see the complete code", new Thickness(0, 0, 0, 24)));

            var stripContent = new DockPanel
            {
                LastChildFill = true
            };
            stripContent.Children.Add(CreateTabStripText("TabStripHeader Content", new Thickness(8, 6, 16, 6), Dock.Left));
            stripContent.Children.Add(CreateTabStripText("TabStripFooter Content", new Thickness(16, 6, 6, 6), Dock.Right));
            root.Children.Add(stripContent);

            var tabControl = CreateTabViewControl("TabViewHeaderFooterSample", false);
            AddGeneratedDocumentTabs(tabControl);
            root.Children.Add(tabControl);
            root.Children.Add(CreateTabViewCommandBar(tabControl, "TabViewHeaderFooterSample"));
            return root;
        }

        private static UIElement CreateTabViewWidthModeExampleContent()
        {
            var root = CreateTabViewExampleRoot(false);
            var tabControl = CreateTabViewControl("TabView3", false);
            tabControl.Items.Add(CreateTabViewTab("Home", "SamplePage1"));
            tabControl.Items.Add(CreateTabViewTab("Tab 2 Has Longer Text", "SamplePage2"));
            tabControl.Items.Add(CreateTabViewTab("Third Tab", "SamplePage3"));
            tabControl.SelectedIndex = 0;
            root.Children.Add(tabControl);

            var comboBox = CreateOptionComboBox("TabWidthBehaviorComboBox", "TabWidthBehavior", "SizeToContent", "Equal", "Compact");
            comboBox.SelectionChanged += delegate
            {
                ApplyTabWidthMode(tabControl, ((ComboBoxItem)comboBox.SelectedItem).Content.ToString());
            };
            root.Children.Add(comboBox);
            ApplyTabWidthMode(tabControl, "SizeToContent");
            return root;
        }

        private static UIElement CreateTabViewCloseButtonExampleContent()
        {
            var root = CreateTabViewExampleRoot(false);
            var tabControl = CreateTabViewControl("TabView4", false);
            tabControl.Items.Add(CreateTabViewTab("Home", "SamplePage1"));
            tabControl.Items.Add(CreateTabViewTab("Tab 2 Has Longer Text", "SamplePage2"));
            tabControl.Items.Add(CreateTabViewTab("Third Tab", "SamplePage3"));
            tabControl.SelectedIndex = 0;
            root.Children.Add(tabControl);

            var comboBox = CreateOptionComboBox("TabCloseButtonOverlayModeComboBox", "TabViewItem CloseButtonOverlayMode", "Auto", "Always", "OnHover");
            comboBox.SelectedIndex = 1;
            comboBox.SelectionChanged += delegate
            {
                ApplyCloseButtonOverlayMode(tabControl, ((ComboBoxItem)comboBox.SelectedItem).Content.ToString());
            };
            root.Children.Add(comboBox);
            ApplyCloseButtonOverlayMode(tabControl, "Always");
            return root;
        }

        private static UIElement CreateTabViewColorIconsExampleContent()
        {
            var root = CreateTabViewExampleRoot(false);
            root.Children.Add(CreateTextBlock(@"Use BitmapIcon.ShowAsMonochrome=""False"" to display full color icons in the TabViewItem", new Thickness(0, 0, 0, 12)));
            var tabControl = CreateTabViewControl("TabViewColorIconsSample", false);
            tabControl.MinWidth = 490;
            tabControl.MinHeight = 0;
            tabControl.Items.Add(CreateTabViewTab(CreateBitmapHeader("CMD Prompt", "cmd.png"), "CMD Prompt"));
            tabControl.Items.Add(CreateTabViewTab(CreateBitmapHeader("PowerShell", "powershell.png"), "PowerShell"));
            tabControl.Items.Add(CreateTabViewTab(CreateBitmapHeader("Windows Subsystem for Linux", "linux.png"), "Windows Subsystem for Linux"));
            tabControl.SelectedIndex = 0;
            root.Children.Add(tabControl);
            return root;
        }

        private static UIElement CreateTabViewAccentBackgroundExampleContent()
        {
            var root = CreateTabViewExampleRoot(false);
            var tabControl = CreateTabViewControl("TabViewAccentSample", false);
            tabControl.Resources["TabViewBackground"] = IsDarkGalleryTheme()
                ? CreateBrush("#004275")
                : CreateBrush("#99EBFF");
            AddGeneratedDocumentTabs(tabControl);
            root.Children.Add(tabControl);
            root.Children.Add(CreateTabViewCommandBar(tabControl, "TabViewAccentSample"));
            return root;
        }

        private static UIElement CreateTabViewWindowingExampleContent()
        {
            var root = CreateTabViewExampleRoot(false);
            var button = CreateButton("Click here to launch the sample");
            button.Name = "TabViewWindowingButton";
            button.Click += delegate
            {
                var owner = Window.GetWindow(button);
                var window = new Window
                {
                    Title = "TabView windowing sample",
                    Width = 720,
                    Height = 460,
                    Content = CreateTabViewBasicExampleContent(false)
                };
                if (owner != null)
                {
                    window.Owner = owner;
                }
                window.Show();
            };
            root.Children.Add(button);
            return root;
        }

        private static GallerySamplePanel CreateTabViewExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("TabView"));
            }

            return root;
        }

        private static TabControl CreateTabViewControl(string name, bool assignPrimaryAutomationId)
        {
            var tabControl = new TabControl
            {
                Name = name,
                MinHeight = 475,
                MaxWidth = TabViewReferenceWidth,
                Margin = new Thickness(-12),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                SelectedIndex = 0,
                Style = FindStyleResource("DefaultTabControlStyle")
            };
            if (assignPrimaryAutomationId)
            {
                GalleryAutomation.WithAutomationId(tabControl, GalleryAutomation.SampleElementId("TabView", "TabView"));
            }

            return tabControl;
        }

        private static void AddGeneratedDocumentTabs(TabControl tabControl)
        {
            for (var i = 0; i < 3; i++)
            {
                tabControl.Items.Add(CreateGeneratedTabViewTab(i));
            }

            tabControl.SelectedIndex = 0;
        }

        private static TabItem CreateGeneratedTabViewTab(int index)
        {
            return CreateTabViewTab("Document " + index, "SamplePage" + (index % 3 + 1));
        }

        private static StackPanel CreateTabViewCommandBar(TabControl tabControl, string namePrefix)
        {
            var commands = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var add = CreateButton("Add tab");
            add.Name = namePrefix + "AddButton";
            add.Click += delegate
            {
                var index = tabControl.Items.Count;
                var tab = CreateGeneratedTabViewTab(index);
                tabControl.Items.Add(tab);
                tabControl.SelectedItem = tab;
            };
            var close = CreateButton("Close selected");
            close.Name = namePrefix + "CloseButton";
            close.Click += delegate
            {
                if (tabControl.SelectedItem != null)
                {
                    tabControl.Items.Remove(tabControl.SelectedItem);
                }
            };
            commands.Children.Add(add);
            commands.Children.Add(close);
            return commands;
        }

        private static TabItem CreateTabViewTab(object header, string pageTitle)
        {
            return new TabItem
            {
                Header = header,
                Style = FindStyleResource("DefaultTabItemStyle"),
                Content = CreateTabViewPageContent(pageTitle)
            };
        }

        private static UIElement CreateTabViewPageContent(string title)
        {
            switch (title)
            {
                case "SamplePage1":
                    return CreateTabViewSamplePage1Content();
                case "SamplePage2":
                    return CreateTabViewSamplePage2Content();
                case "SamplePage3":
                    return CreateTabViewSamplePage3Content();
                default:
                    return CreateTabViewFallbackPageContent(title);
            }
        }

        private static UIElement CreateTabViewFallbackPageContent(string title)
        {
            return new Border
            {
                Padding = new Thickness(18),
                Child = new TextBlock
                {
                    Text = title,
                    FontSize = 22,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        private static ScrollViewer CreateTabViewSamplePage1Content()
        {
            var grid = CreateTabViewSampleGrid(
                GridLength.Auto,
                new GridLength(1, GridUnitType.Star),
                new GridLength(1, GridUnitType.Star));
            AddTabViewSampleRows(grid, 4, true);

            AddTabViewSampleTile(grid, 1, 1, 1, 1, CreateBrush("#A9A9A9"), 0, double.NaN, double.NaN, new Thickness(6));
            AddTabViewSampleTile(grid, 1, 2, 1, 1, CreateBrush("#D3D3D3"), 0, double.NaN, double.NaN, new Thickness(6));
            AddTabViewSampleTile(grid, 2, 1, 1, 1, CreateBrush("#D3D3D3"), 0, double.NaN, double.NaN, new Thickness(6));
            AddTabViewSampleTile(grid, 2, 2, 1, 1, CreateBrush("#A9A9A9"), 0, double.NaN, double.NaN, new Thickness(6));
            AddTabViewAccentSampleTile(grid, 1, 0, 2, 1, 250, double.NaN, double.NaN, new Thickness(5));
            AddTabViewSampleText(grid, 3, 0, 3, SamplePageLoremIpsum, new Thickness(6, 12, 6, 12), 0, FontWeights.Normal);

            return CreateTabViewSampleScrollViewer(grid);
        }

        private static ScrollViewer CreateTabViewSamplePage2Content()
        {
            var grid = CreateTabViewSampleGrid(
                GridLength.Auto,
                new GridLength(1, GridUnitType.Star));
            AddTabViewSampleRows(grid, 2, false);

            AddTabViewAccentSampleTile(grid, 1, 0, 1, 1, 0, 150, 200, new Thickness(12)).VerticalAlignment = VerticalAlignment.Top;

            var panel = new StackPanel
            {
                MinHeight = 200,
                Margin = new Thickness(12)
            };
            panel.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 12),
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap
            });
            panel.Children.Add(CreateTextBlock(SamplePageLoremIpsum, new Thickness(0)));
            Grid.SetRow(panel, 1);
            Grid.SetColumn(panel, 1);
            grid.Children.Add(panel);

            return CreateTabViewSampleScrollViewer(grid);
        }

        private static ScrollViewer CreateTabViewSamplePage3Content()
        {
            var grid = CreateTabViewSampleGrid(
                new GridLength(2, GridUnitType.Star),
                new GridLength(1, GridUnitType.Star),
                new GridLength(1, GridUnitType.Star));
            AddTabViewSampleRows(grid, 4, true);

            AddTabViewSampleTile(grid, 1, 0, 2, 1, CreateBrush("#D3D3D3"), 0, double.NaN, double.NaN, new Thickness(5));
            AddTabViewSampleTile(grid, 1, 1, 1, 1, CreateBrush("#A9A9A9"), 0, double.NaN, double.NaN, new Thickness(5));
            AddTabViewSampleTile(grid, 2, 1, 1, 1, CreateBrush("#808080"), 0, double.NaN, double.NaN, new Thickness(5));
            AddTabViewSampleTile(grid, 1, 2, 1, 1, CreateBrush("#D3D3D3"), 0, double.NaN, double.NaN, new Thickness(5));
            AddTabViewSampleTile(grid, 2, 2, 1, 1, CreateBrush("#A9A9A9"), 0, double.NaN, double.NaN, new Thickness(5));
            AddTabViewSampleText(grid, 3, 0, 3, SamplePageLoremIpsum, new Thickness(5), 0, FontWeights.Normal);

            return CreateTabViewSampleScrollViewer(grid);
        }

        private static Grid CreateTabViewSampleGrid(params GridLength[] columns)
        {
            var grid = new Grid();
            foreach (var width in columns)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
            }

            return grid;
        }

        private static void AddTabViewSampleRows(Grid grid, int count, bool lastRowStar)
        {
            for (var i = 0; i < count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = lastRowStar && i == count - 1 ? new GridLength(1, GridUnitType.Star) : GridLength.Auto
                });
            }
        }

        private static Grid AddTabViewSampleTile(Grid grid, int row, int column, int rowSpan, int columnSpan, Brush background, double minWidth, double width, double height, Thickness margin)
        {
            var tile = new Grid
            {
                MinWidth = minWidth,
                MinHeight = 150,
                Width = width,
                Height = height,
                Margin = margin,
                Background = background
            };
            Grid.SetRow(tile, row);
            Grid.SetColumn(tile, column);
            if (rowSpan > 1)
            {
                Grid.SetRowSpan(tile, rowSpan);
            }
            if (columnSpan > 1)
            {
                Grid.SetColumnSpan(tile, columnSpan);
            }
            grid.Children.Add(tile);
            return tile;
        }

        private static Grid AddTabViewAccentSampleTile(Grid grid, int row, int column, int rowSpan, int columnSpan, double minWidth, double width, double height, Thickness margin)
        {
            var tile = AddTabViewSampleTile(grid, row, column, rowSpan, columnSpan, Brushes.Transparent, minWidth, width, height, margin);
            tile.SetResourceReference(Panel.BackgroundProperty, "SystemControlBackgroundAccentBrush");
            return tile;
        }

        private static void AddTabViewSampleText(Grid grid, int row, int column, int columnSpan, string text, Thickness margin, double fontSize, FontWeight fontWeight)
        {
            var textBlock = CreateTextBlock(text, margin);
            if (fontSize > 0)
            {
                textBlock.FontSize = fontSize;
            }
            textBlock.FontWeight = fontWeight;
            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column);
            if (columnSpan > 1)
            {
                Grid.SetColumnSpan(textBlock, columnSpan);
            }
            grid.Children.Add(textBlock);
        }

        private static ScrollViewer CreateTabViewSampleScrollViewer(UIElement content)
        {
            return new ScrollViewer
            {
                Content = content
            };
        }

        private static DataTemplate CreateTabViewDataContentTemplate()
        {
            var contentControl = new FrameworkElementFactory(typeof(ContentControl));
            contentControl.SetBinding(ContentControl.ContentProperty, new Binding("DataContent"));
            contentControl.SetValue(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            contentControl.SetValue(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
            return new DataTemplate(typeof(TabViewData))
            {
                VisualTree = contentControl
            };
        }

        private static TabViewData CreateTabViewData(int index)
        {
            return new TabViewData
            {
                DataHeader = "MyData Doc " + index,
                DataContent = CreateTabViewPageContent("SamplePage" + (index % 3 + 1))
            };
        }

        private static TextBlock CreateTextBlock(string text, Thickness margin)
        {
            return new TextBlock
            {
                Text = text,
                Margin = margin,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static TextBlock CreateTabStripText(string text, Thickness margin, Dock dock)
        {
            var textBlock = CreateTextBlock(text, margin);
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            DockPanel.SetDock(textBlock, dock);
            return textBlock;
        }

        private static ComboBox CreateOptionComboBox(string name, string header, params string[] options)
        {
            var comboBox = new ComboBox
            {
                Name = name,
                Width = 190,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(comboBox, header);
            foreach (var option in options)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = option });
            }

            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        private static void ApplyTabWidthMode(TabControl tabControl, string mode)
        {
            foreach (TabItem item in tabControl.Items)
            {
                if (string.Equals(mode, "Equal", StringComparison.Ordinal))
                {
                    item.Width = 160;
                }
                else if (string.Equals(mode, "Compact", StringComparison.Ordinal))
                {
                    item.Width = 48;
                }
                else
                {
                    item.ClearValue(FrameworkElement.WidthProperty);
                }
            }
        }

        private static void ApplyCloseButtonOverlayMode(TabControl tabControl, string mode)
        {
            foreach (TabItem item in tabControl.Items)
            {
                AutomationProperties.SetHelpText(item, "CloseButtonOverlayMode=" + mode);
            }
        }

        private static object CreateBitmapHeader(string text, string fileName)
        {
            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var image = new Image
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 6, 0)
            };
            var imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "SampleMedia", fileName);
            if (System.IO.File.Exists(imagePath))
            {
                image.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }

            stack.Children.Add(image);
            stack.Children.Add(new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center
            });
            return stack;
        }

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string title)
        {
            if (snippets == null)
            {
                return null;
            }

            foreach (var snippet in snippets)
            {
                if (string.Equals(snippet.Title, title, StringComparison.OrdinalIgnoreCase))
                {
                    return snippet.Text;
                }
            }

            return null;
        }

        private static GallerySamplePanel CreateNavigationViewDescriptionRoot(string description, bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("NavigationView"));
            }

            if (!string.IsNullOrEmpty(description))
            {
                root.Children.Add(new TextBlock
                {
                    Text = description,
                    Margin = new Thickness(0, 0, 0, 12),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            return root;
        }

        private static Mux.NavigationView CreateNavigationViewShell(
            string name,
            string contentFrameName,
            Mux.NavigationViewPaneDisplayMode paneDisplayMode,
            string header)
        {
            return new Mux.NavigationView
            {
                Name = name,
                Width = 745,
                Height = 460,
                HorizontalAlignment = HorizontalAlignment.Left,
                Header = header,
                IsTitleBarAutoPaddingEnabled = false,
                IsTabStop = false,
                PaneDisplayMode = paneDisplayMode,
                Content = CreateNavigationFrame(contentFrameName)
            };
        }

        private static Frame CreateNavigationFrame(string name)
        {
            return new Frame
            {
                Name = name,
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                Content = CreateNavigationSampleContent()
            };
        }

        private static void AddStandardNavigationItems(
            Mux.NavigationView navigationView,
            bool includeIcons,
            string firstContent,
            string remainingPrefix)
        {
            if (includeIcons)
            {
                navigationView.MenuItems.Add(CreateNavigationItem(firstContent, Mux.Symbol.Play, "SamplePage1"));
                navigationView.MenuItems.Add(CreateNavigationItem(remainingPrefix + "2", Mux.Symbol.Save, "SamplePage2"));
                navigationView.MenuItems.Add(CreateNavigationItem(remainingPrefix + "3", Mux.Symbol.Refresh, "SamplePage3"));
                navigationView.MenuItems.Add(CreateNavigationItem(remainingPrefix + "4", Mux.Symbol.Download, "SamplePage4"));
            }
            else
            {
                navigationView.MenuItems.Add(CreateNavigationItem(firstContent, "SamplePage1"));
                navigationView.MenuItems.Add(CreateNavigationItem(remainingPrefix + "2", "SamplePage2"));
                navigationView.MenuItems.Add(CreateNavigationItem(remainingPrefix + "3", "SamplePage3"));
                navigationView.MenuItems.Add(CreateNavigationItem(remainingPrefix + "4", "SamplePage4"));
            }
        }

        private static void HookNavigationHeaderSelection(Mux.NavigationView navigationView)
        {
            navigationView.SelectionChanged += delegate(Mux.NavigationView sender, Mux.NavigationViewSelectionChangedEventArgs args)
            {
                var item = args.SelectedItemContainer as Mux.NavigationViewItem;
                var selectedItemTag = item == null ? null : item.Tag as string;
                if (!string.IsNullOrEmpty(selectedItemTag))
                {
                    sender.Header = "Sample Page " + selectedItemTag.Substring(selectedItemTag.Length - 1);
                }
            };
        }

        private static void SelectFirstNavigationItem(Mux.NavigationView navigationView)
        {
            if (navigationView.MenuItems.Count > 0)
            {
                navigationView.SelectedItem = navigationView.MenuItems[0];
            }
        }

        private static DataTemplate CreateNavigationCategoryTemplate()
        {
            return (DataTemplate)XamlReader.Parse(
@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
               xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
               xmlns:controls=""clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls""
               xmlns:mux=""clr-namespace:ModernWpf.Controls;assembly=ModernWpf"">
    <controls:NavigationViewItem Content=""{Binding Name}"" Tag=""{Binding Tag}"" ToolTip=""{Binding Tooltip}"">
        <controls:NavigationViewItem.Icon>
            <mux:SymbolIcon Symbol=""{Binding Symbol}"" />
        </controls:NavigationViewItem.Icon>
    </controls:NavigationViewItem>
</DataTemplate>");
        }

        private static Grid CreateNavigationViewExampleLayout(UIElement sample, UIElement options)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid.SetColumn(sample, 0);
            grid.Children.Add(sample);

            if (options != null)
            {
                var optionsHost = new Border
                {
                    Margin = new Thickness(24, 0, 0, 0),
                    Child = options
                };
                Grid.SetColumn(optionsHost, 1);
                grid.Children.Add(optionsHost);
            }

            return grid;
        }

        private static StackPanel CreateNavigationPanePositionOptions(
            string header,
            string leftName,
            string topName,
            string leftCompactName,
            Mux.NavigationView navigationView)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = header,
                Margin = new Thickness(0, 0, 0, 8)
            });

            var groupName = leftName + "Group";
            var left = CreateNamedRadioButton(leftName, "Left mode", true, groupName);
            left.Checked += delegate
            {
                if (left.IsChecked == true)
                {
                    navigationView.PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Left;
                    navigationView.IsPaneOpen = true;
                }
            };
            panel.Children.Add(left);

            var top = CreateNamedRadioButton(topName, "Top mode", false, groupName);
            top.Checked += delegate
            {
                if (top.IsChecked == true)
                {
                    navigationView.PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Top;
                    navigationView.IsPaneOpen = false;
                }
            };
            panel.Children.Add(top);

            if (leftCompactName != null)
            {
                var leftCompact = CreateNamedRadioButton(leftCompactName, "LeftCompact mode", false, groupName);
                leftCompact.Checked += delegate
                {
                    if (leftCompact.IsChecked == true)
                    {
                        navigationView.PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.LeftCompact;
                        navigationView.IsPaneOpen = false;
                    }
                };
                panel.Children.Add(leftCompact);
            }

            return panel;
        }

        private static RadioButton CreateNamedRadioButton(string name, string content, bool isChecked, string groupName)
        {
            return new RadioButton
            {
                Name = name,
                Content = content,
                GroupName = groupName,
                IsChecked = isChecked,
                Margin = new Thickness(0, 0, 0, 4)
            };
        }

        private static StackPanel CreateNavigationViewApiOptions(
            Mux.NavigationView navigationView,
            Mux.HyperlinkButton paneHyperlink,
            StackPanel footerStackPanel,
            Mux.NavigationViewItem samplePage2Item)
        {
            var panel = new StackPanel();

            var settingsCheck = CreateNamedCheckBox("settingsCheck", "Settings item visible", true);
            settingsCheck.Click += delegate { navigationView.IsSettingsVisible = settingsCheck.IsChecked == true; };
            panel.Children.Add(settingsCheck);

            var visibleCheck = CreateNamedCheckBox("visibleCheck", "Back button visible", true);
            visibleCheck.Click += delegate
            {
                navigationView.IsBackButtonVisible = visibleCheck.IsChecked == true
                    ? Mux.NavigationViewBackButtonVisible.Visible
                    : Mux.NavigationViewBackButtonVisible.Collapsed;
            };
            panel.Children.Add(visibleCheck);

            var enableCheck = CreateNamedCheckBox("enableCheck", "Back button enabled", false);
            enableCheck.Click += delegate { navigationView.IsBackEnabled = enableCheck.IsChecked == true; };
            panel.Children.Add(enableCheck);

            var autoSuggestCheck = CreateNamedCheckBox("autoSuggestCheck", "AutoSuggestBox visible", true);
            autoSuggestCheck.Click += delegate
            {
                navigationView.AutoSuggestBox = autoSuggestCheck.IsChecked == true ? CreateNavigationAutoSuggestBox() : null;
            };
            panel.Children.Add(autoSuggestCheck);

            panel.Children.Add(CreateOptionText("Header:"));
            var headerText = new TextBox
            {
                Name = "headerText",
                Text = "Header",
                MinWidth = 160,
                Margin = new Thickness(0, 0, 0, 8)
            };
            AutomationProperties.SetName(headerText, "Header property");
            headerText.TextChanged += delegate { navigationView.Header = headerText.Text; };
            panel.Children.Add(headerText);

            var headerCheck = CreateNamedCheckBox("headerCheck", "Always show header", true);
            headerCheck.Click += delegate { navigationView.AlwaysShowHeader = headerCheck.IsChecked == true; };
            panel.Children.Add(headerCheck);

            panel.Children.Add(CreateOptionText("PaneTitle:"));
            var paneText = new TextBox
            {
                Name = "paneText",
                Text = "Pane Title",
                MinWidth = 160,
                Margin = new Thickness(0, 0, 0, 8)
            };
            AutomationProperties.SetName(paneText, "PaneTitle property");
            paneText.TextChanged += delegate { navigationView.PaneTitle = paneText.Text; };
            panel.Children.Add(paneText);

            var paneCustomContentCheck = CreateNamedCheckBox("panemc_Check", "PaneCustomContent visible", false);
            paneCustomContentCheck.Click += delegate
            {
                paneHyperlink.Visibility = paneCustomContentCheck.IsChecked == true
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            };
            panel.Children.Add(paneCustomContentCheck);

            var paneFooterCheck = CreateNamedCheckBox("paneFooterCheck", "PaneFooter visible", false);
            paneFooterCheck.Click += delegate
            {
                footerStackPanel.Visibility = paneFooterCheck.IsChecked == true
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            };
            panel.Children.Add(paneFooterCheck);

            panel.Children.Add(CreateOptionText("PanePosition:"));
            var left = CreateNamedRadioButton("nvSampleLeft", "Left", true, "nvSamplePanePosition");
            var top = CreateNamedRadioButton("nvSampleTop", "Top", false, "nvSamplePanePosition");
            left.Checked += delegate
            {
                if (left.IsChecked == true)
                {
                    navigationView.PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Left;
                    navigationView.IsPaneOpen = true;
                    footerStackPanel.Orientation = Orientation.Vertical;
                }
            };
            top.Checked += delegate
            {
                if (top.IsChecked == true)
                {
                    navigationView.PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Top;
                    navigationView.IsPaneOpen = false;
                    footerStackPanel.Orientation = Orientation.Horizontal;
                }
            };
            panel.Children.Add(left);
            panel.Children.Add(top);

            var selectionFollowsFocus = CreateNamedCheckBox("sffCheck", "Keyboard SelectionFollowsFocus", false);
            selectionFollowsFocus.Click += delegate
            {
                navigationView.SelectionFollowsFocus = selectionFollowsFocus.IsChecked == true
                    ? Mux.NavigationViewSelectionFollowsFocus.Enabled
                    : Mux.NavigationViewSelectionFollowsFocus.Disabled;
            };
            panel.Children.Add(selectionFollowsFocus);

            var suppressSelection = CreateNamedCheckBox("suppressselectionCheck_Checked", "Selection of Menu Item2 suppressed", false);
            suppressSelection.Click += delegate { samplePage2Item.SelectsOnInvoked = suppressSelection.IsChecked != true; };
            panel.Children.Add(suppressSelection);

            return panel;
        }

        private static CheckBox CreateNamedCheckBox(string name, string content, bool isChecked)
        {
            return new CheckBox
            {
                Name = name,
                Content = content,
                IsChecked = isChecked,
                Margin = new Thickness(0, 0, 0, 4)
            };
        }

        private static TextBlock CreateOptionText(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 4)
            };
        }

        private static Mux.AutoSuggestBox CreateNavigationAutoSuggestBox()
        {
            var autoSuggestBox = new Mux.AutoSuggestBox
            {
                QueryIcon = new Mux.SymbolIcon(Mux.Symbol.Find)
            };
            AutomationProperties.SetName(autoSuggestBox, "Search");
            return autoSuggestBox;
        }

        private static Mux.NavigationViewItem CreateToolTippedNavigationItem(string content, Mux.Symbol symbol, string tag)
        {
            var item = CreateNavigationItem(content, symbol, tag);
            item.ToolTip = content;
            return item;
        }

        private static UIElement CreateNavigationSampleContent()
        {
            var grid = new Grid();
            grid.Resources["TileHeight"] = 150.0;
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            AddNavigationTile(grid, 1, 1, 1, 1, CreateBrush("#A9A9A9"), 0);
            AddNavigationTile(grid, 1, 2, 1, 1, CreateBrush("#D3D3D3"), 0);
            AddNavigationTile(grid, 2, 1, 1, 1, CreateBrush("#D3D3D3"), 0);
            AddNavigationTile(grid, 2, 2, 1, 1, CreateBrush("#A9A9A9"), 0);
            AddNavigationTile(grid, 1, 0, 2, 1, CreateBrush("#0078D4"), 250);

            var text = new TextBlock
            {
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                TextWrapping = TextWrapping.Wrap
            };
            var textHost = new Grid
            {
                Margin = new Thickness(6, 12, 6, 12)
            };
            textHost.Children.Add(text);
            Grid.SetRow(textHost, 3);
            Grid.SetColumnSpan(textHost, 3);
            grid.Children.Add(textHost);

            return new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                Content = grid
            };
        }

        private static void AddNavigationTile(Grid grid, int row, int column, int rowSpan, int columnSpan, Brush background, double minWidth)
        {
            var tile = new Grid
            {
                MinWidth = minWidth,
                MinHeight = 150,
                Margin = new Thickness(column == 0 ? 5 : 6),
                Background = background
            };
            Grid.SetRow(tile, row);
            Grid.SetColumn(tile, column);
            if (rowSpan > 1)
            {
                Grid.SetRowSpan(tile, rowSpan);
            }
            if (columnSpan > 1)
            {
                Grid.SetColumnSpan(tile, columnSpan);
            }
            grid.Children.Add(tile);
        }

        private static Mux.NavigationViewItem CreateNavigationItem(string content, string tag)
        {
            return new Mux.NavigationViewItem
            {
                Content = content,
                Tag = tag
            };
        }

        private static Mux.NavigationViewItem CreateNavigationItem(string content, Mux.Symbol symbol, string tag, string automationName = null)
        {
            var item = new Mux.NavigationViewItem
            {
                Content = content,
                Icon = new Mux.SymbolIcon(symbol),
                Tag = tag
            };
            if (!string.IsNullOrEmpty(automationName))
            {
                AutomationProperties.SetName(item, automationName);
            }

            return item;
        }

        private static Page CreatePageContent(string title, string color)
        {
            return new Page
            {
                Content = new Border
                {
                    Background = CreateBrush(color),
                    Padding = new Thickness(18),
                    Child = new TextBlock
                    {
                        Text = title,
                        FontSize = 22,
                        FontWeight = FontWeights.SemiBold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            };
        }

        private static MenuItem CreateWpfMenuItem(string header, TextBlock output)
        {
            var item = new MenuItem { Header = header };
            item.Click += delegate { output.Text = "Selected " + header.Replace("_", string.Empty); };
            return item;
        }

        private static TabItem CreateTab(string header, string text)
        {
            return new TabItem
            {
                Header = header,
                Content = new Border
                {
                    Padding = new Thickness(16),
                    Child = new TextBlock
                    {
                        Text = text,
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            };
        }

        private static TabItem CreatePivotItem(string header, string text)
        {
            return new TabItem
            {
                Header = header,
                Style = FindStyleResource("TabItemPivotStyle"),
                Content = new TextBlock
                {
                    Text = text,
                    TextWrapping = TextWrapping.Wrap
                }
            };
        }

        private static Style FindStyleResource(string key)
        {
            return Application.Current.TryFindResource(key) as Style;
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new GallerySamplePanel
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

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Content = text,
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
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

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }

        private sealed class BreadcrumbFolder
        {
            public string Name { get; set; }
        }

        private sealed class NavigationCategory
        {
            public NavigationCategory(string name, Mux.Symbol symbol, string tooltip, string tag)
            {
                Name = name;
                Symbol = symbol;
                Tooltip = tooltip;
                Tag = tag;
            }

            public string Name { get; }

            public Mux.Symbol Symbol { get; }

            public string Tooltip { get; }

            public string Tag { get; }
        }

        private sealed class TabViewData
        {
            public string DataHeader { get; set; }

            public object DataContent { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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
            var panel = CreateSamplePanel("NavigationView with default PaneDisplayMode");
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("NavigationView"));
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.Children.Add(new TextBlock
            {
                Text = "If you have five or more equally important navigation categories that should prominently appear on larger window widths, consider using a left navigation pane.",
                Margin = new Thickness(0, 0, 0, 12),
                TextWrapping = TextWrapping.Wrap
            });

            var navigationView = new Mux.NavigationView
            {
                Width = 745,
                Height = 460,
                HorizontalAlignment = HorizontalAlignment.Left,
                Header = "This is Header Text",
                IsTitleBarAutoPaddingEnabled = false,
                IsTabStop = false,
                PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Auto,
                Content = CreateNavigationSampleContent()
            };
            Grid.SetRow(navigationView, 1);
            GalleryAutomation.WithAutomationId(navigationView, GalleryAutomation.SampleElementId("NavigationView", "NavigationView"));

            var item1 = CreateNavigationItem("Menu Item1", Mux.Symbol.Play, "SamplePage1");
            navigationView.MenuItems.Add(item1);
            navigationView.MenuItems.Add(CreateNavigationItem("Menu Item2", Mux.Symbol.Save, "SamplePage2"));
            navigationView.MenuItems.Add(CreateNavigationItem("Menu Item3", Mux.Symbol.Refresh, "SamplePage3"));
            navigationView.MenuItems.Add(CreateNavigationItem("Menu Item4", Mux.Symbol.Download, "SamplePage4"));
            navigationView.SelectionChanged += delegate(Mux.NavigationView sender, Mux.NavigationViewSelectionChangedEventArgs args)
            {
                var item = args.SelectedItemContainer as Mux.NavigationViewItem;
                if (item != null)
                {
                    var selectedItemTag = item.Tag as string;
                    if (!string.IsNullOrEmpty(selectedItemTag))
                    {
                        sender.Header = "Sample Page " + selectedItemTag.Substring(selectedItemTag.Length - 1);
                    }
                }
            };
            navigationView.SelectedItem = item1;

            root.Children.Add(navigationView);
            panel.Children.Add(root);
            return panel;
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
                Margin = new Thickness(-12),
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

        private static Border CreateTabViewPageContent(string title)
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

        private static DataTemplate CreateTabViewDataContentTemplate()
        {
            var textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding("DataContent"));
            textBlock.SetValue(TextBlock.FontSizeProperty, 22.0);
            textBlock.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            textBlock.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlock.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.PaddingProperty, new Thickness(18));
            border.AppendChild(textBlock);
            return new DataTemplate(typeof(TabViewData))
            {
                VisualTree = border
            };
        }

        private static TabViewData CreateTabViewData(int index)
        {
            return new TabViewData
            {
                DataHeader = "MyData Doc " + index,
                DataContent = "SamplePage" + (index % 3 + 1)
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

        private static Mux.NavigationViewItem CreateNavigationItem(string content, Mux.Symbol symbol, string tag)
        {
            return new Mux.NavigationViewItem
            {
                Content = content,
                Icon = new Mux.SymbolIcon(symbol),
                Tag = tag
            };
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

        private sealed class TabViewData
        {
            public string DataHeader { get; set; }

            public string DataContent { get; set; }
        }
    }
}

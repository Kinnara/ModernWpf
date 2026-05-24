using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;
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
            switch (uniqueId)
            {
                case "BreadcrumbBar":
                    return CreateBreadcrumbBarExamples();
                case "Pivot":
                    return CreatePivotExamples();
                case "SelectorBar":
                    return CreateSelectorBarExamples();
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
            var panel = CreateSamplePanel("TabView maps to the ModernWpf-styled WPF TabControl with explicit add and close commands.");
            var tabControl = new TabControl
            {
                Width = 520,
                Height = 260
            };
            tabControl.Items.Add(CreateTab("Document 0", "Document 0 content"));
            tabControl.Items.Add(CreateTab("Document 1", "Document 1 content"));
            tabControl.Items.Add(CreateTab("Document 2", "Document 2 content"));
            tabControl.SelectedIndex = 0;

            var commands = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var add = CreateButton("Add tab");
            var close = CreateButton("Close selected");
            add.Click += delegate
            {
                var index = tabControl.Items.Count;
                var tab = CreateTab("Document " + index, "Document " + index + " content");
                tabControl.Items.Add(tab);
                tabControl.SelectedItem = tab;
            };
            close.Click += delegate
            {
                if (tabControl.SelectedItem is TabItem && tabControl.Items.Count > 1)
                {
                    var selected = (TabItem)tabControl.SelectedItem;
                    tabControl.Items.Remove(selected);
                }
            };
            commands.Children.Add(add);
            commands.Children.Add(close);

            panel.Children.Add(tabControl);
            panel.Children.Add(commands);
            return panel;
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
    }
}

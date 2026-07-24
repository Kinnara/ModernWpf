using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        private const string SamplePageLoremIpsum =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "BreadcrumbBar":
                    return CreateBreadcrumbBarSample();
                case "NavigationView":
                    return CreateNavigationViewSample();
                case "SelectorBar":
                    return CreateSelectorBarSample();
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
                case "SelectorBar":
                    return CreateSelectorBarExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateBreadcrumbBarSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("BreadcrumbBar"));
            panel.Children.Add(CreateBreadcrumbBarSimpleExampleContent(false));
            var templateContent = CreateBreadcrumbBarTemplateExampleContent(out var templateOptions);
            panel.Children.Add(CreateBreadcrumbBarExampleLayout(templateContent, templateOptions));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateBreadcrumbBarExamples()
        {
            var templateContent = CreateBreadcrumbBarTemplateExampleContent(out var templateOptions);
            return new[]
            {
                new GalleryExample(
                    "A BreadcrumbBar control",
                    CreateBreadcrumbBarSimpleExampleContent(true),
                    BreadcrumbBarSimpleXaml,
                    BreadcrumbBarSimpleCSharp),
                new GalleryExample(
                    "BreadCrumbBar Control with Custom DataTemplate",
                    templateContent,
                    BreadcrumbBarTemplateXaml,
                    BreadcrumbBarTemplateCSharp,
                    templateOptions)
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

        private static UIElement CreateBreadcrumbBarTemplateExampleContent(out UIElement optionsContent)
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
            GalleryAutomation.WithAutomationId(breadcrumbBar, GalleryAutomation.SampleElementId("BreadcrumbBar", "TemplateBreadcrumbBar"));
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

            optionsContent = resetSampleButton;
            return breadcrumbBar;
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
            var footerContent = CreateNavigationViewFooterExampleContent(out var footerOptions);
            var hierarchicalContent = CreateNavigationViewHierarchicalExampleContent(out var hierarchicalOptions);
            var apiContent = CreateNavigationViewApiExampleContent(out var apiOptions);
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
                    footerContent,
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample9_xaml.txt"),
                    null,
                    footerOptions),
                new GalleryExample(
                    "Hierarchical NavigationView",
                    hierarchicalContent,
                    FindSampleCodeText(sampleSnippets, "NavigationViewSample8_xaml.txt"),
                    null,
                    hierarchicalOptions),
                new GalleryExample(
                    "API in action",
                    apiContent,
                    NavigationViewApiXaml,
                    null,
                    apiOptions)
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
            GalleryAutomation.WithAutomationId(
                navigationView,
                GalleryAutomation.SampleElementId("NavigationView", "TopNavigationView"));
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
            GalleryAutomation.WithAutomationId(
                navigationView,
                GalleryAutomation.SampleElementId("NavigationView", "AdaptiveNavigationView"));
            AddStandardNavigationItems(navigationView, includeIcons: false, firstContent: "Menu Item1", remainingPrefix: "Menu Item");
            SelectFirstNavigationItem(navigationView);
            AttachAdaptiveNavigationViewPaneMode(root, navigationView);
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
            GalleryAutomation.WithAutomationId(
                navigationView,
                GalleryAutomation.SampleElementId("NavigationView", "TabsNavigationView"));
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
            GalleryAutomation.WithAutomationId(
                navigationView,
                GalleryAutomation.SampleElementId("NavigationView", "DataBindingNavigationView"));
            navigationView.MenuItemsSource = categories;
            navigationView.MenuItemTemplate = CreateNavigationCategoryTemplate();
            HookNavigationHeaderSelection(navigationView);
            navigationView.SelectedItem = categories[0];
            navigationView.Header = "Sample Page 1";
            root.Children.Add(navigationView);
            return root;
        }

        private static UIElement CreateNavigationViewFooterExampleContent(out UIElement optionsContent)
        {
            var root = CreateNavigationViewDescriptionRoot(
                "You can add clickable menu items to the footer of your NavigationView that participate in the same selection model as items in the main menu. In Top PaneDisplayMode, these items will appear aligned to the right of the NavigationView. In Left PaneDisplayMode, these items will appear aligned to the bottom of the NavigationView. ",
                false);

            var navigationView = CreateNavigationViewShell(
                "nvSample9",
                "contentFrame9",
                Mux.NavigationViewPaneDisplayMode.Left,
                "This is Header Text");
            GalleryAutomation.WithAutomationId(
                navigationView,
                GalleryAutomation.SampleElementId("NavigationView", "FooterNavigationView"));
            navigationView.Width = 592.0;
            navigationView.HorizontalAlignment = HorizontalAlignment.Left;
            navigationView.IsSettingsVisible = false;
            navigationView.MenuItems.Add(CreateNavigationItem("Browse", Mux.Symbol.Library, "SamplePage1"));
            navigationView.MenuItems.Add(CreateNavigationItem("Track an Order", Mux.Symbol.Map, "SamplePage2"));
            navigationView.MenuItems.Add(CreateNavigationItem("Order History", Mux.Symbol.Tag, "SamplePage3"));
            navigationView.FooterMenuItems.Add(CreateNavigationItem("Account", Mux.Symbol.Contact, "SamplePage4"));
            navigationView.FooterMenuItems.Add(CreateNavigationItem("Your Cart", Mux.Symbol.Shop, "SamplePage5"));
            navigationView.FooterMenuItems.Add(CreateNavigationItem("Help", Mux.Symbol.Help, "SamplePage5"));
            SelectFirstNavigationItem(navigationView);
            HookNavigationHeaderSelection(navigationView);
            root.Children.Add(navigationView);

            optionsContent = CreateNavigationPanePositionOptions(
                "Pane position:",
                "nvSample9Left",
                "nvSample9Top",
                null,
                navigationView);
            return root;
        }

        private static UIElement CreateNavigationViewHierarchicalExampleContent(out UIElement optionsContent)
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
            GalleryAutomation.WithAutomationId(
                navigationView,
                GalleryAutomation.SampleElementId("NavigationView", "HierarchicalNavigationView"));
            navigationView.Width = 565.0;
            navigationView.HorizontalAlignment = HorizontalAlignment.Left;

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
            HookNavigationHeaderSelection(navigationView);
            SelectFirstNavigationItem(navigationView);
            root.Children.Add(navigationView);

            optionsContent = CreateNavigationPanePositionOptions(
                "PanePosition:",
                "nvSample8Left",
                "nvSample8Top",
                "nvSample8LeftCompact",
                navigationView);
            return root;
        }

        private static UIElement CreateNavigationViewApiExampleContent(out UIElement optionsContent)
        {
            var navigationView = CreateNavigationViewShell(
                "nvSample",
                "contentFrame",
                Mux.NavigationViewPaneDisplayMode.Left,
                "Header");
            GalleryAutomation.WithAutomationId(
                navigationView,
                GalleryAutomation.SampleElementId("NavigationView", "ApiNavigationView"));
            navigationView.Width = 458.0;
            navigationView.HorizontalAlignment = HorizontalAlignment.Left;
            var contentFrame = new Frame
            {
                Name = "contentFrame",
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden
            };
            navigationView.Content = contentFrame;
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
            navigationView.SelectionChanged += delegate(Mux.NavigationView sender, Mux.NavigationViewSelectionChangedEventArgs args)
            {
                if (args.SelectedItemContainer is Mux.NavigationViewItem selectedItem && selectedItem.Tag is string)
                {
                    contentFrame.Content = CreateNavigationSampleContent();
                }
            };
            HookNavigationHeaderSelection(navigationView);

            optionsContent = CreateNavigationViewApiOptions(
                navigationView,
                paneHyperlink,
                footerStackPanel,
                samplePage2Item);
            return navigationView;
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
            panel.Children.Add(CreateSelectorBarItemsControlExampleContent());
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
                    CreateSelectorBarItemsControlExampleContent(),
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
                Name = "SelectorBar1",
                BorderThickness = new Thickness(0)
            };
            GalleryAutomation.WithAutomationId(selectorBar, GalleryAutomation.SampleElementId("SelectorBar", "SelectorBar"));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemRecent", "Recent", Mux.Symbol.Clock, false));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemShared", "Shared", Mux.Symbol.Share, false));
            selectorBar.Items.Add(CreateSelectorBarItem("SelectorBarItemFavorites", "Favorites", Mux.Symbol.Favorite, false));
            Action updateSelectionStatus = delegate
            {
                var selectedItem = selectorBar.SelectedItem as Mux.SelectorBarItem;
                AutomationProperties.SetItemStatus(selectorBar, selectedItem == null ? "" : selectedItem.Text);
            };
            selectorBar.SelectionChanged += delegate { updateSelectionStatus(); };
            updateSelectionStatus();

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
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden
            };

            Action updateContent = delegate
            {
                var currentSelectedIndex = selectorBar.Items.IndexOf(selectorBar.SelectedItem);
                if (currentSelectedIndex < 0)
                {
                    currentSelectedIndex = 0;
                }

                contentFrame.Content = CreateSelectorBarSamplePage(currentSelectedIndex + 1);
            };
            selectorBar.SelectionChanged += delegate { updateContent(); };
            selectorBar.SelectedItem = selectorBar.Items[0];
            updateContent();

            var stack = new StackPanel();
            stack.Children.Add(selectorBar);
            stack.Children.Add(contentFrame);
            return stack;
        }

        private static UIElement CreateSelectorBarItemsControlExampleContent()
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

            var itemsControl = new ItemsControl
            {
                Name = "ItemsView3",
                ItemTemplate = CreateSelectorBarColorTemplate(),
                ItemsSource = pinkColorCollection
            };
            var itemsPanel = new FrameworkElementFactory(typeof(StackPanel));
            itemsPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            itemsControl.ItemsPanel = new ItemsPanelTemplate(itemsPanel);

            selectorBar.SelectionChanged += delegate(Mux.SelectorBar sender, Mux.SelectorBarSelectionChangedEventArgs args)
            {
                if (sender.SelectedItem == pinkItem)
                {
                    itemsControl.ItemsSource = pinkColorCollection;
                }
                else if (sender.SelectedItem == plumItem)
                {
                    itemsControl.ItemsSource = plumColorCollection;
                }
                else
                {
                    itemsControl.ItemsSource = powderBlueColorCollection;
                }
            };
            selectorBar.SelectedItem = pinkItem;

            var stack = new StackPanel();
            stack.Children.Add(selectorBar);
            stack.Children.Add(itemsControl);
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
            GalleryAutomation.WithAutomationId(item, GalleryAutomation.SampleElementId("SelectorBar", name));
            item.Template = CreateSelectorBarItemTemplate();
            if (symbol.HasValue)
            {
                item.Icon = new Mux.SymbolIcon(GetSelectorBarRuntimeSymbol(symbol.Value));
            }

            return item;
        }

        private static Mux.Symbol GetSelectorBarRuntimeSymbol(Mux.Symbol sourceSymbol)
        {
            return sourceSymbol == Mux.Symbol.Favorite
                ? Mux.Symbol.OutlineStar
                : sourceSymbol;
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
            icon.SetValue(FrameworkElement.MarginProperty, new Thickness(-2, 0, 6, 0));
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
            text.SetValue(UIElement.RenderTransformProperty, new TranslateTransform(0, -1));
            content.AppendChild(text);

            var selectionPill = new FrameworkElementFactory(typeof(Rectangle));
            selectionPill.Name = "SelectionPill";
            selectionPill.SetValue(FrameworkElement.WidthProperty, 16.0);
            selectionPill.SetValue(FrameworkElement.HeightProperty, 3.0);
            selectionPill.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            selectionPill.SetValue(Shape.FillProperty, CreateBrush("#0067C0"));
            selectionPill.SetValue(Rectangle.RadiusXProperty, 1.0);
            selectionPill.SetValue(Rectangle.RadiusYProperty, 1.0);
            selectionPill.SetValue(UIElement.OpacityProperty, 0.0);

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
            selectedTrigger.Setters.Add(new Setter(UIElement.OpacityProperty, 1.0, "SelectionPill"));
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
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            border.SetBinding(Border.BackgroundProperty, new Binding());

            return new DataTemplate(typeof(SolidColorBrush))
            {
                VisualTree = border
            };
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

        private static void AttachAdaptiveNavigationViewPaneMode(FrameworkElement root, Mux.NavigationView navigationView)
        {
            void UpdatePaneMode()
            {
                var width = root.ActualWidth > 0 ? root.ActualWidth : navigationView.ActualWidth;
                navigationView.PaneDisplayMode = width >= navigationView.CompactModeThresholdWidth
                    ? Mux.NavigationViewPaneDisplayMode.Top
                    : Mux.NavigationViewPaneDisplayMode.Auto;
            }

            root.Loaded += delegate { UpdatePaneMode(); };
            root.SizeChanged += delegate { UpdatePaneMode(); };
            navigationView.SizeChanged += delegate { UpdatePaneMode(); };
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

        private static Grid CreateNavigationViewExampleLayout(
            UIElement sample,
            UIElement options,
            double optionsColumnWidth)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(optionsColumnWidth) });

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
                MinWidth = 160
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
                MinWidth = 160
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
            // WinUI's API sample uses the controls' native 32-DIP cadence here and
            // gives only the final radio button an explicit 12-DIP bottom margin.
            // The shared pane-mode options intentionally retain their own spacing.
            left.Margin = new Thickness(0);
            top.Margin = new Thickness(0, 0, 0, 12);
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
                IsChecked = isChecked
            };
        }

        private static TextBlock CreateOptionText(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 0)
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
            var sourceElement = AddNavigationTile(grid, 1, 0, 2, 1, GetSelectorBarAccentBrush(), 250);
            sourceElement.Name = "SourceElement";

            var text = new TextBlock
            {
                Text = SamplePageLoremIpsum,
                Margin = new Thickness(0, 1, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            text.SetResourceReference(FrameworkElement.StyleProperty, "BodyTextBlockStyle");
            text.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
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
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = grid
            };
        }

        private static Grid AddNavigationTile(Grid grid, int row, int column, int rowSpan, int columnSpan, Brush background, double minWidth)
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
            return tile;
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

        private static Page CreateSelectorBarSamplePage(int pageNumber)
        {
            UIElement content;
            switch (pageNumber)
            {
                case 2:
                    content = CreateSelectorBarSamplePage2Content();
                    break;
                case 3:
                    content = CreateSelectorBarSamplePage3Content();
                    break;
                case 4:
                    content = CreateSelectorBarSamplePage4Content();
                    break;
                case 5:
                    content = CreateSelectorBarSamplePage5Content();
                    break;
                default:
                    pageNumber = 1;
                    content = CreateNavigationSampleContent();
                    break;
            }

            return new Page
            {
                Title = "SamplePage" + pageNumber,
                Content = content
            };
        }

        private static UIElement CreateSelectorBarSamplePage2Content()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var destinationElement = new Grid
            {
                Name = "DestinationElement",
                Width = 150,
                Height = 200,
                MinHeight = 150,
                Margin = new Thickness(12),
                VerticalAlignment = VerticalAlignment.Top,
                Background = GetSelectorBarAccentBrush()
            };
            Grid.SetRow(destinationElement, 1);
            grid.Children.Add(destinationElement);

            var contentPanel = new StackPanel
            {
                Name = "ContentPanel",
                MinHeight = 200,
                Margin = new Thickness(12)
            };
            var title = new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 12),
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
                TextWrapping = TextWrapping.Wrap
            };
            title.SetResourceReference(FrameworkElement.StyleProperty, "TitleTextBlockStyle");
            contentPanel.Children.Add(title);
            contentPanel.Children.Add(CreateSamplePageBodyText());
            Grid.SetRow(contentPanel, 1);
            Grid.SetColumn(contentPanel, 1);
            grid.Children.Add(contentPanel);

            return new ScrollViewer { Content = grid };
        }

        private static UIElement CreateSelectorBarSamplePage3Content()
        {
            var grid = CreateSamplePageTileGrid(
                new GridLength(2, GridUnitType.Star),
                new GridLength(1, GridUnitType.Star),
                new GridLength(1, GridUnitType.Star));
            AddSamplePageTile(grid, 1, 0, 2, 1, Brushes.LightGray, new Thickness(5));
            AddSamplePageTile(grid, 1, 1, 1, 1, Brushes.DarkGray, new Thickness(5));
            AddSamplePageTile(grid, 2, 1, 1, 1, Brushes.Gray, new Thickness(5));
            AddSamplePageTile(grid, 1, 2, 1, 1, Brushes.LightGray, new Thickness(5));
            AddSamplePageTile(grid, 2, 2, 1, 1, Brushes.DarkGray, new Thickness(5));
            AddSamplePageText(grid, 3, 3, new Thickness(5));
            return new ScrollViewer { Content = grid };
        }

        private static UIElement CreateSelectorBarSamplePage4Content()
        {
            var stack = new StackPanel();

            var firstGrid = new Grid();
            firstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            firstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            firstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            AddSamplePageTile(firstGrid, 0, 0, 1, 1, Brushes.DarkSalmon, new Thickness(5));
            AddSamplePageTile(firstGrid, 0, 1, 1, 1, Brushes.DarkRed, new Thickness(5));
            AddSamplePageTile(firstGrid, 0, 2, 1, 1, Brushes.LightCoral, new Thickness(5));
            stack.Children.Add(firstGrid);

            var secondGrid = new Grid();
            secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            secondGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            secondGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            secondGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            AddSamplePageTile(secondGrid, 1, 1, 1, 1, Brushes.DarkRed, new Thickness(5));
            AddSamplePageTile(secondGrid, 1, 0, 1, 1, Brushes.LightCoral, new Thickness(5));
            AddSamplePageTile(secondGrid, 1, 2, 1, 1, Brushes.IndianRed, new Thickness(5));
            AddSamplePageText(secondGrid, 2, 3, new Thickness(5));
            stack.Children.Add(secondGrid);

            return new ScrollViewer { Content = stack };
        }

        private static UIElement CreateSelectorBarSamplePage5Content()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });
            for (var row = 0; row < 4; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            AddSamplePageTile(grid, 0, 0, 1, 1, Brushes.Khaki, new Thickness(5));
            AddSamplePageTile(grid, 0, 1, 1, 1, Brushes.DarkKhaki, new Thickness(5));

            var largeEllipse = new Ellipse
            {
                Width = 150,
                Height = 150,
                Fill = Brushes.DarkSeaGreen
            };
            Grid.SetColumn(largeEllipse, 2);
            grid.Children.Add(largeEllipse);

            var smallEllipse = new Ellipse
            {
                Width = 75,
                Height = 75,
                Fill = Brushes.MediumSeaGreen
            };
            Grid.SetRow(smallEllipse, 1);
            Grid.SetColumnSpan(smallEllipse, 2);
            grid.Children.Add(smallEllipse);

            AddSamplePageTile(grid, 1, 2, 1, 1, Brushes.DarkOliveGreen, new Thickness(5));
            AddSamplePageText(grid, 3, 3, new Thickness(5));
            return new ScrollViewer { Content = grid };
        }

        private static Grid CreateSamplePageTileGrid(params GridLength[] columnWidths)
        {
            var grid = new Grid();
            foreach (var width in columnWidths)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
            }

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            return grid;
        }

        private static Grid AddSamplePageTile(
            Grid grid,
            int row,
            int column,
            int rowSpan,
            int columnSpan,
            Brush background,
            Thickness margin)
        {
            var tile = new Grid
            {
                MinHeight = 150,
                Margin = margin,
                Background = background
            };
            Grid.SetRow(tile, row);
            Grid.SetColumn(tile, column);
            Grid.SetRowSpan(tile, rowSpan);
            Grid.SetColumnSpan(tile, columnSpan);
            grid.Children.Add(tile);
            return tile;
        }

        private static void AddSamplePageText(Grid grid, int row, int columnSpan, Thickness margin)
        {
            var host = new Grid
            {
                Margin = margin
            };
            host.Children.Add(CreateSamplePageBodyText());
            Grid.SetRow(host, row);
            Grid.SetColumnSpan(host, columnSpan);
            grid.Children.Add(host);
        }

        private static TextBlock CreateSamplePageBodyText()
        {
            return new TextBlock
            {
                Text = SamplePageLoremIpsum,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static Brush GetSelectorBarAccentBrush()
        {
            return Application.Current.TryFindResource("SystemControlBackgroundAccentBrush") as Brush
                ?? CreateBrush("#0078D4");
        }

        private static Style FindStyleResource(string key)
        {
            return Application.Current.TryFindResource(key) as Style;
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

    }
}

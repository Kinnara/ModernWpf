using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ModernWpf.Gallery.Testing;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class TabViewSampleFactory
    {
        private const string AddCloseXaml =
@"<TabView x:Name=""TabView1""
    CanDragTabs=""True""
    CanReorderTabs=""True""
    AddTabButtonClick=""TabView_AddTabButtonClick""
    TabCloseRequested=""TabView_TabCloseRequested"" />";

        private const string AddCloseCode =
@"private void TabView_AddTabButtonClick(TabView sender, object args)
{
    sender.TabItems.Add(CreateNewTab());
    sender.SelectedIndex = sender.TabItems.Count - 1;
}

private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
{
    // The application owns the collection; TabView only raises the request.
    sender.TabItems.Remove(args.Item);
}";

        private const string MarkupItemsXaml =
@"<TabView>
    <TabViewItem Header=""Document 0"" Content=""Document 0 content"" />
    <TabViewItem Header=""Document 1"" Content=""Document 1 content"" />
    <TabViewItem Header=""Document 2"" Content=""Document 2 content"" />
</TabView>";

        private const string DataSourceXaml =
@"<TabView TabItemsSource=""{Binding Documents}"">
    <TabView.TabItemTemplate>
        <DataTemplate>
            <TabViewItem Header=""{Binding Header}""
                         Content=""{Binding Content}""
                         IconSource=""{Binding IconSource}"" />
        </DataTemplate>
    </TabView.TabItemTemplate>
</TabView>";

        private const string DataSourceCode =
@"public ObservableCollection<Document> Documents { get; } = new();

private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
{
    Documents.Remove((Document)args.Item);
}";

        private const string KeyboardXaml =
@"<TabView x:Name=""KeyboardTabView"">
    <!-- Ctrl+Tab / Ctrl+Shift+Tab select; Ctrl+F4 requests close.
         Left and Right traverse tabs, close buttons, and the add button. -->
</TabView>";

        private const string HeaderFooterXaml =
@"<TabView>
    <TabView.TabStripHeader>
        <TextBlock Margin=""8,0"" Text=""Workspace"" />
    </TabView.TabStripHeader>
    <TabView.TabStripFooter>
        <Button Margin=""8,0"" Content=""Tab actions"" />
    </TabView.TabStripFooter>
</TabView>";

        private const string WidthXaml =
@"<TabView TabWidthMode=""Equal"">
    <!-- Equal, SizeToContent, and Compact are supported.
         Overflow buttons appear when the strip cannot fit every tab. -->
</TabView>";

        private const string OverlayXaml =
@"<TabView CloseButtonOverlayMode=""OnPointerOver"">
    <!-- Auto and Always keep close buttons visible.
         OnPointerOver keeps the selected tab visible and overlays the rest. -->
</TabView>";

        private const string ColorIconXaml =
@"<TabViewItem Header=""PowerShell"" IsClosable=""False"">
    <TabViewItem.IconSource>
        <ImageIconSource ImageSource=""/Assets/ControlImages/CommandBar.png"" />
    </TabViewItem.IconSource>
</TabViewItem>";

        private const string AccentXaml =
@"<TabView Foreground=""White"">
    <TabView.Resources>
        <SolidColorBrush x:Key=""TabViewBackground"" Color=""#5637C8"" />
        <SolidColorBrush x:Key=""TabViewItemHeaderBackgroundSelected"" Color=""#7859E8"" />
    </TabView.Resources>
</TabView>";

        private const string WindowingXaml =
@"<TabView CanDragTabs=""True""
         CanReorderTabs=""True""
         CanTearOutTabs=""True""
         TabTearOutWindowRequested=""TabView_TabTearOutWindowRequested""
         TabTearOutRequested=""TabView_TabTearOutRequested""
         ExternalTornOutTabsDropping=""TabView_ExternalTornOutTabsDropping""
         ExternalTornOutTabsDropped=""TabView_ExternalTornOutTabsDropped"" />";

        private const string WindowingCode =
@"private void TabView_TabTearOutWindowRequested(TabView sender,
    TabViewTabTearOutWindowRequestedEventArgs args)
{
    // WPF adaptation: the application creates its own Window and destination TabView.
    args.NewWindow = CreateDocumentWindow();
}

private void TabView_TabTearOutRequested(TabView sender,
    TabViewTabTearOutRequestedEventArgs args)
{
    MoveItems(sender, FindDestination(args.NewWindow), args.Items);
}

private void TabView_ExternalTornOutTabsDropping(TabView sender,
    TabViewExternalTornOutTabsDroppingEventArgs args)
{
    args.AllowDrop = true;
}";

        public static UIElement Create(string uniqueId)
        {
            if (!string.Equals(uniqueId, "TabView", StringComparison.Ordinal))
            {
                return null;
            }

            return CreateAddCloseExample();
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            if (!string.Equals(uniqueId, "TabView", StringComparison.Ordinal))
            {
                return Array.Empty<GalleryExample>();
            }

            var widthContent = CreateWidthExample(out var widthOptions);
            var overlayContent = CreateOverlayExample(out var overlayOptions);
            return new[]
            {
                new GalleryExample(
                    "A TabView with support for adding, closing, and rearranging tabs",
                    CreateAddCloseExample(),
                    AddCloseXaml,
                    AddCloseCode),
                new GalleryExample(
                    "A TabView with TabViewItems defined in markup",
                    CreateMarkupItemsExample(),
                    MarkupItemsXaml,
                    string.Empty),
                new GalleryExample(
                    "A TabView bound to a collection of MyData objects",
                    CreateDataSourceExample(),
                    DataSourceXaml,
                    DataSourceCode),
                new GalleryExample(
                    "A TabView with keyboarding support",
                    CreateKeyboardExample(),
                    KeyboardXaml,
                    string.Empty),
                new GalleryExample(
                    "You can put custom content in TabStripHeader and TabStripFooter",
                    CreateHeaderFooterExample(),
                    HeaderFooterXaml,
                    string.Empty),
                new GalleryExample(
                    "Tab widths can either be equally sized, sized to the content of the tab, or sized to only show the icon when unselected",
                    widthContent,
                    WidthXaml,
                    string.Empty,
                    widthOptions),
                new GalleryExample(
                    "The close button can be persistent or only visible on hover",
                    overlayContent,
                    OverlayXaml,
                    string.Empty,
                    overlayOptions),
                new GalleryExample(
                    "TabView with color tab icons",
                    CreateColorIconExample(),
                    ColorIconXaml,
                    string.Empty),
                new GalleryExample(
                    "A TabView with accent colored TabStrip background",
                    CreateAccentExample(),
                    AccentXaml,
                    string.Empty),
                new GalleryExample(
                    "Complete TabView windowing sample",
                    CreateWindowingExample(),
                    WindowingXaml,
                    WindowingCode)
            };
        }

        private static GallerySamplePanel CreateAddCloseExample()
        {
            var root = CreateRoot("AddClose", true);
            var status = CreateStatus("AddCloseStatus", "Add, close, drag, or right-click a tab.");
            var tabView = CreateTabView("TabView1", "TabView");
            tabView.CanDragTabs = true;
            tabView.CanReorderTabs = true;
            for (var index = 0; index < 4; index++)
            {
                AddConfiguredTab(tabView, status, "Document " + index, Mux.Symbol.Document);
            }

            tabView.AddTabButtonClick += delegate
            {
                var tab = AddConfiguredTab(
                    tabView,
                    status,
                    "Document " + tabView.TabItems.Count,
                    Mux.Symbol.Add);
                tabView.SelectedItem = tab;
                status.Text = "Added " + tab.Header + ".";
            };
            tabView.TabCloseRequested += delegate (Mux.TabView sender, Mux.TabViewTabCloseRequestedEventArgs args)
            {
                sender.TabItems.Remove(args.Item);
                status.Text = "Closed " + args.Tab.Header + ".";
            };
            tabView.TabItemsChanged += delegate
            {
                if (tabView.TabItems.Count != 0)
                {
                    status.Text = "Tab order: " + JoinHeaders(tabView);
                }
            };

            root.Children.Add(tabView);
            root.Children.Add(status);
            return root;
        }

        private static GallerySamplePanel CreateMarkupItemsExample()
        {
            var root = CreateRoot("MarkupItems");
            var tabView = CreateTabView("TabViewMarkupSample");
            tabView.IsAddTabButtonVisible = false;
            tabView.TabItems.Add(CreateTab("Document 0", "Document 0 content", Mux.Symbol.Document));
            tabView.TabItems.Add(CreateTab("Document 1", "Document 1 content", Mux.Symbol.Page));
            tabView.TabItems.Add(CreateTab("Document 2", "Document 2 content", Mux.Symbol.Pictures));
            root.Children.Add(tabView);
            return root;
        }

        private static GallerySamplePanel CreateDataSourceExample()
        {
            var root = CreateRoot("DataSource");
            var status = CreateStatus("DataSourceStatus", "The public collection remains application-owned.");
            var documents = new ObservableCollection<TabDocument>
            {
                new TabDocument("Home", "Home content", Mux.Symbol.Home),
                new TabDocument("Notes", "Notes content", Mux.Symbol.Edit),
                new TabDocument("Pictures", "Pictures content", Mux.Symbol.Pictures)
            };
            var tabView = CreateTabView("TabViewItemsSourceSample");
            tabView.TabItemsSource = documents;
            tabView.TabItemTemplate = CreateDocumentTemplate();
            tabView.AddTabButtonClick += delegate
            {
                documents.Add(new TabDocument(
                    "Document " + documents.Count,
                    "New observable document content",
                    Mux.Symbol.Document));
                tabView.SelectedItem = documents[documents.Count - 1];
                status.Text = "Observable collection count: " + documents.Count;
            };
            tabView.TabCloseRequested += delegate (Mux.TabView sender, Mux.TabViewTabCloseRequestedEventArgs args)
            {
                if (args.Item is TabDocument document)
                {
                    documents.Remove(document);
                    status.Text = "Observable collection count: " + documents.Count;
                }
            };

            root.Children.Add(tabView);
            root.Children.Add(status);
            return root;
        }

        private static GallerySamplePanel CreateKeyboardExample()
        {
            var root = CreateRoot("Keyboard");
            var status = CreateStatus(
                "KeyboardStatus",
                "Ctrl+Tab / Ctrl+Shift+Tab select; Ctrl+F4 requests close; arrows traverse tab controls.");
            var tabView = CreateTabView("TabView2");
            tabView.IsAddTabButtonVisible = true;
            for (var index = 1; index <= 4; index++)
            {
                tabView.TabItems.Add(CreateTab("Keyboard " + index, "Keyboard sample " + index, Mux.Symbol.Keyboard));
            }
            tabView.SelectionChanged += delegate
            {
                status.Text = "Selected: " + (tabView.SelectedTabHeader() ?? "none");
            };
            tabView.TabCloseRequested += delegate (Mux.TabView sender, Mux.TabViewTabCloseRequestedEventArgs args)
            {
                status.Text = "Ctrl+F4 or close requested: " + args.Tab.Header;
            };

            root.Children.Add(tabView);
            root.Children.Add(status);
            return root;
        }

        private static GallerySamplePanel CreateHeaderFooterExample()
        {
            var root = CreateRoot("HeaderFooter");
            var tabView = CreateTabView("TabViewHeaderFooterSample");
            tabView.TabStripHeader = new TextBlock
            {
                Margin = new Thickness(8, 0, 12, 0),
                Text = "Workspace",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold
            };
            var footerButton = new Button
            {
                Margin = new Thickness(8, 0, 0, 0),
                Content = "Tab actions"
            };
            GalleryAutomation.WithAutomationId(
                footerButton,
                GalleryAutomation.SampleElementId("TabView", "FooterActionButton"));
            tabView.TabStripFooter = footerButton;
            tabView.TabItems.Add(CreateTab("Overview", "Overview content", Mux.Symbol.Home));
            tabView.TabItems.Add(CreateTab("Activity", "Activity content", Mux.Symbol.Clock));
            root.Children.Add(tabView);
            return root;
        }

        private static GallerySamplePanel CreateWidthExample(out UIElement optionsContent)
        {
            var root = CreateRoot("Widths");
            var tabView = CreateTabView("TabView3");
            tabView.HorizontalAlignment = HorizontalAlignment.Left;
            for (var index = 1; index <= 12; index++)
            {
                tabView.TabItems.Add(CreateTab(
                    index % 3 == 0 ? "A much longer document " + index : "Tab " + index,
                    "Width and overflow sample " + index,
                    Mux.Symbol.Document));
            }

            var mode = new ComboBox
            {
                MinWidth = 180,
                ItemsSource = Enum.GetValues(typeof(Mux.TabViewWidthMode)),
                SelectedItem = Mux.TabViewWidthMode.Equal
            };
            AutomationProperties.SetName(mode, "Tab width mode");
            GalleryAutomation.WithAutomationId(
                mode,
                GalleryAutomation.SampleElementId("TabView", "TabWidthBehaviorComboBox"));
            mode.SelectionChanged += delegate
            {
                if (mode.SelectedItem is Mux.TabViewWidthMode selectedMode)
                {
                    tabView.TabWidthMode = selectedMode;
                }
            };

            optionsContent = CreateOption("Tab width mode", mode);
            root.Children.Add(tabView);
            return root;
        }

        private static GallerySamplePanel CreateOverlayExample(out UIElement optionsContent)
        {
            var root = CreateRoot("Overlay");
            var tabView = CreateTabView("TabView4");
            tabView.CloseButtonOverlayMode = Mux.TabViewCloseButtonOverlayMode.OnPointerOver;
            for (var index = 1; index <= 4; index++)
            {
                tabView.TabItems.Add(CreateTab("Document " + index, "Overlay sample " + index, Mux.Symbol.Document));
            }

            var mode = new ComboBox
            {
                MinWidth = 180,
                ItemsSource = Enum.GetValues(typeof(Mux.TabViewCloseButtonOverlayMode)),
                SelectedItem = Mux.TabViewCloseButtonOverlayMode.OnPointerOver
            };
            AutomationProperties.SetName(mode, "Close button overlay mode");
            GalleryAutomation.WithAutomationId(
                mode,
                GalleryAutomation.SampleElementId("TabView", "TabCloseButtonOverlayModeComboBox"));
            mode.SelectionChanged += delegate
            {
                if (mode.SelectedItem is Mux.TabViewCloseButtonOverlayMode selectedMode)
                {
                    tabView.CloseButtonOverlayMode = selectedMode;
                }
            };

            optionsContent = CreateOption("Close button overlay mode", mode);
            root.Children.Add(tabView);
            return root;
        }

        private static GallerySamplePanel CreateColorIconExample()
        {
            var root = CreateRoot("ColorIcons");
            var tabView = CreateTabView("TabViewColorIconsSample");
            tabView.IsAddTabButtonVisible = false;
            tabView.TabItems.Add(CreateImageTab("Command bar", "Command-bar content", "CommandBar.png"));
            tabView.TabItems.Add(CreateImageTab("Color picker", "Color-picker content", "ColorPicker.png"));
            tabView.TabItems.Add(CreateImageTab("Navigation", "Navigation content", "NavigationView.png"));
            root.Children.Add(tabView);
            return root;
        }

        private static GallerySamplePanel CreateAccentExample()
        {
            var root = CreateRoot("Accent");
            var tabView = CreateTabView("TabViewAccentSample");
            tabView.Foreground = Brushes.White;
            tabView.Resources["TabViewBackground"] = new SolidColorBrush(Color.FromRgb(86, 55, 200));
            tabView.Resources["TabViewItemHeaderBackgroundSelected"] = new SolidColorBrush(Color.FromRgb(120, 89, 232));
            tabView.Resources["TabViewItemHeaderForeground"] = Brushes.White;
            tabView.Resources["TabViewItemHeaderForegroundSelected"] = Brushes.White;
            for (var index = 1; index <= 3; index++)
            {
                tabView.TabItems.Add(CreateTab("Accent " + index, "Accent sample " + index, Mux.Symbol.Favorite));
            }
            root.Children.Add(tabView);
            return root;
        }

        private static GallerySamplePanel CreateWindowingExample()
        {
            var root = CreateRoot("Windowing");
            var status = CreateStatus(
                "WindowingStatus",
                "Drag a tab outside the strip to request an app-owned WPF Window; drag it back to rejoin.");
            var source = CreateTabView("TabViewWindowingSource");
            source.CanDragTabs = true;
            source.CanReorderTabs = true;
            source.CanTearOutTabs = true;
            source.TabItems.Add(CreateTab("Window document 1", "Window document 1 content", Mux.Symbol.Document));
            source.TabItems.Add(CreateTab("Window document 2", "Window document 2 content", Mux.Symbol.Document));

            Window tearOutWindow = null;
            Mux.TabView destination = null;

            Func<Window> ensureWindow = () =>
            {
                if (tearOutWindow != null)
                {
                    return tearOutWindow;
                }

                destination = CreateTabView("TabViewWindowingDestination");
                destination.CanDragTabs = true;
                destination.CanReorderTabs = true;
                destination.CanTearOutTabs = true;
                ConfigureExternalDrop(source, destination, status);
                ConfigureExternalDrop(destination, source, status);

                var rejoinButton = new Button
                {
                    Margin = new Thickness(0, 12, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Content = "Rejoin selected tab"
                };
                GalleryAutomation.WithAutomationId(
                    rejoinButton,
                    GalleryAutomation.SampleElementId("TabView", "RejoinButton"));
                rejoinButton.Click += delegate
                {
                    if (destination.SelectedItem != null)
                    {
                        var item = destination.SelectedItem;
                        destination.TabItems.Remove(item);
                        source.TabItems.Add(item);
                        source.SelectedItem = item;
                        status.Text = "Rejoined " + ((Mux.TabViewItem)item).Header + ".";
                    }

                    if (destination.TabItems.Count == 0)
                    {
                        tearOutWindow.Close();
                    }
                };

                var body = new StackPanel { Margin = new Thickness(16) };
                body.Children.Add(destination);
                body.Children.Add(rejoinButton);
                tearOutWindow = new Window
                {
                    Width = 620,
                    Height = 320,
                    Title = "ModernWPF TabView tear-out",
                    ShowInTaskbar = false,
                    Content = body
                };
                GalleryAutomation.WithAutomationId(
                    tearOutWindow,
                    GalleryAutomation.SampleElementId("TabView", "TearOutWindow"));
                tearOutWindow.Closed += delegate
                {
                    tearOutWindow = null;
                    destination = null;
                };
                return tearOutWindow;
            };

            source.TabDroppedOutside += delegate (Mux.TabView sender, Mux.TabViewTabDroppedOutsideEventArgs args)
            {
                status.Text = "Dropped outside: " + args.Tab.Header + ".";
            };
            source.TabTearOutWindowRequested += delegate (Mux.TabView sender, Mux.TabViewTabTearOutWindowRequestedEventArgs args)
            {
                args.NewWindow = ensureWindow();
            };
            source.TabTearOutRequested += delegate (Mux.TabView sender, Mux.TabViewTabTearOutRequestedEventArgs args)
            {
                foreach (var item in args.Items)
                {
                    sender.TabItems.Remove(item);
                    destination.TabItems.Add(item);
                    destination.SelectedItem = item;
                }
                status.Text = "Tore out " + args.Items.Length + " tab(s) into an app-owned WPF Window.";
            };

            var openButton = new Button
            {
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = "Open selected tab in a WPF window"
            };
            GalleryAutomation.WithAutomationId(
                openButton,
                GalleryAutomation.SampleElementId("TabView", "OpenTearOutWindowButton"));
            openButton.Click += delegate
            {
                if (source.SelectedItem == null)
                {
                    return;
                }

                var window = ensureWindow();
                var item = source.SelectedItem;
                source.TabItems.Remove(item);
                destination.TabItems.Add(item);
                destination.SelectedItem = item;
                if (!window.IsVisible)
                {
                    window.Owner = Window.GetWindow(root);
                    window.Show();
                }
                status.Text = "Opened " + ((Mux.TabViewItem)item).Header + " in an app-owned WPF Window.";
            };
            root.Unloaded += delegate
            {
                if (tearOutWindow != null)
                {
                    tearOutWindow.Close();
                }
            };

            root.Children.Add(source);
            root.Children.Add(openButton);
            root.Children.Add(status);
            return root;
        }

        private static void ConfigureExternalDrop(Mux.TabView source, Mux.TabView destination, TextBlock status)
        {
            destination.ExternalTornOutTabsDropping += delegate (
                Mux.TabView sender,
                Mux.TabViewExternalTornOutTabsDroppingEventArgs args)
            {
                args.AllowDrop = true;
            };
            destination.ExternalTornOutTabsDropped += delegate (
                Mux.TabView sender,
                Mux.TabViewExternalTornOutTabsDroppedEventArgs args)
            {
                foreach (var item in args.Items)
                {
                    source.TabItems.Remove(item);
                    sender.TabItems.Insert(Math.Min(args.DropIndex, sender.TabItems.Count), item);
                    sender.SelectedItem = item;
                }
                status.Text = "Rejoined external tab at index " + args.DropIndex + ".";
            };
        }

        private static Mux.TabViewItem AddConfiguredTab(
            Mux.TabView tabView,
            TextBlock status,
            string header,
            Mux.Symbol symbol)
        {
            var tab = CreateTab(header, header + " content", symbol);
            AttachMoveMenu(tabView, tab, status);
            tabView.TabItems.Add(tab);
            return tab;
        }

        private static void AttachMoveMenu(Mux.TabView tabView, Mux.TabViewItem tab, TextBlock status)
        {
            var menu = new ContextMenu();
            var moveLeft = new MenuItem { Header = "Move left" };
            var moveRight = new MenuItem { Header = "Move right" };
            moveLeft.Click += delegate
            {
                var index = tabView.TabItems.IndexOf(tab);
                if (index > 0)
                {
                    tabView.TabItems.Move(index, index - 1);
                    status.Text = "Moved " + tab.Header + " left.";
                }
            };
            moveRight.Click += delegate
            {
                var index = tabView.TabItems.IndexOf(tab);
                if (index >= 0 && index < tabView.TabItems.Count - 1)
                {
                    tabView.TabItems.Move(index, index + 1);
                    status.Text = "Moved " + tab.Header + " right.";
                }
            };
            menu.Items.Add(moveLeft);
            menu.Items.Add(moveRight);
            tab.ContextMenu = menu;
        }

        private static Mux.TabView CreateTabView(string name, string automationIdSuffix = null)
        {
            var tabView = new Mux.TabView
            {
                Name = name,
                MinWidth = 360,
                MaxWidth = 760,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            AutomationProperties.SetName(tabView, name);
            GalleryAutomation.WithAutomationId(
                tabView,
                GalleryAutomation.SampleElementId("TabView", automationIdSuffix ?? name));
            return tabView;
        }

        private static Mux.TabViewItem CreateTab(string header, string content, Mux.Symbol symbol)
        {
            var tab = new Mux.TabViewItem
            {
                Header = header,
                Content = new TextBlock
                {
                    Margin = new Thickness(16),
                    Text = content,
                    TextWrapping = TextWrapping.Wrap
                },
                IconSource = new Mux.SymbolIconSource { Symbol = symbol }
            };
            AutomationProperties.SetName(tab, header);
            return tab;
        }

        private static Mux.TabViewItem CreateImageTab(string header, string content, string imageFileName)
        {
            var uri = new Uri(
                "pack://application:,,,/ModernWpf.Gallery;component/Assets/ControlImages/" + imageFileName,
                UriKind.Absolute);
            var tab = CreateTab(header, content, Mux.Symbol.Document);
            tab.IconSource = new Mux.ImageIconSource { ImageSource = new BitmapImage(uri) };
            tab.IsClosable = false;
            return tab;
        }

        private static DataTemplate CreateDocumentTemplate()
        {
            var item = new FrameworkElementFactory(typeof(Mux.TabViewItem));
            item.SetBinding(Mux.TabViewItem.HeaderProperty, new Binding(nameof(TabDocument.Header)));
            item.SetBinding(Mux.TabViewItem.ContentProperty, new Binding(nameof(TabDocument.Content)));
            item.SetBinding(Mux.TabViewItem.IconSourceProperty, new Binding(nameof(TabDocument.IconSource)));
            item.SetBinding(Mux.TabViewItem.IsClosableProperty, new Binding(nameof(TabDocument.IsClosable)));
            return new DataTemplate(typeof(TabDocument)) { VisualTree = item };
        }

        private static GallerySamplePanel CreateRoot(string suffix, bool primary = false)
        {
            var root = new GallerySamplePanel
            {
                MinWidth = 360,
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(
                root,
                primary
                    ? GalleryAutomation.SampleRootId("TabView")
                    : GalleryAutomation.SampleElementId("TabView", suffix + "Root"));
            return root;
        }

        private static TextBlock CreateStatus(string suffix, string text)
        {
            var status = new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                Text = text,
                TextWrapping = TextWrapping.Wrap
            };
            AutomationProperties.SetName(status, text);
            GalleryAutomation.WithAutomationId(
                status,
                GalleryAutomation.SampleElementId("TabView", suffix));
            return status;
        }

        private static StackPanel CreateOption(string label, UIElement control)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 8),
                Text = label,
                FontWeight = FontWeights.SemiBold
            });
            panel.Children.Add(control);
            return panel;
        }

        private static string JoinHeaders(Mux.TabView tabView)
        {
            var headers = new List<string>();
            foreach (var item in tabView.TabItems)
            {
                headers.Add(((Mux.TabViewItem)item).Header?.ToString() ?? string.Empty);
            }
            return string.Join(", ", headers);
        }

        private static string SelectedTabHeader(this Mux.TabView tabView)
        {
            return (tabView.SelectedItem as Mux.TabViewItem)?.Header?.ToString();
        }

        private sealed class TabDocument
        {
            public TabDocument(string header, string content, Mux.Symbol symbol)
            {
                Header = header;
                Content = content;
                IconSource = new Mux.SymbolIconSource { Symbol = symbol };
            }

            public string Header { get; }

            public string Content { get; }

            public Mux.IconSource IconSource { get; }

            public bool IsClosable { get; } = true;
        }
    }
}

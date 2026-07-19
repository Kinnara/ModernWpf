using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class WindowingSampleFactory
    {
        private const string TitleBarConfigurationXaml =
@"<TitleBar
    Title=""$(Title)""
    Subtitle=""$(Subtitle)""
    IsBackButtonVisible=""$(BackButtonVisibility)""
    IsPaneToggleButtonVisible=""$(PaneToggleVisibility)"">
    <TitleBar.IconSource>
        <ImageIconSource ImageSource=""/Assets/Tiles/GalleryIcon.ico"" />
    </TitleBar.IconSource>
    <TitleBar.Content>
        <AutoSuggestBox
            Width=""360""
            VerticalAlignment=""Center""
            PlaceholderText=""Search..""
            QueryIcon=""Find"" />
    </TitleBar.Content>
    <TitleBar.RightHeader>
        <PersonPicture
            Width=""30""
            Height=""30""
            Initials=""JD"" />
    </TitleBar.RightHeader>
</TitleBar>";

        private const string TitleBarDragRegionsXaml =
@"<!-- Starting with WindowsAppSDK 2.1, TitleBar walks TitleBar.Content,
     auto-excludes interactive controls from the drag region, and lets
     non-interactive visuals (and empty space) remain draggable.

     Use TitleBar.IsDragRegion to override the framework decision:
       True   -> always draggable
       False  -> always clickable
       unset  -> framework decides (default) -->
<TitleBar x:Name=""titleBar"" Title=""Drag regions"">
    <TitleBar.Resources>
        <HorizontalAlignment x:Key=""TitleBarContentHorizontalAlignment"">Stretch</HorizontalAlignment>
    </TitleBar.Resources>
    <TitleBar.Content>
        <Grid ColumnSpacing=""8"" HorizontalAlignment=""Stretch"">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width=""*"" />
                <ColumnDefinition Width=""Auto"" />
            </Grid.ColumnDefinitions>
            <AutoSuggestBox
                MaxWidth=""580""
                HorizontalAlignment=""Stretch""
                VerticalAlignment=""Center""
                PlaceholderText=""Search...""
                QueryIcon=""Find"" />
            <Button
                x:Name=""StatusBadge""
                Grid.Column=""1""
                VerticalAlignment=""Center""
                Click=""StatusBadge_Click""
                Content=""Status""
                Style=""{StaticResource AccentButtonStyle}"" />
        </Grid>
    </TitleBar.Content>
</TitleBar>";

        private const string TitleBarDragRegionsCSharp =
@"// Set TitleBar.IsDragRegion at runtime.
TitleBar.SetIsDragRegion(StatusBadge, true);   // always draggable
TitleBar.SetIsDragRegion(StatusBadge, false);  // always clickable
StatusBadge.ClearValue(TitleBar.IsDragRegionProperty); // back to default

// After adding or removing elements in TitleBar.Content dynamically,
// ask the framework to recompute drag regions.
titleBar.RecomputeDragRegions();";

        private const string TitleBarEndToEndXaml =
@"<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto"" />
        <!--  TitleBar  -->
        <RowDefinition Height=""*"" />
        <!--  NavigationView  -->
    </Grid.RowDefinitions>

    <TitleBar
        x:Name=""titleBar""
        BackRequested=""TitleBar_BackRequested""
        IsBackButtonVisible=""{x:Bind navFrame.CanGoBack, Mode=OneWay}""
        IsPaneToggleButtonVisible=""True""
        PaneToggleRequested=""TitleBar_PaneToggleRequested"" />

    <NavigationView
        x:Name=""navView""
        Grid.Row=""1""
        IsBackButtonVisible=""Collapsed""
        IsPaneToggleButtonVisible=""False"">
        <NavigationView.MenuItems... />
        <Frame x:Name=""navFrame"" />
    </NavigationView>
</Grid>";

        private const string TitleBarEndToEndCSharp =
@"this.ExtendsContentIntoTitleBar = true; // Extend the content into the title bar and hide the default titlebar
this.SetTitleBar(titleBar); // Set the custom title bar";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "TitleBar":
                    return CreateTitleBarSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets = null)
        {
            switch (uniqueId)
            {
                case "TitleBar":
                    return CreateTitleBarExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement CreateIntroContent(string uniqueId)
        {
            switch (uniqueId)
            {
                case "TitleBar":
                    return CreateTitleBarIntroContent();
                default:
                    return null;
            }
        }

        private static UIElement CreateTitleBarSample()
        {
            var content = CreateTitleBarConfigurationExampleContent(
                assignRootAutomationId: true,
                out var optionsContent);
            var layout = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            layout.Children.Add(content);
            layout.Children.Add(new Border
            {
                Margin = new Thickness(24, 0, 0, 0),
                Child = optionsContent
            });
            return layout;
        }

        private static TextBlock CreateTitleBarIntroContent()
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Text = "For full title bar customization without using the TitleBar control, see the AppWindowTitleBar sample"
            };
            return textBlock;
        }

        private static IReadOnlyList<GalleryExample> CreateTitleBarExamples()
        {
            var configurationContent = CreateTitleBarConfigurationExampleContent(
                assignRootAutomationId: true,
                out var configurationOptions);
            return new[]
            {
                new GalleryExample(
                    "TitleBar configuration",
                    configurationContent,
                    TitleBarConfigurationXaml,
                    null,
                    configurationOptions)
                    .WithContentAlignment(HorizontalAlignment.Stretch, VerticalAlignment.Center),
                new GalleryExample(
                    "End to end TitleBar sample",
                    CreateTitleBarEndToEndExampleContent(),
                    TitleBarEndToEndXaml,
                    TitleBarEndToEndCSharp)
                    .WithContentAlignment(HorizontalAlignment.Stretch, VerticalAlignment.Center)
            };
        }

        private static GallerySamplePanel CreateTitleBarConfigurationExampleContent(
            bool assignRootAutomationId,
            out UIElement optionsContent)
        {
            var root = new GallerySamplePanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("TitleBar"));
            }

            var titleText = new TextBlock
            {
                Name = "TitleText",
                Text = "WinUI Gallery",
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            var subtitleText = new TextBlock
            {
                Name = "SubtitleText",
                Text = "Preview",
                FontSize = 12,
                Opacity = 0.72,
                VerticalAlignment = VerticalAlignment.Top
            };

            var backButton = CreateTitleBarPreviewButton("BackButton", Mux.Symbol.Back);
            var paneButton = CreateTitleBarPreviewButton("PaneToggleButton", Mux.Symbol.OpenPane);

            var titleBarControl = new ContentControl
            {
                Name = "TitleBarControl",
                Width = 470,
                Height = 48,
                HorizontalAlignment = HorizontalAlignment.Left,
                ClipToBounds = true,
                Focusable = false
            };
            AutomationProperties.SetName(titleBarControl, "TitleBarControl");
            GalleryAutomation.WithAutomationId(titleBarControl, GalleryAutomation.SampleElementId("TitleBar", "TitleBarControl"));
            var titleBarRoot = new Grid();
            var titleBarBackground = new Border
            {
                Name = "TitleBarSurface",
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(-1)
            };
            titleBarBackground.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
            titleBarBackground.SetResourceReference(Border.BorderBrushProperty, "SurfaceStrokeColorDefaultBrush");
            titleBarRoot.Children.Add(titleBarBackground);

            var titleBarGrid = new Grid();
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(backButton, 0);
            Grid.SetColumn(paneButton, 1);
            titleBarGrid.Children.Add(backButton);
            titleBarGrid.Children.Add(paneButton);

            var icon = new Image
            {
                Name = "TitleBarIcon",
                Width = 16,
                Height = 16,
                Margin = new Thickness(14, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Source = CreateBitmap(ResourceUri("Assets/Tiles/GalleryIcon.ico"))
            };
            Grid.SetColumn(icon, 2);
            titleBarGrid.Children.Add(icon);

            var titleStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };
            titleStack.Children.Add(titleText);
            titleStack.Children.Add(subtitleText);
            Grid.SetColumn(titleStack, 5);
            titleBarGrid.Children.Add(titleStack);

            var searchBox = new Mux.AutoSuggestBox
            {
                Name = "TitleBarSearchBox",
                Width = 186,
                VerticalAlignment = VerticalAlignment.Center,
                PlaceholderText = "Search..",
                QueryIcon = new Mux.SymbolIcon(Mux.Symbol.Find),
                Margin = new Thickness(0, 0, 16, 0)
            };
            GalleryAutomation.WithAutomationId(searchBox, GalleryAutomation.SampleElementId("TitleBar", "SearchBox"));
            Grid.SetColumn(searchBox, 3);
            titleBarGrid.Children.Add(searchBox);

            var personPicture = new Mux.PersonPicture
            {
                Name = "TitleBarRightHeader",
                Width = 30,
                Height = 30,
                Initials = "JD",
                Margin = new Thickness(0, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(personPicture, 4);
            titleBarGrid.Children.Add(personPicture);
            titleBarRoot.Children.Add(titleBarGrid);
            titleBarControl.Content = titleBarRoot;

            var titleBox = new TextBox
            {
                Name = "TitleBox",
                Text = "WinUI Gallery"
            };
            ControlHelper.SetHeader(titleBox, "Title");

            var subtitleBox = new TextBox
            {
                Name = "SubtitleBox",
                Text = "Preview"
            };
            ControlHelper.SetHeader(subtitleBox, "Subtitle");

            var backButtonToggle = new Mux.ToggleSwitch
            {
                Name = "BackButtonToggle",
                Header = "IsBackButtonVisible",
                IsOn = false,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var paneToggle = new Mux.ToggleSwitch
            {
                Name = "PaneToggle",
                Header = "IsPaneToggleButtonVisible",
                IsOn = false,
                Margin = new Thickness(0, 12, 0, 0)
            };

            Action updatePreview = delegate
            {
                titleText.Text = titleBox.Text;
                subtitleText.Text = subtitleBox.Text;
                backButton.Visibility = backButtonToggle.IsOn ? Visibility.Visible : Visibility.Collapsed;
                paneButton.Visibility = paneToggle.IsOn ? Visibility.Visible : Visibility.Collapsed;
            };
            titleBox.TextChanged += delegate { updatePreview(); };
            subtitleBox.TextChanged += delegate { updatePreview(); };
            backButtonToggle.Toggled += delegate { updatePreview(); };
            paneToggle.Toggled += delegate { updatePreview(); };

            var options = new StackPanel
            {
                Width = 240,
                Orientation = Orientation.Vertical
            };
            // WinUI TextBox reserves an 8-DIP gap between its built-in Header
            // presenter and input chrome. WPF uses an explicit header element
            // for the port, so preserve that same vertical metric here.
            options.Children.Add(CreateTitleBarOptionHeader("TitleHeader", "Title", new Thickness(0, 0, 0, 8)));
            options.Children.Add(titleBox);
            options.Children.Add(CreateTitleBarOptionHeader("SubtitleHeader", "Subtitle", new Thickness(0, 12, 0, 8)));
            options.Children.Add(subtitleBox);
            options.Children.Add(backButtonToggle);
            options.Children.Add(paneToggle);

            root.Children.Add(titleBarControl);
            optionsContent = options;
            updatePreview();
            return root;
        }

        private static TextBlock CreateTitleBarOptionHeader(string name, string text, Thickness margin)
        {
            var header = new TextBlock
            {
                Name = name,
                Text = text,
                Margin = margin
            };
            header.SetResourceReference(TextBlock.FontSizeProperty, "BodyTextBlockFontSize");
            header.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
            return header;
        }

        private static GallerySamplePanel CreateTitleBarDragRegionsExampleContent()
        {
            var root = new GallerySamplePanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            var stack = new StackPanel
            {
                MaxWidth = 560,
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Drag regions can only be observed on a real window. Click the button below to open a sample window where you can toggle TitleBar.IsDragRegion on a status badge and call RecomputeDragRegions() after dynamic content changes.",
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });

            var showWindowButton = new Button
            {
                Name = "ShowTitleBarDragRegionsWindowButton",
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };
            GalleryAutomation.WithAutomationId(
                showWindowButton,
                GalleryAutomation.SampleElementId("TitleBar", "DragRegionsShowWindowButton"));
            showWindowButton.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
            showWindowButton.Click += delegate
            {
                var window = CreateModernWindow(
                    showWindowButton,
                    "TitleBar drag regions sample",
                    900,
                    640);
                Mux.TitleBar.SetExtendViewIntoTitleBar(window, true);
                Mux.TitleBar.SetIsIconVisible(window, false);
                window.Content = CreateTitleBarDragRegionsWindowBody(window);
                window.Show();
            };
            stack.Children.Add(showWindowButton);
            root.Children.Add(stack);
            return root;
        }

        private static FrameworkElement CreateTitleBarDragRegionsWindowBody(Window window)
        {
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(48) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var titleBar = new Grid
            {
                Name = "DragRegionsTitleBar",
                Height = 48,
                Margin = new Thickness(14, 0, 140, 0)
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var icon = new Image
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Source = CreateBitmap(ResourceUri("Assets/Tiles/GalleryIcon.ico"))
            };
            titleBar.Children.Add(icon);

            var titleStack = new StackPanel
            {
                Margin = new Thickness(0, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            titleStack.Children.Add(new TextBlock
            {
                Text = "Drag regions",
                FontWeight = FontWeights.SemiBold
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = "Try dragging the window",
                FontSize = 12,
                Opacity = 0.72
            });
            Grid.SetColumn(titleStack, 1);
            titleBar.Children.Add(titleStack);

            var searchBox = new Mux.AutoSuggestBox
            {
                Name = "DragRegionsSearchBox",
                MaxWidth = 580,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                PlaceholderText = "Search...",
                QueryIcon = new Mux.SymbolIcon(Mux.Symbol.Find),
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(searchBox, 2);
            titleBar.Children.Add(searchBox);

            var rightHeaderPanel = new StackPanel
            {
                Name = "RightHeaderPanel",
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            var statusBadge = new Button
            {
                Name = "StatusBadge",
                Content = "Status",
                VerticalAlignment = VerticalAlignment.Center
            };
            statusBadge.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
            rightHeaderPanel.Children.Add(statusBadge);
            Grid.SetColumn(rightHeaderPanel, 3);
            titleBar.Children.Add(rightHeaderPanel);
            titleBar.MouseLeftButtonDown += delegate(object sender, System.Windows.Input.MouseButtonEventArgs args)
            {
                if (args.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                {
                    window.DragMove();
                }
            };
            root.Children.Add(titleBar);

            var body = new StackPanel
            {
                Name = "DragRegionsBody",
                MaxWidth = 640,
                Margin = new Thickness(32, 24, 32, 24)
            };
            body.Children.Add(new TextBlock
            {
                Text = "Custom drag regions",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            });
            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 16, 0, 0),
                Text = "Try dragging the window from different parts of the title bar. Interactive controls (like the search box) are automatically excluded from the drag region by the new default behavior in Windows App SDK 2.1.",
                TextWrapping = TextWrapping.Wrap
            });
            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 16, 0, 0),
                Text = "Status badge: TitleBar.IsDragRegion",
                FontWeight = FontWeights.SemiBold
            });
            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 6, 0, 0),
                Text = "Pick a value for the badge in the title bar.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            });

            var dragRegionOptions = new Mux.RadioButtons
            {
                Name = "BadgeIsDragRegionRadios",
                SelectedIndex = 0,
                Margin = new Thickness(0, 8, 0, 0)
            };
            dragRegionOptions.Items.Add("Unset (framework decides — clickable, since Button is interactive)");
            dragRegionOptions.Items.Add("True (always draggable — overrides the framework default)");
            dragRegionOptions.Items.Add("False (always clickable)");
            body.Children.Add(dragRegionOptions);

            var statusText = new TextBlock
            {
                Name = "StatusText",
                Margin = new Thickness(0, 16, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            };
            statusBadge.Click += delegate { statusText.Text = "Status badge clicked"; };

            System.Windows.Input.MouseButtonEventHandler forceDragHandler = delegate(object sender, System.Windows.Input.MouseButtonEventArgs args)
            {
                if (args.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                {
                    args.Handled = true;
                    window.DragMove();
                }
            };
            dragRegionOptions.SelectionChanged += delegate
            {
                statusBadge.PreviewMouseLeftButtonDown -= forceDragHandler;
                if (dragRegionOptions.SelectedIndex == 1)
                {
                    statusBadge.PreviewMouseLeftButtonDown += forceDragHandler;
                }
            };

            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 16, 0, 0),
                Text = "Dynamic content",
                FontWeight = FontWeights.SemiBold
            });
            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 6, 0, 0),
                Text = "When you add or remove elements in TitleBar.Content at runtime, call RecomputeDragRegions() to refresh.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            });

            var actionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Button extraButton = null;
            var toggleExtraButton = new Button
            {
                Name = "ToggleExtraTitleBarButton",
                Content = "Toggle extra title bar button"
            };
            toggleExtraButton.Click += delegate
            {
                if (extraButton == null)
                {
                    extraButton = new Button
                    {
                        Name = "ExtraTitleBarButton",
                        Content = "Extra",
                        Margin = new Thickness(0, 0, 8, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    rightHeaderPanel.Children.Insert(0, extraButton);
                    statusText.Text = "Added a Button to TitleBar.Content. WPF updates its live drag/input tree automatically.";
                }
                else
                {
                    rightHeaderPanel.Children.Remove(extraButton);
                    extraButton = null;
                    statusText.Text = "Removed the Button. WPF updates its live drag/input tree automatically.";
                }
            };
            actionPanel.Children.Add(toggleExtraButton);

            var recomputeButton = new Button
            {
                Name = "RecomputeDragRegionsButton",
                Content = "RecomputeDragRegions()",
                Margin = new Thickness(8, 0, 0, 0)
            };
            recomputeButton.Click += delegate
            {
                statusText.Text = "WPF drag regions follow the live visual/input tree; no explicit recomputation is required.";
            };
            actionPanel.Children.Add(recomputeButton);
            body.Children.Add(actionPanel);
            body.Children.Add(statusText);

            var scrollViewer = new ScrollViewer
            {
                Content = body,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);
            return root;
        }

        private static GallerySamplePanel CreateTitleBarEndToEndExampleContent()
        {
            var root = new GallerySamplePanel();
            var stack = new StackPanel
            {
                MaxWidth = 560,
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Click the button below to see an end to end sample of a TitleBar in an new window, binding some of its properties to the NavigationView and navigation frame.",
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });

            var showWindowButton = new Button
            {
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };
            showWindowButton.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
            showWindowButton.Click += delegate
            {
                var window = CreateModernWindow((FrameworkElement)showWindowButton, "TitleBarWindow", 760, 520);
                Mux.TitleBar.SetExtendViewIntoTitleBar(window, true);
                Mux.TitleBar.SetIsBackButtonVisible(window, true);
                Mux.TitleBar.SetIsBackEnabled(window, false);
                window.Content = CreateTitleBarWindowBody();
                window.Show();
            };
            stack.Children.Add(showWindowButton);
            root.Children.Add(stack);
            return root;
        }

        private static Button CreateTitleBarPreviewButton(string name, Mux.Symbol symbol)
        {
            var button = new Button
            {
                Name = name,
                Width = 40,
                Height = 40,
                Padding = new Thickness(0),
                Margin = new Thickness(4, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed,
                Content = new Mux.SymbolIcon(symbol)
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("TitleBar", name));
            return button;
        }

        private static FrameworkElement CreateTitleBarWindowBody()
        {
            var frame = new Frame
            {
                Name = "navFrame",
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                Content = new Border
                {
                    Padding = new Thickness(32),
                    Child = new TextBlock
                    {
                        Text = "Sample page content",
                        FontSize = 24,
                        FontWeight = FontWeights.SemiBold
                    }
                }
            };

            var navigationView = new Mux.NavigationView
            {
                Name = "navView",
                IsBackButtonVisible = Mux.NavigationViewBackButtonVisible.Collapsed,
                IsPaneToggleButtonVisible = false,
                IsTitleBarAutoPaddingEnabled = false,
                Content = frame
            };
            navigationView.MenuItems.Add(new Mux.NavigationViewItem
            {
                Content = "Home",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Home)
            });
            navigationView.MenuItems.Add(new Mux.NavigationViewItem
            {
                Content = "Documents",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Document)
            });

            return navigationView;
        }

        private static Window CreateModernWindow(FrameworkElement ownerElement, string title, double width, double height)
        {
            var window = new Window
            {
                Title = title,
                Width = width,
                Height = height,
                MinWidth = 360,
                MinHeight = 240,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = CreateBrush("#F9F9F9")
            };
            var owner = Window.GetWindow(ownerElement);
            if (owner != null)
            {
                window.Owner = owner;
            }
            ThemeManager.SetIsThemeAware(window, true);
            WindowHelper.SetUseModernWindowStyle(window, true);
            Mux.TitleBar.SetIsIconVisible(window, true);
            return window;
        }

        private static Border CreateWindowPreview(string title, string subtitle, Brush titleBarBrush, Brush titleBrush)
        {
            var titleText = new TextBlock
            {
                Name = "PreviewTitle",
                Text = title,
                Foreground = titleBrush,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            };
            var subtitleText = new TextBlock
            {
                Name = "PreviewSubtitle",
                Text = subtitle,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(22, 18, 22, 0)
            };
            var icon = new Rectangle
            {
                Name = "Icon",
                Width = 14,
                Height = 14,
                Fill = titleBrush,
                RadiusX = 3,
                RadiusY = 3,
                Margin = new Thickness(14, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var titleBar = new Grid
            {
                Name = "PreviewChrome",
                Height = 38,
                Background = titleBarBrush
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(icon, 0);
            Grid.SetColumn(titleText, 1);
            titleBar.Children.Add(icon);
            titleBar.Children.Add(titleText);
            titleBar.Children.Add(CreateCaptionButtons(titleBrush));

            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(titleBar, 0);
            Grid.SetRow(subtitleText, 1);
            content.Children.Add(titleBar);
            content.Children.Add(subtitleText);

            return new Border
            {
                Width = 520,
                Height = 250,
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#C8C8C8"),
                Background = Brushes.White,
                Child = content
            };
        }

        private static Border CreateInteractiveTitleBarPreview()
        {
            var titleBar = new Grid
            {
                Height = 44,
                Background = CreateBrush("#F9F9F9")
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var back = new Button
            {
                Name = "BackButton",
                Content = "<",
                Width = 44,
                Height = 32,
                Margin = new Thickness(6, 6, 0, 6)
            };
            var icon = new Rectangle
            {
                Name = "Icon",
                Width = 16,
                Height = 16,
                Fill = CreateBrush("#0078D4"),
                RadiusX = 4,
                RadiusY = 4,
                Margin = new Thickness(10, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var title = new TextBlock
            {
                Text = "ModernWpf Gallery",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 16, 0)
            };
            var search = new TextBox
            {
                Text = "Interactive content",
                Width = 180,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 6, 16, 6)
            };
            var buttons = CreateCaptionButtons(CreateBrush("#202020"));

            Grid.SetColumn(back, 0);
            Grid.SetColumn(icon, 1);
            Grid.SetColumn(title, 2);
            Grid.SetColumn(search, 3);
            Grid.SetColumn(buttons, 4);
            titleBar.Children.Add(back);
            titleBar.Children.Add(icon);
            titleBar.Children.Add(title);
            titleBar.Children.Add(search);
            titleBar.Children.Add(buttons);

            var body = new Border
            {
                Padding = new Thickness(20),
                Child = new TextBlock
                {
                    Text = "The preview represents a ModernWpf title bar with drag region controls and optional interactive content.",
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.72
                }
            };
            var root = new Grid
            {
                Background = Brushes.White
            };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(titleBar, 0);
            Grid.SetRow(body, 1);
            root.Children.Add(titleBar);
            root.Children.Add(body);

            return new Border
            {
                Width = 560,
                Height = 190,
                BorderBrush = CreateBrush("#C8C8C8"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Child = root
            };
        }

        private static StackPanel CreateCaptionButtons(Brush foreground)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(panel, 2);
            panel.Children.Add(CreateCaptionGlyph("_", foreground));
            panel.Children.Add(CreateCaptionGlyph("[]", foreground));
            panel.Children.Add(CreateCaptionGlyph("X", foreground));
            return panel;
        }

        private static TextBlock CreateCaptionGlyph(string text, Brush foreground)
        {
            return new TextBlock
            {
                Text = text,
                Width = 44,
                Height = 38,
                Foreground = foreground,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0, 9, 0, 0)
            };
        }

        private static void ApplyPreviewChrome(Border preview, Brush background, Brush foreground, bool iconVisible)
        {
            var chrome = FindNamedElement<Panel>(preview, "PreviewChrome");
            if (chrome != null)
            {
                chrome.Background = background;
            }
            var title = FindNamedElement<TextBlock>(preview, "PreviewTitle");
            if (title != null)
            {
                title.Foreground = foreground;
            }
            var icon = FindNamedElement<Rectangle>(preview, "Icon");
            if (icon != null)
            {
                icon.Fill = foreground;
                icon.Visibility = iconVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void SetPreviewText(Border preview, string title, string subtitle)
        {
            var titleBlock = FindNamedElement<TextBlock>(preview, "PreviewTitle");
            var subtitleBlock = FindNamedElement<TextBlock>(preview, "PreviewSubtitle");
            if (titleBlock != null)
            {
                titleBlock.Text = title;
            }
            if (subtitleBlock != null)
            {
                subtitleBlock.Text = subtitle;
            }
        }

        private static void SetNamedElementVisibility(Border root, string name, bool isVisible)
        {
            var element = FindNamedElement<UIElement>(root, name);
            if (element != null)
            {
                element.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void SetNamedElementOpacity(Border root, string name, double opacity)
        {
            var element = FindNamedElement<UIElement>(root, name);
            if (element != null)
            {
                element.Opacity = opacity;
            }
        }

        private static T FindNamedElement<T>(DependencyObject root, string name)
            where T : UIElement
        {
            if (root == null)
            {
                return null;
            }

            var frameworkElement = root as FrameworkElement;
            var typedElement = frameworkElement as T;
            if (frameworkElement != null && frameworkElement.Name == name && typedElement != null)
            {
                return typedElement;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var match = FindNamedElement<T>(child, name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Size GetWindowDimensions(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 1:
                    return new Size(820, 460);
                case 2:
                    return new Size(380, 280);
                default:
                    return new Size(640, 420);
            }
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

        private static Uri ResourceUri(string relativePath)
        {
            return new Uri("pack://application:,,,/ModernWpf.Gallery;component/" + relativePath, UriKind.Absolute);
        }

        private static BitmapImage CreateBitmap(Uri uri)
        {
            return new BitmapImage(uri);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class ScrollingSampleFactory
    {
        private const string PipsPagerFlipViewXaml =
@"<StackPanel>
    <FlipView x:Name=""Gallery"" MaxWidth=""400"" Height=""270"" ItemsSource=""{x:Bind Pictures}"">
        <FlipView.ItemTemplate>
            <DataTemplate x:DataType=""x:String"">
                <Image Source=""{x:Bind Mode=OneTime}"" />
            </DataTemplate>
        </FlipView.ItemTemplate>
    </FlipView>
    <PipsPager x:Name=""FlipViewPipsPager""
        HorizontalAlignment=""Center""
        Margin=""0, 12, 0, 0""
        NumberOfPages=""{x:Bind Pictures.Count}""
        SelectedPageIndex=""{x:Bind Path=Gallery.SelectedIndex, Mode=TwoWay}"" />
</StackPanel>";

        private const string PipsPagerOptionsXaml =
@"<PipsPager
    Orientation=""$(Orientation)""
    PreviousButtonVisibility=""$(PrevButton)""
    NextButtonVisibility=""$(NextButton)"" />";

        private const string ScrollViewerXaml =
@"<ScrollViewer Height=""266"" Width=""400"" ZoomMode=""$(ZoomMode)""
            IsTabStop=""True"" IsVerticalScrollChainingEnabled=""True""
            HorizontalAlignment=""Left"" VerticalAlignment=""Top""
            ViewChanged=""ScrollViewerControl_ViewChanged""
            HorizontalScrollMode=""$(HorizontalScrollMode)"" HorizontalScrollBarVisibility=""$(HorizontalScrollBarVisibility)""
            VerticalScrollMode=""$(VerticalScrollMode)"" VerticalScrollBarVisibility=""$(VerticalScrollBarVisibility)"">
    <Image Source=""ms-appx:///Assets/SampleMedia/cliff.jpg"" AutomationProperties.Name=""cliff"" Stretch=""None""
           HorizontalAlignment=""Left"" VerticalAlignment=""Top""/>
</ScrollViewer>";

        private const string ScrollViewerCSharp =
@"public ScrollViewerPage()
{
    this.InitializeComponent();
    ScrollViewerControl.ZoomToFactor(4.0f);
}

private void ZoomModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (ScrollViewerControl != null && ZoomSlider != null)
    {
        if (sender is ComboBox cb)
        {
            ScrollViewerControl.ZoomMode = (ZoomMode)cb.SelectedIndex;
            ZoomSlider.IsEnabled = cb.SelectedIndex == 1;

            if (!ZoomSlider.IsEnabled)
            {
                ScrollViewerControl.ZoomToFactor(2.0f);
            }
        }
    }
}

private void ZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
{
    if (ScrollViewerControl != null)
    {
        ScrollViewerControl.ChangeView(null, null, (float)e.NewValue);
    }
}

private void hsmCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (ScrollViewerControl != null)
    {
        if (sender is ComboBox cb)
        {
            ScrollViewerControl.HorizontalScrollMode = (ScrollMode)cb.SelectedIndex;
        }
    }
}

private void hsbvCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (ScrollViewerControl != null)
    {
        if (sender is ComboBox cb)
        {
            ScrollViewerControl.HorizontalScrollBarVisibility = (ScrollBarVisibility)cb.SelectedIndex;
        }
    }
}

private void vsmCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (ScrollViewerControl != null)
    {
        if (sender is ComboBox cb)
        {
            ScrollViewerControl.VerticalScrollMode = (ScrollMode)cb.SelectedIndex;
        }
    }
}

private void vsbvCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (ScrollViewerControl != null)
    {
        if (sender is ComboBox cb)
        {
            ScrollViewerControl.VerticalScrollBarVisibility = (ScrollBarVisibility)cb.SelectedIndex;
        }
    }
}

private void ScrollViewerControl_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
{
    ZoomSlider.Value = ScrollViewerControl.ZoomFactor;
}

private void ScrollViewerControl_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
{
    if (!e.IsIntermediate)
    {
        ZoomSlider.Value = ScrollViewerControl.ZoomFactor;
    }
}";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AnnotatedScrollBar":
                    return CreateAnnotatedScrollBarSample();
                case "PipsPager":
                    return CreatePipsPagerSample();
                case "ScrollView":
                    return CreateScrollViewSample();
                case "ScrollViewer":
                    return CreateScrollViewerSample();
                case "SemanticZoom":
                    return CreateSemanticZoomSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "PipsPager":
                    return CreatePipsPagerExamples();
                case "ScrollViewer":
                    return CreateScrollViewerExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateAnnotatedScrollBarSample()
        {
            var panel = CreateSamplePanel("AnnotatedScrollBar maps to a marker rail that jumps a WPF ScrollViewer to labeled sections.");
            var sections = new[]
            {
                new ScrollSection("Azure", "#0078D4"),
                new ScrollSection("Crimson", "#C50F1F"),
                new ScrollSection("Cyan", "#00B7C3"),
                new ScrollSection("Fuchsia", "#B146C2"),
                new ScrollSection("Gold", "#FFB900")
            };

            var content = new StackPanel();
            foreach (var section in sections)
            {
                section.Anchor = CreateColorSection(section);
                content.Children.Add(section.Anchor);
            }

            var scrollViewer = new ScrollViewer
            {
                Width = 360,
                Height = 260,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = content
            };

            var rail = new StackPanel
            {
                Width = 120,
                Margin = new Thickness(14, 0, 0, 0)
            };
            foreach (var section in sections)
            {
                var marker = new Button
                {
                    Content = section.Label,
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(10, 5, 10, 5),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ToolTip = "Jump to " + section.Label
                };
                marker.Click += delegate
                {
                    section.Anchor.BringIntoView();
                };
                rail.Children.Add(marker);
            }

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(scrollViewer);
            row.Children.Add(rail);
            panel.Children.Add(row);
            return panel;
        }

        private static UIElement CreatePipsPagerSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("PipsPager"));
            panel.Children.Add(CreatePipsPagerFlipViewExampleContent(assignRootAutomationId: false));
            panel.Children.Add(CreatePipsPagerOptionsExampleContent());
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreatePipsPagerExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "PipsPager integrated with a FlipView",
                    CreatePipsPagerFlipViewExampleContent(assignRootAutomationId: true),
                    PipsPagerFlipViewXaml,
                    null),
                new GalleryExample(
                    "PipsPager with options to change its orientation and button visibility.",
                    CreatePipsPagerOptionsExampleContent(),
                    PipsPagerOptionsXaml,
                    null)
            };
        }

        private static UIElement CreatePipsPagerFlipViewExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("PipsPager"));
            }

            var pictures = CreatePipsPagerPictures();
            var gallery = new ContentControl
            {
                Name = "Gallery",
                Width = 400,
                Height = 270,
                MaxWidth = 400,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var pipsPager = new Mux.PipsPager
            {
                Name = "FlipViewPipsPager",
                HorizontalAlignment = HorizontalAlignment.Center,
                NumberOfPages = pictures.Count
            };
            var pipsPagerHost = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0),
                Child = pipsPager
            };
            GalleryAutomation.WithAutomationId(pipsPagerHost, GalleryAutomation.SampleElementId("PipsPager", "PipsPager"));

            Action updatePicture = delegate
            {
                gallery.Content = CreatePipsPagerImage(pictures[pipsPager.SelectedPageIndex]);
            };
            pipsPager.SelectedIndexChanged += delegate
            {
                updatePicture();
            };
            updatePicture();

            var stack = new StackPanel();
            stack.Children.Add(gallery);
            stack.Children.Add(pipsPagerHost);
            root.Children.Add(stack);
            return root;
        }

        private static UIElement CreatePipsPagerOptionsExampleContent()
        {
            var pipsPager = new Mux.PipsPager
            {
                Name = "TestPipsPager2",
                NumberOfPages = 10,
                PreviousButtonVisibility = Mux.PipsPagerButtonVisibility.Visible,
                NextButtonVisibility = Mux.PipsPagerButtonVisibility.Visible
            };

            var orientationComboBox = CreatePipsPagerComboBox(
                "OrientationComboBox",
                "Orientation",
                new[] { "Horizontal", "Vertical" },
                "Horizontal");
            var previousButtonComboBox = CreatePipsPagerComboBox(
                "PrevButtonComboBox",
                "Previous Button Visibility",
                new[] { "Visible", "VisibleOnPointerOver", "Collapsed" },
                "Visible");
            var nextButtonComboBox = CreatePipsPagerComboBox(
                "NextButtonComboBox",
                "Next Button Visibility",
                new[] { "Visible", "VisibleOnPointerOver", "Collapsed" },
                "Visible");

            orientationComboBox.SelectionChanged += delegate
            {
                pipsPager.Orientation = string.Equals(orientationComboBox.SelectedItem as string, "Vertical", StringComparison.Ordinal)
                    ? Orientation.Vertical
                    : Orientation.Horizontal;
            };
            previousButtonComboBox.SelectionChanged += delegate
            {
                pipsPager.PreviousButtonVisibility = ToPipsPagerButtonVisibility(previousButtonComboBox.SelectedItem as string);
            };
            nextButtonComboBox.SelectionChanged += delegate
            {
                pipsPager.NextButtonVisibility = ToPipsPagerButtonVisibility(nextButtonComboBox.SelectedItem as string);
            };

            var options = new StackPanel();
            options.Children.Add(orientationComboBox);
            options.Children.Add(previousButtonComboBox);
            options.Children.Add(nextButtonComboBox);
            return CreatePipsPagerExampleLayout(pipsPager, options);
        }

        private static UIElement CreateScrollViewSample()
        {
            var panel = CreateSamplePanel("ScrollView maps to WPF ScrollViewer plus an explicit zoom transform for oversized content.");
            var scale = new ScaleTransform(1.2, 1.2);
            var content = CreateLargeDiagram();
            content.LayoutTransform = scale;

            var scrollViewer = new ScrollViewer
            {
                Width = 430,
                Height = 260,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = content
            };

            var zoom = new Slider
            {
                Minimum = 0.6,
                Maximum = 2.4,
                Value = scale.ScaleX,
                Width = 260,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(zoom, "Zoom");
            zoom.ValueChanged += delegate
            {
                scale.ScaleX = zoom.Value;
                scale.ScaleY = zoom.Value;
            };

            panel.Children.Add(scrollViewer);
            panel.Children.Add(zoom);
            return panel;
        }

        private static UIElement CreateScrollViewerSample()
        {
            return CreateScrollViewerExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateScrollViewerExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Content inside of a ScrollViewer.",
                    CreateScrollViewerExampleContent(assignRootAutomationId: true),
                    ScrollViewerXaml,
                    ScrollViewerCSharp)
            };
        }

        private static GallerySamplePanel CreateScrollViewerExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ScrollViewer"));
            }

            var zoomTransform = new ScaleTransform(4, 4);
            var image = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Source = CreateSampleMediaBitmap("cliff.jpg"),
                Stretch = Stretch.None,
                LayoutTransform = zoomTransform
            };
            AutomationProperties.SetName(image, "cliff");

            var scrollViewer = new ScrollViewer
            {
                Name = "ScrollViewerControl",
                Width = 400,
                Height = 266,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Focusable = true,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = image
            };
            GalleryAutomation.WithAutomationId(scrollViewer, GalleryAutomation.SampleElementId("ScrollViewer", "ScrollViewer"));

            var zoomCombo = CreateScrollViewerComboBox("zoomCombo", "zoom mode", 1, "Disabled", "Enabled");
            var zoomSlider = new Slider
            {
                Name = "ZoomSlider",
                IsEnabled = true,
                Minimum = 1,
                Maximum = 4,
                Value = 4,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 10, 0, 0)
            };
            ControlHelper.SetHeader(zoomSlider, "Zoom");

            var horizontalScrollMode = CreateScrollViewerComboBox("hsmCombo", "horizontal scroll mode", 1, "Disabled", "Enabled", "Auto");
            var verticalScrollMode = CreateScrollViewerComboBox("vsmCombo", "vertical scroll mode", 1, "Disabled", "Enabled", "Auto");
            var horizontalScrollBarVisibility = CreateScrollViewerComboBox("hsbvCombo", "horizontal scroll bar visibility", 1, "Disabled", "Auto", "Hidden", "Visible");
            var verticalScrollBarVisibility = CreateScrollViewerComboBox("vsbvCombo", "vertical scroll bar visibility", 1, "Disabled", "Auto", "Hidden", "Visible");

            void ApplyZoom(double zoom)
            {
                zoomTransform.ScaleX = zoom;
                zoomTransform.ScaleY = zoom;
            }

            void ApplyScrollSettings()
            {
                var horizontalEnabled = horizontalScrollMode.SelectedIndex != 0;
                var verticalEnabled = verticalScrollMode.SelectedIndex != 0;
                scrollViewer.HorizontalScrollBarVisibility = horizontalEnabled
                    ? ToScrollBarVisibility(horizontalScrollBarVisibility)
                    : ScrollBarVisibility.Disabled;
                scrollViewer.VerticalScrollBarVisibility = verticalEnabled
                    ? ToScrollBarVisibility(verticalScrollBarVisibility)
                    : ScrollBarVisibility.Disabled;
                scrollViewer.PanningMode = horizontalEnabled && verticalEnabled
                    ? PanningMode.Both
                    : horizontalEnabled
                        ? PanningMode.HorizontalOnly
                        : verticalEnabled
                            ? PanningMode.VerticalOnly
                            : PanningMode.None;
            }

            zoomCombo.SelectionChanged += delegate
            {
                var zoomEnabled = zoomCombo.SelectedIndex == 1;
                zoomSlider.IsEnabled = zoomEnabled;
                if (!zoomEnabled)
                {
                    zoomSlider.Value = 2;
                    ApplyZoom(2);
                }
            };
            zoomSlider.ValueChanged += delegate
            {
                ApplyZoom(zoomSlider.Value);
            };
            horizontalScrollMode.SelectionChanged += delegate { ApplyScrollSettings(); };
            verticalScrollMode.SelectionChanged += delegate { ApplyScrollSettings(); };
            horizontalScrollBarVisibility.SelectionChanged += delegate { ApplyScrollSettings(); };
            verticalScrollBarVisibility.SelectionChanged += delegate { ApplyScrollSettings(); };
            ApplyScrollSettings();

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid.SetColumn(scrollViewer, 0);
            layout.Children.Add(scrollViewer);

            var options = CreateScrollViewerOptionsGrid(
                zoomCombo,
                zoomSlider,
                horizontalScrollMode,
                verticalScrollMode,
                horizontalScrollBarVisibility,
                verticalScrollBarVisibility);
            var optionsHost = new Border
            {
                Margin = new Thickness(24, 0, 0, 0),
                Child = options
            };
            Grid.SetColumn(optionsHost, 1);
            layout.Children.Add(optionsHost);

            root.Children.Add(layout);
            return root;
        }

        private static UIElement CreateSemanticZoomSample()
        {
            var panel = CreateSamplePanel("SemanticZoom maps to a toggle between detailed grouped content and a compact group overview.");
            var groups = CreateGroupedItems();
            var host = new ContentControl
            {
                Width = 430,
                Height = 280
            };
            var output = CreateOutput("Showing detailed view.");

            Action showDetailed = null;
            Action showOverview = null;
            showDetailed = delegate
            {
                host.Content = CreateSemanticDetailedView(groups);
                output.Text = "Showing detailed view.";
            };
            showOverview = delegate
            {
                host.Content = CreateSemanticOverview(groups, delegate(string group)
                {
                    host.Content = CreateSemanticDetailedView(groups, group);
                    output.Text = "Showing " + group + ".";
                });
                output.Text = "Showing overview.";
            };

            var commands = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var detailed = CreateButton("Detailed");
            var overview = CreateButton("Overview");
            detailed.Click += delegate { showDetailed(); };
            overview.Click += delegate { showOverview(); };
            commands.Children.Add(detailed);
            commands.Children.Add(overview);

            showDetailed();
            panel.Children.Add(host);
            panel.Children.Add(commands);
            panel.Children.Add(output);
            return panel;
        }

        private static Border CreateColorSection(ScrollSection section)
        {
            var brush = CreateBrush(section.Color);
            var items = new UniformGrid
            {
                Columns = 3,
                Margin = new Thickness(0, 10, 0, 0)
            };
            for (var i = 1; i <= 6; i++)
            {
                items.Children.Add(new Border
                {
                    Width = 86,
                    Height = 42,
                    Margin = new Thickness(0, 0, 8, 8),
                    Background = brush,
                    Child = new TextBlock
                    {
                        Text = i.ToString(),
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                });
            }

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = section.Label,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold
            });
            stack.Children.Add(items);

            return new Border
            {
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 12),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Child = stack
            };
        }

        private static IReadOnlyList<string> CreatePipsPagerPictures()
        {
            return new[]
            {
                "LandscapeImage1.jpg",
                "LandscapeImage2.jpg",
                "LandscapeImage3.jpg",
                "LandscapeImage4.jpg",
                "LandscapeImage5.jpg",
                "LandscapeImage6.jpg",
                "LandscapeImage7.jpg",
                "LandscapeImage8.jpg"
            };
        }

        private static Image CreatePipsPagerImage(string fileName)
        {
            return new Image
            {
                Source = CreateSampleMediaBitmap(fileName),
                Stretch = Stretch.UniformToFill
            };
        }

        private static BitmapImage CreateSampleMediaBitmap(string fileName)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(
                "pack://application:,,,/ModernWpf.Gallery;component/Assets/SampleMedia/" + fileName,
                UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static ComboBox CreatePipsPagerComboBox(string name, string header, IEnumerable<string> items, string selectedItem)
        {
            var comboBox = new ComboBox
            {
                Name = name,
                Width = 220,
                Margin = new Thickness(0, 0, 0, 12),
                ItemsSource = items.ToArray(),
                SelectedItem = selectedItem
            };
            ControlHelper.SetHeader(comboBox, header);
            return comboBox;
        }

        private static Mux.PipsPagerButtonVisibility ToPipsPagerButtonVisibility(string value)
        {
            switch (value)
            {
                case "Visible":
                    return Mux.PipsPagerButtonVisibility.Visible;
                case "VisibleOnPointerOver":
                    return Mux.PipsPagerButtonVisibility.VisibleOnPointerOver;
                case "Collapsed":
                default:
                    return Mux.PipsPagerButtonVisibility.Collapsed;
            }
        }

        private static Grid CreatePipsPagerExampleLayout(UIElement sample, UIElement options)
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

        private static Grid CreateScrollViewerOptionsGrid(
            ComboBox zoomCombo,
            Slider zoomSlider,
            ComboBox horizontalScrollMode,
            ComboBox verticalScrollMode,
            ComboBox horizontalScrollBarVisibility,
            ComboBox verticalScrollBarVisibility)
        {
            var grid = new Grid
            {
                MinWidth = 200
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (var i = 0; i < 8; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            AddScrollViewerOptionRow(grid, 0, "ZoomMode", zoomCombo, new Thickness(0, 0, 10, 0), new Thickness(0));

            Grid.SetRow(zoomSlider, 1);
            Grid.SetColumnSpan(zoomSlider, 2);
            grid.Children.Add(zoomSlider);

            AddScrollViewerOptionHeading(grid, 2, "ScrollMode", new Thickness(0, 12, 0, 12));
            AddScrollViewerOptionRow(grid, 3, "Horizontal", horizontalScrollMode, new Thickness(0, 0, 10, 0), new Thickness(0));
            AddScrollViewerOptionRow(grid, 4, "Vertical", verticalScrollMode, new Thickness(0, 8, 10, 0), new Thickness(0, 8, 0, 0));
            AddScrollViewerOptionHeading(grid, 5, "ScrollbarVisibility", new Thickness(0, 20, 0, 12));
            AddScrollViewerOptionRow(grid, 6, "Horizontal", horizontalScrollBarVisibility, new Thickness(0, 0, 10, 0), new Thickness(0));
            AddScrollViewerOptionRow(grid, 7, "Vertical", verticalScrollBarVisibility, new Thickness(0, 8, 10, 0), new Thickness(0, 8, 0, 0));
            return grid;
        }

        private static void AddScrollViewerOptionHeading(Grid grid, int row, string text, Thickness margin)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Margin = margin,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(textBlock, row);
            Grid.SetColumnSpan(textBlock, 2);
            grid.Children.Add(textBlock);
        }

        private static void AddScrollViewerOptionRow(Grid grid, int row, string labelText, FrameworkElement control, Thickness labelMargin, Thickness controlMargin)
        {
            var label = new TextBlock
            {
                Text = labelText,
                Margin = labelMargin,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(label, row);
            grid.Children.Add(label);

            control.Margin = controlMargin;
            Grid.SetRow(control, row);
            Grid.SetColumn(control, 1);
            grid.Children.Add(control);
        }

        private static ComboBox CreateScrollViewerComboBox(string name, string automationName, int selectedIndex, params string[] items)
        {
            var comboBox = new ComboBox
            {
                Name = name,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            AutomationProperties.SetName(comboBox, automationName);
            foreach (var item in items)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = item });
            }

            comboBox.SelectedIndex = selectedIndex;
            return comboBox;
        }

        private static ScrollBarVisibility ToScrollBarVisibility(ComboBox comboBox)
        {
            switch (GetSelectedComboText(comboBox))
            {
                case "Disabled":
                    return ScrollBarVisibility.Disabled;
                case "Hidden":
                    return ScrollBarVisibility.Hidden;
                case "Visible":
                    return ScrollBarVisibility.Visible;
                case "Auto":
                default:
                    return ScrollBarVisibility.Auto;
            }
        }

        private static string GetSelectedComboText(ComboBox comboBox)
        {
            var item = comboBox.SelectedItem as ComboBoxItem;
            return item == null ? null : item.Content as string;
        }

        private static StackPanel CreateLargeDiagram()
        {
            var canvas = new StackPanel
            {
                Width = 720,
                Height = 460
            };
            canvas.Children.Add(new TextBlock
            {
                Text = "Large scrollable surface",
                FontSize = 24,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12)
            });

            for (var row = 0; row < 5; row++)
            {
                var strip = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                for (var column = 0; column < 6; column++)
                {
                    strip.Children.Add(new Border
                    {
                        Width = 100,
                        Height = 54,
                        Margin = new Thickness(0, 0, 10, 0),
                        Padding = new Thickness(8),
                        BorderThickness = new Thickness(1),
                        BorderBrush = CreateBrush("#D8D8D8"),
                        Child = new TextBlock
                        {
                            Text = "Tile " + (row + 1) + "." + (column + 1),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    });
                }
                canvas.Children.Add(strip);
            }

            return canvas;
        }

        private static Dictionary<string, string[]> CreateGroupedItems()
        {
            return new Dictionary<string, string[]>
            {
                { "Apps", new[] { "Mail", "Calendar", "Photos", "Terminal" } },
                { "Controls", new[] { "Button", "ListView", "NavigationView", "TreeView" } },
                { "Design", new[] { "Color", "Typography", "Spacing", "Iconography" } }
            };
        }

        private static UIElement CreateSemanticDetailedView(Dictionary<string, string[]> groups, string onlyGroup = null)
        {
            var stack = new StackPanel();
            foreach (var pair in groups.Where(pair => onlyGroup == null || pair.Key == onlyGroup))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = pair.Key,
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 8)
                });
                foreach (var item in pair.Value)
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = item,
                        Margin = new Thickness(12, 0, 0, 6)
                    });
                }
            }

            return new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = stack
            };
        }

        private static UIElement CreateSemanticOverview(Dictionary<string, string[]> groups, Action<string> selected)
        {
            var wrap = new WrapPanel();
            foreach (var pair in groups)
            {
                var button = new Button
                {
                    Content = pair.Key + "\n" + pair.Value.Length + " items",
                    Width = 120,
                    Height = 74,
                    Margin = new Thickness(0, 0, 10, 10)
                };
                var group = pair.Key;
                button.Click += delegate
                {
                    selected(group);
                };
                wrap.Children.Add(button);
            }

            return wrap;
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

        private sealed class ScrollSection
        {
            public ScrollSection(string label, string color)
            {
                Label = label;
                Color = color;
            }

            public string Label { get; private set; }

            public string Color { get; private set; }

            public FrameworkElement Anchor { get; set; }
        }
    }
}

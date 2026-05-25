using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class ScrollingSampleFactory
    {
        private const int AnnotatedAzureCount = 32;
        private const int AnnotatedCrimsonCount = 50;
        private const int AnnotatedCyanCount = 8;
        private const int AnnotatedFuchsiaCount = 70;
        private const int AnnotatedGoldCount = 90;
        private const int AnnotatedItemWidth = 120;
        private const int AnnotatedItemHeight = 90;
        private const string PipsPagerGalleryXaml =
@"<StackPanel>
    <ContentControl x:Name=""Gallery"" MaxWidth=""400"" Height=""270"" />
    <PipsPager x:Name=""GalleryPipsPager""
        HorizontalAlignment=""Center""
        Margin=""0, 12, 0, 0""
        NumberOfPages=""{x:Bind Pictures.Count}""
        SelectedIndexChanged=""GalleryPipsPager_SelectedIndexChanged"" />
</StackPanel>";

        private const string PipsPagerOptionsXaml =
@"<PipsPager
    Orientation=""$(Orientation)""
    PreviousButtonVisibility=""$(PrevButton)""
    NextButtonVisibility=""$(NextButton)"" />";

        private const string AnnotatedScrollBarXaml =
@"<ScrollViewer x:Name=""scrollViewer""
    Background=""LightGray"" MaxWidth=""800"" MaxHeight=""500""
    VerticalScrollBarVisibility=""Hidden"">
    <!-- ... -->
</ScrollViewer>

<AnnotatedScrollBar x:Name=""annotatedScrollBar""
    Margin=""4,0,48,0"" MaxHeight=""500""
    HorizontalAlignment=""Right""
    DetailLabelRequested=""AnnotatedScrollBar_DetailLabelRequested""/>";

        private const string AnnotatedScrollBarCSharp =
@"private void AnnotatedScrollBarPage_Loaded(object sender, RoutedEventArgs e)
{
    scrollViewer.ScrollChanged += delegate
    {
        var maxOffset = Math.Max(0, scrollViewer.ExtentHeight - scrollViewer.ViewportHeight);
        annotatedScrollBar.ScrollController.SetValues(0, maxOffset, scrollViewer.VerticalOffset, scrollViewer.ViewportHeight);
    };

    annotatedScrollBar.Scrolling += delegate(AnnotatedScrollBar sender, AnnotatedScrollBarScrollingEventArgs args)
    {
        scrollViewer.ScrollToVerticalOffset(args.ScrollOffset);
    };
}";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AnnotatedScrollBar":
                    return CreateAnnotatedScrollBarSample();
                case "PipsPager":
                    return CreatePipsPagerSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets = null)
        {
            switch (uniqueId)
            {
                case "AnnotatedScrollBar":
                    return CreateAnnotatedScrollBarExamples();
                case "PipsPager":
                    return CreatePipsPagerExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateAnnotatedScrollBarSample()
        {
            return CreateAnnotatedScrollBarExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateAnnotatedScrollBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "AnnotatedScrollBar linked to a ScrollViewer.",
                    CreateAnnotatedScrollBarExampleContent(assignRootAutomationId: true),
                    AnnotatedScrollBarXaml,
                    AnnotatedScrollBarCSharp)
            };
        }

        private static GallerySamplePanel CreateAnnotatedScrollBarExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("AnnotatedScrollBar"));
            }

            var itemsRepeater = CreateAnnotatedColorItems();
            var scrollViewer = new ScrollViewer
            {
                Name = "scrollViewer",
                Width = AnnotatedItemWidth + 16,
                MaxWidth = 800,
                MaxHeight = 500,
                Background = Brushes.LightGray,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = itemsRepeater
            };
            GalleryAutomation.WithAutomationId(scrollViewer, GalleryAutomation.SampleElementId("AnnotatedScrollBar", "ScrollViewer"));

            var annotatedScrollBar = new Mux.AnnotatedScrollBar
            {
                Name = "annotatedScrollBar",
                MaxHeight = 500,
                Margin = new Thickness(4, 0, 48, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            GalleryAutomation.WithAutomationId(annotatedScrollBar, GalleryAutomation.SampleElementId("AnnotatedScrollBar", "AnnotatedScrollBar"));

            void PopulateLabelCollection()
            {
                PopulateAnnotatedScrollBarLabels(annotatedScrollBar, GetAnnotatedItemsPerRow(itemsRepeater));
            }

            void UpdateScrollController()
            {
                var maxOffset = Math.Max(0, scrollViewer.ExtentHeight - scrollViewer.ViewportHeight);
                annotatedScrollBar.ScrollController.SetIsScrollable(maxOffset > 0);
                annotatedScrollBar.ScrollController.SetValues(0, maxOffset, scrollViewer.VerticalOffset, scrollViewer.ViewportHeight);
            }

            annotatedScrollBar.DetailLabelRequested += delegate(Mux.AnnotatedScrollBar sender, Mux.AnnotatedScrollBarDetailLabelRequestedEventArgs args)
            {
                args.Content = GetAnnotatedOffsetLabel(args.ScrollOffset, GetAnnotatedItemsPerRow(itemsRepeater));
            };
            annotatedScrollBar.Scrolling += delegate(Mux.AnnotatedScrollBar sender, Mux.AnnotatedScrollBarScrollingEventArgs args)
            {
                scrollViewer.ScrollToVerticalOffset(args.ScrollOffset);
            };
            scrollViewer.ScrollChanged += delegate
            {
                UpdateScrollController();
            };
            scrollViewer.Loaded += delegate
            {
                PopulateLabelCollection();
                UpdateScrollController();
            };
            annotatedScrollBar.Loaded += delegate
            {
                PopulateLabelCollection();
                UpdateScrollController();
            };
            itemsRepeater.SizeChanged += delegate
            {
                PopulateLabelCollection();
                UpdateScrollController();
            };

            var sampleGrid = new Grid();
            sampleGrid.HorizontalAlignment = HorizontalAlignment.Left;
            sampleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            sampleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(scrollViewer, 0);
            Grid.SetColumn(annotatedScrollBar, 1);
            sampleGrid.Children.Add(scrollViewer);
            sampleGrid.Children.Add(annotatedScrollBar);

            var slider = new Slider
            {
                Name = "AnnotatedScrollBarMaxHeightSlider",
                Minimum = 100,
                Maximum = 500,
                Value = 500,
                Margin = new Thickness(0, 10, 0, 0)
            };
            ControlHelper.SetHeader(slider, "AnnotatedScrollBar maximum height:");
            slider.ValueChanged += delegate
            {
                annotatedScrollBar.MaxHeight = slider.Value;
                UpdateScrollController();
            };

            var options = new Grid
            {
                MinWidth = 200
            };
            options.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            options.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            options.Children.Add(new TextBlock
            {
                Text = "Changing the AnnotatedScrollBar height refreshes its Labels layout.",
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetRow(slider, 1);
            options.Children.Add(slider);

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetColumn(sampleGrid, 0);
            layout.Children.Add(sampleGrid);
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

        private static UIElement CreatePipsPagerSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("PipsPager"));
            panel.Children.Add(CreatePipsPagerGalleryExampleContent(assignRootAutomationId: false));
            panel.Children.Add(CreatePipsPagerOptionsExampleContent());
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreatePipsPagerExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "PipsPager controlling a WPF content gallery",
                    CreatePipsPagerGalleryExampleContent(assignRootAutomationId: true),
                    PipsPagerGalleryXaml,
                    null),
                new GalleryExample(
                    "PipsPager with options to change its orientation and button visibility.",
                    CreatePipsPagerOptionsExampleContent(),
                    PipsPagerOptionsXaml,
                    null)
            };
        }

        private static UIElement CreatePipsPagerGalleryExampleContent(bool assignRootAutomationId)
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
                Name = "GalleryPipsPager",
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

        private static WrapPanel CreateAnnotatedColorItems()
        {
            var itemsRepeater = new WrapPanel
            {
                Name = "itemsRepeater",
                Margin = new Thickness(2)
            };

            AddAnnotatedColorItems(itemsRepeater, Brushes.Azure, AnnotatedAzureCount);
            AddAnnotatedColorItems(itemsRepeater, Brushes.Crimson, AnnotatedCrimsonCount);
            AddAnnotatedColorItems(itemsRepeater, Brushes.Cyan, AnnotatedCyanCount);
            AddAnnotatedColorItems(itemsRepeater, Brushes.Fuchsia, AnnotatedFuchsiaCount);
            AddAnnotatedColorItems(itemsRepeater, Brushes.Gold, AnnotatedGoldCount);
            return itemsRepeater;
        }

        private static void AddAnnotatedColorItems(Panel panel, Brush brush, int count)
        {
            for (var i = 0; i < count; i++)
            {
                panel.Children.Add(new Border
                {
                    Width = 112,
                    Height = 82,
                    Margin = new Thickness(4),
                    Background = brush,
                    CornerRadius = new CornerRadius(4)
                });
            }
        }

        private static void PopulateAnnotatedScrollBarLabels(Mux.AnnotatedScrollBar annotatedScrollBar, int itemsPerRow)
        {
            annotatedScrollBar.Labels.Clear();
            annotatedScrollBar.Labels.Add(new Mux.AnnotatedScrollBarLabel("Azure", GetAnnotatedOffsetOfItem(0, itemsPerRow)));
            annotatedScrollBar.Labels.Add(new Mux.AnnotatedScrollBarLabel("Crimson", GetAnnotatedOffsetOfItem(AnnotatedAzureCount, itemsPerRow)));
            annotatedScrollBar.Labels.Add(new Mux.AnnotatedScrollBarLabel("Cyan", GetAnnotatedOffsetOfItem(AnnotatedAzureCount + AnnotatedCrimsonCount, itemsPerRow)));
            annotatedScrollBar.Labels.Add(new Mux.AnnotatedScrollBarLabel("Fuchsia", GetAnnotatedOffsetOfItem(AnnotatedAzureCount + AnnotatedCrimsonCount + AnnotatedCyanCount, itemsPerRow)));
            annotatedScrollBar.Labels.Add(new Mux.AnnotatedScrollBarLabel("Gold", GetAnnotatedOffsetOfItem(AnnotatedAzureCount + AnnotatedCrimsonCount + AnnotatedCyanCount + AnnotatedFuchsiaCount, itemsPerRow)));
        }

        private static int GetAnnotatedItemsPerRow(FrameworkElement itemsRepeater)
        {
            return itemsRepeater == null || itemsRepeater.ActualWidth == 0
                ? 1
                : (int)Math.Max(itemsRepeater.ActualWidth / AnnotatedItemWidth, 1);
        }

        private static int GetAnnotatedOffsetOfItem(int itemIndex, int itemsPerRow)
        {
            return AnnotatedItemHeight * (itemIndex / Math.Max(itemsPerRow, 1));
        }

        private static string GetAnnotatedOffsetLabel(double offset, int itemsPerRow)
        {
            if (offset <= GetAnnotatedOffsetOfItem(AnnotatedAzureCount - 1, itemsPerRow))
            {
                return "Azure";
            }

            if (offset <= GetAnnotatedOffsetOfItem(AnnotatedAzureCount + AnnotatedCrimsonCount - 1, itemsPerRow))
            {
                return "Crimson";
            }

            if (offset <= GetAnnotatedOffsetOfItem(AnnotatedAzureCount + AnnotatedCrimsonCount + AnnotatedCyanCount - 1, itemsPerRow))
            {
                return "Cyan";
            }

            if (offset <= GetAnnotatedOffsetOfItem(AnnotatedAzureCount + AnnotatedCrimsonCount + AnnotatedCyanCount + AnnotatedFuchsiaCount - 1, itemsPerRow))
            {
                return "Fuchsia";
            }

            return "Gold";
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

    }
}

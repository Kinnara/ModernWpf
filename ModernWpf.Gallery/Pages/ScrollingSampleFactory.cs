using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        private const string AnnotatedScrollBarXaml =
@"<ScrollView x:Name=""scrollView""
    Background=""LightGray"" MaxWidth=""800"" MaxHeight=""500""
    VerticalScrollBarVisibility=""Hidden"">
    <!-- ... -->
</ScrollView>

<AnnotatedScrollBar x:Name=""annotatedScrollBar""
    Margin=""4,0,48,0"" MaxHeight=""500""
    HorizontalAlignment=""Right""
    DetailLabelRequested=""AnnotatedScrollBar_DetailLabelRequested""/>";

        private const string AnnotatedScrollBarCSharp =
@"private void AnnotatedScrollBarPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
{
    scrollView.ScrollPresenter.VerticalScrollController = annotatedScrollBar.ScrollController;
}";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AnnotatedScrollBar":
                    return CreateAnnotatedScrollBarSample();
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
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateAnnotatedScrollBarSample()
        {
            return CreateAnnotatedScrollBarExampleContent(assignRootAutomationId: true, out _);
        }

        private static IReadOnlyList<GalleryExample> CreateAnnotatedScrollBarExamples()
        {
            var content = CreateAnnotatedScrollBarExampleContent(assignRootAutomationId: true, out var options);
            return new[]
            {
                new GalleryExample(
                    "AnnotatedScrollBar linked to a ScrollView.",
                    content,
                    AnnotatedScrollBarXaml,
                    AnnotatedScrollBarCSharp,
                    options)
                    .WithOptionsMaxWidth(448d)
            };
        }

        private static GallerySamplePanel CreateAnnotatedScrollBarExampleContent(bool assignRootAutomationId, out UIElement optionsContent)
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
                Width = AnnotatedItemWidth + 4,
                Margin = new Thickness(12, 0, 0, 0),
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

            var slider = WinUISampleSlider.ShowValueFill(new Slider
            {
                Name = "AnnotatedScrollBarMaxHeightSlider",
                Minimum = 100,
                Maximum = 500,
                Value = 500,
                Margin = new Thickness(0)
            });
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
                VerticalAlignment = VerticalAlignment.Center
            });
            var sliderHost = new StackPanel
            {
                Margin = new Thickness(0, 10, 0, 0)
            };
            var sliderHeader = new TextBlock
            {
                Text = "AnnotatedScrollBar maximum height:",
                Margin = new Thickness(0, 0, 0, 4),
                VerticalAlignment = VerticalAlignment.Center
            };
            sliderHost.Children.Add(sliderHeader);
            sliderHost.Children.Add(slider);
            Grid.SetRow(sliderHost, 1);
            options.Children.Add(sliderHost);
            optionsContent = options;

            root.Children.Add(sampleGrid);
            return root;
        }

        private static WrapPanel CreateAnnotatedColorItems()
        {
            var itemsRepeater = new WrapPanel
            {
                Name = "itemsRepeater",
                Background = Brushes.LightGray,
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

    }
}

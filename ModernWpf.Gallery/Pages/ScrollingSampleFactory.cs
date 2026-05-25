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
        private const double ScrollViewImageViewportTopCompensation = -54;
        private const double ScrollViewInitialHorizontalOffset = 360;
        private const double ScrollViewInitialVerticalOffset = 400;

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

        private const string ScrollViewExample1Xaml =
@"<ScrollView Height=""266"" Width=""400"" ContentOrientation=""None""
    ZoomMode=""$(ZoomMode)"" IsTabStop=""True""
    VerticalAlignment=""Top"" HorizontalAlignment=""Left""
    HorizontalScrollMode=""$(HorizontalScrollMode)"" HorizontalScrollBarVisibility=""$(HorizontalScrollBarVisibility)""
    VerticalScrollMode=""$(VerticalScrollMode)"" VerticalScrollBarVisibility=""$(VerticalScrollBarVisibility)"">
    <Image Source=""ms-appx:///Assets/SampleMedia/cliff.jpg"" AutomationProperties.Name=""cliff"" Stretch=""None""
        HorizontalAlignment=""Center"" VerticalAlignment=""Center""/>
</ScrollView>";

        private const string ScrollViewExample1CSharp =
@"private void ScrollViewPage_Loaded(object sender, RoutedEventArgs e)
{
    scrollView1.ZoomTo(4.0f, null, new ScrollingZoomOptions(ScrollingAnimationMode.Enabled, ScrollingSnapPointsMode.Ignore));
}

private void CmbZoomMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (scrollView1 != null && sender is ComboBox cmb)
    {
        scrollView1.ZoomMode = (ScrollingZoomMode)cmb.SelectedIndex;
    }
}

private void NbZoomFactor_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
{
    if (scrollView1 != null)
    {
        scrollView1.ZoomTo((float)e.NewValue, null);
    }
}

private void CmbHorizontalScrollMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (scrollView1 != null && sender is ComboBox cmb)
    {
        scrollView1.HorizontalScrollMode = (ScrollingScrollMode)cmb.SelectedIndex;
    }
}

private void CmbHorizontalScrollBarVisibility_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (scrollView1 != null && sender is ComboBox cmb)
    {
        scrollView1.HorizontalScrollBarVisibility = (ScrollingScrollBarVisibility)cmb.SelectedIndex;
    }
}

private void CmbVerticalScrollMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (scrollView1 != null && sender is ComboBox cmb)
    {
        scrollView1.VerticalScrollMode = (ScrollingScrollMode)cmb.SelectedIndex;
    }
}

private void CmbVerticalScrollBarVisibility_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (scrollView1 != null && sender is ComboBox cmb)
    {
        scrollView1.VerticalScrollBarVisibility = (ScrollingScrollBarVisibility)cmb.SelectedIndex;
    }
}";

        private const string ScrollViewExample2Xaml =
@"<ScrollView Height=""300"" Width=""400"" IsTabStop=""True""
    VerticalAlignment=""Top"" HorizontalAlignment=""Left"">
    <Image Source=""ms-appx:///Assets/SampleMedia/grapes.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""grapes""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/rainier.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""rainier""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/sunset.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""sunset""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/treetops.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""treetops""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/valley.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""valley""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/cliff.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""cliff""/>
</ScrollView>";

        private const string ScrollViewExample2CSharp =
@"private void NbVerticalVelocity_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e)
{
    if (double.IsNaN(e.OldValue))
    {
        return;
    }

    if (scrollView2 != null)
    {
        float verticalConstantVelocity = (float)nbVerticalVelocity.Value;

        if (e.NewValue <= 30.0 && e.NewValue >= -30)
        {
            if (e.NewValue < e.OldValue)
            {
                verticalConstantVelocity = scrollView2.VerticalOffset == 0 ? 30 : -30;
            }
            else
            {
                verticalConstantVelocity = scrollView2.VerticalOffset == scrollView2.ScrollableHeight ? -30 : 30;
            }
        }

        nbVerticalVelocity.Value = verticalConstantVelocity;
        scrollView2.AddScrollVelocity(new Vector2(0f, verticalConstantVelocity), new Vector2());
    }
}";

        private const string ScrollViewExample3Xaml =
@"<ScrollView Height=""300"" Width=""400"" IsTabStop=""True""
    ScrollAnimationStarting=""ScrollView_ScrollAnimationStarting""
    VerticalAlignment=""Top"" HorizontalAlignment=""Left"">
    <Image Source=""ms-appx:///Assets/SampleMedia/LandscapeImage1.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""leaves""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/LandscapeImage2.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""carousel""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/LandscapeImage3.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""bicycles""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/LandscapeImage4.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""pond""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/LandscapeImage5.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""marina""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/LandscapeImage6.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""beach""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/LandscapeImage7.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""rampart""/>
    <Image Source=""ms-appx:///Assets/SampleMedia/LandscapeImage8.jpg"" Stretch=""Uniform"" AutomationProperties.Name=""mountain""/>
</ScrollView>";

        private const string ScrollViewExample3CSharp =
@"//Default Animation
private void ScrollView_ScrollAnimationStarting(ScrollView sender, ScrollingScrollAnimationStartingEventArgs e)
{
    Vector3KeyFrameAnimation? stockKeyFrameAnimation = e.Animation as Vector3KeyFrameAnimation;

    if (stockKeyFrameAnimation != null)
    {
        stockKeyFrameAnimation.Duration = TimeSpan.FromMilliseconds(nbAnimationDuration.Value);
    }
}

private void BtnScrollWithAnimation_Click(object sender, RoutedEventArgs e)
{
    if (scrollView3 != null)
    {
        scrollView3.ScrollTo(scrollView3.HorizontalOffset, GetTargetVerticalOffset(), new ScrollingScrollOptions(ScrollingAnimationMode.Enabled, ScrollingSnapPointsMode.Ignore));
    }
}

private double GetTargetVerticalOffset()
{
    if (scrollView3.VerticalOffset > scrollView3.ScrollableHeight / 2.0)
    {
        return scrollView3.ScrollableHeight / 5.0;
    }
    else
    {
        return 4.0 * scrollView3.ScrollableHeight / 5.0;
    }
}";

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
                case "ScrollView":
                    return CreateScrollViewExamples(sampleSnippets);
                case "ScrollViewer":
                    return CreateScrollViewerExamples();
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
                    "AnnotatedScrollBar linked to a ScrollView.",
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
                Name = "scrollView",
                Width = AnnotatedItemWidth + 16,
                MaxWidth = 800,
                MaxHeight = 500,
                Background = Brushes.LightGray,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = itemsRepeater
            };
            GalleryAutomation.WithAutomationId(scrollViewer, GalleryAutomation.SampleElementId("AnnotatedScrollBar", "ScrollView"));

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
            return CreateScrollViewExample1Content(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateScrollViewExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            var defaultAnimationCSharp = FindSampleCodeText(sampleSnippets, "ScrollViewSample3_DefaultAnimation_cs.txt") ?? ScrollViewExample3CSharp;
            var accordionAnimationCSharp = FindSampleCodeText(sampleSnippets, "ScrollViewSample3_AccordionAnimation_cs.txt");
            var consumedAnimationSnippets = new[]
                {
                    defaultAnimationCSharp,
                    accordionAnimationCSharp
                }
                .Where(text => !string.IsNullOrEmpty(text))
                .ToArray();

            return new[]
            {
                new GalleryExample(
                    "Content inside of a ScrollView.",
                    CreateScrollViewExample1Content(assignRootAutomationId: true),
                    ScrollViewExample1Xaml,
                    ScrollViewExample1CSharp),
                new GalleryExample(
                    "Constant velocity scrolling.",
                    CreateScrollViewExample2Content(),
                    ScrollViewExample2Xaml,
                    ScrollViewExample2CSharp),
                new GalleryExample(
                    "Programmatic scroll with custom animation.",
                    CreateScrollViewExample3Content(),
                    ScrollViewExample3Xaml,
                    defaultAnimationCSharp,
                    new Thickness(10),
                    consumedAnimationSnippets)
            };
        }

        private static GallerySamplePanel CreateScrollViewExample1Content(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ScrollView"));
            }

            var introduction = new TextBlock
            {
                Text = "This ScrollView allows horizontal and vertical scrolling, as well as zooming. Change the settings on the right to alter those capabilities or the built-in scrollbars' visibility.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var imageSource = CreateSampleMediaBitmap("cliff.jpg");
            var zoomSurface = new Canvas
            {
                Name = "ScrollViewZoomSurface",
                Width = imageSource.Width * 4,
                Height = imageSource.Height * 4
            };
            var image = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Source = imageSource,
                Stretch = Stretch.Fill,
                Width = zoomSurface.Width,
                Height = zoomSurface.Height
            };
            AutomationProperties.SetName(image, "cliff");
            Canvas.SetTop(image, ScrollViewImageViewportTopCompensation);
            zoomSurface.Children.Add(image);

            var scrollViewer = new ScrollViewer
            {
                Name = "scrollView1",
                Width = 400,
                Height = 266,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Focusable = true,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = zoomSurface
            };
            GalleryAutomation.WithAutomationId(scrollViewer, GalleryAutomation.SampleElementId("ScrollView", "ScrollView"));
            scrollViewer.Loaded += delegate
            {
                scrollViewer.ScrollToHorizontalOffset(ScrollViewInitialHorizontalOffset);
                scrollViewer.ScrollToVerticalOffset(ScrollViewInitialVerticalOffset);
                scrollViewer.UpdateLayout();
            };

            var zoomMode = CreateScrollViewerComboBox("cmbZoomMode", "zoom mode", 0, "Enabled", "Disabled");
            var zoomFactor = CreateScrollViewNumberBox("nbZoomFactor", "zoom factor", 0.1, 10, 4, 1, 10);
            var horizontalScrollMode = CreateScrollViewerComboBox("cmbHorizontalScrollMode", "horizontal scroll mode", 2, "Enabled", "Disabled", "Auto");
            var verticalScrollMode = CreateScrollViewerComboBox("cmbVerticalScrollMode", "vertical scroll mode", 2, "Enabled", "Disabled", "Auto");
            var horizontalScrollBarVisibility = CreateScrollViewerComboBox("cmbHorizontalScrollBarVisibility", "horizontal scroll bar visibility", 0, "Auto", "Visible", "Hidden");
            var verticalScrollBarVisibility = CreateScrollViewerComboBox("cmbVerticalScrollBarVisibility", "vertical scroll bar visibility", 0, "Auto", "Visible", "Hidden");

            void ApplyZoom()
            {
                if (zoomMode.SelectedIndex == 0)
                {
                    var zoom = zoomFactor.Value;
                    zoomSurface.Width = imageSource.Width * zoom;
                    zoomSurface.Height = imageSource.Height * zoom;
                    image.Width = zoomSurface.Width;
                    image.Height = zoomSurface.Height;
                }
            }

            void ApplyScrollSettings()
            {
                var horizontalEnabled = horizontalScrollMode.SelectedIndex != 1;
                var verticalEnabled = verticalScrollMode.SelectedIndex != 1;
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

            zoomMode.SelectionChanged += delegate { ApplyZoom(); };
            zoomFactor.ValueChanged += delegate { ApplyZoom(); };
            horizontalScrollMode.SelectionChanged += delegate { ApplyScrollSettings(); };
            verticalScrollMode.SelectionChanged += delegate { ApplyScrollSettings(); };
            horizontalScrollBarVisibility.SelectionChanged += delegate { ApplyScrollSettings(); };
            verticalScrollBarVisibility.SelectionChanged += delegate { ApplyScrollSettings(); };
            ApplyScrollSettings();

            var sampleStack = new StackPanel();
            sampleStack.Children.Add(introduction);
            sampleStack.Children.Add(scrollViewer);

            var options = CreateScrollViewExample1OptionsGrid(
                zoomMode,
                zoomFactor,
                horizontalScrollMode,
                verticalScrollMode,
                horizontalScrollBarVisibility,
                verticalScrollBarVisibility);

            root.Children.Add(CreateScrollViewTwoColumnLayout(sampleStack, options));
            return root;
        }

        private static GallerySamplePanel CreateScrollViewExample2Content()
        {
            var root = new GallerySamplePanel();
            var introduction = new TextBlock
            {
                Text = "Set the vertical velocity to a value greater than 30 to scroll down, or a value smaller than -30 to scroll up at a constant speed.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var scrollViewer = CreateScrollViewImageStack(
                "scrollView2",
                400,
                300,
                new[]
                {
                    Tuple.Create("grapes", "grapes.jpg"),
                    Tuple.Create("rainier", "rainier.jpg"),
                    Tuple.Create("sunset", "sunset.jpg"),
                    Tuple.Create("treetops", "treetops.jpg"),
                    Tuple.Create("valley", "valley.jpg"),
                    Tuple.Create("cliff", "cliff.jpg")
                });

            var verticalVelocity = CreateScrollViewNumberBox("nbVerticalVelocity", "vertical velocity", -200, 200, 30, 10, 30);
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            var velocity = 0.0;
            var updatingVelocity = false;
            timer.Tick += delegate
            {
                if (Math.Abs(velocity) <= 0.0)
                {
                    return;
                }

                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + velocity * 0.016);
                if ((velocity < 0 && scrollViewer.VerticalOffset <= 0) ||
                    (velocity > 0 && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight))
                {
                    velocity = -velocity;
                }
            };
            verticalVelocity.ValueChanged += delegate(Mux.NumberBox sender, Mux.NumberBoxValueChangedEventArgs args)
            {
                if (updatingVelocity || double.IsNaN(args.OldValue))
                {
                    return;
                }

                var nextVelocity = verticalVelocity.Value;
                if (args.NewValue <= 30.0 && args.NewValue >= -30)
                {
                    if (args.NewValue < args.OldValue)
                    {
                        nextVelocity = scrollViewer.VerticalOffset == 0 ? 30 : -30;
                    }
                    else
                    {
                        nextVelocity = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight ? -30 : 30;
                    }
                }
                else if (args.NewValue < 30.0 && scrollViewer.VerticalOffset == 0)
                {
                    nextVelocity = 30;
                }
                else if (args.NewValue > 30.0 && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {
                    nextVelocity = -30;
                }

                updatingVelocity = true;
                verticalVelocity.Value = nextVelocity;
                updatingVelocity = false;
                velocity = nextVelocity;
                if (!timer.IsEnabled)
                {
                    timer.Start();
                }
            };

            var sampleStack = new StackPanel();
            sampleStack.Children.Add(introduction);
            sampleStack.Children.Add(scrollViewer);

            root.Children.Add(CreateScrollViewTwoColumnLayout(sampleStack, CreateScrollViewExample2OptionsGrid(verticalVelocity)));
            return root;
        }

        private static GallerySamplePanel CreateScrollViewExample3Content()
        {
            var root = new GallerySamplePanel();
            var introduction = new TextBlock
            {
                Text = "Pick an animation type and its duration and then click the button on the right to launch a programmatic scroll.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var scrollViewer = CreateScrollViewImageStack(
                "scrollView3",
                400,
                300,
                new[]
                {
                    Tuple.Create("leaves", "LandscapeImage1.jpg"),
                    Tuple.Create("carousel", "LandscapeImage2.jpg"),
                    Tuple.Create("bicycles", "LandscapeImage3.jpg"),
                    Tuple.Create("pond", "LandscapeImage4.jpg"),
                    Tuple.Create("marina", "LandscapeImage5.jpg"),
                    Tuple.Create("beach", "LandscapeImage6.jpg"),
                    Tuple.Create("rampart", "LandscapeImage7.jpg"),
                    Tuple.Create("mountain", "LandscapeImage8.jpg")
                });

            var verticalAnimation = CreateScrollViewerComboBox("cmbVerticalAnimation", "vertical animation options", 0, "Default", "Accordion", "Teleportation");
            var animationDuration = CreateScrollViewNumberBox("nbAnimationDuration", "animation duration", 1000, 5000, 1500, 500, 1000);
            var scrollWithAnimation = new Button
            {
                Name = "btnScrollWithAnimation",
                Content = "Scroll with animation",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            AutomationProperties.SetName(scrollWithAnimation, "scroll with animation");

            var animator = new ScrollViewAnimator(scrollViewer);
            scrollWithAnimation.Click += delegate
            {
                animator.ScrollTo(GetScrollViewTargetVerticalOffset(scrollViewer), TimeSpan.FromMilliseconds(animationDuration.Value), verticalAnimation.SelectedIndex);
            };

            var sampleStack = new StackPanel();
            sampleStack.Children.Add(introduction);
            sampleStack.Children.Add(scrollViewer);

            root.Children.Add(CreateScrollViewTwoColumnLayout(
                sampleStack,
                CreateScrollViewExample3OptionsGrid(verticalAnimation, animationDuration, scrollWithAnimation)));
            return root;
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

        private static Grid CreateScrollViewTwoColumnLayout(UIElement sample, UIElement options)
        {
            return CreatePipsPagerExampleLayout(sample, options);
        }

        private static Grid CreateScrollViewExample1OptionsGrid(
            ComboBox zoomMode,
            Mux.NumberBox zoomFactor,
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

            AddScrollViewerOptionRow(grid, 0, "ZoomMode", zoomMode, new Thickness(0, 0, 10, 0), new Thickness(0));
            AddScrollViewerOptionRow(grid, 1, "ZoomFactor", zoomFactor, new Thickness(0, 16, 10, 0), new Thickness(0, 16, 0, 0));
            AddScrollViewerOptionHeading(grid, 2, "ScrollMode", new Thickness(0, 16, 0, 12));
            AddScrollViewerOptionRow(grid, 3, "Horizontal", horizontalScrollMode, new Thickness(0, 0, 10, 0), new Thickness(0));
            AddScrollViewerOptionRow(grid, 4, "Vertical", verticalScrollMode, new Thickness(0, 16, 10, 0), new Thickness(0, 16, 0, 0));
            AddScrollViewerOptionHeading(grid, 5, "ScrollbarVisibility", new Thickness(0, 16, 0, 12));
            AddScrollViewerOptionRow(grid, 6, "Horizontal", horizontalScrollBarVisibility, new Thickness(0, 0, 10, 0), new Thickness(0));
            AddScrollViewerOptionRow(grid, 7, "Vertical", verticalScrollBarVisibility, new Thickness(0, 16, 10, 0), new Thickness(0, 16, 0, 0));
            return grid;
        }

        private static Grid CreateScrollViewExample2OptionsGrid(Mux.NumberBox verticalVelocity)
        {
            var grid = new Grid
            {
                MinWidth = 200
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddScrollViewerOptionRow(grid, 0, "Vertical velocity", verticalVelocity, new Thickness(0, 0, 10, 0), new Thickness(0));
            return grid;
        }

        private static Grid CreateScrollViewExample3OptionsGrid(
            ComboBox verticalAnimation,
            Mux.NumberBox animationDuration,
            Button scrollWithAnimation)
        {
            var grid = new Grid
            {
                MinWidth = 320
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (var i = 0; i < 3; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            AddScrollViewerOptionRow(grid, 0, "Scroll with animation", verticalAnimation, new Thickness(0, 0, 10, 0), new Thickness(0));
            AddScrollViewerOptionRow(grid, 1, "Animation duration (msec)", animationDuration, new Thickness(0, 16, 10, 0), new Thickness(0, 16, 0, 0));
            scrollWithAnimation.Margin = new Thickness(0, 16, 0, 0);
            Grid.SetRow(scrollWithAnimation, 2);
            Grid.SetColumnSpan(scrollWithAnimation, 2);
            grid.Children.Add(scrollWithAnimation);
            return grid;
        }

        private static Mux.NumberBox CreateScrollViewNumberBox(
            string name,
            string automationName,
            double minimum,
            double maximum,
            double value,
            double smallChange,
            double largeChange)
        {
            var numberBox = new Mux.NumberBox
            {
                Name = name,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                SmallChange = smallChange,
                LargeChange = largeChange,
                SpinButtonPlacementMode = Mux.NumberBoxSpinButtonPlacementMode.Inline
            };
            AutomationProperties.SetName(numberBox, automationName);
            return numberBox;
        }

        private static ScrollViewer CreateScrollViewImageStack(
            string name,
            double width,
            double height,
            IEnumerable<Tuple<string, string>> images)
        {
            var stack = new StackPanel();
            foreach (var imageInfo in images)
            {
                var image = new Image
                {
                    Source = CreateSampleMediaBitmap(imageInfo.Item2),
                    Stretch = Stretch.Uniform
                };
                AutomationProperties.SetName(image, imageInfo.Item1);
                stack.Children.Add(image);
            }

            return new ScrollViewer
            {
                Name = name,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Focusable = true,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = stack
            };
        }

        private static double GetScrollViewTargetVerticalOffset(ScrollViewer scrollViewer)
        {
            return scrollViewer.VerticalOffset > scrollViewer.ScrollableHeight / 2.0
                ? scrollViewer.ScrollableHeight / 5.0
                : 4.0 * scrollViewer.ScrollableHeight / 5.0;
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

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> sampleSnippets, string title)
        {
            if (sampleSnippets == null)
            {
                return null;
            }

            return sampleSnippets
                .Where(snippet => string.Equals(snippet.Title, title, StringComparison.OrdinalIgnoreCase))
                .Select(snippet => snippet.Text)
                .FirstOrDefault();
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

        private sealed class ScrollViewAnimator
        {
            private readonly ScrollViewer _scrollViewer;
            private readonly DispatcherTimer _timer;
            private DateTime _startTime;
            private TimeSpan _duration;
            private double _startOffset;
            private double _targetOffset;
            private int _animationMode;

            public ScrollViewAnimator(ScrollViewer scrollViewer)
            {
                _scrollViewer = scrollViewer;
                _timer = new DispatcherTimer(DispatcherPriority.Render)
                {
                    Interval = TimeSpan.FromMilliseconds(16)
                };
                _timer.Tick += OnTick;
                _scrollViewer.Unloaded += delegate { _timer.Stop(); };
            }

            public void ScrollTo(double targetOffset, TimeSpan duration, int animationMode)
            {
                _timer.Stop();
                _startOffset = _scrollViewer.VerticalOffset;
                _targetOffset = Math.Max(0, Math.Min(targetOffset, _scrollViewer.ScrollableHeight));
                _duration = duration;
                _animationMode = animationMode;
                _startTime = DateTime.UtcNow;

                if (_duration.TotalMilliseconds <= 0 || Math.Abs(_targetOffset - _startOffset) < 0.1)
                {
                    _scrollViewer.ScrollToVerticalOffset(_targetOffset);
                    return;
                }

                _timer.Start();
            }

            private void OnTick(object sender, EventArgs e)
            {
                var progress = (DateTime.UtcNow - _startTime).TotalMilliseconds / _duration.TotalMilliseconds;
                if (progress >= 1)
                {
                    _timer.Stop();
                    _scrollViewer.ScrollToVerticalOffset(_targetOffset);
                    return;
                }

                progress = AdjustProgress(Math.Max(0, Math.Min(progress, 1)), _animationMode);
                _scrollViewer.ScrollToVerticalOffset(_startOffset + ((_targetOffset - _startOffset) * progress));
            }

            private static double AdjustProgress(double progress, int animationMode)
            {
                switch (animationMode)
                {
                    case 1:
                        return Math.Max(0, Math.Min(1, 1 - Math.Pow(1 - progress, 3) + (Math.Sin(progress * Math.PI * 6) * (1 - progress) * 0.08)));
                    case 2:
                        return progress < 0.5
                            ? progress * 0.2
                            : 0.8 + ((progress - 0.5) * 0.4);
                    default:
                        return 1 - Math.Pow(1 - progress, 3);
                }
            }
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
    }
}

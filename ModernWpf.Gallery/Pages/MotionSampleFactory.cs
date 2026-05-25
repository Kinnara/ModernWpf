using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class MotionSampleFactory
    {
        private const double ParallaxExampleWidth = 745;
        private const double ParallaxExampleHeight = 551;
        private const double ParallaxHeaderHeight = 76;
        private const double ParallaxVerticalShift = 500;

        private const string ParallaxViewListViewXaml =
@"<Grid>
    <ParallaxView
        x:Name=""parallaxView""
        Source=""{Binding ElementName=listView}""
        VerticalShift=""500"">
        <Image Source=""ms-appx:///Assets/SampleMedia/cliff.jpg"" />
    </ParallaxView>
    <ListView
        x:Name=""listView""
        AutomationProperties.Name=""all samples""
        Background=""#80000000""
        ItemsSource=""{x:Bind Items}"">
        <ListView.ItemTemplate>
            <DataTemplate x:DataType=""models:ControlInfoDataItem"">
                <TextBlock Foreground=""{ThemeResource SystemControlForegroundAltHighBrush}""
                    Text=""{x:Bind Title}"" />
            </DataTemplate>
        </ListView.ItemTemplate>
        <ListView.Header>
            <TextBlock
                MaxWidth=""280""
                HorizontalAlignment=""Center""
                VerticalAlignment=""Center""
                FontSize=""28""
                Foreground=""White""
                Text=""Scroll the list to see parallaxing of image""
                TextWrapping=""WrapWholeWords"" />
        </ListView.Header>
    </ListView>
</Grid>";

        private const string ParallaxViewScrollViewerXaml =
@"<Grid>
    <ParallaxView Source=""{Binding ElementName=scrollViewer}"" VerticalShift=""500"">
        <Image Source=""ms-appx:///Assets/SampleMedia/cliff.jpg""/>
    </ParallaxView>
    <TextBlock Text=""Scroll the rectangles to see parallaxing of image"" MaxWidth=""280""
        HorizontalAlignment=""Center"" VerticalAlignment=""Top"" Foreground=""White""
        FontSize=""28"" TextWrapping=""WrapWholeWords""/>
    <ScrollViewer x:Name=""scrollViewer"" Width=""150"" HorizontalAlignment=""Left"">
        <StackPanel>
            <Rectangle Fill=""AliceBlue"" Height=""150""/>
            <!-- ... -->
            <Rectangle Fill=""Cyan"" Height=""150""/>
        </StackPanel>
    </ScrollViewer>
</Grid>";

        private static readonly string[] ParallaxRectangleBrushes =
        {
            "AliceBlue",
            "AntiqueWhite",
            "Aqua",
            "Aquamarine",
            "Azure",
            "Beige",
            "Bisque",
            "BlanchedAlmond",
            "BlueViolet",
            "Brown",
            "BurlyWood",
            "CadetBlue",
            "Chartreuse",
            "Chocolate",
            "Coral",
            "CornflowerBlue",
            "Cornsilk",
            "Crimson",
            "Cyan"
        };

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "ParallaxView":
                    return CreateParallaxViewSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "ParallaxView":
                    return CreateParallaxViewExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateParallaxViewSample()
        {
            return CreateParallaxListViewExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateParallaxViewExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Parallax on a ListView",
                    CreateParallaxListViewExampleContent(assignRootAutomationId: true),
                    ParallaxViewListViewXaml,
                    null),
                new GalleryExample(
                    "Parallax with a ScrollViewer",
                    CreateParallaxScrollViewerExampleContent(),
                    ParallaxViewScrollViewerXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateParallaxListViewExampleContent(bool assignRootAutomationId)
        {
            var root = CreateParallaxExampleRoot(assignRootAutomationId);
            var grid = CreateParallaxHostGrid();

            var listView = new ListView
            {
                Name = "listView",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Height = ParallaxExampleHeight - ParallaxHeaderHeight,
                Margin = new Thickness(0, ParallaxHeaderHeight, 0, 0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.White,
                ItemsSource = GetParallaxItemTitles(),
                ItemTemplate = CreateParallaxListItemTemplate()
            };
            AutomationProperties.SetName(listView, "all samples");
            GalleryAutomation.WithAutomationId(listView, GalleryAutomation.SampleElementId("ParallaxView", "ListView"));

            var parallaxView = new Mux.ParallaxView
            {
                Name = "parallaxView",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Source = listView,
                VerticalShift = ParallaxVerticalShift,
                Child = CreateParallaxImage()
            };
            GalleryAutomation.WithAutomationId(parallaxView, GalleryAutomation.SampleElementId("ParallaxView", "ParallaxView"));

            grid.Children.Add(parallaxView);
            grid.Children.Add(new Border
            {
                Name = "ParallaxOverlay",
                Background = CreateBrush("#80000000")
            });
            grid.Children.Add(listView);
            grid.Children.Add(CreateParallaxHeader("Scroll the list to see parallaxing of image"));
            root.Children.Add(grid);
            return root;
        }

        private static GallerySamplePanel CreateParallaxScrollViewerExampleContent()
        {
            var root = CreateParallaxExampleRoot(assignRootAutomationId: false);
            var grid = CreateParallaxHostGrid();

            var rectangles = new StackPanel();
            foreach (var brush in ParallaxRectangleBrushes)
            {
                rectangles.Children.Add(new Rectangle
                {
                    Height = 150,
                    Fill = CreateBrush(brush)
                });
            }

            var scrollViewer = new ScrollViewer
            {
                Name = "scrollViewer",
                Width = 150,
                Height = ParallaxExampleHeight,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = rectangles
            };
            GalleryAutomation.WithAutomationId(scrollViewer, GalleryAutomation.SampleElementId("ParallaxView", "ScrollViewer"));

            var parallaxView = new Mux.ParallaxView
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Source = scrollViewer,
                VerticalShift = ParallaxVerticalShift,
                Child = CreateParallaxImage()
            };

            grid.Children.Add(parallaxView);
            grid.Children.Add(CreateParallaxHeader("Scroll the rectangles to see parallaxing of image"));
            grid.Children.Add(scrollViewer);
            root.Children.Add(grid);
            return root;
        }

        private static GallerySamplePanel CreateParallaxExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel
            {
                Width = ParallaxExampleWidth,
                Height = ParallaxExampleHeight,
                ClipToBounds = true
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ParallaxView"));
            }

            return root;
        }

        private static Grid CreateParallaxHostGrid()
        {
            return new Grid
            {
                Width = ParallaxExampleWidth,
                Height = ParallaxExampleHeight,
                ClipToBounds = true
            };
        }

        private static Image CreateParallaxImage()
        {
            return new Image
            {
                Source = CreateBitmap(ResourceUri("Assets/SampleMedia/cliff.jpg")),
                Stretch = Stretch.UniformToFill,
                Width = ParallaxExampleWidth,
                Height = ParallaxExampleHeight + ParallaxVerticalShift,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
        }

        private static TextBlock CreateParallaxHeader(string text)
        {
            return new TextBlock
            {
                Text = text,
                MaxWidth = 280,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 28,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0),
                IsHitTestVisible = false
            };
        }

        private static DataTemplate CreateParallaxListItemTemplate()
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding());
            textBlockFactory.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            return new DataTemplate { VisualTree = textBlockFactory };
        }

        private static string[] GetParallaxItemTitles()
        {
            return GalleryCatalog.Items
                .Select(item => item.Title)
                .OrderBy(title => title)
                .ToArray();
        }

        private static BitmapImage CreateBitmap(string uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static string ResourceUri(string path)
        {
            return "pack://application:,,,/ModernWpf.Gallery;component/" + path;
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class StylesSampleFactory
    {
        private const string ThemeShadowXaml =
@"<Grid>
    <Grid x:Name=""ShadowCastGrid""/>
    <Border x:Name=""ShadowRect"" Translation=""0,0,$(TranslationSlider)"" Loaded=""ShadowRect_Loaded"" Width=""200"" Height=""200"" CornerRadius=""{ThemeResource OverlayCornerRadius}"" Background=""{ThemeResource CardBackgroundFillColorDefaultBrush}""/>
        <Border.Shadow>
            <ThemeShadow x:Name=""shadow""/>
        </Border.Shadow>
    </Border>
</Grid>";

        private const string ThemeShadowCSharp =
@"private void ShadowRect_Loaded(object sender, RoutedEventArgs e)
{
    shadow.Receivers.Add(ShadowCastGrid);
}";

        private const string IconElementBitmapIconXaml =
@"<BitmapIcon x:Name=""SlicesIcon"" UriSource=""ms-appx:///Assets/SampleMedia/Slices.png"" Width=""50"" ShowAsMonochrome=""$(ShowAsMonochrome)""/>";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "IconElement":
                    return CreateIconElementSample();
                case "ThemeShadow":
                    return CreateThemeShadowSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "IconElement":
                    return CreateIconElementExamples(sampleSnippets);
                case "ThemeShadow":
                    return CreateThemeShadowExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateIconElementSample()
        {
            return CreateBitmapIconExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateIconElementExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "A BitmapIcon with a multicolor bitmap image",
                    CreateBitmapIconExampleContent(assignRootAutomationId: true),
                    IconElementBitmapIconXaml,
                    null),
                new GalleryExample(
                    "A FontIcon using a glyph from a specific font family in a button",
                    CreateFontIconExampleContent(),
                    FindSampleCodeText(sampleSnippets, "FontIconSample1_xaml.txt", System.IO.Path.Combine("Icons", "FontIconSample1_xaml.txt")),
                    null),
                new GalleryExample(
                    "A ImageIcon using a bitmap image in a button",
                    CreateImageIconBitmapExampleContent(),
                    FindSampleCodeText(sampleSnippets, "ImageIconSample1_xaml.txt", System.IO.Path.Combine("Icons", "ImageIconSample1_xaml.txt")),
                    null),
                new GalleryExample(
                    "A ImageIcon using a SVG image in a button",
                    CreateImageIconSvgExampleContent(),
                    FindSampleCodeText(sampleSnippets, "ImageIconSample2_xaml.txt", System.IO.Path.Combine("Icons", "ImageIconSample2_xaml.txt")),
                    null),
                new GalleryExample(
                    "A PathIcon in a button",
                    CreatePathIconExampleContent(),
                    FindSampleCodeText(sampleSnippets, "PathIconSample1_xaml.txt", System.IO.Path.Combine("Icons", "PathIconSample1_xaml.txt")),
                    null),
                new GalleryExample(
                    "A SymbolIcon in a button",
                    CreateSymbolIconExampleContent(),
                    FindSampleCodeText(sampleSnippets, "SymbolIconSample_1_xaml.txt", System.IO.Path.Combine("Icons", "SymbolIconSample_1_xaml.txt")),
                    null)
            };
        }

        private static GallerySamplePanel CreateBitmapIconExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("IconElement"));
            }

            var slicesIcon = new Mux.BitmapIcon
            {
                Name = "SlicesIcon",
                Width = 50,
                HorizontalAlignment = HorizontalAlignment.Left,
                ShowAsMonochrome = false,
                UriSource = new Uri(ResourceUri("Assets/SampleMedia/Slices.png"), UriKind.Absolute)
            };
            GalleryAutomation.WithAutomationId(slicesIcon, GalleryAutomation.SampleElementId("IconElement", "SlicesIcon"));

            var example = CreateIconElementStack("The ShowAsMonochrome property (true by default) will result in a solid block of the foreground color if the property is set to true and the icon is more than one color. This behavior can be ignored by setting the ShowAsMonochrome property to false.");
            example.Children.Add(slicesIcon);

            var monochromeButton = new CheckBox
            {
                Name = "MonochromeButton",
                Content = "Monochrome",
                IsChecked = false
            };
            monochromeButton.Checked += delegate { slicesIcon.ShowAsMonochrome = true; };
            monochromeButton.Unchecked += delegate { slicesIcon.ShowAsMonochrome = false; };

            root.Children.Add(CreateExampleWithOptions(example, monochromeButton));
            return root;
        }

        private static GallerySamplePanel CreateFontIconExampleContent()
        {
            var root = new GallerySamplePanel();
            var button = new Button
            {
                Name = "ExampleButton1",
                Content = new Mux.FontIcon
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Glyph = "\uE790"
                }
            };
            AutomationProperties.SetName(button, "ExampleButton1");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "ExampleButton1"));

            var example = CreateIconElementStack("Use FontIcon as the icon for a control if you want to specify a Glyph value from a FontFamily. Windows 10 uses the Segoe MDL2 Assets FontFamily and that is what this example is showing.");
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static GallerySamplePanel CreateImageIconBitmapExampleContent()
        {
            var root = new GallerySamplePanel();
            var button = new Button
            {
                Name = "ImageExample1",
                Width = 100,
                Content = new Mux.ImageIcon
                {
                    Source = CreateBitmap(ResourceUri("Assets/SampleMedia/Slices.png"))
                }
            };
            AutomationProperties.SetName(button, "ImageExample1");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "ImageExample1"));

            var example = CreateIconElementStack("To use an ImageIcon as the icon for a control, you can set image that has a file format supported by the Image class. The two examples here show a PNG and SVG image as the icon.");
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static GallerySamplePanel CreateImageIconSvgExampleContent()
        {
            var root = new GallerySamplePanel();
            var button = new Button
            {
                Name = "ImageExample2",
                Content = new Mux.ImageIcon
                {
                    Width = 50,
                    Source = CreateCameraPanoramaDrawing()
                }
            };
            AutomationProperties.SetName(button, "ImageExample2");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "ImageExample2"));
            root.Children.Add(button);
            return root;
        }

        private static GallerySamplePanel CreatePathIconExampleContent()
        {
            var root = new GallerySamplePanel();
            var button = new Button
            {
                Name = "Example1Button",
                Content = new Mux.PathIcon
                {
                    Data = Geometry.Parse("F1 M 16,12 20,2L 20,16 1,16"),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            AutomationProperties.SetName(button, "Example1Button");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "Example1Button"));

            var example = CreateIconElementStack("To use a PathIcon as the icon for a control, you specify the path data of the image you are trying to display. The path data draws a series of connected lines and curves.");
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static GallerySamplePanel CreateSymbolIconExampleContent()
        {
            var root = new GallerySamplePanel();
            var content = new StackPanel();
            content.Children.Add(new Mux.SymbolIcon(Mux.Symbol.Accept));
            content.Children.Add(new TextBlock { Text = "Accept" });

            var button = new Button
            {
                Name = "AcceptButton",
                Content = content
            };
            AutomationProperties.SetName(button, "AcceptButton");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("IconElement", "AcceptButton"));

            var example = CreateIconElementStack("To use a SymbolIcon as the icon for a control, you specify the enum value for the glyph you would like to display. SymbolIcon's enum is based off of icons from the Segoe MDL2 font used by Windows 10.");
            example.Children.Add(button);
            root.Children.Add(example);
            return root;
        }

        private static Grid CreateExampleWithOptions(UIElement example, FrameworkElement options)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(example, 0);
            Grid.SetColumn(options, 1);
            options.Margin = new Thickness(24, 0, 0, 0);
            layout.Children.Add(example);
            layout.Children.Add(options);
            return layout;
        }

        private static TextBlock AddLinePointLabel(Canvas canvas, string text, double left, double top)
        {
            var textBlock = new TextBlock
            {
                Text = text
            };
            Canvas.SetLeft(textBlock, left);
            Canvas.SetTop(textBlock, top);
            Panel.SetZIndex(textBlock, 1);
            canvas.Children.Add(textBlock);
            return textBlock;
        }

        private static void SetVisibility(IEnumerable<UIElement> elements, Visibility visibility)
        {
            foreach (var element in elements)
            {
                element.Visibility = visibility;
            }
        }

        private static void InitializeRadialGradientSlider(Slider slider, double maximum, double value, double tickFrequency, double smallChange)
        {
            slider.Maximum = maximum;
            slider.TickFrequency = tickFrequency;
            slider.SmallChange = smallChange;
            slider.Value = value;
        }

        private static UIElement CreateThemeShadowSample()
        {
            return CreateThemeShadowExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateThemeShadowExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "ThemeShadow applied to a Border",
                    CreateThemeShadowExampleContent(assignRootAutomationId: true),
                    ThemeShadowXaml,
                    ThemeShadowCSharp)
            };
        }

        private static GallerySamplePanel CreateThemeShadowExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ThemeShadow"));
            }

            var shadowCastGrid = new Grid
            {
                Name = "ShadowCastGrid"
            };

            var shadowRect = new Border
            {
                Name = "ShadowRect",
                Width = 200,
                Height = 200
            };
            shadowRect.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
            shadowRect.SetResourceReference(Border.CornerRadiusProperty, "OverlayCornerRadius");
            GalleryAutomation.WithAutomationId(shadowRect, GalleryAutomation.SampleElementId("ThemeShadow", "ShadowRect"));

            var shadow = new ThemeShadowChrome
            {
                Name = "shadow",
                Depth = 32,
                TranslationZ = 32,
                Child = shadowRect,
                Margin = new Thickness(36),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            shadow.SetResourceReference(ThemeShadowChrome.CornerRadiusProperty, "OverlayCornerRadius");

            var exampleGrid = new Grid
            {
                Name = "Example3Grid",
                MinWidth = 272,
                MinHeight = 272
            };
            exampleGrid.Children.Add(shadowCastGrid);
            exampleGrid.Children.Add(shadow);

            var translationSlider = new Slider
            {
                Name = "TranslationSliderInApp",
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left,
                Minimum = 0,
                Maximum = 64,
                SmallChange = 1,
                TickFrequency = 1,
                Value = 32
            };
            AutomationProperties.SetName(translationSlider, "shadow intensity");
            ControlHelper.SetHeader(translationSlider, "Z-translation");
            translationSlider.ValueChanged += delegate
            {
                shadow.Depth = translationSlider.Value;
                shadow.TranslationZ = translationSlider.Value;
            };

            var options = new StackPanel
            {
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(translationSlider);

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(exampleGrid, 0);
            Grid.SetColumn(options, 1);
            layout.Children.Add(exampleGrid);
            layout.Children.Add(options);
            root.Children.Add(layout);
            return root;
        }

        private static StackPanel CreateIconElementStack(string description)
        {
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return stack;
        }

        private static DrawingImage CreateCameraPanoramaDrawing()
        {
            var drawing = new DrawingGroup();
            drawing.Children.Add(new GeometryDrawing(CreateBrush("#E6F4FF"), null, Geometry.Parse("M 0,0 L 50,0 L 50,32 L 0,32 Z")));
            drawing.Children.Add(new GeometryDrawing(CreateBrush("#107C10"), null, Geometry.Parse("M 0,23 L 12,13 L 21,21 L 31,10 L 50,25 L 50,32 L 0,32 Z")));
            drawing.Children.Add(new GeometryDrawing(CreateBrush("#FCE100"), null, new EllipseGeometry(new Point(37, 7), 4, 4)));
            drawing.Children.Add(new GeometryDrawing(null, new Pen(CreateBrush("#1F1F1F"), 2), Geometry.Parse("M 1,1 L 49,1 L 49,31 L 1,31 Z")));

            var image = new DrawingImage(drawing);
            image.Freeze();
            return image;
        }

        private static StackPanel CreateIconColumn(string label, UIElement icon)
        {
            var column = new StackPanel
            {
                Width = 116,
                Margin = new Thickness(0, 0, 16, 0)
            };
            var frame = new Border
            {
                Width = 80,
                Height = 70,
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Child = icon,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            column.Children.Add(frame);
            column.Children.Add(new TextBlock
            {
                Text = label,
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return column;
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

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string fileName)
        {
            if (snippets != null)
            {
                foreach (var snippet in snippets)
                {
                    if (string.Equals(snippet.Title, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return snippet.Text;
                    }
                }
            }

            return null;
        }

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string fileName, string fallbackRelativePath)
        {
            var text = FindSampleCodeText(snippets, fileName);
            if (text != null)
            {
                return text;
            }

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode", fallbackRelativePath);
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
        }
    }
}

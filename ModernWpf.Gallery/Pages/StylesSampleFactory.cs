using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
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

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Acrylic":
                    return CreateAcrylicSample();
                case "AnimatedIcon":
                    return CreateAnimatedIconSample();
                case "CompactSizing":
                    return CreateCompactSizingSample();
                case "IconElement":
                    return CreateIconElementSample();
                case "Line":
                    return CreateLineSample();
                case "Shape":
                    return CreateShapeSample();
                case "RadialGradientBrush":
                    return CreateRadialGradientBrushSample();
                case "SystemBackdrops":
                    return CreateSystemBackdropsSample();
                case "SystemBackdropElement":
                    return CreateSystemBackdropElementSample();
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
                case "SystemBackdrops":
                    return CreateSystemBackdropsExamples(sampleSnippets);
                case "SystemBackdropElement":
                    return CreateSystemBackdropElementExamples(sampleSnippets);
                case "ThemeShadow":
                    return CreateThemeShadowExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateAcrylicSample()
        {
            var panel = CreateSamplePanel("AcrylicBrush maps to layered WPF brushes: a backdrop image, tint, and translucent content layer.");
            var tint = CreateBrush("#CCF7F7F7");
            var overlay = new Border
            {
                Width = 300,
                Height = 180,
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#66FFFFFF"),
                Background = tint,
                Padding = new Thickness(18),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = "Acrylic layer",
                    FontSize = 24,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };
            var root = new Grid
            {
                Width = 430,
                Height = 260
            };
            root.Children.Add(new Image
            {
                Source = CreateBitmap(ResourceUri("Assets/SampleMedia/rainier.jpg")),
                Stretch = Stretch.UniformToFill
            });
            root.Children.Add(overlay);

            var opacity = new Slider
            {
                Width = 240,
                Minimum = 0.35,
                Maximum = 0.95,
                Value = tint.Opacity,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(opacity, "Tint opacity");
            opacity.ValueChanged += delegate { tint.Opacity = opacity.Value; };

            panel.Children.Add(root);
            panel.Children.Add(opacity);
            return panel;
        }

        private static UIElement CreateAnimatedIconSample()
        {
            var panel = CreateSamplePanel("AnimatedIcon maps to a ModernWpf FontIcon whose state changes over time.");
            var icon = new Mux.FontIcon
            {
                Glyph = "\xE768",
                FontSize = 54,
                Width = 96,
                Height = 96,
                Foreground = CreateBrush("#0078D4")
            };
            var frame = new Border
            {
                Width = 160,
                Height = 130,
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Child = icon
            };
            var states = new[] { "\xE768", "\xE895", "\xE72C", "\xE7C1" };
            var index = 0;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
            timer.Tick += delegate
            {
                index = (index + 1) % states.Length;
                icon.Glyph = states[index];
            };

            var commands = CreateCommandRow();
            var start = CreateButton("Start");
            var stop = CreateButton("Stop");
            start.Click += delegate { timer.Start(); };
            stop.Click += delegate { timer.Stop(); };
            commands.Children.Add(start);
            commands.Children.Add(stop);

            panel.Children.Add(frame);
            panel.Children.Add(commands);
            return panel;
        }

        private static UIElement CreateCompactSizingSample()
        {
            var panel = CreateSamplePanel("Compact sizing maps to smaller WPF control heights, padding, and item spacing.");
            var comparison = new StackPanel { Orientation = Orientation.Horizontal };
            comparison.Children.Add(CreateSizingColumn("Default", 34, new Thickness(12, 6, 12, 6), 8));
            comparison.Children.Add(CreateSizingColumn("Compact", 26, new Thickness(8, 3, 8, 3), 4));
            panel.Children.Add(comparison);
            return panel;
        }

        private static UIElement CreateIconElementSample()
        {
            var panel = CreateSamplePanel("IconElement is represented by ModernWpf icon elements for symbols, fonts, bitmaps, and paths.");
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(CreateIconColumn("SymbolIcon", new Mux.SymbolIcon(Mux.Symbol.Favorite) { Width = 42, Height = 42 }));
            row.Children.Add(CreateIconColumn("FontIcon", new Mux.FontIcon { Glyph = "\xE8D4", FontSize = 34, Width = 42, Height = 42 }));
            row.Children.Add(CreateIconColumn("BitmapIcon", new Mux.BitmapIcon
            {
                UriSource = new Uri(ResourceUri("Assets/SampleMedia/CoffeeCup.png"), UriKind.Absolute),
                ShowAsMonochrome = false,
                Width = 42,
                Height = 42
            }));
            row.Children.Add(CreateIconColumn("PathIcon", new Mux.PathIcon
            {
                Data = Geometry.Parse("M 0,20 L 12,0 L 24,20 Z"),
                Width = 42,
                Height = 42
            }));
            panel.Children.Add(row);
            return panel;
        }

        private static UIElement CreateLineSample()
        {
            var panel = CreateSamplePanel("Line draws a straight segment between two points.");
            var line = new Line
            {
                X1 = 20,
                Y1 = 30,
                X2 = 320,
                Y2 = 130,
                Stroke = CreateBrush("#0078D4"),
                StrokeThickness = 6,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            var canvas = new Canvas
            {
                Width = 360,
                Height = 160,
                Background = CreateBrush("#F3F3F3")
            };
            canvas.Children.Add(line);

            var thickness = new Slider
            {
                Width = 220,
                Minimum = 1,
                Maximum = 16,
                Value = line.StrokeThickness,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(thickness, "StrokeThickness");
            thickness.ValueChanged += delegate { line.StrokeThickness = thickness.Value; };

            panel.Children.Add(canvas);
            panel.Children.Add(thickness);
            return panel;
        }

        private static UIElement CreateShapeSample()
        {
            var panel = CreateSamplePanel("Shape maps directly to WPF Rectangle, Ellipse, Polygon, and Path elements.");
            var canvas = new Canvas
            {
                Width = 420,
                Height = 190,
                Background = CreateBrush("#F3F3F3")
            };
            AddShape(canvas, new Rectangle
            {
                Width = 86,
                Height = 86,
                RadiusX = 8,
                RadiusY = 8,
                Fill = CreateBrush("#0078D4")
            }, 24, 52);
            AddShape(canvas, new Ellipse
            {
                Width = 86,
                Height = 86,
                Fill = CreateBrush("#C239B3")
            }, 132, 52);
            AddShape(canvas, new Polygon
            {
                Points = new PointCollection { new Point(42, 0), new Point(84, 82), new Point(0, 82) },
                Fill = CreateBrush("#107C10")
            }, 240, 54);
            AddShape(canvas, new Path
            {
                Data = Geometry.Parse("M 6,42 C 24,0 64,0 82,42 C 64,84 24,84 6,42 Z"),
                Fill = CreateBrush("#D83B01")
            }, 328, 54);
            panel.Children.Add(canvas);
            return panel;
        }

        private static UIElement CreateRadialGradientBrushSample()
        {
            var panel = CreateSamplePanel("RadialGradientBrush paints from a center point outward through gradient stops.");
            var brush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.35, 0.28),
                Center = new Point(0.5, 0.5),
                RadiusX = 0.74,
                RadiusY = 0.74
            };
            brush.GradientStops.Add(new GradientStop(Colors.White, 0));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(0, 120, 212), 0.42));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(36, 36, 36), 1));

            var swatch = new Border
            {
                Width = 320,
                Height = 190,
                CornerRadius = new CornerRadius(8),
                Background = brush
            };
            var radius = new Slider
            {
                Width = 240,
                Minimum = 0.25,
                Maximum = 1,
                Value = brush.RadiusX,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(radius, "Radius");
            radius.ValueChanged += delegate
            {
                brush.RadiusX = radius.Value;
                brush.RadiusY = radius.Value;
            };

            panel.Children.Add(swatch);
            panel.Children.Add(radius);
            return panel;
        }

        private static UIElement CreateSystemBackdropsSample()
        {
            return CreateSystemBackdropsBackdropTypesExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateSystemBackdropsExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "Backdrop types",
                    CreateSystemBackdropsBackdropTypesExampleContent(assignRootAutomationId: true),
                    FindSampleCodeText(sampleSnippets, "SystemBackdropsSampleBackdropTypes_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "SystemBackdropsSampleBackdropTypes_cs.txt")),
                new GalleryExample(
                    "MicaController",
                    CreateSystemBackdropsMicaControllerExampleContent(),
                    null,
                    FindSampleCodeText(sampleSnippets, "SystemBackdropsSampleMicaController.txt")),
                new GalleryExample(
                    "DesktopAcrylicController",
                    CreateSystemBackdropsDesktopAcrylicControllerExampleContent(),
                    null,
                    FindSampleCodeText(sampleSnippets, "SystemBackdropsSampleDesktopAcrylicController.txt"))
            };
        }

        private static GallerySamplePanel CreateSystemBackdropsBackdropTypesExampleContent(bool assignRootAutomationId)
        {
            var root = CreateSystemBackdropsExampleRoot(assignRootAutomationId);
            var stack = CreateSystemBackdropsStack();
            var text = CreateSystemBackdropsTextBlock();
            AddInlineText(text, "A window can use one of the following system backdrops:");
            AddLineBreak(text);
            AddInlineText(text, "1. ");
            AddInlineBold(text, "Mica");
            AddInlineText(text, " \u2014 An opaque material that samples the desktop wallpaper once to tint the window background. Best for main app windows.");
            AddLineBreak(text);
            AddInlineText(text, "2. ");
            AddInlineBold(text, "Mica Alt");
            AddInlineText(text, " \u2014 A variant of Mica with stronger tinting. Recommended for apps with a tabbed title bar.");
            AddLineBreak(text);
            AddInlineText(text, "3. ");
            AddInlineBold(text, "Desktop Acrylic (Base)");
            AddInlineText(text, " \u2014 A semi-transparent material that shows a blurred view of the content behind the window.");
            AddLineBreak(text);
            AddInlineText(text, "4. ");
            AddInlineBold(text, "Desktop Acrylic (Thin)");
            AddInlineText(text, " \u2014 A lighter variant of Desktop Acrylic with more transparency.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineBold(text, "Mica vs. Acrylic:");
            AddInlineText(text, " Mica is opaque and renders the desktop wallpaper within the window background. Desktop Acrylic is semi-transparent and reveals a blurred view of what is behind the window in real-time. Mica is more performant because it captures the wallpaper only once, while Acrylic updates continuously.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "There are three backdrop types in the API:");
            AddLineBreak(text);
            AddInlineText(text, "\u2022 ");
            AddInlineBold(text, "SystemBackdrop");
            AddInlineText(text, " \u2014 The base class of every backdrop type.");
            AddLineBreak(text);
            AddInlineText(text, "\u2022 ");
            AddInlineBold(text, "MicaBackdrop");
            AddInlineText(text, " \u2014 Applies the Mica material. Set the Kind property to switch between Base and Alt.");
            AddLineBreak(text);
            AddInlineText(text, "\u2022 ");
            AddInlineBold(text, "DesktopAcrylicBackdrop");
            AddInlineText(text, " \u2014 Applies the Desktop Acrylic material (Base type only).");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "All Mica variants require Windows 11 build 22000 or later. In-app acrylic (AcrylicBrush) is a separate XAML brush used within UI elements, not a window backdrop.");
            stack.Children.Add(text);

            var button = CreateSystemBackdropsShowWindowButton("SystemBackdrops", "ShowWindowButton", "Show window");
            button.Click += delegate
            {
                ShowSystemBackdropPreviewWindow(button, "Built-in system backdrops", "Mica", CreateBrush("#F3F3F3"));
            };
            stack.Children.Add(button);
            root.Children.Add(stack);
            return root;
        }

        private static GallerySamplePanel CreateSystemBackdropsMicaControllerExampleContent()
        {
            var root = CreateSystemBackdropsExampleRoot(assignRootAutomationId: false);
            var stack = CreateSystemBackdropsStack();
            var text = CreateSystemBackdropsTextBlock();
            AddInlineText(text, "MicaController provides a customizable way to apply the Mica material. You can modify: FallbackColor, Kind, LuminosityOpacity, TintColor, and TintOpacity.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "There are 2 kinds of Mica:");
            AddLineBreak(text);
            AddInlineText(text, "1. ");
            AddInlineBold(text, "Base");
            AddInlineText(text, " \u2014 The default, lighter appearance.");
            AddLineBreak(text);
            AddInlineText(text, "2. ");
            AddInlineBold(text, "Alt");
            AddInlineText(text, " \u2014 A darker appearance with stronger tinting of the desktop wallpaper.");
            stack.Children.Add(text);

            var button = CreateSystemBackdropsShowWindowButton("SystemBackdrops", "MicaControllerShowWindowButton", "Show window");
            button.Click += delegate
            {
                ShowSystemBackdropPreviewWindow(button, "MicaController", "Mica", CreateBrush("#F3F3F3"));
            };
            stack.Children.Add(button);
            root.Children.Add(stack);
            return root;
        }

        private static GallerySamplePanel CreateSystemBackdropsDesktopAcrylicControllerExampleContent()
        {
            var root = CreateSystemBackdropsExampleRoot(assignRootAutomationId: false);
            var stack = CreateSystemBackdropsStack();
            var text = CreateSystemBackdropsTextBlock();
            AddInlineText(text, "DesktopAcrylicController provides a customizable way to apply the Desktop Acrylic material. It supports the same customization properties as MicaController.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "There are 2 kinds of Desktop Acrylic:");
            AddLineBreak(text);
            AddInlineText(text, "1. ");
            AddInlineBold(text, "Base");
            AddInlineText(text, " \u2014 The default, darker appearance with less transparency.");
            AddLineBreak(text);
            AddInlineText(text, "2. ");
            AddInlineBold(text, "Thin");
            AddInlineText(text, " \u2014 A lighter appearance with more transparency.");
            AddLineBreak(text);
            AddLineBreak(text);
            AddInlineText(text, "Note: DesktopAcrylicBackdrop always uses the Base kind. To use the Thin kind, you must use DesktopAcrylicController directly.");
            stack.Children.Add(text);

            var button = CreateSystemBackdropsShowWindowButton("SystemBackdrops", "DesktopAcrylicControllerShowWindowButton", "Show window");
            button.Click += delegate
            {
                ShowSystemBackdropPreviewWindow(button, "DesktopAcrylicController", "Desktop Acrylic", CreateBrush("#DDEEF6FF"));
            };
            stack.Children.Add(button);
            root.Children.Add(stack);
            return root;
        }

        private static UIElement CreateSystemBackdropElementSample()
        {
            return CreateSystemBackdropElementExampleContent(assignRootAutomationId: true);
        }

        private static IReadOnlyList<GalleryExample> CreateSystemBackdropElementExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            var acrylicXaml = FindSampleCodeText(sampleSnippets, "SystemBackdropElementAcrylic_xaml.txt");
            var micaXaml = FindSampleCodeText(sampleSnippets, "SystemBackdropElementMica_xaml.txt");
            var micaAltXaml = FindSampleCodeText(sampleSnippets, "SystemBackdropElementMicaAlt_xaml.txt");
            return new[]
            {
                new GalleryExample(
                    "SystemBackdropElement Sample",
                    CreateSystemBackdropElementExampleContent(assignRootAutomationId: true),
                    acrylicXaml,
                    null,
                    new Thickness(10),
                    new[] { micaXaml, micaAltXaml })
            };
        }

        private static GallerySamplePanel CreateSystemBackdropElementExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("SystemBackdropElement"));
            }

            var dynamicBackdropHost = new Border
            {
                Name = "DynamicBackdropHost",
                CornerRadius = new CornerRadius(8),
                Background = CreateBrush("#DDEEF6FF")
            };
            var button = new Button
            {
                Content = "Click Me",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("SystemBackdropElement", "Button"));

            var example = new Grid
            {
                Width = 300,
                Height = 200,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            example.Children.Add(dynamicBackdropHost);
            example.Children.Add(button);

            var backdropTypeComboBox = new ComboBox
            {
                Name = "BackdropTypeComboBox",
                SelectedIndex = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 180
            };
            ControlHelper.SetHeader(backdropTypeComboBox, "Backdrop Type");
            backdropTypeComboBox.Items.Add(CreateComboBoxItem("Acrylic", "Acrylic"));
            backdropTypeComboBox.Items.Add(CreateComboBoxItem("Mica", "Mica"));
            backdropTypeComboBox.Items.Add(CreateComboBoxItem("Mica Alt", "MicaAlt"));

            var cornerRadiusSlider = new Slider
            {
                Name = "CornerRadiusSlider",
                Minimum = 0,
                Maximum = 50,
                TickFrequency = 1,
                Value = 8,
                Width = 220,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(cornerRadiusSlider, "Corner radius");

            backdropTypeComboBox.SelectionChanged += delegate
            {
                var selectedItem = backdropTypeComboBox.SelectedItem as ComboBoxItem;
                switch (selectedItem == null ? null : selectedItem.Tag as string)
                {
                    case "Mica":
                        dynamicBackdropHost.Background = CreateBrush("#F3F3F3");
                        break;
                    case "MicaAlt":
                        dynamicBackdropHost.Background = CreateBrush("#E9EEF5");
                        break;
                    default:
                        dynamicBackdropHost.Background = CreateBrush("#DDEEF6FF");
                        break;
                }
            };
            cornerRadiusSlider.ValueChanged += delegate
            {
                dynamicBackdropHost.CornerRadius = new CornerRadius(cornerRadiusSlider.Value);
            };

            var options = new StackPanel
            {
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(backdropTypeComboBox);
            options.Children.Add(cornerRadiusSlider);

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(example, 0);
            Grid.SetColumn(options, 1);
            layout.Children.Add(example);
            layout.Children.Add(options);
            root.Children.Add(layout);
            return root;
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

        private static GallerySamplePanel CreateSystemBackdropsExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("SystemBackdrops"));
            }

            return root;
        }

        private static StackPanel CreateSystemBackdropsStack()
        {
            return new StackPanel();
        }

        private static TextBlock CreateSystemBackdropsTextBlock()
        {
            return new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static Button CreateSystemBackdropsShowWindowButton(string controlId, string elementName, string content)
        {
            var button = new Button
            {
                Name = elementName,
                Content = content,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, content);
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId(controlId, elementName));
            return button;
        }

        private static void AddInlineText(TextBlock textBlock, string text)
        {
            textBlock.Inlines.Add(new Run(text));
        }

        private static void AddInlineBold(TextBlock textBlock, string text)
        {
            textBlock.Inlines.Add(new Bold(new Run(text)));
        }

        private static void AddLineBreak(TextBlock textBlock)
        {
            textBlock.Inlines.Add(new LineBreak());
        }

        private static void ShowSystemBackdropPreviewWindow(FrameworkElement ownerElement, string title, string backdropName, Brush backdropBrush)
        {
            var window = new Window
            {
                Title = title,
                Width = 560,
                Height = 360,
                MinWidth = 420,
                MinHeight = 260,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = CreateBrush("#F9F9F9"),
                Content = CreateSystemBackdropPreviewWindowContent(title, backdropName, backdropBrush)
            };
            var owner = Window.GetWindow(ownerElement);
            if (owner != null)
            {
                window.Owner = owner;
            }

            ThemeManager.SetIsThemeAware(window, true);
            WindowHelper.SetUseModernWindowStyle(window, true);
            Mux.TitleBar.SetIsIconVisible(window, true);
            window.Show();
        }

        private static FrameworkElement CreateSystemBackdropPreviewWindowContent(string title, string backdropName, Brush backdropBrush)
        {
            var root = new Grid
            {
                Background = CreateBrush("#252525"),
                Margin = new Thickness(0)
            };
            root.Children.Add(CreateBackdropCard(
                backdropName,
                backdropBrush,
                title,
                "This WPF preview represents the WinUI system backdrop sample window."));
            return root;
        }

        private static Border CreateBackdropCard(string name, Brush brush, string title, string subtitle)
        {
            return new Border
            {
                Width = 430,
                Height = 260,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CornerRadius = new CornerRadius(8),
                Background = brush,
                Padding = new Thickness(20),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 24,
                            FontWeight = FontWeights.SemiBold
                        },
                        new TextBlock
                        {
                            Text = subtitle,
                            Opacity = 0.72,
                            Margin = new Thickness(0, 8, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = name,
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 18, 0, 0)
                        }
                    }
                }
            };
        }

        private static StackPanel CreateSizingColumn(string title, double height, Thickness padding, double spacing)
        {
            var column = new StackPanel
            {
                Width = 220,
                Margin = new Thickness(0, 0, 24, 0)
            };
            column.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });
            column.Children.Add(new Button
            {
                Content = "Button",
                MinHeight = height,
                Padding = padding,
                Margin = new Thickness(0, 0, 0, spacing),
                HorizontalAlignment = HorizontalAlignment.Left
            });
            column.Children.Add(new TextBox
            {
                Text = "TextBox",
                MinHeight = height,
                Padding = padding,
                Margin = new Thickness(0, 0, 0, spacing)
            });
            column.Children.Add(new ComboBox
            {
                ItemsSource = new[] { "First", "Second", "Third" },
                SelectedIndex = 0,
                MinHeight = height,
                Padding = padding
            });
            return column;
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

        private static void AddShape(Canvas canvas, Shape shape, double left, double top)
        {
            Canvas.SetLeft(shape, left);
            Canvas.SetTop(shape, top);
            canvas.Children.Add(shape);
        }

        private static StackPanel CreateCommandRow()
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
        }

        private static ComboBoxItem CreateComboBoxItem(string content, string tag)
        {
            return new ComboBoxItem
            {
                Content = content,
                Tag = tag
            };
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
    }
}

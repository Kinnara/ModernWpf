using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class StylesSampleFactory
    {
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
            var panel = CreateSamplePanel("System backdrops map to WPF window material previews for Mica, Mica Alt, and Desktop Acrylic.");
            var preview = CreateBackdropPreview("Mica", CreateBrush("#F3F3F3"));
            var options = new ComboBox
            {
                Width = 240,
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[] { "Mica", "Mica Alt", "Desktop Acrylic Base", "Desktop Acrylic Thin" },
                SelectedIndex = 0
            };
            ControlHelper.SetHeader(options, "Backdrop");
            options.SelectionChanged += delegate
            {
                var name = (string)options.SelectedItem;
                var brush = name == "Mica Alt" ? CreateBrush("#E9EEF5") :
                    name == "Desktop Acrylic Base" ? CreateBrush("#DDEEF6FF") :
                    name == "Desktop Acrylic Thin" ? CreateBrush("#B8EEF6FF") :
                    CreateBrush("#F3F3F3");
                preview.Child = CreateBackdropCard(name, brush);
            };

            panel.Children.Add(preview);
            panel.Children.Add(options);
            return panel;
        }

        private static UIElement CreateSystemBackdropElementSample()
        {
            var panel = CreateSamplePanel("SystemBackdropElement maps to a WPF content host that applies a material brush to one subtree.");
            var host = new Border
            {
                Width = 420,
                Padding = new Thickness(20),
                Background = CreateBrush("#EAF6FF"),
                BorderBrush = CreateBrush("#B8D7F0"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Backdrop element",
                            FontSize = 22,
                            FontWeight = FontWeights.SemiBold
                        },
                        new TextBlock
                        {
                            Text = "Only this content region receives the material background.",
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 8, 0, 14)
                        },
                        new Button
                        {
                            Content = "Contained action",
                            Padding = new Thickness(16, 6, 16, 6),
                            HorizontalAlignment = HorizontalAlignment.Left
                        }
                    }
                }
            };
            panel.Children.Add(host);
            return panel;
        }

        private static UIElement CreateThemeShadowSample()
        {
            var panel = CreateSamplePanel("ThemeShadow uses ModernWpf ThemeShadowChrome to draw a depth-aware shadow around a child.");
            var shadow = new ThemeShadowChrome
            {
                Depth = 32,
                CornerRadius = new CornerRadius(8),
                Child = new Border
                {
                    Width = 260,
                    Height = 120,
                    CornerRadius = new CornerRadius(8),
                    Background = Brushes.White,
                    Padding = new Thickness(18),
                    Child = new TextBlock
                    {
                        Text = "Shadowed surface",
                        FontSize = 22,
                        FontWeight = FontWeights.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }
            };
            var host = new Border
            {
                Width = 420,
                Height = 220,
                Background = CreateBrush("#F3F3F3"),
                Padding = new Thickness(68, 42, 68, 42),
                Child = shadow
            };
            var depth = new Slider
            {
                Width = 240,
                Minimum = 8,
                Maximum = 64,
                Value = shadow.Depth,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(depth, "Depth");
            depth.ValueChanged += delegate { shadow.Depth = depth.Value; };
            var enabled = new ToggleButton
            {
                Content = "Shadow enabled",
                IsChecked = true,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 12, 0, 0)
            };
            enabled.Checked += delegate { shadow.IsShadowEnabled = true; };
            enabled.Unchecked += delegate { shadow.IsShadowEnabled = false; };

            panel.Children.Add(host);
            panel.Children.Add(depth);
            panel.Children.Add(enabled);
            return panel;
        }

        private static Border CreateBackdropPreview(string name, Brush brush)
        {
            return new Border
            {
                Width = 430,
                Height = 260,
                Background = CreateBrush("#252525"),
                Padding = new Thickness(28),
                Child = CreateBackdropCard(name, brush)
            };
        }

        private static Border CreateBackdropCard(string name, Brush brush)
        {
            return new Border
            {
                CornerRadius = new CornerRadius(8),
                Background = brush,
                Padding = new Thickness(20),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = name,
                            FontSize = 24,
                            FontWeight = FontWeights.SemiBold
                        },
                        new TextBlock
                        {
                            Text = "Window backdrop preview",
                            Opacity = 0.72,
                            Margin = new Thickness(0, 8, 0, 0)
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
            return "pack://application:,,,/" + path;
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }
    }
}

using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class DesignAccessibilitySampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Color":
                    return CreateColorSample();
                case "Geometry":
                    return CreateGeometrySample();
                case "Iconography":
                    return CreateIconographySample();
                case "Spacing":
                    return CreateSpacingSample();
                case "Typography":
                    return CreateTypographySample();
                case "AccessibilityColorContrast":
                    return CreateColorContrastSample();
                case "AccessibilityKeyboard":
                    return CreateKeyboardSample();
                case "AccessibilityScreenReader":
                    return CreateScreenReaderSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateColorSample()
        {
            var panel = CreateSamplePanel("Use color to reinforce state, hierarchy, and brand without making it the only signal.");
            var grid = new UniformGrid { Columns = 4 };
            grid.Children.Add(CreateSwatch("Accent", "#005FB8", Brushes.White));
            grid.Children.Add(CreateSwatch("Success", "#0F7B0F", Brushes.White));
            grid.Children.Add(CreateSwatch("Warning", "#FCE100", Brushes.Black));
            grid.Children.Add(CreateSwatch("Critical", "#C42B1C", Brushes.White));
            panel.Children.Add(grid);
            return panel;
        }

        private static UIElement CreateGeometrySample()
        {
            var panel = CreateSamplePanel("Geometry gives controls a consistent shape language while preserving hit-target clarity.");
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(CreateGeometryCard("2 px", new CornerRadius(2)));
            row.Children.Add(CreateGeometryCard("4 px", new CornerRadius(4)));
            row.Children.Add(CreateGeometryCard("8 px", new CornerRadius(8)));
            row.Children.Add(new Ellipse
            {
                Width = 72,
                Height = 72,
                Fill = CreateBrush("#E6F2FB"),
                Stroke = CreateBrush("#005FB8"),
                StrokeThickness = 1,
                Margin = new Thickness(0, 0, 12, 0)
            });
            panel.Children.Add(row);
            return panel;
        }

        private static UIElement CreateIconographySample()
        {
            var panel = CreateSamplePanel("Iconography should make common actions recognizable and stay paired with text where clarity matters.");
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(CreateIconAction(Mux.Symbol.Save, "Save"));
            row.Children.Add(CreateIconAction(Mux.Symbol.Delete, "Delete"));
            row.Children.Add(CreateIconAction(Mux.Symbol.Find, "Find"));
            row.Children.Add(CreateIconAction(Mux.Symbol.Setting, "Settings"));
            panel.Children.Add(row);
            return panel;
        }

        private static UIElement CreateSpacingSample()
        {
            var panel = CreateSamplePanel("Spacing makes related content scan as groups and unrelated content read as separate regions.");
            var stack = new StackPanel { Width = 360, HorizontalAlignment = HorizontalAlignment.Left };
            stack.Children.Add(CreateSpacingBand("4 px compact gap", 4));
            stack.Children.Add(CreateSpacingBand("12 px standard gap", 12));
            stack.Children.Add(CreateSpacingBand("24 px section gap", 24));
            panel.Children.Add(stack);
            return panel;
        }

        private static UIElement CreateTypographySample()
        {
            var panel = CreateSamplePanel("Typography establishes hierarchy with size, weight, and spacing.");
            panel.Children.Add(CreateTypeRow("Title", 28, FontWeights.SemiBold));
            panel.Children.Add(CreateTypeRow("Subtitle", 20, FontWeights.SemiBold));
            panel.Children.Add(CreateTypeRow("Body", 14, FontWeights.Normal));
            panel.Children.Add(CreateTypeRow("Caption", 12, FontWeights.Normal));
            return panel;
        }

        private static UIElement CreateColorContrastSample()
        {
            var panel = CreateSamplePanel("Accessible color choices need enough contrast and should be backed by text or icon meaning.");
            var grid = new UniformGrid { Columns = 2 };
            grid.Children.Add(CreateContrastCard("Pass", "White on blue", "#005FB8", Brushes.White));
            grid.Children.Add(CreateContrastCard("Pass", "Black on yellow", "#FCE100", Brushes.Black));
            grid.Children.Add(CreateContrastCard("Review", "Gray on white", "#FFFFFF", CreateBrush("#777777")));
            grid.Children.Add(CreateContrastCard("State + text", "Error: cannot sync", "#C42B1C", Brushes.White));
            panel.Children.Add(grid);
            return panel;
        }

        private static UIElement CreateKeyboardSample()
        {
            var panel = CreateSamplePanel("Keyboard navigation should expose all commands through tab order, access keys, and default actions.");
            KeyboardNavigation.SetTabNavigation(panel, KeyboardNavigationMode.Continue);
            panel.Children.Add(CreateAccessButton("_Save"));
            panel.Children.Add(CreateAccessButton("_Cancel"));
            panel.Children.Add(CreateAccessButton("_More options"));
            panel.Children.Add(new TextBlock
            {
                Text = "Press Alt plus the underlined letter to invoke an access key.",
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });
            return panel;
        }

        private static UIElement CreateScreenReaderSample()
        {
            var panel = CreateSamplePanel("Screen reader support depends on useful names, roles, and help text.");
            var button = new Button
            {
                Content = "Upload report",
                Padding = new Thickness(16, 6, 16, 6),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, "Upload report");
            AutomationProperties.SetHelpText(button, "Uploads the selected report file to the workspace.");
            panel.Children.Add(button);
            panel.Children.Add(new TextBlock
            {
                Text = "Automation name: Upload report; help text: Uploads the selected report file to the workspace.",
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });
            return panel;
        }

        private static Border CreateSwatch(string label, string color, Brush foreground)
        {
            return new Border
            {
                Width = 132,
                Height = 88,
                Background = CreateBrush(color),
                Margin = new Thickness(0, 0, 12, 12),
                Child = new TextBlock
                {
                    Text = label,
                    Foreground = foreground,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        private static Border CreateGeometryCard(string label, CornerRadius radius)
        {
            return new Border
            {
                Width = 72,
                Height = 72,
                CornerRadius = radius,
                Background = CreateBrush("#E6F2FB"),
                BorderBrush = CreateBrush("#005FB8"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 12, 0),
                Child = new TextBlock
                {
                    Text = label,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        private static Button CreateIconAction(Mux.Symbol symbol, string label)
        {
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(new Mux.SymbolIcon(symbol)
            {
                Margin = new Thickness(0, 0, 8, 0)
            });
            content.Children.Add(new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center
            });

            return new Button
            {
                Content = content,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 8, 0)
            };
        }

        private static Border CreateSpacingBand(string label, double gap)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, gap)
            };
            row.Children.Add(new Border { Width = 48, Height = 24, Background = CreateBrush("#005FB8") });
            row.Children.Add(new Border { Width = gap, Height = 24, Background = CreateBrush("#E6F2FB") });
            row.Children.Add(new Border { Width = 48, Height = 24, Background = CreateBrush("#005FB8") });
            row.Children.Add(new TextBlock
            {
                Text = label,
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            });

            return new Border { Child = row };
        }

        private static TextBlock CreateTypeRow(string label, double size, FontWeight weight)
        {
            return new TextBlock
            {
                Text = label + " " + size.ToString("0") + " px",
                FontSize = size,
                FontWeight = weight,
                Margin = new Thickness(0, 0, 0, 10)
            };
        }

        private static Border CreateContrastCard(string status, string label, string background, Brush foreground)
        {
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = status,
                Foreground = foreground,
                FontWeight = FontWeights.SemiBold
            });
            stack.Children.Add(new TextBlock
            {
                Text = label,
                Foreground = foreground,
                TextWrapping = TextWrapping.Wrap
            });

            return new Border
            {
                Width = 220,
                Height = 88,
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 12, 12),
                Background = CreateBrush(background),
                BorderBrush = CreateBrush("#D8D8D8"),
                BorderThickness = new Thickness(1),
                Child = stack
            };
        }

        private static Button CreateAccessButton(string text)
        {
            return new Button
            {
                Content = new AccessText { Text = text },
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Left
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

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }
    }
}

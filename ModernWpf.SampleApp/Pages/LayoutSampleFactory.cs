using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.SampleApp.Pages
{
    internal static class LayoutSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Border":
                    return CreateBorderSample();
                case "Canvas":
                    return CreateCanvasSample();
                case "Expander":
                    return CreateExpanderSample();
                case "Grid":
                    return CreateGridSample();
                case "RelativePanel":
                    return CreateRelativePanelSample();
                case "SplitView":
                    return CreateSplitViewSample();
                case "StackPanel":
                    return CreateStackPanelSample();
                case "VariableSizedWrapGrid":
                    return CreateVariableSizedWrapGridSample();
                case "Viewbox":
                    return CreateViewboxSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateBorderSample()
        {
            var panel = CreateSamplePanel("Border draws background, stroke, padding, and optional rounded corners around one child.");
            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = CreateBrush("#FFD700"),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(8, 5, 8, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = "Text inside a border",
                    FontSize = 18,
                    Foreground = Brushes.Black
                }
            };

            var thickness = new Slider
            {
                Minimum = 0,
                Maximum = 10,
                Value = 2,
                Width = 220,
                Margin = new Thickness(0, 14, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(thickness, "BorderThickness");
            thickness.ValueChanged += delegate
            {
                border.BorderThickness = new Thickness(Math.Round(thickness.Value));
            };

            var colors = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            colors.Children.Add(CreateBrushCombo("Background", "White", delegate(Brush brush) { border.Background = brush; }));
            colors.Children.Add(CreateBrushCombo("BorderBrush", "Gold", delegate(Brush brush) { border.BorderBrush = brush; }));

            panel.Children.Add(border);
            panel.Children.Add(thickness);
            panel.Children.Add(colors);
            return panel;
        }

        private static UIElement CreateCanvasSample()
        {
            var panel = CreateSamplePanel("Canvas positions children with absolute coordinates and z-index.");
            var canvas = new Canvas
            {
                Width = 150,
                Height = 150,
                Background = Brushes.Gray
            };
            var red = CreateRect(40, 40, Brushes.Red);
            var blue = CreateRect(40, 40, Brushes.Blue);
            var green = CreateRect(40, 40, Brushes.Green);
            var yellow = CreateRect(40, 40, Brushes.Gold);
            Canvas.SetLeft(red, 0);
            Canvas.SetTop(red, 0);
            Canvas.SetLeft(blue, 20);
            Canvas.SetTop(blue, 20);
            Canvas.SetZIndex(blue, 1);
            Canvas.SetLeft(green, 40);
            Canvas.SetTop(green, 40);
            Canvas.SetZIndex(green, 2);
            Canvas.SetLeft(yellow, 60);
            Canvas.SetTop(yellow, 60);
            Canvas.SetZIndex(yellow, 3);
            canvas.Children.Add(red);
            canvas.Children.Add(blue);
            canvas.Children.Add(green);
            canvas.Children.Add(yellow);

            var left = CreateSlider("Canvas.Left", 0, 100, 0);
            var top = CreateSlider("Canvas.Top", 0, 100, 0);
            var zIndex = CreateSlider("Canvas.ZIndex", 0, 4, 0);
            left.ValueChanged += delegate { Canvas.SetLeft(red, left.Value); };
            top.ValueChanged += delegate { Canvas.SetTop(red, top.Value); };
            zIndex.ValueChanged += delegate { Canvas.SetZIndex(red, (int)Math.Round(zIndex.Value)); };

            var options = new StackPanel
            {
                Width = 240,
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(left);
            options.Children.Add(top);
            options.Children.Add(zIndex);

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(canvas);
            row.Children.Add(options);
            panel.Children.Add(row);
            return panel;
        }

        private static UIElement CreateExpanderSample()
        {
            var panel = CreateSamplePanel("Expander reveals or hides additional content under a header.");
            var expander = new Expander
            {
                Header = "This text is in the header",
                Content = new TextBlock
                {
                    Text = "This is in the content",
                    Margin = new Thickness(4)
                },
                IsExpanded = false,
                ExpandDirection = ExpandDirection.Down,
                Width = 360,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var direction = new ComboBox
            {
                Width = 220,
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[] { ExpandDirection.Down, ExpandDirection.Up, ExpandDirection.Left, ExpandDirection.Right },
                SelectedItem = expander.ExpandDirection
            };
            ControlHelper.SetHeader(direction, "ExpandDirection");
            direction.SelectionChanged += delegate
            {
                if (direction.SelectedItem is ExpandDirection)
                {
                    expander.ExpandDirection = (ExpandDirection)direction.SelectedItem;
                }
            };

            panel.Children.Add(expander);
            panel.Children.Add(direction);
            return panel;
        }

        private static UIElement CreateGridSample()
        {
            var panel = CreateSamplePanel("Grid arranges children into rows and columns; spacing is represented with child margins in WPF.");
            var grid = new Grid
            {
                Width = 240,
                Height = 180,
                Background = Brushes.Gray
            };
            for (var i = 0; i < 3; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(56) });
            }

            var red = CreateRect(50, 50, Brushes.Red);
            var blue = CreateRect(50, 50, Brushes.Blue);
            var green = CreateRect(50, 50, Brushes.Green);
            var yellow = CreateRect(50, 50, Brushes.Gold);
            Grid.SetRow(blue, 1);
            Grid.SetColumn(green, 1);
            Grid.SetRow(yellow, 1);
            Grid.SetColumn(yellow, 1);
            grid.Children.Add(red);
            grid.Children.Add(blue);
            grid.Children.Add(green);
            grid.Children.Add(yellow);

            var column = CreateSlider("Grid.Column", 0, 2, 0);
            var row = CreateSlider("Grid.Row", 0, 2, 0);
            var spacing = CreateSlider("Spacing", 0, 16, 6);
            Action update = delegate
            {
                Grid.SetColumn(red, (int)Math.Round(column.Value));
                Grid.SetRow(red, (int)Math.Round(row.Value));
                foreach (UIElement child in grid.Children)
                {
                    ((FrameworkElement)child).Margin = new Thickness(Math.Round(spacing.Value) / 2);
                }
            };
            column.ValueChanged += delegate { update(); };
            row.ValueChanged += delegate { update(); };
            spacing.ValueChanged += delegate { update(); };
            update();

            var options = new StackPanel
            {
                Width = 240,
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(column);
            options.Children.Add(row);
            options.Children.Add(spacing);

            var root = new StackPanel { Orientation = Orientation.Horizontal };
            root.Children.Add(grid);
            root.Children.Add(options);
            panel.Children.Add(root);
            return panel;
        }

        private static UIElement CreateRelativePanelSample()
        {
            var panel = CreateSamplePanel("RelativePanel maps to a WPF Canvas here, using explicit positions to show the same relationships.");
            var canvas = new Canvas
            {
                Width = 300,
                Height = 112,
                Background = CreateBrush("#F3F3F3")
            };
            var red = CreateRect(50, 50, Brushes.Red);
            var blue = CreateRect(50, 50, Brushes.Blue);
            var green = CreateRect(50, 50, Brushes.Green);
            var yellow = CreateRect(50, 50, Brushes.Gold);
            Canvas.SetLeft(red, 0);
            Canvas.SetTop(red, 0);
            Canvas.SetLeft(blue, 58);
            Canvas.SetTop(blue, 0);
            Canvas.SetLeft(green, 250);
            Canvas.SetTop(green, 0);
            Canvas.SetLeft(yellow, 250);
            Canvas.SetTop(yellow, 58);
            canvas.Children.Add(red);
            canvas.Children.Add(blue);
            canvas.Children.Add(green);
            canvas.Children.Add(yellow);

            panel.Children.Add(canvas);
            panel.Children.Add(CreateOutput("Blue is right of red; green is aligned right; yellow is below and centered on green."));
            return panel;
        }

        private static UIElement CreateSplitViewSample()
        {
            var panel = CreateSamplePanel("SplitView is provided by ModernWpf and hosts a pane beside app content.");
            var contentText = new TextBlock
            {
                Text = "Select a pane item.",
                Margin = new Thickness(12),
                TextWrapping = TextWrapping.Wrap
            };
            var splitView = new Mux.SplitView
            {
                Width = 460,
                Height = 300,
                IsPaneOpen = true,
                DisplayMode = Mux.SplitViewDisplayMode.CompactOverlay,
                CompactPaneLength = 48,
                OpenPaneLength = 220,
                PaneBackground = CreateBrush("#F3F3F3"),
                Pane = CreateSplitViewPane(contentText),
                Content = new Border
                {
                    Padding = new Thickness(12),
                    Child = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "SPLITVIEW CONTENT",
                                FontWeight = FontWeights.SemiBold,
                                Margin = new Thickness(0, 0, 0, 8)
                            },
                            contentText
                        }
                    }
                }
            };

            var toggle = new ToggleButton
            {
                Content = "IsPaneOpen",
                IsChecked = splitView.IsPaneOpen,
                Margin = new Thickness(0, 12, 8, 0),
                Padding = new Thickness(14, 5, 14, 5)
            };
            toggle.Checked += delegate { splitView.IsPaneOpen = true; };
            toggle.Unchecked += delegate { splitView.IsPaneOpen = false; };

            var placement = new ComboBox
            {
                Width = 150,
                Margin = new Thickness(0, 12, 8, 0),
                ItemsSource = new[] { Mux.SplitViewPanePlacement.Left, Mux.SplitViewPanePlacement.Right },
                SelectedItem = splitView.PanePlacement
            };
            ControlHelper.SetHeader(placement, "PanePlacement");
            placement.SelectionChanged += delegate
            {
                if (placement.SelectedItem is Mux.SplitViewPanePlacement)
                {
                    splitView.PanePlacement = (Mux.SplitViewPanePlacement)placement.SelectedItem;
                }
            };

            var mode = new ComboBox
            {
                Width = 170,
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[]
                {
                    Mux.SplitViewDisplayMode.Inline,
                    Mux.SplitViewDisplayMode.CompactInline,
                    Mux.SplitViewDisplayMode.Overlay,
                    Mux.SplitViewDisplayMode.CompactOverlay
                },
                SelectedItem = splitView.DisplayMode
            };
            ControlHelper.SetHeader(mode, "DisplayMode");
            mode.SelectionChanged += delegate
            {
                if (mode.SelectedItem is Mux.SplitViewDisplayMode)
                {
                    splitView.DisplayMode = (Mux.SplitViewDisplayMode)mode.SelectedItem;
                }
            };

            var openLength = CreateSlider("OpenPaneLength", 128, 320, splitView.OpenPaneLength);
            openLength.Width = 180;
            openLength.ValueChanged += delegate { splitView.OpenPaneLength = openLength.Value; };

            var controls = new StackPanel { Orientation = Orientation.Horizontal };
            controls.Children.Add(toggle);
            controls.Children.Add(placement);
            controls.Children.Add(mode);
            controls.Children.Add(openLength);

            panel.Children.Add(splitView);
            panel.Children.Add(controls);
            return panel;
        }

        private static UIElement CreateStackPanelSample()
        {
            var panel = CreateSamplePanel("StackPanel arranges children in one direction; ModernWpf SimpleStackPanel adds a spacing property.");
            var stack = new Mux.SimpleStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8
            };
            PopulateColoredChildren(stack);

            var orientation = new ComboBox
            {
                Width = 180,
                Margin = new Thickness(0, 14, 8, 0),
                ItemsSource = new[] { Orientation.Vertical, Orientation.Horizontal },
                SelectedItem = stack.Orientation
            };
            ControlHelper.SetHeader(orientation, "Orientation");
            orientation.SelectionChanged += delegate
            {
                if (orientation.SelectedItem is Orientation)
                {
                    stack.Orientation = (Orientation)orientation.SelectedItem;
                }
            };

            var spacing = CreateSlider("Spacing", 0, 24, stack.Spacing);
            spacing.Width = 180;
            spacing.ValueChanged += delegate { stack.Spacing = Math.Round(spacing.Value); };

            var controls = new StackPanel { Orientation = Orientation.Horizontal };
            controls.Children.Add(orientation);
            controls.Children.Add(spacing);

            panel.Children.Add(stack);
            panel.Children.Add(controls);
            return panel;
        }

        private static UIElement CreateVariableSizedWrapGridSample()
        {
            var panel = CreateSamplePanel("VariableSizedWrapGrid maps to a WPF Grid with fixed cells and row/column spans.");
            var gridHost = new ContentControl();
            var orientation = new ComboBox
            {
                Width = 180,
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[] { Orientation.Vertical, Orientation.Horizontal },
                SelectedItem = Orientation.Vertical
            };
            ControlHelper.SetHeader(orientation, "Orientation");

            Action rebuild = delegate
            {
                gridHost.Content = CreateVariableGrid((Orientation)orientation.SelectedItem);
            };
            orientation.SelectionChanged += delegate { rebuild(); };
            rebuild();

            panel.Children.Add(gridHost);
            panel.Children.Add(orientation);
            return panel;
        }

        private static UIElement CreateViewboxSample()
        {
            var panel = CreateSamplePanel("Viewbox scales one child to fit the available size.");
            var viewbox = new Viewbox
            {
                Width = 200,
                Height = 200,
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.Both,
                Child = CreateViewboxContent()
            };

            var size = CreateSlider("Width/Height", 60, 300, 200);
            size.ValueChanged += delegate
            {
                viewbox.Width = size.Value;
                viewbox.Height = size.Value;
            };

            var stretch = new ComboBox
            {
                Width = 160,
                Margin = new Thickness(0, 12, 8, 0),
                ItemsSource = new[] { Stretch.None, Stretch.Fill, Stretch.Uniform, Stretch.UniformToFill },
                SelectedItem = viewbox.Stretch
            };
            ControlHelper.SetHeader(stretch, "Stretch");
            stretch.SelectionChanged += delegate
            {
                if (stretch.SelectedItem is Stretch)
                {
                    viewbox.Stretch = (Stretch)stretch.SelectedItem;
                }
            };

            var direction = new ComboBox
            {
                Width = 160,
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[] { StretchDirection.UpOnly, StretchDirection.DownOnly, StretchDirection.Both },
                SelectedItem = viewbox.StretchDirection
            };
            ControlHelper.SetHeader(direction, "StretchDirection");
            direction.SelectionChanged += delegate
            {
                if (direction.SelectedItem is StretchDirection)
                {
                    viewbox.StretchDirection = (StretchDirection)direction.SelectedItem;
                }
            };

            var controls = new StackPanel { Orientation = Orientation.Horizontal };
            controls.Children.Add(stretch);
            controls.Children.Add(direction);

            panel.Children.Add(viewbox);
            panel.Children.Add(size);
            panel.Children.Add(controls);
            return panel;
        }

        private static UIElement CreateSplitViewPane(TextBlock contentText)
        {
            var pane = new StackPanel();
            pane.Children.Add(new TextBlock
            {
                Text = "PANE CONTENT",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(60, 12, 0, 10)
            });

            foreach (var item in new[] { "People", "Globe", "Message", "Mail" })
            {
                var button = new Button
                {
                    Content = item,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Padding = new Thickness(14, 8, 14, 8),
                    Margin = new Thickness(4, 0, 4, 4)
                };
                button.Click += delegate
                {
                    contentText.Text = item + " Page";
                };
                pane.Children.Add(button);
            }

            return pane;
        }

        private static Grid CreateVariableGrid(Orientation orientation)
        {
            var grid = new Grid
            {
                Width = 400,
                Height = 170,
                Background = CreateBrush("#F3F3F3")
            };
            for (var i = 0; i < 4; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(48) });
            }

            if (orientation == Orientation.Vertical)
            {
                AddVariableTile(grid, Brushes.Red, 0, 0, 1, 1);
                AddVariableTile(grid, Brushes.Blue, 1, 0, 1, 2);
                AddVariableTile(grid, Brushes.Green, 2, 0, 2, 1);
                AddVariableTile(grid, Brushes.Gold, 0, 2, 2, 2);
            }
            else
            {
                AddVariableTile(grid, Brushes.Red, 0, 0, 1, 1);
                AddVariableTile(grid, Brushes.Blue, 0, 1, 2, 1);
                AddVariableTile(grid, Brushes.Green, 0, 2, 1, 2);
                AddVariableTile(grid, Brushes.Gold, 2, 1, 2, 2);
            }

            return grid;
        }

        private static void AddVariableTile(Grid grid, Brush fill, int column, int row, int columnSpan, int rowSpan)
        {
            var tile = new Rectangle
            {
                Fill = fill,
                Margin = new Thickness(4)
            };
            Grid.SetColumn(tile, column);
            Grid.SetRow(tile, row);
            Grid.SetColumnSpan(tile, columnSpan);
            Grid.SetRowSpan(tile, rowSpan);
            grid.Children.Add(tile);
        }

        private static UIElement CreateViewboxContent()
        {
            var stack = new StackPanel
            {
                Background = Brushes.DarkGray
            };
            var strip = new StackPanel { Orientation = Orientation.Horizontal };
            strip.Children.Add(CreateRect(40, 10, Brushes.Blue));
            strip.Children.Add(CreateRect(40, 10, Brushes.Green));
            strip.Children.Add(CreateRect(40, 10, Brushes.Red));
            strip.Children.Add(CreateRect(40, 10, Brushes.Gold));
            stack.Children.Add(strip);
            stack.Children.Add(new Border
            {
                Width = 160,
                Height = 80,
                Background = CreateBrush("#BDBDBD"),
                Child = new TextBlock
                {
                    Text = "Sample media",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
            stack.Children.Add(new TextBlock
            {
                Text = "This is text.",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 4)
            });

            return new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(15),
                Child = stack
            };
        }

        private static ComboBox CreateBrushCombo(string header, string selected, Action<Brush> changed)
        {
            var combo = new ComboBox
            {
                Width = 160,
                Margin = new Thickness(0, 0, 12, 0),
                ItemsSource = new[] { "Green", "Yellow", "Blue", "White", "Gold" },
                SelectedItem = selected
            };
            ControlHelper.SetHeader(combo, header);
            combo.SelectionChanged += delegate
            {
                changed(CreateNamedBrush((string)combo.SelectedItem));
            };
            return combo;
        }

        private static Brush CreateNamedBrush(string name)
        {
            switch (name)
            {
                case "Green":
                    return Brushes.Green;
                case "Yellow":
                    return Brushes.Yellow;
                case "Blue":
                    return Brushes.Blue;
                case "Gold":
                    return CreateBrush("#FFD700");
                default:
                    return Brushes.White;
            }
        }

        private static Slider CreateSlider(string header, double minimum, double maximum, double value)
        {
            var slider = new Slider
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                Width = 220,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                IsSnapToTickEnabled = true,
                TickFrequency = 1
            };
            ControlHelper.SetHeader(slider, header);
            return slider;
        }

        private static void PopulateColoredChildren(Panel panel)
        {
            panel.Children.Clear();
            panel.Children.Add(CreateRect(40, 40, Brushes.Red));
            panel.Children.Add(CreateRect(40, 40, Brushes.Blue));
            panel.Children.Add(CreateRect(40, 40, Brushes.Green));
            panel.Children.Add(CreateRect(40, 40, Brushes.Gold));
        }

        private static Rectangle CreateRect(double width, double height, Brush fill)
        {
            return new Rectangle
            {
                Width = width,
                Height = height,
                Fill = fill
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

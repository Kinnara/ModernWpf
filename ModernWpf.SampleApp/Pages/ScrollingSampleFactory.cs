using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.SampleApp.Pages
{
    internal static class ScrollingSampleFactory
    {
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
            var panel = CreateSamplePanel("PipsPager maps to glyph-like buttons that page through independent content.");
            var pages = new[]
            {
                "Overview",
                "Details",
                "Activity",
                "Settings",
                "Summary"
            };
            var selectedIndex = 0;
            var content = CreatePagerCard(pages[selectedIndex], selectedIndex + 1, pages.Length);
            var pipRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };

            Action refresh = null;
            refresh = delegate
            {
                content.Child = CreatePagerCardContent(pages[selectedIndex], selectedIndex + 1, pages.Length);
                for (var i = 0; i < pipRow.Children.Count; i++)
                {
                    var button = (Button)pipRow.Children[i];
                    button.FontWeight = i == selectedIndex ? FontWeights.SemiBold : FontWeights.Normal;
                    button.Content = i == selectedIndex ? "●" : "○";
                }
            };

            for (var i = 0; i < pages.Length; i++)
            {
                var pageIndex = i;
                var pip = new Button
                {
                    Content = pageIndex == selectedIndex ? "●" : "○",
                    Width = 34,
                    Height = 34,
                    Margin = new Thickness(0, 0, 6, 0),
                    ToolTip = "Page " + (pageIndex + 1)
                };
                pip.Click += delegate
                {
                    selectedIndex = pageIndex;
                    refresh();
                };
                pipRow.Children.Add(pip);
            }

            var commands = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };
            var previous = CreateButton("Previous");
            var next = CreateButton("Next");
            previous.Click += delegate
            {
                selectedIndex = Math.Max(0, selectedIndex - 1);
                refresh();
            };
            next.Click += delegate
            {
                selectedIndex = Math.Min(pages.Length - 1, selectedIndex + 1);
                refresh();
            };
            commands.Children.Add(previous);
            commands.Children.Add(next);

            panel.Children.Add(content);
            panel.Children.Add(pipRow);
            panel.Children.Add(commands);
            return panel;
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
            var panel = CreateSamplePanel("ScrollViewer displays content that is larger than its viewport and exposes scrollbar policy controls.");
            var scrollViewer = new ScrollViewer
            {
                Width = 420,
                Height = 240,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = CreateWideTextContent()
            };
            var output = CreateOutput("Offset: 0, 0");
            scrollViewer.ScrollChanged += delegate
            {
                output.Text = "Offset: " + scrollViewer.HorizontalOffset.ToString("0") + ", " + scrollViewer.VerticalOffset.ToString("0");
            };

            var options = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            options.Children.Add(CreateVisibilityCombo("Horizontal", scrollViewer.HorizontalScrollBarVisibility, delegate(ScrollBarVisibility value)
            {
                scrollViewer.HorizontalScrollBarVisibility = value;
            }));
            options.Children.Add(CreateVisibilityCombo("Vertical", scrollViewer.VerticalScrollBarVisibility, delegate(ScrollBarVisibility value)
            {
                scrollViewer.VerticalScrollBarVisibility = value;
            }));

            panel.Children.Add(scrollViewer);
            panel.Children.Add(options);
            panel.Children.Add(output);
            return panel;
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

        private static Border CreatePagerCard(string title, int index, int total)
        {
            return new Border
            {
                Width = 320,
                Height = 120,
                Padding = new Thickness(16),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Child = CreatePagerCardContent(title, index, total)
            };
        }

        private static UIElement CreatePagerCardContent(string title, int index, int total)
        {
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 22,
                FontWeight = FontWeights.SemiBold
            });
            stack.Children.Add(new TextBlock
            {
                Text = "Page " + index + " of " + total,
                Margin = new Thickness(0, 8, 0, 0),
                Opacity = 0.72
            });
            return stack;
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

        private static StackPanel CreateWideTextContent()
        {
            var content = new StackPanel
            {
                Width = 720
            };
            for (var i = 1; i <= 18; i++)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Row " + i + ": ScrollViewer keeps this long line readable without forcing the page layout to widen.",
                    Margin = new Thickness(0, 0, 0, 8),
                    TextWrapping = TextWrapping.NoWrap
                });
            }

            return content;
        }

        private static UIElement CreateVisibilityCombo(string header, ScrollBarVisibility selected, Action<ScrollBarVisibility> changed)
        {
            var combo = new ComboBox
            {
                Width = 150,
                Margin = new Thickness(0, 0, 12, 0),
                ItemsSource = new[]
                {
                    ScrollBarVisibility.Auto,
                    ScrollBarVisibility.Visible,
                    ScrollBarVisibility.Hidden,
                    ScrollBarVisibility.Disabled
                },
                SelectedItem = selected
            };
            ControlHelper.SetHeader(combo, header);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedItem is ScrollBarVisibility)
                {
                    changed((ScrollBarVisibility)combo.SelectedItem);
                }
            };
            return combo;
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

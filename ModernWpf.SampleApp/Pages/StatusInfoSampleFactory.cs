using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mux = ModernWpf.Controls;

namespace ModernWpf.SampleApp.Pages
{
    internal static class StatusInfoSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "InfoBadge":
                    return CreateInfoBadgeSample();
                case "InfoBar":
                    return CreateInfoBarSample();
                case "ProgressBar":
                    return CreateProgressBarSample();
                case "ProgressRing":
                    return CreateProgressRingSample();
                case "ToolTip":
                    return CreateToolTipSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateInfoBadgeSample()
        {
            var panel = CreateSamplePanel("InfoBadge highlights new, important, or attention-worthy state near related content.");
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(CreateBadge("1", "#005FB8", "Unread"));
            row.Children.Add(CreateBadge("99+", "#005FB8", "Many"));
            row.Children.Add(CreateBadge("!", "#C42B1C", "Needs attention"));
            panel.Children.Add(row);

            var output = CreateOutput("Badges are decorative WPF elements in this port because ModernWpf does not currently expose InfoBadge.");
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateInfoBarSample()
        {
            var panel = CreateSamplePanel("InfoBar presents inline app status without blocking the current task.");
            var host = new StackPanel();
            var infoBar = CreateInlineMessage(
                "Sync complete",
                "Your settings were saved and will be used the next time the app starts.",
                "#E6F2FB",
                "#005FB8",
                host);
            host.Children.Add(infoBar);

            var reset = new Button
            {
                Content = "Show InfoBar",
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            reset.Click += delegate
            {
                host.Children.Clear();
                host.Children.Add(CreateInlineMessage(
                    "Sync complete",
                    "Your settings were saved and will be used the next time the app starts.",
                    "#E6F2FB",
                    "#005FB8",
                    host));
                host.Children.Add(reset);
            };
            host.Children.Add(reset);
            panel.Children.Add(host);
            return panel;
        }

        private static UIElement CreateProgressBarSample()
        {
            var panel = CreateSamplePanel("ProgressBar communicates task completion for determinate and indeterminate work.");
            panel.Children.Add(new TextBlock { Text = "Installing package", Margin = new Thickness(0, 0, 0, 6) });
            panel.Children.Add(new Mux.ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 64,
                Width = 320,
                HorizontalAlignment = HorizontalAlignment.Left
            });

            panel.Children.Add(new TextBlock { Text = "Checking updates", Margin = new Thickness(0, 18, 0, 6) });
            panel.Children.Add(new Mux.ProgressBar
            {
                IsIndeterminate = true,
                Width = 320,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            return panel;
        }

        private static UIElement CreateProgressRingSample()
        {
            var panel = CreateSamplePanel("ProgressRing shows that work is active when the completion amount is not known.");
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(new Mux.ProgressRing
            {
                Width = 48,
                Height = 48,
                IsActive = true,
                Margin = new Thickness(0, 0, 16, 0)
            });
            row.Children.Add(new TextBlock
            {
                Text = "Loading account details...",
                VerticalAlignment = VerticalAlignment.Center
            });
            panel.Children.Add(row);
            return panel;
        }

        private static UIElement CreateToolTipSample()
        {
            var panel = CreateSamplePanel("ToolTip gives lightweight context when the pointer rests on a control.");
            panel.Children.Add(new Button
            {
                Content = "Hover for details",
                Padding = new Thickness(16, 6, 16, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                ToolTip = new ToolTip { Content = "ToolTips should clarify, not replace, visible labels." }
            });
            return panel;
        }

        private static Border CreateInlineMessage(string title, string message, string background, string accent, Panel host)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textPanel = new StackPanel();
            textPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 2)
            });
            textPanel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            });

            var close = new Button
            {
                Content = "Close",
                Padding = new Thickness(12, 4, 12, 4),
                Margin = new Thickness(16, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            close.Click += delegate
            {
                host.Children.Remove((UIElement)grid.Parent);
            };

            Grid.SetColumn(textPanel, 0);
            Grid.SetColumn(close, 1);
            grid.Children.Add(textPanel);
            grid.Children.Add(close);

            return new Border
            {
                Background = CreateBrush(background),
                BorderBrush = CreateBrush(accent),
                BorderThickness = new Thickness(4, 0, 0, 0),
                Padding = new Thickness(14, 12, 12, 12),
                Child = grid
            };
        }

        private static Border CreateBadge(string text, string background, string toolTip)
        {
            return new Border
            {
                Background = CreateBrush(background),
                CornerRadius = new CornerRadius(10),
                MinWidth = 20,
                Height = 20,
                Padding = new Thickness(6, 0, 6, 1),
                Margin = new Thickness(0, 0, 10, 0),
                ToolTip = toolTip,
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
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

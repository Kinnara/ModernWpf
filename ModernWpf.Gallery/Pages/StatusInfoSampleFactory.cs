using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
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
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("InfoBar"));
            var host = new StackPanel();
            GalleryAutomation.WithAutomationId(host, GalleryAutomation.SampleElementId("InfoBar", "Host"));
            var infoBar = new Mux.InfoBar
            {
                IsOpen = true,
                Severity = Mux.InfoBarSeverity.Informational,
                Title = "Title",
                Message = "Essential app message for your users to be informed of, acknowledge, or take action on.",
                Width = 560,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(infoBar, GalleryAutomation.SampleElementId("InfoBar", "InfoBar"));
            host.Children.Add(infoBar);

            var reset = new Button
            {
                Content = "Show InfoBar",
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(reset, GalleryAutomation.SampleElementId("InfoBar", "ShowButton"));
            reset.Click += delegate
            {
                infoBar.IsOpen = true;
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
            var panel = new GallerySamplePanel
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class DialogsFlyoutsSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "ContentDialog":
                    return CreateContentDialogSample();
                case "Flyout":
                    return CreateFlyoutSample();
                case "Popup":
                    return CreatePopupSample();
                case "TeachingTip":
                    return CreateTeachingTipSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateContentDialogSample()
        {
            var panel = CreateSamplePanel("ContentDialog asks the user to confirm a focused decision before continuing.");
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ContentDialog"));
            var output = CreateOutput("Dialog result: none.");
            var button = CreateButton("Show ContentDialog");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("ContentDialog", "ShowButton"));
            button.Click += async delegate
            {
                var dialog = new Mux.ContentDialog
                {
                    Title = "Delete item?",
                    Content = new TextBlock
                    {
                        Text = "This action removes the selected item from the list.",
                        TextWrapping = TextWrapping.Wrap
                    },
                    PrimaryButtonText = "Delete",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = Mux.ContentDialogButton.Secondary
                };
                var result = await dialog.ShowAsync();
                output.Text = "Dialog result: " + result + ".";
            };

            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateFlyoutSample()
        {
            var panel = CreateSamplePanel("Flyout shows lightweight contextual content anchored to a target.");
            var button = CreateButton("Show Flyout");
            var flyout = new Mux.Flyout
            {
                Placement = FlyoutPlacementMode.Bottom,
                Content = new StackPanel
                {
                    Width = 220,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Quick actions",
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 0, 0, 8)
                        },
                        new Button { Content = "Archive", Margin = new Thickness(0, 0, 0, 6) },
                        new Button { Content = "Pin" }
                    }
                }
            };
            Mux.FlyoutService.SetFlyout(button, flyout);
            panel.Children.Add(button);
            return panel;
        }

        private static UIElement CreatePopupSample()
        {
            var panel = CreateSamplePanel("Popup is a low-level floating surface; use it when you need explicit placement control.");
            var button = CreateButton("Toggle Popup");
            var popup = new Popup
            {
                PlacementTarget = button,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                AllowsTransparency = true,
                Child = CreateSurface(
                    "Popup content",
                    "This surface is positioned with WPF Popup placement.")
            };
            button.Click += delegate
            {
                popup.IsOpen = !popup.IsOpen;
            };

            panel.Children.Add(button);
            return panel;
        }

        private static UIElement CreateTeachingTipSample()
        {
            var panel = CreateSamplePanel("TeachingTip explains a new or important capability near the relevant control.");
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("TeachingTip"));
            panel.Resources["TeachingTipMinWidth"] = 48.0;

            var button = CreateButton("Show TeachingTip");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("TeachingTip", "ShowButton"));
            var tip = new Mux.TeachingTip
            {
                Target = button,
                Title = "This is the title",
                Subtitle = "And this is the subtitle",
                IconSource = new Mux.SymbolIconSource { Symbol = Mux.Symbol.Refresh }
            };
            GalleryAutomation.WithAutomationId(tip, GalleryAutomation.SampleElementId("TeachingTip", "TeachingTip"));
            button.Click += delegate
            {
                tip.IsOpen = true;
            };

            panel.Children.Add(button);
            panel.Children.Add(tip);
            return panel;
        }

        private static Border CreateSurface(string title, string message)
        {
            var textPanel = new StackPanel();
            textPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            textPanel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            });

            return new Border
            {
                Width = 280,
                Margin = new Thickness(0, 12, 0, 0),
                Padding = new Thickness(14),
                BorderThickness = new Thickness(1),
                Background = CreateBrush("#FAFAFA"),
                BorderBrush = CreateBrush("#D8D8D8"),
                Child = textPanel
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

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Content = text,
                Padding = new Thickness(16, 6, 16, 6),
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

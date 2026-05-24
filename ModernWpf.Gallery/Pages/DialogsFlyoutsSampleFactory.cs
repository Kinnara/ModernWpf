using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ModernWpf.Gallery.Models;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class DialogsFlyoutsSampleFactory
    {
        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "TeachingTip":
                    return CreateTeachingTipExamples(sampleSnippets);
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

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
            var panel = CreateSamplePanel("A basic content dialog with content.");
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ContentDialog"));
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var output = new TextBlock
            {
                Margin = new Thickness(12, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            var button = new Button
            {
                Content = "Show dialog"
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("ContentDialog", "ShowButton"));
            button.Click += async delegate
            {
                var dialog = new Mux.ContentDialog
                {
                    Title = "Save your work?",
                    Content = new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Lorem ipsum dolor sit amet, adipisicing elit.",
                                TextWrapping = TextWrapping.Wrap
                            },
                            new CheckBox
                            {
                                Content = "Upload your content to the cloud."
                            }
                        }
                    },
                    PrimaryButtonText = "Save",
                    SecondaryButtonText = "Don't Save",
                    CloseButtonText = "Cancel",
                    DefaultButton = Mux.ContentDialogButton.Primary
                };
                var result = await dialog.ShowAsync();
                if (result == Mux.ContentDialogResult.Primary)
                {
                    output.Text = "User saved their work";
                }
                else if (result == Mux.ContentDialogResult.Secondary)
                {
                    output.Text = "User did not save their work";
                }
                else
                {
                    output.Text = "User cancelled the dialog";
                }
            };

            row.Children.Add(button);
            row.Children.Add(output);
            panel.Children.Add(row);
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

            panel.Children.Add(CreateTargetedTeachingTipExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateTeachingTipExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "Show a targeted TeachingTip on a button.",
                    CreateTargetedTeachingTipExampleContent(assignRootAutomationId: true),
                    FindSnippetText(sampleSnippets, "TeachingTipSample1_xaml.txt"),
                    FindSnippetText(sampleSnippets, "TeachingTipSample1_cs.txt")),
                new GalleryExample(
                    "Show a non-targeted TeachingTip with buttons.",
                    CreateNonTargetedTeachingTipExampleContent(),
                    FindSnippetText(sampleSnippets, "TeachingTipSample2_xaml.txt"),
                    FindSnippetText(sampleSnippets, "TeachingTipSample2_cs.txt")),
                new GalleryExample(
                    "Show a targeted TeachingTip with hero content on a button.",
                    CreateHeroTeachingTipExampleContent(),
                    FindSnippetText(sampleSnippets, "TeachingTipSample3_xaml.txt"),
                    FindSnippetText(sampleSnippets, "TeachingTipSample3_cs.txt"))
            };
        }

        private static GallerySamplePanel CreateTeachingTipExampleRoot()
        {
            var root = new GallerySamplePanel();
            root.Resources["TeachingTipMinWidth"] = 48.0;
            return root;
        }

        private static GallerySamplePanel CreateTargetedTeachingTipExampleContent(bool assignRootAutomationId)
        {
            var root = CreateTeachingTipExampleRoot();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("TeachingTip"));
            }

            var button = new Button
            {
                Content = "Show TeachingTip",
                HorizontalAlignment = HorizontalAlignment.Left
            };
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

            root.Children.Add(button);
            root.Children.Add(tip);
            return root;
        }

        private static GallerySamplePanel CreateNonTargetedTeachingTipExampleContent()
        {
            var root = CreateTeachingTipExampleRoot();
            var button = new Button
            {
                Content = "Show TeachingTip",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("TeachingTip", "NonTargetedShowButton"));
            var tip = new Mux.TeachingTip
            {
                Title = "This is the title",
                Subtitle = "And this is the subtitle",
                ActionButtonContent = "Action button",
                CloseButtonContent = "Close button",
                IsLightDismissEnabled = true,
                PlacementMargin = new Thickness(20),
                PreferredPlacement = Mux.TeachingTipPlacementMode.Auto
            };
            GalleryAutomation.WithAutomationId(tip, GalleryAutomation.SampleElementId("TeachingTip", "NonTargetedTeachingTip"));
            button.Click += delegate
            {
                tip.IsOpen = true;
            };

            root.Children.Add(button);
            root.Children.Add(tip);
            return root;
        }

        private static GallerySamplePanel CreateHeroTeachingTipExampleContent()
        {
            var root = CreateTeachingTipExampleRoot();
            var button = new Button
            {
                Content = "Show TeachingTip",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("TeachingTip", "HeroShowButton"));
            var tip = new Mux.TeachingTip
            {
                Target = button,
                Title = "This is the title",
                Subtitle = "And this is the subtitle",
                PreferredPlacement = Mux.TeachingTipPlacementMode.Bottom,
                HeroContent = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/ModernWpf.Gallery;component/Assets/SampleMedia/sunset.jpg", UriKind.Absolute))
                },
                Content = new TextBlock
                {
                    Margin = new Thickness(0, 16, 0, 0),
                    Text = "Description can go here",
                    TextWrapping = TextWrapping.Wrap
                }
            };
            AutomationProperties.SetName(tip.HeroContent, "Sunset");
            GalleryAutomation.WithAutomationId(tip, GalleryAutomation.SampleElementId("TeachingTip", "HeroTeachingTip"));
            button.Click += delegate
            {
                tip.IsOpen = true;
            };

            root.Children.Add(button);
            root.Children.Add(tip);
            return root;
        }

        private static string FindSnippetText(IReadOnlyList<SampleSnippet> snippets, string title)
        {
            for (var i = 0; i < snippets.Count; i++)
            {
                if (string.Equals(snippets[i].Title, title, StringComparison.Ordinal))
                {
                    return snippets[i].Text;
                }
            }

            return null;
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

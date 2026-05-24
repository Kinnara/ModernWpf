using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using ModernWpf.Gallery.Models;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class DialogsFlyoutsSampleFactory
    {
        private const string FlyoutButtonXaml =
@"<Button Content=""Empty cart"">
    <Button.Flyout>
        <Flyout>
            <StackPanel>
                <TextBlock Style=""{ThemeResource BaseTextBlockStyle}"" Text=""All items will be removed. Do you want to continue?"" Margin=""0,0,0,12"" />
                <Button Click=""DeleteConfirmation_Click"" Content=""Yes, empty my cart"" />
            </StackPanel>
        </Flyout>
    </Button.Flyout>
</Button>";

        private const string FlyoutButtonCSharp =
@"private void DeleteConfirmation_Click(object sender, RoutedEventArgs e)
{
    if (this.Control1.Flyout is Flyout f)
    {
        f.Hide();
    }
}";

        private const string PopupOffsetXaml =
@"<Grid x:Name=""Output"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" >
    <Button Content=""Show Popup (using Offset)"" Click=""ShowPopupOffsetClicked"" />
    <Popup x:Name=""StandardPopup"" VerticalOffset=""$(VerticalOffset)"" HorizontalOffset=""$(HorizontalOffset)"" IsLightDismissEnabled=""$(IsLightDismissEnabled)"">
        <Border Padding=""20"" CornerRadius=""{StaticResource OverlayCornerRadius}"" Width=""200"" Height=""160"" BorderThickness=""1"" BorderBrush=""{ThemeResource SurfaceStrokeColorDefaultBrush}""
                Background=""{ThemeResource AcrylicBackgroundFillColorDefaultBrush}"">
            <StackPanel HorizontalAlignment=""Center"" VerticalAlignment=""Center"" Spacing=""8"">
                <TextBlock Text=""Simple Popup"" FontSize=""16"" HorizontalAlignment=""Center"" />
                <Button Content=""Close"" Click=""ClosePopupClicked"" />
            </StackPanel>
        </Border>
    </Popup>
</Grid>";

        private const string PopupOffsetCSharp =
@"// Handles the Click event on the Button on the page and opens the Popup.
private void ShowPopupOffsetClicked(object sender, RoutedEventArgs e)
{
    // open the Popup if it isn't open already
    if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }
}

// Handles the Click event on the Button inside the Popup control and closes the Popup.
private void ClosePopupClicked(object sender, RoutedEventArgs e)
{
    // if the Popup is open, then close it
    if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
}";

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "ContentDialog":
                    return CreateContentDialogExamples(sampleSnippets);
                case "Flyout":
                    return CreateFlyoutExamples();
                case "Popup":
                    return CreatePopupExamples();
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
            panel.Children.Add(CreateContentDialogExampleContent(
                title: "Save your work?",
                primaryButtonText: "Save",
                secondaryButtonText: "Don't Save",
                defaultButton: Mux.ContentDialogButton.Primary,
                buttonContent: "Show dialog",
                buttonAutomationId: GalleryAutomation.SampleElementId("ContentDialog", "ShowButton"),
                primaryResultText: "User saved their work",
                secondaryResultText: "User did not save their work",
                assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateContentDialogExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "A basic content dialog with content.",
                    CreateContentDialogExampleContent(
                        title: "Save your work?",
                        primaryButtonText: "Save",
                        secondaryButtonText: "Don't Save",
                        defaultButton: Mux.ContentDialogButton.Primary,
                        buttonContent: "Show dialog",
                        buttonAutomationId: GalleryAutomation.SampleElementId("ContentDialog", "ShowButton"),
                        primaryResultText: "User saved their work",
                        secondaryResultText: "User did not save their work",
                        assignRootAutomationId: true),
                    FindSnippetText(sampleSnippets, "ContentDialogSample1_xaml.txt"),
                    FindSnippetText(sampleSnippets, "ContentDialogSample1_cs.txt")),
                new GalleryExample(
                    "A content dialog without a default button.",
                    CreateContentDialogExampleContent(
                        title: "Replace file?",
                        primaryButtonText: "Replace",
                        secondaryButtonText: "Keep",
                        defaultButton: Mux.ContentDialogButton.None,
                        buttonContent: "Show dialog without default button",
                        buttonAutomationId: GalleryAutomation.SampleElementId("ContentDialog", "ShowNoDefaultButton"),
                        primaryResultText: "User replaced the file",
                        secondaryResultText: "User kept the file",
                        assignRootAutomationId: false),
                    FindSnippetText(sampleSnippets, "ContentDialogSample2_xaml.txt"),
                    FindSnippetText(sampleSnippets, "ContentDialogSample2_cs.txt"))
            };
        }

        private static GallerySamplePanel CreateContentDialogExampleContent(
            string title,
            string primaryButtonText,
            string secondaryButtonText,
            Mux.ContentDialogButton defaultButton,
            string buttonContent,
            string buttonAutomationId,
            string primaryResultText,
            string secondaryResultText,
            bool assignRootAutomationId)
        {
            var row = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(row, GalleryAutomation.SampleRootId("ContentDialog"));
            }

            var output = new TextBlock
            {
                Margin = new Thickness(12, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            var button = new Button
            {
                Content = buttonContent
            };
            GalleryAutomation.WithAutomationId(button, buttonAutomationId);
            button.Click += async delegate
            {
                var dialog = new Mux.ContentDialog
                {
                    Title = title,
                    Content = CreateContentDialogContent(),
                    PrimaryButtonText = primaryButtonText,
                    SecondaryButtonText = secondaryButtonText,
                    CloseButtonText = "Cancel",
                    DefaultButton = defaultButton
                };
                var result = await dialog.ShowAsync();
                if (result == Mux.ContentDialogResult.Primary)
                {
                    output.Text = primaryResultText;
                }
                else if (result == Mux.ContentDialogResult.Secondary)
                {
                    output.Text = secondaryResultText;
                }
                else
                {
                    output.Text = "User cancelled the dialog";
                }
            };

            row.Children.Add(button);
            row.Children.Add(output);
            return row;
        }

        private static StackPanel CreateContentDialogContent()
        {
            return new StackPanel
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
            };
        }

        private static UIElement CreateFlyoutSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("Flyout"));
            panel.Children.Add(CreateFlyoutButtonExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateFlyoutExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A button with a flyout",
                    CreateFlyoutButtonExampleContent(assignRootAutomationId: true),
                    FlyoutButtonXaml,
                    FlyoutButtonCSharp)
            };
        }

        private static GallerySamplePanel CreateFlyoutButtonExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("Flyout"));
            }

            root.Resources["SharedFlyout"] = new Mux.Flyout
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = "This Flyout is shared." }
                    }
                }
            };

            var button = new Button
            {
                Name = "Control1",
                Content = "Empty cart"
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("Flyout", "Button"));

            var flyout = new Mux.Flyout();
            flyout.Content = CreateFlyoutConfirmationContent(flyout);
            Mux.FlyoutService.SetFlyout(button, flyout);

            root.Children.Add(button);
            return root;
        }

        private static StackPanel CreateFlyoutConfirmationContent(Mux.Flyout flyout)
        {
            var panel = new StackPanel();
            var message = new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 12),
                Text = "All items will be removed. Do you want to continue?"
            };
            message.SetResourceReference(FrameworkElement.StyleProperty, "BaseTextBlockStyle");
            panel.Children.Add(message);

            var confirm = new Button
            {
                Content = "Yes, empty my cart"
            };
            confirm.Click += delegate(object sender, RoutedEventArgs args)
            {
                flyout.Hide();
            };
            panel.Children.Add(confirm);
            return panel;
        }

        private static UIElement CreatePopupSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("Popup"));
            panel.Children.Add(CreatePopupOffsetExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreatePopupExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Popup with Offset Positioning",
                    CreatePopupOffsetExampleContent(assignRootAutomationId: true),
                    PopupOffsetXaml,
                    PopupOffsetCSharp)
            };
        }

        private static GallerySamplePanel CreatePopupOffsetExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("Popup"));
            }

            var output = new Grid
            {
                Name = "Output",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            var showButton = new Button
            {
                Content = "Show Popup (using Offset)"
            };
            GalleryAutomation.WithAutomationId(showButton, GalleryAutomation.SampleElementId("Popup", "Button"));

            var lightDismiss = new Mux.ToggleSwitch
            {
                Name = "IsLightDismissEnabledToggleSwitch",
                Header = "IsLightDismissEnabled",
                IsOn = true,
                OffContent = "False",
                OnContent = "True"
            };
            var verticalOffset = CreatePopupOffsetNumberBox("VerticalOffset", -100, 100, 0);
            var horizontalOffset = CreatePopupOffsetNumberBox("HorizontalOffset", -100, 500, 200);
            var popup = new Popup
            {
                Name = "StandardPopup",
                PlacementTarget = showButton,
                Placement = PlacementMode.Bottom,
                AllowsTransparency = true,
                StaysOpen = false,
                HorizontalOffset = horizontalOffset.Value,
                VerticalOffset = verticalOffset.Value
            };
            popup.Child = CreatePopupSurface(popup);
            popup.Closed += delegate
            {
                lightDismiss.IsEnabled = true;
            };

            lightDismiss.Toggled += delegate
            {
                popup.StaysOpen = !lightDismiss.IsOn;
            };
            verticalOffset.ValueChanged += delegate(Mux.NumberBox sender, Mux.NumberBoxValueChangedEventArgs args)
            {
                popup.VerticalOffset = args.NewValue;
            };
            horizontalOffset.ValueChanged += delegate(Mux.NumberBox sender, Mux.NumberBoxValueChangedEventArgs args)
            {
                popup.HorizontalOffset = args.NewValue;
            };
            showButton.Click += delegate
            {
                if (!popup.IsOpen)
                {
                    popup.IsOpen = true;
                }

                lightDismiss.IsEnabled = false;
            };

            output.Children.Add(showButton);
            output.Children.Add(popup);
            root.Children.Add(CreatePopupExampleLayout(
                output,
                CreatePopupOptionsPanel(lightDismiss, verticalOffset, horizontalOffset)));
            return root;
        }

        private static Mux.NumberBox CreatePopupOffsetNumberBox(string name, double minimum, double maximum, double value)
        {
            return new Mux.NumberBox
            {
                Name = name,
                Header = name,
                LargeChange = 100,
                Maximum = maximum,
                Minimum = minimum,
                SmallChange = 10,
                SpinButtonPlacementMode = Mux.NumberBoxSpinButtonPlacementMode.Inline,
                Value = value
            };
        }

        private static UIElement CreatePopupSurface(Popup popup)
        {
            var border = new Border
            {
                MinWidth = 240,
                Padding = new Thickness(16),
                BorderThickness = new Thickness(1)
            };
            border.SetResourceReference(Border.BackgroundProperty, "AcrylicBackgroundFillColorDefaultBrush");
            border.SetResourceReference(Border.BorderBrushProperty, "SurfaceStrokeColorDefaultBrush");
            border.SetResourceReference(Border.CornerRadiusProperty, "OverlayCornerRadius");

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                FontSize = 16,
                Text = "Simple Popup"
            });
            var closeButton = new Button
            {
                Content = "Close",
                Margin = new Thickness(0, 8, 0, 0)
            };
            closeButton.Click += delegate
            {
                if (popup.IsOpen)
                {
                    popup.IsOpen = false;
                }
            };
            panel.Children.Add(closeButton);
            border.Child = panel;
            return border;
        }

        private static Grid CreatePopupExampleLayout(UIElement sample, UIElement options)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.Children.Add(sample);
            Grid.SetColumn(options, 2);
            layout.Children.Add(options);
            return layout;
        }

        private static StackPanel CreatePopupOptionsPanel(params UIElement[] children)
        {
            var panel = new StackPanel
            {
                Width = 160
            };
            foreach (var child in children)
            {
                panel.Children.Add(child);
            }

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

    }
}

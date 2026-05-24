using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class StatusInfoSampleFactory
    {
        private const string InfoBarLongMessage = "A long essential app message for your users to be informed of, acknowledge, or take action on. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin dapibus dolor vitae justo rutrum, ut lobortis nibh mattis. Aenean id elit commodo, semper felis nec.";

        private const string InfoBarExample1Xaml =
@"<InfoBar
    IsOpen=""True""
    Severity=""Informational""
    Title=""Title""
    Message=""Essential app message for your users to be informed of, acknowledge, or take action on."" />";

        private const string InfoBarExample2Xaml =
@"<InfoBar
    IsOpen=""True""
    Title=""Title""
    Message=""A long essential app message..."">
</InfoBar>";

        private const string InfoBarExample3Xaml =
@"<InfoBar
    IsOpen=""True""
    IsIconVisible=""True""
    IsClosable=""True""
    Title=""Title""
    Message=""Essential app message for your users to be informed of, acknowledge, or take action on."" />";

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "InfoBar":
                    return CreateInfoBarExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

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
            panel.Children.Add(CreateSeverityInfoBarExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateInfoBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A closable InfoBar with options to change its Severity.",
                    CreateSeverityInfoBarExampleContent(assignRootAutomationId: true),
                    InfoBarExample1Xaml,
                    null),
                new GalleryExample(
                    "A closable InfoBar with a long or short message and various buttons",
                    CreateMessageInfoBarExampleContent(),
                    InfoBarExample2Xaml,
                    null),
                new GalleryExample(
                    "A closable InfoBar with options to display the close button and icon",
                    CreateIconAndCloseInfoBarExampleContent(),
                    InfoBarExample3Xaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateInfoBarExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("InfoBar"));
            }

            return root;
        }

        private static GallerySamplePanel CreateSeverityInfoBarExampleContent(bool assignRootAutomationId)
        {
            var root = CreateInfoBarExampleRoot(assignRootAutomationId);
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

            var isOpen = CreateOptionCheckBox("Is Open", isChecked: true);
            isOpen.Name = "InfoBarIsOpenCheckBox1";
            isOpen.Checked += delegate { infoBar.IsOpen = true; };
            isOpen.Unchecked += delegate { infoBar.IsOpen = false; };

            var severity = CreateOptionComboBox("InfoBarSeverityComboBox");
            severity.Items.Add("Informational");
            severity.Items.Add("Success");
            severity.Items.Add("Warning");
            severity.Items.Add("Error");
            severity.SelectionChanged += delegate
            {
                var selectedSeverity = severity.SelectedItem as string;
                switch (selectedSeverity)
                {
                    case "Error":
                        infoBar.Severity = Mux.InfoBarSeverity.Error;
                        break;
                    case "Warning":
                        infoBar.Severity = Mux.InfoBarSeverity.Warning;
                        break;
                    case "Success":
                        infoBar.Severity = Mux.InfoBarSeverity.Success;
                        break;
                    default:
                        infoBar.Severity = Mux.InfoBarSeverity.Informational;
                        break;
                }
            };
            severity.SelectedItem = "Informational";

            var options = CreateOptionsPanel();
            options.Children.Add(isOpen);
            options.Children.Add(CreateOptionBlock("Severity", severity));

            root.Children.Add(CreateInfoBarExampleLayout(infoBar, options));
            return root;
        }

        private static GallerySamplePanel CreateMessageInfoBarExampleContent()
        {
            var root = CreateInfoBarExampleRoot(assignRootAutomationId: false);
            var infoBar = new Mux.InfoBar
            {
                IsOpen = true,
                Title = "Title",
                Message = InfoBarLongMessage,
                Width = 560,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(infoBar, GalleryAutomation.SampleElementId("InfoBar", "LongMessageInfoBar"));

            var isOpen = CreateOptionCheckBox("Is Open", isChecked: true);
            isOpen.Name = "InfoBarIsOpenCheckBox2";
            isOpen.Checked += delegate { infoBar.IsOpen = true; };
            isOpen.Unchecked += delegate { infoBar.IsOpen = false; };

            var messageLength = CreateOptionComboBox("InfoBarMessageComboBox");
            messageLength.Items.Add(new ComboBoxItem { Content = "Short" });
            messageLength.Items.Add(new ComboBoxItem { Content = "Long" });
            messageLength.SelectionChanged += delegate
            {
                infoBar.Message = messageLength.SelectedIndex == 0
                    ? "A short essential app message."
                    : InfoBarLongMessage;
            };
            messageLength.SelectedIndex = 1;

            var actionButton = CreateOptionComboBox("InfoBarActionButtonComboBox");
            actionButton.Items.Add(new ComboBoxItem { Content = "None" });
            actionButton.Items.Add(new ComboBoxItem { Content = "Button" });
            actionButton.Items.Add(new ComboBoxItem { Content = "Hyperlink" });
            actionButton.SelectionChanged += delegate
            {
                if (actionButton.SelectedIndex == 1)
                {
                    infoBar.ActionButton = new Button { Content = "Action" };
                }
                else if (actionButton.SelectedIndex == 2)
                {
                    infoBar.ActionButton = new Mux.HyperlinkButton
                    {
                        Content = "Informational link",
                        NavigateUri = new Uri("http://www.microsoft.com/")
                    };
                }
                else
                {
                    infoBar.ActionButton = null;
                }
            };
            actionButton.SelectedIndex = 0;

            var options = CreateOptionsPanel();
            options.Children.Add(isOpen);
            options.Children.Add(CreateOptionBlock("Message Length", messageLength));
            options.Children.Add(CreateOptionBlock("Action Button", actionButton));

            root.Children.Add(CreateInfoBarExampleLayout(infoBar, options));
            return root;
        }

        private static GallerySamplePanel CreateIconAndCloseInfoBarExampleContent()
        {
            var root = CreateInfoBarExampleRoot(assignRootAutomationId: false);
            var infoBar = new Mux.InfoBar
            {
                IsOpen = true,
                IsIconVisible = true,
                IsClosable = true,
                Title = "Title",
                Message = "Essential app message for your users to be informed of, acknowledge, or take action on.",
                Width = 560,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(infoBar, GalleryAutomation.SampleElementId("InfoBar", "IconAndCloseInfoBar"));

            var isOpen = CreateOptionCheckBox("Is Open", isChecked: true);
            isOpen.Name = "InfoBarIsOpenCheckBox3";
            isOpen.Checked += delegate { infoBar.IsOpen = true; };
            isOpen.Unchecked += delegate { infoBar.IsOpen = false; };

            var isIconVisible = CreateOptionCheckBox("Is Icon Visible", isChecked: true);
            isIconVisible.Name = "InfoBarIsIconVisibleCheckBox";
            isIconVisible.Checked += delegate { infoBar.IsIconVisible = true; };
            isIconVisible.Unchecked += delegate { infoBar.IsIconVisible = false; };

            var isClosable = CreateOptionCheckBox("Is Closable", isChecked: true);
            isClosable.Name = "InfoBarIsClosableCheckBox";
            isClosable.Checked += delegate { infoBar.IsClosable = true; };
            isClosable.Unchecked += delegate { infoBar.IsClosable = false; };

            var options = CreateOptionsPanel();
            options.Children.Add(isOpen);
            options.Children.Add(isIconVisible);
            options.Children.Add(isClosable);

            root.Children.Add(CreateInfoBarExampleLayout(infoBar, options));
            return root;
        }

        private static Grid CreateInfoBarExampleLayout(UIElement infoBar, UIElement options)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(560) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            layout.Children.Add(infoBar);
            Grid.SetColumn(options, 2);
            layout.Children.Add(options);
            return layout;
        }

        private static StackPanel CreateOptionsPanel()
        {
            return new StackPanel
            {
                Width = 150,
                VerticalAlignment = VerticalAlignment.Top
            };
        }

        private static CheckBox CreateOptionCheckBox(string content, bool isChecked)
        {
            return new CheckBox
            {
                Content = content,
                IsChecked = isChecked,
                Margin = new Thickness(0, 0, 0, 8)
            };
        }

        private static StackPanel CreateOptionBlock(string label, Control control)
        {
            var block = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 8)
            };
            block.Children.Add(new TextBlock
            {
                Text = label,
                Margin = new Thickness(0, 0, 0, 4)
            });
            block.Children.Add(control);
            return block;
        }

        private static ComboBox CreateOptionComboBox(string name)
        {
            return new ComboBox
            {
                Name = name,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
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

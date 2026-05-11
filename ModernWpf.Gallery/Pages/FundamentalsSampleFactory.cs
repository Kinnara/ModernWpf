using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Gallery.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class FundamentalsSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "XamlResources":
                    return CreateResourcesSample();
                case "XamlStyles":
                    return CreateStylesSample();
                case "Binding":
                    return CreateBindingSample();
                case "Templates":
                    return CreateTemplatesSample();
                case "CustomUserControls":
                    return CreateCustomUserControlsSample();
                case "CustomXamlConditionals":
                    return CreateConditionalsSample();
                case "ScratchPad":
                    return CreateScratchPadSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateResourcesSample()
        {
            var panel = CreateSamplePanel();
            panel.Children.Add(CreateDescription("Resources are resolved through WPF resource dictionaries and DynamicResource references."));

            var border = CreateSampleCard();
            border.Padding = new Thickness(20);
            border.SetResourceReference(Border.BackgroundProperty, "SystemControlHighlightAccentBrush");
            border.Child = new TextBlock
            {
                Text = "This card uses SystemControlHighlightAccentBrush from ModernWpf resources.",
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };

            panel.Children.Add(border);
            return panel;
        }

        private static UIElement CreateStylesSample()
        {
            var panel = CreateSamplePanel();
            panel.Children.Add(CreateDescription("A local WPF Style is applied to each button in this sample."));

            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(18, 8, 18, 8)));
            style.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(0, 0, 8, 8)));
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));

            var row = new WrapPanel();
            for (var i = 1; i <= 3; i++)
            {
                row.Children.Add(new Button
                {
                    Content = "Styled button " + i,
                    Style = style
                });
            }

            panel.Children.Add(row);
            return panel;
        }

        private static UIElement CreateBindingSample()
        {
            var viewModel = new BindingSampleViewModel();
            var panel = CreateSamplePanel();
            panel.DataContext = viewModel;
            panel.Children.Add(CreateDescription("The Slider, ProgressBar, and text all bind to the same view-model property."));

            var slider = new Slider
            {
                Minimum = 0,
                Maximum = 100,
                Width = 320,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            slider.SetBinding(Slider.ValueProperty, new Binding(nameof(BindingSampleViewModel.Value)) { Mode = BindingMode.TwoWay });

            var progress = new ProgressBar
            {
                Width = 320,
                Height = 8,
                Margin = new Thickness(0, 12, 0, 0),
                Minimum = 0,
                Maximum = 100,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            progress.SetBinding(ProgressBar.ValueProperty, new Binding(nameof(BindingSampleViewModel.Value)));

            var text = new TextBlock
            {
                Margin = new Thickness(0, 10, 0, 0)
            };
            text.SetBinding(TextBlock.TextProperty, new Binding(nameof(BindingSampleViewModel.Value)) { StringFormat = "Current value: {0:0}" });

            panel.Children.Add(slider);
            panel.Children.Add(progress);
            panel.Children.Add(text);
            return panel;
        }

        private static UIElement CreateTemplatesSample()
        {
            var panel = CreateSamplePanel();
            panel.Children.Add(CreateDescription("The list uses a WPF DataTemplate to render structured data."));

            var list = new ListBox
            {
                Width = 360,
                HorizontalAlignment = HorizontalAlignment.Left,
                ItemsSource = new[]
                {
                    new TemplateSampleItem("Typography", "Text hierarchy and readable spacing"),
                    new TemplateSampleItem("Color", "Theme-aware brushes and accent values"),
                    new TemplateSampleItem("Layout", "Consistent item composition")
                },
                ItemTemplate = (DataTemplate)XamlReader.Parse(
                    "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                    "<StackPanel Margin=\"8\">" +
                    "<TextBlock FontWeight=\"SemiBold\" Text=\"{Binding Title}\" />" +
                    "<TextBlock Margin=\"0,4,0,0\" Opacity=\"0.72\" Text=\"{Binding Detail}\" />" +
                    "</StackPanel>" +
                    "</DataTemplate>")
            };

            panel.Children.Add(list);
            return panel;
        }

        private static UIElement CreateCustomUserControlsSample()
        {
            var panel = CreateSamplePanel();
            panel.Children.Add(CreateDescription("The card below is a WPF UserControl with dependency properties."));
            panel.Children.Add(new CounterBadgeControl
            {
                Title = "Counter badge control",
                Width = 360,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            return panel;
        }

        private static UIElement CreateConditionalsSample()
        {
            var panel = CreateSamplePanel();
            panel.Children.Add(CreateDescription("WPF uses triggers and bindings for conditional UI state."));

            var checkBox = new CheckBox
            {
                Content = "Enable detailed state",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var text = new TextBlock
            {
                Text = "Compact state",
                Padding = new Thickness(14),
                TextWrapping = TextWrapping.Wrap
            };

            var style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.BackgroundProperty, Brushes.Transparent));
            style.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("IsChecked") { Source = checkBox },
                Value = true,
                Setters =
                {
                    new Setter(TextBlock.TextProperty, "Detailed state: conditional styling is active."),
                    new Setter(TextBlock.BackgroundProperty, new SolidColorBrush(Color.FromRgb(224, 244, 255)))
                }
            });
            text.Style = style;

            panel.Children.Add(checkBox);
            panel.Children.Add(text);
            return panel;
        }

        private static UIElement CreateScratchPadSample()
        {
            var panel = CreateSamplePanel();
            panel.Children.Add(CreateDescription("A small live scratch pad for trying text content and seeing it update immediately."));

            var textBox = new TextBox
            {
                Text = "Edit this text",
                Width = 360,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var preview = CreateSampleCard();
            preview.Padding = new Thickness(18);
            var previewText = new TextBlock
            {
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap
            };
            previewText.SetBinding(TextBlock.TextProperty, new Binding("Text") { Source = textBox });
            preview.Child = previewText;

            panel.Children.Add(textBox);
            panel.Children.Add(preview);
            return panel;
        }

        private static StackPanel CreateSamplePanel()
        {
            return new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
        }

        private static TextBlock CreateDescription(string text)
        {
            return new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            };
        }

        private static Border CreateSampleCard()
        {
            return new Border
            {
                MaxWidth = 560,
                HorizontalAlignment = HorizontalAlignment.Left,
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                Background = Brushes.Transparent
            };
        }

        private sealed class BindingSampleViewModel : INotifyPropertyChanged
        {
            private double _value = 42;

            public event PropertyChangedEventHandler PropertyChanged;

            public double Value
            {
                get { return _value; }
                set
                {
                    if (Math.Abs(_value - value) < 0.01)
                    {
                        return;
                    }

                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        private sealed class TemplateSampleItem
        {
            public TemplateSampleItem(string title, string detail)
            {
                Title = title;
                Detail = detail;
            }

            public string Title { get; }
            public string Detail { get; }
        }
    }
}

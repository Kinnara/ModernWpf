using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Mux = ModernWpf.Controls;

namespace ModernWpf.SampleApp.Pages
{
    internal static class BasicInputSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Button":
                    return CreateButtonSample();
                case "DropDownButton":
                    return CreateDropDownButtonSample();
                case "HyperlinkButton":
                    return CreateHyperlinkButtonSample();
                case "RepeatButton":
                    return CreateRepeatButtonSample();
                case "ToggleButton":
                    return CreateToggleButtonSample();
                case "SplitButton":
                    return CreateSplitButtonSample();
                case "ToggleSplitButton":
                    return CreateToggleSplitButtonSample();
                case "CheckBox":
                    return CreateCheckBoxSample();
                case "ColorPicker":
                    return CreateColorPickerSample();
                case "ComboBox":
                    return CreateComboBoxSample();
                case "RadioButton":
                    return CreateRadioButtonSample();
                case "RatingControl":
                    return CreateRatingControlSample();
                case "Slider":
                    return CreateSliderSample();
                case "ToggleSwitch":
                    return CreateToggleSwitchSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateButtonSample()
        {
            var panel = CreateSamplePanel("Trigger an immediate action from a standard WPF Button styled by ModernWpf resources.");
            var output = CreateOutput("Button has not been clicked.");
            var count = 0;
            var button = new Button
            {
                Content = "Click me",
                Padding = new Thickness(18, 8, 18, 8),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            button.Click += delegate
            {
                count++;
                output.Text = "Button clicked " + count + " times.";
            };
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateDropDownButtonSample()
        {
            var panel = CreateSamplePanel("Use DropDownButton when the primary action is choosing from a short command menu.");
            var output = CreateOutput("Choose a message action.");
            var menu = new Mux.MenuFlyout();
            foreach (var label in new[] { "Send", "Reply", "Reply all" })
            {
                var item = new MenuItem { Header = label };
                item.Click += delegate { output.Text = "Selected: " + label; };
                menu.Items.Add(item);
            }

            panel.Children.Add(new Mux.DropDownButton
            {
                Content = "Email",
                Flyout = menu,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateHyperlinkButtonSample()
        {
            var panel = CreateSamplePanel("HyperlinkButton presents navigation as a button-shaped affordance.");
            panel.Children.Add(new Mux.HyperlinkButton
            {
                Content = "Open ModernWpf project",
                NavigateUri = new Uri("https://github.com/Kinnara/ModernWpf"),
                HorizontalAlignment = HorizontalAlignment.Left
            });
            return panel;
        }

        private static UIElement CreateRepeatButtonSample()
        {
            var panel = CreateSamplePanel("RepeatButton keeps firing while it is pressed.");
            var output = CreateOutput("Hold the button to increment.");
            var count = 0;
            var button = new RepeatButton
            {
                Content = "Hold to repeat",
                Delay = 350,
                Interval = 60,
                Padding = new Thickness(18, 8, 18, 8),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            button.Click += delegate
            {
                count++;
                output.Text = "Repeat count: " + count;
            };
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateToggleButtonSample()
        {
            var panel = CreateSamplePanel("ToggleButton stores a binary checked state.");
            var output = CreateOutput("Toggle is off.");
            var button = new ToggleButton
            {
                Content = "Toggle option",
                Padding = new Thickness(18, 8, 18, 8),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            button.Checked += delegate { output.Text = "Toggle is on."; };
            button.Unchecked += delegate { output.Text = "Toggle is off."; };
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateSplitButtonSample()
        {
            var panel = CreateSamplePanel("SplitButton exposes a default action and secondary choices.");
            var output = CreateOutput("No save action selected.");
            var button = new Mux.SplitButton
            {
                Content = "Save",
                HorizontalAlignment = HorizontalAlignment.Left,
                Flyout = CreateCommandFlyout(output, "Save as copy", "Save as template")
            };
            button.Click += delegate { output.Text = "Default save selected."; };
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateToggleSplitButtonSample()
        {
            var panel = CreateSamplePanel("ToggleSplitButton combines a checked state with secondary options.");
            var output = CreateOutput("Preview is off.");
            var button = new Mux.ToggleSplitButton
            {
                Content = "Preview",
                HorizontalAlignment = HorizontalAlignment.Left,
                Flyout = CreateCommandFlyout(output, "Preview left", "Preview right")
            };
            button.IsCheckedChanged += delegate
            {
                output.Text = button.IsChecked ? "Preview is on." : "Preview is off.";
            };
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateCheckBoxSample()
        {
            var panel = CreateSamplePanel("CheckBox works well for independent options.");
            panel.Children.Add(new CheckBox { Content = "Enable notifications", IsChecked = true });
            panel.Children.Add(new CheckBox { Content = "Include preview text", Margin = new Thickness(0, 8, 0, 0) });
            panel.Children.Add(new CheckBox { Content = "Send diagnostics", Margin = new Thickness(0, 8, 0, 0) });
            return panel;
        }

        private static UIElement CreateColorPickerSample()
        {
            var panel = CreateSamplePanel("ModernWpf does not currently expose WinUI ColorPicker; this WPF sample uses RGB sliders and a live preview.");
            var preview = CreatePreviewSwatch();
            var red = CreateColorSlider("Red", 51);
            var green = CreateColorSlider("Green", 102);
            var blue = CreateColorSlider("Blue", 204);
            RoutedPropertyChangedEventHandler<double> update = delegate
            {
                preview.Background = new SolidColorBrush(Color.FromRgb((byte)red.Value, (byte)green.Value, (byte)blue.Value));
            };
            red.ValueChanged += update;
            green.ValueChanged += update;
            blue.ValueChanged += update;
            update(null, null);

            panel.Children.Add(preview);
            panel.Children.Add(red);
            panel.Children.Add(green);
            panel.Children.Add(blue);
            return panel;
        }

        private static UIElement CreateComboBoxSample()
        {
            var panel = CreateSamplePanel("ComboBox lets users choose one item from a compact list.");
            var output = CreateOutput("Selected: Medium");
            var comboBox = new ComboBox
            {
                Width = 220,
                HorizontalAlignment = HorizontalAlignment.Left,
                ItemsSource = new[] { "Small", "Medium", "Large" },
                SelectedIndex = 1
            };
            comboBox.SelectionChanged += delegate
            {
                output.Text = "Selected: " + comboBox.SelectedItem;
            };
            panel.Children.Add(comboBox);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateRadioButtonSample()
        {
            var panel = CreateSamplePanel("RadioButton presents a mutually exclusive choice within a group.");
            panel.Children.Add(new RadioButton { Content = "Daily", GroupName = "Frequency", IsChecked = true });
            panel.Children.Add(new RadioButton { Content = "Weekly", GroupName = "Frequency", Margin = new Thickness(0, 8, 0, 0) });
            panel.Children.Add(new RadioButton { Content = "Monthly", GroupName = "Frequency", Margin = new Thickness(0, 8, 0, 0) });
            return panel;
        }

        private static UIElement CreateRatingControlSample()
        {
            var panel = CreateSamplePanel("RatingControl captures a weighted preference with optional clearing.");
            panel.Children.Add(new Mux.RatingControl
            {
                Caption = "How useful is this sample?",
                MaxRating = 5,
                Value = 3,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            return panel;
        }

        private static UIElement CreateSliderSample()
        {
            var panel = CreateSamplePanel("Slider picks a numeric value from a bounded range.");
            var output = CreateOutput("Value: 50");
            var slider = new Slider
            {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                Width = 320,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            slider.ValueChanged += delegate { output.Text = "Value: " + slider.Value.ToString("0"); };
            panel.Children.Add(slider);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateToggleSwitchSample()
        {
            var panel = CreateSamplePanel("ToggleSwitch is a touch-friendly binary setting.");
            panel.Children.Add(new Mux.ToggleSwitch
            {
                Header = "Notifications",
                OffContent = "Off",
                OnContent = "On",
                IsOn = true,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            return panel;
        }

        private static Mux.MenuFlyout CreateCommandFlyout(TextBlock output, params string[] labels)
        {
            var flyout = new Mux.MenuFlyout();
            foreach (var label in labels)
            {
                var item = new MenuItem { Header = label };
                item.Click += delegate { output.Text = "Selected: " + label; };
                flyout.Items.Add(item);
            }

            return flyout;
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

        private static Border CreatePreviewSwatch()
        {
            return new Border
            {
                Width = 160,
                Height = 72,
                Margin = new Thickness(0, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        private static Slider CreateColorSlider(string name, double value)
        {
            return new Slider
            {
                Minimum = 0,
                Maximum = 255,
                Value = value,
                Width = 320,
                HorizontalAlignment = HorizontalAlignment.Left,
                AutoToolTipPlacement = AutoToolTipPlacement.TopLeft,
                Margin = new Thickness(0, 0, 0, 8),
                ToolTip = name
            };
        }
    }
}

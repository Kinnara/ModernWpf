using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Gallery.Models;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class BasicInputSampleFactory
    {
        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "DropDownButton":
                    return CreateDropDownButtonExamples(sampleSnippets);
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

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
            var panel = CreateSamplePanel("A simple Button with text content.");
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("Button"));
            var output = CreateOutput("");
            var count = 0;
            var button = new Button
            {
                Content = "Standard XAML button",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("Button", "PrimaryButton"));
            button.Click += delegate
            {
                count++;
                output.Text = "You clicked: Button1";
            };
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateDropDownButtonSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("DropDownButton"));
            panel.Children.Add(CreateSimpleDropDownButtonExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateDropDownButtonExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "Simple DropDownButton",
                    CreateSimpleDropDownButtonExampleContent(assignRootAutomationId: true),
                    FindSampleCodeText(sampleSnippets, "Buttons\\DropDown\\DropDownButton_Simple.txt"),
                    null),
                new GalleryExample(
                    "DropDownButton with Icons",
                    CreateIconDropDownButtonExampleContent(),
                    FindSampleCodeText(sampleSnippets, "Buttons\\DropDown\\DropDownButton_Icon.txt"),
                    null)
            };
        }

        private static GallerySamplePanel CreateSimpleDropDownButtonExampleContent(bool assignRootAutomationId)
        {
            var panel = CreateDropDownButtonExampleRoot(assignRootAutomationId);
            var button = new Mux.DropDownButton
            {
                Content = "Email",
                Flyout = CreateEmailMenuFlyout(includeIcons: false),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("DropDownButton", "DropDownButton"));
            panel.Children.Add(button);
            return panel;
        }

        private static GallerySamplePanel CreateIconDropDownButtonExampleContent()
        {
            var panel = CreateDropDownButtonExampleRoot(assignRootAutomationId: false);
            var button = new Mux.DropDownButton
            {
                Content = new Mux.FontIcon { Glyph = "\uE715" },
                Flyout = CreateEmailMenuFlyout(includeIcons: true),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, "Email");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("DropDownButton", "IconDropDownButton"));
            panel.Children.Add(button);
            return panel;
        }

        private static GallerySamplePanel CreateDropDownButtonExampleRoot(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("DropDownButton"));
            }

            return panel;
        }

        private static Mux.MenuFlyout CreateEmailMenuFlyout(bool includeIcons)
        {
            var flyout = new Mux.MenuFlyout
            {
                Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
            };
            flyout.Items.Add(CreateEmailMenuItem("Send", includeIcons ? "\uE725" : null));
            flyout.Items.Add(CreateEmailMenuItem("Reply", includeIcons ? "\uE8CA" : null));
            flyout.Items.Add(CreateEmailMenuItem("Reply All", includeIcons ? "\uE8C2" : null));
            return flyout;
        }

        private static MenuItem CreateEmailMenuItem(string text, string iconGlyph)
        {
            var item = new MenuItem
            {
                Header = text
            };
            if (iconGlyph != null)
            {
                item.Icon = new Mux.FontIcon { Glyph = iconGlyph };
            }

            return item;
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
            var panel = CreateSamplePanel("ColorPicker lets users inspect and adjust a color value with spectrum, slider, preview, and text input surfaces.");
            panel.Children.Add(new Mux.ColorPicker
            {
                Color = System.Windows.Media.Color.FromRgb(51, 102, 204),
                IsAlphaEnabled = true,
                PreviousColor = System.Windows.Media.Colors.White,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            return panel;
        }

        private static UIElement CreateComboBoxSample()
        {
            var panel = CreateSamplePanel("A ComboBox with items defined inline and its width set.");
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ComboBox"));
            var output = new Rectangle
            {
                Width = 100,
                Height = 30,
                Margin = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var comboBox = new ComboBox
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(comboBox, "Colors");
            ControlHelper.SetPlaceholderText(comboBox, "Pick a color");
            comboBox.Items.Add("Blue");
            comboBox.Items.Add("Green");
            comboBox.Items.Add("Red");
            comboBox.Items.Add("Yellow");
            GalleryAutomation.WithAutomationId(comboBox, GalleryAutomation.SampleElementId("ComboBox", "ComboBox"));
            comboBox.SelectionChanged += delegate
            {
                output.Fill = CreateColorBrush(Convert.ToString(comboBox.SelectedItem));
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

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string relativePath)
        {
            var fileName = System.IO.Path.GetFileName(relativePath);
            for (var i = 0; i < snippets.Count; i++)
            {
                if (string.Equals(snippets[i].Title, fileName, StringComparison.Ordinal) ||
                    string.Equals(snippets[i].Title, relativePath, StringComparison.Ordinal))
                {
                    return snippets[i].Text;
                }
            }

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode", relativePath);
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
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

        private static Brush CreateColorBrush(string colorName)
        {
            switch (colorName)
            {
                case "Yellow":
                    return Brushes.Yellow;
                case "Green":
                    return Brushes.Green;
                case "Blue":
                    return Brushes.Blue;
                case "Red":
                    return Brushes.Red;
                default:
                    return Brushes.Transparent;
            }
        }
    }
}

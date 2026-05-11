using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.SampleApp.Pages
{
    internal static class WindowingSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AppWindow":
                    return CreateAppWindowSample();
                case "AppWindowTitleBar":
                    return CreateAppWindowTitleBarSample();
                case "CreateMultipleWindows":
                    return CreateMultipleWindowsSample();
                case "TitleBar":
                    return CreateTitleBarSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateAppWindowSample()
        {
            var panel = CreateSamplePanel("AppWindow maps to WPF Window APIs for size, state, ownership, topmost behavior, and presentation style.");
            var preview = CreateWindowPreview("AppWindow preview", "Overlapped", CreateBrush("#0078D4"), Brushes.White);
            var output = CreateOutput("Ready to open a WPF Window equivalent.");

            var size = new ComboBox
            {
                Width = 210,
                ItemsSource = new[] { "Default 640 x 420", "Wide 820 x 460", "Compact 380 x 280" },
                SelectedIndex = 0,
                Margin = new Thickness(0, 0, 12, 0)
            };
            ControlHelper.SetHeader(size, "Size");

            var presenter = new ComboBox
            {
                Width = 190,
                ItemsSource = new[] { "Overlapped", "Maximized", "Compact overlay" },
                SelectedIndex = 0,
                Margin = new Thickness(0, 0, 12, 0)
            };
            ControlHelper.SetHeader(presenter, "Presenter");

            var topmost = new ToggleButton
            {
                Content = "Topmost",
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 22, 12, 0)
            };

            Action updatePreview = delegate
            {
                var selectedPresenter = presenter.SelectedItem as string ?? "Overlapped";
                var selectedSize = size.SelectedItem as string ?? "Default 640 x 420";
                SetPreviewText(preview, "AppWindow preview", selectedPresenter + ", " + selectedSize);
            };
            size.SelectionChanged += delegate { updatePreview(); };
            presenter.SelectionChanged += delegate { updatePreview(); };

            var settings = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            settings.Children.Add(size);
            settings.Children.Add(presenter);
            settings.Children.Add(topmost);

            var open = CreateButton("Open window");
            open.Margin = new Thickness(0, 12, 8, 0);
            open.Click += delegate
            {
                var selectedSize = size.SelectedIndex;
                var selectedPresenter = presenter.SelectedItem as string ?? "Overlapped";
                var dimensions = GetWindowDimensions(selectedSize);
                var window = CreateModernWindow((FrameworkElement)open, "AppWindow equivalent", dimensions.Width, dimensions.Height);
                window.Topmost = topmost.IsChecked == true || selectedPresenter == "Compact overlay";
                window.ResizeMode = selectedPresenter == "Compact overlay" ? ResizeMode.NoResize : ResizeMode.CanResize;
                window.Content = CreateWindowBody(
                    "AppWindow equivalent",
                    "This WPF window uses ModernWpf chrome and standard Window APIs for sizing and presentation.");

                if (selectedPresenter == "Maximized")
                {
                    window.WindowState = WindowState.Maximized;
                }

                window.Show();
                output.Text = "Opened: " + selectedPresenter + " window (" + dimensions.Width + " x " + dimensions.Height + ").";
            };

            panel.Children.Add(preview);
            panel.Children.Add(settings);
            panel.Children.Add(open);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateAppWindowTitleBarSample()
        {
            var panel = CreateSamplePanel("AppWindowTitleBar maps to ModernWpf title bar attached properties for colors, inactive state, and system buttons.");
            var preview = CreateWindowPreview("Title bar", "Active state", CreateBrush("#0078D4"), Brushes.White);
            var output = CreateOutput("Choose a palette and open a themed WPF window.");

            var background = CreatePaletteCombo("Background", 0);
            var foreground = CreatePaletteCombo("Foreground", 4);
            var inactive = new ToggleButton
            {
                Content = "Inactive preview",
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 22, 12, 0)
            };
            var showIcon = new ToggleButton
            {
                Content = "Icon",
                IsChecked = true,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 22, 12, 0)
            };

            Action updatePreview = delegate
            {
                var bg = inactive.IsChecked == true ? CreateBrush("#E6E6E6") : GetPaletteBrush(background);
                var fg = inactive.IsChecked == true ? CreateBrush("#606060") : GetPaletteBrush(foreground);
                ApplyPreviewChrome(preview, bg, fg, showIcon.IsChecked == true);
                SetPreviewText(preview, "Title bar", inactive.IsChecked == true ? "Inactive state" : "Active state");
            };
            background.SelectionChanged += delegate { updatePreview(); };
            foreground.SelectionChanged += delegate { updatePreview(); };
            inactive.Checked += delegate { updatePreview(); };
            inactive.Unchecked += delegate { updatePreview(); };
            showIcon.Checked += delegate { updatePreview(); };
            showIcon.Unchecked += delegate { updatePreview(); };

            var settings = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            settings.Children.Add(background);
            settings.Children.Add(foreground);
            settings.Children.Add(inactive);
            settings.Children.Add(showIcon);

            var open = CreateButton("Open themed window");
            open.Margin = new Thickness(0, 12, 8, 0);
            open.Click += delegate
            {
                var window = CreateModernWindow((FrameworkElement)open, "AppWindowTitleBar equivalent", 620, 380);
                Mux.TitleBar.SetBackground(window, GetPaletteBrush(background));
                Mux.TitleBar.SetForeground(window, GetPaletteBrush(foreground));
                Mux.TitleBar.SetInactiveBackground(window, CreateBrush("#E6E6E6"));
                Mux.TitleBar.SetInactiveForeground(window, CreateBrush("#606060"));
                Mux.TitleBar.SetIsIconVisible(window, showIcon.IsChecked == true);
                window.Content = CreateWindowBody(
                    "Custom title bar colors",
                    "ModernWpf exposes WPF attached properties for active and inactive title bar appearance.");
                window.Show();
                output.Text = "Opened themed window using ModernWpf title bar attached properties.";
            };

            panel.Children.Add(preview);
            panel.Children.Add(settings);
            panel.Children.Add(open);
            panel.Children.Add(output);
            updatePreview();
            return panel;
        }

        private static UIElement CreateMultipleWindowsSample()
        {
            var panel = CreateSamplePanel("Multiple windows maps to creating several WPF Window instances on the current UI thread.");
            var windows = new List<Window>();
            var output = CreateOutput("Open windows: 0");
            var preview = CreateWindowPreview("Main gallery window", "Creates owned child windows", CreateBrush("#605E5C"), Brushes.White);

            Action refresh = delegate { output.Text = "Open windows: " + windows.Count; };

            var commands = CreateCommandRow();
            var createChild = CreateButton("Create window");
            var createTool = CreateButton("Create tool window");
            var closeAll = CreateButton("Close all");
            createChild.Click += delegate
            {
                var window = CreateNumberedWindow((FrameworkElement)createChild, windows.Count + 1, false);
                windows.Add(window);
                window.Closed += delegate
                {
                    windows.Remove(window);
                    refresh();
                };
                window.Show();
                refresh();
            };
            createTool.Click += delegate
            {
                var window = CreateNumberedWindow((FrameworkElement)createTool, windows.Count + 1, true);
                windows.Add(window);
                window.Closed += delegate
                {
                    windows.Remove(window);
                    refresh();
                };
                window.Show();
                refresh();
            };
            closeAll.Click += delegate
            {
                var copy = windows.ToArray();
                foreach (var window in copy)
                {
                    window.Close();
                }
                windows.Clear();
                refresh();
            };
            commands.Children.Add(createChild);
            commands.Children.Add(createTool);
            commands.Children.Add(closeAll);

            panel.Children.Add(preview);
            panel.Children.Add(commands);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateTitleBarSample()
        {
            var panel = CreateSamplePanel("TitleBar provides ModernWpf chrome with optional icon, back button, and interactive content in the title area.");
            var preview = CreateInteractiveTitleBarPreview();
            var output = CreateOutput("Back button request will be reported here.");

            var showBack = new ToggleButton
            {
                Content = "Back button",
                IsChecked = true,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 12, 0)
            };
            var enableBack = new ToggleButton
            {
                Content = "Back enabled",
                IsChecked = true,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 12, 0)
            };
            var showIcon = new ToggleButton
            {
                Content = "Icon",
                IsChecked = true,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 12, 0)
            };
            var extend = new ToggleButton
            {
                Content = "Extend content",
                IsChecked = false,
                Padding = new Thickness(12, 6, 12, 6)
            };

            Action updatePreview = delegate
            {
                SetNamedElementVisibility(preview, "BackButton", showBack.IsChecked == true);
                SetNamedElementVisibility(preview, "Icon", showIcon.IsChecked == true);
                SetNamedElementOpacity(preview, "BackButton", enableBack.IsChecked == true ? 1 : 0.42);
                var root = preview.Child as Grid;
                if (root != null)
                {
                    root.Background = extend.IsChecked == true ? CreateBrush("#EEF6FC") : Brushes.White;
                }
            };
            showBack.Checked += delegate { updatePreview(); };
            showBack.Unchecked += delegate { updatePreview(); };
            enableBack.Checked += delegate { updatePreview(); };
            enableBack.Unchecked += delegate { updatePreview(); };
            showIcon.Checked += delegate { updatePreview(); };
            showIcon.Unchecked += delegate { updatePreview(); };
            extend.Checked += delegate { updatePreview(); };
            extend.Unchecked += delegate { updatePreview(); };

            var toggles = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            toggles.Children.Add(showBack);
            toggles.Children.Add(enableBack);
            toggles.Children.Add(showIcon);
            toggles.Children.Add(extend);

            var open = CreateButton("Open title bar window");
            open.Margin = new Thickness(0, 12, 8, 0);
            open.Click += delegate
            {
                var window = CreateModernWindow((FrameworkElement)open, "TitleBar equivalent", 660, 390);
                Mux.TitleBar.SetIsBackButtonVisible(window, showBack.IsChecked == true);
                Mux.TitleBar.SetIsBackEnabled(window, enableBack.IsChecked == true);
                Mux.TitleBar.SetIsIconVisible(window, showIcon.IsChecked == true);
                Mux.TitleBar.SetExtendViewIntoTitleBar(window, extend.IsChecked == true);
                Mux.TitleBar.AddBackRequestedHandler(window, delegate(object sender, Mux.BackRequestedEventArgs args)
                {
                    output.Text = "Back requested from child window at " + DateTime.Now.ToLongTimeString() + ".";
                    args.Handled = true;
                });
                window.Content = CreateWindowBody(
                    "ModernWpf TitleBar",
                    "This window uses ModernWpf title bar attached properties from the sample controls.");
                window.Show();
                output.Text = "Opened ModernWpf title bar window.";
            };

            panel.Children.Add(preview);
            panel.Children.Add(toggles);
            panel.Children.Add(open);
            panel.Children.Add(output);
            updatePreview();
            return panel;
        }

        private static Window CreateNumberedWindow(FrameworkElement ownerElement, int number, bool toolWindow)
        {
            var window = CreateModernWindow(ownerElement, toolWindow ? "Tool window " + number : "Child window " + number, toolWindow ? 420 : 520, toolWindow ? 260 : 340);
            window.ShowInTaskbar = !toolWindow;
            window.ResizeMode = toolWindow ? ResizeMode.CanResizeWithGrip : ResizeMode.CanResize;
            Mux.TitleBar.SetIsIconVisible(window, true);
            window.Content = CreateWindowBody(
                toolWindow ? "Owned tool window" : "Owned child window",
                "Window #" + number + " was created from the Gallery sample on the current WPF UI thread.");
            return window;
        }

        private static Window CreateModernWindow(FrameworkElement ownerElement, string title, double width, double height)
        {
            var window = new Window
            {
                Title = title,
                Width = width,
                Height = height,
                MinWidth = 360,
                MinHeight = 240,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = CreateBrush("#F9F9F9")
            };
            var owner = Window.GetWindow(ownerElement);
            if (owner != null)
            {
                window.Owner = owner;
            }
            ThemeManager.SetIsThemeAware(window, true);
            WindowHelper.SetUseModernWindowStyle(window, true);
            Mux.TitleBar.SetIsIconVisible(window, true);
            return window;
        }

        private static FrameworkElement CreateWindowBody(string title, string body)
        {
            var close = CreateButton("Close");
            var grid = new Grid
            {
                Margin = new Thickness(32)
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 28,
                FontWeight = FontWeights.SemiBold
            });
            stack.Children.Add(new TextBlock
            {
                Text = body,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 10, 0, 0),
                Opacity = 0.72
            });
            Grid.SetRow(stack, 0);

            close.HorizontalAlignment = HorizontalAlignment.Right;
            close.Margin = new Thickness(0);
            close.Click += delegate
            {
                var window = Window.GetWindow(close);
                if (window != null)
                {
                    window.Close();
                }
            };
            Grid.SetRow(close, 1);

            grid.Children.Add(stack);
            grid.Children.Add(close);
            return grid;
        }

        private static Border CreateWindowPreview(string title, string subtitle, Brush titleBarBrush, Brush titleBrush)
        {
            var titleText = new TextBlock
            {
                Name = "PreviewTitle",
                Text = title,
                Foreground = titleBrush,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            };
            var subtitleText = new TextBlock
            {
                Name = "PreviewSubtitle",
                Text = subtitle,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(22, 18, 22, 0)
            };
            var icon = new Rectangle
            {
                Name = "Icon",
                Width = 14,
                Height = 14,
                Fill = titleBrush,
                RadiusX = 3,
                RadiusY = 3,
                Margin = new Thickness(14, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var titleBar = new Grid
            {
                Name = "PreviewChrome",
                Height = 38,
                Background = titleBarBrush
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(icon, 0);
            Grid.SetColumn(titleText, 1);
            titleBar.Children.Add(icon);
            titleBar.Children.Add(titleText);
            titleBar.Children.Add(CreateCaptionButtons(titleBrush));

            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(titleBar, 0);
            Grid.SetRow(subtitleText, 1);
            content.Children.Add(titleBar);
            content.Children.Add(subtitleText);

            return new Border
            {
                Width = 520,
                Height = 250,
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#C8C8C8"),
                Background = Brushes.White,
                Child = content
            };
        }

        private static Border CreateInteractiveTitleBarPreview()
        {
            var titleBar = new Grid
            {
                Height = 44,
                Background = CreateBrush("#F9F9F9")
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var back = new Button
            {
                Name = "BackButton",
                Content = "<",
                Width = 44,
                Height = 32,
                Margin = new Thickness(6, 6, 0, 6)
            };
            var icon = new Rectangle
            {
                Name = "Icon",
                Width = 16,
                Height = 16,
                Fill = CreateBrush("#0078D4"),
                RadiusX = 4,
                RadiusY = 4,
                Margin = new Thickness(10, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var title = new TextBlock
            {
                Text = "ModernWpf Gallery",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 16, 0)
            };
            var search = new TextBox
            {
                Text = "Interactive content",
                Width = 180,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 6, 16, 6)
            };
            var buttons = CreateCaptionButtons(CreateBrush("#202020"));

            Grid.SetColumn(back, 0);
            Grid.SetColumn(icon, 1);
            Grid.SetColumn(title, 2);
            Grid.SetColumn(search, 3);
            Grid.SetColumn(buttons, 4);
            titleBar.Children.Add(back);
            titleBar.Children.Add(icon);
            titleBar.Children.Add(title);
            titleBar.Children.Add(search);
            titleBar.Children.Add(buttons);

            var body = new Border
            {
                Padding = new Thickness(20),
                Child = new TextBlock
                {
                    Text = "The preview represents a ModernWpf title bar with drag region controls and optional interactive content.",
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.72
                }
            };
            var root = new Grid
            {
                Background = Brushes.White
            };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(titleBar, 0);
            Grid.SetRow(body, 1);
            root.Children.Add(titleBar);
            root.Children.Add(body);

            return new Border
            {
                Width = 560,
                Height = 190,
                BorderBrush = CreateBrush("#C8C8C8"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Child = root
            };
        }

        private static StackPanel CreateCaptionButtons(Brush foreground)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(panel, 2);
            panel.Children.Add(CreateCaptionGlyph("_", foreground));
            panel.Children.Add(CreateCaptionGlyph("[]", foreground));
            panel.Children.Add(CreateCaptionGlyph("X", foreground));
            return panel;
        }

        private static TextBlock CreateCaptionGlyph(string text, Brush foreground)
        {
            return new TextBlock
            {
                Text = text,
                Width = 44,
                Height = 38,
                Foreground = foreground,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0, 9, 0, 0)
            };
        }

        private static ComboBox CreatePaletteCombo(string header, int selectedIndex)
        {
            var combo = new ComboBox
            {
                Width = 180,
                ItemsSource = new[]
                {
                    "Blue",
                    "Purple",
                    "Neutral",
                    "Light",
                    "White",
                    "Black"
                },
                SelectedIndex = selectedIndex,
                Margin = new Thickness(0, 0, 12, 0)
            };
            ControlHelper.SetHeader(combo, header);
            return combo;
        }

        private static Brush GetPaletteBrush(ComboBox combo)
        {
            switch (combo.SelectedItem as string)
            {
                case "Purple":
                    return CreateBrush("#5C2D91");
                case "Neutral":
                    return CreateBrush("#605E5C");
                case "Light":
                    return CreateBrush("#F3F3F3");
                case "White":
                    return Brushes.White;
                case "Black":
                    return CreateBrush("#202020");
                default:
                    return CreateBrush("#0078D4");
            }
        }

        private static void ApplyPreviewChrome(Border preview, Brush background, Brush foreground, bool iconVisible)
        {
            var chrome = FindNamedElement<Panel>(preview, "PreviewChrome");
            if (chrome != null)
            {
                chrome.Background = background;
            }
            var title = FindNamedElement<TextBlock>(preview, "PreviewTitle");
            if (title != null)
            {
                title.Foreground = foreground;
            }
            var icon = FindNamedElement<Rectangle>(preview, "Icon");
            if (icon != null)
            {
                icon.Fill = foreground;
                icon.Visibility = iconVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void SetPreviewText(Border preview, string title, string subtitle)
        {
            var titleBlock = FindNamedElement<TextBlock>(preview, "PreviewTitle");
            var subtitleBlock = FindNamedElement<TextBlock>(preview, "PreviewSubtitle");
            if (titleBlock != null)
            {
                titleBlock.Text = title;
            }
            if (subtitleBlock != null)
            {
                subtitleBlock.Text = subtitle;
            }
        }

        private static void SetNamedElementVisibility(Border root, string name, bool isVisible)
        {
            var element = FindNamedElement<UIElement>(root, name);
            if (element != null)
            {
                element.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void SetNamedElementOpacity(Border root, string name, double opacity)
        {
            var element = FindNamedElement<UIElement>(root, name);
            if (element != null)
            {
                element.Opacity = opacity;
            }
        }

        private static T FindNamedElement<T>(DependencyObject root, string name)
            where T : UIElement
        {
            if (root == null)
            {
                return null;
            }

            var frameworkElement = root as FrameworkElement;
            var typedElement = frameworkElement as T;
            if (frameworkElement != null && frameworkElement.Name == name && typedElement != null)
            {
                return typedElement;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var match = FindNamedElement<T>(child, name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Size GetWindowDimensions(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 1:
                    return new Size(820, 460);
                case 2:
                    return new Size(380, 280);
                default:
                    return new Size(640, 420);
            }
        }

        private static StackPanel CreateCommandRow()
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
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
    }
}

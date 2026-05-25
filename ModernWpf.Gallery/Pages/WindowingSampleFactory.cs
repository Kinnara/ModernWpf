using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class WindowingSampleFactory
    {
        private const string CreateMultipleWindowsCSharp =
@"// Ensure you close the child window before closing the parent window to avoid application crash.
var childWindow = new Window()
{
    ExtendsContentIntoTitleBar = true,
    SystemBackdrop = new MicaBackdrop(),
    Content = new Page()
    {
        Content = new TextBlock()
        {
            Text = ""New child window!"",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        },
        // Get the theme from the parent.
        RequestedTheme = this.ActualTheme,
    }
};

childWindow.AppWindow.ResizeClient(new SizeInt32(500, 500));
childWindow.Activate();";

        private const string AppWindowTitleBarColorsCSharp =
@"using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.UI;

public sealed partial class AppWindowTitleBarWindow : Window
{
    public AppWindowTitleBarWindow()
    {
        InitializeComponent();

        AppWindow.TitleBar.BackgroundColor = ColorHelper.FromArgb($(BackgroundColor));
        AppWindow.TitleBar.ForegroundColor = ColorHelper.FromArgb($(ForegroundColor));
        AppWindow.TitleBar.ButtonBackgroundColor = ColorHelper.FromArgb($(ButtonBackgroundColor));
        AppWindow.TitleBar.ButtonForegroundColor = ColorHelper.FromArgb($(ButtonForegroundColor));
        AppWindow.TitleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb($(ButtonHoverBackgroundColor));
        AppWindow.TitleBar.ButtonHoverForegroundColor = ColorHelper.FromArgb($(ButtonHoverForegroundColor));
        AppWindow.TitleBar.InactiveBackgroundColor = ColorHelper.FromArgb($(InactiveBackgroundColor));
        AppWindow.TitleBar.InactiveForegroundColor = ColorHelper.FromArgb($(InactiveForegroundColor));
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb($(ButtonInactiveBackgroundColor));
        AppWindow.TitleBar.ButtonInactiveForegroundColor = ColorHelper.FromArgb($(ButtonInactiveForegroundColor));
        AppWindow.TitleBar.ButtonPressedBackgroundColor = ColorHelper.FromArgb($(ButtonPressedBackgroundColor));
        AppWindow.TitleBar.ButtonPressedForegroundColor = ColorHelper.FromArgb($(ButtonPressedForegroundColor));
    }
}";

        private const string AppWindowTitleBarExtendCSharp =
@"using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

public sealed partial class AppWindowTitleBarExtendWindow : Window
{
    public AppWindowTitleBarExtendWindow()
    {
        InitializeComponent();
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = $(ExtendsContentIntoTitleBar);
        if (AppWindow.TitleBar.ExtendsContentIntoTitleBar)
        {
            AppWindow.TitleBar.HeightOption = TitleBarHeightOption.$(TitleBarHeightOption);
        }
    }
}";

        private const string AppWindowTitleBarThemeCSharp =
@"using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

public sealed partial class AppWindowTitleBarThemeHeightWindow : Window
{
    public AppWindowTitleBarThemeHeightWindow()
    {
        InitializeComponent();
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.$(PreferredTheme);
    }
}";

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

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AppWindowTitleBar":
                    return CreateAppWindowTitleBarExamples();
                case "CreateMultipleWindows":
                    return new[]
                    {
                        new GalleryExample(
                            "Create single threaded Multiple Top level Windows(MTW).",
                            CreateMultipleWindowsExampleContent(assignRootAutomationId: true),
                            null,
                            CreateMultipleWindowsCSharp)
                    };
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement CreateIntroContent(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AppWindowTitleBar":
                    return CreateAppWindowTitleBarIntroContent();
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
            return CreateAppWindowTitleBarColorExampleContent(assignRootAutomationId: true);
        }

        private static TextBlock CreateAppWindowTitleBarIntroContent()
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            textBlock.Inlines.Add(new Run("For the default title bar and basic scenarios, use the "));

            var hyperlink = new Hyperlink(new Run("TitleBar"));
            hyperlink.Click += OnTitleBarHyperlinkClick;
            textBlock.Inlines.Add(hyperlink);

            textBlock.Inlines.Add(new Run(" control."));
            return textBlock;
        }

        private static IReadOnlyList<GalleryExample> CreateAppWindowTitleBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "AppWindowTitleBar color customization",
                    CreateAppWindowTitleBarColorExampleContent(assignRootAutomationId: true),
                    null,
                    AppWindowTitleBarColorsCSharp),
                new GalleryExample(
                    "Extending content into the AppWindowTitleBar area",
                    CreateAppWindowTitleBarExtendExampleContent(),
                    null,
                    AppWindowTitleBarExtendCSharp),
                new GalleryExample(
                    "AppWindowTitleBar preferred theme and height options",
                    CreateAppWindowTitleBarThemeExampleContent(),
                    null,
                    AppWindowTitleBarThemeCSharp)
            };
        }

        private static GallerySamplePanel CreateAppWindowTitleBarColorExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("AppWindowTitleBar"));
            }

            var background = CreateTitleBarColorSelector("Background", "BackgroundColor", "#FFF2F6FA");
            var foreground = CreateTitleBarColorSelector("Foreground", "ForegroundColor", "#FF1E2933");
            var buttonBackground = CreateTitleBarColorSelector("ButtonBackground", "ButtonBackgroundColor", "#FF3B82F6");
            var buttonForeground = CreateTitleBarColorSelector("ButtonForeground", "ButtonForegroundColor", "#FFFFFFFF");
            var buttonHoverBackground = CreateTitleBarColorSelector("ButtonHoverBackground", "ButtonHoverBackgroundColor", "#FF2563EB");
            var buttonHoverForeground = CreateTitleBarColorSelector("ButtonHoverForeground", "ButtonHoverForegroundColor", "#FFFFFFFF");
            var inactiveBackground = CreateTitleBarColorSelector("InactiveBackground", "InactiveBackgroundColor", "#FFE5EAF0");
            var inactiveForeground = CreateTitleBarColorSelector("InactiveForeground", "InactiveForegroundColor", "#FF6B7280");
            var buttonInactiveBackground = CreateTitleBarColorSelector("ButtonInactiveBackground", "ButtonInactiveBackgroundColor", "#FFCBD5E1");
            var buttonInactiveForeground = CreateTitleBarColorSelector("ButtonInactiveForeground", "ButtonInactiveForegroundColor", "#FF475569");
            var buttonPressedBackground = CreateTitleBarColorSelector("ButtonPressedBackground", "ButtonPressedBackgroundColor", "#FF1D4ED8");
            var buttonPressedForeground = CreateTitleBarColorSelector("ButtonPressedForeground", "ButtonPressedForegroundColor", "#FFFFFFFF");

            Window sampleWindow = null;
            var showWindowButton = new Button
            {
                Name = "ShowWindowButton",
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(showWindowButton, "Show window");
            GalleryAutomation.WithAutomationId(showWindowButton, GalleryAutomation.SampleElementId("AppWindowTitleBar", "ShowWindowButton"));
            showWindowButton.Click += delegate
            {
                showWindowButton.IsEnabled = false;
                sampleWindow = CreateModernWindow((FrameworkElement)showWindowButton, "AppWindowTitleBarWindow", 620, 380);
                ApplyAppWindowTitleBarColorSettings(
                    sampleWindow,
                    background,
                    foreground,
                    buttonBackground,
                    buttonForeground,
                    buttonHoverBackground,
                    buttonHoverForeground,
                    inactiveBackground,
                    inactiveForeground,
                    buttonInactiveBackground,
                    buttonInactiveForeground,
                    buttonPressedBackground,
                    buttonPressedForeground);
                sampleWindow.Content = CreateWindowBody(
                    "AppWindowTitleBar color customization",
                    "This WPF window maps WinUI AppWindowTitleBar colors to ModernWpf title bar attached properties.");
                sampleWindow.Closed += delegate
                {
                    showWindowButton.IsEnabled = true;
                    sampleWindow = null;
                };
                sampleWindow.Show();
            };

            root.Children.Add(showWindowButton);

            var options = new Grid
            {
                Margin = new Thickness(0, 16, 0, 0)
            };
            options.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            options.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            options.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var normalStates = new StackPanel();
            AddTitleBarColorOption(normalStates, "BackgroundColor", background);
            AddTitleBarColorOption(normalStates, "ForegroundColor", foreground);
            AddTitleBarColorOption(normalStates, "ButtonBackgroundColor", buttonBackground);
            AddTitleBarColorOption(normalStates, "ButtonForegroundColor", buttonForeground);
            AddTitleBarColorOption(normalStates, "ButtonHoverBackgroundColor", buttonHoverBackground);
            AddTitleBarColorOption(normalStates, "ButtonHoverForegroundColor", buttonHoverForeground);
            Grid.SetColumn(normalStates, 0);

            var separator = new Border
            {
                Width = 1,
                Margin = new Thickness(16, 0, 16, 0)
            };
            separator.SetResourceReference(Border.BackgroundProperty, "DividerStrokeColorDefaultBrush");
            Grid.SetColumn(separator, 1);

            var inactiveStates = new StackPanel();
            AddTitleBarColorOption(inactiveStates, "InactiveBackgroundColor", inactiveBackground);
            AddTitleBarColorOption(inactiveStates, "InactiveForegroundColor", inactiveForeground);
            AddTitleBarColorOption(inactiveStates, "ButtonInactiveBackgroundColor", buttonInactiveBackground);
            AddTitleBarColorOption(inactiveStates, "ButtonInactiveForegroundColor", buttonInactiveForeground);
            AddTitleBarColorOption(inactiveStates, "ButtonPressedBackgroundColor", buttonPressedBackground);
            AddTitleBarColorOption(inactiveStates, "ButtonPressedForegroundColor", buttonPressedForeground);
            Grid.SetColumn(inactiveStates, 2);

            options.Children.Add(normalStates);
            options.Children.Add(separator);
            options.Children.Add(inactiveStates);
            root.Children.Add(options);

            return root;
        }

        private static GallerySamplePanel CreateAppWindowTitleBarExtendExampleContent()
        {
            var root = new GallerySamplePanel();
            Window extendWindow = null;

            var showExtendButton = new Button
            {
                Name = "ShowExtendButton",
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(showExtendButton, "Show window");

            var extendContentCheckBox = new CheckBox
            {
                Name = "ExtendContentCheckBox",
                Margin = new Thickness(0, 0, 0, 12),
                Content = "Extend content into title bar",
                IsChecked = true
            };
            var heightComboBox = new ComboBox
            {
                Name = "HeightComboBox",
                Width = 200,
                ItemsSource = new[] { "Standard", "Tall" },
                SelectedIndex = 0,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(heightComboBox, "TitleBarHeightOption");

            showExtendButton.Click += delegate
            {
                showExtendButton.IsEnabled = false;
                extendWindow = CreateModernWindow((FrameworkElement)showExtendButton, "AppWindowTitleBarExtendWindow", 620, 380);
                Mux.TitleBar.SetExtendViewIntoTitleBar(extendWindow, extendContentCheckBox.IsChecked == true);
                extendWindow.Content = CreateWindowBody(
                    "Extending content into the AppWindowTitleBar area",
                    "ModernWpf maps this to TitleBar.ExtendViewIntoTitleBar; the selected height option is represented in the sample source.");
                extendWindow.Closed += delegate
                {
                    showExtendButton.IsEnabled = true;
                    extendWindow = null;
                };
                extendWindow.Show();
            };

            extendContentCheckBox.Checked += delegate
            {
                if (extendWindow != null)
                {
                    Mux.TitleBar.SetExtendViewIntoTitleBar(extendWindow, true);
                }
            };
            extendContentCheckBox.Unchecked += delegate
            {
                if (extendWindow != null)
                {
                    Mux.TitleBar.SetExtendViewIntoTitleBar(extendWindow, false);
                }
            };

            var options = new StackPanel
            {
                Margin = new Thickness(0, 16, 0, 0)
            };
            options.Children.Add(extendContentCheckBox);
            options.Children.Add(heightComboBox);

            root.Children.Add(showExtendButton);
            root.Children.Add(options);
            return root;
        }

        private static GallerySamplePanel CreateAppWindowTitleBarThemeExampleContent()
        {
            var root = new GallerySamplePanel();
            Window themeWindow = null;

            var showThemeHeightButton = new Button
            {
                Name = "ShowThemeHeightButton",
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(showThemeHeightButton, "Show window");

            var themeComboBox = new ComboBox
            {
                Name = "ThemeComboBox",
                Width = 200,
                ItemsSource = new[] { "UseDefaultAppMode", "Light", "Dark" },
                SelectedIndex = 1,
                Margin = new Thickness(0, 16, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(themeComboBox, "TitleBarTheme");

            showThemeHeightButton.Click += delegate
            {
                showThemeHeightButton.IsEnabled = false;
                themeWindow = CreateModernWindow((FrameworkElement)showThemeHeightButton, "AppWindowTitleBarThemeHeightWindow", 620, 380);
                ThemeManager.SetRequestedTheme(themeWindow, GetElementTheme(themeComboBox.SelectedItem as string));
                themeWindow.Content = CreateWindowBody(
                    "AppWindowTitleBar preferred theme",
                    "ModernWpf maps the title bar theme selection to the WPF window requested theme.");
                themeWindow.Closed += delegate
                {
                    showThemeHeightButton.IsEnabled = true;
                    themeWindow = null;
                };
                themeWindow.Show();
            };
            themeComboBox.SelectionChanged += delegate
            {
                if (themeWindow != null)
                {
                    ThemeManager.SetRequestedTheme(themeWindow, GetElementTheme(themeComboBox.SelectedItem as string));
                }
            };

            root.Children.Add(showThemeHeightButton);
            root.Children.Add(themeComboBox);
            return root;
        }

        private static UIElement CreateMultipleWindowsSample()
        {
            return CreateMultipleWindowsExampleContent(assignRootAutomationId: true);
        }

        private static GallerySamplePanel CreateMultipleWindowsExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("CreateMultipleWindows"));
            }

            var button = new Button
            {
                Name = "Control1",
                Content = "Create new Window",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, "Create new Window");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("CreateMultipleWindows", "Control1"));
            button.Click += delegate
            {
                var childWindow = CreateModernWindow((FrameworkElement)button, "New child window!", 500, 500);
                childWindow.Content = new Page
                {
                    Content = new TextBlock
                    {
                        Text = "New child window!",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                childWindow.Show();
            };

            root.Children.Add(button);
            return root;
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

        private static Button CreateTitleBarColorSelector(string name, string automationName, string color)
        {
            var swatch = new Border
            {
                Width = 30,
                Height = 18,
                Background = CreateBrush(color),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2)
            };
            swatch.SetResourceReference(Border.BorderBrushProperty, "ControlStrokeColorDefaultBrush");

            var button = new Button
            {
                Name = name,
                Width = 48,
                Height = 32,
                Padding = new Thickness(6),
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = swatch,
                Tag = color,
                ToolTip = automationName
            };
            AutomationProperties.SetName(button, automationName);
            return button;
        }

        private static void AddTitleBarColorOption(StackPanel stackPanel, string label, Button selector)
        {
            stackPanel.Children.Add(new TextBlock
            {
                Text = label,
                Margin = stackPanel.Children.Count == 0 ? new Thickness(0) : new Thickness(0, 8, 0, 0)
            });
            stackPanel.Children.Add(selector);
        }

        private static void ApplyAppWindowTitleBarColorSettings(
            Window window,
            Button background,
            Button foreground,
            Button buttonBackground,
            Button buttonForeground,
            Button buttonHoverBackground,
            Button buttonHoverForeground,
            Button inactiveBackground,
            Button inactiveForeground,
            Button buttonInactiveBackground,
            Button buttonInactiveForeground,
            Button buttonPressedBackground,
            Button buttonPressedForeground)
        {
            Mux.TitleBar.SetBackground(window, GetTitleBarColorBrush(background));
            Mux.TitleBar.SetForeground(window, GetTitleBarColorBrush(foreground));
            Mux.TitleBar.SetInactiveBackground(window, GetTitleBarColorBrush(inactiveBackground));
            Mux.TitleBar.SetInactiveForeground(window, GetTitleBarColorBrush(inactiveForeground));
            Mux.TitleBar.SetButtonStyle(
                window,
                CreateTitleBarButtonStyle(
                    GetTitleBarColorBrush(buttonBackground),
                    GetTitleBarColorBrush(buttonForeground),
                    GetTitleBarColorBrush(buttonHoverBackground),
                    GetTitleBarColorBrush(buttonHoverForeground),
                    GetTitleBarColorBrush(buttonInactiveBackground),
                    GetTitleBarColorBrush(buttonInactiveForeground),
                    GetTitleBarColorBrush(buttonPressedBackground),
                    GetTitleBarColorBrush(buttonPressedForeground)));
        }

        private static Style CreateTitleBarButtonStyle(
            Brush background,
            Brush foreground,
            Brush hoverBackground,
            Brush hoverForeground,
            Brush inactiveBackground,
            Brush inactiveForeground,
            Brush pressedBackground,
            Brush pressedForeground)
        {
            var style = new Style(typeof(TitleBarButton));
            style.Setters.Add(new Setter(Control.BackgroundProperty, background));
            style.Setters.Add(new Setter(Control.ForegroundProperty, foreground));
            style.Setters.Add(new Setter(TitleBarButton.HoverBackgroundProperty, hoverBackground));
            style.Setters.Add(new Setter(TitleBarButton.HoverForegroundProperty, hoverForeground));
            style.Setters.Add(new Setter(TitleBarButton.InactiveBackgroundProperty, inactiveBackground));
            style.Setters.Add(new Setter(TitleBarButton.InactiveForegroundProperty, inactiveForeground));
            style.Setters.Add(new Setter(TitleBarButton.PressedBackgroundProperty, pressedBackground));
            style.Setters.Add(new Setter(TitleBarButton.PressedForegroundProperty, pressedForeground));
            return style;
        }

        private static Brush GetTitleBarColorBrush(Button selector)
        {
            return CreateBrush((string)selector.Tag);
        }

        private static ElementTheme GetElementTheme(string titleBarTheme)
        {
            switch (titleBarTheme)
            {
                case "Light":
                    return ElementTheme.Light;
                case "Dark":
                    return ElementTheme.Dark;
                default:
                    return ElementTheme.Default;
            }
        }

        private static void OnTitleBarHyperlinkClick(object sender, RoutedEventArgs e)
        {
            var page = FindLogicalAncestor<ItemPage>(sender as DependencyObject);
            var target = GalleryCatalog.FindItem("TitleBar");
            if (page != null && target != null)
            {
                page.ItemRequested?.Invoke(target);
                e.Handled = true;
            }
        }

        private static T FindLogicalAncestor<T>(DependencyObject current)
            where T : class
        {
            while (current != null)
            {
                var match = current as T;
                if (match != null)
                {
                    return match;
                }

                current = LogicalTreeHelper.GetParent(current);
            }

            return null;
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

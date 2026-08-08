using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class WindowingSampleFactory
    {
        private const string TitleBarConfigurationXaml =
@"<TitleBar
    Title=""$(Title)""
    Subtitle=""$(Subtitle)""
    IsBackButtonVisible=""$(BackButtonVisibility)""
    IsPaneToggleButtonVisible=""$(PaneToggleVisibility)"">
    <TitleBar.Resources>
        <HorizontalAlignment x:Key=""TitleBarContentHorizontalAlignment"">Stretch</HorizontalAlignment>
    </TitleBar.Resources>
    <TitleBar.IconSource>
        <SymbolIconSource Symbol=""Library"" />
    </TitleBar.IconSource>
    <TitleBar.Content>
        <AutoSuggestBox
            MaxWidth=""580""
            HorizontalAlignment=""Stretch""
            VerticalAlignment=""Center""
            PlaceholderText=""Search...""
            QueryIcon=""Find"" />
    </TitleBar.Content>
    <TitleBar.RightHeader>
        <PersonPicture
            Width=""30""
            Height=""30""
            Initials=""JD"" />
    </TitleBar.RightHeader>
</TitleBar>";

        private const string TitleBarDragRegionsXaml =
@"<!-- ModernWPF TitleBar walks TitleBar.Content, excludes interactive
     controls from the drag region, and keeps non-interactive visuals
     and empty space draggable.

     Use TitleBar.IsDragRegion to override the framework decision:
       True   -> always draggable
       False  -> always clickable
       unset  -> framework decides (default) -->
<TitleBar x:Name=""titleBar"" Title=""Drag regions"">
    <TitleBar.Resources>
        <HorizontalAlignment x:Key=""TitleBarContentHorizontalAlignment"">Stretch</HorizontalAlignment>
    </TitleBar.Resources>
    <TitleBar.Content>
        <Grid HorizontalAlignment=""Stretch"">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width=""*"" />
                <ColumnDefinition Width=""Auto"" />
            </Grid.ColumnDefinitions>
            <AutoSuggestBox
                MaxWidth=""580""
                HorizontalAlignment=""Stretch""
                VerticalAlignment=""Center""
                PlaceholderText=""Search...""
                QueryIcon=""Find"" />
            <Button
                x:Name=""StatusBadge""
                Grid.Column=""1""
                Margin=""8,0,0,0""
                VerticalAlignment=""Center""
                Click=""StatusBadge_Click""
                Content=""Status""
                Style=""{StaticResource AccentButtonStyle}"" />
        </Grid>
    </TitleBar.Content>
</TitleBar>";

        private const string TitleBarDragRegionsCSharp =
@"// Set TitleBar.IsDragRegion at runtime.
TitleBar.SetIsDragRegion(StatusBadge, true);   // always draggable
TitleBar.SetIsDragRegion(StatusBadge, false);  // always clickable
StatusBadge.ClearValue(TitleBar.IsDragRegionProperty); // back to default

// After adding or removing elements in TitleBar.Content dynamically,
// ask the framework to recompute drag regions.
titleBar.RecomputeDragRegions();";

        private const string TitleBarEndToEndXaml =
@"<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto"" />
        <!--  TitleBar  -->
        <RowDefinition Height=""*"" />
        <!--  NavigationView  -->
    </Grid.RowDefinitions>

    <TitleBar
        x:Name=""titleBar""
        BackRequested=""TitleBar_BackRequested""
        IsBackButtonVisible=""{Binding ElementName=navFrame, Path=CanGoBack}""
        IsPaneToggleButtonVisible=""True""
        PaneToggleRequested=""TitleBar_PaneToggleRequested"" />

    <NavigationView
        x:Name=""navView""
        Grid.Row=""1""
        IsBackButtonVisible=""Collapsed""
        IsPaneToggleButtonVisible=""False"">
        <NavigationView.MenuItems... />
        <Frame x:Name=""navFrame"" />
    </NavigationView>
</Grid>";

        private const string TitleBarEndToEndCSharp =
@"// Extend WPF content into ModernWPF's window chrome. TitleBar performs
// live WPF hit testing so ordinary controls stay interactive while empty
// title-bar space and TitleBar.IsDragRegion=True elements drag the window.
WindowTitleBar.SetExtendsContentIntoTitleBar(this, true);
WindowTitleBar.SetIsIconVisible(this, false);";

        private const string SystemBackdropXaml =
@"<Window
    xmlns:ui=""http://schemas.modernwpf.com/2019""
    ui:WindowBackdrop.Kind=""Mica""
    ui:WindowBackdrop.FallbackBrush=""{DynamicResource WindowBackground}"">
    <!-- Window content remains ordinary WPF. -->
</Window>";

        private const string SystemBackdropCSharp =
@"// Windows 11 22H2 or newer uses the native DWM material.
// High Contrast, disabled composition, older Windows, and DWM failures
// automatically use FallbackBrush and report EffectiveKind=None.
WindowBackdrop.SetFallbackBrush(window, fallbackBrush);
WindowBackdrop.SetKind(window, WindowBackdropKind.Mica);

WindowBackdropKind effective = WindowBackdrop.GetEffectiveKind(window);";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "TitleBar":
                    return CreateTitleBarSample();
                case "SystemBackdrop":
                    return CreateSystemBackdropSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets = null)
        {
            switch (uniqueId)
            {
                case "TitleBar":
                    return CreateTitleBarExamples();
                case "SystemBackdrop":
                    return CreateSystemBackdropExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement CreateIntroContent(string uniqueId)
        {
            switch (uniqueId)
            {
                case "TitleBar":
                    return CreateTitleBarIntroContent();
                case "SystemBackdrop":
                    return CreateSystemBackdropIntroContent();
                default:
                    return null;
            }
        }

        private static UIElement CreateTitleBarSample()
        {
            var content = CreateTitleBarConfigurationExampleContent(
                assignRootAutomationId: true,
                out var optionsContent);
            var layout = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            layout.Children.Add(content);
            layout.Children.Add(new Border
            {
                Margin = new Thickness(24, 0, 0, 0),
                Child = optionsContent
            });
            return layout;
        }

        private static TextBlock CreateTitleBarIntroContent()
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Text = "Use the examples below to configure a ModernWPF TitleBar or integrate one with NavigationView."
            };
            return textBlock;
        }

        private static UIElement CreateSystemBackdropSample()
        {
            return CreateSystemBackdropExampleContent(assignRootAutomationId: true);
        }

        private static TextBlock CreateSystemBackdropIntroContent()
        {
            return new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Text = "Use WindowBackdrop to request native Mica or Desktop Acrylic for a WPF Window. ModernWPF automatically uses a normal theme brush when the material is unavailable or inappropriate."
            };
        }

        private static IReadOnlyList<GalleryExample> CreateSystemBackdropExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Mica and Desktop Acrylic",
                    CreateSystemBackdropExampleContent(assignRootAutomationId: true),
                    SystemBackdropXaml,
                    SystemBackdropCSharp)
                    .WithContentAlignment(HorizontalAlignment.Stretch, VerticalAlignment.Center)
            };
        }

        private static GallerySamplePanel CreateSystemBackdropExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(
                    root,
                    GalleryAutomation.SampleRootId("SystemBackdrop"));
            }

            var stack = new StackPanel
            {
                MaxWidth = 640,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                Text = "Open a real WPF window to see each native material. On Windows 11 22H2 or newer, DWM supplies the backdrop; High Contrast, older systems, disabled composition, and native failures use the WindowBackground fallback.",
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });

            var status = new TextBlock
            {
                Name = "SystemBackdropStatus",
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "No material window is open.",
                TextWrapping = TextWrapping.Wrap
            };
            GalleryAutomation.WithAutomationId(
                status,
                GalleryAutomation.SampleElementId("SystemBackdrop", "Status"));
            var sampleState = new SystemBackdropSampleState(status);

            var buttons = new StackPanel
            {
                Margin = new Thickness(0, 16, 0, 0),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            buttons.Children.Add(CreateSystemBackdropButton(
                "Open Mica window",
                "MicaButton",
                Mux.WindowBackdropKind.Mica,
                sampleState));
            buttons.Children.Add(CreateSystemBackdropButton(
                "Open Desktop Acrylic window",
                "DesktopAcrylicButton",
                Mux.WindowBackdropKind.DesktopAcrylic,
                sampleState,
                new Thickness(8, 0, 0, 0)));

            stack.Children.Add(buttons);
            stack.Children.Add(status);
            root.Children.Add(stack);
            return root;
        }

        private static Button CreateSystemBackdropButton(
            string content,
            string automationIdSuffix,
            Mux.WindowBackdropKind kind,
            SystemBackdropSampleState sampleState,
            Thickness margin = default)
        {
            var button = new Button
            {
                Content = content,
                Margin = margin
            };
            GalleryAutomation.WithAutomationId(
                button,
                GalleryAutomation.SampleElementId("SystemBackdrop", automationIdSuffix));
            button.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
            button.Click += delegate
            {
                sampleState.ActiveWindow?.Close();

                var window = CreateModernWindow(button, content, 720, 480);
                sampleState.ActiveWindow = window;
                GalleryAutomation.WithAutomationId(
                    window,
                    GalleryAutomation.SampleElementId("SystemBackdrop", kind + "Window"));
                window.SetResourceReference(
                    Mux.WindowBackdrop.FallbackBrushProperty,
                    "WindowBackground");
                Mux.WindowBackdrop.SetKind(window, kind);
                window.Content = CreateSystemBackdropWindowBody(kind);
                var effectiveKindDescriptor = DependencyPropertyDescriptor.FromProperty(
                    Mux.WindowBackdrop.EffectiveKindProperty,
                    typeof(Window));
                EventHandler effectiveKindChanged = delegate
                {
                    UpdateSystemBackdropStatus(sampleState, window, kind);
                };
                effectiveKindDescriptor?.AddValueChanged(window, effectiveKindChanged);
                window.ContentRendered += delegate
                {
                    UpdateSystemBackdropStatus(sampleState, window, kind);
                };
                window.Closed += delegate
                {
                    effectiveKindDescriptor?.RemoveValueChanged(window, effectiveKindChanged);
                    if (ReferenceEquals(sampleState.ActiveWindow, window))
                    {
                        sampleState.ActiveWindow = null;
                        sampleState.Status.Text = "No material window is open.";
                    }
                };
                window.Show();
            };
            return button;
        }

        private static void UpdateSystemBackdropStatus(
            SystemBackdropSampleState sampleState,
            Window window,
            Mux.WindowBackdropKind requestedKind)
        {
            if (ReferenceEquals(sampleState.ActiveWindow, window))
            {
                sampleState.Status.Text = string.Format(
                    "Requested {0}; effective material: {1}.",
                    requestedKind,
                    Mux.WindowBackdrop.GetEffectiveKind(window));
            }
        }

        private sealed class SystemBackdropSampleState
        {
            internal SystemBackdropSampleState(TextBlock status)
            {
                Status = status;
            }

            internal Window ActiveWindow { get; set; }

            internal TextBlock Status { get; }
        }

        private static FrameworkElement CreateSystemBackdropWindowBody(Mux.WindowBackdropKind kind)
        {
            var root = new Grid
            {
                Margin = new Thickness(40)
            };
            var card = new Border
            {
                MaxWidth = 520,
                Padding = new Thickness(28),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CornerRadius = new CornerRadius(8)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
            var content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = kind.ToString(),
                FontSize = 24,
                FontWeight = FontWeights.SemiBold
            });
            content.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 8, 0, 0),
                Text = "This surface is ordinary WPF content. The material behind it is supplied by DWM when supported, with WindowBackground as the safe fallback.",
                TextWrapping = TextWrapping.Wrap
            });
            card.Child = content;
            root.Children.Add(card);
            return root;
        }

        private static IReadOnlyList<GalleryExample> CreateTitleBarExamples()
        {
            var configurationContent = CreateTitleBarConfigurationExampleContent(
                assignRootAutomationId: true,
                out var configurationOptions);
            return new[]
            {
                new GalleryExample(
                    "TitleBar configuration",
                    configurationContent,
                    TitleBarConfigurationXaml,
                    null,
                    configurationOptions)
                    .WithContentAlignment(HorizontalAlignment.Stretch, VerticalAlignment.Center),
                new GalleryExample(
                    "TitleBar drag regions",
                    CreateTitleBarDragRegionsExampleContent(),
                    TitleBarDragRegionsXaml,
                    TitleBarDragRegionsCSharp)
                    .WithContentAlignment(HorizontalAlignment.Stretch, VerticalAlignment.Center),
                new GalleryExample(
                    "End to end TitleBar sample",
                    CreateTitleBarEndToEndExampleContent(),
                    TitleBarEndToEndXaml,
                    TitleBarEndToEndCSharp)
                    .WithContentAlignment(HorizontalAlignment.Stretch, VerticalAlignment.Center)
            };
        }

        private static GallerySamplePanel CreateTitleBarConfigurationExampleContent(
            bool assignRootAutomationId,
            out UIElement optionsContent)
        {
            var root = new GallerySamplePanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("TitleBar"));
            }

            var searchBox = new Mux.AutoSuggestBox
            {
                Name = "TitleBarSearchBox",
                MaxWidth = 580,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                PlaceholderText = "Search...",
                QueryIcon = new Mux.SymbolIcon(Mux.Symbol.Find)
            };
            GalleryAutomation.WithAutomationId(searchBox, GalleryAutomation.SampleElementId("TitleBar", "SearchBox"));

            var personPicture = new Mux.PersonPicture
            {
                Name = "TitleBarRightHeader",
                Width = 30,
                Height = 30,
                Initials = "JD",
                VerticalAlignment = VerticalAlignment.Center
            };

            var titleBarControl = new Mux.TitleBar
            {
                Name = "TitleBarControl",
                Width = 470,
                HorizontalAlignment = HorizontalAlignment.Left,
                Title = GalleryBranding.DisplayName,
                Subtitle = "Preview",
                IconSource = new Mux.SymbolIconSource { Symbol = Mux.Symbol.Library },
                Content = searchBox,
                RightHeader = personPicture
            };
            titleBarControl.Resources["TitleBarContentHorizontalAlignment"] = HorizontalAlignment.Stretch;
            AutomationProperties.SetName(titleBarControl, "TitleBarControl");
            GalleryAutomation.WithAutomationId(titleBarControl, GalleryAutomation.SampleElementId("TitleBar", "TitleBarControl"));

            var titleBox = new TextBox
            {
                Name = "TitleBox",
                Text = GalleryBranding.DisplayName
            };
            ControlHelper.SetHeader(titleBox, "Title");

            var subtitleBox = new TextBox
            {
                Name = "SubtitleBox",
                Text = "Preview"
            };
            ControlHelper.SetHeader(subtitleBox, "Subtitle");

            var backButtonToggle = new Mux.ToggleSwitch
            {
                Name = "BackButtonToggle",
                Header = "IsBackButtonVisible",
                IsOn = false,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var paneToggle = new Mux.ToggleSwitch
            {
                Name = "PaneToggle",
                Header = "IsPaneToggleButtonVisible",
                IsOn = false,
                Margin = new Thickness(0, 12, 0, 0)
            };

            Action updatePreview = delegate
            {
                titleBarControl.Title = titleBox.Text;
                titleBarControl.Subtitle = subtitleBox.Text;
                titleBarControl.IsBackButtonVisible = backButtonToggle.IsOn;
                titleBarControl.IsPaneToggleButtonVisible = paneToggle.IsOn;
            };
            titleBox.TextChanged += delegate { updatePreview(); };
            subtitleBox.TextChanged += delegate { updatePreview(); };
            backButtonToggle.Toggled += delegate { updatePreview(); };
            paneToggle.Toggled += delegate { updatePreview(); };

            var options = new StackPanel
            {
                Width = 240,
                MinHeight = 280,
                Orientation = Orientation.Vertical
            };
            // WinUI TextBox reserves an 8-DIP gap between its built-in Header
            // presenter and input chrome. WPF uses an explicit header element
            // for the port, so preserve that same vertical metric here.
            options.Children.Add(CreateTitleBarOptionHeader("TitleHeader", "Title", new Thickness(0, 0, 0, 8)));
            options.Children.Add(titleBox);
            options.Children.Add(CreateTitleBarOptionHeader("SubtitleHeader", "Subtitle", new Thickness(0, 12, 0, 8)));
            options.Children.Add(subtitleBox);
            options.Children.Add(backButtonToggle);
            options.Children.Add(paneToggle);

            root.Children.Add(titleBarControl);
            optionsContent = options;
            updatePreview();
            return root;
        }

        private static TextBlock CreateTitleBarOptionHeader(string name, string text, Thickness margin)
        {
            var header = new TextBlock
            {
                Name = name,
                Text = text,
                Margin = margin
            };
            header.SetResourceReference(TextBlock.FontSizeProperty, "BodyTextBlockFontSize");
            header.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
            return header;
        }

        private static GallerySamplePanel CreateTitleBarDragRegionsExampleContent()
        {
            var root = new GallerySamplePanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            var stack = new StackPanel
            {
                MaxWidth = 560,
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Drag regions can only be observed on a real window. Click the button below to open a sample window where you can toggle TitleBar.IsDragRegion on a status badge and call RecomputeDragRegions() after dynamic content changes.",
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });

            var showWindowButton = new Button
            {
                Name = "ShowTitleBarDragRegionsWindowButton",
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };
            GalleryAutomation.WithAutomationId(
                showWindowButton,
                GalleryAutomation.SampleElementId("TitleBar", "DragRegionsShowWindowButton"));
            showWindowButton.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
            showWindowButton.Click += delegate
            {
                var window = CreateModernWindow(
                    showWindowButton,
                    "TitleBar drag regions sample",
                    900,
                    640);
                GalleryAutomation.WithAutomationId(
                    window,
                    GalleryAutomation.SampleElementId("TitleBar", "DragRegionsWindow"));
                Mux.WindowTitleBar.SetExtendsContentIntoTitleBar(window, true);
                Mux.WindowTitleBar.SetIsIconVisible(window, false);
                window.Content = CreateTitleBarDragRegionsWindowBody(window);
                window.Show();
            };
            stack.Children.Add(showWindowButton);
            root.Children.Add(stack);
            return root;
        }

        private static FrameworkElement CreateTitleBarDragRegionsWindowBody(Window window)
        {
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(48) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var searchBox = new Mux.AutoSuggestBox
            {
                Name = "DragRegionsSearchBox",
                MaxWidth = 580,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                PlaceholderText = "Search...",
                QueryIcon = new Mux.SymbolIcon(Mux.Symbol.Find),
                Margin = new Thickness(0, 0, 8, 0)
            };
            var rightHeaderPanel = new StackPanel
            {
                Name = "RightHeaderPanel",
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            var statusBadge = new Button
            {
                Name = "StatusBadge",
                Content = "Status",
                VerticalAlignment = VerticalAlignment.Center
            };
            statusBadge.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
            rightHeaderPanel.Children.Add(statusBadge);

            var titleBar = new Mux.TitleBar
            {
                Name = "DragRegionsTitleBar",
                Title = "Drag regions",
                Subtitle = "Try dragging the window",
                IconSource = new Mux.SymbolIconSource { Symbol = Mux.Symbol.Library },
                Content = searchBox,
                RightHeader = rightHeaderPanel,
                Margin = new Thickness(0, 0, 140, 0)
            };
            titleBar.Resources["TitleBarContentHorizontalAlignment"] = HorizontalAlignment.Stretch;
            root.Children.Add(titleBar);

            var body = new StackPanel
            {
                Name = "DragRegionsBody",
                MaxWidth = 640,
                Margin = new Thickness(32, 24, 32, 24)
            };
            body.Children.Add(new TextBlock
            {
                Text = "Custom drag regions",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            });
            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 16, 0, 0),
                Text = "Try dragging the window from different parts of the title bar. ModernWPF keeps interactive controls, such as the search box, clickable while non-interactive title-bar space remains draggable.",
                TextWrapping = TextWrapping.Wrap
            });
            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 16, 0, 0),
                Text = "Status badge: TitleBar.IsDragRegion",
                FontWeight = FontWeights.SemiBold
            });
            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 6, 0, 0),
                Text = "Pick a value for the badge in the title bar.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            });

            var dragRegionOptions = new Mux.RadioButtons
            {
                Name = "BadgeIsDragRegionRadios",
                SelectedIndex = 0,
                Margin = new Thickness(0, 8, 0, 0)
            };
            dragRegionOptions.Items.Add("Unset (framework decides — clickable, since Button is interactive)");
            dragRegionOptions.Items.Add("True (always draggable — overrides the framework default)");
            dragRegionOptions.Items.Add("False (always clickable)");
            body.Children.Add(dragRegionOptions);

            var statusText = new TextBlock
            {
                Name = "StatusText",
                Margin = new Thickness(0, 16, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            };
            statusBadge.Click += delegate { statusText.Text = "Status badge clicked"; };

            dragRegionOptions.SelectionChanged += delegate
            {
                switch (dragRegionOptions.SelectedIndex)
                {
                    case 1:
                        Mux.TitleBar.SetIsDragRegion(statusBadge, true);
                        statusText.Text = "Status badge is explicitly part of the drag region.";
                        break;
                    case 2:
                        Mux.TitleBar.SetIsDragRegion(statusBadge, false);
                        statusText.Text = "Status badge is explicitly interactive.";
                        break;
                    default:
                        statusBadge.ClearValue(Mux.TitleBar.IsDragRegionProperty);
                        statusText.Text = "Status badge uses the framework default and remains clickable.";
                        break;
                }

                titleBar.RecomputeDragRegions();
            };

            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 16, 0, 0),
                Text = "Dynamic content",
                FontWeight = FontWeights.SemiBold
            });
            body.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 6, 0, 0),
                Text = "When you add or remove elements in TitleBar.Content at runtime, call RecomputeDragRegions() to refresh.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            });

            var actionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 8, 0, 0)
            };
            Button extraButton = null;
            var toggleExtraButton = new Button
            {
                Name = "ToggleExtraTitleBarButton",
                Content = "Toggle extra title bar button"
            };
            toggleExtraButton.Click += delegate
            {
                if (extraButton == null)
                {
                    extraButton = new Button
                    {
                        Name = "ExtraTitleBarButton",
                        Content = "Extra",
                        Margin = new Thickness(0, 0, 8, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    rightHeaderPanel.Children.Insert(0, extraButton);
                    titleBar.RecomputeDragRegions();
                    statusText.Text = "Added a Button and recomputed the title-bar drag regions.";
                }
                else
                {
                    rightHeaderPanel.Children.Remove(extraButton);
                    extraButton = null;
                    titleBar.RecomputeDragRegions();
                    statusText.Text = "Removed the Button and recomputed the title-bar drag regions.";
                }
            };
            actionPanel.Children.Add(toggleExtraButton);

            var recomputeButton = new Button
            {
                Name = "RecomputeDragRegionsButton",
                Content = "RecomputeDragRegions()",
                Margin = new Thickness(8, 0, 0, 0)
            };
            recomputeButton.Click += delegate
            {
                titleBar.RecomputeDragRegions();
                statusText.Text = "Recomputed the live WPF drag/input tree.";
            };
            actionPanel.Children.Add(recomputeButton);
            body.Children.Add(actionPanel);
            body.Children.Add(statusText);

            var scrollViewer = new ScrollViewer
            {
                Content = body,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);
            return root;
        }

        private static GallerySamplePanel CreateTitleBarEndToEndExampleContent()
        {
            var root = new GallerySamplePanel();
            var stack = new StackPanel
            {
                MaxWidth = 560,
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Click the button below to see an end-to-end TitleBar in a new window, with properties bound to the NavigationView and navigation frame.",
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });

            var showWindowButton = new Button
            {
                Content = "Show window",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };
            GalleryAutomation.WithAutomationId(
                showWindowButton,
                GalleryAutomation.SampleElementId("TitleBar", "EndToEndShowWindowButton"));
            showWindowButton.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
            showWindowButton.Click += delegate
            {
                var window = CreateModernWindow((FrameworkElement)showWindowButton, "TitleBarWindow", 760, 520);
                GalleryAutomation.WithAutomationId(
                    window,
                    GalleryAutomation.SampleElementId("TitleBar", "EndToEndWindow"));
                Mux.WindowTitleBar.SetExtendsContentIntoTitleBar(window, true);
                Mux.WindowTitleBar.SetIsIconVisible(window, false);
                window.Content = CreateTitleBarWindowBody();
                window.Show();
            };
            stack.Children.Add(showWindowButton);
            root.Children.Add(stack);
            return root;
        }

        private static FrameworkElement CreateTitleBarWindowBody()
        {
            var frame = new Frame
            {
                Name = "navFrame",
                NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden,
                Content = new Border
                {
                    Padding = new Thickness(32),
                    Child = new TextBlock
                    {
                        Text = "Sample page content",
                        FontSize = 24,
                        FontWeight = FontWeights.SemiBold
                    }
                }
            };

            var navigationView = new Mux.NavigationView
            {
                Name = "navView",
                IsBackButtonVisible = Mux.NavigationViewBackButtonVisible.Collapsed,
                IsPaneToggleButtonVisible = false,
                IsTitleBarAutoPaddingEnabled = false,
                Content = frame
            };
            navigationView.MenuItems.Add(new Mux.NavigationViewItem
            {
                Content = "Home",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Home)
            });
            navigationView.MenuItems.Add(new Mux.NavigationViewItem
            {
                Content = "Documents",
                Icon = new Mux.SymbolIcon(Mux.Symbol.Document)
            });

            var titleBar = new Mux.TitleBar
            {
                Name = "EndToEndTitleBar",
                Title = "TitleBarWindow",
                IconSource = new Mux.SymbolIconSource { Symbol = Mux.Symbol.Library },
                IsPaneToggleButtonVisible = true,
                Margin = new Thickness(0, 0, 140, 0)
            };
            BindingOperations.SetBinding(
                titleBar,
                Mux.TitleBar.IsBackButtonVisibleProperty,
                new Binding(nameof(Frame.CanGoBack)) { Source = frame });
            BindingOperations.SetBinding(
                titleBar,
                Mux.TitleBar.IsBackButtonEnabledProperty,
                new Binding(nameof(Frame.CanGoBack)) { Source = frame });
            frame.Navigated += delegate
            {
                titleBar.GetBindingExpression(Mux.TitleBar.IsBackButtonVisibleProperty)?.UpdateTarget();
                titleBar.GetBindingExpression(Mux.TitleBar.IsBackButtonEnabledProperty)?.UpdateTarget();
            };
            titleBar.BackRequested += delegate
            {
                if (frame.CanGoBack)
                {
                    frame.GoBack();
                }
            };
            titleBar.PaneToggleRequested += delegate
            {
                navigationView.IsPaneOpen = !navigationView.IsPaneOpen;
            };

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.Children.Add(titleBar);
            Grid.SetRow(navigationView, 1);
            root.Children.Add(navigationView);
            return root;
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
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            window.SetResourceReference(Control.BackgroundProperty, "WindowBackground");
            var owner = Window.GetWindow(ownerElement);
            if (owner != null)
            {
                window.Owner = owner;
            }
            ThemeManager.SetIsThemeAware(window, true);
            WindowHelper.SetUseModernWindowStyle(window, true);
            Mux.WindowTitleBar.SetIsIconVisible(window, true);
            return window;
        }
    }
}

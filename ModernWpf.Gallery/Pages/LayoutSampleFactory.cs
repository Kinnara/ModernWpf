using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class LayoutSampleFactory
    {
        private const string SplitViewBasicXaml =
@"<SplitView x:Name=""splitView"" PaneBackground=""$(PaneBackground)""
           IsPaneOpen=""$(IsPaneOpen)"" OpenPaneLength=""$(OpenPaneLength)"" CompactPaneLength=""$(CompactPaneLength)"" DisplayMode=""$(DisplayMode)"">
    <SplitView.Pane>
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height=""Auto""/>
                <RowDefinition Height=""*""/>
                <RowDefinition Height=""Auto""/>
            </Grid.RowDefinitions>
            <TextBlock Text=""PANE CONTENT"" x:Name=""PaneHeader"" Margin=""60,12,0,0"" Style=""{StaticResource BaseTextBlockStyle}""/>
            <ListView x:Name=""NavLinksList"" Margin=""0,12,0,0"" SelectionMode=""Single"" Grid.Row=""1"" VerticalAlignment=""Stretch""
                    ItemClick=""NavLinksList_ItemClick"" IsItemClickEnabled=""True""
                    ItemsSource=""{x:Bind NavLinks}"" ItemTemplate=""{StaticResource NavLinkItemTemplate}""/>
        </Grid>
    </SplitView.Pane>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height=""Auto""/>
            <RowDefinition Height=""*""/>
        </Grid.RowDefinitions>
        <TextBlock Text=""SPLITVIEW CONTENT"" Margin=""12,12,0,0"" Style=""{StaticResource BaseTextBlockStyle}""/>
        <TextBlock x:Name=""content"" Grid.Row=""1"" Margin=""12,12,0,0"" Style=""{StaticResource BodyTextBlockStyle}"" />
    </Grid>
</SplitView>";

        private const string TwoPaneViewBasicXaml =
@"<ui:TwoPaneView Pane1Length=""2*""
                    Pane2Length=""*""
                    MinWideModeWidth=""480""
                    MinTallModeHeight=""260"">
    <ui:TwoPaneView.Pane1>
        <Border Padding=""24"">
            <TextBlock Text=""Pane 1"" />
        </Border>
    </ui:TwoPaneView.Pane1>
    <ui:TwoPaneView.Pane2>
        <Border Padding=""24"">
            <TextBlock Text=""Pane 2"" />
        </Border>
    </ui:TwoPaneView.Pane2>
</ui:TwoPaneView>";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "SplitView":
                    return CreateSplitViewSample();
                case "TwoPaneView":
                    return CreateTwoPaneViewSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "SplitView":
                    var exampleContent = CreateBasicSplitViewExampleContent(
                        assignRootAutomationId: true,
                        out var optionsContent);
                    return new[]
                    {
                        new GalleryExample(
                            "A basic SplitView.",
                            exampleContent,
                            SplitViewBasicXaml,
                            null,
                            optionsContent)
                    };
                case "TwoPaneView":
                    var twoPaneContent = CreateBasicTwoPaneViewExampleContent(
                        assignRootAutomationId: true,
                        out var twoPaneOptions);
                    return new[]
                    {
                        new GalleryExample(
                            "A TwoPaneView that adapts between single, wide, and tall layouts.",
                            twoPaneContent,
                            TwoPaneViewBasicXaml,
                            null,
                            twoPaneOptions)
                    };
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateTwoPaneViewSample()
        {
            var content = CreateBasicTwoPaneViewExampleContent(
                assignRootAutomationId: true,
                out var optionsContent);
            return CreateTwoPaneViewStandaloneLayout(content, optionsContent);
        }

        private static GallerySamplePanel CreateBasicTwoPaneViewExampleContent(
            bool assignRootAutomationId,
            out UIElement optionsContent)
        {
            var status = new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 8),
                FontWeight = FontWeights.SemiBold
            };
            GalleryAutomation.WithAutomationId(
                status,
                GalleryAutomation.SampleElementId("TwoPaneView", "Mode"));

            var view = new Mux.TwoPaneView
            {
                Width = 600,
                Height = 330,
                MinWideModeWidth = 480,
                MinTallModeHeight = 260,
                Pane1Length = new GridLength(2, GridUnitType.Star),
                Pane2Length = new GridLength(1, GridUnitType.Star),
                Pane1 = CreateTwoPaneContent(
                    "Pane 1",
                    "Primary content remains visible when the view collapses.",
                    "ControlFillColorSecondaryBrush",
                    "#E8E8E8"),
                Pane2 = CreateTwoPaneContent(
                    "Pane 2",
                    "Secondary content appears beside or below the primary pane.",
                    "SubtleFillColorSecondaryBrush",
                    "#F3F3F3")
            };
            GalleryAutomation.WithAutomationId(
                view,
                GalleryAutomation.SampleElementId("TwoPaneView", "View"));

            void UpdateStatus()
            {
                status.Text = "Current mode: " + view.Mode;
            }

            view.ModeChanged += delegate { UpdateStatus(); };
            view.Loaded += delegate { UpdateStatus(); };
            UpdateStatus();

            var content = new GallerySamplePanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    status,
                    view
                }
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(
                    content,
                    GalleryAutomation.SampleRootId("TwoPaneView"));
            }

            var wideConfiguration = CreateEnumComboBox(
                Mux.TwoPaneViewWideModeConfiguration.LeftRight,
                Enum.GetNames(typeof(Mux.TwoPaneViewWideModeConfiguration)));
            GalleryAutomation.WithAutomationId(
                wideConfiguration,
                GalleryAutomation.SampleElementId("TwoPaneView", "WideConfiguration"));
            wideConfiguration.SelectionChanged += delegate
            {
                if (wideConfiguration.SelectedItem is string value)
                {
                    view.WideModeConfiguration = (Mux.TwoPaneViewWideModeConfiguration)Enum.Parse(
                        typeof(Mux.TwoPaneViewWideModeConfiguration),
                        value);
                }
            };

            var tallConfiguration = CreateEnumComboBox(
                Mux.TwoPaneViewTallModeConfiguration.TopBottom,
                Enum.GetNames(typeof(Mux.TwoPaneViewTallModeConfiguration)));
            GalleryAutomation.WithAutomationId(
                tallConfiguration,
                GalleryAutomation.SampleElementId("TwoPaneView", "TallConfiguration"));
            tallConfiguration.SelectionChanged += delegate
            {
                if (tallConfiguration.SelectedItem is string value)
                {
                    view.TallModeConfiguration = (Mux.TwoPaneViewTallModeConfiguration)Enum.Parse(
                        typeof(Mux.TwoPaneViewTallModeConfiguration),
                        value);
                }
            };

            var priority = new Mux.ToggleSwitch
            {
                Header = "Single-pane priority",
                OffContent = "Pane 1",
                OnContent = "Pane 2"
            };
            GalleryAutomation.WithAutomationId(
                priority,
                GalleryAutomation.SampleElementId("TwoPaneView", "PanePriority"));
            priority.Toggled += delegate
            {
                view.PanePriority = priority.IsOn
                    ? Mux.TwoPaneViewPriority.Pane2
                    : Mux.TwoPaneViewPriority.Pane1;
            };

            var viewWidth = CreateSlider("View width", 300, 800, view.Width);
            GalleryAutomation.WithAutomationId(
                viewWidth,
                GalleryAutomation.SampleElementId("TwoPaneView", "Width"));
            viewWidth.ValueChanged += delegate { view.Width = viewWidth.Value; };

            var viewHeight = CreateSlider("View height", 180, 500, view.Height);
            GalleryAutomation.WithAutomationId(
                viewHeight,
                GalleryAutomation.SampleElementId("TwoPaneView", "Height"));
            viewHeight.ValueChanged += delegate { view.Height = viewHeight.Value; };

            var minWideWidth = CreateSlider("Minimum wide width", 200, 900, view.MinWideModeWidth);
            GalleryAutomation.WithAutomationId(
                minWideWidth,
                GalleryAutomation.SampleElementId("TwoPaneView", "MinWideWidth"));
            minWideWidth.ValueChanged += delegate { view.MinWideModeWidth = minWideWidth.Value; };

            var minTallHeight = CreateSlider("Minimum tall height", 200, 700, view.MinTallModeHeight);
            GalleryAutomation.WithAutomationId(
                minTallHeight,
                GalleryAutomation.SampleElementId("TwoPaneView", "MinTallHeight"));
            minTallHeight.ValueChanged += delegate { view.MinTallModeHeight = minTallHeight.Value; };

            var options = new StackPanel();
            options.Children.Add(priority);
            options.Children.Add(CreateSplitViewOption("Wide configuration", wideConfiguration));
            options.Children.Add(CreateSplitViewOption("Tall configuration", tallConfiguration));
            options.Children.Add(viewWidth);
            options.Children.Add(viewHeight);
            options.Children.Add(minWideWidth);
            options.Children.Add(minTallHeight);
            optionsContent = options;
            return content;
        }

        private static ComboBox CreateEnumComboBox(object selectedValue, IEnumerable<string> values)
        {
            var comboBox = new ComboBox
            {
                Width = 220,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            foreach (var value in values)
            {
                comboBox.Items.Add(value);
            }

            comboBox.SelectedItem = selectedValue.ToString();
            return comboBox;
        }

        private static Border CreateTwoPaneContent(
            string title,
            string description,
            string resourceKey,
            string fallbackColor)
        {
            return new Border
            {
                Padding = new Thickness(24),
                Background = GetThemeBrush(resourceKey, fallbackColor),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 20,
                            FontWeight = FontWeights.SemiBold
                        },
                        new TextBlock
                        {
                            Text = description,
                            Margin = new Thickness(0, 8, 0, 0),
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                }
            };
        }

        private static UIElement CreateTwoPaneViewStandaloneLayout(UIElement content, UIElement options)
        {
            var panel = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            panel.Children.Add(content);
            panel.Children.Add(new Border
            {
                Margin = new Thickness(24, 0, 0, 0),
                Child = options
            });
            return panel;
        }

        private static UIElement CreateSplitViewSample()
        {
            var content = CreateBasicSplitViewExampleContent(
                assignRootAutomationId: true,
                out var optionsContent);
            return CreateSplitViewStandaloneLayout(content, optionsContent);
        }

        private static GallerySamplePanel CreateBasicSplitViewExampleContent(
            bool assignRootAutomationId,
            out UIElement optionsContent)
        {
            var panel = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("SplitView"));
            }

            var contentText = new TextBlock
            {
                Name = "content",
                Margin = new Thickness(12, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(contentText, 1);
            var splitView = new Mux.SplitView
            {
                Name = "splitView",
                MaxWidth = 400,
                Height = 300,
                IsPaneOpen = true,
                IsTabStop = false,
                DisplayMode = Mux.SplitViewDisplayMode.Inline,
                CompactPaneLength = 48,
                OpenPaneLength = 256,
                PaneBackground = GetThemeBrush("SystemControlBackgroundChromeMediumLowBrush", "#F3F3F3"),
                Pane = CreateSplitViewPane(contentText),
                Content = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                    },
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "SPLITVIEW CONTENT",
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(12, 12, 0, 0),
                            TextWrapping = TextWrapping.Wrap
                        },
                        contentText
                    }
                }
            };
            GalleryAutomation.WithAutomationId(splitView, GalleryAutomation.SampleElementId("SplitView", "SplitView"));

            var splitViewHost = new Grid
            {
                Width = 400,
                Height = 300,
                VerticalAlignment = VerticalAlignment.Top
            };
            splitViewHost.Children.Add(splitView);

            var toggle = new ToggleButton
            {
                Name = "togglePaneButton",
                Content = "IsPaneOpen",
                IsChecked = splitView.IsPaneOpen
            };
            GalleryAutomation.WithAutomationId(toggle, GalleryAutomation.SampleElementId("SplitView", "IsPaneOpenToggle"));
            toggle.Checked += delegate { splitView.IsPaneOpen = true; };
            toggle.Unchecked += delegate { splitView.IsPaneOpen = false; };

            var placement = new Mux.ToggleSwitch
            {
                MinWidth = 120,
                Margin = new Thickness(0, 12, 0, 0),
                Header = "Placement",
                OffContent = "Left",
                OnContent = "Right"
            };
            placement.Toggled += delegate
            {
                splitView.PanePlacement = placement.IsOn
                    ? Mux.SplitViewPanePlacement.Right
                    : Mux.SplitViewPanePlacement.Left;
                UpdateSplitViewNavLinkLayout(splitView.Pane as Grid, splitView.PanePlacement);
            };

            var mode = new ComboBox
            {
                Name = "displayModeCombobox",
                Width = 196,
                Margin = new Thickness(0, 4, 0, 0)
            };
            mode.Items.Add("Inline");
            mode.Items.Add("CompactInline");
            mode.Items.Add("Overlay");
            mode.Items.Add("CompactOverlay");
            mode.SelectedIndex = 0;
            mode.SelectionChanged += delegate
            {
                if (mode.SelectedItem is string displayMode)
                {
                    splitView.DisplayMode = (Mux.SplitViewDisplayMode)Enum.Parse(typeof(Mux.SplitViewDisplayMode), displayMode);
                }
            };

            var paneBackground = new ComboBox
            {
                Name = "paneBackgroundCombobox",
                Width = 196,
                Margin = new Thickness(0, 4, 0, 0)
            };
            paneBackground.Items.Add("SystemControlBackgroundChromeMediumLowBrush");
            paneBackground.Items.Add("Red");
            paneBackground.Items.Add("Blue");
            paneBackground.Items.Add("Green");
            paneBackground.SelectedIndex = 0;
            paneBackground.SelectionChanged += delegate
            {
                splitView.PaneBackground = GetSplitViewPaneBackground(paneBackground.SelectedItem as string);
            };

            var openLength = CreateSlider("OpenPaneLength", 128, 500, splitView.OpenPaneLength);
            openLength.Name = "openPaneLengthSlider";
            openLength.Width = 196;
            openLength.TickFrequency = 8;
            openLength.Margin = new Thickness(0, 4, 0, 0);
            ControlHelper.SetHeader(openLength, null);
            openLength.ValueChanged += delegate { splitView.OpenPaneLength = openLength.Value; };

            var compactLength = CreateSlider("CompactPaneLength", 24, 128, splitView.CompactPaneLength);
            compactLength.Name = "compactPaneLengthSlider";
            compactLength.Width = 196;
            compactLength.TickFrequency = 8;
            compactLength.Margin = new Thickness(0, 4, 0, 0);
            ControlHelper.SetHeader(compactLength, null);
            compactLength.ValueChanged += delegate { splitView.CompactPaneLength = compactLength.Value; };

            var options = new StackPanel
            {
                Name = "SplitViewOptions"
            };
            options.Children.Add(toggle);
            options.Children.Add(placement);
            options.Children.Add(CreateSplitViewOption("DisplayMode", mode));
            options.Children.Add(CreateSplitViewOption("PaneBackground", paneBackground));
            options.Children.Add(CreateSplitViewOption("OpenPaneLength", openLength));
            options.Children.Add(CreateSplitViewOption("CompactPaneLength", compactLength, 4));

            panel.Children.Add(splitViewHost);
            optionsContent = options;
            return panel;
        }

        private static UIElement CreateSplitViewStandaloneLayout(UIElement content, UIElement options)
        {
            var panel = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            panel.Children.Add(content);
            panel.Children.Add(new Border
            {
                Margin = new Thickness(24, 0, 0, 0),
                Child = options
            });
            return panel;
        }

        private static StackPanel CreateSplitViewOption(string header, FrameworkElement control, double topMargin = 12)
        {
            return new StackPanel
            {
                Margin = new Thickness(0, topMargin, 0, 0),
                Children =
                {
                    new TextBlock
                    {
                        Text = header
                    },
                    control
                }
            };
        }

        private static UIElement CreateSplitViewPane(TextBlock contentText)
        {
            var pane = new Grid();
            pane.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            pane.RowDefinitions.Add(new RowDefinition());
            pane.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            pane.Children.Add(new TextBlock
            {
                Name = "PaneHeader",
                Text = "PANE CONTENT",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(60, 12, 0, 0)
            });

            var navLinksList = new ListView
            {
                Name = "NavLinksList",
                Margin = new Thickness(0, 12, 0, 0),
                SelectionMode = SelectionMode.Single,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            navLinksList.Items.Add(CreateSplitViewNavItem("People", Mux.Symbol.People, Mux.SplitViewPanePlacement.Left));
            navLinksList.Items.Add(CreateSplitViewNavItem("Globe", Mux.Symbol.Globe, Mux.SplitViewPanePlacement.Left));
            navLinksList.Items.Add(CreateSplitViewNavItem("Message", Mux.Symbol.Message, Mux.SplitViewPanePlacement.Left));
            navLinksList.Items.Add(CreateSplitViewNavItem("Mail", Mux.Symbol.Mail, Mux.SplitViewPanePlacement.Left));
            navLinksList.SelectionChanged += delegate
            {
                if (navLinksList.SelectedItem is ListViewItem item && item.Tag is string label)
                {
                    contentText.Text = label + " Page";
                }
            };
            Grid.SetRow(navLinksList, 1);
            pane.Children.Add(navLinksList);
            pane.Tag = navLinksList;

            return pane;
        }

        private static ListViewItem CreateSplitViewNavItem(string label, Mux.Symbol symbol, Mux.SplitViewPanePlacement placement)
        {
            var item = new ListViewItem
            {
                Tag = label,
                Content = CreateSplitViewNavItemContent(label, symbol, placement)
            };
            AutomationProperties.SetName(item, label);
            return item;
        }

        private static Grid CreateSplitViewNavItemContent(string label, Mux.Symbol symbol, Mux.SplitViewPanePlacement placement)
        {
            var grid = new Grid
            {
                Margin = placement == Mux.SplitViewPanePlacement.Right
                    ? new Thickness(0, 0, 2, 0)
                    : new Thickness(2, 0, 0, 0)
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = placement == Mux.SplitViewPanePlacement.Right ? new GridLength(1, GridUnitType.Star) : GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = placement == Mux.SplitViewPanePlacement.Right ? GridLength.Auto : new GridLength(1, GridUnitType.Star) });
            AutomationProperties.SetName(grid, label);

            var icon = new Mux.SymbolIcon(symbol)
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            var text = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = placement == Mux.SplitViewPanePlacement.Right
                    ? new Thickness(0, 0, 24, 0)
                    : new Thickness(24, 0, 0, 0)
            };

            if (placement == Mux.SplitViewPanePlacement.Right)
            {
                grid.Children.Add(text);
                Grid.SetColumn(icon, 1);
                grid.Children.Add(icon);
            }
            else
            {
                grid.Children.Add(icon);
                Grid.SetColumn(text, 1);
                grid.Children.Add(text);
            }

            return grid;
        }

        private static void UpdateSplitViewNavLinkLayout(Grid pane, Mux.SplitViewPanePlacement placement)
        {
            if (pane == null)
            {
                return;
            }

            var navLinksList = pane.Tag as ListView;
            if (navLinksList == null)
            {
                return;
            }

            foreach (var item in navLinksList.Items)
            {
                if (item is ListViewItem listViewItem && listViewItem.Tag is string label)
                {
                    listViewItem.Content = CreateSplitViewNavItemContent(label, GetSplitViewNavSymbol(label), placement);
                }
            }
        }

        private static Mux.Symbol GetSplitViewNavSymbol(string label)
        {
            switch (label)
            {
                case "People":
                    return Mux.Symbol.People;
                case "Globe":
                    return Mux.Symbol.Globe;
                case "Message":
                    return Mux.Symbol.Message;
                default:
                    return Mux.Symbol.Mail;
            }
        }

        private static Brush GetSplitViewPaneBackground(string color)
        {
            switch (color)
            {
                case "Red":
                    return Brushes.Red;
                case "Blue":
                    return Brushes.Blue;
                case "Green":
                    return Brushes.Green;
                default:
                    return GetThemeBrush("SystemControlBackgroundChromeMediumLowBrush", "#F3F3F3");
            }
        }

        private static Brush GetThemeBrush(string resourceKey, string fallbackColor)
        {
            return Application.Current?.TryFindResource(resourceKey) as Brush ?? CreateBrush(fallbackColor);
        }

        private static Slider CreateSlider(string header, double minimum, double maximum, double value)
        {
            var slider = WinUISampleSlider.ShowValueFill(new Slider
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                Width = 220,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                IsSnapToTickEnabled = true,
                TickFrequency = 1
            });
            ControlHelper.SetHeader(slider, header);
            return slider;
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }
    }
}

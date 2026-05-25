using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
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

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Border":
                    return CreateBorderSample();
                case "Canvas":
                    return CreateCanvasSample();
                case "Expander":
                    return CreateExpanderSample();
                case "Grid":
                    return CreateGridSample();
                case "GridSplitter":
                    return CreateGridSplitterSample();
                case "GroupBox":
                    return CreateGroupBoxSample();
                case "ResizeGrip":
                    return CreateResizeGripSample();
                case "SplitView":
                    return CreateSplitViewSample();
                case "StackPanel":
                    return CreateStackPanelSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "SplitView":
                    return new[]
                    {
                        new GalleryExample(
                            "A basic SplitView.",
                            CreateBasicSplitViewExampleContent(assignRootAutomationId: true),
                            SplitViewBasicXaml,
                            null)
                    };
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static UIElement CreateBorderSample()
        {
            var panel = CreateSamplePanel("Border draws background, stroke, padding, and optional rounded corners around one child.");
            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = CreateBrush("#FFD700"),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(8, 5, 8, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = "Text inside a border",
                    FontSize = 18,
                    Foreground = Brushes.Black
                }
            };

            var thickness = new Slider
            {
                Minimum = 0,
                Maximum = 10,
                Value = 2,
                Width = 220,
                Margin = new Thickness(0, 14, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(thickness, "BorderThickness");
            thickness.ValueChanged += delegate
            {
                border.BorderThickness = new Thickness(Math.Round(thickness.Value));
            };

            var colors = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            colors.Children.Add(CreateBrushCombo("Background", "White", delegate(Brush brush) { border.Background = brush; }));
            colors.Children.Add(CreateBrushCombo("BorderBrush", "Gold", delegate(Brush brush) { border.BorderBrush = brush; }));

            panel.Children.Add(border);
            panel.Children.Add(thickness);
            panel.Children.Add(colors);
            return panel;
        }

        private static UIElement CreateCanvasSample()
        {
            var panel = CreateSamplePanel("Canvas positions children with absolute coordinates and z-index.");
            var canvas = new Canvas
            {
                Width = 150,
                Height = 150,
                Background = Brushes.Gray
            };
            var red = CreateRect(40, 40, Brushes.Red);
            var blue = CreateRect(40, 40, Brushes.Blue);
            var green = CreateRect(40, 40, Brushes.Green);
            var yellow = CreateRect(40, 40, Brushes.Gold);
            Canvas.SetLeft(red, 0);
            Canvas.SetTop(red, 0);
            Canvas.SetLeft(blue, 20);
            Canvas.SetTop(blue, 20);
            Canvas.SetZIndex(blue, 1);
            Canvas.SetLeft(green, 40);
            Canvas.SetTop(green, 40);
            Canvas.SetZIndex(green, 2);
            Canvas.SetLeft(yellow, 60);
            Canvas.SetTop(yellow, 60);
            Canvas.SetZIndex(yellow, 3);
            canvas.Children.Add(red);
            canvas.Children.Add(blue);
            canvas.Children.Add(green);
            canvas.Children.Add(yellow);

            var left = CreateSlider("Canvas.Left", 0, 100, 0);
            var top = CreateSlider("Canvas.Top", 0, 100, 0);
            var zIndex = CreateSlider("Canvas.ZIndex", 0, 4, 0);
            left.ValueChanged += delegate { Canvas.SetLeft(red, left.Value); };
            top.ValueChanged += delegate { Canvas.SetTop(red, top.Value); };
            zIndex.ValueChanged += delegate { Canvas.SetZIndex(red, (int)Math.Round(zIndex.Value)); };

            var options = new StackPanel
            {
                Width = 240,
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(left);
            options.Children.Add(top);
            options.Children.Add(zIndex);

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(canvas);
            row.Children.Add(options);
            panel.Children.Add(row);
            return panel;
        }

        private static UIElement CreateExpanderSample()
        {
            var panel = CreateSamplePanel("Expander reveals or hides additional content under a header.");
            var expander = new Expander
            {
                Header = "This text is in the header",
                Content = new TextBlock
                {
                    Text = "This is in the content",
                    Margin = new Thickness(4)
                },
                IsExpanded = false,
                ExpandDirection = ExpandDirection.Down,
                Width = 360,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var direction = new ComboBox
            {
                Width = 220,
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[] { ExpandDirection.Down, ExpandDirection.Up, ExpandDirection.Left, ExpandDirection.Right },
                SelectedItem = expander.ExpandDirection
            };
            ControlHelper.SetHeader(direction, "ExpandDirection");
            direction.SelectionChanged += delegate
            {
                if (direction.SelectedItem is ExpandDirection)
                {
                    expander.ExpandDirection = (ExpandDirection)direction.SelectedItem;
                }
            };

            panel.Children.Add(expander);
            panel.Children.Add(direction);
            return panel;
        }

        private static UIElement CreateGridSample()
        {
            var panel = CreateSamplePanel("Grid arranges children into rows and columns; spacing is represented with child margins in WPF.");
            var grid = new Grid
            {
                Width = 240,
                Height = 180,
                Background = Brushes.Gray
            };
            for (var i = 0; i < 3; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(56) });
            }

            var red = CreateRect(50, 50, Brushes.Red);
            var blue = CreateRect(50, 50, Brushes.Blue);
            var green = CreateRect(50, 50, Brushes.Green);
            var yellow = CreateRect(50, 50, Brushes.Gold);
            Grid.SetRow(blue, 1);
            Grid.SetColumn(green, 1);
            Grid.SetRow(yellow, 1);
            Grid.SetColumn(yellow, 1);
            grid.Children.Add(red);
            grid.Children.Add(blue);
            grid.Children.Add(green);
            grid.Children.Add(yellow);

            var column = CreateSlider("Grid.Column", 0, 2, 0);
            var row = CreateSlider("Grid.Row", 0, 2, 0);
            var spacing = CreateSlider("Spacing", 0, 16, 6);
            Action update = delegate
            {
                Grid.SetColumn(red, (int)Math.Round(column.Value));
                Grid.SetRow(red, (int)Math.Round(row.Value));
                foreach (UIElement child in grid.Children)
                {
                    ((FrameworkElement)child).Margin = new Thickness(Math.Round(spacing.Value) / 2);
                }
            };
            column.ValueChanged += delegate { update(); };
            row.ValueChanged += delegate { update(); };
            spacing.ValueChanged += delegate { update(); };
            update();

            var options = new StackPanel
            {
                Width = 240,
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(column);
            options.Children.Add(row);
            options.Children.Add(spacing);

            var root = new StackPanel { Orientation = Orientation.Horizontal };
            root.Children.Add(grid);
            root.Children.Add(options);
            panel.Children.Add(root);
            return panel;
        }

        private static UIElement CreateGridSplitterSample()
        {
            var panel = CreateSamplePanel("GridSplitter lets users redistribute space between Grid rows or columns.");
            var grid = new Grid
            {
                Width = 520,
                Height = 180
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star), MinWidth = 120 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 120 });

            var left = CreatePane("Left pane", "#D9EAF7");
            var splitter = new GridSplitter
            {
                Width = 8,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                Background = CreateBrush("#808080")
            };
            var right = CreatePane("Right pane", "#E6E6E6");
            Grid.SetColumn(splitter, 1);
            Grid.SetColumn(right, 2);
            grid.Children.Add(left);
            grid.Children.Add(splitter);
            grid.Children.Add(right);
            panel.Children.Add(grid);
            return panel;
        }

        private static UIElement CreateGroupBoxSample()
        {
            var panel = CreateSamplePanel("GroupBox groups related controls with a header and a visible boundary.");
            var options = new StackPanel();
            options.Children.Add(new CheckBox { Content = "Enable notifications", IsChecked = true, Margin = new Thickness(0, 0, 0, 6) });
            options.Children.Add(new CheckBox { Content = "Show message previews", IsChecked = true, Margin = new Thickness(0, 0, 0, 6) });
            options.Children.Add(new CheckBox { Content = "Play a sound" });
            panel.Children.Add(new GroupBox
            {
                Header = "Notification settings",
                Width = 320,
                Padding = new Thickness(12),
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = options
            });
            return panel;
        }

        private static UIElement CreateResizeGripSample()
        {
            var panel = CreateSamplePanel("ResizeGrip provides the standard visual handle used by resizable surfaces.");
            var host = new Grid
            {
                Width = 260,
                Height = 150,
                Background = CreateBrush("#F3F3F3")
            };
            host.Children.Add(new TextBlock
            {
                Text = "Resizable surface preview",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.72
            });
            host.Children.Add(new ResizeGrip
            {
                Width = 16,
                Height = 16,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 4, 4)
            });
            panel.Children.Add(new Border
            {
                BorderBrush = CreateBrush("#D8D8D8"),
                BorderThickness = new Thickness(1),
                Child = host
            });
            return panel;
        }

        private static UIElement CreateSplitViewSample()
        {
            return CreateBasicSplitViewExampleContent(assignRootAutomationId: true);
        }

        private static GallerySamplePanel CreateBasicSplitViewExampleContent(bool assignRootAutomationId)
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
            toggle.Checked += delegate { splitView.IsPaneOpen = true; };
            toggle.Unchecked += delegate { splitView.IsPaneOpen = false; };

            var placement = new Mux.ToggleSwitch
            {
                MinWidth = 120,
                Margin = new Thickness(0, 12, 8, 0),
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
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(toggle);
            options.Children.Add(placement);
            options.Children.Add(CreateSplitViewOption("DisplayMode", mode));
            options.Children.Add(CreateSplitViewOption("PaneBackground", paneBackground));
            options.Children.Add(CreateSplitViewOption("OpenPaneLength", openLength));
            options.Children.Add(CreateSplitViewOption("CompactPaneLength", compactLength));

            panel.Children.Add(splitViewHost);
            panel.Children.Add(options);
            return panel;
        }

        private static UIElement CreateStackPanelSample()
        {
            var panel = CreateSamplePanel("StackPanel arranges children in one direction; ModernWpf StackPanelEx adds WinUI-style spacing and chrome.");
            var stack = new Mux.StackPanelEx
            {
                Orientation = Orientation.Vertical,
                Spacing = 8
            };
            PopulateColoredChildren(stack);

            var orientation = new ComboBox
            {
                Width = 180,
                Margin = new Thickness(0, 14, 8, 0),
                ItemsSource = new[] { Orientation.Vertical, Orientation.Horizontal },
                SelectedItem = stack.Orientation
            };
            ControlHelper.SetHeader(orientation, "Orientation");
            orientation.SelectionChanged += delegate
            {
                if (orientation.SelectedItem is Orientation)
                {
                    stack.Orientation = (Orientation)orientation.SelectedItem;
                }
            };

            var spacing = CreateSlider("Spacing", 0, 24, stack.Spacing);
            spacing.Width = 180;
            spacing.ValueChanged += delegate { stack.Spacing = Math.Round(spacing.Value); };

            var controls = new StackPanel { Orientation = Orientation.Horizontal };
            controls.Children.Add(orientation);
            controls.Children.Add(spacing);

            panel.Children.Add(stack);
            panel.Children.Add(controls);
            return panel;
        }

        private static StackPanel CreateSplitViewOption(string header, FrameworkElement control)
        {
            return new StackPanel
            {
                Margin = new Thickness(0, 12, 0, 0),
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
            AutomationProperties.SetAutomationId(navLinksList, "NavLinksList");
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

        private static ComboBox CreateBrushCombo(string header, string selected, Action<Brush> changed)
        {
            var combo = new ComboBox
            {
                Width = 160,
                Margin = new Thickness(0, 0, 12, 0),
                ItemsSource = new[] { "Green", "Yellow", "Blue", "White", "Gold" },
                SelectedItem = selected
            };
            ControlHelper.SetHeader(combo, header);
            combo.SelectionChanged += delegate
            {
                changed(CreateNamedBrush((string)combo.SelectedItem));
            };
            return combo;
        }

        private static Brush CreateNamedBrush(string name)
        {
            switch (name)
            {
                case "Green":
                    return Brushes.Green;
                case "Yellow":
                    return Brushes.Yellow;
                case "Blue":
                    return Brushes.Blue;
                case "Gold":
                    return CreateBrush("#FFD700");
                default:
                    return Brushes.White;
            }
        }

        private static Slider CreateSlider(string header, double minimum, double maximum, double value)
        {
            var slider = new Slider
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                Width = 220,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                IsSnapToTickEnabled = true,
                TickFrequency = 1
            };
            ControlHelper.SetHeader(slider, header);
            return slider;
        }

        private static void PopulateColoredChildren(Panel panel)
        {
            panel.Children.Clear();
            panel.Children.Add(CreateRect(40, 40, Brushes.Red));
            panel.Children.Add(CreateRect(40, 40, Brushes.Blue));
            panel.Children.Add(CreateRect(40, 40, Brushes.Green));
            panel.Children.Add(CreateRect(40, 40, Brushes.Gold));
        }

        private static Rectangle CreateRect(double width, double height, Brush fill)
        {
            return new Rectangle
            {
                Width = width,
                Height = height,
                Fill = fill
            };
        }

        private static Border CreatePane(string text, string color)
        {
            return new Border
            {
                Background = CreateBrush(color),
                Child = new TextBlock
                {
                    Text = text,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
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

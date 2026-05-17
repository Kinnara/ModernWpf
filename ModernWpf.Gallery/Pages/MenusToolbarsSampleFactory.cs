using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class MenusToolbarsSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AppBarButton":
                    return CreateAppBarButtonSample();
                case "AppBarSeparator":
                    return CreateAppBarSeparatorSample();
                case "AppBarToggleButton":
                    return CreateAppBarToggleButtonSample();
                case "CommandBar":
                    return CreateCommandBarSample();
                case "CommandBarFlyout":
                    return CreateCommandBarFlyoutSample();
                case "MenuBar":
                    return CreateMenuBarSample();
                case "MenuFlyout":
                    return CreateMenuFlyoutSample();
                case "SwipeControl":
                    return CreateSwipeControlSample();
                case "StandardUICommand":
                    return CreateStandardCommandSample();
                case "XamlUICommand":
                    return CreateXamlCommandSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateAppBarButtonSample()
        {
            var panel = CreateSamplePanel("AppBarButton presents a command with a compact icon and label.");
            var output = CreateOutput("Choose a command.");
            var button = CreateAppBarButton(Mux.Symbol.Save, "Save");
            button.Click += delegate { output.Text = "Save command selected."; };
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateAppBarSeparatorSample()
        {
            var panel = CreateSamplePanel("AppBarSeparator visually groups related toolbar commands.");
            var bar = new Mux.CommandBar();
            bar.PrimaryCommands.Add(CreateAppBarButton(Mux.Symbol.Cut, "Cut"));
            bar.PrimaryCommands.Add(CreateAppBarButton(Mux.Symbol.Copy, "Copy"));
            bar.PrimaryCommands.Add(new Mux.AppBarSeparator());
            bar.PrimaryCommands.Add(CreateAppBarButton(Mux.Symbol.Paste, "Paste"));
            panel.Children.Add(bar);
            return panel;
        }

        private static UIElement CreateAppBarToggleButtonSample()
        {
            var panel = CreateSamplePanel("AppBarToggleButton keeps a checked state for toolbar options.");
            var output = CreateOutput("Bold is off.");
            var button = new Mux.AppBarToggleButton
            {
                Icon = new Mux.SymbolIcon(Mux.Symbol.Bold),
                Label = "Bold"
            };
            button.Checked += delegate { output.Text = "Bold is on."; };
            button.Unchecked += delegate { output.Text = "Bold is off."; };
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateCommandBarSample()
        {
            var panel = CreateSamplePanel("CommandBar collects primary commands and overflow commands in one toolbar.");
            var output = CreateOutput("No command selected.");
            var bar = new Mux.CommandBar();
            bar.PrimaryCommands.Add(CreateAppBarButton(Mux.Symbol.Add, "New", output));
            bar.PrimaryCommands.Add(CreateAppBarButton(Mux.Symbol.Edit, "Edit", output));
            bar.PrimaryCommands.Add(CreateAppBarButton(Mux.Symbol.Delete, "Delete", output));
            bar.SecondaryCommands.Add(CreateAppBarButton(Mux.Symbol.Rename, "Rename", output));
            bar.SecondaryCommands.Add(CreateAppBarButton(Mux.Symbol.Share, "Share", output));
            panel.Children.Add(bar);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateCommandBarFlyoutSample()
        {
            var panel = CreateSamplePanel("CommandBarFlyout opens a compact command surface from a button or selected content.");
            var output = CreateOutput("No command selected.");
            var button = CreateButton("Open CommandBarFlyout");
            var flyout = new Mux.CommandBarFlyout
            {
                Placement = FlyoutPlacementMode.Bottom
            };
            flyout.PrimaryCommands.Add(CreateAppBarButton(Mux.Symbol.Copy, "Copy", output));
            flyout.PrimaryCommands.Add(CreateAppBarButton(Mux.Symbol.Paste, "Paste", output));
            flyout.SecondaryCommands.Add(CreateAppBarButton(Mux.Symbol.SelectAll, "Select all", output));
            flyout.SecondaryCommands.Add(CreateAppBarButton(Mux.Symbol.View, "Inspect", output));
            button.Click += delegate { flyout.ShowAt(button); };
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateMenuBarSample()
        {
            var panel = CreateSamplePanel("MenuBar presents top-level menu items with flyout commands.");
            var output = CreateOutput("No menu item selected.");
            var menu = new Mux.MenuBar();
            var file = new Mux.MenuBarItem { Title = "_File" };
            file.Items.Add(CreateMenuItem("_New", output));
            file.Items.Add(CreateMenuItem("_Open", output));
            file.Items.Add(new Separator());
            file.Items.Add(CreateMenuItem("E_xit", output));

            var edit = new Mux.MenuBarItem { Title = "_Edit" };
            edit.Items.Add(CreateMenuItem("_Copy", output));
            edit.Items.Add(CreateMenuItem("_Paste", output));

            menu.Items.Add(file);
            menu.Items.Add(edit);
            panel.Children.Add(menu);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateMenuFlyoutSample()
        {
            var panel = CreateSamplePanel("MenuFlyout shows a menu of contextual actions anchored to a control.");
            var output = CreateOutput("No menu item selected.");
            var button = CreateButton("Open MenuFlyout");
            var flyout = new Mux.MenuFlyout
            {
                Placement = FlyoutPlacementMode.Bottom
            };
            flyout.Items.Add(CreateMenuItem("Open", output));
            flyout.Items.Add(CreateMenuItem("Pin to top", output));
            flyout.Items.Add(new Separator());
            flyout.Items.Add(CreateMenuItem("Delete", output));
            Mux.FlyoutService.SetFlyout(button, flyout);
            panel.Children.Add(button);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateSwipeControlSample()
        {
            var panel = CreateSamplePanel("SwipeControl maps to an explicit WPF action strip because ModernWpf does not currently expose SwipeControl.");
            var output = CreateOutput("No action selected.");
            var row = new Grid { Width = 420, HorizontalAlignment = HorizontalAlignment.Left };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.Children.Add(new Border
            {
                Padding = new Thickness(12),
                BorderThickness = new Thickness(1),
                Child = new TextBlock { Text = "Quarterly report.pdf" }
            });

            var actions = new StackPanel { Orientation = Orientation.Horizontal };
            actions.Children.Add(CreateSmallButton("Archive", output));
            actions.Children.Add(CreateSmallButton("Delete", output));
            Grid.SetColumn(actions, 1);
            row.Children.Add(actions);

            panel.Children.Add(row);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateStandardCommandSample()
        {
            var panel = CreateSamplePanel("StandardUICommand maps to WPF RoutedUICommand in this port.");
            var output = CreateOutput("Command has not run.");
            var saveCommand = new RoutedUICommand("Save", "Save", typeof(MenusToolbarsSampleFactory));
            panel.CommandBindings.Add(new CommandBinding(
                saveCommand,
                delegate { output.Text = "RoutedUICommand executed: Save."; },
                delegate(object sender, CanExecuteRoutedEventArgs args) { args.CanExecute = true; }));

            panel.Children.Add(new Button
            {
                Content = "Save",
                Command = saveCommand,
                Padding = new Thickness(16, 6, 16, 6),
                HorizontalAlignment = HorizontalAlignment.Left
            });
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateXamlCommandSample()
        {
            var panel = CreateSamplePanel("XamlUICommand maps to a WPF command-backed button with explicit icon and text.");
            var output = CreateOutput("Command has not run.");
            var command = new RoutedUICommand("Download", "Download", typeof(MenusToolbarsSampleFactory));
            panel.CommandBindings.Add(new CommandBinding(
                command,
                delegate { output.Text = "Command executed: Download."; },
                delegate(object sender, CanExecuteRoutedEventArgs args) { args.CanExecute = true; }));

            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(new Mux.SymbolIcon(Mux.Symbol.Download)
            {
                Margin = new Thickness(0, 0, 8, 0)
            });
            content.Children.Add(new TextBlock
            {
                Text = "Download",
                VerticalAlignment = VerticalAlignment.Center
            });

            panel.Children.Add(new Button
            {
                Content = content,
                Command = command,
                Padding = new Thickness(16, 6, 16, 6),
                HorizontalAlignment = HorizontalAlignment.Left
            });
            panel.Children.Add(output);
            return panel;
        }

        private static Mux.AppBarButton CreateAppBarButton(Mux.Symbol symbol, string label)
        {
            return new Mux.AppBarButton
            {
                Icon = new Mux.SymbolIcon(symbol),
                Label = label
            };
        }

        private static Mux.AppBarButton CreateAppBarButton(Mux.Symbol symbol, string label, TextBlock output)
        {
            var button = CreateAppBarButton(symbol, label);
            button.Click += delegate { output.Text = label + " selected."; };
            return button;
        }

        private static MenuItem CreateMenuItem(string header, TextBlock output)
        {
            var item = new MenuItem { Header = header };
            item.Click += delegate { output.Text = header.Replace("_", string.Empty) + " selected."; };
            return item;
        }

        private static Button CreateSmallButton(string text, TextBlock output)
        {
            var button = new Button
            {
                Content = text,
                Padding = new Thickness(12, 5, 12, 5),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            button.Click += delegate { output.Text = text + " selected."; };
            return button;
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
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class MenusToolbarsSampleFactory
    {
        private const string CommandBarExampleXaml =
@"<CommandBar Background=""Transparent"" IsOpen=""$(IsOpen)"" DefaultLabelPosition=""Right""$(IsSticky)>
    <AppBarButton Icon=""Add"" Label=""Add""/>
    <AppBarButton Icon=""Edit"" Label=""Edit""/>
    <AppBarButton Icon=""Share"" Label=""Share""/>
    <CommandBar.SecondaryCommands>
        <AppBarButton Icon=""Setting"" Label=""Settings"">
            <AppBarButton.KeyboardAccelerators>
                    <KeyboardAccelerator Modifiers=""Control"" Key=""I"" />
            </AppBarButton.KeyboardAccelerators>
        </AppBarButton>$(MultipleButtonsSecondaryCommands)
    </CommandBar.SecondaryCommands>
</CommandBar>";

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "CommandBar":
                    return CreateCommandBarExamples();
                case "MenuBar":
                    return CreateMenuBarExamples(sampleSnippets);
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

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
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("CommandBar"));
            panel.Children.Add(CreateCommandBarExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateCommandBarExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A command bar with labels on the side free floating in a page",
                    CreateCommandBarExampleContent(assignRootAutomationId: true),
                    CommandBarExampleXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateCommandBarExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("CommandBar"));
            }

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });

            var samplePanel = new StackPanel();
            var output = new TextBlock
            {
                Name = "SelectedOptionText",
                Padding = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            var commandBar = new Mux.CommandBar
            {
                Name = "PrimaryCommandBar",
                DefaultLabelPosition = Mux.CommandBarDefaultLabelPosition.Right,
                IsOpen = false,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(commandBar, GalleryAutomation.SampleElementId("CommandBar", "CommandBar"));

            var addButton = CreateCommandBarAppBarButton(Mux.Symbol.Add, "Add", output, "Ctrl+A");
            addButton.Name = "addButton";
            var editButton = CreateCommandBarAppBarButton(Mux.Symbol.Edit, "Edit", output, "Ctrl+E");
            editButton.Name = "editButton";
            var shareButton = CreateCommandBarAppBarButton(Mux.Symbol.Share, "Share", output, "F4");
            shareButton.Name = "shareButton";
            commandBar.PrimaryCommands.Add(addButton);
            commandBar.PrimaryCommands.Add(editButton);
            commandBar.PrimaryCommands.Add(shareButton);

            var settingsButton = CreateCommandBarAppBarButton(Mux.Symbol.Setting, "Settings", output, "Ctrl+I");
            settingsButton.Name = "settingsButton";
            commandBar.SecondaryCommands.Add(settingsButton);

            samplePanel.Children.Add(commandBar);
            samplePanel.Children.Add(output);
            layout.Children.Add(samplePanel);

            var options = CreateCommandBarOptions(commandBar, output);
            Grid.SetColumn(options, 2);
            layout.Children.Add(options);

            root.Children.Add(layout);
            return root;
        }

        private static StackPanel CreateCommandBarOptions(Mux.CommandBar commandBar, TextBlock output)
        {
            var options = new StackPanel
            {
                Width = 180,
                VerticalAlignment = VerticalAlignment.Top
            };
            options.Children.Add(new TextBlock { Text = "Show or hide" });
            options.Children.Add(CreateOptionButton("Open command bar", delegate
            {
                commandBar.IsOpen = true;
            }));
            options.Children.Add(CreateOptionButton("Close command bar", delegate
            {
                commandBar.IsOpen = false;
            }));
            options.Children.Add(new TextBlock
            {
                Text = "Modify content",
                Margin = new Thickness(0, 16, 0, 0)
            });
            options.Children.Add(CreateOptionButton("Add secondary commands", delegate
            {
                AddCommandBarSecondaryCommands(commandBar, output);
            }));
            options.Children.Add(CreateOptionButton("Remove secondary commands", delegate
            {
                RemoveCommandBarSecondaryCommands(commandBar);
            }));
            return options;
        }

        private static Button CreateOptionButton(string content, RoutedEventHandler click)
        {
            var button = new Button
            {
                Content = content,
                Margin = new Thickness(0, 12, 0, 0)
            };
            button.Click += click;
            return button;
        }

        private static void AddCommandBarSecondaryCommands(Mux.CommandBar commandBar, TextBlock output)
        {
            if (commandBar.SecondaryCommands.Count != 1)
            {
                return;
            }

            commandBar.SecondaryCommands.Add(CreateCommandBarAppBarButton(Mux.Symbol.Add, "Button 1", output, "Ctrl+N"));
            commandBar.SecondaryCommands.Add(CreateCommandBarAppBarButton(Mux.Symbol.Delete, "Button 2", output, "Delete"));
            commandBar.SecondaryCommands.Add(new Mux.AppBarSeparator());
            commandBar.SecondaryCommands.Add(CreateCommandBarAppBarButton(Mux.Symbol.FontDecrease, "Button 3", output, "Ctrl+-"));
            commandBar.SecondaryCommands.Add(CreateCommandBarAppBarButton(Mux.Symbol.FontIncrease, "Button 4", output, "Ctrl++"));
        }

        private static void RemoveCommandBarSecondaryCommands(Mux.CommandBar commandBar)
        {
            while (commandBar.SecondaryCommands.Count > 1)
            {
                commandBar.SecondaryCommands.RemoveAt(commandBar.SecondaryCommands.Count - 1);
            }
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
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("MenuBar"));
            panel.Children.Add(CreateSimpleMenuBarExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateMenuBarExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "A simple MenuBar",
                    CreateSimpleMenuBarExampleContent(assignRootAutomationId: true),
                    FindSnippetText(sampleSnippets, "MenuBarSample1.txt"),
                    null),
                new GalleryExample(
                    "MenuBar with keyboard accelerators",
                    CreateKeyboardAcceleratorsMenuBarExampleContent(),
                    FindSnippetText(sampleSnippets, "MenuBarSample3.txt"),
                    null),
                new GalleryExample(
                    "MenuBar with submenus, separators, and radio items",
                    CreateSubmenusMenuBarExampleContent(),
                    FindSnippetText(sampleSnippets, "MenuBarSample2.txt"),
                    null)
            };
        }

        private static GallerySamplePanel CreateMenuBarExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("MenuBar"));
            }

            return root;
        }

        private static GallerySamplePanel CreateSimpleMenuBarExampleContent(bool assignRootAutomationId)
        {
            var panel = CreateMenuBarExampleRoot(assignRootAutomationId);
            var output = CreateSelectionOutput("SelectedOptionText");
            var menu = CreateMenuBar();
            GalleryAutomation.WithAutomationId(menu, GalleryAutomation.SampleElementId("MenuBar", "MenuBar"));

            var file = new Mux.MenuBarItem { Title = "File" };
            file.Items.Add(CreateMenuItem("New", output));
            file.Items.Add(CreateMenuItem("Open...", output));
            file.Items.Add(CreateMenuItem("Save", output));
            file.Items.Add(CreateMenuItem("Exit", output));

            var edit = new Mux.MenuBarItem { Title = "Edit" };
            edit.Items.Add(CreateMenuItem("Undo", output));
            edit.Items.Add(CreateMenuItem("Cut", output));
            edit.Items.Add(CreateMenuItem("Copy", output));
            edit.Items.Add(CreateMenuItem("Paste", output));

            var help = new Mux.MenuBarItem { Title = "Help" };
            help.Items.Add(CreateMenuItem("About", output));

            menu.Items.Add(file);
            menu.Items.Add(edit);
            menu.Items.Add(help);
            panel.Children.Add(output);
            panel.Children.Add(menu);
            return panel;
        }

        private static GallerySamplePanel CreateKeyboardAcceleratorsMenuBarExampleContent()
        {
            var panel = CreateMenuBarExampleRoot(assignRootAutomationId: false);
            var output = CreateSelectionOutput("SelectedOptionText1");
            var menu = CreateMenuBar();
            GalleryAutomation.WithAutomationId(menu, GalleryAutomation.SampleElementId("MenuBar", "KeyboardAcceleratorsMenuBar"));

            var file = new Mux.MenuBarItem { Title = "File" };
            file.Items.Add(CreateMenuItem("New", output, "Ctrl+N"));
            file.Items.Add(CreateMenuItem("Open...", output, "Ctrl+O"));
            file.Items.Add(CreateMenuItem("Save", output, "Ctrl+S"));
            file.Items.Add(CreateMenuItem("Exit", output, "Ctrl+E"));

            var edit = new Mux.MenuBarItem { Title = "Edit" };
            edit.Items.Add(CreateMenuItem("Undo", output, "Ctrl+Z"));
            edit.Items.Add(CreateMenuItem("Cut", output, "Ctrl+X"));
            edit.Items.Add(CreateMenuItem("Copy", output, "Ctrl+C"));
            edit.Items.Add(CreateMenuItem("Paste", output, "Ctrl+V"));

            var help = new Mux.MenuBarItem { Title = "Help" };
            help.Items.Add(CreateMenuItem("About", output, "Ctrl+I"));

            menu.Items.Add(file);
            menu.Items.Add(edit);
            menu.Items.Add(help);
            panel.Children.Add(output);
            panel.Children.Add(menu);
            return panel;
        }

        private static GallerySamplePanel CreateSubmenusMenuBarExampleContent()
        {
            var panel = CreateMenuBarExampleRoot(assignRootAutomationId: false);
            var output = CreateSelectionOutput("SelectedOptionText2");
            var menu = CreateMenuBar();
            GalleryAutomation.WithAutomationId(menu, GalleryAutomation.SampleElementId("MenuBar", "SubmenusMenuBar"));

            var file = new Mux.MenuBarItem { Title = "File" };
            var newSubmenu = new MenuItem { Header = "New" };
            newSubmenu.Items.Add(CreateMenuItem("Plain Text Document", output));
            newSubmenu.Items.Add(CreateMenuItem("Rich Text Document", output));
            newSubmenu.Items.Add(CreateMenuItem("Other Formats...", output));
            file.Items.Add(newSubmenu);
            file.Items.Add(CreateMenuItem("Open...", output));
            file.Items.Add(CreateMenuItem("Save", output));
            file.Items.Add(new Separator());
            file.Items.Add(CreateMenuItem("Exit", output));

            var edit = new Mux.MenuBarItem { Title = "Edit" };
            edit.Items.Add(CreateMenuItem("Undo", output));
            edit.Items.Add(CreateMenuItem("Cut", output));
            edit.Items.Add(CreateMenuItem("Copy", output));
            edit.Items.Add(CreateMenuItem("Paste", output));

            var view = new Mux.MenuBarItem { Title = "View" };
            view.Items.Add(CreateMenuItem("Output", output));
            view.Items.Add(new Separator());
            view.Items.Add(CreateRadioMenuItem("Landscape", "OrientationGroup", isChecked: false, output));
            view.Items.Add(CreateRadioMenuItem("Portrait", "OrientationGroup", isChecked: true, output));
            view.Items.Add(new Separator());
            view.Items.Add(CreateRadioMenuItem("Small icons", "SizeGroup", isChecked: false, output));
            view.Items.Add(CreateRadioMenuItem("Medium icons", "SizeGroup", isChecked: true, output));
            view.Items.Add(CreateRadioMenuItem("Large icons", "SizeGroup", isChecked: false, output));

            var help = new Mux.MenuBarItem { Title = "Help" };
            help.Items.Add(CreateMenuItem("About", output));

            menu.Items.Add(file);
            menu.Items.Add(edit);
            menu.Items.Add(view);
            menu.Items.Add(help);
            panel.Children.Add(output);
            panel.Children.Add(menu);
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

        private static Mux.AppBarButton CreateCommandBarAppBarButton(Mux.Symbol symbol, string label, TextBlock output, string inputGestureText)
        {
            var button = new Mux.AppBarButton
            {
                Icon = new Mux.SymbolIcon(symbol),
                Label = label,
                InputGestureText = inputGestureText
            };
            button.Click += delegate { output.Text = "You clicked: " + label; };
            return button;
        }

        private static MenuItem CreateMenuItem(string header, TextBlock output)
        {
            return CreateMenuItem(header, output, inputGestureText: null);
        }

        private static Mux.MenuBar CreateMenuBar()
        {
            return new Mux.MenuBar
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                MinWidth = 158
            };
        }

        private static MenuItem CreateMenuItem(string header, TextBlock output, string inputGestureText)
        {
            var item = new MenuItem
            {
                Header = header,
                InputGestureText = inputGestureText
            };
            item.Click += delegate { output.Text = "You clicked: " + header; };
            return item;
        }

        private static Mux.RadioMenuItem CreateRadioMenuItem(string header, string groupName, bool isChecked, TextBlock output)
        {
            var item = new Mux.RadioMenuItem
            {
                Header = header,
                GroupName = groupName,
                IsChecked = isChecked
            };
            item.Click += delegate { output.Text = "You clicked: " + header; };
            return item;
        }

        private static TextBlock CreateSelectionOutput(string name)
        {
            return new TextBlock
            {
                Name = name,
                Text = string.Empty,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static string FindSnippetText(IReadOnlyList<SampleSnippet> snippets, string title)
        {
            for (var i = 0; i < snippets.Count; i++)
            {
                if (string.Equals(snippets[i].Title, title, StringComparison.Ordinal))
                {
                    return snippets[i].Text;
                }
            }

            return null;
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

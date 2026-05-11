using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class NavigationSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "BreadcrumbBar":
                    return CreateBreadcrumbBarSample();
                case "NavigationView":
                    return CreateNavigationViewSample();
                case "Pivot":
                    return CreatePivotSample();
                case "SelectorBar":
                    return CreateSelectorBarSample();
                case "TabView":
                    return CreateTabViewSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateBreadcrumbBarSample()
        {
            var panel = CreateSamplePanel("BreadcrumbBar maps to a clickable WPF breadcrumb trail because ModernWpf does not currently expose BreadcrumbBar.");
            var folders = new List<string> { "Home", "Documents", "Design", "Northwind", "Images", "Folder1", "Folder2", "Folder3" };
            var trail = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var output = CreateOutput("Current location: " + string.Join(" / ", folders));

            Action rebuild = null;
            rebuild = delegate
            {
                trail.Children.Clear();
                for (var i = 0; i < folders.Count; i++)
                {
                    var index = i;
                    var button = new Button
                    {
                        Content = folders[index],
                        Padding = new Thickness(10, 4, 10, 4),
                        Margin = new Thickness(0, 0, 4, 0)
                    };
                    button.Click += delegate
                    {
                        folders.RemoveRange(index + 1, folders.Count - index - 1);
                        output.Text = "Current location: " + string.Join(" / ", folders);
                        rebuild();
                    };
                    trail.Children.Add(button);
                    if (i < folders.Count - 1)
                    {
                        trail.Children.Add(new TextBlock
                        {
                            Text = ">",
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 4, 0),
                            Opacity = 0.72
                        });
                    }
                }
            };
            rebuild();

            var reset = CreateButton("Reset sample");
            reset.Margin = new Thickness(0, 12, 0, 0);
            reset.Click += delegate
            {
                folders.Clear();
                folders.AddRange(new[] { "Home", "Documents", "Design", "Northwind", "Images", "Folder1", "Folder2", "Folder3" });
                output.Text = "Current location: " + string.Join(" / ", folders);
                rebuild();
            };

            panel.Children.Add(trail);
            panel.Children.Add(reset);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateNavigationViewSample()
        {
            var panel = CreateSamplePanel("NavigationView provides a collapsible navigation menu for top-level app areas.");
            var content = new TextBlock
            {
                Text = "Home page",
                Margin = new Thickness(18),
                TextWrapping = TextWrapping.Wrap
            };
            var navigationView = new Mux.NavigationView
            {
                Width = 520,
                Height = 320,
                Header = "Home",
                IsBackButtonVisible = Mux.NavigationViewBackButtonVisible.Collapsed,
                PaneDisplayMode = Mux.NavigationViewPaneDisplayMode.Left,
                Content = new Border
                {
                    Padding = new Thickness(12),
                    Child = content
                }
            };

            var home = CreateNavigationItem("Home");
            var apps = CreateNavigationItem("Apps");
            var controls = CreateNavigationItem("Controls");
            var design = CreateNavigationItem("Design");
            navigationView.MenuItems.Add(home);
            navigationView.MenuItems.Add(apps);
            navigationView.MenuItems.Add(controls);
            navigationView.MenuItems.Add(design);
            navigationView.SelectedItem = home;
            navigationView.SelectionChanged += delegate(Mux.NavigationView sender, Mux.NavigationViewSelectionChangedEventArgs args)
            {
                var item = args.SelectedItemContainer as Mux.NavigationViewItem;
                if (item != null)
                {
                    sender.Header = item.Content;
                    content.Text = item.Content + " page";
                }
            };

            panel.Children.Add(navigationView);
            return panel;
        }

        private static UIElement CreatePivotSample()
        {
            var panel = CreateSamplePanel("Pivot maps to a styled WPF TabControl for switching between related views.");
            var root = new StackPanel();
            root.Children.Add(new TextBlock
            {
                Text = "EMAIL",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });
            var tabs = new TabControl
            {
                Width = 420,
                Height = 220
            };
            tabs.Items.Add(CreateTab("All", "all emails go here."));
            tabs.Items.Add(CreateTab("Unread", "unread emails go here."));
            tabs.Items.Add(CreateTab("Flagged", "flagged emails go here."));
            tabs.Items.Add(CreateTab("Urgent", "urgent emails go here."));
            root.Children.Add(tabs);
            panel.Children.Add(root);
            return panel;
        }

        private static UIElement CreateSelectorBarSample()
        {
            var panel = CreateSamplePanel("SelectorBar maps to a compact row of toggle buttons that swaps a finite content set.");
            var selector = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var content = new Border
            {
                Width = 420,
                Height = 130,
                Padding = new Thickness(16),
                Margin = new Thickness(0, 12, 0, 0),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8")
            };

            var buttons = new List<ToggleButton>();
            Action<string> select = delegate(string name)
            {
                foreach (var button in buttons)
                {
                    button.IsChecked = Equals(button.Content, name);
                }
                content.Child = new TextBlock
                {
                    Text = name + " content",
                    FontSize = 22,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                };
            };

            foreach (var name in new[] { "Recent", "Shared", "Favorites" })
            {
                var button = new ToggleButton
                {
                    Content = name,
                    Padding = new Thickness(14, 6, 14, 6),
                    Margin = new Thickness(0, 0, 8, 0)
                };
                button.Click += delegate { select((string)button.Content); };
                buttons.Add(button);
                selector.Children.Add(button);
            }
            select("Recent");

            panel.Children.Add(selector);
            panel.Children.Add(content);
            return panel;
        }

        private static UIElement CreateTabViewSample()
        {
            var panel = CreateSamplePanel("TabView maps to the ModernWpf-styled WPF TabControl with explicit add and close commands.");
            var tabControl = new TabControl
            {
                Width = 520,
                Height = 260
            };
            tabControl.Items.Add(CreateTab("Document 0", "Document 0 content"));
            tabControl.Items.Add(CreateTab("Document 1", "Document 1 content"));
            tabControl.Items.Add(CreateTab("Document 2", "Document 2 content"));
            tabControl.SelectedIndex = 0;

            var commands = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var add = CreateButton("Add tab");
            var close = CreateButton("Close selected");
            add.Click += delegate
            {
                var index = tabControl.Items.Count;
                var tab = CreateTab("Document " + index, "Document " + index + " content");
                tabControl.Items.Add(tab);
                tabControl.SelectedItem = tab;
            };
            close.Click += delegate
            {
                if (tabControl.SelectedItem is TabItem && tabControl.Items.Count > 1)
                {
                    var selected = (TabItem)tabControl.SelectedItem;
                    tabControl.Items.Remove(selected);
                }
            };
            commands.Children.Add(add);
            commands.Children.Add(close);

            panel.Children.Add(tabControl);
            panel.Children.Add(commands);
            return panel;
        }

        private static Mux.NavigationViewItem CreateNavigationItem(string content)
        {
            return new Mux.NavigationViewItem
            {
                Content = content
            };
        }

        private static TabItem CreateTab(string header, string text)
        {
            return new TabItem
            {
                Header = header,
                Content = new Border
                {
                    Padding = new Thickness(16),
                    Child = new TextBlock
                    {
                        Text = text,
                        TextWrapping = TextWrapping.Wrap
                    }
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using Mux = ModernWpf.Controls;

namespace ModernWpf.SampleApp.Pages
{
    internal static class CollectionsSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "FlipView":
                    return CreateFlipViewSample();
                case "GridView":
                    return CreateGridViewSample();
                case "ItemsRepeater":
                    return CreateItemsRepeaterSample();
                case "ItemsView":
                    return CreateItemsViewSample();
                case "ListBox":
                    return CreateListBoxSample();
                case "ListView":
                    return CreateListViewSample();
                case "PullToRefresh":
                    return CreatePullToRefreshSample();
                case "TreeView":
                    return CreateTreeViewSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateFlipViewSample()
        {
            var panel = CreateSamplePanel("FlipView maps to explicit previous and next navigation because ModernWpf no longer carries the legacy MahApps FlipView adapter.");
            var items = new[] { "Featured", "Recent", "Recommended" };
            var index = 0;
            var content = CreateCollectionCard(items[index]);
            var output = CreateOutput("Item 1 of 3");

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            var previous = CreateButton("Previous");
            var next = CreateButton("Next");
            previous.Click += delegate
            {
                index = (index + items.Length - 1) % items.Length;
                content.Child = CreateCardText(items[index]);
                output.Text = "Item " + (index + 1) + " of " + items.Length;
            };
            next.Click += delegate
            {
                index = (index + 1) % items.Length;
                content.Child = CreateCardText(items[index]);
                output.Text = "Item " + (index + 1) + " of " + items.Length;
            };
            row.Children.Add(previous);
            row.Children.Add(next);

            panel.Children.Add(content);
            panel.Children.Add(row);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateGridViewSample()
        {
            var panel = CreateSamplePanel("GridView maps to WPF ListView with a GridView view for tabular collections.");
            var listView = new ListView
            {
                Width = 420,
                Height = 170,
                ItemsSource = CreatePeople()
            };
            var view = new GridView();
            view.Columns.Add(CreateGridViewColumn("Name", "Name", 160));
            view.Columns.Add(CreateGridViewColumn("Role", "Role", 150));
            view.Columns.Add(CreateGridViewColumn("Status", "Status", 90));
            listView.View = view;
            panel.Children.Add(listView);
            return panel;
        }

        private static UIElement CreateItemsRepeaterSample()
        {
            var panel = CreateSamplePanel("ItemsRepeater renders repeated content from a data source with a reusable item template.");
            var repeater = new Mux.ItemsRepeater
            {
                ItemsSource = new[] { "Inbox", "Archive", "Drafts", "Sent", "Deleted" },
                ItemTemplate = CreateRepeaterTemplate()
            };
            panel.Children.Add(new Mux.ItemsRepeaterScrollHost
            {
                Height = 190,
                Width = 320,
                ScrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = repeater
                }
            });
            return panel;
        }

        private static UIElement CreateItemsViewSample()
        {
            var panel = CreateSamplePanel("ItemsView maps to a selectable WPF ListBox with tile-like items because ModernWpf does not currently expose ItemsView.");
            var listBox = new ListBox
            {
                Width = 420,
                Height = 170,
                ItemsPanel = CreateWrapItemsPanel(),
                ItemTemplate = CreateTileTemplate(),
                ItemsSource = new[] { "Contoso", "Fabrikam", "Northwind", "Tailspin", "AdventureWorks" }
            };
            panel.Children.Add(listBox);
            return panel;
        }

        private static UIElement CreateListBoxSample()
        {
            var panel = CreateSamplePanel("ListBox lets users choose one or more values from a simple list.");
            panel.Children.Add(new ListBox
            {
                Width = 260,
                Height = 150,
                SelectionMode = SelectionMode.Multiple,
                ItemsSource = new[] { "Red", "Green", "Blue", "Yellow" }
            });
            return panel;
        }

        private static UIElement CreateListViewSample()
        {
            var panel = CreateSamplePanel("ListView presents rich rows that can include multiple pieces of data.");
            var listView = new ListView
            {
                Width = 360,
                Height = 170,
                ItemsSource = CreatePeople(),
                ItemTemplate = CreatePersonTemplate()
            };
            panel.Children.Add(listView);
            return panel;
        }

        private static UIElement CreatePullToRefreshSample()
        {
            var panel = CreateSamplePanel("PullToRefresh maps to an explicit refresh action because ModernWpf does not currently expose RefreshContainer.");
            var output = CreateOutput("Last refreshed: not yet.");
            var refresh = CreateButton("Refresh list");
            var refreshCount = 0;
            refresh.Click += delegate
            {
                refreshCount++;
                output.Text = "Last refreshed: " + DateTime.Now.ToLongTimeString() + " (" + refreshCount + ")";
            };
            panel.Children.Add(refresh);
            panel.Children.Add(new ListBox
            {
                Width = 260,
                Height = 120,
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[] { "New mention", "Build complete", "Review requested" }
            });
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateTreeViewSample()
        {
            var panel = CreateSamplePanel("TreeView displays hierarchical data with expandable nodes.");
            var tree = new TreeView { Width = 320, Height = 180 };
            var controls = new TreeViewItem { Header = "Controls", IsExpanded = true };
            controls.Items.Add(new TreeViewItem { Header = "Button" });
            controls.Items.Add(new TreeViewItem { Header = "ListView" });
            controls.Items.Add(new TreeViewItem { Header = "NavigationView" });
            var design = new TreeViewItem { Header = "Design", IsExpanded = true };
            design.Items.Add(new TreeViewItem { Header = "Color" });
            design.Items.Add(new TreeViewItem { Header = "Typography" });
            tree.Items.Add(controls);
            tree.Items.Add(design);
            panel.Children.Add(tree);
            return panel;
        }

        private static GridViewColumn CreateGridViewColumn(string header, string bindingPath, double width)
        {
            return new GridViewColumn
            {
                Header = header,
                Width = width,
                DisplayMemberBinding = new Binding(bindingPath)
            };
        }

        private static ItemsPanelTemplate CreateWrapItemsPanel()
        {
            var factory = new FrameworkElementFactory(typeof(WrapPanel));
            return new ItemsPanelTemplate(factory);
        }

        private static Border CreateCollectionCard(string text)
        {
            return new Border
            {
                Width = 260,
                Height = 120,
                Padding = new Thickness(16),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 12),
                Child = CreateCardText(text)
            };
        }

        private static TextBlock CreateCardText(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 24,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static DataTemplate CreateRepeaterTemplate()
        {
            return (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Border Padding='10' Margin='0,0,0,6' BorderThickness='1' BorderBrush='#D8D8D8'>" +
                "<TextBlock Text='{Binding}' />" +
                "</Border>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateTileTemplate()
        {
            return (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Border Width='120' Height='72' Margin='0,0,8,8' Padding='10' BorderThickness='1' BorderBrush='#D8D8D8'>" +
                "<TextBlock Text='{Binding}' TextWrapping='Wrap' VerticalAlignment='Center' />" +
                "</Border>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreatePersonTemplate()
        {
            return (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<StackPanel Margin='0,4,0,4'>" +
                "<TextBlock Text='{Binding Name}' FontWeight='SemiBold' />" +
                "<TextBlock Text='{Binding Role}' Opacity='0.72' />" +
                "</StackPanel>" +
                "</DataTemplate>");
        }

        private static object[] CreatePeople()
        {
            return new object[]
            {
                new { Name = "Avery Howard", Role = "Designer", Status = "Online" },
                new { Name = "Kai Martin", Role = "Engineer", Status = "Busy" },
                new { Name = "Mina Patel", Role = "PM", Status = "Away" }
            };
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
